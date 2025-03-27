using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStatus
{
    errorTarget = -1,
    normal = 0,
}

public class AttackDisplayResult 
{
    public string attackIdentifier;
    public int attackIndex;
    public string casterID; 
    public string targetID;
    public AttackStatus attackStatus;
    public int damage;
    public int health;
    public bool triggerOtherEffect;
    public StoreItemModel itemModel;

    public AttackDisplayResult(string attackIdentifier, int attackIndex, string casterID, string targetID, AttackStatus attackStatus, int damage, int health, bool triggerOtherEffect, StoreItemModel itemModel)
    {
        this.attackIdentifier = attackIdentifier;
        this.attackIndex = attackIndex;
        this.casterID = casterID;
        this.targetID = targetID;
        this.attackStatus = attackStatus;
        this.damage = damage;
        this.health = health;
        this.triggerOtherEffect = triggerOtherEffect;
        this.itemModel = itemModel;
    }
}

public class NormalAttackResult 
{
    public string attackIdentifier;
    public List<AttackDisplayResult> displayResults;
    public int attackTime;

    public NormalAttackResult(int attackTime)
    {
        attackIdentifier = GameUtil.Instance.GenerateUniqueId();
        displayResults = new List<AttackDisplayResult>();
        this.attackTime = attackTime;
    }

    public NormalAttackResult(string attackIdentifier, List<AttackDisplayResult> displayResults, int attackTime)
    {
        this.attackIdentifier = attackIdentifier;
        this.displayResults = displayResults;
        this.attackTime = attackTime;
    }
}
