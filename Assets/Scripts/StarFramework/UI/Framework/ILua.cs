
using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
///--------------------------------------------------------------------
/// 文件名   :   ILua
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/18 10:12:49
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------


public class LuaPanel
{
    //Mono 模块代码调用
    private Dictionary<string, LuaUICallCSharp1> MonoBehaviourCalls = new Dictionary<string, LuaUICallCSharp1>();

    //UI 逻辑
    private Dictionary<string, LuaUICallCSharp2> UICalls = new Dictionary<string, LuaUICallCSharp2>();

    //lua脚本文件
    private LuaTable scriptTable;

    public LuaTable ScriptTable
    {

        get { return scriptTable; }
    }


    public bool Execute(string name)
    {
        if (MonoBehaviourCalls.TryGetValue(name, out var call) && call != null)
        {
            call.Invoke(scriptTable);
            return true;
        }
        return false;
    }

    public bool Execute(string name, object arg)
    {
        if (UICalls.TryGetValue(name, out var call) && call != null)
        {
            call.Invoke(scriptTable, arg);
            return true;
        }
        return false;
    }

    public void BindCall(LuaTable luaTable)
    {
        scriptTable = luaTable;
        MonoBehaviourCalls.Clear();
        UICalls.Clear();
        scriptTable.Set("self", luaTable);

        BindMonoCall("Awake");
        BindMonoCall("Start");
        BindMonoCall("OnEnable");
        BindMonoCall("OnDisable");
        BindMonoCall("OnDestroy");
        BindUICall("OnOpen");
        BindUICall("OnClose");
    }


    private void BindMonoCall(string name)
    {
        LuaUICallCSharp1 call = scriptTable.Get<LuaUICallCSharp1>(name);
        MonoBehaviourCalls.Add(name, call);
    }
    private void BindUICall(string name)
    {
        LuaUICallCSharp2 call = scriptTable.Get<LuaUICallCSharp2>(name);
        UICalls.Add(name, call);
    }

    public void OnRelease()
    {
        if (MonoBehaviourCalls != null)
        {
            MonoBehaviourCalls.Clear();
        }

        if (UICalls != null)
        {
            UICalls.Clear();
        }

        if (scriptTable != null)
        {
            scriptTable.Dispose();
            scriptTable = null;
        }
    }
}

[CSharpCallLua]
public delegate void LuaUICallCSharp2(object self, object arg);

[CSharpCallLua]
public delegate void LuaUICallCSharp1(object self);

[CSharpCallLua]
public delegate Vector2 LuaUICallCSharp3(object self);


