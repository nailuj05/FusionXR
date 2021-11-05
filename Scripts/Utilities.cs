using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if(gameObject.TryGetComponent<T>(out T t))
            {
                return t;
            }
            else
            {
                return gameObject.AddComponent<T>();
            }
        }

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
        #region Matching
        public static bool ObjectMatchesTags(GameObject obj, string[] tags)
        {
            //When no tag mask is set
            if (tags.Length == 0) return true;

            foreach (string tag in tags)
            {
                if (obj.tag == tag) return true;
            }

            return false;
        }

        public static bool ObjectMatchesLayermask(GameObject obj, LayerMask mask)
        {
            if (mask == ~0) return true;

            if (mask == (mask | (1 << obj.layer)))
            {
                return true;
            }
            else return false;
        }

        public static bool ObjectMatchesAttractType(GameObject obj, AttractType attractType)
        {
            //Dont attach hands
            if (obj.TryGetComponent<FusionXRHand>(out FusionXRHand hand))
            {
                return false;
            }

            if (attractType == AttractType.Grabables)
            {
                return obj.TryGetComponent<Grabable>(out Grabable g);
            }
            else if (attractType == AttractType.Rigidbodys)
            {
                return obj.TryGetComponent<Rigidbody>(out Rigidbody r);
            }
            else //if (attractType == AttractType.AllCollisionObjects)
            {
                return true;
            }
        }
        #endregion

        #region ClosestObject
        public static GameObject ClosestGameobject(GameObject[] gameObjects, Vector3 pos)
        {
            if (gameObjects.Length == 0)
            {
                Debug.LogError("Given List was empty");
                return null;
            }

            float dist = Mathf.Infinity;
            GameObject closestObject = gameObjects[0];

            if (gameObjects.Length != 1)
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
        #endregion

        #region Drivers
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
                case TrackingMode.Force:
                    driver = new ForceDriver();
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
        #endregion

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
