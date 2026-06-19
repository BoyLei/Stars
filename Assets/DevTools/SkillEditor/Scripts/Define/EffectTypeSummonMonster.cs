
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSummonMonster
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
    /// 召唤怪物
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSummonMonster:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray CenterPosArray= new CenterPosArray();

        /// <summary>
        /// 阵营
        /// <summary>
        [LabelText("阵营")]
        public int FactionRelation=-1;

        /// <summary>
        /// 召唤ID
        /// <summary>
        [LabelText("召唤ID")]
        public int SummonID;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}