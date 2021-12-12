using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class HybridRig : MonoBehaviour
    {
        [SerializeField]
        private RigType rigType;

        [SerializeField] private GameObject hybridRig;
        [SerializeField] private GameObject xrRig;

        public GameObject currentRig { get; private set; }

        void Start()
        {
            currentRig = GetCurrentRig();

            // Potentially change the XR Rig of the Collision Adjuster
            if (TryGetComponent(out CollisionAdjuster collisionAdjuster))
            {
                collisionAdjuster.p_XRRig = currentRig.transform;
            }

            Player.main.head = Camera.main.transform;

            GameObject rControllerTarget = currentRig.GetChildByName("Right Tracked Controller", true);
            GameObject lControllerTarget = currentRig.GetChildByName("Left Tracked Controller",  true);

            Player.main.RightHand.trackedController = rControllerTarget.transform;
            Player.main.LeftHand.trackedController  = lControllerTarget.transform;
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
}