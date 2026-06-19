using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Player;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主角面板信息
/// TODO：挂件小曲，动态加载出来
/// </summary>
public class StarPlayerInfoPanel : MonoBehaviour
{
    public Image M_HPBar;
    public Text M_HPText;
    public Image M_MPBar;
    public Text M_MPText;
    public Image M_EXPBar;
    public Text M_EXPText;
    public Text M_Level;
    public Text M_Name;
    // 量槽
    public Image M_Spectral1Bar;
    public Text M_Spectral1Text;
    public Image M_Spectral2Bar;
    public Text M_Spectral2Text;
    public Image M_Spectral3Bar;
    public Text M_Spectral3Text;
    // 主角Buff容器
    public GameObject M_BuffContainer;
    public Image M_HeadPortrait;

    private VitalSignData m_vitalSignData;
    // 血条相关 HealthPoint  MagicPoint
    private long m_HPSum = 100;
    private long m_HPCur = 100;
    private long m_MPSum = 100;
    private long m_MPCur = 100;
    private float m_EXPSum = 100f;
    private float m_EXPCur = 50f;
    // 量槽
    private long m_Spectral1Sum = 100;
    private long m_Spectral1Cur = 100;
    private long m_Spectral2Sum = 100;
    private long m_Spectral2Cur = 100;
    private long m_Spectral3Sum = 100;
    private long m_Spectral3Cur = 100;

