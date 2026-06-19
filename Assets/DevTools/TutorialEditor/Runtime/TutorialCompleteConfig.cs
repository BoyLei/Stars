using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[System.Serializable]
[XLua.LuaCallCSharp]
public class TutorialCompleteConfig
{
    [LabelText("完成条件类型")]
    [ValueDropdown("GetCompleteTypes")]
    public E_TutorialCompleteTypeType CompleteTypeType;
    
    [LabelText("事件类型")]
    [ShowIf("ShouldSerializeEvent")]
    [ValueDropdown("GetEventDefines")]
    public E_EventDefine Event;

    
    [LabelText("参数列表")]
    public List<string> Prams=new List<string>();




    public bool ShouldSerializeEvent()
    {
        return CompleteTypeType == E_TutorialCompleteTypeType.ReciveEvent;
    }
    
    public IEnumerable GetCompleteTypes()
    {
        return TutorialDefine.e_tutorialcompletetypetypes;
    }

    public IEnumerable GetEventDefines()
    {
        return TutorialDefine.e_eventdefines;
    }
}
