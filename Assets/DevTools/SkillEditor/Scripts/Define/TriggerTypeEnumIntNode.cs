
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumIntNode
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
    /// 检测指定运行时黑板上的数据
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumIntNode:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 黑板上的KEY
        /// <summary>
        [LabelText("黑板上的KEY")]
        [ValueDropdown("_runtimetype")]
        public TriRuntimeType RuntimeType= new TriRuntimeType();

        /// <summary>
        /// 黑板上的KEY
        /// <summary>
        [LabelText("黑板上的KEY")]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public CompOperator CompOperator= new CompOperator();

        /// <summary>
        /// 值
        /// <summary>
        [LabelText("值")]
        public int Val;

        public IEnumerable _runtimetype()
        {
            return EnumDefineMap._triruntimetype;
        }

        public IEnumerable _compoperator()
        {
            return EnumDefineMap._compoperator;
        }

    }

}