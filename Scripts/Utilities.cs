using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public enum Direction
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3
    }

    public class Utilities : MonoBehaviour
    {
        public static Direction GetDirectionFromVector(Vector2 input)
        {
            var angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

            var absAngle = Mathf.Abs(angle);

            if (absAngle < 45f)
                return Direction.East;
            if (absAngle > 135f)
                return Direction.West;

            return angle >= 0f ? Direction.North : Direction.South;
        }
    }
}
