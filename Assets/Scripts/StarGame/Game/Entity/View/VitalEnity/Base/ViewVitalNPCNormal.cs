using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SGF;
using SGF.Unity;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.TypeEffect;
using StarProject.Service.Sound;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 这里全是显示层，，mono显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalNPCNormal : ViewAOI
    {
        //============================大招的特殊设定=========================
        //规范中一个动画只绑定一个AC，特殊设计中，每个人都可能携带跟随着
        //public List<AnimancerComponent> FollowAnimancer = new List<AnimancerComponent>();
        //============================大招的特殊设定=========================
        protected override E_AirState GetE_AirState(bool isGround)
        {
            //loading时移动速度 == 0
            //Loading解开首次掉落小速度  
            if (keepFirstToGround != E_EntityGState.readyTouchGroundOnceStop && !isGround)//初始化状态
            {
                e_AirState = E_AirState.InitSpeed;
            }
            else
            {
                if (isGround)
                {
                    e_AirState = E_AirState.VerySmallDownSpeed;
                }
                else
                {
                    e_AirState = E_AirState.GriDownSpeed;
                }
            }

            return e_AirState;
        }
        public new NPCEntityBase M_EntityBase
        {
            get
            {
                return m_entity as NPCEntityBase;
            }
        }

        protected GameContext m_context;

        protected CharacterController m_CharacterController;

        public bool Freezed { get; private set; }

        private int mPauseCount = 0;    // 暂定动画的计数
        private int FreeCount = 0;  // 冰冻的计数

        //-------- GM
        protected bool m_IsSyncPos = true;  // 是否同步坐标的标记，，GM按钮添加的模型不同步坐标
        public void SetIsSyncPos(bool isSync)
        {
            m_IsSyncPos = isSync;
        }
        //-------- GM

        private Vector3 tmpV3 = new();

        public Transform CircleShadow;


        // 暂时不需要NavMeshAgent来驱动主角【普通移动】部分
        //private UnityEngine.AI.NavMeshAgent navMeshAgent;
        public bool KeepStartIdleNotStand = false; // 保持开始动作, 不主动切到 stand 动作

        protected override void Create(EntityObject entity)
        {
            // 干，逻辑层的数据在  base.Create 先设置为true了， 所以默认值 要放在 base.Create 之前声明
            KeepStartIdleNotStand = false;

            base.Create(entity);

            m_context = GameManager.Instance.Context;

            mPauseCount = 0;

            FreeCount = 0;

            InitCompBind();
            // SGF.Debuger.Log($"[Idle_{m_entity.EntityId}] Create : KeepStartIdleNotStand {KeepStartIdleNotStand} ");
        }

        private void InitCompBind()
        {
            if (CircleShadow == null)
            {
                CircleShadow = transform.Find("ModelOffset/CircleShadow");
            }
            if (CircleShadow != null)
            {
                CircleShadow.gameObject.SetActive(false);
            }

            GlobalEvent.OnQualChangeTinyModuleReflesh.AddListener(OnShowLevelReflesh);
            DelayInvoker.DelayInvoke(this, 1f, OnShowLevelRefleshDelay, new object[] { (int)GameConfig.MachineQualityLevel }
            );
        }

        private void OnShowLevelRefleshDelay(object[] args)
        {
            OnShowLevelReflesh((int)args[0]);
        }

        //set to level
        private void OnShowLevelReflesh(int arg0)
        {
            if (CircleShadow != null && m_SkinMeshRender != null && m_SkinMeshRender.Count > 0)
            {
                switch ((MachineQualityLevel)arg0)
                {
                    case MachineQualityLevel.TopestLevel:
                        CircleShadow.gameObject.SetActive(false && m_SkinMeshRender[0].enabled);
                        RefleshRendererShadow(m_SkinMeshRender, !(false && m_SkinMeshRender[0].enabled));
                        break;
                    case MachineQualityLevel.TopLevel:
                        CircleShadow.gameObject.SetActive(false && m_SkinMeshRender[0].enabled);
                        RefleshRendererShadow(m_SkinMeshRender, !(false && m_SkinMeshRender[0].enabled));
                        break;
                    case MachineQualityLevel.MiddleLevel:
                        CircleShadow.gameObject.SetActive(true && m_SkinMeshRender[0].enabled);
                        RefleshRendererShadow(m_SkinMeshRender, !(true && m_SkinMeshRender[0].enabled));
                        break;
                    case MachineQualityLevel.LowerLevel:
                        CircleShadow.gameObject.SetActive(true && m_SkinMeshRender[0].enabled);
                        RefleshRendererShadow(m_SkinMeshRender, !(true && m_SkinMeshRender[0].enabled));
                        break;
                    case MachineQualityLevel.LowestLevel:
                        CircleShadow.gameObject.SetActive(true && m_SkinMeshRender[0].enabled);
                        RefleshRendererShadow(m_SkinMeshRender, !(true && m_SkinMeshRender[0].enabled));
                        break;

                    default:
                        break;
                }
            }
        }
         //开关启用CircleShadow对象本身是否渲染阴影
         public void RefleshRendererShadow(List<SkinnedMeshRenderer> skinMeshRenderers, bool ifShadow)
         {
             foreach (var VARIABLE in skinMeshRenderers)
             {
                 if (ifShadow == true)
                 {
                     VARIABLE.shadowCastingMode = ShadowCastingMode.On;
                 }
                 
                 else
                 {
                     VARIABLE.shadowCastingMode = ShadowCastingMode.Off;
                 }
             }
         }

        protected override void OnEventListener()
        {
            base.OnEventListener();

            M_EntityBase.ActionOnForceSyncPosPerSkill += ForceSynvPosPerSkill;
            M_EntityBase.DoSkillPathMove += OnSkillMove;
            M_EntityBase.PauseAnimation += PauseAnimation;
            M_EntityBase.FreezModel += Freez;
            M_EntityBase.OnCharStateChange += OnCharStateChange;

            M_EntityBase.ActionOnStackShowTypeChange += OnActionStackShowTypeChange;
            M_EntityBase.ActionOnTranslucentEffect += OnActionTranslucentEffect;
            M_EntityBase.ActionOnCreateSimulateSummon += OnActionCreateSimulateSummon;
            M_EntityBase.ActionOnTurnSimulateSummon += OnActionTurnSimulateSummon;

            M_EntityBase.ShaderChange += ShaderChange;
            M_EntityBase.ShaderChange2 += ShaderChange2;
            M_EntityBase.ActionDie += OnActionDie;
            M_EntityBase.ActionOnStartTimeHidden += StartTimeHidden;
            M_EntityBase.ActionOnStopTimeHidden += StopTimeHidden;
            M_EntityBase.ControlShowHide += OnLogicControlShow;

            if (M_EntityBase.skillDispatcher != null)
            {
                M_EntityBase.skillDispatcher.SkillController.ActionOnSkillPlayAudio += OnActionPlayAudio;
                M_EntityBase.skillDispatcher.SkillController.ActionOnSkillStopAudio += OnActionStopAudio;
            }

            M_EntityBase.ActionOnPlayThrowBullet += StartSimulateThrowBullet;
            M_EntityBase.ActionOnPlayTrackingBullet += StartSimulateTrackingBullet;
            M_EntityBase.ActionOnPlayBezierBullet += StartSimulateBezierBullet;
            M_EntityBase.ActionOnPlayEffectLineRenderer += StartPlayLineRenders;
            M_EntityBase.ActionOnStopEffectLineRenderer += StopTagMatchLineRenders;
            M_EntityBase.ViewEnterFrameAction += OnEnterFixFrame;

            M_EntityBase.ActionOnMoveStateChange += OnMoveStateChange;

            M_EntityBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;

            GlobalEvent.onSceneBeginChange.AddListener(OnSceneBeginChange);
            GlobalEvent.onSceneLoaded.AddListener(OnSceneLoaded);

            if (M_EntityBase.EntityType != E_EntityType.Player)
            {
                M_EntityBase.ActionOnBeAttackFlashColor += PlayBeAttackFlashColor;
            }

        }

        protected override void OffEventListener()
        {
            base.OffEventListener();

            if (M_EntityBase.Data.isMainPlayer)
            {
                SoundManager.Instance.RevertListener();
            }
            M_EntityBase.ActionOnForceSyncPosPerSkill -= ForceSynvPosPerSkill;
            M_EntityBase.DoSkillPathMove -= OnSkillMove;
            M_EntityBase.ActionDie -= OnActionDie;
            M_EntityBase.FreezModel -= Freez;
            M_EntityBase.OnCharStateChange -= OnCharStateChange;

            M_EntityBase.ActionOnStackShowTypeChange -= OnActionStackShowTypeChange;

            M_EntityBase.ActionOnTranslucentEffect -= OnActionTranslucentEffect;
            M_EntityBase.ActionOnCreateSimulateSummon -= OnActionCreateSimulateSummon;
            M_EntityBase.ActionOnTurnSimulateSummon -= OnActionTurnSimulateSummon;

            M_EntityBase.ShaderChange -= ShaderChange;
            M_EntityBase.ShaderChange2 -= ShaderChange2;

            M_EntityBase.PauseAnimation -= PauseAnimation;
            M_EntityBase.ActionOnStartTimeHidden -= StartTimeHidden;
            M_EntityBase.ActionOnStopTimeHidden -= StopTimeHidden;
            M_EntityBase.ControlShowHide -= OnLogicControlShow;


            if (M_EntityBase.skillDispatcher != null)
            {
                M_EntityBase.skillDispatcher.SkillController.ActionOnSkillPlayAudio -= OnActionPlayAudio;
                M_EntityBase.skillDispatcher.SkillController.ActionOnSkillStopAudio -= OnActionStopAudio;
            }

            M_EntityBase.ActionOnPlayThrowBullet -= StartSimulateThrowBullet;
            M_EntityBase.ActionOnPlayTrackingBullet -= StartSimulateTrackingBullet;
            M_EntityBase.ActionOnPlayBezierBullet -= StartSimulateBezierBullet;

            M_EntityBase.ActionOnPlayEffectLineRenderer -= StartPlayLineRenders;
            M_EntityBase.ActionOnStopEffectLineRenderer -= StopTagMatchLineRenders;

            M_EntityBase.ViewEnterFrameAction -= OnEnterFixFrame;
            M_EntityBase.ActionOnMoveStateChange -= OnMoveStateChange;

            M_EntityBase.ActionOnViewCreateFinish -= OnActionOnViewCreateFinifh;

            GlobalEvent.onSceneBeginChange.RemoveListener(OnSceneBeginChange);
            GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoaded);

            if (M_EntityBase.EntityType != E_EntityType.Player)
            {
                M_EntityBase.ActionOnBeAttackFlashColor -= PlayBeAttackFlashColor;
            }
        }

        protected virtual void OnEnterFixFrame(Vector3 skillmove)
        {
            // 驱动每帧移动
            OnFrameMove();

            FollowSummonHost();
        }

        /// <summary>
        /// 进入场景加载
        /// </summary>
        /// <param name="arg"></param>
        private void OnSceneBeginChange(object arg)
        {
            ForceSetPosState = true;

            CheckCreateFootprint();
        }
        /// <summary>
        /// 场景加载结束
        /// </summary>
        /// <param name="loadedName"></param>
        /// <param name="sameScene"></param>
        private void OnSceneLoaded(string loadedName, bool sameScene = false)
        {
            ForceSetPosState = false;
        }

        private void OnActionOnViewCreateFinifh()
        {
            InitModelHeight();
            // 暂时不需要NavMeshAgent来驱动主角【普通移动】部分
            if (M_EntityBase != null)
            {
                if (M_EntityBase.Data.isMainPlayer)
                {
                    // 主角自己，设置自动寻路
                    //navMeshAgent = GameObjectUtils.EnsureComponent<UnityEngine.AI.NavMeshAgent>(gameObject);
                    // 主角相机还是要挂在BattleCamera上,音频组人说距离人越近越有声音大小，想提高主角扮演的临场感，另外想处理相机远处听声音不清的问题，所以移动到主角身上
                    //1，问题一是进入游戏的时候并没有主角，不能依赖主角【已解决】
                    //2，问题二是Dota2也是挂在相机上，相机更偏重玩家【如果需要你可以改变SoundMgr里面父节点相机，距离在相机主角，甚至动态控制】
                    //3，只有特殊情况采用主角，比如隐身效果的声音根据主角距离相关的才这样【主角->相机->相机主角切换
                    //本质是相机，主角，中间动画控制变化位置】
                    //ui声音gob给的是ui，给相机吧，ui相机1000米高，现在要给battleCamera这个是listener
                    //New：增加GListener，Init给相机，有主角给主角(主角死了不会换模型，一直放在主角，相机下面）
                    //UI可以听到，BGM可以听到，触发声音不必刚体可以听到。
                    //GameObjectUtils.EnsureComponent<AkAudioListener>(gameObject).SetIsDefaultListener(true);
                    SoundManager.Instance.SetGListener(transform);

                    //if (GCamera == null)
                    //{
                    //    GCamera = GameManager.Instance.M_GameCamera.GetComponent<GameCamera>();
                    //}

                    // 主角设置摄像机的Y轴偏移量
                    float offsetY = 0.77f;
                    if (M_ModelOffLineData != null)
                    {
                        Transform BipPoint = M_ModelOffLineData.GetTransformByKey(HangPoint.Bip001.ToString());
                        if (BipPoint != null)
                        {
                            offsetY = BipPoint.localPosition.y;
                        }
                    }
                    StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraOffect(new Vector3(0, offsetY, 0));

                    // 主角模型创建完毕，loadingView 才关闭
                    //GameManager.Instance.Loading.OnProcess(0.9f);//Single
                    //GameManager.Instance.Loading.OnProcessEnd();
                }
            }
        }

        private void InitModelHeight()
        {
            m_CharacterController = GameObjectUtils.EnsureComponent<CharacterController>(gameObject);

            float height = 2f;
            // 挂点位置
            if (M_ModelOffLineData != null)
            {
                //Transform UnitInfoPoint = M_ModelOffLineData.GetTransformByKey(HangPoint.Top_D.ToString());
                //if (UnitInfoPoint != null)
                //{
                //    Vector3 v3 = WorldItemChecker.Instance.PlayerAndNpcPos(UnitInfoPoint, E_AnchorPresets.Center, UiOr3D.D3, PivotPos.Center);
                //    height = v3.y;
                //    Transform RootPoint = M_ModelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                //    if (RootPoint != null)
                //    {
                //        Vector3 v3root = WorldItemChecker.Instance.PlayerAndNpcPos(RootPoint, E_AnchorPresets.Center, UiOr3D.D3, PivotPos.Center);
                //        height -= v3root.y;
                //    }
                //}
                height = M_ModelOffLineData.GetModelHeight();
            }
            m_CharacterController.height = height;
            //射线起点高抬为了让射线起点能在地表上
            m_CharacterController.skinWidth = GameConfig.PLAYER_SKIN_WIDTH + GameConfig.PLAYER_GRI_CHECKER_ORGIN_HEIGHT;
            //绝对贴地逻辑
            m_CharacterController.center = Vector3.up * ((m_CharacterController.height / 2f) + GameConfig.PLAYER_SKIN_WIDTH);//结果是模型贴地 = 一半胶囊高度 + 胶囊厚度 = 设置给世界我角色的中心点

            float radiusConfig = GameConfig.MODEL_RADIUS;
            if (M_EntityBase.avatarDataCell != null)
            {
                radiusConfig = (float)M_EntityBase.avatarDataCell.GetCollierRadius() / 100.0f;
                float radius2 = height / 2;//保证不错的基础，胶囊里面只有两个R，除以2是稳的，除以大的会很瘦


                radiusConfig = Math.Min(radiusConfig, radius2);//取得配置和真实的最小值保证不错，但是可能碰撞可能肥
            }

            m_CharacterController.radius = radiusConfig;/** (float)M_EntityBase.avatarDataCell.GetModelScaling() / 100.0f*/;
            if (CircleShadow != null)
            {
                CircleShadow.localScale = Vector3.one * m_CharacterController.radius * 4;
            }

            WorldItemChecker.Instance.RegToRoleHeight(EntityID, height);
        }

        private void OnActionPlayAudio(I_AudioParam aPara)
        {
            StarProject.Service.Sound.SoundManager.Instance.PlayAudioParam(aPara, this.gameObject);
        }

        private void OnActionStopAudio(I_AudioParam aPara)
        {
            StarProject.Service.Sound.SoundManager.Instance.StopAudio(aPara, this.gameObject);
        }

        private void OnActionDie()
        {
            KillLerpMover_StopInPlace(true);     //死亡

            SetLerpAngelKill();
        }

        #region 模型效果逻辑

        /// <summary>
        /// 枚举类型 对应的 效果 list. 同一种 ShaderEnum 同时只显示 一种 shader 效果.
        /// </summary>
        private Dictionary<GlobalShowType, List<BaseTypeEffect>> ShaderEffects = new();

        private Dictionary<ShaderEnum, int> ShaderEffectDic = new();

        private void UpdateShaderCount(ShaderEnum shaderEnum, bool addOne)
        {
            int originCount = 0;
            if (!ShaderEffectDic.TryGetValue(shaderEnum, out int curCount))
            {
                curCount = 0;
            }
            originCount = curCount;
            curCount = addOne ? curCount + 1 : curCount - 1;
            curCount = curCount < 0 ? 0 : curCount;

            ShaderEffectDic[shaderEnum] = curCount;

            UpdateShaderModel(shaderEnum, curCount, originCount);
        }

        private void UpdateShaderModel(ShaderEnum shaderEnum, int curCount, int originCount)
        {
            if (curCount == originCount)
            {
                return;
            }
            // 数量变少
            // bool addCount = curCount > originCount;

            bool enabledShader = curCount > 0;

            string shaderKey = "";

            switch (shaderEnum)
            {
                case ShaderEnum.Freeze:
                    {
                        shaderKey = "_ElemetalVFX";
                    }
                    break;
                //case ShaderEnum.Translucent:
                //    {
                //        //TODO:DL
                //        // 等 武文 告诉我shader 
                //        SGF.Debuger.LogError($"[半透] 效果 Translucent : {enabledShader} ");
                //        Translucent(enabledShader);
                //        return;
                //    }
                //    break;

                default: break;
            }

            if (shaderKey == "")
            {
                return;
            }

            foreach (var render in m_SkinMeshRender)
            {
                if (render != null && render.material != null)
                {
                    if (enabledShader)
                    {
                        render.material.EnableKeyword(shaderKey);
                    }
                    else
                    {
                        render.material.DisableKeyword(shaderKey);
                    }
                }
            }
        }

        private void ShaderChange(ShaderEnum shaderEnum, bool OnEnter)
        {
            UpdateShaderCount(shaderEnum, OnEnter);
        }

        private void ShaderChange2(bool onEnter, BaseTypeEffect effect)
        {
            UpdateShaderEffects(onEnter, effect);
        }
        /// <summary>
        /// 半透效果
        /// </summary>
        /// <param name="start"></param>
        private void Translucent(bool start)
        {
            m_CharacterFadeOuts.ForEach((CharacterFadeOutController controller) =>
            {
                if (start)
                {
                    controller.IfBeginVFX = true;
                }
                else
                {
                    controller.IfStopVFX = true;
                }
            });
        }

        /// <summary>
        /// 收到 栈顶 globalShowType 的效果 变化的通知.
        /// </summary>
        /// <param name="updateType"></param>
        /// <param name="updateEffect">需要更新的效果</param>
        /// <param name="curEffect">在更新之时, 当前栈顶的效果</param>
        private void OnActionStackShowTypeChange(TypeEffectUpdateType updateType, BaseTypeEffect updateEffect, BaseTypeEffect curEffect)
        {
            switch (updateType)
            {
                case TypeEffectUpdateType.OnEnter:
                    {
                        // 效果 进入的时候, 需要处理:
                        // 1. 需要当前栈顶效果的 退出;
                        ExitBaseTypeEffect(curEffect);

                        // 2. 新消息的 进入
                        EnterBaseTypeEffect(updateEffect);

                    }
                    break;
                case TypeEffectUpdateType.OnUpdate:
                    {
                        // EntityCtrlBase 中已经处理了 效果的进出栈的关系.  
                        // 所以此处 执行 OnUpdate 的时候,就不需要再去主力 栈顶效果. 
                        // 直接 更新 当前 效果即可
                        UpdateBaseTypeEffect(updateEffect);
                    }
                    break;
                case TypeEffectUpdateType.OnExit:
                    {
                        ExitBaseTypeEffect(updateEffect);
                    }
                    break;

                default: break;
            }
        }

        private void ExitBaseTypeEffect(BaseTypeEffect updateEffect)
        {
            updateEffect?.OnShowExit();
        }

        private void EnterBaseTypeEffect(BaseTypeEffect updateEffect)
        {
            updateEffect.OnShowEnter();
        }

        private void UpdateBaseTypeEffect(BaseTypeEffect updateEffect)
        {
            updateEffect.OnShowUpdate();
        }


        /// <summary>
        /// 角色的 shader 状态变化
        /// </summary>
        private void OnCharStateChange(CharStateController.CharacterState characterState, bool isShow)
        {
            // 如果是 需要移除的效果, 那就判断一下当前是否是 准备死亡的状态. 
            // 如果准备死亡, 那就不移除 这个shader 效果. 等到 怪物死亡动画结束后执行 release 的时候, 再去还原这个状态效果
            if (!isShow && M_EntityBase.IsReadyDead)
            {
                return;
            }
            charStateController?.SetCharState(isShow ? characterState : CharStateController.CharacterState.Normal);
        }

        private void ResetDefaultCharState()
        {
            charStateController?.SetCharState(CharStateController.CharacterState.Normal);
        }

        /// <summary>
        /// 冰冻
        /// </summary>
        /// <param name="freez"></param>
        private void Freez(bool freez)
        {
            FreeCount += freez ? 1 : -1;
            if (FreeCount < 0)
            {
                FreeCount = 0;
            }

            DoFree(FreeCount > 0);
        }

        private void DoFree(bool freez)
        {
            if (m_SkinMeshRender == null || m_SkinMeshRender.Count <= 0)
            {
                return;
            }
            if (Freezed != freez)
            {
                Freezed = freez;
                if (Freezed)
                {
                    foreach (var render in m_SkinMeshRender)
                    {
                        if (render != null && render.material != null)
                        {
                            render.material.EnableKeyword("_ElemetalVFX");
                        }
                    }
                }
                else
                {
                    foreach (var render in m_SkinMeshRender)
                    {
                        if (render != null && render.material != null)
                        {
                            render.material.DisableKeyword("_ElemetalVFX");
                        }
                    }
                }
            }
        }

        private void SetEdgeLight(float rimWidth, Color rimColor, Color mainColor)
        {
            HandleRenders((render) =>
            {
                var material = render.material;
                if (material.HasFloat("_RimWidth"))
                {
                    material.SetFloat("_RimWidth", rimWidth);
                }
                if (material.HasColor("_RimColor"))
                {
                    render.material.SetColor("_RimColor", rimColor);
                }
                if (material.HasColor("_RimWidth"))
                {
                    render.material.SetColor("_MainColor", mainColor);
                }
            });
        }

        /// <summary>
        /// 处理外发光
        /// </summary>
        /// <param name="shaderEffect"></param>
        public void EdgeLight(BaseTypeEffect shaderEffect, bool onEnter)
        {

            if (!onEnter)
            {
                ResetDefaultEdgeLight();
                return;
            }
            var effect = shaderEffect.EffectTypeSerialize.BUFF_ChangeColorShader;



            float rimWidth = effect.EdgePer / 1000f;
            Color rimColor = effect.GetEdgeColor();

            Color mainColor = effect.GetMainColor();

            SetEdgeLight(rimWidth, rimColor, mainColor);
        }

        /// <summary>
        /// 半透效果的 action 接口
        /// note:
        ///     gl 要求做 多个半透效果的处理(显示的永远是最后的一个半透效果的值)。
        ///     所以 半透效果的 渐变逻辑 需要在效果 effect 中 做lerp 渐变.
        ///     而在 主角的身上, 只需要提供 刷新的接口
        /// </summary>
        /// <param name="translucentEffect"></param>
        /// <param name="onEnter"></param>
        private void OnActionTranslucentEffect(TranslucentEffect translucentEffect, bool onEnter)
        {
            // 先 不做 多个半透效果的叠加, 先做 替换
            TranslucentShaderNew(translucentEffect, onEnter);
        }

        /// <summary>
        /// 新的shader 半透效果
        /// </summary>
        /// <param name="shaderEffect"></param>
        /// <param name="onEnter"></param>
        public void TranslucentShaderNew(TranslucentEffect shaderEffect, bool onEnter)
        {

            if (!onEnter)
            {
                ResetDefaultTranslucent();
                return;
            }

            var meshs = GetGlobalShowTypeMeshRenders(GlobalShowType.BUFF_TransChange);
            if (meshs == null)
            {
                return;
            }

            // 先设置 渲染队列
            meshs.ForEach(item =>
            {
                item.Key.materials.ForEach((material) =>
                {
                    material.renderQueue = 3200;
                    material.SetFloat("_Alpha", shaderEffect.curAlpha);
                });
            });
        }

        /// <summary>
        /// 更新 shader Effect
        /// </summary>
        /// <param name="shaderEnum"></param>
        /// <param name="onEnter"></param>
        /// <param name="shaderEffect"></param>
        private void UpdateShaderEffects(bool onEnter, BaseTypeEffect shaderEffect)
        {
            GlobalShowType globalShowType = shaderEffect.EffectTypeSerialize.GlobalShowType;
            if (onEnter)
            {
                if (ShaderEffects.TryGetValue(globalShowType, out var effects))
                {
                    effects.Add(shaderEffect);
                }
                else
                {
                    ShaderEffects.Add(globalShowType, new List<BaseTypeEffect>() { shaderEffect });
                }

                UpdateCurShaderEffect(globalShowType, shaderEffect);
            }
            else
            {
                if (!ShaderEffects.TryGetValue(globalShowType, out var effects))
                {
                    return;
                }
                else
                {
                    int count = effects.Count;
                    if (count > 0 && effects[count - 1] == shaderEffect)
                    {
                        effects.Remove(shaderEffect);

                        count = effects.Count;
                        if (count > 0)
                        {
                            UpdateCurShaderEffect(globalShowType, effects[count - 1]);
                        }
                        else
                        {
                            UpdateCurShaderEffect(globalShowType, null);
                        }

                    }
                    else
                    {
                        effects.Remove(shaderEffect);
                    }
                }
            }

        }

        /// <summary>
        /// 更新 当前的 shader 效果
        /// </summary>
        /// <param name="shaderEnum"></param>
        /// <param name="shaderEffect"></param>
        private void UpdateCurShaderEffect(GlobalShowType globalShowType, BaseTypeEffect shaderEffect)
        {
            // 2023/8/8
            // 跟 高磊 沟通 确认如下:
            // 1.shader 效果 可以修改多个 参数;
            // 2.不同的 shader 效果 应该由 策划 保证 不修改相同的 参数, 从而避免 同一个 参数 被不同的 shader 效果修改的问题
            switch (globalShowType)
            {
                case GlobalShowType.BUFF_ChangeColorShader:
                    {
                        EdgeLight(shaderEffect, shaderEffect != null);
                    }
                    break;
                case GlobalShowType.BUFF_TransChange:
                    {
                        // TranslucentShaderNew(shaderEffect, shaderEffect != null);
                    }
                    break;
            }

            if (globalShowType == GlobalShowType.BUFF_ChangeAnim)
            {

            }
        }

        private void PauseAnimation(bool pause)
        {
            mPauseCount += pause ? 1 : -1;
            if (mPauseCount < 0)
            {
                mPauseCount = 0;
            }

            if (Animancer != null)
            {
                Animancer.Playable.IsGraphPlaying = mPauseCount < 1;
            }
        }


        /// <summary>
        /// 播放闪白效果
        /// </summary>
        private void PlayBeAttackFlashColor()
        {
            if (m_PlayBeAttackFlashColorDic.Count <= 0)
            {
                foreach (var render in m_SkinMeshRender)
                {
                    if (render != null)
                    {
                        Material[] materials = render.materials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            Material ms = materials[i];
                            var tween = DOTween.To(() => 0, (x) =>
                            {
                                ms.SetFloat("_ColorChange", x % 1.0f);
                            }, 1.0f, 0.1f);
                            tween.onComplete = () =>
                            {
                                ms.SetFloat("_ColorChange", 0);
                                tween = null;
                            };

                            AddFlashColorMatDic(ms.name, ms, tween);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in m_PlayBeAttackFlashColorDic)
                {
                    if (item.Value != null && item.Value.mat != null)
                    {
                        if (item.Value.tweener != null && item.Value.tweener.IsActive() && item.Value.tweener.IsPlaying())
                        {
                            item.Value.tweener.Kill(true);
                        }
                        var tween = DOTween.To(() => 0, (x) =>
                        {
                            //SGF.Debuger.Log($"闪白 x={x},intensity={x % 1.0f}");
                            item.Value.mat.SetFloat("_ColorChange", x % 1.0f);
                        }, 1.0f, 0.1f);
                        tween.onComplete = () =>
                        {
                            item.Value.mat.SetFloat("_ColorChange", 0);//结尾好还原
                            tween = null;
                        };

                        SetFlashColorMatDic(item.Value.mat.name, item.Value.mat, tween);
                    }
                }
            }
        }

        private void AddFlashColorMatDic(string name, Material ms, TweenerCore<float, float, FloatOptions> tweener)
        {
            if (m_PlayBeAttackFlashColorDic.ContainsKey(name))
            {
                SGF.Debuger.LogWarning($"ViewVitalNPCNormal AddFlashColorMatDic() name={name} err!!!");
            }
            else
            {
                BeAttackFlashColor beAttackFlashColor = new(ms, tweener);
                m_PlayBeAttackFlashColorDic.Add(name, beAttackFlashColor);
            }
        }

        private void SetFlashColorMatDic(string name, Material ms, TweenerCore<float, float, FloatOptions> tweener)
        {
            if (m_PlayBeAttackFlashColorDic.ContainsKey(name))
            {
                var data = m_PlayBeAttackFlashColorDic[name];
                data.tweener = tweener;
            }
        }

        private void ResetPlayBeAttackFlashColor()
        {
            foreach (var item in m_PlayBeAttackFlashColorDic)
            {
                if (item.Value.tweener != null && item.Value.tweener.IsActive() && item.Value.tweener.IsPlaying())
                {
                    item.Value.tweener.Kill(true);
                }
                if (item.Value.mat != null)
                {
                    item.Value.mat.SetFloat("_ColorChange", 0);//结尾好还原
                }
            }
            m_PlayBeAttackFlashColorDic.Clear();
        }

        #endregion

        #region 移动逻辑

        //private GameObject m_SkillGob;

        /// <summary>
        /// 循环请求最新技能点坐标
        /// 1，技能不修改朝向（可以通用）
        /// 2，技能播放受伤（可以设置）
        /// 3， Action不同
        /// </summary>
        /// <param name="nextPoint">位移结束点</param>
        /// <param name="sec">时间（秒）</param>
        /// <param name="e_MoveStateType">技能位移类型</param>
        public void OnSkillMove(Vector3 nextPoint, float sec, MoveLabel moveLabel, MoveType moveType, string key, Action<bool> action)
        {
            //SGF.Debuger.LogError($"移动调试 OnSkillMove id={M_EntityBase.EntityId} nextPoint {nextPoint}, m_IsSyncPos {m_IsSyncPos}, M_IsAlive {M_EntityBase.M_IsAlive}");

            if (!m_IsSyncPos)
            {
                return;
            }
            if (!M_EntityBase.M_IsAlive)
            {
                return;
            }
            if (nextPoint != Vector3.zero)
            {
                //4，技能打断普通，技能打断技能通用，技能优先级高直接杀掉没问题
                KillLerpMover_StopInPlace(true);
                ThdPersionDataCache.Clear();
                M_isSkillMove = true;
                //Vector3 distXZ = nextPoint - transform.localPosition;
                //distXZ.y = 0;
                //float moveTimeSec = (distXZ).magnitude / M_EntityBase.Speed;
                //if (M_EntityBase.Data.isMainPlayer)
                //{
                //    SGF.Debuger.Log($"移动调试 OnSkillMove--start id={M_EntityBase.EntityId},pos={M_EntityBase.Position()},localPos={transform.localPosition},nextPoint={nextPoint},Speed={M_EntityBase.Speed},sec={sec}");
                //}
                //if (m_SkillGob == null)
                //{
                //    //m_SkillGob = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/1") as GameObject);
                //    m_SkillGob = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/1");
                //}

                //m_SkillGob.name = "NextPoint";
                //m_SkillGob.transform.position = nextPoint;

                // 如果是闪现位移的话，需要隐藏模型
                if (moveLabel == MoveLabel.Flash)
                {
                    //OnActionModelVisiableTime(false, false, sec, key);
                    StartTimeHidden(sec, key);
                }
                if (moveType == MoveType.Flash)
                {
                    // 闪现就穿墙
                    IsOpenCollision(false);
                }

                // 对 位移的时间 sec做个 修正, 防止外面由于 speed ==0 ，导致计算的 sec 出现NaN
                if (float.IsNaN(sec) || float.IsInfinity(sec))
                {
                    sec = 0;
                }
                //SGF.Debuger.LogError($"移动调试 OnSkillMove id={M_EntityBase.EntityId} LerpMover1 start sec: {sec} {nextPoint}");

                //M_EntityBase.SetCurrentPos(nextPoint.x, nextPoint.y, nextPoint.z);
                pathStartPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);

                if (sec == 0)
                {
                    OnSkillMoveLerpComplete(targetPoint: nextPoint, moveType, action);
                    return;
                }

                // SGF.Debuger.Log($"[Move] start skillMove y: {transform.localPosition.y} ------------------");

                LerpMover1 = DOTween.To(() => pathStartPos, lerpingCurVector =>
                {
                    // SGF.Debuger.Log($"[Move] skillMove y: {transform.localPosition.y} ");
                    moveMotion = lerpingCurVector - transform.localPosition;
                    moveMotion.y = 0;//要享受重力，服务器控制技能
                    /*  所有地方dg自动的都去掉了y 享受重力运算单独给y解开dg 和unity 调用耦合。确认享受重力即可。  @曲振新 本来第三人服务器中转的我想节省不让别人机器进行运算因为我算的没问题。但是第一我来源于navmesh 的可能有问题。第二我到服务器其他人是还原不及时救出来问题这个节省不了*/
                    //moveMotion.y = this.VerticalVelocity;
                    //改都改过了所有DG，重力如果不设置他一直存在检测底下0.1米，除非设置配置他是飞行物
                    MoveCharacterController(moveMotion/*, "OnSkillMove"*/); //时刻更新当前
                                                                            //m_EntityPosition = m_context.EntityToViewPoint(startPos);                            //过程中同步显示层
                                                                            // SGF.Debuger.Log($"[Move] characterMove skillMove y: {transform.localPosition.y} ------------------");

                    M_EntityBase.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);//过程【中】同步逻辑层

                    if (M_EntityBase.Data.isMainPlayer)
                    {
                        //M_EntityBase.SetCollY_FilterSSPData(pos, true);//技能目前不用发，服务器知道目标点，而且他只有目标点
                        //SGF.Debuger.LogWarning($"移动调试 OnSkillMove--update id={M_EntityBase.EntityId},pos={M_EntityBase.Position()},localPos={transform.localPosition},moveMotion={moveMotion},nextPoint={nextPoint},Speed={M_EntityBase.Speed},sec={sec}");
                        //GCamera.UpdateCameraPos(m_EntityPosition);
                    }
                }, nextPoint, sec).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed);

                LerpMover1.onComplete = () =>
                {
                    LerpMover1.onComplete = null;
                    // SGF.Debuger.Log($"[Move] end skillMove y: {transform.localPosition.y} ------------------");

                    OnSkillMoveLerpComplete(targetPoint: nextPoint, moveType, action);
                };
            }
        }

        private void OnSkillMoveLerpComplete(Vector3 targetPoint, MoveType moveType, Action<bool> completeCb)
        {
            if (moveType == MoveType.Flash)
            {
                // 闪现就穿墙
                IsOpenCollision(true);
            }
            M_isSkillMove = false;
            //SGF.Debuger.LogError($"移动调试 OnSkillMove--end  entityId={M_EntityBase.EntityId},pos={M_EntityBase.Position()},localPosition={transform.localPosition},nextPoint={tempV3},sec={sec}");
            M_EntityBase.SetCurrentPos(targetPoint.x, transform.localPosition.y, targetPoint.z, true);//过程【结束后】

            if (completeCb != null)
            {
                completeCb?.Invoke(true);
            }

        }

        /// <summary>=========
        /// 【更正技能位置】
        /// 在移动和技能同时处理时
        /// 技能优先级高于移动，且提前于移动信息
        /// 优先按照技能中的起始坐标进行位置强同步，且忽略移动信息中的信息
        /// </summary>
        public void ForceSynvPosPerSkill(Vector3 destPos, Action action)
        {

            //先解除上面的锁

            if (m_entity.IsMainPlayer)
            {
                KillLerpMover_StopInPlace(true);     //强制更正
                OnForceMove(destPos);
            }


            //Vector3 distXZ = destPos - transform.localPosition; //移动速度是表面移动速度，类似周长，我假设模拟的是XZ平面的，爬坡移动速度变快了其实，先用服务器设定同步就没问题
            //distXZ.y = 0;
            //float moveTimeSec = (distXZ).magnitude / M_EntityBase.Speed;
            //if (M_EntityBase.Data.isMainPlayer)
            //{
            //    SGF.Debuger.Log($"移动调试 ForceSynvPosPerSkill id={M_EntityBase.EnityId},localPos={transform.localPosition},destPos={destPos},Speed={M_EntityBase.Speed},moveTimeSec={moveTimeSec},magnitude={distXZ.magnitude}");
            //}
            //Debug.LogError($"属性同步 使用技能  当前坐标={transform.localPosition.x},{transform.localPosition.y},{transform.localPosition.z},,新坐标={destPos.x},{destPos.y},{destPos.z}");
            {

                //SGF.Debuger.Log($"{TagFlag} ForceSynvPosPerSkill--end  entityId={M_EntityBase.EnityId},destPos={destPos},localPosition={transform.localPosition},moveTimeSec={moveTimeSec}");

                action.Invoke();
            }
            // LerpMover1 = DOTween.To(() => transform.localPosition, changeVector =>
            // {
            //     moveMotion = changeVector - transform.localPosition; //时刻根据当前，取得下次的小步长
            //     moveMotion.y = this.VerticalVelocity;
            //     m_CharacterController.Move(moveMotion);              //时刻更新当前
            //     m_EntityPosition = changeVector;                            //过程中同步显示层
            //     M_EntityBase.SetCurrentPos(changeVector.x, changeVector.y, changeVector.z);//过程【中】同步逻辑层
            // }, destPos, moveTimeSec).SetEase(Ease.Flash);
            // LerpMover1.onComplete = () =>
            // {
            //     LerpMover1.onComplete = null;
            //     M_EntityBase.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z, true);//过程【结束后】
            //                                                                                                                   //m_ForceMoveLocker = false;//中转给Update可继续执行
            //     SGF.Debuger.Log($"{TagFlag} ForceSynvPosPerSkill--end  entityId={M_EntityBase.EnityId},destPos={destPos},localPosition={transform.localPosition},moveTimeSec={moveTimeSec}");

            //     action.Invoke();
            // };
        }

        //由于上层缓存，第三人本质就是一个数据，只是不改成一个了
        public Queue<Vector3> ThdPersionDataCache = new();
        /// <summary>
        /// thd只按照单点进行移动
        /// 现基于缓存，因为策略要都走到，
        /// 路店是客户端发的，是实际走的，所以要走到；逻辑产生于玩家实际行走的轨迹-一次-必须表现，不同于怪物中途改变策略（会被电脑控制）反复重新驱动走出来
        /// 不存在发空让我停下，也不可能发0；所以的最后一个坐标就是最后一个（实际就是客户端的路店记录给服务器的）（实际就是别的玩家走的）
        /// </summary>
        /// <param name="destPos"></param>
        public override void OnThdPersionMove(Vector3 destPos)
        {
            // SGF.Debuger.Log($"[Move] {M_EntityBase.EntityType} set born pos : {destPos}");

            if (!m_IsSyncPos)
            {
                return;
            }
            if (!M_EntityBase.M_IsAlive)
            {
                return;
            }
            // 黑洞状态下的第三方移动 改为黑洞 每帧驱动
            if (M_EntityBase.Data.IsBattleState_BlackHole)
            {
                return;
            }

            ThdPersionDataCache.Enqueue(destPos);

            //需帮他启动
            //首次 或 非活跃 或 没播放（休息）
            //数据不会改变他，除非休息了
            if (LerpMover1 == null || LerpMover1.IsActive() == false || LerpMover1.IsPlaying() == false)
            {
                BeginThdPersionMove();
            }
        }

        /// <summary>
        /// 递归【服务器中转的第三人移动】
        /// 当调用我必然调用新的
        /// 【正常移动结束】【循环结束过程之外不会中断】
        /// </summary>
        public void BeginThdPersionMove()
        {
            // SGF.Debuger.LogError($"[Move]  BeginThdPersionMove");

            //通常是不活跃外面会驱动，刚取消活跃内部会调用，所以常规就是不活跃就会跳过，保证活跃就杀掉重启
            //有，且播放就干掉
            KillLerpMover_StopInPlace();

            if (ThdPersionDataCache.Count == 0)
            {
                return;
            }

            Vector3 curDestPos = ThdPersionDataCache.Dequeue();

            //客户端发送是0.1频率，客户端能走的都没问题 ： 按照时间还是空间
            ////GameObject nextPointBox = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/1") as GameObject);
            //GameObject nextPointBox = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/1");

            //nextPointBox.name = m_entity.Data.M_EntityID.ToString();
            //nextPointBox.transform.position = curDestPos;
            //nextPointBox.transform.localScale = UnityEngine.Vector3.one;

            Vector3 distXZ = curDestPos - transform.localPosition;
            distXZ.y = 0;

            //已经取出了目前是还有拥堵数量;剩余0个拥堵-正常-目前就是1倍数，剩余1个就要加速1.2f，2个1.44
            //1.44追赶，1.2追赶，1追赶-很快就会赶上
            //全局拥堵Count就是网络拥堵不必在乎，下面多少是过程化数据

            float clientMovePara = M_EntityBase.Speed * Mathf.Pow(GameConfig.THD_PERSION_CLIENT_SIM_SPEED_PARA, ThdPersionDataCache.Count)/* * Mathf.Pow(GameConfig.G_CLIENT_SIM_SPEED_PARA, (float)DynamicDataFactory.GetCount<SerNttSnapData>(EntityID))*/;

            float moveTimeSec = clientMovePara != 0 ? (distXZ.magnitude / clientMovePara) : 0;
            //SGF.Debuger.Log($"[Speed] 第三人移动 Count: {ThdPersionDataCache.Count}, speed: {M_EntityBase.Speed} ---> {clientMovePara} , moveTime: {moveTimeSec} ");

            //Debug.LogError($"[Move] 属性同步 第三人称路点 队列长度 ={ThdPersionDataCache.Count},正常速度={M_EntityBase.Speed},当前速度={clientMovePara},时间={moveTimeSec},长度={(distXZ).magnitude}");

            //if (M_EntityBase.Data.isMainPlayer)
            //{
            //    SGF.Debuger.Log($"移动调试 OnThdPersionMove id={M_EntityBase.EnityId},localPos={transform.localPosition},destPos={destPos},Speed={M_EntityBase.Speed},moveTimeSec={moveTimeSec},magnitude={distXZ.magnitude}");
            //}

            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnThdPersionMove] 1111111 id={M_EntityBase.EntityId},localPos={transform.localPosition},destPos={curDestPos},Speed={M_EntityBase.Speed},moveTimeSec={moveTimeSec},magnitude={distXZ.magnitude}");
            //}

            pathStartPos.x = transform.localPosition.x;
            pathStartPos.y = transform.localPosition.y;
            pathStartPos.z = transform.localPosition.z;

            // 第三方人 的移动朝向 要设置一下
            if (!M_EntityBase.Data.isMainPlayer)
            {
                /// 2023/06/29
                /// 如果 其他人的原子锁 没有锁住,并且是 简单的移动,那就使用 玩家到下一个点的 移动朝向
                if (!M_EntityBase.Data.Is___ForbidDir && M_EntityBase.IsSimpleMovingState())
                {
                    tmpV3 = VectorUtils.V3Set(tmpV3, curDestPos.x - transform.localPosition.x, 0, curDestPos.z - transform.localPosition.z);
                    if (tmpV3.magnitude > 0.1)
                    {
                        // SGF.Debuger.LogError($"[Rotate] entityID: {M_EntityBase.EntityId} 服务器 [AOI] 坐标 , IsSimpleMovingState: {M_EntityBase.IsSimpleMovingState()}, 设置移动朝向: {tmpV3} ");
                        M_EntityBase.ClientSetRotationByDir(tmpV3, true, 0);
                        M_EntityBase.M_EntityMoveDir = tmpV3;
                    }
                    tmpV3 = VectorUtils.V3Set(tmpV3, 0, 0, 0);
                }
            }

            //SGF.Debuger.LogError($"[Move]  BeginThdPersionMove lerp to ---> {curDestPos} ,time: {moveTimeSec}");

            if (moveTimeSec == 0)
            {
                OnThirdMoveLerpComplete(curDestPos);
                return;
            }
            LerpMover1 = DOTween.To(() => pathStartPos, changeVector =>
            {
                moveMotion = changeVector - transform.localPosition; //时刻根据当前，取得下次的小步长
                moveMotion.y = 0;

                MoveCharacterController(moveMotion/*, "OnThdPersionMove"*/);//时刻更新当前
                m_EntityPosition = changeVector;                            //过程中同步显示层

                M_EntityBase.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);//过程【中】同步逻辑层
            }, curDestPos, moveTimeSec).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed);

            LerpMover1.onComplete = () =>
            {
                LerpMover1.onComplete = null;
                OnThirdMoveLerpComplete(curDestPos);
            };
        }

        private void OnThirdMoveLerpComplete(Vector3 targetPos)
        {
            M_EntityBase.SetCurrentPos(targetPos.x, transform.localPosition.y, targetPos.z, true);//过程【结束后】

            //跑完加速了要设定转角，角度也没有缓存thd角度要缓存（可能乱）没必要强制更新角度，移动不是转角的高优先级，甚至移动也不是分割消息的关键符号（所有消息是同级只是这里异步处理而已）
            //OnAngelChangeMove(Vector3.zero,true);

            //当异步结束时，后续没有缓存路点
            //说明当前没有要继续走的指令时才停下
            //不然后续还有指令一个是1，动画切换也有时间的会很怪，2,这个士兵很喜欢休息明显有后续指令直接按照后续指令连续做
            /// if (ThdPersionDataCache.Count == 0)上面的调用方式一个是循环（这里和上面一样），一个是服务器推送（必然有数据-不可能走），这里更偏重结尾的逻辑处理，而且是异步结果
            if (ThdPersionDataCache.Count == 0)
            {
                I_AnimParam animParam = M_EntityBase.GetAnimParamByState(E_ULayerSubState.Idle);
                M_EntityBase.ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
            }

            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnThdPersionMove] 22222222 id={M_EntityBase.EntityId},localPos={transform.localPosition},destPos={curDestPos},Speed={M_EntityBase.Speed},moveTimeSec={moveTimeSec},magnitude={distXZ.magnitude}");
            //}

            //本质这里有加速处理，走到，但是没必要作为衡量标准，走完根本不算什么，外层会一直放消息，移动转角路店可能消化不掉自己处理

            //先清空【消息层】：当自己消息都处理结束，放开标记，【等待外层消息推动客户端继续执行(服务器突然延迟情况）】 
            //M_EntityBase.Data.ClearSerMRSnapDataRuningTag();

            //递归循环【消息层】：目前消息也加上反馈才循环责这里需要答复上层完成：控制层就在这里因为转角优先级最低【请求本次缓存数据，如果有就执行，没有就等服务器】
            //M_EntityBase.Data.ExecuteTransformChange();
            //阻断外循环请求；因为由数据拥堵导致的循环的加速处理本质不重要，并不需要循环异步，并且消息等级相同，【外层自己会推】

            //递归循环【自己】：如果信息没有进来，内层就是死循环；如果外层消息自由则信息就是活的甚至拥堵
            BeginThdPersionMove();

            //自己管自己循环，新消息也要走完，所以不会被打断，所以自己会缓存消息，外面该推送还是推送
            //不管外面请求，清空，自己闭环，
        }

        public override void OnForceMove(Vector3 nextPoint)
        {
            if (!m_IsSyncPos)
            {
                return;
            }
            //-----------------------------------------------------------
            ForceSetPosState = true;
            if (m_CharacterController != null)
            {
                m_CharacterController.enabled = false;
            }
            //if (navMeshAgent != null)
            //{
            //    navMeshAgent.enabled = false;
            //}
            //-----------------------------------------------------------
            //if (LerpMover1 != null && !LerpMover1.IsPlaying())
            //{
            //LerpMover1.Kill(false);//目前继续走完
            //}
            //if (m_entity.Data.isMainPlayer)
            //{
            //    SGF.Debuger.Log($"移动调试 OnForceMove id={m_entity.EnityId},localPos={transform.localPosition},nextPoint={nextPoint},Speed={m_entity.Speed}");
            //}

            // 对服务器发过来的 强同步坐标做个 地表检测，防止掉下去
            //{
            //    RaycastHit hit;
            //    //nextPoint.y = 0;
            //    bool sd = Physics.Raycast(nextPoint + (Vector3.up), Vector3.down, out hit, 10.0f, GroundLayer);
            //    if (sd)
            //    {
            //        nextPoint.y = hit.point.y + 0.08f;
            //    }
            //}
            //1，这个方法检测地面“稳”
            //2，另外水上不应该有碰撞，以此类推其他地表下不应该有碰撞
            //SGF.Debuger.Log($"[move] {M_EntityBase.EntityType} forceMove: {nextPoint}");
            if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            {
                ulong hostID = M_EntityBase.SummonHostID;
                if (hostID != 0)
                {
                    Vector3 pos = GameManager.Instance.GetEntityPosById(hostID);
                    if (pos != null && pos != Vector3.zero)
                    {
                        nextPoint.y = pos.y;
                    }
                }
            }
            else
            {
                if (!M_IsStayInAir)
                {
                    SetPosOnGround(ref nextPoint);
                }
                //nextPoint.y += 0.08f;
            }

            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnForceMove] 11111 id={M_EntityBase.EntityId},localPos={transform.localPosition},nextPoint={nextPoint}");
            //}

            m_EntityPosition = transform.position = nextPoint;     //既然过程中同步了，实时设置即可[太远才配置]，不用干掉因为后面还有[1/很多]后续目标，直接设置Tweener播放中：动画[目前同步后是idle]和插入代码【会强制更新也没问题会变更的】

            //if (m_IsSyncPos)
            {
                m_entity.SetCurrentPos(nextPoint.x, nextPoint.y, nextPoint.z, true);    // 强制同步后同步逻辑层
            }
            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnForceMove] 22222 id={M_EntityBase.EntityId},localPos={transform.localPosition},nextPoint={nextPoint}");
            //}
            //if (navMeshAgent != null)
            //{
            //    navMeshAgent.enabled = true;
            //}
            if (m_CharacterController != null)
            {
                m_CharacterController.enabled = true;
            }
            ForceSetPosState = false;
            //-----------------------------------------------------------
        }

        /// <summary>
        /// 设置 一个坐标 落到地面上
        /// </summary>
        /// <param name="pos"></param> <summary>
        private void SetPosOnGround(ref Vector3 pos)
        {
            // 重新封装的 射线检测 
            Vector3 hitPos = PhysicsUtils.GetHitGroundPos(ref pos, ref birthHits, GroundLayer, out bool result);
            if (result)
            {
                /// 如果 目标点的 坐标 距离地表 范围 超过了 一定范围的 radio半径, 那就 将坐标设置为 radio的 0.6部分 
                if (m_CharacterController != null && Mathf.Abs(pos.y - hitPos.y) >= m_CharacterController.radius * 0.8f)
                {
                    pos.y = hitPos.y + (m_CharacterController.radius * 0.6f);
                }
            }
            birthHits = new RaycastHit[10];
        }

        private void FollowSummonHost()
        {
            if (!IsFollowSummonHost())
            {
                return;
            }
            float y = GetFollowSummonHostY(out var result);
            if (result)
            {
                if (math.abs(transform.position.y - y) > 0.02f)
                {
                    UpdatePos(transform.position.x, y, transform.position.z);
                }
            }
        }

        private bool IsFollowSummonHost()
        {
            if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            {
                if (M_EntityBase.EntityModelConfig == null)
                {
                    return false;
                }

                var bulletJson = M_EntityBase.EntityModelConfig as BulletJson;
                if (bulletJson == null)
                {
                    return false;
                }

                return bulletJson.config.BulletHeightType == BulletHeightType.FollowRole;
            }

            return false;
        }

        /// <summary>
        /// 获取 召唤物 主人的 Y
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private float GetFollowSummonHostY(out bool result)
        {
            result = false;
            ulong hostID = M_EntityBase.SummonHostID;
            if (hostID != 0 && true)
            {
                Vector3 pos = GameManager.Instance.GetEntityPosById(hostID);
                if (pos != null && pos != Vector3.zero)
                {
                    result = true;
                    return pos.y;
                }
            }
            return 0;
        }

        private Vector3 tempV3 = Vector3.zero;
        private void UpdatePos(float x, float y, float z)
        {
            tempV3.Set(x, y, z);
            UpdatePos(tempV3);
        }

        private void UpdatePos(Vector3 vector3)
        {
            m_EntityPosition = transform.position = vector3;
            m_entity.SetCurrentPos(vector3.x, vector3.y, vector3.z, true);
        }

        public override void OnBirthPos(Vector3 birthPos)
        {
            /// 2023/12/17
            /// 补丁:
            ///     用来处理 再 skillMove 位移途中收到服务器 ServerSetPosNTF 强同步,导致位移瞬移过去的bug.
            /// note:
            ///     目前跟夏哥确认的如下:
            ///     在收到强同步时, 客户端需要判定本地是否有原子锁, 在移动被锁定的情况下， 不应该直接设置坐标
            /// 补丁:
            ///     此处先 只处理 技能移动过程中的强同步
            if (M_EntityBase.Data.Is___ForbidMove && LerpMover1 != null && M_isSkillMove)
            {
                SGF.Debuger.LogWarning($"补丁: [viewVitalNPCNromal] 在技能移动过程中收到服务器强同步 pos: {birthPos}, 不处理. 此处曲爷 以后要改");
                return;
            }
            ForceSetPosState = true;
            if (m_CharacterController != null)
            {
                m_CharacterController.enabled = false;
            }
            //if (navMeshAgent != null)
            //{
            //    navMeshAgent.enabled = false;
            //}

            if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            {
                var bulletJson = M_EntityBase.EntityModelConfig as BulletJson;

                // 子弹的配置 如果存在
                if (bulletJson != null)
                {
                    // 判断 子弹跟随的类型, 如果 创建跟随地面
                    if (bulletJson.config.BulletHeightType == BulletHeightType.BornCheckSpace)
                    {
                        SetPosOnGround(ref birthPos);
                    }

                    // 如果 跟随主人创建坐标, 那就取 主人坐标
                    if (bulletJson.config.BulletHeightType == BulletHeightType.BornWithRole)
                    {
                        float y = GetFollowSummonHostY(out bool result);
                        if (result)
                        {
                            birthPos.y = y;
                        }

                    }
                }

            }
            else
            {
                if (!M_IsStayInAir)
                {
                    SetPosOnGround(ref birthPos);
                }

                //bool sd = Physics.Raycast(birthPos + (Vector3.up * 200), Vector3.down, out hit, 250f, LayerMask.GetMask("Ground"));
                //if (sd)
                //{
                //    birthPos.y = hit.point.y + 0.08f;
                //}
                //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
                //{
                //    SGF.Debuger.LogError($"子弹移动调试 [OnBirthPos] 11111111 id={M_EntityBase.EntityId},localPos={transform.localPosition},nextPoint={birthPos}");
                //}
                //birthPos.y += 0.08f;
            }

            UpdatePos(birthPos);
            // SGF.Debuger.Log($"[Move] {M_EntityBase.EntityType} set born pos : {birthPos}");


            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnBirthPos] 2222222 id={M_EntityBase.EntityId},localPos={transform.localPosition},nextPoint={birthPos}");
            //}
            //if (navMeshAgent != null)
            //{
            //    navMeshAgent.enabled = true;
            //}
            if (m_CharacterController != null)
            {
                m_CharacterController.enabled = true;
            }
            ForceSetPosState = false;
        }

        public override void OnAOIObjectMoveing(Vector3 movingMotion)
        {
            //必要就存储为变量，非必要就注释掉
            MoveCharacterController(movingMotion/*, "OnAOIObjectMoveing"*/);//基础移动，基础掉落
        }

        /// <summary>
        /// 立刻杀掉，并且原地暂停，但如原目标不变的话再次启动还会继续向着目标移动
        /// </summary>
        /// <param name="isFroce"></param>
        public override void KillLerpMover_StopInPlace(bool isFroce = false)
        {
            // if (M_EntityBase.IsMainPlayer)
            // {
            //     SGF.Debuger.Log($"[Move] KillLerpMover_StopInPlace  立即杀掉");
            // }
            //普通移动状态 或 任意状态的强制
            if ((LerpMover1 != null && LerpMover1.IsActive() && LerpMover1.IsPlaying() && !M_isSkillMove) || isFroce)
            {
                // SGF.Debuger.LogError($"[Move]  kill lerp");

                //LerpMover1.Kill(true);
                //1这个是错的，↑↑↑↑↑↑
                //2先到目标点，然后再移动到技能触发点，
                //3其实应该停下，
                //4那目前停下就说明服务器在过程中：
                //5,服务器先删除了path，
                //6后发送了技能点，
                //7此时还没到目标，
                //8说明什么：根据七推测还没到要继续往前走（命令），根据五知道怪物要暂停了（动作+逻辑），根据六推测走到技能点（命令），
                //9目前很有顺序比较流程通顺
                //10隐患：56是具有顺序的只有一起延迟，就是刷回去可以接受，我可以保证中途发我暂停继续逻辑MoveTODO：弱网朝向：弱网动作=
                LerpMover1.Kill(GameConfig.MoveSyncMode);//可能在0~0.5秒之间的任意个时间，如果在0.01秒的时候就会很闪烁，攻击前的倒数第二次移动

                if (this == null || transform == null)
                {
                    SGF.Debuger.LogWarning("[ViewVitalNpaNormal] KillLerpMover_StopInPlace: 节点销毁了,但是还是在移动!!!");
#if UNITY_EDITOR
                    Debug.Break();
#endif
                    return;
                }
                m_EntityPosition = transform.position;
                M_EntityBase.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);

                // fix:任务高亮时再次点击 高亮消失http://jira.sanguosha.com:8080/browse/STARS-2582(注释掉)
                if (isFroce)
                {
                    M_EntityBase.BreakFindPath();
                }


                //M_EntityBase.OnFindPathCallBack?.Invoke(false);
                //M_EntityBase.OnFindPathCallBack = null;
                //M_EntityBase.Is_MainPlayer_FindingPath = false;
            }
        }

        /// <summary>
        ///  调用环境：DotweenFix（条件调用），fixUpdate（常规调用），dotween通过时间算很多点通过插值fix给CC.Move频率不影响不会有问题
        /// </summary>
        /// <param name="movingMotion"></param>
        private void MoveCharacterController(Vector3 movingMotion/*, string tag*/)
        {
            //if (m_entity.EntityType != E_EntityType.Monster)
            //{
            //    return;
            //}
            //Vector3 curPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            //SGF.Debuger.LogError($"[pos-x-{m_entity.EnityId}] ,tag {tag},  [MoveCharacterController] movingMotion [{movingMotion.x},{movingMotion.y},{movingMotion.z}] , curPos [{curPos.x},{curPos.y},{curPos.z}] ");

            Move(movingMotion);

            //Vector3 newPos = transform.localPosition;
            //SGF.Debuger.LogError($"[pos-x-{m_entity.EnityId}] ,tag {tag} --> [MoveCharacterController] newPos [{newPos.x},{newPos.y},{newPos.z}] ,magnitude {(newPos - curPos).magnitude} ");
        }

        protected virtual void Move(Vector3 movingMotion)
        {
            if (m_CharacterController == null)
            {
                return;
            }
            m_CharacterController.Move(movingMotion);//【其他别人通知】
        }

        /// <summary>
        /// 第三方每帧 的 移动逻辑。
        /// 其实 服务器 驱动的 第三方 属性坐标移动 都可以 采用每帧驱动的方式。
        /// 在网络波动的情况下， 通过 M_EntitySmoothMoveV3 方式 达到尽量平滑的移动.
        /// 目前 移动采用 tween 的方式, 先保留，在 黑洞状态下， 切换到每帧移动的模式
        /// </summary>
        public void OnFrameMove()
        {
            if (M_EntityBase == null || M_EntityBase.Data == null)
            {
                return;
            }
            // 在原子锁的状态下的 移动 由 服务器每帧驱动, 同时 客户端做平滑
            if (M_EntityBase.Data.IsBattleState_BlackHole)
            {
                // 看样子 不太好直接传 force = true. 因为 技能移动 也是走的lerpMove
                KillLerpMover_StopInPlace(false);

                // 虽然我不一定能不掉 lerpMove. 但是我可以清除 第三人移动的 ThdPersionDataCache 队列.
                // 同时，  在每帧中去 取服务器同步过来的每帧的 移动偏移量
                ThdPersionDataCache.Clear();

                // note:
                // TODO: DL
                // 原子锁状态下的黑洞属性同步问题 后面可能需要再去细化,目前主角在黑洞状态下的技能释放没看出太大问题
                // 此处可能需要区分以下 原子锁 是 客户端锁还是服务器锁。 
                // 客户端锁一般是为了动画， 而 服务器锁 是服务器通知。 
                // 黑洞状态下, 原子锁锁住应该也能被属性同步才对
                if (!M_EntityBase.Data.Is___ForbidMove)
                {
                    moveMotion = M_EntityBase.M_EntitySmoothMoveV3.UpdateMoveData(M_EntityBase.m_EntityServerFrameMoveDir).CurMoveData;
                    SGF.Debuger.LogWarning($" [黑洞] [平滑] M_EntityServerFrameMoveDir: {M_EntityBase.m_EntityServerFrameMoveDir} ---> {moveMotion}  ");

                    // MoveState
                    MoveCharacterController(moveMotion);//时刻更新当前
                    M_EntityBase.m_EntityServerFrameMoveDir = Vector3.zero;


                    // 同步表现层坐标
                    M_EntityBase.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);//过程【中】同步逻辑层
                }
            }
        }

        public void OnMoveStateChange()
        {
            if (!M_EntityBase.Data.IsBattleState_BlackHole)
            {
                return;
            }
            if (M_EntityBase.Data.isMainPlayer)
            {
                return;
            }

            // 其他人收到了移动状态改变, 并且在黑洞效果的情况下, 
            if (SGF.Time.TimeUtils.ServerNowStampMilli - M_EntityBase.MoveState <= 100)
            {
                I_AnimParam animParam = M_EntityBase.GetAnimParamByState(E_ULayerSubState.SingleMoving);
                M_EntityBase.ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false);
            }
            else
            {
                I_AnimParam animParam = M_EntityBase.GetAnimParamByState(E_ULayerSubState.Idle);
                M_EntityBase.ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
            }
        }

        #endregion

        #region 显隐逻辑

        public void OnActionVisible(bool visible)
        {
            //Animancer.gameObject.SetActive(visible);
            Renderer render = Animancer.gameObject.GetComponent<Renderer>();
            if (render != null)
            {
                render.enabled = visible;
            }
        }

        private float lastScale = 1;

        /// <summary>
        /// 如果需要战斗隐身要单独传
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="obj"></param>
        private void OnLogicControlShow(EntityShowHidenTag tag, bool obj)
        {
            // SGF.Debuger.LogError($"[shadow] OnLogicControlShow : {tag} ");
            //SGF.Debuger.LogError($"EntityType={M_EntityBase.EntityType},entityid={M_EntityBase.EntityId},tag={tag},show={obj}");
            if (tag == EntityShowHidenTag.Self || tag == EntityShowHidenTag.BattleSelf)
            {
                bool isHaveSkinMeshEender = m_SkinMeshRender.Count > 0;
                // 隐藏模型：通用处理
                for (int i = 0; i < m_SkinMeshRender.Count; i++)
                {
                    m_SkinMeshRender[i].enabled = obj;

                }
                if (CircleShadow != null)
                {
                    CircleShadow.gameObject.active &= obj;
                }
                LogicSignShow = obj;
                if (tag == EntityShowHidenTag.BattleSelf)
                {
                    BattleDesignShow = obj;
                }

                //子弹的单独二级处理
                {
                    if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
                    {
                        float scale = obj ? 1 : 0;
                        // SGF.Debuger.LogError($"[=x=] {transform.name} scale : {m_ModelOffset.transform.localScale} , try setScale {scale}");
                        m_ModelOffset.transform.SetScaleXYZ(scale, scale, scale);
                        lastScale = scale;
                        // SGF.Debuger.LogError($"[=x=] {transform.name} ===》 scale : {m_ModelOffset.transform.localScale} ");
                    }
                    else if (!isHaveSkinMeshEender && M_ModelOffLineData != null && M_ModelOffLineData.gameObject != null)
                    {
                        // 如果没有骨骼，是粒子直接当成模型来用，父节点scale为0，并不会隐藏粒子
                        // 所以直接关闭绑点的root
                        M_ModelOffLineData?.gameObject.SetActive(LogicSignShow);
                    }
                }

                //=========
                // 发送事件隐藏组件
                M_EntityBase.CompoentShowAction?.Invoke(!obj);
            }
        }

        #endregion

        #region 创建本地模拟召唤物效果
        private Transform FindEnsureNode(Transform targetNode, string nameKey)
        {
            Transform node = null;
            node = targetNode.Find(nameKey);
            if (node == null)
            {
                var go = new GameObject(nameKey);
                node = go.transform;
                node.SetParent(targetNode, false);
            }

            return node;
        }

        private Transform GetSimulateRootNode(bool isFollowRotate, ulong runtimeID)
        {
            Transform node = null;
            if (isFollowRotate)
            {
                node = FindEnsureNode(m_ModelOffset, $"SimulateRoot_{runtimeID}");
            }
            else
            {
                node = FindEnsureNode(transform, $"SimulateNoFollowRotateRoot_{runtimeID}");

            }
            return node;
        }

        Tween simulateSummonTween = null;
        private void OnActionTurnSimulateSummon(ulong runtimeID, int angle, bool isFollowRotate, bool isPlay)
        {
            Transform root = GetSimulateRootNode(isFollowRotate, runtimeID);

            // 先关闭 之前 模拟召唤物的 tween
            if (simulateSummonTween != null)
            {
                simulateSummonTween.Kill();
            }

            // 播放那就产生一个新的 tween
            if (isPlay)
            {
                float time = 360.0f / angle;
                // float time = 3.3f;
                simulateSummonTween = root.DOLocalRotate(new Vector3(0, 360, 0), time, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            }
        }
        public void OnActionCreateSimulateSummon(ulong runtimeID, string key, Vector3 pos, int avatarID, bool isFollowRotate)
        {
            // 创建相应的 goObject， 召唤物的坐标由 go 来控制即可
            GameObject go = new(key);
            Transform parent = GetSimulateRootNode(isFollowRotate, runtimeID);
            go.transform.SetParent(parent, false);

            // 设置相应的 召唤物坐标;
            go.transform.localPosition = pos;

            // 创建本地实体
            ulong entityID = GameManager.Instance.CreateLocalEntity(StarProjectDef.E_LocalEntityType.Summon, new object[]
                        {
                            // 召唤物的 模型id
                            avatarID,
                            // 召唤物的出生点坐标, 因为存在 父节点 go, 所以坐标可以不设置
                            Vector3.zero,
                            // 跟随的父节点
                            go.transform,
                            // 设置为跟随 父节点
                            true,
                            // 模拟召唤物的 summonHost
                            m_entity.EntityId
                        });

            // 将召唤物的 key 和 entityID 绑定
            GameManager.Instance.RecordFormateKey2EntityID(key, entityID);

        }
        #endregion

        #region 主角脚印效果

        private Footprints FootprintRoot = null;

        private Action<GameObject> LoadFootprintCB = null;

        protected override void CheckCreateFootprint()
        {
            bool m_isCreateFootprint = false;
            if (M_EntityBase != null && M_EntityBase.IsMainPlayer)
            {
                m_isCreateFootprint = GameMap.IsCreateFootprint;
            }
            if (m_isCreateFootprint == false)
            {
                if (FootprintRoot != null)
                {
                    FootprintRoot.gameObject.SetActive(false);
                }
            }
            else
            {
                CreateFootprint();
            }
        }

        private void CreateFootprint()
        {
            if (FootprintRoot == null)
            {
                LoadFootprintCB = (GameObject go) =>
                {
                    if (go != null)
                    {
                        GameObject gob = GameObject.Instantiate<GameObject>(go);
                        gob.transform.SetParent(transform);
                        gob.transform.SetLocalScale(Vector3.one);
                        gob.transform.localPosition = Vector3.zero;
                        gob.transform.localRotation = Quaternion.identity;
                        FootprintRoot = gob.GetComponent<Footprints>();
                        gob.SetActive(true);
                    }
                    LoadFootprintCB = null;
                };
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(GameConfig.MAIN_PLAYER_FOOTPRINT_PATH, LoadFootprintCB);
            }
            else
            {
                if (FootprintRoot != null)
                {
                    FootprintRoot.gameObject.SetActive(true);
                }
                LoadFootprintCB = null;
            }
        }


        #endregion


        protected override void Reset()
        {
            KeepStartIdleNotStand = false;
            //SGF.Debuger.Log($"[Idle_{m_entity.EntityId}] Reset : KeepStartIdleNotStand {KeepStartIdleNotStand} ");
            LoadFootprintCB = null;

            OnActionDie();  // 清除所有的dotween

            if (FootprintRoot != null)
            {
                Destroy(FootprintRoot.gameObject);
            }

            ResetPlayBeAttackFlashColor();

            base.Reset();
        }

        protected override void Release()
        {
            GlobalEvent.OnQualChangeTinyModuleReflesh.RemoveListener(OnShowLevelReflesh);

            // 还原默认的 shader 状态
            ResetDefaultCharState();

            Reset();
            base.Release();
        }

        private int groundLayer = -1;
        public int GroundLayer //这里是根据unity实际转化int值
        {
            get
            {
                if (groundLayer == -1)
                {
                    groundLayer = LayerMask.NameToLayer(E_LayerType.Ground.ToString());
                }
                return groundLayer;
            }
        }

        RaycastHit[] hits = new RaycastHit[10];    // 定义RaycastHit数组，存储碰撞信息
        private RaycastHit[] birthHits = new RaycastHit[10];    // 定义RaycastHit数组，存储碰撞信息

        int hitCount = 0;
        bool hasGround = false;
        Ray ray = new(Vector3.zero, Vector3.down);
        ////不行这个是一直在变的值，初始化之后会设定不能直接节省，centerY是相对根节点的相对值
        //private float rayCheckDist = m_CharacterController.center.y + fixCharacterCtrl4Terrain;
        /// <summary>
        /// 不用这个判断，之后利用自己写的box判断，修改枚举
        /// 通过枚举来决策
        /// </summary>
        /// <returns></returns>
        protected override bool IsGrounded()
        {
            if (isOnloading >= 1)
            {
                return true;
            }
            if (this != null && this.transform != null)
            {
                hasGround = false;
                /*  this.jobHandle.Complete();

                  if (!raycastHitNativeArray.IsCreated)
                  {
                      return true;
                  }
                  for (int i = 0; i < raycastHitNativeArray.Length; i++)
                  {
                      //所有脚下的都是Ground （桥碰撞部分也是Ground）；向下不必关心Wall
                      if (raycastHitNativeArray[i].collider != null && raycastHitNativeArray[i].collider.gameObject.layer == GroundLayer)
                      {
                          hasGround = true;
                          break;
                      }
                  }

                  raycastRaycastCommandNativeArray[0] = new RaycastCommand(this.transform.position, Vector3.down, 0.05f);
                  this.jobHandle = RaycastCommand.ScheduleBatch(raycastRaycastCommandNativeArray, raycastHitNativeArray, 1);*/

                ray.origin = transform.position;
                ray.direction = Vector3.down;
                if (Physics.Raycast(ray, out RaycastHit hit, GameConfig.PLAYER_GRI_CHECKER_LEN)) // 射线向下检测，最大长度为0.05f
                {

                    if (hit.collider.gameObject.layer == GroundLayer) // 判断是否是地面Layer
                    {
                        return true; // 检测到地面，返回true
                    }
                }

                return false; // 未检测到地面，返回false
            }



            //调用时机的时候，如果初始化什么都没有，没检测就会是自由落体一直往下掉落
            return hasGround;
        }


        //private void OnTriggerEnter(Collider other)
        //{
        //    Debug.Log("OnTriggerEnter" + other);
        //}
        //private void OnTriggerExit(Collider other)
        //{
        //    Debug.Log("OnTriggerExit" + other);
        //}


    }
}
