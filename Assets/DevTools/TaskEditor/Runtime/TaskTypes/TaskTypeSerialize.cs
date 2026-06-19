///--------------------------------------------------------------------
/// 文件名   :   TaskTypeSerialize.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:35:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class TaskTypeSerialize
    {

        [LabelText("任务目标类型")]
        [ValueDropdown("GetTaskTypes")]
        [OnValueChanged("OnTaskTypeChanged")]
        public TaskType TaskType = TaskType.Dialogue;

#if UNITY_EDITOR
        private void OnTaskTypeChanged(Sirenix.OdinInspector.Editor.InspectorProperty property, TaskType value)
        {
            if (TaskEnumUtils.TaskTypeFactory.TryGetValue(value, out var tp))
            {
                TaskInfo = (BaseTaskType)Activator.CreateInstance(tp);
                TaskInfo.MapID = _mapid;
            }
        }

#endif

        [LabelText("任务参数")]
        [SerializeReference]
        [HideReferenceObjectPicker]
        public BaseTaskType TaskInfo;

        public TaskTypeSerialize() {
            if (TaskEnumUtils.TaskTypeFactory.TryGetValue(TaskType, out var tp))
            {
                TaskInfo = (BaseTaskType)Activator.CreateInstance(tp);
                TaskInfo.MapID = _mapid;
            }
        }
        public TaskTypeSerialize(TaskTypeJsonData jsonData)
        {
            TaskType = jsonData.TaskType;
            if (TaskEnumUtils.TaskTypeFactory.TryGetValue(TaskType, out var tp))
            {
                TaskInfo = (BaseTaskType)Activator.CreateInstance(tp);
                TaskInfo.MapID = _mapid;
                TaskInfo.OnDeSerialized(jsonData.Pramas);
            }
        }
        public IEnumerable GetTaskTypes()
        {
            return TaskEnumUtils._tasktypes;
        }

        public int MapID
        {
            set {
                _mapid = value;
                TaskInfo.MapID = _mapid;
            }
        }
        private int _mapid;
    }

    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskTypeJsonData
    {
        [Key(0)]
        public TaskType TaskType;
        [Key(1)]
        public List<string> Pramas;

        public TaskTypeJsonData()
        {
            if (Pramas == null)
            {
                Pramas = new List<string>();
            }
            Pramas.Clear();
        }

        public TaskTypeJsonData(TaskTypeSerialize serialize)
        {
            if (Pramas == null)
            {
                Pramas = new List<string>();
            }
            Pramas.Clear();
            TaskType = serialize.TaskType;
            var taskBaseType = serialize.TaskInfo;
            taskBaseType?.OnSerialized(Pramas);
        }
    }
}
