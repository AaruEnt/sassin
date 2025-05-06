using UnityEngine;

namespace Cosmocat.InstantFish
{
    public class FishSpawner : MonoBehaviour
    {
        /// <summary>
        /// Type(s) of fish to spawn
        /// </summary>
        public FishSpawnerProfile[] spawnerProfiles;

        /// <summary>
        /// Bounding box for fish spawning
        /// </summary>
        public Bounds bounds = new Bounds(Vector3.zero, new Vector3(8f, 8f, 8f));

        /// <summary>
        /// Should fish cast shadows?
        /// </summary>
        public bool castShadows;

        /// <summary>
        /// Should fish receive shadows?
        /// </summary>
        public bool receiveShadows;

        /// <summary>
        /// Percentage of bounds to fill with fish
        /// </summary>
        [Range(1, 100)] public int fishDensity = 50;

        /// <summary>
        /// Size of grid to use for spawning
        /// </summary>
        private int gridSize = 2;

        void Awake()
        {
            if (bounds.extents.x < gridSize ||
                bounds.extents.y < gridSize ||
                bounds.extents.z < gridSize)
            {
                Debug.LogError("Bounds too small");
            }
        }

        void Start()
        {
            Spawn();            
        }        

        public void Spawn()
        {
            int layers = (int)((bounds.extents.y * 2f) / gridSize);

            int startY = (int)(bounds.center.y - layers + (gridSize / 2));
            int endY = (int)(bounds.center.y + layers - (gridSize / 2));

            for (int y = startY; y <= endY; y += gridSize)
            {
                SpawnLayer(y);
            }
        }

        private void SpawnLayer(int y)
        {
            int xSteps = (int)((bounds.extents.x * 2f) / gridSize);
            int startX = (int)(bounds.center.x - xSteps + (gridSize / 2));
            int endX = (int)(bounds.center.x + xSteps - (gridSize / 2));

            int zSteps = (int)((bounds.extents.z * 2f) / gridSize);
            int startZ = (int)(bounds.center.z - zSteps + (gridSize / 2));
            int endZ = (int)(bounds.center.z + zSteps - (gridSize / 2));

            for(int x = startX; x <= endX; x += gridSize )
            {
                for (int z = startZ; z <= endZ; z += gridSize)
                {
                    if(Random.Range(0f, 100f) <= fishDensity)
                    {
                        SpawnFish(new Vector3(x, y, z));
                    }
                }
            }
        }

        private void SpawnFish(Vector3 spawnPos)
        {
            if(Physics.CheckBox(spawnPos, new Vector3(gridSize /2f, gridSize / 2f, gridSize /2f)))
            {
                return;
            }

            var randomFish = spawnerProfiles[Random.Range(0, spawnerProfiles.Length)];
            var randomPrefab = randomFish.fish[Random.Range(0, randomFish.fish.Length)];
            var randomProfile = randomFish.fishProfiles[Random.Range(0, randomFish.fishProfiles.Length)];
            var go = Instantiate(randomPrefab, new Vector3(spawnPos.x, spawnPos.y, spawnPos.z), Quaternion.identity);

            var fish = go.GetComponent<Fish>();
            fish.profile = randomProfile;
            fish.castShadows = castShadows;
            fish.receiveShadows = receiveShadows;
            fish.InitShadows();
        }

    }
}

