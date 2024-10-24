using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

[RequireComponent(typeof(SaveGame))]
public class PostMissionReport : MonoBehaviour
{
    public GameObject paperPrefab;
    public Transform paperSpawnPoint;
    private SaveGame m_SaveGame;
    private AvailableResources m_LastMissionResources;
    // Start is called before the first frame update
    void Start()
    {
        m_SaveGame = GetComponent<SaveGame>();
        StartCoroutine(DelayedCreateReport());
    }

    private IEnumerator DelayedCreateReport()
    {
        yield return new WaitForSeconds(2);
        m_LastMissionResources = m_SaveGame.lastGameData;
        if (m_LastMissionResources != null && m_LastMissionResources.TotalResources() > 0)
            CreateReport();
    }

    private void CreateReport()
    {
        var paper = Instantiate(paperPrefab, null);
        paper.transform.position = paperSpawnPoint.position;
        paper.transform.rotation = paperSpawnPoint.rotation;
        PaperConstructor constr = paper.GetComponentInChildren<PaperConstructor>();
        constr.SetText(GenerateReport());
    }

    private string GenerateReport()
    {
        string sb = "";
        if (!m_LastMissionResources.role.IsNullOrEmpty())
        {
            sb += m_LastMissionResources.role;
            sb += "\n";
        }
        if (m_LastMissionResources.oceanCrystal > 0)
        {
            sb += "Oceancrystal:";
            for (int i = 0; i < m_LastMissionResources.oceanCrystal; i++)
            {
                if (m_LastMissionResources.oceanCrystal >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.oceanCrystal)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.sandCrystal > 0)
        {
            sb += "Sandsteel:";
            for (int i = 0; i < m_LastMissionResources.sandCrystal; i++)
            {
                if (m_LastMissionResources.sandCrystal >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.sandCrystal)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.food > 0)
        {
            sb += "Food:";
            for (int i = 0; i < m_LastMissionResources.food; i++)
            {
                if (m_LastMissionResources.food >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.food)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.wood > 0)
        {
            sb += "Wood:";
            for (int i = 0; i < m_LastMissionResources.wood; i++)
            {
                if (m_LastMissionResources.wood >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.wood)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.stone > 0)
        {
            sb += "Stone:";
            for (int i = 0; i < m_LastMissionResources.stone; i++)
            {
                if (m_LastMissionResources.stone >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.stone)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.leather > 0)
        {
            sb += "Leather:";
            for (int i = 0; i < m_LastMissionResources.leather; i++)
            {
                if (m_LastMissionResources.leather >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.leather)
                {
                    sb += "\n";
                }
            }
        }
        if (m_LastMissionResources.skulls > 0)
        {
            sb += "Skulls:";
            for (int i = 0; i < m_LastMissionResources.skulls; i++)
            {
                if (m_LastMissionResources.skulls >= 20)
                {
                    sb += " a lot\n";
                    break;
                }
                if (i % 5 == 0)
                    sb += " ";
                sb += "|";
                if (i + 1 == m_LastMissionResources.skulls)
                {
                    sb += "\n";
                }
            }
        }
        return sb;
    }
}
