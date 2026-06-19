
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCheckChargeTime
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
namespace SkillEditor
{
    /// <summary>
    /// 判断蓄力时间
    /// </summary>
    [System.Serializable]
    public  class EffectTypeCheckChargeTime:BaseEffectType 
    {
        /// <summary>
        /// 蓄力Key
        /// <summary>
        [LabelText("蓄力Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 检查值
        /// <summary>
        [LabelText("检查值")]
        public int CheckValue= new int();

        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public CompOperator CompOperator= new CompOperator();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _compoperator()
        {
            return EnumDefineMap._compoperator;
        }

    }

}