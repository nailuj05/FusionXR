using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    #region Enums
    public enum Hand
    {
        Left = 0,
        Right = 1
    }

    public enum TrackingMode
    {
        Kinematic = 0,
        Velocity = 1,
        Joint = 2
    }

    public enum GrabMode
    {
        Kinematic = 0,
        Velocity = 1,
    }
    #endregion

    public class FusionXRHand : MonoBehaviour
    {

        #region Variables

        //Tracking
        public Hand hand;
        public Transform trackedController;
        private Transform followObject;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public TrackingMode trackingMode;

        [HideInInspector] public Vector3 posWithOffset;
        [HideInInspector] public Quaternion rotWithOffset;

        public Rigidbody rb;

        //Velocity Tracking
        public float positionStrength = 15;
        public float rotationStrength = 30;

        [Tooltip("How many Frames (Fixed Frames) of Velcity should be stored?")]
        [SerializeField] private int storedVelocityHistory;
        private List<Vector3> lastVelocities;

        //Joint Tracking
        private ConfigurableJoint armJoint;
        private Rigidbody playerRB;

        private Vector3 lastControllerPos;

        public float positionSpring = 5000;
        public float positionDamper = 1000;
        public float maxForce = 1500;

        public float slerpSpring = 3000;
        public float slerpDamper = 200;
        public float slerpMaxForce = 1500;

        //Inputs
        public InputActionReference grabReference;
        public InputActionReference pinchReference;

        //Grabbing
        public float grabRange = 0.1f;
        public GrabMode grabMode;
        public Transform palm;
        [SerializeField] private float reachDist = 0.1f; //, joinDist = 0.05f;

        private bool isGrabbing;
        private Grabable grabbedObject;
        private HandPoser handPoser;

        public Transform grabSpot;
        private bool generatedPoint;

        private Rigidbody targetBody;

        private Vector3 rotOffset = Vector3.zero;
        public Vector3 RotOffset
        {
            get { return rotOffset; }
            set { rotOffset = value; }
        }

        private FusionXRHand instance;
        public FusionXRHand Instance
        {
            get { return this; }
        }

        #endregion

        #region Start and Update
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            followObject = trackedController;
            handPoser = GetComponent<HandPoser>();

            if(trackingMode == TrackingMode.Joint)
            {
                SetupHandJoint();
            }

            grabReference.action.started += OnGrabbed;
            grabReference.action.canceled += OnLetGo;

            pinchReference.action.started += OnPinched;
            pinchReference.action.canceled += OnPinchedCancelled;

            lastVelocities = new List<Vector3>();
        }

        private void Update()
        {
            posWithOffset = followObject.TransformPoint(positionOffset);
            rotWithOffset = followObject.rotation * Quaternion.Euler(rotationOffset);

            switch ((int)trackingMode)
            {
                case 0:             //Case: kinematic tracking
                    TrackPositionKinematic(posWithOffset);
                    TrackRotationKinematic(rotWithOffset);
                    break;
                case 1:             //Case: velocity tracking;
                    TrackPositionVelocity(posWithOffset);
                    TrackRotationVelocity(rotWithOffset);
                    break;
                case 2:             //Case: joint tracking
                    TrackPosRotJoint(posWithOffset, rotWithOffset);
                    break;
            }
        }

        private void FixedUpdate()
        {
            //if (lastVelocities.Count >= storedVelocityHistory)
            //{
            //    lastVelocities.RemoveAt(0);
            //    lastVelocities.Add(rb.velocity);
            //}
            //else
            //{
            //    lastVelocities.Add(rb.velocity);
            //}
        }

        #endregion

        #region Events 
        //Grab Event Called if Grab Action is performed
        void OnGrabbed(InputAction.CallbackContext obj)
        {
            if (isGrabbing || (bool)grabbedObject)
            {
                return;
            }

            GameObject closestGrabable = ClosestGrabable(out Collider closestColl);

            if (closestGrabable == null)
                return;

            //Possible TODO: Add Check for Parent rb
            if (closestGrabable.TryGetComponent(out Rigidbody objectRigidbody))
            {
                grabbedObject = closestGrabable.GetComponent<Grabable>();

                targetBody = objectRigidbody;
            }
            else
            {
                return;
            }

            if(grabbedObject.TryGetClosestGrapPoint(transform.position, hand, out Transform grabPoint))
            {
                StartCoroutine(GrabObject(closestColl, grabPoint));
            }
            else
            {
                StartCoroutine(GrabObject(closestColl));
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
        //Always with a defined GrabPoint, if there is none, overload will generate one
        IEnumerator GrabObject(Collider closestColl, Transform givenGrabPoint)
        {
            grabSpot = givenGrabPoint;

            isGrabbing = true;
            grabbedObject.Grab(this);

            //Freeze
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            targetBody.velocity = Vector3.zero;
            targetBody.angularVelocity = Vector3.zero;

            targetBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            targetBody.interpolation = RigidbodyInterpolation.Interpolate;

            followObject = trackedController;

            if (generatedPoint)
            {
                Debug.Log("Generated Point " + grabSpot.transform.parent.name);
                handPoser.AttachHand(grabSpot);
            }
            else
            {
                GrabPoint grabPoint = grabSpot.GetComponent<GrabPoint>();

                if (grabPoint.hasCustomPose)
                {
                    Debug.Log("Static Point " + grabSpot.transform.parent.name);
                    handPoser.AttachHand(grabSpot, grabPoint.pose, false);
                }
                else
                {
                    Debug.Log("Physical Point " + grabSpot.transform.parent.name);
                    handPoser.AttachHand(grabSpot);
                }
            }

            yield return null;
        }

        //If no GrabPoint is set, auto generate
        IEnumerator GrabObject(Collider closestColl)
        {
            //Grab Point
            grabSpot = new GameObject().transform;
            grabSpot.position = closestColl.ClosestPoint(palm.position);
            grabSpot.localRotation = transform.rotation;
            grabSpot.parent = grabbedObject.transform;

            generatedPoint = true;

            //Raycasting to find GrabSpots Normal
            RaycastHit hit;

            Vector3 grabPointPosOffset = grabSpot.TransformPoint(Vector3.up * 0.2f);

            //Ray ray = new Ray(transform.position, grabSpot.position - transform.position);
            Ray ray = new Ray(grabPointPosOffset, grabSpot.position - grabPointPosOffset);

            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider == closestColl)
                {
                    Debug.DrawRay(ray.origin, ray.direction.normalized * 0.3f, Color.red, 4f);
                    Debug.Log("Raycast successfull | defining normal");

                    grabSpot.rotation = Quaternion.FromToRotation(grabSpot.up, hit.normal) * grabSpot.rotation;
                    grabSpot.position = grabSpot.TransformPoint(Vector3.up * 0.05f);    //TEST THIS
                }
            }

            StartCoroutine(GrabObject(closestColl, grabSpot));

            yield return null;
        }

        //As a function so it can also be called from a grabbable that wants to switch hands
        public void Release()
        {
            isGrabbing = false;
            followObject = trackedController;

            if (grabSpot != null && generatedPoint)
            {
                Destroy(grabSpot.gameObject);
                generatedPoint = false;
            }

            if (grabbedObject != null)
            {
                grabbedObject.Release(this);
                grabbedObject.GetComponent<Rigidbody>().velocity = rb.velocity;
                grabSpot = null;
                grabbedObject = null;
            }

            handPoser.ReleaseHand();
        }

        Vector3 AvgVel()
        {
            Vector3 allVel = new Vector3();

            for (int i = 0; i < lastVelocities.Count; i++)
            {
                allVel += lastVelocities[i];
            }

            return allVel / storedVelocityHistory;
        }

        GameObject ClosestGrabable(out Collider closestColl)
        {
            //find all close Colliders
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

        void TrackPositionVelocity(Vector3 targetPos)
        {
            var vel = (targetPos - transform.position).normalized * positionStrength * Vector3.Distance(targetPos, transform.position);
            rb.velocity = vel;
        }

        void TrackRotationVelocity(Quaternion targetRot)
        {
            Quaternion deltaRotation = targetRot * Quaternion.Inverse(transform.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);
            
            if (angle > 180f)
            {
                angle -= 360;
            }

            if(Mathf.Abs(axis.magnitude) != Mathf.Infinity)
                rb.angularVelocity = axis * (angle * rotationStrength * Mathf.Deg2Rad);
        }

        void TrackPositionKinematic(Vector3 targetPos)
        {
            transform.position = targetPos;
        }

        void TrackRotationKinematic(Quaternion targetRot)
        {
            transform.rotation = targetRot;
        }

        void TrackPosRotJoint(Vector3 targetPos, Quaternion targetRot)
        {
            if(armJoint != null)
            {
                UpdateHandJointDrives();
            }
            else
            {
                SetupHandJoint();
            }

            armJoint.targetPosition = playerRB.transform.InverseTransformPoint(targetPos);
            armJoint.targetRotation = Quaternion.Inverse(playerRB.rotation) * targetRot;

            UpdateTargetVelocity(targetPos);
        }

        void SetupHandJoint()
        {
            playerRB = Player.main.rigidbody;

            armJoint = Player.main.gameObject.AddComponent<ConfigurableJoint>();
            armJoint.connectedBody = this.rb;
            armJoint.autoConfigureConnectedAnchor = false;
            armJoint.anchor = Vector3.zero;
            armJoint.connectedAnchor = Vector3.zero;

            armJoint.enableCollision = false;
            armJoint.enablePreprocessing = false;

            armJoint.rotationDriveMode = RotationDriveMode.Slerp;

            UpdateHandJointDrives();
        }

        //Incase the Joint Drive Values need to be modified
        void UpdateHandJointDrives()
        {
            var drive = new JointDrive();
            drive.positionSpring = positionSpring;
            drive.positionDamper = positionDamper;
            drive.maximumForce = maxForce;

            armJoint.xDrive = armJoint.yDrive = armJoint.zDrive = drive;

            var slerpDrive = new JointDrive();
            slerpDrive.positionSpring = slerpSpring;
            slerpDrive.positionDamper = slerpDamper;
            slerpDrive.maximumForce = slerpMaxForce;

            armJoint.slerpDrive = slerpDrive;
        }

        //The Target Velcity of the Joint should be the the velcity of the contoller
        //ToDo: Add Camera Velocity
        void UpdateTargetVelocity(Vector3 targetPos)
        {
            var currentControllerPos = playerRB.transform.InverseTransformPoint(targetPos);
            var velocity = (currentControllerPos - lastControllerPos) / Time.fixedDeltaTime;
            lastControllerPos = currentControllerPos;

            armJoint.targetVelocity = velocity;
        }
        #endregion
    }
}
