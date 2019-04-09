using System;
using System.Collections.Generic;
using UnityEngine;

public struct Coefficient
{
    [SerializeField] private HashSet<string> _allowedBits;
    public HashSet<string> AllowedBits { get => _allowedBits;
        set {
            _allowedBits = value;
        }
    }

    public void Initialize()
    {
        AllowedBits = new HashSet<string>();
    }

    public string Print()
    {
        string bitDisplay = "";

        foreach (string s in _allowedBits)
        {
            bitDisplay += "{" + s + "}";
        }

        return bitDisplay;
    }
}