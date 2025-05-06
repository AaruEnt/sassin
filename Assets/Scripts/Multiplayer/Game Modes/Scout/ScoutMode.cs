using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoutMode : MonoBehaviour
{
    public static ScoutMode Instance;
    public List<ScoutTarget> targets = new List<ScoutTarget>();
    public UnityEvent OnWin; // placeholder for winning until this is actually implemented
    internal int hitTargets = 0;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (targets.Count > 0 && hitTargets >= targets.Count)
        {
            OnWin.Invoke();
        }
    }
}
