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

    void Update()
    {
        var mousePos = Input.mousePosition;
        var mouseRay = Camera.main.ScreenPointToRay(mousePos);

        if (!Input.GetMouseButtonDown(0))
            return;

        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            if(hit.collider.TryGetComponent(out Grabable grabable))
            {
                var grabPoint = grabable.GetClosestGrabPoint(hit.point, currentHandPoser.hand);

                if(grabPoint == null)
                {
                    grabPoint = new GameObject().transform;
                    grabPoint.position = hit.point;
                    grabPoint.up = hit.normal;
                    grabPoint.parent = grabable.transform;
                    grabPoint.transform.localPosition += Vector3.up * 0.01f;
                }

                currentHandPoser.AttachHand(grabPoint);
            }
        }
    }
}
