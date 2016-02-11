using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TimingsManager 
{
    private const float leeway = 0.1f;
    private const float leewayMiss = 0.08f;

    private int nextPlayerTimingIndex = -1;
    private int nextTriggerTimingIndex = -1;
    private bool finished = false;

    private List<Timing> timings;

    public enum TimingResult
    {
        GOOD, BAD, MISS, NO_BEAT
    }

    public TimingsManager(TextAsset timingsTextAsset, BaseTriggers possibleTriggers)
    {
        timings = TimingsParser.ParseTimingsFormat(timingsTextAsset.text, possibleTriggers);
        moveToNextPlayerTiming();
        moveToNextTriggerTiming();
    }
    
    private bool moveToNextPlayerTiming()
    {
        nextPlayerTimingIndex++;
        if (nextPlayerTimingIndex >= timings.Count)
        {
            // No more timings
            finished = true;
            nextPlayerTimingIndex = -1;
            return false;
        }
        return true;
    }

    private void moveToNextTriggerTiming()
    {
        nextTriggerTimingIndex++;
        if (nextTriggerTimingIndex >= timings.Count)
        {
            // No more timings
            finished = true;
            nextTriggerTimingIndex = -1;
        }
    }

    public TimingResult checkForMiss(float ellapsedTime, out List<int> triggers)
    {
        Timing timing = timings[nextPlayerTimingIndex];
        float nextPlayerTiming = timing.time;
        triggers = timing.triggers;
        if (!finished && ellapsedTime > nextPlayerTiming + leeway + leewayMiss)
        {
            moveToNextPlayerTiming();
            return TimingResult.MISS;
        }
        return TimingResult.NO_BEAT;
    }

    public TimingResult checkAttempt(float ellapsedTime, out List<int> triggers)
    {
        Timing timing = timings[nextPlayerTimingIndex];
        float nextPlayerTiming = timing.time;
        triggers = timing.triggers;
        if (finished)
        {
            return TimingResult.NO_BEAT;
        } 
        else if (ellapsedTime >= nextPlayerTiming - leeway && 
            ellapsedTime <= nextPlayerTiming + leeway)
        {
            moveToNextPlayerTiming();
            return TimingResult.GOOD;
        }
        else if (ellapsedTime > nextPlayerTiming - leeway - leewayMiss && 
            ellapsedTime < nextPlayerTiming + leeway + leewayMiss)
        {
            moveToNextPlayerTiming();
            return TimingResult.BAD;
        }
        else
        {
            return TimingResult.NO_BEAT;
        }
    }

    public List<int> checkForTrigger(float ellapsedTime)
    {
        List<int> triggers = new List<int>();
        Timing nextTriggerTiming = timings[nextTriggerTimingIndex];
        if (!finished && ellapsedTime > nextTriggerTiming.time)
        {
            triggers = nextTriggerTiming.triggers;
            moveToNextTriggerTiming();
        }
        return triggers;
    }

    public List<Timing> Timings
    {
        get { return timings; }
    }

    public int NextPlayerTimingIndex
    {
        get { return nextPlayerTimingIndex; }
    }

    public int NextTriggerTimingIndex
    {
        get { return nextTriggerTimingIndex; }
    }

    // TODO(jaween): Make this a struct?
    public class Timing
    {
        // TODO(jaween): Generate C# class with enums or const ints instread of strings
        public float time;
        public List<int> triggers;
    }

    // TODO(jaween): Can this and it's devrived classes be static?
    public abstract class BaseTriggers
    {
        public const int NO_TRIGGER = -1;
        public abstract int GetTrigger(string triggerString);
    }
}
