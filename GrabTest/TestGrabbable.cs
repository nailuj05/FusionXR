using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.XR;

public class TestGrabbable : MonoBehaviour
{
    public GameObject[] grabPoints;

    public GameObject GetClosestGrabPoint(Vector3 pos)
    {
        if(grabPoints.Length > 0)
            return Utils.ClosestGameObject(grabPoints, pos);
        return null;
    }
}
