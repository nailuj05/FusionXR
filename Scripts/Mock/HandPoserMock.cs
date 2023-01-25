using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.XR;

public class HandPoserMock : MonoBehaviour
{
    public HandPoser handPoserL;
    public HandPoser handPoserR;

    private HandPoser currentHandPoser;

    private void Start()
    {
        currentHandPoser = handPoserR;
    }

    public void SwitchHand()
    {
        currentHandPoser = currentHandPoser == handPoserR ? handPoserL : handPoserR;
    }

    void Update()
    {
        var mousePos = Input.mousePosition;
        var mouseRay = Camera.main.ScreenPointToRay(mousePos);

        if (!Input.GetMouseButtonDown(0))
            return;

        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            if(hit.collider.TryGetComponent(out IGrabbable gripbable))
            {
                GrabPoint gripPoint = gripbable.GetClosestGrabPoint(hit.point, transform, currentHandPoser.hand);
                //TODO: Fix
                Transform gripPosition = null; //= gripPoint.GetAligned(currentHandPoser.Position);

                if (gripPoint == null)
                {
                    gripPosition = new GameObject().transform;
                    gripPosition.position = hit.point;
                    gripPosition.up = hit.normal;
                    gripPosition.parent = gripbable.Transform;
                    gripPosition.transform.localPosition += Vector3.up * 0.01f;
                }
                else if (gripPoint.hasCustomPose)
                {
                    currentHandPoser.AttachHand(gripPosition, gripPoint.pose);
                    return;
                }

                currentHandPoser.AttachHand(gripPosition);
            }
        }
    }
}
