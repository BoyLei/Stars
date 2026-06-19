///--------------------------------------------------------------------
/// 文件名   :   TaskNpcManager.cs
/// 内  容   :   任务NPC管理器
/// 说  明   :  
/// 创建日期 :   2023/01/03 09:44:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject;
using System;
using System.Collections.Generic;

public class TaskNpcManager
{
    //Npc
    private Dictionary<long, List<TaskNpc>> Datas = null;
    
    //交互物
    private Dictionary<long, List<TaskNpc>> InterDatas = null;
    public void Init()
    {
        Datas = new Dictionary<long, List<TaskNpc>>();
        InterDatas = new Dictionary<long, List<TaskNpc>>();
        
       // GlobalEvent.TaskStageChange.AddListener(OnTaskChange);
    }

    /*private void OnTaskChange(int type, int taskID)
    {
        UpdataAll();
        //Update(taskID);
    }*/

    public void Release()
    {
        //GlobalEvent.TaskStageChange.RemoveListener(OnTaskChange);
        Clear();
        Datas = null;
        InterDatas = null;
    }

    public bool QueryNpcTaskState(int npcID)
    {
        if (Datas != null && Datas.ContainsKey(npcID))
        {
            var list = Datas[npcID];
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (item.State)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 获取NPC的任务状态
    /// 已完成 > 任务进行中 任务目标已完成 > 任务进行中 任务目标未完成
    /// </summary>
    /// <param name="npcID"></param>
    /// <returns></returns>
    public TaskNpc.StateEnum QueryNpcTaskEnum(int npcID, ref uint taskID)
    {
        TaskNpc.StateEnum stateE = TaskNpc.StateEnum.None;
        if (Datas != null && Datas.ContainsKey(npcID))
        {
            var list = Datas[npcID];
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    if ((int)item.StateE > (int)stateE)
                    {
                        stateE = item.StateE;
                        taskID = item.TaskID;
                        if (stateE == TaskNpc.StateEnum.Commit)
                        {
                            return stateE;
                        }
                    }
                }
            }
        }
        return stateE;
    }

    public TaskNpc.StateEnum QueryInterTaskEnum(int configID, ref uint taskID)
    {
        TaskNpc.StateEnum stateE = TaskNpc.StateEnum.None;
        if (InterDatas != null && InterDatas.ContainsKey(configID))
        {
            var list = InterDatas[configID];
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    if ((int)item.StateE > (int)stateE)
                    {
                        stateE = item.StateE;
                        taskID = item.TaskID;
                        if (stateE == TaskNpc.StateEnum.Commit)
                        {
                            return stateE;
                        }
                    }
                }
            }
        }
        return stateE;
    }

    public void UpdataAll()
    {
        if (Datas != null)
        {
            foreach (var item in Datas)
            {
                foreach (var it in item.Value)
                {
                    if (it.Update(it.TaskID))
                    {
                       GlobalEvent.OnTaskNpcChange?.Invoke(item.Key, it.TaskID, it.TaskType, it.StateE);
                    }
                }
            }

        }

        if (InterDatas != null)
        {
            foreach (var item in InterDatas)
            {
                foreach (var it in item.Value)
                {
                    if (it.Update(it.TaskID))
                    {
                        GlobalEvent.OnTaskInterChange?.Invoke(item.Key, it.TaskID, it.TaskType, it.StateE);
                    }
                }
            } 
        }
        
        GlobalEvent.OnNpcInterStageChange.Invoke(1);
        
        //广播
        //重置
        
        if (Datas != null)
        {
            foreach (var item in Datas)
            {
                foreach (var it in item.Value)
                {
                    it.ResetDirty();
                }
            }

        }

        if (InterDatas != null)
        {
            foreach (var item in InterDatas)
            {
                foreach (var it in item.Value)
                {
                    it.ResetDirty();
                }
            } 
        }


    }
    private void UpdateInter(uint InterID)
    {
        if (Datas != null)
        {
            foreach (var item in InterDatas)
            {
                foreach (var it in item.Value)
                {
                    if (it.Update(InterID))
                    {
                       // GlobalEvent.OnTaskNpcChange?.Invoke(item.Key, it.TaskType, it.State, it.StateE);
                    }
                }
            }
        }

    }

    private void UpdateNpc(uint taskID)
    {
        if (Datas != null)
        {
            foreach (var item in Datas)
            {
                foreach (var it in item.Value)
                {
                    if (it.Update(taskID))
                    {
                        GlobalEvent.OnTaskNpcChange?.Invoke(item.Key, it.TaskID, it.TaskType, it.StateE);
                    }
                }
            }
        }

    }

    public List<TaskNpc> GetNpcTasks(int npcID)
    {
        if (Datas.TryGetValue(npcID,out var  result) && result != null)
        {
            return result;
        }

        return null;
    }
    public List<TaskNpc> GetInterTasks(int npcID)
    {
        if (InterDatas.TryGetValue(npcID,out var  result) && result != null)
        {
            return result;
        }

        return null;
    }
    
    
    Dictionary<long, List<TaskNpc>> taskNpcDis = new Dictionary<long, List<TaskNpc>>();
    public Dictionary<long, List<TaskNpc>> GetCanReceiveNpcTask()
    {
        taskNpcDis.Clear();
        foreach (var item in Datas)
        {
            foreach (var it in item.Value)
            {
                if (it.StateE == TaskNpc.StateEnum.Accept)
                {
                    if (taskNpcDis.ContainsKey(item.Key))
                    {
                        taskNpcDis[item.Key].Add(it);
                    }
                    else
                    {
                        List<TaskNpc> taskNpcs = new List<TaskNpc>();
                        taskNpcs.Add(it);
                        taskNpcDis.Add(item.Key, taskNpcs);
                    }
                }
            }
        }

        return taskNpcDis;
    }
    
    Dictionary<long, List<TaskNpc>> taskInterDis = new Dictionary<long, List<TaskNpc>>();
    public Dictionary<long, List<TaskNpc>> GetCanReceiveInterTask()
    {
        taskInterDis.Clear();
        foreach (var item in InterDatas)
        {
            foreach (var it in item.Value)
            {
                if (it.StateE == TaskNpc.StateEnum.Accept)
                {
                    if (taskInterDis.ContainsKey(item.Key))
                    {
                        taskInterDis[item.Key].Add(it);
                    }
                    else
                    {
                        List<TaskNpc> taskNpcs = new List<TaskNpc>();
                        taskNpcs.Add(it);
                        taskInterDis.Add(item.Key, taskNpcs);
                    }
                }
            }
        }

        return taskInterDis;
    }


    public void RegisterInter(long interID, TaskNpc taskNpc)
    {
        if (InterDatas.ContainsKey(interID))
        {
            InterDatas[interID].Add(taskNpc);
        }
        else
        {
            List<TaskNpc> taskNpcs = new List<TaskNpc>();
            taskNpcs.Add(taskNpc);
            InterDatas.Add(interID, taskNpcs);
        }
        GlobalEvent.OnTaskInterChange?.Invoke(interID, taskNpc.TaskID, taskNpc.TaskType, taskNpc.StateE);
    }
    
    public void RegisterNpc(long npcID, TaskNpc taskNpc)
    {
        if (Datas.ContainsKey(npcID))
        {
            Datas[npcID].Add(taskNpc);
        }
        else
        {
            List<TaskNpc> taskNpcs = new List<TaskNpc>();
            taskNpcs.Add(taskNpc);
            Datas.Add(npcID, taskNpcs);
        }

        GlobalEvent.OnTaskNpcChange?.Invoke(npcID, taskNpc.TaskID, taskNpc.TaskType, taskNpc.StateE);
    }

    public void Clear()
    {
        if (Datas != null)
        {
            Datas.Clear();
        }

        if (InterDatas != null)
        {
            InterDatas.Clear();
        }
    }

}

