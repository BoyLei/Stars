
///--------------------------------------------------------------------
/// 文件名   :   ClientSummonEffect
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
    /// 客户端召唤物播放特效
    /// </summary>
    [System.Serializable]
    public  class ClientSummonEffect:BaseEffectType 
    {
        /// <summary>
        /// 播放索引
        /// <summary>
        [LabelText("播放索引")]
        [HideReferenceObjectPicker]
        public List<int> Index= new List<int>();

        /// <summary>
        /// 受击特效
        /// <summary>
        [LabelText("受击特效")]
        [HideReferenceObjectPicker]
        public List<EffectTypeHitEffect> HitEffect= new List<EffectTypeHitEffect>();

    }

}