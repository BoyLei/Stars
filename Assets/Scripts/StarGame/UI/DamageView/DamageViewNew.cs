using SGF.UI.Framework;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.WithOutLife.InfoEntity;
using StarProject.OffLine;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StarProject.Service.WorldToUI.WorldItemChecker;

public class DamageViewNew : MonoBehaviour
{
    private const string LOG_TAG = "[DamageViewNew]";

    private const string separator = "=";   // 分隔符

    public Transform parent;
    public Transform animGod;
    public Image statusIcon;
    public TextMeshProUGUI Text;

    private Animation m_animation;

    private RectTransform rect;
    private Canvas canvas;

    private DamageEntity m_damageEntity;

    private DamageTextDataCell damageTextDataCell;
    private bool isCrit = false;    // 是否暴击
    private bool isMainPlayer = false;  // 受击者是否是主角自己
    private bool isMainPlayerSummon;    // 攻击者是否主人召唤的
    private bool isAttackedPartner;    // 被攻击者是否伙伴
    private bool isAttackerPartner;    // 攻击者是否伙伴

    private Vector3 pointWorldPos = Vector3.zero;
    private bool isMove = false;

    private Action<Sprite> loadStatusIcon;

    private StringBuilder sb = new();
    private StringBuilder sb2 = new();


    #region 暴露出去可以调试

    public float Offsetx = 36f;
    public float OffsetX_min = 7.2f;
    public float OffsetX_max = 72f;

    public float OffsetY_min = 50.4f;
    public float OffsetY_max = 64.8f;

    #endregion

    private void Awake()
    {
        //anim = transform.GetComponent<Animator>();
        m_animation = transform.GetComponent<Animation>();
        rect = transform.GetComponent<RectTransform>();
        canvas = transform.GetComponent<Canvas>();


        Text.spriteAsset = BattleManager.Instance.CurLanguageBattleSprite;
    }

    private void LateUpdate()
    {
        if (isMove)
        {
            rect.anchoredPosition = PositionConvert.ConvertWorldToCanvasPosition(pointWorldPos);// 获取挂点位置
        }
    }

    /// <summary>
    /// 动画完成回调
    /// </summary>
    public void OnAnimFinish()
    {
        //SGF.Debuger.Log($"伤害飘字 动画完成 111111111 name={transform.name}");
        m_damageEntity.OnRemove();
    }

    /// <summary>
    /// 播放文本
    /// </summary>
    /// <param name="moveType">动画类型</param>
    private void PlayerText(E_TextMoveType moveType)
    {
        switch (moveType)
        {
            case E_TextMoveType.None:
                {
                    m_animation.Play("Heal");
                }
                break;
            case E_TextMoveType.LeftParabola:
                {
                    m_animation.Play("DamageLeft");
                }
                break;
            case E_TextMoveType.LeftParabolaCrit:
                {
                    m_animation.Play("Crit_L");
                }
                break;
            case E_TextMoveType.LeftParabolaNew:
                {
                    m_animation.Play("Attribute_L");
                }
                break;
            case E_TextMoveType.RightParabola:
                {
                    m_animation.Play("DamageRight");
                }
                break;
            case E_TextMoveType.RightParabolaCrit:
                {
                    m_animation.Play("Crit_R");
                }
                break;
            case E_TextMoveType.Bounce:
                {
                    m_animation.Play("Heal");
                }
                break;
            case E_TextMoveType.Floating:
                {
                    m_animation.Play("Buff");
                }
                break;
            case E_TextMoveType.FloatingNew:
                {
                    m_animation.Play("Heal0001");
                }
                break;
            case E_TextMoveType.Emerge:
                {
                    m_animation.Play("Heal");
                }
                break;
            case E_TextMoveType.CommonDamage:
                {
                    m_animation.Play("Damage");
                }
                break;
            case E_TextMoveType.Crit:
                {
                    m_animation.Play("Crit");
                }
                break;
            case E_TextMoveType.Common:
                {
                    m_animation.Play("Harm");
                }
                break;
            case E_TextMoveType.Crit2:
                {
                    m_animation.Play("Critical Hits");
                }
                break;
            default:
                {
                    m_animation.Play("Heal");
                }
                break;
        }
    }

