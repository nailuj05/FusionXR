using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
#if UNITY_EDITOR
    public class PoseSetup : EditorWindow
    {
        [MenuItem("FusionXR/Pose/Add Grab Point")]
        static void AddGrabPoint()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            GameObject grabPoint = new GameObject();

            grabPoint.name = "GrabPoint";

            try
            {
                grabPoint.transform.parent = gameObjects[0].transform;
            }
            catch
            {
                grabPoint.transform.parent = null;
            }
            grabPoint.transform.localPosition = Vector3.zero;
            grabPoint.transform.localRotation = Quaternion.identity;

            grabPoint.AddComponent<GrabPoint>();
        }

        [MenuItem("FusionXR/Pose/Add Pose Editor")]
        static void SetupGrabPoint()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            gameObjects[0].AddComponent<PoseEditor>();
        }

    }
#endif
}
