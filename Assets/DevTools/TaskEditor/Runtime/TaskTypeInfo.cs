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
    [InlineProperty]
    public class TaskTypeInfo
    {
        [ReadOnly]
        [Key(0)]
        public int TaskType = 0;//TaskClassifyType

        [FoldoutGroup("类型属性", Expanded = true)]
        [LabelText("是否可放弃")]
        [Key(1)]
        public bool CanDelete;

        [FoldoutGroup("类型属性", Expanded = true)]
        [LabelText("失败是否可重新接取")]
        [Key(2)]
        public bool CanAgain;

        [FoldoutGroup("类型属性", Expanded = true)]
        [LabelText("是否有接取提示")]
        [Key(3)]
        public bool GetTips;

        [FoldoutGroup("类型属性", Expanded = true)]
        [LabelText("是否有完成提示")]
        [Key(4)]
        public bool FinishTips = true;

        [FoldoutGroup("类型属性")]
        [LabelText("是否显示特殊进度")]
        [Key(5)]
        public bool ShowProcess = false;

        [FoldoutGroup("类型属性", Expanded = true)]
        [LabelText("任务主界面类型图标(文件名)")]
        [Key(6)]
        public string IconPath = string.Empty;

    }
}