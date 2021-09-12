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

        private GrabMode grabMode;
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

        public virtual void FixedUpdate()
        {
            if (!isGrabbed)
                return;
        }

        #endregion

        #region Events
        public void Grab(FusionXRHand hand, GrabMode mode) 
        {
            grabMode = mode;

            if (twoHandedMode == TwoHandedMode.SwitchHand)   //Case: Switch Hands (Release the other hand)
            {
                //The order of these operations is critical, if the next hand is added before the last one released the "if" will fail
                if (attachedHands.Count > 0)
                    attachedHands[0].Release();

                attachedHands.Add(hand);
            }
            else if(twoHandedMode == TwoHandedMode.Average) //Case: Averaging Between Hands;
            {
                attachedHands.Add(hand);
            }

            foreach (Collider coll in GetComponents<Collider>())
            {
                Physics.IgnoreCollision(hand.GetComponent<Collider>(), coll, true);
            }

            isGrabbed = true; //This needs to be called at the end, if not the releasing Hand will set "isGrabbed" to false and it will stay that way
        }

        public void Release(FusionXRHand hand)
        {
            foreach (Collider coll in GetComponents<Collider>())
            {
                Physics.IgnoreCollision(hand.GetComponent<Collider>(), coll, false);
            }

            attachedHands.Remove(hand);

            if(attachedHands.Count == 0)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.None;
                isGrabbed = false;
            }
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

        #endregion
    }
}
