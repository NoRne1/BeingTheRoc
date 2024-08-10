using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;

public enum RoundTime
{
    begin = 0,
    acting = 1,
    end = 2,
    prepare = 3,
}

public enum AttackStatus
{
    errorTarget = -1,
    normal = 0,
    miss = 1,
    toDeath = 2,
    block = 3,
}

public class BattleManager : MonoSingleton<BattleManager>
{
    public BattleItemManager battleItemManager = new BattleItemManager();
    public BattleRoundManager roundManager = new BattleRoundManager();
    public BattleMoveBarManager moveBarManager;
    public BattleChessboardManager chessboardManager;

    // UI
    public UIMoveBar moveBar;
    public UIChessboard chessBoard;
    public UICharacterPlaceBox placeBox;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UIBattleItemInfo uiBattleItemInfo;
    public UIBattleBag uiBattleBag;
    public TownBattleInfoModel battleInfo;
    public bool isInBattle = false;
    // Data
    public float difficultyExtraFactor = 0f;

    // Subject
    // (string, string, int, AttackStatus, bool, bool) => (casterID, targetID, damage, attackStatus, isCritical, trigger other effect)
    public Subject<(string, string, int, AttackStatus, bool, bool)> battleItemDamageSubject = new Subject<(string, string, int, AttackStatus, bool, bool)>();

