using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Finger : MonoBehaviour
{
    public Transform[] fingerBones;

    [HideInInspector] public Vector3 offset;
    [HideInInspector] public float radius;
    [HideInInspector] public LayerMask collMask;

    private Quaternion[] targetPose;

    private float lerp = 0;


    //Rotate to Pose instantly rotates to the pose

    public void RotateToPose(Quaternion[] rotations)
    {
        for (int i = 0; i < fingerBones.Length; i++)
        {
            fingerBones[i].localRotation = rotations[i];
        }
    }

    //Lerp to Pose interpolates smoothly to the pose

    public void LerpToPose(Quaternion[] rotations, float lerpTime, float maxLerp = 1)
    {
        targetPose = rotations;

        for (int i = 0; i < fingerBones.Length; i++)
        {
            StartCoroutine(LerpRotation(fingerBones[i], i, lerpTime, maxLerp));
        }
    }

    private IEnumerator LerpRotation(Transform bone, int index, float lerpTime, float maxLerp)
    {
        lerp = 0;

        Quaternion originalRot = bone.localRotation;

        while(lerp < maxLerp)
        {
            bone.localRotation = Quaternion.Lerp(originalRot, targetPose[index], lerp);
            lerp += Time.deltaTime * lerpTime;

            yield return new WaitForEndOfFrame();
        }
    }

    //Try Lerp to Pose takes collisions into account

    public void TryLerpToPose(Quaternion[] rotations, float lerpTime, float maxLerp = 1)
    {
        targetPose = rotations;

        for (int i = 0; i < fingerBones.Length; i++)
        {
            StartCoroutine(TryLerpRotation(fingerBones[i], i, lerpTime, maxLerp));
        }
    }

    private IEnumerator TryLerpRotation(Transform bone, int index, float lerpTime, float maxLerp)
    {
        lerp = 0;

        Quaternion originalRot = bone.localRotation;

        yield return new WaitForFixedUpdate();

        while (lerp < maxLerp)
        {
            Collider[] colliders = Physics.OverlapSphere(bone.TransformPoint(GetFingerCollisionOffset(index)), radius, collMask);

            if (colliders.Length == 0) //Only rotate if we didn't hit anything
            {
                bone.localRotation = Quaternion.Lerp(originalRot, targetPose[index], lerp);
            }

            lerp += Time.deltaTime * lerpTime;

            yield return new WaitForFixedUpdate();
        }
    }

    public Quaternion[] GetRotations()
    {
        Quaternion[] rotations = new Quaternion[3];

        for (int i = 0; i < fingerBones.Length; i++)
        {
            rotations[i] = fingerBones[i].localRotation;
        }

        return rotations;
    }

    public void SetupFingerBones()
    {
        fingerBones = new Transform[3];

        for (int i = 0; i < 3; i++)
        {
            if (i == 0)
                fingerBones[0] = transform;
            else
                fingerBones[i] = fingerBones[i - 1].GetChild(0);
        }
    }

    public Vector3 GetFingerCollisionOffset(int fingerIndex)
    {
        Vector3 fingerCollisionOffset = new Vector3();

        if(fingerIndex < fingerBones.Length - 1) //If the index is not the last finger
        {
            fingerCollisionOffset = fingerBones[fingerIndex + 1].localPosition;
        }
        else
        {
            fingerCollisionOffset = fingerBones[fingerIndex].localPosition + offset;
        }

        return fingerCollisionOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        for (int i = 0; i < fingerBones.Length; i++)
        {
            Gizmos.color = new Color(0, 0, 1, .4f);
            Gizmos.DrawSphere(fingerBones[i].TransformPoint(GetFingerCollisionOffset(i)), radius);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(Finger))]
[CanEditMultipleObjects]
public class FingerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Finger finger = (Finger)target;

        if (GUILayout.Button("Setup Fingers"))
        {
            finger.SetupFingerBones();
        }
    }
}
#endif