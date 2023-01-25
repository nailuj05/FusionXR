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
        public TrackingMode gripbedTrackingMode;
        public Transform palm;
        [SerializeField] private float reachDist = 0.1f;

        public LayerMask gripMask = 768;

        private bool isGrabbing;
        private bool generatedGrabPoint;
        private IGrabbable gripbedGrabbable;

        public bool useHandPoser;
        private HandPoser handPoser;

        [HideInInspector]
        public TrackDriver gripbedTrackDriver;

        /// <summary>
        /// This stores the Transform of the gripPoint, doesn't matter wether it is generated or not
        /// </summary>
        public Transform gripPosition { get; private set; }

        /// <summary>
        /// This stores the actual gripPoint Component
        /// </summary>
        private GrabPoint gripPoint;

        [Header("Inputs")]
        public InputAction grip;
        public InputAction trigger;

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
            ///Return if already gripbing
            if (isGrabbing)
                return;

            gripPoint = null;
            generatedGrabPoint = false;

            ///Check for gripbable in Range, if none return
            GameObject closestGrabbable = ClosestGrabbable(out Collider closestColl);

            if (closestGrabbable == null)
                return;

            isGrabbing = true;

            ///Get gripbable component and possible grip points
            gripbedGrabbable = closestGrabbable.GetComponentInParent<IGrabbable>();

            gripPoint = gripbedGrabbable.GetClosestGrabPoint(transform.position, transform, hand);

            gripPosition = gripPoint?.AlignedTransform;

            ///Generate a GrabPoint if there is no given one
            //TODO: What if no grip Point should be generated and we just can't grip it?
            if (gripPosition == null)
            {
                gripPosition = GenerateGrabPoint(closestColl, gripbedGrabbable);
                generatedGrabPoint = true;
            }

            transform.position = gripPosition.position;
            transform.rotation = gripPosition.rotation;

            gripbedGrabbable.Grab(this, gripbedTrackingMode, trackingBase);

            if (!useHandPoser)
                return;

            if (!generatedGrabPoint && gripPoint.hasCustomPose)
            {
                handPoser.AttachHand(gripPosition, gripPoint.pose, true);
            }
            else
            {
                handPoser.AttachHand(gripPosition);
            }
        }

        ///A function so it can also be called from a gripbable that wants to switch hands
        public void Release()
        {
            if (!isGrabbing) return;
            isGrabbing = false;

            //Destroy the gripPoint, unlock if needed
            if (generatedGrabPoint)
            {
                if(gripPosition != null)
                    Destroy(gripPosition.gameObject);
            }
            else if(gripPoint != null)
            {
                //Release the GrabPoint to unlock it
                gripPoint.ReleaseGrabPoint();
            }

            //Release the Grabbable and reset the hand
            if (gripbedGrabbable != null)
            {
                gripbedGrabbable.Release(this);
                gripbedGrabbable.GameObject.GetComponent<Rigidbody>().velocity = rb.velocity;   //NOTE: Apply Better velocity for throwing here
                gripbedGrabbable = null;
            }

            if (useHandPoser)
            {
                handPoser?.ReleaseHand();
            }
        }

        public Transform GenerateGrabPoint(Collider closestCollider, IGrabbable gripbable)
        {
            Transform gripSpot = new GameObject().transform;
            gripSpot.position = closestCollider.ClosestPoint(palm.position);

            //Raycasting to find GrabSpots Normal
            RaycastHit hit;
            var dir = palm.position - closestCollider.bounds.center;
            Ray ray = new Ray(palm.position - dir, dir);

            //TODO: Better ignore mask
            if (Physics.Raycast(ray, out hit, 1f, ~gripMask))
            {
                gripSpot.parent = gripbable.Transform;
                gripSpot.localPosition = gripbedGrabbable.Transform.InverseTransformPoint(hit.point);
                gripSpot.position = closestCollider.ClosestPoint(hit.point);

                var n = Vector3.Project(dir, hit.normal);
                gripSpot.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), n);
            }
            else
            {
                gripSpot.localRotation = transform.rotation;
                gripSpot.parent = gripbable.Transform;
            }

            return gripSpot;
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
                    if (!Utils.ObjectMatchesLayermask(coll.gameObject, gripMask))
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
