using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : MonoBehaviour
{
    public float speed, stretch, add;

    void Update()
    {
        var pos = transform.localPosition;
        pos.y = add + Mathf.Sin(Time.timeSinceLevelLoad * speed) * stretch;
        transform.localPosition = pos;
    }
}