///--------------------------------------------------------------------
/// 文件名   :   FreezeBuffEffect
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/01 15:20:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;
using SkillEditor;

namespace StarProject.Game.TypeEffect
{
    public class ShaderChangeEffect : BaseTypeEffect
    {
        /// <summary>
        /// 冰冻效果
        /// </summary>


        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                player.HandleStackEffect(this, TypeEffectUpdateType.OnEnter);

            }

            // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} buffid={buffid} Time={Time.time}");
        }

        public override void OnShowEnter()
        {
            base.OnShowEnter();
            HandleShaderChange(true);
        }

        public override void OnShowExit()
        {
            base.OnShowExit();

            HandleShaderChange(false);

        }

        private void HandleShaderChange(bool onEnter)
        {
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(ownerEntityID);

            switch (EffectTypeSerialize.BUFF_ShaderChange.ShaderEnum)
            {
                case ShaderEnum.Freeze:
                    {
                        player.OnShaderStateChange(CharStateController.CharacterState.Frozen, onEnter);
                    }
                    break;
                case ShaderEnum.RedPatches:
                    {
                        player.OnShaderStateChange(CharStateController.CharacterState.RedPatches, onEnter);
                    }
                    break;
                case ShaderEnum.Drench:
                    {
                        player.OnShaderStateChange(CharStateController.CharacterState.Water, onEnter);
                    }
                    break;

                case ShaderEnum.Crazy:
                    {
                        player.OnShaderStateChange(CharStateController.CharacterState.OrangeRing, onEnter);
                    }
                    break;

                default: break;
            }
        }

        public override void OnExit()
        {

            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                player.HandleStackEffect(this, TypeEffectUpdateType.OnExit);

            }

            // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} buffid={BuffID} Time={Time.time}");
            base.OnExit();
        }
    }
}