using ProtoMsg;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.Skill.Utils;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.DisplayProcess;
using StarProject.Service.FindPath;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.StarFramework.UI.Extend;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using Task;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.UI.WorldMap
{
    ////1,清理伙伴怪物，2移动是控制主角位置，3清理全部数据
    /// <summary>
    /// 移动和生成任务在StarWorldModule
    /// 其他在WorldMapModule
    /// 
    /// 
    /// </summary>
    public class MiniMapWindow : UIWindow
    {
        private string LOG_TAG = "[MiniMapWindow]";

        public ScrollViewMapExtend M_scrollRect;
        public RectTransform M_RectTransform; // 滑动区域
        public Text MapName; // 地图名字
        public Image m_miniMapImg; // 地图图片
        public Image m_miniMapMaskUpImg; // 地图遮挡图片
        public Image m_miniMapMaskDownImg; // 地图遮挡图片
        public RectTransform MainRole; // 主角节点
        public RectTransform m_miniMapNPCRoot; // NPC 父节点
        public RectTransform m_miniMapTaskRoot; // 任务标识 父节点
        public RectTransform m_miniMapMonsterRoot; // 怪物 父节点
        public RectTransform m_miniMapSpaceArenaRoot; // 异步竞技场 父节点
        public Transform m_miniMapAreaNameRoot; // 区域名 父节点
        public RectTransform m_miniMapWantedRoot; // 通缉 父节点
        public RectTransform m_FindPathRoot; // 寻路item 父节点
        public RectTransform m_TargetItem; // 点击 父节点
        public RectTransform m_FindPathDotItem; // 寻路item
        public RectTransform m_FindPathDotItem2; // 寻路item
        public Image m_LogoImageItem; // 怪物/NPC/任务Item 
        public RectTransform m_pvpRedItem;
        public Text m_AreaNameItem; // 区域名Item
        public RectTransform m_WantedItem; // 通缉Item
        public RectTransform m_MonsterItem; // 怪物Item
        public RectTransform m_GVEBossItem; // GVEBoss
        public JButton m_WorldMapJBtn; // 世界地图按钮
        public RectTransform mDailyTeamAreaRect; // 组队本光圈
        public RectTransform mDailyAreaRect; // 单人本光圈
        public WMapEntityRoot m_WMapEntityRoot; // 地图列表显示
        public Slider m_ScaleSlider; // 地图缩放滑动框
        public Text m_ScaleMin; // 地图缩放滑动最小值
        public Text m_ScaleMax; // 地图缩放滑动最大值

        private WorldMapModule WorldMapModule;
        private StarWorldModule My_Module;

        private Vector3 MainPlayerAngel = Vector3.zero; // 主角角度缓存
        private Vector2 MainPlayerAnchPos; // 主角坐标缓存

        private float CurMapScale = 1.0f; // 当前地图缩放值

        private bool NeedRefesh = false;

        public List<WorldMapGveItem> worldMapGveItems;


        private RectTransform _Canvas;

        public RectTransform Canvas
        {
            get
            {
                if (_Canvas == null)
                {
                    _Canvas = UIManager.Instance.M_Canvas.GetComponent<RectTransform>();
                }

                return _Canvas;
            }
        }

        private List<Image> NpcList = new();
        private List<RectTransform> MonsterMapList = new();
        private Dictionary<ulong, RectTransform> MonsterAoiList = new(); // 怪物 缓存

        private List<RectTransform> GVEBossList = new(); // 区域名 缓存
        private Dictionary<int, Dictionary<int, RectTransform>> WantedDic = new(); // 通缉标识节点缓存

        private List<Text> AreaNameList = new(); // 区域名 缓存

        //应该公用小地图的数据：1，我需要动态支持增加和减少，2，我支持移动，3，清理并且用别人的数据初始化我

        private Dictionary<ulong, RectTransform> SpaceArenaImages = new();  // 异步竞技场

        //【2这里是UI坐标不是3D坐标,需映射UI坐标给4倍，因为128对应1024所以系数是8】，【3因为Canvas中Transform移动和世界是1比1，Recttransform和世界比是4：1】
        private const float MiniMapConfWidth = 1024.0f; // [目前UI给的1300的图片但实际的行走区域是1024]【我们规定1024是实际行走区域】
        private const float MiniMapConfHeight = 1024.0f; // [目前UI给的1300的图片但实际的行走区域是1024]【我们规定1024是实际行走区域】
        private const float MapConfSzie = 256f; // 地图制作的规定尺寸
        private float m_MiniMapParaX = 4.0f; // 小地图图片宽MiniMapConfWidth1024与地图配置的X宽比例
        private float m_MiniMapParaY = 4.0f; // 小地图图片高MiniMapConfHeight1024与地图配置的Y高比例
        private float InitUIOffsetX = 1.0f; // 坐标初始X偏移量 地图配置的X宽 * m_MiniMapParaX / 2 [目的是把原点放在小地图图片的左下角]
        private float InitUIOffsetY = 1.0f; // 坐标初始Y偏移量 地图配置的Y高 * m_MiniMapParaY / 2 [目的是把原点放在小地图图片的左下角]
        private float CurMimiMapScaleConf = 1.0f; // 地图配置的缩放

        private bool isOpenWorldMap = false; // 是否点开了世界地图
        private SceneJsonData chanageMapSceneJsonData = null; // 世界地图切换小地图显示
        private ProtoMsg.SpaceType curMapType = SpaceType.SpaceDefault; // 当前地图的类型
        private bool m_OtherMapFindPathState = false; // 其他场景寻路状态

        /// <summary>
        /// 采集物件
        /// </summary>
        private List<RectTransform> m_Mines = new();
        public RectTransform m_Mine;
        public Transform m_MineRoot;
        public JButton mGatherBtn;
        public GameObject mSelect;
        public GameObject mUnSelect;
        public GameObject mGatherMapUI;
        public GatherMapUI mGather;

        public CurrencyBar mCurrencyBar;
        public Transform mCurrencyBarParent;
        public GameObject mGuideImage;
        private Dictionary<int, string> GatherSpriteName = new()
        {
            {101,"Live_icon_19"},
            {102,"Live_icon_18"},
            {103,"Live_icon_17"},
        };

        protected override void Awake()
        {
            onCloseDestroy = false;
            base.Awake();
            //WorldMapModule = ModuleManager.Instance.GetModule(ModuleDef.WorldMapModule) as WorldMapModule;
            //Open/Close;Init一次Current
            //挂件 和 主pageModule一一对应
            //这个MiniModule 和 全地图表现window一一对应

            float minScale = (float)SystemConstConfigs.LMapMinZoomRatio / 1000;
            m_ScaleMin.text = $"x{minScale}";
            m_ScaleSlider.minValue = minScale;
            float maxScale = (float)SystemConstConfigs.LMapMaxZoomRatio / 1000;
            m_ScaleMax.text = $"x{maxScale}";
            m_ScaleSlider.maxValue = maxScale;

            m_ScaleSlider.onValueChanged.AddListener(OnValueChanged);

            M_scrollRect.SetDragDistance(10.0f);
            M_scrollRect.SetScale(minScale, maxScale);
            M_scrollRect.SetScaleTarget(m_miniMapImg.rectTransform);
            M_scrollRect.FingerScaleAction = OnFingerScaleAction;

            m_WorldMapJBtn.OnClick = OnClickWorldMapJBtn;

            // 按钮事件
            {
                // 关闭按钮
                JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/CloseBtn").GetComponent<JButton>();
                Btn.OnClick = (go) => { OnBtnClose(); };
                // 地图遮挡按钮上
                {
                    Button maskBtn = m_miniMapMaskUpImg.GetComponent<Button>();
                    maskBtn.onClick.AddListener(OnMapMaskBtn);
                }
                // 地图遮挡按钮下
                {
                    Button maskBtn = m_miniMapMaskDownImg.GetComponent<Button>();
                    maskBtn.onClick.AddListener(OnMapMaskBtn);
                }
            }


            //采集相关
            mSelect.SetActive(false);
            mUnSelect.SetActive(true);
            mGather = new GatherMapUI(mGatherMapUI, OnGatherClose);
            mGatherBtn.OnClick += (go) =>
            {
                OnOpenMine();
            };
            mCurrencyBar = new CurrencyBar();
            mCurrencyBar.Init(mCurrencyBarParent);
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            My_Module = ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
            WorldMapModule = ModuleManager.Instance.GetModule(ModuleDef.Name.WorldMapModule) as WorldMapModule;
            WorldMapModule.OnChanageMapAction = OnChanageMap;

            InitMapPos();

            //My_Module.OnMonsterMoveAction += UpdateMiniMapMonster;
            //GameManager.Instance.OnMonsterDataChange += OnMonsterAddDelete;
            //My_Module.OnNpcMiniShowDataChange += AddNPC;//功能打开的放大，原功能数据必然已经准备好，这里是调用；还留着动态更新防止服务器传送我这时候；
            My_Module.OnMainPlayerMoveAction += UpdateMainPlayerPos;
            My_Module.OnMainPlayerRotAction += UpdateMainPlayerRot;
            My_Module.OnSpaceArenaMoveAction += UpdateSpaceArenaPos;
            My_Module.OnMonsterMoveAction += UpdateAoiMiniMapMonster;

            GameManager.Instance.OnMonsterDataChange += OnMonsterAddDelete;
            GameManager.Instance.OnSpaceArenaDataChange += OnSpaceArenaAddDelete;

            GlobalEvent.OnSceneLoadedBinded.AddListener(OnSceneLoadSuccess);
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
            GlobalEvent.TaskStageChange.AddListener(OnTaskChange);
            GlobalEvent.OnPersonSecretBossAppearRet.AddListener(OnPersonSecretBossAppearRet);
            GlobalEvent.OnMiniMapShowEctypeTask.AddListener(OnMiniMapShowEctypeTaskHandler);
            GlobalEvent.OnGVEBossRefesh.AddListener(OnGVEBossRefeshHandler);
            GlobalEvent.onRefreshWTaskPointTarInfo.AddListener(RefreshWantedEntity);

            OnSceneLoadSuccess(null); // 加载地图

            OnSceneMapLoadComplete(0); // 初始化【区域名】【NPC】【怪物】【GVEBOSS】

            InitMainPlayerData();
            InitWantedData();
            InitTask();

            InitWMapEntityRoot(GameManager.Instance.M_Map.GetMapId(), curMapType);

            OnShowEctypeTask(WorldMapModule.EctypeStage);

            OnShowEctypDailyTeam();

            SetShowWorldMapBtn(curMapType);
            // 设置寻路点的显隐
            InitFindPathTarget();

            if (mCurrencyBar != null)
            {
                mCurrencyBar.InitBar(new List<long>() { 116 });
                mCurrencyBar.OnListener();
            }
            M_scrollRect.SetState(true);
            mGatherBtn.gameObject.SetActive(curMapType == SpaceType.SpaceScene);
            OnSelect(GameManager.Instance.M_Map.GetMapId(), GatherMapSelect.GetMapSelect(GameManager.Instance.M_Map.GetMapId()));
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
            My_Module.OnMainPlayerMoveAction -= UpdateMainPlayerPos;
            My_Module.OnMainPlayerRotAction -= UpdateMainPlayerRot;

            My_Module.OnSpaceArenaMoveAction -= UpdateSpaceArenaPos;

            My_Module.OnMonsterMoveAction -= UpdateAoiMiniMapMonster;

            GameManager.Instance.OnMonsterDataChange -= OnMonsterAddDelete;

            GameManager.Instance.OnSpaceArenaDataChange -= OnSpaceArenaAddDelete;

            //My_Module.OnNpcMiniShowDataChange -= AddNPC;

            GlobalEvent.OnSceneLoadedBinded.RemoveListener(OnSceneLoadSuccess);
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);

            GlobalEvent.TaskStageChange.RemoveListener(OnTaskChange);

            GlobalEvent.OnPersonSecretBossAppearRet.RemoveListener(OnPersonSecretBossAppearRet);

            GlobalEvent.OnMiniMapShowEctypeTask.RemoveListener(OnMiniMapShowEctypeTaskHandler);
            GlobalEvent.OnGVEBossRefesh.RemoveListener(OnGVEBossRefeshHandler);
            GlobalEvent.onRefreshWTaskPointTarInfo.RemoveListener(RefreshWantedEntity);
            chanageMapSceneJsonData = null;

            ClearAoiMonster();
            ClearMapWanted();
            ClearMapGVEBoss();
            ClearMapNPC();
            ClearAreaName();
            ClearTaskFlag();
            OnShowEctypeTask(-1);
            ClearEctypDailyTeam();
            ClearMines();
            m_TargetItem.gameObject.SetActive(false);
            isOpenWorldMap = false;
            HideEntityRoot();
            ClearFindPath();
            ClearSpaceArena();


            EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
            if (mainPlayer != null && mainPlayer.M_Curr != null)
            {
                mainPlayer.M_Curr.OnFindPathCallBack -= OnMainPlayerFindPathCallBack;
            }
            mGather.OnClose();
            M_scrollRect.SetState(false);

            if (mCurrencyBar != null)
            {
                mCurrencyBar.OffListener();
                mCurrencyBar.OnRelease();
                mCurrencyBar = null;
            }
        }

        //刷新boss倒计时
        private void Update()
        {
            #region GVEBOSS

            if (NeedRefesh)
            {
                var GVEStartI2S = GameManager.Instance.GVEStartI2S;
                if (GVEStartI2S != null)
                {
                    long serverNowTime = GameManager.Instance.GetServerTimeStamp() / 1000;
                    long serverCloseTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.Duration;
                    long leftTime = serverCloseTime - serverNowTime;

                    if (leftTime > 0)
                    {
                        long bossInterval = GVEStartI2S.BossInterval; //boss刷新间隔
                        int curWave = GVEStartI2S.CurWave; //当前已刷的波次（变
                        long bossDelay = GVEStartI2S.BossDelay; //boss首波延迟
                        long nextBossTime = (GVEStartI2S.StartTime / 1000) + bossDelay + (curWave * bossInterval);

                        if (nextBossTime < serverCloseTime)
                        {
                            for (int i = 0; i < worldMapGveItems.Count; i++)
                            {
                                if (worldMapGveItems[i].gameObject.activeSelf)
                                {
                                    worldMapGveItems[i].UpdateTime(nextBossTime - serverNowTime);
                                }
                            }
                        }
                        else
                        {
                            //GVEBossLeftTime.text = "curWave:"+ curWave;
                        }
                    }
                    else
                    {
                        NeedRefesh = false;
                        for (int i = 0; i < worldMapGveItems.Count; i++)
                        {
                            worldMapGveItems[i].Show(false);
                        }

                        ClearMapGVEBoss();
                    }
                }
            }

            #endregion

            #region 屏幕点击寻路

            if (!isOpenWorldMap && (curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceGuildTerritory))
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (M_scrollRect.isDrag || M_scrollRect.isScaleing)
                    {
                        // 触发滑动，过滤点击寻路
                        SGF.Debuger.LogWarning($"触发滑动，过滤点击寻路");
                        return;
                    }

                    //SGF.Debuger.LogError($"1111111111");
                    if (!m_OtherMapFindPathState && (curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceGuildTerritory) &&
                        m_miniMapMaskUpImg.gameObject.activeInHierarchy &&
                        m_miniMapMaskDownImg.gameObject.activeInHierarchy)
                    {
                        // 没有点击到正确路线上
                        SGF.Debuger.LogWarning($"没有点击到正确路线上");
                        return;
                    }

                    Vector2 uisize = Canvas.sizeDelta; //得到画布的尺寸
                    Vector2 screenpos = Input.mousePosition;
                    Vector2 screenpos2;
                    screenpos2.x = screenpos.x - (Screen.width / 2); //转换为以屏幕中心为原点的屏幕坐标
                    screenpos2.y = screenpos.y - (Screen.height / 2);
                    Vector2 uipos; //UI坐标
                    uipos.x = screenpos2.x * (uisize.x / Screen.width); //转换后的屏幕坐标*画布与屏幕宽高比
                    uipos.y = screenpos2.y * (uisize.y / Screen.height);
                    uipos.x -= M_RectTransform.anchoredPosition.x;
                    uipos.y -= M_RectTransform.anchoredPosition.y;
                    //uipos *= CurMapScale;
                    //m_TargetItem.transform.localPosition = uipos;
                    //m_TargetItem.gameObject.SetActive(true);
                    ClickScreenFindPath(uipos);
                }
            }

            #endregion
        }

        private Vector2 GetPos(float x, float y)
        {
            Vector2 pos = Vector2.zero;
            pos.x = (x * m_MiniMapParaX) - InitUIOffsetX;
            pos.y = (y * m_MiniMapParaY) - InitUIOffsetY;
            if (CurMimiMapScaleConf > 1)
            {
                pos *= CurMimiMapScaleConf;
            }

            if (CurMapScale > 1)
            {
                pos *= CurMapScale;
            }

            return pos;
        }

        private void ClickScreenFindPath(Vector2 pos)
        {
            EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
            if (mainPlayer != null && mainPlayer.M_Curr != null)
            {
                Vector3 startPos = mainPlayer.M_Curr.Position();
                Vector3 target = Vector3.zero;
                if (CurMimiMapScaleConf > 1)
                {
                    pos /= CurMimiMapScaleConf;
                }

                if (CurMapScale > 1)
                {
                    pos /= CurMapScale;
                }

                target.x = (pos.x + InitUIOffsetX) / m_MiniMapParaX;
                target.z = (pos.y + InitUIOffsetY) / m_MiniMapParaY;
                if (chanageMapSceneJsonData == null)
                {
                    float y = BusinessManager.Instance.GetGroundHeight(target.x, target.z);
                    target.y = y > 0 ? y : startPos.y;
                    bool isCheck = FindPathManager.Instance.FindValidPoint(target, out target);

                    if (isCheck)
                    {
                        // {
                        //     /// 2024/4/24
                        //     /// 点击小地图后，如果原子锁锁住移动, 那需要等原子锁解开之后， 需要能够移动过去.
                        //     /// 对于自动战斗而言, 自动战斗 技能基本都是 无缝衔接, 所以原子锁没法立即解开.
                        //     /// 所以先将 自动战斗的 状态设置为 寻路状态.
                        //     /// note:
                        //     ///     由于CheckWalkable 已经做了检查, 所以目标点是可以到达的.
                        //     ///     如果由于原子锁导致 没法寻路, 已经通过 recordOnStateForbid  的方式做了原子锁解开后 寻路的恢复
                        //     GameManager.Instance.TriggerEvent("FindingPath", true);
                        // }
                        BusinessManager.Instance.FindPathByPosition(GameManager.Instance.M_Map.GetMapId(), target, 0.1f,
                            (result) =>
                            {
                                // 小地图结束后, 需要 恢复自动战斗的 逻辑.
                                // 目前 寻路没有 类型, 所以 需要在 对应的调用点 增加相应的自动战斗相关的 逻辑
                                // BattleManager.Instance.RecordAutoBattleStartPos(true);





                            }, false, true);


                        m_TargetItem.transform.localPosition = pos;
                        m_TargetItem.gameObject.SetActive(true);
                        UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);




                    }
                    else
                    {
                        //DisplayProcessDispenser.Instance.AddSystemMessage(GameConfig.LocalStr["PosUnreachable"]);
                        DisplayProcessDispenser.Instance.AddSystemMessage(LanguageManager.Instance.GetLanguageByKey("PosUnreachable"));
                    }
                }
                else
                {
                    if (m_OtherMapFindPathState)
                    {
                        BusinessManager.Instance.FindPathByPosition(chanageMapSceneJsonData.SceneID, target, 0.1f, null,
                            true);
                        UIManager.Instance.CloseWindow(UIDef.WorldMapWindow);
                        UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
                    }
                }

                m_OtherMapFindPathState = false;
                ////GameObject god = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/PosGobCache") as GameObject);
                //GameObject god = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/PosGobCache");

                //god.transform.position = target;

                SGF.Debuger.LogWarning($"点击寻路 = target={target}");
            }
        }



        #region 地图缩放

        private void InitMapPos()
        {
            // 每次打开小地图，都回归00默认位置
            Rect rect = M_RectTransform.rect;
            M_RectTransform.transform.localPosition = Vector3.zero;
            rect.xMax = 0;
            rect.yMax = 0;
            // 缩放还原
            CurMapScale = 1;
            m_miniMapImg.transform.SetLocalScale(Vector3.one);
            m_ScaleSlider.value = CurMapScale;
        }

        private void OnValueChanged(float value)
        {
            if (value == CurMapScale)
            {
                return;
            }

            ScaleMap(CurMapScale, value);
        }

        private void OnFingerScaleAction(float value)
        {
            if (value == CurMapScale)
            {
                return;
            }

            m_ScaleSlider.value = value;
            ScaleMap(CurMapScale, value);
        }

        private void ScaleMap(float lastScale, float curScale)
        {
            CurMapScale = curScale;
            Vector3 scale = Vector3.one * curScale;
            m_miniMapImg.transform.SetLocalScale(scale);
            UpdateScaleMainPlayerPos(curScale, lastScale);
            UpdateScaleMonsterPos(curScale, lastScale);
            UpdateScaleAoiMonsterPos(curScale, lastScale);
            UpdateScaleTaskFlagPos(curScale, lastScale);
            UpdateScaleNpcPos(curScale, lastScale);
            UpdateScaleGVEBossPos(curScale, lastScale);
            UpdateScaleAreaNamePos(curScale, lastScale);
            UpdateScaleWantedPos(curScale, lastScale);
            UpdateScaleEctypeTaskPos(curScale, lastScale);
            UpdateScalemDailyTeamAreaRectPos(curScale, lastScale);
            UpdateScaleContentPos(curScale, lastScale);
            UpdateFindPathPos(curScale, lastScale);
            UpdateFindPathTargetPos(curScale, lastScale);
            UpdateScaleSpaceArenaPos(curScale, lastScale);
            UpdateScaleMinePos(curScale, lastScale);
        }

        private void UpdateScaleContentPos(float scale, float lastScale)
        {
            RectTransform rt = M_scrollRect.content;
            Vector2 pos = rt.anchoredPosition / lastScale * scale;
            rt.anchoredPosition = pos;
        }

        #endregion

        #region 主角逻辑

        private void InitMainPlayerData()
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
            {
                UpdateMainPlayerRot(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ServerAngles);
                UpdateMainPlayerPos(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position());
                MainRole.gameObject.SetActive(true);
            }
        }

        // 更新主角【移动】
        private void UpdateMainPlayerPos(Vector3 v3PlayerPos)
        {
            //00 对应 00 :服务器要求左下角是00，所以地编也是左下角00，但是UI需要在中心点：【1需要映射逻辑给InitUIOffsetX】
            //ui 和   世界 一一对应
            //X物理正，UI负数；Z正Y负数
            //if (MiniMapPos != null)
            //          {
            //333不要删除，万一他要呢，哪天你再加回来么？
            //              MiniMapPos.text = "X (" + v3PlayerPos.x.ToString("f1") + ")" + "  Y (" + v3PlayerPos.z.ToString("f1") + ")";
            //m_AnchoredV2.x = InitUIOffsetX + /*transform.position.z */v3PlayerPos.x * m_MiniMapPara ;//
            //m_AnchoredV2.y = InitUIOffsetY + /*transform.position.x*/v3PlayerPos.z * m_MiniMapPara ;

            ////MiniGround45度，对应摄像机45度，对应坐标变换45度
            //m_AnchoredV2 = CalcNewPoint(m_AnchoredV2, Vector2.zero, Cur_Angel);//这里是0度,
            MainPlayerAnchPos = GetPos(v3PlayerPos.x, v3PlayerPos.z);
            MainRole.anchoredPosition = MainPlayerAnchPos;
            //地图不动
            //miniRoot.anchoredPosition = m_AnchoredV2;
            //主角不动
            //MainRole.anchoredPosition = m_AnchoredV2;//【2这里是UI坐标不是3D坐标,需映射UI坐标给4倍，因为128对应512所以系数是4】，【3因为Canvas中Transform移动和世界是1比1，Recttransform和世界比是4：1
            UpdateFindPathItemAction(MainRole.localPosition);
        }

        // 更新主角【旋转】
        private void UpdateMainPlayerRot(float rot)
        {
            MainPlayerAngel.z = rot - 90;
            MainRole.localEulerAngles = MainPlayerAngel;
            //Cur_Angel = CameraAngel.y;
        }

        private void UpdateScaleMainPlayerPos(float scale, float lastScale)
        {
            Vector2 pos = MainRole.anchoredPosition / lastScale * scale;
            MainRole.anchoredPosition = pos;
            //if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
            //{
            //    UpdateMainPlayerPos(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position());
            //}
        }

        #endregion

        #region 设置寻路点

        private void InitFindPathTarget()
        {
            ClearFindPath();
            bool isShow = false;
            EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
            if (mainPlayer != null && mainPlayer.M_Curr != null)
            {
                isShow = mainPlayer.M_Curr.Is_MainPlayer_FindingPath;
                mainPlayer.M_Curr.OnFindPathCallBack += OnMainPlayerFindPathCallBack;
            }

            m_TargetItem.gameObject.SetActive(false);
            if (isShow)
            {
                var pathArr = mainPlayer.M_Curr.FindPathPoints.ToList();
                Vector3 startPos = mainPlayer.M_Curr.Position();
                Vector3 endPos = Vector3.zero;
                if (pathArr.Count > 0)
                {
                    endPos = pathArr[pathArr.Count - 1];
                }
                else
                {
                    endPos = mainPlayer.M_Curr.nextMovePoint;
                }
                if (endPos != Vector3.zero)
                {
                    pathArr.Insert(0, startPos);
                    pathArr.Add(endPos);
                    m_TargetItem.transform.localPosition = GetPos(endPos.x, endPos.z);
                    m_TargetItem.gameObject.SetActive(true);
                }
                InitFindPath(startPos, endPos, pathArr);
            }
        }

        private void OnMainPlayerFindPathCallBack(bool isFind)
        {
            m_TargetItem.gameObject.SetActive(false);
            ClearFindPath();
        }

        private void InitFindPath(Vector3 curPos, Vector3 endPos, List<Vector3> findPathPoints)
        {
            List<Vector3> findPath = new();
            Vector3 tempStartPoint = Vector3.zero;
            for (int i = 0; i < findPathPoints.Count; i++)
            {
                if (i + 1 < findPathPoints.Count)
                {
                    Vector3 firstPos = tempStartPoint == Vector3.zero ? findPathPoints[i] : tempStartPoint;
                    Vector3 nextPos = findPathPoints[i + 1];
                    List<Vector3> points = Fire.Utils.DividePoints(firstPos, nextPos, 8.0f, false, true, ref tempStartPoint);
                    if (firstPos == tempStartPoint)
                    {
                        continue;
                    }
                    //SGF.Debuger.LogWarning($"寻路等分坐标 i={i}----------------------firstPos={firstPos},nextPos={nextPos},tempStartPoint={tempStartPoint}");
                    for (int j = 0; j < points.Count; j++)
                    {
                        Vector3 point = points[j];
                        findPath.Add(point);
                        //SGF.Debuger.Log($"寻路等分坐标 i={i},j={j},point={point}");
                    }
                    //CreatFindPathItem2(pathArr[i]);
                }
            }
            //SGF.Debuger.LogError($"寻路等分坐标 curPos={curPos},endPos={endPos},pointsCount={findPath.Count}");
            SetFindPathItem(findPath);
        }

        private void SetFindPathItem(List<Vector3> findPathPoints)
        {
            Vector3 tempPoint = Vector3.zero;
            for (int i = 0; i < findPathPoints.Count; i++)
            {
                Vector3 pos = findPathPoints[i];
                //if (tempPoint != Vector3.zero)
                //{
                //    SGF.Debuger.Log($"寻路等分坐标 间距 i={i},point={(pos - tempPoint).magnitude}");
                //}
                if (tempPoint == pos)
                {
                    continue;
                }
                CreatFindPathItem(pos);
                tempPoint = pos;
            }
        }

        private void ClearFindPath()
        {
            m_FindPathRoot.DestroyChildren();
        }

        private void CreatFindPathItem(Vector3 pos)
        {
            var item = Instantiate(m_FindPathDotItem);
            item.SetParent(m_FindPathRoot);
            item.gameObject.SetActive(true);

            item.anchoredPosition = GetPos(pos.x, pos.z);
        }

        private void CreatFindPathItem2(Vector3 pos)
        {
            var item = Instantiate(m_FindPathDotItem2);
            item.SetParent(m_FindPathRoot);
            item.gameObject.SetActive(true);

            item.anchoredPosition = GetPos(pos.x, pos.z);
        }
        public float spacingDistance = 20;
        private void UpdateFindPathItemAction(Vector3 mainPlayerPos)
        {
            if (m_FindPathRoot.childCount <= 0)
            {
                return;
            }

            foreach (var item in m_FindPathRoot.GetChildren())
            {
                if (item != null && item.gameObject && item.gameObject.activeInHierarchy)
                {
                    bool isShow = (mainPlayerPos - item.transform.localPosition).magnitude > spacingDistance;
                    item.gameObject.SetActive(isShow);
                }
            }
        }

        private void UpdateFindPathPos(float scale, float lastScale)
        {
            if (m_FindPathRoot.childCount <= 0)
            {
                return;
            }
            for (int i = m_FindPathRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = m_FindPathRoot.transform.GetChild(i);
                if (child != null)
                {
                    RectTransform rect = child.GetComponent<RectTransform>();
                    Vector2 pos = rect.anchoredPosition / lastScale * scale;
                    rect.anchoredPosition = pos;
                }
            }
        }

        private void UpdateFindPathTargetPos(float scale, float lastScale)
        {
            Vector2 pos = m_TargetItem.anchoredPosition / lastScale * scale;
            m_TargetItem.anchoredPosition = pos;
        }

        #endregion

        #region 任务标识逻辑

        private void InitTask()
        {
            FreshTasks(GameMap.sceneJsonData);
        }

        private void OnTaskChange(int arg0, int arg1)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            FreshTasks(GameMap.sceneJsonData);
        }

        private void FreshTasks(SceneJsonData sceneJsonData)
        {
            //清理数据
            ClearTaskFlag();

            if (chanageMapSceneJsonData == null)
            {
                {
                    Dictionary<long, List<TaskNpc>> canReceiveNpcTask = TaskHelper.GetCanReceiveNpcTask();
                    if (canReceiveNpcTask != null && canReceiveNpcTask.Count > 0)
                    {
                        foreach (var item in canReceiveNpcTask)
                        {
                            Vector3 position = GameManager.Instance.GetEntityPosByCfgID(item.Key);
                            foreach (var item2 in item.Value)
                            {
                                var taskConfig = TaskHelper.GetTaskConfig(item2.TaskID);
                                Task.TaskClassifyType taskType = Task.TaskClassifyType.MainLine;
                                if (taskConfig != null && taskConfig.Base != null)
                                {
                                    taskType = taskConfig.Base.TaskType;
                                }
                                CreateTaskFlag((int)taskType, TaskStateEnum.TaskStateNoGetReward, position);
                            }
                        }
                    }
                }

                {
                    Dictionary<long, List<TaskNpc>> canReceiveInterTask = TaskHelper.GetCanReceiveInterTask();
                    if (canReceiveInterTask != null && canReceiveInterTask.Count > 0)
                    {
                        foreach (var item in canReceiveInterTask)
                        {
                            Vector3 position = GameManager.Instance.GetEntityPosByCfgID(item.Key);
                            foreach (var item2 in item.Value)
                            {
                                var taskConfig = TaskHelper.GetTaskConfig(item2.TaskID);
                                Task.TaskClassifyType taskType = Task.TaskClassifyType.MainLine;
                                if (taskConfig != null && taskConfig.Base != null)
                                {
                                    taskType = taskConfig.Base.TaskType;
                                }
                                CreateTaskFlag((int)taskType, TaskStateEnum.TaskStateNoGetReward, position);
                            }
                        }
                    }
                }
            }

            {
                //当前运行的任务数据
                var Tasks = TaskHelper.GetAllTask();
                if (Tasks != null && Tasks.Count > 0)
                {
                    foreach (var item in Tasks)
                    {
                        bool delete = item.TaskState == ProtoMsg.TaskStateEnum.TaskStateBeDeleted ||
                                      item.TaskState == ProtoMsg.TaskStateEnum.TaskStateGetReward;
                        if (delete)
                        {
                            continue;
                        }

                        if (item.TaskTarget != null && item.TaskTarget.Count > 0)
                        {
                            int taskType = item.TaskType;
                            TaskStateEnum TaskState = item.TaskState;
                            foreach (var target in item.TaskTarget)
                            {
                                if (target.FindPath.FindPathType == Task.E_FindPath.None)
                                {
                                    continue;
                                }

                                if (target.MapID != sceneJsonData.SceneID)
                                {
                                    continue;
                                }

                                Vector3 position = Vector3.zero;
                                bool isFind = false;
                                switch (target.FindPath.FindPathType)
                                {
                                    case Task.E_FindPath.NPC:

                                        var npc = sceneJsonData.GetNPCJsonData(target.FindPath.ID);
                                        if (npc != null)
                                        {
                                            position = npc.Position.Convert();
                                            isFind = true;
                                        }
                                        break;
                                    case Task.E_FindPath.InterAction:
                                        var mine = sceneJsonData.GetMineJsonData(target.FindPath.ID);
                                        if (mine != null)
                                        {
                                            position = mine.Position.Convert();
                                            isFind = true;
                                        }
                                        break;
                                    case Task.E_FindPath.Area:
                                        if (sceneJsonData.Areas.ContainsKey((int)target.FindPath.ID))
                                        {
                                            position = sceneJsonData.Areas[(int)target.FindPath.ID].Position.Convert();
                                            isFind = true;
                                        }
                                        break;
                                    case Task.E_FindPath.Spawer:
                                        if (sceneJsonData.Spawners.ContainsKey((int)target.FindPath.ID))
                                        {
                                            position = sceneJsonData.Spawners[(int)target.FindPath.ID].Position.Convert();
                                            isFind = true;
                                        }
                                        break;
                                }
                                if (isFind)
                                {
                                    CreateTaskFlag(taskType, TaskState, position);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateTaskFlag(int taskType, TaskStateEnum TaskState, Vector3 position)
        {
            string iconPath = TaskManager.GetUITaskIconNameByType((TaskClassifyType)taskType, TaskState);
            Image image = Instantiate(m_LogoImageItem);
            GetSprite(iconPath, (sp) =>
            {
                if (image != null)
                {
                    image.sprite = sp;
                    image.SetNativeSize();
                }
            });
            image.rectTransform.SetParent(m_miniMapTaskRoot.transform);
            image.gameObject.SetActive(true);
            image.transform.localPosition = Vector3.zero;
            image.transform.localScale = Vector3.one;
            image.rectTransform.anchoredPosition = GetPos(position.x, position.z);
            image.rectTransform.name = $"{position.x}_{position.z}";
        }

        private void ClearTaskFlag()
        {
            for (int i = m_miniMapTaskRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = m_miniMapTaskRoot.transform.GetChild(i);
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void UpdateScaleTaskFlagPos(float scale, float lastScale)
        {
            for (int i = m_miniMapTaskRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = m_miniMapTaskRoot.transform.GetChild(i);
                if (child != null)
                {
                    RectTransform rect = child.GetComponent<RectTransform>();
                    //rect.anchoredPosition *= scale;
                    Vector2 pos = rect.anchoredPosition / lastScale * scale;
                    rect.anchoredPosition = pos;
                    //string[] posArr = rect.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    rect.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        #endregion

        #region NPC逻辑

        //【Init生成，缓存dic】，并且时刻等待数据变化；为了方便控制；数据清理—表现清理
        private void InitNPC(SceneJsonData sceneJsonData)
        {
            if (sceneJsonData != null && sceneJsonData.Npcs != null && sceneJsonData.Npcs.Count > 0)
            {
                AddNPC(sceneJsonData.Npcs);
            }
        }

        private void AddNPC(Dictionary<long, NPCJsonData> allNpcs)
        {
            int index = 0;
            foreach (var item in allNpcs)
            {
                NpcDataCell npc = LocalDataManager.Instance.GetNPCDataCell((uint)item.Value.NpcID);
                if (npc == null)
                {
                    continue;
                }

                if (npc.MapLogo == null || npc.MapLogo == string.Empty || npc.MapLogo == "0")
                {
                    continue;
                }
                else
                {
                    bool isShow = true;
                    var npcEntityCtrl = GameManager.Instance.GetNPCCtrlGroupByConfig(npc.GetID());
                    if (npcEntityCtrl != null)
                    {
                        isShow = npcEntityCtrl.IsShow;
                    }
                    if (isShow)
                    {
                        string iconName = $"{npc.MapLogo}";
                        Action<Sprite> cb = (Sprite sp) => { SetNpcLogo(sp, item.Value, index); };
                        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, cb);
                        index++;
                    }
                }
            }
        }

        private void SetNpcLogo(Sprite img, NPCJsonData nPCJsonData, int index)
        {
            Image image = GetNPCItem(index);
            if (img != null)
            {
                image.sprite = img;
            }

            image.transform.name = nPCJsonData.NpcID.ToString();
            bool isShow = BusinessManager.Instance.GetNPCVisiable(nPCJsonData.NpcID);
            image.rectTransform.anchoredPosition = GetPos(nPCJsonData.Position.x, nPCJsonData.Position.z);
            image.gameObject.SetActive(isShow);
        }

        private Image GetNPCItem(int index)
        {
            Image rectGod = GetIdleNPCByIndex(index);
            if (rectGod == null)
            {
                rectGod = Instantiate(m_LogoImageItem);
                rectGod.transform.SetParent(m_miniMapNPCRoot.transform);
                NpcList.Add(rectGod);
            }

            rectGod.gameObject.SetActive(true);
            rectGod.transform.localPosition = Vector3.zero;
            rectGod.transform.localScale = Vector3.one;

            return rectGod;
        }

        private Image GetIdleNPCByIndex(int index)
        {
            if (NpcList.Count > index)
            {
                return NpcList[index];
            }

            return null;
        }

        private void ClearMapNPC()
        {
            for (int i = 0; i < NpcList.Count; i++)
            {
                var child = NpcList[i];
                if (child != null)
                {
                    child.transform.name = "0";
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateScaleNpcPos(float scale, float lastScale)
        {
            for (int i = 0; i < NpcList.Count; i++)
            {
                var child = NpcList[i];
                if (child != null)
                {
                    //child.anchoredPosition *= scale;
                    Vector2 pos = child.rectTransform.anchoredPosition / lastScale * scale;
                    child.rectTransform.anchoredPosition = pos;
                    //string[] posArr = child.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        #endregion

        #region GVEBoss逻辑

        private void InitGVEBoss(int mapID, SceneJsonData sceneJsonData)
        {
            if (!BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.PlayModeGVE))
                return;

            var GVEStartI2S = GameManager.Instance.GVEStartI2S;
            //GVE活动中的boss
            if (GVEStartI2S != null)
            {
                long serverNowTime = GameManager.Instance.GetServerTimeStamp() / 1000;
                long serverCloseTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.Duration;
                long serverDelayTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.BossDelay;
                long leftTime = serverCloseTime - serverNowTime;

                //在活动时间内
                if (leftTime > 0)
                {
                    var bossList = GameManager.Instance.BossList;
                    NeedRefesh = true;
                    if (sceneJsonData != null)
                    {
                        int index = 0;
                        if (sceneJsonData.GVE_Points != null && sceneJsonData.GVE_Points.Count > 0)
                        {
                            foreach (var areaJson in sceneJsonData.GVE_Points)
                            {
                                int BossIdx = areaJson.Key;

                                bool isFindboss = false;

                                //boss出现延迟
                                if (serverNowTime > serverDelayTime)
                                {
                                    //有boss就显示boss
                                    for (int i = 0; i < bossList.Count; i++)
                                    {
                                        var boss = bossList[i];
                                        //同一张地图的boss
                                        if (boss.MapID == mapID && !boss.IsDead && boss.PosIndex == BossIdx)
                                        {
                                            int bossType =
                                                LocalDataManager.Instance.GetGVEBossType(boss.BossID, GVEStartI2S.PlayID);
                                            var rectGod = GetGVEBossItem(index, bossType);
                                            rectGod.anchoredPosition = GetPos(boss.Pos.X, boss.Pos.Z);
                                            rectGod.name = $"{boss.Pos.X}_{boss.Pos.Z}";
                                            isFindboss = true;

                                            var bossCfg = LocalDataManager.Instance.GetMonsterDataCell(boss.BossID);
                                            worldMapGveItems[index].SetBossInfo(bossCfg.Name,
                                                rectGod.GetComponent<Image>().sprite);

                                            index++;
                                            break;
                                        }
                                    }
                                }


                                //没boss就显示洞
                                if (!isFindboss)
                                {
                                    var rectGod = GetGVEBossItem(index, 0);
                                    rectGod.anchoredPosition = GetPos(areaJson.Value.x, areaJson.Value.z);
                                    rectGod.name = $"{areaJson.Value.x}_{areaJson.Value.z}";

                                    worldMapGveItems[index].SetNullBoss(rectGod.GetComponent<Image>().sprite);


                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private RectTransform GetGVEBossItem(int index, int type)
        {
            RectTransform rectGod = GetIdleGVEBossByIndex(index);
            if (rectGod == null)
            {
                rectGod = Instantiate(m_GVEBossItem);
                rectGod.SetParent(m_miniMapMonsterRoot);
                GVEBossList.Add(rectGod);
            }

            rectGod.gameObject.SetActive(true);
            rectGod.transform.localPosition = Vector3.zero;
            rectGod.transform.localScale = Vector3.one;

            var ImageName = "";
            if (type == 0)
            {
                ImageName = "Map_icon_04";
            }
            else if (type == 1)
            {
                ImageName = "Map_icon_02";
            }
            else if (type == 2)
            {
                ImageName = "Map_icon_03";
            }
            else if (type == 3)
            {
                ImageName = "Map_icon_01";
            }

            GetSprite(ImageName, (sp) =>
            {
                if (rectGod != null)
                {
                    var Image = rectGod.GetComponent<Image>();
                    if (Image != null)
                    {
                        Image.sprite = sp;
                    }
                }
            });

            return rectGod;
        }

        private RectTransform GetIdleGVEBossByIndex(int index)
        {
            if (GVEBossList.Count > index)
            {
                return GVEBossList[index];
            }

            return null;
        }

        private void ClearMapGVEBoss()
        {
            NeedRefesh = false;
            for (int i = 0; i < worldMapGveItems.Count; i++)
            {
                worldMapGveItems[i].Show(false);
            }

            for (int i = 0; i < GVEBossList.Count; i++)
            {
                var child = GVEBossList[i];
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void OnGVEBossRefeshHandler(int arg0)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            ClearMapGVEBoss();
            int mapID = GameManager.Instance.M_Map.GetMapId();
            InitGVEBoss(mapID, GameMap.sceneJsonData);
        }

        private void UpdateScaleGVEBossPos(float scale, float lastScale)
        {
            for (int i = 0; i < GVEBossList.Count; i++)
            {
                var child = GVEBossList[i];
                if (child != null)
                {
                    Vector2 pos = child.anchoredPosition / lastScale * scale;
                    child.anchoredPosition = pos;
                    //string[] posArr = child.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        #endregion

        #region 怪物逻辑

        private void InitMapMonster(SceneJsonData sceneJsonData, ProtoMsg.SpaceType curMapType)
        {
            if (sceneJsonData != null)
            {
                int index = 0;
                if (curMapType == ProtoMsg.SpaceType.SpaceSercet || curMapType == ProtoMsg.SpaceType.SpaceTeamDaily)
                {
                    if (sceneJsonData.RandomMonsters != null && sceneJsonData.RandomMonsters.Count > 0)
                    {
                        foreach (var areaJson in sceneJsonData.RandomMonsters)
                        {
                            if (areaJson.Value.MonsterType == 3 /*(int)MapEditor.MonsterType.Boss*/)
                            {
                                var rectGod = GetMapMonsterItem(index, areaJson.Value.MonsterType);
                                rectGod.anchoredPosition = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                                rectGod.name = $"{areaJson.Value.Position.x}_{areaJson.Value.Position.z}";
                                index++;
                            }
                        }
                    }
                }
                else
                {
                    if (sceneJsonData.Monsters != null && sceneJsonData.Monsters.Count > 0)
                    {
                        foreach (var areaJson in sceneJsonData.Monsters)
                        {
                            if (areaJson.Value.MonsterType == 3 /*(int)MapEditor.MonsterType.Boss*/)
                            {
                                var rectGod = GetMapMonsterItem(index, areaJson.Value.MonsterType);
                                rectGod.anchoredPosition = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                                rectGod.name = $"{areaJson.Value.Position.x}_{areaJson.Value.Position.z}";
                                index++;
                            }
                        }
                    }
                }
            }
        }

        private RectTransform GetMapMonsterItem(int index, int monsterType)
        {
            RectTransform rectGod = GetIdleMapMonsterByIndex(index);
            if (rectGod == null)
            {
                rectGod = Instantiate(m_MonsterItem);
                rectGod.SetParent(m_miniMapMonsterRoot);
                MonsterMapList.Add(rectGod);
            }

            var ImageName = "";
            switch (monsterType)
            {
                case 0:
                    {
                        ImageName = "Map_icon_04";
                    }
                    break;
                case 1:
                    {
                        ImageName = "Map_icon_02";
                    }
                    break;
                case 2:
                    {
                        ImageName = "Map_icon_03";
                    }
                    break;
                case 3:
                    {
                        ImageName = "Map_icon_01";
                    }
                    break;
                default:
                    break;
            }

            GetSprite(ImageName, (sp) =>
            {
                if (rectGod != null)
                {
                    var Image = rectGod.GetComponent<Image>();
                    if (Image != null)
                    {
                        Image.sprite = sp;
                    }
                }
            });

            rectGod.gameObject.SetActive(true);
            rectGod.transform.localPosition = Vector3.zero;
            rectGod.transform.localScale = Vector3.one;

            return rectGod;
        }

        private RectTransform GetIdleMapMonsterByIndex(int index)
        {
            if (MonsterMapList.Count > index)
            {
                return MonsterMapList[index];
            }

            return null;
        }


        private void UpdateScaleMonsterPos(float scale, float lastScale)
        {
            for (int i = 0; i < MonsterMapList.Count; i++)
            {
                var child = MonsterMapList[i];
                if (child != null)
                {
                    //child.anchoredPosition *= scale;
                    Vector2 pos = child.anchoredPosition / lastScale * scale;
                    child.anchoredPosition = pos;
                    //string[] posArr = child.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        #region AOI实体中的怪物逻辑，在AOI范围内才会显示【显示、移动、死亡销毁】

        private void InitAoiMonster()
        {
            ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
            if (curMapType != SpaceType.SpaceSercet)
            {
                return;
            }
            List<ulong> monsterList = GameManager.Instance.MonsterList;
            if (monsterList != null && monsterList.Count != 0)
            {
                for (int i = 0; i < monsterList.Count; i++)
                {
                    AddMonsterPos(monsterList[i]);
                }
            }
        }

        private void UpdateScaleAoiMonsterPos(float scale, float lastScale)
        {
            foreach (var item in MonsterAoiList)
            {
                if (item.Value != null)
                {
                    Vector2 pos = item.Value.anchoredPosition / lastScale * scale;
                    item.Value.anchoredPosition = pos;
                }
            }
        }

        private void UpdateAoiMiniMapMonster(ulong entityId, Vector3 v3MonsterPos)
        {
            if (MonsterAoiList.ContainsKey(entityId))
            {
                Vector2 pos = GetPos(v3MonsterPos.x, v3MonsterPos.z);
                MonsterAoiList[entityId].anchoredPosition = pos;
            }
        }

        /// <summary>
        /// 字典维护，uid，image ，然后移动
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="isAdd"></param>
        private void OnMonsterAddDelete(ulong arg1, bool isAdd)
        {
            if (isAdd)
            {
                if (!MonsterAoiList.ContainsKey(arg1))
                {
                    ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
                    if (curMapType == SpaceType.SpaceSercet)
                    {
                        AddMonsterPos(arg1);
                    }
                }
            }
            else
            {
                if (MonsterAoiList.ContainsKey(arg1))
                {
                    RemoveMonster(arg1);
                }
            }
        }

        private void AddMonsterPos(ulong entityID)
        {
            // 怪物只有普通和精英的才会显示
            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(entityID);
            int monsterType = 0;
            if (entityCtrlBase != null)
            {
                // 如果是怪物，那也有可能会是主角的友方
                bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(entityCtrlBase.M_Curr);
                if (isTargetNtt)
                {
                    MonsterDataCell monsterAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)entityCtrlBase.M_Curr.ConfigIndex);
                    if (monsterAttrDataCell != null)
                    {
                        monsterType = monsterAttrDataCell.GetMonType();
                    }
                    if (monsterType == 1 || monsterType == 2)
                    {
                        var rectGod = GetAoiMonsterItem(entityCtrlBase.M_Curr.EntityId, monsterType);
                        var aoiPos = entityCtrlBase.M_Curr.Position();
                        Vector2 pos = GetPos(aoiPos.x, aoiPos.z);
                        rectGod.anchoredPosition = pos;
                    }
                }
            }
        }

        private RectTransform GetAoiMonsterItem(ulong entityID, int monsterType)
        {
            RectTransform rectGod = Instantiate(m_pvpRedItem);
            rectGod.SetParent(m_miniMapMonsterRoot);
            MonsterAoiList.Add(entityID, rectGod);
            rectGod.gameObject.SetActive(true);
            rectGod.transform.localPosition = Vector3.zero;
            rectGod.transform.localScale = Vector3.one;

            return rectGod;
        }

        private void RemoveMonster(ulong arg1)
        {
            if (MonsterAoiList.ContainsKey(arg1))
            {
                if (MonsterAoiList[arg1] != null)
                {
                    DestroyImmediate(MonsterAoiList[arg1].gameObject);
                }

                MonsterAoiList.Remove(arg1);
            }
        }


        #endregion

        private void ClearAoiMonster()
        {
            foreach (var item in MonsterAoiList)
            {
                if (item.Value != null)
                {
                    DestroyImmediate(item.Value.gameObject);
                }
            }
            m_miniMapMonsterRoot.transform.DestroyChildren();
            MonsterAoiList.Clear();
        }

        #endregion

        #region 区域名逻辑

        private void InitAreaName(SceneJsonData sceneJsonData)
        {
            if (sceneJsonData != null && sceneJsonData.Areas != null && sceneJsonData.Areas.Count > 0)
            {
                int index = 0;
                foreach (var areaJson in sceneJsonData.Areas)
                {
                    // 100 管辖区域
                    if (areaJson.Value.areaType == 100)
                    {
                        var textGod = GetAreaNameItem(index);
                        textGod.text = areaJson.Value.AreaName;
                        textGod.rectTransform.anchoredPosition =
                            GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                        textGod.transform.name = $"{areaJson.Value.Position.x}_{areaJson.Value.Position.z}";
                        index++;
                    }
                }
            }
        }

        private Text GetAreaNameItem(int index)
        {
            var textGod = GetIdleAreaNameByIndex(index);
            if (textGod == null)
            {
                textGod = Instantiate(m_AreaNameItem);
                textGod.rectTransform.SetParent(m_miniMapAreaNameRoot);
                AreaNameList.Add(textGod);
            }

            textGod.gameObject.SetActive(true);
            textGod.transform.localPosition = Vector3.zero;
            textGod.transform.localScale = Vector3.one;

            return textGod;
        }

        private Text GetIdleAreaNameByIndex(int index)
        {
            if (AreaNameList.Count > index)
            {
                return AreaNameList[index];
            }

            return null;
        }

        private void ClearAreaName()
        {
            for (int i = 0; i < AreaNameList.Count; i++)
            {
                var child = AreaNameList[i];
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateScaleAreaNamePos(float scale, float lastScale)
        {
            for (int i = 0; i < AreaNameList.Count; i++)
            {
                var child = AreaNameList[i];
                if (child != null)
                {
                    // child.rectTransform.anchoredPosition *= scale;
                    Vector2 pos = child.rectTransform.anchoredPosition / lastScale * scale;
                    child.rectTransform.anchoredPosition = pos;
                    //string[] posArr = child.rectTransform.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.rectTransform.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        #endregion

        #region 采集物

        public void OnOpenMine()
        {
            var mapJson = chanageMapSceneJsonData;
            if (mapJson == null)
            {
                mapJson = GameMap.sceneJsonData;
            }
            mGather.OnOpen(mapJson.SceneID, OnSelect, mapJson.Mines);
        }

        public void OnGatherClose()
        {
            // mSelect.SetActive(false); 
            //mUnSelect.SetActive(true);
            //ClearMines();
        }

        public void OnSelect(int mapID, int mineID)
        {
            bool active = mineID > 0;
            mSelect.SetActive(active);
            mUnSelect.SetActive(!active);
            if (active)
            {
                var mapJson = chanageMapSceneJsonData;
                if (mapJson == null)
                {
                    mapJson = GameMap.sceneJsonData;
                }
                ClearMines();
                InitMines(mineID, mapJson);
            }
            else
            {
                ClearMines();
            }

            SGF.Debuger.Log($"当前选中的地图{mapID}  当前选中的矿ID{mineID}");
        }
        private void UpdateScaleMinePos(float scale, float lastScale)
        {
            for (int i = 0; i < m_Mines.Count; i++)
            {
                var child = m_Mines[i];
                if (child != null)
                {
                    //child.anchoredPosition *= scale;
                    Vector2 pos = child.anchoredPosition / lastScale * scale;
                    child.anchoredPosition = pos;
                    //string[] posArr = child.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        private void InitMines(int mineID, SceneJsonData sceneJsonData)
        {
            if (sceneJsonData != null && sceneJsonData.Mines != null && sceneJsonData.Mines.Count > 0)
            {
                AddMine(mineID, sceneJsonData.Mines);
            }
        }

        private void AddMine(int mineID, Dictionary<long /*唯一ID*/, MineJsonData> mines)
        {
            int index = 0;
            foreach (var item in mines)
            {
                InteractDataCell mine = LocalDataManager.Instance.GetInteractDataCell(item.Value.MineID);
                if (mine == null)
                {
                    continue;
                }

                if (mine.GetMineID() == 0 || mine.GetMineID() == -1)
                {
                    continue;
                }

                if (mine.GetMineID() == mineID)
                {
                    var config = LocalDataManager.Instance.GetLifeSkillMineDataCell((int)mine.GetMineID());
                    if (config == null)
                    {
                        continue;
                    }

                    RectTransform rectGod = GetMineRectTransform(index);
                    rectGod.gameObject.SetActive(true);
                    rectGod.transform.localPosition = Vector3.zero;
                    rectGod.transform.localScale = Vector3.one;
                    rectGod.transform.name = mine.GetMineID().ToString();
                    rectGod.anchoredPosition = GetPos(item.Value.Position.x, item.Value.Position.z);

                    // 设置图片
                    Image image = rectGod.GetComponent<Image>();
                    if (image != null)
                    {
                        AtlasManager.Instance.GetSpriteAsync("ui/livingskills/atlas/livingskills", GatherSpriteName[config.GetSkillID()],
                            (sp) => { image.sprite = sp; });
                    }
                    rectGod.gameObject.SetActive(true);
                    index++;
                }
            }
        }

        private RectTransform GetMineRectTransform(int index)
        {
            if (index >= m_Mines.Count)
            {
                var go = Instantiate(m_Mine);
                go.transform.SetParent(m_MineRoot.transform);
                m_Mines.Add(go);

                return go;
            }
            else
            {
                return m_Mines[index];
            }

        }

        private void ClearMines()
        {
            for (int i = 0; i < m_Mines.Count; i++)
            {
                var child = m_Mines[i];
                if (child != null)
                {
                    child.transform.name = "0";
                    child.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        #region 切换地图逻辑

        private void OnSceneLoadSuccess(string arg0)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            curMapType = GameManager.Instance.GetCurMapType();
            GameMap gameMap = GameManager.Instance.M_Map;
            if (gameMap != null)
            {
                string name = gameMap.GetSceneLevelName(); // 获取地图名
                // 加载小地图
                string miniMapPath = gameMap.GetMiniMapPath();
                SetMapConfData(name, miniMapPath, gameMap.Size, gameMap.CurMimiMapScale);
                SetShowGuideImage(gameMap.GetMapId());
            }
        }


        private void SetShowGuideImage(int mapID)
        {
            if (mGuideImage != null)
            {
                mGuideImage.SetActive(mapID == 12);
            }

        }
        private void SetMapConfData(string mapName, string miniMapPath, Vector3 size, float curMimiMapScale)
        {
            MapName.text = mapName;

            if (miniMapPath != "")
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>(miniMapPath,
                    (Sprite img) =>
                    {
                        if (img == null)
                        {
                            LoadDefaultMapIcon();
                            return;
                        }

                        if (m_miniMapImg != null)
                        {
                            m_miniMapImg.sprite = img;
                        }
                    });
                m_miniMapMaskUpImg.gameObject.SetActive(false);
                m_miniMapMaskDownImg.gameObject.SetActive(false);

                if (curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceGuildTerritory)
                {
                    var spl = miniMapPath.Split("/");
                    if (spl.Length > 1)
                    {
                        string miniMapIconName = spl[spl.Length - 1];
                        string maskUpPath = miniMapIconName + "_Mask_Up";
                        //SGF.Debuger.LogError($"地图路径遮挡 上 path={maskUpPath}");
                        GetMiniMapMaskSprite(maskUpPath, (Sprite img) =>
                        {
                            if (img == null)
                            {
                                return;
                            }

                            if (m_miniMapMaskUpImg != null)
                            {
                                m_miniMapMaskUpImg.sprite = img;
                                m_miniMapMaskUpImg.alphaHitTestMinimumThreshold = 0.5f;
                                m_miniMapMaskUpImg.gameObject.SetActive(true);
                            }
                        });

                        string maskDownPath = miniMapIconName + "_Mask_Down";
                        //SGF.Debuger.LogError($"地图路径遮挡 下 path={maskDownPath}");
                        GetMiniMapMaskSprite(maskDownPath, (Sprite img) =>
                        {
                            if (img == null)
                            {
                                return;
                            }

                            if (m_miniMapMaskDownImg != null)
                            {
                                m_miniMapMaskDownImg.sprite = img;
                                m_miniMapMaskDownImg.alphaHitTestMinimumThreshold = 0.5f;
                                m_miniMapMaskDownImg.gameObject.SetActive(true);
                            }
                        });
                    }
                }


                // Service.Resource.ResourceManager.Instance.LoadAssetAsync<Sprite>(miniMapPath,
                // (Sprite img, int index, object obj) =>
                // {
                //     if (m_miniMapImg != null)
                //     {
                //         m_miniMapImg.sprite = img;
                //     }
                // });
                //// 加载小地图背景
                //string miniMapBgPath = miniMapPath + "_bg";
                //Service.Resource.ResourceManager.Instance.LoadAssetAsync<Sprite>(miniMapBgPath,
                //(Sprite img, int index, object obj) =>
                //{
                //    if (m_miniMapBgImg != null)
                //    {
                //        m_miniMapBgImg.sprite = img;
                //    }
                //});
            }
            else
            {
                LoadDefaultMapIcon();
            }

            CurMimiMapScaleConf = curMimiMapScale;

            // 根据地图的宽高，设置偏移量
            Vector2 sizeReality = size; // 真是尺寸
            Vector2 sizeMinimumStandard = size; // 最低标准尺寸

            if (sizeMinimumStandard.x < MapConfSzie)
            {
                sizeMinimumStandard.x = MapConfSzie;
            }

            if (sizeMinimumStandard.y < MapConfSzie)
            {
                sizeMinimumStandard.y = MapConfSzie;
            }

            m_MiniMapParaX = MiniMapConfWidth / sizeMinimumStandard.x;
            m_MiniMapParaY = MiniMapConfHeight / sizeMinimumStandard.y;

            InitUIOffsetX = sizeReality.x * m_MiniMapParaX / 2;
            InitUIOffsetY = sizeReality.y * m_MiniMapParaY / 2;
        }

        private void LoadDefaultMapIcon()
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>("ui/WorldMap/Atlas/Common_map_01",
            (Sprite img) =>
            {
                if (img != null)
                {
                    if (m_miniMapImg != null)
                    {
                        m_miniMapImg.sprite = img;
                    }
                }
            });
        }

        private void GetMiniMapMaskSprite(string SpriteName, Action<Sprite> callBack)
        {
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathMiniMapMask, SpriteName, callBack);
        }

        #endregion

        #region 场景配置文件加载完毕

        private void OnSceneMapLoadComplete(int arg0)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            curMapType = GameManager.Instance.GetCurMapType();

            ClearAreaName();
            InitAreaName(GameMap.sceneJsonData);

            ClearAoiMonster();
            bool isShowCurrencyBar = true;
            if (curMapType != ProtoMsg.SpaceType.SpaceSercet)
            {
                InitMapMonster(GameMap.sceneJsonData, curMapType);
            }
            else if (curMapType == ProtoMsg.SpaceType.SpaceSercet)
            {
                if (WorldMapModule.IsSecretBossAppear)
                {
                    InitMapMonster(GameMap.sceneJsonData, curMapType);
                }
                isShowCurrencyBar = false;
            }
            mCurrencyBarParent.gameObject.SetActive(isShowCurrencyBar);
            InitAoiMonster();

            ClearMapGVEBoss();
            int mapID = GameManager.Instance.M_Map.GetMapId();
            InitGVEBoss(mapID, GameMap.sceneJsonData);

            ClearMapNPC();
            InitNPC(GameMap.sceneJsonData);

            ClearSpaceArena();
            InitSpaceArena();

            OnSelect(mapID, GatherMapSelect.GetMapSelect(mapID));
        }

        #endregion

        #region 秘境副本BOSS显示通知

        private void OnPersonSecretBossAppearRet(bool isAppear)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            if (isAppear)
            {
                ClearAoiMonster();
                InitMapMonster(GameMap.sceneJsonData, SpaceType.SpaceSercet);
            }
        }

        #endregion

        #region 异步竞技场

        private void UpdateSpaceArenaPos(ulong entityId, Vector3 vector)
        {
            if (SpaceArenaImages.ContainsKey(entityId))
            {
                Vector2 pos = GetPos(vector.x, vector.z);
                SpaceArenaImages[entityId].anchoredPosition = pos;
            }
            else
            {
                OnSpaceArenaAddDelete(entityId, true, vector);
            }
        }

        private void InitSpaceArena()
        {
            if (GameManager.Instance.GetCurMapType() != SpaceType.SpaceArena)
            {
                return;
            }
            Vector3 pos = Vector3.zero;
            var listEntityCtrl = GameManager.Instance.M_listEntityCtrl;
            for (int i = listEntityCtrl.Count - 1; i >= 0; i--)
            {
                if (listEntityCtrl[i] != null && listEntityCtrl[i].M_Curr != null && listEntityCtrl[i].M_Curr.Data != null)
                {
                    var entityCtrl = listEntityCtrl[i].M_Curr;
                    if (entityCtrl.IsMainPlayer == false)
                    {
                        var isRobot = (entityCtrl.Data.ActMark & (1 << (int)GamePlay.GpWildBoss)) > 0;
                        if (entityCtrl.EntityType == E_EntityType.Player || listEntityCtrl[i].Data.IsRobot || isRobot)
                        {
                            pos = entityCtrl.Position();
                            OnSpaceArenaAddDelete(entityCtrl.EntityId, true, pos);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 字典维护，uid，image ，然后移动
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="isAdd"></param>
        private void OnSpaceArenaAddDelete(ulong arg1, bool isAdd, Vector3 vector3)
        {
            if (isAdd)
            {
                if (!SpaceArenaImages.ContainsKey(arg1))
                {
                    AddSpaceArena(arg1, vector3);
                }
            }
            else
            {
                if (SpaceArenaImages.ContainsKey(arg1))
                {
                    RemoveSpaceArena(arg1, vector3);
                }
            }
        }

        private void AddSpaceArena(ulong key, Vector3 pos)
        {
            RectTransform image = Instantiate(m_pvpRedItem);
            image.transform.SetParent(m_miniMapSpaceArenaRoot.transform);
            image.gameObject.SetActive(true);
            image.transform.localPosition = Vector3.zero;
            image.transform.localScale = Vector3.one;
            Vector2 poss = GetPos(pos.x, pos.z);
            image.anchoredPosition = poss;
            if (!SpaceArenaImages.ContainsKey(key))
            {
                SpaceArenaImages.Add(key, image);
            }
        }

        private void UpdateScaleSpaceArenaPos(float scale, float lastScale)
        {
            foreach (var item in SpaceArenaImages)
            {
                var child = item.Value;
                if (child != null)
                {
                    //child.anchoredPosition *= scale;
                    Vector2 pos = child.anchoredPosition / lastScale * scale;
                    child.anchoredPosition = pos;
                    //string[] posArr = child.name.Split("_");
                    //if (posArr != null && posArr.Length > 1)
                    //{
                    //    float x = float.Parse(posArr[0]);
                    //    float z = float.Parse(posArr[1]);
                    //    child.anchoredPosition = GetPos(x, z);
                    //}
                }
            }
        }

        private void RemoveSpaceArena(ulong arg1, Vector3 vector3)
        {
            if (SpaceArenaImages.ContainsKey(arg1))
            {
                if (SpaceArenaImages[arg1] != null)
                {
                    DestroyImmediate(SpaceArenaImages[arg1].gameObject);
                }

                SpaceArenaImages.Remove(arg1);
            }
        }

        private void ClearSpaceArena()
        {
            for (int i = m_miniMapSpaceArenaRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = m_miniMapSpaceArenaRoot.transform.GetChild(i);
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            SpaceArenaImages.Clear();
        }


        #endregion

        #region 通缉目标逻辑

        private void InitWantedData()
        {
            // 创建本分线和本地图的通缉实体
            Dictionary<int, Dictionary<int, ProtoMsg.WTaskPointTarInfo>> curServerMapAllWantedData = GameManager.Instance.GetCurMapWantedData();
            if (curServerMapAllWantedData == null)
            {
                return;
            }

            foreach (var item in curServerMapAllWantedData)
            {
                int pointType = item.Key;
                foreach (var item1 in item.Value)
                {
                    ProtoMsg.WTaskPointTarInfo data = item1.Value;
                    CreateWantedItem(pointType, data);
                }
            }
        }

        private void CreateWantedItem(int pointType, ProtoMsg.WTaskPointTarInfo wTaskPointTarInfo)
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.WantedTasks != null &&
                GameMap.sceneJsonData.WantedTasks.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.WantedTasks)
                {
                    if (areaJson.Value != null && areaJson.Value.Index == wTaskPointTarInfo.PointID &&
                        areaJson.Value.Type == pointType)
                    {
                        var rectGod = CreateWantedItem(pointType, wTaskPointTarInfo.PointID);
                        rectGod.anchoredPosition = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                        rectGod.name = $"{areaJson.Value.Position.x}_{areaJson.Value.Position.z}";
                        var ImageName = "";

                        if (pointType == 1)
                        {
                            ImageName = "Map_icon_04";
                        }
                        else if (pointType == 2)
                        {
                            ImageName = "Map_icon_03";

                        }
                        else if (pointType == 3)
                        {
                            ImageName = "Map_icon_02";
                        }

                        GetSprite(ImageName, (sp) =>
                        {
                            if (rectGod != null)
                            {
                                var Image = rectGod.GetComponent<Image>();
                                if (Image != null)
                                {
                                    Image.sprite = sp;
                                }
                            }
                        });

                        if (wTaskPointTarInfo.CurState == 1)
                        {
                            rectGod.gameObject.SetActive(false);
                        }

                        break;
                    }
                }
            }
        }

        private RectTransform CreateWantedItem(int pointType, int pointID)
        {
            if (!WantedDic.ContainsKey(pointType))
            {
                WantedDic[pointType] = new();
            }

            if (WantedDic[pointType].ContainsKey(pointID))
            {
                DestroyImmediate(WantedDic[pointType][pointID].gameObject);
                WantedDic[pointType].Remove(pointID);
            }

            RectTransform rectGod = Instantiate(m_WantedItem);
            rectGod.SetParent(m_miniMapWantedRoot);
            rectGod.gameObject.SetActive(true);
            rectGod.transform.localPosition = Vector3.zero;
            rectGod.transform.localScale = Vector3.one;
            WantedDic[pointType].Add(pointID, rectGod);

            return rectGod;
        }

        private void RefreshWantedEntity(int pointType, ProtoMsg.WTaskPointTarInfo wTaskPointTarInfo)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            if (wTaskPointTarInfo == null)
            {
                return;
            }

            if (WantedDic == null)
            {
                return;
            }

            RectTransform rect = null;
            if (WantedDic.TryGetValue(pointType, out var keyValuePairs))
            {
            }

            if (keyValuePairs != null)
            {
                if (keyValuePairs.TryGetValue(wTaskPointTarInfo.PointID, out rect))
                {
                }
            }

            if (rect == null)
            {
                CreateWantedItem(pointType, wTaskPointTarInfo);
            }
        }

        private void ClearMapWanted()
        {
            if (WantedDic != null)
            {
                foreach (var item in WantedDic)
                {
                    if (item.Value != null)
                    {
                        foreach (var item2 in item.Value)
                        {
                            if (item2.Value != null)
                            {
                                item2.Value.gameObject.SetActive(false);
                                DestroyImmediate(item2.Value.gameObject);
                            }
                        }
                    }
                }

                WantedDic.Clear();
            }
        }

        private void UpdateScaleWantedPos(float scale, float lastScale)
        {
            if (WantedDic != null)
            {
                foreach (var item in WantedDic)
                {
                    if (item.Value != null)
                    {
                        foreach (var item2 in item.Value)
                        {
                            if (item2.Value != null)
                            {
                                //string[] posArr = item2.Value.name.Split("_");
                                //if (posArr != null && posArr.Length > 1)
                                //{
                                //    float x = float.Parse(posArr[0]);
                                //    float z = float.Parse(posArr[1]);
                                //    item2.Value.anchoredPosition = GetPos(x, z);
                                //}
                                // item2.Value.anchoredPosition *= scale;
                                Vector2 pos = item2.Value.anchoredPosition / lastScale * scale;
                                item2.Value.anchoredPosition = pos;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 单人副本怪物进度光圈

        private void OnMiniMapShowEctypeTaskHandler(int arg0)
        {
            if (chanageMapSceneJsonData != null)
            {
                return;
            }

            OnShowEctypeTask(arg0);
        }

        private void OnShowEctypeTask(int stage)
        {
            bool isFind = false;
            if (stage != -1 && GameMap.sceneJsonData.Spawners != null && GameMap.sceneJsonData.Spawners.Count > 0)
            {
                if (GameMap.sceneJsonData.Spawners.TryGetValue(stage, out var data))
                {
                    Vector3 position = data.Position.Convert();
                    mDailyAreaRect.anchoredPosition = GetPos(position.x, position.z);
                    mDailyAreaRect.name = $"{position.x}_{position.z}";
                    float sizex = data.Range * 0.01f * m_MiniMapParaX;
                    float sizey = data.Range * 0.01f * m_MiniMapParaY;
                    mDailyAreaRect.sizeDelta = new Vector2(sizex * 2, sizey * 2);
                    isFind = true;
                }
            }

            mDailyAreaRect.gameObject.SetActive(isFind);
        }

        private void UpdateScaleEctypeTaskPos(float scale, float lastScale)
        {
            //mDailyAreaRect.anchoredPosition *= scale;
            Vector2 pos = mDailyAreaRect.anchoredPosition / lastScale * scale;
            mDailyAreaRect.anchoredPosition = pos;
            //string[] posArr = mDailyAreaRect.name.Split("_");
            //if (posArr != null && posArr.Length > 1)
            //{
            //    float x = float.Parse(posArr[0]);
            //    float z = float.Parse(posArr[1]);
            //    mDailyAreaRect.anchoredPosition = GetPos(x, z);
            //}
        }

        #endregion

        #region 组队副本怪物进度光圈

        private void OnShowEctypDailyTeam()
        {
            bool find = false;
            int areaID = 0;
            LuaModule luaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.TeamModule) as LuaModule;
            if (luaModule != null)
            {
                TeamInfo teamInfo = luaModule.GetLuaTable().GetInPath<TeamInfo>("tinfo");

                if (teamInfo != null)
                {
                    var teamDaily = teamInfo.Entry;
                    if (teamDaily != null && teamDaily.MapID == GameManager.Instance.GetCurMapId())
                    {
                        areaID = teamDaily.AreaID;
                        find = true;
                    }
                }
            }

            if (find && GameMap.sceneJsonData.Areas.TryGetValue(areaID, out var area) && area != null)
            {
                Vector3 position = area.Position.Convert();
                mDailyTeamAreaRect.anchoredPosition = GetPos(position.x, position.z);
                mDailyTeamAreaRect.name = $"{position.x}_{position.z}";

                float sizex = area.Radius * 0.01f * m_MiniMapParaX;
                float sizey = area.Radius * 0.01f * m_MiniMapParaY;
                mDailyTeamAreaRect.sizeDelta = new Vector2(sizex * 2, sizey * 2);
                mDailyTeamAreaRect.gameObject.SetActive(true);
            }
            else
            {
                mDailyTeamAreaRect.gameObject.SetActive(false);
            }
        }

        private void ClearEctypDailyTeam()
        {
            mDailyTeamAreaRect.gameObject.SetActive(false);
        }

        private void UpdateScalemDailyTeamAreaRectPos(float scale, float lastScale)
        {
            //mDailyTeamAreaRect.anchoredPosition *= scale;
            Vector2 pos = mDailyTeamAreaRect.anchoredPosition / lastScale * scale;
            mDailyTeamAreaRect.anchoredPosition = pos;
            //string[] posArr = mDailyTeamAreaRect.name.Split("_");
            //if (posArr != null && posArr.Length > 1)
            //{
            //    float x = float.Parse(posArr[0]);
            //    float z = float.Parse(posArr[1]);
            //    mDailyTeamAreaRect.anchoredPosition = GetPos(x, z);
            //}
        }

        #endregion

        #region 地图实体列表

        private void InitWMapEntityRoot(int mapID, ProtoMsg.SpaceType curMapType)
        {
            //ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
            if (curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceGuildTerritory)
            {
                //int mapID = GameManager.Instance.M_Map.GetMapId();
                m_WMapEntityRoot.SetMapShowData(mapID, OnClickEntityItem);
            }
            else
            {
                m_WMapEntityRoot.gameObject.SetActive(false);
                //HideEntityRoot();
            }
        }

        private void OnClickEntityItem(int mapID, MapShowEntity mapShowEntity)
        {
            if (mapShowEntity == null)
            {
                return;
            }

            BusinessManager.Instance.FindPathByPosition(mapID, mapShowEntity.Pos, 0.1f, null);
            UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
        }

        private void HideEntityRoot()
        {
            if (m_WMapEntityRoot == null)
            {
                return;
            }

            //m_WMapEntityRoot.HideItem();
        }

        #endregion

        #region 世界地图切换小地图显示

        private void SetShowWorldMapBtn(SpaceType spaceType)
        {
            m_WorldMapJBtn.gameObject.SetActive(spaceType == SpaceType.SpaceScene || spaceType == SpaceType.SpaceGuildTerritory);
        }

        private void OnChanageMap(SceneJsonData _SceneJsonData, int mapID)
        {
            int curMapID = GameManager.Instance.M_Map.GetMapId();
            if (mapID == curMapID)
            {
                return;
            }
            //只有主城显示该图
            chanageMapSceneJsonData = _SceneJsonData;
            curMapType = SpaceType.SpaceScene;
            GetMapConf(mapID);
            SetShowGuideImage(mapID);
            ChanageMapInitEntity(mapID);
        }

        private void GetMapConf(int mapID)
        {
            var mapCfgData = LocalDataManager.Instance.GetMapCfgData(mapID);
            if (mapCfgData == null)
            {
                SGF.Debuger.LogWarning($"地图ID没找到配置 ID={mapID}");
                return;
            }
            var mapBaseDataCell = LocalDataManager.Instance.GetMapBaseDataCell(mapCfgData.MapBaseID);
            string name = mapCfgData.MapName;
            string miniMapPath = mapBaseDataCell.MiniMap;
            Vector2 size = Vector2.zero;
            size.x = mapBaseDataCell.GetMapWidth();
            size.y = mapBaseDataCell.GetMapHeight();
            float scale = (float)mapBaseDataCell.GetMiniMapScale() / 100;

            SetMapConfData(name, miniMapPath, size, scale);
        }

        private void ChanageMapInitEntity(int mapID)
        {
            ClearMapWanted();
            ClearMapGVEBoss();
            ClearMapNPC();
            ClearAreaName();
            ClearTaskFlag();
            ClearAoiMonster();
            OnShowEctypeTask(-1);
            ClearEctypDailyTeam();
            ClearFindPath();
            m_TargetItem.gameObject.SetActive(false);
            MainRole.gameObject.SetActive(false);

            InitAreaName(chanageMapSceneJsonData);
            InitMapMonster(chanageMapSceneJsonData, SpaceType.SpaceScene);
            InitGVEBoss(mapID, chanageMapSceneJsonData);
            InitNPC(chanageMapSceneJsonData);
            FreshTasks(chanageMapSceneJsonData);
            InitWMapEntityRoot(mapID, SpaceType.SpaceScene);
            OnSelect(mapID, GatherMapSelect.GetMapSelect(mapID));
        }

        #endregion

        #region 点击事件

        // 点击世界地图
        private void OnClickWorldMapJBtn(GameObject arg0)
        {
            //UIWindow uIWindow = UIManager.Instance.OpenWindow(UIDef.WorldMapWindow, null, MainPageCommond.HideNone, false, false);
            //if (uIWindow != null)
            //{
            //    M_scrollRect.SetState(false);
            //    isOpenWorldMap = true;
            //    uIWindow.onClose += (arg) =>
            //    {
            //        isOpenWorldMap = false;
            //        M_scrollRect.SetState(true);
            //    };
            //}

            Action<UIWindow> action = (UIWindow ui) =>
            {
                if (ui != null)
                {
                    M_scrollRect.SetState(false);
                    isOpenWorldMap = true;
                    ui.onClose += (arg) =>
                    {
                        DelayInvoker.DelayInvoke(0,
                        (object[] args) =>
                        {
                            isOpenWorldMap = false;
                            M_scrollRect.SetState(true);
                        });    
                    };
                }
            };
            UIManager.Instance.OpenWindowAsync(UIDef.WorldMapWindow, action, null, MainPageCommond.HideNone, false, false);
        }

        private void OnBtnClose()
        {
            if (chanageMapSceneJsonData != null)
            {
                chanageMapSceneJsonData = null;
                OnClickWorldMapJBtn(null);
                OnSceneLoadSuccess(null); // 加载地图
                OnSceneMapLoadComplete(0); // 初始化【区域名】【NPC】【怪物】【GVEBOSS】
                InitMainPlayerData();
                InitWantedData();
                InitTask();
                curMapType = GameManager.Instance.GetCurMapType();
                InitWMapEntityRoot(GameManager.Instance.M_Map.GetMapId(), curMapType);
                OnShowEctypeTask(WorldMapModule.EctypeStage);
                OnShowEctypDailyTeam();
            }
            else
            {
                UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
            }
        }

        private void OnMapMaskBtn()
        {
            m_OtherMapFindPathState = true;
        }

        #endregion
    }
}