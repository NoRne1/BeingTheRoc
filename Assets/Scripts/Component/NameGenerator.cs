using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum Gender
{
    None = 0,
    Male = 1,
    Female = 2,
    Random = 3
}

// NameData 类用于存储每行数据
[System.Serializable]
public class NameData
{
    public int ID;
    public string chineseName;
    public string englishName;
    public Gender gender;
    
    public NameData() {}
    public NameData(int id, string chinese, string english, Gender gender)
    {
        this.ID = id;
        this.chineseName = chinese;
        this.englishName = english;
        this.gender = gender;
    }

    // 为了 HashSet 可以正确比较 NameData，我们重写 Equals 和 GetHashCode
    public override bool Equals(object obj)
    {
        return obj is NameData data &&
               chineseName == data.chineseName &&
               englishName == data.englishName &&
               gender == data.gender;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(chineseName, englishName, gender);
    }
}

public class NameGenerator
{
    // 用于存储未使用的名字（通过 ID 快速查找）
    private Dictionary<int, NameData> unusedMaleNames = new Dictionary<int, NameData>();
    private Dictionary<int, NameData> unusedFemaleNames = new Dictionary<int, NameData>();

    // 用于记录已经选择的名字，避免重复（存储完整的 NameData）
    private HashSet<NameData> usedMaleNames = new HashSet<NameData>();
    private HashSet<NameData> usedFemaleNames = new HashSet<NameData>();

    // 读取文件并将数据分为男性和女性
    public void LoadNameData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("文件不存在: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        int id = 0; // ID 从 0 开始递增
        foreach (string line in lines)
        {
            string[] parts = line.Split('|');
            if (parts.Length == 3)
            {
                string chineseName = parts[0].Trim();
                string englishName = parts[1].Trim();
                string gender = parts[2].Trim();

                // 根据性别分类，并为每个名字分配一个唯一的 ID
                NameData nameData = new NameData(id++, chineseName, englishName, gender == "M" ? Gender.Male : Gender.Female);
                if (gender == "M")
                {
                    unusedMaleNames.Add(nameData.ID, nameData);
                }
                else if (gender == "F")
                {
                    unusedFemaleNames.Add(nameData.ID, nameData);
                }
            }
        }
    }

    // 根据性别随机选择一个未使用的名字
    public NameData GetRandomNameByGender(Gender gender)
    {
        Dictionary<int, NameData> unusedNames = null;
        HashSet<NameData> usedNamesSet = null;

        switch (gender)
        {
            case Gender.Male:
                unusedNames = unusedMaleNames;
                usedNamesSet = usedMaleNames;
                break;
            case Gender.Female:
                unusedNames = unusedFemaleNames;
                usedNamesSet = usedFemaleNames;
                break;
            case Gender.Random:
                bool randomFlag = UnityEngine.Random.Range(0, 100) < 50;
                unusedNames = randomFlag ? unusedMaleNames : unusedFemaleNames;
                usedNamesSet = randomFlag ? usedMaleNames : usedFemaleNames;
                break;
            case Gender.None:
                Debug.LogError("Gender.None cannot GetRandomNameByGender");
                return new NameData();
        }

        // 确保有未使用的名字
        if (unusedNames.Count == 0)
        {
            Debug.LogError("没有更多可用的名字！");
            return null;
        }

        // 从 unusedNames 中随机选择一个名字的 ID
        int randomIndex = UnityEngine.Random.Range(0, unusedNames.Count);
        NameData selectedName = unusedNames.GetValueByIndex(randomIndex);

        // 记录已使用的名字，并从 unusedNames 中移除
        usedNamesSet.Add(selectedName);
        unusedNames.Remove(selectedName.ID);

        return selectedName;
    }

    // 重置已用的名字，将它们添加回未使用列表
    public void Reset()
    {
        // 将 usedMaleNames 和 usedFemaleNames 中的名字重新加入到 unused 列表
        foreach (var name in usedMaleNames)
        {
            unusedMaleNames.Add(name.ID, name);
        }
        foreach (var name in usedFemaleNames)
        {
            unusedFemaleNames.Add(name.ID, name);
        }

        // 清空 used 名单
        usedMaleNames.Clear();
        usedFemaleNames.Clear();
    }
}