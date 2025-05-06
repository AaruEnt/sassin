using UnityEngine;

namespace Cosmocat.InstantFish
{
    [CreateAssetMenuAttribute(fileName = "FishSpawnerProfile", menuName = "InstantFish/FishSpawnerProfile")]
    public class FishSpawnerProfile : ScriptableObject
    {

        /// <summary>
        /// Pool of fish prefabs to choose from
        /// </summary>
        public GameObject[] fish;

        // Pool of profiles to choose from
        public FishProfile[] fishProfiles; 

    }

}
