using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class Movement : MonoBehaviour
    {
        public float playerSpeed = 2f;

        public virtual void Move(Vector3 direction)
        {

        }

        public virtual void Move(Vector3 direction, float customSpeed)
        {

        }
    }
}
