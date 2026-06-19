using System;
using System.Collections;
using System.Collections.Generic;
using Animancer.FSM;
using DG.Tweening;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.LocalDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProjectDef;
using UnityEngine;


/// <summary>
/// 本地模拟的召唤物表现层逻辑.
/// note:
///     1.召唤物有两种, 一种跟随主角一起位移. 这种召唤物的 父节点是 summonHost
///     2.一种是 独立于主角的召唤物, 坐标系为 世界坐标系
///     3.本地召唤物 目前都只需要实现 模型加载 特效播放 以及对应的位移即可
/// </summary>
public class SummonView : ViewLocalStatic, I_VVitalAnim
{
    private readonly StateMachine<VitalState>.WithDefault _StateMachine = new StateMachine<VitalState>.WithDefault();
    public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;

    private VitalState Stage_Idle;
    private VitalState Stage_Dead;

    /// <summary>
    /// 放置独有的动作 比如 idle ,dead
    /// </summary>
    private Dictionary<string, VitalState> VitalStates = new Dictionary<string, VitalState>();

    /// <summary>
    /// 放置通用的 ，如交互 进入等
    /// </summary>
    private List<VitalState> CommonVitalStates = new List<VitalState>();

    private bool FollowNodeIsParent = false;
    private Transform FollowNode = null;

    protected override void Create(EntityObject entity)
    {
        //find，手绑定editor，工具
        BindState();

        base.Create(entity);

        var summonEntity = entity as LocalSummonEntity;
        FollowNodeIsParent = summonEntity.FollowNodeIsParent;
        FollowNode = summonEntity.FollowNode;

        Stage_Idle.IdleState += OnIdleStateAction;
        StateMachine.DefaultState = Stage_Idle;
    }

    protected override void Release()
    {
        VitalStates.Clear();
        CommonVitalStates.Clear();

        FollowNodeIsParent = false;
        FollowNode = null;

        base.Release();
    }

    protected override void OnEventListener()
    {
        base.OnEventListener();

        if (m_entity != null)
        {
            m_entity.PlayAnimationAction += PlayIAnimImmediateAsync;
            m_entity.SetIdleAnimation += SetIdleAnimationAsync;

            MonoHelper.AddFixedUpdateListener(OnEnterFrame);
        }
    }

    protected override void OffEventListener()
    {
        base.OffEventListener();

        if (m_entity != null)
        {
            m_entity.PlayAnimationAction -= PlayIAnimImmediateAsync;
            m_entity.SetIdleAnimation -= SetIdleAnimationAsync;

            MonoHelper.RemoveFixedUpdateListener(OnEnterFrame);
        }
    }

    private void BindState()
    {
        Stage_Idle = transform.Find("ModelOffset/StateMachines/Idle").GetComponent<VitalState>();
        var Common_1 = transform.Find("ModelOffset/StateMachines/Common_1").GetComponent<VitalState>();
        var Common_2 = transform.Find("ModelOffset/StateMachines/Common_2").GetComponent<VitalState>();
        Stage_Dead = transform.Find("ModelOffset/StateMachines/Dead").GetComponent<VitalState>();

        VitalStates.Add(ObjectSubState.Idle.ToString(), Stage_Idle);
        VitalStates.Add(ObjectSubState.Dead.ToString(), Stage_Dead);

        Stage_Idle.Character = this;
        Common_1.Character = this;
        Common_2.Character = this;
        Stage_Dead.Character = this;

        CommonVitalStates.Add(Common_1);
        CommonVitalStates.Add(Common_2);
    }

    #region 【委托回调】
    private void OnEnterFrame()
    {
        // 如果followNode 就是 召唤物的父节点, 那就不需要每帧跟随
        if (FollowNodeIsParent)
        {
            return;
        }

        SetPosition(FollowNode.position);
    }

    private void SetIdleAnimationAsync(string AnimationName)
    {
        string path = $"{m_entity.avatarDataCell.AnimsPath}/{AnimationName}";
        path = path.Replace("Assets/Res/", string.Empty);
        path = path.Replace(".anim", string.Empty);

        StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
        (AnimationClip animationClip) =>
        {
            if (animationClip != null && m_entity != null)
            {
                Stage_Idle.SetClip(animationClip);
            }
        });

        Stage_Idle.FadeTimeMilSeconds = -1;
        Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.Idle);
        StateMachine.ForceSetState(Stage_Idle);
    }

    private VitalState PlayIAnimImmediateAsync(I_AnimParam animParam, string VitalStateName = "")
    {
        bool find = false;
        VitalState vitalState = GetVitalState(VitalStateName, out find);
        if (vitalState != null)
        {
            if (!find && m_entity != null && m_entity.modelDataCell != null)
            {
                string path = animParam.GetCurAnimationPath(m_entity.avatarDataCell);
                StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                (AnimationClip animationClip) =>
                {
                    if (animationClip != null && m_entity != null)
                    {
                        vitalState.SetClip(animationClip);
                        vitalState.speed = animParam.Speed;
                        Stage_Idle.FadeTimeMilSeconds = -1;
                        Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.Performance);
                        StateMachine.TrySetState(vitalState);
                    }
                });
            }
        }
        return vitalState;
    }

    public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action endStateCB)
    {

    }

    private void OnIdleStateAction(int obj)
    {
        //m_entity.ForceSetDefaultState();
    }

    #endregion

    #region 获取状态机

    private VitalState GetVitalState(string VitalStateName, out bool find)
    {
        VitalState vitalState = null;
        find = false;
        if (!string.IsNullOrEmpty(VitalStateName))
        {
            VitalStates.TryGetValue(VitalStateName, out vitalState);
            find = vitalState != null;
        }

        if (vitalState == null)
        {
            //使用通用的
            if (CommonVitalStates != null)
            {
                foreach (var item in CommonVitalStates)
                {
                    if (!item.enabled)
                    {
                        vitalState = item;
                        break;
                    }
                }
            }
        }
        return vitalState;
    }

    #endregion

    #region 召唤物的 移动.
    /// 召唤物的移动分为2种:
    /// 1.直接 跟随 soummonHost 一起移动.(可能自己也会有一些旋转之类的, 但坐标依旧是跟随主人)
    /// 2.脱离 主人自己的移动. 比如主角 往前面移动个10m, 召唤物加个缓动跟随的移动效果. 
    ///     这种就是脱离了 主人,客户端完全移动的模拟
    ///    
    ///   对于1， 召唤物直接挂载在 主人的特定节点之下;
    ///   对于2,  召唤物需要 自己去 跟随到目标节点





    #endregion

}
