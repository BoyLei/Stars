using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.Input;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarProject.UI.SkillBtn
{
    public class UniversalButton : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
    {
        [HideInInspector]
        public string LOG_TAG => $"UniversalButton_{skillUnit?.CurShowSkillInfo?.skillId}";

        public RectTransform ActiveAimer;       // 活跃的指示器（黄色）
        public RectTransform ActivePointer;     // 活跃的指针（黄色）
        public RectTransform PressedAimer;      // 禁止的指示器（红色）
        public RectTransform PressedPointer;    // 禁止的指针（红色）

        #region :: Config ::
        public CanvasScaler scaler;
        protected bool isAimable = false;      // 是否有指示器
        protected bool isCanCancel = false;    // 是否能取消
        private SkillCanceller m_SkillCanceller;  // 取消技能
        protected E_SkillBtnState state = E_SkillBtnState.Active;   // 按钮状态  
        protected bool M_IsActive = true;   // 按钮状态是否活跃的

        #endregion

        #region :: Parameters ::

        protected float aimerRadius;   // 按钮指示器的半径
        protected Vector3 initialFingerPosition;    // 手指初始点击坐标
        protected int fingerId = -99;  // EventData手指ID
        protected Vector3 fingerPosition;   // 手指初始点击坐标2
        protected Vector3 direction;   // 按钮指示器遥感的方向向量
        protected Vector3 directionXZ; // 按钮指示器遥感的方向向量 Y轴一直为0
        protected Vector3 rawDir;   // 技能方向向量中的临时缓存的
        private float cancellerRadius;   // 取消按钮的半径，包括canvas的缩放比
        private bool canActivateSkill = true;   // 记录是否是在取消技能按钮时抬起的

        #endregion

        #region Cosmetics
        [HideInInspector]
        private Color colorActive = Color.white;
        [HideInInspector]
        private Color colorInactive = Color.gray;
        [HideInInspector]
        private Color colorPressed = Color.white;
        #endregion

        #region Events
        public int SkillBtnPos = -1;  // 技能按钮的槽位
        [HideInInspector]
        public Action<UniversalButton> onPointerDown;
        [HideInInspector]
        public Action<UniversalButton> onBeginDrag;
        [HideInInspector]
        public Action<UniversalButton> onDrag;
        [HideInInspector]
        public Action<Vector3, E_SkillBtnState, float> onPointerUp;
        [HideInInspector]
        public Action<Vector3> onEndDrag;
        [HideInInspector]
        public Action onCancelSkill;
        [HideInInspector]
        public Action<Vector3> BtnDirChange;    // 按钮遥感移动的方向向量改变 委托
        [HideInInspector]
        public Action onPressing;   // 长按委托

        /// <summary>
        /// 指示器被抬起的 UI通知．
        /// TODO: 曲
        /// 1.此处 的 OnPointerUPUI 只是为了通知 skillComponent 按钮被抬起。
        /// 2.按曲 的 说法, skillComponent 那边 onPointerUp 会通知技能, 所以此处 新增一个OnPointerUPUI 单独告诉ui 按钮抬起.
        /// 
        /// 3.后续  UniversalButton 应该移除跟技能相关的逻辑，此处应该只是 纯粹的 虚拟按钮逻辑. 所有技能相关的逻辑 放入 skillComponent 中
        /// /// </summary>
        public Action<UniversalButton> OnPointerUPUI;
        #endregion

        #region 【长按】

        private bool interactable = true;    // 长按配置
        private float intervalTime = 0f; // 长按多少秒触发时间
        [HideInInspector]
        public float trrigerIntervalTime = 0.1f;    // 长按每次多久触发一次事件
        private bool m_IsStartPress = false;    // 开始长按的倒计时 锁
        private float m_CountTime;  // 开始长按的倒计时
        private bool isPressing = false; // 触发长按回调的倒计时 锁
        private float m_TriggerCountTime;   // 触发长按回调的倒计时

        #endregion

        #region 技能按钮

        public Text M_Countdown;
        public Text M_SkillIconDes; // 技能的描述（长按、位移）
        public Image M_CountdownIcon;
        public Image M_SkillIcon;
        public Image M_SkillJobSpectralUI;
        public Text M_SkillJobSpectralText;

        public Image M_MultistageCountdownIcon;
        public GameObject M_Lock;
        public Text M_LockLv;
        public AnimParamsShow SpecialAnim;
        public CanvasGroup SpecialAnimCanvasGroup;
        public Image M_WaitIntervalCountDown;

        private Image m_SkillBgIcon;

        private float m_SkillCooldownSum;    // 技能总倒计时
        private float m_SkillCooldown;  // 当前倒计时

        private float m_SkillWaitIntervalCooldownSum;
        private float m_SkillWaitIntervalCooldown;

        [HideInInspector]
        public SkillContainer skillUnit;    // 绑定技能数据层
        private bool IsHasSkill = false;
        private EntityCtrlBase M_EntityCtrlBase;    // 绑定技能数据层
        private SkillPosSetDataCell skillPosSetDataCell;    // 技能按钮位默认信息配置
        //private int m_ShowCancellerTime = 0;    // 显示取消按钮的次数
        private int m_KeyCode = -1; // 技能按钮的物理按钮key
        [HideInInspector]
        public bool m_IsKeyDown = false;
        private bool isSendEnergyAction = true;
        private ulong m_CurSkillRumTime = 0;    // 当前技能运行时id （蓄力技能需要记录）
        private bool m_IsNotShowCD = false;
        private bool m_EnergySkillShowCD = true;

        private Action<Sprite> loadSkillIcon;
        private Action<Sprite> loadSkillBgIcon;
        private Action<Sprite> loadSkillConsumeBg;

        private List<GameObject> IntensifyAnim = new();
        private GameObject DodgeCDFinishAnim = null;

        private Service.SystemOpen.SystemItem sysItem = null;
        private bool m_IsCanClick = true;

        #endregion

        #region 动画
        //private float animCheckIntervalTime = 10000f;  // 按钮动画防错
        public float m_PlayerClickIntervalTime = 200f;   // 播放点击动画的间隔（毫秒）
        private long m_LastTime = 0;
        public List<AnimationClip> Clips = new();
        private Animancer.AnimancerComponent Animancer;

        private Dictionary<string, AnimationClip> AnimationClips = new();


        public Animator anim;

        #endregion

        #region  给 按钮增加一个 button 事件
        [Serializable]
        /// <summary>
        /// Function definition for a button click event.
        /// </summary>
        public class ButtonClickedEvent : UnityEvent { }

        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnClick = new();


        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }
        #endregion

        protected virtual void Awake()
        {
            InputManager.Instance.OnVirtualInput += InputVKey;
            GlobalEvent.onMainPlayerDie.AddListener(OnMainPlayerDie);
            GlobalEvent.OnPlayTimelineEvent.AddListener(OnPlayTimelineEventCB);
            GlobalEvent.onSceneBeginChange.AddListener(onSceneBeginChangeCB);
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
            GlobalEvent.OnXinSGUIShow.AddListener(OnXinSGUIShowCB);

            InitAnim();
        }

        protected virtual void Start()
        {
            this.UpdateBound();
        }

        protected virtual void Update()
        {
            #region 按钮倒计时显示
            if (m_SkillCooldown > 0)
            {
                m_SkillCooldown -= Time.deltaTime;
                if (m_SkillCooldown > 0f)
                {
                    SetCountdown(math.ceil(m_SkillCooldown).ToString());
                    //SetText(((int)cooldown % 60).ToString());
                }
                else
                {
                    SetCountdown("");
                    SetState(E_SkillBtnState.Active);
                    //SetActiveState(true);
                }
                //SGF.Debuger.LogWarning($"技能倒计时 m_SkillCooldown={m_SkillCooldown}");

                SetCountdownIcon(true);
            }
            #endregion

            #region 间断倒计时

            if (m_SkillWaitIntervalCooldown > 0)
            {
                m_SkillWaitIntervalCooldown -= Time.deltaTime;
                SetWaitIntervalCooldown(m_SkillWaitIntervalCooldown);
            }

            #endregion

            #region 长按逻辑
            if (interactable)
            {
                if (m_IsStartPress)
                {
                    //onPressing?.Invoke();   // 刚按下，立即触发一次
                    m_CountTime -= Time.deltaTime;
                    if (m_CountTime <= 0)
                    {
                        m_TriggerCountTime = trrigerIntervalTime;
                        m_IsStartPress = false;
                        isPressing = true;
                    }
                }
                if (isPressing)
                {
                    m_TriggerCountTime -= Time.deltaTime;
                    if (m_TriggerCountTime <= 0)
                    {
                        m_TriggerCountTime = trrigerIntervalTime;
                        onPressing?.Invoke();
                    }
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                if (interactable && onPressing != null && (m_IsStartPress || isPressing))
                {
                    m_IsStartPress = false;
                    isPressing = false;
                }
            }
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchCount <= 0)
            {
                if (interactable && onPressing != null && (m_IsStartPress || isPressing))
                {
                    m_IsStartPress = false;
                    isPressing = false;
                }
            }
#endif

            #endregion

            #region 防错

            //if (IsHasSkill && animCheckIntervalTime > 0)
            //{
            //    animCheckIntervalTime -= Time.deltaTime;
            //    if (animCheckIntervalTime <= 0)
            //    {
            //        UpdateColor();
            //        PlayAnimation("SkillAnim_Default");
            //        animCheckIntervalTime = 10000f;
            //    }
            //}

            #endregion
        }

        protected virtual void Destroy()
        {
            Release();
        }

        public void Release()
        {
            Reset();
        }

        private void Reset()
        {
            InputManager.Instance.OnVirtualInput -= InputVKey;
            GlobalEvent.onMainPlayerDie.RemoveListener(OnMainPlayerDie);
            GlobalEvent.OnPlayTimelineEvent.RemoveListener(OnPlayTimelineEventCB);
            GlobalEvent.onSceneBeginChange.RemoveListener(onSceneBeginChangeCB);
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
            GlobalEvent.OnXinSGUIShow.RemoveListener(OnXinSGUIShowCB);

            ResetSkillData();
        }

        public void ResetSkillData()
        {
            ReleaseAction();
            skillUnit = null;
            M_EntityCtrlBase = null;
            skillPosSetDataCell = null;
            //m_ShowCancellerTime = 0;
            m_KeyCode = -1;
            loadSkillIcon = null;
            loadSkillBgIcon = null;
            M_WaitIntervalCountDown?.gameObject.SetActive(false);
            DodgeCDFinishAnim?.SetActive(false);
            M_CountdownIcon?.gameObject.SetActive(true);
            M_Countdown?.gameObject.SetActive(true);
            M_MultistageCountdownIcon?.gameObject.SetActive(false);

            //gameObject.SetActive(false);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (skillUnit != null)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} 技能按钮 OnPointerDown state={state},isSendEnergyAction={isSendEnergyAction},m_IsKeyDown={m_IsKeyDown}");
            }

            if (M_EntityCtrlBase == null && M_EntityCtrlBase.M_Curr == null && M_EntityCtrlBase.M_Curr.Data == null)
            {
                return;
            }

            if (!m_IsCanClick)
            {
                return;
            }

            // if (M_EntityCtrlBase.M_Curr.EntityType == E_EntityType.Player)
            // {
            //     var res = skillUnit.GetCurNotMatchAttrCondition(out var isOver);
            //     if (res != null)
            //     {
            //         // 条件不足
            //         var JobID = M_EntityCtrlBase.M_Curr.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            //         JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)JobID);
            //         VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)res.ID);
            //         string attrName = string.Empty;
            //         if (vitalSignAOIClientAttrs != null)
            //         {
            //             switch (vitalSignAOIClientAttrs.Name)
            //             {
            //                 case AOIAttrDefine.curSpectral1:
            //                     {
            //                         attrName = jobDataCell.Spectral1;
            //                     }
            //                     break;
            //                 case AOIAttrDefine.curSpectral2:
            //                     {
            //                         attrName = jobDataCell.Spectral2;
            //                     }
            //                     break;
            //                 case AOIAttrDefine.curSpectral3:
            //                     {
            //                         attrName = jobDataCell.Spectral3;
            //                     }
            //                     break;
            //                 default:
            //                     break;
            //             }
            //         }
            //         Frame.Util.ShowMessageByCode(227, attrName);
            //         return;
            //     }
            // }


            if (state == E_SkillBtnState.Active)
            {
                if (m_KeyCode != -1)
                {
                    if (!m_IsKeyDown)
                    {
                        if (eventData != null)
                        {
                            fingerId = eventData.pointerId;
                            initialFingerPosition = eventData.position;
                        }
                        InputManager.Instance.DispatchVKey(m_KeyCode, 1);
                        return;
                    }
                }

                if (m_IsKeyDown)
                {
                    // 如果是技能按钮，先判断技能能否释放技能
                    if (skillUnit != null && M_EntityCtrlBase != null)
                    {
                        bool isCanClick = M_EntityCtrlBase.CheckCanSkillBtn(skillUnit);
                        if (!isCanClick)
                        {
                            return;
                        }
                    }
                    directionXZ = Vector3.zero;
                    fingerPosition = initialFingerPosition;
                    UpdateIndicatorShow(true);
                    SetState(E_SkillBtnState.Pressed);
                    //SGF.Debuger.LogError($"{LOG_TAG} 发送使用技能");

                    onPointerDown?.Invoke(this);
                }
            }

            m_OnClick.Invoke();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (/*isAimable &&*/ state == E_SkillBtnState.Pressed)
            {
                this.UpdateAiming(eventData);
                onBeginDrag?.Invoke(this);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == fingerId && state == E_SkillBtnState.Pressed)
            {
                //if (isAimable)
                {
                    UpdateAiming(eventData);
                }

                if (isCanCancel && m_SkillCanceller != null)
                {
                    this.UpdateSkillCancellerState();
                }

                onDrag?.Invoke(this);
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (skillUnit != null)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} 技能按钮 OnPointerUp state={state},isSendEnergyAction={isSendEnergyAction},m_IsKeyDown={m_IsKeyDown}");
            }

            if (!m_IsCanClick)
            {
                return;
            }

            UpdateIndicatorShow(false);

            if (state == E_SkillBtnState.Pressed || state == E_SkillBtnState.OnCooldown)
            {
                if (m_KeyCode != -1)
                {
                    if (m_IsKeyDown)
                    {
                        InputManager.Instance.DispatchVKey(m_KeyCode, 0);
                        return;
                    }
                }
                if (!m_IsKeyDown)
                {
                    E_SkillBtnState enterState = state;
                    fingerId = -99;
                    SetState(E_SkillBtnState.Active);
                    //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----222222222 skillid={skillUnit.CurSkillInfo.cfg.ID},isSendEnergyAction={isSendEnergyAction}");

                    if (isSendEnergyAction)
                    {
                        // TODO: 曲 这块先这样改，一会儿再调整下
                        //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----333333333333 skillid={skillUnit.CurSkillInfo.cfg.ID},canActivateSkill={canActivateSkill}");
                        if (canActivateSkill)
                        {
                            //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----4444444444 发发发发发");
                            onPointerUp?.Invoke(directionXZ, enterState, _arg);
                        }
                        else if (onCancelSkill != null)
                        {
                            //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----55555555555 skillid={skillUnit.CurSkillInfo.cfg.ID}");
                            onCancelSkill.Invoke();
                        }

                        BtnDirChange?.Invoke(Vector3.zero);
                    }

                    if (m_SkillCooldown > 0f)
                    {
                        SetCountdown(math.ceil(m_SkillCooldown).ToString());
                    }
                    SetCountdownIcon(false);

                    m_EnergySkillShowCD = true;
                    //SGF.Debuger.Log($"虚拟按键 keyCode={m_KeyCode}, 蓄力技能 显示CD");
                }

                OnPointerUPUI?.Invoke(this);
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (isAimable)
            {
                directionXZ = Vector3.zero;
                onEndDrag?.Invoke(directionXZ);
                BtnDirChange?.Invoke(directionXZ);

                UpdateIndicatorShow(false);
            }
        }

        /// <summary>
        /// 更新手指节点的位置
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void UpdateAiming(PointerEventData eventData)
        {
            fingerPosition.x = eventData.position.x;
            fingerPosition.y = eventData.position.y;
            rawDir = fingerPosition - ActiveAimer.position;
            rawDir = Vector3.ClampMagnitude(rawDir, aimerRadius);
            ActivePointer.position = ActiveAimer.position + rawDir;
            PressedPointer.position = ActiveAimer.position + rawDir;

            this.UpdateDirection();
        }

        /// <summary>
        /// 更新方向向量
        /// </summary>
        protected virtual void UpdateDirection()
        {
            direction = aimerRadius <= 0 ? rawDir : rawDir / aimerRadius;
            directionXZ.x = direction.x;
            directionXZ.y = 0f;
            directionXZ.z = direction.y;
            BtnDirChange?.Invoke(directionXZ);
        }

        protected void SetState(E_SkillBtnState e_SkillBtnState, bool isChanageColor = true)
        {
            state = e_SkillBtnState;
            if (isChanageColor)
            {
                UpdateColor();
            }
        }

        /// <summary>
        /// 更新颜色
        /// </summary>
        private void UpdateColor()
        {
            if (M_SkillIcon == null)
            {
                return;
            }
            if (m_IsCding && !m_IsNotShowCD && m_EnergySkillShowCD)
            {
                M_SkillIcon.color = colorInactive;
                // if (skillUnit != null && skillUnit.CurShowSkillInfo != null)
                // {
                //     SGF.Debuger.Log($"{LOG_TAG} 技能按钮置灰了 UpdateColor id={skillUnit.CurShowSkillInfo.skillId},state={state},m_IsCding={m_IsCding},m_IsNotShowCD={m_IsNotShowCD},m_EnergySkillShowCD={m_EnergySkillShowCD}");
                // }
            }
            switch (state)
            {
                case E_SkillBtnState.None:
                    M_SkillIcon.color = colorInactive;
                    if (skillUnit != null && skillUnit.CurShowSkillInfo != null)
                    {
                        // SGF.Debuger.Log($"{LOG_TAG} 技能按钮置灰了111 UpdateColor id={skillUnit.CurShowSkillInfo.skillId},state={state},m_IsCding={m_IsCding},m_IsNotShowCD={m_IsNotShowCD},m_EnergySkillShowCD={m_EnergySkillShowCD}");
                    }
                    break;
                case E_SkillBtnState.Active:
                    M_SkillIcon.color = colorActive;
                    break;
                case E_SkillBtnState.Inactive:
                    M_SkillIcon.color = colorActive;
                    break;
                case E_SkillBtnState.Pressed:
                    M_SkillIcon.color = colorPressed;
                    break;
            }
            if (M_SkillIcon.color != colorActive)
            {
                SGF.Debuger.Log($"{LOG_TAG} 刷新按钮 UpdateColor id={skillUnit.CurShowSkillInfo.skillId},state={state},m_IsCding={m_IsCding},m_IsNotShowCD={m_IsNotShowCD},m_EnergySkillShowCD={m_EnergySkillShowCD}");
            }

            //if (skillUnit.CurShowSkillInfo.skillId == 300301)
            //{
            //    SGF.Debuger.LogError($"{LOG_TAG} 技能按钮 UpdateColor id={skillUnit.CurShowSkillInfo.skillId},state={state},m_IsCding={m_IsCding}");
            //}
            //SGF.Debuger.LogWarning($"{LOG_TAG} 技能按钮 UpdateColor id={skillUnit.CurShowSkillInfo.skillId},state={state},m_IsCding={m_IsCding}");
        }

        /// <summary>
        /// 更新按钮和取消按钮的范围
        /// </summary>
        public void UpdateBound()
        {
            if (isAimable)
            {
                aimerRadius = ActiveAimer.rect.width / 2f * scaler.scaleFactor;
            }

            if (m_SkillCanceller != null)
            {
                cancellerRadius = m_SkillCanceller.GetComponent<RectTransform>().rect.width / 2f * scaler.scaleFactor;
            }
        }

        #region 取消按钮逻辑

        /// <summary>
        /// 更新取消按钮的显隐
        /// </summary>
        /// <param name="isShow"></param>
        private void UpdateSkillCancellerActive(bool isShow)
        {
            if (m_SkillCanceller == null || skillUnit == null)
            {
                return;
            }
            if (skillUnit != null && skillUnit.skillPos != null)
            {
                m_SkillCanceller?.SetVisiableState(skillUnit.skillPos.PosID, isShow);
            }

            //m_SkillCanceller.SetVisiable(isShow);
        }

        /// <summary>
        /// 更新取消按钮的状态
        /// </summary>
        private void UpdateSkillCancellerState()
        {
            if (m_SkillCanceller == null)
            {
                return;
            }

            bool isFinger = IsFingerOverSkillCancellerButton();
            canActivateSkill = !isFinger;

            if (isFinger)
            {
                m_SkillCanceller.SetState(E_SkillBtnState.Pressed);
            }
            else
            {
                m_SkillCanceller.SetState(E_SkillBtnState.Active);
            }

            if (isAimable)
            {
                ActiveAimer.gameObject.SetActive(!isFinger);
                PressedAimer.gameObject.SetActive(isFinger);
            }
        }

        private bool IsFingerOverSkillCancellerButton()
        {
            if (m_SkillCanceller == null)
            {
                return false;
            }
            return Vector3.Distance(fingerPosition, m_SkillCanceller.transform.position) < cancellerRadius;
        }

        protected virtual void UpdateIndicatorShow(bool isShow, bool isEndPressing = true)
        {
            //---- 设置取消技能
            if (isCanCancel && m_SkillCanceller != null)
            {
                UpdateSkillCancellerActive(isShow);
                //m_ShowCancellerTime++;
                UpdateSkillCancellerState();
            }
            // ---- 设置滑动框
            if (isAimable)
            {
                ActiveAimer.gameObject.SetActive(isShow);
                ActiveAimer.localPosition = Vector3.zero;
                ActivePointer.gameObject.SetActive(isShow);
                ActivePointer.localPosition = Vector3.zero;

                PressedAimer.gameObject.SetActive(false);
                PressedAimer.localPosition = Vector3.zero;
                PressedPointer.gameObject.SetActive(false);
                PressedPointer.localPosition = Vector3.zero;
            }
            // -- 标记抬起的状态
            GameInput.SetTouchState(transform.name, isShow);
            // -- 如果有指示器就抬起来
            if (M_EntityCtrlBase != null && !isShow)
            {
                M_EntityCtrlBase.SkillPointerUpActions?.Invoke(skillUnit);
            }
            // 是否可以触发长按
            if (interactable && onPressing != null)
            {
                if (isEndPressing)
                {
                    m_IsStartPress = isShow;
                    isPressing = false;
                    m_CountTime = intervalTime;
                }
            }
        }

        #endregion

        #region 动画

        private void InitAnim()
        {
            AnimationClips.Clear();
            if (Clips != null && Clips.Count > 0)
            {
                foreach (var item in Clips)
                {
                    AnimationClips.Add(item.name, item);
                }
            }
            //Animancer = transform.GetComponent<Animancer.AnimancerComponent>();
        }

        private void PlayAnimation(string AimationName)
        {
            if (anim == null)
            {
                return;
            }
            if (SkillBtnPos != 1)
            {
                anim.Play(AimationName);
            }

            //if (Animancer == null)
            //{
            //    return;
            //}
            //SGF.Debuger.Log($"技能倒计时 播放动画 AimationName={AimationName}");

            //if (AnimationClips.TryGetValue(AimationName, out AnimationClip animationClip) && animationClip != null)
            //{
            //    Animancer.Animator.enabled = true;  // todo 下面
            //    var state = Animancer.Play(animationClip, 0f, FadeMode.NormalizedFromStart);
            //    state.Events.OnEnd = () =>
            //    {
            //        state.IsPlaying = false;
            //        state.Events.OnEnd = null;
            //        SGF.Debuger.Log($"技能倒计时 播放动画[结束] AimationName={AimationName}");
            //        if (AimationName != "SkillAnim_Default")
            //        {
            //            PlayAnimation("SkillAnim_Default");
            //        }
            //    };
            //}
        }

        private void OnSceneMapLoadComplete(int arg0)
        {
            PlayAnimation("SkillAnim_Default");

            if (skillUnit == null)
            {
                return;
            }

            if (skillUnit != null && !skillUnit.HasSkill)
            {
                return;
            }

            string mapSubType = GameManager.Instance.GetMapSubType();
            if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
            {
                //RefreshActive();

                bool isShow = BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
                if (gameObject.activeSelf != isShow)
                {
                    SetActive(isShow);
                }
            }
            else
            {
                if (SkillBtnPos == 7 && sysItem != null && !sysItem.IsOpen)
                {
                    sysItem.IsRefreshSystemNodeActive = true;
                    sysItem.MarkDirty();
                }
            }
        }

        public void PlayerClickAnim()
        {
            long curTime = TimeUtils.ClientNowStampMilli;
            if ((curTime - m_LastTime) > m_PlayerClickIntervalTime)
            {
                if (SkillBtnPos == 1)
                {
                    SpecialAnim.PlayAnimByKey("fade1", SpecialAnimFinishCB);
                }
                else
                {
                    PlayAnimation("SkillAnim_Click");
                }
                m_LastTime = curTime;
            }
            //SGF.Debuger.Log($"技能动画   差={curTime - m_LastTime}，，是否播放={(curTime - m_LastTime) > m_PlayerClickIntervalTime}");
        }

        private void SpecialAnimFinishCB()
        {
            SpecialAnimCanvasGroup.alpha = 0f;
        }

        #endregion

        // 主角死亡回调
        private void OnMainPlayerDie(bool isDie)
        {
            m_IsKeyDown = false;
            OnPointerUp(null);
        }

        private void OnPlayTimelineEventCB(bool arg0)
        {
            SendCancelSkill();
        }

        private void onSceneBeginChangeCB(object arg0)
        {
            SendCancelSkill();
        }

        public virtual void OnXinSGUIShowCB(object arg0)
        {
            if (skillUnit == null)
            {
                return;
            }

            if (skillUnit != null && !skillUnit.HasSkill)
            {
                return;
            }
            bool isShow = BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
            if (gameObject.activeSelf != isShow)
            {
                SetActive(isShow);
                RefreshActive();
                if (isShow)
                {
                    if (SkillBtnPos == 1 || SkillBtnPos == 3 || SkillBtnPos == 4)
                    {
                        return;
                    }
                    PlayAnimation("SkillAnim_Go");
                }
            }
        }

        // 缓存的 InputVKey 收到的 参数 arg, 可以根据 arg 判断按钮点击的类型
        private float _arg = -999;

        // 虚拟按钮输入回调
        protected virtual void InputVKey(int vkey, float arg)
        {
            if (gameObject == null || !gameObject.activeSelf)
            {
                return;
            }
            if (m_KeyCode == vkey)
            {
                _arg = arg;
                if (arg == 1)
                {
                    // 按下
                    m_IsKeyDown = true;
                    //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 按键----按下 skillid={skillUnit.CurSkillInfo.cfg.ID}");
                    OnPointerDown(null);
                }
                else if (arg == 0)
                {
                    // 抬起
                    //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 按键----抬起 skillid={skillUnit.CurSkillInfo.cfg.ID}");
                    m_IsKeyDown = false;
                    SetState(E_SkillBtnState.Pressed, false);
                    OnPointerUp(null);
                }
                else if (arg == -1)
                {
                    // 取消使用技能
                    m_IsKeyDown = false;
                    SendCancelSkill();
                }
                else if (arg == 2)
                {
                    // 自动战斗触发的，直接【按下】后【抬起】
                    m_IsKeyDown = true;
                    OnPointerDown(null);
                    m_IsKeyDown = false;
                    OnPointerUp(null);
                }
                else
                {
                    //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 按键----抬起 skillid={skillUnit.CurSkillInfo.cfg.ID}");
                    m_IsKeyDown = false;
                    SetState(E_SkillBtnState.Pressed, false);
                    OnPointerUp(null);
                }



                _arg = -999;
            }
        }

        #region 技能逻辑

        /// <summary>
        /// 技能槽按钮create
        /// </summary>
        /// <param name="skillPos">技能位</param>
        /// <param name="skillContainer">技能槽的信息</param>
        /// <param name="entityCtrlBase">实体逻辑</param>
        public bool Create(int skillPos, SkillContainer skillContainer, EntityCtrlBase entityCtrlBase, SkillPosSetDataCell _skillPosSetDataCell)
        {
            ReleaseAction();
            //if (skillContainer == null)
            //{
            //    return false;
            //}
            skillPosSetDataCell = _skillPosSetDataCell;
            if (skillPosSetDataCell == null)
            {
                return false;
            }
            M_EntityCtrlBase = entityCtrlBase;
            transform.name = $"SkillBtnSeat_{skillPos}";
            GetComponent<RectTransform>().anchoredPosition3D = new UnityEngine.Vector3(skillPosSetDataCell.SkillPos[0], skillPosSetDataCell.SkillPos[1], skillPosSetDataCell.SkillPos[2]);

            scaler = GameInput.GetCanvasScaler();
            m_SkillCanceller = GameInput.GetCancelBtn();

            skillUnit = skillContainer;
            SkillBtnPos = skillPos;

            isAimable = false;
            isCanCancel = false;

            InitSystemItemType();
            InitSkillData();
            InitJobSkillConsume();

            bool isShow = true;
            string mapSubType = GameManager.Instance.GetMapSubType();
            if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
            {
                isShow = BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
            }
            SetActive(isShow);

            return true;
        }

        private void InitSystemItemType()
        {
            sysItem = GetComponent<Service.SystemOpen.SystemItem>();
            sysItem.IsRefreshSystemNodeActive = false;



            // 不确定 曲的 技能按钮 是否会复用. 如果复用,那刷技能 就是 多次执行 create.
            // 所以 此处 action 是先 移除注册 再添加注册
            sysItem.ActionOnRefreshSystemNodeActive -= ActionOnRefreshSystemItemActive;
            sysItem.ActionOnRefreshSystemNodeActive += ActionOnRefreshSystemItemActive;
            switch (SkillBtnPos)
            {
                case 1:
                    {
                        CreateDodgeCDFinishAnim();
                    }
                    break;
                case 2:
                    {

                    }
                    break;
                case 3:
                    {
                        sysItem.InitWithType(SystemOpenType.SkillOne);
                    }
                    break;
                case 4:
                    {
                        sysItem.InitWithType(SystemOpenType.SkillTwo);
                    }
                    break;
                case 5:
                    {
                        sysItem.InitWithType(SystemOpenType.SkillThree);
                    }
                    break;
                case 6:
                    {
                        sysItem.InitWithType(SystemOpenType.SkillFour);
                    }
                    break;
                case 7:
                    {
                        string mapSubType = GameManager.Instance.GetMapSubType();
                        bool isRefreshSystemNodeActive = true;
                        if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                        {
                            isRefreshSystemNodeActive = false;
                        }
                        sysItem.IsRefreshSystemNodeActive = isRefreshSystemNodeActive;
                        //sysItem.IsRefreshSystemNodeActive = true;  
                        sysItem.InitWithType(SystemOpenType.Ult);

                    }
                    break;

                default: break;
            }

            RefreshActive();

            int unlockRoleLevel = sysItem.GetUnlockRoleLevel();
            M_LockLv.text = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_Unlock_Lv"), unlockRoleLevel);
        }

        private void InitJobSkillConsume()
        {
            bool isShowJobSpectralUI = false;
            uint job = M_EntityCtrlBase.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            if (job != 0)
            {
                if (skillUnit != null && skillUnit.CurShowSkillInfo != null)
                {
                    int num = skillUnit.CurShowSkillInfo.GetPlayerSkillConsume();
                    if (num > 0)
                    {
                        M_SkillJobSpectralText.text = $"{num}";
                        isShowJobSpectralUI = true;
                    }
                }

                int shi = Mathf.FloorToInt(job / 10);
                if (isShowJobSpectralUI)
                {
                    isShowJobSpectralUI = shi == 3;
                }

                if (isShowJobSpectralUI)
                {
                    string path = $"SkillBtnConsumeBG_{shi}";
                    loadSkillConsumeBg = (Sprite sp) =>
                    {
                        if (sp != null && M_SkillJobSpectralUI != null)
                        {
                            M_SkillJobSpectralUI.sprite = sp;
                            M_SkillJobSpectralUI.gameObject.SetActive(isShowJobSpectralUI);
                        }
                        loadSkillConsumeBg = null;
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill, path, loadSkillConsumeBg);
                }
            }
            if (!isShowJobSpectralUI)
            {
                M_SkillJobSpectralUI.gameObject.SetActive(isShowJobSpectralUI);
            }
        }

        private void ActionOnRefreshSystemItemActive(bool isSystemItemActive)
        {
            RefreshActive();
        }

        private void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 刷新技能槽 的 active
        /// </summary>
        private void RefreshActive()
        {
            m_IsCanClick = sysItem.IsOpen;

            /// 目前看 有以下几种情况会控制 技能槽的显隐:
            /// 1.如果 技能槽 未填充技能, 技能槽 关闭;
            /// 2.如果 技能槽 技能未达到 系统开放等级， 技能槽关闭;
            /// 3.如果 玩家 身上有 这个槽位的 变身技能效果标签, 技能槽 显示;
            /// 
            /// 
            //sysItem = GetComponent<Service.SystemOpen.SystemItem>();
            // SGF.Debuger.Log($"[SkillItem] refres: {sysItem.systemType} , isOpen: {sysItem.IsOpen} , skillUnit == null : {skillUnit == null}, !skillUnit.HasSkill: {!skillUnit.HasSkill}");

            //// 技能槽 不存在技能的时候, 直接 关闭;
            if (skillUnit == null || !skillUnit.HasSkill)
            {
                // 技能没有的时候， 槽位不能关闭
                SetActive(false);
                return;
            }

            /// 如果存在技能, 判断这个技能 是否是 变身效果填充的技能.
            /// 目前高磊的 说法是 变身效果的技能如果存在, 这个槽位不管开不开起, 都需要显示
            /// 比如
            ///     新手引导的变身. 玩家一开始的技能槽是 不满足系统开放条件 并且 被 效果标签隐藏的。
            ///     但是对于 特定的技能槽，策划会配置 特定的变身技能。这个槽位也需要显示
            if (skillUnit.IsChangeSkillSlot)
            {
                sysItem.ShowTips = false;
                string mapSubType = GameManager.Instance.GetMapSubType();
                if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                {
                    m_IsCanClick = BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
                }
                else
                {
                    m_IsCanClick = true;
                }
                SetActive(m_IsCanClick);
                //return;
            }

            //// 如果不是变身技能, 就默认需要提示 tips
            //sysItem.ShowTips = true;

            //// 如果都不是变身技能槽, 那就采用 系统开放 控制这个节点
            //gameObject.SetActive(sysItem.IsOpen);

            //string mapSubType = GameManager.Instance.GetMapSubType();
            //if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
            //{
            //    m_IsCanClick = sysItem.IsOpen || BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
            //}
            //sysItem.ShowTips = false;
            M_SkillIcon.gameObject.SetActive(m_IsCanClick);
            M_Lock.SetActive(!m_IsCanClick);
        }

        private void InitSkillData()
        {
            BindSkillAction();

            if (m_SkillBgIcon == null)
            {
                m_SkillBgIcon = GameObjectUtils.EnsureComponent<Image>(gameObject);
            }

            // 在按钮创建 初始化的时候 先去 清理之前的 按钮状态
            if (skillUnit != null && skillUnit.HasShowSkill)
            {
                state = E_SkillBtnState.Active;
            }

            SetSkillBtnData();
        }

        private void CreateBlueRingIntensifyAnim(string path)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
            (GameObject go) =>
            {
                if (go == null)
                {
                    return;
                }
                var gob = GameObject.Instantiate<GameObject>(go);
                gob.transform.SetParent(transform, false);
                gob.transform.localPosition = Vector3.zero;
                //gob.transform.localScale = Vector3.one * 1.34f;
                gob.transform.SetAsFirstSibling();
                IntensifyAnim.Add(gob);
            });
        }

        private void ReleaseIntensifyAnim()
        {
            foreach (var item in IntensifyAnim)
            {
                GameObject.Destroy(item);
            }
            IntensifyAnim.Clear();
        }

        private void SetSkillBtnData()
        {
            SetCountdown("");
            IsHasSkill = false;
            m_IsNotShowCD = false;
            UnRegAllTagChangeListeners();
            ReleaseIntensifyAnim();
            if (skillUnit != null)
            {
                RegChangeListener();
                if (skillUnit.CurShowSkillInfo == null)
                {
                    return;
                }
                IsHasSkill = skillUnit.HasShowSkill;
                m_IsKeyDown = false;

                SetIcon(skillUnit.CurShowSkillInfo.GetSkillIconPath());
                if (IsHasSkill)
                {
                    SkillConfig cfg = skillUnit.CurShowSkillInfo.cfg;
                    CastMethodType castMethodType = cfg.CastMethod;
                    SkillWheelInfo curSkillCfg = (M_EntityCtrlBase.M_Curr as NPCEntityBase).skillDispatcher.SkillController.GetSkillWheelInfo(cfg.ID);
                    if (curSkillCfg != null)
                    {
                        castMethodType = curSkillCfg.CastMethod;
                    }
                    if (cfg != null)
                    {
                        // 规定：蓄力技能，不能取消
                        // 立即施法技能不能取消
                        // 只有：指示器技能的才能取消
                        isCanCancel = castMethodType == CastMethodType.WheelCast;
                        m_IsNotShowCD = cfg.IsNotShowCD;
                        // 规定：只有：指示器技能和蓄力释放 才显示技能遥感
                        isAimable = castMethodType == CastMethodType.WheelCast || castMethodType == CastMethodType.GatherWheelCast;
                        m_EnergySkillShowCD = true;

                        // 普攻没有按钮遮罩
                        M_CountdownIcon.fillAmount = 0;
                        // 设置技能按钮的描述
                        string skillIconDesc = "";
                        if (!string.IsNullOrEmpty(cfg.SkillIconDesc) && !string.IsNullOrWhiteSpace(cfg.SkillIconDesc))
                        {
                            skillIconDesc = cfg.SkillIconDesc;
                        }
                        M_SkillIconDes.text = skillIconDesc;
                        // 闪避按钮不需要背景图
                        // 绑定键盘虚拟按键
                        if (skillPosSetDataCell.KeyCode != "")
                        {
                            KeyCode keyCode;
                            if (Enum.TryParse<KeyCode>(skillPosSetDataCell.KeyCode, out keyCode))
                            {
                                m_KeyCode = (int)keyCode;
                                //SGF.Debuger.Log($"虚拟按键 keyCode={m_KeyCode},skillid={cfg.ID}");
                            }
                        }
                        else
                        {
                            m_KeyCode = -1;
                        }
                    }

                    //OnPointerUp(null);

                    // 设置默认CD
                    if (skillUnit.IsCD())
                    {
                        //bool isInactive = true;
                        if (state == E_SkillBtnState.Pressed)
                        {
                            //if (skillUnit.HasSkill && skillUnit.CurShowSkillInfo != null && !skillUnit.CurShowSkillInfo.IsCDCloseTouch)
                            //{
                            //    isInactive = false;
                            //}
                            if (isAimable)
                            {
                                m_EnergySkillShowCD = false;
                            }
                        }
                        ////SGF.Debuger.LogWarning($"虚拟按键 keyCode={m_KeyCode},skillid={cfg.ID}, 有CD");
                        //if (isInactive)
                        //{
                        //    //SGF.Debuger.LogWarning($"虚拟按键 keyCode={m_KeyCode},skillid={cfg.ID}, 但是拦截按钮了点击");
                        //    //SetState(E_SkillBtnState.Inactive, false);
                        //}
                        SetCooldown(skillUnit.LeastCD / 1000f, false, false);
                    }
                    else
                    {
                        bool isInactive = true;
                        if (state == E_SkillBtnState.Pressed)
                        {
                            if (skillUnit.HasShowSkill && skillUnit.CurShowSkillInfo != null && !skillUnit.CurShowSkillInfo.IsCDCloseTouch)
                            {
                                isInactive = false;
                            }
                        }
                        if (isInactive)
                        {
                            SetState(E_SkillBtnState.Active, false);
                        }
                        SetCooldown(0, false, m_IsCding);
                    }

                    // 强化显示
                    InitCreateSlotSpShow();
                }
                else
                {
                    if (state != E_SkillBtnState.Pressed)
                    {
                        SGF.Debuger.Log($"{LOG_TAG} SetSkillBtnData:  SetState  None 1");

                        // 按钮置灰
                        SetState(E_SkillBtnState.None, false);
                    }
                }
            }
            else
            {
                if (state != E_SkillBtnState.Pressed)
                {
                    SGF.Debuger.Log($"{LOG_TAG} SetSkillBtnData:  SetState  None 2");

                    // 按钮置灰
                    SetState(E_SkillBtnState.None, false);
                }
            }
            this.UpdateColor();

            SetBgIcon();
            UpdateBound();
            transform.SetSiblingIndex(skillPosSetDataCell.GetSort());
        }

        private void BindSkillAction()
        {
            if (skillUnit != null)
            {
                skillUnit.ActionRefreshSkill += OnActionRefreshSkillOnUsed;
                skillUnit.ActionOnStartCD += OnActionOnStartCD;
                skillUnit.ActionOnEndCD += OnActionOnEndCD;
                skillUnit.ActionOnRefreshCD += OnActionOnRefreshCD;
                skillUnit.ActionOnRefreshWaitInterval += OnActionOnRefreshWaitInterval;
            }


        }

        private void ReleaseAction()
        {
            if (skillUnit != null)
            {
                skillUnit.ActionRefreshSkill -= OnActionRefreshSkillOnUsed;
                skillUnit.ActionOnStartCD -= OnActionOnStartCD;
                skillUnit.ActionOnEndCD -= OnActionOnEndCD;
                skillUnit.ActionOnRefreshCD -= OnActionOnRefreshCD;
                skillUnit.ActionOnRefreshWaitInterval -= OnActionOnRefreshWaitInterval;
            }


        }

        private void InitCreateSlotSpShow()
        {
            List<EnumSkillSlotSpShow> skillSlotSpShow = skillUnit.CurShowSkillInfo.GetSkillSlotSpShow();
            foreach (var item in skillSlotSpShow)
            {
                switch (item)
                {
                    case EnumSkillSlotSpShow.BlueRing:
                        CreateBlueRingIntensifyAnim("UI/StarWorld/Prefab/Fx_UI_SkillBtn_QH");
                        break;
                    default:
                        break;
                }
            }
        }

        private void CreateDodgeCDFinishAnim()
        {
            if (DodgeCDFinishAnim == null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/FX_UI_ShanBi_CD",
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }
                    if (DodgeCDFinishAnim != null)
                    {
                        return;
                    }
                    var gob = GameObject.Instantiate<GameObject>(go);
                    gob.transform.SetParent(transform, false);
                    gob.transform.localPosition = Vector3.zero;
                    gob.transform.localScale = Vector3.one;
                    gob.SetActive(false);
                    DodgeCDFinishAnim = gob;
                });
            }
        }

        public void SetSiblingIndex()
        {
            transform.SetSiblingIndex(skillPosSetDataCell.GetSort());   // 每个技能都有个排序ID

            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            float _scale = skillPosSetDataCell.GetScale() / 100.0f;
            rectTransform.SetLocalScale(new Vector3(_scale, _scale, _scale));

            M_SkillIcon.SetNativeSize();
            m_SkillBgIcon.SetNativeSize();
        }

        private void SetIcon(string path)
        {
            loadSkillIcon = (Sprite sp) =>
            {
                SetIcon(sp);
            };
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill, path, loadSkillIcon);
        }

        private void SetIcon(Sprite image)
        {
            if (M_SkillIcon != null)
            {
                M_SkillIcon.sprite = image;
                //M_SkillIcon.gameObject.SetActive(true);
                M_SkillIcon.SetNativeSize();
                //RectTransform rectTransform = transform.GetComponent<RectTransform>();
                //float _scale = skillPosSetDataCell.GetScale() / 100.0f;
                //rectTransform.SetLocalScale(new Vector3(_scale, _scale, _scale));
                // 修改背景的大小
                //btnBg.SetNativeSize();
                //skillBtn.btnBg.SetLocalScale(new UnityEngine.Vector3(_scale, _scale, _scale));

                RectTransform rectTransform = M_SkillIcon.GetComponent<RectTransform>();
                RectTransform countdownRectTransform = M_CountdownIcon.GetComponent<RectTransform>();
                RectTransform waitIntervalCountdownRectTransform = M_WaitIntervalCountDown.GetComponent<RectTransform>();

                if (SkillBtnPos == 1)
                {
                    M_CountdownIcon.sprite = image;
                    M_CountdownIcon.SetNativeSize();
                    M_CountdownIcon.transform.SetLocalScale(Vector3.one);
                    M_CountdownIcon.fillMethod = Image.FillMethod.Vertical;
                }
                else
                {
                    M_CountdownIcon.fillMethod = Image.FillMethod.Radial360;
                    countdownRectTransform.SetWidth(rectTransform.rect.width);
                    countdownRectTransform.SetHeight(rectTransform.rect.height);

                    waitIntervalCountdownRectTransform.SetWidth(rectTransform.rect.width);
                    waitIntervalCountdownRectTransform.SetWidth(rectTransform.rect.height);
                }
            }
        }

        private void SetBgIcon()
        {
            string path = skillPosSetDataCell.SkillBg;
            loadSkillBgIcon = (Sprite sp) =>
            {
                SetBgIcon(sp);
            };
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill, path, loadSkillBgIcon);
        }

        private void SetBgIcon(Sprite image)
        {
            if (m_SkillBgIcon != null)
            {
                m_SkillBgIcon.sprite = image;
                m_SkillBgIcon.SetNativeSize();

                //RectTransform rectTransform = transform.GetComponent<RectTransform>();

                //RectTransform countdownRectTransform = M_CountdownIcon.GetComponent<RectTransform>();
                //countdownRectTransform.SetWidth(rectTransform.rect.width);
                //countdownRectTransform.SetHeight(rectTransform.rect.height);
            }
        }

        private void SetCountdown(string t)
        {
            if (M_Countdown != null && !m_IsNotShowCD && m_EnergySkillShowCD)
            {
                M_Countdown.text = t;
            }
        }

        private void SetCooldown(float sec, bool isChanageColor, bool isPlayAnim)
        {
            m_SkillCooldown = sec;
            if (skillUnit.CurShowSkillInfo != null && skillUnit.HasShowSkill)
            {
                m_SkillCooldownSum = (float)skillUnit.CurShowSkillInfo.GetRellyCD() / 1000;
            }
            m_IsCding = m_SkillCooldown > 0f;
            if (m_SkillCooldown > 0f)
            {
                //SetText(((int)cooldown % 60).ToString());
                SetCountdown(math.ceil(m_SkillCooldown).ToString());
            }
            else
            {
                SetCountdown("");
                SetCountdownIcon(isPlayAnim);
            }

            if (isChanageColor)
            {
                this.UpdateColor();
            }

            //// 如果是伙伴&&并且是主角的召唤物
            //if (M_EntityCtrlBase != null && M_EntityCtrlBase.M_Curr.EntityType == E_EntityType.Partner && M_EntityCtrlBase.M_Curr.M_IsMainPlayerSummon)
            //{
            //    GlobalEvent.OnPartnerSkillCDUpdate.Invoke(M_EntityCtrlBase.M_Curr.ConfigIndex, m_SkillCooldown, m_SkillCooldownSum);
            //}
        }

        private void SetWaitIntervalCooldown(float sec)
        {
            m_SkillWaitIntervalCooldown = sec;
            M_MultistageCountdownIcon.fillAmount = m_SkillWaitIntervalCooldown / m_SkillWaitIntervalCooldownSum;
            if (m_SkillWaitIntervalCooldown <= 0)
            {
                M_CountdownIcon.gameObject.SetActive(true);
                M_Countdown.gameObject.SetActive(true);
            }
        }

        private void SetCountdownIcon(bool isPlayAnim)
        {
            if (M_CountdownIcon != null)
            {
                if (m_SkillCooldown > 0f)
                {
                    if (!m_IsNotShowCD && m_EnergySkillShowCD)
                    {
                        M_CountdownIcon.fillAmount = m_SkillCooldown / m_SkillCooldownSum;
                    }
                }
                else
                {
                    M_CountdownIcon.fillAmount = 0;
                    //SGF.Debuger.LogError($"技能倒计时 m_SkillCooldown={m_SkillCooldown},m_IsCding={m_IsCding}");

                    if (isPlayAnim && !m_IsNotShowCD && m_EnergySkillShowCD)
                    {
                        // CD转好后的，提示提示动效
                        if (SkillBtnPos == 1)
                        {
                            if (DodgeCDFinishAnim != null)
                            {
                                DodgeCDFinishAnim.SetActive(false);
                                DodgeCDFinishAnim.SetActive(true);
                            }
                        }
                        else
                        {
                            PlayAnimation("SkillAnim_Cd");
                        }
                    }
                    if (m_IsCding)
                    {
                        m_IsCding = false;
                    }
                }
            }
        }

        // 发送取消技能
        private void SendCancelSkill()
        {
            if (!m_IsKeyDown)
            {
                E_SkillBtnState enterState = state;
                fingerId = -99;
                UpdateIndicatorShow(false);
                SetState(E_SkillBtnState.Active);

                if (!isCanCancel)
                {
                    onPointerUp?.Invoke(directionXZ, enterState, _arg);
                }
                else
                {
                    onCancelSkill?.Invoke();
                }
                BtnDirChange?.Invoke(Vector3.zero);
            }
        }

        #region 技能委托逻辑

        #region 蓄力技能回调
        // 响应技能蓄力开始
        public void OnActionOnEnergyStart(SkillEntity skillEntity)
        {
            m_CurSkillRumTime = skillEntity.RuntimeID;
        }

        // 响应技能蓄力结束 
        public void OnSkillEnergyEndAction(SkillEntity skillEntity)
        {
            if (skillEntity.RuntimeID != m_CurSkillRumTime)
            {
                return;
            }
            isSendEnergyAction = false;
            m_IsKeyDown = false;
            OnPointerUp(null);
            isSendEnergyAction = true;
        }

        // 响应技能被打断了 
        public void OnActionOnEndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType)
        {
            if (skillEntity.RuntimeID != m_CurSkillRumTime)
            {
                return;
            }
            switch (e_SkillExitType)
            {
                case E_SkillExitType.Default:
                case E_SkillExitType.ServerDefault:
                    break;
                case E_SkillExitType.ClientBreakSkill:
                case E_SkillExitType.ServerBreakSkill:
                case E_SkillExitType.FailedSetMainSkill:
                case E_SkillExitType.Reset:
                    {
                        isSendEnergyAction = false;
                        m_IsKeyDown = false;
                        OnPointerUp(null);
                        isSendEnergyAction = true;
                    }
                    break;
                default:
                    break;
            }
            //switch (e_SkillExitType)
            //{
            //    //1.如果被服务器的协议打断，那肯定要去还原按钮状态
            //    case E_ClientSkillEndType.ServerEnd:
            //        {
            //            Debug.Log($"技能  响应  技能被打断了 {clientSkillEndType}");
            //            OnPointerUp(null);
            //        }
            //        break;
            //    //2.正常的技能阶段结束，肯定也是要还原按钮状态的
            //    case E_ClientSkillEndType.ClientNormalEnd:
            //        {
            //            Debug.Log($"技能  响应  技能被打断了 {clientSkillEndType}");
            //            OnPointerUp(null);
            //        }
            //        break;
            //    //3.客户度技能之间的相互打断，比如第一个技能的拖尾状态，使用第二个技能，这个时候按钮就不适合还原
            //    //  因为按钮可能还需要监听接下来的按钮抬起事件
            //    case E_ClientSkillEndType.UseSkill:
            //        {
            //            Debug.Log($"技能  响应  技能被打断了 {clientSkillEndType}，但是我就不干啥事");
            //        }
            //        break;

            //    default:
            //        {
            //            Debug.Log($"技能  响应  技能被打断了 {clientSkillEndType}，但是我就不干啥事");
            //        }
            //        break;
            //}


        }
        #endregion

        #region 技能CD回调

        private bool m_IsCding = false; // 是否在CD中

        private void OnActionOnStartCD(float cd, bool IsCDCloseTouch)
        {
            //SGF.Debuger.LogWarning($"技能倒计时 OnActionOnStartCD cd={cd},IsCDCloseTouch={IsCDCloseTouch}");

            if (cd > 0f)
            {
                if (IsCDCloseTouch)
                {
                    UpdateIndicatorShow(false, false);
                }
                SetCooldown(cd / 1000, true, false);
            }
        }

        // 刷新CD不会有0的时候
        // 如果有0就会发OnActionOnEndCD
        private void OnActionOnRefreshCD(float cd)
        {
            if (state == E_SkillBtnState.Pressed)
            {
                if (isAimable)
                {
                    m_EnergySkillShowCD = false;
                }
                //SGF.Debuger.LogWarning($"虚拟按键 keyCode={m_KeyCode}蓄力技能 不显示CD");
            }
            //else
            //{
            //    SGF.Debuger.Log($"虚拟按键 keyCode={m_KeyCode} 蓄力技能");
            //}
            SetCooldown(cd / 1000, true, false);
        }

        private void OnActionOnEndCD()
        {
            //SGF.Debuger.LogError($"技能倒计时 OnActionOnEndCD");

            SetCooldown(0f, false, m_IsCding);
            if (state != E_SkillBtnState.None && state != E_SkillBtnState.Pressed)
            {
                SetState(E_SkillBtnState.Active);
            }
            //SetActiveState(true);
        }
        #endregion

        private void OnActionRefreshSkillOnUsed(SkillInfo CurSkillInfo, SkillInfo CurShowSkillInfo)
        {
            /// Fix issue:
            ///     修复 策划 gm完成所有任务开放 技能按钮后， 技能按钮由于 属性未及时同步, 导致槽内 可现实技能没有而按钮不显示的bug
            RefreshActive();

            if (CurShowSkillInfo == null)
            {
                SGF.Debuger.Log($"{LOG_TAG} OnActionRefreshSkillOnUsed() 显示技能都没了");
                return;
            }

            SetSkillBtnData();

            if (CurSkillInfo == null)
            {
                // 按钮置灰
                //SetActiveState(false);
                if (skillPosSetDataCell != null)
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} OnActionRefreshSkillOnUsed() 当前技能没有了 skillPos={skillPosSetDataCell.GetID()},type={skillPosSetDataCell.GetSort()},sort={skillPosSetDataCell.GetSkillType()}");
                }
                else
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} OnActionRefreshSkillOnUsed() 当前技能没有了");
                }
                if (state != E_SkillBtnState.Pressed)
                {
                    SGF.Debuger.Log($"{LOG_TAG} OnActionRefreshSkillOnUsed: curSkillInfo ==null");

                    SetState(E_SkillBtnState.None);
                }
            }


