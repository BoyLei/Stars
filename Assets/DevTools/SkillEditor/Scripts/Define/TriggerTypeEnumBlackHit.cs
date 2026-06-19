
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumBlackHit
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
    /// 伤害节点-命中集合人数
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumBlackHit:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 检查对象
        /// <summary>
        [LabelText("检查对象")]
        public InputKey Tar= new InputKey();

        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public CompOperator CompOperator= new CompOperator();

        /// <summary>
        /// 人数
        /// <summary>
        [LabelText("人数")]
        public int Value;

        public IEnumerable _compoperator()
        {
            return EnumDefineMap._compoperator;
        }

    }

}