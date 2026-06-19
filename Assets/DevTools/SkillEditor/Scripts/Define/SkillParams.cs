
///--------------------------------------------------------------------
/// 文件名   :   SkillParams
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
    /// 技能数据
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class SkillParams 
    {
        /// <summary>
        /// 数值组ID
        /// <summary>
        [LabelText("数值组ID")]
        public int GroupID;

        /// <summary>
        /// 公式所需Key
        /// <summary>
        [LabelText("公式所需Key")]
        public InputKey[] ParamKeys;

    }

}