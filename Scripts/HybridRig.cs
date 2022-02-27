using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class HybridRig : Singleton<HybridRig>
    {
        [SerializeField]
        private RigType rigType;

        [SerializeField] private GameObject mockRig;
        [SerializeField] private GameObject xrRig;

        public GameObject currentRig { get { return GetCurrentRig(); } private set { currentRig = value; } }

        private void OnValidate()
        {
            SetRig();
        }

        public void SetRig()
        {
            // Potentially change the XR Rig of the Collision Adjuster
            if (TryGetComponent(out CollisionAdjuster collisionAdjuster))
            {
                collisionAdjuster.XRRig = currentRig.transform;
            }

            var player = Player.main;
            //var player = FindObjectOfType<Player>();

            //player.head = currentRig.GetChildByName("Main Camera", true).transform;

            GameObject rControllerTarget = currentRig.GetChildByName("Right Tracked Controller", true);
            GameObject lControllerTarget = currentRig.GetChildByName("Left Tracked Controller",  true);

            player.RightHand.trackedController = rControllerTarget.transform;
            player.LeftHand.trackedController  = lControllerTarget.transform;

            var poserL = player.LeftHand.GetComponent<HandPoser>();
            var poserR = player.RightHand.GetComponent<HandPoser>();

            poserL.debugMode = poserR.debugMode = rigType == RigType.Mock;

#if UNITY_EDITOR
            EditorUtility.SetDirty(poserR);
            EditorUtility.SetDirty(poserL);
            EditorUtility.SetDirty(player.LeftHand);
            EditorUtility.SetDirty(player.RightHand);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(player.head);
            if(collisionAdjuster)
                EditorUtility.SetDirty(collisionAdjuster);
#endif
        }

        public GameObject GetCurrentRig()
        {
            if (rigType == RigType.Mock)
            {
                mockRig.SetActive(true);
                xrRig.SetActive(false);

                return mockRig;
            }
            else
            {
                mockRig.SetActive(false);
                xrRig.SetActive(true);

                return xrRig;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HybridRig))]
    public class HybridRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox($"Currently: {((HybridRig)target).GetCurrentRig().name}", MessageType.Info);

            if(GUILayout.Button("Update Rig"))
            {
                ((HybridRig)target).SetRig();
            }
        }
    }
#endif
}