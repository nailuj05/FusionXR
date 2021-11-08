using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    /// <summary>
    /// This will set the buttons reach position to its positiv or negative maxium position allowed by the joint so it cannot be "pulled out".
    /// </summary>
    [ExecuteAlways]
    public class ButtonHelper : EditorWindow
    {
        [MenuItem("Fusion/XR/Setup Button (+)")]
        public static void ButtonMaximumPos()
        {
            SetupJoint(1);
        }

        [MenuItem("Fusion/XR/Setup Button (-)")]
        public static void ButtonMaximumNeg()
        {
            SetupJoint(-1);
        }

        static void SetupJoint(int maximum)
        {
            GameObject obj = Selection.gameObjects[0];

            ConfigurableJoint joint = obj.GetComponent<ConfigurableJoint>();
            var limit = joint.linearLimit.limit;

            Vector3 axis = Vector3.zero;
            if (joint.xMotion == ConfigurableJointMotion.Limited) axis[0] = 1;
            if (joint.yMotion == ConfigurableJointMotion.Limited) axis[1] = 1;
            if (joint.zMotion == ConfigurableJointMotion.Limited) axis[2] = 1;

            Object[] objectsToChange = { joint, joint.connectedBody.transform };
            Undo.RecordObjects(objectsToChange, "Set Button Maxium");

            joint.connectedBody.transform.position += joint.transform.TransformVector(axis * limit * -maximum);

            joint.targetPosition = axis * limit * maximum;
        }
    }
}
