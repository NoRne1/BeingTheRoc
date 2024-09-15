using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Newtonsoft.Json;
using UniRx;

public class DataManager : Singleton<DataManager>
{
    public string DataPath;
    public Dictionary<int, CharacterDefine> Characters = null;
    public Dictionary<String, String> Language = null;
    public Dictionary<int, Dictionary<String, String>> LanguagesDic = null;
    public Dictionary<int, TownActionDefine> TownActions = null;
    public Dictionary<int, StoreItemDefine> StoreItems = null;
    public List<StoreItemDefine> SellableItems = null;
    public Dictionary<int, EquipDefine> EquipDefines = null;
    public Dictionary<int, ExpendableItemDefine> ExpendableItemDefines = null;
    public Dictionary<int, TreasureDefine> TreasureDefines = null;
    public Dictionary<int, GoodsDefine> GoodsDefines = null;
    public Dictionary<int, SpecialItemDefine> SpecialItemDefines = null;

    public Dictionary<int, SkillDefine> Skills = null;
    public Dictionary<int, ExtraEntryDesc> ExtraEntrys = null;
    public Dictionary<int, EnemyDefine> EnemyDefines = null;
    public Dictionary<int, BuffDefine> BuffDefines = null;
    public BehaviorSubject<bool> DataLoaded = new BehaviorSubject<bool>(false);

    public DataManager()
    {
        this.DataPath = "Data/";
        Debug.LogFormat("DataManager > DataManager()");
    }

    public void Load()
    {
        string json = File.ReadAllText(this.DataPath + "CharacterDefine.json");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

        json = File.ReadAllText(this.DataPath + "MutiLanguage.json");
        this.LanguagesDic = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, String>>>(json);
        Language = this.LanguagesDic[Config.Language];

        json = File.ReadAllText(this.DataPath + "StoreItemDefine.json");
        this.StoreItems = JsonConvert.DeserializeObject<Dictionary<int, StoreItemDefine>>(json);
        this.SellableItems= this.StoreItems.Values.Where(item => item.price != -1).ToList();

        json = File.ReadAllText(this.DataPath + "EquipDefine.json");
        this.EquipDefines = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);

        json = File.ReadAllText(this.DataPath + "ExpendableItemDefine.json");
        this.ExpendableItemDefines = JsonConvert.DeserializeObject<Dictionary<int, ExpendableItemDefine>>(json);

        json = File.ReadAllText(this.DataPath + "TreasureDefine.json");
        this.TreasureDefines = JsonConvert.DeserializeObject<Dictionary<int, TreasureDefine>>(json);

        json = File.ReadAllText(this.DataPath + "GoodsDefine.json");
        this.GoodsDefines = JsonConvert.DeserializeObject<Dictionary<int, GoodsDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SpecialItemDefine.json");
        this.SpecialItemDefines = JsonConvert.DeserializeObject<Dictionary<int, SpecialItemDefine>>(json);

        json = File.ReadAllText(this.DataPath + "TownActionDefine.json");
        this.TownActions = JsonConvert.DeserializeObject<Dictionary<int, TownActionDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SkillDefine.json");
        this.Skills = JsonConvert.DeserializeObject<Dictionary<int, SkillDefine>>(json);

        json = File.ReadAllText(this.DataPath + "ExtraEntryDesc.json");
        this.ExtraEntrys = JsonConvert.DeserializeObject<Dictionary<int, ExtraEntryDesc>>(json);

        json = File.ReadAllText(this.DataPath + "EnemyDefine.json");
        this.EnemyDefines = JsonConvert.DeserializeObject<Dictionary<int, EnemyDefine>>(json);

        json = File.ReadAllText(this.DataPath + "BuffDefine.json");
        this.BuffDefines = JsonConvert.DeserializeObject<Dictionary<int, BuffDefine>>(json);

