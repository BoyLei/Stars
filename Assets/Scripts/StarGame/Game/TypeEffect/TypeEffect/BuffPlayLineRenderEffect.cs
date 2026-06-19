using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Game.TypeEffect;
using UnityEngine;

/// <summary>
/// buff 播放 lineRender效果
/// </summary>
public class BuffPlayLineRenderEffect : BaseTypeEffect
{
    public override void OnEnter(ulong owneruid, int cfgId)
    {
        base.OnEnter(owneruid, cfgId);
        EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
        if (player != null)
        {
            HandleLineRender(true, player);
        }

        // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} CfgID={CfgID} Time={Time.time}");
    }

    private void HandleLineRender(bool onEnter, EntityCtrlBase player)
    {
        var lineRenderCfg = EffectTypeSerialize.BUFF_PlayEffectLineRenderer.LineRendererConfig;

        // 从 buff 黑板中 取 相关的数据, 目前 效果中 关联了 buffRuntime 其实并不是 一个 很好的方法, 后面 要想个独立出来的方法
        var fromKey = lineRenderCfg.FromTarget.Result;
        var starts = EffectUtils.GetBlackBoardKeyTarget(fromKey, BlackBoard).KToList();

        var targetKey = lineRenderCfg.ToTarget.Result;
        var targets = EffectUtils.GetBlackBoardKeyTarget(targetKey, BlackBoard).KToList();

        var tagKey = $"[Effect_{CfgID}]_{RuntimeID}_{GetHashCode()}";
        if (onEnter)
        {
            player.ControlPlayLineRender(lineRenderCfg, starts, targets, 0, tagKey);
        }
        else
        {
            player.ControlCloseLineRender(lineRenderCfg, starts, targets, tagKey);
        }

    }

    public override void OnExit()
    {
        var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
        if (player != null)
        {
            HandleLineRender(false, player);
        }

        // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} CfgID={CfgID} Time={Time.time}");
        base.OnExit();
    }
}
