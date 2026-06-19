using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Data;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    public class Touch2UseSkillEffect : ScreenTouchEffect
    {
        public override void OnTouchScreen(object v, EntityCtrlBase player)
        {
            base.OnTouchScreen(v, player);
            Vector3 pos = (Vector3)v;
            int skillID = EffectTypeSerialize.BUFF_ClickUseSkill.UseSkill;

            SGF.Debuger.Log($"屏幕 点击 使用 技能 : {skillID} , pos: {pos.ToString()} ");
            player.Data.TriggerChange(GameConfig.TOUCH_USE_SKILL_EVENT, new TriggerTypeEffectData(GameConfig.TOUCH_USE_SKILL_EVENT, new KeyValuePair<int, Vector3>(skillID, pos)));
        }
    }
}
