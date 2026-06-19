///--------------------------------------------------------------------
/// 文件名   :   TaskHelper.cs
/// 内  容   :   任务辅助类，不做任何的逻辑处理
/// 说  明   :  
/// 创建日期 :   2022/10/31 09:44:18
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF;
using StarProject.Module;
using Task;
using Debug = UnityEngine.Debug;
using TaskType = Task.TaskType;
using Vector3 = UnityEngine.Vector3;
using StarProject.Service.Language;
using StarProject.Service.FindPath;

[XLua.LuaCallCSharp]
public static class TaskHelper
{
    public static bool UseOldPartner = true;

    // 通缉任务特殊ID
    public const int WANTEDTASKID = 9999321;

    //接受任务
    public const string ACCEPTTASKREQ = "AcceptTaskReq";

    //放弃任务
    public const string DROPTASKREQ = "DropTaskReq";

    //提交任务
    public const string SUBMITTASKREQ = "SubmitTaskReq";

    //任务领取奖励
    public const string GETTASKREWARDREQ = "GetTaskRewardReq";

    //播放黑幕
    public const string PLAYBLACKMOVIE = "PlayBlackMovie";

    //播放章节
    public const string PLAYPLOT = "PlayPlot";

    /// <summary>
    /// 播放剧情图片
    /// </summary>
    public const string PLAYIMAGE = "PlayImage";

    // 刷新任务
    public const string FRESHTASK = "FreshTask";

    public const string UPDATETASKDATA = "UpdateTaskData";

    //NPC 通用符
    public const string WILDCARD_NPC = "[NPC]";

    //怪物 通用符
    public const string WILDCARD_MON = "[MON]";

    //物件 通用符
    public const string WILDCARD_OBJ = "[OBJ]";

    private static string GetMonsterName(long monsterId)
    {
        var data = LocalDataManager.Instance.GetMonsterDataCell(monsterId);
        if (data != null)
        {
            return data.Name;
        }
        else
        {
            Debug.Log($"读取怪物失败 id{monsterId}");
        }

        return string.Empty;
    }

    private static string GetNpcName(uint npcid)
    {
        var data = LocalDataManager.Instance.GetNPCDataCell(npcid);
        if (data != null)
        {
            return data.Name;
        }
        else
        {
            Debug.Log($"读取NPC 失败 id{npcid}");
        }

        return string.Empty;
    }

    private static string GetInterName(long interid)
    {
        var data = LocalDataManager.Instance.GetInteractDataCell(interid);
        if (data != null)
        {
            return data.ModeltalkeName;
        }
        else
        {
            Debug.Log($"读取怪物交互物 id{interid}");
        }

        return string.Empty;
    }

