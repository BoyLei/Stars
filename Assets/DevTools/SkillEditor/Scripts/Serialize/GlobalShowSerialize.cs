
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowSerialize
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
    /// 表现标签序列化
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowSerialize 
    {
        /// <summary>
        /// 表现标签
        /// <summary>
        [LabelText("表现标签")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_globalshowtype")]
        public GlobalShowType GlobalShowType= new GlobalShowType();

        /// <summary>
        /// BUFFUI显示详情
        /// <summary>
        [LabelText("BUFFUI显示详情")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_SpecialBUFFUI")]
        public GlobalShowBUFF_BUFFUIDateils BUFF_SpecialBUFFUI= new GlobalShowBUFF_BUFFUIDateils();

        /// <summary>
        /// 修改Shader
        /// <summary>
        [LabelText("修改Shader")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ShaderChange")]
        public GlobalShowBUFF_ShaderChange BUFF_ShaderChange= new GlobalShowBUFF_ShaderChange();

        /// <summary>
        /// 附影
        /// <summary>
        [LabelText("附影")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ShadowFollow")]
        public GlobalShowBUFF_ShadowFollow BUFF_ShadowFollow= new GlobalShowBUFF_ShadowFollow();

        /// <summary>
        /// 状态动作修改
        /// <summary>
        [LabelText("状态动作修改")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ChangeAnim")]
        public GlobalShowBUFF_ChangeAnim BUFF_ChangeAnim= new GlobalShowBUFF_ChangeAnim();

        /// <summary>
        /// 动作暂停
        /// <summary>
        [LabelText("动作暂停")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_Paralysis")]
        public GlobalShowBUFF_Paralysis BUFF_Paralysis= new GlobalShowBUFF_Paralysis();

        /// <summary>
        /// 击倒
        /// <summary>
        [LabelText("击倒")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_Knock")]
        public GlobalShowBUFF_Knock BUFF_Knock= new GlobalShowBUFF_Knock();

        /// <summary>
        /// 隐身
        /// <summary>
        [LabelText("隐身")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_Hide")]
        public GlobalShowBUFF_Hide BUFF_Hide= new GlobalShowBUFF_Hide();

        /// <summary>
        /// 隐藏UI
        /// <summary>
        [LabelText("隐藏UI")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_HideUI")]
        public GlobalShowBUFF_HideUI BUFF_HideUI= new GlobalShowBUFF_HideUI();

        /// <summary>
        /// 点击屏幕释放技能
        /// <summary>
        [LabelText("点击屏幕释放技能")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ClickUseSkill")]
        public GlobalShowBUFF_ClickUseSkill BUFF_ClickUseSkill= new GlobalShowBUFF_ClickUseSkill();

        /// <summary>
        /// 修改颜色Shader
        /// <summary>
        [LabelText("修改颜色Shader")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ChangeColorShader")]
        public GlobalShowBUFF_ChangeColorShader BUFF_ChangeColorShader= new GlobalShowBUFF_ChangeColorShader();

        /// <summary>
        /// 摄像机移动
        /// <summary>
        [LabelText("摄像机移动")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeGlobal_CameraMove")]
        public GlobalShowGlobal_CameraMove Global_CameraMove= new GlobalShowGlobal_CameraMove();

        /// <summary>
        /// 量谱图切换
        /// <summary>
        [LabelText("量谱图切换")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeGlobal_SpectralSkill")]
        public GlobalShowGlobal_SpectralSkill Global_SpectralSkill= new GlobalShowGlobal_SpectralSkill();

        /// <summary>
        /// 与主人连线
        /// <summary>
        [LabelText("与主人连线")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_PlayEffectLineRenderer")]
        public GlobalShowBUFF_PlayEffectLineRenderer BUFF_PlayEffectLineRenderer= new GlobalShowBUFF_PlayEffectLineRenderer();

        /// <summary>
        /// 特殊时间展示
        /// <summary>
        [LabelText("特殊时间展示")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_SpTimeShow")]
        public GlobalShowBUFF_SpTimeShow BUFF_SpTimeShow= new GlobalShowBUFF_SpTimeShow();

        /// <summary>
        /// 修改化身
        /// <summary>
        [LabelText("修改化身")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_AvatarChange")]
        public GlobalShowBUFF_AvatarChange BUFF_AvatarChange= new GlobalShowBUFF_AvatarChange();

        /// <summary>
        /// 技能槽隐藏
        /// <summary>
        [LabelText("技能槽隐藏")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_SkillSlotHide")]
        public GlobalShowBUFF_SkillSlotHide BUFF_SkillSlotHide= new GlobalShowBUFF_SkillSlotHide();

        /// <summary>
        /// 量谱显示方式修改
        /// <summary>
        [LabelText("量谱显示方式修改")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_SpectralChange")]
        public GlobalShowBUFF_SpectralChange BUFF_SpectralChange= new GlobalShowBUFF_SpectralChange();

        /// <summary>
        /// 透明度修改
        /// <summary>
        [LabelText("透明度修改")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_TransChange")]
        public GlobalShowBUFF_TransChange BUFF_TransChange= new GlobalShowBUFF_TransChange();

        /// <summary>
        /// 新增职业技能
        /// <summary>
        [LabelText("新增职业技能")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_JobSkillChange")]
        public GlobalShowBUFF_JobSkillChange BUFF_JobSkillChange= new GlobalShowBUFF_JobSkillChange();

        /// <summary>
        /// 基础职业技能失效
        /// <summary>
        [LabelText("基础职业技能失效")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_JobSkillCancel")]
        public GlobalShowBUFF_JobSkillCancel BUFF_JobSkillCancel= new GlobalShowBUFF_JobSkillCancel();

        /// <summary>
        /// 改变阵营
        /// <summary>
        [LabelText("改变阵营")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeBUFF_ChangeFaction")]
        public GlobalShowBUFF_ChangeFaction BUFF_ChangeFaction= new GlobalShowBUFF_ChangeFaction();

        /// <summary>
        /// 召唤客户端召唤物
        /// <summary>
        [LabelText("召唤客户端召唤物")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeGlobal_AddClientSummon")]
        public Global_AddClientSummon Global_AddClientSummon= new Global_AddClientSummon();

        /// <summary>
        /// 技能槽特殊表现
        /// <summary>
        [LabelText("技能槽特殊表现")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeSkill_SkillSlotSpShow")]
        public GlobalShowSkill_SkillSlotSpShow Skill_SkillSlotSpShow= new GlobalShowSkill_SkillSlotSpShow();

        public IEnumerable _globalshowtype()
        {
            return EnumDefineMap._globalshowtype;
        }

        public bool ShouldSerializeBUFF_SpecialBUFFUI()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_SpecialBUFFUI;
        }

        public bool ShouldSerializeBUFF_ShaderChange()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ShaderChange;
        }

        public bool ShouldSerializeBUFF_ShadowFollow()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ShadowFollow;
        }

        public bool ShouldSerializeBUFF_ChangeAnim()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ChangeAnim;
        }

        public bool ShouldSerializeBUFF_Paralysis()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_Paralysis;
        }

        public bool ShouldSerializeBUFF_Knock()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_Knock;
        }

        public bool ShouldSerializeBUFF_Hide()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_Hide;
        }

        public bool ShouldSerializeBUFF_HideUI()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_HideUI;
        }

        public bool ShouldSerializeBUFF_ClickUseSkill()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ClickUseSkill;
        }

        public bool ShouldSerializeBUFF_ChangeColorShader()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ChangeColorShader;
        }

        public bool ShouldSerializeGlobal_CameraMove()
        {
            return this.GlobalShowType == GlobalShowType.Global_CameraMove;
        }

        public bool ShouldSerializeGlobal_SpectralSkill()
        {
            return this.GlobalShowType == GlobalShowType.Global_SpectralSkill;
        }

        public bool ShouldSerializeBUFF_PlayEffectLineRenderer()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_PlayEffectLineRenderer;
        }

        public bool ShouldSerializeBUFF_SpTimeShow()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_SpTimeShow;
        }

        public bool ShouldSerializeBUFF_AvatarChange()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_AvatarChange;
        }

        public bool ShouldSerializeBUFF_SkillSlotHide()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_SkillSlotHide;
        }

        public bool ShouldSerializeBUFF_SpectralChange()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_SpectralChange;
        }

        public bool ShouldSerializeBUFF_TransChange()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_TransChange;
        }

        public bool ShouldSerializeBUFF_JobSkillChange()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_JobSkillChange;
        }

        public bool ShouldSerializeBUFF_JobSkillCancel()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_JobSkillCancel;
        }

        public bool ShouldSerializeBUFF_ChangeFaction()
        {
            return this.GlobalShowType == GlobalShowType.BUFF_ChangeFaction;
        }

        public bool ShouldSerializeGlobal_AddClientSummon()
        {
            return this.GlobalShowType == GlobalShowType.Global_AddClientSummon;
        }

        public bool ShouldSerializeSkill_SkillSlotSpShow()
        {
            return this.GlobalShowType == GlobalShowType.Skill_SkillSlotSpShow;
        }

    }

}