public class TaskNpc
{
    //任务ID
    public uint TaskID;

    //事件ID
    public int TaskEventID;

    //任务类型
    public int TaskType;

    //来源
    public TaskNPCSource taskNPCSource;

    //条件ID
    public int ConditionID;

    public bool IsDirty;

    public void ResetDirty()
    {
        IsDirty = false;
    }
    
    
    public bool State { get { return StateE == StateEnum.Runnig; } }
    private StateEnum stateE;

    public StateEnum StateE
    {
        get{return stateE;}
        set
        {
            if (stateE!= value)
            {
                stateE = value;
                IsDirty = true;
            }
        }
    }

    public bool Update(uint taskID)
    {
        if (TaskID == taskID)
        {
            if (TaskHelper.ContainTask(taskID))
            {
                //任务是否状态
                var task = TaskHelper.GetRunningTask(taskID);

                //已完成未领奖
                if (task.TaskState == ProtoMsg.TaskStateEnum.TaskStageFinished)
                {
                    
                        if (taskNPCSource == TaskNPCSource.TaskEvent)
                        {
                            if (TaskHelper.IsFinishTaskEvent(taskID, TaskEventID))
                            {
                                StateE = StateEnum.Finish;
                            }
                            else
                            {
                                StateE = StateEnum.Runnig;
                            }
                            
                        }
                        else
                        {
                            StateE = StateEnum.Commit;
                        }
                    
                }
                //任务进行中
                else if (task.TaskState == ProtoMsg.TaskStateEnum.TaskStageRunning)
                {
                    if (taskNPCSource == TaskNPCSource.TaskEvent)
                    {
                        if (TaskHelper.IsFinishTaskEvent(taskID, TaskEventID))
                        {
                            StateE = StateEnum.Finish;
                        }
                        else
                        {
                            StateE = StateEnum.Runnig;
                        }
                            
                    }
                    else
                    {
                        StateE = StateEnum.Runnig;
                    }
    

                }
                else
                {
  
                    StateE = StateEnum.Finish;
                    //其他状态算完成  
                }
            }
            else if (TaskHelper.IsTaskFinsh(taskID))
            {
                StateE = StateEnum.Finish;
            }
            else
            {
                StateE = StateEnum.None;
                if (taskNPCSource==TaskNPCSource.Accept)
                {
                    if (StarProject.Game.GameManager.Instance.IsConditionMete(ConditionID))
                    {
                        StateE = StateEnum.Accept;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public enum StateEnum
    {
        None = 0,//任务不存在
        Finish = 1, //任务已完成
        Runnig = 2,   //任务进行中
        Accept =3,  //任务可以接取
        Commit=4,   //任务可以提交
    }

    public enum TaskNPCSource
    {
        Accept=1,       //接取任务
        Commit=2,       //提交任务
        TaskEvent=3,    //任务目标
    }
}
