using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ScoutTarget : MonoBehaviour
{
    private string targetTag = "ScoutTarget";
    public UnityEvent onScoutTargetEntered;

    private void Start()
    {
        var c = GetComponent<Collider>();
        c.enabled = true;
        c.isTrigger = true;
        gameObject.tag = targetTag;
        onScoutTargetEntered.AddListener(ScoutModeHelper);
    }

    public void ScoutModeHelper()
    {
        if (ScoutMode.Instance)
        {
            ScoutMode.Instance.hitTargets++;
        }
    }

    private IEnumerator DelayAddToScoutMode()
    {
        yield return new WaitForSeconds(1);
        if (ScoutMode.Instance)
        {
            ScoutMode.Instance.targets.Add(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
