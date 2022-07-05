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
        public InputActionReference grabReference;
        public InputActionReference pinchReference;

        [Header("Events")]
        public UnityEvent OnGrabStart;
        public UnityEvent OnGrabEnd;

        public UnityEvent OnPinchStart;
        public UnityEvent OnPinchEnd;

        #endregion

        #region Start and Update
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            followObject = trackedController;

            ///Set the tracking Mode accordingly
            var newTrackDriver = Utils.DriverFromEnum(trackingMode);
            trackDriver = ChangeTrackDriver(newTrackDriver);

            trackingBase.tracker = this.gameObject;
            trackingBase.palm = palm;
            trackDriver.StartTrack(transform, trackingBase);

            ///Subscribe to the actions
            grabReference.action.started += OnGrabbed;
            grabReference.action.canceled += OnLetGo;

            pinchReference.action.started += OnPinched;
            pinchReference.action.canceled += OnPinchedCancelled;

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
            OnGrabStart?.Invoke();
            GrabObject();
        }

        private void OnLetGo(InputAction.CallbackContext obj)
        {
            OnGrabEnd?.Invoke();
            Release();
        }

        private void OnPinched(InputAction.CallbackContext obj)
        {
            OnPinchStart?.Invoke();
        }

        private void OnPinchedCancelled(InputAction.CallbackContext obj)
        {
            OnPinchEnd?.Invoke();
        }

        #endregion

        #region DebugEvents
        public void DebugGrab()
        {
            OnGrabStart?.Invoke();
            GrabObject();
        }

        public void DebugLetGo()
        {
            OnGrabEnd?.Invoke();
            if (isGrabbing)
                Release();
        }

        public void DebugPinchStart()
        {
            OnPinchStart.Invoke();
        }

        public void DebugPinchEnd()
        {
            OnPinchEnd.Invoke();
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
            grabSpot.position = closestCollider.ClosestPoint(palm.position);

            //Raycasting to find GrabSpots Normal
            RaycastHit hit;
            var dir = palm.position - closestCollider.bounds.center;
            Ray ray = new Ray(palm.position - dir, dir);

            //TODO: Better ignore mask
            if (Physics.Raycast(ray, out hit, 1f, ~grabMask))
            {
                grabSpot.parent = grabbable.Transform;
                grabSpot.localPosition = grabbedGrabbable.Transform.InverseTransformPoint(hit.point);
                grabSpot.position = closestCollider.ClosestPoint(hit.point);

                var n = Vector3.Project(dir, hit.normal);
                grabSpot.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), n);
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
