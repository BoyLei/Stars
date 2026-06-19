using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    public class ParalysisBuffEffect : BaseTypeEffect
    {
        /// <summary>
        /// 麻痹(类似冰冻效果，但没有shader表现，只是动作暂停)
        /// </summary>

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                player.PauseAniamtion(true);
                //player.Freez(true);
            }

            // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} buffid={buffid} Time={Time.time}");
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                player.PauseAniamtion(false);
                //player.Freez(false);
            }

            // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} buffid={BuffID} Time={Time.time}");
            base.OnExit();
        }
    }
}