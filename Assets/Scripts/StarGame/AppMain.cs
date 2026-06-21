using ClientNpc;
using DG.Tweening;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Game.Skill;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.Cam;
using StarProject.Service.CameraShake;
using StarProject.Service.DisplayProcess;
using StarProject.Service.FindPath;
using StarProject.Service.Input;
using StarProject.Service.LocalDynamic;
using StarProject.Service.Lua;
using StarProject.Service.ServerService;
using StarProject.Service.SystemOpen;
using StarProject.Service.Time;
using StarProject.Service.Timeline;
using StarProject.Service.TriggerEntity;
using StarProject.Service.UniversalRenderPipeline;
using StarProject.Service.User;
using StarProject.Service.WorldToUI;
using StarProject.UI.StarWorld;
using StarProjectDef;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/// <summary>
/// 项目第一个管理器
/// </summary>
/// 
///玩家设定也可以修改画质，不过低配机器选择不了高级配置
public enum MachineQualityLevel : byte
{
    [Description("高配")]
    TopLevel = 0,//顶级机器
    [Description("一般")]
    MiddleLevel = 1,//中配机
    [Description("低")]
    LowerLevel = 2,
    [Description("省电")]
    LowestLevel = 3,
    [Description("高高配")]
    TopestLevel = 4,//Timeline最高还原手机配置
}
public enum ModelQualityLevel
{
    Low = 0,//低模
    Middle = 1,//仅自己高模
    High = 2,
}

/// <summary>
/// TODO: DELETE
/// 临时加的 防止代码报错
/// </summary>
public static class CustomShadowPerformanceAdepter
{
    public static void SetDynamicLayer(bool isDynamic)
    {

    }
}
public class AppMain : MonoSingletonEx<AppMain>
{
    //记录游戏加载逻辑，static isInited ; MonoSingleton*
    //1，一次加载，一次载入
    //2，掌管App类逻辑，如视频播放
    //public static AppMain Instance;
    private E_SceneType CurrentSceneType;

    private bool videoLoaded = false;

    private bool isVulken = false;

    UniversalRenderPipelineAsset urpAsset;

    //注意继承MonoSingleton，子类的Awake会覆盖流程
    protected override void Init()
    {
        urpAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Loom.Initialize();
        ModuleDef.Init();
        //AppConfig.Init();
        //本地设定缓存，和质量设置再gameperpare中因为要准备的资源可能都不同。.
        //选择安装平台
        CheckDriverPlatformRenderType();
        //根据平台进行优
        InitEngineSetting();
        //依据脱离引擎机制，平台的APP进行监听
        InitAppSetting();
        //游戏内服务模块开启
        InitServices();
        //游戏业务模块开启
        InitBusiness();
        //游戏辅助工具开启
        InitHelper();
        //启动三方插件
        InitThd();
        //RegPerLoadVideo();
        OnFinishedPlaying();
        //资源预加载
        //PerLoadAddressable();

    }


    //===========
    //===========

    private void InitThd()
    {
        DOTween.Init(false, true, LogBehaviour.Verbose).SetCapacity(300, 50);
    }

    private void InitAppSetting()
    {
        APPHelper.Instance.InitAPPHelper();
    }


    public static bool IsPlayOnAndroidEmulator()
    {
        //#if !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
        {
            if (SystemInfo.graphicsDeviceVendorID == 0)
            {
                return true;
            }
            //SystemInfo.graphicsDeviceID = 0;

        }

        //#endif
        return false;
    }

    private void CheckDriverPlatformRenderType()
    {

        // 获取当前平台的图形API设置;SystemInfo.graphicsDeviceType会自动返回当前平台的图形API设置，不需要额外指定平台。
        // 所以是这样的，配置里面首先能检测的到你配置了什么
        // 如果你vk 和 ogl 按照这个顺序配置了，就会根据顺序依次生效
        // 如果 vk 是手机的 默认用vk ，如不行就下降到 ogles3
        // 所以如果要记录就用editor记录txt文本来读取
        // 运行时不必指定平台，而且生成什么只有一个
        // 当我目前配置只有一个的情况 vk 必定不生效
        // 现实手机 80%支持vk 100都是ogl
        // 所以你配置vk 80%生效，不配置 100默认下探ogl
        // 所以ogl 和 vk 决定于你配置不配置
        GraphicsDeviceType/*[]*/ apis = SystemInfo.graphicsDeviceType;
        isVulken = false;
        // 检查是否包含Vulkan API
        /*  foreach (GraphicsDeviceType api in apis)
          {
              if (api == GraphicsDeviceType.Vulkan)
              {
                  isVulken = true;
              }
          }*/
        isVulken = apis == GraphicsDeviceType.Vulkan;


        /*   //【#编辑器下检测】
           BuildTarget target = BuildTarget.Android; // 你要检测的构建目标

           // 获取对应构建目标的图形API设置
           GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(target);

           isVulken = false;

           // 检查是否包含Vulkan API
           foreach (GraphicsDeviceType api in apis)
           {
               if (api == GraphicsDeviceType.Vulkan)
               {
                   isVulken = true;
                   break;
               }
           }
   */
        if (isVulken)
        {
            Debug.Log("Vulkan API enabled for: " + isVulken);
        }
        else
        {
            Debug.Log("Vulkan API not enabled for: " + isVulken);

        }





    }

