///--------------------------------------------------------------------
/// 文件名   :   BuffEffectFactory
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/01 15:35:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using SkillEditor;

namespace StarProject.Game.TypeEffect
{
    public class TypeEffectFactory
    {
        public static Dictionary<GlobalShowType, System.Type> Type2Effects = new Dictionary<GlobalShowType, Type>()
        {
            { GlobalShowType.BUFF_ShaderChange, typeof(ShaderChangeEffect) },
            {GlobalShowType.BUFF_Knock,typeof(KnockDownBuffEffect) },
            {GlobalShowType.BUFF_ShadowFollow,typeof(ShadowFollowBuffEffect) },
            {GlobalShowType.BUFF_ChangeAnim,typeof(ChangeAnimsBuffEffect) },
            {GlobalShowType.BUFF_Hide,typeof(InvisibleBuffEffect) },
            {GlobalShowType.BUFF_Paralysis,typeof(ParalysisBuffEffect) },
            {GlobalShowType.BUFF_HideUI,typeof(HiddenUIEffect) },
            {GlobalShowType.BUFF_ClickUseSkill,typeof(Touch2UseSkillEffect) },
            {GlobalShowType.Global_CameraMove,typeof(CameraMoveTypeEffect) },
            {GlobalShowType.Global_SpectralSkill,typeof(SpectralSkillBuffEffect) },
            {GlobalShowType.BUFF_ChangeColorShader,typeof(ShaderChangeEffect2) },
            {GlobalShowType.BUFF_PlayEffectLineRenderer,typeof(BuffPlayLineRenderEffect) },
            {GlobalShowType.BUFF_AvatarChange,typeof(AvatarChangeEffect)},
            {GlobalShowType.BUFF_SkillSlotHide,typeof(HiddenSkillSlotEffect)},
            // {GlobalShowType.BUFF_SpectralChange,typeof()},
            {GlobalShowType.BUFF_JobSkillChange,typeof(ChangeSkillEffect)},
            {GlobalShowType.BUFF_TransChange,typeof(TranslucentEffect)},
            {GlobalShowType.Global_AddClientSummon,typeof(SimulateSummonEffect)},

        };

        public static BaseTypeEffect Create(GlobalShowSerialize typeSerialize)
        {
            GlobalShowType effectType = typeSerialize.GlobalShowType;

            if (Type2Effects.TryGetValue(effectType, out Type type))
            {
                BaseTypeEffect effect = (BaseTypeEffect)System.Activator.CreateInstance(type);
                effect.Init(typeSerialize);
                return effect;
            }

            return null;
        }
    }
}