    private void Start()
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase == null)
        {
            return;
        }

        m_vitalSignData = GameManager.Instance.M_MainPlayerCtrlBase.Data;

        if (m_vitalSignData == null)
        {
            return;
        }

        // 血量
        m_HPSum = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);
        m_HPCur = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
        SetHP(m_HPCur);
        // 蓝量
        m_MPSum = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthMP);
        m_MPCur = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curMp);
        SetMP(m_MPCur);
        // 经验条
        m_EXPSum = m_vitalSignData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerExp);
        SetEXP(m_EXPSum);
        // 角色名
        string name = m_vitalSignData.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Name);
        M_Name.text = name;
        // 等级
        int level = m_vitalSignData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);
        SetLevel(level);
        // 量槽1
        m_Spectral1Sum = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral1);
        m_Spectral1Cur = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        SetSpectral1(m_Spectral1Cur);
        // 量槽2
        m_Spectral2Sum = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral2);
        m_Spectral2Cur = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
        SetSpectral2(m_Spectral2Cur);
        // 量槽3
        m_Spectral3Sum = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral3);
        m_Spectral3Cur = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral3);
        SetSpectral3(m_Spectral3Cur);

        RegisterAttribute();
    }

    private void OnDisable()
    {
        Release();
    }

    private void Reset()
    {
        UnRegisterAttribute();
        m_vitalSignData = null;
    }

    private void Release()
    {
        Reset();
    }


    #region 属性变更监听

    private void RegisterAttribute()
    {
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.TruthHp, OnAOITruthHpChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curMp, OnAOICurMpChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.TruthMP, OnAOITruthMPChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.PlayerLevel, OnAOIPlayerLevelChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.PlayerExp, OnAOIPlayerExpChange);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curSpectral1, OnAOIcurSpectral1Change);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curSpectral2, OnAOIcurSpectral2Change);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.curSpectral3, OnAOIcurSpectral3Change);
        m_vitalSignData.RegisterAttribute(AOIAttrDefine.TruthSpectral3, OnAOITruthSpectral3Change);
    }

    private void UnRegisterAttribute()
    {
        if (m_vitalSignData == null)
        {
            return;
        }
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.TruthHp, OnAOITruthHpChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curMp, OnAOICurMpChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.TruthMP, OnAOITruthMPChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.PlayerLevel, OnAOIPlayerLevelChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.PlayerExp, OnAOIPlayerExpChange);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curSpectral1, OnAOIcurSpectral1Change);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curSpectral2, OnAOIcurSpectral2Change);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.curSpectral3, OnAOIcurSpectral3Change);
        m_vitalSignData.UnRegisterAttribute(AOIAttrDefine.TruthSpectral3, OnAOITruthSpectral3Change);
    }

    #region 监听属性改变的回调
    /// <summary> AOI [当前血量] 变化 </summary>
    private void OnAOICurHpChange(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        SetHP(value);
    }
    /// <summary> AOI [实际血量] 变化 </summary>
    private void OnAOITruthHpChange(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        UpdateTruthHp(value);
    }
    /// <summary> AOI [当前蓝量] 变化 </summary>
    private void OnAOICurMpChange(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        SetMP(value);
    }
    /// <summary> AOI [实际蓝量] 变化 </summary>
    private void OnAOITruthMPChange(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        UpdateTruthMp(value);
    }
    /// <summary> AOI [等级] 变化 </summary>
    private void OnAOIPlayerLevelChange(string key, object val)
    {
        int value = m_vitalSignData.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
        SetLevel(value);
    }
    /// <summary> AOI [经验] 变化 </summary>
    private void OnAOIPlayerExpChange(string key, object val)
    {
        int value = m_vitalSignData.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
        SetEXP(value);
    }
    /// <summary> AOI [当前量槽 1] 变化 </summary>
    private void OnAOIcurSpectral1Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        SetSpectral1(value);
    }
    /// <summary> AOI [实际量槽 1] 变化 </summary>
    private void OnAOITruthSpectral1Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        UpdateTruthSpectral1(value);
    }
    /// <summary> AOI [当前量槽 2] 变化 </summary>
    private void OnAOIcurSpectral2Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        SetSpectral2(value);
    }
    /// <summary> AOI [实际量槽 2] 变化 </summary>
    private void OnAOITruthSpectral2Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        UpdateTruthSpectral2(value);
    }
    /// <summary> AOI [当前量槽 3] 变化 </summary>
    private void OnAOIcurSpectral3Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        SetSpectral3(value);
    }
    /// <summary> AOI [实际量槽 3] 变化 </summary>
    private void OnAOITruthSpectral3Change(string key, object val)
    {
        long value = m_vitalSignData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
        UpdateTruthSpectral3(value);
    }
    #endregion

    #endregion

    #region 更新属性信息
    // 设置血量
    public void SetHP(long curHp)
    {
        m_HPCur = curHp;
        if (m_HPCur > m_HPSum)
        {
            m_HPSum = m_HPCur;
        }
        float ratio = m_HPSum == 0 ? 0 : (float)m_HPCur / m_HPSum;
        M_HPBar.fillAmount = ratio;
        M_HPText.text = m_HPCur.ToString();
    }
    public void UpdateTruthHp(long hp)
    {
        m_HPSum = hp;
        if (m_HPCur > m_HPSum)
        {
            m_HPCur = m_HPSum;
        }
        float ratio = m_HPSum == 0 ? 0 : (float)m_HPCur / m_HPSum;
        M_HPBar.fillAmount = ratio;
        M_HPText.text = m_HPCur.ToString();
    }
    // 设置蓝量
    public void SetMP(long curMp)
    {
        m_MPCur = curMp;
        if (m_MPCur > m_MPSum)
        {
            m_MPSum = m_MPCur;
        }
        float ratio = m_MPSum == 0 ? 0 : (float)m_MPCur / m_MPSum;
        M_MPBar.fillAmount = ratio;
        M_MPText.text = m_MPCur.ToString();
    }
    public void UpdateTruthMp(long mp)
    {
        m_MPSum = mp;
        if (m_MPCur > m_MPSum)
        {
            m_MPCur = m_MPSum;
        }
        float ratio = m_MPSum == 0 ? 0 : (float)m_MPCur / m_MPSum;
        M_MPBar.fillAmount = ratio;
        M_MPText.text = m_MPCur.ToString();
    }
    // 设置量槽1
    public void SetSpectral1(long curSpectral)
    {
        m_Spectral1Cur = curSpectral;
        if (m_Spectral1Cur > m_Spectral1Sum)
        {
            m_Spectral1Sum = m_Spectral1Cur;
        }
        SetSpectral1Text();
    }
    public void UpdateTruthSpectral1(long spectral)
    {
        m_Spectral1Sum = spectral;
        if (m_Spectral1Cur > m_Spectral1Sum)
        {
            m_Spectral1Cur = m_Spectral1Sum;
        }
        SetSpectral1Text();
    }
    private void SetSpectral1Text()
    {
        float ratio = m_Spectral1Sum == 0 ? 0 : (float)m_Spectral1Cur / m_Spectral1Sum;
        M_Spectral1Bar.fillAmount = ratio;
        M_Spectral1Text.text = m_Spectral1Cur.ToString();
    }
    // 设置量槽2
    public void SetSpectral2(long curSpectral)
    {
        m_Spectral2Cur = curSpectral;
        if (m_Spectral2Cur > m_Spectral2Sum)
        {
            m_Spectral2Sum = m_Spectral2Cur;
        }
        SetSpectral2Text();
    }
    public void UpdateTruthSpectral2(long spectral)
    {
        m_Spectral2Sum = spectral;
        if (m_Spectral2Cur > m_Spectral2Sum)
        {
            m_Spectral2Cur = m_Spectral2Sum;
        }
        SetSpectral2Text();
    }
    private void SetSpectral2Text()
    {
        float ratio = m_Spectral2Sum == 0 ? 0 : (float)m_Spectral2Cur / m_Spectral2Sum;
        M_Spectral2Bar.fillAmount = ratio;
        M_Spectral2Text.text = m_Spectral2Cur.ToString();
    }
    // 设置量槽3
    public void SetSpectral3(long curSpectral)
    {
        m_Spectral3Cur = curSpectral;
        if (m_Spectral3Cur > m_Spectral3Sum)
        {
            m_Spectral3Sum = m_Spectral3Cur;
        }
        SetSpectral3Text();
    }
    public void UpdateTruthSpectral3(long spectral)
    {
        m_Spectral3Sum = spectral;
        if (m_Spectral3Cur > m_Spectral3Sum)
        {
            m_Spectral3Cur = m_Spectral3Sum;
        }
        SetSpectral3Text();
    }
    private void SetSpectral3Text()
    {
        float ratio = m_Spectral3Sum == 0 ? 0 : (float)m_Spectral3Cur / m_Spectral3Sum;
        M_Spectral3Bar.fillAmount = ratio;
        M_Spectral3Text.text = m_Spectral3Cur.ToString();
    }
    // 设置经验
    public void SetEXP(float curExp)
    {
        m_EXPCur = curExp;
        float ratio = m_EXPSum == 0 ? 0 : (float)m_EXPCur / m_EXPSum;
        M_EXPBar.fillAmount = ratio;
        M_EXPText.text = $"{m_EXPCur}/{m_EXPSum}";
    }
    // 设置等级
    private void SetLevel(int level)
    {
        //M_Level.text = level + "级";
        //M_Level.text = string.Format(GameConfig.LocalStr["LvStr"], level);
        M_Level.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), level);
    }
    #endregion

}
