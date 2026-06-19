///--------------------------------------------------------------------
/// 文件名   :   TaskTargetInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:19:49
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Sirenix.OdinInspector;
using StarProject;
using StarProject.Service.Language;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskTarget
    {

        [FoldoutGroup("$BaseText", expanded: true)]
        //[FoldoutGroup("$BaseText", expanded: true)]
        [LabelText("目标ID")]
        [Key(0)]
        public int ID;

        [FoldoutGroup("$BaseText", expanded: true)]
        [ShowInInspector]
        [LabelText("任务目标描述")]
        [Key(1)]
        //public string Desc;
        public string Desc
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return desc;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(Desc_Key);
            }
            set { desc = value; }
        }
        //[TextArea]
        private string desc;
        [HideInInspector]
        [Key(2)]
        public string Desc_Key;

        [IgnoreMember]
        public string BaseText
        {
            get
            {
                return "基础信息：" + Desc;
            }
        }



        [FoldoutGroup("$BaseText", expanded: true)]
        [LabelText("完成所需次数")]
        [Key(3)]
        public int MaxNum = 1;

        [FoldoutGroup("$BaseText", expanded: true)]
        [LabelText("是否显示进度")]
        [Key(4)]
        public bool ShowProcess = false;
        
        [FoldoutGroup("$BaseText", expanded: true)]
        [LabelText("地图ID")]
        [Key(5)]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownMapList",NumberOfItemsBeforeEnablingSearch =3, SortDropdownItems=true, DropdownTitle="地图名字")]
        [OnValueChanged("OnMapIDChanged")]
        [OnInspectorInit("OnMapIDChanged")]
#endif
        public int MapID;

        [FoldoutGroup("任务类型")]
        [ShowInInspector]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        [HideLabel]
        [Key(6)]
        public TaskTypeSerialize EditorTaskType;

        
        
        [HideInInspector]
        [Key(7)]
        public TaskTypeJsonData TaskType;


        [FoldoutGroup("寻路相关")]
        [ShowInInspector]
        [HideLabel]
        [Key(8)]
        public TaskFindPath FindPath;

        [FoldoutGroup("触发效果")]
        [LabelText("任务目标完成时任务目标进度自动+1")]
        [PropertyTooltip("关卡，杀怪，收集，使用道具，使用技能 ,交互物 等不可勾选")]
        [ShowIf("ShowAddProcss")]
        [ReadOnly]
        [Key(9)]
        public bool AutoAddProcess = true;

        [FoldoutGroup("触发效果")]
        [LabelText("效果目标类型")]
        [ValueDropdown("GetTargetType")]
        [OnValueChanged("OnEffectTargetTypeChange")]
        [Key(10)]
        public EffectTargetType TargetType;

        
        [FoldoutGroup("触发效果")]
        [LabelText("效果目标ID")]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownTargetID", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "目标")]
#endif
        [Searchable]
        [Key(11)]
        public long TargetID;

        [FoldoutGroup("触发效果")]
        [ShowInInspector]
        [LabelText("效果")]
        [OnCollectionChanged("OnEditorEffects")]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public List<EffectSerialize> EditorEffects;

        [HideInInspector]
        [Key(12)]
        public List<EffectJsonData> Effects;


        

        public bool ShowAddProcss()
        {
            if (EditorTaskType != null)
            {
                var baseType = EditorTaskType.TaskType;
                AutoAddProcess = baseType == Task.TaskType.Dialogue;
            }
            else
            {
                AutoAddProcess = false;
            }
            return MaxNum != 1;
        }
        public bool CanAutoAddProcess()
        {
            if(EditorTaskType != null)
            {
                var baseType = EditorTaskType.TaskType;
                switch (baseType)
                {
                    // case Task.TaskType.Dialogue:
                    //     break;
                    case Task.TaskType.Level:
                    case Task.TaskType.KillMonster:
                    case Task.TaskType.Collect:
                    case Task.TaskType.UseItem:
                    case Task.TaskType.UseSkill:
                    case Task.TaskType.InterAction:
                    case Task.TaskType.KillMonsterGetItem:
                    case Task.TaskType.BagItems:
                    case Task.TaskType.Plays:
                        return false;
                    // case Task.TaskType.Arrive:
                    // case Task.TaskType.Escort:
                    // case Task.TaskType.Prologue:
                    // case Task.TaskType.Shady:
                    //     break;
                    default:
                        break;
                }
               
                
            }

            return true;
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if(EditorEffects==null)
            {
                EditorEffects = new List<EffectSerialize>();
            }
            EditorEffects.Clear();
            if (Effects != null)
            {
                foreach (var item in Effects)
                {
                    EffectSerialize effectSerialize = new EffectSerialize(item);
                    EditorEffects.Add(effectSerialize);
                }
            }

            if(TaskType!=null)
            {
                EditorTaskType = new TaskTypeSerialize(TaskType);
                EditorTaskType.MapID = MapID;
            }
            if (EditorEffects != null)
            {
                for (int i = 0; i < EditorEffects.Count; i++)
                {
                    if (EditorEffects[i] != null && EditorEffects[i].baseEffect!= null) { 
                        EditorEffects[i].baseEffect.Index = i; 
                    }
                    
                }
            }
        }

        [OnSerializing]
        public void OnSerializd(StreamingContext context)
        {
            if(Effects==null)
            {
                Effects = new List<EffectJsonData>();
            }
            Effects.Clear();
            if(EditorEffects!=null)
            {
                foreach (var item in EditorEffects)
                {
                    EffectJsonData effectJson = new EffectJsonData(item);
                    Effects.Add(effectJson);
                }
            }


            if (EditorTaskType != null)
            {
                TaskType = new TaskTypeJsonData(EditorTaskType);
                if (FindPath != null) 
                {
                    FindPath.TaskTypeInfo = EditorTaskType;
                }
                //if (EditorTaskType != null)
                //{
                //    EditorTaskType.FindPathInfo = FindPath;
                //}
            }
        }

#if UNITY_EDITOR
        private void OnMapIDChanged(Sirenix.OdinInspector.Editor.InspectorProperty property, int value, TaskConfigInfo root)
        {
            if (this.FindPath != null)
                this.FindPath.MapID = value;
            if (this.EditorTaskType != null)
            {
                this.EditorTaskType.MapID = value;
            }
        }

#endif
        public IEnumerable GetTargetType()
        {
            return TaskEnumUtils._effecttargettype;
        }

        /// <summary>
        /// 获取所有地图的下拉列表
        /// </summary>
        /// <returns></returns>
#if UNITY_EDITOR
        public void OnEditorEffects(Sirenix.OdinInspector.Editor.CollectionChangeInfo info, object value)
        {
            if (EditorEffects != null && info.Index < EditorEffects.Count)
            {
                for (int i = 0; i < EditorEffects.Count; i++)
                {
                    EditorEffects[i].baseEffect.Index = i;
                }
            }
        }
        public IEnumerable GetDropdownMapList()
        {
            return TaskEnumUtils._maplist;
        }

        public void OnEffectTargetTypeChange()
        {
            TargetID = 0;
        }

        public IEnumerable GetDropdownTargetID()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.MapID,out var scfg))
            {
                switch (this.TargetType)
                {
                    case EffectTargetType.NPC:
                        return scfg._npclist;
                    case EffectTargetType.InterAction:
                        return scfg._interlist;
                    case EffectTargetType.Area:
                        return scfg._arealist;
                    case EffectTargetType.FinishTarget:
                        return null;
                    default:
                        return null;
                }
            }
            return null;
        }
#endif
    }
}
