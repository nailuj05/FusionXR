using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsulAdjuster : CollisionAdjuster
    {
        private CapsuleCollider p_capsuleCollider;

        private void Awake()
        {
            p_capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public override void UpdateCollision(float p_height, Vector3 p_localPositionOffset)
        {
            p_capsuleCollider.height = p_height;
            p_capsuleCollider.center = p_localPositionOffset + Vector3.up * p_height / 2;
        }
    }
}
