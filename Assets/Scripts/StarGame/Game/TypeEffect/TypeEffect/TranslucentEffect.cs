using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Game.TypeEffect;
using UnityEngine;

public class TranslucentEffect : BaseTypeEffect
{
    public float curAlpha = 0;

    private TweenerCore<float, float, FloatOptions> translucentTween;
    public override void OnEnter(ulong owneruid, int cfgId)
    {
        base.OnEnter(owneruid, cfgId);
        EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
        if (player != null)
        {
            player.StartTranslucentEffect(this);
        }

        var effect = EffectTypeSerialize.BUFF_TransChange;

        float startAlpha = effect.StartTrans / 100f;
        float endAlpha = effect.EndTrans / 100f;
        float time = effect.ChangeTime / 1000f;
        translucentTween = DOTween.To(() => startAlpha, changeAlpha =>
           {
               startAlpha = changeAlpha;
               curAlpha = changeAlpha;
               UpdateTranslucent();
           }, endAlpha, time);

        // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} CfgID={CfgID} Time={Time.time}");
    }

    private void UpdateTranslucent()
    {
        var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
        if (player != null)
        {
            player.UpdateTranslucent(this);
        }
    }



    public override void OnExit()
    {
        translucentTween.Kill(true);
        translucentTween = null;

        var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
        if (player != null)
        {
            player.StopTranslucentEffect(this);
        }

        // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} CfgID={CfgID} Time={Time.time}");
        base.OnExit();
    }
}

