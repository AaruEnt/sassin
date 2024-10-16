using Com.Aaru.Sassin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using UnityEngine.Events;

public class QuestBoardPopulation : MonoBehaviour
{
    public Launcher launcher;
    public List<PaperWeightSelection> papers;
    public List<QuestBoardInfo> quests;
    public List<Transform> targets;
    public PaperBurnEvent OnPaperBurned;

    private Dictionary<GameObject, int> convertedWeights;

    // Start is called before the first frame update
    void Start()
    {
        if (launcher == null)
        {
            launcher = GameObject.Find("Launcher")?.GetComponent<Launcher>();
            if (!launcher)
                Debug.LogError("No launcher component attached or found");
        }
        convertedWeights = new Dictionary<GameObject, int>();
        foreach (var paper in papers)
        {
            convertedWeights.Add(paper.paperPrefab, paper.weight);
        }

        foreach (var quest in quests)
        {
            Transform t = quest.requiredSpawnLoc != null ? quest.requiredSpawnLoc : Randomizer.PickRandomObject(targets);
            GameObject paperToUse = quest.requiredPaper != null ? quest.requiredPaper : Randomizer.PickRandomObjectWeighted(convertedWeights);
            if (convertedWeights.ContainsKey(paperToUse))
            {
                //UnityEngine.Debug.Log(string.Format("Paper {0} chosen with weight {1}", paperToUse.name, convertedWeights[paperToUse]));
                convertedWeights[paperToUse] = convertedWeights[paperToUse] - 10 >= 0 ? convertedWeights[paperToUse] - 10 : 0;
            }
            StartCoroutine(CreateNewPaper(quest, paperToUse, t));
            targets.Remove(t);
            CheckResetConvertedWeights();
        }
    }

    private void CheckResetConvertedWeights()
    {
        List<GameObject> tmp = convertedWeights.Keys.ToList();
        int count = 0;
        foreach (var paper in tmp)
        {
            if (convertedWeights[paper] > 0)
            {
                count++;
                break;
            }
        }
        if (count == 0)
        {
            convertedWeights.Clear();
            foreach (var paper in papers)
            {
                convertedWeights.Add(paper.paperPrefab, paper.weight);
            }
        }
    }

    private IEnumerator CreateNewPaper(QuestBoardInfo quest, GameObject prefab, Transform loc)
    {
        GameObject g = Instantiate(prefab, loc.position, loc.rotation);
        yield return null;
        PaperConstructor pc = g.GetComponentInChildren<PaperConstructor>();
        pc.SetText(quest.paperText);

        QuestStarter qs = g.GetComponentInChildren<QuestStarter>();
        qs.launcher = launcher;
        if (quest.manualSceneSelection)
            qs.sceneToLoad = quest.sceneToLoad;
        else
            qs.sceneToLoad = GamemodeHelper.GetSceneFromMode(quest.gameMode);

        qs.startOffline = quest.offlineOnly;
        qs.createNewRoom = quest.newRoomOnly;
        string mode = "";
        if (quest.gameMode == "Gather" || quest.gameMode == "Invasion")
            mode = "Gather/Invasion";
        else
            mode = quest.gameMode;
        qs.mode = mode;

        Burnable br = g.GetComponentInChildren<Burnable>();
        br.BurnStarted += PaperBurnedHandler;
    }

    public void PaperBurnedHandler(Paper p)
    {
        OnPaperBurned.Invoke(p);
    }
}

[System.Serializable]
public class QuestBoardInfo
{
    public bool manualSceneSelection = false;
    [Scene, ShowIf("manualSceneSelection")]
    public string sceneToLoad;
    [AllowNesting]
    [Tooltip("GameModeOptions: \"Invasion\", \"Arena\", \"Gather\", \"Scout\"")]
    public string gameMode;
    [TextArea(3, 20)]
    public string paperText;
    public string assignedBy;
    public bool showOptionalVars = false;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, allows for this quest to always use a specific paper prefab, i.e. quests from the tavern wench always having a dagger.")]
    public GameObject? requiredPaper = null;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, allows for this quest to always use a specific spawn location, i.e. scouting quests always spawning in specific hard to reach locations")]
    public Transform? requiredSpawnLoc = null;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, allows for this quest to always be offline only")]
    public bool offlineOnly = false;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, this quest will always create a room that is open to other players, but will never join another room and will instead make your own room")]
    public bool newRoomOnly = false;

}

[System.Serializable]
public class PaperWeightSelection
{
    public GameObject paperPrefab;
    public int weight;
}

[System.Serializable]
public class PaperBurnEvent : UnityEvent<Paper>
{ }
