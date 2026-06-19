using ProtoMsg;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game.Entity.VitalSigns;
using StarProject.OffLine;
using StarProject.Service.Business;
using StarProject.Service.Input;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProject.UI.SkillBtn;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


namespace StarProject.Game
{
    /// <summary>
    /// GameInput 挂载在 MobileController
    /// 1,点击触碰输入
    /// 2，手柄轮盘输入
    /// </summary>
    [XLua.LuaCallCSharp]
    public class GameInput : MonoBehaviour
    {
        // 按钮状态
        public static DictionaryEx<string, bool> m_MapKeyTouchState = new();

        /// <summary>
        /// 为调用者抛出虚拟按键事件！
        /// </summary>
        //public static Action<Vector3> OnVkeyVecNormal;

        private static GameInput m_Instance = null;
        public static GameInput Instance
        {
            get { return m_Instance; }
            set { m_Instance = value; }
        }

        public static Action GameInputFinishAction = null;

        private static UniversalButton m_AnalogStick;  // 遥感
        /// <summary> 面板技能取消按钮 </summary>
        public SkillCanceller skillCanceller;
        /// <summary> 创建的技能按钮字典（技能位ID，技能节点） </summary>
        public static DictionaryEx<int, UniversalButton> SkillUnitDic = new();

        //药品
        //public JButton MedicineJB2;
        public JButton MedicineJB;
        public Image MedicineItemIcon;
        public Text NumText;
        public GameObject CountDownObj;
        public Image countDownImg;
        public Text CountDownText;
        long leftTime = 0;
        long totalTime = 0;
        ulong entityID = 0;

        public Transform LeftGroup;
        public Transform RightGroup;

        public CanvasGroup mBattle;
        public CanvasGroup mInter;
        public JButton mStopBtn;
        public Text mStopText;
        private long m_InterID;
        private int Type;
        
        [SerializeField]
        private GameObject m_MineTrackBtn;
        public bool IsInter
        {
            get { return Type != 0 && m_InterID != 0; }
        }
        #region 动画

        public List<AnimationClip> Clips = new();
        public Animancer.AnimancerComponent Animancer;

        private Dictionary<string, AnimationClip> AnimationClips = new();

        #endregion
        //     //-------------------------------------------------------------------
        //     /// <summary>
        //     /// 用来控制移动的轮盘
        //     /// </summary>
        //     private EasyJoystick m_Joystick;

        //     /// <summary>
        //     /// 用来加速，或者使用技能的按钮
        //     /// </summary>
        //     private EasyButton m_Button;


        /// <summary>
        /// 初始化，用来在当前场景添加GameInput对象
        /// Debug,开启面板
        /// </summary>
        public static void Create()
        {
            if (Instance != null)
            {
                SGF.Debuger.LogError("GameInput 不能重复初始化！");
                return;
            }

            //实例化GameInput的Prefab，里面预置了EasyJoystick脚本！
            //因为EasyJoystick有一些参数，在Prefab里比较容易配置
            //GameObject prefab = Resources.Load<GameObject>("GameInput");
            //GameObject go = GameObject.Instantiate(prefab);
            //GameObject go = ResourceManager.LoadModulePrefab("MobileController");
            //GameObject go = StarProject.Service.Resource.ResourceManager.Instance.LoadGameObject(UIDef.MobileController);
            //if (go == null)
            //{
            //    SGF.Debuger.LogError($"[GameInput] Create() prefab={UIDef.MobileController} err!!!");
            //    return;
            //}
            //Instance = GameObjectUtils.EnsureComponent<GameInput>(go);

            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(UIDef.MobileController,
            (GameObject go) =>
            {
                if (go == null)
                {
                    return;
                }
                var gob = GameObject.Instantiate<GameObject>(go);
                if (gob != null)
                {
                    Instance = GameObjectUtils.EnsureComponent<GameInput>(gob);
                    GameInputFinishAction?.Invoke();
                    GameInputFinishAction = null;
                }
            });

            //SGF.Debuger.Log("顺序   加载按钮");

            //Canvas canvas = go.GetComponent<Canvas>();
            //canvas.renderMode = RenderMode.ScreenSpaceCamera;
            //canvas.worldCamera = StarProject.Service.Cam.CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.UICam).Camera;
            //canvas.planeDistance = 10;
            //canvas.sortingOrder = 0;

            //GameObject debugCanvasLog = go.transform.Find("Canvas Scale H/ConsoleLog").gameObject;
            //debugCanvasLog.SetActive(isDebug);
            //调用时并不知道，静态全局方法（脱离类的实例），是否已经实例化
            //2随后执行后续方法，即使是后续执行，但mono并不确定你具有Gob组件
            //SGF.Debuger.LogError("11Create");
            //不被mono驱动是不会生效的，unity并不驱动，unity不确定你的组件
            //DontDestroyOnLoad(m_Instance);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            //SGF.Debuger.LogError("11Awake");
            //1加载出来优先调用,
            //加载后都应该在临时Scene中，所有的module，且不论切换Scene
            m_AnalogStick = GameObjectUtils.FindComponent<AnalogStick>(gameObject, "Cut4Cam90/AssemblyRatio2/Left/AnalogStick");
            // 清空技能按钮字典
            SkillUnitDic.Clear();

            InitAnim();
        }

