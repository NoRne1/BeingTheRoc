using UnityEngine;
using UnityEngine.UI;
using UniRx;
using static UnityEditor.Progress;

public class UIBattleItem : MonoBehaviour
{
    public Image itemIcon;
    public string itemID;
    public GameObject border;
    public GameObject indicator;
    public FightTextManager fightTextManager;
    public Animator ani;
    public Slider hpSlider;
    public Slider shieldSlider;

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
        this.ani = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(string itemID)
    {
        this.itemID = itemID;
        var item = GlobalAccess.GetBattleItem(itemID);
        itemIcon.overrideSprite = Resloader.LoadSprite(item.Resource, ConstValue.playersPath);
        NorneStore.Instance.ObservableObject(new BattleItem(itemID)).AsObservable().TakeUntilDestroy(this).Subscribe(item =>
        {
            hpSlider.maxValue = item.attributes.MaxHP;
            shieldSlider.maxValue = item.attributes.MaxHP;
            hpSlider.value = item.attributes.currentHP;
            shieldSlider.value = Mathf.Min(shieldSlider.maxValue, item.attributes.currentShield);
        });
    }

    public AttackStatus Damage(int damage, bool isCritical, bool hitFlag)
    {
        //todo Critical UI display
        Debug.Log("Damage:" + damage + " isCritical:" + isCritical);

        if (!hitFlag)
        {
            return AttackStatus.miss;
        }
        bool isBlock = false;
        
        //伤害溢出时，血量允许被扣成负数
        var item = GlobalAccess.GetBattleItem(itemID);
        int tempHP = item.attributes.currentHP;

        if (item.attributes.currentShield >= damage) {
            item.attributes.currentShield -= damage;
            isBlock = true;
        } else
        {
            if (item.isInvincible)
            {
                isBlock = true;
            } else
            {
                tempHP = item.attributes.currentHP + item.attributes.currentShield - damage;
            }
            item.attributes.currentShield = 0;
        }
        fightTextManager.CreatFightText("-" + damage.ToString(), TextAnimationType.Burst, TextMoveType.RightParabola, transform, isCritical);
        

        if (tempHP <= 0 && item.avoidDeath && item.avoidDeathFunc != null)
        {
            item.avoidDeathFunc(itemID);
            return AttackStatus.normal;
        } else
        {
            item.attributes.currentHP = tempHP;
        }
        GlobalAccess.SaveBattleItem(item);

        if (item.attributes.currentHP <= 0)
        {
            this.Die();
            return AttackStatus.toDeath;
        } else if (isBlock)
        {
            return AttackStatus.block;
        } else
        {
            return AttackStatus.normal;
        }
    }

    public void AddHP(int hp, bool isCritical)
    {
        var item = GlobalAccess.GetBattleItem(itemID);
        item.attributes.currentHP = Mathf.Min(item.attributes.MaxHP, item.attributes.currentHP + hp);
        fightTextManager.CreatFightText("+" + hp.ToString(), TextAnimationType.Normal, TextMoveType.RightParabola, transform, isCritical);
        GlobalAccess.SaveBattleItem(item);
    }

    //todo die
    public void Die()
    {
        BattleManager.Instance.CharacterDie(itemID);
    }
}

