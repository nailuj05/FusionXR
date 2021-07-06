using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class CollisionAdjuster : MonoBehaviour
    {
        public Transform p_XRRig;
        public Transform p_VRCamera;

        private void Awake()
        {
            p_VRCamera = Camera.main.transform;
        }

        private void Update()
        {
            float p_height = p_VRCamera.position.y;
            Vector3 p_localCameraPosition = p_XRRig.position - p_VRCamera.position;

            UpdateCollision(p_height, p_localCameraPosition);
        }

        public virtual void UpdateCollision(float p_height, Vector3 p_localCameraPosition)
        {
            //Override this for the different collision types
        }
    }
}
