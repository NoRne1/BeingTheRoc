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
    public Dictionary<GeneralLevel, List<CharacterDefine>> levelCharacters = null;

    public Dictionary<String, String> Language = null;

    //0:中文, 1:英文
    public Dictionary<int, Dictionary<String, String>> LanguagesDic = null;
    public Dictionary<int, TownActionDefine> TownActions = null;
    public Dictionary<int, StoreItemDefine> StoreItems = null;
    public Dictionary<int, EquipDefine> EquipDefines = null;
    public Dictionary<int, ExpendableItemDefine> ExpendableItemDefines = null;
    public Dictionary<int, TreasureDefine> TreasureDefines = null;
    public Dictionary<int, GoodsDefine> GoodsDefines = null;
    public Dictionary<int, SpecialItemDefine> SpecialItemDefines = null;
    public Dictionary<int, FoodDefine> FoodDefines = null;

    public Dictionary<int, SkillDefine> Skills = null;
    public Dictionary<int, ExtraEntryDesc> ExtraEntrys = null;
    public Dictionary<int, EnemyDefine> EnemyDefines = null;
    public Dictionary<int, BuffDefine> BuffDefines = null;
    public Dictionary<int, CollectItemDefine> collectItemDefines = null;
    public Dictionary<int, EquipExtraEntryDefine> equipExtraEntryDefines = null;
    public Dictionary<int, Dictionary<int, int>> mergeEquipDic = null;
    public Dictionary<int, WeatherDefine> weatherDefines = null;
    public Dictionary<int, EventDefine> eventsDefines = null;

    public NameGenerator nameGenerator = new NameGenerator();
    public TownNameGenerator townNameGenerator = new TownNameGenerator();
    public BehaviorSubject<bool> DataLoaded = new BehaviorSubject<bool>(false);

    public DataManager()
    {
        this.DataPath = "Data/";
        Debug.LogFormat("DataManager > DataManager()");
    }

    public void Load()
    {
        nameGenerator.LoadNameData(this.DataPath + "NameDatabase.txt");
        townNameGenerator.LoadTownNameData(this.DataPath + "TownNameDatabase.txt");

        string json = File.ReadAllText(this.DataPath + "CharacterDefine.json");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);
        this.levelCharacters = new Dictionary<GeneralLevel, List<CharacterDefine>>();
        this.levelCharacters.Add(GeneralLevel.green, this.Characters.Values.Where(c=>c.Level == GeneralLevel.green).ToList());
        this.levelCharacters.Add(GeneralLevel.blue, this.Characters.Values.Where(c=>c.Level == GeneralLevel.blue).ToList());
        //主角总是红色，所以需要在红色中过滤掉主角
        this.levelCharacters.Add(GeneralLevel.red, this.Characters.Values.Where(c=>c.ID >= GlobalAccess.subCharacterStartIndex && c.Level == GeneralLevel.red).ToList());

        json = File.ReadAllText(this.DataPath + "MutiLanguage.json");
        this.LanguagesDic = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, String>>>(json);
        Language = this.LanguagesDic[Config.Language];

        json = File.ReadAllText(this.DataPath + "StoreItemDefine.json");
        this.StoreItems = JsonConvert.DeserializeObject<Dictionary<int, StoreItemDefine>>(json);

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

        json = File.ReadAllText(this.DataPath + "FoodDefine.json");
        this.FoodDefines = JsonConvert.DeserializeObject<Dictionary<int, FoodDefine>>(json);

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

        json = File.ReadAllText(this.DataPath + "CollectItemDefine.json");
        this.collectItemDefines = JsonConvert.DeserializeObject<Dictionary<int, CollectItemDefine>>(json);

        json = File.ReadAllText(this.DataPath + "EquipExtraEntryDefine.json");
        this.equipExtraEntryDefines = JsonConvert.DeserializeObject<Dictionary<int, EquipExtraEntryDefine>>(json);

        json = File.ReadAllText(this.DataPath + "MergeEquipDefine.json");
        var equipExtraEntryDefines = JsonConvert.DeserializeObject<Dictionary<int, MergeEquipDefine>>(json);
        ProcessMergeEquipDic(equipExtraEntryDefines);

        json = File.ReadAllText(this.DataPath + "WeatherDefine.json");
        this.weatherDefines = JsonConvert.DeserializeObject<Dictionary<int, WeatherDefine>>(json);

        json = File.ReadAllText(this.DataPath + "EventDefine.json");
        this.eventsDefines = JsonConvert.DeserializeObject<Dictionary<int, EventDefine>>(json);
        
        DataLoaded.OnNext(true);
    }

    public IEnumerator LoadData()
    {
        nameGenerator.LoadNameData(this.DataPath + "NameDatabase.txt");
        townNameGenerator.LoadTownNameData(this.DataPath + "TownNameDatabase.txt");
        yield return null;
        
        string json = File.ReadAllText(this.DataPath + "CharacterDefine.json");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);
        this.levelCharacters = new Dictionary<GeneralLevel, List<CharacterDefine>>();
        this.levelCharacters.Add(GeneralLevel.green, this.Characters.Values.Where(c=>c.Level == GeneralLevel.green).ToList());
        this.levelCharacters.Add(GeneralLevel.blue, this.Characters.Values.Where(c=>c.Level == GeneralLevel.blue).ToList());
        this.levelCharacters.Add(GeneralLevel.red, this.Characters.Values.Where(c=>c.Level == GeneralLevel.red).ToList());
        yield return null;

        json = File.ReadAllText(this.DataPath + "MutiLanguage.json");
        this.LanguagesDic = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, String>>>(json);
        Language = this.LanguagesDic[Config.Language];
        yield return null;

        json = File.ReadAllText(this.DataPath + "StoreItemDefine.json");
        this.StoreItems = JsonConvert.DeserializeObject<Dictionary<int, StoreItemDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "EquipDefine.json");
        this.EquipDefines = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "ExpendableItemDefine.json");
        this.ExpendableItemDefines = JsonConvert.DeserializeObject<Dictionary<int, ExpendableItemDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "FoodDefine.json");
        this.FoodDefines = JsonConvert.DeserializeObject<Dictionary<int, FoodDefine>>(json);
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

        json = File.ReadAllText(this.DataPath + "CollectItemDefine.json");
        this.collectItemDefines = JsonConvert.DeserializeObject<Dictionary<int, CollectItemDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "EquipExtraEntryDefine.json");
        this.equipExtraEntryDefines = JsonConvert.DeserializeObject<Dictionary<int, EquipExtraEntryDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "MergeEquipDefine.json");
        var equipExtraEntryDefines = JsonConvert.DeserializeObject<Dictionary<int, MergeEquipDefine>>(json);
        ProcessMergeEquipDic(equipExtraEntryDefines);
        yield return null;

        json = File.ReadAllText(this.DataPath + "WeatherDefine.json");
        this.weatherDefines = JsonConvert.DeserializeObject<Dictionary<int, WeatherDefine>>(json);
        yield return null;

        json = File.ReadAllText(this.DataPath + "EventDefine.json");
        this.eventsDefines = JsonConvert.DeserializeObject<Dictionary<int, EventDefine>>(json);
        yield return null;
        
        //json = File.ReadAllText(this.DataPath + "TeleporterDefine.json");
        //this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);
        //yield return null;

        DataLoaded.OnNext(true);
    }

    public void Reset()
    {
        nameGenerator.Reset();
        townNameGenerator.Reset();
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

    public List<EnermyModel> getEnermyModels(int baseNum, int supportNum)
    {
        if (baseNum + supportNum <= 0) { return new List<EnermyModel>(); }
        List<EnermyModel> enermyModels = new List<EnermyModel>();
        //List<int> ids = GameUtil.Instance.GenerateUniqueRandomList(GlobalAccess.subCharacterStartIndex,
        //            GlobalAccess.subCharacterStartIndex + GlobalAccess.subCharacterNum, baseNum + supportNum);
        List<int> ids = new List<int>(Enumerable.Range(100, baseNum + supportNum));
        foreach (var id in ids)
        {
            //相同CID的敌人，随机选择一个
            EnemyDefine define = EnemyDefines.Values.Where(define => define.CID == id).ToList().OrderBy(x => UnityEngine.Random.Range(0, 100)).FirstOrDefault();
            if (define != null)
            {
                //使用CharacterDefine初始化EnermyModel，使用EnemyDefine设置持有装备等
                //属性在战斗开始时转成battleItem时，根据difficultyFactor设置
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
        for (int i = 0; i < baseNum + supportNum; i++)
        {
            enermyModels[i].isSupport = i >= baseNum;
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

    private void ProcessMergeEquipDic(Dictionary<int, MergeEquipDefine> defines)
    {
        mergeEquipDic = new Dictionary<int, Dictionary<int, int>>();
        foreach(var define in defines.Values)
        {
            if (mergeEquipDic.ContainsKey(define.equipA))
            {
                if(!mergeEquipDic[define.equipA].ContainsKey(define.equipB))
                {
                    mergeEquipDic[define.equipA].Add(define.equipB, define.equipResult);
                }
            } else {
                mergeEquipDic.Add(define.equipA, new Dictionary<int, int>());
                mergeEquipDic[define.equipA].Add(define.equipB, define.equipResult);
            }
            if (mergeEquipDic.ContainsKey(define.equipB))
            {
                if(!mergeEquipDic[define.equipB].ContainsKey(define.equipA))
                {
                    mergeEquipDic[define.equipB].Add(define.equipA, define.equipResult);
                }
            } else {
                mergeEquipDic.Add(define.equipB, new Dictionary<int, int>());
                mergeEquipDic[define.equipB].Add(define.equipA, define.equipResult);
            }
        }
    }

    public Dictionary<int, int> GetCanMergeEquips(int equipA)
    {
        if (mergeEquipDic.ContainsKey(equipA))
        {
            return mergeEquipDic[equipA];
        } else {
            return new Dictionary<int, int>();
        }
    }

    public int GetMergedEquip(int equipA, int equipB)
    {
        if (mergeEquipDic.ContainsKey(equipA) && mergeEquipDic[equipA].ContainsKey(equipB))
        {
            return mergeEquipDic[equipA][equipB];
        } else {
            return -1;
        }
    }

    public (bool, CharacterDefine) GetRandomCharacter(float greenRate, float blueRate, float redRate)
    {
        float randomValue = UnityEngine.Random.value; // 生成一个到1之间的随机数
        GeneralLevel selectedLevel;

        // 根据概率选择对应的等级
        if (randomValue < greenRate)
        {
            selectedLevel = GeneralLevel.green;
        }
        else if (randomValue < greenRate + blueRate)
        {
            selectedLevel = GeneralLevel.blue;
        }
        else if (randomValue < greenRate + blueRate + redRate)
        {
            selectedLevel = GeneralLevel.red;
        } else 
        {
            return (false, new CharacterDefine());
        }

        // 获取对应等级的所有角色
        if (levelCharacters.TryGetValue(selectedLevel, out List<CharacterDefine> characterList))
        {
            if (characterList.Count > 0)
            {
                // 随机选择一个角色
                return (true, characterList[UnityEngine.Random.Range(0, characterList.Count)]);
            }
            else
            {
                Debug.LogWarning($"No characters found for level {selectedLevel}.");
            }
        }
        else
        {
            Debug.LogWarning($"Level {selectedLevel} does not exist.");
        }

        return (false, new CharacterDefine()); // 若未找到角色，则返回 null
    }
}
