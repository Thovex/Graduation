using UnityEngine;

namespace Thovex.WFC
{
    public struct CompatibilityRule
    {
        public string Current { get; set; }
        public string NextInDirection { get; set; }
        public Vector3Int Direction { get; set; }

        public CompatibilityRule(string _current, string _nextInDirection, Vector3Int _direction)
        {
            Current = _current;
            NextInDirection = _nextInDirection;
            Direction = _direction;
        }
    }


}