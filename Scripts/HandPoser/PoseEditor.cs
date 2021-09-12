using System.IO;
using System.Text;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    [ExecuteInEditMode] [System.Serializable]
    public class PoseEditor : MonoBehaviour
    {
        public bool isEditingPose;
        public bool isLeftHand;

        //public TextAsset Pose;
        public HandPose pose;
        public string displayName = "";
        private HandPoser handPoser;

        private GameObject prevHand;
        private Vector3 palmOffset = new Vector3(-0.35f, -0.21f, -0.012f);

        public void SpawnPoserHand(Transform obj)
        {
            //Get mesh Hand and place it
            prevHand = Resources.Load<GameObject>("PrevHandPrefab") as GameObject;
            prevHand = Instantiate(prevHand);

            handPoser = prevHand.GetComponent<HandPoser>();
            handPoser.attachedObj = this.transform;
            Transform palm = handPoser.palm;

            StartCoroutine(UpdateHandPos(obj, palm));
        }

        public void RemovePoserHand()
        {
            DestroyImmediate(prevHand);
        }

        public void LoadPose()
        {
            handPoser.RotateToPose(pose);
        }

        public void SavePose()
        {
            pose.SetAllRotations(handPoser.SavePose());
            pose.isLeftHand = isLeftHand;
        }

        private IEnumerator UpdateHandPos(Transform obj, Transform palm)
        {
            while (isEditingPose)
            {
                handPoser.PlaceRenderHand();

                yield return null;
            }
        }

        public void SwitchHand(Hand hand)
        {
            handPoser.hand = hand;

            if(hand == Hand.Left)
            {
                prevHand.transform.localScale = new Vector3(-1, 1, 1);
            }
            if (hand == Hand.Right)
            {
                prevHand.transform.localScale = new Vector3(1, 1, 1);
            }

            //Refresh RenderHand, because Editor Update() is not reliable
            handPoser.PlaceRenderHand();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PoseEditor))]
    public class PoseEditorWindow : Editor
    {
        bool hasCustomPose = false;

        [SerializeField] GUIStyle leftStyle;
        [SerializeField] GUIStyle rightStyle;

        private void Awake()
        {
            rightStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { background = Texture2D.whiteTexture },
            };

            leftStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { background = Texture2D.grayTexture },
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PoseEditor poseEditor = (PoseEditor)target;

            if (poseEditor.isEditingPose != true)
            {
                if (GUILayout.Button("Edit Pose"))
                {
                    poseEditor.isEditingPose = true;
                    poseEditor.pose = CreateInstance<HandPose>();
                    poseEditor.isLeftHand = false;
                    hasCustomPose = false;
                    poseEditor.displayName = "";

                    poseEditor.SpawnPoserHand(poseEditor.transform);

                    if (poseEditor.GetComponent<GrabPoint>() != null)
                    {
                        GrabPoint grabPoint = poseEditor.GetComponent<GrabPoint>();

                        if (grabPoint.hasCustomPose)
                        {
                            poseEditor.pose = grabPoint.pose;
                            poseEditor.LoadPose();
                        }
                    }
                }
            }
            else
            {
                //Name and Pose
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("pose"));

                if (EditorGUI.EndChangeCheck())
                {
                    hasCustomPose = true;
                    poseEditor.displayName = poseEditor.pose.name;
                }

                EditorGUILayout.Space();

                //Left / Right Hand
                EditorGUILayout.BeginHorizontal("box");

                if (GUILayout.Button("Right Hand", rightStyle))
                {
                    poseEditor.SwitchHand(Hand.Right);

                    rightStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                    leftStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
                }

                if (GUILayout.Button("Left Hand", leftStyle))
                {
                    poseEditor.SwitchHand(Hand.Left);

                    leftStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                    rightStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
                }

                EditorGUILayout.EndHorizontal();

                //Save, Load, Update and Cancel
                EditorGUILayout.BeginHorizontal("Box");

                if (GUILayout.Button("UpdatePose"))
                {
                    poseEditor.LoadPose();
                }

                if(GUILayout.Button("New Pose"))
                {
                    poseEditor.isEditingPose = true;
                    poseEditor.pose = CreateInstance<HandPose>();
                    hasCustomPose = false;
                    poseEditor.displayName = "";
                }

                if (GUILayout.Button("SavePose"))
                {
                    if(poseEditor.displayName != "" || hasCustomPose)
                    {
                        poseEditor.SavePose();

                        poseEditor.isEditingPose = false;
                        poseEditor.RemovePoserHand();

                        string path = "Assets/FusionBasicXR/Poses/" + poseEditor.displayName + ".asset";

                        if (!hasCustomPose)
                        {
                            AssetDatabase.CreateAsset(poseEditor.pose, path);
                        }

                        AssetDatabase.Refresh();

                        HandPose assetToSave = AssetDatabase.LoadAssetAtPath<HandPose>(path);
                        EditorUtility.SetDirty(assetToSave);
                        EditorUtility.SetDirty(poseEditor.pose);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(assetToSave);

                        AssetDatabase.SaveAssets();

                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogWarning("No name set");
                    }
                }
                if(GUILayout.Button("Cancel"))
                {
                    poseEditor.isEditingPose = false;
                    poseEditor.RemovePoserHand();
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
