using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class CosmeticUnlocker : MonoBehaviour
{
    public string manualUnlockName = "";
    [Button]
    public void UnlockCosmeticWithName() { if (string.IsNullOrEmpty(manualUnlockName)) return; PlayerPrefs.SetInt(manualUnlockName, 1); }

    // Start is called before the first frame update
    void Start()
    {
        var dt = DateTime.Now;
        int day;
        Int32.TryParse(dt.ToString("dd"), out day);
        switch (dt.ToString("MMMM"))
        {
            case "January":
                if (day < 10)
                {
                    if (!PlayerPrefs.HasKey("newYears"))
                    {
                        PlayerPrefs.SetInt("newYears", 1);
                    }
                    if (!PlayerPrefs.HasKey("newYears" + dt.ToString("yyyy")))
                    {
                        PlayerPrefs.SetInt("newYears" + dt.ToString("yyyy"), 1);
                    }
                } else if (day > 20)
                {
                    if (!PlayerPrefs.HasKey("lunarNewYear"))
                    {
                        PlayerPrefs.SetInt("lunarNewYear", 1);
                    }
                    if (!PlayerPrefs.HasKey("lunarNewYear" + dt.ToString("yyyy")))
                    {
                        PlayerPrefs.SetInt("lunarNewYear" + dt.ToString("yyyy"), 1);
                    }
                }
                break;
            case "February":
                if (!PlayerPrefs.HasKey("valentines"))
                {
                    PlayerPrefs.SetInt("valentines", 1);
                }
                if (!PlayerPrefs.HasKey("valentines" + dt.ToString("yyyy")))
                {
                    PlayerPrefs.SetInt("valentines" + dt.ToString("yyyy"), 1);
                }
                if (day < 10)
                {
                    if (!PlayerPrefs.HasKey("lunarNewYear"))
                    {
                        PlayerPrefs.SetInt("lunarNewYear", 1);
                    }
                    if (!PlayerPrefs.HasKey("lunarNewYear" + dt.ToString("yyyy")))
                    {
                        PlayerPrefs.SetInt("lunarNewYear" + dt.ToString("yyyy"), 1);
                    }
                }
                break;
            case "April":
                if (!PlayerPrefs.HasKey("easter"))
                {
                    PlayerPrefs.SetInt("easter", 1);
                }
                if (!PlayerPrefs.HasKey("easter" + dt.ToString("yyyy")))
                {
                    PlayerPrefs.SetInt("easter" + dt.ToString("yyyy"), 1);
                }
                break;
            case "October":
                if (!PlayerPrefs.HasKey("halloween"))
                {
                    PlayerPrefs.SetInt("halloween", 1);
                }
                if (!PlayerPrefs.HasKey("halloween" + dt.ToString("yyyy")))
                {
                    PlayerPrefs.SetInt("halloween" + dt.ToString("yyyy"), 1);
                }
                break;
            case "November":
                if (day <= 5 || (day <= 15 && string.Compare(dt.ToString("yyyy"), "2024") == 0))
                {
                    if (!PlayerPrefs.HasKey("halloween"))
                    {
                        PlayerPrefs.SetInt("halloween", 1);
                    }
                    if (!PlayerPrefs.HasKey("halloween" + dt.ToString("yyyy")))
                    {
                        PlayerPrefs.SetInt("halloween" + dt.ToString("yyyy"), 1);
                    }
                }
                if (day >= 20)
                {
                    if (!PlayerPrefs.HasKey("thanksgiving"))
                    {
                        PlayerPrefs.SetInt("thanksgiving", 1);
                    }
                    if (!PlayerPrefs.HasKey("thanksgiving" + dt.ToString("yyyy")))
                    {
                        PlayerPrefs.SetInt("thanksgiving" + dt.ToString("yyyy"), 1);
                    }
                }
                break;
            case "December":
                if (!PlayerPrefs.HasKey("christmas"))
                {
                    PlayerPrefs.SetInt("christmas", 1);
                }
                if (!PlayerPrefs.HasKey("christmas" + dt.ToString("yyyy")))
                {
                    PlayerPrefs.SetInt("christmas" + dt.ToString("yyyy"), 1);
                }
                break;
            default:
                break;
        }
        UnityEngine.Debug.Log("valentines" + dt.ToString("yyyy"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
