using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(Animator))]
    public class HovercraftCanopy : MonoBehaviour
    {
        private Animator anim;
        [SerializeField] private bool IsOpen;
        private static readonly int Open = Animator.StringToHash("IsOpen");

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        /// <summary>
        /// Toggle the canopy open or closed, depending on its current state
        /// </summary>
        public void HandleCanopy()
        {
            IsOpen = !IsOpen;
            anim.SetBool(Open, IsOpen);
        }
        
        /// <summary>
        /// Set canopy state to a specific state
        /// </summary>
        /// <param name="isOpen"></param>
        public void HandleCanopy(bool isOpen)
        {
            //may not be needed but left in as an option just in case
            IsOpen = isOpen;
            anim.SetBool(Open, IsOpen);
        }
    }
}