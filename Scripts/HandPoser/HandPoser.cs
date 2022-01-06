using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Fusion.XR
{
    public class HandPoser : MonoBehaviour
    {
        [Header("Poses")]

        [Tooltip("The Pose of an open Hand")]
        public HandPose handOpen;

        [Tooltip("The Pose of an closed Hand")]
        public HandPose handClosed;

        [Tooltip("The Pose of an even more open (strechted open) Hand to be called just before grabbing")]
        public HandPose handAwait;

        [Tooltip("The Pose of an Hand Pinching with Thump and Index Finger")]
        public HandPose handPinch;

        [Tooltip("The Pose of an closed Hand with the Index Finger pointing outwards")]
        public HandPose handPoint;

        [Range(0.5f, 5f)]
        public float poseLerpSpeed = 0.1f;

        [Header("Attachment")]
        //Whether the Renderhand is attached to something of following the controller
        [SerializeField] private bool isAttached;
        //The Object the Renderhand is following if it is attached
        [HideInInspector] public Transform attachedObj;
        //Whether the hand is grabbing something and the pose should not be overridden by the HandState
        [SerializeField] private bool poseLocked;

        [Tooltip("The hand will smoothly move to its attachement Point allowing better proc fingerplacement")]
        public bool smoothAttachment = true;
        public float smoothAttachSpeed = 0.5f;
        private float attachLerp = 0;

        public Transform palm;
        public Transform renderHand;

        [Header("Fingers")]
        public FingerTrackingMode handPosingDriver = FingerTrackingMode.Kinematic;
        public FingerTrackingMode grabbingDriver = FingerTrackingMode.CollisionTest;
        public FingerTrackingBase fingerSettings = new FingerTrackingBase();
        public Finger[] fingers;

        [Header("Hand State")]
        [Tooltip("Debug Mode is used to control the grab and pinch values form script, rather than using controller data")]
        public bool debugMode;
        public InputActionReference pinchReference;
        public InputActionReference grabReference;
        public Hand hand;

        private HandPose currentPose;
        private List<Quaternion[]> lastHandState;

        private float currentLerp;

        [Header("Debug Hand State")]
        private float pinchValue;
        private float grabValue;

        public float pinchState;
        public float grabState;

        public HandState handState = HandState.open;

        void Start()
        {
            //Default Setup and tracking Base Config
            UpdateDriverAndTracking(handPosingDriver);

            currentPose = handOpen;
            lastHandState = SavePose();
        }

        public void Update()
        {
            if (!debugMode)
            {
                pinchValue = pinchReference.action.ReadValue<float>();
                grabValue = grabReference.action.ReadValue<float>();
            }

            if (isAttached)
            {

                if (!smoothAttachment || attachLerp > 1)
                {
                    PlaceRenderHand();
                }
                else
                {
                    LerpRenderHand();
                }
            }

            FingerUpdate();

            #region HandState
            if (!poseLocked)
            {
                //Open Hand
                if (pinchValue == 0 && grabValue == 0)
                {
                    if (handState == HandState.open)
                        return;

                    pinchState = grabState = 0;

                    handState = HandState.open;

                    SetPoseTarget(handOpen);
                }
                //Grabbing
                if (pinchValue > 0 && grabValue > 0)
                {
                    if (handState == HandState.grab)
                        return;

                    pinchState = grabState = 1;

                    handState = HandState.grab;

                    SetPoseTarget(handClosed);
                }
                //Pinching
                if (pinchValue > 0 && grabValue == 0)
                {
                    if (handState == HandState.pinch)
                        return;

                    pinchState = 1;
                    grabState = 0;

                    handState = HandState.pinch;

                    SetPoseTarget(handPinch);
                }
                //Pointing
                if (pinchValue == 0 && grabValue > 0)
                {
                    if (handState == HandState.point)
                        return;

                    pinchState = 0;
                    grabState = 1;

                    handState = HandState.point;

                    SetPoseTarget(handPoint);
                }
            }
            #endregion
        }

        public void PlaceRenderHand()
        {
            if (Application.isPlaying)
            {
                renderHand.transform.position = attachedObj.TransformPoint(-palm.localPosition);
                renderHand.transform.rotation = attachedObj.transform.rotation;
            }
            else
            {
                renderHand.transform.position = attachedObj.TransformPoint(-palm.localPosition);
                renderHand.transform.rotation = attachedObj.transform.rotation;
            }
        }

        public void LerpRenderHand()
        {
            var targetPos = attachedObj.TransformPoint(-palm.localPosition + Vector3.up * 0.003f);
            var lerpPos = attachedObj.TransformPoint(-palm.localPosition);

            renderHand.transform.position = Vector3.Lerp(lerpPos, targetPos, attachLerp);
            renderHand.transform.rotation = attachedObj.transform.rotation;

            attachLerp += Time.deltaTime / smoothAttachSpeed;
        }

        public void SetPinchGrabDebug(float pinch, float grab)
        {
            pinchValue = pinch;
            grabValue = grab;
        }

#region Posing Functions

        public void AttachHand(Transform attachmentPoint)
        {
            AttachHand(attachmentPoint, handClosed, false);
        }

        public void AttachHand(Transform attachmentPoint, HandPose pose)
        {
            AttachHand(attachmentPoint, pose, true);
        }

        public void AttachHand(Transform attachmentPoint, HandPose pose, bool customPose)
        {
            //Debug.Log($"Attaching with {pose} custom: {customPose}");
            poseLocked = true;
            isAttached = true;
            attachLerp = 0;
            attachedObj = attachmentPoint;

            //Whether we should use default or Kinematic tracking (for predfined poses)
            UpdateTracking(customPose ? FingerTrackingMode.Kinematic : grabbingDriver);

            RotateToPose(handAwait);

            SetPoseTarget(pose);
        }

        private void SetPoseTarget(HandPose pose)
        {
            lastHandState = SavePose();
            currentPose = pose;

            currentLerp = 0;
        }

        private void FingerUpdate()
        {
            //This right?
            currentLerp += currentLerp <= 1f ? Time.deltaTime * poseLerpSpeed : 0;

            if (currentLerp >= 1)
                return;

            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].FingerUpdate(lastHandState[i], currentPose.GetRotationByIndex(i), currentLerp);
            }
        }

        public void ReleaseHand()
        {
            poseLocked = false;
            isAttached = false;

            attachedObj = null;

            //SetPoseTarget(handOpen);
            RotateToPose(handOpen);

            renderHand.transform.localPosition = Vector3.zero;
            renderHand.transform.localRotation = Quaternion.identity;
        }

        public void UpdateDriverAndTracking(FingerTrackingMode driver)
        {
            UpdateTrackingBase();
            UpdateTracking(driver);
        }

        public void UpdateTrackingBase()
        {
            foreach (var finger in fingers)
            {
                //Create a new Tracking Base with the same values as "Finger Settings", won't work with a pointer to Finger Settings
                FingerTrackingBase trackingBase = new FingerTrackingBase();
                trackingBase.fingerBones = finger.fingerBones;
                trackingBase.collMask = fingerSettings.collMask;
                trackingBase.offset = fingerSettings.offset;
                trackingBase.radius = fingerSettings.radius;

                finger.ChangeTrackingBase(trackingBase);
            }
        }

        public void UpdateTracking(FingerTrackingMode driver)
        {
            //Debug.Log($"Changed driver to {driver}");
            foreach (var finger in fingers)
            {
                finger.ChangeFingerDriver(Utils.FingerDriverFromEnum(driver));
            }
        }

#endregion

#region Pose Editor Functions

        public void RotateToPose(HandPose pose)
        {
            //PoseTarget still needs to be set before rotating to Pose instantly
            SetPoseTarget(handAwait);

            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].RotateToPose(pose.GetRotationByIndex(i));
            }
        }

        public List<Quaternion[]> SavePose()
        {
            List<Quaternion[]> allRots = new List<Quaternion[]>
            {
                new Quaternion[3],
                new Quaternion[3],
                new Quaternion[3],
                new Quaternion[3],
                new Quaternion[3]
            };

            for (int i = 0; i < fingers.Length; i++)
            {
                allRots[i] = fingers[i].GetRotations();
            }

            return allRots;
        }

#endregion
    }

#region Editor

#if UNITY_EDITOR
    [CustomEditor(typeof(HandPoser))] [CanEditMultipleObjects]
    public class HandPoserEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HandPoser handPoser = (HandPoser)target;

            if (GUILayout.Button("UpdateFingerGizmos"))
            {
                handPoser.UpdateDriverAndTracking(handPoser.grabbingDriver);
            }
        }
    }
#endif

#endregion
}
