using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    [CustomEditor(typeof(FusionXRHand))] [CanEditMultipleObjects]
    public class FusionXRHandEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hand"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackedController"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackingMode"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("positionOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationOffset"));

            FusionXRHand fusionXRHand = (FusionXRHand)target;

            switch ((int)fusionXRHand.trackingMode)
            {
                case 0:
                    break;
                case 1:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("positionStrength"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationStrength"));
                    break;
                case 2:
                    //Check if the Player has a Rigidbody attached to it. Without a Rigidbody the Joints wont work.
                    if(fusionXRHand.GetComponentInParent<Player>().TryGetComponent(out Rigidbody rb))
                    {
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpring"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionDamper"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxForce"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slerpSpring"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slerpDamper"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slerpMaxForce"));
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("The Player needs to have a Rigidbody attached to it for the Joints to work", MessageType.Error);
                    }
                    break;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("grabMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grabRange"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("grabReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pinchReference"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("palm"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reachDist"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("joinDist"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
