using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AntiClipping : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] LayerMask detectionLayers;

    [Header("Events")]
    public UnityEvent ClippingStart;
    public UnityEvent ClippingEnd;

    private Vector3 currentPos;
    private Vector3 lastPos;

    public bool isClipping { get; private set; }

    private void FixedUpdate()
    {
        currentPos = transform.position;

        if (Physics.Linecast(lastPos, currentPos, detectionLayers))
        {
            if (isClipping == false)
                ClippingStart?.Invoke();

            isClipping = true;
        }
        else
        {
            if (isClipping == true)
                ClippingEnd?.Invoke();

            lastPos = transform.position;
            isClipping = false;
        }
    }
}
