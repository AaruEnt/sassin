using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GamemodeHelper : MonoBehaviour
{
    public static List<Gamemode> Gamemodes = new List<Gamemode>();
    public List<Gamemode> GamemodesList = new List<Gamemode>();

    private void Start()
    {
        Gamemodes = GamemodesList;
    }

    public static string GetSceneFromMode(string mode)
    {
        foreach (Gamemode gamemode in Gamemodes)
        {
            if (gamemode.mode == mode)
                return Randomizer.PickRandomObject(gamemode.sceneOptions);
        }
        return string.Empty;
    }
}

[System.Serializable]
public class Gamemode
{
    public string mode;
    [Scene]
    public List<string> sceneOptions;
}