    public float ScreenDPICache;
    private void InitEngineSetting()
    {
        ScreenDPICache = Screen.dpi;
        //如果只是在Quality Settings中手动勾选，则Editor下会起作用，但移动端可能会出现不起作用的情况。注意：真机android测试功能，真机调试内存数据表现别信EditorPro
        QualitySettings.streamingMipmapsActive = true;

        //去掉缓存池，需要配合帧处理删除频繁io当时保持整洁，相机变数较少的情况；默认的false是让被动删除替换具备缓存池
        //Texture.streamingTextureDiscardUnusedMips = true;

        //在实际项目中，我们可能需要针对不同性能的机型使用Mipmap偏移。要想达到Mipmap偏移有几种方法：
        //其默认值为0，将其值设置为x，会对所有开启Mipmap的Texture2D资源（不管是否开启Mipmap Streaming）使用第x级Mipmap。
        //【全局】分辨率压缩，最大值;压分辨率
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.maximumLODLevel = 0;//Lod草地分三级，物件分成2级不可想用降级策略
        GameConfig.AllowDynamicResolution = true;

        SetQualityLevel(GameConfig.MachineQualityLevel);

        //动态加载的才生效，另外可以配置相机渲染上限，并且Budget少一点可以看出效果
        //【相机】Streaming Controller组件上的Mip Map Bias：此值只在QualitySettings.AddAllCamera未启用，且在Camera上激活Streaming Controller时起作用。此设置会针对当前摄像机需要渲染的Renderer其使用的Texture（开启Mipmap Streaming的Texture）进行Mipmap偏移，比较推荐使用这一种方法。
        /*     StreamingController.streamingMipmapBias;//相机组件
      CancelPreloading 中止预加载。
     IsPreloading 用于确定 StreamingController 当前是否正在预加载纹理 Mipmap。
     SetPreloading*/



        //对于单个Texture设置其Mipmap偏移。
        //Texture.mipMapBias


        //OCCLU配合九宫格还是相机的0.33


        //==========================基于mip池管理：================================================
        //本身就适合moba，fps，free3Dgame，飞行：锁定视角在于内存管理优化
        //AOI不在视觉范围内影响也不大
        /*        1aoi变小
        2aoi就等于occlu
        3就等于模型*/

        //false：流式加载具备缓存池，清理缓存的时机是新图能不能进来，通常是具备缓存池 ， Moba，ACT镜头，FreeCamera3DGame相对有规律；缓存还是有意义，复用性变化频度不高，池就有意义，被动式清理基于新的图片进来，清理图片和自我池

        //true：不具备池，无用则立刻删除，适用于频繁切换，且内存要求严格空间，CPU不严格（耗），类似游戏是，高速移动飞机模拟游戏，准星，狙击步枪瞄准；他更适合MipMap复用性非常低，原来是高mipN很快就Mip0了  主动式接近于缓存毫无意义不如全用CPU也别浪费我内存，

        //频繁切换有池更健壮,一旦有规律就推荐false，显卡式可以省的只要gMip都节省，aut都是有效式，
        //strMip是基于False情况有预算，和优先Redu（美术表现最好）（美术加载优先级）优先加载的

        //是设定，不是方法
        Texture.streamingTextureDiscardUnusedMips = false;//改变变更不明显，基于新图加入，人位置挪动想渲染新的mip请求内存空间；被动式，缓存式，请求频度低，高效利用
        //Texture.force
        //==========================基于mip池管理：================================================

        //QualitySettings.streamingMipmapsMemoryBudget
    }

    //接口提取出来，外部可以使用
    public void SetQualityLevel(MachineQualityLevel quanlityLevel)
    {
        switch (quanlityLevel)
        {
            case MachineQualityLevel.TopestLevel:
                UseHighHighHighLevelSetting();
                break;

            case MachineQualityLevel.TopLevel:
                UseHighLevelSetting();

                break;
            case MachineQualityLevel.MiddleLevel:
                UseMiddleLevelSetting();

                break;
            case MachineQualityLevel.LowerLevel:
                UseLowLevelSetting();

                break;
            case MachineQualityLevel.LowestLevel:
                UseLowestLevelSetting();

                break;
            default:
                QualitySettings.masterTextureLimit = 0;//通过设置设置最大精度
                break;
        }
    }


    public int PRIQuality_Index = 0;
    /// <summary>
    /// TimeLineRender时候能用到
    /// 设置中看不到
    /// </summary>
    public void UseHighHighHighLevelSetting()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.masterTextureLimit = 0;//降低分辨率但是不修改ui
        QualitySettings.maximumLODLevel = 0;//没问题：Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用物件0~1都是1级物件渲染策略：共享等级
        QualitySettings.lodBias = 1f;//1是向上取整,他不是0~1他之所以不能0是因为0次幂没法拉伸lod横线
        GameConfig.QualityForCameraClipNeighbor = 2;//慢速裁切都表现
        QualitySettings.resolutionScalingFixedDPIFactor = ScreenDPICache;
        //SetFixDPIFac(0.46f);
        float scale = 1.0f;

