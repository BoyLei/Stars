using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.Skill.Utils;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.UI.StarWorld;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using Task;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

//地图会有转角度 
//不能是00因为加上封边左边要留一部分区域
//追杀目标
public class MiniMapWidget : UIWidget
{
    private CanvasGroup m_CanvasGroup; // 小地图图片节点

    public GameObject miniMapBoader;

    //public RectTransform m_miniMapBgImg;  // 地图图片
    public RectTransform miniRoot; // 小地图图片节点
    public RectTransform MainRole; // 主角节点
    public RectTransform m_miniMapNPCRoot; // NPC 父节点
    public RectTransform m_miniMapTaskRoot; // 任务标识 父节点
    public RectTransform m_miniMapMonsterRoot; // 怪物 父节点
    public RectTransform m_miniMapSpaceArenaRoot; // 异步竞技场 父节点
    public Transform m_miniMapAreaNameRoot; // 区域名 父节点
    public RectTransform m_miniMapWantedRoot; // 通缉 
    public Transform m_minMapPvpRoot;       //pvp
    public Button MapButton; // 小地图打开大地图
    public Image m_LogoImageItem; // 怪物/NPC/任务Item 
    public Text m_AreaNameItem; // 区域名Item
    public RectTransform m_WantedItem; // 通缉Item
    public RectTransform m_MonsterItem; // 怪物Item
    public RectTransform m_GVEBossItem; // GVEBoss
    public RectTransform mDailyTeamAreaRect;    // 组队本光圈
    public RectTransform mDailyAreaRect;        // 单人本光圈
    public RectTransform m_FindPathRoot; // 寻路item 父节点
    public RectTransform m_TargetItem; // 点击 父节点
    public RectTransform m_FindPathDotItem; // 寻路item
    public Text MiniMapPos; // 主角坐标
    public Text MiniMapLine; // 主角分线
    public GameObject m_CommonPos;
    public GameObject m_ChanageLinePos;
    public Transform m_ChanageLineListPoint;   // 切分线界面的点（世界坐标）

    public RectTransform m_pvpBlueItem;
    public RectTransform m_pvpRedItem;

    private WorldMapModule WorldMapModule;
    private StarWorldModule My_Module;

    private Vector3 MainPlayerAngel = Vector3.zero; // 主角角度缓存
    private float Cur_Angel = 0; //摄像机角度，小地图角度，操作角度 + 算法：如果一起改变完全映射没问题
    private Vector2 m_AnchoredV2 = Vector2.zero;
    private static Vector2 CaleNewPointV2;


    private List<Image> NpcList = new(); // NPC 缓存
    private List<RectTransform> MonsterMapList = new(); // 怪物 缓存
    private Dictionary<ulong, RectTransform> MonsterAoiList = new(); // 怪物 缓存
    private List<RectTransform> GVEBossList = new(); // GVEBOSS 缓存
    private List<Text> AreaNameList = new(); // 区域名 缓存
    private Dictionary<int, Dictionary<int, RectTransform>> WantedDic = new(); // 通缉标识节点缓存
    //应该公用小地图的数据：1，我需要动态支持增加和减少，2，我支持移动，3，清理并且用别人的数据初始化我
    private Dictionary<ulong, RectTransform> SpaceArenaImages = new();
    // 采集物
    private List<RectTransform> m_Mines = new();
    public RectTransform m_Mine;
    public Transform m_MineRoot;
    private Dictionary<int, string> GatherSpriteName = new()
    {
        {101,"Live_icon_19"},
        {102,"Live_icon_18"},
        {103,"Live_icon_17"},
    };

