using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimerType
{
    normal = 0,
    round = 1,
}

public class Timer
{
    public Dictionary<string, int> normalTimers = new Dictionary<string, int>();
    public Dictionary<string, (int, bool)> roundTimers = new Dictionary<string, (int, bool)>();
    public Dictionary<string, int> loopTimes = new Dictionary<string, int>();
    public Dictionary<string, int> round = new Dictionary<string, int>();

    public Timer()
    {

    }

    public void Clean()
    {
        normalTimers.Clear();
        roundTimers.Clear();
        loopTimes.Clear();
        round.Clear();
    }

    public void NextRound(string uuid)
    {
        if (uuid != null && round.ContainsKey(uuid))
        {
            round[uuid]++;
        }
    }

    //loopTime = 冷却时间+1
    public bool CreateTimer(TimerType type, string id, int loopTime)
    {
        switch (type)
        {
            case TimerType.normal:
                if (normalTimers.ContainsKey(id) && loopTimes.ContainsKey(id))
                {
                    return false;
                }
                else
                {
                    normalTimers[id] = 0;
                    loopTimes[id] = loopTime;
                    return true;
                }
            case TimerType.round:
                if (roundTimers.ContainsKey(id) && loopTimes.ContainsKey(id))
                {
                    return false;
                }
                else
                {
                    round.Add(id, 0);
                    roundTimers[id] = (round[id], false);
                    loopTimes[id] = loopTime;
                    return true;
                }
            default:
                return false;
        }
    }

    public bool TimerNext(string id)
    {
        if (normalTimers.ContainsKey(id))
        {
            normalTimers[id]++;
            return normalTimers[id] % loopTimes[id] == 1;
        } else if (roundTimers.ContainsKey(id))
        {
            if (!roundTimers[id].Item2 || (round[id] - roundTimers[id].Item1) % loopTimes[id] == loopTimes[id] - 1)
            {
                // 没有触发过 或者 上次触发过，但是已经过了冷却回合数
                roundTimers[id] = (round[id], true);
                return true;
            } else
            {
                return false;
            }
        } else
        {
            return false;
        }
    }
}
