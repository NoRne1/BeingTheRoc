using UnityEngine;
using System.Collections;

public class AttributeDynamic
{
    public int currentHP { get; set; }
    public int lostHP { get; set; }
    public int currentShield { get; set; }
    public int currentEnergy { get; set; }

    public AttributeDynamic(int currentHP, int lostHP, int currentShield, int currentEnergy) 
    {
        this.currentHP = currentHP;
        this.lostHP = lostHP;
        this.currentShield = currentShield;
        this.currentEnergy = currentEnergy;
    }
}

