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

        [SerializeField]
        TwoHandedModes TwoHandedMode = TwoHandedModes.SwitchHand;
        public TwoHandedModes twoHandedMode { get { return TwoHandedMode; } set { TwoHandedMode = value; } }

        public bool isGrabbed { get; protected set; }

        [SerializeField]
        public GrabPoint[] GrabPoints;
        public GrabPoint[] gripPoints { get { return GrabPoints; } set { GrabPoints = value; } }

        public List<FusionXRHand> attachedHands { get; private set; } = new List<FusionXRHand>();
        #endregion

        [SerializeField]
        protected bool allowCollisionInteraction = false;

        [SerializeField] [Tooltip("Used to filter which objects can interact besides gripbing the interactable")]
        protected LayerMask interactionLayers = ~0;

        [SerializeField]
        protected bool canBeGrabbed = true;

        [SerializeField]
        public Vector3 axis = Vector3.right;

        //TODO: Is this still needed (obsolete by isGrabbed?)
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

            Grabbable.ToggleHandCollisions(hand, false);

            isGrabbed = true;
            isInteracting = true;

            InteractionStart();
        }

        public void Release(FusionXRHand hand)
        {
            Grabbable.ToggleHandCollisions(hand, true);

            attachedHands.Remove(hand);

            isGrabbed = attachedHands.Count > 0;
            isInteracting = attachedHands.Count > 0;

            InteractionEnd();
        }

        protected Vector3 GetMeanPosition()
        {
            Vector3 meanPos = attachedHands[0].targetPosition;

            if(attachedHands.Count > 1)
            {
                meanPos += attachedHands[1].targetPosition;
                meanPos *= 0.5f;
            }

            return meanPos;
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
        public GrabPoint GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand)
        {
            GrabPoint gripPoint = Utils.ClosestGrabPoint(this, point, handTransform, desiredHand);

            if (gripPoint != null)
            {
                gripPoint.BlockGrabPoint();
                return gripPoint;
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

            Handles.color = new Color(0, 0.5f, 0, 0.25f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.TransformDirection(t.axis), 0.1f);

            Handles.DrawLine(t.transform.position, t.transform.TransformPoint(t.axis), 3);
        }
    }
#endif
}
