using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsPlayerMockInput : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ((LocoSphereMover)Player.main.movement).OnJump();
            }
        }
    } 
}
