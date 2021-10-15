using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class Grabable : MonoBehaviour
    {
        public TwoHandedMode twoHandedMode = TwoHandedMode.SwitchHand;
        public float releaseThreshold = 0.4f;

        [HideInInspector] public bool isGrabbed;
        [SerializeField] private GrabPoint[] grabPoints;

        [HideInInspector] public List<FusionXRHand> attachedHands = new List<FusionXRHand>();

        //private TrackDriver trackDriver;
        private Rigidbody rb;

        //If 2 Handed:
        private Vector3 posOffset;
        private Vector3 rotOffset;

        private Vector3 refVel;

        #region UnityFunctions

        public virtual void Start()
        {
            try
            {
                gameObject.layer = LayerMask.NameToLayer("Interactables");
            }
            catch
            {
                Debug.LogError("Layers need to be setup correctly!");
            }

            rb = GetComponent<Rigidbody>();
            gameObject.tag = "Grabable";
        }

        public virtual void Update()
        {
            if (!isGrabbed)
                return;

            //Reset Target Position and Rotation
            Quaternion targetRotation = Quaternion.identity;
            Vector3 targetPosition = Vector3.zero;

            int handsCount = attachedHands.Count;

            if (handsCount == 1) //If there is one hand grabbing
            {
                //Get GrabPoint Offsets
                Vector3 offsetPos = attachedHands[0].grabPosition.localPosition;
                Quaternion offsetRot = attachedHands[0].grabPosition.localRotation;

                //Delta Vector/Quaternion from Grabable (+ offset) to hand
                targetPosition = attachedHands[0].targetPosition - transform.TransformVector(offsetPos);
                targetRotation = attachedHands[0].targetRotation * Quaternion.Inverse(offsetRot);

                //Apply Target Transformation to hand
                attachedHands[0].grabbedTrackDriver.UpdateTrack(targetPosition, targetRotation);
            }
            else //If there is two hands grabbing 
            {
                Vector3[] posTargets = new Vector3[handsCount];
                Quaternion[] rotTargets = new Quaternion[handsCount];

                for (int i = 0; i < handsCount; i++)
                {
                    //Get GrabPoint Offsets
                    Vector3 offsetPos = attachedHands[i].grabPosition.localPosition;
                    Quaternion offsetRot = attachedHands[i].grabPosition.localRotation;

                    //Delta Vector/Quaternion from Grabable (+ offset) to hand
                    posTargets[i] = attachedHands[i].targetPosition - transform.TransformVector(offsetPos);
                    rotTargets[i] = attachedHands[i].targetRotation * Quaternion.Inverse(offsetRot);
                }

                //Average target transformation
                targetPosition = Vector3.Lerp(posTargets[0], posTargets[1], 0.5f);
                targetRotation = Quaternion.Lerp(rotTargets[0], rotTargets[1], 0.5f);

                //Apply Target Transformation to hands
                attachedHands[0].grabbedTrackDriver.UpdateTrack(targetPosition, targetRotation);
                attachedHands[1].grabbedTrackDriver.UpdateTrack(targetPosition, targetRotation);
            }
        }

        #endregion

        #region Events
        public void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase) 
        {
            ///Manage new hand first (so the last driver gets removed before a new one is added)
            ManageNewHand(hand);

            ///Setup and Start Track Driver
            hand.grabbedTrackDriver = Utilities.DriverFromEnum(mode);
            hand.grabbedTrackDriver.StartTrack(transform, trackingBase);

            EnableOrDisableCollisions(hand, true);

            ///This needs to be called at the end, if not the releasing Hand will set "isGrabbed" to false and it will stay that way
            isGrabbed = true; 
        }

        public void Release(FusionXRHand hand)
        {
            EnableOrDisableCollisions(hand, false);

            RemoveHand(hand);

            //If the releasing hand was the last one grabbing the object, end the tracking/trackDriver
            hand.grabbedTrackDriver.EndTrack();
        }

        #endregion

        #region Functions

        public Transform GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand)
        {
            GrabPoint grabPoint = ClosestGrabPoint(grabPoints, point, handTransform, desiredHand);

            return grabPoint.transform;
        }

        //For returning the transform and the GrabPoint
        public Transform GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand, out GrabPoint grabPoint)
        {
            grabPoint = ClosestGrabPoint(grabPoints, point, handTransform, desiredHand);

            if (grabPoint != null)
            {
                return grabPoint.transform;
            }
            else
            {
                return null;
            }
        }

        GrabPoint ClosestGrabPoint(GrabPoint[] grabPoints, Vector3 point, Transform handTransform, Hand desiredHand)
        {
            GrabPoint closestGrabPoint = null;
            float distance = float.MaxValue;

            if (grabPoints != null)
            {
                foreach (GrabPoint currentGrabPoint in grabPoints)
                {
                    if (currentGrabPoint.IsGrabPossible(handTransform, desiredHand) && currentGrabPoint.isActive) //Check if the GrabPoint is for the correct Hand and if it isActive
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

        void ManageNewHand(FusionXRHand hand)
        {
            if (twoHandedMode == TwoHandedMode.SwitchHand)   //Case: Switch Hands (Release the other hand)
            {
                //The order of these operations is critical, if the next hand is added before the last one released the "if" will fail
                if (attachedHands.Count > 0)
                {
                    //This will also call the release function on this grabable, with this structure the hand can also be forced to release whatever it is holding
                    attachedHands[0].Release();
                }

                attachedHands.Add(hand);
            }
            else if (twoHandedMode == TwoHandedMode.Average) //Case: Averaging Between Hands;
            {
                attachedHands.Add(hand);
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

        void EnableOrDisableCollisions(FusionXRHand hand, bool disable)
        {
            foreach (Collider coll in GetComponents<Collider>())
            {
                Physics.IgnoreCollision(hand.GetComponent<Collider>(), coll, disable);
            }
        }

        #endregion
    }
}
