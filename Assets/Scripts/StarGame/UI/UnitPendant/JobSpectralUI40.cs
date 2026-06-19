using SGF.Unity;
using SkillEditor;
using StarProject.Game.Player;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

public class JobSpectralUI40 : JobSpectralUI
{
    private const int CureMaxLength = 2;    // 治疗最大数 量谱一
    private const int AttackMaxLength = 3;  // 攻击最大数 量谱二

    private GameObject ItemRoot1;
    private GameObject ItemRoot2;

    private List<GameObject> Spectral1ItemList = new();
    private List<GameObject> Spectral1AddAnimList = new();
    private List<GameObject> Spectral1DelAnimList = new();
    private List<GameObject> Spectral1BuffAnimList = new();
    private List<bool> Spectral1AnimStateList = new();

    private List<GameObject> Spectral2ItemList = new();
    private List<GameObject> Spectral2AddAnimList = new();
    private List<GameObject> Spectral2DelAnimList = new();
    private List<GameObject> Spectral2BuffAnimList = new();
    private List<bool> Spectral2AnimStateList = new();

    private long m_TruthSpectral1 = 0;
    private long m_CurSpectral1 = 0;

    private long m_TruthSpectral2 = 0;
    private long m_CurSpectral2 = 0;

    private int m_Anim1State = 0;
    private int m_Anim2State = 0;

    private bool m_IsCountdownTreat = false;
    private float m_MaxTimeTreat = 0;
    private float m_CountdownTreat = 0;

    private bool m_IsCountdownAttack = false;
    private float m_MaxTimeAttack = 0;
    private float m_CountdownAttack = 0;

    public override void Init(EntityCtrlBase entityCtrlBase)
    {
        BindGob();
        base.Init(entityCtrlBase);
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }

        InitSpectral1State();
        InitSpectral2State();

