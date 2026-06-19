using System;
using System.Collections;
using System.Collections.Generic;
using Animancer.FSM;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.LocalDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProjectDef;
using UnityEngine;

/// <summary>
/// 交互物类型的 view 基类.
/// ViewLocalDynamic 应该只是个通用 view 基类. 不是所有的类 都需要有交互属性
/// 
/// </summary>
public abstract class ViewInterctive : ViewLocalDynamic, I_VVitalAnim
{
    private GameObject go_interactiveEff;

    private bool m_IsShowInteractiveEff = false;

    private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
    public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;
    private ObjectSubState currentState = ObjectSubState.Default;

    private VitalState Stage_Idle;
    private VitalState Stage_Dead;

    private Dictionary<string, VitalState> VitalStates = new();

    private List<VitalState> CommonVitalStates = new();

    protected override void Create(EntityObject entity)
    {
        BindState();

        base.Create(entity);

        currentState = ObjectSubState.Idle;
        Stage_Idle.IdleState += OnIdleStateAction;
        StateMachine.DefaultState = Stage_Idle;
    }
    protected override void Reset()
    {
        base.Reset();

        m_IsShowInteractiveEff = false;
    }

    protected override void Release()
    {
        VitalStates.Clear();
        CommonVitalStates.Clear();

        base.Release();
    }

    protected override void OnEventListener()
    {
        base.OnEventListener();

        if (m_entity != null)
        {
            m_entity.ActionModelFesnel += OnActionModelFesnel;
            m_entity.ActionModelInteractiveEff += ActionModelInteractiveEff;
            m_entity.PlayAnimationAction += PlayIAnimImmediateAsync;
            m_entity.SetIdleAnimation += SetIdleAnimationAsync;
        }
    }

    protected override void OffEventListener()
    {
        base.OffEventListener();

        if (m_entity != null)
        {
            m_entity.ActionModelFesnel -= OnActionModelFesnel;
            m_entity.ActionModelInteractiveEff -= ActionModelInteractiveEff;
            m_entity.PlayAnimationAction -= PlayIAnimImmediateAsync;
            m_entity.SetIdleAnimation -= SetIdleAnimationAsync;
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
    private void OnIdleStateAction(int obj)
    {
        //m_entity.ForceSetDefaultState();
    }


    public void ActionModelInteractiveEff(bool isShow)
    {
        m_IsShowInteractiveEff = isShow;

        if (m_IsShowInteractiveEff)
        {
            if (go_interactiveEff == null)
            {
                //加载特效
                //添加贴图
                //加载交互特效
                if (m_entity != null && !string.IsNullOrEmpty(m_entity.EffectAddress))
                {
                    Action<GameObject> cb = (go) =>
                    {
                        if (go == null)
                        {
                            SGF.Debuger.LogWarning($"[ViewLocal] ActionModelInteractiveEff() EffectAddress={m_entity.EffectAddress},isShow={isShow},err!!!");
                            return;
                        }
                        go_interactiveEff = GameObject.Instantiate(go);
                        go_interactiveEff.transform.SetParent(transform);
                        go_interactiveEff.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        go_interactiveEff.transform.localPosition = new Vector3(0, 0.3f, 0);
                        go_interactiveEff.SetActive(m_IsShowInteractiveEff);
                    };
                    StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(m_entity.EffectAddress, E_AssetType.Effects, false, cb);
                }
            }
            else
            {
                go_interactiveEff.SetActive(true);
            }
        }
        else
        {
            if (go_interactiveEff != null)
            {
                go_interactiveEff.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 打开关闭模型菲涅尔
    /// </summary>
    /// <param name="isOpen"></param>
    public void OnActionModelFesnel(bool isOpen)
    {
        if (m_entity == null)
        {
            return;
        }
        if (isOpen)
        {
            Transform[] childrenList = m_ModelOffset.transform.GetComponentsInChildren<Transform>();
            foreach (Transform child in childrenList)
            {
                if (m_entity.SpecialEffect == 1)
                {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.material.SetFloat("_FesColorFlag", 1);
                    }
                }
                else if (m_entity.SpecialEffect == 2)
                {

                }
            }
        }
        else
        {
            Transform[] childrenList = m_ModelOffset.transform.GetComponentsInChildren<Transform>();
            foreach (Transform child in childrenList)
            {
                if (m_entity.SpecialEffect == 1)
                {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.material.SetFloat("_FesColorFlag", 0);
                    }
                }
                else
                {
                    //删除贴图
                }
            }
        }
    }

    public VitalState PlayIAnimImmediateAsync(I_AnimParam animParam, string VitalStateName = "")
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
                        Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.InterAction1);
                        StateMachine.TrySetState(vitalState);
                    }
                });
            }
            //Stage_Idle.FadeTimeMilSeconds = -1;
            //Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.InterAction1);
            //StateMachine.TrySetState(vitalState);
        }
        return vitalState;
    }

    public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action endStateCB)
    {

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
            else
            {
                SGF.Debuger.LogWarning($"[WantedView] SetIdleAnimationAsync AnimationName={AnimationName},path={path},animationClip=null");
            }
            if (currentState == ObjectSubState.Idle)
            {
                Stage_Idle.FadeTimeMilSeconds = -1;
                Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.Idle);
                StateMachine.ForceSetState(Stage_Idle);
            }
        });
    }

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

}
