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

        [SerializeField] private FusionXRHand leftHand;
        [SerializeField] private FusionXRHand rightHand;

        void Start()
        {
            GameObject currentRig;

            if(rigType == RigType.Mock)
            {
                hybridRig.SetActive(true);
                xrRig.SetActive(false);

                currentRig = hybridRig;
            }
            else
            {
                hybridRig.SetActive(false);
                xrRig.SetActive(true);

                currentRig = xrRig;
            }

            GameObject rControllerTarget = currentRig.GetChildByName("Right Tracked Controller", true);
            GameObject lControllerTarget = currentRig.GetChildByName("Left Tracked Controller",  true);

            rightHand.trackedController = rControllerTarget.transform;
            leftHand.trackedController  = lControllerTarget.transform;
        }
    }
}