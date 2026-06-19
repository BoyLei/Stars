using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    /// <summary>
    /// 隐身效果
    /// </summary>
    public class InvisibleBuffEffect : BaseTypeEffect
    {
        private bool hiddenFlag = false;

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            hiddenFlag = false;
            /// 隐身效果 
            /// 对于 主角来说, 隐身效果的拥有者是否需要隐身,取决于 他是否是 主角的 同一阵营.
            /// 目前与 gl 的约定, 隐身 会敌方  和 中立 阵营都 有效
            /// 所以,隐身对自己来说 其实是没效果的
            if (owneruid == GameManager.Instance.mainPlayerId)
            {
                return;
            }
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
                // 中立阵营
                List<int> NeutralFaction = mainPlayer.M_Curr.NeutralFaction;
                // 敌对阵营
                List<int> OpposingFaction = mainPlayer.M_Curr.OpposingFaction;

                var playerFaction = player.M_Curr.Faction;

                // 如果 效果的拥有者 在 主角的 中立阵营或者敌对阵营 , 那么就将这个玩家隐身
                if (NeutralFaction.Contains(playerFaction) || OpposingFaction.Contains(playerFaction))
                {
                    player.SetModelVisiable(false);
                    hiddenFlag = true;
                }
            }
        }

        public override void OnExit()
        {
            if (!hiddenFlag)
            {
                return;
            }
            hiddenFlag = false;
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                player.SetModelVisiable(true);
            }

            base.OnExit();
        }

    }
}
