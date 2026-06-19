///--------------------------------------------------------------------
/// 文件名   :   TaskConfigInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/15 21:10:37
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Text;
using MessagePack;

namespace Task
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskConfigInfo
    {
        [Newtonsoft.Json.JsonIgnore] [IgnoreMember] public System.Action<TaskClassifyType, uint> DeleteTaskCall;

        [Newtonsoft.Json.JsonIgnore] [IgnoreMember] public HasTaskDelegate ContinesTask;

        [Newtonsoft.Json.JsonIgnore] [IgnoreMember] [HideInInspector]
        public bool IsDirty = false;

        public void OnDirty()
        {
            IsDirty = true;
            Base?.TaskNameChange();
        }


        public TaskConfigInfo()
        {
            Base = new TaskBaseInfo();
            Condition = new TaskConditionInfo();
            TaskFinishInfo = new TaskFinishInfo();
        }

        [TabGroup("基础信息")] [ShowInInspector] [HideLabel] [OnValueChanged("OnDirty", IncludeChildren = true)] [Key(0)]
        public TaskBaseInfo Base;

        [TabGroup("承接任务")] [ShowInInspector] [HideLabel] [OnValueChanged("OnDirty", IncludeChildren = true)] [Key(1)]
        public TaskConditionInfo Condition;

        [TabGroup("交付任务")]
        [ShowInInspector]
        [HideLabel]
        [OnValueChanged("OnDirty", IncludeChildren = true)]
        [OnCollectionChanged("After")]
        [Key(3)]
        public TaskFinishInfo TaskFinishInfo;

        [Button("删除任务")]
        public void DeleteTask()
        {
            DeleteTaskCall?.Invoke(Base.TaskType, Base.TaskID);
        }

        [Button("修复ID")]
        public void ModifyTaskID()
        {
            uint taskType = (uint)Base.TaskType;
            Base.TaskID = (taskType << 28) | ((uint)Base.ChainID << 16) | (Base.UIndex << 0);
        }

        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public string TaskTile
        {
            get { return $"{Base.Index}_{Base.TaskID}_{Base.TaskName}"; }
        }

        public string GetTaskTypeName()
        {
            string typeName = "";
            switch (Base.TaskType)
            {
                case TaskClassifyType.MainLine:
                    typeName = $"{(int)Base.TaskType}.主线/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.SubbranchLine:
                    typeName = $"{(int)Base.TaskType}.支线/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.Biography:
                    typeName = $"{(int)Base.TaskType}.传记/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.Play:
                    typeName = $"{(int)Base.TaskType}.日常/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.Guide:
                    typeName = $"{(int)Base.TaskType}.引导/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.Other:
                    typeName = $"{(int)Base.TaskType}.其他/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
                case TaskClassifyType.Challenge:
                    typeName = $"{(int)Base.TaskType}.挑战/{Base.Chapter}.{Base.ChapterName}/{Base.ChainID}";
                    break;
            }

            return typeName;
        }

        public bool IsModuleEvent()
        {
            return (!string.IsNullOrEmpty(Base.ModuleName) && !string.IsNullOrEmpty(Base.EventName));
        }
#if UNITY_EDITOR
        /// <summary>
        /// 任务检查,
        /// </summary>
        /// <returns></returns>
        public string CheckTask()
        {
            if (Base == null)
            {
                Debug.LogWarning("Task Base is Null ");
                return string.Empty;
            }
            if (ContinesTask == null)
            {
                Debug.LogWarning("Task ContinesTask is Null ");
                return string.Empty;
            }
            StringBuilder message = new StringBuilder();
            //前置任务检查
            if (Base.PreTaskID != 0 && !ContinesTask.Invoke(Base.PreTaskID))
            {
                message.AppendLine($"{Base.TaskID} 前置任务 {Base.PreTaskID} 不存在 ");
            }

            //后置任务检查
            if (Base.NextTaskID != 0 && !ContinesTask.Invoke(Base.NextTaskID))
            {
                message.AppendLine($"{Base.TaskID} 后置任务 {Base.NextTaskID} 不存在 ");
            }
            if (Base.TaskType != TaskClassifyType.Challenge && Base.TaskType != TaskClassifyType.Other &&!TaskFinishInfo.AutoFinish && !TaskFinishInfo.ClickFinish&& TaskFinishInfo.FinishTaskMapID == 0&& TaskFinishInfo.FinishTaskNPC == 0) 
            {
                message.AppendLine($"{Base.TaskID} 交付配置都为false MapID NPCID为0 不合法 ");
            }

                //任务目标检查
            if (TaskFinishInfo.Targets == null || TaskFinishInfo.Targets.Count < 1)
            {
                message.AppendLine($"{Base.TaskID} 没有任务目标 ");
            }
            else
            {
                foreach (var item in TaskFinishInfo.Targets)
                {
                    if (item.Target.MapID < 1)
                    {
                        //message.AppendLine($"{Base.TaskID} 任务目标 {item.Target.ID} MapID<1 不合法 ");
                    }

                    if (item.Target.MaxNum < 1)
                    {
                        message.AppendLine($"{Base.TaskID} 任务目标 {item.Target.ID} MaxNum<1 不合法 ");
                    }

                    switch (item.Target.TargetType)
                    {
                        case EffectTargetType.NPC:
                        {
                            if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                            {
                                if (!value._npclist.Exists((match) => match.Value == item.Target.TargetID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 任务目标 {item.Target.ID} {item.Target.TargetType}时  item.TargetID 不合法 ");
                                }
                            }
                        }
                            break;
                        case EffectTargetType.InterAction:
                        {
                            if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                            {
                                if (!value._interlist.Exists((match) => match.Value == item.Target.TargetID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 任务目标 {item.Target.ID} {item.Target.TargetType}时  item.TargetID 不合法 ");
                                }
                            }
                        }
                            break;
                        case EffectTargetType.Area:
                        {
                            if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                            {
                                if (!value._arealist.Exists((match) => match.Value == item.Target.TargetID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 任务目标 {item.Target.ID} {item.Target.TargetType}时  item.TargetID 不合法 ");
                                }
                            }
                        }
                            break;
                        case EffectTargetType.FinishTarget:
                            break;
                        default:
                            break;
                    }

                    if (item.Target.FindPath != null)
                    {
                        switch (item.Target.FindPath.FindPathType)
                        {
                            case E_FindPath.None:
                                break;
                            case E_FindPath.Spawer:
                            {
                                if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                                {
                                    if (!value._spawner.Exists((match) => match.Value == item.Target.FindPath.ID))
                                    {
                                        message.AppendLine(
                                            $"{Base.TaskID} 任务目标 {item.Target.ID} 寻路类型为 {item.Target.FindPath.FindPathType}  item.FindPath.ID 不合法 ");
                                    }
                                }
                            }
                                break;
                            case E_FindPath.NPC:
                            {
                                if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                                {
                                    if (!value._npclist.Exists((match) => match.Value == item.Target.FindPath.ID))
                                    {
                                        //message.AppendLine(
                                        //    $"{Base.TaskID} 任务目标 {item.Target.ID} 寻路类型为 {item.Target.FindPath.FindPathType}  item.FindPath.ID 不合法 ");
                                    }
                                }
                            }
                                break;
                            case E_FindPath.InterAction:
                            {
                                /*if (TaskEnumUtils._SceneList.TryGetValue(item.MapID, out SceneCfg value))
                                {
                                    if (!value._interlist.Exists((match) => match.Value == item.FindPath.ID))
                                    {
                                        message.AppendLine(
                                            $"{Base.TaskID} 任务目标 {item.ID} 寻路类型为 {item.FindPath.FindPathType}  item.FindPath.ID 不合法 ");
                                    }
                                }*/
                            }
                                break;
                            case E_FindPath.Area:
                            {
                                if (TaskEnumUtils._SceneList.TryGetValue(item.Target.MapID, out SceneCfg value))
                                {
                                    if (!value._arealist.Exists((match) => match.Value == item.Target.FindPath.ID))
                                    {
                                        message.AppendLine(
                                            $"{Base.TaskID} 任务目标 {item.Target.ID} 寻路类型为 {item.Target.FindPath.FindPathType}  item.FindPath.ID 不合法 ");
                                    }
                                }
                            }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            //条件模块检查
            if (Condition != null && Condition.Conditions != null && Condition.Conditions.Count > 0)
            {
                foreach (var it in Condition.Conditions)
                {
                    if (it == null || it.SubEditorConditions == null || it.SubEditorConditions.Count < 1)
                    {
                        continue;
                    }

                    foreach (var item in it.SubEditorConditions)
                    {
                        switch (item.ConditionType)
                        {
                            case ConditionType.TaskIsFinish:
                                if (!ContinesTask.Invoke(item.TaskFinish.TaskID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 条件模块:判断任务是否完成  任务ID{item.TaskFinish.TaskID} 不存在 ");
                                }

                                break;
                            case ConditionType.TaskIsRunning:
                                if (!ContinesTask.Invoke(item.TaskRunning.TaskID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 条件模块:判断任务是否进行中 任务ID{item.TaskRunning.TaskID} 不存在 ");
                                }

                                break;
                            case ConditionType.TaskIsCommit:
                                if (!ContinesTask.Invoke(item.TaskCommit.TaskID))
                                {
                                    message.AppendLine(
                                        $"{Base.TaskID} 条件模块:判断任务是否已提交 任务ID{item.TaskCommit.TaskID} 不存在 ");
                                }

                                break;
                            case ConditionType.TaskEventIsFinish:
                                if (!ContinesTask.Invoke(item.TaskEvent.TaskID))
                                {
                                    message.AppendLine($"{Base.TaskID} 条件模块:任务事件是否完成 任务ID{item.TaskEvent.TaskID} 不存在 ");
                                }

                                break;
                        }
                    }
                }
            }


            return message.ToString();
        }
#endif
    }
}