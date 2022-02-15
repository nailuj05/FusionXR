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
            if(hit.collider.TryGetComponent(out IGrabbable grabbable))
            {
                GrabPoint grabPoint = grabbable.GetClosestGrabPoint(hit.point, transform, currentHandPoser.hand);
                //TODO: Fix
                Transform grabPosition = null; //= grabPoint.GetAligned(currentHandPoser.Position);

                if (grabPoint == null)
                {
                    grabPosition = new GameObject().transform;
                    grabPosition.position = hit.point;
                    grabPosition.up = hit.normal;
                    grabPosition.parent = grabbable.Transform;
                    grabPosition.transform.localPosition += Vector3.up * 0.01f;
                }
                else if (grabPoint.hasCustomPose)
                {
                    currentHandPoser.AttachHand(grabPosition, grabPoint.pose);
                    return;
                }

                currentHandPoser.AttachHand(grabPosition);
            }
        }
    }
}
