using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SGF.UI.Framework;
using StarProject.Game;
using StarProjectDef;
using System;
using System.Collections;
using UnityEngine;

public class PlayTimelineBlackWidgetArgs
{
    //public TimelineConfigDataCell TimelineConfigData = null;

    // 0不播放 1淡入 2淡出
    public int FadeType;
    //时间
    public float FadeTime;
    //是否需要跳过
    public bool IsNeedSkip;
    //回调事件
    public System.Action CallBack;
    //回调事件
    public System.Action SkipCallBack;
}

public class PlayTimelineBlackWidget : UIWidget
{
    [SerializeField]
    private CanvasGroup Black;

    [SerializeField]
    private JButton JBtnSkipPV;

    [SerializeField]
    private GameObject Cover;

    private TweenerCore<float, float, FloatOptions> blackFade = null;

    private Action CloseCallBack = null;
    private Action SkipCallBack = null;


    private bool IsForbidMove = false;
    private PlayTimelineBlackWidgetArgs m_PlayTimelineBlackWidgetArgs = null;
    private Coroutine m_iEnumerator;

    protected override void Awake()
    {
        JBtnSkipPV.gameObject.SetActive(false);
        JBtnSkipPV.OnClick += OnClickJBtnSkipPV;
    }

    protected override void OnDestroy()
    {
    }

    protected override void OnOpen(object arg = null)
    {
        m_PlayTimelineBlackWidgetArgs = arg as PlayTimelineBlackWidgetArgs;
        //SGF.Debuger.LogError($"打开了 timeline 黑幕 effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");

        if (!IsForbidMove)
        {
            GameManager.Instance.RegisterMainPlayerClientBattleStates();
        }
        IsForbidMove = true;

        JBtnSkipPV.gameObject.SetActive(false);

        CloseCallBack = m_PlayTimelineBlackWidgetArgs.CallBack;
        SkipCallBack = m_PlayTimelineBlackWidgetArgs.SkipCallBack;

        //SetBlackFadeKill(false);
        //StopIEnumerator();

        int fadeType = m_PlayTimelineBlackWidgetArgs.FadeType;
        if (fadeType == 0)
        {
            bool isShow = m_PlayTimelineBlackWidgetArgs != null ? m_PlayTimelineBlackWidgetArgs.IsNeedSkip : false;
            JBtnSkipPV.gameObject.SetActive(isShow);
            OnPlayComplete(false);
        }
        else
        {
            bool fade = fadeType == 1;
            float fadeTime = m_PlayTimelineBlackWidgetArgs.FadeTime;
            OnPlayBlackDOFade(fade, fadeTime, () =>
            {
                // 这里进的太快了
                OnPlayComplete(false);
                bool isShow = m_PlayTimelineBlackWidgetArgs != null ? m_PlayTimelineBlackWidgetArgs.IsNeedSkip : false;
                JBtnSkipPV.gameObject.SetActive(isShow);
            });
        }

        /*
        Action cb = () =>
        {
            //SGF.Debuger.Log($"timeline 黑幕 播放 结束 effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");
            CloseCallBack?.Invoke();
            bool isShow = m_PlayTimelineBlackWidgetArgs != null && m_PlayTimelineBlackWidgetArgs.TimelineConfigData != null ? m_PlayTimelineBlackWidgetArgs.TimelineConfigData.IsSkip : true;
            JBtnSkipPV.gameObject.SetActive(isShow);
        };
        if (m_PlayTimelineBlackWidgetArgs.TimelineConfigData != null && m_PlayTimelineBlackWidgetArgs.TimelineConfigData.IsInBalack)
        {
            OnPlayBlackDOFade(true, 1.5f, cb);
        }
        else
        {
            Black.alpha = 0f;
            cb?.Invoke();
        }*/
    }

    private void OnPlayComplete(bool isSkip = false)
    {
        SetBlackFadeKill(false);
        StopIEnumerator();

        Cover.SetActive(false);
        if (isSkip)
        {
            //UIManager.Instance.CloseWidget(UIDef.PlayTimelineBlackWidget, null, true);
            SkipCallBack?.Invoke();
            UIManager.Instance.CloseWidget(UIDef.PlayTimelineBlackWidget, null, true);
        }
        else
        {
            CloseCallBack?.Invoke();
        }
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
        //SGF.Debuger.LogError($"timeline 黑幕 关闭 effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");

        SetBlackFadeKill(false);
        StopIEnumerator();
        if (IsForbidMove)
        {
            GameManager.Instance.UnRegisterMainPlayerClientBattleStates();
        }
        IsForbidMove = false;

        //CloseCallBack = null;
        //SkipCallBack = null;
        //m_PlayTimelineBlackWidgetArgs = null;
        //JBtnSkipPV.gameObject.SetActive(false);
        CloseCallBack = null;
        SkipCallBack = null;
        m_PlayTimelineBlackWidgetArgs = null;
        JBtnSkipPV.gameObject.SetActive(false);
    }

    public void OnPlayBlackDOFade(bool isToBlack, float time, Action cb = null)
    {
        SetBlackFadeKill(false);
        Black.alpha = isToBlack ? 0f : 1.0f;
        Cover.SetActive(true);
        float toAlpha = isToBlack ? 1f : 0f;
        //SGF.Debuger.Log($"timeline 黑幕 播放 开始 isToBlack={isToBlack},time={time},effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");

        blackFade = Black.DOFade(toAlpha, time);
        Black.blocksRaycasts = true;

        //为什么加 0.1f 和 dotween DoFade 冲突了，会拉回拉扯 
        /*DelayInvoker.DelayInvoke(time+0.1f, (a) =>
        {
            Black.alpha = 0f;
            SGF.Debuger.Log($"timeline 黑幕 播放结束 isToBlack={isToBlack},time={time}");
            cb?.Invoke();
        },null);*/
        StopIEnumerator();
        m_iEnumerator = StartCoroutine(DelayedAction(time, isToBlack, () =>
        {
            Black.blocksRaycasts = false;
            // 这里是你原来在匿名函数内部的操作
            cb?.Invoke(); // 如果cb是外部的回调函数，也可以在这里调用
        }));
    }

    private IEnumerator DelayedAction(float delay, bool isToBlack, System.Action callback)
    {
        yield return new WaitForSeconds(delay);

        // 执行你的操作
        Black.alpha = 0f;
        //SGF.Debuger.Log($"timeline 黑幕 播放结束 isToBlack={isToBlack},time={delay},effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");

        // 调用回调函数
        callback?.Invoke();
    }

    private void StopIEnumerator()
    {
        if (m_iEnumerator != null)
        {
            StopCoroutine(m_iEnumerator);
        }
        m_iEnumerator = null;
    }

    private void SetBlackFadeKill(bool complete)
    {
        if (blackFade != null && blackFade.IsActive() && blackFade.IsPlaying())
        {
            blackFade.Kill(complete);
            blackFade.onComplete = null;
        }
        blackFade = null;
    }

    private void OnClickJBtnSkipPV(GameObject arg0)
    {
        //SGF.Debuger.Log($"点击跳过了 timeline 黑幕 effectid={m_PlayTimelineBlackWidgetArgs.TimelineConfigData.GetID()}");
        JBtnSkipPV.gameObject.SetActive(false);
        OnPlayComplete(true);
    }

}

