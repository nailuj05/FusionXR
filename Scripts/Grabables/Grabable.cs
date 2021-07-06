using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public enum TwoHandedMode
    {
        SwitchHand = 0,
        Average = 1,
        //AttachHand = 2
    }

    public class Grabable : MonoBehaviour
    {
        public TwoHandedMode twoHandedMode = TwoHandedMode.SwitchHand;
        [HideInInspector] public bool isGrabbed;
        public float releaseThreshold = 0.4f;
        [SerializeField] private GrabPoint[] grabPoints;

        [HideInInspector] public List<FusionXRHand> attachedHands = new List<FusionXRHand>();

        private Rigidbody rb;

        //If 2 Handed:
        private Vector3 posOffset;
        private Vector3 rotOffset;

        private Vector3 refVel;

        #region UnityFunctions
        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            gameObject.tag = "Grabable";
        }

        public virtual void FixedUpdate()
        {
            if (!isGrabbed)
                return;

            Vector3 avgPos = Vector3.zero;
            Quaternion avgRot = Quaternion.identity;
            Vector3 offsetPos = Vector3.zero;

            if (attachedHands.Count > 1)
            {
                Vector3[] handsPosOffset = new Vector3[2];
                Quaternion[] handsRotOffset = new Quaternion[2];

                handsPosOffset[0] = attachedHands[0].grabSpot.localPosition;
                handsPosOffset[1] = attachedHands[1].grabSpot.localPosition;

                handsRotOffset[0] = attachedHands[0].rotWithOffset * Quaternion.Inverse(attachedHands[0].grabSpot.localRotation);
                handsRotOffset[1] = attachedHands[1].rotWithOffset * Quaternion.Inverse(attachedHands[1].grabSpot.localRotation);

                avgRot = Quaternion.Lerp(handsRotOffset[0], handsRotOffset[1], 0.5f);
                avgPos = Vector3.Lerp(attachedHands[0].posWithOffset, attachedHands[1].posWithOffset, 0.5f);

                offsetPos = Vector3.Lerp(handsPosOffset[0], handsPosOffset[1], .5f);
            }
            else
            {
                avgRot = attachedHands[0].rotWithOffset * Quaternion.Inverse(attachedHands[0].grabSpot.localRotation); //Quaternion.Euler(attachedHands[0].RotOffset);
                avgPos = attachedHands[0].posWithOffset;
                offsetPos = attachedHands[0].grabSpot.localPosition;
            }

            TrackRotationVelocity(avgRot);
            TrackPositionVelocity(avgPos, offsetPos);

            return;
        }

        #endregion

        #region Events
        public void Grab(FusionXRHand hand) 
        {
            foreach(Collider coll in GetComponents<Collider>())
            {
                Physics.IgnoreCollision(hand.GetComponent<Collider>(), coll, true);
            }

            //Case: Switch Hands
            if(twoHandedMode == TwoHandedMode.SwitchHand)
            {
                if(attachedHands.Count > 0)
                    attachedHands[0].Release();

                attachedHands.Add(hand);
            }
            //Case: Averaging Between Hands;
            else if(twoHandedMode == TwoHandedMode.Average)
            {
                attachedHands.Add(hand);
            }

            isGrabbed = true;
            //UpdatePivotPoint();
        }

        public void Release(FusionXRHand hand)
        {
            foreach (Collider coll in GetComponents<Collider>())
            {
                Physics.IgnoreCollision(hand.GetComponent<Collider>(), coll, false);
            }

            attachedHands.Remove(hand.Instance);

            if(attachedHands.Count == 0)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.None;
                isGrabbed = false;
            }

            //UpdatePivotPoint();
        }

        #endregion

        #region Functions

        public bool TryGetClosestGrapPoint(Vector3 point, Hand desiredHand, out Transform GrapPoint)
        {
            GrapPoint = ClosestGrabPoint(grabPoints, point, desiredHand);

            return GrapPoint != null;
        }

        Transform ClosestGrabPoint(GrabPoint[] grabPoints, Vector3 point, Hand desiredHand)
        {
            Transform closestGrabPoint = null;
            float distance = float.MaxValue;

            if (grabPoints != null)
            {
                foreach (GrabPoint currentGrabPoint in grabPoints)
                {
                    if (currentGrabPoint.CorrectHand(desiredHand) && currentGrabPoint.isActive) //Check if the GrapPoint is for the correct Hand and if it isActive
                    {
                        if ((currentGrabPoint.transform.position - point).sqrMagnitude < distance) //Check if next Point is closer than last Point
                        {
                            closestGrabPoint = currentGrabPoint.transform;
                            distance = (currentGrabPoint.transform.position - point).sqrMagnitude; //New (smaller) distance
                        }
                    }
                }
            }
            return closestGrabPoint;
        }

        //Tracking Functions
        void TrackPositionVelocity(Vector3 targetPos, Vector3 offset)
        {
            var positionDelta = targetPos - transform.TransformPoint(offset);

            Vector3 velocityTarget = (positionDelta * 60f);

            if (float.IsNaN(velocityTarget.x) == false)
            {
                rb.velocity = Vector3.MoveTowards(rb.velocity, velocityTarget, 20f);
            }
        }

        void TrackRotationVelocity(Quaternion targetRot)
        {
            Quaternion deltaRotation = targetRot * Quaternion.Inverse(transform.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                Vector3 angularTarget = axis * (Mathf.Deg2Rad * angle * 10);

                rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, angularTarget, 30f);
            }
        }
        #endregion

        #region Deprecated Functions
        public Vector3 CalculateOffset(FusionXRHand hand)
        {
            Vector3 offset = Vector3.zero;

            if (grabPoints.Length > 0)
            {
                Transform grabPoint = ClosestGrabPoint(grabPoints, hand.transform.position, hand.hand);

                offset = grabPoint.position - transform.position;
            }
            else
            {
                offset = transform.position - hand.transform.position;
            }

            return offset;
        }

        public Quaternion CalculateRotationOffset(FusionXRHand hand)
        {
            Quaternion rotationOffset;

            if (grabPoints.Length > 0)
            {
                Transform grabPoint = ClosestGrabPoint(grabPoints, hand.transform.position, hand.hand);

                rotationOffset = grabPoint.rotation;
            }
            else
            {
                rotationOffset = hand.transform.rotation * Quaternion.Inverse(transform.rotation);
            }

            return rotationOffset;
        }


        #endregion
    }
}
