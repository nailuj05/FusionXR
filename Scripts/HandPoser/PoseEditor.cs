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
        private HandPoser handPoser;

        private GameObject prevHand;

        //Close all other opened editors
        private void Update()
        {
            if (!isEditingPose) return;

            var editors = FindObjectsOfType<PoseEditor>();

            foreach (var editor in editors)
            {
                if (editor.gameObject != Selection.gameObjects[0] & editor.isEditingPose)
                {
                    editor.isEditingPose = false;
                    editor.RemovePoserHand();
                }
            }
        }

        public void SpawnPoserHand(Transform obj)
        {
            //Get mesh Hand and place it
            prevHand = Resources.Load<GameObject>("PrevHandPrefab") as GameObject;
            prevHand = Instantiate(prevHand);

            handPoser = prevHand.GetComponent<HandPoser>();
            handPoser.attachedObj = this.transform;

            StartCoroutine(UpdateHandPos());
        }

        public void RemovePoserHand()
        {
            if (!prevHand) return;

            GetComponent<GrabPoint>().RotateToMatchHand(Hand.Right);
            DestroyImmediate(prevHand);
        }

        public void LoadPose()
        {
            handPoser.RotateToPose(pose);
        }

        public void SavePose()
        {
            pose.SetAllRotations(handPoser.SavePose());
        }

        private IEnumerator UpdateHandPos()
        {
            while (isEditingPose)
            {
                handPoser.PlaceRenderHand();
                yield return null;
            }
        }

        public void SwitchHand(GrabPointType type)
        {
            if(type == GrabPointType.Both || type == GrabPointType.Right)
            {
                SwitchHand(Hand.Right);
            }
            else
            {
                SwitchHand(Hand.Left);
            }
        }

        public void SwitchHand(Hand hand)
        {
            handPoser.hand = hand;

            if(hand == Hand.Left)
            {
                prevHand.transform.GetChild(0).localScale = new Vector3(-1, 1, 1);
                handPoser.palm = prevHand.GetChildByName("palmL").transform;
            }
            if (hand == Hand.Right)
            {
                prevHand.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
                handPoser.palm = prevHand.GetChildByName("palmR").transform;
            }

            GetComponent<GrabPoint>().RotateToMatchHand(hand);

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
                    //Close other editors
                    var editors = FindObjectsOfType<PoseEditor>();

                    foreach (var editor in editors)
                    {
                        if (editor.gameObject != poseEditor & editor.isEditingPose)
                        {
                            editor.isEditingPose = false;
                            editor.RemovePoserHand();
                        }
                    }

                    //Setup editor
                    poseEditor.isEditingPose = true;
                    poseEditor.pose = CreateInstance<HandPose>();
                    hasCustomPose = false;
                    poseEditor.displayName = "";

                    poseEditor.SpawnPoserHand(poseEditor.transform);

                    if (poseEditor.GetComponent<GrabPoint>() != null)
                    {
                        GrabPoint grabPoint = poseEditor.GetComponent<GrabPoint>();
                        poseEditor.SwitchHand(grabPoint.grabPointType);

                        if (grabPoint.hasCustomPose)
                        {
                            poseEditor.pose = grabPoint.pose;
                            poseEditor.LoadPose();
                        }

                        if (grabPoint.grabPointType == GrabPointType.Right)
                        {
                            rightStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                            leftStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
                        }
                        else if (grabPoint.grabPointType == GrabPointType.Left)
                        {
                            leftStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                            rightStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
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