    //【2这里是UI坐标不是3D坐标,需映射UI坐标给4倍，因为128对应1024所以系数是8】，【3因为Canvas中Transform移动和世界是1比1，Recttransform和世界比是4：1】
    private const float MiniMapConfWidth = 512.0f; // [目前UI给的1300的图片但实际的行走区域是1024]【我们规定1024是实际行走区域】{HUD小地图特殊-UI给宽1300，我们节点设置的要除以2，但实际使用的时候512才是行走区域}
    private const float MiniMapConfHeight = 512.0f; // [目前UI给的1300的图片但实际的行走区域是1024]【我们规定1024是实际行走区域】{HUD小地图特殊-UI给高1300，我们节点设置的要除以2，但实际使用的时候512才是行走区域}
    private const float MapConfSzie = 256f; // 地图制作的规定尺寸
    private float m_MiniMapParaX = 4.0f; // 小地图图片宽MiniMapConfWidth与地图配置的X宽比例
    private float m_MiniMapParaY = 4.0f; // 小地图图片高MiniMapConfHeight与地图配置的Y高比例
    private float InitUIOffsetX = 1.0f; // 坐标初始X偏移量 地图配置的X宽 * m_MiniMapParaX / 2
    private float InitUIOffsetY = 1.0f; // 坐标初始Y偏移量 地图配置的Y高 * m_MiniMapParaY / 2
    private float CurMimiMapScale = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        m_CanvasGroup = transform.Find("MiniMapBoader").GetComponent<CanvasGroup>();
        // 告知对应地图所有服务器线
        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.MapLinesRetID, OnMapLinesMsg, this);
        MiniMapPos.transform.parent.GetComponent<JButton>().OnClick += OnClickMainPlayerPos;
        MiniMapLine.transform.parent.GetComponent<JButton>().OnClick += OnClickChanageLine;

        MapButton.onClick.AddListener(OnMapBtnClick);

        InitPool();
    }
    
    void InitPool()
    {
        for (int i = 0; i < 10; i++)
        {
            var go = GameObject.Instantiate(m_LogoImageItem, m_miniMapTaskRoot.transform, true);
            mPools.Add(go);
        }

    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 告知对应地图所有服务器线
        NetworkManager.Instance.OffMessageEnum(MsgIDEnum.MapLinesRetID, OnMapLinesMsg, this);
        MiniMapPos.transform.parent.GetComponent<JButton>().OnClick = null;
        MiniMapLine.transform.parent.GetComponent<JButton>().OnClick = null;

        MapButton.onClick.RemoveListener(OnMapBtnClick);
    }

    protected override void OnOpen(object arg = null)
    {
        base.OnOpen(arg);
        My_Module = ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
        if (My_Module == null)
        {
            return;
        }
        WorldMapModule = ModuleManager.Instance.GetModule(ModuleDef.Name.WorldMapModule) as WorldMapModule;

        My_Module.OnMainPlayerMoveAction += UpdateMainPlayerPos;
        My_Module.OnMainPlayerRotAction += UpdateMainPlayerRot;
        My_Module.OnPvpPlayerMoveAction += UpdatePvpPlayerPos;
        My_Module.OnPvpSetFaction += SetPvpPlayerFaction;
        My_Module.OnSpaceArenaMoveAction += UpdateSpaceArenaPos;
        GameManager.Instance.OnSpaceArenaDataChange += OnSpaceArenaAddDelete;

        GameManager.Instance.OnPvpAddPlayerAction += AddPvpPlayer;
        GameManager.Instance.OnPvpRemovePlayerAction += RemovePvpPlayer;

        My_Module.OnMonsterMoveAction += UpdateAoiMiniMapMonster;
        GameManager.Instance.OnMonsterDataChange += OnMonsterAddDelete;

        //My_Module.OnNpcMiniShowDataChange += InitOrFleshNPCPos;

        GlobalEvent.OnSceneLoadedBinded.AddListener(OnSceneLoadSuccess);

        GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
        GlobalEvent.TaskStageChange.AddListener(OnTaskChange);
        GlobalEvent.OnNpcCreateFinished.AddListener(OnNPCChange);
        GlobalEvent.OnNpcInterStageChange.AddListener(OnTaskNPCChange); 
        GlobalEvent.OnTaskConfigLoad.AddListener(OnTaskConfigLoad);

        GlobalEvent.OnPersonSecretBossAppearRet.AddListener(OnPersonSecretBossAppearRet);
        GlobalEvent.OnMiniMapShowEctypeTask.AddListener(OnMiniMapShowEctypeTaskHandler);
        GlobalEvent.OnGVEBossRefesh.AddListener(OnGVEBossRefeshHandler);

        GlobalEvent.onRefreshWTaskPointTarInfo.AddListener(RefreshWantedEntity);
        GlobalEvent.NPCVisiableRet.AddListener(OnNPCVisiableRet);
        GlobalEvent.RefreshAllNPCVisiableRet.AddListener(OnRefreshAllNPCVisiableRet);

        GlobalEvent.onEnterSpaceEvent.AddListener(OnEnterSpaceEvent);

        GlobalEvent.AutoBattleEvent.AddListener(OnAutoBattleEvent);

        GlobalEvent.OnXinSGUIShow.AddListener(OnXinSGUIShowCB);

        OnSceneLoadSuccess(null);

        OnSceneMapLoadComplete(0); // 初始化【区域名】【NPC】

        InitMainPlayerData();
        InitWantedData(null);
        InitTask();

        InitPvpPlayers();

        // 设置寻路点的显隐
        InitFindPathTarget();

        UnRegAllTagChangeListeners();
        RegChangeListener();

        // BusinessManager.Instance.RegisterTDEntryOpenNtfChanageAction(OnTDEntryOpenNtfChanageAction);
        StarProject.GlobalEvent.OnTeamInfoChange.AddListener(OnTDEntryOpenNtfChanageAction);
        OnShowDailyTeam();
        GlobalEvent.OnGratherSelectChange.AddListener(OnGratherSelectChange);
        OnSelect(GameManager.Instance.M_Map.GetMapId(), GatherMapSelect.GetMapSelect(GameManager.Instance.M_Map.GetMapId()));
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
        foreach (var item in mPools)
        {
            GameObject.Destroy(item);
        }
        mPools.Clear();
        My_Module.OnMainPlayerMoveAction -= UpdateMainPlayerPos;
        My_Module.OnMainPlayerRotAction -= UpdateMainPlayerRot;
        GameManager.Instance.OnSpaceArenaDataChange -= OnSpaceArenaAddDelete;

        My_Module.OnPvpPlayerMoveAction -= UpdatePvpPlayerPos;
        My_Module.OnPvpSetFaction -= SetPvpPlayerFaction;
        My_Module.OnSpaceArenaMoveAction -= UpdateSpaceArenaPos;

        GameManager.Instance.OnPvpAddPlayerAction -= AddPvpPlayer;
        GameManager.Instance.OnPvpRemovePlayerAction -= RemovePvpPlayer;

        My_Module.OnMonsterMoveAction -= UpdateAoiMiniMapMonster;
        GameManager.Instance.OnMonsterDataChange -= OnMonsterAddDelete;

        //My_Module.OnNpcMiniShowDataChange -= AddNPC;

        GlobalEvent.OnSceneLoadedBinded.RemoveListener(OnSceneLoadSuccess);
        GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);

        GlobalEvent.TaskStageChange.RemoveListener(OnTaskChange);
        GlobalEvent.OnTaskConfigLoad.RemoveListener(OnTaskConfigLoad);
        GlobalEvent.OnNpcInterStageChange.RemoveListener(OnTaskNPCChange);
        GlobalEvent.OnNpcCreateFinished.RemoveListener(OnNPCChange);
        GlobalEvent.OnPersonSecretBossAppearRet.RemoveListener(OnPersonSecretBossAppearRet);
        GlobalEvent.OnMiniMapShowEctypeTask.RemoveListener(OnMiniMapShowEctypeTaskHandler);
        GlobalEvent.OnGVEBossRefesh.RemoveListener(OnGVEBossRefeshHandler);

        GlobalEvent.onRefreshWTaskPointTarInfo.RemoveListener(RefreshWantedEntity);
        StarProject.GlobalEvent.OnTeamInfoChange.RemoveListener(OnTDEntryOpenNtfChanageAction);
        // BusinessManager.Instance.UnRegisterTDEntryOpenNtfChanageAction(OnTDEntryOpenNtfChanageAction);
        GlobalEvent.NPCVisiableRet.RemoveListener(OnNPCVisiableRet);
        GlobalEvent.RefreshAllNPCVisiableRet.RemoveListener(OnRefreshAllNPCVisiableRet);

        GlobalEvent.onEnterSpaceEvent.RemoveListener(OnEnterSpaceEvent);
        GlobalEvent.OnGratherSelectChange.RemoveListener(OnGratherSelectChange);
        UnRegAllTagChangeListeners();

        GlobalEvent.AutoBattleEvent.RemoveListener(OnAutoBattleEvent);

        GlobalEvent.OnXinSGUIShow.RemoveListener(OnXinSGUIShowCB);

        m_TargetItem.gameObject.SetActive(false);

        ClearAoiMonster();
        ClearMapWanted();
        ClearMapGVEBoss();
        ClearMapNPC();
        ClearAreaName();
        ClearTaskFlag();
        ClearPvpPlayers();
        OnShowEctypeTask(-1);
        ClearSpaceArena();
        ClearFindPath();
    }

    private Vector2 GetPos(float x, float y)
    {
        Vector2 pos = Vector2.zero;
        pos.x = (x * m_MiniMapParaX) - InitUIOffsetX;
        pos.y = (y * m_MiniMapParaY) - InitUIOffsetY;
        if (CurMimiMapScale > 1)
        {
            pos *= CurMimiMapScale;
        }
        return pos;
    }


    #region 主角逻辑

    private void InitMainPlayerData()
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
        {
            UpdateMainPlayerRot(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ServerAngles);
            UpdateMainPlayerPos(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position());

            int serverID = GameManager.Instance.GetCurServerIDShow();
            //MiniMapLine.text = $"{serverID}线";
            //MiniMapLine.text = string.Format(GameConfig.LocalStr["LineStr"], serverID);
            MiniMapLine.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LineStr"), serverID);
        }
    }

    // 更新主角【移动】
    private void UpdateMainPlayerPos(Vector3 v3PlayerPos)
    {
        //00 对应 00 :服务器要求左下角是00，所以地编也是左下角00，但是UI需要在中心点：【1需要映射逻辑给InitUIOffsetX】
        //ui 和   世界 一一对应
        //X物理正，UI负数；Z正Y负数
        //if (MiniMapPos != null)
        {
            //MiniMapPos.text = "X (" + v3PlayerPos.x.ToString("f1") + ")" + "  Y (" + v3PlayerPos.z.ToString("f1") + ")";
            //m_AnchoredV2.x = InitUIOffsetX + (v3PlayerPos.x * m_MiniMapParaX * -1); //反向挪动世界，“错的是世界，不是我”
            //m_AnchoredV2.y = InitUIOffsetY + (v3PlayerPos.z * m_MiniMapParaY * -1);
            m_AnchoredV2 = GetPos(v3PlayerPos.x, v3PlayerPos.z);
            m_AnchoredV2 *= -1;
            //MiniGround45度，对应摄像机45度，对应坐标变换45度
            //m_AnchoredV2 = CalcNewPoint(m_AnchoredV2, Vector2.zero, Cur_Angel); //这里是0度,
            miniRoot.anchoredPosition = m_AnchoredV2; //【2这里是UI坐标不是3D坐标,需映射UI坐标给4倍，因为128对应512所以系数是4】，【3因为Canvas中Transform移动和世界是1比1，Recttransform和世界比是4：1】
            MiniMapPos.text = $"{(int)v3PlayerPos.x},{(int)v3PlayerPos.z}";

            UpdateFindPathItemAction(miniRoot.localPosition * -1);
        }
    }

    // 更新主角【旋转】
    private void UpdateMainPlayerRot(float rot)
    {
        MainPlayerAngel.z = rot - 90;
        MainRole.localEulerAngles = MainPlayerAngel;
        //Cur_Angel = CameraAngel.y;
    }

    private static Vector2 CalcNewPoint(Vector2 p, Vector2 pCenter, float angle)
    {
        // calc arc 
        float l = (float)(angle * Mathf.PI / 180);

        //sin/cos value
        float cosv = (float)Mathf.Cos(l);
        float sinv = (float)Mathf.Sin(l);

        // calc new point
        float newX = (float)(((p.x - pCenter.x) * cosv) - ((p.y - pCenter.y) * sinv) + pCenter.x);
        float newY = (float)(((p.x - pCenter.x) * sinv) + ((p.y - pCenter.y) * cosv) + pCenter.y);

        CaleNewPointV2.x = (int)newX;
        CaleNewPointV2.y = (int)newY;

        return CaleNewPointV2;
    }

    #endregion

    #region 设置寻路点

    private void OnAutoBattleEvent(string eventType, object v)
    {
        if (eventType == "FindingPath")
        {
            bool isFindingPath = (bool)v;
            if (isFindingPath)
            {
                // 主角的寻路委托
                InitFindPathTarget();
            }
            else
            {
                ClearFindPath();
            }
        }
    }


    private void InitFindPathTarget()
    {
        ClearFindPath();
        bool isShow = false;
        if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
        {
            isShow = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Is_MainPlayer_FindingPath;
        }

        if (isShow)
        {
            var pathArr = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.FindPathPoints.ToList();
            Vector3 startPos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            Vector3 endPos = Vector3.zero;
            if (pathArr.Count > 0)
            {
                endPos = pathArr[pathArr.Count - 1];
            }
            else
            {
                endPos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.nextMovePoint;
            }
            if (endPos != Vector3.zero)
            {
                //pathArr.Insert(0, startPos);
                //pathArr.Add(endPos);

                m_TargetItem.transform.localPosition = GetPos(endPos.x, endPos.z);
                m_TargetItem.gameObject.SetActive(true);
            }
            InitFindPath(startPos, endPos, pathArr);
        }
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
        findPath.Add(findPathPoints[findPathPoints.Count - 1]);

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
        m_TargetItem.gameObject.SetActive(false);
    }

    private void CreatFindPathItem(Vector3 pos)
    {
        var item = Instantiate(m_FindPathDotItem);
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

    #endregion


    #region 分线

    private void OnEnterSpaceEvent(int arg0)
    {
        // 设置主角的分线
        //MiniMapLine.text = $"{arg0}线";
        //MiniMapLine.text = string.Format(GameConfig.LocalStr["LineStr"], arg0);
        MiniMapLine.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LineStr"), arg0);
    }

    #endregion

    #region 任务逻辑

    private void OnTaskConfigLoad(object arg0)
    {
        InitTask();
    }

    private void InitTask()
    {
        FreshTasks();
    }
    private void OnNPCChange(int arg0)
    {
        FreshTasks();
    }
    private void OnTaskChange(int arg0, int arg1)
    {
        FreshTasks();
    }
    private void OnTaskNPCChange(int type)
    {
        FreshTasks();
    }

    private Image GetImage()
    {
        Image go=null;
        if (mPools.Count > 0)
        {
            go = mPools[0];
            mPools.RemoveAt(0);
        }

        if (go == null)
        {
            go = GameObject.Instantiate(m_LogoImageItem);
            go.transform.SetParent(m_miniMapTaskRoot.transform);
        }
        return go;
    }
    private void CreateTaskFlag(int taskType, TaskStateEnum TaskState, Vector3 position)
    {
        string iconPath = TaskManager.GetUITaskIconNameByType((TaskClassifyType)taskType, TaskState);
        Image image =GetImage();
        GetSprite(iconPath, (sp) =>
        {
            if (image != null)
            {
                image.sprite = sp;
            }
        });
      
        image.gameObject.SetActive(true);
        image.transform.localPosition = Vector3.zero;
        image.transform.localScale = Vector3.one;
        Vector2 pos = GetPos(position.x, position.z);
        image.rectTransform.anchoredPosition = pos;
        used.Add(image);
    }

    private void FreshTasks()
    {
        //清理数据
        ClearTaskFlag();

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

                            if (GameMap.sceneJsonData == null)
                            {
                                continue;
                            }

                            if (GameMap.sceneJsonData != null && target.MapID != GameMap.sceneJsonData.SceneID)
                            {
                                continue;
                            }

                            Vector3 position = Vector3.zero;
                            bool isFind = false;
                            switch (target.FindPath.FindPathType)
                            {
                                case Task.E_FindPath.NPC:

                                    var npc = GameMap.sceneJsonData.GetNPCJsonData(target.TargetID);
                                    if (npc != null)
                                    {
                                        position = npc.Position.Convert();
                                        isFind = true;
                                    }
                                    break;
                                case Task.E_FindPath.InterAction:
                                    var mine = GameMap.sceneJsonData.GetMineJsonData(target.FindPath.ID);
                                    if (mine != null)
                                    {
                                        position = mine.Position.Convert();
                                        isFind = true;
                                    }
                                    break;
                                case Task.E_FindPath.Area:
                                    if (GameMap.sceneJsonData.Areas.ContainsKey((int)target.FindPath.ID))
                                    {
                                        position = GameMap.sceneJsonData.Areas[(int)target.FindPath.ID].Position.Convert();
                                        isFind = true;
                                    }
                                    break;
                                case Task.E_FindPath.Spawer:
                                    if (GameMap.sceneJsonData.Spawners.ContainsKey((int)target.FindPath.ID))
                                    {
                                        position = GameMap.sceneJsonData.Spawners[(int)target.FindPath.ID].Position.Convert();
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

    private List<Image> mPools = new List<Image>();
    private List<Image> used = new List<Image>();
    
    private void ClearTaskFlag()
    {
        for (int i = used.Count - 1; i >= 0; i--)
        {
            var child = used[i];
            if (child != null)
            {
                mPools.Add(child);
                child.gameObject.SetActive(false);
            }
        }
        used.Clear();
        
        /*for (int i = m_miniMapTaskRoot.transform.childCount - 1; i >= 0; i--)
        {
            var child = m_miniMapTaskRoot.transform.GetChild(i);
            if (child != null)
            {
                mPools.Add(child.GetComponent<Image>());
                child.gameObject.SetActive(false);
                //DestroyImmediate(child.gameObject);
            }
        }*/
    }

    #endregion

    #region NPC逻辑

    private void OnNPCVisiableRet(int configID, Vector3 pos, bool isShow)
    {
        Image item = GetNpcItemByID(configID);
        if (item == null && isShow == false)
        {
            // 一开始就没创建出来，还是隐藏的，就不创建了
            return;
        }
        if (item == null)
        {
            NpcDataCell npc = LocalDataManager.Instance.GetNPCDataCell(configID);
            if (npc == null)
            {
                return;
            }
            if (npc.MapLogo == null || npc.MapLogo == string.Empty || npc.MapLogo == "0")
            {
                return;
            }
            item = GetNPCItem();
            item.transform.name = configID.ToString();
            item.rectTransform.anchoredPosition = GetPos(pos.x, pos.z);
            string iconName = $"{npc.MapLogo}";
            Action<Sprite> cb = (Sprite sp) =>
            {
                if (sp != null)
                {
                    SetNpcLogo(item, sp, isShow);
                }
            };
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, cb);
            SGF.Debuger.Log($"111新解锁了一个NPC={configID},isShow={isShow}");
        }
        else
        {
            // 设置显隐
            item.gameObject.SetActive(isShow);
        }
    }

    private void SetNpcLogo(Image image, Sprite sp, bool isShow)
    {
        if (image == null)
        {
            return;
        }
        if (sp != null)
        {
            image.sprite = sp;
        }
        image.gameObject.SetActive(isShow);
    }

    private Image GetNPCItem()
    {
        Image rectGod = GetIdleNPCItem();
        if (rectGod == null)
        {
            rectGod = Instantiate(m_LogoImageItem);
            rectGod.transform.SetParent(m_miniMapNPCRoot.transform);
            NpcList.Add(rectGod);
        }

        rectGod.gameObject.SetActive(false);
        rectGod.transform.localPosition = Vector3.zero;
        rectGod.transform.localScale = Vector3.one;

        return rectGod;
    }

    private Image GetIdleNPCItem()
    {
        for (int i = 0; i < NpcList.Count; i++)
        {
            var child = NpcList[i];
            if (child != null)
            {
                if (child.gameObject.activeSelf == false)
                {
                    return child;
                }
            }
        }
        return null;
    }

    private Image GetNpcItemByID(int configID)
    {
        for (int i = 0; i < NpcList.Count; i++)
        {
            var child = NpcList[i];
            if (child != null)
            {
                int npcConfigID = 0;
                int.TryParse(child.transform.name, out npcConfigID);
                if (npcConfigID != 0 && npcConfigID == configID)
                {
                    return child;
                }
            }
        }
        return null;
    }

    private void OnRefreshAllNPCVisiableRet(object arg0)
    {
        for (int i = 0; i < NpcList.Count; i++)
        {
            var child = NpcList[i];
            if (child != null)
            {
                int npcConfigID = 0;
                int.TryParse(child.transform.name, out npcConfigID);
                if (npcConfigID != 0)
                {
                    var npcEntityCtrl = GameManager.Instance.GetNPCCtrlGroupByConfig(npcConfigID);
                    bool isShow = true;
                    if (npcEntityCtrl != null)
                    {
                        isShow = npcEntityCtrl.IsShow;
                    }
                    if (isShow != child.gameObject.activeSelf)
                    {
                        SGF.Debuger.LogWarning($"新解锁了一个NPC={npcConfigID},isShow={isShow}");
                    }
                    child.gameObject.SetActive(isShow);
                }
            }
        }
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

    #endregion

    #region 通缉目标逻辑

    private void InitWantedData(object[] args)
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
        if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.WantedTasks != null && GameMap.sceneJsonData.WantedTasks.Count > 0)
        {
            foreach (var areaJson in GameMap.sceneJsonData.WantedTasks)
            {
                if (areaJson.Value != null && areaJson.Value.Index == wTaskPointTarInfo.PointID && areaJson.Value.Type == pointType)
                {
                    var rectGod = CreateWantedItem(pointType, wTaskPointTarInfo.PointID);
                    Vector2 pos = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                    rectGod.anchoredPosition = pos;

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
                        if (rectGod != null && rectGod.rect != null)
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

    #endregion

    #region GVEBoss逻辑

    private void InitGVEBoss()
    {
        if (!StarProject.Service.Business.BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.PlayModeGVE))
            return;

        int mapID = GameManager.Instance.M_Map.GetMapId();
        var GVEStartI2S = GameManager.Instance.GVEStartI2S;

        //GVE活动中的boss
        if (GVEStartI2S != null)
        {
            bool isSameMap = false;
            foreach (var it in GVEStartI2S.MapList)
            {
                if (it.Key == mapID)
                {
                    isSameMap = true;
                }
            }
            if (isSameMap)
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "OnStartGve", new object[] { });
                //Debug.LogError("相同地图");
            }
            else
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "OnEndGve", new object[] { });
                //Debug.LogError("不同地图");
            }

            long serverNowTime = GameManager.Instance.GetServerTimeStamp() / 1000;
            long serverCloseTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.Duration;
            long serverDelayTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.BossDelay;
            long leftTime = serverCloseTime - serverNowTime;

            //在活动时间内
            if (leftTime > 0)
            {
                var bossList = GameManager.Instance.BossList;
                NeedRefesh = true;
                if (GameMap.sceneJsonData != null)
                {
                    int index = 0;
                    if (GameMap.sceneJsonData.GVE_Points != null && GameMap.sceneJsonData.GVE_Points.Count > 0)
                    {
                        foreach (var areaJson in GameMap.sceneJsonData.GVE_Points)
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
                                        int bossType = LocalDataManager.Instance.GetGVEBossType(boss.BossID, GVEStartI2S.PlayID);
                                        var rectGod = GetGVEBossItem(index, bossType);
                                        Vector2 pos = GetPos(boss.Pos.X, boss.Pos.Z);
                                        rectGod.anchoredPosition = pos;
                                        isFindboss = true;
                                        index++;
                                        break;
                                    }
                                }
                            }

                            //没boss就显示洞
                            if (!isFindboss)
                            {
                                var rectGod = GetGVEBossItem(index, 0);
                                Vector2 pos = GetPos(areaJson.Value.x, areaJson.Value.z);
                                rectGod.anchoredPosition = pos;
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

    bool NeedRefesh = false;

    //刷新boss倒计时
    private void Update()
    {
        if (NeedRefesh)
        {
            var GVEStartI2S = GameManager.Instance.GVEStartI2S;

            if (GVEStartI2S != null)
            {
                long serverNowTime = GameManager.Instance.GetServerTimeStamp() / 1000;
                long serverCloseTime = (GVEStartI2S.StartTime / 1000) + GVEStartI2S.Duration;
                long leftTime = serverCloseTime - serverNowTime;

                //活动结束
                if (leftTime <= 0)
                {
                    NeedRefesh = false;
                    ClearMapGVEBoss();
                }
            }
        }
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
        ClearMapGVEBoss();
        InitGVEBoss();
    }

    #endregion

    #region 怪物逻辑

    private void InitMapMonster()
    {
        if (GameMap.sceneJsonData != null)
        {
            int index = 0;
            ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
            if (curMapType == ProtoMsg.SpaceType.SpaceSercet || curMapType == ProtoMsg.SpaceType.SpaceTeamDaily)
            {
                if (GameMap.sceneJsonData.RandomMonsters != null && GameMap.sceneJsonData.RandomMonsters.Count > 0)
                {
                    foreach (var areaJson in GameMap.sceneJsonData.RandomMonsters)
                    {
                        if (areaJson.Value.MonsterType == 3 /*(int)MapEditor.MonsterType.Boss*/)
                        {
                            var rectGod = GetMapMonsterItem(index, areaJson.Value.MonsterType);
                            Vector2 pos = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                            rectGod.anchoredPosition = pos;
                            index++;
                        }
                    }
                }
            }
            else
            {
                if (GameMap.sceneJsonData.Monsters != null && GameMap.sceneJsonData.Monsters.Count > 0)
                {
                    foreach (var areaJson in GameMap.sceneJsonData.Monsters)
                    {
                        if (areaJson.Value.MonsterType == 3 /*(int)MapEditor.MonsterType.Boss*/)
                        {
                            var rectGod = GetMapMonsterItem(index, areaJson.Value.MonsterType);
                            Vector2 pos = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                            rectGod.anchoredPosition = pos;
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

    #endregion

    #region 区域名逻辑

    private void InitAreaName()
    {
        if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Areas != null &&
            GameMap.sceneJsonData.Areas.Count > 0)
        {
            int index = 0;
            foreach (var areaJson in GameMap.sceneJsonData.Areas)
            {
                // 100 管辖区域
                if (areaJson.Value.areaType == 100)
                {
                    var textGod = GetAreaNameItem(index);
                    textGod.text = areaJson.Value.AreaName;
                    Vector2 pos = GetPos(areaJson.Value.Position.x, areaJson.Value.Position.z);
                    textGod.rectTransform.anchoredPosition = pos;
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

    #endregion

    #region 切换地图逻辑

    private void OnSceneLoadSuccess(string arg0)
    {
        GameMap gameMap = GameManager.Instance.M_Map;
        if (gameMap != null)
        {
            // 加载小地图
            string miniMapPath = gameMap.GetMiniMapPath();
            if (miniMapPath != "")
            {
                //Debug.LogError(miniMapPath);
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>(miniMapPath,
                    (Sprite img) =>
                    {
                        if (img != null)
                        {
                            if (miniRoot != null)
                            {
                                miniRoot.transform.GetComponent<Image>().sprite = img;
                            }
                        }
                        else
                        {
                            LoadDefaultMapIcon();
                        }
                    });

                // StarProject.Service.Resource.ResourceManager.Instance.LoadAssetAsync<Sprite>(miniMapPath,
                // (Sprite img, int index, object obj) =>
                // {
                //     if (miniRoot != null)
                //     {
                //         miniRoot.transform.GetComponent<Image>().sprite = img;
                //     }
                // });
                // 加载小地图背景(HUD的小地图目前先不加底图)
                //string miniMapBgPath = miniMapPath + "_bg";
                //StarProject.Service.Resource.ResourceManager.Instance.LoadAssetAsync<Sprite>(miniMapBgPath,
                //(Sprite img, int index, object obj) =>
                //{
                //    if (m_miniMapBgImg != null)
                //    {
                //        m_miniMapBgImg.transform.GetComponent<Image>().sprite = img;
                //    }
                //});
            }
            else
            {
                LoadDefaultMapIcon();
            }

            // 根据地图的宽高，设置偏移量
            Vector2 sizeReality = Vector2.zero;// 真是尺寸
            Vector2 sizeMinimumStandard = Vector2.zero;    // 最低标准尺寸
            if (gameMap != null)
            {
                sizeReality = gameMap.Size;
                sizeMinimumStandard = gameMap.Size;
                if (sizeMinimumStandard.x < MapConfSzie)
                {
                    sizeMinimumStandard.x = MapConfSzie;
                }
                if (sizeMinimumStandard.y < MapConfSzie)
                {
                    sizeMinimumStandard.y = MapConfSzie;
                }
            }

            CurMimiMapScale = gameMap.CurMimiMapScale;

            m_MiniMapParaX = MiniMapConfWidth / sizeMinimumStandard.x;          //512/256
            m_MiniMapParaY = MiniMapConfHeight / sizeMinimumStandard.y;

            InitUIOffsetX = sizeReality.x * m_MiniMapParaX / 2;     //150*2/2
            InitUIOffsetY = sizeReality.y * m_MiniMapParaY / 2;
        }
        //ClearMonster();
    }

    private void LoadDefaultMapIcon()
    {
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>("ui/WorldMap/Atlas/Common_map_01",
        (Sprite img) =>
        {
            if (img != null)
            {
                if (miniRoot != null)
                {
                    miniRoot.transform.GetComponent<Image>().sprite = img;
                }
            }
        });
    }

    #endregion

    #region 场景配置文件加载完毕

    private void OnSceneMapLoadComplete(int arg0)
    {
        ClearAreaName();
        InitAreaName();

        ClearAoiMonster();
        ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
        if (curMapType != ProtoMsg.SpaceType.SpaceSercet)
        {
            InitMapMonster();
        }
        else if (curMapType == ProtoMsg.SpaceType.SpaceTeamDaily)
        {
            InitMapMonster();
        }
        else if (curMapType == ProtoMsg.SpaceType.SpaceSercet && WorldMapModule.IsSecretBossAppear)
        {
            InitMapMonster();
        }

        InitAoiMonster();

        ClearMapGVEBoss();
        InitGVEBoss();

        ClearMapNPC();

        ClearMapWanted();
        if (curMapType == SpaceType.SpaceScene)
        {
            DelayInvoker.DelayInvoke(1, InitWantedData);
        }
        OnShowDailyTeam();
        ClearPvpPlayers();

        ClearSpaceArena();
        InitSpaceArena();

        OnSelect(GameManager.Instance.M_Map.GetMapId(), GatherMapSelect.GetMapSelect(GameManager.Instance.M_Map.GetMapId()));

        m_CommonPos.SetActive(true);
        m_ChanageLinePos.SetActive(false);

        GlobalEvent.ShowTaskInfoPanel?.Invoke(true);

        // 特殊需求，镜像副本和新手副本关闭地图
        if (miniRoot != null)
        {
            if (curMapType == ProtoMsg.SpaceType.SpaceMirror)
            {
                UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
                miniRoot.gameObject.SetActive(false);
            }
            else
            {
                miniRoot.gameObject.SetActive(true);
            }
        }

        if (m_CanvasGroup != null)
        {
            bool isHide = false;
            if (curMapType == SpaceType.SpacePlot)
            {
                string mapSubType = GameManager.Instance.GetMapSubType();
                bool isHide2 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;

                bool isHide3 = true;
                if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                {
                    isHide3 = BusinessManager.Instance.GetXinSGStateDic("-1");
                }
                isHide = isHide2 || !isHide3;
            }
            //else if (curMapType == SpaceType.SpaceArena)
            //{
            //    isHide = true;
            //}
            m_CanvasGroup.alpha = isHide ? 0 : 1;
            m_CanvasGroup.interactable = !isHide;
            m_CanvasGroup.blocksRaycasts = !isHide;
        }
    }

    #endregion

    #region 新手关
    private void OnXinSGUIShowCB(object arg0)
    {
        if (m_CanvasGroup != null)
        {
            ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
            bool isHide = false;
            if (curMapType == SpaceType.SpacePlot)
            {
                string mapSubType = GameManager.Instance.GetMapSubType();
                bool isHide2 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;

                bool isHide3 = true;
                if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                {
                    isHide3 = BusinessManager.Instance.GetXinSGStateDic("-1");
                }
                isHide = isHide2 || !isHide3;
            }
            //else if (curMapType == SpaceType.SpaceArena)
            //{
            //    isHide = true;
            //}
            m_CanvasGroup.alpha = isHide ? 0 : 1;
            m_CanvasGroup.interactable = !isHide;
            m_CanvasGroup.blocksRaycasts = !isHide;
        }
    }

    #endregion

    #region 显示组队日常本光圈

    private void OnTDEntryOpenNtfChanageAction(object arg)
    {
        OnShowDailyTeam();
    }

    private void OnShowDailyTeam()
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
            Vector2 pos = GetPos(position.x, position.z);
            mDailyTeamAreaRect.anchoredPosition = pos;

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

    #endregion

    #region 单人副本怪物进度光圈

    private void OnMiniMapShowEctypeTaskHandler(int arg0)
    {
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
                Vector2 pos = GetPos(position.x, position.z);
                mDailyAreaRect.anchoredPosition = pos;

                float sizex = data.Range * 0.02f * m_MiniMapParaX;
                float sizey = data.Range * 0.02f * m_MiniMapParaY;
                mDailyAreaRect.sizeDelta = new Vector2(sizex * 2, sizey * 2);
                isFind = true;

                StarProject.Service.Battle.BattleManager.Instance.SetAutoBattleMovePoint(position);
                StarProject.Service.Battle.BattleManager.Instance.SetFindTargetPosFlag(true);
            }
        }

        mDailyAreaRect.gameObject.SetActive(isFind);
    }

    #endregion

    #region 秘境副本BOSS显示通知

    private void OnPersonSecretBossAppearRet(bool isAppear)
    {
        if (isAppear)
        {
            ClearAoiMonster();
            InitMapMonster();
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
        //else
        //{
        //    OnSpaceArenaAddDelete(entityId, true, vector);
        //}
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

    #region 注册监听的UI变化接口

    private void RegChangeListener()
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.Data != null)
        {
            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegChangeListener(TriggleEventUtils.FromatUITriggleKey(GameConfig.HUD_MINI_MAP), OnRegChangeListener, this.GetHashCode().ToString());
        }
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
                    miniMapBoader.SetActive(!value);
                }
                break;
            default:
                break;
        }
    }

    private void UnRegAllTagChangeListeners()
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.Data != null)
        {
            GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegAllTagChangeListeners(this.GetHashCode().ToString());
        }
    }

    #endregion

    #region 点击事件

    private void OnMapBtnClick()
    {
        //调用模式是 |_|,通常标签层之间不会互相调用。
        //需要参数自己获取，未必入口只有这里一个。 //这个是不推荐的。因为表现层次
        //UIManager.Instance.OpenWindow(UIDef.WorldMapWindow);

        //来自系统策划需求，镜像副本不要打开小地图
        if (GameManager.Instance.GetCurMapType() == SpaceType.SpaceMirror)
        {
            return;
        }

        My_Module.OpenFullMapWindow();
    }

    #endregion

    #region 切分线

    private bool m_IsMapLinesReq = false;
    private void OnClickChanageLine(GameObject arg0)
    {
        if (m_IsMapLinesReq)
        {
            return;
        }
        SpaceType spaceType = GameManager.Instance.GetCurMapType();
        if (spaceType != SpaceType.SpaceScene)
        {
            return;
        }
        /// 获取地图的全部分线信息
        MapLinesReq mapLinesMsg = new();
        mapLinesMsg.MapID = GameManager.Instance.GetCurMapId();
        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, mapLinesMsg, false);
        m_IsMapLinesReq = true;
    }

    private void OnMapLinesMsg(MessageHandleData data)
    {
        if (!m_IsMapLinesReq)
        {
            return;
        }
        m_IsMapLinesReq = false;
        MapLinesRet mapLinesRetMsg = (MapLinesRet)data.data;
        SGF.Debuger.LogWarning($"OnMapLinesMsg  mapLinesRetMsg={mapLinesRetMsg}");
        if (mapLinesRetMsg.ServerLines == null && mapLinesRetMsg.ServerLines.Count <= 0)
        {
            return;
        }

        m_CommonPos.SetActive(false);
        m_ChanageLinePos.SetActive(true);
        GlobalEvent.ShowTaskInfoPanel?.Invoke(false);
        //ChanageLineListWidget widget = (ChanageLineListWidget)UIManager.Instance.OpenWidget(UIDef.ChanageLineListWidget, true, mapLinesRetMsg, null, MainPageCommond.HideNone, false, true);
        //if (widget != null)
        //{
        //    widget.SetPos(m_ChanageLineListPoint.position);

        //    widget.onClose += (arg) =>
        //    {
        //        m_CommonPos.SetActive(true);
        //        m_ChanageLinePos.SetActive(false);
        //        GlobalEvent.ShowTaskInfoPanel?.Invoke(true);
        //    };
        //}

        Action<UIWidget> action = (UIWidget ui) =>
        {
            if (ui != null)
            {
                ChanageLineListWidget widget = (ChanageLineListWidget)ui;
                widget.SetPos(m_ChanageLineListPoint.position);
                widget.onClose += (arg) =>
                {
                    m_CommonPos.SetActive(true);
                    m_ChanageLinePos.SetActive(false);
                    GlobalEvent.ShowTaskInfoPanel?.Invoke(true);
                };
            }
        };
        UIManager.Instance.OpenWidgetAsync(UIDef.ChanageLineListWidget, action, true, mapLinesRetMsg, null, MainPageCommond.HideNone, false, true);
    }

    // 复制当前坐标
    private void OnClickMainPlayerPos(GameObject arg0)
    {

    }



    #endregion


    #region pvp逻辑
    Dictionary<ulong, RectTransform> allPvpPlayers = new();
    private void InitPvpPlayers()
    {
        foreach (var item in allPvpPlayers)
        {
            GameObject.Destroy(item.Value.gameObject);
        }
        allPvpPlayers.Clear();
    }

    private void ClearPvpPlayers()
    {
        foreach (var item in allPvpPlayers)
        {
            GameObject.Destroy(item.Value.gameObject);
        }
        allPvpPlayers.Clear();
    }

    bool IsPvpFaction(int faction)
    {
        return faction == 5 || faction == 6;
    }

    bool IsSameFaction(int faction)
    {
        var myFaction = GameManager.Instance.M_MainPlayerCtrlBase.Data.myOwnerNtt.Faction;
        return myFaction == faction;
    }

    public void SetPvpPlayerFaction(ulong eid, int faction)
    {
        if (!IsPvpFaction(faction))
        {
            return;
        }

        if (allPvpPlayers.ContainsKey(eid))
        {
            return;
        }

        RectTransform rt = null;
        if (IsSameFaction(faction))
        {
            rt = GameObject.Instantiate(m_pvpBlueItem);
        }
        else
        {
            rt = GameObject.Instantiate(m_pvpRedItem);
        }

        rt.transform.SetParent(m_minMapPvpRoot);
        rt.gameObject.SetActive(true);
        rt.transform.localPosition = Vector3.zero;
        rt.transform.localScale = Vector3.one;
        allPvpPlayers[eid] = rt;
    }

    public void AddPvpPlayer(ulong eid)
    {
        if (allPvpPlayers.ContainsKey(eid))
        {
            allPvpPlayers[eid].gameObject.SetActive(true);
            return;
        }
    }

    public void RemovePvpPlayer(ulong eid)
    {
        if (allPvpPlayers.ContainsKey(eid))
        {
            allPvpPlayers[eid].gameObject.SetActive(false);
            return;
        }
    }

    public void UpdatePvpPlayerPos(ulong eid, Vector3 v3PlayerPos)
    {
        m_AnchoredV2 = GetPos(v3PlayerPos.x, v3PlayerPos.z);
        if (allPvpPlayers.ContainsKey(eid))
        {
            allPvpPlayers[eid].anchoredPosition = m_AnchoredV2;
        }
    }

    #endregion

    #region  采集物

    private void OnGratherSelectChange(int mapID, int mineID)
    {
        if (mapID == GameManager.Instance.GetCurMapId())
        {
            OnSelect(mapID, mineID);
        }
    }

    public void OnSelect(int mapID, int mineID)
    {
        bool active = mineID > 0;
        if (active)
        {
            ClearMines();
            InitMines(mineID, GameMap.sceneJsonData);
        }
        else
        {
            ClearMines();
        }

        SGF.Debuger.Log($"当前选中的地图{mapID}  当前选中的矿ID{mineID}");
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
}