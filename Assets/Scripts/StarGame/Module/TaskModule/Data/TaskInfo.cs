///--------------------------------------------------------------------
/// 文件名   :   TaskInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:31:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using ProtoMsg;
using System.Collections.Generic;
using StarProject.Service.LocalData;
using Task;
using UnityEngine;
using StarProjectDef;
using StarProject.Service.Language;

[XLua.LuaCallCSharp]
public class TaskInfo
{
    //服务器数据
    public uint TaskID;
    public TaskStateEnum TaskState;
    public long TaskOverdueTime;
    public Dictionary<int, long> TaskTargetProcess;

    //配置数据
    public int TaskType;
    public string TaskTitle;
    public string TaskDesc;
    public string TaskTag;
    public int Chapter;
    public int ShowItemID;
    public int DropID;
    public List<TaskTarget> TaskTarget;
    public List<long> PickSpawner; // 承接NPC
    public List<long> FinishSpawner; //交付NPC
    public bool AutoFinish;
    public bool ClickFinish;
    public string FinishServiceName;

    public string TaskTargetDesc;
    public string ChapterName;
    public bool ShowProcess = true;
    public string SpecTaskDesc;
    public int GroupID;
    //public int GuideID;
    //public int JumpID;
    public Dictionary<long, long> Awards;
    public Dictionary<int, int> RewardMarks;

    public TaskInfo()
    {
    }

    public TaskInfo(TaskPropgress taskPropgress)
    {
        TaskTarget = new List<TaskTarget>();
        TaskTargetProcess = new Dictionary<int, long>();
        PickSpawner = new List<long>();
        FinishSpawner = new List<long>();
        Awards = new Dictionary<long, long>();
        RewardMarks = new Dictionary<int, int>();

        UpdateTask(taskPropgress);
        if (TaskManager.TaskConfigs != null && TaskManager.TaskConfigs.list.TryGetValue((uint)taskPropgress.TaskID, out TaskConfigInfo info))
        {
            TaskType = (int)info.Base.TaskType;
            TaskTitle = info.Base.TaskName;
            TaskDesc = info.Base.Desc;
            TaskTag = "";
            ShowItemID = 0;
            DropID = info.Base.AwardID;
            Chapter = info.Base.Chapter;
            AutoFinish = info.TaskFinishInfo.AutoFinish;
            ClickFinish = info.TaskFinishInfo.ClickFinish;
            ShowProcess = info.Base.ShowProcess;
            SpecTaskDesc = info.Base.SpecTaskDesc;
            //GuideID=info.Base.GuideID;
            GroupID = info.Base.GroupID;
           // JumpID=info.Base.JumpID;
            var taskReward = LocalDataManager.Instance.GetTaskRewardDataCell(taskPropgress.TaskID);
            if (taskReward != null)
            {
                TaskTag = taskReward.Tips;

                var tempRewardMarks= taskReward.RewardMark;

                for(int i=0;i< tempRewardMarks.Count;i++)
                {
                    if (i % 2 == 1)
                        continue;

                    RewardMarks.Add(tempRewardMarks[i], tempRewardMarks[i+1]);
                }

                //ShowItemID = taskReward.GetRewardMark();
                ShowItemID = 0;
                ResetShowItemID();


                int index = 0;
                if (taskReward.Reward != null && taskReward.Reward.Count > 0)
                {
                    if (taskReward.Reward.Count % 2 != 0)
                    {
                        Debug.LogError($"{taskPropgress.TaskID}奖励配置错误");
                    }
                    
                    while (index < taskReward.Reward.Count - 1)
                    {
                        long id = taskReward.Reward[index++];
                        long num = taskReward.Reward[index++];
                        if (Awards.ContainsKey(id))
                        {
                            Awards[id] += num;
                        }
                        else
                        {
                            Awards.Add(id, num);
                        }
                    }
                }
                if (ShowItemID != 0)
                {
                    Awards[ShowItemID] = 1;
                }
                if (taskReward.RewardHid != null && taskReward.RewardHid.Count > 0)
                {
                    index = 0;
                    while (index < taskReward.RewardHid.Count - 1)
                    {
                        long id = taskReward.RewardHid[index++];
                        long num = taskReward.RewardHid[index++];
                        if (Awards.ContainsKey(id))
                        {
                            Awards[id] += num;
                        }
                        else
                        {
                            Awards.Add(id, num);
                        }
                    }
                }
            }

            if (!AutoFinish)
            {
                FinishServiceName = TaskTitle;
                if (info.TaskFinishInfo.FinishEffects != null && info.TaskFinishInfo.FinishEffects.Count > 0)
                {
                    FinishServiceName = info.TaskFinishInfo.FinishEffects[0].ShowText;
                    if (!String.IsNullOrEmpty(FinishServiceName))
                    {
                        FinishServiceName = TaskHelper.WildCardStr(FinishServiceName, info.TaskFinishInfo.FinishTaskNPC);
                    }
                }
            }

            ChapterName = info.Base.ChapterName;
            PickSpawner.Add(info.Condition.PickTaskMapID);
            PickSpawner.Add(info.Condition.PickTaskNPC);
            FinishSpawner.Add(info.TaskFinishInfo.FinishTaskMapID);
            FinishSpawner.Add(info.TaskFinishInfo.FinishTaskNPC);
            if (string.IsNullOrEmpty(ChapterName))
            {
                //ChapterName = GameConfig.LocalStr["TaskNameEmpty"];
                ChapterName = LanguageManager.Instance.GetLanguageByKey("TaskNameEmpty");
            }
        }
        else
        {
            SGF.Debuger.LogError($"任务配置找不到 id={TaskID}");
            TaskTitle = $"配置不存在{TaskID}";
            TaskDesc = "配置不存在";
        }
    }

