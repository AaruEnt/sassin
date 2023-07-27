using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToLoadingScene : MonoBehaviour
{
    public int loadingSceneBuildNumber = 0;

    public void LoadLoadingScene(int nextScene)
    {
        LoadingData.sceneToLoad = nextScene;
        SceneManager.LoadScene(loadingSceneBuildNumber);
    }
}
