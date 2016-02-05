using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Timings 
{
    private const float leeway = 0.5f;
    private const float leewayMiss = 0.5f;

    private int currentTimingIndex = -1;
    private float currentTiming = -1;
    private bool finished = false;

    private List<float> timings;

    public enum TimingResult
    {
        GOOD, BAD, MISS, IGNORE_ATTEMPT
    }

    public Timings(List<float> timings)
    {
        this.timings = timings;
        moveToNextTiming();
    }

    private bool moveToNextTiming()
    {
        currentTimingIndex++;
        if (currentTimingIndex < timings.Count)
        {
            currentTiming = timings[currentTimingIndex];
            return true;
        }

        // No more timings
        finished = true;
        currentTimingIndex = -1;
        currentTiming = -1;
        return false;
    }

    public TimingResult checkForMiss(float ellapsedTime)
    {
        if (!finished && ellapsedTime > currentTiming + leeway + leewayMiss)
        {
            moveToNextTiming();
            return TimingResult.MISS;
        }
        return TimingResult.IGNORE_ATTEMPT;
    }

    public TimingResult checkAttempt(float ellapsedTime)
    {
        if (finished)
        {
            // TODO(jaween): Separate out player timings and level timings
            return TimingResult.IGNORE_ATTEMPT;
        } 
        else if (ellapsedTime > currentTiming - leeway && ellapsedTime < currentTiming + leeway)
        {
            moveToNextTiming();
            return TimingResult.GOOD;
        }
        else if (ellapsedTime > currentTiming - leeway - leewayMiss && 
            ellapsedTime < currentTiming + leeway + leewayMiss)
        {
            moveToNextTiming();
            return TimingResult.BAD;
        }
        else
        {
            return TimingResult.IGNORE_ATTEMPT;
        }
    }
}
