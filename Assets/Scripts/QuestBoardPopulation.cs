using Com.Aaru.Sassin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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
            Transform t = Randomizer.PickRandomObject(targets);
            GameObject paperToUse = quest.requiredPaper != null ? quest.requiredPaper : Randomizer.PickRandomObjectWeighted(convertedWeights);
            StartCoroutine(CreateNewPaper(quest, paperToUse, t));
            targets.Remove(t);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
    [Tooltip("Optional variable, allows for this quest to always use a specific paper prefab, i.e. quests from the tavern wench always having a dagger.")]
    public GameObject? requiredPaper = null;
    public string assignedBy;
}

[System.Serializable]
public class PaperWeightSelection
{
    public GameObject paperPrefab;
    public int weight;
}
