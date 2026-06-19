
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeBulletSpeed
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
    /// 修改子弹移动速率
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeBulletSpeed:BaseEffectType 
    {
        /// <summary>
        /// 修改方式
        /// <summary>
        [LabelText("修改方式")]
        [ValueDropdown("_propchangetype")]
        public PropChangeType PropChangeType= new PropChangeType();

        /// <summary>
        /// 修改值
        /// <summary>
        [LabelText("修改值")]
        public int ChangeValue;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _propchangetype()
        {
            return EnumDefineMap._propchangetype;
        }

    }

}