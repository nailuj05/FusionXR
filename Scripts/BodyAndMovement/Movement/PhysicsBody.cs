using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [ReadOnly]
        public float actualHeight;

        [Header("Body Settings")]
        public LayerMask playerMask;

        [Range(0.1f, 0.9f)] public float chestPercent = 0.3f;
        [Range(0.1f, 0.9f)] public float legsPercent = 0.7f;
        public float heightAdjustmentFactor = 0.05f;

        public CollisionDetector LocoSphereCollDetector;
        [ReadOnly] public bool isGrounded;
        [ReadOnly] public Collider walkedCollider;

        [Header("Joint Settings")]
        public float jointStrength = 5000;
        public float jointDampener = 500;
        public float jointMaxStrength = 1000;

        [Header("Tracking Settings")]
        public float FenderHeight = 0.1f;

        [Header("Jumping")]
        public float retractSpeed = 2f;
        private float retractAmount = 1;
        private float targetRetract = 1;

        [Header("Rigidbodys")]
        public Rigidbody Head;
        public Rigidbody Chest;
        public Rigidbody Legs;
        public Rigidbody LocoSphere;

        [Header("Colliders")]
        public SphereCollider HeadCol;
        public CapsuleCollider ChestCol;
        public CapsuleCollider LegsCol;
        public SphereCollider FenderCol;
        public SphereCollider LocoSphereCollider;

        [Header("Joints")]
        public ConfigurableJoint ChestJoint;
        public ConfigurableJoint LegsJoint;
        private ConfigurableJoint HeadJoint;

        [Header("Debug Objects")]
        public bool renderDebugObjects = true;
        public GameObject d_LocoSphere;
        public GameObject d_Fender;
        public GameObject d_Legs;
        public GameObject d_Chest;

        #region Private vars to avoid frame allocations

        //FixedUpdate
        Vector3 cameraPos;

        //Jumping
        //Init Ray with a inital downward direction
        Ray jumpRay = new Ray(Vector3.zero, Vector3.down);
        RaycastHit jumpRayHit;

        //Update Chest/Legs
        float heightPercent;
        Vector3 positionToReach;

        //HandleHMDMovement
        Vector3 delta;
        Vector3 deltaHead;

        //HandleHMDRotation
        Quaternion deltaRot;
        private float step = 500;
        float lastEulers, newEulers, deltaEulers, targetEulers;

        //StopHorizontalMomentum
        Vector3 vel;
        #endregion

        private void Start()
        {
            //Set Rigidbody on Player
            Player.main.Rigidbody = Chest;

            //Ground Detection
            LocoSphereCollDetector.CollisionEnter += (c) => { walkedCollider = c;    isGrounded = true;  };
            LocoSphereCollDetector.CollisionStay  += (c) => { walkedCollider = c;    isGrounded = true;  };
            LocoSphereCollDetector.CollisionExit  += (c) => { walkedCollider = null; isGrounded = false; };

            //Setup Joints and Drives
            Chest.WakeUp();

            HeadJoint = SetupJoint(Chest, Head);
            UpdateJointDrive(ChestJoint);
            UpdateJointDrive(LegsJoint);

            //Update alle Joints initally
            UpdateChest();
            UpdateLegs();
            PlaceFender();

            //Interpolate all RBs
            Head.interpolation = Chest.interpolation = Legs.interpolation = LocoSphere.interpolation = RigidbodyInterpolation.Extrapolate;

            ToggleDebugObjects(renderDebugObjects);
        }

        void FixedUpdate()
        {
            //Get Head Info
            cameraPos = GetCameraGlobal();
            actualHeight = GetActualHeight();

            //Update Retract Amount
            retractAmount = Mathf.MoveTowards(retractAmount, targetRetract, retractSpeed * Time.fixedDeltaTime);

            //Updated Body
            UpdateHead();
            UpdateChest();
            UpdateLegs();

            PlaceFender();

            //Adjust for HMD (Playspace) Movement
            HandleHMDMovement();
            HandleHMDRotation();
        }

        #region Body

        private void UpdateHead()
        {
            HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos);
        }

        private void UpdateChest()
        {
            heightPercent = chestPercent;

            ChestJoint.targetPosition = new Vector3(0, -localHeight * heightPercent * retractAmount - LocoSphereCollider.radius, 0);

            ChestCol.height = actualHeight - (actualHeight * heightPercent - LocoSphereCollider.radius);
        }

        private void UpdateLegs()
        {
            heightPercent = (chestPercent * legsPercent * retractAmount);

            LegsJoint.targetPosition = new Vector3(0, localHeight * heightPercent, 0);

            LegsCol.height = actualHeight * heightPercent + LocoSphereCollider.radius;
        }

        private void PlaceFender()
        {
            FenderCol.transform.position = LocoSphereCollider.transform.position + Vector3.up * FenderHeight;
        }

        #endregion

        #region Jumping & Crouching

        public void StartCrouch()
        {
            targetRetract = 0.7f;
        }

        public void StartJump(float jumpForce)
        {
            targetRetract = 1;

            if (!isGrounded) return;

            Chest.AddForce(Vector3.up * jumpForce);

            StartCoroutine(Jump());
        }

        IEnumerator Jump()
        {
            //Wait for Locosphere to leave the ground
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            //Retract the legs
            targetRetract = 0.5f;

            //Wait for touching the ground again
            while (!isGrounded)
            {
                yield return new WaitForFixedUpdate();

                //Check if we reached the peak of the jump
                if (Chest.velocity.y < 0)
                {
                    jumpRay.origin = Head.position;

                    //If there is a floor below the legs expand them again
                    if (Physics.Raycast(jumpRay, out jumpRayHit, actualHeight, playerMask))
                        break;
                }
            }

            targetRetract = 1;
        }

        #endregion

        #region HMD Movement and Rotation

        private void HandleHMDMovement()
        {
            delta = VRCamera.position - Chest.transform.position;
            delta.y = 0f;

            deltaHead = VRCamera.position - Head.transform.position;
            XRRig.transform.localPosition += Chest.transform.InverseTransformDirection(deltaHead.y * Vector3.down);

            if (delta.magnitude > 0.01f)
            {
                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                Head.MovePosition(Head.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
        }

        //Do we need to expose step to the user?
        private void HandleHMDRotation()
        {
            newEulers = Mathf.MoveTowardsAngle(lastEulers, XRRig.transform.eulerAngles.y - VRCamera.transform.eulerAngles.y, step * Time.deltaTime);
            deltaEulers = (lastEulers - newEulers);

            deltaRot = Quaternion.AngleAxis(deltaEulers, Vector3.up);

            Chest.MoveRotation(Chest.rotation * deltaRot);

            XRRig.RotateAround(VRCamera.position, Vector3.up, -deltaEulers);

            lastEulers = newEulers;
        }
        #endregion

        #region Debug Objects

        private void LateUpdate()
        {
            if (renderDebugObjects)
            {
                AlignObjectWithCollider(ChestCol, d_Chest);
                AlignObjectWithCollider(LegsCol, d_Legs);
            }
        }

        private void ToggleDebugObjects(bool enabled)
        {
            d_Chest.SetActive(enabled);
            d_Fender.SetActive(enabled);
            d_Legs.SetActive(enabled);
            d_LocoSphere.SetActive(enabled);
        }

        private void AlignObjectWithCollider(CapsuleCollider coll, GameObject gameObject)
        {
            gameObject.transform.position = coll.transform.TransformPoint(coll.center);
            gameObject.transform.localScale = new Vector3(coll.radius * 2, coll.height / 2, coll.radius * 2);
        }  
        #endregion

        #region Helper Functions

        private Vector3 GetCameraInRigSpace()
        {
            return LocoSphere.transform.localPosition + Vector3.up * (localHeight - LocoSphereCollider.radius);
        }

        private Vector3 GetCameraGlobal()
        {
            return Chest.position + Vector3.up * (localHeight * (1 - chestPercent) - LocoSphereCollider.radius - heightAdjustmentFactor);
        }

        private float GetActualHeight()
        {
            return Head.transform.position.y - LocoSphere.transform.position.y + LocoSphereCollider.radius;
        }

        private void StopHorizontalMomentum(Rigidbody rb)
        {
            vel = rb.velocity;
            vel.x = 0;
            vel.z = 0;
            rb.velocity = vel;
        }

        JointDrive drive;
        private void UpdateJointDrive(ConfigurableJoint joint)
        {
            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = jointMaxStrength;

            joint.xDrive = joint.yDrive = joint.zDrive = drive;
            joint.xMotion = joint.zMotion = ConfigurableJointMotion.Locked;
        }

        private ConfigurableJoint SetupJoint(Rigidbody connectTo, Rigidbody connectedBody)
        {
            ConfigurableJoint joint = connectTo.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectedBody;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;

            joint.xMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;

            var drive = new JointDrive();
            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = jointMaxStrength;

            joint.xDrive = joint.yDrive = joint.zDrive = drive;

            return joint;
        }
        #endregion
    }
}