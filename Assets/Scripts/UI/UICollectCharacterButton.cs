using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICollectCharacterButton : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public TextMeshProUGUI price;
    public CollectCharacterInfo info;
    
    // Start is called before the first frame update
    public void Setup(int index)
    {
        if(index == 0)
        {
            info = new CollectCharacterInfo(0,"普通召集",5,10,20,65,2,500);
            title.text = info.title;
            desc.text = info.GetDisplayDesc();
            price.text = info.price.ToString();
        } else if(index == 1)
        {
            info = new CollectCharacterInfo(1,"快速召集",5,10,20,65,1,800);
            title.text = info.title;
            desc.text = info.GetDisplayDesc();
            price.text = info.price.ToString();
        } else if(index == 2)
        {
            info = new CollectCharacterInfo(2,"特别召集",10,20,30,40,2,1200);
            title.text = info.title;
            desc.text = info.GetDisplayDesc();
            price.text = info.price.ToString();
        } else 
        {
            Debug.LogError("UICollectCharacterButton Setup index error");
        }
    }
}

public class CollectCharacterInfo
{
    public int ID;
    public string title;
    public int redRate;
    public int blueRate;
    public int greenRate;
    public int whiteRate;
    public int waitTime;
    public int price;
    
    string descFormat = "红：{0}%\\n蓝：{1}%\\n绿：{2}%\\n经典时尚：{3}%\\n等待时间：{4}d\\n";
    
    public CollectCharacterInfo(int ID, string title, int redRate, int blueRate, int greenRate, int whiteRate, int waitTime, int price)
    {
        this.ID = ID;
        this.title = title;
        this.redRate = redRate;
        this.blueRate = blueRate;
        this.greenRate = greenRate;
        this.whiteRate = whiteRate;
        this.waitTime = waitTime;
        this.price = price;
    }

    public string GetDisplayDesc()
    {
        object[] args = new object[5];
        args[0] = redRate;
        args[1] = blueRate;
        args[2] = greenRate;
        args[3] = whiteRate;
        args[4] = waitTime;
        return string.Format(descFormat, args).ReplaceNewLines();;
    }
}
