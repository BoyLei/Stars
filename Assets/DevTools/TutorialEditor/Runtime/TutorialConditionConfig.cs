using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Sirenix.OdinInspector;
using StarProjectDef;

[System.Serializable]

[XLua.LuaCallCSharp]
public class TutorialConditionConfig
{
    [LabelText("条件类型")]
    [ValueDropdown("GetConditionTypes")]
    [JsonConverter(typeof(StringEnumConverter))]
    public E_TutorialConditionType ConditionType;

    [LabelText("界面名称")]
    [ShowIf("ShouldSerializeViewName")]
    public string ViewName;

    [LabelText("任务ID")]
    [ShowIf("ShouldSerializeTaskID")]
    public int TaskID;



    [LabelText("设置标记位索引")]
    [ShowIf("ShouldSerializeFlagIndex")]
    [SuffixLabel("$GetSetFlag")]
    public int FlagIndex;
    
    
    [LabelText("系统枚举ID")]
    [ShowIf("ShouldSerializeSystemID")]
    [ValueDropdown("GetSystemOpenTypes")]
    [JsonConverter(typeof(StringEnumConverter))]
    public SystemOpenType SystemID;

    [LabelText("场景ID")]
    [ShowIf("ShouldSerializeSceneID")]
    public int SceneID;

    public bool ShouldSerializeViewName()
    {
        return ConditionType == E_TutorialConditionType.OpenUI;
    }
    public bool ShouldSerializeTaskID()
    {
        return ConditionType == E_TutorialConditionType.FinishTask;
    }   
    public bool ShouldSerializeTutorialID()
    {
        return ConditionType == E_TutorialConditionType.FinishTutorial;
    }

    public bool ShouldSerializeSystemID()
    {
        return ConditionType == E_TutorialConditionType.SystemOpen;
    }

    public  bool ShouldSerializeFlagIndex()
    {
        return ConditionType == E_TutorialConditionType.FinishTutorial;
    }
    
    public bool ShouldSerializeSceneID()
    {
        return ConditionType == E_TutorialConditionType.EntryScene || ConditionType == E_TutorialConditionType.ExitScene;
    }
    public IEnumerable GetConditionTypes()
    {
        return TutorialDefine.e_tutorialconditiontypes;
    }

    public IEnumerable GetSystemOpenTypes()
    {
        return TutorialDefine.e_systemopentypes;
    }
    
    public string GetSetFlag()
    {
        if (FlagIndex > 0 && FlagIndex < TutorialFlagConfig.Instance.Flags.Count)
        {
            return  TutorialFlagConfig.Instance.Flags[FlagIndex].FlagName;
        }

        return "未知";
    }
    
}
