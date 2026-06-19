using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Fire;
using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject;
using StarProject.Game;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
public class PlayVideoWidgetArgs
{
    public int PvCfgID = -1;
    public int EffectID = -1;
    public string Path;
    public string SoundEventName;
    public System.Action CallBack;
    public bool IsShowSkipBtn;
    public bool IsInBalack;
    public bool IsOutBlack;
}

public class PlayVideoWidget : UIWidget
{
    [SerializeField]
    private CanvasGroup Black;

    [SerializeField]
    private VideoPlayer VideoPlayer;

    [SerializeField]
    private JButton JBtnSkipPV;

    [SerializeField]
    private RawImage Rawimg;

    private PlayVideoWidgetArgs m_PlayVideoWidgetArgs;
    private TweenerCore<float, float, FloatOptions> blackFade = null;
    private bool blackAnimFinish = false;
    private bool BlackAnimFinish
    {
        get
        {
            return blackAnimFinish;
        }
        set
        {
            blackAnimFinish = value;
            if (blackAnimFinish)
            {
                PlayVideo();
            }
        }
    }
    private bool videoLoadFinish = false;

    private bool VideoLoadFinish
    {
        get
        {
            return videoLoadFinish;
        }
        set
        {
            videoLoadFinish = value;
            if (videoLoadFinish)
            {
                PlayVideo();
            }
        }
    }

    protected override void Awake()
    {
        VideoPlayer.loopPointReached += UnityVideoPlayOver;
        JBtnSkipPV.gameObject.SetActive(false);
        JBtnSkipPV.OnClick += OnClickJBtnSkipPV;
        VideoPlayer.prepareCompleted += (p) =>
        {
            SGF.Debuger.Log($"视频 准备好了，开始播放");
            VideoLoadFinish = true;
        };

        var renderTex = Resources.Load<RenderTexture>("Render/Yoka");
        //清理rt 残留，别问为啥，，
        RenderTexture rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = renderTex;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;

        VideoPlayer.targetTexture = renderTex;
        Rawimg.texture = renderTex;
    }

    protected override void OnDestroy()
    {
        VideoPlayer.loopPointReached -= UnityVideoPlayOver;
    }

    protected override void OnOpen(object arg = null)
    {
        GameManager.Instance.RegisterMainPlayerClientBattleStates();

        m_PlayVideoWidgetArgs = arg as PlayVideoWidgetArgs;

        if (m_PlayVideoWidgetArgs.IsInBalack)
        {
            Debug.Log("进入黑脸");
            OnPlayBlackDOFade(true, 0.1f, SetBlackAnimFinish);
        }
        else
        {
            Black.alpha = 1.0f;
            SetBlackAnimFinish();
        }

        GameManager.Instance.EventPreNewPlayerEvent($"2_{m_PlayVideoWidgetArgs.PvCfgID}_1");

        string path = Utils.GetVideoPath(m_PlayVideoWidgetArgs.Path);
        SGF.Debuger.LogWarning("视频 VideoPath=" + path);
        VideoPlayer.source = VideoSource.Url;
        VideoPlayer.url = path;
        VideoPlayer.Prepare();
        //VideoPlayer.gameObject.SetActive(true);

        /*StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<VideoClip>(m_PlayVideoWidgetArgs.Path,
        (VideoClip vc) =>
        {
            if (vc != null)
            {
                JBtnSkipPV.gameObject.SetActive(m_PlayVideoWidgetArgs.IsShowSkipBtn);
                VideoPlayer.clip = vc;
                VideoPlayer.Play();
            }
            else
            {
                if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.EffectID != -1)
                {
                    GlobalEvent.OnEffectEnd?.Invoke(m_PlayVideoWidgetArgs.EffectID);
                }
                UIManager.Instance.CloseWidget(UIDef.PlayVideoWidget, null, true);
            }
        });*/
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);

        GameManager.Instance.UnRegisterMainPlayerClientBattleStates();

        JBtnSkipPV.gameObject.SetActive(false);
        //VideoPlayer.gameObject.SetActive(false);
        VideoPlayer.Stop();
        //RemoveTargetframe();

