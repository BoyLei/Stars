using DG.Tweening;
using Sirenix.OdinInspector;
using StarProject.Game.Skill;
using StarProject.Service.AtlasManager;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ThreeDBuffItemNew : MonoBehaviour
{
    [LabelText("BUFF背景")]
    public Image BuffBg;
    [LabelText("BUFF边框倒计时")]
    public Image BuffFrame;
    [LabelText("BUFF图标")]
    public Image BuffIcon;
    [LabelText("BUFF层数")]
    public Text BuffFloor;

    [HideInInspector]
    public bool IsUsed = false;
    [HideInInspector]
    public int Index = -1;

    private AnimParamsShow ScaleAnim = null;
    private AnimParamsShow FadeFrameAnim = null;
    private AnimParamsShow FadeIconAnim = null;
    private AnimParamsShow FadeFloorAnim = null;

    public SkillBuff BuffEntity = null;

    private bool m_IsShowCountdown = false;
    private float m_MaxTime;
    private float m_Countdown;
    private Action<SkillBuff> m_ReleaseCB = null;

    private string m_IconBgPath = "";
    private string m_IconPath = "";
    private string m_FramePath = "";

    private bool isPlayFade = false;

    private void Awake()
    {
        if (BuffBg != null)
        {
            ScaleAnim = BuffBg.transform.GetComponent<AnimParamsShow>();
        }
        if (BuffFrame != null)
        {
            FadeFrameAnim = BuffFrame.transform.GetComponent<AnimParamsShow>();
        }
        if (BuffIcon != null)
        {
            FadeIconAnim = BuffIcon.transform.GetComponent<AnimParamsShow>();
        }
        if (BuffFloor != null)
        {
            FadeFloorAnim = BuffFloor.transform.GetComponent<AnimParamsShow>();
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
        if (BuffEntity != null && BuffEntity.BuffInfo != null)
        {
            transform.name = $"{BuffEntity.RuntimeID}_{BuffEntity.BuffID}";

            int state = BuffEntity.BuffInfo.GetBUFFUIShowState((int)SkillEditor.BUFFUIShowPosEnum.ThreeDHPUp);
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
            // 是否显示图标
            bool isShowIcon = CheckState(state, (int)SkillEditor.BuffUIShowDetailsEnum.ShowIcon);
            if (isShowIcon && BuffEntity.BuffInfo.Cfg != null)
            {
                // 背景
                string iconBgPath = BuffEntity.BuffInfo.Cfg.IsDebuff ? "Hud_Debuffbg_01" : "Hud_Buffbg_01";
                if (m_IconBgPath != iconBgPath)
                {
                    Action<Sprite> cb = (Sprite sprite) =>
                    {
                        if (sprite != null && BuffBg != null)
                        {
                            BuffBg.sprite = sprite;
                            m_IconBgPath = iconBgPath;
                        }
                    };
                    GetSprite(iconBgPath, cb);
                }
                // 边框
                string framePath = BuffEntity.BuffInfo.Cfg.IsDebuff ? "Hud_Debuff_Countdown" : "Hud_Buff_Countdown";
                if (m_FramePath != framePath)
                {
                    Action<Sprite> cb = (Sprite sprite) =>
                    {
                        if (sprite != null && BuffFrame != null)
                        {
                            BuffFrame.sprite = sprite;
                            m_FramePath = framePath;
                        }
                    };
                    GetSprite(framePath, cb);
                }
                // 图标
                string buffIconPath = BuffEntity.GetBuffIconPath();
                if (!string.IsNullOrEmpty(buffIconPath))
                {
                    if (m_IconPath != buffIconPath)
                    {
                        Action<Sprite> cb = (Sprite sprite) =>
                        {
                            if (sprite != null && BuffIcon != null)
                            {
                                BuffIcon.sprite = sprite;
                                m_IconPath = buffIconPath;
                            }
                        };
                        GetSprite(buffIconPath, cb);
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
        //SGF.Debuger.LogWarning($"动效 刷新 runtiemid={BuffEntity.RuntimeID},index={index}");
    }

    public void Release()
    {
        if (m_ReleaseCB != null)
        {
            m_ReleaseCB.Invoke(BuffEntity);
        }
        Reset();
        KillAllAnim();
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

    private void GetSprite(string SpriteName, Action<Sprite> callBack)
    {
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathBuff, SpriteName, callBack);
    }

    #region 动画

    public void PlayScaleAnim()
    {
        if (ScaleAnim != null)
        {
            ScaleAnim.PlayAnimByType(AnimType.Scale);
        }
    }

    private void PlayFadeAnim()
    {
        if (ScaleAnim != null)
        {
            ScaleAnim.PlayAnimByType(AnimType.Fade);
        }
        if (FadeFrameAnim != null)
        {
            FadeFrameAnim.PlayAnimByType(AnimType.Fade);
        }
        if (FadeIconAnim != null)
        {
            FadeIconAnim.PlayAnimByType(AnimType.Fade);
        }
        if (FadeFloorAnim != null)
        {
            FadeFloorAnim.PlayAnimByType(AnimType.Fade);
        }
    }

    private void KillAllAnim()
    {
        if (ScaleAnim != null)
        {
            ScaleAnim.DOKill();
        }
        if (FadeFrameAnim != null)
        {
            FadeFrameAnim.DOKill();
        }
        if (FadeIconAnim != null)
        {
            FadeIconAnim.DOKill();
        }
        if (FadeFloorAnim != null)
        {
            FadeFloorAnim.DOKill();
        }
    }
    #endregion
}
