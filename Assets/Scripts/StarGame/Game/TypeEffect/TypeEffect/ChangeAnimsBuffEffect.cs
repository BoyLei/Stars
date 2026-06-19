using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    public class ChangeAnimsBuffEffect : BaseTypeEffect
    {

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                // List<ChangeAnim> changeAnims = BuffInfo.Cfg.ChangeAnims;
                // player.RegisterChangeAnim(RuntimeID.ToString(), changeAnims);
                HandleChangeAnim(true, player);
            }

        }

        private void HandleChangeAnim(bool onEnter, EntityCtrlBase player)
        {
            List<ChangeAnim> changeAnims = EffectTypeSerialize.BUFF_ChangeAnim.ChangeAnims;

            if (onEnter)
            {
                player.RegisterChangeAnim(RuntimeID.ToString(), changeAnims);
            }
            else
            {
                player.UnRegisterChangeAnim(RuntimeID.ToString(), changeAnims);

            }
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                // List<ChangeAnim> changeAnims = BuffInfo.Cfg.ChangeAnims;
                // player.UnRegisterChangeAnim(RuntimeID.ToString(), changeAnims);
                HandleChangeAnim(false, player);
            }

            base.OnExit();
        }

    }
}
