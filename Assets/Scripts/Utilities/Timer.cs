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
    public int round;

    public Timer()
    {
        round = 0;
    }

    public void Clean()
    {
        normalTimers.Clear();
        roundTimers.Clear();
        loopTimes.Clear();
    }

    public void NextRound()
    {
        round++;
    }

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
                    roundTimers[id] = (round, false);
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
            if (!roundTimers[id].Item2 || (round - roundTimers[id].Item1) % loopTimes[id] == loopTimes[id] - 1)
            {
                // 没有触发过 或者 上次触发过，但是已经过了冷却回合数
                roundTimers[id] = (round, true);
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