
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeUserInput
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
    /// 用户输入
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeUserInput:BaseEffectType 
    {
        /// <summary>
        /// 是否开启输入
        /// <summary>
        [LabelText("是否开启输入")]
        [HideInInspector]
        public bool OpenInput;

        /// <summary>
        /// 输入类型
        /// <summary>
        [LabelText("输入类型")]
        [ValueDropdown("_inputtype")]
        public InputType InputType=InputType.Prepare;

        /// <summary>
        /// 提前禁止用户输入时间
        /// <summary>
        [LabelText("提前禁止用户输入时间")]
        public int StopInputBeforeEnd;

        /// <summary>
        /// 非活跃阶段效果持续时间
        /// <summary>
        [LabelText("非活跃阶段效果持续时间")]
        public int PassiveStageDuringTime;

        /// <summary>
        /// 输入逻辑
        /// <summary>
        [LabelText("输入逻辑")]
        [ValueDropdown("_ontimelogic")]
        public StageEvent OnTimeLogic=StageEvent.KillStage;

        /// <summary>
        /// 移动时禁止预输入
        /// <summary>
        [LabelText("移动时禁止预输入")]
        public bool CanMarkInvalid;

        /// <summary>
        /// 超时逻辑
        /// <summary>
        [LabelText("超时逻辑")]
        [ValueDropdown("_outtimelogic")]
        public StageEvent OutTimeLogic=StageEvent.KillStage;

        /// <summary>
        /// 施法方式
        /// <summary>
        [LabelText("施法方式")]
        [ValueDropdown("_castmethod")]
        public CastMethodType CastMethod= new CastMethodType();

        /// <summary>
        /// 技能输入类型
        /// <summary>
        [LabelText("技能输入类型")]
        [ValueDropdown("_skillinputtype")]
        public SkillInputType SkillInputType= new SkillInputType();

        /// <summary>
        /// 技能施法条件
        /// <summary>
        [LabelText("技能施法条件")]
        public List<SkillCondition> Conditions= new List<SkillCondition>();

        /// <summary>
        /// 指示器修改方式
        /// <summary>
        [LabelText("指示器修改方式")]
        [ValueDropdown("_dynamicrangetype")]
        public DynamicRangeType DynamicRangeType=DynamicRangeType.NoChange;

        /// <summary>
        /// 轮盘配置
        /// <summary>
        [LabelText("轮盘配置")]
        public WheelConfig WheelCfg= new WheelConfig();

        /// <summary>
        /// 轮盘可选范围
        /// <summary>
        [LabelText("轮盘可选范围")]
        public ShapeRingFan WheelRange= new ShapeRingFan();

        /// <summary>
        /// 技能最大转身时间
        /// <summary>
        [LabelText("技能最大转身时间")]
        public int SkillTurnAroundTime;

        /// <summary>
        /// 释放自动转向
        /// <summary>
        [LabelText("释放自动转向")]
        public bool IsAutoTurnToTarget;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _inputtype()
        {
            return EnumDefineMap._inputtype;
        }

        public IEnumerable _ontimelogic()
        {
            return EnumDefineMap._stageevent;
        }

        public IEnumerable _outtimelogic()
        {
            return EnumDefineMap._stageevent;
        }

        public IEnumerable _castmethod()
        {
            return EnumDefineMap._castmethodtype;
        }

        public IEnumerable _skillinputtype()
        {
            return EnumDefineMap._skillinputtype;
        }

        public IEnumerable _dynamicrangetype()
        {
            return EnumDefineMap._dynamicrangetype;
        }

    }

}