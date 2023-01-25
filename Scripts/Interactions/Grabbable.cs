using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR
{
    public class Grabbable : MonoBehaviour, IGrabbable
    {
        public GrabbableType gripableType = GrabbableType.Interactables;

        #region IGrabbable Implementation
        public Transform Transform { get { return transform; } }
        public GameObject GameObject { get { return gameObject; } }

        [SerializeField]
        TwoHandedModes TwoHandedMode = TwoHandedModes.SwitchHand;
        public TwoHandedModes twoHandedMode { get { return TwoHandedMode; } set { TwoHandedMode = value; } }

        public bool isGrabbed { get; protected set; }

        [SerializeField]
        GrabPoint[] GrabPoints;
        public GrabPoint[] gripPoints { get { return GrabPoints; } set { GrabPoints = value; } }

        public List<FusionXRHand> attachedHands { get; private set; } = new List<FusionXRHand>();
        #endregion

        private Rigidbody rb;
        private RigidbodyInterpolation originalInterpolation;
        private bool originalKinematicMode;

        public bool overrideTrackingMode;

        [SerializeField]
        private TrackingMode customTrackingMode;

        [Header("Events")]
        public UnityEvent OnGrab;
        public UnityEvent OnRelease;

        public UnityEvent OnPinchStart;
        public UnityEvent OnPinchEnd;

        //If 2 Handed:
        private Vector3 posOffset;
        private Vector3 rotOffset;

        private Vector3 refVel;

        #region UnityFunctions

        public virtual void Start()
        {
            try
            {
                SetLayer(gameObject, gripableType.ToString());
            }
            catch
            {
                Debug.LogError("Layers need to be setup correctly!");
            }

            rb = GetComponent<Rigidbody>();
        }

        public virtual void FixedUpdate()
        {
            if (!isGrabbed)
                return;

            //Reset Target Position and Rotation
            Quaternion targetRotation = Quaternion.identity;
            Vector3 targetPosition = Vector3.zero;

            int handsCount = attachedHands.Count;

            if (handsCount == 1) //If there is one hand gripbing
            {
                //Get GrabPoint Offsets
                Vector3 offsetPos = attachedHands[0].gripPosition.localPosition;
                Quaternion offsetRot = attachedHands[0].gripPosition.localRotation;

                //Delta Vector/Quaternion from Grabbable (+ offset) to hand
                targetPosition = attachedHands[0].targetPosition - transform.TransformVector(offsetPos);
                targetRotation = attachedHands[0].targetRotation * Quaternion.Inverse(offsetRot);


                //Apply Target Transformation to hand
                attachedHands[0].gripbedTrackDriver.UpdateTrackFixed(targetPosition, targetRotation);
            }
            else //If there is two hands gripbing 
            {
                Vector3[] posTargets = new Vector3[handsCount];
                Quaternion[] rotTargets = new Quaternion[handsCount];

                for (int i = 0; i < handsCount; i++)
                {
                    //Get GrabPoint Offsets
                    Vector3 offsetPos = attachedHands[i].gripPosition.localPosition;
                    Quaternion offsetRot = attachedHands[i].gripPosition.localRotation;

                    //Delta Vector/Quaternion from Grabbable (+ offset) to hand
                    posTargets[i] = attachedHands[i].targetPosition - transform.TransformVector(offsetPos);
                    rotTargets[i] = attachedHands[i].targetRotation * Quaternion.Inverse(offsetRot);
                }

                //Average target transformation
                targetPosition = Vector3.Lerp(posTargets[0], posTargets[1], 0.5f);
                targetRotation = Quaternion.Lerp(rotTargets[0], rotTargets[1], 0.5f);

                //Apply Target Transformation to hands
                attachedHands[0].gripbedTrackDriver.UpdateTrackFixed(targetPosition, targetRotation);
                attachedHands[1].gripbedTrackDriver.UpdateTrackFixed(targetPosition, targetRotation);
            }
        }

        #endregion

        #region Events
        public virtual void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase)
        {
            ///Manage new hand first (so the last driver gets removed before a new one is added)
            ManageNewHand(hand, attachedHands, twoHandedMode);

            originalInterpolation = rb.interpolation;
            originalKinematicMode = rb.isKinematic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;

            ///Setup and Start Track Driver
            var m = overrideTrackingMode ? customTrackingMode : mode;

            hand.gripbedTrackDriver = Utils.DriverFromEnum(m);
            hand.gripbedTrackDriver.StartTrack(transform, trackingBase);

            ToggleHandCollisions(hand, false);

            OnGrab?.Invoke();
            hand.OnPinchStart.AddListener(delegate { OnPinchStart?.Invoke(); });
            hand.OnPinchEnd.AddListener(delegate { OnPinchEnd?.Invoke(); });

            ///This needs to be called at the end, if not the releasing Hand will set "isGrabbed" to false and it will stay that way
            isGrabbed = true;
        }

        public virtual void Release(FusionXRHand hand)
        {
            ToggleHandCollisions(hand, true);

            rb.interpolation = originalInterpolation;
            rb.isKinematic = originalKinematicMode;

            RemoveHand(hand);

            if(!isGrabbed)
                OnRelease?.Invoke();

            hand.OnPinchStart.RemoveAllListeners();

            //If the releasing hand was the last one gripbing the object, end the tracking/trackDriver
            hand.gripbedTrackDriver.EndTrack();
        }

        #endregion

        #region Functions

        void SetLayer(GameObject obj, string layer)
        {
            obj.layer = LayerMask.NameToLayer(layer);

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayer(obj.transform.GetChild(i).gameObject, layer);
            }
        }

        //For returning the transform and the GrabPoint
        public GrabPoint GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand)
        {
            GrabPoint gripPoint = Utils.ClosestGrabPoint(this, point, handTransform, desiredHand);

            if (gripPoint != null)
            {
                gripPoint = gripPoint.GetAligned(handTransform);
                gripPoint.BlockGrabPoint();
                return gripPoint;
            }
            else
            {
                return null;
            }
        }

        public static void ManageNewHand(FusionXRHand hand, List<FusionXRHand> currentHands, TwoHandedModes mode)
        {
            if (mode == TwoHandedModes.SwitchHand)   //Case: Switch Hands (Release the other hand)
            {
                //The order of these operations is critical, if the next hand is added before the last one released the "if" will fail
                if (currentHands.Count > 0)
                {
                    //This will also call the release function on this gripable, with this structure the hand can also be forced to release whatever it is holding
                    currentHands[0].Release();
                }

                currentHands.Add(hand);
            }
            else if (mode == TwoHandedModes.Average) //Case: Averaging Between Hands;
            {
                currentHands.Add(hand);
            }
            else if (mode == TwoHandedModes.JointRotation) //Case: Averaging Between Hands;
            {
                currentHands.Add(hand);
            }
        }

        void RemoveHand(FusionXRHand hand)
        {
            attachedHands.Remove(hand);

            if (attachedHands.Count == 0)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.None;
                isGrabbed = false;
            }
        }

        public static void ToggleHandCollisions(FusionXRHand hand, bool enable)
        {
            foreach (Collider coll in hand.GetComponentsInChildren<Collider>())
            {
                coll.enabled = enable;
            }
        }

        #endregion
    }
}
