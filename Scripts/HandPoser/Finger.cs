using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class Finger : MonoBehaviour
    {
        public Transform[] fingerBones;

        public FingerDriver fingerDriver;
        private FingerTrackingBase fingerTrackingBase;

        public void FingerUpdate(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp)
        {
            fingerDriver.UpdateTrack(lastTargetRotations, targetRotations, currentLerp);
        }

        //Rotate to Pose instantly rotates to the pose

        public void RotateToPose(Quaternion[] rotations)
        {
            for (int i = 0; i < fingerBones.Length; i++)
            {
                fingerBones[i].localRotation = rotations[i];
            }
        }

        public Quaternion[] GetRotations()
        {
            Quaternion[] rotations = new Quaternion[3];

            for (int i = 0; i < fingerBones.Length; i++)
            {
                rotations[i] = fingerBones[i].localRotation;
            }

            return rotations;
        }

        /// <summary>
        /// Changes the settings of the fingers. Does not update the driver itself
        /// </summary>
        /// <param name="newFingerTrackingBase"></param>
        public void ChangeTrackingBase(FingerTrackingBase newFingerTrackingBase)
        {
            fingerTrackingBase = newFingerTrackingBase;
        }

        /// <summary>
        /// Changes the driver of the fingers.
        /// This does not update the TrackingSettings but uses the current ones
        /// </summary>
        /// <param name="newFingerDriver"></param>
        public void ChangeFingerDriver(FingerDriver newFingerDriver)
        {
            if (fingerDriver != null)
                fingerDriver.EndTrack();

            fingerDriver = newFingerDriver;
            fingerDriver.StartTrack(fingerTrackingBase);
        }

        public void SetupFingerBones()
        {
            fingerBones = new Transform[3];

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    fingerBones[0] = transform;
                else
                    fingerBones[i] = fingerBones[i - 1].GetChild(0);
            }
        }

        public static Vector3 GetFingerCollisionOffset(int fingerIndex, FingerTrackingBase trackingBase)
        {
            Vector3 fingerCollisionOffset = new Vector3();

            if (fingerIndex < trackingBase.fingerBones.Length - 1) //If the index is not the last finger
            {
                fingerCollisionOffset = trackingBase.fingerBones[fingerIndex + 1].localPosition;
            }
            else
            {
                fingerCollisionOffset = trackingBase.fingerBones[fingerIndex].localPosition + trackingBase.offset;
            }

            return fingerCollisionOffset;
        }

    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Finger))]
    [CanEditMultipleObjects]
    public class FingerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Finger finger = (Finger)target;

            if (GUILayout.Button("Setup Fingers"))
            {
                finger.SetupFingerBones();
            }
        }
    }
#endif

#endregion
}