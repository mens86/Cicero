
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class GameUtility
{

    public const string SavePrefKey = "Game_Highscore_Value";

    public const string FileName = "Q";
    public static string fileDir
    {
        get
        {
            return Application.dataPath + "/";
        }
    }
}

