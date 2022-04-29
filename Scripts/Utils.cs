using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    #if UNITY_EDITOR
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif

    #region ConfigJointExtensions

    //Grabbed from https://gist.github.com/mstevenson/4958837#file-configurablejointextensions-cs-L13
    public static class ConfigurableJointExtensions
    {
        /// <summary>
        /// Sets a joint's targetRotation to match a given local rotation.
        /// The joint transform's local rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
            }
            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        /// <summary>
        /// Sets a joint's targetRotation to match a given world rotation.
        /// The joint transform's world rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
        {
            if (!joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
            }
            SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
        }

        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion target, Quaternion startRot, Space space)
        {
            Vector3 right = joint.axis;
            Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            Quaternion localToJointSpace = Quaternion.LookRotation(forward, up);
            if (space == Space.World)
            {
                Quaternion worldToLocal = Quaternion.Inverse(joint.transform.parent.rotation);
                target = worldToLocal * target;
            }
            joint.targetRotation = Quaternion.Inverse(localToJointSpace) * Quaternion.Inverse(target) * startRot * localToJointSpace;
        }

        static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
        {
            // Calculate the rotation expressed by the joint's axis and secondary axis
            var right = joint.axis;
            var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

            // Transform into world space
            Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

            // Counter-rotate and apply the new local rotation.
            // Joint space is the inverse of world space, so we need to invert our value
            if (space == Space.World)
            {
                resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
            }
            else
            {
                resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
            }

            // Transform back into joint space
            resultRotation *= worldToJointSpace;

            // Set target rotation to our newly calculated rotation
            joint.targetRotation = resultRotation;
        }
    } 
    #endregion

    public static class Extensions
    {
        public static void FlipCheck(this Quaternion q)
        {
            if (q.w < 0)
            {
                q.x = -q.x;
                q.y = -q.y;
                q.z = -q.z;
                q.w = -q.w;
            }
        }

        public static Quaternion InverseTransformRotation(this Transform t, Quaternion rot)
        {
            return rot * Quaternion.Inverse(t.rotation);
        }

        public static List<Transform> GetParentsFromTo(this Transform transform, Transform to)
        {
            List<Transform> transforms = new List<Transform>();
            Transform currentParent = transform;

            while(currentParent != to && currentParent.parent != null)
            {
                currentParent = currentParent.parent;
                Debug.Log(currentParent);
                transforms.Add(currentParent);
            }

            return transforms;
        }

        public static Vector3 ClampVector(this Vector3 vector, float maxLength)
        {
            if (vector.magnitude < maxLength) return vector;

            return vector.normalized * maxLength;
        }

        public static void TryDestroyComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out T t))
            {
                if(Application.isPlaying)
                {
                    Object.Destroy(t);
                }
                else
                {
                    Object.DestroyImmediate(t);
                }
            }
        }

        public static T GetComponentInAllChildren<T>(this Transform transform) where T : Component
        {
            if (transform.TryGetComponent<T>(out T t))
            {
                return t;
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);

                    var c = child.GetComponentInAllChildren<T>();

                    if(c != null)
                    {
                        return c;
                    }
                }
            }

            return null;
        }

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

        public static GameObject GetChildByName(this GameObject gameObject, string name, [Optional] bool recursive)
        {
            GameObject obj = null;

            if (recursive == true)
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);

                    if (child.name == name)
                    {
                        obj = child.gameObject;
                        break;
                    }

                    var possibleObj = child.gameObject.GetChildByName(name, true);

                    if (possibleObj)
                    {
                        obj = possibleObj;
                    }
                }
            }
            else
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);

                    if (child.name == name)
                    {
                        obj = child.gameObject;
                    }
                }
            }

            return obj;
        }
    }

    public static class Utils
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

            if (attractType == AttractType.Grabbables)
            {
                return obj.TryGetComponent<IGrabbable>(out IGrabbable g);
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

        public static Vector3 ClosestPointOnLine(Vector3 linePnt, Vector3 lineDir, float lineLength, Vector3 pnt)
        {
            lineDir.Normalize();
            var v = pnt - linePnt;
            var d = Vector3.Dot(v, lineDir);
            return linePnt + (lineDir * d).ClampVector(lineLength * 0.5f);
        }

        public static GrabPoint ClosestGrabPoint(IGrabbable grabbable, Vector3 point, Transform handTransform, Hand desiredHand)
        {
            GrabPoint closestGrabPoint = null;
            float distance = float.MaxValue;

            if (grabbable.grabPoints != null)
            {
                foreach (GrabPoint currentGrabPoint in grabbable.grabPoints)
                {
                    if (currentGrabPoint.IsGrabPossible(handTransform, desiredHand, grabbable.twoHandedMode)) //Check if the GrabPoint is for the correct Hand and if it isActive
                    {
                        if ((currentGrabPoint.transform.position - point).sqrMagnitude < distance) //Check if next Point is closer than last Point
                        {
                            closestGrabPoint = currentGrabPoint;
                            distance = (currentGrabPoint.transform.position - point).sqrMagnitude; //New (smaller) distance
                        }
                    }
                }
            }
            return closestGrabPoint;
        }

        public static GameObject ClosestGameObject(GameObject[] gameObjects, Vector3 pos)
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

        public static GameObject ClosestGameObject(List<GameObject> gameObjects, Vector3 pos)
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
                case TrackingMode.FixedJoint:
                    driver = new FixedJointDriver();
                    break;
                case TrackingMode.PDForce:
                    driver = new PDForceDriver();
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
                case FingerTrackingMode.Joint:
                    driver = new JointDriver();
                    break;
                default:
                    Debug.LogError("No matching FingerDriver was setup for the given FingerTrackingMode enum, defaulting to a Kinematic Driver. Define a matching FingerDriver and declare it in Utilities.cs");
                    break;
            }

            return driver;
        }
        #endregion

        #region Collision
        public static Collider[] CheckBoxCollider(Transform transform, BoxCollider boxCollider)
        {
            Vector3 boxCenter = transform.TransformPoint(boxCollider.center);

            return Physics.OverlapBox(boxCenter, boxCollider.size / 2, transform.rotation);
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

    [System.Serializable]
    public class PID
    {
        public float P, I, D;

        public PID(float P, float I, float D)
        {
            this.P = P;
            this.I = I;
            this.D = D;
        }

        Vector3 current;
        public Vector3 CalcVector(Vector3 setPoint, Vector3 actualPoint, float deltaTime)
        {
            current.Set(
                CalcScalar(setPoint.x, actualPoint.x, deltaTime),
                CalcScalar(setPoint.y, actualPoint.y, deltaTime),
                CalcScalar(setPoint.z, actualPoint.z, deltaTime));
            return current;
        }

        float present, derivative, lastError, integral;
        public float CalcScalar(float setPoint, float actual, float deltaTime)
        {
            present = setPoint - actual;
            integral += present * deltaTime;
            lastError = present;
            derivative = (present - lastError) / deltaTime;
            return present * P + integral * I + derivative * D;
        }
    }
}