    /// <summary>
    /// 设置文本
    /// </summary>
    /// <param name="text">文本</param>
    private void SetText(string text)
    {
        if (damageTextDataCell == null)
        {
            return;
        }
        float fontSize = damageTextDataCell.GetFontSize() * 1.0f;
        Text.fontSize = fontSize;
        int colorIndex = 0;
        if (damageTextDataCell != null)
        {
            colorIndex = damageTextDataCell.GetColorIndex();
        }
        // 如果没有文本信息，那就使用配置表的文本下标
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
        {
            sb2.Append($"<sprite={damageTextDataCell.GetTextIndex()}>");
        }
        else
        {
            if (m_damageEntity.hurtData != null && isAttackerPartner)
            {
                sb2.Append($"<sprite={(colorIndex * 20) + 10}>");
            }

            //if (isCrit)
            //{
            //    sb2.Append("<sprite=107>");
            //}
            var srtArr = text.Split(separator);
            foreach (string c in srtArr)
            {
                if (c == LanguageManager.Instance.GetLanguageByKey("SkillAbsorb"))
                {
                    sb2.Append($" <sprite={(colorIndex * 20) + 10 + 2}>");
                }
                else
                {
                    bool canConvert2Int = int.TryParse(c, out var number);
                    if (canConvert2Int)
                    {
                        sb2.Append($"<sprite={number + (colorIndex * 20)}>");
                    }
                    else
                    {
                        // 其他（中文、符号、）
                    }
                }
            }
            if (isCrit)
            {
                sb2.Append($"<sprite={(colorIndex * 20) + 10 + 1}>");
            }
        }
        Text.text = sb2.ToString();
        isMove =  true;
    }

    private void SetDamageDataText(string text)
    {
        if (damageTextDataCell == null)
        {
            return;
        }
        float fontSize = damageTextDataCell.GetFontSize() * 1.0f;
        Text.fontSize = fontSize;
        int colorIndex = 0;
        if (damageTextDataCell != null)
        {
            colorIndex = damageTextDataCell.GetColorIndex();
        }
        // 如果没有文本信息，那就使用配置表的文本下标
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
        {
            sb2.Append($"<sprite={damageTextDataCell.GetTextIndex()}>");
        }
        else
        {
            if (m_damageEntity.DamageData != null && isAttackerPartner)
            {
                sb2.Append($"<sprite={(colorIndex * 20) + 10}>");
            }
            //if (isCrit)
            //{
            //    sb2.Append("<sprite=107>");
            //}
            var srtArr = text.Split(separator);
            foreach (string c in srtArr)
            {
                if (c == LanguageManager.Instance.GetLanguageByKey("SkillAbsorb"))
                {
                    sb2.Append($" <sprite={(colorIndex * 20) + 10 + 2}>");
                }
                else
                {
                    bool canConvert2Int = int.TryParse(c, out var number);
                    if (canConvert2Int)
                    {
                        sb2.Append($"<sprite={number + (colorIndex * 20)}>");
                    }
                    else
                    {
                        // 其他（中文、符号、）
                    }
                }
            }
            if (isCrit)
            {
                sb2.Append($"<sprite={(colorIndex * 20) + 10 + 1}>");
            }
        }
        Text.text = sb2.ToString();
        isMove =  true;
    }

