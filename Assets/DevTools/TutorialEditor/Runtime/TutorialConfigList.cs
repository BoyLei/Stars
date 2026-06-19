using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[XLua.LuaCallCSharp]
public class TutorialConfigList
{
    public Dictionary<int, TutorialConfig> list = new();
}