        public static CanvasScaler GetCanvasScaler()
        {
            return m_Instance.GetComponent<CanvasScaler>();
        }

        public static void AddSkillBtn(int skillPosID, UniversalButton skillBtn)
        {
            if (!SkillUnitDic.ContainsKey(skillPosID))
            {
                SkillUnitDic.Add(skillPosID, skillBtn);
                skillBtn.transform.SetParent(Instance.mBattle.transform, false);
            }
        }

        public static void DelSkillBtn(int skillPosID)
        {
            if (SkillUnitDic.ContainsKey(skillPosID))
            {
                SkillUnitDic[skillPosID].Release();
                GameObject.Destroy(SkillUnitDic[skillPosID].gameObject);
                SkillUnitDic.Remove(skillPosID);
            }
        }

        public static UniversalButton GetSkillBtn(int skillPosID)
        {
            if (!SkillUnitDic.ContainsKey(skillPosID))
            {
                return null;
            }
            return SkillUnitDic[skillPosID];
        }

        public static SkillCanceller GetCancelBtn()
        {
            return m_Instance.skillCanceller;
        }

        public static void ResetSkill()
        {
            foreach (var item in SkillUnitDic)
            {
                if (item.Value != null)
                {
                    item.Value.ResetSkillData();
                }
            }
        }

        /// <summary>
        /// 释放当前创建的GameInput对象
        /// </summary>
        public static void Release()
        {
            m_MapKeyTouchState.Clear();
            if (Instance != null)
            {
                GameObject.Destroy(Instance.gameObject);
                Instance = null;
            }

            foreach (var item in SkillUnitDic)
            {
                if (item.Value != null)
                {
                    item.Value.Release();
                    GameObject.Destroy(item.Value);
                }
            }
            if (m_AnalogStick != null)
            {
                m_AnalogStick.BtnDirChange -= On_JoystickMove;
                m_AnalogStick.Release();
            }

            // 清空技能按钮字典
            SkillUnitDic.Clear();
            m_LastAnimName = string.Empty;
        }


