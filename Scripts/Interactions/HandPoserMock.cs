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
                Transform grabPointTransform = grabbable.GetClosestGrabPoint(hit.point, transform, currentHandPoser.hand, out GrabPoint grabPoint);

                if(grabPoint == null)
                {
                    grabPointTransform = new GameObject().transform;
                    grabPointTransform.position = hit.point;
                    grabPointTransform.up = hit.normal;
                    grabPointTransform.parent = grabbable.Transform;
                    grabPointTransform.transform.localPosition += Vector3.up * 0.01f;
                }
                else if (grabPoint.hasCustomPose)
                {
                    currentHandPoser.AttachHand(grabPointTransform, grabPoint.pose);
                    return;
                }

                currentHandPoser.AttachHand(grabPointTransform);
            }
        }
    }
}
