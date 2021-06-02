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

        //public TextAsset Pose;
        public HandPose pose;
        public string displayName = "";

        private GameObject prevHand;
        private Vector3 palmOffset = new Vector3(-0.35f, -0.21f, -0.012f);

        public void SpawnPoserHand(Transform obj)
        {
            //Get mesh Hand and place it
            prevHand = Resources.Load<GameObject>("PrevHandPrefab") as GameObject;
            Transform palm = prevHand.GetComponent<HandPoser>().palm;

            prevHand = Instantiate(prevHand);

            prevHand.transform.rotation = transform.rotation;
            prevHand.transform.position = transform.position;

            StartCoroutine(UpdateHandPos(obj, palm));
        }

        public void RemovePoserHand()
        {
            DestroyImmediate(prevHand);
        }

        public void LoadPose()
        {
            HandPoser handPoser = prevHand.GetComponent<HandPoser>();

            handPoser.RotateToPose(pose);
        }

        public void SavePose()
        {
            HandPoser handPoser = prevHand.GetComponent<HandPoser>();

            pose.SetAllRotations(handPoser.SavePose());
        }

        private IEnumerator UpdateHandPos(Transform obj, Transform palm)
        {
            while (isEditingPose)
            {
                prevHand.transform.rotation = transform.rotation;
                prevHand.transform.position = transform.position;

                yield return null;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PoseEditor))]
    public class PoseEditorWindow : Editor
    {
        bool hasCustomPose = false;

        public override void OnInspectorGUI()
        {
            PoseEditor poseEditor = (PoseEditor)target;

            if (poseEditor.isEditingPose != true)
            {
                if (GUILayout.Button("Edit Pose"))
                {
                    poseEditor.isEditingPose = true;
                    poseEditor.pose = CreateInstance<HandPose>();
                    hasCustomPose = false;
                    poseEditor.displayName = "";

                    poseEditor.SpawnPoserHand(poseEditor.transform);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("pose"));

                if (EditorGUI.EndChangeCheck())
                {
                    hasCustomPose = true;
                    poseEditor.displayName = poseEditor.pose.name;
                }

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
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
