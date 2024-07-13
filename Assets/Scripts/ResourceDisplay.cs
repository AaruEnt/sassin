using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{
    public TMP_Text text;
    private void OnEnable()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var save = SaveGame.GetSaveInfo();
        if (save != null)
        {
            string sb = string.Format("Resources available:\nWood: {0}\nStone: {1}\nFood: {2}\nLeather: {3}\nSalvaged sandsteel chunks: {4}\nOcean Crystal: {5}", save.resources.wood, save.resources.stone, save.resources.food, save.resources.leather, save.resources.sandCrystal, save.resources.oceanCrystal);
            text.text = sb;
        }
    }
}
