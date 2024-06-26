using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class SaveGame : MonoBehaviour
{
    private SaveInfo _saveInfo;
    private string _saveData;
    private SaveInfo _lastLoadedSave = new SaveInfo();
    // Precursor save system, will eventually be ideally moved to a server side save
    // Start is called before the first frame update
    void Start()
    {
        LoadGame();
        StartCoroutine(DelayedGatherSaveInfo());
    }

    private IEnumerator DelayedGatherSaveInfo()
    {
        yield return new WaitForSeconds(0.1f);
        GetSaveData();
    }

    public void GetSaveData()
    {
        _saveInfo = CreateSaveInfo();
        UnityEngine.Debug.Log( _saveData );

        ResourceTracker? r = GameObject.Find("ResourceTracker")?.GetComponent<ResourceTracker>();
        if (!r)
        {
            _saveInfo = _lastLoadedSave;
        } else
        {
            _saveInfo.resources.sandCrystal = r.sandCrystal + _lastLoadedSave.resources.sandCrystal;
            _saveInfo.resources.oceanCrystal = r.oceanCrystal + _lastLoadedSave.resources.oceanCrystal;
            _saveInfo.resources.stone = r.stone + _lastLoadedSave.resources.stone;
            _saveInfo.resources.wood = r.wood + _lastLoadedSave.resources.wood;
            _saveInfo.resources.food = r.food + _lastLoadedSave.resources.food;
            _saveInfo.resources.leather = r.leather + _lastLoadedSave.resources.leather;
        }

        _saveData = JsonUtility.ToJson(_saveInfo);
        UnityEngine.Debug.Log("New save contents: " +  _saveData );
        if (r)
            Destroy(r.gameObject);
        DoSave();
    }

    public void DoSave()
    {
        string key = DateTime.Now.ToString();
        byte[] eKey = Encryption.sha256_hash(key);
        string e = Encryption.EncryptString(eKey, _saveData);

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
            SaveInfo tmp = JsonUtility.FromJson<SaveInfo>(dec);
            _lastLoadedSave = tmp;
            UnityEngine.Debug.Log("Decrypted File Converted: " + tmp);
        }
        catch (Exception e) {
            UnityEngine.Debug.Log("Error loading save: File may be corrupt or have been tampered with");
        }
    }

    public SaveInfo CreateSaveInfo()
    {
        SaveInfo info = new SaveInfo();

        return info;
    }
}

[System.Serializable]
public class SaveInfo
{
    public AvailableResources resources = new AvailableResources();
}

[System.Serializable]
public class AvailableResources
{
    public int sandCrystal = 0;
    public int oceanCrystal = 0;
    public int wood = 0;
    public int stone = 0;
    public int food = 0;
    public int leather = 0;
}
