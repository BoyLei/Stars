using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Entity;
using StarProject.Game.Player;
using StarProject.Game.TypeEffect;
using UnityEngine;

/// <summary>
/// 模拟召唤物的效果
/// 1.根据配置, 生成指定的 本地召唤物实体;
/// 2.后续 看需求 , 拓展召唤物创建时 相应的 动画效果；
/// 3.gl 确认的召唤物需要实现以下几种需求:
///     01.播放动作;
///     02.播放特效;
///     03.持续的朝向目标(播放效果)
///   基于上, 相当于 buff 指定召唤物 播放特定的效果;
/// </summary>
public class SimulateSummonEffect : BaseTypeEffect
{

    // 本地创建出来的召唤物 感觉没必要放在 EntityCtrlBase 里面. 
    // 所以此处 buff 创建,相应的也由 buff 销毁
    // 至于其它效果需要查找本地召唤物, GameManager.Instance.GetLocalEntity() 也可以查找到.
    private Dictionary<string, ulong> summons = new();

    bool isFollowRotate = false;

    public override void OnEnter(ulong owneruid, int cfgId)
    {
        base.OnEnter(owneruid, cfgId);
        EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
        if (player == null)
        {
            return;
        }
        var effect = EffectTypeSerialize.Global_AddClientSummon;

        int avatarID = effect.AvatarID;

        float itemOffsetAngle = 0;

        if (effect.Num > 1)
        {
            itemOffsetAngle = (effect.MaxRot - effect.MinRot) * 1.0f / (effect.Num - 1);
        }


        isFollowRotate = effect.IsFollowPlayerTurn;
        float r = effect.Distance / 100f;
        for (int i = 0; i < effect.Num; i++)
        {
            float angle = effect.MinRot + i * itemOffsetAngle;
            Vector3 pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * r, 0, Mathf.Cos(angle * Mathf.Deg2Rad) * r);

            // gl 配置的 index 都是 1 2 3 ，从1 开始
            CreateIndex(i + 1, pos, avatarID, player);

        }

        InitLoopFxs(effect.LoopEffectInEditors);

        PlaySummonLoopEffects(true);

        PlayTurn(effect.IsTurn, player);
        // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} CfgID={CfgID} Time={Time.time}");
    }



    /// <summary>
    /// 创建几号位
    /// note:
    ///     1.目前来看, 大部分创建指定几号位的 召唤物都是绑定在玩家的节点坐标系下面;
    ///     2.创建 对于的 GameObject, 将它 作为 
    /// </summary>
    private void CreateIndex(int index, Vector3 pos, int avatarID, EntityCtrlBase player)
    {
        string key = GameManager.Instance.FormateLocalEntityKey(RuntimeID, index);

        player.OnCreateSimulateSummon(RuntimeID, key, pos, avatarID, isFollowRotate);

        // 获得 key 对应的 entityID. 上面创建实体是同步
        ulong entityID = GameManager.Instance.GetFormateKey2EntityID(key);

        // 模拟召唤物 相对应的key
        summons.Add(key, entityID);
    }

    private List<FxParam> loopFxs = new();

    private void InitLoopFxs(List<EffectTypeHitEffect> LoopEffect)
    {
        List<FXJson> LoopEffectFxs = new List<FXJson>();
        ConverFx.LoopEffects2FxJsons(LoopEffect, LoopEffectFxs);

        for (int i = 0; i < LoopEffectFxs.Count; i++)
        {
            FxParam fxParam = new FxParam();
            fxParam.InitWithFxJson(LoopEffectFxs[i], BuilderID, ownerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_loopEffectFx");

            loopFxs.Add(fxParam);
        }
    }

    private void PlaySummonLoopEffects(bool isCreate)
    {
        for (int i = 0; i < loopFxs.Count; i++)
        {
            PlaySummonLoopEffect(loopFxs[i], isCreate);
        }
    }

    private void PlaySummonLoopEffect(FxParam fxParam, bool isCreate)
    {
        foreach (var item in summons)
        {
            string key = item.Key;
            ulong entityID = item.Value;

            EntityLocalStatic entity = GameManager.Instance.GetLocalEntity(entityID);
            if (entity != null)
            {
                entity.PlaySpecialEffect(fxParam, isCreate);
            }
        }
    }

    /// <summary>
    /// 播放召唤物的转向
    /// </summary>
    private void PlayTurn(int angle, EntityCtrlBase player)
    {
        if (angle == 0)
        {
            return;
        }
        player.OnSimulateSummonTurn(RuntimeID, angle, isFollowRotate, true);
    }

    private void StopPlayTurn()
    {
        EntityCtrlBase player = GameManager.Instance.GetEntityCtr(ownerEntityID);
        if (player == null)
        {
            return;
        }
        player.OnSimulateSummonTurn(RuntimeID, 0, isFollowRotate, false);
    }


    public override void OnExit()
    {
        // 关闭召唤物根节点的自转
        StopPlayTurn();

        // 先关闭 循环特效
        PlaySummonLoopEffects(false);

        foreach (var item in summons)
        {
            string key = item.Key;
            ulong entityID = item.Value;

            GameManager.Instance.RemoveFormateKey2EntityID(key);
            GameManager.Instance.RemoveLocalEntity(entityID);
        }


        summons.Clear();

        // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} CfgID={CfgID} Time={Time.time}");
        base.OnExit();
    }
}

