using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using NaughtyAttributes;

public class SaveGame : MonoBehaviour
{
    private static SaveInfo _saveInfo;
    public static SaveInfo GetSaveInfo() { return _saveInfo; }

    [Button]
    public void DisplayLastSave() { UnityEngine.Debug.LogFormat("Save: {0}, role: {1}", GetSaveInfo().ToString(), GetSaveInfo().resources.role); }

    internal AvailableResources lastGameData = new AvailableResources();

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
            lastGameData.Clear();
            lastGameData.CopyOverResources(r);
            lastGameData.role = r.role;
            _saveInfo.resources.CopyOverResources(r);
            _saveInfo.resources.CopyOverResources(_lastLoadedSave.resources);
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

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
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
    public int skulls = 0;

    internal string role = "";

    public void CopyOverResources(ResourceTracker rt)
    {
        sandCrystal += rt.sandCrystal;
        oceanCrystal += rt.oceanCrystal;
        wood += rt.wood;
        stone += rt.stone;
        food += rt.food;
        leather += rt.leather;
        skulls += rt.skulls;
    }

    public void CopyOverResources(AvailableResources rt)
    {
        sandCrystal += rt.sandCrystal;
        oceanCrystal += rt.oceanCrystal;
        wood += rt.wood;
        stone += rt.stone;
        food += rt.food;
        leather += rt.leather;
        skulls += rt.skulls;
    }

    public void Clear()
    {
        sandCrystal = 0;
        oceanCrystal = 0;
        wood = 0;
        stone = 0;
        food = 0;
        leather = 0;
        skulls = 0;
        role = "";
    }

    public int TotalResources()
    {
        int res = 0;
        res += sandCrystal;
        res += oceanCrystal;
        res += wood;
        res += stone;
        res += food;
        res += leather;
        res += skulls;
        return res;
    }
}
