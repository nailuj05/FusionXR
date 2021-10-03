using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class GrabPointReach : GrabPoint
    {
        public float reachDistance = 0.2f;
        FusionXRHand[] hands;

        private void Start()
        {
            hands = new FusionXRHand[] { Player.main.LeftHand, Player.main.RightHand };

            StartCoroutine(AreHandsInReach());
        }

        IEnumerator AreHandsInReach()
        {
            while (true)
            {
                int handsInReach = 0;

                foreach (FusionXRHand hand in hands)
                {
                    if (Vector3.Distance(hand.transform.position, transform.position) < reachDistance)
                    {
                        handsInReach++;
                    }
                }

                if (handsInReach > 0)
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                }

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 0, 1, .2f);
            Gizmos.DrawSphere(transform.position, reachDistance);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GrabPointReach))] [CanEditMultipleObjects]
    public class GrabPointReachEditor : Editor
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

            EditorGUILayout.Slider(serializedObject.FindProperty("reachDistance"), 0.01f, 1f);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasCustomPose"));

            if (grabPoint.hasCustomPose)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pose"));
            }

            EditorGUILayout.HelpBox("This GrabPoint is " + serializedObject.FindProperty("isActive").boolValue, MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
