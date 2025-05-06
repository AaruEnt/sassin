using UnityEngine;

namespace Cosmocat.InstantFish
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class CookieAnimation : MonoBehaviour
    {
        public Texture2D[] frames;        
        public int framesPerSecond;

        private float cycle;
        private new Light light;
        private int frameIndex;

        private void Awake()
        {
            light = GetComponent<Light>();
        }

        void Start()
        {
            light.cookie = frames[0];
        }

        void Update()
        {
            cycle += Time.deltaTime;
            if (cycle > 1f / framesPerSecond)
            {
                cycle = 0f;
                frameIndex++;
                if (frameIndex == frames.Length - 1)
                {
                    frameIndex = 0;
                }
                light.cookie = frames[frameIndex];
            }
        }
    }
}