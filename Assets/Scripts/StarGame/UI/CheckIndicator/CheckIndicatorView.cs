using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Game.Skill.Utils;
using StarProject.Service.Battle;
using StarProjectDef;
using System;
using UnityEngine;

/// <summary>
/// 选中指示器
/// </summary>
public class CheckIndicatorView : MonoBehaviour
{
    private string LOG_TAG = "[CheckIndicatorView]";

    public SpriteRenderer m_LockingBg;

    private const string m_Path = "UI/StarWorld/Textures/";

    private GameContext m_context;
    private EntityCtrlBase m_entityCtrl;
    private bool m_isShow = false;
    private bool m_isFlashHide = false;

    #region 资源异步加载回调

    private Action<Sprite> loadIcon;

    #endregion

    public void Init(EntityCtrlBase entityCtrl)
    {
        m_entityCtrl = entityCtrl;
        if (m_entityCtrl == null)
        {
            SGF.Debuger.Log($"{LOG_TAG} Init m_entityCtrl=null");
            return;
        }
        Vector3 pos = Vector3.zero;
        pos.y = 0.1f;
        transform.localPosition = pos;
        m_isShow = true;

        BattleManager.EntityRelationIconPath iconPath = null;
        // 主角-》敌对-》队伍-》工会-》非好友
        switch (m_entityCtrl.M_Curr.EntityType)
        {
            case E_EntityType.None:
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.None);
                }
                break;
            case E_EntityType.Player:
                {
                    if (m_entityCtrl.Data.isMainPlayer)
                    {
                        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerMain);
                    }
                    else
                    {
                        bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
                        if (isTargetNtt)
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerOtherHostility);
                        }
                        else
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PlayerOtherFriendly);
                        }
                    }
                }
                break;
            case E_EntityType.Npc:
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.NPC);
                }
                break;
            case E_EntityType.Monster:
            case E_EntityType.GVEBoss:
            case E_EntityType.Robot:
                {
                    bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
                    if (isTargetNtt)
                    {
                        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterNormal);
                    }
                    else
                    {
                        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.MonsterOtherFriendly);
                    }
                }
                break;
            case E_EntityType.Summon:
                {
                    if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
                    {
                        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonMainPlayer);
                    }
                    else
                    {
                        bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
                        if (isTargetNtt)
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherHostility);
                        }
                        else
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.SummonOtherFriendly);
                        }
                    }
                }
                break;
            case E_EntityType.RoomSpace:
            case E_EntityType.BulletEntity:
            case E_EntityType.Interact:
            case E_EntityType.Partner:
                {
                    if (m_entityCtrl.M_Curr.M_IsMainPlayerSummon)
                    {
                        iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerMainPlayer);
                    }
                    else
                    {
                        bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(m_entityCtrl.M_Curr);
                        if (isTargetNtt)
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerOtherHostility);
                        }
                        else
                        {
                            iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.PartnerOtherFriendly);
                        }
                    }
                }
                break;
            default:
                {
                    iconPath = BattleManager.Instance.GetEntityRelationIconPath(E_EntityRelationType.None);
                }
                break;
        }

        // 主角-》敌对-》队伍-》工会-》非好友
        // 根据加载血条
        loadIcon = (Sprite img) =>
        {
            if (img != null && m_LockingBg != null && m_entityCtrl != null && m_entityCtrl.M_Curr != null)
            {
                m_LockingBg.sprite = img;
                Vector2 size = Vector2.one * (m_entityCtrl.M_Curr.ModelRadius * 2);
                m_LockingBg.size = size;
            }
        };
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAsync($"{m_Path}{iconPath._checkCircle}", loadIcon);
    }
    internal void EnterFrame(int frameIndex)
    {

    }
    public void Release()
    {
        if (m_entityCtrl != null)
        {
            //
        }
        loadIcon = null;
        GameObject.Destroy(gameObject);
    }

    public void SetFlashHide(bool isHide)
    {
        if (m_isFlashHide == isHide)
        {
            return;
        }
        m_isFlashHide = isHide;
        if (m_isFlashHide)
        {
            gameObject.SetActive(false);
        }
        else if (m_isShow)
        {
            gameObject.SetActive(true);
        }
    }

    public void SetCheckActive(bool isShow)
    {
        if (m_isShow == isShow)
        {
            return;
        }
        m_isShow = isShow;
        if (!m_isShow)
        {
            gameObject.SetActive(false);
        }
        else if (!m_isFlashHide)
        {
            gameObject.SetActive(true);
        }
    }
}
