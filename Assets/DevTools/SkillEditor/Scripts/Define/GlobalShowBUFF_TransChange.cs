
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_TransChange
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MessagePack;
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 透明度修改
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_TransChange:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 初始透明度
        /// <summary>
        [LabelText("初始透明度")]
        public int StartTrans;

        /// <summary>
        /// 目标透明度
        /// <summary>
        [LabelText("目标透明度")]
        public int EndTrans;

        /// <summary>
        /// 完全透明时间
        /// <summary>
        [LabelText("完全透明时间")]
        public int ChangeTime;

    }

}