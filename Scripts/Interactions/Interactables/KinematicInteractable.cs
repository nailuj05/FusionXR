using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public abstract class KinematicInteractable : MonoBehaviour, IGrabable
    {
        #region IGrabable Implementation
        public Transform Transform { get { return transform; } }
        public GameObject GameObject { get { return gameObject; } }

        public TwoHandedMode twoHandedMode = TwoHandedMode.SwitchHand;

        public bool isGrabbed { get; protected set; }

        [SerializeField] private GrabPoint[] grabPoints;

        public List<FusionXRHand> attachedHands { get; private set; } = new List<FusionXRHand>();
        #endregion

        protected bool allowCollisionInteraction = true;

        [SerializeField] [Tooltip("Used to filter which objects can interact besides grabbing the interactable")]
        protected LayerMask interactionLayers = ~0;

        [SerializeField]
        protected bool canBeGrabbed = true;

        [SerializeField]
        protected Vector3 axis = Vector3.right;

        protected bool isInteracting = false;


        #region Grab & Release
        public void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase)
        {
            Grabable.ManageNewHand(hand, attachedHands, twoHandedMode);

            Grabable.EnableOrDisableCollisions(gameObject, hand, true);

            isGrabbed = true;

            InteractionStart();
        }

        public void Release(FusionXRHand hand)
        {
            Grabable.EnableOrDisableCollisions(gameObject, hand, false);

            isGrabbed = false;

            attachedHands.Remove(hand);

            InteractionEnd();
        }
        #endregion

        #region Interaction
        private void OnCollisionEnter(Collision collision)
        {
            if (!allowCollisionInteraction) return;

            if (Utilities.ObjectMatchesLayermask(collision.gameObject, interactionLayers) & !isInteracting)
            {
                InteractionStart();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!allowCollisionInteraction) return;

            if (Utilities.ObjectMatchesLayermask(collision.gameObject, interactionLayers) & isInteracting)
            {
                InteractionEnd();
            }
        }

        public void Update()
        {
            if (isInteracting)
            {
                InteractionUpdate();
            }
        }

        protected abstract void InteractionUpdate();

        protected abstract void InteractionStart();

        protected abstract void InteractionEnd(); 
        #endregion

        //For returning the transform and the GrabPoint
        public Transform GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand, out GrabPoint grabPoint)
        {
            grabPoint = Utilities.ClosestGrabPoint(grabPoints, point, handTransform, desiredHand);

            if (grabPoint != null)
            {
                grabPoint.BlockGrabPoint();
                return grabPoint.transform;
            }
            else
            {
                return null;
            }
        }
    }
}
