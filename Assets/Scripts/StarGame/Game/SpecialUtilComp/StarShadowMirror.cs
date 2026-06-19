using Animancer;
using Animancer.FSM;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarProject.Game.SpecialUtilComp
{
    //大招的一种注册方式，因为大招会有很多，不可能拓展再脚本里面
    //作为插件组件的一种方式
    //通常伙伴是，ctrl创建管理，逻辑，表现，控制
    //这里需要是：注册进去，然后让表现/逻辑层，具有插件样式的能力
    public class StarShadowMirror : ViewModel, I_VVitalAnim
    {
        private string TagFlag = "[StarShadowMirror]";

        private float DelayTime;// = Time.fixedDeltaTime * 10f;//2f;

        private ViewVitalAnim vva;  // 主人的动画层
        protected EntityObject m_entity;
        public HeroEntityBase M_EntityBase
        {
            get
            {
                return m_entity as HeroEntityBase;
            }
        }
        private EntityShowHidenTag ShowHidenTag = EntityShowHidenTag.None;

        private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
        public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;

        private E_ULayerSubState currentState = E_ULayerSubState.Default;
        public E_ULayerSubState CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;
            }
        }

        private VitalState Stage_Idle;
        private List<VitalState> CommonVitalStates = new();
        private VitalState NextVitalState;
        //private bool m_IsInit = true;
        // 标识脏状态-》下一帧切换
        private bool dirtyState = false;

        public static StringBuilder sb = new();
        private string modelPath = "";
        public override string ModelPath => modelPath;


        private string pathKey = "";
        private string angelKey = "";
        private string animKey = "";

        protected override void Create(EntityObject entity, string _modelPath)
        {
            base.Create(entity);
            m_entity = entity;

            BindState();
            m_ModelOffset = transform.Find("ModelOffset");

            modelPath = _modelPath;

            //DelayTime = Time.fixedDeltaTime * 6f;//主要是update到fix中就对了时间无所谓的 ，、0.3f;  表现到逻辑也对，表现也对； 逻辑代码里面也是对的（看需要）
            DelayTime = 0;
            //Debug.Log(DelayTime);
            //KEngineDebuger.log
            //GFrameDebugera
            //SGF.Debuger.LogError(DelayTime + "----------------------------");

            ShowHidenTag = EntityShowHidenTag.Self | EntityShowHidenTag.MirrorShadow;

            pathKey = $"{TagFlag}_Path_{GetHashCode()}_";
            angelKey = $"{TagFlag}_angle_{GetHashCode()}_";
            animKey = $"{TagFlag}_Anim_{GetHashCode()}_";

            if (M_EntityBase != null)
            {
                pathKey += M_EntityBase.EntityId;
                angelKey += M_EntityBase.EntityId;
                animKey += M_EntityBase.EntityId;

                vva = transform.parent.GetComponentInChildren<ViewVitalAnim>();
                vva.OnAngelChange += OnAngelChange;
                vva.OnStateChange += OnStateChange;

                M_EntityBase.OnPosChange += OnPosChange;
                M_EntityBase.ControlShowHide += OnControlShadowShowHide;//这里可以控制主角的隐藏，伙伴隐藏，以及影子隐藏，接入就隐藏呗
                M_EntityBase.CheckHasControllerShow += onCheckHasControllerShow;//这里可以控制主角的隐藏，伙伴隐藏，以及影子隐藏，接入就隐藏呗

                transform.position = M_EntityBase.Position();
                transform.eulerAngles = vva.GetRotation();
                transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
            }

            Action<bool> modelFinish = (res) =>
            {
                if (!res)
                {
                    SGF.Debuger.LogError($"{TagFlag} Create() entityid={M_EntityBase.EntityId},entitytype={M_EntityBase.EntityType},ModelPath={ModelPath},模型加载失败 err!!!");
                    return;
                }

                if (M_EntityBase != null)
                {

                }

                if (vva != null)
                {
                    OnStateChange(vva.M_NextVitalState);
                }
            };
            CreateModelAsync(modelFinish);

            StateMachine.DefaultState = Stage_Idle;
            CurrentState = E_ULayerSubState.Idle;
        }

        private void CreateModelAsync(Action<bool> createFinish = null)
        {
            if (M_EntityBase != null)
            {
                RefreshModelAsync(createFinish);
            }
        }

        private void RefreshModelAsync(Action<bool> callback)
        {
            // 模型的标签 tag 规则:
            // 先检查 是否有 modelDataCell, 如果有, 那不管这个资源 能不能通过这个路径加载到, tag 都是配置的路径;
            // 如果 modelDataCell 没有, 就按 实体类型判定。
            // 由上, 不管 资源有没有, 其实 tag 是完全确定的. 那么在切换模型的时候, 可以不销毁模型, 根据tag 查找之前是否缓存了这个 model
            sb.Clear();
            string tag = sb.Append(modelPath).Replace(".prefab", string.Empty).Replace("Assets/Res/", string.Empty).ToString();
            // 首先 关闭当前的 模型节点
            if (M_ModelOffLineData != null && M_ModelOffLineData.transform != null)
            {
                M_ModelOffLineData.transform.gameObject.SetActive(false);
            }

            GameObject gob = null;
            // 如果找到了 之前隐藏的模型,那就直接显示之前隐藏的模型就可以了
            if (ModelRecord.TryGetValue(tag, out GameObject gob2))
            {
                if (gob2 != null)
                {
                    gob = gob2;
                    gob.SetActive(true);
                }
                else
                {
                    // 如果节点被销毁了(之前切换模型 直接执行的 destroy), 就移除之前的记录
                    ModelRecord.Remove(tag);
                }
            }
            // 如果 找不到 相应的 模型子节点, 那就 新创建对应的模型
            if (gob == null)
            {
                Action<GameObject> cb = (go) =>
                {
                    if (go != null)
                    {
                        LoadModelCallBack(go, tag);
                    }
                    callback?.Invoke(go != null);
                };
                CreateModelAsync(tag, cb);
            }
            else
            {
                LoadModelCallBack(gob, tag);
                callback?.Invoke(true);
            }
        }

        private void LoadModelCallBack(GameObject gob, string tag)
        {
            if (gob == null)
            {
                SGF.Debuger.LogError($"{TagFlag} LoadModelCallBack() tag={tag},gob={gob},err!!!");
                return;
            }

            gob.transform.localEulerAngles = Vector3.zero;

            RefreshModelAnimancer(gob);

            RefreshModelOfflineData(gob);

            ModelRecord.Add(tag, gob);

            SelfParticleSystem = gob.GetComponent<ParticleSystem>();

            gob.SetActive(true);
        }

        protected override void Release()
        {
            base.Release();

            if (vva != null)
            {
                vva.OnAngelChange -= OnAngelChange;
                vva.OnStateChange -= OnStateChange;
            }
            if (M_EntityBase != null)
            {
                M_EntityBase.OnPosChange -= OnPosChange;
                M_EntityBase.ControlShowHide -= OnControlShadowShowHide;
                M_EntityBase.CheckHasControllerShow -= onCheckHasControllerShow;//这里可以控制主角的隐藏，伙伴隐藏，以及影子隐藏，接入就隐藏呗
            }

            CommonVitalStates.Clear();
            //m_IsInit = true;

            vva = null;
            m_entity = null;

            DelayInvoker.CancelInvoke(pathKey);
            DelayInvoker.CancelInvoke(angelKey);
            DelayInvoker.CancelInvoke(animKey);
        }

        private void BindState()
        {
            Stage_Idle = transform.Find("ModelOffset/StateMachines/Idle").GetComponent<VitalState>();
            Stage_Idle.Character = this;

            var Common_1 = transform.Find("ModelOffset/StateMachines/Common_1").GetComponent<VitalState>();
            var Common_2 = transform.Find("ModelOffset/StateMachines/Common_2").GetComponent<VitalState>();
            Common_1.Character = this;
            Common_2.Character = this;
            CommonVitalStates.Add(Common_1);
            CommonVitalStates.Add(Common_2);
        }

        private void OnDestroy()
        {
            Release();
        }

        // -----------------------------------------
        // -----------------------------------------
        // -----------------------------------------

        public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action action)
        {
            //
        }

        private void OnControlShadowShowHide(EntityShowHidenTag tag, bool show)
        {
            if (EntityShowHidenTag.Self == tag || EntityShowHidenTag.FollwShadow == tag)
            {
                if (show)
                {
                    ShowHidenTag = ShowHidenTag | tag;
                }
                else
                {
                    ShowHidenTag = ShowHidenTag & ~tag;
                }


                //每个人的隐藏方式有很多种类，有些人是meshRender，但是我的隐藏是直接ActiveFalse
                if ((ShowHidenTag & EntityShowHidenTag.Self) == EntityShowHidenTag.Self && (ShowHidenTag & EntityShowHidenTag.FollwShadow) == EntityShowHidenTag.FollwShadow)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private bool onCheckHasControllerShow(EntityShowHidenTag tag)
        {
            if (EntityShowHidenTag.FollwShadow == tag)
            {
                return true;
            }
            return false;
        }

        private void OnStateChange(VitalState vitalState)
        {
            object[] Os = new object[] { vitalState };
            DelayInvoker.DelayInvoke(animKey, DelayTime,
                  (object[] args) =>
                  {
                      VitalState newVitalState = (VitalState)args[0];
                      E_ULayerSubState currentState = newVitalState.AnimState;
                      AnimationClip animationClip = newVitalState.Clip;
                      switch (currentState)
                      {
                          case E_ULayerSubState.Idle:
                              {
                                  Stage_Idle.AnimState = E_ULayerSubState.Idle;
                                  bool res = RefreshStateAnim(Stage_Idle, animationClip);
                                  if (res)
                                  {
                                      Stage_Idle.isNeedPlayFromStart = false;
                                  }
                                  Stage_Idle.FadeTimeMilSeconds = newVitalState.FadeTimeMilSeconds;
                                  EnterNewState(Stage_Idle);
                              }
                              break;
                          case E_ULayerSubState.BattleIdle:
                          case E_ULayerSubState.WeaponRetractionIdle:
                          case E_ULayerSubState.WanderMoving:
                          case E_ULayerSubState.SingleMoving:
                          case E_ULayerSubState.WeaponRetractionMoving:
                          case E_ULayerSubState.BattleMoving:
                          case E_ULayerSubState.Deading:
                              {
                                  VitalState vitalState = GetFreeVitalState(currentState.ToString());
                                  if (vitalState != null)
                                  {
                                      vitalState.AnimState = currentState;
                                      bool res = RefreshStateAnim(vitalState, animationClip);
                                      if (res)
                                      {
                                          vitalState.speed = newVitalState.speed;
                                          vitalState.isNeedPlayFromStart = false;
                                          vitalState.FadeTimeMilSeconds = newVitalState.FadeTimeMilSeconds;
                                          EnterNewState(vitalState);
                                      }
                                  }
                              }
                              break;
                          case E_ULayerSubState.BegineKnockDown:  // 目前没动作 
                          case E_ULayerSubState.KnockDown:        // 目前没动作
                          case E_ULayerSubState.EndKnockDown:     // 目前没动作
                          case E_ULayerSubState.IsSonscious:      // 目前没动作
                          case E_ULayerSubState.WeakNess:         // 目前没动作
                          case E_ULayerSubState.BeingControl:     // 目前没动作
                              {
                              }
                              break;
                          case E_ULayerSubState.Hurt:
                              {
                                  VitalState vitalState = GetFreeVitalState(currentState.ToString());
                                  if (vitalState != null)
                                  {
                                      vitalState.AnimState = currentState;
                                      bool res = RefreshStateAnim(vitalState, animationClip);
                                      if (res)
                                      {
                                          vitalState.speed = newVitalState.speed;
                                          vitalState.isNeedPlayFromStart = false;
                                          vitalState.FadeTimeMilSeconds = newVitalState.FadeTimeMilSeconds;
                                          EnterNewState(vitalState);
                                      }
                                  }
                              }
                              break;
                          case E_ULayerSubState.InterAction1:
                          case E_ULayerSubState.InterAction2:
                              {
                                  VitalState vitalState = GetFreeVitalState(currentState.ToString());
                                  if (vitalState != null)
                                  {
                                      vitalState.AnimState = currentState;
                                      bool res = RefreshStateAnim(vitalState, animationClip);
                                      if (res)
                                      {
                                          vitalState.speed = newVitalState.speed;
                                          vitalState.SetStartTime(newVitalState.NormalizedStartTime);
                                          vitalState.isNeedPlayFromStart = false;
                                          vitalState.FadeTimeMilSeconds = newVitalState.FadeTimeMilSeconds;
                                          EnterNewState(vitalState);
                                      }
                                  }
                              }
                              break;
                          case E_ULayerSubState.Skill_Common1:
                          case E_ULayerSubState.Skill_Common2:
                              {
                                  VitalState vitalState = GetFreeVitalState(currentState.ToString());
                                  if (vitalState != null)
                                  {
                                      vitalState.AnimState = currentState;
                                      bool res = RefreshStateAnim(vitalState, animationClip);
                                      if (res)
                                      {
                                          vitalState.speed = newVitalState.speed;
                                          vitalState.SetStartTime(newVitalState.NormalizedStartTime);
                                          vitalState.isNeedPlayFromStart = newVitalState.isNeedPlayFromStart;
                                          vitalState.FadeTimeMilSeconds = newVitalState.FadeTimeMilSeconds;
                                          EnterNewState(vitalState);
                                      }
                                  }
                              }
                              break;
                          default:
                              break;
                      }
                  }
              , Os);
        }

        /// <summary>
        /// 获取VitalState  获取空闲的VitalState
        /// </summary>
        /// <param name="VitalStateName"></param>
        /// <returns></returns>
        private VitalState GetFreeVitalState(string VitalStateName)
        {
            VitalState vitalState = null;
            if (!string.IsNullOrEmpty(VitalStateName) && !string.IsNullOrWhiteSpace(VitalStateName))
            {
                string stateName = VitalStateName.ToLower();
                if (stateName.IndexOf("idle") != -1)
                {
                    vitalState = Stage_Idle;
                }
            }

            if (vitalState == null)
            {
                // 使用通用的
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

        protected bool RefreshStateAnim(VitalState state, AnimationClip animationClip)
        {
            if (state == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim state=null");
                return false;
            }
            if (M_EntityBase == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim state={state.name}  M_EntityBase=null");
                return false;
            }
            if (animationClip != null)
            {
                state.SetClip(animationClip);
                return true;
            }
            else
            {
                SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim state {state.name},animationClip=null");
            }
            return false;
        }

        private void EnterNewState(VitalState newState)
        {
            if (StateMachine.CurrentState != null && StateMachine.CurrentState.Clip != null)
            {
                bool isSkillState = newState.AnimState == E_ULayerSubState.Skill_Common1 || newState.AnimState == E_ULayerSubState.Skill_Common2;

                if (isSkillState && CommonVitalStates[0].Clip == CommonVitalStates[1].Clip)
                {
                    if (StateMachine.CurrentState.Character.Animancer != null && StateMachine.CurrentState.Character.Animancer.States.TryGet(newState.Key, out AnimancerState nowState))
                    {
                        newState.isNeedPlayFromStart = true;
                        // newState.FadeTimeMilSeconds = 0f;
                    }
                }
            }
            newState.SetStateDefaultFadeTime(currentState);
            if (newState.isNeedPlayFromStart)
            {
                StateMachine.TryResetState(newState);
            }
            else
            {
                StateMachine.TrySetState(newState);
            }
        }


        #region 旋转

        //private Vector3 TheAngelInFuture;
        private void OnAngelChange(Vector3 obj)
        {
            /*  if ((TheAngelInFuture - obj).magnitude < 0.5f)//和刚才 对 “未来缓存”的也没啥差别就拉倒
              {
                  return;
              }
              TheAngelInFuture = obj;//未来缓存*/
            //大于0.1就存，就处理；处理后下次会和这次对比
            object[] Os = new object[] { obj };
            DelayInvoker.DelayInvoke(angelKey, DelayTime,
                  //延迟处理--------------------
                  (object[] args) =>
                  {
                      //Vector3 v3 = (Vector3)args[0];
                      transform.eulerAngles = (Vector3)args[0];
                  }
              , Os);
        }
        #endregion

        #region 移动
        //private Vector3 ThePosInFuture;
        //public Tweener tr;
        private void OnPosChange(Vector3 obj)
        {
            /*  if ((ThePosInFuture - obj).magnitude < 1f)//和刚才 对 “未来缓存”的也没啥差别就拉倒
              {
                  return;
              }
              ThePosInFuture = obj;//未来缓存*/
            //大于0.1就存，就处理；处理后下次会和这次对比

            object[] Os = new object[] { obj };
            DelayInvoker.DelayInvoke(pathKey, DelayTime,
                  //延迟处理--------------------
                  (object[] args) =>
                  {
                      //Vector3 v3 = (Vector3)args[0];
                      //if (tr != null && tr.IsPlaying())
                      //{
                      //    tr.Kill(false);
                      //}
                      //tr = transform.DOMove((Vector3)args[0], DelayTime).SetEase(Ease.Flash);
                      transform.position = (Vector3)args[0];
                  }
              , Os);

        }
        #endregion



    }
}