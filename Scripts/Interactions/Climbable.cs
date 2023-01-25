using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class Climbable : Grabbable
    {
        //private Vector3 targetVelocity;

        //public override void Start()
        //{
        //    gameObject.tag = "Grabbable";
        //}

        //public override void Update()
        //{
        //    if (attachedHands.Count == 0)
        //        return;

        //    Vector3 deltaVelocity = Vector3.zero;

        //    if (attachedHands.Count > 1)
        //    {
        //        //Take the velocity of the gripbing hand
        //        deltaVelocity = attachedHands[0].rb.velocity;
        //    }
        //    else
        //    {
        //        //Average the Velocity between the 2 hands gripbing the object
        //        deltaVelocity = Vector3.Lerp(attachedHands[0].rb.velocity, attachedHands[0].rb.velocity, 0.5f);
        //    }

        //    deltaVelocity *= -1;

        //    targetVelocity = Vector3.MoveTowards(targetVelocity, deltaVelocity * 5, 1);

        //    Player.main.movement.Move(targetVelocity);
        //}
    }
}