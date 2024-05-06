using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChessboardSlotColor
{
    none = 0,
    green = 1,
    red = 2,
    yellow = 3,

}

public class UIChessboardSlot : MonoBehaviour
{
    public Vector2 position;
    public Image colorImage;
    // Start is called before the first frame update
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(ChessboardSlotColor color)
    {
        switch (color)
        {
            case ChessboardSlotColor.none:
                colorImage.color = GameUtil.Instance.hexToColor("#FFFFFF", 0f);
                break;
            case ChessboardSlotColor.green:
                colorImage.color = GameUtil.Instance.hexToColor("#CAFFBF", 0.5f);
                break;
            case ChessboardSlotColor.red:
                colorImage.color = GameUtil.Instance.hexToColor("#FFADAD", 0.5f);
                break;
            case ChessboardSlotColor.yellow:
                colorImage.color = GameUtil.Instance.hexToColor("#FFD6A5", 0.5f);
                break;
        }
    }
}
