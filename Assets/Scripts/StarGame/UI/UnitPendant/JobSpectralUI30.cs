using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SGF.Unity;
using SkillEditor;
using StarProject.Game.Player;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobSpectralUI30 : JobSpectralUI
{
    private const int MaxLength = 6;
    private GameObject ItemRoot;
    private GameObject MaxAnim;
    private RectTransform AddAnim;
    private List<GameObject> StateSpectral2List = new();
    private List<GameObject> StateSpectral2AnimList = new();

    private Image Progress;

    private long m_TruthSpectral1 = 0;
    private long m_CurSpectral1 = 0;

    private float m_lastRatio = -1;

    private TweenerCore<float, float, FloatOptions> dotween1;
    private TweenerCore<Vector2, Vector2, VectorOptions> dotween2;

    private Vector2 AddAnimInitPos = Vector2.zero;
    private bool m_IsCountdown = false;
    private float m_MaxTime = 5;
    private float m_Countdown = 0;
    private float m_EventItemPer = 0;   // 最大倒计时/最大个数
    private int m_LastIndex = 0;

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
        //SetUISpectral1(false);

        AddAnimInitPos.y = 8.4f;

        SetUISpectral11();
    }

    private void BindGob()
    {
        ItemRoot = transform.Find("ItemRoot").gameObject;
        MaxAnim = transform.Find("ItemRoot/MaxAnim").gameObject;
        AddAnim = transform.Find("ItemRoot/AddAnim").GetComponent<RectTransform>();

        //StateSpectral2List.Clear();
        //for (int i = 0; i < MaxLength; i++)
        //{
        //    GameObject gob = transform.Find($"ItemRoot/Item{i + 1}/Shell").gameObject;
        //    StateSpectral2List.Add(gob);
        //    GameObject gobAnim = transform.Find($"ItemRoot/Item{i + 1}/Anim").gameObject;
        //    StateSpectral2AnimList.Add(gobAnim);
        //}

        Progress = transform.Find("ItemRoot/ProgressBg/Progress").GetComponent<Image>();
    }

    protected override void Reset()
    {
        m_TruthSpectral1 = 0;
        m_CurSpectral1 = 0;

        m_IsCountdown = false;
        m_MaxTime = 0;
        m_Countdown = 0;
        m_EventItemPer = 0;   // 最大倒计时/最大个数
        m_LastIndex = 0;

        m_lastRatio = -1;
        SetLerpDotween1Kill();

        base.Reset();
        //ResetItem();
    }

    public override void Release()
    {
        Reset();
        base.Release();
    }

    //private void Update()
    //{
    //    if (m_IsCountdown)
    //    {
    //        if (m_Countdown > 0)
    //        {
    //            m_Countdown -= Time.deltaTime;
    //            float dif = m_MaxTime - m_Countdown;
    //            int num = Mathf.FloorToInt(dif / m_EventItemPer);
    //            if (m_LastIndex != num)
    //            {
    //                SetUISpectral1ItemFun(num - 1, true);
    //                m_LastIndex = num;
    //            }
    //            if (m_Countdown <= 0)
    //            {
    //                m_IsCountdown = false;
    //            }
    //        }
    //    }
    //}

    protected override void RegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        //m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
        m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curSpectral1, OnAOICurSpectral1Change);
    }

    protected override void UnRegisterAttribute()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        //m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthSpectral1, OnAOITruthSpectral1Change);
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

    private void OnAOICurSpectral1Change(string key, object value)
    {
        if (m_entityCtrl == null && m_entityCtrl.Data == null)
        {
            return;
        }
        m_CurSpectral1 = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
        //if (m_CurSpectral1 > m_TruthSpectral1)
        //{
        //    m_TruthSpectral1 = m_CurSpectral1;
        //}
        //if (m_CurSpectral1 <= 0)
        //{
        //    SetMaxAnimShow(false);
        //}
        //SetUISpectral1();

        SetUISpectral11();
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
        //SetUISpectral1();
        SetUISpectral11();
    }

    private void SetUISpectral1(bool isPlayAnim = true)
    {
        for (int i = 0; i < StateSpectral2List.Count; i++)
        {
            bool isShow = m_CurSpectral1 > i;
            StateSpectral2List[i].SetActive(isShow);
            if (isShow)
            {
                if (isPlayAnim)
                {
                    object[] Os = new object[] { i };
                    DelayInvoker.DelayInvoke(0.1f * i,
                           //延迟处理--------------------
                           (object[] args) =>
                           {
                               SetUISpectral1ItemFun((int)args[0]);
                           }
                   , Os);
                    if (i + 1 == MaxLength)
                    {
                        DelayInvoker.DelayInvoke(0.1f * (i + 2), OnSetMaxAnimShow);
                    }
                }
            }
            else
            {
                StateSpectral2AnimList[i].SetActive(isShow);
            }
        }
    }

    private void SetUISpectral11()
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
        dotween1 = DG.Tweening.DOTweenModuleUI.DOFillAmount(Progress, ratio, 0.2f);

        if (m_lastRatio != -1 && ratio != 1 && AddAnim != null)
        {
            SetLerpDotween2Kill();
            SetAddAnimShow(false);
            SetAddAnimShow(true);
            dotween2 = DG.Tweening.DOTweenModuleUI.DOAnchorPosX(AddAnim, 72 * ratio, 0.4f);
            dotween2.onComplete = () =>
            {
                SetAddAnimShow(false);
                dotween2.onComplete = null;
            };
        }

        SetMaxAnimShow(false);
        if (ratio == 1)
        {
            SetMaxAnimShow(true);
            m_lastRatio = 0;
            AddAnim.anchoredPosition = AddAnimInitPos;
        }
        else
        {
            m_lastRatio = ratio;
        }
    }

    private void SetLerpDotween1Kill()
    {
        if (dotween1 != null && dotween1.IsActive() && dotween1.IsPlaying())
        {
            dotween1.onComplete = null;
            dotween1.Kill(false);
        }
    }

    private void SetLerpDotween2Kill()
    {
        if (dotween2 != null && dotween2.IsActive() && dotween2.IsPlaying())
        {
            dotween2.onComplete = null;
            dotween2.Kill(false);
        }
    }

    private void OnSetMaxAnimShow(object[] args)
    {
        SetMaxAnimShow(true);
    }

    private void SetUISpectral1ItemFun(int idx)
    {
        bool isShow = m_CurSpectral1 > idx;
        StateSpectral2AnimList[idx].SetActive(isShow);
    }

    private void SetMaxAnimShow(bool isShow)
    {
        if (MaxAnim == null)
        {
            return;
        }
        MaxAnim.SetActive(isShow);
    }

    private void SetAddAnimShow(bool isShow)
    {
        if (AddAnim == null)
        {
            return;
        }
        AddAnim.gameObject.SetActive(isShow);
    }

    private void SetUISpectral1ItemFun(int idx, bool isShow)
    {
        if (idx < 0)
        {
            return;
        }
        if (idx > StateSpectral2List.Count)
        {
            return;
        }
        StateSpectral2List[idx].SetActive(isShow);
        //CommonStateSpectral2AnimList[idx].SetActive(isShow);
    }

    private void ResetItem()
    {
        for (int i = 0; i < StateSpectral2List.Count; i++)
        {
            StateSpectral2List[i].SetActive(false);
            StateSpectral2AnimList[i].SetActive(false);
        }

        SetMaxAnimShow(false);
    }

    public override void SetCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, float countdown, float startCountdown)
    {
        //m_MaxTime = countdown;
        //m_Countdown = startCountdown;
        //m_EventItemPer = (float)m_MaxTime / (float)MaxLength;
    }

    public override void SetOpenCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, bool isOpen)
    {
        //m_IsCountdown = isOpen;
        //m_LastIndex = 0;
        //if (!m_IsCountdown)
        //{
        //    SetUISpectral1(false);
        //}
    }


}
