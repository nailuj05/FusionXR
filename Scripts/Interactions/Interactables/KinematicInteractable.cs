using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public abstract class KinematicInteractable : MonoBehaviour, IGrabbable
    {
        #region IGrabbable Implementation
        public Transform Transform { get { return transform; } }
        public GameObject GameObject { get { return gameObject; } }

        public TwoHandedMode twoHandedMode = TwoHandedMode.SwitchHand;

        public bool isGrabbed { get; protected set; }

        [SerializeField] private GrabPoint[] grabPoints;

        public List<FusionXRHand> attachedHands { get; private set; } = new List<FusionXRHand>();
        #endregion

        [SerializeField]
        protected bool allowCollisionInteraction = false;

        [SerializeField] [Tooltip("Used to filter which objects can interact besides grabbing the interactable")]
        protected LayerMask interactionLayers = ~0;

        [SerializeField]
        protected bool canBeGrabbed = true;

        [SerializeField]
        public Vector3 axis = Vector3.right;

        protected bool isInteracting = false;

        private void Start()
        {
            try
            {
                gameObject.layer = LayerMask.NameToLayer("Interactables");
            }
            catch
            {
                Debug.LogError("Layers need to be setup correctly!");
            }
        }

        #region Grab & Release
        public void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase)
        {
            Grabbable.ManageNewHand(hand, attachedHands, twoHandedMode);

            Grabbable.EnableOrDisableCollisions(gameObject, hand, true);

            isGrabbed = true;

            InteractionStart();
        }

        public void Release(FusionXRHand hand)
        {
            Grabbable.EnableOrDisableCollisions(gameObject, hand, false);

            isGrabbed = false;

            attachedHands.Remove(hand);

            InteractionEnd();
        }
        #endregion

        #region Interaction
        private void OnCollisionEnter(Collision collision)
        {
            if (!allowCollisionInteraction) return;

            if (Utils.ObjectMatchesLayermask(collision.gameObject, interactionLayers) & !isInteracting)
            {
                InteractionStart();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!allowCollisionInteraction) return;

            if (Utils.ObjectMatchesLayermask(collision.gameObject, interactionLayers) & isInteracting)
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
            grabPoint = Utils.ClosestGrabPoint(grabPoints, point, handTransform, desiredHand);

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

#if UNITY_EDITOR
    [CustomEditor(typeof(KinematicInteractable), true)]
    public class KinematicInteractableEditor : Editor
    {
        private void OnSceneGUI()
        {
            var t = (KinematicInteractable)target;

            Handles.color = new Color(0, 0.5f, 0, 0.1f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.TransformDirection(t.axis), 0.1f);
        }
    }
#endif
}
