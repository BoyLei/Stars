///--------------------------------------------------------------------
/// 文件名   :   TaskManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:39:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using ProtoMsg;
using SGF;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Module;
using StarProject.Service.DisplayProcess;
using StarProject.Service.Function;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.ServerService;
using StarProjectDef;
using System.Collections.Generic;
using Task;
using UnityEngine;
using static StarProject.Service.Function.GlobalFunctionManager;

public class TaskManager
{
    private static Dictionary<int, ClassifyTask> ServerTaskDatas = null;

    private TaskNpcManager taskNpcManager = new();

    private CacheFindPath cacheFindPath = new();


    public readonly string JsonPath = "Config/Task/TaskConfigs";
    public readonly string BinaryPath = "Config/Task/TaskConfigsBin";

    //备份数据用于 还原
    public static TaskConfigs TaskConfigs { get; private set; }


    public int CurrentSelectTaskID { get; private set; }

    public void ClearTask()
    {
        ServerTaskDatas?.Clear();
    }
    public void Init()
    {
        var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(BinaryPath);
        if (textasset != null)
        {
            TaskConfigs = MessagePack.MessagePackSerializer.Deserialize<TaskConfigs>(textasset.bytes);
            StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(BinaryPath);
        }

        taskNpcManager.Init();
        /*数据*/
        if (ServerTaskDatas == null)
        {
            ServerTaskDatas = new Dictionary<int, ClassifyTask>();
        }

        ServerTaskDatas.Clear();

        //  --任务列表
        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TaskMDID, OnNewReceiveTasks, null);
        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TasksID, OnReceiveTasks, null);

        //---单个任务数据修改通知
        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.NoticeTasksID, OnModifyTask, null);

        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TaskRewardNtfID, OnTaskRewardNtf, null);
        GlobalEvent.SelectTaskItem.AddListener(OnTaskSelect);

        GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
        GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
    }

    private void OnRoleCreateComplete(object complete)
    {
        StarProject.Game.GameManager.Instance.UpdateAllNpcServiceState();
        if (taskNpcManager != null)
        {
            taskNpcManager.UpdataAll();
        }
    }

    public void Release()
    {
        taskNpcManager.Release();
        ServerTaskDatas.Clear();
        NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TaskMDID, OnNewReceiveTasks, null);
        NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TasksID, OnReceiveTasks, null);

        //---单个任务数据修改通知
        NetworkManager.Instance.OffMessageEnum(MsgIDEnum.NoticeTasksID, OnModifyTask, null);
        NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TaskRewardNtfID, OnTaskRewardNtf, null);
        GlobalEvent.SelectTaskItem.RemoveListener(OnTaskSelect);
        GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
    }

    private void OnTaskSelect(int taskID)
    {
        CurrentSelectTaskID = taskID;
    }

    public bool QueryNpcTaskState(int npcID)
    {
        return taskNpcManager.QueryNpcTaskState(npcID);
    }

    public TaskNpc.StateEnum QueryNpcTaskEnum(int npcID, ref uint taskID)
    {
        return taskNpcManager.QueryNpcTaskEnum(npcID, ref taskID);
    }

    public TaskNpc.StateEnum QueryInterTaskEnum(int configID, ref uint taskID)
    {
        return taskNpcManager.QueryInterTaskEnum(configID, ref taskID);
    }

    public Dictionary<long, List<TaskNpc>> GetCanReceiveNpcTask()
    {
        return taskNpcManager.GetCanReceiveNpcTask();
    }

    public Dictionary<long, List<TaskNpc>> GetCanReceiveInterTask()
    {
        return taskNpcManager.GetCanReceiveInterTask();
    }

    public List<TaskNpc> GetTaskNpcs(int npcID)
    {
        return taskNpcManager.GetNpcTasks(npcID);
    }

    public List<TaskNpc> GetInterTasks(int interID)
    {
        return taskNpcManager.GetInterTasks(interID);
    }

    /// <summary>
    /// 任务是否完成
    /// </summary>
    /// <param name="taskID"></param>
    /// <returns></returns>
    public bool IsTaskFinsh(uint taskID)
    {
        var data = GetTaskConfig(taskID);
        if (data != null && data.Base != null)
        {
            int taskType = (int)data.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                int chainID = data.Base.ChainID;
                int saveID = (int)data.Base.UIndex;
                return ServerTaskDatas[taskType].ContainKey(chainID, saveID);
            }
        }
        else
        {
            SGF.Debuger.LogWarning($"不存在 taskID={taskID} 的配置 err!!! 但是被引用到了");
        }
        return false;
    }

    public void UpData(uint taskID, byte val)
    {
        var data = GetTaskConfig(taskID);
        int taskType = (int)data.Base.TaskType;
        if (ServerTaskDatas.ContainsKey(taskType))
        {
            int chainID = data.Base.ChainID;
            int saveID = (int)data.Base.UIndex;
            ServerTaskDatas[taskType].UpData(chainID, saveID, val);
        }
    }

    public uint PackIndex(uint type, uint chap, uint id)
    {
        uint result = (type << 28) | (chap << 16) | (id << 0);
        return result;
    }

    public (uint type, uint chainid, uint id) UnPackIndex(uint taskID)
    {
        uint type = (taskID >> 28) & 0xF;
        uint chap = (taskID >> 16) & 0xFFF;
        uint id = taskID & 0xFFFF;
        return (type, chap, id);
    }

    /// <summary>
    /// 任务目标是否完成
    /// </summary>
    /// <param name="taskID"></param>
    /// <param name="eventID"></param>
    /// <returns></returns>
    public bool IsFinishTaskEvent(uint taskID, int eventID)
    {
        var data = GetTaskConfig(taskID);
        if (data != null && data.Base != null)
        {
            int taskType = (int)data.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                int chainID = data.Base.ChainID;
                int saveID = (int)data.Base.UIndex;
                if (ServerTaskDatas[taskType].IsFinishTaskEvent(taskID, eventID))
                {
                    return true;
                }
                else
                {
                    return ServerTaskDatas[taskType].ContainKey(chainID, saveID);
                }
            }
        }
        else
        {
            SGF.Debuger.LogWarning($"不存在 taskID={taskID} 的配置 err!!! 但是被引用到了");
        }
        return false;
        return false;
    }

    /// <summary>
    /// 是否存在任务
    /// </summary>
    /// <param name="taskID"></param>
    /// <returns></returns>
    public bool ContainTask(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                return ServerTaskDatas[taskType].ContainTask(taskID);
            }
        }

        return false;
    }

    public TaskInfo GetRunningTask(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                return ServerTaskDatas[taskType].GetTask(taskID);
            }
        }

        return null;
    }


    public bool TaskIsCommit(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                return ServerTaskDatas[taskType].TaskIsCommit(taskID);
            }
        }

        return false;
    }

    private void OnSceneMapLoadComplete(int mapID)
    {
        //清除所有服务
        StarProject.Service.ServerService.ServerServiceManager.Instance.ClearAllService();
        taskNpcManager.Clear();
        if (TaskConfigs != null && TaskConfigs.list != null && TaskConfigs.list.Count > 0)
        {
            foreach (var item in TaskConfigs.list)
            {
                int index = 0;
                //承接任务

                if (!item.Value.Condition.AutoPick && item.Value.Condition.PickTaskMapID == mapID &&
                    item.Value.Condition.PickEffects != null && item.Value.Condition.PickEffects.Count > 0)
                {
                    var data = GameMap.sceneJsonData.GetNPCJsonData(item.Value.Condition.PickTaskNPC);

                    if (data != null)
                    {
                        ServerService service = new();
                        service.ConditionGroupID = GlobalFunctionManager.Instance.GetConditionGroupID();
                        service.EffectID = (int)(item.Value.Base.TaskID * 100) + index++;
                        service.ServiceSourceID = (int)item.Value.Base.TaskID;
                        service.ObjID = (uint)item.Value.Condition.PickTaskNPC;
                        service.regServiceSubType = RegServiceSubType.Task_Pick;
                        service.ServiceName = item.Value.Base.TaskName;
                        service.IsTaskServer = true;

                        if (item.Value.Condition.PickEffects != null && item.Value.Condition.PickEffects.Count > 0)
                        {
                            service.ServiceName = item.Value.Condition.PickEffects[0].ShowText;
                            service.ServiceName =
                                TaskHelper.WildCardStr(service.ServiceName, item.Value.Condition.PickTaskNPC);
                        }

                        ServerServiceManager.Instance.RegisterService(ServerServiceType.NPC, service);


                        //
                        List<ConditionData> cMconditions = new();
                        //前置任务是否满足
                        if (item.Value.Base.PreTaskID != 0)
                        {
                            ConditionData preTask = new(service.ConditionGroupID, ConditionType.TaskIsFinish,
                                new TaskFinishCondition()
                                { TaskID = item.Value.Base.PreTaskID, IsShow = false, Flag = false, SubID = 0 }, 0);
                            cMconditions.Add(preTask);
                        }

                        //角色等级是否满足

                        ConditionData level = new(service.ConditionGroupID, ConditionType.LevelLimit,
                            new LevelCondition()
                            { Level = item.Value.Condition.Level, IsShow = false, Flag = false, SubID = 0 }, 0);
                        cMconditions.Add(level);

                        //不在进行中的任务
                        ConditionData notdo = new(service.ConditionGroupID, ConditionType.TaskIsRunning,
                            new TaskRunningCondition()
                            { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = true, SubID = 0 }, 0);
                        cMconditions.Add(notdo);


                        //任务未完成
                        ConditionData notfinish = new(service.ConditionGroupID, ConditionType.TaskIsFinish,
                            new TaskFinishCondition()
                            { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = true, SubID = 0 }, 0);

                        // ConditionData notfinish = new ConditionData(service.ConditionGroupID, InterActionConditionType.TaskIsFinish, new List<string>() { item.Value.TaskId }, false,false);
                        cMconditions.Add(notfinish);

                        ///条件注册
                        GlobalFunctionManager.Instance.RegisterConditions(service.ConditionGroupID, cMconditions);


                        TaskNpc npc = new();
                        npc.TaskID = item.Value.Base.TaskID;
                        npc.TaskType = (int)item.Value.Base.TaskType;
                        npc.taskNPCSource = TaskNpc.TaskNPCSource.Accept;
                        npc.ConditionID = service.ConditionGroupID;
                        npc.Update(npc.TaskID);
                        taskNpcManager.RegisterNpc(item.Value.Condition.PickTaskNPC, npc);
                    }
                }


                //交付NPC

                if (!item.Value.TaskFinishInfo.AutoFinish && item.Value.TaskFinishInfo.FinishTaskMapID == mapID &&
                    item.Value.TaskFinishInfo.FinishEffects != null && item.Value.TaskFinishInfo.FinishEffects.Count > 0)
                {
                    var data = GameMap.sceneJsonData.GetNPCJsonData(item.Value.TaskFinishInfo.FinishTaskNPC);

                    if (data != null)
                    {
                        ServerService service = new();
                        service.ConditionGroupID = GlobalFunctionManager.Instance.GetConditionGroupID();
                        service.EffectID = (int)(item.Value.Base.TaskID * 100) + index++;
                        service.ServiceSourceID = (int)item.Value.Base.TaskID;
                        service.ObjID = (uint)item.Value.TaskFinishInfo.FinishTaskNPC;
                        service.regServiceSubType = RegServiceSubType.Task_Finish;
                        service.ServiceName = item.Value.Base.TaskName;
                        service.IsTaskServer = true;
                        /*if (item.Value.Base.FinishEffects != null && item.Value.Base.FinishEffects.Count > 0)
                        {
                            service.ServiceName = item.Value.Base.FinishEffects[0].ShowText;
                            service.ServiceName =
                                TaskHelper.WildCardStr(service.ServiceName, item.Value.Base.FinishTaskNPC);
                        }*/


                        ServerServiceManager.Instance.RegisterService(ServerServiceType.NPC, service);


                        List<ConditionData> cMconditions = new();
                        //任务进行中
                        ConditionData condition1 = new(service.ConditionGroupID, ConditionType.TaskIsRunning,
                            new TaskRunningCondition()
                            { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = false, SubID = 0 }, 0);

                        //   ConditionData condition1 = new ConditionData(service.ConditionGroupID, InterActionConditionType.TaskIsRunning, new List<string>() { item.Value.TaskId }, false, false);

                        //任务已完成未提交
                        ConditionData condition = new(service.ConditionGroupID, ConditionType.TaskIsCommit,
                            new TaskCommitCondition()
                            { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = false, SubID = 0 }, 0);

                        // ConditionData condition = new ConditionData(service.ConditionGroupID, InterActionConditionType.TaskIsCommit, new List<string>() { item.Value.TaskId }, false,false);
                        cMconditions.Add(condition);
                        cMconditions.Add(condition1);

                        GlobalFunctionManager.Instance.RegisterConditions(service.ConditionGroupID, cMconditions);


                        TaskNpc npc = new();
                        npc.TaskID = item.Value.Base.TaskID;
                        npc.TaskType = (int)item.Value.Base.TaskType;
                        npc.taskNPCSource = TaskNpc.TaskNPCSource.Commit;
                        npc.ConditionID = service.ConditionGroupID;
                        npc.Update(npc.TaskID);
                        taskNpcManager.RegisterNpc(item.Value.TaskFinishInfo.FinishTaskNPC, npc);
                    }
                }


                //任务事件

                if (item.Value.TaskFinishInfo.Targets != null && item.Value.TaskFinishInfo.Targets.Count > 0)
                {
                    foreach (var data in item.Value.TaskFinishInfo.Targets)
                    {
                        if (data.Target.TargetType == EffectTargetType.NPC)
                        {
                            var npccfg = GameMap.sceneJsonData.GetNPCJsonData(data.Target.TargetID);

                            if (npccfg != null)
                            {
                                ServerService service = new();
                                service.ConditionGroupID = GlobalFunctionManager.Instance.GetConditionGroupID();
                                service.EffectID = (int)(item.Value.Base.TaskID * 100) + index++;
                                service.ServiceSourceID = (int)item.Value.Base.TaskID;
                                service.ObjID = (uint)data.Target.TargetID;
                                service.regServiceSubType = RegServiceSubType.Task_Event;
                                service.ServiceSourceSubID = data.Target.ID;
                                string serviceName = item.Value.Base.TaskName;
                                if (data.Target.Effects != null && data.Target.Effects.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(data.Target.Effects[0].ShowText))
                                    {
                                        serviceName = data.Target.Effects[0].ShowText;
                                        serviceName = TaskHelper.WildCardStr(serviceName, data.Target.TargetID);
                                    }
                                }

                                service.ServiceName = serviceName;
                                service.IsTaskServer = true;
                                ServerServiceManager.Instance.RegisterService(ServerServiceType.NPC, service);

                                List<ConditionData> cMconditions = new();

                                //任务进行中
                                ConditionData condition1 = new(service.ConditionGroupID, ConditionType.TaskIsRunning,
                                    new TaskRunningCondition()
                                    { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = false, SubID = 0 },
                                    0);

                                //任务目标在进行中
                                ConditionData condition = new(service.ConditionGroupID, ConditionType.TaskEventIsFinish,
                                    new TaskEventFinishCondition()
                                    {
                                        TaskID = item.Value.Base.TaskID,
                                        EventID = data.Target.ID,
                                        IsShow = false,
                                        Flag = true,
                                        SubID = 0
                                    }, 0);

                                cMconditions.Add(condition);
                                cMconditions.Add(condition1);

                                GlobalFunctionManager.Instance.RegisterConditions(service.ConditionGroupID,
                                    cMconditions);

                                TaskNpc npc = new();
                                npc.TaskID = item.Key;
                                npc.TaskEventID = data.Target.ID;
                                npc.TaskType = (int)data.Target.TaskType.TaskType;
                                npc.taskNPCSource = TaskNpc.TaskNPCSource.TaskEvent;
                                npc.Update(npc.TaskID);
                                taskNpcManager.RegisterNpc(data.Target.TargetID, npc);
                            }
                        }
                        else if (data.Target.TargetType == EffectTargetType.InterAction)
                        {
                            var mineCfg = GameMap.sceneJsonData.GetMineJsonData(data.Target.TargetID);
                            if (data != null)
                            {
                                ServerService service = new();
                                service.ConditionGroupID = GlobalFunctionManager.Instance.GetConditionGroupID();
                                service.EffectID = 0;
                                service.ServiceSourceID = (int)item.Value.Base.TaskID;
                                service.ObjID = (uint)data.Target.TargetID;
                                service.regServiceSubType = RegServiceSubType.Task_Event;
                                service.ServiceSourceSubID = data.Target.ID;
                                string serviceName = item.Value.Base.TaskName;
                                if (data.Target.Effects != null && data.Target.Effects.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(data.Target.Effects[0].ShowText))
                                    {
                                        serviceName = data.Target.Effects[0].ShowText;
                                        serviceName = TaskHelper.WildCardStr(serviceName, data.Target.TargetID);
                                    }
                                }

                                service.ServiceName = serviceName;
                                service.IsTaskServer = true;
                                ServerServiceManager.Instance.RegisterService(ServerServiceType.InterAction, service);


                                List<ConditionData> cMconditions = new();

                                //任务进行中
                                ConditionData condition1 = new(service.ConditionGroupID, ConditionType.TaskIsRunning,
                                    new TaskRunningCondition()
                                    { TaskID = item.Value.Base.TaskID, IsShow = false, Flag = false, SubID = 0 },
                                    0);

                                //任务目标在进行中
                                ConditionData condition = new(service.ConditionGroupID, ConditionType.TaskEventIsFinish,
                                    new TaskEventFinishCondition()
                                    {
                                        TaskID = item.Value.Base.TaskID,
                                        EventID = data.Target.ID,
                                        IsShow = false,
                                        Flag = true,
                                        SubID = 0
                                    }, 0);

                                cMconditions.Add(condition);
                                cMconditions.Add(condition1);

                                GlobalFunctionManager.Instance.RegisterConditions(service.ConditionGroupID,
                                    cMconditions);
                                InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(data.Target.TargetID);
                                if (interactDataCell != null && interactDataCell.GetIsShowTask())//这里判断交互物是否要显示任务状态
                                {
                                    TaskNpc npc = new();
                                    npc.TaskID = item.Key;
                                    npc.TaskEventID = data.Target.ID;
                                    npc.TaskType = (int)data.Target.TaskType.TaskType;
                                    npc.taskNPCSource = TaskNpc.TaskNPCSource.TaskEvent;
                                    npc.Update(npc.TaskID);
                                    taskNpcManager.RegisterInter(data.Target.TargetID, npc);
                                }
                            }
                        }
                    }
                }
            }
        }

        StarProject.Game.GameManager.Instance.UpdateAllNpcServiceState();
        taskNpcManager.UpdataAll();

        GlobalEvent.OnTaskConfigLoad?.Invoke(null);
        // DoCacheFindPath();
    }

    public void DoCacheFindPath()
    {
        if (cacheFindPath != null && !cacheFindPath.IsNull)
        {
            //cacheFindPath.DoFindPath();

            DelayInvoker.DelayInvoke(0.5f, (arg) => { cacheFindPath.DoFindPath(); }, null);
        }
    }

    public void SetCacheFindPath(ulong serverID, int mapID, long cfgid, UnityEngine.Vector3 pos, E_FindPath e_Find, float range, System.Action<bool> cb)
    {
        if (cacheFindPath != null && cacheFindPath.IsNull)
        {
            // 判断跳转的场景是否解锁
            bool isOpen = GameManager.Instance.CheckMapIsOpen(mapID);
            if (isOpen)
            {
                StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
                // 这里的任务是 切场景

                //判断是否是公会领地
                MapCfgData mapCfgData = LocalDataManager.Instance.GetMapCfgData(mapID);
                if (mapCfgData != null)
                {

                    //地图切换二次确认框
                    UIAPI.ShowMsgBox(74, (EventName) => {
                    
                        if(EventName.Equals("SURE"))
                        {

                            //出现交互条
                            if (GlobalEvent.InterEventByStr != null)
                            {
                                //设置玩家为交互状态
                                PlayerCtrlGroup player = (PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
                                if (player != null)
                                {
                                    player.PrepareInteract(0);
                                }
                                //交互条
                                var time = LocalDataManager.Instance.GetSystemDataCell(329).Value;
                                GlobalEvent.InterEventByStr.Invoke(LanguageManager.Instance.GetLanguageByKey("TransferWaiting"), time, () => {

                                    //回调开始切换地图
                                    SpaceType spaceType = mapCfgData.MapType;
                                    if (spaceType == SpaceType.SpaceGuildTerritory)
                                    {
                                        starWorld.SendFBChangeReq(ChangeReason.Instance, mapID, SpaceType.SpaceGuildTerritory, serverID, 0, true);
                                    }
                                    else
                                    {
                                        starWorld.SendFBChangeReq(ChangeReason.SameServer, mapID, SpaceType.SpaceScene, serverID);
                                    }
                                    cacheFindPath.SetData(mapID, cfgid, pos, e_Find, range, cb);
                                },()=> {
                                    //中途取消
                                    cb?.Invoke(false);
                                });
                            }



                        }
                        else
                        {
                            cb?.Invoke(false);
                        }
                    
                    }, mapCfgData.MapName);


                }

                
            }
            else
            {
                cb?.Invoke(false);
                SGF.Debuger.LogWarning($"地图未解锁 mapID={mapID},serverID={serverID},cfgid={cfgid}");
                //DisplayProcessDispenser.Instance.AddSystemMessage(GameConfig.LocalStr["MapLockTips"]);
                DisplayProcessDispenser.Instance.AddSystemMessage(LanguageManager.Instance.GetLanguageByKey("MapLockTips"));
            }
        }
    }

    public Dictionary<int, UITaskData> GetAllWindow(bool isAddWantedTask = false)
    {
        Dictionary<int, UITaskData> maps = new();
        var mains = GetTasks(TaskClassifyType.MainLine);
        mains.Sort(SortTask);

        var subs = GetTasks(TaskClassifyType.SubbranchLine);
        subs.Sort(SortTask);

        var biogs = GetTasks(TaskClassifyType.Biography);
        biogs.Sort(SortTask);

        var Plays = GetTasks(TaskClassifyType.Play);
        Plays.Sort(SortTask);

        var guides = GetTasks(TaskClassifyType.Guide);
        guides.Sort(SortTask);

        var challenges = GetTasks(TaskClassifyType.Challenge);
        challenges.Sort(SortTask);
        //var others = GetTasks(TaskClassifyType.Other);
        // others.Sort(SortTask);


        foreach (var it in mains)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                //uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }

        foreach (var it in subs)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                //uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }

        foreach (var it in biogs)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                //uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }

        // 日常任务
        {
            foreach (var it in Plays)
            {
                if (!maps.ContainsKey(it.TaskType))
                {
                    UITaskData uITaskData = new();
                    //uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                    uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                    maps.Add(it.TaskType, uITaskData);
                }

                maps[it.TaskType].AddTask(it);
            }
            // 是否包含通缉任务
            // 通缉任务不在任务系统里面，如果需要获取，需要自行加进去
            if (isAddWantedTask)
            {
                var wantedTaskInfo = GameManager.Instance.GetMainPlayerWantTaskMD();
                if (wantedTaskInfo != null && wantedTaskInfo.IsAcc && wantedTaskInfo.RewNum < SystemConstConfigs.TeamWantedAwardLimits)
                {
                    PlayOpenConditionsDataCell wantedPlayerConf = LocalDataManager.Instance.GetPlayOpenConditionsDataCell(3);
                    TaskInfo wantedTaskNew = new();
                    wantedTaskNew.TaskID = TaskHelper.WANTEDTASKID;
                    wantedTaskNew.TaskState = TaskStateEnum.TaskStageRunning;
                    wantedTaskNew.TaskOverdueTime = 0;
                    wantedTaskNew.TaskTargetProcess = new Dictionary<int, long>();
                    wantedTaskNew.TaskType = (int)TaskClassifyType.Play;
                    wantedTaskNew.TaskTitle = wantedPlayerConf.TaskTitle;
                    wantedTaskNew.TaskDesc = wantedPlayerConf.TaskDesc;
                    wantedTaskNew.Chapter = 1;
                    wantedTaskNew.ChapterName = wantedPlayerConf.TaskTitle;
                    // 目标
                    TaskTarget taskTarget1 = new();
                    taskTarget1.TaskDesc = string.Format(wantedPlayerConf.TaskTarget, SystemConstConfigs.TeamWantedAwardLimits - wantedTaskInfo.RewNum, SystemConstConfigs.TeamWantedAwardLimits);
                    wantedTaskNew.TaskTarget = new List<TaskTarget>() { taskTarget1 };
                    // 奖励
                    wantedTaskNew.Awards = new();
                    List<DropShowDataCell> rewardList = LocalDataManager.Instance.GetDropShowList((int)wantedPlayerConf.GetAwardID());
                    if (rewardList != null && rewardList.Count > 0)
                    {
                        foreach (var item in rewardList)
                        {
                            int itmeID = item.GetItemID();
                            int itemNum = item.GetNum();
                            if (wantedTaskNew.Awards.ContainsKey(itmeID))
                            {
                                wantedTaskNew.Awards[itmeID] += itemNum;
                            }
                            else
                            {
                                wantedTaskNew.Awards.Add(itmeID, itemNum);
                            }
                        }
                    }
                    int index = (int)TaskClassifyType.Play;
                    if (!maps.ContainsKey(index))
                    {
                        UITaskData uITaskData = new();
                        //uITaskData.TypeName = TaskHelper.TaskTypeToName(TaskClassifyType.Play) + GameConfig.LocalStr["TaskLabel"];
                        uITaskData.TypeName = TaskHelper.TaskTypeToName(TaskClassifyType.Play) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                        maps.Add(index, uITaskData);
                    }
                    maps[(int)TaskClassifyType.Play].AddTask(wantedTaskNew);
                }
            }
        }

        foreach (var it in guides)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                //uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }
        foreach (var it in challenges)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }
        /*foreach (var it in others)
        {
            if (!maps.ContainsKey(it.TaskType))
            {
                UITaskData uITaskData = new();
                uITaskData.TypeName = TaskHelper.TaskTypeToName((TaskClassifyType)it.TaskType) + GameConfig.LocalStr["TaskLabel"];
                maps.Add(it.TaskType, uITaskData);
            }

            maps[it.TaskType].AddTask(it);
        }*/

        //foreach (var item in maps)
        //{
        //    item.Value.Sort();
        //}
        return maps;
    }

    //public List<TaskScrollInfo> GetAllWindow()
    //{
    //    List<TaskScrollInfo> taskInfos = new List<TaskScrollInfo>();
    //    List<TaskInfo> list = new List<TaskInfo>();
    //    foreach (var item in ServerTaskDatas)
    //    {
    //        foreach (var it in item.Value.RunningData)
    //        {
    //            list.Add(it.Value);
    //        }
    //    }
    //    list.Sort(SortTask);

    //    Dictionary<int, List<int>> addtypes = new Dictionary<int, List<int>>();
    //    List<int> addchapter = new List<int>();

    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (!addtypes.ContainsKey(list[i].TaskType))
    //        {
    //            TaskScrollInfo taskScrollInfo = new TaskScrollInfo(1, TaskHelper.TaskTypeToName((TaskClassifyType)list[i].TaskType) + GameConfig.LocalStr["TaskLabel"], "", "", null, list[i].TaskType == (int)TaskClassifyType.MainLine);
    //            taskInfos.Add(taskScrollInfo);

    //            addtypes.Add(list[i].TaskType, new List<int>());
    //        }

    //        if (!addtypes[list[i].TaskType].Contains(list[i].Chapter))
    //        {
    //            TaskScrollInfo taskScrollInfo1 = new TaskScrollInfo(2, "", list[i].ChapterName, "", null, list[i].TaskType == (int)TaskClassifyType.MainLine);
    //            taskScrollInfo1.Count = list.Count;
    //            taskInfos.Add(taskScrollInfo1);
    //            addtypes[list[i].TaskType].Add(list[i].Chapter);
    //        }
    //        TaskScrollInfo taskScrollInfo2 = new TaskScrollInfo(3, "", "", list[i].TaskTitle, list[i], list[i].TaskType == (int)TaskClassifyType.MainLine);
    //        taskInfos.Add(taskScrollInfo2);
    //    }

    //    bool bselect = false;
    //    foreach (var item in taskInfos)
    //    {
    //        if (item.ScrollType == 3)
    //        {
    //            if (!bselect)
    //            {
    //                item.select = true;
    //                bselect = true;
    //                break;
    //            }
    //        }
    //    }
    //    return taskInfos;
    //}

    public List<TaskScrollInfo> GetWindowByType(TaskClassifyType classifyType)
    {
        int taskSelectType = (int)classifyType;
        List<TaskScrollInfo> taskInfos = new();
        List<TaskInfo> list = new();
        foreach (var item in ServerTaskDatas)
        {
            foreach (var it in item.Value.RunningData)
            {
                if (taskSelectType == it.Value.TaskType)
                {
                    list.Add(it.Value);
                }
            }
        }

        list.Sort(SortTask);

        Dictionary<int, List<int>> addtypes = new();
        List<int> addchapter = new();

        for (int i = 0; i < list.Count; i++)
        {
            if (!addtypes.ContainsKey(list[i].TaskType))
            {
                //TaskScrollInfo taskScrollInfo = new(1, TaskHelper.TaskTypeToName((TaskClassifyType)list[i].TaskType) + GameConfig.LocalStr["TaskLabel"], "", "", null, list[i].TaskType == (int)TaskClassifyType.MainLine);
                TaskScrollInfo taskScrollInfo = new(1, TaskHelper.TaskTypeToName((TaskClassifyType)list[i].TaskType) + LanguageManager.Instance.GetLanguageByKey("TaskLabel"), "", "", null, list[i].TaskType == (int)TaskClassifyType.MainLine);
                taskInfos.Add(taskScrollInfo);
                addtypes.Add(list[i].TaskType, new List<int>());
            }

            if (!addtypes[list[i].TaskType].Contains(list[i].Chapter))
            {
                TaskScrollInfo taskScrollInfo1 = new(2, "", list[i].ChapterName, "", null,
                    list[i].TaskType == (int)TaskClassifyType.MainLine);
                taskScrollInfo1.Count = list.Count;
                taskInfos.Add(taskScrollInfo1);
                addtypes[list[i].TaskType].Add(list[i].Chapter);
            }

            TaskScrollInfo taskScrollInfo2 = new(3, "", "", list[i].TaskTitle, list[i],
                list[i].TaskType == (int)TaskClassifyType.MainLine);
            taskInfos.Add(taskScrollInfo2);
        }

        bool bselect = false;
        foreach (var item in taskInfos)
        {
            if (item.ScrollType == 3)
            {
                if (!bselect)
                {
                    item.select = true;
                    bselect = true;
                    break;
                }
            }
        }

        return taskInfos;
    }

    public List<TaskInfo> GetTasks(TaskClassifyType taskType)
    {
        List<TaskInfo> taskInfos = new();
        foreach (var item in ServerTaskDatas)
        {
            taskInfos.AddRange(item.Value.GetTasks(taskType));
        }

        return taskInfos;
    }


    private void OnNewReceiveTasks(MessageHandleData message)
    {
        if (message == null)
        {
            return;
        }

        TaskMD tasks = message.data as TaskMD;
        if (tasks == null)
        {
            return;
        }

        //主线
        ResolveTask((int)ProtoMsg.TaskType.TtMain, tasks.MainRunList, tasks.MainEndList);
        //支线
        ResolveTask((int)ProtoMsg.TaskType.TtSub, tasks.SubRunList, tasks.SubEndList);
        //传记
        ResolveTask((int)ProtoMsg.TaskType.TtBio, tasks.BioRunList, tasks.BioEndList);
        //每日
        ResolveTask((int)ProtoMsg.TaskType.TtDay, tasks.DayRunList, tasks.DayEndList);
        //引导
        ResolveTask((int)ProtoMsg.TaskType.TtGuide, tasks.GuideRunList, tasks.GuideEndList);
        //其他
        ResolveTask((int)ProtoMsg.TaskType.TtOther, tasks.OtherRunList, tasks.OtherEndList);
        //环任务
        //tasks.DayTaskRing
        //挑战
        ResolveTask((int)ProtoMsg.TaskType.TtNpcchallenge, tasks.NPCChallengeRunList, tasks.NPCChallengeEndList);

        taskNpcManager.UpdataAll();
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.UPDATETASKDATA,
            new object[] { GetAllTask() });
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.FRESHTASK);
        StarProject.Game.GameManager.Instance.UpdateAllNpcServiceState();

        // SystemOpenManager.Instance.TriggerRefresh();
    }

    private void ResolveTask(int taskType, TaskingModel runlist, TaskBinary finishes)
    {
        ClassifyTask classifyTask = new(runlist, finishes);
        if (!ServerTaskDatas.ContainsKey(taskType))
        {
            ServerTaskDatas.Add(taskType, classifyTask);
        }
        else
        {
            ServerTaskDatas[taskType] = classifyTask;
        }
    }

    private void OnReceiveTasks(MessageHandleData message)
    {
        /*if (message == null)
        {
            return;
        }

        Tasks tasks = message.data as Tasks;
        if (tasks == null)
        {
            return;
        }

        //SGF.Debuger.LogError("顺序   任务【全量】同步");

        if (tasks.AllTaskData != null && tasks.AllTaskData.Count > 0)
        {
            for (int i = 0; i < tasks.AllTaskData.Count; i++)
            {
                SingleTaskData singleTask = tasks.AllTaskData[i];
                ClassifyTask classifyTask = new ClassifyTask(singleTask);
                int taskType = (int)singleTask.TType;
                if (!ServerTaskDatas.ContainsKey(taskType))
                {
                    ServerTaskDatas.Add(taskType, classifyTask);
                }
                else
                {
                    ServerTaskDatas[taskType] = classifyTask;
                }
            }
        }

        taskNpcManager.UpdataAll();

        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.UPDATETASKDATA, new object[] { GetAllTask() });
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.FRESHTASK);
        StarProject.Game.GameManager.Instance.UpdateAllNpcServiceState();*/
    }

    private void OnTaskRewardNtf(MessageHandleData message)
    {
        if (message == null)
        {
            return;
        }

        TaskRewardNtf aRewardNtf = message.data as TaskRewardNtf;
        if (aRewardNtf == null)
        {
            return;
        }


        var list = new List<ItemMD>();
        foreach (var award in aRewardNtf.Reward)
        {
            ItemMD itemMD = new() { BaseID = award.Index, Num = award.Count };
            list.Add(itemMD);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "ShowRewardTips", new object[] { award.Index, award.Count });
        }
        foreach (var award in aRewardNtf.HideReward)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "ShowRewardTips", new object[] { award.Index, award.Count });
        }
        if (list.Count > 0)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.RewardsPopModule, "OnOpenRewardsPop", list);
        }



        foreach (var award in aRewardNtf.HideReward)
        {
            ItemMD itemMD = new() { BaseID = award.Index, Num = award.Count };
            ModuleManager.Instance.SendMessage(ModuleDef.Name.DropInfoListModule, "OnAddDrop", new object[] { itemMD });
        }

    }

    private void OnModifyTask(MessageHandleData message)
    {
        if (message == null)
        {
            return;
        }

        NoticeTasks noticeTasks = message.data as NoticeTasks;
        if (noticeTasks == null)
        {
            return;
        }
        //SGF.Debuger.LogError("顺序   任务【单个】同步");

        foreach (var item in noticeTasks.Data)
        {
            if (TaskConfigs.list.TryGetValue((uint)item.TaskID, out TaskConfigInfo taskDataCell))
            {
                int taskType = (int)taskDataCell.Base.TaskType;
                if (ServerTaskDatas.ContainsKey(taskType))
                {
                    ServerTaskDatas[taskType].ModifyTask(item);
                }
                else
                {
                    Debuger.LogWarning($"not find task type  {taskType}");
                }
            }
        }

        //ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.UPDATETASKDATA, GetAllTask());
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.FRESHTASK);
        StarProject.Game.GameManager.Instance.UpdateAllNpcServiceState();
        taskNpcManager.UpdataAll();
    }

    private int SortTask(TaskInfo task1, TaskInfo task2)
    {
        if ((task1.TaskState == TaskStateEnum.TaskStageFinished && task1.ClickFinish) && (task2.TaskState != TaskStateEnum.TaskStageFinished || !task2.ClickFinish))
        {
            return -1;
        }
        else if ((task2.TaskState == TaskStateEnum.TaskStageFinished && task2.ClickFinish) && (task1.TaskState != TaskStateEnum.TaskStageFinished || !task1.ClickFinish))
        {
            return 1;
        }
        else
        {
            if (task1.TaskType < task2.TaskType)
            {
                return -1;
            }
            else if (task1.TaskType > task2.TaskType)
            {
                return 1;
            }
            else
            {
                if (task1.Chapter < task2.Chapter)
                {
                    return -1;
                }
                else if (task1.Chapter > task2.Chapter)
                {
                    return 1;
                }
                else
                {
                    if (task1.TaskID < task2.TaskID)
                    {
                        return -1;
                    }
                    else if (task1.TaskID > task2.TaskID)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取所有的任务
    /// </summary>
    /// <returns></returns>
    public List<TaskInfo> GetAllTask()
    {
        List<TaskInfo> taskInfos = new();
        foreach (var item in ServerTaskDatas)
        {
            if (item.Key == (int)ProtoMsg.TaskType.TtOther)
            {
                continue;
            }

            foreach (var it in item.Value.RunningData)
            {
                taskInfos.Add(it.Value);
            }
        }

        taskInfos.Sort(SortTask);
        return taskInfos;
    }

    public TaskTarget GetTaskTarget(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                return ServerTaskDatas[taskType].GetTaskTarget(taskID);
            }
        }

        return null;
    }
    public string GetTaskTypeIcon(TaskClassifyType taskType)
    {
        if (TaskConfigs.typelist.TryGetValue((uint)taskType, out var typeInfo))
        {
            return typeInfo.IconPath;
        }

        return string.Empty;
    }
    public int GetTaskRunningEventID(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                return ServerTaskDatas[taskType].GetTaskRunningEventID(taskID);
            }
        }

        return 0;
    }

    public void RemovekRunningTask(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            int taskType = (int)taskDataCell.Base.TaskType;
            if (ServerTaskDatas.ContainsKey(taskType))
            {
                ServerTaskDatas[taskType].RemovekRunningTask(taskID);
            }
        }
    }

    public TaskConfigInfo GetTaskConfig(uint taskID)
    {
        if (TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo info))
        {
            return info;
        }

        return null;
    }

    public static string GetHUDTaskIconNameByType(TaskClassifyType taskType, TaskNpc.StateEnum stateEnum)
    {
        int type = GetHUDTaskIconIndex(taskType, stateEnum);
        return GetHUDTaskIconNameByType(type);
    }

    private static int GetHUDTaskIconIndex(TaskClassifyType taskType, TaskNpc.StateEnum stateEnum)
    {
        int index = (int)taskType * 1000 + (int)stateEnum;
        return index;
    }

    public static string GetHUDTaskIconNameByType(int type, string mapLogo = "")
    {
        string npcTypeIconPath = string.Empty;
        switch (type)
        {
            // 主线
            case 1002:
                {
                    // 任务进行中 主线 Icon_Task_01 日常 Icon_Task_Daily_01 支线 Icon_Task_Subline_01
                    npcTypeIconPath = "Icon_Task_01";
                }
                break;
            case 1003:
                {
                    // 任务可以接取 主线 Icon_Submit_01 日常 Icon_Task_Daily_02 支线 Icon_Task_Subline_02
                    npcTypeIconPath = "Icon_Submit_01";
                }
                break;
            case 1004:
                {
                    // 任务可以提交 主线 Icon_Receive_01 日常 Icon_Task_Daily_03 支线 Icon_Task_Subline_03
                    npcTypeIconPath = "Icon_Receive_01";
                }
                break;
            // ----------------------------支线
            case 2002:
                {
                    // 任务进行中 主线 Icon_Task_01 日常 Icon_Task_Daily_01 支线 Icon_Task_Subline_01
                    npcTypeIconPath = "Icon_Task_Subline_01";
                }
                break;
            case 2003:
                {
                    // 任务可以接取 主线 Icon_Submit_01 日常 Icon_Task_Daily_02 支线 Icon_Task_Subline_02
                    npcTypeIconPath = "Icon_Task_Subline_02";
                }
                break;
            case 2004:
                {
                    // 任务可以提交 主线 Icon_Receive_01 日常 Icon_Task_Daily_03 支线 Icon_Task_Subline_03
                    npcTypeIconPath = "Icon_Task_Subline_03";
                }
                break;
            // ----------------------------日常
            case 4002:
                {
                    // 任务进行中 主线 Icon_Task_01 日常 Icon_Task_Daily_01 支线 Icon_Task_Daily_01
                    npcTypeIconPath = "Icon_Task_Daily_01";
                }
                break;
            case 4003:
                {
                    // 任务可以接取 主线 Icon_Submit_01 日常 Icon_Task_Daily_02 支线 Icon_Task_Daily_02
                    npcTypeIconPath = "Icon_Task_Daily_02";
                }
                break;
            case 4004:
                {
                    // 任务可以提交 主线 Icon_Receive_01 日常 Icon_Task_Daily_03 支线 Icon_Task_Subline_03
                    npcTypeIconPath = "Icon_Task_Daily_03";
                }
                break;
            case 7002:
                {
                    // 任务进行中 主线 Icon_Task_01 日常 Icon_Task_Daily_01 支线 Icon_Task_Daily_01
                    npcTypeIconPath = "Icon_Task_Challenge_01";
                }
                break;
            case 7003:
                {
                    // 任务可以接取 主线 Icon_Submit_01 日常 Icon_Task_Daily_02 支线 Icon_Task_Daily_02
                    npcTypeIconPath = "Icon_Task_Challenge_02";
                }
                break;
            case 7004:
                {
                    // 任务可以提交 主线 Icon_Receive_01 日常 Icon_Task_Daily_03 支线 Icon_Task_Subline_03
                    npcTypeIconPath = "Icon_Task_Challenge_03";
                }
                break;
            /// ------------ 任务的状态

            /// --------------- NPC功能标识
            case 1:
                {
                    // 功能
                    if (string.IsNullOrEmpty(mapLogo))
                    {
                        npcTypeIconPath = "Icon_Sell_01";
                    }
                    else
                    {
                        npcTypeIconPath = mapLogo;
                    }
                }
                break;
            case 2:
                {
                    // 玩法
                    if (string.IsNullOrEmpty(mapLogo))
                    {
                        npcTypeIconPath = "Hud_Btn_PlayMode";
                    }
                    else
                    {
                        npcTypeIconPath = mapLogo;
                    }
                }
                break;
            // 玩法 Hud_Btn_PlayMode
            default:
                break;
        }
        return npcTypeIconPath;
    }

    public static string GetUITaskIconNameByType(TaskClassifyType taskType, TaskStateEnum taskState)
    {
        int type = GetUITaskIconIndex(taskType, taskState);
        return GetUITaskIconNameByType(type);
    }

    private static int GetUITaskIconIndex(TaskClassifyType taskType, TaskStateEnum taskState)
    {
        int index = 0;

        switch (taskType)
        {
            case TaskClassifyType.MainLine:
                index = 1000;
                break;
            case TaskClassifyType.SubbranchLine:
                index = 2000;
                break;
            case TaskClassifyType.Biography:
                index = 4000;
                break;
            case TaskClassifyType.Play:
                index = 4000;
                break;
            case TaskClassifyType.Guide:
                index = 4000;
                break;
            case TaskClassifyType.Other:
                index = 4000;
                break;
            case TaskClassifyType.Challenge:
                index = 7000;
                break;
            default:
                break;
        }
        switch (taskState)
        {
            case TaskStateEnum.TaskStateNoGetReward:
                index += 3;
                break;
            case TaskStateEnum.TaskStateGetReward:
                break;
            case TaskStateEnum.TaskStageRunning:
                index += 2;
                break;
            case TaskStateEnum.TaskStateFailure:
                break;
            case TaskStateEnum.TaskStageFinished:
                index += 4;
                break;
            case TaskStateEnum.TaskStateBeDeleted:
                break;
            default:
                break;
        }

        return index;
    }

    private static string GetUITaskIconNameByType(int type)
    {
        string npcTypeIconPath = "Hud_Map_Task";
        switch (type)
        {
            // 主线
            case 1002:
                {
                    npcTypeIconPath = "Map_icon_16";
                }
                break;
            case 1003:
                {
                    npcTypeIconPath = "Map_task_icon_1";
                }
                break;
            case 1004:
                {
                    npcTypeIconPath = "Map_icon_12";
                }
                break;
            // ----------------------------支线
            case 2002:
                {
                    npcTypeIconPath = "Map_icon_17";
                }
                break;
            case 2003:
                {
                    npcTypeIconPath = "Map_task_icon_2";
                }
                break;
            case 2004:
                {
                    npcTypeIconPath = "Map_icon_13";
                }
                break;
            // ----------------------------日常
            case 4002:
                {
                    npcTypeIconPath = "Map_icon_18";
                }
                break;
            case 4003:
                {
                    npcTypeIconPath = "Map_task_icon_4";
                }
                break;
            case 4004:
                {
                    npcTypeIconPath = "Map_icon_14";
                }
                break;
            case 7002:
                {
                    npcTypeIconPath = "Icon_Map_Challenge_01";
                }
                break;
            case 7003:
                {
                    npcTypeIconPath = "Icon_Map_Challenge_02";
                }
                break;
            case 7004:
                {
                    npcTypeIconPath = "Icon_Map_Challenge_03";
                }
                break;
            /// ------------ 任务的状态

            /// --------------- NPC功能标识
            case 1:
                {
                    // 功能
                    npcTypeIconPath = "Icon_Sell_01";
                }
                break;
            case 2:
                {
                    // 玩法
                    npcTypeIconPath = "Hud_Btn_PlayMode";
                }
                break;
            // 玩法 Hud_Btn_PlayMode
            default:
                break;
        }
        return npcTypeIconPath;
    }



    /// <summary>
    /// 用于跨场景寻路
    /// </summary>
    public class CacheFindPath
    {
        public ulong ServerID;
        public int MapID;
        public long ConfigID;
        public E_FindPath e_FindPath;
        public float Range = 1;
        public UnityEngine.Vector3 Pos;
        public System.Action<bool> action = null;

        public bool IsNull
        {
            get { return MapID == 0 && ConfigID == 0 && Pos == UnityEngine.Vector3.zero; }
        }

        private void Reset()
        {
            SetData(0, 0, UnityEngine.Vector3.zero, E_FindPath.None, 0, null);
        }

        public void SetData(int mapID, long cfgid, UnityEngine.Vector3 pos, E_FindPath e_Find, float range, System.Action<bool> cb)
        {
            MapID = mapID;
            ConfigID = cfgid;
            Pos = pos;
            e_FindPath = e_Find;
            Range = range;
            action = cb;
        }

        public void DoFindPath()
        {
            if (IsNull)
            {
                return;
            }

            switch (e_FindPath)
            {
                case E_FindPath.Spawer:
                    {
                        TaskHelper.FindPathBySpawner(MapID, (int)ConfigID, Range, action);
                    }
                    break;
                case E_FindPath.NPC:
                    {
                        TaskHelper.FindPathByNpc(MapID, ConfigID, Range, action);
                    }
                    break;
                case E_FindPath.InterAction:
                    {
                        TaskHelper.FindPathByInterAction(MapID, ConfigID, Range, action);
                    }
                    break;
                case E_FindPath.Area:
                    {
                        TaskHelper.FindPathByArea(MapID, (int)ConfigID, Range, action);
                    }
                    break;
                case E_FindPath.Position:
                    TaskHelper.FindPathByPosition(Pos, Range, action);
                    break;
                case E_FindPath.Wanted:
                    TaskHelper.FindPathByWanted(GameManager.Instance.GetCurServerID(), (ulong)MapID, (int)ConfigID, Range, action);
                    break;
                case E_FindPath.ChanageMapPosition:
                    TaskHelper.FindPathByPosition(Pos, Range, action, true);
                    break;
                case E_FindPath.None:
                    Debug.LogError("不支持寻路");
                    break;
            }

            Reset();
        }
    }
}
