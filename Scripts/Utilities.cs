using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public static class Extensions
    {
        public static GameObject GetChildByName(this GameObject gameObject, string name)
        {
            GameObject obj = null;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if(child.name == name)
                {
                    obj = child.gameObject;
                }
            }

            return obj;
        }
    }

    public static class Utilities
    {
        public static GameObject ClosestGameobject(GameObject[] gameObjects, Vector3 pos)
        {
            if (gameObjects.Length == 0)
            {
                Debug.LogError("Given List was empty");
                return null;
            }

            float dist = Mathf.Infinity;
            GameObject closestObject = gameObjects[0];

            if(gameObjects.Length != 1)
            {
                foreach (GameObject obj in gameObjects)
                {
                    float currDist = (obj.transform.position - pos).sqrMagnitude;
                    if (currDist < dist)
                    {
                        dist = currDist;
                        closestObject = obj;
                    }
                }
            }

            return closestObject;
        }

        public static GameObject ClosestGameobject(List<GameObject> gameObjects, Vector3 pos)
        {
            if (gameObjects.Count == 0)
            {
                Debug.LogError("Given List was empty");
                return null;
            }

            float dist = Mathf.Infinity;
            GameObject closestObject = gameObjects[0];

            if (gameObjects.Count != 1)
            {
                foreach (GameObject obj in gameObjects)
                {
                    float currDist = (obj.transform.position - pos).sqrMagnitude;
                    if (currDist < dist)
                    {
                        dist = currDist;
                        closestObject = obj;
                    }
                }
            }

            return closestObject;
        }

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
                    driver = new ActiveJointDriver();
                    break;
                case TrackingMode.PassiveJoint:
                    driver = new PassiveJointDriver();
                    break;
                default:
                    Debug.LogError("No matching TrackDriver was setup for the given trackingMode enum, defaulting to a Kinematic Driver. Define a matching TrackDriver and declare it in Utilities.cs");
                    break;
            }

            return driver;
        }

        public static FingerDriver FingerDriverFromEnum(FingerTrackingMode fingerTrackingMode)
        {
            //Defaulting to Kinematic Driver
            FingerDriver driver = new KinematicFingerDriver();

            switch (fingerTrackingMode)
            {
                case FingerTrackingMode.Kinematic:
                    driver = new KinematicFingerDriver();
                    break;
                case FingerTrackingMode.CollisionTest:
                    driver = new CollisionTestDriver();
                    break;
                default:
                    Debug.LogError("No matching FingerDriver was setup for the given FingerTrackingMode enum, defaulting to a Kinematic Driver. Define a matching FingerDriver and declare it in Utilities.cs");
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
