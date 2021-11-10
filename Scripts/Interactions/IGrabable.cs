using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public interface IGrabable
    {
        Transform Transform { get; }

        GameObject GameObject { get; }

        bool isGrabbed { get; }

        List<FusionXRHand> attachedHands { get; }

        void Grab(FusionXRHand hand, TrackingMode mode, TrackingBase trackingBase);

        void Release(FusionXRHand hand);

        Transform GetClosestGrabPoint(Vector3 point, Transform handTransform, Hand desiredHand, out GrabPoint grabPoint);
    }
}
