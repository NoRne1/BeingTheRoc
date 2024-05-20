using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Newtonsoft.Json;
using UniRx;

/// <summary>
/// ���ݹ�����
/// </summary>
public class DataManager : Singleton<DataManager>
{
    public string DataPath;
    public Dictionary<int, CharacterDefine> Characters = null;
    public Dictionary<String, String> Language = null;
    public Dictionary<int, Dictionary<String, String>> LanguagesDic = null;
    public Dictionary<int, TownActionDefine> TownActions = null;
    public Dictionary<int, StoreItemDefine> StoreItems = null;
    public Dictionary<int, SkillDefine> Skills = null;
    public Dictionary<int, ExtraEntryDesc> ExtraEntrys = null;
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

        json = File.ReadAllText(this.DataPath + "TownActionDefine.json");
        this.TownActions = JsonConvert.DeserializeObject<Dictionary<int, TownActionDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SkillDefine.json");
        this.Skills = JsonConvert.DeserializeObject<Dictionary<int, SkillDefine>>(json);

        json = File.ReadAllText(this.DataPath + "ExtraEntryDesc.json");
        this.ExtraEntrys = JsonConvert.DeserializeObject<Dictionary<int, ExtraEntryDesc>>(json);

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
}