        DataLoaded.OnNext(true);
    }

    public IEnumerator LoadData()
    {
        string json = File.ReadAllText(this.DataPath + "CharacterDefine.json");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "MutiLanguage.json");
        this.LanguagesDic = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, String>>>(json);
        Language = this.LanguagesDic[Config.Language];
        yield return null;

        json = File.ReadAllText(this.DataPath + "StoreItemDefine.json");
        this.StoreItems = JsonConvert.DeserializeObject<Dictionary<int, StoreItemDefine>>(json);
        this.SellableItems= this.StoreItems.Values.Where(item => item.price != -1).ToList();
        yield return null;

        json = File.ReadAllText(this.DataPath + "EquipDefine.json");
        this.EquipDefines = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "ExpendableItemDefine.json");
        this.ExpendableItemDefines = JsonConvert.DeserializeObject<Dictionary<int, ExpendableItemDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "TreasureDefine.json");
        this.TreasureDefines = JsonConvert.DeserializeObject<Dictionary<int, TreasureDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "GoodsDefine.json");
        this.GoodsDefines = JsonConvert.DeserializeObject<Dictionary<int, GoodsDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "SpecialItemDefine.json");
        this.SpecialItemDefines = JsonConvert.DeserializeObject<Dictionary<int, SpecialItemDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "TownActionDefine.json");
        this.TownActions = JsonConvert.DeserializeObject<Dictionary<int, TownActionDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "SkillDefine.json");
        this.Skills = JsonConvert.DeserializeObject<Dictionary<int, SkillDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "ExtraEntryDesc.json");
        this.ExtraEntrys = JsonConvert.DeserializeObject<Dictionary<int, ExtraEntryDesc>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "EnemyDefine.json");
        this.EnemyDefines = JsonConvert.DeserializeObject<Dictionary<int, EnemyDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "BuffDefine.json");
        this.BuffDefines = JsonConvert.DeserializeObject<Dictionary<int, BuffDefine>>(json);
        yield return null;

        //json = File.ReadAllText(this.DataPath + "TeleporterDefine.json");
        //this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);
        //yield return null;

        DataLoaded.OnNext(true);
    }
//#if UNITY_EDITOR
//    public void SaveTeleporters()
//    {
//        string json = JsonConvert.SerializeObject(this.Teleporters, Formatting.Indented);
//        File.WriteAllText(this.DataPath + "TeleporterDefine.txt", json);
//    }
//    public void SaveSpawnPoints()
//    {
//        string json = JsonConvert.SerializeObject(this.SpawnPoints, Formatting.Indented);
//        File.WriteAllText(this.DataPath + "SpawnPointDefine.txt", json);
//    }

//#endif

    public List<EnermyModel> getEnermyModels()
    {
        List<EnermyModel> enermyModels = new List<EnermyModel>();
        //List<int> ids = GameUtil.Instance.GenerateUniqueRandomList(GlobalAccess.subCharacterStartIndex,
        //            GlobalAccess.subCharacterStartIndex + GlobalAccess.subCharacterNum, 2);
        List<int> ids = new List<int>() { 0, 1, 2 };
        foreach (var id in ids)
        {
            //相同CID的敌人，随机选择一个
            EnemyDefine define = EnemyDefines.Values.Where(define => define.CID == id).ToList().OrderBy(x => UnityEngine.Random.Range(0, 100)).FirstOrDefault();
            if (define != null)
            {
                EnermyModel model = new EnermyModel(Characters[id]);
                model.aiType = define.aiType;
                if (define.equip1 != null)
                {
                    model.backpack.Place(buildStoreItem(define.equip1), define.equip1.postion);
                }
                if (define.equip2 != null)
                {
                    model.backpack.Place(buildStoreItem(define.equip2), define.equip2.postion);
                }
                if (define.equip3 != null)
                {
                    model.backpack.Place(buildStoreItem(define.equip3), define.equip3.postion);
                }
                if (define.equip4 != null)
                {
                    model.backpack.Place(buildStoreItem(define.equip4), define.equip4.postion);
                }  
                if (define.equip5 != null)
                {
                    model.backpack.Place(buildStoreItem(define.equip5), define.equip5.postion);
                }
                enermyModels.Add(model);
            }
        }
        return enermyModels;
    }

    private StoreItemModel buildStoreItem(EnemyEquip equip)
    {
        StoreItemModel item = new StoreItemModel(StoreItems[equip.id]);
        item.position = equip.postion;
        item.Rotate(equip.rotation);
        return item;
    }
}
