using Frame;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections.Generic;
using Task;

///--------------------------------------------------------------------
/// 文件名   :   ClassifyTask.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:40:19
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
[XLua.LuaCallCSharp]
public class ClassifyTask
{
    /// <summary>
    /// 当前任务列表
    /// </summary>
    public Dictionary<uint, TaskInfo> RunningData { get; private set; }


    /// <summary>
    /// 已经完成的任务数据
    /// </summary>
    public TaskBinary FinishedData;

    public TaskTarget GetTaskTarget(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                return task.GetTaskTarget();
            }
        }

        return null;
    }

    public int GetTaskRunningEventID(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                return task.GetTaskRunningEventID();
            }
        }

        return 0;
    }

    public bool IsFinishTaskEvent(uint taskID, int eventID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                return task.IsFinishTaskEvent(eventID);
            }
        }

        return false;
    }

    public bool TaskIsCommit(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                return task.TaskState == TaskStateEnum.TaskStageFinished;
            }
        }

        return false;
    }

    public TaskInfo GetTask(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                return task;
            }
        }

        return null;
    }

    public bool ContainTask(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.TryGetValue(taskID, out var task) && task != null)
            {
                bool delete = (task.TaskState == ProtoMsg.TaskStateEnum.TaskStateBeDeleted ||
                               task.TaskState == ProtoMsg.TaskStateEnum.TaskStateGetReward);
                return !delete;
            }
        }

        return false;
    }

    public bool ContainKey(int chainID, int index)
    {
        if (FinishedData == null || FinishedData.Data == null || chainID >= FinishedData.Data.Count)
        {
            SGF.Debuger.LogError($"任务错误 chainID={chainID} 大于了数组长度 err!!!");
            return false;
        }

        ulong data = FinishedData.Data[chainID];
        var result = ((data >> index) & 1UL) == 1;
        return result;
    }

    public void UpData(int chainID, int index, byte val)
    {
        ulong data = FinishedData.Data[chainID];
        if (val == 1)
            data |= 1UL << index;
        else
            data &= ~(1UL << index);
        FinishedData.Data[chainID] = data;
    }


    public ClassifyTask(TaskingModel singleTask, TaskBinary finishedData)
    {
        FinishedData = finishedData;
        RunningData = new Dictionary<uint, TaskInfo>();
        RunningData.Clear();

        if (singleTask.Tasks != null && singleTask.Tasks.Count > 0)
        {
            foreach (var item in singleTask.Tasks)
            {
                CreateTask(item.Value);
            }
        }
    }

    public void CreateTask(TaskPropgress propgress)
    {
        TaskInfo taskInfo = new TaskInfo(propgress);
        if (!RunningData.ContainsKey(taskInfo.TaskID))
        {
            RunningData.Add(taskInfo.TaskID, taskInfo);
        }
        else
        {
            RunningData[taskInfo.TaskID] = taskInfo;
        }
    }

    public void RemovekRunningTask(uint taskID)
    {
        if (RunningData != null)
        {
            if (RunningData.ContainsKey(taskID))
            {
                var TaskState = RunningData[taskID].TaskState;
                bool delete = (TaskState == TaskStateEnum.TaskStateBeDeleted ||
                               TaskState == TaskStateEnum.TaskStateGetReward);
                if (delete)
                {
                    RunningData.Remove(taskID);
                    //ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.UPDATETASKDATA, TaskHelper.GetAllTask());
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, TaskHelper.FRESHTASK);
                    TrrigerNpcFresh(taskID);
                }
            }
        }
    }

    public void ModifyTask(TaskPropgress taskPropgress)
    {
        bool delete = (taskPropgress.State == TaskStateEnum.TaskStateBeDeleted ||
                       taskPropgress.State == TaskStateEnum.TaskStateGetReward);
        StarDebug.Log(StarDebug.Orange, $"TaskID={taskPropgress.TaskID} State={taskPropgress.State} delete={delete}");

        if (delete)
        {
            if (TaskManager.TaskConfigs.list.TryGetValue((uint)taskPropgress.TaskID, out TaskConfigInfo info))
            {
                int chainID = (int)info.Base.ChainID;
                int saveID = (int)info.Base.UIndex;

                UpData(chainID, saveID, 1);
                if (RunningData.ContainsKey((uint)taskPropgress.TaskID))
                {
                    RunningData[(uint)taskPropgress.TaskID].TaskState = taskPropgress.State;
                }

                //延迟1秒 删除留个表现层
                //DelayInvoker.DelayInvoke(this,0.1f, (args) =>
                //{
                //    RunningData.Remove(taskPropgress.TaskID);
                //    ModuleManager.Instance.SendMessage(ModuleDef.TaskModule, TaskHelper.UPDATETASKDATA, TaskHelper.GetAllTask());
                //    ModuleManager.Instance.SendMessage(ModuleDef.TaskModule, TaskHelper.FRESHTASK);
                //}, null);

                //if (LocalDataManager.Instance.M_taskData.StaticTaskDatas.TryGetValue(taskPropgress.TaskID, out TaskDataCell taskDataCell))
                //{
                //    int saveID = taskPropgress.TaskID % 10000;

                //    UpData(saveID, 1);
                //}A
            }

            //任务完成
            GlobalEvent.TaskStageChange.Invoke(3, taskPropgress.TaskID);
            TrrigerNpcFresh((uint)taskPropgress.TaskID);
            //SoundManager.Instance.PlayWwiseAudio("UI_Skill_End_Sound", false, E_SoundNTFtype.MyListener_SystemSound);
            SoundManager.Instance.PlayEventName("UI_System_Task_Completed_01", null, null);


            //环任务领取奖励
            /*
            var allRingTasks = BusinessManager.Instance.GetAllRingTaskIDs();
            if (allRingTasks != null && allRingTasks.Count > 0 &&
                taskPropgress.TaskID == allRingTasks[allRingTasks.Count - 1])
            {
                BusinessManager.Instance.RingTaskExReq();
            }
           
            //触发环任务完成
            var allRingTasks = BusinessManager.Instance.GetAllRingTaskIDs();
            if (allRingTasks != null && allRingTasks.Count > 0)
            {
                for(int i=0;i< allRingTasks.Count;i++)
                {
                    if(taskPropgress.TaskID == allRingTasks[i])
                    {
                        //GameManager.Instance.GetRingTaskModule().OnFinishRingTask( i + 1 );
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.RingTaskModule, "OnFinishRingTask",  i + 1 );
                    }
                }
            }
            */

        }
        else
        {
            TaskInfo taskInfo = new TaskInfo(taskPropgress); //这里不用每次都New一个  同一个任务只需要赋值就可以了

            if (!RunningData.ContainsKey(taskInfo.TaskID))
            {
                StarDebug.Log(StarDebug.Orange, $"新增 TaskID={taskPropgress.TaskID} State={taskPropgress.State} ");


                //新增
                RunningData.Add(taskInfo.TaskID, taskInfo);
                if (TaskManager.TaskConfigs.list.TryGetValue((uint)taskPropgress.TaskID, out TaskConfigInfo info))
                {
                    int chainID = (int)info.Base.ChainID;
                    int saveID = (int)info.Base.UIndex;
                    if (ContainKey(chainID, saveID))
                    {
                        UpData(chainID, saveID, 0);
                    }
                }

                GlobalEvent.TaskStageChange.Invoke(1, taskPropgress.TaskID);
                TrrigerNpcFresh((uint)taskPropgress.TaskID);


                var allRingTasks = BusinessManager.Instance.GetAllRingTaskIDs();
                //领取了第一个环任务
                if (allRingTasks != null && allRingTasks.Count > 0 && taskInfo.TaskID == allRingTasks[0])
                {
                    //ModuleManager.Instance.SendMessage(ModuleDef.Name.RingTaskModule, "OnCkeckAndOpenRingTaskWindow", new object[] { });
                    //GameManager.Instance.GetRingTaskModule().OnAcceptRingTask(null);
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.RingTaskModule, "OnAcceptRingTask", 0);
                }
            }
            else
            {
                //判断如果进度发生变化  并且任务没有结束 弹tips
                CheckTaskProcessChanged(taskInfo, RunningData[taskInfo.TaskID]);

                StarDebug.Log(StarDebug.Orange, $"更新 TaskID={taskPropgress.TaskID} State={taskPropgress.State} ");
                //更新
                RunningData[taskInfo.TaskID] = taskInfo;

                if (taskInfo.TaskState == TaskStateEnum.TaskStateFailure ||
                    taskInfo.TaskState == TaskStateEnum.TaskStageFinished)
                {
                    //任务失败
                    GlobalEvent.TaskStageChange.Invoke(2, taskPropgress.TaskID);
                    TrrigerNpcFresh((uint)taskPropgress.TaskID);
                }
                else
                {
                    GlobalEvent.TaskStageChange.Invoke(4, taskPropgress.TaskID);
                }
            }
        }
    }

    private void CheckTaskProcessChanged(TaskInfo taskInfo, TaskInfo preTaskInfo)
    {
        if (taskInfo == null || preTaskInfo == null) return;

        if (taskInfo.TaskType != (int)ProtoMsg.TaskType.TtOther && taskInfo.ShowProcess && (taskInfo.TaskState == TaskStateEnum.TaskStageFinished ||
                                     taskInfo.TaskState == TaskStateEnum.TaskStageRunning))
        {
            foreach (var curTarget in taskInfo.TaskTarget)
            {
                foreach (var preTarget in preTaskInfo.TaskTarget)
                {
                    if (curTarget.TaskEventID == preTarget.TaskEventID)
                    {
                        if (curTarget.CurProgress > preTarget.CurProgress)
                        {
                            string des;
                            if (taskInfo.IsSpecialDesc())
                            {
                                des = taskInfo.SpecTaskDesc + taskInfo.GetTotalProcessDesc().Item1;
                            }
                            else
                            {
                                des = curTarget.TaskDesc;
                            }

                            if (taskInfo.TaskState == TaskStateEnum.TaskStageRunning ||
                                taskInfo.TaskState == TaskStateEnum.TaskStageFinished && !taskInfo.AutoFinish &&
                                taskInfo.FinishSpawner.Count > 0)
                            {
                                Util.ShowMessage(des, 2);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }

    private void TrrigerNpcFresh(uint taskID)
    {
        List<uint> npcids = new List<uint>();
        List<uint> inters = new List<uint>();
        if (TaskManager.TaskConfigs.list.TryGetValue(taskID, out TaskConfigInfo taskDataCell))
        {
            if (taskDataCell.TaskFinishInfo.FinishTaskNPC != 0)
            {
                npcids.Add((uint)taskDataCell.TaskFinishInfo.FinishTaskNPC);
            }

            if (taskDataCell.Condition.PickTaskNPC != 0)
            {
                npcids.Add((uint)taskDataCell.Condition.PickTaskNPC);
            }

            if (taskDataCell.TaskFinishInfo.Targets != null && taskDataCell.TaskFinishInfo.Targets.Count > 0)
            {
                foreach (var item in taskDataCell.TaskFinishInfo.Targets)
                {
                    if (item.Target.TargetType == EffectTargetType.NPC)
                    {
                        npcids.Add((uint)item.Target.TargetID);
                    }
                    else if (item.Target.TargetType == EffectTargetType.InterAction)
                    {
                        inters.Add((uint)item.Target.TargetID);
                    }
                }
            }
        }

        GameManager.Instance.UpdateNpcServiceState(StarProject.Service.ServerService.ServerServiceType.NPC, npcids);
        GameManager.Instance.UpdateNpcServiceState(StarProject.Service.ServerService.ServerServiceType.InterAction,
            inters);
    }

    public List<TaskInfo> GetTasks(TaskClassifyType taskType)
    {
        List<TaskInfo> taskInfos = new List<TaskInfo>();
        if (RunningData != null && RunningData.Count > 0)
        {
            foreach (var item in RunningData)
            {
                if (item.Value.TaskType == (int)taskType)
                {
                    taskInfos.Add(item.Value);
                }
            }
        }

        return taskInfos;
    }
}