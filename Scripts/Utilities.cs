using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public static class Utilities
    {
        public static TrackDriver DriverFromEnum(TrackingMode trackingMode)
        {
            //Defaulting to Kinematic Driver
            TrackDriver driver = new KinematicDriver();

            switch (trackingMode)
            {
                case TrackingMode.Kinematic:
                    driver = new KinematicDriver();
                    break;
                case TrackingMode.Velocity:
                    driver = new VelocityDriver();
                    break;
                case TrackingMode.ActiveJoint:
                    break;
                case TrackingMode.PassiveJoint:
                    break;
                default:
                    Debug.LogError("No matching TrackDriver was setup for the given trackingMode enum, defaulting to a Kinematic Driver. Define a matching TrackDriver and declare it in Utilities.cs");
                    break;
            }

            return driver;
        }

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