    public static string WildCardStr(string desc, long id)
    {
        string result = desc;
        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result) && result != " ")
        {
            string rep = string.Empty;
            if (desc.Contains(WILDCARD_NPC))
            {
                rep = GetNpcName((uint)id);
                if (!string.IsNullOrEmpty(rep))
                {
                    result = desc.Replace(WILDCARD_NPC, rep);
                }
            }
            else if (desc.Contains(WILDCARD_MON))
            {
                rep = GetMonsterName(id);
                if (!string.IsNullOrEmpty(rep))
                {
                    result = desc.Replace(WILDCARD_MON, rep);
                }
            }
            else if (desc.Contains(WILDCARD_OBJ))
            {
                rep = GetInterName(id);
                if (!string.IsNullOrEmpty(rep))
                {
                    result = desc.Replace(WILDCARD_OBJ, rep);
                }
            }

            result = result.Replace("[-n]", "\n");
        }

        return result;
    }


    /// <summary>
    /// 查询NPC任务状态
    /// </summary>
    /// <param name="npcID"></param>
    /// <returns></returns>
    public static bool QueryNpcTaskState(int npcID)
    {
        return taskManager.QueryNpcTaskState(npcID);
    }

    /// <summary>
    /// 获取NPC的任务状态
    /// 已完成 > 任务进行中 任务目标已完成 > 任务进行中 任务目标未完成
    /// </summary>
    /// <param name="npcID"></param>
    /// <returns></returns>
    public static TaskNpc.StateEnum QueryNpcTaskEnum(int npcID, ref uint taskID)
    {
        return taskManager.QueryNpcTaskEnum(npcID, ref taskID);
    }

    public static TaskNpc.StateEnum QueryInterTaskEnum(int configID, ref uint taskID)
    {
        return taskManager.QueryInterTaskEnum(configID, ref taskID);
    }

    public static Dictionary<long, List<TaskNpc>> GetCanReceiveNpcTask()
    {
        return taskManager.GetCanReceiveNpcTask();
    }

    public static Dictionary<long, List<TaskNpc>> GetCanReceiveInterTask()
    {
        return taskManager.GetCanReceiveInterTask();
    }

    public static List<TaskNpc> GetTaskNpcs(int npcID)
    {
        return taskManager.GetTaskNpcs(npcID);
    }

    public static List<TaskNpc> GetInterTasks(int interID)
    {
        return taskManager.GetInterTasks(interID);
    }

    /// <summary>
    /// 客户端完成任务
    /// </summary>
    /// <param name="taskID">任务ID</param>
    /// <param name="targets">任务目标集合</param>
    public static void ClientFinishTask(int taskID, int eventid)
    {
        TaskPropgressChange taskPropgress = new();
        taskPropgress.ID = taskID;
        taskPropgress.Evts = eventid;
        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, taskPropgress, isAutoChangeMsgTarget: false);
    }

    public static void DoFinishTaskEvent(int eventid)
    {
        StarProject.Service.Function.GlobalFunctionManager.Instance.DoFunction(eventid);
    }

    /// <summary>
    /// 播放黑幕
    /// </summary>
    /// <param name="id"></param>
    public static void PlayBlackMovie(int id, int EffectID = 0)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, PLAYBLACKMOVIE, id, EffectID);
    }

    /// <summary>
    /// 当前选中的任务ID
    /// </summary>
    public static int CurrentSelectTaskID
    {
        get { return taskManager.CurrentSelectTaskID; }
    }

    /// <summary>
    /// 播放章节
    /// </summary>
    /// <param name="id"></param>
    public static void PlayPlot(int id, int EffectID = 0,bool Enqueue=true)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, PLAYPLOT, id, EffectID,Enqueue);
    }

    public static void PlayImage(int id, int EffectID = 0)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, PLAYIMAGE, id, EffectID);
    }

    /// <summary>
    /// 接受任务
    /// </summary>
    /// <param name="taskid"></param>
    public static void AcceptTaskReq(int taskid)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, ACCEPTTASKREQ, taskid);
    }


    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <param name="taskid"></param>
    public static void DropTaskReq(int taskid)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, DROPTASKREQ, taskid);
    }

    /// <summary>
    /// 提交任务
    /// </summary>
    /// <param name="taskid"></param>
    public static void SubmitTaskReq(int taskid)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, SUBMITTASKREQ, taskid);
    }

    /// <summary>
    /// 任务领取奖励
    /// </summary>
    /// <param name="taskid"></param>
    public static void GetTaskRewardReq(int taskid)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, GETTASKREWARDREQ, taskid);
    }


    public static int GetTask(BaseBinary baseBinary, int index)
    {
        int len = baseBinary.ArrayLen; //数据长度
        int dlen = baseBinary.OneDataBitNum;

        for (int i = 0; i < len; i += dlen)
        {
            int id = BitConverter.ToUInt16(baseBinary.Data.ToByteArray(), i);

            Debug.LogError("GetTask" + id);
        }


        return 0;
    }

    public static void RemovekRunningTask(uint taskID)
    {
        taskManager.RemovekRunningTask(taskID);
    }


    /// <summary>
    /// 巡逻到动态目标
    /// </summary>
    /// <param name="EnityID"></param>
    /// <param name="range"></param>
    /// <param name="action"></param>
    public static bool FindToDynamicTarget(string key, ulong enityid, float range = 1,
        System.Action<bool, string> action = null, bool tips = true)
    {
        var MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
        if (MainPlayer != null)
        {
            return MainPlayer.FollowDynamicEnity(key, enityid, range, action, tips);
        }

        return false;
    }

    /// <summary>
    /// 副本寻路
    /// </summary>
    /// <param name="spawnerid"></param>
    public static void FindPathByEctype(int spawnerid, float range = 1, System.Action<bool> action = null)
    {
        // 副本寻路, 目前 都是任务 主动驱动的 寻路, 暂时先跟 自动战斗的 寻路 互斥
        GameManager.Instance.TriggerEvent("FindPathByEctype", spawnerid);
        //本场景寻路
        FindPathBySpawner(GameMap.sceneJsonData.SceneID, spawnerid, range, action);
    }

    public static void FindPathToTask(uint taskid, int EventID = 1, System.Action<bool> action = null)
    {
        var taskCfg = GetTaskConfig(taskid);
        if (taskCfg == null)
        {
            Debuger.LogError($"{taskid} 读取任务配置失败 ");
            return;
        }


        FindPathByNpc(taskCfg.Condition.PickTaskMapID, taskCfg.Condition.PickTaskNPC, 1, action);

        /*
        var target = taskCfg.GetTaskTarget(EventID);
        if (target == null)
        {
            Debuger.LogError($"{taskid} 读取任务目标{EventID} 失败 ");
            return;
        }
        FindPathByTask(target.MapID, target.FindPath, action);
        */

        /*FindPathByTask(target.MapID, target.FindPath, (result) =>
        {
            if (result)
            {
                AcceptTaskReq((int)taskid);
            }
        });*/
    }

    public static void FindPathByTask(int mapID, TaskFindPath task, long targetID, System.Action<bool> action = null)
    {
        //本场景寻路
        switch (task.FindPathType)
        {
            case E_FindPath.Spawer:
                {
                    FindPathBySpawner(mapID, (int)task.ID, task.Range * 0.01f, (result) =>
                    {
                        SendReqSpeedBuff(false);
                        action?.Invoke(result);
                    }, true, (targetPos) =>
                    {
                        TryReqSpeedBuff(targetPos);
                    });
                }
                break;
            case E_FindPath.NPC:
                {
                    FindPathByNpc(mapID, task.ID, task.Range * 0.01f, (result) =>
                    {
                        SendReqSpeedBuff(false);
                        action?.Invoke(result);
                    }, true, targetID, (targetPos) =>
                    {
                        TryReqSpeedBuff(targetPos);
                    });
                }
                break;
            case E_FindPath.InterAction:
                {
                    FindPathByInterAction(mapID, task.ID, task.Range * 0.01f, (result) =>
                    {
                        SendReqSpeedBuff(false);
                        action?.Invoke(result);
                    }, true, targetID, (targetPos) =>
                    {
                        TryReqSpeedBuff(targetPos);
                    });
                }
                break;
            case E_FindPath.Area:
                {
                    FindPathByArea(mapID, (int)task.ID, task.Range * 0.01f, (result) =>
                    {
                        SendReqSpeedBuff(false);
                        action?.Invoke(result);
                    }, true, (targetPos) =>
                    {
                        TryReqSpeedBuff(targetPos);
                    });
                }
                break;
            case E_FindPath.None:
                Debug.LogError("不支持寻路");
                break;
        }
    }

    public static void FindPathByArea(int mapID, int areaID, float range = 1, System.Action<bool> action = null,
        bool recordOnStateForbid = true, Action<Vector3> targetPosAction = null)
    {
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID)
        {
            if (GameMap.sceneJsonData == null || GameMap.sceneJsonData.Areas == null)
            {
                targetPosAction?.Invoke(Vector3.zero);
                return;
            }

            if (GameMap.sceneJsonData.Areas.TryGetValue(areaID, out var data) && data != null)
            {
                targetPosAction?.Invoke(data.Position.Convert());
                FindPathByPosition(data.Position.Convert(), range, action, false, recordOnStateForbid);
            }
            else
            {
                targetPosAction?.Invoke(Vector3.zero);
            }
        }
        else
        {
            targetPosAction?.Invoke(Vector3.zero);
            taskManager.SetCacheFindPath(0, mapID, areaID, Vector3.zero, E_FindPath.Area, range, action);
        }
    }

    public static void FindPathByInterAction(int mapID, long interID, float range = 1,
        System.Action<bool> action = null, bool recordOnStateForbid = true, long targetID = 0,Action<Vector3> targetPosAction = null)
    {
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID)
        {
            if (GameMap.sceneJsonData == null)
            {
                targetPosAction?.Invoke(Vector3.zero);
                return;
            }

            var data = GameMap.sceneJsonData.GetMineJsonData(targetID);
            InteractDataCell npcDataCell = LocalDataManager.Instance.GetInteractDataCell(targetID);
            if (data != null&& npcDataCell != null)
            {
                targetPosAction?.Invoke(data.Position.Convert());
                FindPathByPosition(data.Position.Convert(), npcDataCell.TriggerRange*0.01f, action, false, recordOnStateForbid);
            }
            else
            {
                targetPosAction?.Invoke(Vector3.zero);
            }
        }
        else
        {
            targetPosAction?.Invoke(Vector3.zero);
            taskManager.SetCacheFindPath(0, mapID, interID, Vector3.zero, E_FindPath.InterAction, range, action);
        }
    }

    public static void FindPathByNpc(int mapID, long npcID, float range = 1, System.Action<bool> action = null,
        bool recordOnStateForbid = true, long targetID = 0, Action<Vector3> targetPosAction = null)
    {
        if (npcID == 0)
        {
            npcID = targetID;
        }
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID)
        {
            if (GameMap.sceneJsonData == null)
            {
                return;
            }

            var data = GameMap.sceneJsonData.GetNPCJsonData(npcID);
            NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(npcID);

            if (data != null && npcDataCell != null)
            {
                targetPosAction?.Invoke(data.Position.Convert());
                FindPathByPosition(data.Position.Convert(), npcDataCell.TriggerRange * 0.01f, action, false, recordOnStateForbid);
            }
            else
            {
                targetPosAction?.Invoke(Vector3.zero);
            }
        }
        else
        {
            targetPosAction?.Invoke(Vector3.zero);
            taskManager.SetCacheFindPath(0, mapID, npcID, Vector3.zero, E_FindPath.NPC, range, action);
        }
    }

    public static void FindPathBySpawner(int mapID, int spawnerid, float range = 1, System.Action<bool> action = null,
        bool recordOnStateForbid = true, Action<Vector3> targetPosAction = null)
    {
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID)
        {
            if (GameMap.sceneJsonData == null || GameMap.sceneJsonData.Spawners == null)
            {
                targetPosAction?.Invoke(Vector3.zero);
                return;
            }

            if (GameMap.sceneJsonData.Spawners.TryGetValue(spawnerid, out var data) && data != null)
            {
                targetPosAction?.Invoke(data.Position.Convert());
                FindPathByPosition(data.Position.Convert(), range, action, false, recordOnStateForbid);
            }
            else
            {
                targetPosAction?.Invoke(Vector3.zero);
            }
        }
        else
        {
            targetPosAction?.Invoke(Vector3.zero);
            taskManager.SetCacheFindPath(0, mapID, spawnerid, Vector3.zero, E_FindPath.Spawer, range, action);
        }
    }

    public static void FindPostion(int mapID, Vector3 postion, float range = 1, System.Action<bool> action = null,
        bool isChanageFindPath = false, bool recordOnStateForbid = true)
    {
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID)
        {
            FindPathByPosition(postion, range, action, false, recordOnStateForbid);
        }
        else
        {
            E_FindPath e_FindPath = isChanageFindPath ? E_FindPath.ChanageMapPosition : E_FindPath.Position;
            taskManager.SetCacheFindPath(0, mapID, 0, postion, e_FindPath, range, action);
        }
    }

    // 寻路到通缉目标点
    public static void FindPathByWanted(ulong srvID, ulong spaceID, int pointID, float range = 1,
        System.Action<bool> action = null, bool recordOnStateForbid = true)
    {
        if (GameMap.sceneJsonData == null)
        {
            return;
        }

        int mapID = (int)spaceID;
        ulong CurServerID = GameManager.Instance.GetCurServerID();
        //本场景寻路
        if (GameMap.sceneJsonData.SceneID == mapID && CurServerID == srvID)
        {
            var data = GameMap.sceneJsonData.GetWantedTaskData(pointID);
            if (data != null)
            {
                FindPathByPosition(data.Position.Convert(), range, action, false, recordOnStateForbid);
            }
        }
        else
        {
            taskManager.SetCacheFindPath(0, mapID, pointID, Vector3.zero, E_FindPath.Wanted, range, action);
        }
    }

    public static void DoCacheFindPath()
    {
        taskManager.DoCacheFindPath();
    }

    public static bool FindPathByPosition(Vector3 postion, float range = 1, System.Action<bool> action = null,
        bool isChanageFindPath = false, bool recordOnStateForbid = true)
    {
        float rangeTemp = Math.Max(0, range - 0.5f);
        var MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
        if (MainPlayer != null)
        {
            return MainPlayer.ClientNavFindPath(postion, rangeTemp, action, isChanageFindPath, recordOnStateForbid);
        }

        return false;
    }

    public delegate void ChangeTaskDelegate(int taskType, params object[] args);

    public static Dictionary<TaskType, ChangeTaskDelegate> TaskMap = new();

    public static Dictionary<int, List<RunningTaskTarget>> RunnigTaskMap =
        new();

    /// <summary>
    /// 对话，气泡
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="args"></param>
    private static void OnDialogue(int taskType, object[] args)
    {
        if (taskType != (int)TaskType.Dialogue)
        {
            return;
        }

        if (args == null)
        {
            return;
        }

        if (args.Length < 2)
        {
            return;
        }

        if (RunnigTaskMap.TryGetValue(taskType, out var targets) && targets != null && targets.Count > 0)
        {
            int type = System.Convert.ToInt32(args[0]);

            int id = 0;
            int talkid = 0;

            if (type == 3)
            {
                talkid = System.Convert.ToInt32(args[1]);
            }
            else
            {
                id = System.Convert.ToInt32(args[1]);
                talkid = System.Convert.ToInt32(args[2]);
            }

            RunningTaskTarget runningTask = null;
            int targertype = 0;
            int targetid = 0;
            int targettalkid = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                runningTask = targets[i];
                if (runningTask != null)
                {
                    System.Int32.TryParse(runningTask.Args[0], out targertype);
                    if (targertype == 3)
                    {
                        System.Int32.TryParse(runningTask.Args[1], out targettalkid);
                    }
                    else
                    {
                        System.Int32.TryParse(runningTask.Args[1], out targetid);
                        System.Int32.TryParse(runningTask.Args[2], out targettalkid);
                    }

                    System.Int32.TryParse(runningTask.Args[1], out targettalkid);
                    if (type == targertype && id == targetid && talkid == targettalkid)
                    {
                        ClientFinishTask(runningTask.TaskID, runningTask.EventID);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 前往 区域
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="args"></param>
    private static void OnArrive(int taskType, object[] args)
    {
        if (taskType != (int)TaskType.Arrive)
        {
            return;
        }

        if (args == null)
        {
            return;
        }

        if (args.Length != 1)
        {
            return;
        }

        if (RunnigTaskMap.TryGetValue(taskType, out var targets) && targets != null && targets.Count > 0)
        {
            int id = System.Convert.ToInt32(args[0]);

            RunningTaskTarget runningTask = null;
            int targetid = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                runningTask = targets[i];
                if (runningTask != null)
                {
                    System.Int32.TryParse(runningTask.Args[0], out targetid);
                    if (id == targetid)
                    {
                        ClientFinishTask(runningTask.TaskID, runningTask.EventID);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 序章
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="args"></param>
    private static void OnPrologue(int taskType, object[] args)
    {
        if (taskType != (int)TaskType.Prologue)
        {
            return;
        }

        if (args == null)
        {
            return;
        }

        if (args.Length != 1)
        {
            return;
        }

        if (RunnigTaskMap.TryGetValue(taskType, out var targets) && targets != null && targets.Count > 0)
        {
            int id = System.Convert.ToInt32(args[0]);

            RunningTaskTarget runningTask = null;
            int targetid = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                runningTask = targets[i];
                if (runningTask != null)
                {
                    System.Int32.TryParse(runningTask.Args[0], out targetid);
                    if (id == targetid)
                    {
                        ClientFinishTask(runningTask.TaskID, runningTask.EventID);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 黑幕
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="args"></param>
    private static void OnShady(int taskType, object[] args)
    {
        if (taskType != (int)TaskType.Shady)
        {
            return;
        }

        if (args == null)
        {
            return;
        }

        if (args.Length != 1)
        {
            return;
        }

        if (RunnigTaskMap.TryGetValue(taskType, out var targets) && targets != null && targets.Count > 0)
        {
            int id = System.Convert.ToInt32(args[0]);

            RunningTaskTarget runningTask = null;
            int targetid = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                runningTask = targets[i];
                if (runningTask != null)
                {
                    System.Int32.TryParse(runningTask.Args[0], out targetid);
                    if (id == targetid)
                    {
                        ClientFinishTask(runningTask.TaskID, runningTask.EventID);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 其他模块通知客户端，任务进度变化 如对话，黑幕，序章 ，到达区域
    /// </summary>
    /// <param name="taskType"></param>
    /// <param name="args"></param>
    public static void NoticeClientTaskChange(TaskType taskType, params object[] args)
    {
        if (RunnigTaskMap == null)
        {
            return;
        }

        if (RunnigTaskMap.Count < 1)
        {
            return;
        }

        if (TaskMap.ContainsKey(taskType))
        {
            TaskMap[taskType].Invoke((int)taskType, args);
        }
    }

    private static TaskManager taskManager;

    static public void Init()
    {
        taskManager = new TaskManager();
        TaskMap.Clear();
        TaskMap.Add(TaskType.Dialogue, OnDialogue); //对话
        TaskMap.Add(TaskType.Arrive, OnArrive); //前往
        TaskMap.Add(TaskType.Prologue, OnPrologue); //序章
        TaskMap.Add(TaskType.Shady, OnShady); //黑幕
        RunnigTaskMap.Clear();
        taskManager.Init();
    }

    static public void Release()
    {
        RunnigTaskMap.Clear();
        taskManager.Release();
    }

    public static void ClearTask()
    {
        taskManager?.ClearTask();
    }

    public static void UnRegister(int taskID, int eventID, int type)
    {
        if (RunnigTaskMap.TryGetValue(type, out var runningTasks) && runningTasks != null && runningTasks.Count > 0)
        {
            RunningTaskTarget target = null;
            int index = -1;
            for (int i = 0; i < runningTasks.Count; i++)
            {
                target = runningTasks[i];
                if (target != null)
                {
                    if (target.TaskID == taskID && target.EventID == eventID)
                    {
                        index = i;
                        break;
                    }
                }
            }

            runningTasks.RemoveAt(index);
            if (runningTasks.Count == 0)
            {
                RunnigTaskMap.Remove(type);
            }
        }
    }


    public class RunningTaskTarget
    {
        public int TaskID;
        public int EventID;
        public int TaskType;
        public List<string> Args = new();

        public RunningTaskTarget(int taskID, int eventID, int type, string arg)
        {
            this.TaskID = taskID;
            this.EventID = eventID;
            this.TaskType = type;
            this.Args.Clear();
            string[] temps = arg.Split(',');
            if (temps != null && temps.Length > 0)
            {
                foreach (var item in temps)
                {
                    this.Args.Add(item);
                }
            }
        }
    }

    public static string GetTaskTypeName(TaskClassifyType taskType, string ChapterName)
    {
        switch (taskType)
        {
            case TaskClassifyType.MainLine:
                //return $"[{GameConfig.LocalStr["TaskLabel1"]}] ";
                return $"[{LanguageManager.Instance.GetLanguageByKey("TaskLabel1")}] ";
            case TaskClassifyType.SubbranchLine:
                //return $"[{GameConfig.LocalStr["TaskLabel2"]}] ";
                return $"[{LanguageManager.Instance.GetLanguageByKey("TaskLabel2")}] ";
            case TaskClassifyType.Biography:
                //return $"[{GameConfig.LocalStr["TaskLabel3"]}] ";
                return $"[{LanguageManager.Instance.GetLanguageByKey("TaskLabel3")}] ";
            case TaskClassifyType.Play:
                return $"[{ChapterName}] ";
            case TaskClassifyType.Guide:
                return $"[{ChapterName}] ";
            case TaskClassifyType.Other:
                return $"[{ChapterName}] ";
            case TaskClassifyType.Challenge:
                return $"[{ChapterName}] ";
        }

        return string.Empty;
    }

    public static string GetTaskTypeNameNoColor(TaskClassifyType taskType, string ChapterName)
    {
        switch (taskType)
        {
            case TaskClassifyType.MainLine:
                //return $"【{GameConfig.LocalStr["TaskLabel1"]}】";
                return $"【{LanguageManager.Instance.GetLanguageByKey("TaskLabel1")}】";
            case TaskClassifyType.SubbranchLine:
                //return $"【{GameConfig.LocalStr["TaskLabel2"]}】";
                return $"【{LanguageManager.Instance.GetLanguageByKey("TaskLabel2")}】";
            case TaskClassifyType.Biography:
                //return $"【{GameConfig.LocalStr["TaskLabel3"]}】";
                return $"【{LanguageManager.Instance.GetLanguageByKey("TaskLabel3")}】";
            case TaskClassifyType.Play:
                return $"【{ChapterName}】";
            case TaskClassifyType.Guide:
                return $"【{ChapterName}】";
            case TaskClassifyType.Other:
                return $"【{ChapterName}】";
        }

        return string.Empty;
    }

    public static string GetTaskProgressStr(uint taskID, int groupID)
    {
        if (groupID == 0)
        {
            return string.Empty;
        }
        else
        {
            uint uIndex = 0;
            List<uint> progress = new List<uint>();
            if (TaskManager.TaskConfigs.list != null)
            {
                foreach (var item in TaskManager.TaskConfigs.list)
                {
                    if (item.Value.Base.GroupID == groupID)
                    {
                        progress.Add(item.Value.Base.UIndex);
                        if (taskID == item.Value.Base.TaskID)
                        {
                            uIndex = item.Value.Base.UIndex;
                        }
                    }
                }
            }

            progress.Sort();
            int index = progress.IndexOf(uIndex) + 1;
            return string.Format("({0}/{1})", index, progress.Count);
        }
    }

    public static string TaskTypeToName(TaskClassifyType taskType)
    {
        switch (taskType)
        {
            case TaskClassifyType.MainLine:
                //return GameConfig.LocalStr["TaskLabel1"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel1");
            case TaskClassifyType.SubbranchLine:
                //return GameConfig.LocalStr["TaskLabel2"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel2");
            case TaskClassifyType.Biography:
                //return GameConfig.LocalStr["TaskLabel3"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel3");
            case TaskClassifyType.Play:
                //return GameConfig.LocalStr["TaskLabel4"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel4");
            case TaskClassifyType.Guide:
                //return GameConfig.LocalStr["TaskLabel5"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel5");
            case TaskClassifyType.Other:
                //return GameConfig.LocalStr["TaskLabel6"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel6");
            case TaskClassifyType.Challenge:
                //return GameConfig.LocalStr["TaskLabel6"];
                return LanguageManager.Instance.GetLanguageByKey("TaskLabel7");
        }

        return string.Empty;
    }

    public static string GetChapterName(int id)
    {
        if (LocalDataManager.Instance.M_PlotData != null &&
            LocalDataManager.Instance.M_PlotData.StaticPlotDatas != null)
        {
            if (LocalDataManager.Instance.M_PlotData.StaticPlotDatas.TryGetValue(id, out var data))
            {
                return data.Desc;
            }
        }

        return string.Empty;
    }


    public static bool TaskIsCommit(uint taskID)
    {
        return taskManager.TaskIsCommit(taskID);
    }

    /// <summary>
    /// 任务是否完成
    /// </summary>
    /// <param name="taskID"></param>
    /// <returns></returns>
    public static bool IsTaskFinsh(uint taskID)
    {
        return taskManager.IsTaskFinsh(taskID);
    }


    public static void UpData(uint taskID, byte val)
    {
        taskManager.UpData(taskID, val);
    }

    /// <summary>
    /// 任务目标是否完成
    /// </summary>
    /// <param name="taskID"></param>
    /// <param name="eventID"></param>
    /// <returns></returns>
    public static bool IsFinishTaskEvent(uint taskID, int eventID)
    {
        return taskManager.IsFinishTaskEvent(taskID, eventID);
    }

    /// <summary>
    /// 是否存在任务
    /// </summary>
    /// <param name="taskID"></param>
    /// <returns></returns>
    public static bool ContainTask(uint taskID)
    {
        return taskManager.ContainTask(taskID);
    }

    public static TaskInfo GetRunningTask(uint taskID)
    {
        return taskManager.GetRunningTask(taskID);
    }

    public static TaskTarget GetTaskTarget(uint taskID)
    {
        return taskManager.GetTaskTarget(taskID);
    }
    public static string GetTaskTypeIcon(int type)
    {
        return taskManager.GetTaskTypeIcon((TaskClassifyType)type);
    }
    public static TaskConfigInfo GetTaskConfig(uint taskID)
    {
        return taskManager.GetTaskConfig(taskID);
    }

    public static int GetTaskRunningEventID(uint taskID)
    {
        return taskManager.GetTaskRunningEventID(taskID);
    }

    public static void DoGuide(int id)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
        if (module != null)
        {
            module.TestTutorial(id);
        }
    }

    public static void EndGuide(int id)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
        if (module != null)
        {
            module.EndGuide(id);
        }
    }


    public static bool GuideFinished(int id)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
        if (module != null)
        {
            return module.GuideFinished(id);
        }

        return false;
    }

    public static bool IsRuningGuide(int id)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
        if (module != null)
        {
            return module.IsRuningGuide(id);
        }
        return false;
    }

    // <summary>
    /// 获取所有的任务
    /// </summary>
    /// <returns></returns>
    public static List<TaskInfo> GetAllTask()
    {
        return taskManager.GetAllTask();
    }


    public static Dictionary<int, UITaskData> GetAllWindow(bool isAddWantedTask = false)
    {
        return taskManager.GetAllWindow(isAddWantedTask);
    }

    public static List<TaskScrollInfo> GetWindowByType(TaskClassifyType taskType)
    {
        return taskManager.GetWindowByType(taskType);
    }

    public static List<TaskInfo> GetTasks(TaskClassifyType taskType)
    {
        return taskManager.GetTasks(taskType);
    }

    public static float GetAreaDataRange(int areaID)
    {
        if (GameMap.sceneJsonData.Areas.TryGetValue(areaID, out var area) && area != null)
        {
            return area.Radius;
        }

        return 1;
    }

    public static bool FindPostionPath(Vector3 targePos, out Vector3[] coners, out float distance)
    {
        coners = new Vector3[] { };
        distance = 0;

        if (GameManager.Instance.M_MainPlayerCtrlBase == null)
        {
            return false;
        }

        return FindPathManager.Instance.FindPath(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position(), targePos, out coners, out distance);
    }

    // public void

    static GetMoveBuffReq getMoveBuffReq = new();
    public static void SendReqSpeedBuff(bool isOpen = true)
    {
        SpaceType curMapType = GameManager.Instance.GetCurMapType();

        // 副本类型 不需要加
        if (curMapType != SpaceType.SpaceDefault && curMapType != SpaceType.SpaceScene)
        {
            return;
        }



        // SGF.Debuger.LogError($"[SpeedBuff] isOpen: {isOpen}");
        getMoveBuffReq.IsOpen = isOpen;

        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
        battleSocket.SendRPCMsg(ServerType.ServerTypeScene, getMoveBuffReq);

    }

    public static bool TryReqSpeedBuff(Vector3 targetPos)
    {
        FindPostionPath(targetPos, out Vector3[] coners, out float distance);
        var disCfg = LocalDataManager.Instance.GetSystemDataCell("Guidance_MoveFastFindPathDIST");

        if (distance <= disCfg.Value* disCfg.Value)
        {
            return false;
        }

        var cfg = LocalDataManager.Instance.GetSystemDataCell("Guidance_MoveFastEndLv");
        if (cfg == null)
        {
            return false;
        }

        var playerLevel = GameManager.Instance.GetPlayerLevel();
        if (playerLevel >= cfg.Value)
        {
            return false;
        }

        SendReqSpeedBuff(true);

        return true;
    }


}