using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NightWalkTriggers : TimingsManager.BaseTriggers 
{
    // TODO(jaween): Maybe make a class that holds these enum, that way we only have to up-cast from the base class to the enum holder class
    public const int POP = 0;
    public const int BEAT = 1;
    public const int ROLL_AUDIO = 2;
    public const int START_ROLL = 3;
    public const int END_ROLL = 4;
    public const int GAP = 5;
    public const int RAISES = 6;
    public const int RAISED = 7;
    public const int RESULT = 8;

    private static Dictionary<string, int> mapping = new Dictionary<string, int>
    {
        { "pop", POP },
        { "beat", BEAT },
        { "roll_audio", ROLL_AUDIO },
        { "start_roll", START_ROLL },
        { "end_roll", END_ROLL },
        { "gap", GAP },
        { "raises", RAISES },
        { "raised", RAISED },
        { "result", RESULT }
    };

    public override int GetTrigger(string triggerString)
    {
        if (mapping.ContainsKey(triggerString))
        {
            return mapping[triggerString];
        }
        else
        {
            return -1;
        }
    }

    // TODO(jaween): Put this in the base class
    /*private static IEnumerable<T> GetEnumValues<T>()
    {
        return (T[])Enum.GetValues(typeof(T));
    }*/
}
