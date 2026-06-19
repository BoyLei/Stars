
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChargeInput
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
    /// 蓄力输入
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChargeInput:BaseEffectType 
    {
        /// <summary>
        /// 是否开启输入
        /// <summary>
        [LabelText("是否开启输入")]
        [HideInInspector]
        public bool OpenInput;

        /// <summary>
        /// 输入逻辑
        /// <summary>
        [LabelText("输入逻辑")]
        [ValueDropdown("_ontimelogic")]
        public StageEvent OnTimeLogic=StageEvent.KillStage;

        /// <summary>
        /// 每层时间
        /// <summary>
        [LabelText("每层时间")]
        public int LayerTime;

        /// <summary>
        /// 蓄力最小时间
        /// <summary>
        [LabelText("蓄力最小时间")]
        public int MinTime;

        /// <summary>
        /// 蓄力最大时间
        /// <summary>
        [LabelText("蓄力最大时间")]
        public int MaxTime;

        /// <summary>
        /// 分段时间
        /// <summary>
        [LabelText("分段时间")]
        public List<int> SegmentationTime= new List<int>();

        /// <summary>
        /// 轮盘配置
        /// <summary>
        [LabelText("轮盘配置")]
        [HideReferenceObjectPicker]
        public WheelConfig WheelCfg= new WheelConfig();

        /// <summary>
        /// 轮盘可选范围
        /// <summary>
        [LabelText("轮盘可选范围")]
        [HideReferenceObjectPicker]
        public ShapeRingFan WheelRange= new ShapeRingFan();

        /// <summary>
        /// 最小层数
        /// <summary>
        [LabelText("最小层数")]
        public int MinLayer;

        /// <summary>
        /// 最大层数
        /// <summary>
        [LabelText("最大层数")]
        public int MaxLayer;

        /// <summary>
        /// 层数显示方式
        /// <summary>
        [LabelText("层数显示方式")]
        [ValueDropdown("_layershowtype")]
        public LayerShowType LayerShowType=LayerShowType.Offset;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _ontimelogic()
        {
            return EnumDefineMap._stageevent;
        }

        public IEnumerable _layershowtype()
        {
            return EnumDefineMap._layershowtype;
        }

    }

}