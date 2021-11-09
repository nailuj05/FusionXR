using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public abstract class KinematicInteractable : Grabable
    {
        [SerializeField] [Tooltip("Used to filter which objects can interact besides grabbing the interactable")]
        protected LayerMask interactionLayers = ~0;

        [SerializeField]
        protected bool canBeGrabbed = true;

        [SerializeField]
        protected Vector3 axis = Vector3.right;

        protected bool isInteracting = false;

        public override void Update()
        {
            if (isInteracting)
            {
                InteractionUpdate();
            }
        }

        public override void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase)
        {
            ManageNewHand(hand);

            InteractionStart();
        }

        public override void Release(FusionXRHand hand)
        {
            InteractionEnd();
        }

        protected abstract void InteractionUpdate();

        protected abstract void InteractionStart();

        protected abstract void InteractionEnd();
    }
}
