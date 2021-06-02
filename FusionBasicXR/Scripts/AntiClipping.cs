using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AntiClipping : MonoBehaviour
{
    [SerializeField] LayerMask detectionLayers;
    [SerializeField] Volume volume;

    [Range(0.1f, 1f)]
    [SerializeField] float smoothTime = 0.3f;

    private Vector3 currentPos;
    private Vector3 lastPos;

    private bool isInWall;
    public bool IsInWall
        {
            get { return isInWall; }
        }

    private float refVel;

    private int firstFrame = 0;

    private void LateUpdate()
    {
        if (firstFrame < 4)
        {
            firstFrame++;
            lastPos = transform.position;
            return;
        }

        currentPos = transform.position;

        Debug.DrawLine(currentPos, lastPos);

        if(Physics.Linecast(lastPos, currentPos, detectionLayers))
        {
            isInWall = true;
        }
        else
        {
            lastPos = transform.position;
            isInWall = false;
        }

        //Todo: Use DO BETWEEN
        volume.weight = Mathf.SmoothDamp(volume.weight, System.Convert.ToInt32(isInWall), ref refVel, smoothTime);

        if (volume.weight < 0.1f)
            volume.weight = 0;
    }

    IEnumerator SmoothWeight(float smoothTime, int factor)
    {
        float currentSmoothTime = smoothTime;

        while(currentSmoothTime > 0.01f)
        {
            currentSmoothTime -= Time.deltaTime;

            Debug.Log(currentSmoothTime);

            volume.weight += factor * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        if (volume.weight > 1) volume.weight = 1;
        else if (volume.weight < 0) volume.weight = 0;
    }
}
