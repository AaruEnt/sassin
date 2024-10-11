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
    internal string mode = "None";

    private void OnTriggerEnter(Collider other)
    {
        if (!ignoreStart && other.gameObject.CompareTag("QuestStart"))
        {
            launcher.gameMode = mode;
            launcher.CreateNewRoom(createNewRoom);
            launcher.UseOfflineMode(launcher.useOfflineMode | startOffline);
            launcher.Connect(sceneToLoad);
            this.enabled = false;
        }
    }
}
