using SGF.Unity;
using StarProject.Game.Player;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

public class JobSpectralUI20 : JobSpectralUI
{
    private const int MaxLength = 6;

    private GameObject ItemRoot;

    private List<GameObject> Spectral1ItemList = new();
    private List<GameObject> Spectral1AddAnimList = new();
    private List<GameObject> Spectral1DelAnimList = new();
    private List<bool> Spectral1AnimStateList = new();

    private long m_TruthSpectral1 = 0;
    private long m_CurSpectral1 = 0;

    private int m_AnimState = 0;

    public override void Init(EntityCtrlBase entityCtrlBase)
    {
        BindGob();
        base.Init(entityCtrlBase);
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }

        InitSpectral1State();

        m_TruthSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral1);
        m_CurSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        SetUISpectral1(false);
    }

    private void BindGob()
    {
        ItemRoot = transform.Find("ItemRoot").gameObject;
        Spectral1ItemList.Clear();
        Spectral1AddAnimList.Clear();
        Spectral1DelAnimList.Clear();
        Spectral1AnimStateList.Clear();

        for (int i = 0; i < MaxLength; i++)
        {
            GameObject gob = transform.Find($"ItemRoot/Item{i + 1}/Shell").gameObject;
            Spectral1ItemList.Add(gob);
            GameObject gobAddAnim = transform.Find($"ItemRoot/Item{i + 1}/AddAnim").gameObject;
            Spectral1AddAnimList.Add(gobAddAnim);
            GameObject gobDelAnim = transform.Find($"ItemRoot/Item{i + 1}/DelAnim").gameObject;
            Spectral1DelAnimList.Add(gobDelAnim);

            Spectral1AnimStateList.Add(true);
        }
    }

    protected override void Reset()
    {
        m_TruthSpectral1 = 0;
        m_CurSpectral1 = 0;

        base.Reset();
        ResetItem();
    }

    public override void Release()
    {
        Reset();
        base.Release();
    }

    protected override void RegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
    }

    protected override void UnRegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
    }

    public override void SetShow(bool isShow)
    {
        base.SetShow(isShow);
        if (ItemRoot == null)
        {
            return;
        }
        ItemRoot.SetActive(isShow);
    }

    private void InitSpectral1State()
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            bool isShow = m_CurSpectral1 > i;
            Spectral1AnimStateList[i] = isShow;
        }
    }

    private void OnAOICurSpectral1Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_AnimState = 0;
        long newValue = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        if (newValue > m_CurSpectral1)
        {
            m_AnimState = 1;
        }
        else
        {
            m_AnimState = -1;
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
        //int add = 1;
        //if (m_AnimState == -1)
        //{
        //    index = Spectral1ItemList.Count;
        //    count = 0;
        //    add = -1;
        //}

        for (int i = index; i < count; i++)
        {
            bool isShow = m_CurSpectral1 > i;
            if (isPlayAnim)
            {
                if (m_AnimState == -1)
                {
                    SetUISpectral1ItemFun(i);
                }
                else
                {
                    object[] Os = new object[] { i };
                    DelayInvoker.DelayInvoke(0.1f * i,
                           //ÑÓ³Ù´¦Àí--------------------
                           (object[] args) =>
                           {
                               SetUISpectral1ItemFun((int)args[0]);
                           }
                   , Os);
                }
                if (isShow)
                {
                    if (i + 1 == MaxLength)
                    {
                        DelayInvoker.DelayInvoke((0.1f * i) + 0.5f, OnSetMaxAnimShow);
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

    private void OnSetMaxAnimShow(object[] args)
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            Spectral1DelAnimList[i].SetActive(false);
            Spectral1DelAnimList[i].SetActive(true);
        }
    }

    private void ResetItem()
    {
        for (int i = 0; i < Spectral1ItemList.Count; i++)
        {
            Spectral1ItemList[i].SetActive(false);
            Spectral1AddAnimList[i].SetActive(false);
            Spectral1DelAnimList[i].SetActive(false);
        }
    }
}
