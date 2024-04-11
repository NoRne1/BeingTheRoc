using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoSingleton<EquipManager>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(CharacterModel character, StoreItemModel item)
    {
        if (item.CanUse())
        {

        }
    }

    public void Equip(CharacterModel character, StoreItemModel item)
    {

    }

    public void Unequip(CharacterModel character, StoreItemModel item)
    {

    }
}
