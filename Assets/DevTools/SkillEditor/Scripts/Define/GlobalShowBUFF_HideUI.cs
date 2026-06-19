
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_HideUI
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
    /// 隐藏UI
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_HideUI:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public CompOperator CompOperator= new CompOperator();

        /// <summary>
        /// 隐藏标签
        /// <summary>
        [LabelText("隐藏标签")]
        [ValueDropdown("_hidelabels")]
        public List<UILabel> HideLabels= new List<UILabel>();

        public IEnumerable _compoperator()
        {
            return EnumDefineMap._compoperator;
        }

        public IEnumerable _hidelabels()
        {
            return EnumDefineMap._uilabel;
        }

    }

}