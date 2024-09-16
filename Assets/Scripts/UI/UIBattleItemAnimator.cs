using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIBattleItemAnimator : MonoBehaviour
{
    public bool positiveOrNegative;
    public Animator animator;
    public UIBattleItem uIBattleItem;

    public void ProcessPositiveResults()
    {
        if (positiveOrNegative) 
        {
            if (uIBattleItem.attackResult != null)
            {
                foreach (var result in uIBattleItem.attackResult.displayResults.Where(result => result.attackIndex == uIBattleItem.currentDisplayResultIndex).ToList())
                {
                    // 处理当前 displayResult
                    BattleCommonMethods.ProcessNormalAttack(result);
                }
                uIBattleItem.currentDisplayResultIndex++;
            }
        }
    }

    // 处理队列中的攻击结果
    public void ProcessNegativeResults()
    {
        if (!positiveOrNegative) 
        {
            if (uIBattleItem.currentDisplayResult != null)
            {
                uIBattleItem.Damage(uIBattleItem.currentDisplayResult);
            }
        }
    }
}
