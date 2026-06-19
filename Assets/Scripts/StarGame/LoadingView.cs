///--------------------------------------------------------------------
/// 文件名   :   LoadingView.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/21 16:23:18
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using ProtoMsg;
using SGF.UI.Framework;
using StarProject;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class LoadingView : UIWidget
{
    public bool IsPrepare = false;
    private CanvasGroup m_Block;
    private CanvasGroup m_WhiteBlock;
    private GameObject m_Root;
    private Transform m_LoadingLiteRoot;
    private RawImage m_RawImage;
    private UIBGAdaptive m_RawImageBGAdaptive;

    private Slider m_Slider;
    private Text m_Text;

    private string m_content = string.Empty;

    private bool Init = false;
    private float CurrentProcess;
    private bool IsToBlack = false;
    private bool IsToWhite = false;

    private TweenerCore<float, float, FloatOptions> blackFade = null;
    private Tweener proTweener = null;


    Vector2 upPivot = new(0.5f, /*0.75f*/0.5f);
    Vector2 downPivot = new(0.5f, /*0.25f*/0.5f);
    private TweenerCore<Vector2, Vector2, VectorOptions> changeLoadingPic1;
    private TweenerCore<Color, Color, ColorOptions> changeLoadingPic2;
    private float useTime;
    private float startTime;
    private const float FADE_TIME = 0.5f;
    private const float SHOW_TIME = 0.5f;
    private float LoadingStartShowTime;//分针动画表现逻辑，表现大于实际逻辑

    private bool isFirst = true;
    private GameObject dynamicMainTown;


    protected override void Awake()
    {
        if (!Init)
        {
            m_Block = transform.Find("Block").GetComponent<CanvasGroup>();
            m_WhiteBlock = transform.Find("WhiteBlock").GetComponent<CanvasGroup>();
            m_Root = transform.Find("Root").gameObject;
            m_LoadingLiteRoot = transform.Find("LoadingLite");
            m_RawImage = m_Root.transform.Find("RawImage").GetComponent<RawImage>();
            m_RawImageBGAdaptive = m_Root.transform.Find("RawImage").GetComponent<UIBGAdaptive>();
            m_Slider = m_Root.transform.Find("Process/Slider").GetComponent<Slider>();
            m_Text = m_Root.transform.Find("Process/Text").GetComponent<Text>();
            DontDestroyOnLoad(this.gameObject);
            Init = true;

            if (!IsPrepare)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(UIDef.FX_UI_UILoginPage,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }
                    dynamicMainTown = GameObject.Instantiate<GameObject>(go, m_RawImage.transform);
                    if (dynamicMainTown != null)
                    {
                        //dynamicMainTown.transform.SetParent(m_RawImage.transform);
                        //gob.transform.SetLocalScale(Vector3.one);
                        dynamicMainTown.transform.localPosition = Vector3.zero;
                        dynamicMainTown.transform.localRotation = Quaternion.identity;

                        UIFXBGAdaptive uIFXBGAdaptive = dynamicMainTown.GetComponent<UIFXBGAdaptive>();
                        if (uIFXBGAdaptive != null)
                        {
                            uIFXBGAdaptive.Init();
                        }
                    }
                });
            }
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        if (FirstInitTrueLoadingView)
        {
            FirstInitTrueLoadingView = false;
        }
        else
        {
            ResetProcess();
        }
    }

    protected override void OnEnable()
    {
        GlobalEvent.OnLoadingViewEvent?.Invoke(true);
    }

    protected override void OnDisable()
    {
        GlobalEvent.OnLoadingViewEvent?.Invoke(false);
    }

    private void ResetProcess()
    {
        if (m_LoadingLiteRoot)
        {
            m_LoadingLiteRoot.gameObject.SetActive(false);
        }

        m_Slider.value = 0;
        m_Text.text = string.Empty;
        m_content = string.Empty;
        m_Block.alpha = 1f;
        m_WhiteBlock.alpha = 0f;
        m_Root.SetActive(true);
        LoadingStartShowTime = Time.time;

    }

    public void OnProcessStart()
    {
        if (!Init)
        {
            Awake();
        }
        if (gameObject == null)
        {
            return;
        }

        ResetProcess();
        SGF.Debuger.LogWarning("首场景 进度开始-------------------");

        proTweener = DoTweenFactory.DOValue(m_Slider, 0.99f, 5).SetEase(Ease.OutQuad);
        //proTweener.onUpdate = () =>
        //{
        //    SGF.Debuger.LogError($"首场景 切图 进度 时间={SGF.Time.TimeUtils.ServerNow} ,curPro={m_Slider.value}");
        //};
        transform.SetAsLastSibling();
        gameObject.SetActive(true);//TODO uiManager

        UIManager.Instance.SetShowLoadingViewToHideHud(true);
        GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);
    }

    private bool FirstInitTrueLoadingView = true;
    private const string preExcel = "ExcelBytes/PreCSText";
    private PreCSTextData preCSTextData = null;
    private string GetTipsTextByKey(string key)
    {
        PreCSTextDataCell cfg;
        if (preCSTextData != null && preCSTextData.StaticPreCSTextDatas.TryGetValue(key, out cfg))
        {
            switch (LanguageManager.Instance.CurLanguageType)
            {
                case LanguageType.None:
                case LanguageType.Chinese:
                    return cfg.Cn;
                    break;
                case LanguageType.English:
                    return cfg.En;
                    break;
                default:
                    return cfg.Cn;
                    break;
            }
        }

        return string.Empty;
    }
    private void LoadPreCSTextDataCfg()
    {
        SGF.Debuger.Log("首场景 111111111  加载首个配置");
        try
        {
            UnityEngine.TextAsset file = (UnityEngine.TextAsset)UnityEngine.Resources.Load(preExcel);
            preCSTextData = MessagePack.MessagePackSerializer.Deserialize<PreCSTextData>(file.bytes);
            StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(preExcel);
        }
        catch (System.Exception e)
        {
            SGF.Debuger.LogError($"[GameUpdate] LoadPreCSTextDataCfg MessagePack Deserialize 发生异常: {e.Message}");
        }
    }
    /// <summary>
    /// Prepare -> Init -> MainTown(id==0)
    /// 真loading走这里，假的走Gob的Awake
    /// </summary>
    /// <param name="spaceType"></param>
    /// <param name="mapID"></param>
    public void OnProcessStart(SpaceType spaceType, int mapID)
    {

        if (!Init)
        {
            Awake();
        }
        if (gameObject == null)
        {
            return;
        }

        if (FirstInitTrueLoadingView)
        {
            LoadPreCSTextDataCfg();
            OnShowTextSpec(GetTipsTextByKey("HotDownload8"));
            m_Slider.value = 0.99f;
        }
        else
        {
            var loadingFuncDataCell = LocalDataManager.Instance.GetLoadingFuncDataCell(spaceType, mapID);
            if (loadingFuncDataCell == null)
            {
                GameManager.Instance.Loading.OnShowSpecialLoading();
            }
            else
            {
                ResetProcess();
                // 当前 pivot

                LoadPicBefore();
                {
                    List<LoadingGalleryDataCell> iconList = LocalDataManager.Instance.GetLoadingGalleryDataCellList(loadingFuncDataCell.GetGalleryID());
                    if (iconList.Count > 0)
                    {
                        int random = 0;
                        if (iconList.Count > 1)
                        {
                            random = UnityEngine.Random.Range(0, iconList.Count);
                        }
                        string path = $"{iconList[random].LoadingPic}";
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextureAsync(path, LoadLoadingTextureFinish);
                    }
                }
                {
                    List<LoadingTipsDataCell> tipsList = LocalDataManager.Instance.GetLoadingTipsDataCellList(loadingFuncDataCell.GetTipsID());
                    if (tipsList.Count > 0)
                    {
                        int random = 0;
                        if (tipsList.Count > 1)
                        {
                            random = UnityEngine.Random.Range(0, tipsList.Count);
                        }
                        string tips = tipsList[random].Desc;
                        m_Text.text = tips;
                    }
                }

                proTweener = DoTweenFactory.DOValue(m_Slider, 0.99f, 5).SetEase(Ease.Flash);
            }
        }

        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        UIManager.Instance.SetShowLoadingViewToHideHud(true);
        GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);

        AppMain.Instance.LoadingQualitySetting();
    }

    /// <summary>
    /// 最少0.5秒变黑色保证效果，然后替换图片变正常色
    /// </summary>
    private void LoadPicBefore()
    {
        if (isFirst)//首次loading不要动画
        {
            m_RawImage.rectTransform.pivot = Vector2.one * 0.5f;
            startTime = Time.time;
            useTime = Time.time - startTime;
        }
        else
        {
            changeLoadingPic1 = DOTween.To(
               () => m_RawImage.rectTransform.pivot,  // 获取当前值的方法
               value => { m_RawImage.rectTransform.pivot = value; useTime = Time.time - startTime; },  // 设置新值的方法
               downPivot,  // 目标值
               FADE_TIME  // 动画持续时间
           )
           .SetEase(Ease.OutQuad) // 设置 pivot 动画的缓动方式
           .OnStart(() =>
           {
               startTime = Time.time;
               m_RawImage.rectTransform.pivot = upPivot;
           }).SetUpdate(UpdateType.Fixed);

            m_RawImage.color = Color.black;
            /*changeLoadingPic2 = m_RawImage.DOColor(Color.black, FADE_TIME) 
        .SetEase(Ease.OutQuad); */
            //策划说两个图信息太大
        }
        if (dynamicMainTown != null)
        {
            dynamicMainTown.SetActive(/*isFirst*/false);
        }
    }

    private void LoadLoadingTextureFinish(Texture2D texture2D)
    {
        if (gameObject == null)
        {
            return;
        }
        if (isFirst)
        {
            useTime = 0;
            if (m_RawImage != null)
            {
                m_RawImage.texture = texture2D;
                m_RawImage.rectTransform.pivot = Vector2.one * 0.5f;
                m_RawImage.color = Color.white;
            }

            if (m_RawImageBGAdaptive != null)
            {
                m_RawImageBGAdaptive.ForceExcute();
            }
        }
        else
        {
            if (useTime != 0 && useTime + 0.1f < FADE_TIME)
            {
                StartCoroutine(DelayedExecute(FADE_TIME - useTime, texture2D)); // 延迟 1 秒，传递 texture2D
            }
            else
            {
                if (m_RawImage != null)
                {
                    m_RawImage.texture = texture2D; m_RawImage.rectTransform.pivot = Vector2.one * 0.5f;
                    m_RawImage.color = Color.white;
                }

                if (m_RawImageBGAdaptive != null)
                {
                    m_RawImageBGAdaptive.ForceExcute();
                }
                useTime = 0;
            }
        }
        isFirst = false;
    }

    private IEnumerator DelayedExecute(float delay, Texture2D texture2D)
    {
        yield return new WaitForSeconds(delay);

        if (m_RawImage != null)
        {
            m_RawImage.texture = texture2D;
        }

        if (m_RawImageBGAdaptive != null)
        {
            m_RawImageBGAdaptive.ForceExcute();
        }

        changeLoadingPic1 = DOTween.To(
                    () => m_RawImage.rectTransform.pivot,  // 获取当前值的方法
                    value => { m_RawImage.rectTransform.pivot = value; useTime = Time.time - startTime; },  // 设置新值的方法
                    upPivot,  // 目标值
                    SHOW_TIME  // 动画持续时间
                )
                .SetEase(Ease.OutQuad) // 设置 pivot 动画的缓动方式
                .OnStart(() =>
                {
                    startTime = Time.time;
                    m_RawImage.rectTransform.pivot = upPivot;
                }).SetUpdate(UpdateType.Fixed).
                OnComplete(() =>
                {
                    useTime = 0;
                });



        changeLoadingPic2 = m_RawImage.DOColor(Color.white, 1f)
            .SetEase(Ease.OutQuad);
    }

    public void OnShowText(string content)
    {
        m_content = content;
        if (m_Text != null)
        {
            int pro = Math.Min(99, (int)(m_Slider.value * 100));
            m_Text.text = $"{content} ({pro})%";
        }
    }
    public void OnShowTextSpec(string content)
    {
        if (m_Text != null)
        {
            m_Text.text = ".." + content + "..(100)%";
        }
    }

    public void OnProcess(float process, bool isLerp = false, LoadingState loadingState = LoadingState.Common)
    {
        SetSimulationProcess(process, loadingState);
    }

    /// <summary>
    /// 大于1破顺的等待逻辑
    /// </summary>
    /// <param name="targetProcess"></param>
    /// <param name="loadingState"></param>
    private void SetSimulationProcess(float targetProcess, LoadingState loadingState)
    {
        if (targetProcess >= 1)
        {
            SetProTweenKill();
            m_Slider.value = 1;
            return;
        }
        float curPro = m_Slider.value;
        //最大0.99
        targetProcess = Math.Min(0.99f, targetProcess);
        if (targetProcess > curPro)
        {
            SetProTweenKill();
            float time = 10f;

            proTweener = DoTweenFactory.DOValue(m_Slider, targetProcess, time).SetEase(Ease.Flash);
        }
    }

    public void OnPlayBlackDOFade(bool isToBlack, float time, Action<bool> cb = null)
    {
        if (!Init)
        {
            Awake();
        }
        if (gameObject == null)
        {
            return;
        }
        if (IsToBlack == isToBlack)
        {
            return;
        }
        IsToBlack = isToBlack;
        ResetProcess();
        m_Root.SetActive(false);

        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        UIManager.Instance.SetShowLoadingViewToHideHud(true);
        //GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);
        if (isToBlack)
        {
            GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);
        }
        SetBlackFadeKill();
        m_WhiteBlock.alpha = 0f;
        m_Block.alpha = isToBlack ? 0f : 1.0f;
        float toAlpha = isToBlack ? 1f : 0f;
        //time = isToBlack ? toBlackTime : toWhiteTime;
        if (time <= 0)
        {
            m_Block.alpha = toAlpha;
            cb?.Invoke(false);
            return;
        }
        blackFade = m_Block.DOFade(toAlpha, time);
        blackFade.onComplete = () =>
        {
            SGF.Debuger.Log($"黑幕 播放结束 isToBlack={isToBlack},time={time}");
            cb?.Invoke(false);
        };
    }
    public void OnShowSpecialLoading()
    {
        if (!Init)
        {
            Awake();
        }
        if (gameObject == null || m_LoadingLiteRoot == null)
        {
            return;
        }
        m_Root.SetActive(false);
        gameObject.SetActive(true);
        m_LoadingLiteRoot.gameObject.SetActive(true);
    }
    public void OnPlayWhiteDOFade(bool isToWhite, float time, Action<bool> cb = null)
    {
        if (!Init)
        {
            Awake();
        }
        if (gameObject == null)
        {
            return;
        }
        if (IsToWhite == isToWhite)
        {
            return;
        }
        IsToWhite = isToWhite;
        ResetProcess();
        m_Root.SetActive(false);

        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        UIManager.Instance.SetShowLoadingViewToHideHud(true);
        //GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);
        if (isToWhite)
        {
            GlobalEvent.IsBeginLoadingOrEnd.Invoke(true);
        }

        m_Block.alpha = 0f;
        m_WhiteBlock.alpha = isToWhite ? 0f : 1.0f;
        float toAlpha = isToWhite ? 1f : 0f;
        // time = isToWhite ? toBlackTime : toWhiteTime;
        if (time <= 0)
        {
            m_WhiteBlock.alpha = toAlpha;
            return;
        }
        SetBlackFadeKill();
        blackFade = m_WhiteBlock.DOFade(toAlpha, time);
        blackFade.onComplete = () =>
        {
            SGF.Debuger.Log($"闪白 播放结束 isToWhite={isToWhite},time={time}");
            cb?.Invoke(false);
        };
    }

    public float GetProcess()
    {
        if (m_Slider != null)
        {
            return m_Slider.value;

        }
        return 0;
    }

    public void OnProcessBlackEnd()
    {
        if (gameObject == null)
        {
            return;
        }

        OnPlayBlackDOFade(false, 1f, OnProcessEnd);
    }

    public void OnProcessWhiteEnd()
    {
        if (gameObject == null)
        {
            return;
        }

        OnPlayWhiteDOFade(false, 1f, OnProcessEnd);
    }

    public void OnProcessEnd(bool isLerp = false)
    {
        if (gameObject == null)
        {
            return;
        }
        SetProTweenKill();
        if (isLerp)
        {
            proTweener = DoTweenFactory.DOValue(m_Slider, 1, 0.1f).SetEase(Ease.OutQuad);
            proTweener.onComplete = () =>
            {
                ProcessEnd();
            };
        }
        else
        {
            ProcessEnd();
        }
        AppMain.Instance.RefQualitySetting();
    }

    private void ProcessEnd()
    {
        if (Time.time - LoadingStartShowTime > (FADE_TIME + SHOW_TIME))
        {
            StartCoroutine(DelayedExecuteClose(0.1f)); // 延迟 1 秒，传递 texture2D
        }
        else
        {
            StartCoroutine(DelayedExecuteClose(FADE_TIME + SHOW_TIME - (Time.time - LoadingStartShowTime) + 0.1f)); // 延迟 1 秒，传递 texture2D
        }
    }

    private IEnumerator DelayedExecuteClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_Slider.value = 1;
        GlobalEvent.IsBeginLoadingOrEnd.Invoke(false);
        gameObject.SetActive(false);
        SetBlackFadeKill();
        UIManager.Instance.SetShowLoadingViewToHideHud(false);
    }

    protected void SetBlackFadeKill()
    {
        if (blackFade != null && blackFade.IsActive() && blackFade.IsPlaying())
        {
            blackFade.onComplete = null;
            blackFade.Kill(true);
        }
        blackFade = null;
    }

    private void SetProTweenKill()
    {
        if (proTweener != null && proTweener.IsActive() && proTweener.IsPlaying())
        {
            proTweener.onComplete = null;
            proTweener.onUpdate = null;
            proTweener.Kill(false);
        }
        proTweener = null;
    }

    protected override void OnDestroy()
    {
        ResetProcess();
        isFirst = true;
    }

}