    // Use this for initialization
    void Start()
    { 
        moveBarManager = new BattleMoveBarManager(moveBar);
        chessboardManager = new BattleChessboardManager(chessBoard, placeBox, battleItemPrefab, battleItemFather, uiBattleItemInfo, uiBattleBag);
        MapManager.Instance.battleResultSubject.AsObservable().TakeUntilDestroy(this).Subscribe(result =>
        {
            if (result)
            {
                difficultyExtraFactor += 0.2f;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        chessboardManager.OuterExecuteActions();
        if (GameManager.Instance.currentPageType == PageType.battle)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (chessboardManager.currentPlaceIndex.Value != -1)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
                bool flag = false;
                // 处理每个射线碰撞到的对象
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("ChessBoardSlot"))
                    {
                        //鼠标在棋盘内
                        placeBox.gameObject.SetActive(true);
                        // 获取鼠标当前的屏幕坐标位置
                        Vector3 mousePositionScreen = Input.mousePosition;

                        // 将鼠标屏幕坐标位置转换为世界坐标位置
                        Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
                        mousePositionWorld.z = 0f; // 将 z 轴位置设为 0，使其与 2D 平面对齐

                        // 将自身 GameObject 的位置设置为鼠标的世界坐标位置
                        placeBox.transform.position = mousePositionWorld;

                        flag = true;
                        break;
                    }
                }
                if (!flag)
                { 
                    placeBox.gameObject.SetActive(false);
                }
            }

            if (Input.GetMouseButtonUp(0) && !UIManager.Instance.HasActiveUIWindow())
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
                bool flag = false;
                UIChessboardSlot slot = null;
                // 处理每个射线碰撞到的对象
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("ChessBoardSlot"))
                    {
                        flag = true;
                        slot = hit.collider.GetComponent<UIChessboardSlot>();
                    }
                    if (hit.collider != null && hit.collider.CompareTag("TopUI"))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag && slot != null)
                {
                    chessboardManager.clickedSlot.OnNext(slot);
                }
            }
        }
    }

    ~BattleManager()
    {
        
    }

    private void BattleInit(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        GameUtil.Instance.DetachChildren(battleItemFather);
        roundManager.Init();
        moveBarManager.Init();
        chessboardManager.Init();
        battleItemManager.Init(characterIDs, battleInfo);
    }

    public void StartBattle(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        isInBattle = true;
        BattleInit(characterIDs, battleInfo);
        
        this.battleInfo = battleInfo;
        
        roundManager.StartBattle();
        GameManager.Instance.treasureManager.BattleStart();
    }

    public void RoundEnd()
    {
        roundManager.roundTime.OnNext((battleItemManager.battleItemIDs[0], RoundTime.end));
    }

    public void MoveBarItemClicked(Button button)
    {
        BattleItem battleItem = null;
        if (button.GetComponent<UIMoveBarFirstItem>() != null)
        {
            battleItem = button.GetComponent<UIMoveBarFirstItem>().item;
        }
        else if (button.GetComponent<UIMoveBarOtherItem>() != null)
        {
            battleItem = button.GetComponent<UIMoveBarOtherItem>().item;
        }

        if (battleItem != null)
        {
            chessboardManager.clickSlotReason = ClickSlotReason.viewBattleItem;
            string uuID = battleItem.uuid;
            try
            {
                Vector2 vect = battleItemManager.pos_uibattleItemDic.First(x => x.Value.itemID == uuID).Key;
                chessboardManager.clickedSlot.OnNext(chessBoard.slots[vect]);
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("MoveBarItemClicked InvalidOperationException: " + e.Message);
            }
        }
    }

    public void EquipClicked(string uuid, UIEquipItem equipItem) {
        var battleItem0 = GlobalAccess.GetBattleItem(battleItemManager.battleItemIDs[0]);
        if (uuid == battleItem0.uuid && battleItem0.battleItemType == BattleItemType.player)
        {
            //只处理正在行动的玩家角色的装备点击
            ItemUseManager.Instance.Use(uuid, equipItem.storeItem);
        }
    }

    public void ShakeEnergy()
    {
        uiBattleItemInfo.ShakeEnergy();
    }

    public void BlinkEnergy()
    {
        uiBattleItemInfo.BlinkEnergy();
    }

    // 常规攻击（受属性影响，会暴击，触发特效）
    // 此处返回的AttackStatus是为了触发击伤和击杀效果
    public AttackStatus ProcessNormalAttack(string selfID, List<string> targetIDs, int baseAccuracy, int value, EquipClass equipClass = EquipClass.none)
    {
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        int totalDamage = 0;
        foreach (var id in targetIDs)
        {
            AttackStatus tempStatus;
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(id)).Value;
            int[] Data = new int[(int)EquipClass.MAX];
            Data[(int)equipClass] = GameManager.Instance.treasureManager.equipClassEffect.ContainsKey(equipClass) ?
                GameManager.Instance.treasureManager.equipClassEffect[equipClass] : 0;
            bool hitFlag = GameUtil.Instance.GetRandomRate(baseAccuracy *
                    (1 - target.attributes.Dodge / 100.0f) *
                    (1 + (self.attributes.Accuracy + Data[(int)EquipClass.arrow]) / 100.0f)
                );
            bool criticalFlag = GameUtil.Instance.GetRandomRate(self.attributes.Lucky);
            if (!hitFlag)
            {
                statuses.Add(AttackStatus.miss);
            }
            int damage = (int)((value + Data[(int)EquipClass.sword])
                * (1 + self.attributes.Strength / 100.0f) //力量乘区
                * (1 - target.attributes.Defense / (target.attributes.Defense + 100.0f)) //防御乘区
                * (hitFlag ? 1 : 0) //命中乘区
                * (criticalFlag ? 1 : 2) //暴击乘区
                * (1 - (self.attributes.Protection / 100.0f)) //减伤乘区
                * (1 + (self.attributes.EnchanceDamage / 100.0f))); //增伤乘区

            var targetItem = battleItemManager.pos_uibattleItemDic.Values.Where((item) => item.itemID == id).ToList().FirstOrDefault();
            
            if (battleItemManager.HasBattleItem(id))
            {
                tempStatus = battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[id]].Damage(damage, criticalFlag, hitFlag);
                statuses.Add(tempStatus);
            } else
            {
                tempStatus = AttackStatus.errorTarget;
                statuses.Add(tempStatus);
            }

            totalDamage += damage;
            battleItemDamageSubject.OnNext((selfID, id, damage, tempStatus, criticalFlag, true));
        }

        int hemato = (int)(totalDamage * (self.attributes.Hematophagia / 100.0f));
        if (hemato > 0)
        {
            ProcessDirectHealth(selfID, selfID, hemato);
        }
        if(statuses.Contains(AttackStatus.toDeath))
        {
            return AttackStatus.toDeath;
        } else {
            foreach(var status in statuses)
            {
                switch (status)
                {
                    case AttackStatus.normal:
                        return AttackStatus.normal;
                    case AttackStatus.miss:
                        continue;
                    case AttackStatus.toDeath:
                        return AttackStatus.toDeath;
                }
            }
            return AttackStatus.miss;
        }
    }

    // 直接伤害
    public AttackStatus ProcessDirectAttack(string casterID, string targetID, int value)
    {
        var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;
        var targetItem = battleItemManager.pos_uibattleItemDic.Values.Where((item) => item.itemID == targetID).ToList().FirstOrDefault();

        if (battleItemManager.HasBattleItem(targetID))
        {
            var attackStatus = battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[targetID]].Damage(value, false, true);
            battleItemDamageSubject.OnNext((casterID, targetID, value, attackStatus, false, false));
            return attackStatus;
        } else
        {
            return AttackStatus.errorTarget;
        }
    }

    // 常规回血（受属性影响，会暴击，触发特效）
    public void ProcessNormalHealth(string selfID, List<string> targetIDs, int value)
    {
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        foreach (var id in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(id)).Value;
            bool criticalFlag = UnityEngine.Random.Range(0, 100) < (self.attributes != null ? self.attributes.Lucky : 0);
            int healthHp = (int)(value
                * (1 + (self.attributes != null ? self.attributes.Strength : 0) / 100.0f)
                * (criticalFlag ? 1 : 2));
            if (battleItemManager.HasBattleItem(id))
            {
                battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[id]].AddHP(healthHp, criticalFlag);
            }
        }
    }

    // 直接回血
    public void ProcessDirectHealth(string selfID, string targetID, int value)
    {
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;
        if (battleItemManager.HasBattleItem(targetID))
        {
            battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[targetID]].AddHP(value, false);
        }
    }

    public void CharacterDie(string uuid)
    {
        battleItemManager.battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        battleItemManager.PlayerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        var pairs = battleItemManager.pos_uibattleItemDic.Where(item => item.Value.itemID == uuid);
        foreach(var pair in pairs)
        {
            Destroy(pair.Value.gameObject);
            battleItemManager.pos_uibattleItemDic.Remove(pair.Key);
        }
        GameManager.Instance.RemoveCharacter(uuid);
    }

    //离开战场（非死亡）
    public void CharacterLeave(string uuid)
    {
        battleItemManager.battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        battleItemManager.PlayerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        var pairs = battleItemManager.pos_uibattleItemDic.Where(item => item.Value.itemID == uuid);
        foreach (var pair in pairs)
        {
            Destroy(pair.Value.gameObject);
            battleItemManager.pos_uibattleItemDic.Remove(pair.Key);
        }
    }

    public void BattleEnd(bool result)
    {
        isInBattle = false;
        SkillManager.Instance.BattleEnd();
        foreach (var id in battleItemManager.battleItemIDs)
        {
            var item = GlobalAccess.GetBattleItem(id);
            item.BattleEnd();
        }
    }
}