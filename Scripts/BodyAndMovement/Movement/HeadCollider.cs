using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(SphereCollider))]
    public class HeadCollider : CollisionAdjuster
    {
        private SphereCollider sphereCollider;

        private void Start()
        {
            sphereCollider = GetComponent<SphereCollider>();
        }

        public override void UpdateCollision(float p_height, Vector3 p_localCameraPosition, float p_CollisionRadius)
        {
            sphereCollider.center = p_localCameraPosition + Vector3.up * (p_height / 2);
            sphereCollider.radius = p_CollisionRadius;
        }
    } 
}
