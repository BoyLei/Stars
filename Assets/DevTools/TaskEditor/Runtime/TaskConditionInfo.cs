///--------------------------------------------------------------------
/// 文件名   :   TaskConditionInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:14:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    [InlineProperty]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskConditionInfo
    {
        [FoldoutGroup("承接任务")]
        [LabelText("是否自动承接")]
        [Key(0)]
        [OnValueChanged("OnAutoPick")]
        public bool AutoPick = true;

        [FoldoutGroup("承接任务")]
        [LabelText("承接MapID")]
        [ShowIf("IsShowPickMap")]
        [Key(1)]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownMapList", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "地图名字")]
#endif
        public int PickTaskMapID;

        [FoldoutGroup("承接任务")]
        [LabelText("承接NPCID")]
        [ShowIf("IsShowPickMap")]
        [Key(2)]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownPickNpcList", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "Npc名字")]
#endif
        public long PickTaskNPC;

        [ShowInInspector]
        [FoldoutGroup("承接任务")]
        [LabelText("承接效果组")]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public List<EffectSerialize> EditorPickEffects;

        [HideInInspector]
        [Key(3)]
        public List<EffectJsonData> PickEffects;

        [FoldoutGroup("角色条件")]
        [LabelText("角色等级")]
        [Key(4)]
        public int Level;

        [FoldoutGroup("角色条件")]
        [LabelText("角色性别")]
        [Key(5)]
        public int Sex;

        [FoldoutGroup("角色条件")]
        [LabelText("角色职业")]
        [Key(6)]
        public int Job;
        

        [FoldoutGroup("接取条件")]
        [ShowInInspector]
        [LabelText("条件")]
        [Key(7)]
        public List<TaskSubContionGroup> Conditions;
        private bool IsShowPickMap()
        {
            return !this.AutoPick;
        }
        private void OnAutoPick() 
        {
            if (AutoPick)
            {
                PickTaskMapID = 0;
                PickTaskNPC = 0;
            }
        }
#if UNITY_EDITOR
        public IEnumerable GetDropdownMapList()
        {
            return TaskEnumUtils._maplist;
        }

        public IEnumerable GetDropdownPickNpcList()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.PickTaskMapID, out var scfg))
            {
                return scfg._npclist;
            }
            return null;
        }
#endif
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (EditorPickEffects == null)
            {
                EditorPickEffects = new List<EffectSerialize>();
            }
            EditorPickEffects.Clear();
            if (PickEffects != null)
            {
                foreach (var item in PickEffects)
                {
                    EffectSerialize effectSerialize = new EffectSerialize(item);
                    effectSerialize.EffectType = item.EffectType;
                    EditorPickEffects.Add(effectSerialize);
                }
            }
        }

        [OnSerializing]
        public void OnSerializd(StreamingContext context)
        {
            if (PickEffects == null)
            {
                PickEffects = new List<EffectJsonData>();
            }
            PickEffects.Clear();
            if (EditorPickEffects != null)
            {
                foreach (var item in EditorPickEffects)
                {
                    EffectJsonData effectJson = new EffectJsonData(item);
                    PickEffects.Add(effectJson);
                }
            }
        }
    }

    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskSubContionGroup
    {
        [ShowInInspector]
        [LabelText("子组")]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public List<ConditionSerialize> SubEditorConditions;

        [HideInInspector]
        [Key(0)]
        public List<JsonConditon> SubConditions;

        public TaskSubContionGroup()
        {
            SubEditorConditions = new List<ConditionSerialize>();
            SubConditions = new List<JsonConditon>();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if(SubEditorConditions==null)
            {
                SubEditorConditions = new List<ConditionSerialize>();
            }
            SubEditorConditions.Clear();
            if(SubConditions!=null)
            {
                foreach (var item in SubConditions)
                {
                    if(item.ConditionType==ConditionType.None)
                    {
                        continue;
                    }
                    ConditionSerialize conditionSerialize = new ConditionSerialize(item);
                    conditionSerialize.ConditionType = item.ConditionType;
                    SubEditorConditions.Add(conditionSerialize);
                }
            }
        }

        [OnSerializing]
        public void OnSerializd(StreamingContext context)
        {
            if(SubConditions==null)
            {
                SubConditions = new List<JsonConditon>();
            }
            SubConditions.Clear();
            if(SubEditorConditions!=null)
            {
                foreach (var item in SubEditorConditions)
                {
                    JsonConditon conditon = new JsonConditon(item);
                    SubConditions.Add(conditon);
                }
            }

        }
    }
}