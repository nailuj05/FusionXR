using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class GrabPoint : MonoBehaviour
    {
        private Vector3 palmOffset = new Vector3(-0.035f, -0.021f, -0.0012f);

        public bool hasCustomPose;
        public HandPose pose;

        private void OnDrawGizmos()
        {
            if(!(TryGetComponent<PoseEditor>(out PoseEditor pe) && pe.isEditingPose))
            {
                Mesh hand = Resources.Load<Mesh>("PrevHand") as Mesh;

                //Debug.Log(transform.parent.name + " | " + transform.lossyScale.x + " & " + transform.localScale.x);

                Gizmos.color = Color.green;
                Gizmos.DrawWireMesh(hand, transform.TransformPoint(palmOffset), transform.rotation, transform.localScale * 0.01f);
            }
        }
    }
}