        void Start()
        {
            //         m_Joystick = this.GetComponentInChildren<EasyJoystick>();
            //         m_Button = this.GetComponentInChildren<EasyButton>();

            //         if (m_Joystick == null || m_Button == null)
            //         {
            //             this.LogError("Start() m_Joystick == null || m_Button == null!");
            //         }

            Game.Player.PlayerCtrlGroup mainPlayerCtrl = (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            // MedicineJB2.OnLongPressDown = (go) =>
            // {
            //     MedicineJB2.CancleLongPress();
            //     ModuleManager.Instance.SendMessage(ModuleDef.Name.MedicineModule, "OnOpenMedicine", new object[] { mainPlayerCtrl });
            //     //UIManager.Instance.M_Canvas.renderMode = 0;//渲染器不要直接修改模式，好像也没啥办法，；camera，当时是做了先overlay，然后还原之前的camera；那是因为camera制作方案导致的问题；现在都是overlay不用切换过去也不用换回来，over-over-over，所以一直是支持的了
            //     //UIManager.Instance.M_Canvas.sortingOrder = /*2*/1000;//再次使用的时候注意这个值还对不对了我调整了层级，为了拖拽，还原功能在lua里面
            // };
            // MedicineJB2.OnClick = (go) =>
            // {
            //     Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_drug_03);
            // };

            MedicineJB.OnLongPressDown = (go) =>
            {
                MedicineJB.CancleLongPress();
                NPCEntityBase entityBase = (NPCEntityBase)mainPlayerCtrl.M_Curr;
                ModuleManager.Instance.SendMessage(ModuleDef.Name.MedicineModule, "OnOpenMedicine", new object[] { mainPlayerCtrl });
                SoundManager.Instance.PlayEventName("UI_Click_On_Sound");
                //UIManager.Instance.M_Canvas.renderMode = 0;//不应该设置，默认支持拉现在
                //UIManager.Instance.M_Canvas.sortingOrder = /*2*/1000;//最高
            };
            MedicineJB.OnClick = (go) =>
            {
                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_drug_03);
            };

            RefreshMedicineBtn();
            //事件
            string PanelName = "";
            var pod = gameObject.GetComponent<PanelOffLineData>();
            if (pod != null)
            {
                PanelName = pod.SPECIALNAME;
            }

            if (!string.IsNullOrEmpty(PanelName))
            {
                GlobalEvent.OnOpenUI.Invoke(PanelName);
            }
            else
            {
                string realName = gameObject.name;
                var t = name.Split("/");
                if (t != null && t.Length > 0)
                {
                    realName = t[t.Length - 1];
                }
                GlobalEvent.OnOpenUI.Invoke(realName);
            }
            Type = 0;
            m_InterID = 0;
            mBattle.alpha = 1;
            mBattle.interactable = true;
            mBattle.blocksRaycasts = true;
            mInter.alpha = 0;
            mInter.interactable = false;
            mInter.blocksRaycasts = false;
            LeftGroup.gameObject.SetActive(true);
            GlobalEvent.OnStopIner.AddListener(OnStopHandler);
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
            OnSceneMapLoadComplete(0);
            mStopBtn.OnClick += OnClickStopBtnHandler;
        }

        private void OnSceneMapLoadComplete(int arg0)
        {
            string mapSubType = GameManager.Instance.GetMapSubType();
            bool isHide = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;

            mBattle.alpha = !IsInter && !isHide ? 1f : 0f;
            mBattle.interactable = !IsInter && !isHide;
            mBattle.blocksRaycasts = !IsInter && !isHide;

            mInter.alpha = IsInter && !isHide ? 1f : 0f;
            mInter.interactable = IsInter && !isHide;
            mInter.blocksRaycasts = IsInter && !isHide;

            if (m_AnalogStick != null)
            {
                m_AnalogStick.OnXinSGUIShowCB(null);
            }
        }

