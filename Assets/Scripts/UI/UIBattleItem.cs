using UnityEngine;
using UnityEngine.UI;
using UniRx;
using static UnityEditor.Progress;
using System.Collections;
using DG.Tweening;
using System.Linq;

public class UIBattleItem : MonoBehaviour
{
    public Image itemIcon;
    public string itemID;
    public GameObject border;
    public GameObject indicator;
    public FightTextManager fightTextManager;
    public UIBattleItemAnimator positiveAni;
    public UIBattleItemAnimator negativeAni;
    public Slider hpSlider;
    public Slider shieldSlider;
    public Image positiveEquipIcon;
    public Image negativeEquipIcon;

    public bool Selected
    {
        get { return border.activeSelf; }
        set { border.SetActive(value); }
    }

    public bool roundActive
    {
        get { return indicator.activeSelf; }
        set { indicator.SetActive(value); }
    }

    private Coroutine hpChangeCoroutine;
    private Coroutine dodgeCoroutine;
    private Vector3 dodgeStartPosition;
    private Coroutine hittedCoroutine;
    private Vector3 hittedStartPosition;

     // attack result queue
    public NormalAttackResult attackResult;
    public AttackDisplayResult currentDisplayResult;
    // 当前处理的 displayResult 索引
    public int currentDisplayResultIndex = 0;

