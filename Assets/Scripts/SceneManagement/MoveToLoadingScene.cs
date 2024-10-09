using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class MoveToLoadingScene : MonoBehaviour
{
    public int loadingSceneBuildNumber = 0;

    public void LoadLoadingScene(int nextScene)
    {
        SteamVR_Fade.View(Color.black, 1f);
        StartCoroutine(RealLoadScene(nextScene));
    }

    IEnumerator RealLoadScene(int nextScene)
    {
        LoadingData.sceneToLoad = nextScene;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(loadingSceneBuildNumber);
    }

    public void TempTutorialHelper() {
        PlayerPrefs.SetInt("BeatTutorial", 1);
    }
}
