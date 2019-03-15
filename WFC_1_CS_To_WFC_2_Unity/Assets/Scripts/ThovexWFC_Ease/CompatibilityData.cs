using UnityEngine;
using System.Collections.Generic;

namespace Thovex.WFC
{
    public struct CompatibilityData
    {
        public List<CompatibilityRule> Compatibilities { get; set; }
        public Dictionary<string, int> Weights { get; set; }

        public CompatibilityData(Dictionary<string, int> _weights, List<CompatibilityRule> _compatibilities)
        {
            Weights = _weights;
            Compatibilities = _compatibilities;
        }
    }
}