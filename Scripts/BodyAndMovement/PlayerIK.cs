using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PlayerIK : MonoBehaviour
    {
        [Header("Body")]
        [SerializeField] Transform root, head;
        [SerializeField] float headDistanceOffset;
        [SerializeField] Vector3 bodyOffset;

        [Header("Head")]
        [SerializeField] Transform headIKTarget;
        [SerializeField] Vector3 headPositionOffset, headRotationOffset;

        [Header("Hands")]
        [SerializeField] Transform rightIKTarget, leftIKTarget;
        [SerializeField] Vector3 posROffset, rotROffset;
        [SerializeField] Vector3 posLOffset, rotLOffset;

        private float autoHeadHeight;

        private void Start()
        {
            autoHeadHeight = Vector3.Distance(head.position, root.position);
        }

        void Update()
        {
            //Root positioning
            root.position = Player.main.head.transform.TransformPoint(bodyOffset + headPositionOffset) + Vector3.down * (autoHeadHeight + headDistanceOffset);
            root.forward  = Vector3.ProjectOnPlane(Player.main.head.transform.forward, Vector3.up);

            //Head Rotation+Offset
            headIKTarget.position = Player.main.head.TransformPoint(headPositionOffset);
            headIKTarget.rotation = Player.main.head.transform.rotation * Quaternion.Euler(headRotationOffset);

            //Hand IKs
            rightIKTarget.position = Player.main.RightHand.transform.TransformPoint(posROffset);
            leftIKTarget.position  = Player.main.LeftHand.transform.TransformPoint(posLOffset);

            rightIKTarget.rotation = Player.main.RightHand.transform.rotation * Quaternion.Euler(rotROffset);
            leftIKTarget.rotation  = Player.main.LeftHand.transform.rotation  * Quaternion.Euler(rotLOffset);
        }
    }

}