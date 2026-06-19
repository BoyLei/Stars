///--------------------------------------------------------------------
/// 文件名   :   GamePrepare.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/12 11:14:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using DG.Tweening;
using Fire;
using SGF;
using SGF.UI.Framework;
using StarProject;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Lua;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Yoka.UnityString.Core;
/// <summary>
/// GamePerpare 播放视频准备信息
/// GameUpdate 开始更新游戏
/// InIt GameAPP启动
/// </summary>
public class GamePrepare : MonoBehaviour
{
    #region AVPRO
    /*[SerializeField]
        //Logo用
    private MediaPlayer LogoMediaPlayer;*/

    /*    [SerializeField]
        //热更用
        private MediaPlayer HotMediaPlayer;*/

    /*    [SerializeField]
     *  //显示用
        private DisplayUGUI DisplayUGUI;*/
    #endregion
    #region Unity默认的
    [SerializeField]
    //Logo用
    private VideoPlayer LogoMediaPlayer;

    /*    [SerializeField]
    //Unity默认带的
    private RectTransform DisplayUGUIUnity;*/
    #endregion

    [SerializeField]
    private LoadingView GameProcess;

    [SerializeField]
    private GameUpdate GameUpdate;

    [SerializeField]
    private RawImage YkLogo;    // 默认的公司Logo
    [SerializeField]
    private CanvasGroup AdviceTips; // 健康游戏16字公告

    int scWidth = Screen.width;
    int scHeight = Screen.height;

    int designWidth = 1280; //这个是设计分辨率
    int designHeight = 720;

    private int _graphQualityLevel = 1;// 0H   1M    2L

    private MachineQualityLevel machineQualityLevel = MachineQualityLevel.TopLevel;


    public bool FirstRunnig = true;

    void Awake()
    {
        //av
        /* LogoMediaPlayer.Events.AddListener(OnMediaPlayerEventHandler);*/
        if (AppConfig.IsGM())
        {
            LogCollect.Init();
        }

        InitLog();
        SaveManager.Instance.Init("Stars/");
        LocalDataManager.Instance.Init();
        StarProject.Service.Language.LanguageManager.Instance.Init();
        //unity
        //LogoMediaPlayer.started += OnStart;
        LogoMediaPlayer.loopPointReached += UnityVideoPlayOver;//视频播放结束后异步回调
        var renderTex = Resources.Load<RenderTexture>("Render/Yoka");
        //清理rt 残留，别问为啥，，
        RenderTexture rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = renderTex;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;

        LogoMediaPlayer.targetTexture = renderTex;
        YkLogo.texture = renderTex;

        LogoMediaPlayer.prepareCompleted += (p) =>
        {
            SGF.Debuger.Log($"视频 准备好了，开始播放");
            LogoMediaPlayer.Play();
        };

        //YkLogo.color = Color.white;
        AdviceTips.alpha = 0; // 默认先关闭一下
                              //为什么再这里因为下载的资源可能都不同
        GetHardward();//硬件，确认发布系统，【目标平台，和引擎软件设定在appmain】
        InitLocalSettingCache();
        SetQuality();
        //Application.quitting += OnQuit;

        UString.Initialize();

        LuaManager.Instance.Init();
    }

    private void InitLog()
    {
        //TODO:log日志 是本地写，log远程是发到远程，本地写感觉耗性能;查看方便远程 bugly
        //STAR_DEV 是正常测试都开，DevelopmentBuild是分情况开连调
        if (AppConfig.IsDev())
        {
            //BugLy + ：以下【本地log】 = 收集error + crash + 本地log

            SGF.Debuger.EnableLog = true;//能看就可能出异常，有小性能问题，但是排错，所以两个都要开
            SGF.Debuger.EnableTime = true;
            SGF.Debuger.EnableSave = false;//SGF.Debuger.EnableSave = true;//能出异常就要写,性能问题但是可能崩帮助//有bugly了，不用本地了，不代表有gm就要写本地
            SGF.Debuger.EnableStack = true;
#if UNITY_EDITOR
            SGF.Debuger.UseUnityEngine = true;
#else
            SGF.Debuger.UseUnityEngine = false;
#endif
            /* SGF.Debuger.LogFileWriter = new System.IO.StreamWriter();*/
            SGF.Debuger.LogFileDir = string.Empty;/*Application.persistentDataPath + "/UnityLog.txt";*/
#if UNITY_ANDROID && !UNITY_EDITOR
            SGF.Debuger.LogFileDir = Application.persistentDataPath;
#endif


            //SGF.Debuger.LogFileName = "UnityLog.txt";
        }
        else //正式版
        {
            //【BugLy】= 收集error + crash
            //有bugly了不要链条出log了
            SGF.Debuger.EnableLog = false;//只是Log用不着
            SGF.Debuger.EnableTime = false;//各个级别公用的，必须要有
            SGF.Debuger.EnableSave = false;//不要了
            SGF.Debuger.EnableStack = false;//error会强制开堆栈，  这个就是控制log级开不开
#if UNITY_EDITOR
            SGF.Debuger.UseUnityEngine = true;
            SGF.Debuger.EnableLog = true;//只是Log用不着
#else
            SGF.Debuger.UseUnityEngine = false;
#endif
            /* SGF.Debuger.LogFileWriter = new System.IO.StreamWriter();*/
            SGF.Debuger.LogFileDir = string.Empty;/*Application.persistentDataPath + "/UnityLog.txt";*/
#if UNITY_ANDROID && !UNITY_EDITOR
            SGF.Debuger.LogFileDir = Application.persistentDataPath;
#endif


            //SGF.Debuger.LogFileName = "UnityLog.txt";
        }
    }

