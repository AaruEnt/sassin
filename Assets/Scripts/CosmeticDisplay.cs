using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CosmeticDisplay : MonoBehaviourPun
{
    public List<Cosmetic> headSlot = new List<Cosmetic>();
    public List<Cosmetic> neckSlot = new List<Cosmetic>();
    public List<Cosmetic> maskSlot = new List<Cosmetic>();
    public List<Cosmetic> outfit = new List<Cosmetic>();

    public TMP_Text displayText;

    internal int headIndex = -1;
    internal int neckIndex = -1;
    internal int maskIndex = -1;
    internal int outfitIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            this.photonView.RPC("SetNetworkAppearance", RpcTarget.All, PlayerPrefs.GetString("OutfitItem"), PlayerPrefs.GetString("HeadSlotItem"), PlayerPrefs.GetString("NeckSlotItem"), PlayerPrefs.GetString("MaskSlotItem"));
        }
        else
        {
            if (PlayerPrefs.HasKey("OutfitItem") && !string.IsNullOrEmpty(PlayerPrefs.GetString("OutfitItem")))
            {
                var outfitItem = FindCosmeticWithName(PlayerPrefs.GetString("OutfitItem"), outfit);
                outfitIndex = outfit.IndexOf(outfitItem);
                outfitItem.displayItem.SetActive(true);
            }
            else
            {
                PlayerPrefs.SetString("OutfitItem", "");
                if (PlayerPrefs.HasKey("HeadSlotItem") && !string.IsNullOrEmpty(PlayerPrefs.GetString("HeadSlotItem")))
                {
                    var startHead = FindCosmeticWithName(PlayerPrefs.GetString("HeadSlotItem"), headSlot);
                    headIndex = headSlot.IndexOf(startHead);
                    startHead.displayItem.SetActive(true);
                }
                else
                    PlayerPrefs.SetString("HeadSlotItem", "");
                if (PlayerPrefs.HasKey("NeckSlotItem") && !string.IsNullOrEmpty(PlayerPrefs.GetString("NeckSlotItem")))
                {
                    var startNeck = FindCosmeticWithName(PlayerPrefs.GetString("NeckSlotItem"), neckSlot);
                    neckIndex = neckSlot.IndexOf(startNeck);
                    startNeck.displayItem.SetActive(true);
                }
                else
                    PlayerPrefs.SetString("NeckSlotItem", "");
                if (PlayerPrefs.HasKey("MaskSlotItem") && !string.IsNullOrEmpty(PlayerPrefs.GetString("MaskSlotItem")))
                {
                    var startMask = FindCosmeticWithName(PlayerPrefs.GetString("MaskSlotItem"), maskSlot);
                    maskIndex = maskSlot.IndexOf(startMask);
                    startMask.displayItem.SetActive(true);
                }
                else
                    PlayerPrefs.SetString("MaskSlotItem", "");
            }
        }
    }

    [PunRPC]
    public void SetNetworkAppearance(string o, string h, string n, string m)
    {
        if (!string.IsNullOrEmpty(o))
        {
            var outfitItem = FindCosmeticWithName(o, outfit);
            outfitIndex = outfit.IndexOf(outfitItem);
            outfitItem.displayItem.SetActive(true);
        }
        else
        {
            if (!string.IsNullOrEmpty(h))
            {
                var startHead = FindCosmeticWithName(h, headSlot);
                headIndex = headSlot.IndexOf(startHead);
                startHead.displayItem.SetActive(true);
            }
            if (!string.IsNullOrEmpty(n))
            {
                var startNeck = FindCosmeticWithName(n, neckSlot);
                neckIndex = neckSlot.IndexOf(startNeck);
                startNeck.displayItem.SetActive(true);
            }
            if (!string.IsNullOrEmpty(m))
            {
                var startMask = FindCosmeticWithName(m, maskSlot);
                maskIndex = maskSlot.IndexOf(startMask);
                startMask.displayItem.SetActive(true);
            }
        }
    }

    // 0 - head, 1 - neck, 2 - mask, 3 - outfit
    public void IncrementDisplay(int list)
    {
        bool tmpBool = false;
        displayText.text = "";
        if (list != 3 && outfitIndex >= 0)
        {
            outfit[outfitIndex].displayItem.SetActive(false);
            PlayerPrefs.SetString("OutfitItem", "");
            outfitIndex = -1;
        }
        switch (list)
        {
            case 0:
                if (headIndex < 0)
                    headIndex++;
                else
                {
                    headSlot[headIndex].displayItem.SetActive(false);
                    headIndex++;
                }
                while (!tmpBool)
                {
                    if (headIndex < headSlot.Count)
                    {
                        if (string.IsNullOrEmpty(headSlot[headIndex].unlockConditions) || CheckUnlocked(headSlot[headIndex]))
                            tmpBool = true;
                        else
                            headIndex++;
                    }
                    else
                        tmpBool = true;
                }
                if (headIndex >= headSlot.Count)
                    headIndex = -1;
                else
                {
                    headSlot[headIndex].displayItem.SetActive(true);
                    displayText.text = headSlot[headIndex].name;
                }
                PlayerPrefs.SetString("HeadSlotItem", headIndex >= 0 ? headSlot[headIndex].name : "");
                break;
            case 1:
                if (neckIndex < 0)
                    neckIndex++;
                else
                {
                    neckSlot[neckIndex].displayItem.SetActive(false);
                    neckIndex++;
                }
                while (!tmpBool)
                {
                    if (neckIndex < neckSlot.Count)
                    {
                        if (string.IsNullOrEmpty(neckSlot[neckIndex].unlockConditions) || CheckUnlocked(neckSlot[neckIndex]))
                            tmpBool = true;
                        else
                            neckIndex++;
                    }
                    else
                        tmpBool = true;
                }
                if (neckIndex >= neckSlot.Count)
                    neckIndex = -1;
                else
                {
                    neckSlot[neckIndex].displayItem.SetActive(true);
                    displayText.text = neckSlot[neckIndex].name;
                }
                PlayerPrefs.SetString("NeckSlotItem", neckIndex >= 0 ? neckSlot[neckIndex].name : "");
                break;
            case 2:
                if (maskIndex < 0)
                    maskIndex++;
                else
                {
                    maskSlot[maskIndex].displayItem.SetActive(false);
                    maskIndex++;
                }
                while (!tmpBool)
                {
                    if (maskIndex < maskSlot.Count)
                    {
                        if (string.IsNullOrEmpty(maskSlot[maskIndex].unlockConditions) || CheckUnlocked(maskSlot[maskIndex]))
                            tmpBool = true;
                        else
                            maskIndex++;
                    }
                    else
                        tmpBool = true;
                }
                if (maskIndex >= maskSlot.Count)
                    maskIndex = -1;
                else
                {
                    maskSlot[maskIndex].displayItem.SetActive(true);
                    displayText.text = maskSlot[maskIndex].name;
                }
                PlayerPrefs.SetString("MaskSlotItem", maskIndex >= 0 ? maskSlot[maskIndex].name : "");
                break;
            case 3:
                if (maskIndex >= 0)
                    maskSlot[maskIndex].displayItem.SetActive(false);
                if (neckIndex >= 0)
                    neckSlot[neckIndex].displayItem.SetActive(false);
                if (headIndex >= 0)
                    headSlot[headIndex].displayItem.SetActive(false);
                maskIndex = -1;
                headIndex = -1;
                neckIndex = -1;
                PlayerPrefs.SetString("HeadSlotItem", "");
                PlayerPrefs.SetString("NeckSlotItem", "");
                PlayerPrefs.SetString("MaskSlotItem", "");

                if (outfitIndex < 0)
                    outfitIndex++;
                else
                {
                    outfit[outfitIndex].displayItem.SetActive(false);
                    outfitIndex++;
                }
                while (!tmpBool)
                {
                    if (outfitIndex < outfit.Count)
                    {
                        if (string.IsNullOrEmpty(outfit[outfitIndex].unlockConditions) || CheckUnlocked(outfit[outfitIndex]))
                            tmpBool = true;
                        else
                            outfitIndex++;
                    }
                    else
                        tmpBool = true;
                }
                if (outfitIndex >= outfit.Count)
                    outfitIndex = -1;
                else
                {
                    outfit[outfitIndex].displayItem.SetActive(true);
                    displayText.text = outfit[outfitIndex].name;
                }
                PlayerPrefs.SetString("OutfitItem", outfitIndex >= 0 ? outfit[outfitIndex].name : "");
                break;
        }
    }

    internal Cosmetic FindCosmeticWithName(string name, List<Cosmetic> list)
    {
        UnityEngine.Debug.LogFormat("FindCall with name: {0}", name);
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var c in list)
        {
            if (c.name == name)
            { return c; }
        }
        return null;
    }

    internal bool CheckUnlocked(Cosmetic cosmetic)
    {
        if (string.IsNullOrEmpty(cosmetic.unlockConditions))
            return true;
        string[] reqs = cosmetic.unlockConditions.Split(',');
        foreach (string req in reqs)
        {
            if (!PlayerPrefs.HasKey(req) || PlayerPrefs.GetInt(req) == 0)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class Cosmetic
{
    public GameObject displayItem;
    [AllowNesting, Tooltip("Display name")]
    public string name;
    [AllowNesting, Tooltip("Leave blank for no requirements. Multiple requirements can be separated by comma")]
    public string unlockConditions = "";
}
