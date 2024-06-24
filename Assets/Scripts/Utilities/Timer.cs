using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public Dictionary<string, int> timers = new Dictionary<string, int>();
    public Dictionary<string, int> loopTimes = new Dictionary<string, int>();

    public bool CreateTimer(string id, int loopTime)
    {
        if (timers.ContainsKey(id) && loopTimes.ContainsKey(id))
        {
            return false;
        }
        else
        {
            timers[id] = 0;
            loopTimes[id] = loopTime;
            return true;
        }
    }

    public bool TimerNext(string id)
    {
        if (timers.ContainsKey(id))
        {
            timers[id]++;
            return timers[id] % loopTimes[id] == 1;
        } else
        {
            return false;
        }
    }
}
