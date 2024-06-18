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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("QuestStart"))
        {
            launcher.Connect(sceneToLoad);
            this.enabled = false;
        }
    }
}
