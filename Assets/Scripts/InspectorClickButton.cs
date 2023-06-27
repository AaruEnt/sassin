using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class InspectorClickButton : MonoBehaviour
{
    public Button btn;
    [Button]
    private void CallOnClick() { btn.onClick.Invoke(); }

    void Start()
    {
        if (btn == null)
            btn = GetComponent<Button>();
    }
}