    private float CPU_SCORE;
    private void GetHardward()
    {
      

        // 检查当前平台
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                _graphQualityLevel = 0;
                break;
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                //一定是android包，但是確是在window上運行的特徵
                isEmulator = IsRunningOnEmulator();
                GameConfig.isEmulator = isEmulator;
               
                SetGraphQualityLevelForMobile();
                break;
            default:
                // 如果是其他平台，默认值为2
                _graphQualityLevel = 2;
                break;
        }
        Debug.Log("Graph quality level set to: " + _graphQualityLevel);
    }

    private void SetGraphQualityLevelForMobile()
    {

        //先不统计了
        //功能等中台
        //先简单写了



        CPU_SCORE = SystemInfo.processorCount * SystemInfo.processorFrequency;//2496MHZ == 2.5G
        if (SystemInfo.deviceName.Contains("Mi", StringComparison.Ordinal))//小米是骗子 2倍给你表现
        {
            if (CPU_SCORE >= 8 * 6600)
            {
                _graphQualityLevel = 0;
            }
            else if (CPU_SCORE >= 8 * 6300)//红米K70  3190 实际是 6374
            {
                _graphQualityLevel = 1;
            }
            else if (CPU_SCORE >= 8 * 5600)/*小米11 外部说的是 2840  实际是5683*/
            {
                _graphQualityLevel = 1;
            }
            else
            {
                _graphQualityLevel = 1;
            }
            //缓存和设定怎么选
            //缓存 > 推荐设定 ： 机器不行他就要高配那就高配
            //老数据和 新数据设定 ： 清理数据一切以新的目标为主
        }
        else if (SystemInfo.deviceName.Contains("HUAWEI", StringComparison.Ordinal) ||
    SystemInfo.deviceName.Contains("HONOR", StringComparison.Ordinal))
        {
            // 这是华为或荣耀设备:老大要提高华为的荣耀炫耀程度。
            if (CPU_SCORE >= 8 * 2890)
            {
                _graphQualityLevel = 0;
            }
            else if (CPU_SCORE >= 8 * 2590)//红米K70  3190 实际是 6374
            {
                _graphQualityLevel = 1;
            }
            else if (CPU_SCORE >= 8 * 2490)/*小米11 外部说的是 2840  实际是5683*/
            {
                _graphQualityLevel = 1;
            }
            else
            {
                _graphQualityLevel = 1;
            }
        }
        else
        {
            if (CPU_SCORE >= 8 * 3160)//gen2 小米13 去年旗舰
            {
                _graphQualityLevel = 0;
            }
            else if (CPU_SCORE >= 8 * 3080)//K70 mi 去年阉割
            {
                _graphQualityLevel = 1;
            }
            else if (CPU_SCORE >= 8 * 2590)//华为meta60 小米11 
            /*小米11 外部说的是 2840  实际是小米谎报数据5683*/
            //华为meta60才2620,这个要是中配啊标准下降到2590 //2840 .2800 2620 2590
            {
                _graphQualityLevel = 1;
            }
            else
            {
                _graphQualityLevel = 1;
            }
        }


    }



    private void InitLocalSettingCache()
    {
        // 读取图像品质设置的缓存
        {
            bool isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_GRAPH, "Setting");
            if (isExists)
            {
                _graphQualityLevel = SaveManager.Instance.Load<int>(GameConfig.SETTING_GRAPH, "Setting");
            }

            isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_SGSR, "Setting");
            if (isExists)
            {
                GameConfig.UseSGSR = SaveManager.Instance.Load<bool>(GameConfig.SETTING_SGSR, "Setting");
            }

            isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_SGSR_EdgeSharpness, "Setting");
            if (isExists)
            {
                GameConfig.SGSR_EdgeSharpness = SaveManager.Instance.Load<float>(GameConfig.SETTING_SGSR_EdgeSharpness, "Setting");
            }
        }
    }



    private void Start()
    {
        string videoName = "Video/YoKaLogoH.mp4";
        switch (LanguageManager.Instance.CurLanguageType)
        {
            case LanguageType.None:
                break;
            case LanguageType.Chinese:
                break;
            case LanguageType.English:
                videoName = "Video/YoKaLogoH_en.mp4";
                break;
            default:
                break;
        }
        string path = Utils.GetVideoPath(videoName);
        LogoMediaPlayer.source = VideoSource.Url;
        LogoMediaPlayer.url = path;
        LogoMediaPlayer.Prepare();
        //LogoMediaPlayer.gameObject.SetActive(true);
    }

    /*void PlayVideo(VideoClip vlip)
    {
        //可以引用外部视频 也可以通过资源加载
        RenderTexture targetTexture = RenderTexture.GetTemporary(m_Texture.width, m_Texture.height, 16);
        m_Texture.mainTexture = targetTexture;
        m_Video.clip = clip;
        m_Video.targetTexture = targetTexture;
        m_Video.isLooping = false;
        m_Video.Play();
    }*/
    /*void OnStartVideo(VideoPlayer source)
    {
        //todo 开始播放
    }
    void EndWithVideoPlay(VideoPlayer source)
    {
        //TODO播放完成后事件
    }*/

    bool isEmulator = false;
    bool IsRunningOnEmulator()
    {

        // 获取设备型号
        /*string product = SystemInfo.deviceModel.ToLower();*/
        string processorType = SystemInfo.processorType.ToLower();
        // 這裏給的是手機型號：
        UnityEngine.Debug.LogError("111111111processorType:" + processorType);

        // 常见的桌面 CPU 型号
        string[] emulatorProcessorTypes = { "x86", "x86-64", "amd", "intel", "generic","86" };//86-64 sse3 sse4.1 sse4.2 avx
        foreach (string desktopProcessor in emulatorProcessorTypes)
        {
            if (processorType.Contains(desktopProcessor))
            {
                isEmulator = true;
            }
        }


        // 常见的移动 CPU 型号
        string[] mobileProcessors = { "snapdragon", "apple", "mediatek", "exynos", "arm64" };//arm64 fp asimd aes
        foreach (string mobileProcessor in mobileProcessors)
        {
            if (processorType.Contains(mobileProcessor))
            {
                isEmulator = false;
            }
        }


        // 检查常见的模拟器特征
        /* bool isEmulator = product.Contains("sdk") || product.Contains("emulator") || product.Contains("genymotion")
             || product.Contains("mumu") || product.Contains("nox") || product.Contains("unknown")
             || product.Contains("ldplayer") || product.Contains("bluestacks") || product.Contains("xyaz")
             || product.Contains("tencent");*/
        UnityEngine.Debug.LogError("111111111isEmulator:" + isEmulator);
        return isEmulator;
    }
    private void SetQuality()
    {
        //在哪运行的
        //保持屏幕比例，保持2n（不必），上限表现精度
        //暂时不开启动态分辨率：ui能自动刷么，两个canvas都要重新awake，所有界面都要reinit

        //0，识别高中低配
        CheckQuality();
        //1，先设定清晰视觉 460
        //Done
        //2，分分辨率:最少等比减少到720P
        SetResolution();
        //3,分高地配置,对应处理


        //开关GRSR
        GameManager.Instance.SetSGSR(GameConfig.UseSGSR);

        //=======================================================================================
        //显卡
        //内存
        //CPU
        //硬盘
        //machineQualityLevel
        //这里必须做，检测带宽必然频繁io也不对，所以邻居哪个只是静止更好
        //但是首先要有全局性能检测和控制，然后再去找机器的带宽
        //=======================================================================================
    }

    private void CheckQuality()
    {
        GameConfig.MachineQualityLevel = (MachineQualityLevel)_graphQualityLevel;
        GameConfig.InitQualityLevel = (MachineQualityLevel)_graphQualityLevel;
    }

    /// <summary>
    /// 模拟器的额外降低分辨率和延后启动的APPmain性能设置，互相不冲突也互相不影响
    /// </summary>
    private void SetResolution()
    {


#if UNITY_ANDROID
 //1，最低是中配任何情况下；2，模拟器老大说不在压缩了；3，模拟器识别还是低配这个是个bug要看看
       /* 
        if (isEmulator)
        {
            Debuger.LogWarning("发现模拟器特征，会压缩分辨率：特征是：" + SystemInfo.deviceModel.ToLower());
            scWidth = Screen.width;
            scHeight = Screen.height;

            float paraAutio = (float)Screen.width / (float)Screen.height;

            int ScaleWidth = Screen.width / 3 * 2;
            int ScaleHeight = Screen.height / 3 * 2;
            //1080 -> 720

            //1，原来就是720什么都不做,我默认比例了，其实ratio大于2的720高的屏幕，没法享受优化了目前     
            //但是小于720的异形屏幕：也不能缩了
            //例如1281*1281的方块，9999*721的方块基本不会优化漏检测了，但通常能检测到贫穷像素机约等于贫穷机
            //优化为固定小像素

            if (scWidth <= designWidth && scHeight <= designHeight)
            {
                return;
            }

            //2，大于等于他没必要取到720，小于720就720吧
            if (ScaleHeight <= designHeight)
            {
                ScaleHeight = designHeight;
                ScaleWidth = (int)(paraAutio * designHeight);
            }

            //3，大于720P就应用缩放的分辨率
            Screen.SetResolution(ScaleWidth, ScaleHeight, true);
        }*/
        /*   */
#endif
    }

    /*    private void OnMediaPlayerEventHandler(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
            {
                //启动热更
                //HotMediaPlayer.gameObject.SetActive(true);
                //DisplayUGUI._mediaPlayer = HotMediaPlayer;
                LogoMediaPlayer.gameObject.SetActive(false);
                GameProcess.gameObject.SetActive(true);
                GameUpdate.CheckUpdate(GameProcess);
            }
        }*/

    // 1.播放公司的logo
    private void UnityVideoPlayOver(VideoPlayer source)
    {
        SDKManager.Instance.SetOnSdkInit(() =>
        {
            PrepareToUpdate();
        });
    }

    private void PrepareToUpdate()
    {
        string deviceInfo = "OS: " + SystemInfo.deviceType + " - " + SystemInfo.deviceModel + " - " + SystemInfo.operatingSystem + " - " + Application.systemLanguage;
        string cpuInfo = "CPU: " + SystemInfo.processorType + " - " + SystemInfo.processorCount + " threads - " + +SystemInfo.systemMemorySize + "KB";
        string gfxInfo = "GPU: " + SystemInfo.graphicsDeviceName + " - " + SystemInfo.graphicsDeviceVendor + " - " + SystemInfo.graphicsDeviceVersion + " - " + SystemInfo.graphicsMemorySize + "KB - " + SystemInfo.maxTextureSize;
        SGF.Debuger.LogWarning("上报手机型号" + deviceInfo + cpuInfo + gfxInfo);
        Debug.Log("1.logo播放视频 -- 结束");
        //LogoMediaPlayer.gameObject.SetActive(false);
        if (LanguageManager.Instance.CurLanguageType == LanguageType.Chinese)
        {
            PlayAdvice();
        }
        else
        {
            DOVirtual.DelayedCall(0.1f, CheckGameUpdate);
        }
        //LogoMediaPlayer.gameObject.SetActive(false);
        //if (!isPlayPVVidel)
        //{
        //    Debug.Log("1.logo播放视频 -- 结束");
        //    PlayAdvice();
        //}
        //else
        //{
        //    LogoMediaPlayer.gameObject.SetActive(false);
        //    CheckGameUpdate();
        //}
    }

    // 2.播放八字
    private void PlayAdvice()
    {
        Debug.Log("2.播放八字 -- 开始");
        //YkLogo.color = Color.clear;
        AdviceTips.alpha = 1f;
        DOVirtual.DelayedCall(3, CloseAdvice);
    }

    private void CloseAdvice()
    {
        Debug.Log("2.播放八字 -- 结束");
        AdviceTips.alpha = 0f;
        CheckGameUpdate();
        ////var tween = AdviceTips.DOFade(0.5f, 3);
        ////tween.onComplete = () =>
        ////{
        ////    Debug.Log("2.播放八字 -- 结束");
        ////    AdviceTips.alpha = 0f;
        ////    PlayPVVideoPlay();
        ////};
    }

    // 4.游戏更新，切换场景
    private void CheckGameUpdate()
    {
        Debug.Log("4.游戏更新，切换场景");
        GameProcess.gameObject.SetActive(true);
        GameUpdate.CheckUpdate(GameProcess);
    }

    private void OnDestroy()
    {
        /*  LogoMediaPlayer.Events.RemoveListener(OnMediaPlayerEventHandler);*/
        LogoMediaPlayer.loopPointReached -= UnityVideoPlayOver;//结束回调
    }

    //public static void OnQuit()
    //{
    //    Application.quitting -= OnQuit;
    //    UnityEditor.EditorPrefs.SetBool("LocalPlay", false);
    //}
}
