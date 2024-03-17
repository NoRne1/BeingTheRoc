using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TownNodeType
{
    king = 0,
    town3 = 1,
    town2 = 2,
    town1 = 3
}

public class UITownNode : MonoBehaviour
{
    public TownNodeType type;
    public int townID;
    public Image character_icon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerIsThere(bool isThere)
    {
        if (isThere)
        {
            character_icon.gameObject.SetActive(true);
        } else
        {
            character_icon.gameObject.SetActive(false);
        }
    }

    public void GoNextTown()
    {
        MapManager.Instance.GoNextTown(townID);
    }
}
