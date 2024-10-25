using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Com.Aaru.Sassin;
using UnityEngine.Events;

public class QuestStarter : MonoBehaviour
{
    [Scene]
    public string sceneToLoad = "TutorialDemo"; //Default val, set in QuestBoardPopulation
    internal Launcher launcher;
    public bool ignoreStart = false;
    public bool createNewRoom = false;
    public bool startOffline = false;
    public float delayTime = 0f;
    public UnityEvent<QuestStarter> OnBeforeScan;
    internal string mode = "None";
    private bool scanned = false;
    [Button]
    public void ManualStartGame() { StartGame(); }

    private void OnTriggerEnter(Collider other)
    {
        if (!ignoreStart && other.gameObject.CompareTag("QuestStart") && !scanned)
        {
            OnBeforeScan.Invoke(this);
            scanned = true;
            StartGame();
        }
    }

    private void StartGame()
    {
        UnityEngine.Debug.LogFormat("Mode: {0}, Scene: {1}", mode, sceneToLoad);
        launcher.gameMode = mode;
        launcher.CreateNewRoom(createNewRoom);
        launcher.UseOfflineMode(launcher.useOfflineMode | startOffline);
        if (delayTime > 0f)
        {
            StartCoroutine(DelayStart());
            return;
        }
        else
        {
            launcher.Connect(sceneToLoad);
            this.enabled = false;
        }
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(delayTime);
        launcher.Connect(sceneToLoad);
        this.enabled = false;
    }
}