    public void ResetShowItemID()
    {
        //如果是全职业就不用读取其他职业的
        if(RewardMarks.ContainsKey(0))
        {
            ShowItemID = RewardMarks[0];
            return;
        }


        //如果找到职业的就返回职业的
        int job=(int)StarProject.Game.GameManager.Instance.GetPlayJobBaseID();
        if(RewardMarks.ContainsKey(job))
        {
            ShowItemID = RewardMarks[job];
            return;
        }

        //什么都没找到就不显示
        ShowItemID = 0;
    }

    public Color GetStateColor(bool finish)
    {
        Color color = Color.white;
        if (finish)
        {
            ColorUtility.TryParseHtmlString("#A88946", out color);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#C0B6A0", out color);
        }

        return color;
    }

    public bool IsSpecialDesc()
    {
        return !string.IsNullOrEmpty(SpecTaskDesc);
    }


    public (string, bool) GetTotalProcessDesc()
    {
        int cur = 0;
        int total = 0;
        if (TaskTarget != null && TaskTarget.Count > 0)
        {
            foreach (var item in TaskTarget)
            {
                if (item.ShowProcess)
                {
                    cur += ((int)item.CurProgress);
                    total += item.MaxCount;
                }
            }
            return ($"({cur}/{total})", cur >= total);
        }
        else
        {
            return ($"", cur >= total); ;
        }
    }

    public TaskTarget GetTaskTarget()
    {
        if (TaskTarget != null && TaskTarget.Count > 0)
        {
            foreach (var item in TaskTarget)
            {
                if (!item.IsFinish())
                {
                    return item;
                }
            }
        }

        return null;
    }

    public int GetTaskRunningEventID()
    {
        if (TaskTarget != null && TaskTarget.Count > 0)
        {
            foreach (var item in TaskTarget)
            {
                if (!item.IsFinish())
                {
                    return item.TaskEventID;
                }
            }
        }

        return 0;
    }

