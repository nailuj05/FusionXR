using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    public class FusionXRHand : MonoBehaviour
    {
        #region Variables

        [Header("Tracking")]
        public Hand hand;
        public Transform trackedController;
        private Transform followObject;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        //Tracking for Hands (and trackingBase for Grabbables)
        public TrackingMode trackingMode;

        public TrackingBase trackingBase;
        private TrackDriver trackDriver;

        ///The Transformation the hand WANTS to reach, public get for access from Grabbable
        public Vector3 targetPosition { get; private set; }
        public Quaternion targetRotation { get; private set; }

        [HideInInspector]
        public Rigidbody rb;

        [Header("Grabbing")]
        public TrackingMode grabbedTrackingMode;
        public Transform palm;
        [SerializeField] private float reachDist = 0.1f;

        public LayerMask grabMask = 768;

        private bool isGrabbing;
        private bool generatedGrabPoint;
        private IGrabbable grabbedGrabbable;

        public bool useHandPoser;
        private HandPoser handPoser;

        [HideInInspector]
        public TrackDriver grabbedTrackDriver;

        /// <summary>
        /// This stores the Transform of the grabPoint, doesn't matter wether it is generated or not
        /// </summary>
        public Transform grabPosition { get; private set; }

        /// <summary>
        /// This stores the actual grabPoint Component
        /// </summary>
        private GrabPoint grabPoint;

        [Header("Inputs")]
        public InputAction grip;
        public InputAction trigger;

        [Header("Events")]
        public UnityEvent OnGripStart;
        public UnityEvent OnGripEnd;

        public UnityEvent OnTriggerStart;
        public UnityEvent OnTriggerEnd;

        #endregion

        #region Start and Update
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            followObject = trackedController;

            ///Set the tracking Mode accordingly
            var newTrackDriver = Utils.DriverFromEnum(trackingMode);
            trackDriver = ChangeTrackDriver(newTrackDriver);

            trackingBase.tracker = this.transform;
            trackingBase.rotationOffset = rotationOffset;
            trackingBase.palm = palm;
            trackDriver.StartTrack(transform, trackingBase);

            ///Subscribe to the actions
            grip.Enable();
            trigger.Enable();

            grip.started += OnGrabbed;
            grip.canceled += OnLetGo;

            trigger.started += OnPinched;
            trigger.canceled += OnPinchedCancelled;

            trackingBase.startRot = transform.rotation;
            trackingBase.startRotLocal = transform.localRotation;

            if (useHandPoser)
            {
                handPoser = GetComponent<HandPoser>();
            }
        }

        private void LateUpdate()
        {
            targetPosition = followObject.TransformPoint(positionOffset);
            targetRotation = followObject.rotation * Quaternion.Euler(rotationOffset);

            trackDriver.UpdateTrack(targetPosition, targetRotation);
        }

        private void FixedUpdate()
        {
            trackDriver.UpdateTrackFixed(targetPosition, targetRotation);
        }

        #endregion

        #region Events
        private void OnGrabbed(InputAction.CallbackContext obj)
        {
            OnGripStart?.Invoke();
            GrabObject();
        }

        private void OnLetGo(InputAction.CallbackContext obj)
        {
            OnGripEnd?.Invoke();
            Release();
        }

        private void OnPinched(InputAction.CallbackContext obj)
        {
            OnTriggerStart?.Invoke();
        }

        private void OnPinchedCancelled(InputAction.CallbackContext obj)
        {
            OnTriggerEnd?.Invoke();
        }

        #endregion

        #region DebugEvents
        public void DebugGrab()
        {
            OnGripStart?.Invoke();
            GrabObject();
        }

        public void DebugLetGo()
        {
            OnGripEnd?.Invoke();
            if (isGrabbing)
                Release();
        }

        public void DebugPinchStart()
        {
            OnTriggerStart.Invoke();
        }

        public void DebugPinchEnd()
        {
            OnTriggerEnd.Invoke();
        }
        #endregion

        #region Functions
        ///Always with a defined GrabPoint, if there is none, overload will generate one
        void GrabObject()
        {
            ///Return if already grabbing
            if (isGrabbing)
                return;

            grabPoint = null;
            generatedGrabPoint = false;

            ///Check for grabbable in Range, if none return
            GameObject closestGrabbable = ClosestGrabbable(out Collider closestColl);

            if (closestGrabbable == null)
                return;

            isGrabbing = true;

            ///Get grabbable component and possible grab points
            grabbedGrabbable = closestGrabbable.GetComponentInParent<IGrabbable>();

            grabPoint = grabbedGrabbable.GetClosestGrabPoint(transform.position, transform, hand);

            grabPosition = grabPoint?.AlignedTransform;

            ///Generate a GrabPoint if there is no given one
            //TODO: What if no grab Point should be generated and we just can't grab it?
            if (grabPosition == null)
            {
                grabPosition = GenerateGrabPoint(closestColl, grabbedGrabbable);
                generatedGrabPoint = true;
            }

            transform.position = grabPosition.position;
            transform.rotation = grabPosition.rotation;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            grabbedGrabbable.Grab(this, grabbedTrackingMode, trackingBase);

            if (!useHandPoser)
                return;

            if (!generatedGrabPoint && grabPoint.hasCustomPose)
            {
                handPoser.AttachHand(grabPosition, grabPoint.pose, true);
            }
            else
            {
                handPoser.AttachHand(grabPosition);
            }
        }

        ///A function so it can also be called from a grabbable that wants to switch hands
        public void Release()
        {
            if (!isGrabbing) return;
            isGrabbing = false;

            //Destroy the grabPoint, unlock if needed
            if (generatedGrabPoint)
            {
                if(grabPosition != null)
                    Destroy(grabPosition.gameObject);
            }
            else if(grabPoint != null)
            {
                //Release the GrabPoint to unlock it
                grabPoint.ReleaseGrabPoint();
            }

            //Release the Grabbable and reset the hand
            if (grabbedGrabbable != null)
            {
                grabbedGrabbable.Release(this);
                grabbedGrabbable.GameObject.GetComponent<Rigidbody>().velocity = rb.velocity;   //NOTE: Apply Better velocity for throwing here
                grabbedGrabbable = null;
            }

            if (useHandPoser)
            {
                handPoser?.ReleaseHand();
            }
        }

        public Transform GenerateGrabPoint(Collider closestCollider, IGrabbable grabbable)
        {
            Transform grabSpot = new GameObject().transform;

            Vector3 dir = closestCollider.ClosestPoint(palm.position) - palm.position;

            if(Physics.Raycast(palm.position, dir, out RaycastHit hit, 0.1f, grabMask))
            {
                grabSpot.parent = grabbable.Transform;
                grabSpot.position = hit.point;

                grabSpot.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            }
            else
            {
                grabSpot.localRotation = transform.rotation;
                grabSpot.parent = grabbable.Transform;
            }

            return grabSpot;
        }

        //TODO: remove redunant find closest gameobject
        GameObject ClosestGrabbable(out Collider closestColl)
        {
            Collider[] nearObjects = Physics.OverlapSphere(palm.position, reachDist);

            GameObject ClosestGameObj = null;
            closestColl = null;
            float Distance = float.MaxValue;

            //Check for the closest Grabbable Object
            if (nearObjects != null)
            {
                foreach (Collider coll in nearObjects)
                {
                    if (!Utils.ObjectMatchesLayermask(coll.gameObject, grabMask))
                        continue;

                    if ((coll.transform.position - transform.position).sqrMagnitude < Distance)
                    {
                        closestColl = coll;
                        ClosestGameObj = coll.gameObject;
                        Distance = (coll.transform.position - transform.position).sqrMagnitude;
                    }
                }
            }
            return ClosestGameObj;
        }

        public TrackDriver ChangeTrackDriver(TrackDriver newDriver)
        {
            if (trackDriver != null) ///End the current trackDriver if it exists
                trackDriver.EndTrack();
            return newDriver;
        }
        #endregion
    }
}
