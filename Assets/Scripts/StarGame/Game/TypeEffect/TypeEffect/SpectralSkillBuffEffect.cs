using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;

namespace StarProject.Game.TypeEffect
{
    public class SpectralSkillBuffEffect : BaseTypeEffect
    {
        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null)
            {
                HandleSpectralSkill(true, player.M_Curr as NPCEntityBase);
            }
        }

        private void HandleSpectralSkill(bool onEnter, NPCEntityBase player)
        {
            //SGF.Debuger.LogError($"BUFFÁ¿Æ× ------------------------------- onEnter={onEnter}");
            player.ActionOnSpectralSkillBuff?.Invoke(onEnter);
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            //SGF.Debuger.LogError($"[shadow] OnExit::owneruid={ownerEntityID} buffid={BuffID} Time={Time.time}");
            if (player != null)
            {
                HandleSpectralSkill(false, player.M_Curr as NPCEntityBase);
            }

            base.OnExit();
        }
    }
}