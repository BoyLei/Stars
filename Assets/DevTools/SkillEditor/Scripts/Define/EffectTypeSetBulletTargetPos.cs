
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSetBulletTargetPos
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
    /// 设置子弹目标点
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSetBulletTargetPos:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos SingleCenterPos= new SingleCenterPos();

        /// <summary>
        /// 根据自身或者根据坐标
        /// <summary>
        [LabelText("根据自身或者根据坐标")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_builderorpos")]
        public BuilderOrPos BuilderOrPos= new BuilderOrPos();

        /// <summary>
        /// 距离
        /// <summary>
        [LabelText("距离")]
        public int Distance;

        /// <summary>
        /// 移动最大距离
        /// <summary>
        [LabelText("移动最大距离")]
        public int MaxMove=-1;

        /// <summary>
        /// 到目标点时间
        /// <summary>
        [LabelText("到目标点时间")]
        public int TimeToTarget=-1;

        public IEnumerable _builderorpos()
        {
            return EnumDefineMap._builderorpos;
        }

    }

}