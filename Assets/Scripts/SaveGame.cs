using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using static UnityEngine.Rendering.DebugUI;

public class SaveGame : MonoBehaviour
{
    // Precursor save system, will eventually be ideally moved to a server side save
    // Start is called before the first frame update
    void Start()
    {
        DoSave();
        LoadGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSave()
    {
        string key = DateTime.Now.ToString();
        byte[] eKey = Encryption.sha256_hash(key);
        string e = Encryption.EncryptString(eKey, "EncryptThisStringPlease");

        string filename = Application.persistentDataPath + "/saveGame.sav";
        UnityEngine.Debug.Log($"{filename}");
        using (FileStream fs = File.Create(filename)) {
            byte[] info = new UTF8Encoding(true).GetBytes(e);
            fs.Write(info, 0, info.Length);
            fs.Close();
        }
            UnityEngine.Debug.Log(string.Format("Last edit time: {0}, Now: {1}", File.GetLastWriteTime(filename).ToString(), DateTime.Now.ToString()));
    }

    public void LoadGame()
    {
        string filename = Application.persistentDataPath + "/saveGame.sav";
        if (!File.Exists(filename))
            return;
        string key = File.GetLastWriteTime(filename).ToString();
        byte[] eKey = Encryption.sha256_hash(key);

        byte[] buff = File.ReadAllBytes(filename);
        string decBuff = new UTF8Encoding(true).GetString(buff);
        try
        {
            string dec = Encryption.DecryptString(eKey, decBuff);
            UnityEngine.Debug.Log("Decrypted file: " + dec);
        }
        catch (Exception e) {
            UnityEngine.Debug.Log("Error loading save: File may have been tampered with");
        }
    }
}
