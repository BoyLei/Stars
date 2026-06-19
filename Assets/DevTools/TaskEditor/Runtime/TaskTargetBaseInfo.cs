///--------------------------------------------------------------------
/// 文件名   :   TaskTargetBaseInfo.cs
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
    public class TaskTargetBase
    {
        //[TabGroup("任务目标信息")]
        [ShowInInspector]
        [LabelText("任务目标")]
        [Key(0)]
        public TaskTarget Target;
    }
}
