///--------------------------------------------------------------------
/// 文件名   :   ViewVitalObjectAnim
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/27 18:39:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Animancer.FSM;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalObjectAnim : ViewInterActionObject, I_VVitalAnim
    {
        private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
        public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;

        private ObjectSubState currentState = ObjectSubState.Default;

        private VitalState Stage_Idle;
        private VitalState Stage_Dead;

        /// <summary>
        /// 放置独有的动作 比如 idle ,dead
        /// </summary>
        private Dictionary<string, VitalState> VitalStates = new();

        private List<VitalState> CommonVitalStates = new();

        protected override void Create(EntityObject entity)
        {
            //find，手绑定editor，工具
            BindState();

            base.Create(entity);

            currentState = ObjectSubState.Idle;
            Stage_Idle.IdleState += OnIdleStateAction;
            StateMachine.DefaultState = Stage_Idle;
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
                m_entity.PlayAnimationAction += PlayIAnimImmediateAsync;
                m_entity.SetIdleAnimation += SetIdleAnimationAsync;

                m_entity.ActionOnPlayWwiseAudio += OnActionOnPlayWwiseAudio;

                m_entity.ActionOnPlaySpecialEffects += BasePlayEfxAsync;
                m_entity.ActionOnStopSpecialEffects += BaseStopFx;
            }
        }

        protected override void OffEventListener()
        {
            base.OffEventListener();

            if (m_entity != null)
            {
                m_entity.PlayAnimationAction -= PlayIAnimImmediateAsync;
                m_entity.SetIdleAnimation -= SetIdleAnimationAsync;

                m_entity.ActionOnPlayWwiseAudio -= OnActionOnPlayWwiseAudio;

                m_entity.ActionOnPlaySpecialEffects -= BasePlayEfxAsync;
                m_entity.ActionOnStopSpecialEffects -= BaseStopFx;
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
        public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action action)
        {

        }

        private VitalState PlayIAnimImmediateAsync(I_AnimParam animParam, string VitalStateName = "")
        {
            // 是否找到了特定的
            bool find = false;
            VitalState vitalState = GetVitalState(VitalStateName, out find);
            if (vitalState != null)
            {
                if (!find && m_entity != null && m_entity.avatarDataCell != null)
                {
                    string path = animParam.GetCurAnimationPath(m_entity.avatarDataCell);
                    StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                    (AnimationClip animationClip) =>
                    {
                        if (animationClip != null && m_entity != null)
                        {
                            vitalState.SetClip(animationClip);
                            vitalState.speed = animParam.Speed;
                            StateMachine.TrySetState(vitalState);
                        }
                    });
                }
                Stage_Idle.FadeTimeMilSeconds = -1;
                Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.InterAction1);
                StateMachine.TrySetState(vitalState);
            }
            return vitalState;
        }

        private void SetIdleAnimationAsync(string AnimationName)
        {
            if (m_entity == null || m_entity.avatarDataCell == null)
            {
                return;
            }
            string path = $"{m_entity.avatarDataCell.AnimsPath}/{AnimationName}";
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".anim", string.Empty);

            StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
            (AnimationClip animationClip) =>
            {
                if (animationClip != null && m_entity != null)
                {
                    Stage_Idle.SetClip(animationClip);
                    StateMachine.ForceSetState(Stage_Idle);
                }
            });

            if (currentState == ObjectSubState.Idle)
            {
                Stage_Idle.FadeTimeMilSeconds = -1;
                Stage_Idle.SetStateDefaultFadeTime(E_ULayerSubState.Idle);
                StateMachine.ForceSetState(Stage_Idle);
            }
        }

        private void OnIdleStateAction(int obj)
        {
            m_entity?.ForceSetDefaultState();
        }

        private void OnActionOnPlayWwiseAudio(string eventName, E_SoundNTFtype ftype)
        {
            GameObject soundMaker = null;
            if (ftype == E_SoundNTFtype.Target)
            {
                soundMaker = this.gameObject;
            }
            SoundManager.Instance.PlayWwiseAudio(eventName, false, ftype, soundMaker);
        }

        #endregion

        #region 获取状态机

        /// <summary>
        /// 获取VitalState  获取空闲的VitalState
        /// </summary>
        /// <param name="VitalStateName"></param>
        /// <returns></returns>
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
}