    public bool IsFinishTaskEvent(int eventID)
    {
        if (TaskTarget != null && TaskTarget.Count > 0)
        {
            foreach (var item in TaskTarget)
            {
                if (item.TaskEventID == eventID)
                {
                    return item.IsFinish();
                }
            }
        }

        return false;
    }

    public void UpdateTask(TaskPropgress taskPropgress)
    {
        TaskID = (uint)taskPropgress.TaskID;
        TaskState = taskPropgress.State;
        TaskOverdueTime = taskPropgress.OverdueTime;
        TaskTargetProcess.Clear();
        TaskTarget.Clear();
        TaskTargetDesc = string.Empty;
        if (taskPropgress.Propgress != null && taskPropgress.Propgress.Count > 0)
        {
            for (int i = 0; i < taskPropgress.Propgress.Count; i++)
            {
                var data = taskPropgress.Propgress[i];
                if (TaskTargetProcess.ContainsKey(data.Condition))
                {
                    TaskTargetProcess[data.Condition] = data.CurProgress;
                }
                else
                {
                    TaskTargetProcess.Add(data.Condition, data.CurProgress);
                }
            }
        }

        if (TaskManager.TaskConfigs != null && TaskManager.TaskConfigs.list != null &&
            TaskManager.TaskConfigs.list.TryGetValue((uint)taskPropgress.TaskID, out TaskConfigInfo info))
        {
            if (info != null)
            {
                foreach (var item in TaskTargetProcess)
                {
                    Task.TaskTarget target = info.TaskFinishInfo.GetTaskTarget(item.Key);
                    if (target != null)
                    {
                        TaskTarget taskTargets = new();
                        taskTargets.MapID = target.MapID;
                        taskTargets.TaskEventID = target.ID;
                        taskTargets.CurProgress = item.Value;
                        taskTargets.MaxCount = target.MaxNum;
                        taskTargets.FindPath = target.FindPath;
                        taskTargets.TargetID = target.TargetID;
                        taskTargets.taskType = target.TaskType.TaskType;
                        taskTargets.Pramas = target.TaskType.Pramas;
                        taskTargets.ShowProcess = target.ShowProcess;
                        if (taskTargets.ShowProcess)
                        {
                            int targetID = 0;
                            if (target.Desc.Contains(TaskHelper.WILDCARD_MON))
                            {
                                targetID = taskTargets.GetID();
                            }
                            else
                            {
                                targetID = (int)taskTargets.TargetID;
                            }
                            taskTargets.TaskDesc =
                                $"{TaskHelper.WildCardStr(target.Desc, targetID)}({taskTargets.CurProgress}/{taskTargets.MaxCount})";
                        }
                        else
                        {
                            taskTargets.TaskDesc = $"{TaskHelper.WildCardStr(target.Desc, taskTargets.TargetID)}";
                        }

                        TaskTarget.Add(taskTargets);
                    }
                }
            }
        }
    }


    public float GetHeight()
    {
        float Height = 0;
        if (IsSpecialDesc() && TaskTarget != null && TaskTarget.Count > 0)
        {
            foreach (var item in TaskTarget)
            {
                Height += item.Height;
                if (item.Height > 30)
                {
                    Height += +10;
                }
            }
        }
        else
        {
            Height += 30;
        }

        return Height;
    }


    public int GetCount()
    {
        int count = TaskTarget == null ? 0 : TaskTarget.Count;
        return count;
    }

    public string GetStringType()
    {
        return TaskHelper.GetTaskTypeName((TaskClassifyType)TaskType, ChapterName);
    }

    public string GetTaskTypeNoColor()
    {
        return TaskHelper.GetTaskTypeNameNoColor((TaskClassifyType)TaskType, ChapterName);
    }

    public string GetTaskProgress()
    {
        return TaskHelper.GetTaskProgressStr(TaskID,GroupID);
    }
    public string GetTaskTagets()
    {
        return TaskTargetDesc;
    }
}