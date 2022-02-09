using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Fusion.XR
{
    /// <summary>
    /// A container for all the important parts that makeup the player.
    /// This class holds all Components on the player and allows easy access and setup.
    /// </summary>
    public class Player : Singleton<Player> 
    {
        public Transform head => Camera.main.transform;

        public FusionXRHand LeftHand;
        public FusionXRHand RightHand;

        [HideInInspector]
        private Rigidbody rb;
        public Rigidbody Rigidbody
        {
            get
            {
                if(rb == null)
                {
                    var pb = GetComponentInChildren<PhysicsBody>();

                    if (pb)
                    {
                        rb = pb.Chest;
                    }
                    else
                    {
                        rb = GetComponent<Rigidbody>();
                    }
                }

                return rb;
            }
            set
            {
                rb = value;
            }
        }

        public Movement movement;
        public InputActionReference movementAction;
        public InputActionReference turnAction;

        public CollisionAdjuster collisionAdjuster;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Player))]
    public class PlayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameObject obj = ((Player)target).gameObject;

            if (((Player)target).GetComponentInChildren<PhysicsBody>()) return;

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

                    move.rb = obj.GetComponent<Rigidbody>();
                }
                catch {}

                EditorUtility.SetDirty(obj);
                FindObjectOfType<HybridRig>().SetRig();
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

                EditorUtility.SetDirty(obj);
                FindObjectOfType<HybridRig>().SetRig();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Setup Hover Controller"))
            {
                DestroyComponents(obj);

                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.mass = 100;

                obj.AddComponent<SphereCollider>().radius = 0.2f;
                HoverMover move = obj.AddComponent<HoverMover>();
                CollisionAdjuster adj = obj.AddComponent<HeadCollider>();

                Player player = obj.GetComponent<Player>();
                player.collisionAdjuster = adj;
                player.movement = move;

                try
                {
                    SetupAdjusterAndMovement(adj, move, player);

                    move.rb = obj.GetComponent<Rigidbody>();
                }
                catch { }

                EditorUtility.SetDirty(obj);
                FindObjectOfType<HybridRig>().SetRig();
            }

            EditorGUILayout.EndHorizontal();
        }

        void SetupAdjusterAndMovement(CollisionAdjuster adj, Movement move, Player player)
        {
            adj.p_VRCamera = GameObject.Find("Main Camera").transform;

            try
            {
                adj.p_XRRig = player.GetComponent<HybridRig>().GetCurrentRig().transform;
            }
            catch
            {
                try
                {
                    adj.p_XRRig = GameObject.Find("XR Rig").transform;
                }
                catch
                {
                    Debug.LogError("Could not find XR/Mock Rig, set up Rig/Hybrid Rig correctly or assign Collision Adjusters XR Rig var manually.");
                }
            }

            move.head = GameObject.Find("Main Camera").transform;
            move.hand = GameObject.Find("HandLeft").transform;
            move.movementAction = player.movementAction;
            move.turnAction = player.turnAction;

            move.canMove = true;
        }

        void DestroyComponents(GameObject obj)
        {
            obj.TryDestroyComponent<CollisionAdjuster>();
            obj.TryDestroyComponent<Movement>();
            obj.TryDestroyComponent<Rigidbody>();
            obj.TryDestroyComponent<Collider>();
            obj.TryDestroyComponent<CharacterController>();
        }
    }
#endif
}
