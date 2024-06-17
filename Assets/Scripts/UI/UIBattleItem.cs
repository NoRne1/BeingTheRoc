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
            hpSlider.maxValue = item.MaxHP;
            shieldSlider.maxValue = item.MaxHP;
            hpSlider.maxValue = item.currentHP;
            shieldSlider.maxValue = item.shield;
        });
    }

    public AttackStatus Damage(int damage, bool isCritical)
    {
        //todo Critical UI display
        Debug.Log("Damage:" + damage + " isCritical:" + isCritical);

        //伤害溢出时，血量允许被扣成负数
        var item = GlobalAccess.GetBattleItem(itemID);
        if (item.shield >= damage) {
            item.shield -= damage;
        } else
        {
            item.currentHP = item.currentHP + item.shield - damage;
            item.shield = 0;
        }
        fightTextManager.CreatFightText("-" + damage.ToString(), TextAnimationType.Burst, TextMoveType.RightParabola, transform, isCritical);
        GlobalAccess.SaveBattleItem(item);
        if (item.currentHP <= 0)
        {
            this.Die();
            return AttackStatus.toDeath;
        } else
        {
            return AttackStatus.normal;
        }
    }

    public void AddHP(int hp, bool isCritical)
    {
        var item = GlobalAccess.GetBattleItem(itemID);
        item.currentHP = Mathf.Min(item.MaxHP, item.currentHP + hp);
        fightTextManager.CreatFightText("+" + hp.ToString(), TextAnimationType.Normal, TextMoveType.RightParabola, transform, isCritical);
        GlobalAccess.SaveBattleItem(item);
    }

    //todo die
    public void Die()
    {
        BattleManager.Instance.CharacterDie(itemID);
    }
}

