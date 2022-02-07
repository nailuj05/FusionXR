using System.Collections;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicButton : KinematicInteractable
    {
        [SerializeField]
        private float pressDistance;
        [SerializeField]
        private float buttonMoveTime = 1;

        private Vector3 initialPosition;
        private Vector3 pressPosition;

        private void Start()
        {
            initialPosition = transform.localPosition;
            pressPosition = initialPosition + axis * pressDistance;
        }

        protected override void InteractionStart()
        {
            //Release hand immediatly?
            StopAllCoroutines();

            StartCoroutine(AnimateButtons());

            isInteracting = true;
        }

        IEnumerator AnimateButtons()
        {
            float lerp = 0;
            Vector3 startPosition = transform.localPosition;
            float speedAdj = Vector3.Distance(startPosition, pressPosition) / Mathf.Abs(pressDistance);

            while(lerp < 1)
            {
                lerp += Time.deltaTime / (buttonMoveTime * 0.5f) / speedAdj;
                transform.localPosition = Vector3.Lerp(startPosition, pressPosition, lerp);

                yield return new WaitForEndOfFrame();
            }

            lerp = 0;

            while (lerp < 1)
            {
                lerp += Time.deltaTime / (buttonMoveTime * 0.5f);
                transform.localPosition = Vector3.Lerp(pressPosition, initialPosition, lerp);

                yield return new WaitForEndOfFrame();
            }
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