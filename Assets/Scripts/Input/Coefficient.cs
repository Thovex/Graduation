using System.Collections.Generic;
using UnityEngine;

public struct Coefficient
{
    public Coefficient(Dictionary<string, List<Possibility>> allowedBits)
    {
        AllowedBits = allowedBits;
    }

    [SerializeField] public Dictionary<string, List<Possibility>> AllowedBits { get; set; }
}