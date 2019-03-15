using UnityEngine;

namespace Thovex.WFC
{
    public struct CompatibilityRule
    {
        public string Current { get; set; }
        public string NextInDirection { get; set; }
        public Vector2Int Direction { get; set; }

        public CompatibilityRule(string _current, string _nextInDirection, Vector2Int _direction)
        {
            Current = _current;
            NextInDirection = _nextInDirection;
            Direction = _direction;
        }
    }


}