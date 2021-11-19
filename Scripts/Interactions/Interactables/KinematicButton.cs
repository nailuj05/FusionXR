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
        private float buttonMoveTime = 1;

        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.localPosition;
        }

        protected override void InteractionStart()
        {
            //Release hand immediatly?
            Sequence s = DOTween.Sequence();

            s.Append(transform.DOLocalMove(initialPosition + axis * pressDistance, buttonMoveTime / 2));
            s.Append(transform.DOLocalMove(initialPosition, buttonMoveTime / 2));
            s.SetLoops(1);
            s.Play();

            isInteracting = true;
        }

        protected override void InteractionUpdate() { }

        protected override void InteractionEnd()
        {
            isInteracting = false;
        }

        private void OnDrawGizmosSelected()
        {
            
        }
    }
}