        private void OnClickStopBtnHandler(GameObject go)
        {
            if (!IsInter)
            {
                return;
            }

            if (Type == 1)
            {
                Game.Player.PlayerCtrlGroup mainPlayerCtrl = (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
                if (mainPlayerCtrl != null)
                {
                    mainPlayerCtrl.BreakInteract();
                }
                //ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule,"");
            }
            else if (Type == 2)
            {
                Game.Player.PlayerCtrlGroup mainPlayerCtrl = (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
                if (mainPlayerCtrl != null)
                {
                    mainPlayerCtrl.BreakCreate();
                }
            }
        }

        private void OnStopHandler(int type, long id, bool active)
        {
            if (active)
            {
                if (IsInter)
                {
                    StarDebug.LogError($"已经在交互 类型={Type}  ID= {m_InterID}");
                    return;
                }

                this.Type = type;
                this.m_InterID = id;
                mBattle.alpha = 0;
                mBattle.interactable = false;
                mBattle.blocksRaycasts = false;
                LeftGroup.gameObject.SetActive(false);
                mInter.alpha = 1;
                mInter.interactable = true;
                mInter.blocksRaycasts = true;

                if (Type==1)
                {
                    //mStopText.text = GameConfig.LocalStr["StopGather"];
                    mStopText.text = LanguageManager.Instance.GetLanguageByKey("StopGather");
                }
                else if (Type==2)
                {
                    //mStopText.text = GameConfig.LocalStr["StopMake"];
                    mStopText.text = LanguageManager.Instance.GetLanguageByKey("StopMake");
                }
            }
            else
            {
                this.Type = 0;
                this.m_InterID = 0;
                mBattle.alpha = 1;
                mBattle.interactable = true;
                mBattle.blocksRaycasts = true;
                mInter.alpha = 0;
                mInter.interactable = false;
                mInter.blocksRaycasts = false;
                LeftGroup.gameObject.SetActive(true);
            }
        }

        public void RefreshMedicineBtn()
        {
            Game.Player.PlayerCtrlGroup mainPlayerCtrl = (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (mainPlayerCtrl == null || mainPlayerCtrl.Data == null || mainPlayerCtrl.Data.Attrs == null)
            {
                return;
            }
            var obj = mainPlayerCtrl.Data.Attrs.GetProtoValue(StarProjectDef.AOIAttrDefine.StorageDrugs);
            var storageDrugs = (ProtoMsg.StorageDrugsMd)obj;
            bool findDrug = false;
            NumText.text = "0";
            if (storageDrugs?.Eitemid.Count > 0)
            {
                var id = storageDrugs.Eitemid[0];
                entityID = id;
                var items = BusinessManager.Instance.GetItemsByItemType(3, 3);

                foreach (var item in items)
                {
                    if (item.EntityID == id)
                    {
                        findDrug = true;
                        string path = LocalDataManager.Instance.GetItemDataCell(item.BaseID).Icon;
                        //var sp = StarProject.ResourceHelperMono.LoadSprite(path);
                        //MedicineJB.GetComponent<Image>().sprite = StarProject.Service.Resource.ResourceManager.Instance.LoadSprite(path);
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>(path, (Sprite img) =>
                        {
                            if (img == null || MedicineItemIcon == null)
                            {
                                return;
                            }
                            MedicineItemIcon.sprite = img;
                            MedicineItemIcon.color = new Color(1, 1, 1, 1);
                        });

                        //MedicineItemIcon.sprite = StarProject.Service.Resource.ResourceManager.Instance.LoadSprite(path);
                        NumText.text = item.Num.ToString();
                        MedicineJB.OnClick = (go) =>
                        {
                            if (CountDownObj.activeSelf)
                            {
                                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_skill);
                                return;
                            }
                            var hero = mainPlayerCtrl.M_Curr as HeroEntityBase;
                            var hp = mainPlayerCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
                            var sum = mainPlayerCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);
                            if (hp >= sum)
                            {
                                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_drug_02);
                                return;
                            }
                            // if (hero.M_BattleMixTag != E_ClentMainPlayerState.Battle)
                            // {
                            //     Frame.Util.ShowSystemMessage("不处于战斗状态，无法使用");
                            //     return;
                            // }
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.MedicineModule, "OnUseMedicine", new object[] { id });
                        };
                        break;
                    }
                }
            }
            else
            {
                entityID = 0;
                leftTime = -1;
                CountDownObj.SetActive(false);
            }

