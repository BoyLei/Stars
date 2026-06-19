///--------------------------------------------------------------------
/// 文件名   :   TaskBaseInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:03:46
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Sirenix.OdinInspector;
using StarProject.Service.Language;
using System.Collections;
using UnityEngine;

namespace Task
{
    public delegate bool HasTaskDelegate(uint taskID);

    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskBaseInfo
    {
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public System.Action<uint> OnFreshItemName;



        [Newtonsoft.Json.JsonIgnore]
        [IgnoreMember]
        public System.Action<TaskClassifyType, uint> ModifyIndexAction;

        public void TaskNameChange()
        {
            OnFreshItemName?.Invoke(TaskID);
        }
        [FoldoutGroup("基础信息")]
        [LabelText("创建流水号")]
        [HideInInspector]
        [DisplayAsString]
        [Key(0)]
        public uint UIndex;

        [FoldoutGroup("基础信息")]
        [LabelText("序号")]
        [Key(44)]
        [InlineButton("ModfiyIndex", "修改序号")]
        public int Index;

        [FoldoutGroup("基础信息")]
        [LabelText("任务ID")]
        [DisplayAsString]
        [Key(1)]
        public uint TaskID;

        [FoldoutGroup("基础信息")]
        [LabelText("任务链ID")]
        [DisplayAsString]
        [Key(2)]
        public ushort ChainID;

        [FoldoutGroup("基础信息")]
        [LabelText("章节ID")]
        [DisplayAsString]
        [Key(3)]
        public int Chapter;

        [FoldoutGroup("基础信息")]
        [LabelText("章节名称")]
        [PropertyOrder(-1)]
        [ShowInInspector]
        [DisplayAsString]
        [Key(4)]
        //public string ChapterName;
        public string ChapterName
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return chapterName;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(ChapterName_Key);
            }
            set { chapterName = value; }
        }
        private string chapterName;
        [HideInInspector]
        [Key(5)]
        public string ChapterName_Key;



        [FoldoutGroup("基础信息")]
        [LabelText("任务类型")]
        [ReadOnly]
        [ValueDropdown("GetClassifytypes")]
        [Key(6)]
        public TaskClassifyType TaskType;

        [FoldoutGroup("基础信息")]
        [LabelText("任务周期")]
        [ValueDropdown("GetTaskperiods")]
        [Key(7)]
        public TaskPeriod TaskCycle;

        [FoldoutGroup("基础信息")]
        [LabelText("任务名称")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [Key(8)]
        //public string TaskName;
        public string TaskName
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return taskName;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(TaskName_Key);
            }
            set { taskName = value; }
        }
        private string taskName;
        [HideInInspector]
        [Key(9)]
        public string TaskName_Key;

        [FoldoutGroup("基础信息")]
        [LabelText("任务组ID")]
        [Key(10)]
        public int GroupID;

        [FoldoutGroup("基础信息")]
        [LabelText("任务限时")]
        [SuffixLabel("毫秒")]
        [Key(11)]
        public int LimtTime;

        [FoldoutGroup("基础信息")]
        [LabelText("是否显示特殊进度")]
        [HideInInspector]
        [Key(12)]
        public bool ShowProcess = true;

        [FoldoutGroup("基础信息")]
        [LabelText("特殊描述")]
        [ShowInInspector]
        [Key(13)]
        //public string SpecTaskDesc;
        public string SpecTaskDesc
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return specTaskDesc;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(SpecTaskDesc_Key);
            }
            set { specTaskDesc = value; }
        }
        private string specTaskDesc;
        [HideInInspector]
        [Key(14)]
        public string SpecTaskDesc_Key;

        [FoldoutGroup("基础信息")]
        [ToggleLeft]
        [LabelText("前置任务(勾选  程序自动更新前置，否则 策划自己填写)")]
        [SuffixLabel("$PreTaskID")]
        [Key(17)]
        [OnValueChanged("OnHasPre")]
        public bool HasPre = true;

        [FoldoutGroup("基础信息")]
        [LabelText("前置任务ID")]
        [DisableIf("HasPre")]
        [Key(18)]
        public uint PreTaskID = 0;


        [FoldoutGroup("基础信息")]
        [ToggleLeft]
        [ShowIf("OnShowIfHasNext")]
        [LabelText("后置任务 (勾选  程序自动更新后置，否则 策划自己填写)")]
        [SuffixLabel("$NextTaskID")]
        [Key(19)]
        public bool HasNext = true;

        [FoldoutGroup("基础信息")]
        [LabelText("后置任务ID")]
        [ShowIf("OnShowIfHasNext")]
        [DisableIf("HasNext")]
        [Key(20)]
        public uint NextTaskID = 0;


        [HideInInspector]
        [Key(21)]
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

        [FoldoutGroup("基础信息")]
        [LabelText("任务描述")]
        [TextArea]
        [SerializeField]
        private string desc;
        [HideInInspector]
        [Key(22)]
        public string Desc_Key;

        [FoldoutGroup("任务属性")]
        [LabelText("启用自定义类型配置(如果不勾选 则用任务类型的配置)")]
        [Key(23)]
        public bool CanCustomTypeConfig = false;

        [FoldoutGroup("任务属性")]
        [LabelText("是否可放弃")]
        [ShowIf("IsShowCustomTypeConfig")]
        [Key(24)]
        public bool CanDelete;

        [FoldoutGroup("任务属性")]
        [LabelText("失败是否可重新接取")]
        [ShowIf("IsShowCustomTypeConfig")]
        [Key(25)]
        public bool CanAgain;

        [FoldoutGroup("任务属性")]
        [LabelText("是否有接取提示")]
        [ShowIf("IsShowCustomTypeConfig")]
        [Key(26)]
        public bool GetTips;

        [FoldoutGroup("任务属性")]
        [LabelText("是否有完成提示")]
        [ShowIf("IsShowCustomTypeConfig")]
        [Key(27)]
        public bool FinishTips = true;

        [HideInInspector]
        [FoldoutGroup("任务奖励")]
        [LabelText("任务奖励ID(掉落ID)")]
        [Key(37)]
        public int AwardID;

        [HideInInspector]
        [FoldoutGroup("任务奖励")]
        [LabelText("奖励标签")]
        [ValueDropdown("GetTaskTag")]
        [Key(38)]
        public string AwardTag;

        [HideInInspector]
        [FoldoutGroup("任务奖励")]
        [LabelText("临时获得")]
        [Key(39)]
        public int TempAward;

        [HideInInspector]
        [FoldoutGroup("任务奖励")]
        [LabelText("即将获得")]
        [Key(40)]
        public int ShowAward;

        [HideInInspector]
        [FoldoutGroup("任务奖励")]
        [LabelText("奖励展示")]
        [Key(41)]
        public int DropShow;

        [FoldoutGroup("服务")]
        [LabelText("模块名称")]
        [Key(42)]
        public string ModuleName;

        [FoldoutGroup("服务")]
        [LabelText("事件名称")]
        [Key(43)]
        public string EventName;

        public void ModfiyIndex()
        {
            ModifyIndexAction?.Invoke(TaskType, TaskID);
        }

        public IEnumerable GetClassifytypes()
        {
            return TaskEnumUtils._taskclassifytypes;
        }

        public IEnumerable GetTaskperiods()
        {
            return TaskEnumUtils._taskperiods;
        }

        public IEnumerable GetTaskTag()
        {
            return TaskEnumUtils._tasktag;
        }

        private bool IsShowCustomTypeConfig()
        {
            return CanCustomTypeConfig;
        }
        private bool OnShowIfHasNext() 
        {
            return Chapter == 13|| TaskType == TaskClassifyType.Challenge;
        }
        private void OnHasPre()
        {
            if (!HasPre)
            {
                PreTaskID = 0;
            }
        }
    }
}