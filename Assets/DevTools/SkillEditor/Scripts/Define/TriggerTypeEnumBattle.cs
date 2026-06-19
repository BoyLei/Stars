
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumBattle
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
    /// Owner的战斗状态
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumBattle:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 是否在战斗
        /// <summary>
        [LabelText("是否在战斗")]
        public bool Negate;

    }

}