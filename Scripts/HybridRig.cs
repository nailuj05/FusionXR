using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class HybridRig : MonoBehaviour
    {
        [SerializeField]
        private RigType rigType;

        [SerializeField] private GameObject hybridRig;
        [SerializeField] private GameObject xrRig;

        public GameObject currentRig { get; private set; }

        public void SetRig()
        {
            currentRig = GetCurrentRig();

            // Potentially change the XR Rig of the Collision Adjuster
            if (TryGetComponent(out CollisionAdjuster collisionAdjuster))
            {
                collisionAdjuster.p_XRRig = currentRig.transform;
            }

            var player = FindObjectOfType<Player>();

            player.head = currentRig.GetChildByName("Main Camera", true).transform;

            GameObject rControllerTarget = currentRig.GetChildByName("Right Tracked Controller", true);
            GameObject lControllerTarget = currentRig.GetChildByName("Left Tracked Controller",  true);

            player.RightHand.trackedController = rControllerTarget.transform;
            player.LeftHand.trackedController  = lControllerTarget.transform;

            var poserL = player.LeftHand.GetComponent<HandPoser>();
            var poserR = player.RightHand.GetComponent<HandPoser>();

            poserL.debugMode = poserR.debugMode = rigType == RigType.Mock;

            EditorUtility.SetDirty(poserR);
            EditorUtility.SetDirty(poserL);
            EditorUtility.SetDirty(player.LeftHand);
            EditorUtility.SetDirty(player.RightHand);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(player.head);
            EditorUtility.SetDirty(collisionAdjuster);
        }

        public GameObject GetCurrentRig()
        {
            if (rigType == RigType.Mock)
            {
                hybridRig.SetActive(true);
                xrRig.SetActive(false);

                return hybridRig;
            }
            else
            {
                hybridRig.SetActive(false);
                xrRig.SetActive(true);

                return xrRig;
            }
        }
    }

    [CustomEditor(typeof(HybridRig))]
    public class HybridRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Update Rig"))
            {
                ((HybridRig)target).SetRig();
            }
        }
    }
}