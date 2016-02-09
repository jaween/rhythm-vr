using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TimingsParser {

    private static char[] lineDelimiters = new char[] { '\n' };
    private static char[] tokenDelimeters = new char[] { ',' };

    private static float previousTime = 0;
    private static List<TimingsManager.Timing> timings;

    public static List<TimingsManager.Timing> ParseTimingsFormat(string text, List<string> possibleEvents)
    {
        timings = new List<TimingsManager.Timing>();

        previousTime = 0;
        var lines = text.Replace(" ", "").Replace("\r", "").Split(lineDelimiters);
        foreach (var line in lines)
        {
            var tokens = line.Split(tokenDelimeters);
            if (tokens.Length > 1)
            {
                // Skips blank tokens
                if (tokens[1] == "")
                {
                    continue;
                }

                if (possibleEvents.Contains(tokens[1]))
                {
                    // TODO(jaween): Get all events from line
                    var events = new List<string>();
                    events.Add(tokens[1]);
                    AddTimingFromString(tokens[0], events);
                }
                else
                {
                    Debug.Log("Unknown timing event: '" + tokens[1] + "'");
                }
            }
            else
            {
                AddTimingFromString(tokens[0], null);
            }
        }

        return timings;
    }

    private static void AddTimingFromString(string timingString, List<string> events)
    {
        float timingFloat;
        if (StringToFloat(timingString, out timingFloat))
        {
            TimingsManager.Timing timing = new TimingsManager.Timing();
            timing.time = timingFloat;
            timing.triggers = events;
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
