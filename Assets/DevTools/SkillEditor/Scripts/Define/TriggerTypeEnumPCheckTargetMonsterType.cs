
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumPCheckTargetMonsterType
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
using Newtonsoft.Json.Converters;
using MessagePack;
namespace SkillEditor
{
    /// <summary>
    /// 检查怪物类型
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public  class TriggerTypeEnumPCheckTargetMonsterType:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 检查对象
        /// <summary>
        [LabelText("检查对象")]
        [HideReferenceObjectPicker]
        public InputKey Tar= new InputKey();

        /// <summary>
        /// 检查类型
        /// <summary>
        [LabelText("检查类型")]
        public List<MonsterType> TypeList= new List<MonsterType>();

    }

}