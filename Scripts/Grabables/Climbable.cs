using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class Climbable : Grabable
    {
        public override void Start()
        {
            gameObject.tag = "Grabable";
        }

        public override void FixedUpdate()
        {
            Vector3 deltaVelocity = Vector3.zero;

            if (attachedHands.Count > 1)
            {
                //Take the velocity of the grabbing hand
                deltaVelocity = attachedHands[0].rb.velocity;
            }
            else
            {
                //Average the Velocity between the 2 hands grabbing the object
                deltaVelocity = Vector3.Lerp(attachedHands[0].rb.velocity, attachedHands[0].rb.velocity, 0.5f);
            }

            deltaVelocity *= -1;

            Player.main.movement.Move(deltaVelocity);
        }
    }
}