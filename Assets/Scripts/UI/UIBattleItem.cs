using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIBattleItem : MonoBehaviour
{
    public Image itemIcon;
    public BattleItem item;
    public GameObject border;
    public GameObject indicator;
    public FightTextManager fightTextManager;
    public Animator ani;

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

    public void Setup(BattleItem item)
    {
        itemIcon.overrideSprite = Resloader.LoadSprite(item.Resource, ConstValue.playersPath);
        this.item = item;
    }

    public void Damage(int damage)
    {
        var temp = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(item.uuid)).Value;
        temp.currentHP -= damage;
        fightTextManager.CreatFightText("-" + damage.ToString(), TextAnimationType.Burst, TextMoveType.RightParabola, transform);
        if (temp.currentHP <= 0)
            this.Die();
        NorneStore.Instance.Update<BattleItem>(temp, true);
    }

    public void AddHP(int hp)
    {
        var temp = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(item.uuid)).Value;
        temp.currentHP += hp;
        fightTextManager.CreatFightText("+" + hp.ToString(), TextAnimationType.Normal, TextMoveType.RightParabola, transform);
        if (temp.currentHP > temp.MaxHP)
            temp.currentHP = temp.MaxHP;
        NorneStore.Instance.Update<BattleItem>(temp, true);
    }

    public void Die()
    {
        Debug.Log(item.Name + "die!");
    }
}

