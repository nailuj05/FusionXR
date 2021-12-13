using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class CollisionAdjuster : MonoBehaviour
    {
        public Transform p_XRRig;
        public Transform p_VRCamera;

        [Range(0.1f, 0.5f)]
        public float p_CollisionRadius = 0.2f;

        [HideInInspector]
        public float p_localHeight;

        private void Start()
        {
            p_VRCamera = Player.main.head;
        }

        //public Vector3 GetVRCamera

        private void Update()
        {
            //The local height of the camera (not the localPosition because localPos takes rotation into account)
            float p_height = transform.InverseTransformPoint(p_VRCamera.position).y;

            //the local Position of the Camera within the XRRig and half the height of the camera so it is exactly in the middle between floor and head
            Vector3 p_localCameraPosition = transform.InverseTransformPoint(p_VRCamera.position) - Vector3.up * p_height / 2;

            //Store players local height globally so other scripts can access it
            p_localHeight = p_height;

            //The Update Function is called, all overrides will interpret the data different now
            UpdateCollision(p_height, p_localCameraPosition, p_CollisionRadius);
        }

        public virtual void UpdateCollision(float p_height, Vector3 p_localCameraPosition, float p_CollisionRadius)
        {
            //Override this for the different collision types
        }
    }
}