        //场景想调整必须开这个，这个设置关联resizebuffer；ui想改变这个也要
        //CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.allowDynamicResolution = true;
        if (isVulken)
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        else
        {

            urpAsset.renderScale = scale;
        }
        //设置Shader LOD
        Shader.globalMaximumLOD = 500;
        //开关后处理
        UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        uacd.renderPostProcessing = true;

        //高级自由帧率
        //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);
        Application.targetFrameRate = 60;//不是那个time，time是逻辑执行；；；；这个是渲染速率
        QualitySettings.vSyncCount = 0;//这个是不等硬件
        DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 80;
        SetOutline(true);
        urpAsset.supportsSoftShadows = true;

        SetDownScaleRendererRF(0.9f, 0.9f);//地表，水
        if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
        {
            GameManager.Instance.M_Map.M_rootHelper.RefleshVolume();
        }
        //UniversalAdditionalCameraData 优先级更高，
        //全局设定msaa优先级最高 > UniversalAdditionalCameraData如果选定Msaa次高 > ALLOWMSAA是区别多个相机谁能接受信号的五官紧要
        SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.High);
        LocalFxManager.Instance.MaxFxCount = 50;

        //shadow setting
        //DecalShadowMgr.instance.enable = false;
        CustomShadowPerformanceAdepter.SetDynamicLayer(true);


        PRIQuality_Index = 0;
        RefQualitySetting();
    }




    public void UseHighLevelSetting()
    {
        //最高级别可以还原   QualitySettings.resolutionScalingFixedDPIFactor = ScreenDPICache；
        //特效Lod√
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.masterTextureLimit = 0;//降低分辨率但是不修改ui
        QualitySettings.maximumLODLevel = 0;//没问题：Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用物件0~1都是1级物件渲染策略：共享等级
        QualitySettings.lodBias = 1f;//1是向上取整,他不是0~1他之所以不能0是因为0次幂没法拉伸lod横线
        GameConfig.QualityForCameraClipNeighbor = 2;//慢速裁切都表现
        QualitySettings.resolutionScalingFixedDPIFactor = 0.326f;
        //SetFixDPIFac(0.46f);
        float scale = 1.0f;
        if (isVulken)
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        else
        {

            urpAsset.renderScale = scale;
        }

        //设置Shader LOD
        Shader.globalMaximumLOD = 500;
        //开关后处理
        UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        uacd.renderPostProcessing = true;
        //高级自由帧率
        //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 50;
        SetOutline(true);
        urpAsset.supportsSoftShadows = true;
        SetDownScaleRendererRF(0.85f, 0.85f);//地表，水
        if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
        {
            GameManager.Instance.M_Map.M_rootHelper.RefleshVolume();
        }
        SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.Medium);
        LocalFxManager.Instance.MaxFxCount = 10;

        //shadow setting
        //DecalShadowMgr.instance.enable = false;
        CustomShadowPerformanceAdepter.SetDynamicLayer(true);
        PRIQuality_Index = 0;
        RefQualitySetting();
    }


    public void UseMiddleLevelSetting()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.masterTextureLimit = 1;//通过设置设置最大精度
        GameConfig.QualityForCameraClipNeighbor = 1;//快速裁切
        QualitySettings.maximumLODLevel = 1;//Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用物件0~1都是1级物件渲染策略：共享等级
        QualitySettings.lodBias = 0.6f;//可不是只显示1级的0.6是概念距离的缩减；就算是按照0.6倍数拉动距离条，但是裁切的lod是固定距离的所以表现会很不同
                                       //SetFixDPIFac(0.36f);//接近比例尺：人眼睛极限
        QualitySettings.resolutionScalingFixedDPIFactor = 0.326f;
        float scale = 0.8f;
        if (isVulken)
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        else
        {
            urpAsset.renderScale = scale;
        }


        //设置Shader LOD
        Shader.globalMaximumLOD = 200;
        //开关后处理
        UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        uacd.renderPostProcessing = true;
        //中级自由帧率
        /* Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);//不但老旧，而且ui也一起弄了*/
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;//硬件等待
        DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 0;//主角交互必然留，子弹必然不要，这个是其他全去掉
        SetOutline(false);
        urpAsset.supportsSoftShadows = true;
        SetDownScaleRendererRF(0.75f, 0.75f);//地表，水//有点早所以必须cameraa.main，设置只有一次，所以反射也可以
        if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
        {
            GameManager.Instance.M_Map.M_rootHelper.RefleshVolume();
        }
        SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
        LocalFxManager.Instance.MaxFxCount = 10;

        //shadow setting
        //DecalShadowMgr.instance.enable = true;
        CustomShadowPerformanceAdepter.SetDynamicLayer(true);
        PRIQuality_Index = 1;
        RefQualitySetting();
    }


    public void UseLowLevelSetting()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.masterTextureLimit = 2;//通过设置设置最大精度
        GameConfig.QualityForCameraClipNeighbor = 0;//快速裁切
        QualitySettings.maximumLODLevel = 1;//Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用
        QualitySettings.lodBias = 0.5f;//0是向下取整  
                                       //物件0~1都是1级物件渲染策略：共享等级
                                       //SetFixDPIFac(0.30f);//接近比例尺：
                                       //因为经过压缩了，也别自由了，你最高60
                                       //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);
        QualitySettings.resolutionScalingFixedDPIFactor = 0.3f;
        float scale = 0.5f;
        if (isVulken)
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        else
        {
            urpAsset.renderScale = scale;
        }

        //设置Shader LOD
        Shader.globalMaximumLOD = 200;
        //开关后处理
        UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        uacd.renderPostProcessing = false;

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 0;
        SetOutline(false);
        urpAsset.supportsSoftShadows = false;
        SetDownScaleRendererRF(0.35f, 0.35f);//地表，水
        if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
        {
            GameManager.Instance.M_Map.M_rootHelper.RefleshVolume();
        }
        SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
        LocalFxManager.Instance.MaxFxCount = 10;

        //shadow setting
        //DecalShadowMgr.instance.enable = true;
        CustomShadowPerformanceAdepter.SetDynamicLayer(false);
        PRIQuality_Index = 2;
        RefQualitySetting();
    }


    public void UseLowestLevelSetting()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        QualitySettings.masterTextureLimit = 3;//通过设置设置最大精度
        GameConfig.QualityForCameraClipNeighbor = 0;//快速裁切
        QualitySettings.maximumLODLevel = 2;//Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用
        QualitySettings.lodBias = 0.3f;//0是向下取整  
                                       //物件0~1都是1级物件渲染策略：共享等级
                                       //SetFixDPIFac(0.30f);//接近比例尺：
        QualitySettings.resolutionScalingFixedDPIFactor = 0.25f;
        //锁定30即可了，你机器实在不行了。
        //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 30);

        // TOOD 和马老板确认这个暂时不开放，所以仅先留代码在这，如启用，缩放比例需重新考虑
        float scale = 0.5f;
        if (isVulken)
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        else
        {
            urpAsset.renderScale = scale;
        }

        //设置Shader LOD
        Shader.globalMaximumLOD = 200;
        //开关后处理
        UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        uacd.renderPostProcessing = false;

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 0;
        SetOutline(false);
        urpAsset.supportsSoftShadows = false;
        SetDownScaleRendererRF(0.35f, 0.35f);//地表，水
        if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
        {
            GameManager.Instance.M_Map.M_rootHelper.RefleshVolume();
        }
        SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
        LocalFxManager.Instance.MaxFxCount = 10;

        //shadow setting
        //DecalShadowMgr.instance.enable = true;
        CustomShadowPerformanceAdepter.SetDynamicLayer(false);
        PRIQuality_Index = 2;
        RefQualitySetting();
    }

    public void RefQualitySetting()
    {
        QualitySettings.SetQualityLevel(PRIQuality_Index);
        UnityEngine.Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Normal;
    }

    public void LoadingQualitySetting()
    {
        QualitySettings.SetQualityLevel(4);
        UnityEngine.Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;

    }

    /// <summary>
    /// 模型质量是晓鹏封接口，默认高中低配置走高中低不变，但是可以人为修改人为修改的优先级高于模型质量的设置
    /// ViewAoi：680
    /// </summary>

    public void IsShowOutline(bool isShowOutline)
    {
        if (isShowOutline)
        {

        }
        else
        {
            DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = 0;
        }

    }


    /// <summary>
    /// 屏蔽他人特效
    /// </summary>
    /// <param name="OnlyShowMainPlayerFX"></param>
    public void SetOnlyMainPlayerShowFX(bool OnlyShowMainPlayerFX)
    {
        LocalFxManager.Instance.OnlyShowMainPlayerFX = OnlyShowMainPlayerFX;
    }

    public void SettingCameraAAByType(bool isOpen, GraphLevelEnum level)
    {
        switch (level)
        {
            case GraphLevelEnum.None:
                SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                break;
            case GraphLevelEnum.Lowest:
                SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                break;
            case GraphLevelEnum.Low:
                SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                break;
            case GraphLevelEnum.Middle:
                SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.Medium);
                break;
            case GraphLevelEnum.High:
                SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.High);
                break;
            case GraphLevelEnum.Custom:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 抗锯齿
    /// </summary>
    /// <param name="Msaa"></param>
    /// <param name="antialiasingMode"></param>
    /// <param name="antialiasingQuality"></param>
    public void SettingCameraAA(bool Msaa, AntialiasingMode antialiasingMode = AntialiasingMode.None, AntialiasingQuality antialiasingQuality = AntialiasingQuality.Low)
    {
        if (CameraManager.Instance != null)
        {
            UniversalAdditionalCameraData uacd = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).gameObject.GetComponent<UniversalAdditionalCameraData>();
            uacd.antialiasing = antialiasingMode;
            uacd.antialiasingQuality = antialiasingQuality;
            CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera.allowMSAA = Msaa;
            uacd.allowXRRendering = false;
            //UI=XR MsAA 不要抗锯齿 
            UniversalAdditionalCameraData uacd1 = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.UICam).gameObject.GetComponent<UniversalAdditionalCameraData>();
            uacd1.antialiasing = antialiasingMode;
            uacd1.antialiasingQuality = antialiasingQuality;
            CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.UICam).Camera.allowMSAA = Msaa;
            uacd.allowXRRendering = false;
            //RAW=XR MsAA 高级抗锯齿
            UniversalAdditionalCameraData uacd2 = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.SpecialCam).gameObject.GetComponent<UniversalAdditionalCameraData>();
            uacd2.antialiasing = antialiasingMode;
            uacd2.antialiasingQuality = antialiasingQuality;
            CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.SpecialCam).Camera.allowMSAA = Msaa;
            uacd.allowXRRendering = false;
        }
    }

    //同屏角色数量
    //小曲没做
    public void MaxPlayerCount()
    {


    }



    ///  targetPixelDensity: 995
    ///  resolutionScalingMode: 0
    ///直接降低分辨率的好办法：1不同于全部降低分辨率（1080p-720p--*UI要注意别放大是否可以），设置刷新帧率；2也不同于mipmap（不降低ui）；3是依据dpi查看的分辨率降低（锚定其眼睛看不出来）
    //private static void SetFixDPIFac(float para)
    //{
    //    //这里运行时修改不生效，基于默认unity设置初始化生效，涉及者意识高配能跑多高跑多高
    //    //本来想超过300是看不出来的，但是设计者说是可以看出来的，就保持适配才可以关掉
    //    //如果有此设定，必然可以，按照人的眼睛极限来节省渲染量级
    //    //他不会重新刷新,所以不会适配策略，要么找到刷新
    //    //1，不可运行时修改（下面那个是多窗口），2，unity这个属于开始的策略，3，不可因为多状态动态改变也不是urp管钱，4，脚本设置无用，5，需求也是全显示，6，这个功能暂时不开启保持手机不关心DPI就好,所以给ScaleMode关了
    //    //QualitySettings.resolutionScalingFixedDPIFactor = para;//接近无限高：30 16 479

    //    //Iphone460是目前最高的分辨率
    //    //设定是:能高就最高，中间视野分辨机型，低等视野分辨机型
    // unity先算了一下他的DPI，然后你填写一个DPI，看谁小为A，这个清晰度，如果大于他就得到一个系数，去缩小分辨率
    ////min(Target DPI * Factor / Screen DPI, 1)
    ////min（460 * 1 / 360,1）= 1; BigLed = 就用1, 分辨率不降低，小于你的值按照你的分辨率！！！460顶级就好 1000也行 * 0.46
    ////min（460 * 1 / 920,1）= 1; MicroLed = 0.5, 分辨率0.5就够了，因为960意味着分辨率一样屏幕变小了所以可以降低；大于你的值可以缩小分辨率
    //// 问题是：460 * 460 / 360，永远不缩放
    ////
    //    /*因子是系数大于1变成更难被缩放，同样机器
    //    因子系数小于1变成更容易被缩放，同样机器
    //    就很难理解
    //    所以你要调整目标系数统一理解  X*Y = 目标DPI
    //    1000*/
    ////问题是460 * 460 / 360，永远不缩放

    //}


    /// <summary>
    /// GM工具 和 运行时需要的MonoHelper
    /// </summary>
    private void InitHelper()
    {
        if (AppConfig.IsDev())
        {
            GMCommand.Instance.GetInstanceID();
        }
        if (AppConfig.IsGM())
        {
            GMManager.Instance.Init();
        }
    }


    #region 如游戏需要大量播放视频，串联再游戏流程中，需要提取到 媒体服务管理类
    public void OpenVideo()
    {
        //用时再拓展
    }
    public void CloseVideo()
    {
        // avPro.CloseVideo();
        // avPro.gameObject.SetActive(false);
    }
    #endregion



    /// <summary>
    /// @切场景时，this不会携带到新场景。
    /// </summary>
    private void RegPerLoadVideo()
    {
        // string path = "Video/YoKaLogoH.mp4";
        string path = "Video/New/PYLogoH.mp4";
        // avPro.OnAvPreLoad(path, true);
        //AvProManager.Instance.PlayVideo(E_AVType.AvCG, path);
        //AvProManager.Instance.ActionOnFinishPlaying += OnFinishedPlaying;
    }

    private void OnFinishedPlaying()
    {
        videoLoaded = true;
        TriggerLoadScene();
    }

    private void InitGlobal(System.Action<GameObject> cb)
    {
        //ResourceManager1.Instance.Init();
        ResourceHelperMono.AddressablesLoadGPrefab(cb);// 1，异步会顺序错乱，2resmgr还没创建所以直接调用AA的同步创建
        //ResourceManager
    }

    private void InitServices()
    {
#if !GM && !STAR_DEV  //正式版本，会收集ERROR日志
        Application.logMessageReceived -= SGF.Debuger.OnLogMessageReceived;
        Application.logMessageReceived += SGF.Debuger.OnLogMessageReceived;
        //Lua报错:一个是lua报错，会被unity监听！；lua崩溃会被bugly收集！，lualog会被堆栈收集要么就多打点log

        //LuaEnv.
#endif
        ModuleManager.Instance.Init("StarProject.Module");
        //SDKManager.Instance.Init();
        MsgRetManager.Instance.Init();
        FixMessageManager.Instance.Init();
        Client2ThirdMsgManager.Instance.Init();
        NetworkManager.Instance.Init();
        //StarProject.Service.Resource.ResourceManager.Instance.Init();
        //StarProject.Service.Resource.ResourceFormalManager.Instance.Init();

        SGF.UI.Framework.UIManager.Instance.Init("UI/");
        SGF.UI.Framework.UIManager.MainPage = UIDef.UILoginPage;
        SGF.UI.Framework.UIManager.MainScene = "MainTown";
        SGF.UI.Framework.UIQueueManager.Instance.Init();
        RenderManager.Instance.Init();
        UserManager.Instance.Init();
        LuaManager.Instance.OnPostInit();
        //LocalDataManager.Instance.Init();
        PreLoading.Instance.Init();
        TimeManager.Instance.Init();
        LocalCache.Init();
        LocalCacheManager.Instance.Init();
        GameManager.Instance.Init();
        BusinessManager.Instance.Init();
        SlotManager.Instance.Init();
        CameraManager.Instance.Init();
        InputManager.Instance.Init();
        FindPathManager.Instance.Init();
        AtlasManager.Instance.Init();
        DisplayProcessDispenser.Instance.Init();
        StarProject.Service.Sound.SoundManager.Instance.Init();
        StarScenesManager.Instance.Init();
        TriggerEntityManager.Instance.Init(EnvironmentRoot.Instance.RemoteDynamicRoot);
        LocalFxManager.Instance.Init(EnvironmentRoot.Instance.LocalDynamicRoot);
        StarProject.Service.Function.GlobalFunctionManager.Instance.Init();
        //AvProManager.Instance.Init(AVProRoot.Instance);
        CameraShakeManager.Instance.Init();
        ServerServiceManager.Instance.Init();
        PartnerManager.Instance.Init();
        UniRenderPipline.Instance.Init();
        TimelineManager.Instance.Init();
        ClientNpcManager.Instance.Init();
        StarProject.Service.Battle.BattleManager.Instance.Init();
        StarProject.Service.Battle.FightPowerManager.Instance.Init();
        SkillEffectManager.Instance.Init();
        WorldItemChecker.Instance.Init();
        DynamicRenderQueueManager.Instance.Init();

        RedPointManager.Instance.Init();
        SystemOpenManager.Instance.Init();
        MutiScenesMergeManager.Instance.Init();

        //ServiceInitTest();

        LogModule.LogReport.Instance.Init();

    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // 当游戏从 暂停回来后, 重新请求一次 服务器时间
            TimeManager.Instance.RegReqServerTime();
        }
    }

    private void ServiceInitTest()
    {
        timer = new StarProject.Service.Time.Base.StarTimer(5f, false);
        timer.SetInterval(1f, OnInterval);
        timer.onEnter = OnStart;
        timer.onEnd = OnExit;



    }
    public void AppQuit()
    {
        Application.Quit();
    }
    private void OnExit()
    {
        SGF.Debuger.Log("=========星海项目成功载入=========" + TimeUtils.GetTimeSpanSince1970().Seconds);
    }

    private void OnStart()
    {
        SGF.Debuger.Log("=========星海项目开始加载=========" + TimeUtils.GetTimeSpanSince1970().Seconds);
    }
    private int count = 0;
    private void OnInterval(object args)
    {

        SGF.Debuger.Log("=========【" + count + "】=========" + TimeUtils.GetTimeSpanSince1970().Seconds);
        count++;
    }

    StarProject.Service.Time.Base.StarTimer timer;


    private void TriggerLoadScene()
    {
        if (videoLoaded)
        {
            TravelToScene(E_SceneType.GameEnter_Login, true);

        }
    }

    ScriptableRendererFeature featureDS;
    private void SetDownScaleRendererRF(float opaqueDownScale, float transparentDownScale)
    {
        var camera = Camera.main;//CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam);
        if (camera != null)
        {

            if (featureDS == null)
            {
                var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer((int)SGRenderingPaths.NewYokaRenderer);
                var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
                foreach (var feature in features)
                {
                    if (feature.GetType() == typeof(SGameDownScaleRenderer))
                    {
                        featureDS = feature;
                        break;

                    }
                }
            }


            if (featureDS != null)
            {
                featureDS.SetActive(true);
                (featureDS as SGameDownScaleRenderer).DownScale = opaqueDownScale;
                (featureDS as SGameDownScaleRenderer).TransparentDownScale = transparentDownScale;
            }


            //性能节省-首次
            //默认开启-代码中，不加关闭开启
            //正确的值阈 



        }
    }

    ScriptableRendererFeature featureOutline;
    private void SetOutline(bool active)
    {
        var camera = Camera.main;
        if (camera != null)
        {

            if (featureOutline == null)
            {
                var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer((int)SGRenderingPaths.NewYokaRenderer);
                var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
                foreach (var feature in features)
                {
                    if (feature.name == "OutLine")
                    {
                        featureOutline = feature;
                        break;

                    }
                }
            }


            if (featureOutline != null)
            {
                featureOutline.SetActive(active);
            }


            //性能节省-首次
            //默认开启-代码中，不加关闭开启
            //正确的值阈 



        }
    }

    //这里要分c# 和lua c#先创建 lua的后创建
    private void InitBusiness()
    {

        /*所有Cs和Lua都提前启动好了
        如果动态启动Cs和LuaModule呢，那要吧这个事件交给Cs创建时候交给Cs来处理*/

        ModuleManager.Instance.CreateModule(ModuleDef.Name.LoginModule);
        //ModuleManager.Instance.CreateModule(ModuleDef.Name.LuaModuleTest, "LuaModuleTest--");
        //ModuleManager.Instance.CreateModule(ModuleDef.Name.SevenDaysSignModule, "SevenDaysSignModule--");
        //ModuleManager.Instance.CreateModule(ModuleDef.Name.HomeModule);
        //ModuleManager.Instance.CreateModule(ModuleDef.Name.PVEModule);
        //ModuleManager.Instance.CreateModule(ModuleDef.Name.StarWorldModule);         //世界战斗

        bool isEditorMode = EditorModeTest.EditorMode.IsEditorMode;

        if (!isEditorMode)
        {
            ModuleManager.Instance.CreateModule(ModuleDef.Name.TutorialModule);           //引导模块
        }
        ModuleManager.Instance.CreateModule(ModuleDef.Name.TriggerModule);           //触发器模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.InterActionModule);       //交互模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.AmuletModule);       //符纹模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.InscriptionModule);       //鸣器模块

        //ModuleManager.Instance.CreateModule(ModuleDef.Name.HostModule);

        ModuleManager.Instance.CreateModule(ModuleDef.Name.AvgLuaModule);       //AVG模块

        //道具模块Lua
        //BusinessModule imm= ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemControllerModule);//道具模块
        //GameManager.Instance.SetItemModule(imm);

        //掉落管理模块Lua
        ModuleManager.Instance.CreateModule(ModuleDef.Name.DropControllerModule);//掉落管理模块

        //ModuleManager.Instance.CreateModule(ModuleDef.Name.AnnounceModule);             //服务器通知模块 
        ModuleManager.Instance.CreateModule(ModuleDef.Name.RewardsPopModule);  //奖励弹窗UI模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemResolveModule);//道具分解模块


        ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemControllerModule);       //道具模块C#
        ModuleManager.Instance.CreateModule(ModuleDef.Name.BagModule);       //背包模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemTipsModule);  //道具Tips模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.FuncTipsModule);  //功能Tips模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemUseModule);              //道具使用弹框
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ItemBuyModule);              //道具购买弹框

        ModuleManager.Instance.CreateModule(ModuleDef.Name.DropInfoListModule);  //掉落UI模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.QuickEquipModule);  //快速穿戴模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.ObjectInteractiveModule);  //物体交互模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.WantedModule);         //通缉玩法
        ModuleManager.Instance.CreateModule(ModuleDef.Name.TaskModule);              //任务模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.SecretAreaModule);              //个人秘境
        ModuleManager.Instance.CreateModule(ModuleDef.Name.PersonalTowerModule);              //个人爬塔
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EquipModule);              //装备模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.BattleTeamModule);        //伙伴出战

        ModuleManager.Instance.CreateModule(ModuleDef.Name.EctypeModule);            //副本模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.MedicineModule);          //药品模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.EquipSlotControllerModule);          //装备槽位强化模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EquipUpgradeModule);          //装备强化模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.SkillWindowModule);          //技能模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.PlayerLocalCache);  //玩家本地缓存数据

        ModuleManager.Instance.CreateModule(ModuleDef.Name.ChatModule); // 聊天木块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.FriendModule); // 好友木块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.AdventureLevelModule); // 冒险等级模块

        #region 可以放在page时
        //： 开启模块，加载界面，awake，UI注册自己的Module，然后UI通过自己的Module和别人进行通讯
        // 1，Module先建立；2，挂件加载（先拼接上去）OnShow，3，pageModule-WorldMapModule的Open；4，WorldMap.OnShow处理
        ModuleManager.Instance.CreateModule(ModuleDef.Name.WorldMapModule);          //世界地图 
        #endregion

        #region 需要才开启的
        #endregion

        ModuleManager.Instance.CreateModule(ModuleDef.Name.ShopModule);              //商店模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.RunHorseModule);              //跑马灯模块

        ModuleManager.Instance.CreateModule(ModuleDef.Name.GamePlayChoiceModule);        //玩法选择
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EctypeEntranceModule);        //玩法入口
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EctypeBagModule);        //临时背包
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EctypePopModule);        //弹窗
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EctypeSettleModule);

        ModuleManager.Instance.CreateModule(ModuleDef.Name.RayCheckModule);        //遮挡半透

        ModuleManager.Instance.CreateModule(ModuleDef.Name.RingTaskModule);        //环任务

        ModuleManager.Instance.CreateModule(ModuleDef.Name.MailModule);              //邮件

        ModuleManager.Instance.CreateModule(ModuleDef.Name.RechargeModule);        //交易
        ModuleManager.Instance.CreateModule(ModuleDef.Name.GuildModule);        //工会
        ModuleManager.Instance.CreateModule(ModuleDef.Name.TeamModule);      //组队任务
        ModuleManager.Instance.CreateModule(ModuleDef.Name.DailyTeamModule);        //组队日常本

        ModuleManager.Instance.CreateModule(ModuleDef.Name.TreasureModule);         //打开藏宝图

        ModuleManager.Instance.CreateModule(ModuleDef.Name.DrawCardModule);         //抽卡系统
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EventModule);         //活动系统
        ModuleManager.Instance.CreateModule(ModuleDef.Name.EventBossRankModule);         //活动boss排行
        ModuleManager.Instance.CreateModule(ModuleDef.Name.AfternoonGveModule);         //午间GVE

        ModuleManager.Instance.CreateModule(ModuleDef.Name.ExchangeModule);             //交易行
        ModuleManager.Instance.CreateModule(ModuleDef.Name.TalentModule);         //天赋
        ModuleManager.Instance.CreateModule(ModuleDef.Name.SelectorModule);         //选择
        ModuleManager.Instance.CreateModule(ModuleDef.Name.BankModule);         //银行

        ModuleManager.Instance.CreateModule(ModuleDef.Name.SystemOpenTipsModule);         //创建 系统openTips

        ModuleManager.Instance.CreateModule(ModuleDef.Name.PvpModule);
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ArenaModule);    //异步竞技场
        ModuleManager.Instance.CreateModule(ModuleDef.Name.WildBossModule);    //野外boss

        ModuleManager.Instance.CreateModule(ModuleDef.Name.AnnouncementModule);

        ModuleManager.Instance.CreateModule(ModuleDef.Name.CommercializationModule); //商业化

        ModuleManager.Instance.CreateModule(ModuleDef.Name.DonateModule);       //公会捐献

        ModuleManager.Instance.CreateModule(ModuleDef.Name.LivingSkillsModule);     //生活技能
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ActivityModule); //活动

        ModuleManager.Instance.CreateModule(ModuleDef.Name.DailyActModule); //每日活跃度

        ModuleManager.Instance.CreateModule(ModuleDef.Name.WorldLineModule);//世界线

        ModuleManager.Instance.CreateModule(ModuleDef.Name.GamePlayCalendarModule);    //玩法日历
        ModuleManager.Instance.CreateModule(ModuleDef.Name.FightPowerModule);    // 战力模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.OnlineRewardModule);    //在线奖励

        ModuleManager.Instance.CreateModule(ModuleDef.Name.PartyTimeModule);    //公会篝火

        ModuleManager.Instance.CreateModule(ModuleDef.Name.PlayerPreviewModule);    //玩家信息预览



        ModuleManager.Instance.CreateModule(ModuleDef.Name.RankModule); //排行榜
        ModuleManager.Instance.CreateModule(ModuleDef.Name.ScenePlayModule); //场景玩法模块
        ModuleManager.Instance.CreateModule(ModuleDef.Name.BeginnerTargetModule); //新手目标
        ModuleManager.Instance.CreateModule(ModuleDef.Name.SkillUnlockTipsModule);    //技能解锁表现
        ModuleManager.Instance.CreateModule(ModuleDef.Name.MindRepairModule);    //心灵修复小游戏
                                                                                 //      ModuleManager.Instance.CreateModule(ModuleDef.Name.DownLoadModule);//dlc下载模块
    }
    public void TravelToScene(E_SceneType e_SceneType, bool isFirst = false)
    {
        if (isFirst)
        {
            //TODO：NoLoading
            ModuleManager.Instance.ShowModule(ModuleDef.Name.LoginModule);

            //TODO：横向加载UI
            //TODO：纵向进入场景
            //


            //1,代码开放组件开启
            //2,True显示调试面板
            //3,False隐藏调试面板
            //GameInput.Create(false);//直接打开控制

        }
        else
        {
            //资源释放..
            //回调
            //ModuleManager.Instance.ShowModule(ModuleDef.Name.LoginModule);
        }

        //TODO：SceneManager===1PerLoad，2Loading（加载），3Loaded



        CurrentSceneType = e_SceneType;
        SGF.Debuger.Log("已经进入：【" + CurrentSceneType + "】流程");

    }
    private void OnDestroy()
    {
        ///切场景移除监听
        //AvProManager.Instance.ActionOnFinishPlaying -= OnFinishedPlaying;
    }

    //游戏关闭
    void OnApplicationQuit()
    {

        GameExit();

        SGF.Debuger.LogError("已经进入：游戏退出流程");
        NetworkManager.Instance.Close();
        FixMessageManager.Instance.Close();
        LocalCache.Dispose();
    }

    void GameExit()
    {
        StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
        if (starWorld != null)
            starWorld.GetGame().GameExit();
    }
    /*
        public string IsRunningOnEmulator6()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build");
                string radioVersion = buildClass.CallStatic<string>("getRadioVersion");
                return radioVersion;
            }
            return "";
        }*/
}





//GameLoading ，通用载入，场景切换
public enum E_SceneType
{
    PerInit_ResCheck = 1000,            //资源检测
    PerInit_PerLoad,                    //解压，预加载

    GameEnter_Login = 2000,             //登录
    GameEnter_RoleCreate,               //创角

    GameScene_Town = 3000,                    //游戏主城堡，安全区
    GameScene_Explorer               //野外

}