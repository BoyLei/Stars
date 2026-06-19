using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[System.Serializable]
[XLua.LuaCallCSharp]
public class TutorialTargetConfig
{

    [LabelText("引导对象类型")]
    [ValueDropdown("GetutorialTargetTypes")]
    public E_TutorialTargetType TargetType;

    //手势动作
    //[LabelText("手势动作")]
    //[ValueDropdown("GetFingerTypes")]
    //public E_FingerType FingerType;

    
    [LabelText("引导参数")]
    public string Params;

    //[LabelText("手指偏移")]
   // public Vector3Int FingerOffset;
    
    public IEnumerable GetutorialTargetTypes()
    {
        return TutorialDefine.e_tutorialtargettypes;
    }
    
    public IEnumerable GetFingerTypes()
    {
        return TutorialDefine.e_fingertypes;
    }
}
