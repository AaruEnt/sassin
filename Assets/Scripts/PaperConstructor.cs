using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaperConstructor : MonoBehaviour
{
    public Text paperText;

    public void SetText(string text)
    {
        if (text == null || text == string.Empty)
            text = "You're A Cunt";
        paperText.text = text;
    }
}
