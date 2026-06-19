using System;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using SGF.Utlis;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    public class KnockDownBuffEffect : BaseTypeEffect
    {
        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            var player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                Transform m_container = UnityExtension.FindFunc(player.Container.transform, "Control_BegineDown");
                if (m_container != null)
                {
                    VitalState vitalState = m_container.GetComponent<VitalState>();
                    float time = vitalState.ClipDuration;
                    I_AnimParam animParam = player.Data.myOwnerNtt.GetAnimParamByState((E_ULayerSubState)GameKeyCommand.BegineKnockDown);
                    player.Data.myOwnerNtt.ChangeState(GameKeyCommand.BegineKnockDown, animParam, false);

                    DelayInvoker.DelayInvoke(time, OnDelayKnockDown);
                }


            }
            SGF.Debuger.Log($"OnEnter:: owneruid={owneruid} buffid={buffid} Time={Time.time}");
        }

        private void OnDelayKnockDown(object[] args)
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                I_AnimParam animParam = player.Data.myOwnerNtt.GetAnimParamByState((E_ULayerSubState)GameKeyCommand.KnockDown);
                player.Data.myOwnerNtt.ChangeState(GameKeyCommand.KnockDown, animParam, false);
            }
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null)
            {
                I_AnimParam animParam = player.Data.myOwnerNtt.GetAnimParamByState((E_ULayerSubState)GameKeyCommand.EndKnockDown);
                player.Data.myOwnerNtt.ChangeState(GameKeyCommand.EndKnockDown, animParam, false);
            }

            SGF.Debuger.Log($"OnExit::owneruid={ownerEntityID} buffid={CfgID} Time={Time.time}");
            base.OnExit();
        }
    }
}