
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeCD
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
    /// 修改CD
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeCD:BaseEffectType 
    {
        /// <summary>
        /// 修改CD目标Key
        /// <summary>
        [LabelText("修改CD目标Key")]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// 修改方式
        /// <summary>
        [LabelText("修改方式")]
        [ValueDropdown("_changetype")]
        public CDChangeType ChangeType= new CDChangeType();

        /// <summary>
        /// 数值组ID
        /// <summary>
        [LabelText("数值组ID")]
        public int GroupID;

        /// <summary>
        /// 数值组Key
        /// <summary>
        [LabelText("数值组Key")]
        [HideReferenceObjectPicker]
        public InputKey[] GroupIDKey;

        /// <summary>
        /// 减CD特效
        /// <summary>
        [LabelText("减CD特效")]
        [HideReferenceObjectPicker]
        public List<EffectTypeHitEffect> HitEffect= new List<EffectTypeHitEffect>();

        public IEnumerable _changetype()
        {
            return EnumDefineMap._cdchangetype;
        }

    }

}