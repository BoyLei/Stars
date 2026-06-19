using StarProject.Game.Skill;
using StarProject.Service.AtlasManager;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PartnerBuffItem : MonoBehaviour
{
    public Image BuffBg;     // BUFF背景
    public Image BuffFrame;     // BUFF边框倒计时
    public Image BuffIcon;     // BUFF图标
    public Text BuffFloor;          // BUFF层数

    [HideInInspector]
    public bool IsUsed = false;
    [HideInInspector]
    public int Index = -1;

    private AnimParamsShow AnimComp = null;

    public SkillBuff BuffEntity = null;

    private bool m_IsShowCountdown = false;
    private float m_MaxTime;
    private float m_Countdown;
    private Action<SkillBuff> m_ReleaseCB = null;

    private string m_IconBgPath = "";
    private string m_IconPath = "";
    private string m_FramePath = "";

    private bool isPlayFade = false;

    private Action<Sprite> loadBuffBg;
    private Action<Sprite> loadBuffFrame;
    private Action<Sprite> loadBuffIcon;

    private void Awake()
    {
        if (BuffBg != null)
        {
            AnimComp = BuffBg.transform.GetComponent<AnimParamsShow>();
        }
    }

    public void SetBuffItem(SkillBuff buffEntity, Action<SkillBuff> releaseCB)
    {
        Reset();

        if (buffEntity == null)
        {
            gameObject.SetActive(false);
            return;
        }

        m_ReleaseCB = releaseCB;
        IsUsed = true;

        SetBuffData(buffEntity, true);
    }

    public void SetBuffData(SkillBuff buffEntity, bool isFirst = false)
    {
        BuffEntity = buffEntity;
        if (BuffEntity.BuffInfo != null)
        {
            transform.name = $"{BuffEntity.RuntimeID}_{BuffEntity.BuffID}";

            int state = BuffEntity.BuffInfo.GetBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.HUDHpUp);
            // 是否显示层数
            bool isShowFloor = CheckState(state, (int)SkillEditor.BuffUIShowDetailsEnum.ShowFloor);
            if (isShowFloor)
            {
                BuffFloor.text = BuffEntity.StackCount > 1 ? BuffEntity.StackCount + "" : "";
            }
            // 是否显示倒计时
            m_IsShowCountdown = CheckState(state, (int)SkillEditor.BuffUIShowDetailsEnum.ShowCountdown);
            if (m_IsShowCountdown)
            {
                m_MaxTime = BuffEntity.MaxTime / 1000;
                m_Countdown = (float)BuffEntity.GetEndime() / 1000;
                isPlayFade = true;
            }
            else
            {
                isFirst = false;
            }
            // 是否显示图标
            bool isShowIcon = CheckState(state, (int)SkillEditor.BuffUIShowDetailsEnum.ShowIcon);
            if (isShowIcon)
            {
                // 背景
                string iconBgPath = BuffEntity.BuffInfo.Cfg.IsDebuff ? "Hud_Debuffbg_01" : "Hud_Buffbg_01";
                if (m_IconBgPath != iconBgPath)
                {
                    loadBuffBg = (Sprite sp) =>
                    {
                        if (BuffBg != null && sp != null)
                        {
                            BuffBg.sprite = sp;
                            m_IconBgPath = iconBgPath;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathBuff, iconBgPath, loadBuffBg);
                }
                // 边框
                string framePath = BuffEntity.BuffInfo.Cfg.IsDebuff ? "Hud_Debuff_Countdown" : "Hud_Buff_Countdown";
                if (m_FramePath != framePath)
                {
                    loadBuffFrame = (Sprite sp) =>
                    {
                        if (BuffFrame != null && sp != null)
                        {
                            BuffFrame.sprite = sp;
                            m_FramePath = framePath;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathBuff, framePath, loadBuffFrame);
                }
                // 图标
                string buffIconPath = BuffEntity.GetBuffIconPath();
                if (!string.IsNullOrEmpty(buffIconPath))
                {
                    if (m_IconPath != buffIconPath)
                    {
                        loadBuffIcon = (Sprite sp) =>
                        {
                            if (BuffIcon != null && sp != null)
                            {
                                BuffIcon.sprite = sp;
                                m_IconPath = buffIconPath;
                            }
                        };
                        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathBuff, buffIconPath, loadBuffIcon);
                    }
                }
            }
            gameObject.SetActive(true);
            if (isFirst)
            {
                transform.SetAsFirstSibling();
                PlayScaleAnim();
            }
        }
    }

    public void SetIndex(int index)
    {
        Index = index;
        if (Index >= GameConfig.BUFF_SHOW_MAX_COUNT)
        {
            BuffBg.gameObject.SetActive(false);
        }
        else
        {
            BuffBg.gameObject.SetActive(true);
        }
    }

    public void Release()
    {
        if (m_ReleaseCB != null)
        {
            m_ReleaseCB.Invoke(BuffEntity);
        }
        Reset();
        gameObject.SetActive(false);
    }

    private void Reset()
    {
        IsUsed = false;
        BuffEntity = null;
        Index = -1;
        m_IsShowCountdown = false;
        m_MaxTime = 0;
        m_Countdown = 0;
        m_ReleaseCB = null;
        isPlayFade = true;

        loadBuffBg = null;
        loadBuffFrame = null;
        loadBuffIcon = null;

        BuffFloor.text = "";
    }

    private void Update()
    {
        if (m_Countdown > 0)
        {
            m_Countdown -= Time.deltaTime;

            // 播放快消失的闪烁动画
            if (m_Countdown <= GameConfig.UI_BUFF_FADE_MIN_TIME && isPlayFade)
            {
                PlayFadeAnim();
                isPlayFade = false;
            }

            if (m_Countdown <= 0)
            {
                // 播放消失动画
                Release();
            }
        }
    }

    private void OnDestroy()
    {
        Reset();
    }

    private bool CheckState(int state, int checkState)
    {
        return (state & checkState) == checkState;
    }

    public void PlayScaleAnim()
    {
        if (AnimComp != null)
        {
            AnimComp.PlayAnimByType(AnimType.Scale);
        }
    }

    private void PlayFadeAnim()
    {
        if (AnimComp != null)
        {
            AnimComp.PlayAnimByType(AnimType.Fade);
        }
    }


}
