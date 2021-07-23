using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Fusion.XR
{
    //[ExecuteAlways]
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

        [Range(0.05f, 2f)]
        public float poseLerpSpeed = 0.1f;

        [Header("Attachment")]
        [SerializeField] private bool isAttached;
        [SerializeField] private Transform attachedObj;
        [SerializeField] private bool poseLocked;
        public Transform palm;
        public Transform renderHand;

        [Header("Fingers")]
        public Finger[] fingers;
        public Vector3 fingerOffset;
        public float fingerTipRadius;
        public float fingerIncrement;
        public LayerMask collMask;

        [Header("Hand State")]
        public InputActionReference pinchReference;
        public InputActionReference grabReference;
        public Hand hand;
        private HandPose currentCustomPose;

        [Header("Debug Hand State")]
        private float pinchValue;
        private float grabValue;

        public float pinchState;
        public float grabState;

        public bool openPosed;
        public bool grabPosed;
        public bool pinchPosed;
        public bool pointPosed;

        public void Update()
        {
            pinchValue = pinchReference.action.ReadValue<float>();
            grabValue  =  grabReference.action.ReadValue<float>();

            //Pose Fingers Here

            if (isAttached)
            {
                Vector3 palmOffset = -palm.localPosition;

                renderHand.transform.position = attachedObj.TransformPoint(palmOffset); //Change this for left hand (?)
                renderHand.transform.rotation = attachedObj.transform.rotation;
            }

            if (poseLocked)
                return;

            #region HandState

            //Open Hand
            if (pinchValue == 0 && grabValue == 0)
            {
                if (openPosed)
                    return;

                pinchState = grabState = 0;

                openPosed = true;
                grabPosed = false;
                pinchPosed = false;
                pointPosed = false;

                LerpToPose(handOpen, poseLerpSpeed, 1);
            }
            //Grabbing
            if(pinchValue == 1 && grabValue == 1)
            {
                if (grabPosed)
                    return;

                pinchState = grabState = 1;

                openPosed = false;
                grabPosed = true;
                pinchPosed = false;
                pointPosed = false;

                LerpToPose(handClosed, poseLerpSpeed, 1);
            }
            //Pinching
            if (pinchValue == 1 && grabValue == 0)
            {
                if (pinchPosed)
                    return;

                pinchState = 1;
                grabState = 0;

                openPosed = false;
                grabPosed = false;
                pinchPosed = true;
                pointPosed = false;

                LerpToPose(handPinch, poseLerpSpeed, 1);
            }
            //Pointing
            if (pinchValue == 0 && grabValue == 1)
            {
                if (pointPosed)
                    return;

                pinchState = 0;
                grabState = 1;

                openPosed = false;
                grabPosed = false;
                pinchPosed = false;
                pointPosed = true;

                LerpToPose(handPoint, poseLerpSpeed, 1);
            }
            #endregion
        }


        #region Posing Functions

        public void AttachHand(Transform attachmentPoint)
        {
            AttachHand(attachmentPoint, handClosed, true);
        }

        public void AttachHand(Transform attachmentPoint, HandPose pose)
        {
            AttachHand(attachmentPoint, pose, false);
        }

        public void AttachHand(Transform attachmentPoint, HandPose pose, bool physicalPose)
        {
            poseLocked = true;
            isAttached = true;
            attachedObj = attachmentPoint;

            currentCustomPose = pose;

            if (physicalPose)
            {
                //Debug.Log("Physical Pose");
                TryLerpToPose(pose, poseLerpSpeed, 1);
            }
            else
            {
                //Debug.Log("Static Pose");
                LerpToPose(pose, poseLerpSpeed, 1);
            }
        }

        public void ReleaseHand()
        {
            poseLocked = false;
            isAttached = false;

            attachedObj = null;

            LerpToPose(handOpen, poseLerpSpeed, 1);

            renderHand.transform.localPosition = Vector3.zero;
            renderHand.transform.localRotation = Quaternion.identity;
        }

        public void LerpToPose(HandPose pose, float lerpTime, float maxLerp)
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].LerpToPose(pose.GetRotationByIndex(i), lerpTime, maxLerp);
            }
        }

        public void TryLerpToPose(HandPose pose, float lerpTime, float maxLerp)
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].TryLerpToPose(pose.GetRotationByIndex(i), lerpTime, maxLerp);
            }
        }

        public void SetupFingers()
        {
            foreach (Finger finger in fingers)
            {
                finger.offset = fingerOffset;
                finger.radius = fingerTipRadius;
                finger.increment = fingerIncrement;
                finger.collMask = collMask;
            }
        }

        #endregion

        #region Pose Editor Functions

        private void Start()
        {
            SetupFingers();
        }

        public void RotateToPose(HandPose pose)
        {
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
    [CustomEditor(typeof(HandPoser))] [CanEditMultipleObjects]
    public class HandPoserEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HandPoser handPoser = (HandPoser)target;

            if (GUILayout.Button("UpdateFingerGizmos"))
            {
                handPoser.SetupFingers();
            }
        }
    }
    #endregion
}
