using ProtoMsg;
using SGF.Network;
using SGF.Time;
using StarProject;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.AtlasManager;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Time;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class PartnerItem : MonoBehaviour
{
    private const string LOG_TAG = "[PartnerItem]";

    //public Image CheckBg; // 选中背景
    public Image Frame; // 伙伴边框
    //public Image CheckFrame; // 选中边框
    public Image Icon; // 伙伴图标
    public GameObject CheckState; // 选中状态
    public Image HPProgress; // 血量进度
    public GameObject LineDownIcon;
    public GameObject LineDownIcon2;

    public Image ShieldValBarLeft;
    public Image ShieldValBarRight;
    public Text PercentumHp; // 血量百分比血
    public GameObject CheckSkillBg; // 选中技能背景
    public Image SkillIcon; // 技能图标
    public Image SkillCDProgress; // 技能CD进度条
    public Text SkillCDLab; // 技能CD文本
    public CanvasGroup BuffRoot; // BUFF root
    public Text DeadCountdown; // 死亡倒计时
    public Image SeatCDMask;    // 切换CD
    public Image SeatCDMask2;    // 切换CD
    public GameObject TutorialCircle;   // 引导圆圈
    public Transform ObjDialogRoot;   // 伙伴喊话节点

    #region 动画

    public List<AnimationClip> Clips = new();
    public Animancer.AnimancerComponent Animancer;
    private Dictionary<string, AnimationClip> AnimationClips = new();

    #endregion

    public ParticleSystem SkillClickFx; // 技能点击特效
    public GameObject SkillCDFinishFx; // 技能CD完成特效
    public GameObject SkillCDFinishFxOne; // 技能CD完成特效

    public GameObject SkillSeatCDFinishFx; // 切换位置CD完成特效

    private GameObject m_BuffItem;
    private string AtlasPath;
    private PartnerMD m_PartnerMD = null;
    private ParSkillDataCell m_ParSkillDataCell = null;

    private JButton m_jButton;

    public long M_PartnerID = 0;
    private int m_curSeat = -1;
    private int m_lastSeat = -1;

    private PartnerConcretizeReq m_PartnerConcretizeReq = new();

    private bool IsPlaySeatEffect = false;
    private float m_LeastSeatCD = 0;
    private float m_SkillCD = 0; // 技能CD倒计时（毫秒）
    private float m_SkillMaxCD = 0;
    private float m_DeadCountdown = 0; // 死亡复活倒计时(毫秒)
    private bool m_isOpenAutoRebornTime = false; // 是否开启自动复活时间
    private float m_DeadMaxCountdown = 0; // 死亡复活最大倒计时(秒)
    private float m_RebornTimeCountdown = 0; // 回血倒计时(秒)


    private Color m_DeadGrayColor = Color.white; // 死亡灰色

    // 血条相关
    private long m_healthSum = 100;
    private long m_healthCur = 100;

    private Action<Sprite> loadRoleIcon;
    private Action<Sprite> loadSkillIcon;

    /// <summary> 技能分发器 </summary>
    private SkillDispatcher m_SkillDispatcher;

    private List<SkillBuff> m_CurSkillBuffs = new(); // 当前的技能实体
    private List<PartnerBuffItem> m_CurBuffItemList = new(); // 当前在场的buffitem列表

    public void Awake()
    {
        m_jButton = GetComponent<JButton>();
        m_jButton.OnClick = OnSeatClick;

        if (ColorUtility.TryParseHtmlString("#999999", out m_DeadGrayColor))
        {
        }

        InitAnim();
    }

    public void FixedUpdate()
    {
        // 切换位置公共倒计时
        if (m_LeastSeatCD > 0)
        {
            m_LeastSeatCD -= TimeUtils.FixedDeltaTime;
            RefreshSeatCD(m_LeastSeatCD <= 0);
        }

        // 技能倒计时
        if (m_SkillCD > 0)
        {
            m_SkillCD -= TimeUtils.FixedDeltaTime;
            RefreshSkillCD();
        }

        // 死亡倒计时
        if (m_DeadCountdown > 0)
        {
            m_DeadCountdown -= TimeUtils.FixedDeltaTime;
            RefreshDeadCountdown();
        }

        // 客户端模拟回血
        if (m_isOpenAutoRebornTime)
        {
            if (m_RebornTimeCountdown > 0)
            {
                m_RebornTimeCountdown -= TimeUtils.FixedDeltaTime / 1000;
                UpdateReborn();
                if (m_RebornTimeCountdown <= 0)
                {
                    m_isOpenAutoRebornTime = false;
                }
            }
        }
    }

    public void OnDestroy()
    {
        Reset();

        m_jButton.OnClick = null;
        ReleaseBuffItem();
    }

    private void Reset()
    {
        OffEventMessage();

        m_PartnerMD = null;
        M_PartnerID = 0;
        m_ParSkillDataCell = null;

        m_curSeat = -1;
        m_lastSeat = -1;
        m_LeastSeatCD = 0;
        RefreshSeatCD(false);

        m_SkillCD = 0;
        m_SkillMaxCD = 0;
        m_DeadCountdown = 0;

        m_isOpenAutoRebornTime = false;
        m_RebornTimeCountdown = 0;

        m_healthSum = 100;
        m_healthCur = 100;

        loadRoleIcon = null;
        loadSkillIcon = null;

        ReleaseBindBuff();
        ReleaseBuffList();
        //ResetBuff();
        ReleaseBuffShieldDic();

        m_SkillDispatcher = null;
        m_PartnerMD = null;
        m_ParSkillDataCell = null;

        SkillCDFinishFx.SetActive(false);
        SkillCDFinishFxOne.SetActive(false);
        SkillSeatCDFinishFx.SetActive(false);
    }

    private void OnEventMessage()
    {
        GlobalEvent.OnPartnerEntityCreateFinish.AddListener(OnPartnerEntityCreateFinishCb);
        GlobalEvent.OnPartnerBloodUpdate.AddListener(RefreshBlood);
        GlobalEvent.OnPartnerDeadUpdate.AddListener(RefreshDead);
        //GlobalEvent.OnPartnerSkillCDUpdate.AddListener(RefreshSkillCD);

        GlobalEvent.OnPartnerCDUpdateNotice.AddListener(OnPartnerCDUpdateNoticeCb);
        GlobalEvent.OnShowGuidancePartner.AddListener(OnShowGuidancePartner);
        GlobalEvent.OnShowGuidancePartnerClean.AddListener(OnShowGuidancePartnerClean);
    }

    private void OffEventMessage()
    {
        GlobalEvent.OnPartnerEntityCreateFinish.RemoveListener(OnPartnerEntityCreateFinishCb);
        GlobalEvent.OnPartnerBloodUpdate.RemoveListener(RefreshBlood);
        GlobalEvent.OnPartnerDeadUpdate.RemoveListener(RefreshDead);
        //GlobalEvent.OnPartnerSkillCDUpdate.RemoveListener(RefreshSkillCD);

        GlobalEvent.OnPartnerCDUpdateNotice.RemoveListener(OnPartnerCDUpdateNoticeCb);
        GlobalEvent.OnShowGuidancePartner.RemoveListener(OnShowGuidancePartner);
        GlobalEvent.OnShowGuidancePartnerClean.AddListener(OnShowGuidancePartnerClean);
    }

    private void OnPartnerEntityCreateFinishCb(ulong entityId)
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        if (entityId != m_PartnerMD.ID)
        {
            return;
        }

        InitBuffByEntityID(m_PartnerMD.ID);
    }

    #region Buff

    private void InitBuffByEntityID(ulong entityID)
    {
        if (m_SkillDispatcher != null)
        {
            if (m_SkillDispatcher.EntityId == entityID)
            {
                return;
            }
        }

        ReleaseBindBuff();
        ResetBuff();

        EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(entityID);
        if (entityCtrlBase == null || entityCtrlBase.M_Curr == null)
        {
            return;
        }

        NPCEntityBase npcEntity = entityCtrlBase.M_Curr as NPCEntityBase;
        m_SkillDispatcher = npcEntity.skillDispatcher;
        InitGetBuffs(npcEntity);
        InitBindBuff();
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

    private void InitGetBuffs(NPCEntityBase npcEntity)
    {
        List<SkillBuff> buffs = npcEntity.GetSkillBuffs();
        for (int i = 0; i < buffs.Count - 1; i++)
        {
            var child = buffs[i];
            if (child != null && child.BuffInfo != null && child.BuffInfo.Cfg != null)
            {
                // 判断BUFF是否显示
                bool isShowThreeD = child.BuffInfo.GetHaveBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.HUDHpUp);
                if (isShowThreeD)
                {
                    if (!m_CurSkillBuffs.Contains(child))
                    {
                        AddBuffItem(child);
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} InitGetBuffs() buffid={child.BuffInfo.BuffID} buff已经存在了");
                    }
                }
                // 获取BUFF护盾总数
                CheckBuffShieldVal(child, true);
            }
        }
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

    private void ResetBuff()
    {
        ReleaseBuffList();
        ResetBuffItem();
    }

    private void OnActionOnBuffCreate(SkillBuff skillBuff, ProtoMsg.BuffCreateRet buffCreateRet)
    {
        if (skillBuff != null)
        {
            if (skillBuff.BuffInfo != null && skillBuff.BuffInfo.Cfg != null)
            {
                // 判断BUFF是否显示
                bool isShowThreeD = skillBuff.BuffInfo.GetHaveBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.HUDHpUp);
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

        PartnerBuffItem item = GetBuffItemBySkillBuff(skillBuff);
        if (item != null)
        {
            item.Release();
        }

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

        PartnerBuffItem item = GetBuffItemBySkillBuff(skillBuff);
        if (item != null)
        {
            m_CurSkillBuffs.Remove(item.BuffEntity); // 列表中删除
            m_CurSkillBuffs.Insert(0, skillBuff); // 添加更新后的buff
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

    #region 【BUFF】SkillBuffList

    private void AddSkillBuffList(SkillBuff skillBuff)
    {
        if (!m_CurSkillBuffs.Contains(skillBuff))
        {
            m_CurSkillBuffs.Insert(0, skillBuff);
        }
    }

    private void RemoveSkillBuffListByIndex(SkillBuff skillBuff)
    {
        if (m_CurSkillBuffs.Contains(skillBuff))
        {
            m_CurSkillBuffs.Remove(skillBuff);
        }
    }

    private void ReleaseBuffList()
    {
        m_CurSkillBuffs.Clear();
    }

    // 根据buff实体获取下标
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

    private void CreateBuffItem(SkillBuff skillBuff, int index)
    {
        if (m_BuffItem == null)
        {
            return;
        }

        var gob = GameObject.Instantiate<GameObject>(m_BuffItem);
        if (gob != null)
        {
            gob.transform.SetParent(BuffRoot.transform);
            gob.transform.localPosition = UnityEngine.Vector3.zero;
            gob.transform.localRotation = Quaternion.identity;
            gob.transform.SetLocalScale(UnityEngine.Vector3.one);
            UpdateBuffItem(skillBuff, gob, index);
        }
    }

    private void UpdateBuffItem(SkillBuff skillBuff, GameObject go, int index)
    {
        if (go == null)
        {
            return;
        }

        PartnerBuffItem item = go.GetComponent<PartnerBuffItem>();
        if (item != null)
        {
            item.SetBuffItem(skillBuff, BuffItemReleaseCb);
            item.SetIndex(index);
            AddBuffItemList(item);
        }
    }

    private void AddBuffItemList(PartnerBuffItem item)
    {
        if (!m_CurBuffItemList.Contains(item))
        {
            m_CurBuffItemList.Insert(0, item);
        }
    }

    private PartnerBuffItem GetIdleBuffItem()
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

    private PartnerBuffItem GetBuffItemBySkillBuff(SkillBuff skillBuff)
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
        PartnerBuffItem buffItem = GetIdleBuffItem();
        // 创建新的item
        if (buffItem == null)
        {
            CreateBuffItem(skillBuff, index);
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

        // 刷新在场的item index
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

    #endregion

    public void UpdateData(PartnerDataCell partnerDataCell, PartnerMD partnerMD, string atlasPath, GameObject buffItem)
    {
        if (partnerDataCell == null)
        {
            Reset();
            gameObject.SetActive(false);
            return;
        }

        if (partnerMD == null)
        {
            Reset();
            gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        // 判断时候是刚切换上来具象化出战的
        //if (m_PartnerMD != null)
        //{
        //    if (m_PartnerMD.State != PartnerState.Concretization && partnerMD.State == PartnerState.Concretization)
        //    {
        //        // 播放动画
        //        StartCoroutine(PlayParticleAnimationWithCallback(SkillClickFx));
        //    }
        //}
        m_DeadMaxCountdown = (float)partnerDataCell.GetRebornTime() / 1000;
        m_BuffItem = buffItem;
        m_PartnerMD = partnerMD;
        M_PartnerID = m_PartnerMD.Index;
        AtlasPath = atlasPath;
        m_ParSkillDataCell = LocalDataManager.Instance.GetParSkillDataCell((int)M_PartnerID, m_PartnerMD.CurStar);

        AvatarDataCell m_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(partnerDataCell.GetAvatarID());
        if (m_AvatarDataCell != null)
        {
            RefreshIcon(m_AvatarDataCell.GetHeadID());
        }

        InitDead();

        RefreshScale(m_PartnerMD.State);

        RefreshFrame(partnerDataCell.GetQuality());

        long hp = 0;
        if (m_PartnerMD.CurHp <= 0 && m_DeadCountdown == 0)
        {
            hp = 10000;
        }
        else
        {
            hp = m_PartnerMD.CurHp;
        }

        RefreshPercentumHp(hp);

        InitBuffByEntityID(m_PartnerMD.ID);

        InitSkill();

        OnEventMessage();
    }

    public void SetSeat(int curSeat, int lastSeat)
    {
        m_curSeat = curSeat;
        m_lastSeat = lastSeat;
    }

    private void RefreshScale(PartnerState partnerState)
    {
        if (transform == null)
        {
            return;
        }

        m_PartnerMD.State = partnerState;

        //float scaleXYZ = partnerState == PartnerState.Concretization ? 1.0f : 0.8f;
        //transform.SetScaleXYZ(scaleXYZ, scaleXYZ, scaleXYZ);

        //m_jButton.DoScale = partnerState != PartnerState.Concretization;
        float alpha = partnerState == PartnerState.Concretization ? 1f : 0f;
        BuffRoot.alpha = alpha;

        {
            CheckState.SetActive(partnerState == PartnerState.Concretization);
        }
        {
            //Vector3 scale = partnerState == PartnerState.Concretization ? Vector3.one : Vector3.one * 0.8f;
            //HPProgress.transform.parent.SetLocalScale(scale);
            //Vector3 pos = partnerState == PartnerState.Concretization
            //    ? new Vector3(-70, -38, 0)
            //    : new Vector3(0, -38, 0);
            //HPProgress.transform.parent.GetComponent<RectTransform>().anchoredPosition = pos;

            HPProgress.transform.parent.gameObject.SetActive(partnerState == PartnerState.Concretization);
        }
        {
            //Vector3 scale = partnerState == PartnerState.Concretization ? Vector3.one : Vector3.one * 0.8f;
            //Frame.transform.parent.SetLocalScale(scale);

            Vector3 pos = partnerState == PartnerState.Concretization
                           ? new Vector3(10, 7, 0)
                           : new Vector3(38, 4, 0);
            Frame.transform.parent.GetComponent<RectTransform>().anchoredPosition = pos;
        }
        {
            Vector3 pos = partnerState == PartnerState.Concretization
                ? new Vector3(-80, 15, 0)
                : new Vector3(-50, 0, 0);
            SkillIcon.transform.parent.GetComponent<RectTransform>().anchoredPosition = pos;

            float scale = partnerState == PartnerState.Concretization ? 1.3f : 1.1f;
            Vector3 scaleV3 = Vector3.one * scale;
            SkillIcon.transform.SetLocalScale(scaleV3);
        }
        {
            //Vector3 pos = partnerState == PartnerState.Concretization
            //     ? new Vector3(15, 17, 0)
            //     : new Vector3(50, 12, 0);
            //Icon.transform.parent.GetComponent<RectTransform>().anchoredPosition = pos;

            float height = partnerState == PartnerState.Concretization ? 150 : 135;
            Icon.transform.parent.GetComponent<RectTransform>().SetHeight(height);

            float posY = partnerState == PartnerState.Concretization ? 19 : 14;
            Icon.transform.parent.SetLocalPositionY(posY);
        }

        {
            LineDownIcon.SetActive(partnerState == PartnerState.Concretization);
            LineDownIcon2.SetActive(partnerState != PartnerState.Concretization);
        }

        //CheckBg.gameObject.SetActive(partnerState == PartnerState.Concretization);
        //CheckFrame.gameObject.SetActive(partnerState == PartnerState.Concretization);
        //CheckSkillBg.SetActive(partnerState == PartnerState.Concretization);

        //if (partnerState == PartnerState.Concretization)
        //{
        //    transform.SetAsFirstSibling();
        //}
    }

    private void RefreshIcon(int HeadID)
    {
        if (Icon == null)
        {
            return;
        }

        var headcfg = LocalDataManager.Instance.GetModelHeadDataCell(HeadID);
        if (headcfg != null)
        {
            loadRoleIcon = (Sprite sp) =>
            {
                if (sp != null)
                {
                    if (Icon != null)
                    {
                        Icon.sprite = sp;
                    }
                    if (SeatCDMask != null)
                    {
                        SeatCDMask.sprite = sp;
                    }
                }
            };
            AtlasManager.Instance.GetSpriteAsync(headcfg.HeadAtlasName, headcfg.MaxHead, loadRoleIcon);
        }
    }

    private void RefreshFrame(int quality)
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        if (Frame == null)
        {
            return;
        }

        var color = ColorDefine.Instance.GetColor($"Partner_Head_{quality}");
        Frame.color = color;
        //string frameIcon = $"Hud_Partner_Bg_{quality}";
        //AtlasManager.Instance.GetSpriteAsync(AtlasPath, frameIcon, (img) =>
        //{
        //    if (img != null)
        //    {
        //        Frame.sprite = img;
        //    }
        //});

    }

    private void RefreshBlood(uint partnerID, long blood, long maxBlood)
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        if (partnerID != M_PartnerID)
        {
            return;
        }

        m_healthCur = blood;
        m_healthSum = maxBlood;

        SetMixHpShield();
        //float ratio = m_healthSum == 0 ? 0 : (float)m_healthCur / (float)m_healthSum;
        ////SetUIBloodImage(ratio);
        //ratio *= 100;
        //SetUIPercentumHp(Mathf.RoundToInt(ratio));
    }

    private void RefreshPercentumHp(long percentum)
    {
        float _p = percentum > 0 ? (float)percentum / 100 : 0;
        //SetUIPercentumHp(Mathf.RoundToInt(_p));
        float ratio = _p / 100;
        HPProgress.fillAmount = ratio;
        ShieldValBarLeft.fillAmount = 0;
        ShieldValBarRight.fillAmount = 0;
        // < 100 就是不满百分百血量
        if (m_PartnerMD != null && m_PartnerMD.State != PartnerState.Concretization && _p < 100 && _p > 0)
        {
            // 需要客户端模拟回血
            m_isOpenAutoRebornTime = true;
            m_RebornTimeCountdown = (1 - ratio) * m_DeadMaxCountdown;
        }
        else
        {
            m_isOpenAutoRebornTime = false;
            m_RebornTimeCountdown = 0;
        }
    }

    private void UpdateReborn()
    {
        float ratio = (m_DeadMaxCountdown - m_RebornTimeCountdown) / m_DeadMaxCountdown;
        //SetUIPercentumHp(Mathf.RoundToInt(ratio * 100));
        HPProgress.fillAmount = ratio;
        ShieldValBarLeft.fillAmount = 0;
        ShieldValBarRight.fillAmount = 0;
    }

    private void SetUIPercentumHp(int percentum)
    {
        if (PercentumHp == null)
        {
            return;
        }

        PercentumHp.text = $"{percentum}%";
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
                HPProgress.fillAmount = ratio;
                float ratioShield = (float)ShieldValSum / (float)mixHp;
                ShieldValBarRight.fillAmount = ratioShield;
                ShieldValBarLeft.fillAmount = 0;
            }
            else
            {
                float ratio = m_healthSum == 0 ? 0 : (float)m_healthCur / (float)m_healthSum;
                HPProgress.fillAmount = ratio;
                float ratioShield = m_healthSum == 0 ? 0 : (float)ShieldValSum / (float)m_healthSum;
                ShieldValBarLeft.fillAmount = ratioShield + ratio;
                ShieldValBarRight.fillAmount = 0;
            }
        }
        else
        {
            float ratio = m_healthSum == 0 ? 0 : (float)m_healthCur / (float)m_healthSum;
            HPProgress.fillAmount = ratio;
            ShieldValBarLeft.fillAmount = 0;
            ShieldValBarRight.fillAmount = 0;
        }
    }

    private void RefreshDead(ulong entityID, bool isDead)
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        if (m_PartnerMD.ID != entityID)
        {
            return;
        }

        //if (DieState == null)
        //{
        //    return;
        //}
        //DieState.SetActive(isDead);
        // 伙伴死了就，吧头像置灰
    }

    private void InitDead()
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        m_DeadCountdown = Fire.Utils.CalculateCD(TimeManager.Instance.GetServerTimeStamp(), m_PartnerMD.ReviveTime);
        //SGF.Debuger.LogWarning($"{LOG_TAG} InitDead() ReviveTime={m_PartnerMD.ReviveTime},本地服务器时间={TimeManager.Instance.GetServerTimeStamp()}");

        bool isDead = m_DeadCountdown > 0;
        DeadCountdown.text = isDead ? Mathf.Round(m_DeadCountdown / 1000).ToString() : "";

        Frame.color = isDead ? m_DeadGrayColor : Color.white;
        Icon.color = isDead ? m_DeadGrayColor : Color.white;
        SkillIcon.color = isDead ? m_DeadGrayColor : Color.white;
        //CheckBg.color = isDead ? m_DeadGrayColor : Color.white;
        //CheckFrame.color = isDead ? m_DeadGrayColor : Color.white;

        //SGF.Debuger.LogWarning($"伙伴血量 初始化 id={M_PartnerID},Countdown={m_DeadCountdown},text={DeadCountdown.text}");
    }

    private void RefreshDeadCountdown()
    {
        if (m_PartnerMD == null)
        {
            return;
        }
        float countdown = Mathf.Round(m_DeadCountdown / 1000);
        if (countdown > 0)
        {
            DeadCountdown.text = countdown.ToString();
        }
        else
        {
            InitDead();
        }
    }

    private void InitSkill()
    {
        if (m_PartnerMD == null)
        {
            return;
        }

        if (m_ParSkillDataCell == null)
        {
            return;
        }

        InitSkillIcon();

        InitSkillCD();
    }

    private void InitSkillIcon()
    {
        if (SkillIcon == null)
        {
            return;
        }

        //PartnerSkillDescDataCell partnerSkillDescDataCell = LocalDataManager.Instance.GetPartnerSkillDescDataCellBySkillIdAndLevel((int)M_PartnerID, m_PartnerMD.CurStar);
        int curSkillId = m_ParSkillDataCell.GetParSkill();
        int skillLevel = m_ParSkillDataCell.GetParSkillLV();
        PartnerSkillDescDataCell partnerSkillDescDataCell = LocalDataManager.Instance.GetPartnerSkillDescDataCellBySkillIdAndLevel(curSkillId, skillLevel);
        if (partnerSkillDescDataCell != null)
        {
            loadSkillIcon = (Sprite sp) =>
            {
                if (SkillIcon != null && sp != null)
                {
                    SkillIcon.sprite = sp;
                }
            };
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill1, partnerSkillDescDataCell.IconPath, loadSkillIcon);
        }
    }

    private void InitSkillCD()
    {
        int curSkillId = m_ParSkillDataCell.GetParSkill();
        GameManager.Instance.GetPartnerSkillCDBySkillID(curSkillId, out m_SkillCD, out m_SkillMaxCD);
        RefreshSkillCD();
    }

    private void OnPartnerCDUpdateNoticeCb(int skillId)
    {
        if (TutorialCircle != null)
        {
            TutorialCircle.SetActive(false);
        }

        if (m_PartnerMD == null)
        {
            return;
        }

        if (m_ParSkillDataCell == null)
        {
            return;
        }

        int curSkillId = m_ParSkillDataCell.GetParSkill();
        if (skillId != curSkillId)
        {
            return;
        }

        GameManager.Instance.GetPartnerSkillCDBySkillID(curSkillId, out m_SkillCD, out m_SkillMaxCD);
    }

    private void RefreshSkillCD()
    {
        // 正在出战的伙伴不显示公共cd的倒计时
        if (m_PartnerMD == null)
        {
            return;
        }

        if (SkillCDProgress == null)
        {
            return;
        }

        if (m_SkillCD > 0 && m_SkillMaxCD > 0)
        {
            float ratio = m_SkillCD / m_SkillMaxCD;
            SkillCDProgress.fillAmount = ratio;
            SkillCDLab.text = (m_SkillCD / 1000).ToString("N0");
            SkillCDFinishFx.SetActive(false);
            SkillCDFinishFxOne.SetActive(false);
        }
        else
        {
            SkillCDLab.text = string.Empty;
            SkillCDProgress.fillAmount = 0;
            PlayTween(true);
        }
    }

    /// <summary>
    /// 开启 伙伴位 CD. 伙伴出战的时候, 点击伙伴头像, 切换伙伴到 出战位(第一个头像),
    /// 同时, 之前的 伙伴位 进入 cd 状态
    /// </summary>
    public void StartSeatCD(long start, long end)
    {
        m_LeastSeatCD = Fire.Utils.CalculateCD(start, end);
        IsPlaySeatEffect = true;
        //m_jButton.DoScale = m_LeastSeatCD <= 0;

        //SGF.Debuger.Log($"[PartnerItem_{M_PartnerID}] StartSeatCD : start {start} , end {end} , LeastSeatCD : {m_LeastSeatCD}");
        RefreshSeatCD(false);
        PlayChanageAnim();
    }
    public float SeatCDScale = 1.25f;
    private void RefreshSeatCD(bool isPlayAnim)
    {
        // TODO:2024/03/13 汉华需求
        // 打开座位CD显示
        // TODO:2023/07/04 汉华需求
        // 现在切换的公共CD不显示了
        //return;
        /// 2023/4/20
        /// 策划 伙伴验收模块中要求:
        /// 更换伙伴CD时，所有伙伴伙伴头像出现公共CD（更换伙伴CD）
        {
            //正在出战的伙伴不显示公共cd的倒计时
            if (m_PartnerMD != null && m_PartnerMD.State == PartnerState.Concretization)
            {
                return;
            }
        }

        bool showSeatCD = m_LeastSeatCD > 0;
        float time = m_LeastSeatCD / 1000.0f;
        //if (SeatCDLab != null)
        //{
        //    SeatCDLab.text = time.ToString("f0");
        //    SeatCDLab.gameObject.SetActive(showSeatCD);
        //}

        if (SeatCDMask != null)
        {
            SeatCDMask.fillAmount = (time + SeatCDScale) / ((float)SystemConstConfigs.Chanage_Partner_SeatCD);
            SeatCDMask.gameObject.SetActive(showSeatCD);

            SeatCDMask2.fillAmount = time / (float)SystemConstConfigs.Chanage_Partner_SeatCD;
            SeatCDMask2.gameObject.SetActive(showSeatCD);
        }

        if (SkillSeatCDFinishFx != null && IsPlaySeatEffect)
        {
            if (time <= 0/*(float)SystemConstConfigs.Chanage_Partner_SeatCD * 0.3f*/)
            {
                //SGF.Debuger.LogWarning($"提前播放座位CD完成动效--------{M_PartnerID}");
                SkillSeatCDFinishFx.SetActive(false);
                SkillSeatCDFinishFx.SetActive(true);
                IsPlaySeatEffect = false;
            }
        }
    }

    private void PlayChanageAnim()
    {
        string animName = string.Empty;
        if (m_curSeat == 0)
        {
            animName = "PartnerChange_Go"; // 出战
        }
        else if (m_curSeat < m_lastSeat)
        {
            animName = "PartnerChange_Up"; // 下
        }
        else if (m_curSeat > m_lastSeat)
        {
            animName = "PartnerChange_Under"; // 上
        }

        if (animName == string.Empty)
        {
            return;
        }

        //SGF.Debuger.LogError($"伙伴的位置 i={m_curSeat},id={m_PartnerMD?.Index},animName={animName}");

        PlayAnimation(animName, null);
    }

    public void MainPlayerBattleShowCDAnim(bool isPlay)
    {
        PlayTween(m_SkillCD <= 0 && isPlay);
    }

    // 播放CD完成的动画
    private void PlayTween(bool isPlay)
    {
        if (isPlay)
        {
            if (m_PartnerMD == null)
            {
                isPlay = false;
            }
        }
        SkillCDFinishFx.SetActive(false);
        SkillCDFinishFxOne.SetActive(false);
        if (isPlay)
        {
            if (m_PartnerMD.State != PartnerState.Concretization && BattleManager.Instance.IsSeverBattleState)
            {
                SkillCDFinishFx.SetActive(isPlay);
            }
            else
            {
                SkillCDFinishFxOne.SetActive(isPlay);
            }
        }
    }

    IEnumerator PlayParticleAnimationWithCallback(ParticleSystem particleSystem)
    {
        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
        yield return new WaitForSeconds(particleSystem.main.duration);
        particleSystem.gameObject.SetActive(false);
    }

    private void OnSeatClick(GameObject go)
    {
        // 已经出战的伙伴不能重复点击请求出战，服务器要求，因为他们不好判断
        if (m_PartnerMD == null)
        {
            return;
        }

        if (m_PartnerMD.State == PartnerState.Concretization)
        {
            StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("PartnerGoOutErrTip"));
            return;
        }

        if (m_LeastSeatCD > 0)
        {
            //StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage("CD中");
            return;
        }

        if (m_DeadCountdown > 0)
        {
            return;
        }

        m_PartnerConcretizeReq.Index = M_PartnerID;
        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, m_PartnerConcretizeReq, false);
    }

    public PartnerState GetPartnerState()
    {
        PartnerState partnerState = PartnerState.DefaultState;
        if (m_PartnerMD != null)
        {
            partnerState = m_PartnerMD.State;
        }

        return partnerState;
    }

    private void OnShowGuidancePartner(int partnerid)
    {
        if (M_PartnerID != partnerid || m_PartnerMD == null)
        {
            return;
        }
        if (TutorialCircle != null)
        {
            TutorialCircle.SetActive(true);
        }
    }

    private void OnShowGuidancePartnerClean(int index)
    {
        if (TutorialCircle != null)
        {
            TutorialCircle.SetActive(false);
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

    private void PlayAnimation(string AimationName, System.Action action = null)
    {
        if (Animancer == null)
        {
            return;
        }

        if (AnimationClips.TryGetValue(AimationName, out AnimationClip animationClip) && animationClip != null)
        {
            var state = Animancer.Play(animationClip);
            state.Events.OnEnd = () =>
            {
                state.IsPlaying = false;
                //Animancer.Playable.DestroyGraph();
                Animancer.Playable = null;
                action?.Invoke();
            };
            //var state = Animancer.Play(animationClip, 0.25f, FadeMode.FromStart);
            //state.Events.OnEnd = () => {
            //    state.IsPlaying = false;
            //    action?.Invoke();
            //};
        }
    }

    #endregion
}