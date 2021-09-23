using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    public class FusionXRHand : MonoBehaviour
    {
        #region Variables

        //Tracking
        public Hand hand;
        public Transform trackedController;
        private Transform followObject;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        //Tracking for Hands (and trackingBase for Grabables)
        public TrackingMode trackingMode;

        public TrackingBase trackingBase;
        private TrackDriver trackDriver;

        ///The Transformation the hand WANTS to reach, public get for access from Grabable
        public Vector3 targetPosition { get; private set; }
        public Quaternion targetRotation { get; private set; }

        [HideInInspector]
        public Rigidbody rb;

        //Inputs
        public InputActionReference grabReference;
        public InputActionReference pinchReference;

        //Grabbing
        public float grabRange = 0.1f;
        public TrackingMode grabbedTrackingMode;
        public Transform palm;
        [SerializeField] private float reachDist = 0.1f; //, joinDist = 0.05f;

        private bool isGrabbing;
        private Grabable grabbedGrabable;
        public Transform grabPoint { get; private set; }

        #endregion

        #region Start and Update
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            followObject = trackedController;

            ///Set the tracking Mode accordingly
            var newTrackDriver = Utilities.DriverFromEnum(trackingMode);
            trackDriver = ChangeTrackDriver(newTrackDriver);

            trackingBase.tracker = this.gameObject;
            trackDriver.StartTrack(transform, trackingBase);

            ///Subscribe to the actions
            grabReference.action.started += OnGrabbed;
            grabReference.action.canceled += OnLetGo;

            pinchReference.action.started += OnPinched;
            pinchReference.action.canceled += OnPinchedCancelled;
        }

        private void Update()
        {
            targetPosition = followObject.TransformPoint(positionOffset);
            targetRotation = followObject.rotation * Quaternion.Euler(rotationOffset);

            trackDriver.UpdateTrack(targetPosition, targetRotation);
        }

        #endregion

        #region Events
        private void OnGrabbed(InputAction.CallbackContext obj)
        {
            GrabObject();
        }

        private void OnLetGo(InputAction.CallbackContext obj)
        {
            Release();
        }

        private void OnPinched(InputAction.CallbackContext obj)
        {

        }

        private void OnPinchedCancelled(InputAction.CallbackContext obj)
        {

        }

        #endregion

        #region DebugEvents
        public void DebugGrab()
        {
            GrabObject();
        }

        public void DebugLetGo()
        {
            if (isGrabbing)
                Release();
        }
        #endregion

        #region Functions
        ///Always with a defined GrabPoint, if there is none, overload will generate one
        void GrabObject()
        {
            ///Return if already grabbing
            if (isGrabbing)
                return;

            ///Check for grabable in Range, if none return
            GameObject closestGrabable = ClosestGrabable(out Collider closestColl);

            if (closestGrabable == null)
                return;

            ///Get grabable component and possible grab points
            grabbedGrabable = closestGrabable.GetComponentInParent<Grabable>();

            grabPoint = grabbedGrabable.GetClosestGrabPoint(transform.position, hand);

            ///Generate a GrabPoint if there is no given one
            if (grabPoint == null)
            {
                grabPoint = GenerateGrabPoint(closestColl, grabbedGrabable);
            }

            isGrabbing = true;

            grabbedGrabable.Grab(this, grabbedTrackingMode, trackingBase);

            Debug.Log($"Grab {grabbedGrabable.gameObject.name}");
        }

        ///A function so it can also be called from a grabbable that wants to switch hands
        public void Release()
        {
            isGrabbing = false;
            
            //Destory the grabPoint
            if (grabPoint != null)
            {
                Destroy(grabPoint.gameObject);
            }

            //Release the Grabable and reset the hand
            if (grabbedGrabable != null)
            {
                grabbedGrabable.Release(this);
                grabbedGrabable.GetComponent<Rigidbody>().velocity = rb.velocity;   //NOTE: Apply Better velocity for throwing here
                grabbedGrabable = null;
            }
        }

        Transform GenerateGrabPoint(Collider closestCollider, Grabable grabable)
        {
            Transform grabSpot = new GameObject().transform;
            grabSpot.position = closestCollider.ClosestPoint(palm.position);
            grabSpot.localRotation = transform.rotation;
            grabSpot.parent = grabable.transform;

            return grabSpot;
        }

        GameObject ClosestGrabable(out Collider closestColl)
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
                    if(coll.gameObject.tag == "Grabable")
                    {
                        if ((coll.transform.position - transform.position).sqrMagnitude < Distance)
                        {
                            closestColl = coll;
                            ClosestGameObj = coll.gameObject;
                            Distance = (coll.transform.position - transform.position).sqrMagnitude;
                        }
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
