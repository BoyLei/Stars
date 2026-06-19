using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class JobSpectralUI50 : JobSpectralUI
{
    private GameObject ItemRoot;
    private Image Progress1;
    private GameObject MaxAnim1;
    private GameObject AddAnim1;
    private GameObject DelAnim1;

    private Image Progress2;
    private GameObject MaxAnim2;
    private GameObject AddAnim2;
    private GameObject DelAnim2;

    private long m_TruthSpectral1 = 0;
    private long m_CurSpectral1 = 0;

    private long m_TruthSpectral2 = 0;
    private long m_CurSpectral2 = 0;

    private TweenerCore<float, float, FloatOptions> dotween1;
    private Tween tween1;
    private float m_lastRatio1 = -1;

    private TweenerCore<float, float, FloatOptions> dotween2;
    private Tween tween2;
    private float m_lastRatio2 = -1;

    public override void Init(EntityCtrlBase entityCtrlBase)
    {
        BindGob();
        base.Init(entityCtrlBase);
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }

        //m_TruthSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral1);
        m_TruthSpectral1 = 100;
        m_CurSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        SetUISpectral1();
        //m_TruthSpectral2 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthSpectral2);
        m_TruthSpectral2 = 100;
        m_CurSpectral2 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
        SetUISpectral2();
    }

    private void BindGob()
    {
        ItemRoot = transform.Find("ItemRoot").gameObject;
        Progress1 = transform.Find("ItemRoot/ProgressBg1/Progress1").GetComponent<Image>();
        MaxAnim1 = transform.Find("ItemRoot/ProgressBg1/Max").gameObject;
        AddAnim1 = transform.Find("ItemRoot/ProgressBg1/Add").gameObject;
        DelAnim1 = transform.Find("ItemRoot/ProgressBg1/Del").gameObject;

        Progress2 = transform.Find("ItemRoot/ProgressBg2/Progress2").GetComponent<Image>();
        MaxAnim2 = transform.Find("ItemRoot/ProgressBg2/Max").gameObject;
        AddAnim2 = transform.Find("ItemRoot/ProgressBg2/Add").gameObject;
        DelAnim2 = transform.Find("ItemRoot/ProgressBg2/Del").gameObject;
    }

    protected override void Reset()
    {
        m_TruthSpectral1 = 0;
        m_CurSpectral1 = 0;
        m_lastRatio1 = -1;

        m_TruthSpectral2 = 0;
        m_CurSpectral2 = 0;
        m_lastRatio2 = -1;

        SetLerpDotween1Kill();
        SetLerptween1Kill();
        SetLerpDotween2Kill();
        SetLerptween2Kill();

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
        //m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
        //m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral2, OnAOICurSpectral2Change);
    }

    protected override void UnRegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        //m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
        //m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral2, OnAOITruthSpectral2Change);
        m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curSpectral2, OnAOICurSpectral2Change);
    }

    public override void SetShow(bool isShow)
    {
        base.SetShow(isShow);

        if (ItemRoot != null)
        {
            ItemRoot.SetActive(isShow);
        }
    }

    #region 量谱一

    private void OnAOICurSpectral1Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        long newValue = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        if (newValue > m_CurSpectral1 && newValue < m_TruthSpectral1)
        {
            SetAddAnim1Show(false);
            SetLerptween1Kill();
            float ratio = 0;
            if (m_TruthSpectral1 > 0)
            {
                ratio = (float)newValue / (float)m_TruthSpectral1;
                if (ratio > 1)
                {
                    ratio = 1;
                }
            }

            SetAddAnim1Show(true, ratio);
            tween1 = DOVirtual.DelayedCall(2f, () =>
            {
                SetAddAnim1Show(false);
            });
        }
        m_CurSpectral1 = newValue;
        //if (m_CurSpectral1 > m_TruthSpectral1)
        //{
        //    m_TruthSpectral1 = m_CurSpectral1;
        //}
        SetUISpectral1();
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
        SetUISpectral1();
    }

    private void SetUISpectral1()
    {
        float ratio = 0;
        if (m_TruthSpectral1 > 0)
        {
            ratio = (float)m_CurSpectral1 / (float)m_TruthSpectral1;
            if (ratio > 1)
            {
                ratio = 1;
            }
        }
        SetLerpDotween1Kill();
        dotween1 = DG.Tweening.DOTweenModuleUI.DOFillAmount(Progress1, ratio, 0.2f);

        if (ratio == 1)
        {
            SetDelAnim1Show(false);

            SetMaxAnim1Show(false);
            SetMaxAnim1Show(true);
        }
        else if (m_lastRatio1 == 1 && ratio < 1)
        {
            SetMaxAnim1Show(false);
            SetAddAnim1Show(false);

            SetDelAnim1Show(false);
            SetDelAnim1Show(true);
        }
        m_lastRatio1 = ratio;
    }

    private void SetLerpDotween1Kill()
    {
        if (dotween1 != null && dotween1.IsActive() && dotween1.IsPlaying())
        {
            dotween1.onComplete = null;
            dotween1.Kill(false);
        }
    }

    private void SetLerptween1Kill()
    {
        if (tween1 != null && tween1.IsActive() && tween1.IsPlaying())
        {
            tween1.onComplete = null;
            tween1.Kill(false);
        }
    }

    private void SetMaxAnim1Show(bool isShow)
    {
        if (MaxAnim1 == null)
        {
            return;
        }
        MaxAnim1.SetActive(isShow);
    }

    private void SetDelAnim1Show(bool isShow)
    {
        if (DelAnim1 == null)
        {
            return;
        }
        DelAnim1.SetActive(isShow);
    }

    private void SetAddAnim1Show(bool isShow, float ratio = 1)
    {
        if (AddAnim1 == null)
        {
            return;
        }
        if (isShow)
        {
            float y = 100 * ratio - 50;
            AddAnim1.transform.SetLocalPositionY(y);
        }
        AddAnim1.SetActive(isShow);
    }

    #endregion

    #region 量谱二

    private void OnAOICurSpectral2Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        long newValue = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
        if (newValue > m_CurSpectral2 && newValue < m_TruthSpectral2)
        {
            SetAddAnim2Show(false);
            SetLerptween2Kill();
            float ratio = 0;
            if (m_TruthSpectral2 > 0)
            {
                ratio = (float)newValue / (float)m_TruthSpectral2;
                if (ratio > 1)
                {
                    ratio = 1;
                }
            }

            SetAddAnim2Show(true, ratio);
            tween2 = DOVirtual.DelayedCall(2f, () =>
            {
                SetAddAnim2Show(false);
            });
        }
        m_CurSpectral2 = newValue;
        //if (m_CurSpectral2 > m_TruthSpectral2)
        //{
        //    m_TruthSpectral2 = m_CurSpectral2;
        //}
        SetUISpectral2();
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
        SetUISpectral2();
    }

    private void SetUISpectral2()
    {
        float ratio = 0;
        if (m_TruthSpectral2 > 0)
        {
            ratio = (float)m_CurSpectral2 / (float)m_TruthSpectral2;
            if (ratio > 1)
            {
                ratio = 1;
            }
        }
        SetLerpDotween2Kill();
        dotween2 = DG.Tweening.DOTweenModuleUI.DOFillAmount(Progress2, ratio, 0.2f);

        if (ratio == 1)
        {
            SetDelAnim2Show(false);

            SetMaxAnim2Show(false);
            SetMaxAnim2Show(true);
        }
        else if (m_lastRatio2 == 1 && ratio < 1)
        {
            SetMaxAnim2Show(false);
            SetAddAnim2Show(false);

            SetDelAnim2Show(false);
            SetDelAnim2Show(true);
        }
        m_lastRatio2 = ratio;
    }

    private void SetLerpDotween2Kill()
    {
        if (dotween2 != null && dotween2.IsActive() && dotween2.IsPlaying())
        {
            dotween2.onComplete = null;
            dotween2.Kill(false);
        }
    }

    private void SetLerptween2Kill()
    {
        if (tween2 != null && tween2.IsActive() && tween2.IsPlaying())
        {
            tween2.onComplete = null;
            tween2.Kill(false);
        }
    }

    private void SetMaxAnim2Show(bool isShow)
    {
        if (MaxAnim2 == null)
        {
            return;
        }
        MaxAnim2.SetActive(isShow);
    }

    private void SetDelAnim2Show(bool isShow)
    {
        if (DelAnim2 == null)
        {
            return;
        }
        DelAnim2.SetActive(isShow);
    }

    private void SetAddAnim2Show(bool isShow, float ratio = 1)
    {
        if (AddAnim2 == null)
        {
            return;
        }
        if (isShow)
        {
            float y = 100 * ratio - 50;
            AddAnim2.transform.SetLocalPositionY(y);
        }
        AddAnim2.SetActive(isShow);
    }


    #endregion



    private void ResetItem()
    {

    }
}
