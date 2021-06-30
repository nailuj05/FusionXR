using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public enum GrabPointType
    {
        Left = 0,
        Right = 1,
        Both = 2
    }

    public class GrabPoint : MonoBehaviour
    {
        private Vector3 palmOffset = new Vector3(-0.035f, -0.021f, -0.0012f);

        public GrabPointType grabPointType;

       public bool isActive = true;

        public bool hasCustomPose;
        public HandPose pose;

        private void OnDrawGizmos()
        {
            if(!(TryGetComponent<PoseEditor>(out PoseEditor pe) && pe.isEditingPose))
            {
                Mesh hand = Resources.Load<Mesh>("PrevHand") as Mesh;

                Vector3 scale = transform.localScale;
                Vector3 adjustedPalmOffset = palmOffset;

                if(grabPointType == GrabPointType.Left)
                {
                    scale -= 2 * Vector3.right;

                    adjustedPalmOffset.x *= -1;
                    adjustedPalmOffset.z *= -1;
                }

                Gizmos.color = new Color(0, 1, 0, .5f);
                Gizmos.DrawMesh(hand, transform.TransformPoint(adjustedPalmOffset), transform.rotation, scale * 0.01f);
            }
        }

        public bool CorrectHand(Hand hand)
        {
            if((int)hand == (int)grabPointType) //TRUE: if hands match
            {
                return true;
            }
            else if(grabPointType == GrabPointType.Both) //TRUE: if both hands are allowed
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [CustomEditor(typeof(GrabPoint))]
    public class GrabPointEditor : Editor
    {
        GUIStyle rightStyle;
        GUIStyle leftStyle;
        GUIStyle bothStyle;

        GUIStyle highlightedStyle;
        GUIStyle unhighlightedStyle;

        private void Awake()
        {
            highlightedStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { background = Texture2D.whiteTexture },
            };

            unhighlightedStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { background = Texture2D.grayTexture },
            };

            rightStyle = highlightedStyle;
            leftStyle = unhighlightedStyle;
            bothStyle = unhighlightedStyle;
        }

        public override void OnInspectorGUI()
        {
            GrabPoint grabPoint = (GrabPoint)target;

            EditorGUILayout.BeginHorizontal("box");

            rightStyle = unhighlightedStyle;
            leftStyle = unhighlightedStyle;
            bothStyle = unhighlightedStyle;

            switch ((int)grabPoint.grabPointType)
            {
                case 0: //Left Hand
                    leftStyle = highlightedStyle;
                    break;
                case 1: //Right Hand
                    rightStyle = highlightedStyle;
                    break;
                case 2: //Both Hand
                    bothStyle = highlightedStyle;
                    break;
            }

            if (GUILayout.Button("Right", rightStyle))
            {
                grabPoint.grabPointType = GrabPointType.Right;

                rightStyle = highlightedStyle;
                leftStyle = unhighlightedStyle;
                bothStyle = unhighlightedStyle;
            }
            if (GUILayout.Button("Left", leftStyle))
            {
                grabPoint.grabPointType = GrabPointType.Left;

                rightStyle = unhighlightedStyle;
                leftStyle = highlightedStyle;
                bothStyle = unhighlightedStyle;
            }
            if (GUILayout.Button("Both", bothStyle))
            {
                grabPoint.grabPointType = GrabPointType.Both;

                rightStyle = unhighlightedStyle;
                leftStyle = unhighlightedStyle;
                bothStyle = highlightedStyle;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasCustomPose"));

            if (grabPoint.hasCustomPose)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pose"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