        m_TruthSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral1);
        m_CurSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        SetUISpectral1(false);
        m_TruthSpectral2 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral2);
        m_CurSpectral2 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
        SetUISpectral2(false);
    }

    private void BindGob()
    {
        ItemRoot1 = transform.Find("ItemRoot1").gameObject;
        ItemRoot2 = transform.Find("ItemRoot2").gameObject;

        Spectral1ItemList.Clear();
        Spectral1AddAnimList.Clear();
        Spectral1DelAnimList.Clear();
        Spectral1AnimStateList.Clear();

        for (int i = 0; i < CureMaxLength; i++)
        {
            GameObject gob = transform.Find($"ItemRoot1/Item{i + 1}/Shell").gameObject;
            Spectral1ItemList.Add(gob);
            GameObject gobAddAnim = transform.Find($"ItemRoot1/Item{i + 1}/AddAnim").gameObject;
            Spectral1AddAnimList.Add(gobAddAnim);
            GameObject gobDelAnim = transform.Find($"ItemRoot1/Item{i + 1}/DelAnim").gameObject;
            Spectral1DelAnimList.Add(gobDelAnim);
            GameObject gobBuffAnim = transform.Find($"ItemRoot1/Item{i + 1}/BuffAnim").gameObject;
            Spectral1BuffAnimList.Add(gobBuffAnim);

            Spectral1AnimStateList.Add(true);
        }

        for (int i = 0; i < AttackMaxLength; i++)
        {
            GameObject gob = transform.Find($"ItemRoot2/Item{i + 1}/Shell").gameObject;
            Spectral2ItemList.Add(gob);
            GameObject gobAddAnim = transform.Find($"ItemRoot2/Item{i + 1}/AddAnim").gameObject;
            Spectral2AddAnimList.Add(gobAddAnim);
            GameObject gobDelAnim = transform.Find($"ItemRoot2/Item{i + 1}/DelAnim").gameObject;
            Spectral2DelAnimList.Add(gobDelAnim);
            GameObject gobBuffAnim = transform.Find($"ItemRoot2/Item{i + 1}/BuffAnim").gameObject;
            Spectral2BuffAnimList.Add(gobBuffAnim);

            Spectral2AnimStateList.Add(true);
        }
    }

    protected override void Reset()
    {
        m_TruthSpectral1 = 0;
        m_CurSpectral1 = 0;

        m_TruthSpectral2 = 0;
        m_CurSpectral2 = 0;

        m_IsCountdownTreat = false;
        m_MaxTimeTreat = 0;
        m_CountdownTreat = 0;

        m_IsCountdownAttack = false;
        m_MaxTimeAttack = 0;
        m_CountdownAttack = 0;

        base.Reset();
        ResetItem();
    }

    public override void Release()
    {
        Reset();
        base.Release();
    }

    private void Update()
    {
        // 治疗的BUFF动画
        if (m_IsCountdownTreat)
        {
            if (m_CountdownTreat > 0)
            {
                m_CountdownTreat -= Time.deltaTime;
                if (m_CountdownTreat <= 0)
                {
                    m_IsCountdownTreat = false;
                    // 关闭动画
                    SetTreatBuffAnim(false);
                }
            }
        }
        // 强攻的BUFF动画
        if (m_IsCountdownAttack)
        {
            if (m_CountdownAttack > 0)
            {
                m_CountdownAttack -= Time.deltaTime;
                if (m_CountdownAttack <= 0)
                {
                    m_IsCountdownAttack = false;
                    // 关闭动画
                    SetAttackBuffAnim(false);
                }
            }
        }
    }

    protected override void RegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral2, OnAOICurSpectral2Change);
    }

    protected override void UnRegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curSpectral2, OnAOICurSpectral2Change);
    }

    public override void SetShow(bool isShow)
    {
        base.SetShow(isShow);
        //if (ItemRoot1 != null)
        //{
        //    ItemRoot1.SetActive(isShow);
        //}
        if (ItemRoot2 != null)
        {
            ItemRoot2.SetActive(isShow);
        }
    }

    private void InitSpectral1State()
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            bool isShow = m_CurSpectral1 > i;
            Spectral1AnimStateList[i] = isShow;
        }
    }

    private void InitSpectral2State()
    {
        for (int i = 0; i < Spectral2ItemList.Count; i++)
        {
            bool isShow = m_CurSpectral2 > i;
            Spectral2AnimStateList[i] = isShow;
        }
    }

    #region 量谱一

    private void OnAOICurSpectral1Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_Anim1State = 0;
        long newValue = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        if (newValue > m_CurSpectral1)
        {
            m_Anim1State = 1;
        }
        else
        {
            m_Anim1State = -1;
        }

        m_CurSpectral1 = newValue;
        if (m_CurSpectral1 > m_TruthSpectral1)
        {
            m_TruthSpectral1 = m_CurSpectral1;
        }
        SetUISpectral1(true);
    }

    private void OnAOITruthSpectral1Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_TruthSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral1);
        if (m_CurSpectral1 > m_TruthSpectral1)
        {
            m_TruthSpectral1 = m_CurSpectral1;
        }
        SetUISpectral1(false);
    }

    private void SetUISpectral1(bool isPlayAnim)
    {
        int index = 0;
        int count = Spectral1ItemList.Count;
        int add = 1;
        //if (m_Anim1State == -1)
        //{
        //    index = Spectral1ItemList.Count;
        //    count = 1;
        //    add = -1;
        //}

        for (int i = index; i < count; i += add)
        {
            bool isShow = m_CurSpectral1 > i;
            if (isPlayAnim)
            {
                if (m_Anim1State == -1)
                {
                    SetUISpectral1ItemFun(i);
                }
                else
                {
                    object[] Os = new object[] { i };
                    DelayInvoker.DelayInvoke(0.1f * i,
                           //延迟处理--------------------
                           (object[] args) =>
                           {
                               SetUISpectral1ItemFun((int)args[0]);
                           }
                   , Os);
                }
                if (isShow)
                {
                    if (i + 1 == Spectral1ItemList.Count)
                    {
                        DelayInvoker.DelayInvoke((0.1f * i) + 0.5f, OnSetMaxAnim1Show);
                    }
                }
            }
        }
    }

    private void SetUISpectral1ItemFun(int idx)
    {
        bool isShow = m_CurSpectral1 > idx;
        bool lastState = Spectral1AnimStateList[idx];

        if (isShow == true && lastState == true)
        {

        }
        else
        {
            Spectral1AddAnimList[idx].SetActive(false);
            Spectral1DelAnimList[idx].SetActive(false);

            if (isShow == true && lastState == false)
            {
                Spectral1AddAnimList[idx].SetActive(true);
            }
            else if (isShow == false && lastState == true)
            {
                Spectral1DelAnimList[idx].SetActive(true);
            }
        }

        Spectral1ItemList[idx].SetActive(isShow);
        Spectral1AnimStateList[idx] = isShow;
    }

    private void OnSetMaxAnim1Show(object[] args)
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            Spectral1AddAnimList[i].SetActive(false);
            Spectral1AddAnimList[i].SetActive(true);
        }
    }

    #endregion

    #region 量谱二

    private void OnAOICurSpectral2Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_Anim2State = 0;
        long newValue = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
        if (newValue > m_CurSpectral1)
        {
            m_Anim2State = 1;
        }
        else
        {
            m_Anim2State = -1;
        }

        m_CurSpectral2 = newValue;
        if (m_CurSpectral2 > m_TruthSpectral2)
        {
            m_TruthSpectral2 = m_CurSpectral2;
        }
        SetUISpectral2(true);
    }

    private void OnAOITruthSpectral2Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_TruthSpectral2 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral2);
        if (m_CurSpectral2 > m_TruthSpectral2)
        {
            m_TruthSpectral2 = m_CurSpectral2;
        }
        SetUISpectral2(false);
    }

    private void SetUISpectral2(bool isPlayAnim)
    {
        int index = 0;
        int count = Spectral2ItemList.Count;
        int add = 1;
        //if (m_Anim2State == -1)
        //{
        //    index = Spectral2ItemList.Count;
        //    count = 1;
        //    add = -1;
        //}

        for (int i = index; i < count; i += add)
        {
            bool isShow = m_CurSpectral2 > i;
            if (isPlayAnim)
            {
                if (m_Anim2State == -1)
                {
                    SetUISpectral2ItemFun(i);
                }
                else
                {
                    object[] Os = new object[] { i };
                    DelayInvoker.DelayInvoke(0.1f * i,
                           //延迟处理--------------------
                           (object[] args) =>
                           {
                               SetUISpectral2ItemFun((int)args[0]);
                           }
                   , Os);
                }
                if (isShow)
                {
                    if (i + 1 == Spectral2ItemList.Count)
                    {
                        DelayInvoker.DelayInvoke((0.1f * i) + 0.5f, OnSetMaxAnim2Show);
                    }
                }
            }
        }
    }

    private void SetUISpectral2ItemFun(int idx)
    {
        bool isShow = m_CurSpectral2 > idx;
        bool lastState = Spectral2AnimStateList[idx];

        if (isShow == true && lastState == true)
        {

        }
        else
        {
            Spectral2AddAnimList[idx].SetActive(false);
            Spectral2DelAnimList[idx].SetActive(false);
            Spectral2BuffAnimList[idx].SetActive(false);

            if (isShow == true && lastState == false)
            {
                Spectral2AddAnimList[idx].SetActive(true);
            }
            else if (isShow == false && lastState == true)
            {
                Spectral2DelAnimList[idx].SetActive(true);
            }
        }

        Spectral2ItemList[idx].SetActive(isShow);
        Spectral2AnimStateList[idx] = isShow;
    }

    private void OnSetMaxAnim2Show(object[] args)
    {
        for (int i = 0; i < Spectral2ItemList.Count; i++)
        {
            Spectral2AddAnimList[i].SetActive(false);
            Spectral2AddAnimList[i].SetActive(true);
            Spectral2BuffAnimList[i].SetActive(false);
            Spectral2BuffAnimList[i].SetActive(true);
        }
    }


    #endregion

    private void ResetItem()
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            Spectral1ItemList[i].SetActive(false);
            Spectral1AddAnimList[i].SetActive(false);
            Spectral1DelAnimList[i].SetActive(false);
            Spectral1BuffAnimList[i].SetActive(false);
        }

        for (int i = 0; i < Spectral2ItemList.Count; i++)
        {
            Spectral2ItemList[i].SetActive(false);
            Spectral2AddAnimList[i].SetActive(false);
            Spectral2DelAnimList[i].SetActive(false);
            Spectral2BuffAnimList[i].SetActive(false);
        }

    }

    public override void SetCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, float countdown, float startCountdown)
    {
        if (spTimeShowTypeEnum == SpTimeShowTypeEnum.MuYTreat)
        {
            SetTreatCountdown(countdown, startCountdown);
        }
        else if (spTimeShowTypeEnum == SpTimeShowTypeEnum.MuYAttack)
        {
            SetAttackCountdown(countdown, startCountdown);
        }
    }

    public override void SetOpenCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, bool isOpen)
    {
        if (spTimeShowTypeEnum == SpTimeShowTypeEnum.MuYTreat)
        {
            SetTreatOpenCountdown(isOpen);
        }
        else if (spTimeShowTypeEnum == SpTimeShowTypeEnum.MuYAttack)
        {
            SetAttackOpenCountdown(isOpen);
        }
    }

    #region 治疗

    private void SetTreatBuffAnim(bool isShow)
    {
        for (int i = 0; i < Spectral1BuffAnimList.Count; i++)
        {
            Spectral1BuffAnimList[i].SetActive(isShow);
        }
    }

    private void SetTreatCountdown(float countdown, float startCountdown)
    {
        m_MaxTimeTreat = countdown;
        m_CountdownTreat = startCountdown;
    }

    private void SetTreatOpenCountdown(bool isOpen)
    {
        m_IsCountdownTreat = isOpen;
        SetTreatBuffAnim(isOpen);
    }

    #endregion

    #region 攻击
    private void SetAttackBuffAnim(bool isShow)
    {
        for (int i = 0; i < Spectral2BuffAnimList.Count; i++)
        {
            Spectral2BuffAnimList[i].SetActive(isShow);
        }
    }

    private void SetAttackCountdown(float countdown, float startCountdown)
    {
        m_MaxTimeAttack = countdown;
        m_CountdownAttack = startCountdown;
    }

    // -- 2024/6/3 磊子说，牧业的量谱满了后不播放这个buff特效了，
    private void SetAttackOpenCountdown(bool isOpen)
    {
        //m_IsCountdownAttack = isOpen;
        //SetAttackBuffAnim(isOpen);
    }
    #endregion


}
