using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterPlaceBox : MonoBehaviour
{
    public Image characterIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string res)
    {
        characterIcon.overrideSprite = Resloader.LoadSprite(res, ConstValue.battleItemsPath);
    }
}
