using StarProject;
using StarProject.Game;
using StarProject.Game.Entity;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectUnitPendantViewNew : MonoBehaviour
{
    private string LOG_TAG = "[ObjectUnitPendantViewNew]";

    private Canvas m_Canvas;
    private CanvasGroup m_CanvasGroup;
    private RectTransform rect;

    [SerializeField] private RectTransform Layout;
    [SerializeField] private GameObject Arrow;
    [SerializeField] private Text Name;
    [SerializeField] private Image NPCUIBg;
    [SerializeField] private Image NPCUI;
    [SerializeField] private GameObject mJobObj;
    [SerializeField] private Image m_JobIcon;
    [SerializeField] private Text mStateText;
    [SerializeField] private GameObject mStateBg;

    private ObjectCtrlGroup m_entityCtrl;
    private EntityLocalDynamic m_entityLocalDynamic;
    private float m_modelHeight = 2f;
    private InteractDataCell m_InteractDataCell;
    private bool m_IsHasMineID = false; // 是否有关联矿物

    private bool m_EntityFlash = false; // ----- 模型隐藏（技能效果、NPC解锁）
    private bool m_IsPointInFrustum = false; // 是否在相机范围内
    private bool m_IsExceedMapGridSize = false; // 是否超过了地图格子范围

    #region 资源异步加载回调

    private Action<Sprite> loadTaskIcon;

    #endregion

    private void Awake()
    {
        m_Canvas = GetComponent<Canvas>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void InitAoiObject(ObjectCtrlGroup entityCtrl)
    {
        Reset();
        m_entityCtrl = entityCtrl;
        m_entityLocalDynamic = null;
        if (m_entityCtrl == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} InitAoiObject m_entityCtrl=null");
            return;
        }

        transform.SetLocalScale(Vector3.one);
        transform.position = Vector3.zero;
        //Name.gameObject.SetActive(false);
        m_IsHasMineID = false;

        m_InteractDataCell = LocalDataManager.Instance.GetInteractDataCell(m_entityCtrl.ConfigID);
        if (m_InteractDataCell != null)
        {
            m_IsHasMineID = m_InteractDataCell.GetMineID() > 0;
        }

        if (m_IsHasMineID)
        {
            SetGather(m_entityCtrl.Index);
        }
        else
        {
            SetNameText(m_entityCtrl.ObjectName);
        }

        // 设置任务状态图标
        uint taskID = 0;
        TaskNpc.StateEnum stateEnum = TaskHelper.QueryInterTaskEnum((int)m_entityCtrl.M_Curr.ConfigIndex, ref taskID);
        SetTaskTypeIcon(taskID, stateEnum);

        SetPos();

        GlobalEvent.OnMapMineChange.AddListener(OnMapMineChangeHandler);
        GlobalEvent.OnTaskInterChange.AddListener(OnTaskInterChange);
    }

    public void InitLocalObject(EntityLocalDynamic entityLocalDynamic)
    {
        Reset();
        m_entityLocalDynamic = entityLocalDynamic;
        m_entityCtrl = null;
        if (m_entityLocalDynamic == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} InitLocalObject m_entityLocalDynamic=null");
            return;
        }

        m_InteractDataCell = m_entityLocalDynamic.interactDataCell;

        if (m_InteractDataCell != null)
        {
            SetNameText(m_InteractDataCell.ModelName);
        }

        transform.SetLocalScale(Vector3.one);
        transform.position = Vector3.zero;
        mJobObj.SetActive(false);
        ForceRebuildLayout();
        SetPos();
    }

    internal void EnterFrame(int frameIndex)
    {
    }

    private void LateUpdate()
    {
        #region 跟随实体移动

        SetPos(false);

        #endregion
    }

    private void Reset()
    {
        m_entityLocalDynamic = null;
        m_entityCtrl = null;
        m_IsHasMineID = false;
        m_InteractDataCell = null;
        m_EntityFlash = false;
        //m_IsFlash = true;
        m_IsPointInFrustum = false;
        m_IsExceedMapGridSize = false;

        GlobalEvent.OnMapMineChange.RemoveListener(OnMapMineChangeHandler);
        GlobalEvent.OnTaskInterChange.RemoveListener(OnTaskInterChange);

        SetNameText(string.Empty);
        //Name.gameObject.SetActive(false);
        SetArrowShow(false);
        mJobObj.SetActive(false);
    }

    public void Release()
    {
        Reset();
    }

    private void OnMapMineChangeHandler(int arg0)
    {
        if (m_entityCtrl != null)
        {
            SetGather(m_entityCtrl.Index);
        }
    }

    public void SetY(float y)
    {
        m_modelHeight = y;
        //Scale.SetLocalPositionY(y);
    }

    private void SetPos(bool isInit = true)
    {
        if (!m_EntityFlash || isInit)
        {
            if (m_entityCtrl != null && m_entityCtrl.M_Curr != null)
            {
                Vector3 curPos = m_entityCtrl.M_Curr.Position();
                curPos.y += m_modelHeight;
                Vector2 uiPos = PositionConvert.ConvertWorldToCanvasPosition(curPos);
                rect.anchoredPosition = uiPos;

                int order = WorldItemChecker.Instance.GetOverlayRenderGroupDistOrder(curPos, E_OverLayTypeOrderBase.SpecSelectPendant);
                SetIsPointInFrustum(order > 0);
                if (order < 0)
                {
                    order = 1;
                }

                m_Canvas.sortingOrder = order;
            }
            else if (m_entityLocalDynamic != null)
            {
                Vector3 curPos = m_entityLocalDynamic.Position();

                bool isExceedMapGridSize = GameManager.Instance.IsExceedMapGridSize(curPos);
                SetIsExceedMapGridSize(isExceedMapGridSize);

                curPos.y += m_modelHeight;
                Vector2 uiPos = PositionConvert.ConvertWorldToCanvasPosition(curPos);
                rect.anchoredPosition = uiPos;

                int order = WorldItemChecker.Instance.GetOverlayRenderGroupDistOrder(curPos, E_OverLayTypeOrderBase.SpecSelectPendant);
                SetIsPointInFrustum(order > 0);
                if (order < 0)
                {
                    order = 1;
                }

                m_Canvas.sortingOrder = order;
            }
        }
    }

    private void SetNameText(string name)
    {
        Name.text = name;

        SetNameColoe();
    }

    private void SetNameColoe()
    {
        if (m_InteractDataCell == null)
        {
            return;
        }

        int nameCfgID = 7001;
        if (m_IsHasMineID)
        {
            nameCfgID = 8001;   // 采集物名称
        }
        else
        {
            switch (m_InteractDataCell.GetObjectType())
            {
                case 1:
                    {
                        nameCfgID = 7001;   // [普通npc](名字)颜色
                    }
                    break;
                case 2:
                    {
                        nameCfgID = 7101;   // [功能npc](名字)颜色
                    }
                    break;
                case 3:
                    {
                        nameCfgID = 7201;   // [玩法npc](名字)颜色
                    }
                    break;
                default:
                    break;
            }
        }

        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(nameCfgID);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetNameTextColor() cfgID={nameCfgID},cfg=null,err!!!");
            return;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        Name.color = nowColor;
        Name.fontSize = cfg.GetFontSize();
    }

    private void SetStateText(string name, int textCfgID)
    {
        mStateText.text = name;

        SetStateColoe(textCfgID);
    }

    private void SetStateColoe(int textCfgID)
    {
        if (m_InteractDataCell == null)
        {
            return;
        }

        var cfg = LocalDataManager.Instance.GetHeadNameColorDataCell(textCfgID);
        if (cfg == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetStateColoe() cfgID={textCfgID},cfg=null,err!!!");
            return;
        }

        Color nowColor;
        ColorUtility.TryParseHtmlString(cfg.Color, out nowColor);
        mStateText.color = nowColor;
        mStateText.fontSize = cfg.GetFontSize();
    }

    public void SetArrowShow(bool isShow)
    {
        Arrow.SetActive(isShow);
        ForceRebuildLayout();
    }

    // 如果有关联矿物ID就是采集物交互物件实体
    // 会再次设置一次头顶信息
    private void SetGather(int index)
    {
        mJobObj.SetActive(false);
        if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.LifeSkill))
        {
            return;
        }
        if (m_InteractDataCell != null && m_IsHasMineID)
        {
            Name.text = string.Empty;
            var minecfg = LocalDataManager.Instance.GetLifeSkillMineDataCell((int)m_InteractDataCell.GetMineID());
            if (minecfg != null)
            {
                var serJobInfo = BusinessManager.Instance.GetLifeJobSkill(minecfg.GetSkillID());
                if (serJobInfo != null)
                {
                    if (serJobInfo.SkillLevel < minecfg.GetLevel())
                    {
                        return;
                    }

                    SetNameText(minecfg.Name);
                    bool isLess = false;
                    bool red = false;

                    int usecnt = BusinessManager.Instance.GetMineUseCount(GameManager.Instance.GetCurMapId(), index);
                    string stateText = "";
                    int textCfgID = 8002;   // 默认 采集物状态-等级
                    if (usecnt >= minecfg.GetPersonReserve())
                    {
                        //stateText = $"<color=#eacc7c>{GameConfig.LocalStr["GatherState"]}</color>";
                        stateText = $"<color=#eacc7c>{LanguageManager.Instance.GetLanguageByKey("GatherState")}</color>";
                        textCfgID = 8004;   // 采集物状态-枯竭
                        isLess = true;
                        red = true;
                    }
                    else
                    {
                        //stateText = $"<color=#ffffff>{minecfg.GetLevel()}级</color>";
                        //stateText = $"<color=#ffffff>{string.Format(GameConfig.LocalStr["LineStr"], minecfg.GetLevel())}</color>";
                        stateText = $"<color=#ffffff>{string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), minecfg.GetLevel())}</color>";

                        textCfgID = 8002;   // 采集物状态-等级
                    }
                    SetStateText(stateText, textCfgID);
                    var spriteName = "Live_icon_22";
                    if (minecfg.GetSkillID() == 102)
                    {
                        spriteName = "Live_icon_21";
                    }
                    else if (minecfg.GetSkillID() == 103)
                    {
                        spriteName = "Live_icon_20";
                    }

                    if (m_entityCtrl != null)
                    {
                        m_entityCtrl.MineIsDrain = isLess;
                    }

                    if (red)
                    {
                        spriteName = "Live_icon_23";
                    }

                    AtlasManager.Instance.GetSpriteAsync("UI/LivingSkills/Atlas/LivingSkills", spriteName,
                        (s) => { m_JobIcon.sprite = s; });

                    mStateBg.SetActive(!red);

                    //m_JobIcon.color=red ? new Color(0.5f,0.5f,0.5f,1):new Color(1,1,1,1);
                    // m_JobIcon.SetGray(!red);
                }

                mJobObj.SetActive(true);
            }
        }
        ForceRebuildLayout();
    }

    #region 任务状态图标

    private void OnTaskInterChange(long configID, uint taskID, int type, TaskNpc.StateEnum stateEnum)
    {
        if (m_entityCtrl == null || m_entityCtrl.M_Curr == null)
        {
            return;
        }

        if (m_entityCtrl.M_Curr.ConfigIndex != configID)
        {
            return;
        }

        SetTaskTypeIcon(taskID, stateEnum);
    }

    private void SetTaskTypeIcon(uint taskID, TaskNpc.StateEnum stateEnum)
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
            //InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell((long)m_entityCtrl.M_Curr.ConfigIndex);
            //if (interactDataCell != null)
            //{
            //    npcType = interactDataCell.GetNpcType();
            //    isShow = true;
            //}
        }
        SetTaskUIShow(isShow, iconPath);
    }

    private void SetTaskUIShow(bool isShow, string iconPath)
    {
        NPCUIBg.gameObject.SetActive(false);
        //if (!m_IsHasMineID)
        //{
        //    Name.gameObject.SetActive(isShow);
        //}
        if (isShow)
        {
            if (!string.IsNullOrEmpty(iconPath))
            {
                loadTaskIcon = (Sprite sp) =>
                {
                    if (sp != null && NPCUIBg != null)
                    {
                        NPCUI.sprite = sp;
                        NPCUIBg.gameObject.SetActive(true);
                        ForceRebuildLayout();
                    }
                    loadTaskIcon = null;
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconPath, loadTaskIcon);
            }
        }
        else
        {
            ForceRebuildLayout();
        }
    }

    #endregion

    #region 设置显隐

    public void SetFlashHide(bool isHide)
    {
        if (m_entityCtrl == null && m_entityLocalDynamic == null)
        {
            return;
        }
        //if (m_entityLocalDynamic != null)
        //{
        //    SGF.Debuger.LogWarning($"通缉实体 头顶设置 key={m_entityLocalDynamic.EntityKey},m_EntityFlash={m_EntityFlash},isHide={isHide}");
        //}
        if (m_EntityFlash == isHide)
        {
            return;
        }
        m_EntityFlash = isHide;


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

    private void SetIsExceedMapGridSize(bool isExceed)
    {
        if (m_IsExceedMapGridSize == isExceed)
        {
            return;
        }
        m_IsExceedMapGridSize = isExceed;
        SetSelfShow();
    }

    private void SetSelfShow()
    {
        m_CanvasGroup.alpha = m_IsPointInFrustum && !m_EntityFlash && !m_IsExceedMapGridSize ? 1 : 0;
    }

    private void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(Layout);
    }

    #endregion

}