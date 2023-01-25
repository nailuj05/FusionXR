using System.IO;
using System.Text;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
#if UNITY_EDITOR
    [ExecuteInEditMode] [System.Serializable]
    public class PoseEditor : MonoBehaviour
    {
        public bool isEditingPose;

        //public TextAsset Pose;
        public HandPose pose;
        public string displayName = "";
        private HandPoser handPoser;

        private GameObject prevHand;

        private void Awake()
        {
            isEditingPose = false;
        }

        //Close all other opened editors
        private void Update()
        {
            if (!isEditingPose) return;

            if(isEditingPose)
            {
                GetComponent<GrabPoint>()?.UpdateAlignedPoint();
                handPoser.PlaceRenderHand();
            }

            var editors = FindObjectsOfType<PoseEditor>();

            foreach (var editor in editors)
            {
                if (Selection.gameObjects.Length == 0) return;

                if (editor.gameObject != Selection.gameObjects[0] && editor.isEditingPose)
                {
                    if(Selection.gameObjects[0].TryGetComponent<PoseEditor>(out PoseEditor p))
                    {
                        editor.isEditingPose = false;
                        editor.RemovePoserHand();
                    }
                }
            }
        }

        public void SpawnPoserHand(Transform obj)
        {
            //Get mesh Hand and place it
            prevHand = Resources.Load<GameObject>("PrevHandPrefab") as GameObject;
            prevHand = Instantiate(prevHand);

            handPoser = prevHand.GetComponent<HandPoser>();

            if(TryGetComponent(out GrabPoint gripPoint))
            {
                handPoser.attachedObj = gripPoint.AlignedTransform;
            }
            else
            {
                handPoser.attachedObj = this.transform;
            }
        }

        public void RemovePoserHand()
        {
            if (!prevHand) return;

            GetComponent<GrabPoint>()?.ChangeCurrentHand(Hand.Right);
            GetComponent<GrabPoint>()?.RemoveAlignedForEditor();
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
                handPoser.renderHand.localScale = new Vector3(-1, 1, 1);
                handPoser.palm = prevHand.GetChildByName("palmL").transform;
            }
            if (hand == Hand.Right)
            {
                handPoser.renderHand.localScale = new Vector3(1, 1, 1);
                handPoser.palm = prevHand.GetChildByName("palmR").transform;
            }

            GetComponent<GrabPoint>()?.ChangeCurrentHand(hand);

            //Refresh RenderHand, because Editor Update() is not reliable
            handPoser.PlaceRenderHand();
        }

        public void NewPose(string name)
        {
            isEditingPose = true;
            pose = ScriptableObject.CreateInstance<HandPose>();
            pose.name = name;
            displayName = name;
        }
    }

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
                        GrabPoint gripPoint = poseEditor.GetComponent<GrabPoint>();
                        poseEditor.SwitchHand(gripPoint.gripPointType);

                        if (gripPoint.hasCustomPose)
                        {
                            poseEditor.pose = gripPoint.pose;
                            poseEditor.LoadPose();
                            poseEditor.displayName = gripPoint.pose.name;
                        }

                        if (gripPoint.gripPointType == GrabPointType.Right)
                        {
                            rightStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                            leftStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
                        }
                        else if (gripPoint.gripPointType == GrabPointType.Left)
                        {
                            leftStyle.normal = new GUIStyleState() { background = Texture2D.whiteTexture };
                            rightStyle.normal = new GUIStyleState() { background = Texture2D.grayTexture };
                        }
                    }
                }
            }
            else
            {
                #region Name, Pose and Hand Buttons
                //Name and Pose
                EditorGUILayout.LabelField("Pose Editor", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Currently Editing:", poseEditor.displayName, EditorStyles.boldLabel);

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
                #endregion

                //Save, Load, Update and Cancel
                EditorGUILayout.BeginHorizontal("Box");

                if (GUILayout.Button("UpdatePose"))
                {
                    poseEditor.LoadPose();
                }

                if (GUILayout.Button("New Pose"))
                {
                    NamePopout.Init(poseEditor);
                    hasCustomPose = false;
                }

                if (GUILayout.Button("SavePose"))
                {
                    if(poseEditor.displayName != "" || hasCustomPose)
                    {
                        poseEditor.SavePose();

                        poseEditor.isEditingPose = false;
                        poseEditor.RemovePoserHand();

                        string path = "Assets/FusionXR/Poses/" + poseEditor.displayName + ".asset";

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

                if (GUILayout.Button("Cancel"))
                {
                    poseEditor.isEditingPose = false;
                    poseEditor.RemovePoserHand();
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    public class NamePopout : EditorWindow
    {
        private PoseEditor contextEditor;
        private string name;

        public static void Init(PoseEditor context)
        {
            NamePopout w = ScriptableObject.CreateInstance<NamePopout>();
            w.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            w.contextEditor = context;
            w.titleContent = new GUIContent("New Pose");
            w.name = "New Pose";

            w.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("New Pose: ", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            name = EditorGUILayout.TextField(new GUIContent("Name: "), name);

            EditorGUILayout.Space();
            if (GUILayout.Button("Create!"))
            {
                contextEditor.NewPose(name);
                this.Close();
            }
        }
    }

#endif
}