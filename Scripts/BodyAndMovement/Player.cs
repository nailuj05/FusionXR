using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Fusion.XR
{
    //This class holds all Components on the player and allows easy access and setup
    public class Player : MonoBehaviour 
    {
        public static Player main;

        public FusionXRHand LeftHand;
        public FusionXRHand RightHand;

        public Movement movement;
        public InputActionReference movementAction;
        public InputActionReference turnAction;

        public CollisionAdjuster collisionAdjuster;

        [HideInInspector]
        public Rigidbody rb;

        private void Awake()
        {
            main = this;
            rb = GetComponent<Rigidbody>();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Player))]
    public class PlayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameObject obj = ((Player)target).gameObject;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Setup Rigidbody"))
            {
                DestroyComponents(obj);

                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.mass = 100;

                obj.AddComponent<CapsuleCollider>().radius = 0.2f;
                RigidbodyMover move = obj.AddComponent<RigidbodyMover>();
                CollisionAdjuster adj = obj.AddComponent<CapsuleAdjuster>();

                Player player = obj.GetComponent<Player>();
                player.collisionAdjuster = adj;
                player.movement = move;

                try
                {
                    SetupAdjusterAndMovement(adj, move, player);

                    move.rigidBody = obj.GetComponent<Rigidbody>();
                }
                catch {}
            }
            if (GUILayout.Button("Setup CharacterController"))
            {
                DestroyComponents(obj);

                obj.AddComponent<CharacterController>().radius = 0.2f;
                Movement move = obj.AddComponent<CharacterControllerMover>();
                CollisionAdjuster adj = obj.AddComponent<CharacterControllerAdjuster>();

                Player player = obj.GetComponent<Player>();
                player.collisionAdjuster = adj;
                player.movement = move;

                try
                {
                    SetupAdjusterAndMovement(adj, move, player);
                }
                catch { }
            }

            EditorGUILayout.EndHorizontal();
        }

        void SetupAdjusterAndMovement(CollisionAdjuster adj, Movement move, Player player)
        {
            adj.p_VRCamera = GameObject.Find("Main Camera").transform;
            adj.p_XRRig = GameObject.Find("XR Rig").transform;

            move.head = GameObject.Find("Main Camera").transform;
            move.hand = GameObject.Find("HandLeft").transform;
            move.movementAction = player.movementAction;
            move.turnAction = player.turnAction;

            move.canMove = true;
        }

        void DestroyComponents(GameObject obj)
        {
            if (obj.TryGetComponent(out CapsuleAdjuster capA))
                DestroyImmediate(capA);
            if (obj.TryGetComponent(out CapsuleCollider capC))
                DestroyImmediate(capC);
            if (obj.TryGetComponent(out RigidbodyMover rbM))
                DestroyImmediate(rbM);
            if (obj.TryGetComponent(out Rigidbody rb))
                DestroyImmediate(rb);

            if (obj.TryGetComponent(out CharacterControllerMover ccM))
                DestroyImmediate(ccM);
            if (obj.TryGetComponent(out CharacterControllerAdjuster ccA))
                DestroyImmediate(ccA);
            if (obj.TryGetComponent(out CharacterController cc))
                DestroyImmediate(cc);
        }
    }
#endif
}
