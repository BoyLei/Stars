using DG.Tweening;
using Sirenix.OdinInspector;
using SkillEditor;
using StarProject;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Game.Skill.Utils;
using StarProject.Service.AtlasManager;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色头顶信息面板：还需要头顶信息和2d，3d位置对齐管理
/// TxeMeshPro-中文图集
/// 1，等级
/// 2，名字，名字颜色
/// 3，等等状态
/// 包含血条，
/// </summary>
public class UnitPendantViewNew : MonoBehaviour
{
    private string LOG_TAG = "[UnitPendantView]";

    private Canvas m_Canvas;

    public CanvasGroup CanvasGroup;
    [LabelText("缩放节点")]
    public Transform Scale;
    [LabelText("排序节点")]
    public RectTransform Layout;
    [LabelText("箭头")]
    public Image Arrow;
    [LabelText("pvp状态")]
    public GameObject PvpState;
    [LabelText("NPC状态背景")]
    public Image NPCUIBg;
    [LabelText("NPC状态")]
    public Image NPCUI;
    [LabelText("星星内容")]
    public Transform StarContent;
    [LabelText("星星")]
    public Transform StarTemp;
    [LabelText("称号")]
    public Text Title;
    [LabelText("名称")]
    public Text Name;
    [LabelText("工会")]
    public Text Guild;
    [LabelText("称谓")]
    public Text Appellation;
    [LabelText("BUFFRoot")]
    public Transform BuffRoot;
    [LabelText("BuffItem")]
    public GameObject BuffItem;
    [LabelText("血量背景")]
    public Image HealthBg;
    [LabelText("血量选中背景")]
    public Image HealthCheckBg;
    [LabelText("血量进度条")]
    public Image HealthBar;
    [LabelText("护盾进度条原点左")]
    public Image ShieldValBarLeft;
    [LabelText("护盾进度条原点右")]
    public Image ShieldValBarRight;
    //[LabelText("血量数值")]
    //public TMP_Text HealthVal;            
    [LabelText("蓄力进度条")]
    public Image EnergyBar;
    [LabelText("怪物类型")]
    public Image MonsterTypeIcon;
    [LabelText("等级背景")]
    public Image LevelBg;
    [LabelText("等级选中背景")]
    public Image LevelCheckBg;
    [LabelText("等级")]
    public Text Level;
    [LabelText("职业量谱")]
    public Transform JobSpectralRoot;
    [LabelText("Mvp动画")]
    public GameObject MvpAnim;

    protected GameContext m_context;

    private EntityCtrlBase m_entityCtrl;
    /// <summary> 技能分发器 </summary>
    private SkillDispatcher m_SkillDispatcher;
    /// <summary> 技能实体 </summary>
    private SkillEntity m_skillEntity;

    // 血条相关
    private long m_healthSum = 100;
    private long m_healthCur = 100;

    #region 控制血条显隐
    private bool m_isBattleState = false;   // 是否在战斗状态
    private bool m_isBuffRootState = false;   // BuffRoot状态
    private bool m_isShowHealth = false;    // 是否显示血条
    #endregion

    private bool m_IsDefaultShowName = true;    // 是否默认显示名字
    private bool m_CurCheckTarget = false;  // 当前选中的
    private bool m_SpecialMapShow = false;
    private bool m_ForceNoShowHPBUFF = false;

    private void SetCurCheckTarget(bool isCheck)
    {
        m_CurCheckTarget = isCheck;

        //InitScale();
        SetBuffRootShow(((m_CurCheckTarget && m_CurSkillBuffs.Count > 0) || m_SpecialMapShow) && !m_ForceNoShowHPBUFF);
        SetShowHealthCheckBg((m_CurCheckTarget || m_SpecialMapShow) && !m_ForceNoShowHPBUFF);
        SetArrowShow(m_CurCheckTarget && !m_ForceNoShowHPBUFF);
        SetNameShow(m_IsDefaultShowName || m_CurCheckTarget || m_SpecialMapShow);
        SetNpcAppellationShow(m_IsDefaultShowName || m_CurCheckTarget || m_SpecialMapShow);
    }

    #region 蓄力进度条相关
    /// <summary> 单层蓄力的时间 </summary>
    private int energyTime;
    /// <summary> 已经蓄力的层数,可能蓄力0层</summary>
    private int energyedCount = 0;
    /// <summary> 当前蓄力的时间 </summary>
    private double curEnergyTime = 0;
    /// <summary> 最大蓄力层数 </summary>
    private int energyMaxNum = 0;
    /// <summary> 是否显示蓄力 </summary>
    private bool m_isShowEnergy = false;
    private Vector2 m_energSize = new(0f, 0.1f);
    #endregion

    #region 隐藏属性
    private bool m_isDieHide = false;   // 是否死亡隐藏
    private float m_curDieHideTime = 0; // 死亡倒计时
    private bool m_EntityFlash = false; // ----- 模型隐藏（技能效果、NPC解锁）
    private bool m_IsFlash = true; // 是否在相机范围内
    private bool m_IsPointInFrustum = false; // 是否在相机范围内
    #endregion

    private List<SkillBuff> m_CurSkillBuffs = new(); // 当前的BUFF实体
    private List<ThreeDBuffItemNew> m_CurBuffItemList = new();    // 当前在场的buffitem列表

    #region 职业量谱节点
    private JobSpectralUI m_JobSpectralUI = null;
    private string m_JobSpectralUIPath = string.Empty;
    #endregion

    private Vector2 m_lastPos = Vector2.zero;

    private Color orgColor = Color.white;


    private Tween MvpAnimDelayedCall = null;

    #region 资源异步加载回调

    private Action<Sprite> loadHpIcon;
    private Action<Sprite> loadHpBg;
    private Action<Sprite> loadHpCheckBg;
    private Action<Sprite> loadLevelBg;
    private Action<Sprite> loadLevelCheckBg;
    private Action<Sprite> loadArrowIcon;
    private Action<Sprite> loadMonsterTypeIcon;
    private Action<Sprite> loadNPCIcon;

    #endregion

    private void Awake()
    {
        m_Canvas = transform.GetComponent<Canvas>();
    }

