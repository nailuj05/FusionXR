using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleAdjuster : CollisionAdjuster
    {
        private CapsuleCollider p_capsuleCollider;

        private void Awake()
        {
            p_capsuleCollider = GetComponent<CapsuleCollider>();
            p_capsuleCollider.center = Vector3.up;
        }

        public override void UpdateCollision(float p_height, Vector3 p_localPositionOffset, float p_CollisionRadius)
        {
            p_capsuleCollider.height = p_height;
            p_capsuleCollider.center = p_localPositionOffset;
            p_capsuleCollider.radius = p_CollisionRadius;
        }
    }
}
