using System.Collections.Generic;
using UnityEngine;

public struct Coefficient
{
    public Coefficient(Dictionary<string, List<Possibility>> keys)
    {
        Keys = keys;
    }

    [SerializeField] public Dictionary<string, List<Possibility>> Keys { get; set; }
}