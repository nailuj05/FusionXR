using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMover : Movement
    {
        public CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public override void Move(Vector3 direction)
        {
            //TODO: Check this
            CurrentVelocity = direction / Time.deltaTime;
            controller.Move(direction * Time.deltaTime);
        }
    }
}
