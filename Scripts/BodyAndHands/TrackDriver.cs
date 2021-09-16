using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class TrackDriver
    {
        protected Transform trackTarget;

        public virtual void StartTrack(Transform objectToTrack)
        {
            trackTarget = objectToTrack;
        }

        public virtual void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {

        }

        public virtual void EndTrack()
        {

        }
    }

    public class KinematicDriver : TrackDriver
    {
        public override void StartTrack(Transform objectToTrack)
        {
            base.StartTrack(objectToTrack);
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            trackTarget.position = targetPosition;
            trackTarget.rotation = targetRotation;
        }

        public override void EndTrack()
        {

        }
    }

    public class VelocityDriver : TrackDriver
    {
        private Rigidbody rb;

        public float positionStrength = 15f;
        public float rotationStrength = 35f;

        public override void StartTrack(Transform objectToTrack)
        {
            rb = objectToTrack.GetComponent<Rigidbody>();
            base.StartTrack(objectToTrack);
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            //Track Position
            Vector3 deltaVelocity = (targetPosition - trackTarget.position) * positionStrength;

            rb.velocity = deltaVelocity;

            //Track Rotation
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(trackTarget.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (Mathf.Abs(axis.magnitude) != Mathf.Infinity)
                rb.angularVelocity = axis * (angle * rotationStrength * Mathf.Deg2Rad);
        }

        public override void EndTrack()
        {

        }
    }
}