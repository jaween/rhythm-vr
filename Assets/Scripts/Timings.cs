using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Timings 
{
    private const float leeway = 500;
    private const float leewayMiss = 250;

    private int currentTimingIndex = -1;
    private float currentTiming;

    private List<float> timings;

    public enum TimingResult
    {
        GOOD, BAD, MISS, IGNORE_ATTEMPT
    }

    Timings(List<float> timings)
    {
        this.timings = timings;
        moveToNextTiming();
    }

    private void moveToNextTiming()
    {
        currentTimingIndex++;
        currentTiming = timings[currentTimingIndex];
    }

    public TimingResult checkForMiss(float ellapsedTime)
    {
        if (ellapsedTime > currentTiming + leeway + leewayMiss)
        {
            moveToNextTiming();
            return TimingResult.MISS;
        }
        return TimingResult.IGNORE_ATTEMPT;
    }

    public TimingResult checkAttempt(float ellapsedTime)
    {
        if (ellapsedTime > currentTiming - leeway && ellapsedTime < currentTiming + leeway)
        {
            moveToNextTiming();
            return TimingResult.GOOD;
        }
        else if (ellapsedTime > currentTiming - leeway - leewayMiss && ellapsedTime < currentTiming + leeway + leewayMiss)
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
