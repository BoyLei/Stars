using System;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using SGF.Utlis;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    public class ShadowFollowBuffEffect : BaseTypeEffect
    {
        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            // SGF.Debuger.LogError($"[shadow] OnEnter:: owneruid={owneruid} buffid={CfgID} Time={Time.time}");
            if (player != null)
            {
                HandleShaderFollow(true, player);
                // string path = BuffInfo.Cfg.ShadowPath;
                // player.CreateShadowView(path);
            }
        }

        private void HandleShaderFollow(bool onEnter, EntityCtrlBase player)
        {
            if (onEnter)
            {
                string path = EffectTypeSerialize.BUFF_ShadowFollow.ShadowPath;
                player.CreateShadowView(path);
            }
            else
            {
                player.HiddenShadowView();
            }
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            // SGF.Debuger.LogError($"[shadow] OnExit::owneruid={ownerEntityID} buffid={CfgID} Time={Time.time}");
            if (player != null)
            {
                HandleShaderFollow(false, player);
                // player.HiddenShadowView();
            }

            base.OnExit();
        }
    }
}