        blackAnimFinish = false;
        videoLoadFinish = false;

        if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.CallBack != null)
        {
            m_PlayVideoWidgetArgs.CallBack.Invoke();
        }
        if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.EffectID != -1)
        {
            GlobalEvent.OnEffectEnd?.Invoke(m_PlayVideoWidgetArgs.EffectID);
        }
        m_PlayVideoWidgetArgs = null;
    }

    public void OnPlayBlackDOFade(bool isToBlack, float time, Action cb = null)
    {
        SetBlackFadeKill();
        Black.alpha = isToBlack ? 0f : 1.0f;
        float toAlpha = isToBlack ? 1f : 0f;
        blackFade = Black.DOFade(toAlpha, time);
        blackFade.onComplete = () =>
        {
            SGF.Debuger.Log($"视频 黑幕 播放结束 isToBlack={isToBlack},time={time}");
            cb?.Invoke();
        };
    }

    private void SetBlackFadeKill()
    {
        if (blackFade != null && blackFade.IsActive() && blackFade.IsPlaying())
        {
            blackFade.onComplete = null;
            blackFade.Kill(true);
        }
    }

    private void SetBlackAnimFinish()
    {
        BlackAnimFinish = true;
        JBtnSkipPV.gameObject.SetActive(m_PlayVideoWidgetArgs.IsShowSkipBtn);
    }

    private void UnityVideoPlayOver(VideoPlayer source)
    {
        SGF.Debuger.Log($"视频 播放结束 ");
        JBtnSkipPV.gameObject.SetActive(false);
        //VideoPlayer.gameObject.SetActive(false);
        if (source != null)
        {
            GameManager.Instance.EventPreNewPlayerEvent($"2_{m_PlayVideoWidgetArgs.PvCfgID}_3");
        }
        StarProject.Service.Sound.SoundManager.Instance.PlayEventName("PV_XinSG_Stop", gameObject, null);
        RemoveTargetframe();

        ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnTransition",
new object[] { 2, 0 });

        //if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.EffectID == 1000704)
        //{
        //    ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnOpenAvgTalkBoxForTimeLine", new object[] { 990015 });
        //}

        if (m_PlayVideoWidgetArgs.IsOutBlack)
        {
            OnPlayBlackDOFade(false, 1.0f, CloseSelf);
        }
        else
        {
            CloseSelf();
        }
    }

    /// <summary>
    ///texture 清除上一帧渲染
    /// </summary>
    private void RemoveTargetframe()
    {
        VideoPlayer.targetTexture.Release();
        VideoPlayer.targetTexture.MarkRestoreExpected();
        Rawimg.gameObject.SetActive(false);
    }

    private void PlayVideo()
    {
        if (!BlackAnimFinish)
        {
            return;
        }
        if (!VideoLoadFinish)
        {
            return;
        }
        Rawimg.gameObject.SetActive(true);
        VideoPlayer.Play();
        if (!string.IsNullOrEmpty(m_PlayVideoWidgetArgs.SoundEventName))
        {
            StarProject.Service.Sound.SoundManager.Instance.PlayEventName(m_PlayVideoWidgetArgs.SoundEventName, gameObject, null);
        }
    }

    private void OnClickJBtnSkipPV(GameObject arg0)
    {
        VideoPlayer.Stop();
        UnityVideoPlayOver(null);
        GameManager.Instance.EventPreNewPlayerEvent($"2_{m_PlayVideoWidgetArgs.PvCfgID}_2");

    }

    private void CloseSelf()
    {

        //SGF.Debuger.Log($"视频 黑幕 播放结束 关闭界面");
        //if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.EffectID == 1000704)
        //{
        //    ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnOpenAvgTalkBoxForTimeLine", new object[] { 990015 });
        //}
        UIManager.Instance.CloseWidget(UIDef.PlayVideoWidget, null, true);
        //LuaModuleDispatcher: SendMessage(ModuleDef.AvgLuaModule, "OnOpenAvgTalkBox", { 990015})

        //播放一个转场
        //if (m_PlayVideoWidgetArgs != null && m_PlayVideoWidgetArgs.EffectID == 1000704)
        //{

       // }

    }
}

