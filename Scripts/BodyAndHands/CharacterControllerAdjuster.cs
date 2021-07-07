using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerAdjuster : CollisionAdjuster
    {
        private CharacterController p_characterController;

        private void Awake()
        {
            p_characterController = GetComponent<CharacterController>();
        }

        public override void UpdateCollision(float p_height, Vector3 p_localPositionOffset, float p_CollisionRadius)
        {
            p_characterController.height = p_height;
            p_characterController.center = p_localPositionOffset;
            p_characterController.radius = p_CollisionRadius;
        }
    }
}