    public void Init(EntityCtrlBase entityCtrl)
    {
        m_entityCtrl = entityCtrl;
        if (m_entityCtrl == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} Init m_entityCtrl=null");
            return;
        }
        ResetJobSoectralUI();
        m_context = GameManager.Instance.Context;
        NPCEntityBase nPCEntityBase = m_entityCtrl.M_Curr as NPCEntityBase;
        m_SkillDispatcher = nPCEntityBase.skillDispatcher;
        m_isDieHide = false;
        m_curDieHideTime = 0f;
        // 称号、工会、血量；；目前没有
        BattleManager.EntityRelationIconPath iconPath = null;
        m_IsDefaultShowName = true;
        // 05：同队友/主角   04：非好友  06：敌人 07：同工会
        switch (m_entityCtrl.M_Curr.EntityType)
        {
            case E_EntityType.None:
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.None);
                }
                break;
            case E_EntityType.Player:
                {
                    InitPlayerData(ref iconPath);
                }
                break;
            case E_EntityType.Npc:
                {
                    InitNPCData(ref iconPath);
                    // NPC判断是不是空模型
                    if (nPCEntityBase.modelDataCell != null && nPCEntityBase.modelDataCell.GetModleID() == 1)
                    {
                        // 如果是就不显示
                        InitScale(Vector3.zero);
                    }
                }
                break;
            case E_EntityType.Monster:
            case E_EntityType.GVEBoss:
            case E_EntityType.Robot:
                {
                    InitMonstrtData(ref m_IsDefaultShowName, ref iconPath);
                }
                break;
            case E_EntityType.Summon:
                {
                    InitSummonData(ref m_IsDefaultShowName, ref iconPath);
                }
                break;
            case E_EntityType.Partner:
                {
                    InitPartnerData(ref iconPath);
                }
                break;
            case E_EntityType.RoomSpace:
            case E_EntityType.BulletEntity:
                {
                    InitOtherData(ref iconPath);
                }
                break;
            case E_EntityType.Interact:
                {
                    InitInteractData(ref iconPath);
                }
                break;
            default:
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.None);
                }
                break;
        }

        if (iconPath == null)
        {
            Debug.LogError($"iconPath is null {m_entityCtrl.Data.M_EntityID}");
            return;
        }
        gameObject.name = $"{nPCEntityBase.EntityId}_{nPCEntityBase.Index}";
        //InitLevelData();
        InitLoadHpBgIcon(iconPath._hpBg, iconPath._hpBgCheck);
        InitLoadHpIcon(iconPath._hp);
        //InitLoadLevelBgIcon(levelBgPath);
        InitLoadArrowIcon(iconPath._checkArrow);

        int pvpstate = m_entityCtrl.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PVPState);
        UpdatePvpIcon(pvpstate);

        InitHpData();
        InitNameData();

        SetBuffRootShow(false, true);
        SetCurCheckTarget(BattleManager.Instance.CurAtkEntityId == m_entityCtrl.Data.M_EntityID);
        InitBindBuff();
        InitGetBuffs();

        SetTitleShow(false);
        SetLaborUnionShow(false);
        // 1.怪物头顶类型图标先关闭了
        MonsterTypeIcon.gameObject.SetActive(false);

        // 目前先隐藏伙伴的头顶星星
        //SetStarShow(m_entityCtrl.M_Curr.EntityType == E_EntityType.Partner);

        //InitTransformData();
        InitBattleState();

        transform.SetLocalScale(Vector3.one);
        transform.position = Vector3.zero;
        transform.GetComponent<RectTransform>().anchoredPosition = Vector2.one * 100000;

        // 头顶信息移动创建坑位
        //if (m_entityCtrl.Data != null && m_entityCtrl.Data.isMainPlayer)
        //{
        //    SGF.Debuger.LogError($"名字测试 主角创建 3坑位2D填充");
        //}
        WorldItemChecker.Instance.RegToRectTransPend(m_entityCtrl.Data.M_EntityID, transform.GetComponent<RectTransform>());
        SetPos(true);
    }

    internal void EnterFrame(int frameIndex)
    {
        #region 死亡
        if (m_isDieHide && m_curDieHideTime > 0)
        {
            m_curDieHideTime -= Time.deltaTime;
            if (m_curDieHideTime <= 0)
            {
                m_isDieHide = false;
                SetFlash(false);
            }
        }
        #endregion

        #region 蓄力进度
        if (m_isShowEnergy && m_skillEntity != null)
        {
            curEnergyTime = m_skillEntity.GetCurEnergyTime(true);
            energyedCount = m_skillEntity.GetCurEnergyCount(true);

            UpdatErnergy(Mathf.Ceil((float)curEnergyTime % energyTime));
        }
        #endregion
    }

    private void LateUpdate()
    {
        #region 跟随实体移动
        SetPos(false);
        #endregion
    }

    public void Release()
    {
        if (m_entityCtrl != null)
        {
            if (m_entityCtrl.Data.isMainPlayer)
            {
                //m_SkillDispatcher.SkillController.ActionOnEnergyStart -= OnActionOnEnergyStart;
                //m_SkillDispatcher.SkillController.ActionOnEnergyEnd -= OnActionOnEnergyEnd;
                //m_SkillDispatcher.SkillController.ActionOnEndSkillStage -= OnActionOnEndSkillStage;
            }

            if (m_entityCtrl.M_Curr.EntityType == E_EntityType.Npc)
            {
                GlobalEvent.OnTaskNpcChange.RemoveListener(OnTaskNpcChange);
            }
        }
        ResetJobSoectralUI();
        Title.text = "";
        SetTitleShow(false);
        Guild.text = "";
        SetLaborUnionShow(false);
        SetHealthValShow(false, false);
        Name.text = "";
        SetNameShow(false);
        SetEnergyValShow(false);
        SetStarShow(false);
        NPCUIBg.gameObject.SetActive(false);
        SetNpcUIShow(false, "");
        Appellation.text = "";
        SetNpcAppellationShow(false);
        SetCheckActive(false);
        MvpAnim.SetActive(false);
        ReleaseBindBuff();
        ReleaseBuffItem();
        ReleaseBuffList();
        ReleaseBuffSpTimeShowDic();
        ReleaseBuffShieldDic();

        if (MvpAnimDelayedCall != null)
        {
            MvpAnimDelayedCall.Kill(true);
        }

        m_EntityFlash = false;
        m_IsFlash = true;
        m_IsPointInFrustum = false;
        loadHpIcon = null;
        loadHpBg = null;
        loadHpCheckBg = null;
        loadLevelBg = null;
        loadLevelCheckBg = null;
        loadArrowIcon = null;
        loadMonsterTypeIcon = null;
        loadNPCIcon = null;
        m_ForceNoShowHPBUFF = false;
        m_SpecialMapShow = false;
    }

    private void ResetJobSoectralUI()
    {
        if (m_JobSpectralUI != null)
        {
            m_JobSpectralUI.Release();
            StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_JobSpectralUIPath, m_JobSpectralUI.gameObject);
        }
        m_JobSpectralUI = null;
        m_JobSpectralUIPath = string.Empty;
    }

    #region 设置模型的高度

    private float m_modelHeight = 2f;
    public void SetY(float y, bool isRefresh = false)
    {
        m_modelHeight = y;
        if (isRefresh)
        {
            SetPos(true);
        }
        //Scale.SetLocalPositionY(y);
    }

    #endregion

    #region 初始化信息

    private void InitPlayerData(ref BattleManager.EntityRelationIconPath iconPath)
    {
        Title.text = "称号五个字";
        // 可被设置为显示/隐藏 状态
        Guild.text = "[工会名称]·职位";

        int nameCfgID = 1201;    // 默认是[陌生人](名字)颜色

        // 角色名不可主动设置成隐藏
        // 角色名颜色和字体等效果可能会根据PK状态进行改变
        if (m_entityCtrl.Data.isMainPlayer)
        {
            //m_SkillDispatcher.SkillController.ActionOnEnergyStart += OnActionOnEnergyStart;
            //m_SkillDispatcher.SkillController.ActionOnEnergyEnd += OnActionOnEnergyEnd;
            //m_SkillDispatcher.SkillController.ActionOnEndSkillStage += OnActionOnEndSkillStage;
            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerMain);
            nameCfgID = 1001; // 主角
        }
        else
        {
            // 人实体也要判断是否是相同阵营的
            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
            if (isTargetNtt)
            {
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerOtherHostility);
                nameCfgID = 1101; // 敌人
            }
            else
            {
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerOtherFriendly);
                bool isMyTeammate = GameManager.Instance.IsPlayerInTeam(m_entityCtrl.Data.M_EntityID);
                if (isMyTeammate)
                {
                    nameCfgID = 1002; // 我的队友
                }
                else
                {
                    nameCfgID = 1201; // 陌生人
                }
            }
        }

        SetNameColor(nameCfgID);
        SetGuildColor();

        // 创建量谱节点
        uint job = m_entityCtrl.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
        if (job != 0)
        {
            var cfg = LocalDataManager.Instance.GetJobDataCell((int)job);
            m_JobSpectralUIPath = $"ui/StarWorld/Prefab/JobSpectralUI{cfg.GetBaseJob()}";
            StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(m_JobSpectralUIPath, (go) =>
            {
                if (go != null)
                {
                    if (m_entityCtrl != null)
                    {
                        go.transform.SetParent(JobSpectralRoot, false);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                        m_JobSpectralUI = go.GetComponent<JobSpectralUI>();
                        if (m_JobSpectralUI != null)
                        {
                            m_JobSpectralUI.Init(m_entityCtrl);
                            m_JobSpectralUI.gameObject.SetActive(true);
                            m_JobSpectralUI.SetShow(m_isShowHealth);
                        }
                    }
                    else
                    {
                        StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_JobSpectralUIPath, go);
                    }
                }
            });
        }
    }

    private void InitNPCData(ref BattleManager.EntityRelationIconPath iconPath)
    {
        // 查询是否显示NPC对话UI
        GlobalEvent.OnTaskNpcChange.AddListener(OnTaskNpcChange);
        uint taskID = 0;
        TaskNpc.StateEnum stateEnum = TaskHelper.QueryNpcTaskEnum((int)m_entityCtrl.M_Curr.ConfigIndex, ref taskID);
        NPCUIBg.gameObject.SetActive(false);
        OnTaskNpcChange((int)m_entityCtrl.M_Curr.ConfigIndex, taskID, 0, stateEnum);
        int nameCfgID = 7001;    // 默认是[普通npc](名字)颜色
        int appellationCfgID = 7102;    // 默认是[功能npc](称号/副标题)颜色
        int npcType = 1;

        // 显示称谓
        string _Appellation = m_entityCtrl.M_Curr.M_Appellation;
        if (string.IsNullOrEmpty(_Appellation))
        {
            NpcDataCell npcCfg = LocalDataManager.Instance.GetNPCDataCell(m_entityCtrl.M_Curr.ConfigIndex);
            if (npcCfg != null)
            {
                _Appellation = npcCfg.Title;
                npcType = npcCfg.GetNpcType();
            }
        }

        switch (npcType)
        {
            case 1:
                {
                    nameCfgID = 7001;   // [普通npc](名字)颜色
                }
                break;
            case 2:
                {
                    nameCfgID = 7101;   // [普通npc](名字)颜色
                    appellationCfgID = 7102;    // [功能npc](称号/副标题)颜色
                }
                break;
            case 3:
                {
                    nameCfgID = 7201;   // [普通npc](名字)颜色
                    appellationCfgID = 7202;    // [功能npc](称号/副标题)颜色
                }
                break;
            default:
                break;
        }

        //SetAppellation(_Appellation, GameConfig.COLOR_NAME_NPC);

        SetNameColor(nameCfgID);
        SetAppellation(_Appellation, appellationCfgID);

        SetNpcAppellationShow(true);
        // 改变颜色
        //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_NPC, GameConfig.COLOR_NAME_TEAM);
        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.NPC);
    }

    private void InitMonstrtData(ref bool isShowName, ref BattleManager.EntityRelationIconPath iconPath)
    {
        isShowName = false;
        // 如果是怪物，那也有可能会是主角的友方
        bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
        //string colorStr = GameConfig.COLOR_NAME_ENEMY;
        int nameCfgID = 4001;    // 默认是[普通][敌人](名字)颜色
        int appellationCfgID = 4002;    // 默认是[普通][敌人](称号/副标题)颜色
        //bool isShow = true;
        string _Appellation = m_entityCtrl.M_Curr.M_Appellation;
        if (m_entityCtrl.M_Curr.Data.IsRobot)
        {
            if (isTargetNtt)
            {
                // 敌方机器人
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterBoss);
                //colorStr = GameConfig.COLOR_NAME_ENEMY2;
                nameCfgID = 1101;// 敌人
            }
            else
            {
                // 友方机器人
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterOtherFriendly);
                //colorStr = GameConfig.COLOR_NAME_FRIEND;
                nameCfgID = 1201;// 陌生人
                // 2.友善怪有别名直接显示
                string name = m_entityCtrl.M_Curr.Data.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Alias);
                if (!string.IsNullOrEmpty(name))
                {
                    string[] names = name.Split('@');
                    if (names.Length > 1)
                    {
                        name = string.Format(LanguageManager.Instance.GetLanguageByKey(names[1]), names[0]);
                    }
                    isShowName = true;
                }
            }
            //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, colorStr, GameConfig.COLOR_NAME_TEAM);
        }
        else
        {
            MonsterDataCell monsterAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)m_entityCtrl.M_Curr.ConfigIndex);
            if (monsterAttrDataCell != null)
            {
                int monsterType = monsterAttrDataCell.GetMonType();
                bool isPassivityMonster = false;    // 是否被动攻击怪物
                AITemplateDataCell aITemplateDataCell = LocalDataManager.Instance.GetAITemplateDataCell(monsterAttrDataCell.GetAiindex());
                if (aITemplateDataCell != null)
                {
                    isPassivityMonster = aITemplateDataCell.GetScanEnemyRange() <= 0;
                }
                //iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerOtherFriendly);
                bool isMyFriend = SkillUtils.GetEntityIsMainPlayerFriendly(m_entityCtrl.M_Curr, SelectType.Friend);
                if (isMyFriend)
                {
                    nameCfgID = 1002; // 我的队友
                    appellationCfgID = 1002;
                }
                else
                {
                    //nameCfgID = 1201; // 陌生人
                    string monsterTypeIconName = string.Empty;
                    switch (monsterType)
                    {
                        case 1:
                            {
                                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterNormal);
                                monsterTypeIconName = "Hud_Icon_Blame01";
                                if (isTargetNtt)
                                {
                                    if (isPassivityMonster)
                                    {
                                        nameCfgID = 4101; // 普通 被动怪物
                                        appellationCfgID = 4102;
                                    }
                                    else
                                    {
                                        nameCfgID = 4001; // 普通 主动怪物
                                        appellationCfgID = 4002;
                                    }
                                    string mapSubType = GameManager.Instance.GetMapSubType();
                                    m_SpecialMapShow = mapSubType == GameConfig.INSTANCE_PLOT_SECOND;
                                }
                                else
                                {
                                    nameCfgID = 4201; // 普通 友善怪物
                                    appellationCfgID = 4202;
                                }
                            }
                            break;
                        case 2:
                            {
                                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterElite);
                                monsterTypeIconName = "Hud_Icon_Blame02";
                                if (isTargetNtt)
                                {
                                    if (isPassivityMonster)
                                    {
                                        nameCfgID = 5101; // 精英 被动怪物
                                        appellationCfgID = 5102;
                                    }
                                    else
                                    {
                                        nameCfgID = 5001; // 精英 主动怪物
                                        appellationCfgID = 5002;
                                    }
                                    m_SpecialMapShow = true;
                                }
                                else
                                {
                                    nameCfgID = 5201; // 精英 友善怪物
                                    appellationCfgID = 5202;
                                }
                            }
                            break;
                        case 3:
                            {
                                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterBoss);
                                monsterTypeIconName = "Hud_Icon_Blame03";
                                m_ForceNoShowHPBUFF = true;
                                //isShow = false;
                                if (isTargetNtt)
                                {
                                    if (isPassivityMonster)
                                    {
                                        nameCfgID = 6101; // BOSS 被动怪物
                                        appellationCfgID = 6102;
                                    }
                                    else
                                    {
                                        nameCfgID = 6001; // BOSS 主动怪物
                                        appellationCfgID = 6002;
                                    }
                                }
                                else
                                {
                                    nameCfgID = 6201; // BOSS 友善怪物
                                    appellationCfgID = 6202;
                                }

                            }
                            break;
                        default:
                            {
                                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterNormal);
                                if (isTargetNtt)
                                {
                                    if (isPassivityMonster)
                                    {
                                        nameCfgID = 4101; // 普通 被动怪物
                                        appellationCfgID = 4102;
                                    }
                                    else
                                    {
                                        nameCfgID = 4001; // 普通 主动怪物
                                        appellationCfgID = 4002;
                                    }
                                }
                                else
                                {
                                    nameCfgID = 4201; // 普通 友善怪物
                                    appellationCfgID = 4202;
                                }
                            }
                            break;
                    }
                    InitLoadMonsterTypeIcon(monsterTypeIconName);
                }
                //if (monsterAttrDataCell.GetNameColor() > 0)
                //{
                //    nameCfgID = monsterAttrDataCell.GetNameColor();
                //    //colorStr = GameConfig.COLOR_NAME_ENEMY2;
                //}
                if (!isTargetNtt)
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterOtherFriendly);
                }
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, colorStr, GameConfig.COLOR_NAME_TEAM);
                // 显示称谓
                if (string.IsNullOrEmpty(_Appellation))
                {
                    _Appellation = monsterAttrDataCell.Appellation;
                }
                // 1.不是BOSS怪都显示名字
                isShowName = monsterAttrDataCell.GetMonType() > 1;
                // 2.友善怪有别名直接显示
                if (isShowName == false && !isTargetNtt)
                {
                    string name = m_entityCtrl.M_Curr.Data.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Alias);
                    if (!string.IsNullOrEmpty(name))
                    {

                        string[] names = name.Split('@');
                        if (names.Length > 1)
                        {
                            name = string.Format(LanguageManager.Instance.GetLanguageByKey(names[1]), names[0]);
                        }
                        isShowName = true;
                    }
                }
            }
        }
        SetNameColor(nameCfgID);
        SetAppellation(_Appellation, appellationCfgID);
        //InitScale(isShow ? Vector3.one : Vector3.zero);
    }

    private void InitSummonData(ref bool isShowName, ref BattleManager.EntityRelationIconPath iconPath)
    {
        isShowName = false;

        int nameCfgID = 3301;    // 默认是 陌生人的召唤物
        //int appellationCfgID = 7102;    // 默认是[功能npc](称号/副标题)颜色
        if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
        {
            // 主角的召唤物
            //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
            nameCfgID = 3001;   // 主角自己的召唤物
            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonMainPlayer);
        }
        else
        {
            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
            if (isTargetNtt)
            {
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_ENEMY, GameConfig.COLOR_NAME_TEAM);
                nameCfgID = 3101;   // 敌方的召唤物
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherHostility);
            }
            else
            {
                ulong SummonHostID = m_entityCtrl.M_Curr.SummonHostID;
                if (SummonHostID != 0)
                {
                    bool isMyTeammate = GameManager.Instance.IsPlayerInTeam(SummonHostID);
                    if (isMyTeammate)
                    {
                        nameCfgID = 3002;   // 组队队友的召唤物
                    }
                    else
                    {
                        nameCfgID = 3301;   // 陌生人的召唤物
                    }
                }
                else
                {
                    nameCfgID = 3301;   // 陌生人的召唤物
                }
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherFriendly);
            }
        }
        SetNameColor(nameCfgID);
    }

    private void InitPartnerData(ref BattleManager.EntityRelationIconPath iconPath)
    {
        // 伙伴打开星星
        //InitParterStar();

        int nameCfgID = 2001;    // 默认是 自己的佣兵
        if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
        {
            //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
            nameCfgID = 2001;    // 自己的佣兵
            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerMainPlayer);
        }
        else
        {
            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
            if (isTargetNtt)
            {
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_ENEMY, GameConfig.COLOR_NAME_TEAM);
                nameCfgID = 2101;    // 敌方的佣兵
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerOtherHostility);
            }
            else
            {
                ulong SummonHostID = m_entityCtrl.M_Curr.SummonHostID;
                if (SummonHostID != 0)
                {
                    bool isMyTeammate = GameManager.Instance.IsPlayerInTeam(SummonHostID);
                    if (isMyTeammate)
                    {
                        nameCfgID = 2002;   // 组队队友的佣兵
                    }
                    else
                    {
                        nameCfgID = 2201;   // 陌生人的佣兵
                    }
                }
                else
                {
                    nameCfgID = 2201;   // 陌生人的佣兵
                }
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerOtherFriendly);
            }
        }
        SetNameColor(nameCfgID);
    }

    private void InitInteractData(ref BattleManager.EntityRelationIconPath iconPath)
    {
        int nameCfgID = 7001;    // 默认是[普通npc](名字)颜色
        int appellationCfgID = 7102;    // 默认是[功能npc](称号/副标题)颜色
        int npcType = 1;
        bool isHaveMineID = false;
        // 显示称谓
        string _Appellation = m_entityCtrl.M_Curr.M_Appellation;
        if (string.IsNullOrEmpty(_Appellation))
        {
            InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell((long)m_entityCtrl.M_Curr.ConfigIndex);
            if (interactDataCell != null)
            {
                _Appellation = interactDataCell.Title;
                npcType = interactDataCell.GetInterType();
                isHaveMineID = interactDataCell.GetMineID() > 0;
            }
        }

        switch (npcType)
        {
            case 1:
                {
                    nameCfgID = 7001;   // [普通npc](名字)颜色
                }
                break;
            case 2:
                {
                    nameCfgID = 7101;   // [功能npc](名字)颜色
                    appellationCfgID = 7102;    // [功能npc](称号/副标题)颜色
                }
                break;
            case 3:
                {
                    nameCfgID = 7201;   // [玩法npc](名字)颜色
                    appellationCfgID = 7202;    // [玩法npc](称号/副标题)颜色
                }
                break;
            default:
                break;
        }

        if (isHaveMineID)
        {
            nameCfgID = 8001;   // 采集物名称
        }

        SetNameColor(nameCfgID);
        SetAppellation(_Appellation, appellationCfgID);

        SetNpcAppellationShow(true);
        // 改变颜色
        //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_NPC, GameConfig.COLOR_NAME_TEAM);
        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.NPC);
    }

    private void InitOtherData(ref BattleManager.EntityRelationIconPath iconPath)
    {
        int nameCfgID = 3301;    // 默认是 陌生人的召唤物
        //int appellationCfgID = 7102;    // 默认是[功能npc](称号/副标题)颜色
        if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
        {
            // 主角的召唤物
            //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
            nameCfgID = 3001;   // 主角自己的召唤物
            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonMainPlayer);
        }
        else
        {
            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
            if (isTargetNtt)
            {
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_ENEMY, GameConfig.COLOR_NAME_TEAM);
                nameCfgID = 3101;   // 敌方的召唤物
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherHostility);
            }
            else
            {
                ulong SummonHostID = m_entityCtrl.M_Curr.SummonHostID;
                if (SummonHostID != 0)
                {
                    bool isMyTeammate = GameManager.Instance.IsPlayerInTeam(SummonHostID);
                    if (isMyTeammate)
                    {
                        nameCfgID = 3002;   // 组队队友的召唤物
                    }
                    else
                    {
                        nameCfgID = 3301;   // 陌生人的召唤物
                    }
                }
                else
                {
                    nameCfgID = 3301;   // 陌生人的召唤物
                }
                //SetBaseTextColor(GameConfig.COLOR_TITLE_TEAM, GameConfig.COLOR_NAME_FRIEND, GameConfig.COLOR_NAME_TEAM);
                iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherFriendly);
            }
        }
        SetNameColor(nameCfgID);
    }

    private void InitBattleState()
    {
        uint value = m_entityCtrl.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.State);
        UpdateState(value);
    }

    private void InitHpData()
    {
        // 1.主角不显示血条 2.不在战斗状态不显示血条
        m_healthSum = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);
        m_healthCur = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
        UpdateHealth(m_healthCur);
        SetHealthValShow(m_SpecialMapShow, false);
    }

    private void InitLoadHpIcon(string healthBgIconName)
    {
        // 主角-》敌对-》队伍-》工会-》非好友
        // 根据加载血条
        loadHpIcon = (Sprite sp) =>
        {
            if (HealthBar != null && sp != null)
            {
                HealthBar.sprite = sp;
                HealthBar.SetNativeSize();
                ShieldValBarLeft.rectTransform.SetWidth(HealthBar.rectTransform.rect.width);
                ShieldValBarLeft.rectTransform.SetHeight(HealthBar.rectTransform.rect.height);
                ShieldValBarRight.rectTransform.SetWidth(HealthBar.rectTransform.rect.width);
                ShieldValBarRight.rectTransform.SetHeight(HealthBar.rectTransform.rect.height);
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, healthBgIconName, loadHpIcon);
    }

    private void InitLoadHpBgIcon(string healthBgIconName, string healthCheckBgIconPath)
    {
        loadHpBg = (Sprite sp) =>
        {
            if (HealthBg != null && sp != null)
            {
                HealthBg.sprite = sp;
                HealthBg.SetNativeSize();
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, healthBgIconName, loadHpBg);

        loadHpCheckBg = (Sprite sp) =>
        {
            if (HealthCheckBg != null && sp != null)
            {
                HealthCheckBg.sprite = sp;
                HealthCheckBg.SetNativeSize();
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, healthCheckBgIconPath, loadHpCheckBg);
    }

    private void InitLoadLevelBgIcon(string iconName)
    {
        loadLevelBg = (Sprite sp) =>
        {
            if (LevelBg != null && sp != null)
            {
                LevelBg.sprite = sp;
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, loadLevelBg);

        loadLevelCheckBg = (Sprite sp) =>
        {
            if (LevelCheckBg != null && sp != null)
            {
                LevelCheckBg.sprite = sp;
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName + "s", loadLevelCheckBg);
    }

    private void InitLoadArrowIcon(string arrowIconPath)
    {
        loadArrowIcon = (Sprite sp) =>
        {
            if (Arrow != null && sp != null)
            {
                Arrow.sprite = sp;
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, arrowIconPath, loadArrowIcon);
    }

    public void UpdatePvpIcon(int pvpstate)
    {
        PvpState.gameObject.SetActive(pvpstate != 0);
        for (int i = 0; i < PvpState.transform.childCount; i++)
        {
            PvpState.transform.GetChild(i).gameObject.SetActive(false);
        }
        if (pvpstate == (int)ProtoMsg.PVPRoleState.PvpBz)
        {
            PvpState.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (pvpstate == (int)ProtoMsg.PVPRoleState.PvpDsts)
        {
            PvpState.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (pvpstate == (int)ProtoMsg.PVPRoleState.PvpWrnd)
        {
            PvpState.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (pvpstate == (int)ProtoMsg.PVPRoleState.PvpZzbs)
        {
            PvpState.transform.GetChild(3).gameObject.SetActive(true);
        }
        else if (pvpstate == (int)ProtoMsg.PVPRoleState.PvpCs)
        {
            PvpState.transform.GetChild(4).gameObject.SetActive(true);
        }
    }

    private void InitLoadMonsterTypeIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            MonsterTypeIcon.gameObject.SetActive(false);
            return;
        }

        loadMonsterTypeIcon = (Sprite sp) =>
        {
            if (MonsterTypeIcon != null && sp != null)
            {
                MonsterTypeIcon.sprite = sp;
                //MonsterTypeIcon.gameObject.SetActive(true);
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, loadMonsterTypeIcon);
    }

    private void InitScale(Vector3 scale)
    {
        Scale.SetLocalScale(scale);
    }

    private void InitLevelData()
    {
        string attrLevelName = m_entityCtrl.M_Curr.EntityType == E_EntityType.Player ? AOIAttrDefine.PlayerLevel : AOIAttrDefine.EntityLevel;
        int level = m_entityCtrl.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, attrLevelName);
        UpdateLevel(level);
    }

    private bool IsPvpFaction()
    {
        int factioin = m_entityCtrl.M_Curr.Data.myOwnerNtt.Faction;
        return factioin == 5 || factioin == 6;
    }

    private bool IsSameFaction()
    {
        var myFaction = GameManager.Instance.M_MainPlayerCtrlBase.Data.myOwnerNtt.Faction;
        int factioin = m_entityCtrl.M_Curr.Data.myOwnerNtt.Faction;
        return myFaction == factioin;
    }

    private void InitNameData()
    {
        //组队，队伍名称
        //string ShowCaptainName = m_entityCtrl.Data.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.ShowCaptainName);
        //if (!string.IsNullOrEmpty(ShowCaptainName))
        //{
        //    m_name = ShowCaptainName;
        //}
        // 加实体id的后4位
        //{
        //    string idstr = m_entityCtrl.Data.M_EntityID.ToString();
        //    int attackerIdLength = idstr.Length;
        //    if (attackerIdLength > 4)
        //    {
        //        idstr = idstr.Substring(attackerIdLength - 4);
        //    }
        //    m_name += $"_{idstr}";
        //}

        Name.text = m_entityCtrl.M_Curr.M_Name;

        if (IsPvpFaction())
        {
            if (IsSameFaction())
            {
                // 同阵营
                SetNameTextColor(/*"蓝方_" + */m_entityCtrl.M_Curr.M_Name, 1003);
            }
            else
            {
                // 敌对阵营
                SetNameTextColor(/*"红方_" + */m_entityCtrl.M_Curr.M_Name, 1103);
            }
        }
    }

    private void InitBindBuff()
    {
        if (m_SkillDispatcher == null || m_SkillDispatcher.SkillController == null)
        {
            return;
        }
        m_SkillDispatcher.SkillController.ActionOnBuffCreate += OnActionOnBuffCreate;
        m_SkillDispatcher.SkillController.ActionOnBuffEnd += OnActionOnBuffEnd;
        m_SkillDispatcher.SkillController.ActionOnBuffUpdate += OnActionOnBuffUpdate;

    }

    private void InitGetBuffs()
    {
        List<SkillBuff> buffs = (m_entityCtrl.M_Curr as NPCEntityBase).GetSkillBuffs();
        for (int i = 0; i < buffs.Count - 1; i++)
        {
            var child = buffs[i];
            if (child != null && child.BuffInfo != null && child.BuffInfo.Cfg != null)
            {
                bool isShowThreeD = child.BuffInfo.GetHaveBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.ThreeDHPUp);
                if (isShowThreeD)
                {
                    AddBuffItem(child);
                }
                else
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} InitGetBuffs() buffid={child.BuffInfo.BuffID} buff已经存在了");
                }
                // 查看女枪炮的标识并记录
                CheckBuffTag(child, false);
                // 获取BUFF护盾总数
                CheckBuffShieldVal(child, true);
            }
        }
        SetShieldValSum();
    }

    private void ReleaseBindBuff()
    {
        if (m_SkillDispatcher == null || m_SkillDispatcher.SkillController == null)
        {
            return;
        }
        m_SkillDispatcher.SkillController.ActionOnBuffCreate -= OnActionOnBuffCreate;
        m_SkillDispatcher.SkillController.ActionOnBuffEnd -= OnActionOnBuffEnd;
        m_SkillDispatcher.SkillController.ActionOnBuffUpdate -= OnActionOnBuffUpdate;

    }

    public void InitTransformData()
    {
        //// 模型放大缩小的倍数!=1就，响应修改节点的缩放
        //float scaleCfg = m_entityCtrl.M_Curr.ModleScale;
        //if (scaleCfg != 1)
        //{
        //    float scaleCur = 1 - ((scaleCfg - 1) / scaleCfg);
        //    transform.SetLocalScale(new Vector3(scaleCur, scaleCur, scaleCur));
        //}
        //// 返回原点本地坐标
        //transform.localPosition = Vector3.zero;
    }

    #endregion

    #region 【委托】【技能】
    /// <summary>
    /// 服务器通知--怪物、其他人的使用蓄力技能
    /// </summary>
    /// <param name="skillUnit"></param>
    private void OnActionOnEnergyStart(SkillEntity skillEntity)
    {
        SetEenergy(skillEntity);
    }
    /// <summary>
    /// 蓄力技能结束
    /// </summary>
    /// <param name="num">层数</param>
    private void OnActionOnEnergyEnd(SkillEntity skillEntity)
    {
        if (m_skillEntity != null && skillEntity != null && skillEntity.RuntimeID == m_skillEntity.RuntimeID)
        {
            SetEnergyValShow(false);
        }
    }
    /// <summary>
    /// 服务器通知-技能结束
    /// </summary>
    private void OnActionOnEndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType)
    {
        if (m_skillEntity != null && skillEntity != null && skillEntity.RuntimeID == m_skillEntity.RuntimeID)
        {
            SetEnergyValShow(false);
        }
    }

    #endregion

    #region 【委托】【BUFF】

    private void OnActionOnBuffCreate(SkillBuff skillBuff, ProtoMsg.BuffCreateRet buffCreateRet)
    {
        if (skillBuff != null)
        {
            if (skillBuff.BuffInfo != null && skillBuff.BuffInfo.Cfg != null)
            {
                // 判断BUFF是否显示3d血条上方显示
                bool isShowThreeD = skillBuff.BuffInfo.GetHaveBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.ThreeDHPUp);
                if (isShowThreeD)
                {
                    if (!m_CurSkillBuffs.Contains(skillBuff))
                    {
                        AddBuffItem(skillBuff);
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnBuffCreate() buffid={skillBuff.BuffInfo.BuffID} buff已经存在了");
                    }
                }
                // 查看女枪炮的标识并记录
                CheckBuffTag(skillBuff, true);
                // 获取BUFF护盾总数
                bool isChanage = CheckBuffShieldVal(skillBuff, true);
                if (isChanage)
                {
                    SetShieldValSum();
                }
            }
        }
    }

    private void OnActionOnBuffEnd(SkillBuff skillBuff, ProtoMsg.BuffEndRet buffEndRet)
    {
        if (skillBuff == null || skillBuff.BuffInfo == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnBuffEnd() buff数据都不存在了，传给我干毛线啊！！");
            return;
        }
        ThreeDBuffItemNew item = GetBuffItemBySkillBuff(skillBuff);
        if (item != null)
        {
            item.Release();
        }
        // 查看女枪炮的标识并记录
        CheckBuffTag(skillBuff, false);
        // 获取BUFF护盾总数
        bool isChanage = CheckBuffShieldVal(skillBuff, false);
        if (isChanage)
        {
            SetShieldValSum();
        }
    }

    private void OnActionOnBuffUpdate(SkillBuff skillBuff)
    {
        if (skillBuff == null || skillBuff.BuffInfo == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnBuffUpdate() buff数据都不存在了，传给我干毛线啊！！");
            return;
        }
        ThreeDBuffItemNew item = GetBuffItemBySkillBuff(skillBuff);
        if (item != null)
        {
            m_CurSkillBuffs.Remove(item.BuffEntity);  // 列表中删除
            m_CurSkillBuffs.Insert(0, skillBuff);   // 添加更新后的buff
            item.SetBuffData(skillBuff, true);
            RefreshBuffItemIndex(); // 刷新所有的index
        }
        // 获取BUFF护盾总数
        bool isChanage = CheckBuffShieldVal(skillBuff, true);
        if (isChanage)
        {
            SetShieldValSum();
        }
    }

    #endregion

    #region 设置文本的显示状态

    private void SetPos(bool isInit)
    {
        if (m_entityCtrl != null && m_entityCtrl.M_Curr != null && m_entityCtrl.M_Curr.Data != null)
        {
            //if (Vector3.Distance(m_lastPos, curPos) > 0.01f)
            {
                Vector3 curPos = m_entityCtrl.M_Curr.Position();
                curPos.y += m_modelHeight;
                if (isInit)
                {
                    //Vector2 uiPos = WorldItemChecker.Instance.ConvertWorldToCanvasPosition(curPos);
                    //transform.GetComponent<RectTransform>().anchoredPosition = uiPos;
                    transform.GetComponent<RectTransform>().anchoredPosition = PositionConvert.ConvertWorldToCanvasPosition(curPos);
                    //float dis = Vector2.Distance(m_lastPos, uiPos);
                    //if (dis < difffff)
                    //{
                    //    return;
                    //}
                    //if (isLerp)
                    //{
                    //    transform.localPosition = Vector2.Lerp(transform.localPosition, uiPos, Time.deltaTime * speed);
                    //}
                    //else
                    //{
                    //    transform.localPosition = uiPos;
                    //}
                }
                E_OverLayTypeOrderBase e_OverLayTypeOrderBase = E_OverLayTypeOrderBase.Pendant;
                if (m_entityCtrl.M_Curr.Data.isMainPlayer)
                {
                    e_OverLayTypeOrderBase = E_OverLayTypeOrderBase.MainPlayerPendant;
                }
                else if (m_CurCheckTarget || m_SpecialMapShow)
                {
                    e_OverLayTypeOrderBase = E_OverLayTypeOrderBase.SpecSelectPendant;
                }
                int order = WorldItemChecker.Instance.GetOverlayRenderGroupDistOrder(curPos, e_OverLayTypeOrderBase);
                SetIsPointInFrustum(order > 0);
                if (order < 0)
                {
                    order = 1;
                }
                m_Canvas.sortingOrder = order;
                //m_lastPos = uiPos;
            }
        }
    }
    public void SetTitleShow(bool isShow)
    {
        Title.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }
    public void SetLaborUnionShow(bool isShow)
    {
        Guild.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }
    public void SetHealthValShow(bool isShow, bool isSort)
    {
        m_isShowHealth = isShow && !m_ForceNoShowHPBUFF;
        // 血条、等级、buff一起控制显隐
        //SGF.Debuger.LogError($"头顶信息显示 isShow={isShow},isSort={isSort},cur={HealthBg.gameObject.active}");
        HealthBg.gameObject.SetActive(m_isShowHealth);
        if (m_isShowHealth == false)
        {
            HealthCheckBg.gameObject.SetActive(m_isShowHealth);
        }
        if (m_JobSpectralUI != null)
        {
            m_JobSpectralUI.SetShow(m_isShowHealth);
        }
        if (m_isShowHealth && m_entityCtrl != null && m_entityCtrl.EntityType == E_EntityType.Player)
        {
            GlobalEvent.onPlayerShowHPUI?.Invoke(m_entityCtrl.M_Curr.EntityId);
        }
        //MonsterTypeIcon.gameObject.SetActive(isShow);
        //if (!m_entityCtrl.Data.isMainPlayer && isShow && isSort)
        //{
        //    //int num = m_CurCheckTarget ? 9999 : 0;
        //    //SortingGroupHp.sortingOrder = num;
        //    int index = m_CurCheckTarget ? 1 : 2;
        //    SetHpMaterial(index);
        //}
    }
    private void SetArrowShow(bool isShow)
    {
        Arrow.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }
    public void SetNameShow(bool isShow)
    {
        Name.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }
    private void SetShowHealthCheckBg(bool isShow)
    {
        if (m_entityCtrl == null || m_entityCtrl.M_Curr == null)
        {
            return;
        }
        switch (m_entityCtrl.M_Curr.EntityType)
        {
            case E_EntityType.None:
                break;
            case E_EntityType.RoomSpace:
                break;
            case E_EntityType.Player:
                {
                    HealthCheckBg.gameObject.SetActive(isShow);
                    SetHealthValShow(isShow || m_isBattleState, true);
                }
                break;
            case E_EntityType.Npc:
                break;
            case E_EntityType.Monster:
            case E_EntityType.GVEBoss:
            case E_EntityType.Robot:
                {
                    HealthCheckBg.gameObject.SetActive(isShow);
                    if (isShow)
                    {
                        SetHealthValShow(true, true);
                    }
                    //else if (m_isBattleState == false && isShow == false)
                    //{
                    //    SetHealthValShow(false, true);
                    //}
                }
                break;
            case E_EntityType.BulletEntity:
                break;
            case E_EntityType.Interact:
                break;
            case E_EntityType.Summon:
                {
                    HealthCheckBg.gameObject.SetActive(isShow);
                    if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
                    {
                        SetHealthValShow(isShow || m_isBattleState, true);
                    }
                    else if (isShow)
                    {
                        SetHealthValShow(true, true);
                    }
                    else if (m_isBattleState == false && isShow == false)
                    {
                        SetHealthValShow(false, true);
                    }
                }
                break;
            case E_EntityType.Partner:
                {
                    HealthCheckBg.gameObject.SetActive(isShow);
                    SetHealthValShow(isShow || m_isBattleState, true);
                }
                break;
            default:
                {
                    HealthCheckBg.gameObject.SetActive(isShow);
                    SetHealthValShow(isShow || m_isBattleState, true);
                }
                break;
        }

        //SetHealthValShow(true, false);
        //if (isShow)
        //{
        //    int index = m_CurCheckTarget ? 1 : 2;
        //    SetHpMaterial(index);
        //}
    }
    public void SetEnergyValShow(bool isShow)
    {
        if (isShow == m_isShowEnergy)
        {
            return;
        }

        m_isShowEnergy = isShow;
        if (!m_isShowEnergy)
        {
            m_skillEntity = null;
        }
        //HealthVal.gameObject.SetActive(isShow);
        EnergyBar.transform.parent.gameObject.SetActive(m_isShowEnergy);
        //HealthVal.text = $"{m_healthCur}/{m_healthSum}";
    }
    public void SetFlashHide(bool isHide)
    {
        if (m_entityCtrl == null || m_entityCtrl.M_Curr == null)
        {
            return;
        }
        if (m_EntityFlash == isHide)
        {
            return;
        }
        m_EntityFlash = isHide;
        if (m_EntityFlash)
        {
            SetFlash(false);
            return;
        }

        switch (m_entityCtrl.M_Curr.EntityType)
        {
            case E_EntityType.None:
            case E_EntityType.RoomSpace:
            case E_EntityType.Npc:
            case E_EntityType.BulletEntity:
            case E_EntityType.Interact:
                {
                    SetFlash(true);
                }
                break;
            case E_EntityType.Player:
            case E_EntityType.Monster:
            case E_EntityType.GVEBoss:
            case E_EntityType.Robot:
            case E_EntityType.Summon:
            case E_EntityType.Partner:
                {
                    if (m_healthCur > 0 && !m_isDieHide && !m_EntityFlash)
                    {
                        SetFlash(true);
                    }
                }
                break;
            default:
                {
                    SetFlash(true);
                }
                break;
        }
    }
    public void SetStarShow(bool isShow)
    {
        StarContent.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }

    public void SetNpcUIShow(bool isShow, string iconPath)
    {
        if (isShow)
        {
            if (!string.IsNullOrEmpty(iconPath))
            {
                loadNPCIcon = (Sprite sp) =>
                {
                    if (sp != null && NPCUIBg != null)
                    {
                        NPCUI.sprite = sp;
                        NPCUIBg.gameObject.SetActive(true);
                        ForceRebuildLayout();
                    }
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconPath, loadNPCIcon);
            }
        }
        else
        {
            ForceRebuildLayout();
        }
    }

    public void SetAppellation(string appellation, string color)
    {
        if (!string.IsNullOrEmpty(appellation))
        {
            Appellation.text = appellation;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(color, out nowColor);
        Appellation.color = nowColor;
    }

    public void SetAppellation(string appellation, int cfgID)
    {
        if (!string.IsNullOrEmpty(appellation))
        {
            Appellation.text = appellation;
        }

        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(cfgID);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetAppellation() cfgID={cfgID},cfg=null,err!!!");
            return;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        Appellation.color = nowColor;
        Appellation.fontSize = cfg.GetFontSize();
    }

    public void SetNpcAppellationShow(bool isShow)
    {
        if (isShow && string.IsNullOrEmpty(Appellation.text))
        {
            return;
        }
        Appellation.gameObject.SetActive(isShow);
        ForceRebuildLayout();
    }

    public void SetCheckActive(bool isShow)
    {
        SetCurCheckTarget(isShow);
        //Vector3 scale = Vector3.one;
        //// 设置缩放倍数
        ////if (isShow)
        ////{
        ////    //scale *= 1.3f;
        ////}
        //InitScale(scale);

        //if (isShow)
        //{
        //    if (m_entityCtrl == null || m_entityCtrl.M_Curr == null)
        //    {
        //        return;
        //    }
        //    // 其他实体类型都会显示箭头
        //    // 箭头只是个表现形式
        //    //if (m_entityCtrl.M_Curr.EntityType != E_EntityType.Monster)
        //    //{
        //    //    return;
        //    //}
        //}
    }

    private void SetBuffRootShow(bool isShow, bool isForce = false)
    {
        if (m_isBuffRootState == isShow && !isForce)
        {
            return;
        }
        m_isBuffRootState = isShow && (m_CurCheckTarget || m_SpecialMapShow);
        Vector3 scale = m_isBuffRootState ? Vector3.one : Vector3.zero;
        BuffRoot.SetLocalScale(scale);
    }

    private void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(Layout);
    }

    #endregion

    #region 设置显隐

    private void SetFlash(bool flash)
    {
        if (m_IsFlash == flash)
        {
            return;
        }
        m_IsFlash = flash;
        SetSelfShow();
    }

    private void SetIsPointInFrustum(bool isIn)
    {
        if (m_IsPointInFrustum == isIn)
        {
            return;
        }
        m_IsPointInFrustum = isIn;
        SetSelfShow();
    }

    private void SetSelfShow()
    {
        CanvasGroup.alpha = m_IsPointInFrustum && m_IsFlash ? 1 : 0;
    }

    public void SetMvpAnim()
    {
        MvpAnim.SetActive(true);
        if (MvpAnimDelayedCall != null)
        {
            MvpAnimDelayedCall.Kill(true);
        }
        MvpAnimDelayedCall = DOVirtual.DelayedCall(3.0f, OnAnimFinish);
    }

    public void OnAnimFinish()
    {
        //SGF.Debuger.Log("动画完成");
        MvpAnim.SetActive(false);
    }


    #endregion

    #region 设置文本的颜色

    // 设置公会文本信息
    private void SetGuildColor()
    {
        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(1301);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetGuildColor() cfgID=1301,cfg=null,err!!!");
            return;
        }
        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        Guild.color = nowColor;
        Guild.fontSize = cfg.GetFontSize();
    }

    // 设置名字文本信息
    private void SetNameColor(int cfgID)
    {
        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(cfgID);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetNameColor() cfgID={cfgID},cfg=null,err!!!");
            return;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        Name.color = nowColor;
        Name.fontSize = cfg.GetFontSize();

        orgColor = nowColor;
    }

    public void SetNameTextColor(string textStr, int headNameColorID)
    {
        Name.text = textStr;

        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(headNameColorID);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetNameTextColor() cfgID={headNameColorID},cfg=null,err!!!");
            return;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        Name.color = nowColor;
        Name.fontSize = cfg.GetFontSize();
    }

    public void SetNameOrgColor()
    {
        Name.color = orgColor;
    }

    #endregion

    #region 更新属性信息

    public void UpdateHealth(long curHp)
    {
        m_healthCur = curHp;
        if (m_healthCur > m_healthSum)
        {
            m_healthSum = m_healthCur;
        }

        SetMixHpShield();
        //HealthVal.text = $"{m_healthCur}/{m_healthSum}";

        // 没血了，要0.5s后隐藏
        if (m_healthCur <= 0 && !m_isDieHide)
        {
            if (m_entityCtrl.M_Curr.EntityType != E_EntityType.Npc && m_entityCtrl.M_Curr.EntityType != E_EntityType.BulletEntity && m_entityCtrl.M_Curr.EntityType != E_EntityType.Player)
            {
                m_isDieHide = true;
                m_curDieHideTime = 0.5f;
            }
            //SetHealthValShow(false, false);
        }
        if (m_healthCur > 0 && !m_isDieHide && !m_EntityFlash)
        {
            m_isDieHide = false;
            SetFlash(true);
        }
    }

    public void UpdateTruthHp(long hp)
    {
        m_healthSum = hp;
        if (m_healthCur > m_healthSum)
        {
            m_healthCur = m_healthSum;
        }
        SetMixHpShield();
        //HealthVal.text = $"{m_healthCur}/{m_healthSum}";
    }

    private void SetMixHpShield()
    {
        bool isHaveShield = ShieldValSum > 0;
        if (isHaveShield)
        {
            long mixHp = ShieldValSum + m_healthCur;
            bool isExceedMaxHp = mixHp > m_healthSum;
            if (isExceedMaxHp)
            {
                // 超过实体的最大血量
                float ratio = (float)m_healthCur / (float)mixHp;
                HealthBar.fillAmount = ratio;
                float ratioShield = (float)ShieldValSum / (float)mixHp;
                ShieldValBarRight.fillAmount = ratioShield;
                ShieldValBarLeft.fillAmount = 0;
            }
            else
            {
                float ratio = m_healthSum == 0 ? 0 : (float)m_healthCur / (float)m_healthSum;
                HealthBar.fillAmount = ratio;
                float ratioShield = m_healthSum == 0 ? 0 : (float)ShieldValSum / (float)m_healthSum;
                ShieldValBarLeft.fillAmount = ratioShield + ratio;
                ShieldValBarRight.fillAmount = 0;
            }
        }
        else
        {
            float ratio = m_healthSum == 0 ? 0 : (float)m_healthCur / (float)m_healthSum;
            HealthBar.fillAmount = ratio;
            ShieldValBarLeft.fillAmount = 0;
            ShieldValBarRight.fillAmount = 0;
        }
    }

    public void UpdateState(uint value)
    {
        if (m_entityCtrl.M_Curr == null)
        {
            return;
        }
        bool isBattleStateServer = m_entityCtrl.M_Curr.Data.IsBattleStateServer;
        bool isDead = m_entityCtrl.M_Curr.Data.IsDead;
        if (m_isBattleState == isBattleStateServer)
        {
            return;
        }
        m_isBattleState = isBattleStateServer;
        // 怪物脱战后血条消失 || 是主角召唤物 
        switch (m_entityCtrl.M_Curr.EntityType)
        {
            case E_EntityType.None:
                break;
            case E_EntityType.RoomSpace:
                break;
            case E_EntityType.Player:
                {
                    SetHealthValShow(m_isBattleState, true);
                }
                break;
            case E_EntityType.Npc:
                break;
            case E_EntityType.Monster:
            case E_EntityType.GVEBoss:
            case E_EntityType.Robot:
                {
                    if (!isDead)
                    {
                        if (GameManager.Instance.GetCurMapType() == ProtoMsg.SpaceType.SpaceSercet)
                        {
                            SetHealthValShow(m_isBattleState, true);
                        }
                        else
                        {
                            if (!m_isBattleState)
                            {
                                SetHealthValShow(m_isBattleState || m_SpecialMapShow, true);
                            }
                        }
                    }
                }
                break;
            case E_EntityType.BulletEntity:
                break;
            case E_EntityType.Interact:
                break;
            case E_EntityType.Summon:
                {
                    if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
                    {
                        SetHealthValShow(m_isBattleState, true);
                    }
                    else if (!m_isBattleState)
                    {
                        SetHealthValShow(m_isBattleState, false);
                    }
                }
                break;
            case E_EntityType.Partner:
                {
                    SetHealthValShow(m_isBattleState, true);
                }
                break;
            default:
                {
                    SetHealthValShow(m_isBattleState, true);
                }
                break;
        }
    }

    public void UpdateLevel(int level)
    {
        Level.text = level + "";
    }

    public void SetEenergy(SkillEntity skillEntity)
    {
        if (skillEntity == null)
        {
            return;
        }
        m_skillEntity = skillEntity;
        energyTime = m_skillEntity.EnergyItemTime;
        energyMaxNum = m_skillEntity.EnergyMaxCount;
        energyedCount = m_skillEntity.GetCurEnergyCount();
        curEnergyTime = m_skillEntity.GetCurEnergyTime();
        m_energSize.x = 0f;
        //EnergyBar.size = m_energSize;

        SetEnergyValShow(true);
    }

    /// <summary>
    /// 更新蓄力进度条
    /// </summary>
    /// <param name="curEnerg">单条蓄力的时间</param>
    public void UpdatErnergy(float curEnerg)
    {
        float ratio = energyTime == 0 ? 0 : curEnerg / energyTime;
        m_energSize.x = ratio;
        //EnergyBar.size = m_energSize;
    }
    #endregion

    #region 伙伴星星

    private void InitParterStar()
    {
        int starNum = 0;
        uint index = m_entityCtrl.M_Curr.ConfigIndex;
        PartnerDataCell partnerDataCell = LocalDataManager.Instance.GetPartnerDataCell(index);
        if (partnerDataCell != null)
        {
            starNum = partnerDataCell.GetStar();
        }

        int count = starNum;
        int childCount = StarContent.childCount;
        int maxLength = Mathf.Max(childCount, count);
        for (int i = 0; i < maxLength; i++)
        {
            Transform item = null;
            bool isShow = starNum >= StarContent.childCount;
            if (childCount > count)
            {
                item = StarContent.GetChild(i);
            }
            else
            {
                item = Instantiate<Transform>(StarTemp);
                item.SetParent(StarContent);
            }
            item.gameObject.SetActive(isShow);
        }
    }

    #endregion

    #region NPC

    private void OnTaskNpcChange(long NPCID, uint taskID, int type, TaskNpc.StateEnum stateEnum)
    {
        if (stateEnum == TaskNpc.StateEnum.None)//任务不存在为什么还要刷新事件？
        {
            return;
        }
        if (m_entityCtrl == null || m_entityCtrl.M_Curr == null)
        {
            return;
        }

        if (m_entityCtrl.M_Curr.ConfigIndex != NPCID)
        {
            return;
        }
        var tasks = TaskHelper.GetTaskNpcs((int)NPCID);
        List<TaskNpc> taskListTemp = new List<TaskNpc>();
        foreach (var task in tasks)
        {
            if (task.StateE != TaskNpc.StateEnum.None && task.StateE != TaskNpc.StateEnum.Finish)
            {
                taskListTemp.Add(task);
            }
        }
        NPCUIBg.gameObject.SetActive(false);
        if (taskListTemp.Count > 0)
        {
            taskListTemp.Sort((a, b) => b.TaskType < a.TaskType ? 1 : -1);
            foreach (var task in taskListTemp)
            {
                SetNpcTypeIcon(task.TaskID, task.StateE);
                break;
            }
        }
    }

    private bool SetNpcTypeIcon(uint taskID, TaskNpc.StateEnum stateEnum)
    {
        var taskConfig = TaskHelper.GetTaskConfig(taskID);
        Task.TaskClassifyType taskType = Task.TaskClassifyType.MainLine;
        if (taskConfig != null && taskConfig.Base != null)
        {
            taskType = taskConfig.Base.TaskType;
        }

        // 1、气泡和任务类型无关
        // 2、任务气泡显示状态 可交> 可接>已接未完成>已完成（不显示）
        bool isShow = false;
        string iconPath = string.Empty;
        if (stateEnum != TaskNpc.StateEnum.None && stateEnum != TaskNpc.StateEnum.Finish)
        {
            iconPath = TaskManager.GetHUDTaskIconNameByType(taskType, stateEnum);
            isShow = true;
        }
        else
        {
            NpcDataCell npcCfg = LocalDataManager.Instance.GetNPCDataCell(m_entityCtrl.M_Curr.ConfigIndex);
            if (npcCfg != null)
            {
                iconPath = TaskManager.GetHUDTaskIconNameByType(npcCfg.GetNpcType(), npcCfg.MapLogo);
                isShow = true;
            }
        }

        SetNpcUIShow(isShow, iconPath);
        return isShow;
    }

    #endregion

    #region SkillBuff

    #region 【BUFF】SkillBuffList

    private void AddSkillBuffList(SkillBuff skillBuff)
    {
        if (!m_CurSkillBuffs.Contains(skillBuff))
        {
            m_CurSkillBuffs.Insert(0, skillBuff);

            SetBuffRootShow(true);
        }
    }

    private void RemoveSkillBuffListByIndex(SkillBuff skillBuff)
    {
        if (m_CurSkillBuffs.Contains(skillBuff))
        {
            m_CurSkillBuffs.Remove(skillBuff);
        }
        if (m_CurSkillBuffs.Count <= 0)
        {
            SetBuffRootShow(false);
        }
    }

    private void ReleaseBuffList()
    {
        m_CurSkillBuffs.Clear();
        SetBuffRootShow(false);
    }

    private int GetBuffIndexBySkillBuff(SkillBuff skillBuff)
    {
        for (int i = 0; i < m_CurSkillBuffs.Count; i++)
        {
            var child = m_CurSkillBuffs[i];
            if (child != null && child.RuntimeID == skillBuff.RuntimeID)
            {
                return i;
            }
        }
        return -1;
    }

    #endregion

    #region 【BUFF】BuffItemList

    private void CreateBuffItem(SkillBuff skillBuff)
    {
        if (BuffItem == null)
        {
            return;
        }

        var gob = GameObject.Instantiate<GameObject>(BuffItem);
        if (gob != null)
        {
            gob.transform.SetParent(BuffRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.localRotation = Quaternion.identity;
            gob.transform.SetLocalScale(Vector3.one);
            int index = GetBuffIndexBySkillBuff(skillBuff);
            UpdateBuffItem(skillBuff, gob, index);
        }
    }

    private void UpdateBuffItem(SkillBuff skillBuff, GameObject go, int index)
    {
        if (go == null)
        {
            return;
        }
        ThreeDBuffItemNew item = go.GetComponent<ThreeDBuffItemNew>();
        if (item != null)
        {
            item.SetBuffItem(skillBuff, BuffItemReleaseCb);
            item.SetIndex(index);
            AddBuffItemList(item);
        }
    }

    private void AddBuffItemList(ThreeDBuffItemNew item)
    {
        if (!m_CurBuffItemList.Contains(item))
        {
            m_CurBuffItemList.Insert(0, item);
        }
    }

    private ThreeDBuffItemNew GetIdleBuffItem()
    {
        foreach (var item in m_CurBuffItemList)
        {
            if (!item.IsUsed)
            {
                return item;
            }
        }
        return null;
    }

    private ThreeDBuffItemNew GetBuffItemBySkillBuff(SkillBuff skillBuff)
    {
        foreach (var item in m_CurBuffItemList)
        {
            if (item.IsUsed && item.BuffEntity != null)
            {
                if (item.BuffEntity.RuntimeID == skillBuff.RuntimeID)
                {
                    return item;
                }
            }
        }
        return null;
    }

    private void ReleaseBuffItem()
    {
        for (int i = m_CurBuffItemList.Count - 1; i >= 0; i--)
        {
            var child = m_CurBuffItemList[i];
            if (child != null)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    private void ResetBuffItem()
    {
        foreach (var item in m_CurBuffItemList)
        {
            if (item != null)
            {
                item.SetBuffItem(null, null);
            }
        }
    }

    #endregion

    #region 【BUFF】标识

    private Dictionary<string, int> buffSpTimeShowDic = new();

    private void CheckBuffTag(SkillBuff skillBuff, bool isOpend)
    {
        if (skillBuff == null || skillBuff.BuffInfo == null || skillBuff.BuffInfo.Cfg == null)
        {
            return;
        }
        foreach (var item in skillBuff.BuffInfo.Cfg.GlobalShows)
        {
            if (item.GlobalShowType == GlobalShowType.BUFF_SpTimeShow)
            {
                if (item.BUFF_SpTimeShow != null && item.BUFF_SpTimeShow.SpTimeShowType != null)
                {
                    foreach (var child in item.BUFF_SpTimeShow.SpTimeShowType)
                    {
                        string key = $"{skillBuff.RuntimeID}_{child}";
                        int keyValue = GetBuffSpTimeShowDicVal(key);
                        if (keyValue == -1 && !isOpend)
                        {
                            // 如果要关闭了，但是没记录那就，过滤
                            continue;
                        }
                        // 添加标识的字典记录
                        SetBuffSpTimeShowDic(key, isOpend);
                        //int curKeyValue = GetBuffSpTimeShowDicVal(key);
                        // 开启/关闭对应的动画
                        switch (child)
                        {
                            case SpTimeShowTypeEnum.OverheadClock:
                                {
                                    // 头顶时钟显示
                                    SetOverheadClock(skillBuff, isOpend);
                                }
                                break;
                            case SpTimeShowTypeEnum.ProgressBelowHead:
                                {
                                    // 头顶血条下方读条
                                }
                                break;
                            case SpTimeShowTypeEnum.MuYTreat:
                                {
                                    // 牧夜治疗
                                    SetMuYTreat(skillBuff, isOpend);
                                }
                                break;
                            case SpTimeShowTypeEnum.MuYAttack:
                                {
                                    // 牧夜强攻
                                    SetMuYAttack(skillBuff, isOpend);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }

    private int GetBuffSpTimeShowDicVal(string key)
    {
        if (buffSpTimeShowDic.TryGetValue(key, out var value))
        {
            return value;
        }
        return -1;
    }

    private void SetBuffSpTimeShowDic(string key, bool isOpen)
    {
        if (buffSpTimeShowDic.TryGetValue(key, out var value))
        {
            int add = isOpen ? 1 : -1;
            value += add;
            buffSpTimeShowDic[key] = value;
        }
        else
        {
            int add = isOpen ? 1 : 0;
            buffSpTimeShowDic.Add(key, add);
        }
    }

    private void SetOverheadClock(SkillBuff skillBuff, bool isOpen)
    {
        if (isOpen)
        {
            // 开启
            if (skillBuff != null)
            {
                if (m_JobSpectralUI != null)
                {
                    m_JobSpectralUI.SetCountdown(SpTimeShowTypeEnum.OverheadClock, (float)skillBuff.MaxTime / 1000, (float)skillBuff.GetEndime() / 1000);
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.OverheadClock, true);
                }
                //OverheadClockMaxTime = skillBuff.MaxTime / 1000;
                ////JobStateIcon.material.SetFloat("_Angle", 0);
                //OverheadClockCurCountDown = (float)skillBuff.GetEndime() / 1000;
                ////SGF.Debuger.LogError($"BUFF时间显示 开始 OverheadClockCurCountDown={OverheadClockCurCountDown}");
                //isOverheadClock = true;
                //JobStateIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            int curTime = 0;
            if (skillBuff != null)
            {
                string key = $"{skillBuff.RuntimeID}_{SpTimeShowTypeEnum.OverheadClock}";
                curTime = GetBuffSpTimeShowDicVal(key);
            }
            // 关闭
            if (curTime <= 0)
            {
                if (m_JobSpectralUI != null)
                {
                    //m_JobSpectralUI.SetCountdown(0, 0);
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.OverheadClock, false);
                }
                //OverheadClockCurCountDown = 0;
                //isOverheadClock = false;
                ////JobStateIcon.material.SetFloat("_Angle", 0);
                ////JobStateIcon.gameObject.SetActive(false);
                ////SGF.Debuger.LogError($"BUFF时间显示 结束 OverheadClockCurCountDown={OverheadClockCurCountDown}");
            }
        }
    }

    private void SetMuYTreat(SkillBuff skillBuff, bool isOpen)
    {
        if (isOpen)
        {
            // 开启
            if (skillBuff != null)
            {
                if (m_JobSpectralUI != null)
                {
                    m_JobSpectralUI.SetCountdown(SpTimeShowTypeEnum.MuYTreat, (float)skillBuff.MaxTime / 1000, (float)skillBuff.GetEndime() / 1000);
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.MuYTreat, true);
                }
            }
        }
        else
        {
            int curTime = 0;
            if (skillBuff != null)
            {
                string key = $"{skillBuff.RuntimeID}_{SpTimeShowTypeEnum.MuYTreat}";
                curTime = GetBuffSpTimeShowDicVal(key);
            }
            // 关闭
            if (curTime <= 0)
            {
                if (m_JobSpectralUI != null)
                {
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.MuYTreat, false);
                }
            }
        }
    }

    private void SetMuYAttack(SkillBuff skillBuff, bool isOpen)
    {
        if (isOpen)
        {
            // 开启
            if (skillBuff != null)
            {
                if (m_JobSpectralUI != null)
                {
                    m_JobSpectralUI.SetCountdown(SpTimeShowTypeEnum.MuYAttack, (float)skillBuff.MaxTime / 1000, (float)skillBuff.GetEndime() / 1000);
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.MuYAttack, true);
                }
            }
        }
        else
        {
            int curTime = 0;
            if (skillBuff != null)
            {
                string key = $"{skillBuff.RuntimeID}_{SpTimeShowTypeEnum.MuYAttack}";
                curTime = GetBuffSpTimeShowDicVal(key);
            }
            // 关闭
            if (curTime <= 0)
            {
                if (m_JobSpectralUI != null)
                {
                    m_JobSpectralUI.SetOpenCountdown(SpTimeShowTypeEnum.MuYAttack, false);
                }
            }
        }
    }

    private void ReleaseBuffSpTimeShowDic()
    {
        buffSpTimeShowDic.Clear();

        SetOverheadClock(null, false);
        SetMuYTreat(null, false);
        SetMuYAttack(null, false);
    }

    #endregion

    #region 【BUFF】护盾

    private int ShieldValSum = 0;

    private Dictionary<ulong, int> buffShieldDic = new();

    private bool CheckBuffShieldVal(SkillBuff skillBuff, bool isOpen)
    {
        bool isChanage = false;
        if (skillBuff == null || skillBuff.BuffInfo == null || skillBuff.BuffInfo.Cfg == null)
        {
            return isChanage;
        }

        int shieldVal = isOpen ? skillBuff.ShieldVal : 0;

        if (buffShieldDic.ContainsKey(skillBuff.RuntimeID))
        {
            if (shieldVal > 0)
            {
                int lastShield = buffShieldDic[skillBuff.RuntimeID];
                if (lastShield != shieldVal)
                {
                    buffShieldDic[skillBuff.RuntimeID] = shieldVal;
                    isChanage = true;
                }
            }
            else
            {
                buffShieldDic.Remove(skillBuff.RuntimeID);
                isChanage = true;
            }
        }
        else if (shieldVal > 0)
        {
            buffShieldDic.Add(skillBuff.RuntimeID, shieldVal);
            isChanage = true;
        }

        return isChanage;
    }

    private void SetShieldValSum()
    {
        int sum = 0;
        foreach (var item in buffShieldDic)
        {
            sum += item.Value;
        }
        ShieldValSum = sum;
        // 通知血条比例修改
        SetMixHpShield();
    }

    private void ReleaseBuffShieldDic()
    {
        buffShieldDic.Clear();
        ShieldValSum = 0;
    }

    #endregion

    private void AddBuffItem(SkillBuff skillBuff)
    {
        AddSkillBuffList(skillBuff);
        int count = m_CurSkillBuffs.Count;
        //SGF.Debuger.LogError($"动效 新加的 runtiemid={skillBuff.RuntimeID},index={0}");
        //SGF.Debuger.LogError($"动效 新加的 总数={count}");
        int index = 0;
        // 判断是否有空闲的item
        ThreeDBuffItemNew buffItem = GetIdleBuffItem();
        // 创建新的item
        if (buffItem == null)
        {
            CreateBuffItem(skillBuff);
        }
        else
        {
            UpdateBuffItem(skillBuff, buffItem.gameObject, index);
        }
        if (count > GameConfig.BUFF_SHOW_MAX_COUNT)
        {
            RefreshBuffItemIndex();
        }
    }

    private void BuffItemReleaseCb(SkillBuff skillBuff)
    {
        // list中删除
        RemoveSkillBuffListByIndex(skillBuff);

        RefreshBuffItemIndex();
    }

    // 刷新在场的index
    private void RefreshBuffItemIndex()
    {
        // 刷新在场的item index
        foreach (var item in m_CurBuffItemList)
        {
            if (item != null && item.BuffEntity != null)
            {
                int index = GetBuffIndexBySkillBuff(item.BuffEntity);
                if (index != -1)
                {
                    //SGF.Debuger.Log($"动效 刷新 runtiemid={item.BuffEntity.RuntimeID},index={index}");
                    item.SetIndex(index);
                }
            }
        }
    }

    private void ResetBuff()
    {
        ReleaseBuffList();
        ResetBuffItem();
    }

    #endregion

    #region 
    /// <summary>
    /// 随机一个 聊天对话
    /// </summary>
    public void RandomChat()
    {

    }

    #endregion

}
