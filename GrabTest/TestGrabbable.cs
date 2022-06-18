using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.XR;

public class TestGrabbable : MonoBehaviour
{
    public GameObject[] grabPoints;

    public TrackingBase trackingBase;
    private TrackDriver grabDriver;

    private TestGrabber grabber;

    public bool isGrabbed;

    public GameObject GetClosestGrabPoint(Vector3 pos)
    {
        if(grabPoints.Length > 0)
            return Utils.ClosestGameObject(grabPoints, pos);
        return null;
    }

    private void Update()
    {
        if(isGrabbed)
            grabDriver.UpdateTrack(grabber.targetPosition, grabber.targetRotation);
    }

    private void FixedUpdate()
    {
        if(isGrabbed)
            grabDriver.UpdateTrackFixed(grabber.targetPosition, grabber.targetRotation);
    }

    public void Grab(TestGrabber g)
    {
        grabber = g;
        isGrabbed = true;

        trackingBase.tracker = grabber.gameObject;
        trackingBase.palm = grabber.palm;
        grabDriver = Utils.DriverFromEnum(TrackingMode.FixedJoint);
        grabDriver.StartTrack(transform, trackingBase);
    }
}
