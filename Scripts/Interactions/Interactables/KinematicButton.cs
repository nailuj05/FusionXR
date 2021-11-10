using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Fusion.XR
{
    public class KinematicButton : KinematicInteractable
    {
        [SerializeField]
        private float pressDistance;
        [SerializeField]
        private Transform button;

        protected override void InteractionStart()
        {
            //Release hand immediatly?
            button.DOLocalMove(axis * pressDistance, 0.5f).SetLoops(2, LoopType.Yoyo);
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
