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
        //Grab Event Called if Grab Action is performed
        void OnGrabbed(InputAction.CallbackContext obj)
        {
            if (isGrabbing)
            {
                return;
            }

            GameObject closestGrabable = ClosestGrabable(out Collider closestColl);

            if (closestGrabable == null)
                return;

            grabbedGrabable = closestGrabable.GetComponentInParent<Grabable>();

            if(grabbedGrabable.TryGetClosestGrapPoint(transform.position, hand, out Transform grabPoint))
            {
                StartCoroutine(GrabObject(closestColl, grabPoint));
            }
            else
            {
                StartCoroutine(GrabObject(closestColl, null));
            }
        }

        void OnLetGo(InputAction.CallbackContext obj)
        {
            Release();
        }

        void OnPinched(InputAction.CallbackContext obj)
        {

        }

        void OnPinchedCancelled(InputAction.CallbackContext obj)
        {

        }

        #endregion

        #region Functions
        public TrackDriver ChangeTrackDriver(TrackDriver newDriver)
        {
            if (trackDriver != null) ///End the current trackDriver if it exists
                trackDriver.EndTrack();
            return newDriver;
        }

        ///Always with a defined GrabPoint, if there is none, overload will generate one
        IEnumerator GrabObject(Collider closestColl, Transform givenGrabPoint)
        {
            yield return null;
        }

        ///A function so it can also be called from a grabbable that wants to switch hands
        public void Release()
        {
            isGrabbing = false;
            followObject = trackedController;

            if (grabbedGrabable != null)
            {
                grabbedGrabable.Release(this);
                grabbedGrabable.GetComponent<Rigidbody>().velocity = rb.velocity;
                grabbedGrabable = null;
            }
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
        #endregion
    }
}
