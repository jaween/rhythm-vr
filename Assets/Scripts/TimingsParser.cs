using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TimingsParser {

    private static char[] lineDelimiters = new char[] { '\n' };
    private static char[] tokenDelimeters = new char[] { ',' };

    private static float previousTime = 0;
    private static List<TimingsManager.Timing> timings;

    public static List<TimingsManager.Timing> ParseTimingsFormat(string text, TimingsManager.BaseTriggers possibleTriggers)
    {
        timings = new List<TimingsManager.Timing>();

        previousTime = 0;
        var lines = text.Replace(" ", "").Replace("\r", "").Split(lineDelimiters);
        foreach (var line in lines)
        {
            var tokens = line.Split(tokenDelimeters);
            var triggers = new List<int>();
            if (tokens.Length > 1)
            {
                foreach (var token in tokens)
                {
                    int trigger = possibleTriggers.GetTrigger(tokens[1]);
                    if (trigger != TimingsManager.BaseTriggers.NO_TRIGGER)
                    {
                        triggers.Add(trigger);
                    }
                    else
                    {
                        Debug.Log("Unknown timing event: '" + tokens[1] + "'");
                    }
                }
            }
            AddTimingFromString(tokens[0], triggers);
        }

        return timings;
    }

    private static void AddTimingFromString(string timingString, List<int> triggers)
    {
        float timingFloat;
        if (StringToFloat(timingString, out timingFloat))
        {
            TimingsManager.Timing timing = new TimingsManager.Timing();
            timing.time = timingFloat;
            timing.triggers = triggers;
            timings.Add(timing);
        }
    }

    private static bool StringToFloat(string s, out float f)
    {
        f = 0;
        try
        {
            f = float.Parse(s);
            if (f < previousTime)
            {
                Debug.Log("Error: A time value wasn't in asceding order: " + f);
            }
            previousTime = f;
            return true;
        }
        catch (System.FormatException)
        {
            return false;
        }
    }
}
