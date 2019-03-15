using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thovex.WFC
{
    public class CompatibilityOracle
    {
        public List<CompatibilityRule> Data { get; set; }

        public CompatibilityOracle(List<CompatibilityRule> _data)
        {
            Data = _data;
        }

        public bool Check(CompatibilityRule ruleToCheck)
        {
            return Data.Contains(ruleToCheck);
        }
    }
}