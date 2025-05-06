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
            string sb = string.Format("Resources available:\n");// Ocean Crystal: {5}\nWood: {0}\nStone: {1}\nFood: {2}\nLeather: {3}\nSkulls: {6}", save.resources.wood, save.resources.stone, save.resources.food, save.resources.leather, save.resources.sandCrystal, save.resources.oceanCrystal, save.resources.skulls);
            if (save.resources.sandCrystal > 0)
                sb += (string.Format("Sandsteel: {0}\n", save.resources.sandCrystal));
            if (save.resources.oceanCrystal > 0)
                sb += (string.Format("Seaglass: {0}\n", save.resources.oceanCrystal));
            if (save.resources.food > 0)
                sb += (string.Format("Food: {0}\n", save.resources.food));
            if (save.resources.wood > 0)
                sb += (string.Format("Wood: {0}\n", save.resources.wood));
            if (save.resources.stone > 0)
                sb += (string.Format("Stone: {0}\n", save.resources.stone));
            if (save.resources.leather > 0)
                sb += (string.Format("Leather: {0}\n", save.resources.leather));
            if (save.resources.skulls > 0)
                sb += (string.Format("Skulls: {0}\n", save.resources.skulls));
            text.text = sb;
        }
    }
}
