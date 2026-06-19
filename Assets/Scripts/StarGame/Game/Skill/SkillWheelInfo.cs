using SkillEditor;

/// <summary>
/// 自定义封装的 技能轮盘配置
/// </summary>
public class SkillWheelInfo
{
    /// <summary>
    /// 施法方式
    /// </summary>
    public CastMethodType CastMethod = CastMethodType.DirectCast;

    /// <summary>
    /// 技能输入类型
    /// <summary>
    public SkillInputType SkillInputType = SkillInputType.DirInput;

    /// <summary>
    /// 指示器修改方式
    /// <summary>
    public DynamicRangeType DynamicRangeType = DynamicRangeType.NoChange;

    /// <summary>
    /// 轮盘配置
    /// <summary>
    public WheelConfig WheelCfg = new();

    /// <summary>
    /// 轮盘可选范围
    /// <summary>
    public ShapeRingFan WheelRange = new();

    /// <summary>
    /// 释放自动转向 
    /// <summary>
    public bool IsAutoTurnToTarget = new();

    public void Reset()
    {
        DynamicRangeType = DynamicRangeType.NoChange;
        WheelCfg = null;
        WheelRange = null;
        IsAutoTurnToTarget = false;

        CastMethod = CastMethodType.DirectCast;
        SkillInputType = SkillInputType.DirInput;
    }


    public SkillWheelInfo Init(EffectTypeUserInput userInput)
    {
        DynamicRangeType = userInput.DynamicRangeType;
        WheelCfg = userInput.WheelCfg;
        WheelRange = userInput.WheelRange;
        IsAutoTurnToTarget = userInput.IsAutoTurnToTarget;

        CastMethod = userInput.CastMethod;
        SkillInputType = userInput.SkillInputType;
        return this;
    }

    public SkillWheelInfo Init(SkillConfig skillConfig)
    {
        DynamicRangeType = skillConfig.DynamicRangeType;
        WheelCfg = skillConfig.WheelCfg;
        WheelRange = skillConfig.WheelRange;
        IsAutoTurnToTarget = skillConfig.IsAutoTurnToTarget;

        CastMethod = skillConfig.CastMethod;
        SkillInputType = skillConfig.SkillInputType;
        return this;
    }

    public SkillWheelInfo Init(SkillConfig skillConfig, EffectTypeChargeInput chargeInput)
    {
        // 蓄力轴 默认不自动索敌, 同时动态类型 策划未配置
        {
            DynamicRangeType = DynamicRangeType.NoChange;
            IsAutoTurnToTarget = false;
        }
        WheelCfg = chargeInput.WheelCfg;
        WheelRange = chargeInput.WheelRange;

        CastMethod = skillConfig.CastMethod;
        SkillInputType = skillConfig.SkillInputType;
        return this;

    }
}
