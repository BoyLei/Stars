///--------------------------------------------------------------------
/// 文件名   :   TaskFinishInfo
/// Info.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:19:49
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskFinishInfo
    {
        [FoldoutGroup("交付任务")]
        [LabelText("是否自动提交")]
        [Key(0)]
        [OnValueChanged("OnAutoFinishChanged")]
        public bool AutoFinish = true;

        [FoldoutGroup("交付任务")]
        [LabelText("是否点击直接完成")]
        [Key(1)]
        [OnValueChanged("OnClickFinishChanged")]
        public bool ClickFinish = false;

        [FoldoutGroup("交付任务")]
        [LabelText("交付MapID")]
        [ShowIf("IsShowFinishMap")]
        [Key(2)]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownMapList", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "地图名字")]
#endif
        public int FinishTaskMapID;

        [FoldoutGroup("交付任务")]
        [LabelText("交付NPCID")]
        [ShowIf("IsShowFinishMap")]
        [Key(3)]
#if UNITY_EDITOR
        [ValueDropdown("GetDropdownFinishNpcList", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "Npc名字")]
#endif
        public long FinishTaskNPC;

        [ShowInInspector]
        [FoldoutGroup("交付任务")]
        [LabelText("交付效果组")]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public List<EffectSerialize> EditorFinishEffects;

        [HideInInspector]
        [Key(4)]
        public List<EffectJsonData> FinishEffects;

        //[TabGroup("任务目标")]
        [ShowInInspector]
        [LabelText("任务目标列表")]
        [OnCollectionChanged("After")]
        [Key(5)]
        public List<TaskTargetBase> Targets;
#if UNITY_EDITOR
        public void After(Sirenix.OdinInspector.Editor.CollectionChangeInfo info, object value)
        {
            if (info.ChangeType == Sirenix.OdinInspector.Editor.CollectionChangeType.Add)
            {
                if (Targets != null && info.Index < Targets.Count)
                {
                    TaskTargetBase taskTarget = info.Value as TaskTargetBase;
                    taskTarget.Target = new TaskTarget();
                    taskTarget.Target.ID = GetTargetID();
                }
            }
        }

        private int GetTargetID()
        {
            int ID = 1;
            if (Targets != null && Targets.Count > 0)
            {
                while (true)
                {
                    for (int i = 0; i < Targets.Count; i++)
                    {
                        if (Targets[i].Target.ID == ID)
                        {
                            ID++;
                        }
                    }

                    break;
                }
            }

            return ID;
        }
#endif

        public TaskTarget GetTaskTarget(int EventID)
        {
            if (Targets != null || Targets.Count > 0)
            {
                for (int i = 0; i < Targets.Count; i++)
                {
                    if (Targets[i].Target.ID == EventID)
                    {
                        return Targets[i].Target;
                    }
                }
            }


            return null;
        }
        private bool IsShowFinishMap()
        {
            return !(this.AutoFinish || ClickFinish);
        }
        private void OnAutoFinishChanged()
        {
            if (AutoFinish && ClickFinish)
            {
                ClickFinish = false;
            }
        }
        private void OnClickFinishChanged()
        {
            if (ClickFinish && AutoFinish)
            {
                AutoFinish = false;
            }
        }
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (EditorFinishEffects == null)
            {
                EditorFinishEffects = new List<EffectSerialize>();
            }

            EditorFinishEffects.Clear();
            if (FinishEffects != null)
            {
                foreach (var item in FinishEffects)
                {
                    EffectSerialize effectSerialize = new EffectSerialize(item);
                    effectSerialize.EffectType = item.EffectType;
                    EditorFinishEffects.Add(effectSerialize);
                }
            }
        }

        [OnSerializing]
        public void OnSerializd(StreamingContext context)
        {
            if (FinishEffects == null)
            {
                FinishEffects = new List<EffectJsonData>();
            }
            FinishEffects.Clear();
            if (EditorFinishEffects != null)
            {
                foreach (var item in EditorFinishEffects)
                {
                    EffectJsonData effectJson = new EffectJsonData(item);
                    FinishEffects.Add(effectJson);
                }
            }
        }


        public IEnumerable GetTargetType()
        {
            return TaskEnumUtils._effecttargettype;
        }

        /// <summary>
        /// 获取所有地图的下拉列表
        /// </summary>
        /// <returns></returns>
#if UNITY_EDITOR
        public IEnumerable GetDropdownFinishNpcList()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.FinishTaskMapID, out var scfg))
            {
                return scfg._npclist;
            }
            return null;
        }
        public IEnumerable GetDropdownMapList()
        {
            return TaskEnumUtils._maplist;
        }
#endif
    }
}