#if UNITY_EDITOR
            //SetCountdown($"id_{skillUnit.CurSkillId}");
#endif
        }

        private void OnActionOnRefreshWaitInterval(SkillContainer container, bool isStar, int totalTime, int curTime)
        {
            if (skillUnit != null && skillUnit.CurShowSkillInfo != null && skillUnit.CurShowSkillInfo.cfg != null && skillUnit.CurShowSkillInfo.cfg.IsNotShowCD)
            {
                return;
            }
            if (isStar)
            {
                M_CountdownIcon.gameObject.SetActive(false);
                M_Countdown.gameObject.SetActive(false);
                m_SkillWaitIntervalCooldownSum = (float)totalTime / 1000;
                SetWaitIntervalCooldown((float)(totalTime - curTime) / 1000);
            }
            else
            {
                SetWaitIntervalCooldown(0);
            }
            M_MultistageCountdownIcon.gameObject.SetActive(isStar);
        }

        #region 注册监听的UI变化接口

        private void RegChangeListener()
        {
            if (M_EntityCtrlBase == null)
            {
                return;
            }
            if (M_EntityCtrlBase.Data == null)
            {
                return;
            }
            if (skillUnit.CurShowSkillInfo == null || !skillUnit.HasShowSkill)
            {
                return;
            }
            M_EntityCtrlBase.Data.RegChangeListener(TriggleEventUtils.FromatComplexUITriggleKey(skillUnit.CurShowSkillInfo.SkillComplexConfig), OnRegChangeListener, this.GetHashCode().ToString());
        }

        private void UnRegAllTagChangeListeners()
        {
            if (M_EntityCtrlBase == null)
            {
                return;
            }
            if (M_EntityCtrlBase.Data == null)
            {
                return;
            }
            M_EntityCtrlBase.Data.UnRegAllTagChangeListeners(this.GetHashCode().ToString());
        }

        private void OnRegChangeListener(object v)
        {
            if (v == null)
            {
                return;
            }
            TriggerTypeEffectData data = (TriggerTypeEffectData)v;
            bool value = (bool)data.Value;
            switch (data.Event)
            {
                case GameConfig.HIDDEN_UI_EVENT:
                    {
                        // 表现问磊子
                        //..先不管
                        SGF.Debuger.Log($"{LOG_TAG} 技能按钮 OnRegChangeListener() skillid={skillUnit.CurShowSkillInfo.cfg.ID},state={state},val={data.Value}");

                        if (value)
                        {
                            //SetCountdown(GameConfig.LocalStr["Lock"]);
                            SetCountdown(LanguageManager.Instance.GetLanguageByKey("Lock"));
                        }
                        else
                        {
                            SetCountdown("");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion


        #endregion

        #endregion

    }

}