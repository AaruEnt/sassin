using UnityEngine;

namespace Cosmocat.InstantFish
{
    [CreateAssetMenuAttribute(fileName = "FishProfile", menuName = "InstantFish/FishProfile")]
    public class FishProfile : ScriptableObject
    {
        public Material[] bodyMats;
        public Material[] finMats;
        public Material[] eyeMats;

        /// <summary>
        /// Should custom jaw materials be used? (otherwise, defaults to finMats)
        /// </summary>
        public bool useJawMats = false;
        public Material[] jawMats;

        /// <summary>
        /// Should custom head materials be used? (otherwise, defaults to bodyMats)
        /// </summary>
        public bool useHeadMat = false;
        public Material[] headMats;

        /// <summary>
        /// Should custom tail materials be used? (otherwise, defaults to bodyMats)
        /// </summary>
        public bool useTailFinMat = false;
        public Material[] tailFinMats;

        public Mesh[] dorsalFinMeshes;
        public Mesh[] pectoralFinMeshs;
        public Mesh[] tailFinMeshes;

    }

}
