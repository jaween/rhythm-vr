using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        GOOD, BAD, MISS, IGNORE_ATTEMPT
    }

    public TimingsManager(TextAsset timingTextAsset, List<string> events)
    {
        timings = TimingsParser.ParseTimingsFormat(timingTextAsset.text, events);
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

    public TimingResult checkForMiss(float ellapsedTime, out List<string> triggers)
    {
        Timing timing = timings[nextPlayerTimingIndex];
        float nextPlayerTiming = timing.time;
        triggers = timing.triggers;
        if (!finished && ellapsedTime > nextPlayerTiming + leeway + leewayMiss)
        {
            moveToNextPlayerTiming();
            return TimingResult.MISS;
        }
        return TimingResult.IGNORE_ATTEMPT;
    }

    public TimingResult checkAttempt(float ellapsedTime, out List<string> triggers)
    {
        Timing timing = timings[nextPlayerTimingIndex];
        float nextPlayerTiming = timing.time;
        triggers = timing.triggers;
        if (finished)
        {
            return TimingResult.IGNORE_ATTEMPT;
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
            return TimingResult.IGNORE_ATTEMPT;
        }
    }

    public List<string> checkForTrigger(float ellapsedTime)
    {
        List<string> triggers = new List<string>();
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

    // TODO(jaween): Make this a struct?
    public class Timing
    {
        // TODO(jaween): Generate C# class with enums or const ints instread of strings
        public float time;
        public List<string> triggers;

    }
}
