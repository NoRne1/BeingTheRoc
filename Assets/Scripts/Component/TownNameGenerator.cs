using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TownNameGenerator
{
    // 用于存储未使用的城镇名字（通过 ID 快速查找）
    private Dictionary<int, NameData> unusedTownNames = new Dictionary<int, NameData>();

    // 用于记录已经选择的城镇名字，避免重复（存储完整的 NameData）
    private HashSet<NameData> usedTownNames = new HashSet<NameData>();

    // 读取文件并加载城镇名字数据
    public void LoadTownNameData(string filePath)
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
            if (parts.Length == 2)
            {
                string chineseName = parts[0].Trim();
                string englishName = parts[1].Trim();

                // 为每个城镇名字分配一个唯一的 ID
                NameData townName = new NameData(id++, chineseName, englishName, Gender.None);
                unusedTownNames.Add(townName.ID, townName);
            }
        }
    }

    // 随机选择一个未使用的城镇名字
    public NameData GetRandomTownName()
    {
        // 确保有未使用的名字
        if (unusedTownNames.Count == 0)
        {
            Debug.LogError("没有更多可用的城镇名字！");
            return null;
        }

        // 从 unusedTownNames 中随机选择一个名字的 ID
        int randomIndex = UnityEngine.Random.Range(0, unusedTownNames.Count);
        NameData selectedName = unusedTownNames.GetValueByIndex(randomIndex);

        // 记录已使用的名字，并从 unusedTownNames 中移除
        usedTownNames.Add(selectedName);
        unusedTownNames.Remove(selectedName.ID);

        return selectedName;
    }

    // 重置已用的名字，将它们添加回未使用列表
    public void Reset()
    {
        // 将 usedTownNames 中的名字重新加入到 unused 列表
        foreach (var name in usedTownNames)
        {
            unusedTownNames.Add(name.ID, name);
        }

        // 清空 used 名单
        usedTownNames.Clear();
    }
}
