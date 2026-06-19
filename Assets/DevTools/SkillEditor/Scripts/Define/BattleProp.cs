
///--------------------------------------------------------------------
/// 文件名   :   BattleProp
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
    /// 战斗属性
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class BattleProp 
    {
        /// <summary>
        /// 属性
        /// <summary>
        [LabelText("属性")]
        [ValueDropdown("_prop")]
        public BattlePropEnum Prop= new BattlePropEnum();

        /// <summary>
        /// 属性值
        /// <summary>
        [LabelText("属性值")]
        public int PropVaule;

        public IEnumerable _prop()
        {
            return EnumDefineMap._battlepropenum;
        }

    }

}