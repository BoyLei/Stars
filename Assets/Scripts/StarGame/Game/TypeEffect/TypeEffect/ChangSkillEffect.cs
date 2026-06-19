using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Game.TypeEffect;
using UnityEngine;

public class ChangeSkillEffect : BaseTypeEffect
{
    public override void OnEnter(ulong owneruid, int cfgId)
    {
        base.OnEnter(owneruid, cfgId);
        EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
        if (player != null)
        {
            player.StartChangSkillEffect(this);
        }

        // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} CfgID={CfgID} Time={Time.time}");
    }



    public override void OnExit()
    {
        var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
        if (player != null)
        {
            player.StopChangSkillEffect(this);
        }

        // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} CfgID={CfgID} Time={Time.time}");
        base.OnExit();
    }
}

