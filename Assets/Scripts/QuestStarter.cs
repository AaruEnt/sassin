using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Com.Aaru.Sassin;

public class QuestStarter : MonoBehaviour
{
    [Scene]
    public string sceneToLoad = "TutorialDemo"; //Default val, set in QuestBoardPopulation
    internal Launcher launcher;
    public bool ignoreStart = false;
    public bool createNewRoom = false;
    public bool startOffline = false;
    public float delayTime = 0f;
    internal string mode = "None";
    [Button]
    public void ManualStartGame() { StartGame(); }

    private void OnTriggerEnter(Collider other)
    {
        if (!ignoreStart && other.gameObject.CompareTag("QuestStart"))
        {
            StartGame();
            ignoreStart = true;
        }
    }

    private void StartGame()
    {
        launcher.gameMode = mode;
        launcher.CreateNewRoom(createNewRoom);
        launcher.UseOfflineMode(launcher.useOfflineMode | startOffline);
        if (delayTime > 0f)
        {
            StartCoroutine(DelayStart());
            return;
        }
        launcher.Connect(sceneToLoad);
        this.enabled = false;
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(delayTime);
        launcher.Connect();
        this.enabled = false;
    }
}