    public void Create(EntityObject entity)
    {
        Reset();
        m_damageEntity = (DamageEntity)entity;
        if (m_damageEntity != null)
        {
            if (m_damageEntity.attackedCtrlBase != null && m_damageEntity.attackerCtrlBase != null && m_damageEntity.attackerCtrlBase.Data != null && m_damageEntity.attackedCtrlBase.Data != null)
            {
                // 设置节点名【方便测试】
                {
                    string attackerId = m_damageEntity.attackerCtrlBase.Data.M_EntityID.ToString().Substring(m_damageEntity.attackerCtrlBase.Data.M_EntityID.ToString().Length - 4);
                    int attackerIdLength = attackerId.Length;
                    if (attackerIdLength > 4)
                    {
                        attackerId = attackerId.Substring(attackerIdLength - 4);
                    }
                    string victimId = m_damageEntity.attackedCtrlBase.Data.M_EntityID.ToString();
                    int targetIdLength = victimId.Length;
                    if (targetIdLength > 4)
                    {
                        victimId = victimId.Substring(targetIdLength - 4);
                    }
                    transform.name = $"{attackerId}->{victimId}";
                }
                // 判断1.是不是主角的召唤物 2.施法者是不是主角的召唤物
                isMainPlayerSummon = m_damageEntity.attackerCtrlBase.M_Curr.M_IsMainPlayerSummon;
                if (!isMainPlayerSummon && m_damageEntity.buildCtrlBase != null)
                {
                    isMainPlayerSummon = m_damageEntity.buildCtrlBase.M_Curr.M_IsMainPlayerSummon;
                }
                // 判断攻击者是伙伴
                isAttackerPartner = m_damageEntity.attackerCtrlBase.M_Curr.EntityType == E_EntityType.Partner;
                if (!isAttackerPartner && m_damageEntity.buildCtrlBase != null)
                {
                    isAttackerPartner = m_damageEntity.buildCtrlBase.M_Curr.EntityType == E_EntityType.Partner;
                }
                // 判断被攻击者是伙伴
                isAttackedPartner = m_damageEntity.attackedCtrlBase.M_Curr.EntityType == E_EntityType.Partner;

                //SGF.Debuger.Log($"召唤物 entityId={m_damageEntity.attackerId},isMainPlayerSummon={isMainPlayerSummon}");

                // 是否本人
                isMainPlayer = m_damageEntity.attackedCtrlBase.Data.M_EntityID == GameManager.Instance.mainPlayerId;
                // 是否暴击
                isCrit = m_damageEntity.hurtData != null ? m_damageEntity.hurtData.IsCrit : false;
                // 是否播放文字
                bool isDamageText = m_damageEntity.hurtData != null || !string.IsNullOrEmpty(m_damageEntity.damageText);
                // 飘字配置ID
                damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(m_damageEntity.damageTextId);

                // 根据不同的参数数据，修改
                if (isDamageText)
                {
                    SetDamageText();
                }
                else
                {
                    SetBuffView();
                }
                // 随机取一种动画类型
                //TextMoveType[] type = Enum.GetValues(typeof(TextMoveType)) as TextMoveType[];
                //System.Random random = new System.Random();
                //TextMoveType moveType = type[random.Next(1, type.Length)];
                //Init($"-{m_damageEntity.hurtData.Hurt}", "#FF0000", 10, moveType);
            }
            else
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} Create : not find attackID or attackerID, direct remove!!!");
                m_damageEntity.OnRemove();
            }
        }
    }


    public void BindDamageEntity(EntityObject entity)
    {
        //SGF.Debuger.Log("伤害飘字 逻辑层 初始化 00000000000");

        Reset();
        m_damageEntity = (DamageEntity)entity;
        if (m_damageEntity != null)
        {
            if (m_damageEntity.attackedCtrlBase != null && m_damageEntity.attackerCtrlBase != null && m_damageEntity.attackerCtrlBase.Data != null && m_damageEntity.attackedCtrlBase.Data != null)
            {
                // 设置节点名【方便测试】
                {
                    string attackerId = m_damageEntity.attackerCtrlBase.Data.M_EntityID.ToString().Substring(m_damageEntity.attackerCtrlBase.Data.M_EntityID.ToString().Length - 4);
                    int attackerIdLength = attackerId.Length;
                    if (attackerIdLength > 4)
                    {
                        attackerId = attackerId.Substring(attackerIdLength - 4);
                    }
                    string victimId = m_damageEntity.attackedCtrlBase.Data.M_EntityID.ToString();
                    int targetIdLength = victimId.Length;
                    if (targetIdLength > 4)
                    {
                        victimId = victimId.Substring(targetIdLength - 4);
                    }
                    transform.name = $"{attackerId}->{victimId}";
                }
                // 判断1.是不是主角的召唤物 2.施法者是不是主角的召唤物
                isMainPlayerSummon = m_damageEntity.attackerCtrlBase.M_Curr.M_IsMainPlayerSummon;
                if (!isMainPlayerSummon && m_damageEntity.buildCtrlBase != null)
                {
                    isMainPlayerSummon = m_damageEntity.buildCtrlBase.M_Curr.M_IsMainPlayerSummon;
                }
                // 判断攻击者是伙伴
                isAttackerPartner = m_damageEntity.attackerCtrlBase.M_Curr.EntityType == E_EntityType.Partner;
                if (!isAttackerPartner && m_damageEntity.buildCtrlBase != null)
                {
                    isAttackerPartner = m_damageEntity.buildCtrlBase.M_Curr.EntityType == E_EntityType.Partner;
                }
                // 判断被攻击者是伙伴
                isAttackedPartner = m_damageEntity.attackedCtrlBase.M_Curr.EntityType == E_EntityType.Partner;

                //SGF.Debuger.Log($"召唤物 entityId={m_damageEntity.attackerId},isMainPlayerSummon={isMainPlayerSummon}");

                // 是否本人
                isMainPlayer = m_damageEntity.attackedCtrlBase.Data.M_EntityID == GameManager.Instance.mainPlayerId;
                // 是否暴击
                isCrit = m_damageEntity.DamageData != null ? m_damageEntity.DamageData.IsCrit : false;
                // 是否播放文字
                bool isDamageText = m_damageEntity.DamageData != null || !string.IsNullOrEmpty(m_damageEntity.damageText);
                // 飘字配置ID
                damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(m_damageEntity.damageTextId);

                // 根据不同的参数数据，修改
                if (isDamageText)
                {
                    SetDamageDataText();
                }
                else
                {
                    SetBuffView();
                }
            }
            else
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} Create : not find attackID or attackerID, direct remove!!!");
                m_damageEntity.OnRemove();
            }
        }
    }


    /// <summary>
    /// 设置伤害文字
    /// </summary>
    private void SetDamageText()
    {
        // 如果有飘字配置-》直接走飘字配置
        // 如果没有，走配置表中前13种分类
        if (m_damageEntity.hurtData != null && damageTextDataCell == null)
        {
            if (isMainPlayer)
            {
                // 自身被暴击
                SetMainPlayerData();
            }
            else if (isAttackedPartner)
            {
                // 被攻击者是伙伴
                SetPartnerAttackedData();
            }
            else if (isAttackerPartner)
            {
                // 攻击者是伙伴
                SetPartnerAttackerData();
            }
            else
            {
                SetOtherData();
            }
        }
        if (damageTextDataCell == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetDamageText : damageTextDataCell == null");
            m_damageEntity.OnRemove();
            return;
        }
        // 伙伴对敌方的伤害飘字，要显示特殊的UI
        //if (m_damageEntity.hurtData != null && isAttackerPartner)
        //{
        //    // 播放配置表的图标
        //    string iconName = $"Icon_Partner";
        //    float scale = 1.0f;
        //    if (m_damageEntity.damageTextId > 0)
        //    {
        //        // 登场技
        //        iconName += m_damageEntity.hurtData.SuckBlood > 0 ? "4" : "2";
        //    }
        //    else
        //    {
        //        if (isCrit)
        //        {
        //            iconName += "3";
        //        }
        //        else
        //        {
        //            iconName += "1";
        //            scale = 0.8f;
        //        }
        //    }
        //    statusIcon.transform.SetLocalScale(Vector3.one * scale);
        //    loadStatusIcon = (Sprite sp) =>
        //    {
        //        if (sp != null && statusIcon != null)
        //        {
        //            statusIcon.sprite = sp;
        //            statusIcon.transform.SetLocalScale(Vector3.one);
        //            statusIcon.gameObject.SetActive(true);
        //        }
        //    };
        //    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, loadStatusIcon);
        //}
        //string textContent = "";
        E_TextMoveType actionType = (E_TextMoveType)damageTextDataCell.GetActionType();
        int displayArea = damageTextDataCell.GetDisplayArea();
        //// 二次设置动画类型
        //{
        //    if (actionType == E_TextMoveType.RightParabola)
        //    {
        //        if (m_damageEntity.StageType == E_StageType.Buff)
        //        {
        //            actionType = E_TextMoveType.LeftParabola;
        //        }
        //    }
        //}
        // 如果不是主角 并且 配置是左或右 抛物线 显示
        // 就根据坐标位置来确定是那个
        //if (!isMainPlayer && (actionType == E_TextMoveType.LeftParabola || actionType == E_TextMoveType.RightParabola))
        //{
        //    actionType = attackerPos.x > playerPos.x ? E_TextMoveType.LeftParabola : E_TextMoveType.RightParabola;
        //}
        // 文本
        {
            if (m_damageEntity.hurtData != null)
            {
                if (m_damageEntity.hurtData.Hurt > 0 && m_damageEntity.hurtData.ShieldValue <= 0)
                {
                    sb.Append(AddSeparator(m_damageEntity.hurtData.Hurt));
                    //textContent = AddSeparator(m_damageEntity.hurtData.Hurt);
                }
                else if (m_damageEntity.hurtData.Hurt > 0 && m_damageEntity.hurtData.ShieldValue > 0)
                {
                    //textContent = $"{AddSeparator(m_damageEntity.hurtData.Hurt)}{separator}{GameConfig.LocalStr["SkillAbsorb"]}{separator}{AddSeparator(m_damageEntity.hurtData.ShieldValue)}";
                    //sb.Append(AddSeparator(m_damageEntity.hurtData.Hurt)).Append(separator).Append(GameConfig.LocalStr["SkillAbsorb"]).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                    //sb.Append(AddSeparator(m_damageEntity.hurtData.Hurt)).Append(separator).Append(GameConfig.LocalStr["SkillAbsorb"]).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                    sb.Append(AddSeparator(m_damageEntity.hurtData.Hurt)).Append(separator).Append(LanguageManager.Instance.GetLanguageByKey("SkillAbsorb")).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                }
                else if (m_damageEntity.hurtData.ShieldValue > 0)
                {
                    //textContent = $"{GameConfig.LocalStr["SkillAbsorb"]}{separator}{AddSeparator(m_damageEntity.hurtData.ShieldValue)}";
                    //sb.Append(GameConfig.LocalStr["SkillAbsorb"]).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                    //sb.Append(GameConfig.LocalStr["SkillAbsorb"]).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                    sb.Append(LanguageManager.Instance.GetLanguageByKey("SkillAbsorb")).Append(separator).Append(AddSeparator(m_damageEntity.hurtData.ShieldValue));
                }
                else if (m_damageEntity.hurtData.SuckBlood > 0)
                {
                    //textContent = AddSeparator(m_damageEntity.hurtData.SuckBlood);
                    sb.Append(AddSeparator(m_damageEntity.hurtData.SuckBlood));
                }
            }
            //else
            //{
            //    textContent = m_damageEntity.damageText;
            //}
        }
        // 挂点位置
        {
            E_AnchorPresets e_AnchorPresets = displayArea == 1 ? E_AnchorPresets.Center : E_AnchorPresets.Up;
            // toto: 2024/5/16 汉化需求
            // 特殊临时判断  怪物模型是 战争巨人  就吧挂点改到中心
            pointWorldPos = GetModelPointPos(e_AnchorPresets, actionType);
            if (m_damageEntity.attackedCtrlBase.M_Curr.modelDataCell != null && m_damageEntity.attackedCtrlBase.M_Curr.modelDataCell.GetModleID() == 11501)
            {
                pointWorldPos.y -= 10.0f;
            }
            int order = WorldItemChecker.Instance.GetOverlayRenderGroupDistOrder(pointWorldPos, E_OverLayTypeOrderBase.BattleMesg);
            DynamicUIRoot.SetDamagePartnt(order, transform);
            canvas.sortingOrder = order;
            rect.SetLocalPositionZ(0);
            rect.anchoredPosition = PositionConvert.ConvertWorldToCanvasPosition(pointWorldPos);// 获取挂点位置
            //SGF.Debuger.Log($"模型 头顶坐标 v3={v3}，，pos={transform.position}");
        }

        SetText(sb.ToString());
        PlayerText(actionType);
    }

    private void SetDamageDataText()
    {
        // 如果有飘字配置-》直接走飘字配置
        // 如果没有，走配置表中前13种分类
        if (m_damageEntity.DamageData != null && damageTextDataCell == null)
        {
            GetDamageTextDataCell();
        }
        if (damageTextDataCell == null)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} SetDamageDataText : damageTextDataCell == null");
            m_damageEntity.OnRemove();
            return;
        }
        E_TextMoveType actionType = (E_TextMoveType)damageTextDataCell.GetActionType();
        int displayArea = damageTextDataCell.GetDisplayArea();
        // 文本
        {
            if (m_damageEntity.DamageData != null && m_damageEntity.DamageData.Hurt > 0)
            {
                switch (m_damageEntity.DamageData.HurtType)
                {
                    case E_HurtType.Hurt_Null:
                    case E_HurtType.Hurt_Physic:
                    case E_HurtType.Hurt_Magic:
                    case E_HurtType.Hurt_Elem1:
                    case E_HurtType.Hurt_Elem2:
                    case E_HurtType.Hurt_Elem3:
                    case E_HurtType.Hurt_Elem4:
                    case E_HurtType.Hurt_Elem5:
                    case E_HurtType.Hurt_Elem6:
                    case E_HurtType.SuckBlood:
                    case E_HurtType.ThornsBlood:
                        {
                            sb.Append(AddSeparator(m_damageEntity.DamageData.Hurt));
                        }
                        break;
                    case E_HurtType.ShieldValue:
                        {
                            //sb.Append(GameConfig.LocalStr["SkillAbsorb"]).Append(separator).Append(AddSeparator(m_damageEntity.DamageData.Hurt));
                            sb.Append(LanguageManager.Instance.GetLanguageByKey("SkillAbsorb")).Append(separator).Append(AddSeparator(m_damageEntity.DamageData.Hurt));
                        }
                        break;
                    default:
                        {
                            sb.Append(AddSeparator(m_damageEntity.DamageData.Hurt));
                        }
                        break;
                }
            }
        }
        // 挂点位置
        {
            E_AnchorPresets e_AnchorPresets = displayArea == 1 ? E_AnchorPresets.Center : E_AnchorPresets.Up;
            // toto: 2024/5/16 汉化需求
            // 特殊临时判断  怪物模型是 战争巨人  就吧挂点改到中心
            pointWorldPos = GetModelPointPos(e_AnchorPresets, actionType);
            //if (m_damageEntity.attackedCtrlBase.M_Curr.modelDataCell != null && m_damageEntity.attackedCtrlBase.M_Curr.modelDataCell.GetModleID() == 11501)
            //{
            //    pointWorldPos.y -= 10.0f;
            //}
            int order = WorldItemChecker.Instance.GetOverlayRenderGroupDistOrder(pointWorldPos, E_OverLayTypeOrderBase.BattleMesg);
            DynamicUIRoot.SetDamagePartnt(order, transform);
            canvas.sortingOrder = order;
            rect.SetLocalPositionZ(0);
            rect.anchoredPosition = PositionConvert.ConvertWorldToCanvasPosition(pointWorldPos);// 获取挂点位置
            //SGF.Debuger.Log($"模型 头顶坐标 v3={v3}，，pos={transform.position}");
        }

        SetDamageDataText(sb.ToString());
        PlayerText(actionType);
    }


    private Vector3 GetModelPointPos(E_AnchorPresets e_AnchorPresets, E_TextMoveType moveType)
    {
        Vector3 pos = Vector3.zero;
        Vector2 parentPos = Vector3.zero;
        switch (moveType)
        {
            case E_TextMoveType.None:
            case E_TextMoveType.Bounce:
            case E_TextMoveType.Floating:
            case E_TextMoveType.FloatingNew:
            case E_TextMoveType.Emerge:
            case E_TextMoveType.CommonDamage:
            case E_TextMoveType.Crit:
                {
                    parentPos.x = UnityEngine.Random.Range(-Offsetx, Offsetx);
                }
                break;
            case E_TextMoveType.LeftParabola:
            case E_TextMoveType.LeftParabolaCrit:
            case E_TextMoveType.LeftParabolaNew:
                {
                    parentPos.x = UnityEngine.Random.Range(-OffsetX_min, -OffsetX_max);
                }
                break;
            case E_TextMoveType.RightParabola:
            case E_TextMoveType.RightParabolaCrit:
                {
                    parentPos.x = UnityEngine.Random.Range(OffsetX_min, OffsetX_max);
                }
                break;
            case E_TextMoveType.Common:
            case E_TextMoveType.Crit2:
                {
                    parentPos.x = UnityEngine.Random.Range(-Offsetx, Offsetx);

                }
                break;
            default:
                {
                    parentPos.x = UnityEngine.Random.Range(-Offsetx, Offsetx);
                }
                break;
        }
        ModelOffLineData modelOffLineData = m_damageEntity.attackedCtrlBase.Container.GetComponentInChildren<ModelOffLineData>();
        if (modelOffLineData != null)
        {
            Transform UnitInfoPoint = null;
            if (e_AnchorPresets == E_AnchorPresets.Up)
            {
                UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.UnitInfo_D.ToString());
                if (UnitInfoPoint != null)
                {
                    pos = UnitInfoPoint.position;
                    //Transform RootPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                    //float height = UnitInfoPoint.position.y - RootPoint.position.y;
                    parentPos.y = UnityEngine.Random.Range(OffsetY_min, OffsetY_max);
                }
                else
                {
                    UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                    if (UnitInfoPoint != null)
                    {
                        pos = UnitInfoPoint.position;
                        pos.y += 2.0f;
                        parentPos.y = UnityEngine.Random.Range(OffsetY_min, OffsetY_max);
                    }
                }
            }
            else if (e_AnchorPresets == E_AnchorPresets.Center)
            {
                UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.Hurt_D.ToString());
                if (UnitInfoPoint != null)
                {
                    pos = UnitInfoPoint.position;
                    parentPos.y = UnityEngine.Random.Range(OffsetY_min, OffsetY_max);
                }
                else
                {
                    UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                    if (UnitInfoPoint != null)
                    {
                        pos = UnitInfoPoint.position;
                        pos.y += 2.0f;
                        parentPos.y = UnityEngine.Random.Range(OffsetY_min, OffsetY_max);
                    }
                }
            }
            else
            {
                UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                if (UnitInfoPoint != null)
                {
                    pos = UnitInfoPoint.position;
                    pos.y += 2.0f;
                    parentPos.y = UnityEngine.Random.Range(OffsetY_min, OffsetY_max);
                }
            }
        }
        parent.GetComponent<RectTransform>().anchoredPosition = parentPos;
        return pos;
    }

    private void SetMainPlayerData()
    {
        int id = 1101;  // 默认普通掉血
        if (!m_damageEntity.hurtData.Ishit)
        {
            // 未命中
            id = 1301;
        }
        else if (m_damageEntity.hurtData.Hurt > 0 && m_damageEntity.StageType == E_StageType.Buff)
        {
            // buff攻击掉血
            id = isCrit ? 1104 : 1102;
        }
        else if (m_damageEntity.hurtData.Hurt > 0)
        {
            // 普通攻击掉血
            id = isCrit ? 1103 : 1101;
        }
        else if (m_damageEntity.hurtData.SuckBlood > 0)
        {
            // 吸血治疗
            id = isCrit ? 1202 : 1201;
        }

        damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(id);
    }

    private void GetDamageTextDataCell()
    {
        int id = 4010;
        if (m_damageEntity.DamageData != null)
        {
            if (isMainPlayer)
            {
                switch (m_damageEntity.DamageData.HurtType)
                {
                    case E_HurtType.Hurt_Null:
                    case E_HurtType.Hurt_Physic:
                    case E_HurtType.Hurt_Magic:
                    case E_HurtType.Hurt_Elem1:
                    case E_HurtType.Hurt_Elem2:
                    case E_HurtType.Hurt_Elem3:
                    case E_HurtType.Hurt_Elem4:
                    case E_HurtType.Hurt_Elem5:
                    case E_HurtType.Hurt_Elem6:
                        id = 3000;
                        break;
                    case E_HurtType.SuckBlood:
                        id = isCrit ? 4101 : 4100;
                        break;
                    case E_HurtType.ThornsBlood:
                        id = isCrit ? 4201 : 4200;
                        break;
                    case E_HurtType.ShieldValue:
                        id = isCrit ? 4301 : 4300;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (m_damageEntity.DamageData.HurtType)
                {
                    case E_HurtType.Hurt_Null:
                        break;
                    case E_HurtType.Hurt_Physic:
                        id = isCrit ? 4011 : 4010;
                        break;
                    case E_HurtType.Hurt_Magic:
                        id = isCrit ? 4021 : 4020;
                        break;
                    case E_HurtType.Hurt_Elem1:
                        id = isCrit ? 4031 : 4030;
                        break;
                    case E_HurtType.Hurt_Elem2:
                        id = isCrit ? 4051 : 4050;
                        break;
                    case E_HurtType.Hurt_Elem3:
                        id = isCrit ? 4041 : 4040;
                        break;
                    case E_HurtType.Hurt_Elem4:
                        id = isCrit ? 4061 : 4060;
                        break;
                    case E_HurtType.Hurt_Elem5:
                        id = isCrit ? 4071 : 4070;
                        break;
                    case E_HurtType.Hurt_Elem6:
                        id = isCrit ? 4081 : 4080;
                        break;
                    case E_HurtType.SuckBlood:
                        id = isCrit ? 4101 : 4100;
                        break;
                    case E_HurtType.ThornsBlood:
                        id = isCrit ? 4201 : 4200;
                        break;
                    case E_HurtType.ShieldValue:
                        id = isCrit ? 4301 : 4300;
                        break;
                    default:
                        break;
                }
            }
        }

        damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(id);
    }


    private void SetPartnerAttackedData()
    {
        int id = 3101;  // 默认普通掉血
        if (!m_damageEntity.hurtData.Ishit)
        {
            // 未命中
            id = 3105;
        }
        else if (m_damageEntity.hurtData.Hurt > 0 && m_damageEntity.StageType == E_StageType.Buff)
        {
            // buff攻击掉血
            id = isCrit ? 3104 : 3102;
        }
        else if (m_damageEntity.hurtData.Hurt > 0)
        {
            // 普通攻击掉血
            id = isCrit ? 3103 : 3101;
        }
        else if (m_damageEntity.hurtData.SuckBlood > 0)
        {
            // 吸血治疗
            id = isCrit ? 3108 : 3107;
        }

        damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(id);
    }

    private void SetPartnerAttackerData()
    {
        int id = 3109;  // 默认普通掉血
        if (!m_damageEntity.hurtData.Ishit)
        {
            // 未命中
            id = 3113;
        }
        else if (m_damageEntity.hurtData.Hurt > 0 && m_damageEntity.StageType == E_StageType.Buff)
        {
            // buff攻击掉血
            id = isCrit ? 3112 : 3110;
        }
        else if (m_damageEntity.hurtData.Hurt > 0)
        {
            // 普通攻击掉血
            id = isCrit ? 3111 : 3109;
        }
        else if (m_damageEntity.hurtData.SuckBlood > 0)
        {
            // 吸血治疗
            id = isCrit ? 3108 : 3107;
        }

        damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(id);
    }

    private void SetOtherData()
    {
        int id = 2101;
        if (!m_damageEntity.hurtData.Ishit)
        {
            id = 2402;
        }
        else if (isMainPlayerSummon && m_damageEntity.hurtData.Hurt > 0)
        {
            if (m_damageEntity.StageType == E_StageType.Buff)
            {
                id = isCrit ? 2104 : 2102;
                //id = isCrit ? 2204 : 2202;
            }
            else
            {
                id = isCrit ? 2103 : 2101;
                //id = isCrit ? 2203 : 2201;
            }
        }
        else if (m_damageEntity.hurtData.Hurt > 0)
        {
            if (m_damageEntity.StageType == E_StageType.Buff)
            {
                id = isCrit ? 2104 : 2102;
            }
            else
            {
                id = isCrit ? 2103 : 2101;
            }
        }
        else if (m_damageEntity.hurtData.SuckBlood > 0)
        {
            id = isCrit ? 2302 : 2301;
        }
        else
        {
            // 否者就miss
            id = 2402;
        }
        damageTextDataCell = LocalDataManager.Instance.GetDamageTextDataCell(id);
    }

    /// <summary>
    /// 设置BUFF
    /// </summary>
    private void SetBuffView()
    {
        if (m_damageEntity.addEffectFly.Count > 0)
        {
            string textContent = "";
            E_TextMoveType ActionType = E_TextMoveType.Floating;
            string type = m_damageEntity.addEffectFly[0];
            if (type == "1")
            {
                // 播放配置表的文字
                textContent = m_damageEntity.addEffectFly[1];
            }
            else if (type == "2")
            {
                // 播放配置表的图标
                string path = $"Icon/Buff/Icon/{m_damageEntity.addEffectFly[1]}";
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAsync(path, (Sprite img) =>
                {
                    if (img != null && statusIcon != null)
                    {
                        statusIcon.sprite = img;
                        statusIcon.gameObject.SetActive(true);
                    }
                });
            }

            if (m_damageEntity.attackedCtrlBase != null && m_damageEntity.attackerCtrlBase != null)
            {
                Vector3 playerPos = m_damageEntity.attackedCtrlBase.M_Curr.Position();
                Vector3 attackerPos = m_damageEntity.attackerCtrlBase.M_Curr.Position();
                ActionType = attackerPos.x > playerPos.x ? E_TextMoveType.LeftParabola : E_TextMoveType.RightParabola;
            }

            if (!string.IsNullOrEmpty(textContent))
            {
                SetText(textContent);
            }
            PlayerText(ActionType);
        }
        else
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} show buffView is err. remove");
            m_damageEntity.OnRemove();
        }
    }

    private string AddSeparator(long number)
    {
        string numberStr = number.ToString();
        char[] digits = numberStr.ToCharArray();

        string result = string.Join(separator.ToString(), digits);
        return result;
    }

    public void Reset()
    {
        Text.text = "";
        //Text.transform.localPosition = Vector3.zero; // 默认挂点到实体中间
        parent.transform.localPosition = Vector3.zero;
        //anim.transform.localPosition = Vector3.zero;
        statusIcon.gameObject.SetActive(false);
        //SGF.Debuger.Log($"伤害飘字 重置数据 3333333333 name={transform.name}");
        if (m_animation.isPlaying)
        {
            m_animation.Stop();
        }

        m_damageEntity = null;

        damageTextDataCell = null;
        isCrit = false;    // 是否暴击
        isMainPlayer = false;  // 是否是主角自己
        isMainPlayerSummon = false;    // 是否主人召唤的
        isAttackedPartner = false;
        isAttackerPartner = false;
        pointWorldPos = Vector3.zero;
        isMove = false;
        loadStatusIcon = null;

        sb.Clear();
        sb2.Clear();

        statusIcon.gameObject.SetActive(false);
        transform.SetLocalScale(Vector3.one);
        Text.transform.SetLocalScale(Vector3.one);
        animGod.SetLocalScale(Vector3.one);
    }
}
