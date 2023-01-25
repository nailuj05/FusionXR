using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.XR;

public class TestGrabbable : MonoBehaviour
{
    public GameObject[] gripPoints;

    public TrackingBase trackingBase;
    private TrackDriver gripDriver;

    private TestGrabber gripber;

    public bool isGrabbed;

    public GameObject GetClosestGrabPoint(Vector3 pos)
    {
        if(gripPoints.Length > 0)
            return Utils.ClosestGameObject(gripPoints, pos);
        return null;
    }

    private void Update()
    {
        if(isGrabbed)
            gripDriver.UpdateTrack(gripber.targetPosition, gripber.targetRotation);
    }

    private void FixedUpdate()
    {
        if(isGrabbed)
            gripDriver.UpdateTrackFixed(gripber.targetPosition, gripber.targetRotation);
    }

    public void Grab(TestGrabber g)
    {
        gripber = g;
        isGrabbed = true;

        trackingBase.tracker = gripber.transform;
        trackingBase.palm = gripber.palm;
        gripDriver = Utils.DriverFromEnum(TrackingMode.FixedJoint);
        gripDriver.StartTrack(transform, trackingBase);
    }
}
