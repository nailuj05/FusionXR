using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [CreateAssetMenu(menuName = "FusionXR/New Pose")]
    [System.Serializable]
    public class HandPose : ScriptableObject
    {
        public Quaternion[] thumbRots = new Quaternion[3];
        public Quaternion[] indexRots = new Quaternion[3];
        public Quaternion[] middleRots = new Quaternion[3];
        public Quaternion[] ringRots = new Quaternion[3];
        public Quaternion[] pinkyRots = new Quaternion[3];

        public List<Quaternion[]> GetAllRotations()
        {
            List<Quaternion[]> allRotations = new List<Quaternion[]>
            {
                thumbRots,
                indexRots,
                middleRots,
                ringRots,
                pinkyRots
            };

            return allRotations;
        }

        public Quaternion[] GetRotationByIndex(int index)
        {
            return GetAllRotations()[index];
        }

        public void SetAllRotations(List<Quaternion[]> rotations)
        {
            for (int i = 0; i < 5; i++)
            {
                SetRotationByIndex(rotations[i], i);
            }
        }

        public void SetRotationByIndex(Quaternion[] rotations, int index)
        {
            switch (index)
            {
                case 0:
                    thumbRots = rotations;
                    break;
                case 1:
                    indexRots = rotations;
                    break;
                case 2:
                    middleRots = rotations;
                    break;
                case 3:
                    ringRots = rotations;
                    break;
                case 4:
                    pinkyRots = rotations;
                    break;
            }
        }
    }
}