///--------------------------------------------------------------------
/// 文件名   :   TaskScrollInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:31:07
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public class TaskScrollInfo
{
    public int ScrollType;
    public string TaskTypeName;
    public string TaskCharpterName;
    public string TaskName;
    public TaskInfo TaskInfo;
    public bool select;
    public bool IsMainLine;
    public int Count = 0;
    public TaskScrollInfo(int type, string taskTypeName, string taskCharpterName, string taskname, TaskInfo taskInfo, bool ismain)
    {
        ScrollType = type;
        TaskTypeName = taskTypeName;
        TaskCharpterName = taskCharpterName;
        TaskName = taskname;
        TaskInfo = taskInfo;
        IsMainLine = ismain;
    }

    public float GetHeight()
    {
        switch (ScrollType)
        {
            case 1:
                return 100;
            case 2:
                return 110;
            case 3:
                return 96;
        }
        return 100;
    }
}
