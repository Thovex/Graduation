using System;
using System.Collections.Generic;
using UnityEngine;

public class Coefficient
{
    [SerializeField] public Dictionary<Pattern, bool> allowedPatterns = new Dictionary<Pattern, bool>();

    public Coefficient(Dictionary<Pattern, bool> allowedDict)
    {
        allowedPatterns = allowedDict;
    }

    public int AllowedCount ()
    {
        int allowedCount = 0;

        foreach (var pair in allowedPatterns)
        {
            if (pair.Value)
            {
                allowedCount++;
            }
        }

        return allowedCount;
    }

    public List<Pattern> GetValidPatterns()
    {
        List<Pattern> validPatterns = new List<Pattern>();

        foreach (var pair in allowedPatterns)
        {
            if (pair.Value)
            {
                validPatterns.Add(pair.Key);
            }
        }

        return validPatterns;
    }

    public Pattern GetLastAllowedPattern()
    {
        if (AllowedCount() == 1)
        {
            foreach (var pair in allowedPatterns)
            {
                if (pair.Value)
                {
                    return pair.Key;
                }
            }
        }

        Debug.LogError("Can't be here");
        return null;
    }
}