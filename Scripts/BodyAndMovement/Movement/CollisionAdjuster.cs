using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class CollisionAdjuster : MonoBehaviour
    {
        public Transform XRRig;
        public Transform VRCamera;

        [Range(0.1f, 0.5f)]
        public float CollisionRadius = 0.2f;

        [SerializeField] [ReadOnly]
        private float _localHeight;
        public float localHeight { get { return _localHeight; } private set { _localHeight = value; } }

        public void Awake()
        {
            VRCamera = Player.main?.head;

            if (!XRRig)
            {
                if (HybridRig.main)
                    XRRig = HybridRig.main?.currentRig.transform;
                else
                    XRRig = transform;
            }
        }

        protected float p_height;
        protected Vector3 p_localCameraPosition;
        private void Update()
        {
            //The local height of the camera (not the localPosition because localPos takes rotation into account)
            p_height = XRRig.InverseTransformPoint(VRCamera.position).y;

            //the local Position of the Camera within the XRRig and half the height of the camera so it is exactly in the middle between floor and head
            p_localCameraPosition = transform.InverseTransformPoint(VRCamera.position) - Vector3.up * p_height / 2;

            //Store players local height globally so other scripts can access it
            localHeight = p_height;

            //The Update Function is called, all overrides will interpret the data different now
            UpdateCollision(p_height, p_localCameraPosition, CollisionRadius);
        }

        public virtual void UpdateCollision(float p_height, Vector3 p_localCameraPosition, float p_CollisionRadius)
        {
            //Override this for the different collision types
        }
    }
}
