using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEvents : MonoBehaviour
{
    public SceneType type = SceneType.Home;
    public delegate void SceneEvent();
    public static SceneEvent OnSceneLoaded;

    private void Start()
    {
        OnSceneLoaded.Invoke();
    }
}

public enum SceneType
{
    Playable,
    Transition,
    Home
}
