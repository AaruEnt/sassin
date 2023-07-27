using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class LoadingScene : MonoBehaviour
{
    private float t = 0f;
    public float minTime = 4f;

    [Button]
    private void clearTutorialData() { PlayerPrefs.SetInt("BeatTutorial", 0); UnityEngine.Debug.Log("Cleared!"); }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log(LoadingData.sceneToLoad);
        if (LoadingData.sceneToLoad == 0)
        {
            if (PlayerPrefs.HasKey("BeatTutorial") && PlayerPrefs.GetInt("BeatTutorial") == 1)
                LoadingData.sceneToLoad = 1;
            else
                LoadingData.sceneToLoad = 2;
        }

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        UnityEngine.AsyncOperation operation = SceneManager.LoadSceneAsync(LoadingData.sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            t += Time.deltaTime;
            UnityEngine.Debug.Log(string.Format("Progress: {0}", operation.progress));
            if (operation.progress >= 0.9f && t >= minTime)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
