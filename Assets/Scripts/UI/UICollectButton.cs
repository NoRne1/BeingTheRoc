using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICollectButton : MonoBehaviour
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
            info = new CollectCharacterInfo(0,"普通召集",0.05f,0.1f,0.2f,0.65f,2,500);
            title.text = info.title;
            desc.text = info.GetDisplayDesc();
            price.text = info.price.ToString();
        } else if(index == 1)
        {
            info = new CollectCharacterInfo(1,"快速召集",0.05f,0.1f,0.2f,0.65f,1,800);
            title.text = info.title;
            desc.text = info.GetDisplayDesc();
            price.text = info.price.ToString();
        } else if(index == 2)
        {
            info = new CollectCharacterInfo(2,"特别召集",0.1f,0.2f,0.3f,0.4f,2,1200);
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
    public float redRate;
    public float blueRate;
    public float greenRate;
    public float whiteRate;
    public int waitTime;
    public int price;
    
    string descFormat = "红：{0}%\\n蓝：{1}%\\n绿：{2}%\\n经典时尚：{3}%\\n等待时间：{4}d\\n";
    
    public CollectCharacterInfo(int ID, string title, float redRate, float blueRate, float greenRate, float whiteRate, int waitTime, int price)
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
        args[0] = (int)(redRate * 100);
        args[1] = (int)(blueRate * 100);
        args[2] = (int)(greenRate * 100);
        args[3] = (int)(whiteRate * 100);
        args[4] = waitTime;
        return string.Format(descFormat, args).ReplaceNewLines();;
    }
}
