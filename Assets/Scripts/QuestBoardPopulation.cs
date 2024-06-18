using Com.Aaru.Sassin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class QuestBoardPopulation : MonoBehaviour
{
    public Launcher launcher;
    public List<PaperWeightSelection> papers;
    public List<QuestBoardInfo> quests;
    public List<Transform> targets;

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
                UnityEngine.Debug.Log(string.Format("Paper {0} chosen with weight {1}", paperToUse.name, convertedWeights[paperToUse]));
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
        qs.sceneToLoad = quest.sceneToLoad;
    }
}

[System.Serializable]
public class QuestBoardInfo
{
    [Scene]
    public string sceneToLoad;
    [TextArea(3, 20)]
    public string paperText;
    public string assignedBy;
    public bool showOptionalVars = false;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, allows for this quest to always use a specific paper prefab, i.e. quests from the tavern wench always having a dagger.")]
    public GameObject? requiredPaper = null;
    [AllowNesting, ShowIf("showOptionalVars"), Tooltip("Optional variable, allows for this quest to always use a specific paper prefab, i.e. quests from the tavern wench always having a dagger.")]
    public Transform? requiredSpawnLoc = null;
}

[System.Serializable]
public class PaperWeightSelection
{
    public GameObject paperPrefab;
    public int weight;
}
