using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI
{
    public abstract IEnumerator TurnAction(string uuid);
}

public class TankAI: EnemyAI
{
    public override IEnumerator TurnAction(string uuid)
    {
        Debug.Log("TankAI TurnAction");
        yield return new WaitForSeconds(1f);
        BattleManager.Instance.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class WarriorAI : EnemyAI
{
    public override IEnumerator TurnAction(string uuid)
    {
        Debug.Log("WarriorAI TurnAction");
        yield return new WaitForSeconds(1f);
        BattleManager.Instance.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class AssassinAI: EnemyAI
{
    public override IEnumerator TurnAction(string uuid)
    {
        Debug.Log("AssassinAI TurnAction");
        yield return new WaitForSeconds(1f);
        BattleManager.Instance.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class MagicianAI : EnemyAI
{
    public override IEnumerator TurnAction(string uuid)
    {
        Debug.Log("MagicianAI TurnAction");
        yield return new WaitForSeconds(1f);
        BattleManager.Instance.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class PastorAI : EnemyAI
{
    public override IEnumerator TurnAction(string uuid)
    {
        Debug.Log("PastorAI TurnAction");
        yield return new WaitForSeconds(1f);
        BattleManager.Instance.roundTime.OnNext((uuid, RoundTime.end));
    }
}