    // Use this for initialization
    void Start()
    {
        border.SetActive(false);
        indicator.SetActive(false);
        this.fightTextManager = this.GetComponent<FightTextManager>();
        if (fightTextManager != null)
        {
            fightTextManager.FightTextCanvas = GameObject.FindWithTag("FightTextCanvas").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(string itemID)
    {
        this.itemID = itemID;
        var item = GlobalAccess.GetBattleItem(itemID);
        itemIcon.overrideSprite = Resloader.LoadSprite(item.Resource, ConstValue.battleItemsPath);
        NorneStore.Instance.ObservableObject(new BattleItem(itemID)).AsObservable().TakeUntilDestroy(this).Subscribe(item =>
        {
            hpSlider.maxValue = item.attributes.MaxHP;
            shieldSlider.maxValue = item.attributes.MaxHP;
            SetHpAni(item.attributes.currentHP, false);
            shieldSlider.value = Mathf.Min(shieldSlider.maxValue, item.attributes.currentShield);
        });
    }


    public void PorcessDisplayResult(AttackDisplayResult attackResult) 
    {
        //先开始新动画，从而停止旧动画
        ItemUsedAni(attackResult.itemModel);
        if (currentDisplayResult != null)
        {
            //结算上次没结算的伤害
            Damage(currentDisplayResult);
        }
        currentDisplayResult = attackResult;
    }

    public void Damage(AttackDisplayResult attackResult)
    {
        //todo Critical UI display
        Debug.Log("Damage:" + attackResult.damage + " status:" + attackResult.attackStatus);
        switch (attackResult.attackStatus) 
        {
            case AttackStatus.errorTarget:
                Debug.LogError("UIBattleItem Damage Error Target");
                return;
            case AttackStatus.miss:
                DodgeAni();
                return;
            case AttackStatus.critical:
            case AttackStatus.normal:
                HittedAni();
                //伤害溢出时，血量允许被扣成负数
                var item = GlobalAccess.GetBattleItem(itemID);
                int tempHP = item.attributes.currentHP;

                if (item.attributes.currentShield >= attackResult.damage) {
                    item.attributes.currentShield -= attackResult.damage;
                } else
                {
                    if (!item.isInvincible)
                    {
                        tempHP = item.attributes.currentHP + item.attributes.currentShield - attackResult.damage;
                        if (attackResult.triggerOtherEffect && attackResult.itemModel != null)
                        {
                            StartCoroutine(ItemUseManager.Instance.InvokeEffect(EffectInvokeType.damage, attackResult.casterID, null, Vector2.negativeInfinity, attackResult.itemModel));
                        }
                    }
                    item.attributes.currentShield = 0;
                }

                if (tempHP <= 0 && item.avoidDeath && item.avoidDeathFunc != null)
                {
                    item.avoidDeathFunc(itemID);
                } else
                {
                    HPChange(tempHP - item.attributes.currentHP, attackResult.attackStatus == AttackStatus.critical);
                }
                GlobalAccess.SaveBattleItem(item);

                if (item.attributes.currentHP <= 0)
                {
                    this.Die();
                    var caster = GlobalAccess.GetBattleItem(attackResult.casterID);
                    caster.defeatSubject.OnNext(Unit.Default);
                    if (attackResult.triggerOtherEffect && attackResult.itemModel != null)
                    {
                        StartCoroutine(ItemUseManager.Instance.InvokeEffect(EffectInvokeType.toDeath, attackResult.casterID, null, Vector2.negativeInfinity, attackResult.itemModel));
                    }
                }
                break;
            default:
                Debug.LogError("UIBattleItem Damage Unknown Status");
                break;
        }
        currentDisplayResult = null;
    }

    public void HPChange(int change, bool isCritical)
    {
        var item = GlobalAccess.GetBattleItem(itemID);
        item.attributes.currentHP = Mathf.Max(Mathf.Min(item.attributes.MaxHP, item.attributes.currentHP + change), 0);
        fightTextManager.CreatFightText((change >= 0 ? "+" : "") + change.ToString(), TextAnimationType.Burst, TextMoveType.RightParabola, transform, isCritical);
        SetHpAni(item.attributes.currentHP);
        GlobalAccess.SaveBattleItem(item);
    }

    public void SetHpAni(float targetHealth, bool animate = true)
    {
        if (hpChangeCoroutine == null && hpSlider.value == targetHealth)
        {
            return;
        }
        // 如果有正在进行的血条减少动画，先停止它
        if (hpChangeCoroutine != null)
        {
            StopCoroutine(hpChangeCoroutine);
        }

        // 启动新的血条减少动画
        hpChangeCoroutine = StartCoroutine(AnimateHpChange(targetHealth, animate));
    }

    private IEnumerator AnimateHpChange(float targetHealth, bool animate)
    {
        float elapsedTime = 0f;
        float duration = 0.5f; // 动画持续时间
        float startValue = hpSlider.value;
        float endValue = Mathf.Max(targetHealth, 0);
        if(animate)
        {
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                hpSlider.value = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
                yield return null;
            }
        }
        hpSlider.value = endValue;
        hpChangeCoroutine = null; // 动画结束后清空引用
    }

    //todo die
    public void Die()
    {
        BattleManager.Instance.CharacterDie(itemID);
    }

    
    public void DodgeAni()
    {
        // 如果有正在进行的闪避动画，先停止它，初始位置保持原来的
        if (dodgeCoroutine != null)
        {
            StopCoroutine(dodgeCoroutine);
        } else 
        {
            dodgeStartPosition = transform.position;
        }

        // 启动新的闪避动画
        dodgeCoroutine = StartCoroutine(DodgeCoroutine());
    }

    private IEnumerator DodgeCoroutine()
    {
        // 计算闪避后的目标位置
        Vector3 dodgePosition = Camera.main.ScreenToWorldPoint(Camera.main.WorldToScreenPoint(dodgeStartPosition) + new Vector3(45, -45, 0)); // 向右下移动
        // 使用 DOTween 移动角色到目标位置
        Tween moveOut = transform.DOMove(dodgePosition, 0.25f);
        yield return moveOut.WaitForCompletion();

        // 闪避完成后，返回原位置
        Tween moveBack = transform.DOMove(dodgeStartPosition, 0.25f);
        yield return moveBack.WaitForCompletion();

        dodgeCoroutine = null; // 动画结束后清空引用
    }

    public void HittedAni() 
    {
        // 如果有正在进行的受击动画（初始位置保持原来的)，先停止它
        if (hittedCoroutine != null)
        {
            StopCoroutine(hittedCoroutine);
        } else 
        {
            hittedStartPosition = transform.position;
        }

        // 启动新的闪避动画
        hittedCoroutine = StartCoroutine(HittedCoroutine());
    }

    private IEnumerator HittedCoroutine()
    {
        // 计算闪避后的目标位置
        Vector3 hittedPosition = Camera.main.ScreenToWorldPoint(Camera.main.WorldToScreenPoint(hittedStartPosition) + new Vector3(20, 0, 0));

        // 使用 DOTween 移动角色到目标位置
        Tween moveOut = transform.DOMove(hittedPosition, 0.2f);
        yield return moveOut.WaitForCompletion();

        // 闪避完成后，返回原位置
        Tween moveBack = transform.DOMove(hittedStartPosition, 0.2f);
        yield return moveBack.WaitForCompletion();

        hittedCoroutine = null; // 动画结束后清空引用
    }

    //使用装备的动画
    public void ItemUseAni(StoreItemModel item)
    {
        if (!item.CanEquip()) 
        {
            Debug.LogError("UIBattleItem AttackAni item not a Equip");
            return;
        }
        positiveEquipIcon.overrideSprite = Resloader.LoadSprite(item.iconResource, ConstValue.equipsPath);
        switch (item.equipDefine.equipClass)
        {
            case EquipClass.arch:
                positiveAni.animator.SetTrigger("arch");
                break;
            case EquipClass.shield:
                positiveAni.animator.SetTrigger("other");
                break;
            case EquipClass.sword:
                positiveAni.animator.SetTrigger("sword");
                break;
            case EquipClass.other:
                positiveAni.animator.SetTrigger("other");
                break;  
            default:
                Debug.LogError("UIBattleItem AttackAni Unknown EquipClass");
                break;
        }
    }
    //作为装备使用目标的动画
    public void ItemUsedAni(StoreItemModel item)
    {
        if (!item.CanEquip()) 
        {
            Debug.LogError("UIBattleItem AttackAni item not a Equip");
            return;
        }
        negativeEquipIcon.overrideSprite = Resloader.LoadSprite(item.iconResource, ConstValue.equipsPath);
        switch (item.equipDefine.equipClass)
        {
            case EquipClass.arch:
                negativeAni.animator.SetTrigger("arch_used");
                break;
            case EquipClass.shield:
                negativeAni.animator.SetTrigger("shield_used");
                break;
            case EquipClass.sword:
                negativeAni.animator.SetTrigger("sword_used");
                break;
            case EquipClass.other:
                // ani.SetTrigger("other");
                break;  
            default:
                Debug.LogError("UIBattleItem AttackAni Unknown EquipClass");
                break;
        }
    }

    public void SetAttackResult(NormalAttackResult result)
    {
        currentDisplayResultIndex = 0;
        attackResult = result;
    }
}

