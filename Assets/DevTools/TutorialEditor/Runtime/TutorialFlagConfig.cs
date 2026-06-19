using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Assets/Create TutorialFlagConfig")]
[System.Serializable]
public class TutorialFlagConfig : ScriptableObject
{
    private const string FilePath = "Assets/Res/Config/Guide/TutorialFlagConfig.asset";
    private static TutorialFlagConfig _instance;

    public static TutorialFlagConfig Instance
    {
        get
        {
#if UNITY_EDITOR
            _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialFlagConfig>(FilePath);
            if (_instance == null)
            {
                _instance = CreateInstance<TutorialFlagConfig>();
                UnityEditor.AssetDatabase.CreateAsset(_instance, FilePath);
                UnityEditor.AssetDatabase.SaveAssets();
            }

#endif
            return _instance;
        }
    }

    [LabelText("服务器存储标记")][TableList(ShowIndexLabels =true,CellPadding=2)]
    public List<TutorialFlagItem> Flags = new List<TutorialFlagItem>();

    public int GetIndex(string val)
    {
        int index = -1;
        for (int i = 0; i < Flags.Count; i++)
        {
            if (Flags[i].FlagName == val)
            {
                index = i;
                break;
            }
        }
        return index;
        //return Flags.IndexOf(val);
    }
    
}

[System.Serializable]
public class TutorialFlagItem
{
    public bool Delete;
    public string FlagName;
    public uint TaskID;
    public uint MapID;
}