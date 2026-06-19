using System.Collections.Generic;
using UnityEngine;

///--------------------------------------------------------------------
/// 文件名   :   TaskTarget.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:32:45
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
[XLua.LuaCallCSharp]
public class TaskTarget
{
    public int TaskEventID;
    public string TaskDesc;
    public long CurProgress;
    public int MaxCount;
    public List<int> MapServer;
    public Task.TaskFindPath FindPath;
    public float Height = 30;
    public int MapID;
    public long TargetID;
    public Task.TaskType taskType;
    public List<string> Pramas;
    public bool ShowProcess;
    
    public int GetID()
    {
        int id = 0;
        if (Pramas != null && Pramas.Count > 0)
        {
            System.Int32.TryParse(Pramas[0], out id);
        }

        return id;
    }

    public Color GetStateColor()
    {
        Color color = Color.white;
        if (IsFinish())
        {
            ColorUtility.TryParseHtmlString("#A88946", out color);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#C0B6A0", out color);
        }

        return color;
    }

    public bool IsFinish()
    {
        return CurProgress >= MaxCount;
    }
}