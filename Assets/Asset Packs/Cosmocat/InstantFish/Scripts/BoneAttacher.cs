using System.Collections.Generic;
using UnityEngine;

namespace Cosmocat.InstantFish
{
    public class BoneAttacher : MonoBehaviour
    {
        public Transform armature;
        public List<TransformStringPair> parts = new List<TransformStringPair>();

        public Transform[] bones;

        public void Init()
        {
            GetBones();
            for (int i = 0; i < parts.Count; i++)
            {
                TransformStringPair part = parts[i];
                Transform targetBone = GetBoneByName(part.boneName);
                if (targetBone != null)
                {
                    AttachToBone(part.part, targetBone);
                }
                else
                {
                    Debug.Log("bone " + part.boneName + " is missing");
                }
            }
        }

        Transform GetBoneByName(string s)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].name == s)
                {
                    return bones[i];
                }
            }
            return null;
        }

        Transform[] GetBones()
        {
            bones = armature.GetComponentsInChildren<Transform>();
            return bones;
        }

        void AttachToBone(Transform part, Transform bone)
        {
            part.SetParent(bone);
        }
    }

    [System.Serializable]
    public class TransformStringPair
    {
        public Transform part;
        public string boneName;
    }

}