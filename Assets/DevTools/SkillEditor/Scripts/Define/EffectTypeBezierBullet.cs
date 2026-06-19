
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeBezierBullet
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
    /// 贝塞尔子弹
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeBezierBullet:BaseEffectType 
    {
        /// <summary>
        /// 输入Key
        /// <summary>
        [LabelText("输入Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 接收者挂点
        /// <summary>
        [LabelText("接收者挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_totargethangpoint")]
        public HangPoint ToTargetHangPoint=HangPoint.Hurt_D ;

        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos SingleCenterPos= new SingleCenterPos();

        /// <summary>
        /// 目标点高度
        /// <summary>
        [LabelText("目标点高度")]
        [HideReferenceObjectPicker]
        public int SingleCenterPosHight;

        /// <summary>
        /// 随机点1
        /// <summary>
        [LabelText("随机点1")]
        [HideReferenceObjectPicker]
        public RandomVector2 RandomPoint1= new RandomVector2();

        /// <summary>
        /// 随机点2
        /// <summary>
        [LabelText("随机点2")]
        [HideReferenceObjectPicker]
        public RandomVector2 RandomPoint2= new RandomVector2();

        /// <summary>
        /// 随机角度1
        /// <summary>
        [LabelText("随机角度1")]
        [HideReferenceObjectPicker]
        public RandomVector2 RandomAngle= new RandomVector2();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _totargethangpoint()
        {
            return EnumDefineMap._hangpoint;
        }

    }

}