            if (!findDrug)
            {
                StarProject.Service.AtlasManager.AtlasManager.Instance.GetSpriteAsync(Service.AtlasManager.AtlasManager.AtlasPathHud, "Btn_Drug Add", (Sprite img) =>
                {
                    if (img == null || MedicineItemIcon == null)
                    {
                        return;
                    }
                    MedicineItemIcon.sprite = img;
                    MedicineItemIcon.color = new Color(1, 1, 1, 0.2352941f);
                });
            }
            //MedicineJB2.gameObject.SetActive(!findDrug);
            //MedicineJB.gameObject.SetActive(findDrug);
            NumText.gameObject.SetActive(findDrug);
        }

        public void ShowHideMedicine(bool show)
        {
            if(MedicineJB != null)
            {
                MedicineJB.gameObject.SetActive(show);
            }
        }

        void Update()
        {
            if (entityID == 0)
            {
                return;
            }

            var ent = BusinessManager.Instance.GetItemByItemEntityId(entityID);
            if (ent == null)
            {
                return;
            }

            leftTime = ent.CD;
            leftTime = leftTime - SGF.Time.TimeUtils.ServerNowStampMilli;
            if (totalTime < leftTime)
            {
                totalTime = leftTime;
            }
            if (leftTime > 0)
            {
                CountDownObj.SetActive(true);
                countDownImg.fillAmount = leftTime * 1.0f / totalTime;
                leftTime = (long)Mathf.Ceil(leftTime * 1.0f / 1000);
                CountDownText.text = leftTime.ToString();
            }
            else
            {
                CountDownObj.SetActive(false);
                totalTime = 0;
            }
        }


        void OnEnable()
        {
            //初始化动画的声明周期 TODO
            //初始化数据 TODO
            /* EasyJoystick.On_JoystickMove += On_JoystickMove;
               EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
               EasyButton.On_ButtonUp += On_ButtonUp;
               EasyButton.On_ButtonDown += On_ButtonDown;*/
            //事件驱动，没被帧限制发送
            if (m_AnalogStick != null)
            {
                m_AnalogStick.BtnDirChange += On_JoystickMove;
            }
        }

        void OnDisable()
        {
            /* EasyJoystick.On_JoystickMove -= On_JoystickMove;
               EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
               EasyButton.On_ButtonDown -= On_ButtonDown;
               EasyButton.On_ButtonUp -= On_ButtonUp;*/
            if (m_AnalogStick != null)
            {
                m_AnalogStick.BtnDirChange -= On_JoystickMove;
            }
        }

        void OnDestroy()
        {
            /*                 EasyJoystick.On_JoystickMove -= On_JoystickMove;
                             EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
                             EasyButton.On_ButtonDown -= On_ButtonDown;
                             EasyButton.On_ButtonUp -= On_ButtonUp;*/
            if (m_AnalogStick != null)
            {
                m_AnalogStick.BtnDirChange -= On_JoystickMove;
            }

            GlobalEvent.OnStopIner.RemoveListener(OnStopHandler);
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
        }


        //     //-------------------------------------------------------------------


        static void On_JoystickMove(Vector3 direction)
        {
            InputManager.Instance.OnJoystickMove(direction);
        }

        public static void SetTouchState(string key, bool value)
        {
            if (m_MapKeyTouchState.ContainsKey(key))
            {
                m_MapKeyTouchState[key] = value;
            }
            else
            {
                m_MapKeyTouchState[key] = value;
            }
        }

