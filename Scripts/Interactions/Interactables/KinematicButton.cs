using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicButton : KinematicInteractable
    {
        [SerializeField]
        private float pressDistance;

        protected override void InteractionStart()
        {
            if (isInteracting)
            {

            }

            isInteracting = true;
        }

        protected override void InteractionUpdate()
        {

        }

        protected override void InteractionEnd()
        {

        }
    }
}
