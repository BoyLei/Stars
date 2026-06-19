
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeBreakCurRuntimeInBullet
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
    /// 打断指定类型阶段
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeBreakCurRuntimeInBullet:BaseEffectType 
    {
        /// <summary>
        /// 阶段类型
        /// <summary>
        [LabelText("阶段类型")]
        [ValueDropdown("_stagetype")]
        public StageType StageType= new StageType();

        /// <summary>
        /// 打断方式
        /// <summary>
        [LabelText("打断方式")]
        [ValueDropdown("_ontimelogic")]
        public StageEvent OnTimeLogic= new StageEvent();

        public IEnumerable _stagetype()
        {
            return EnumDefineMap._stagetype;
        }

        public IEnumerable _ontimelogic()
        {
            return EnumDefineMap._stageevent;
        }

    }

}