        public static bool GetTouchState()
        {
            foreach (bool val in m_MapKeyTouchState.Values)
            {
                if (val)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetIsTouchDown
        {
            get
            {
                return GetTouchState();
            }
        }
        #region 动画

        private void InitAnim()
        {
            AnimationClips.Clear();
            if (Clips != null && Clips.Count > 0)
            {
                foreach (var item in Clips)
                {
                    if (!AnimationClips.ContainsKey(item.name))
                    {
                        AnimationClips.Add(item.name, item);
                    }
                }
            }
        }
        private static string m_LastAnimName = string.Empty;

        private void PlayAnimation(string AimationName, System.Action action = null)
        {
            if (Animancer == null)
            {
                action?.Invoke();
                return;
            }
            if (AnimationClips.TryGetValue(AimationName, out AnimationClip animationClip) && animationClip != null)
            {
                //Animancer.Animator.enabled = true;
                var state = Animancer.Play(animationClip);
                state.Events.OnEnd = () =>
                {
                    state.IsPlaying = false;
                    //state.Stop();
                    //Animancer.Animator.enabled = false;
                    //Animancer.Playable.DestroyGraph();
                    //Animancer.Playable = null;
                    action?.Invoke();
                };
                //var state = Animancer.Play(animationClip, 0.25f, FadeMode.FromStart);
                //state.Events.OnEnd = () => {
                //    state.IsPlaying = false;
                //    action?.Invoke();
                //};
            }
        }

        private MainPageCommond lastFinalEveryPartNeedHide = MainPageCommond.HideNone;

        public void PlayAnimByState(bool isOpen, MainPageCommond finalEveryPartNeedHide)
        {
            //SGF.Debuger.LogError($"主界面动画  --------------- isOpen={isOpen},finalEveryPartNeedHide={finalEveryPartNeedHide}");
            string animName = string.Empty;
            if ((finalEveryPartNeedHide & MainPageCommond.WidgetHide) == MainPageCommond.WidgetHide)
            {
                //string mapSubType = GameManager.Instance.GetMapSubType();
                //if (mapSubType != GameConfig.INSTANCE_PLOT_SECOND)
                //{
                //    animName = "MobileControllerAnim_Close";
                //}
                animName = "MobileControllerAnim_Close";

            }
            else if ((finalEveryPartNeedHide & MainPageCommond.HideSkillWidgetCommond) == MainPageCommond.HideSkillWidgetCommond)
            {
                animName = "MobileControllerAnim_RightOut";
            }
            else if (finalEveryPartNeedHide == MainPageCommond.HideNone && lastFinalEveryPartNeedHide == MainPageCommond.HudHideRight)
            {
                animName = "MobileControllerAnim_RightGo";
            }
            else if (finalEveryPartNeedHide == MainPageCommond.HideNone)
            {
                //string mapSubType = GameManager.Instance.GetMapSubType();
                //if (mapSubType != GameConfig.INSTANCE_PLOT_SECOND)
                {
                    animName = "MobileControllerAnim_Open";
                }
            }

            lastFinalEveryPartNeedHide = finalEveryPartNeedHide;

            System.Action action = () =>
            {
                SetAreaShow(finalEveryPartNeedHide);
            };
            if (animName == string.Empty)
            {
                action?.Invoke();
                return;
            }
            if (m_LastAnimName != string.Empty && animName == m_LastAnimName)
            {
                action?.Invoke();
                return;
            }
            m_LastAnimName = animName;
            PlayAnimation(animName, action);
        }

        private void SetAreaShow(MainPageCommond finalEveryPartNeedHide)
        {
            return;
            {
                float alpha = 1f;
                if ((finalEveryPartNeedHide & MainPageCommond.HideMoveWidgetCommond) == MainPageCommond.HideMoveWidgetCommond)
                {
                    alpha = 0f;
                }
                LeftGroup.GetComponent<CanvasGroup>().alpha = alpha;
                LeftGroup.GetComponent<CanvasGroup>().interactable = alpha == 1;
                LeftGroup.GetComponent<CanvasGroup>().blocksRaycasts = alpha == 1;

            }

            {
                float alpha = 1f;
                if ((finalEveryPartNeedHide & MainPageCommond.HideSkillWidgetCommond) == MainPageCommond.HideSkillWidgetCommond)
                {
                    alpha = 0f;
                }
                RightGroup.GetComponent<CanvasGroup>().alpha = alpha;
                RightGroup.GetComponent<CanvasGroup>().interactable = alpha == 1;
                RightGroup.GetComponent<CanvasGroup>().blocksRaycasts = alpha == 1;

            }
        }

        #endregion

        //     #endregion
        //     //-------------------------------------------------------------------
    }
}