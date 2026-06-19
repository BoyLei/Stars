using StarProject.Game.Skill;
using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingSkillItem : MonoBehaviour
{
    public Image SkillIcon;
    public Image SkillBgIcon;
    public Text SkillDes; // 技能的描述（长按、位移）
    public Text SkillName;
    public Toggle ToggleSelf;

    private SkillPosSetDataCell SkillPosSetDataCell;    // 技能按钮位默认信息配置
    private SkillInfo FirstSkillInfo;
    private int m_posID = -1;
    private Action<bool , int> ClickCB = null;

    private void Awake()
    {
        ToggleSelf.onValueChanged.AddListener((bool value) => OnValueChange());
    }

    public void Reset()
    {
        ClickCB = null;
        SkillPosSetDataCell = null;
        m_posID = -1;
        SkillDes.text = "";
        SkillDes.transform.parent.gameObject.SetActive(false);
    }

    public void SetSettingSkillItem(SkillInfo skillInfo, int posID, SkillPosSetDataCell _skillPosSetDataCell, bool isOn ,Action<bool, int> cb)
    {
        Reset();
        ClickCB = cb;

        FirstSkillInfo = skillInfo;
        if (FirstSkillInfo == null)
        {
            return;
        }

        SkillPosSetDataCell = _skillPosSetDataCell;
        if (SkillPosSetDataCell == null)
        {
            return;
        }
        m_posID = posID;
        ToggleSelf.SetIsOnWithoutNotify(isOn);

        SetSkillIcon(FirstSkillInfo.GetSkillIconPath());
        SetBgIcon();
        SkillName.text = FirstSkillInfo.GetSkillName();
        SkillDes.text = LanguageManager.Instance.GetLanguageByKey("UltimateSkill");
        SkillDes.transform.parent.gameObject.SetActive(SkillPosSetDataCell.GetSkillType() == 4);

        gameObject.SetActive(true);
        transform.SetSiblingIndex(7 - SkillPosSetDataCell.GetID());
    }

    private void SetSkillIcon(string path)
    {
        Action<Sprite> action = (sp) =>
        {
            if (SkillIcon != null && sp != null)
            {
                SkillIcon.sprite = sp;
                SkillIcon.gameObject.SetActive(true);
                SkillIcon.SetNativeSize();
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill, path, action);
    }

    private void SetBgIcon()
    {
        string path = SkillPosSetDataCell.SkillBg;
        Action<Sprite> action = (sp) =>
        {
            if (SkillBgIcon != null && sp != null)
            {
                SkillBgIcon.sprite = sp;
                SkillBgIcon.SetNativeSize();
            }
        };
        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathSkill, path, action);
    }

    private void OnValueChange()
    {
        if (FirstSkillInfo == null)
        {
            return;
        }
        if (m_posID == -1)
        {
            return;
        }
        ClickCB?.Invoke(ToggleSelf.isOn, m_posID);
    }
}
