using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Finger : MonoBehaviour
{
    public Transform[] fingerBones;

    [HideInInspector] public Vector3 offset;
    [HideInInspector] public float radius;
    [HideInInspector] public float increment;
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

        while (lerp < maxLerp)
        {
            Collider[] colliders = Physics.OverlapSphere(bone.TransformPoint(offset), radius, collMask);

            if (colliders.Length == 0) //Only rotate if we didn't hit anything
            {
                bone.localRotation = Quaternion.Lerp(originalRot, targetPose[index], lerp);
            }

            lerp += Time.deltaTime * lerpTime;

            yield return new WaitForEndOfFrame();
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
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        foreach (Transform finger in fingerBones)
        {
            Gizmos.color = new Color(0, 0, 1, .4f);
            Gizmos.DrawSphere(finger.TransformPoint(offset), radius);
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