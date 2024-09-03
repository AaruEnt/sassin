using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Cosmocat.InstantFish
{
    public class FishSlideshow : MonoBehaviour
    {
        public Fish[] fishes;
        public FishProfile[] fishProfiles;
        public float slideTime = 0.5f;
        public TMP_Text text;
        public TMP_Text pauseButtonText;
        public CanvasGroup canvasGroup;

        private string template = "<b>Fish type:</b> {0}\n<b>Profile:</b> {1}";
        private Fish lastFish;
        private bool pause;

        private void Awake()
        {
            foreach(Fish fish in fishes)
            {
                fish.gameObject.SetActive(false);
                fish.move = false;
            }
        }

        void Start()
        {
            StartCoroutine(NextSlideCo());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (canvasGroup.interactable)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.alpha = 0f;
                }
                else
                {
                    canvasGroup.interactable = true;
                    canvasGroup.alpha = 1f;
                }
            }
        }

        IEnumerator NextSlideCo()
        {
            while(true)
            {
                foreach (FishProfile profile in fishProfiles)
                {

                    while(pause)
                    {
                        yield return null;
                    }

                    Fish fish = fishes[Random.Range(0, fishes.Length)];

                    if (lastFish && lastFish != fish)
                    {
                        lastFish.gameObject.SetActive(false);
                    }

                    text.text = string.Format(template, fish.gameObject.name, fish.profile.name);

                    fish.gameObject.SetActive(true);
                    fish.profile = profile;
                    fish.RefreshProfile();

                    lastFish = fish;
                    yield return new WaitForSeconds(slideTime);                    
                }
            }
        }

        public void OnAquariumClick()
        {
#if UNITY_EDITOR
            EditorSceneManager.LoadScene("Aquarium");
#else
            SceneManager.LoadScene("Aquarium");
#endif
        }

        public void OnPauseClick()
        {
            pause = !pause;
            pauseButtonText.text = pause ? "RESUME" : "PAUSE";
        }
    }

}

