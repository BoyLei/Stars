//#if UNITY_EDITOR：未必??器可以看，其他也可以看
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using GameDLL;
using UnityEditor;
using UnityEngine.Rendering;
using StarProjectDef;
using UnityEngine.SceneManagement;
using StarProject.Service.Resource;
using UnityEngine.UI;
using System;
using TMPro;
using StarProject.Service.Cam;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.ResourceProviders;
using CameraEvent = UnityEngine.Rendering.CameraEvent;

using System.Reflection;
using SGF;

using UnityEngine.Rendering.Universal;

using SGF;

public class TestMipMap : MonoBehaviour
{
    public VisualizationMode visualizationMode = new VisualizationMode();
    public Vector2 _scroll = Vector2.zero;
    public InputField x;
    public InputField y;
    public Button setting_;//高低配
    public Button auto_;//自动高低配
    public Button cameraPara;//设置scribuffer

    public Button autoCameraPara;//自动buffer

    public Text screenText;
    FrameTiming[] frameTimings = new FrameTiming[10];
    private float maxResolutionWidthScale = 1.0f;
    private float maxResolutionHeightScale = 1.0f;
    private float minResolutionWidthScale = 0.1f;
    private float minResolutionHeightScale = 0.1f;
    private float scaleWidthIncrement = 0.05f;//手动
    private float scaleHeightIncrement = 0.05f;

    private float scaleWidthIncrementAuto = 0.01f;
    private float scaleHeightIncrementAuto = 0.01f;

    float m_widthScale = 1.0f;
    float m_heightScale = 1.0f;

    // Variables for dynamic resolution algorithm that persist across frames
    uint m_frameCount = 0;

    const uint kNumFrameTimings = 10;//设置成CD模式 10的时候处理一次； 10到100s处于CD期间；或者用冷却模式bool控制因为在处理分辨率的时候很容易出现这时候时间还是很卡
    const uint kNumFrameTimingsMAX = 100;


    double m_gpuFrameTime;
    double m_cpuFrameTime;


    double m_gpuFrameTimeAVG;
    double m_cpuFrameTimeAVG;
    double totalGpuFrameTime;
    double totalCpuFrameTime;

    float GPU_SHOULD_TIME = 1 / 50f;
    float CPU_SHOULD_TIME = 1 / 50f;

    float GPU_SHOULD_TIME_MAX = 1 / 55f;
    float CPU_SHOULD_TIME_MAX = 1 / 55f;

    //aes 不用enable，虽然enable好，但是内部逻辑很好不论一次还是多次，都可以，所以不用关心1次多次
    //每一次都会杀掉注册，一次也是保留，多次也是，所以随意换
    //不必换想换就换吧

    public Camera StarWorldCam;
    public Camera _backBufferCamera;

    float OpaqueDownScale = 0.7f;
    float TransparentDownScale = 0.7f;
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        setting_.onClick.RemoveAllListeners();
        auto_.onClick.RemoveAllListeners();
        cameraPara.onClick.RemoveAllListeners();
        setting_.onClick.AddListener(settingfoo);//直接高低配
        auto_.onClick.AddListener(auto);//自动高低配
        cameraPara.onClick.AddListener(cameraParafoo);//自动场景高低配
        autoCameraPara.onClick.RemoveAllListeners();
        autoCameraPara.onClick.AddListener(autoScalableBuffer);

        int rezWidth = (int)Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
        int rezHeight = (int)Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
        screenText.text = string.Format("Scale: {0:F3}x{1:F3}\nResolution: {2}x{3}\n",
            m_widthScale,
            m_heightScale,
            rezWidth,
            rezHeight);

        //InitializeDynamic();
    }
    
    private void InitializeDynamic()
    {
        //把所有ui和相机全 给新的策略
        //把ui给相机  rt   给新的----------11111111111111111111

        /*StarWorldCam = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).Camera;*/

        // 获取当前相机的UniversalAdditionalCameraData组件
        //UniversalAdditionalCameraData additionalCameraData = StarWorldCam.GetUniversalAdditionalCameraData();

        // 创建要添加到相机堆叠中的新相机
        /*Camera newCamera = new GameObject("NewCamera").AddComponent<Camera>();*/

        // 将新相机添加到相机堆叠中
        //additionalCameraData.cameraStack.Add(UICamera);

        var backBufferCameraGo = new GameObject("Back Buffer Camera");
        _backBufferCamera = backBufferCameraGo.AddComponent<Camera>();
        _backBufferCamera.cullingMask = 0;
        _backBufferCamera.transform.parent = transform;
        _backBufferCamera.clearFlags = CameraClearFlags.Nothing;
        _backBufferCamera.useOcclusionCulling = false;
        _backBufferCamera.allowHDR = false;
        _backBufferCamera.allowMSAA = false;
        _backBufferCamera.allowDynamicResolution = false;
        //_backBufferCamera.cameraStack.Add(UICamera);
        _frameBuffer = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Blit to back buffer";
    }

    [SerializeField, Range(0.1f, 1)]
    private float _scale = 1f;
    private RenderTexture _frameBuffer;
    private CommandBuffer _commandBuffer;
    /// <summary>
    /// 今のScale値を元にセットアップする
    /// </summary>
    private void Setup()
    {

        if (_frameBuffer != null)
        {
            _frameBuffer.Release();
            Destroy(_frameBuffer);
            _frameBuffer = null;
        }
        var width = (int)(Screen.width * _scale);
        var height = (int)(Screen.height * _scale);

        _frameBuffer = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        _frameBuffer.useMipMap = false;
        _frameBuffer.filterMode = FilterMode.Bilinear;
        _frameBuffer.Create();
        StarWorldCam.targetTexture = _frameBuffer;

        // バックバッファ描画用のカメラにCommandBufferを設定する
        if (_commandBuffer != null)
        {
            _backBufferCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
            _commandBuffer = null;
        }
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Blit to back buffer";
        _commandBuffer.Blit((RenderTargetIdentifier)_frameBuffer, BuiltinRenderTextureType.CameraTarget);
        _backBufferCamera.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);

    }

    private void settingfoo()
    {
        float texmpX = 0;
        if (float.TryParse(x.text, out texmpX))
        {
            QualitySettings.resolutionScalingFixedDPIFactor = texmpX;
        }
    }

    private Vector3 martixPos = Vector3.zero;
    private Vector3 martixScale = Vector3.one;
    Matrix4x4 matrix;
    private void cameraParafoo() 
    {
        float texmpy = 0;
        if (float.TryParse(y.text, out texmpy))
        {

            /* Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);//不但老旧，而且ui也一起弄了，下文方法1不合理*/

            // 动态分辨率方法一
            // Camera cam = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).GetComponent<Camera>();
            // cam.allowDynamicResolution = GameConfig.AllowDynamicResolution;
            // Rect rect = cam.rect;
            // rect = new Rect(rect.x, rect.y, rect.width * texmpy, rect.height * texmpy);
            // CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).GetComponent<Camera>().rect = rect;
            // martixPos.x = (1.0f - texmpy) / 2.0f;
            // martixPos.y = (1.0f - texmpy) / 2.0f;

            // martixScale.x /= texmpy;
            // martixScale.y /= texmpy;
            // //平铺矩阵，1需解决位置和缩放和景深问题，2需要解决RT问题，3需解决3Dcam应该作为栈给UiCam
            ///* matrix = Matrix4x4.TRS(cam.projectionMatrix.GetPosition(), Quaternion.identity, martixScale);
            // cam.projectionMatrix = matrix;*/

            //【应用级：动态分辨率【机器优劣判断】 X 场景分辨率【美术公摊责任+程序动态调谐+程序工具解决】】！！！！

            //RenderTextureCreationFlags.DynamicallyScalable =》 ScalabeBufferManager
            //相机
            //ScalableBufferManager:横纵全局
            //RT

            // 动态分辨率方法二：更 
            ScalableBufferManager.ResizeBuffers(texmpy, texmpy);

            Debuger.Log("CPU数据：" + SystemInfo.processorCount + "_" + SystemInfo.processorFrequency);



            //PPI:全局
            //ScalableBufferManager.ResizeBuffers：文字   方法1：屏幕矩阵不行；方法2：多相机只有builtint，3相机stack不行，【【4新相机融合可以】，5rt可以】


            //更新RenderTexture:Rt和RtCamera都不要模糊
            /*Graphics.Blit（_tex，_renderTexture）;*/
            //【相机 * RT 设置可以联动 X 帧数检测联动 = 动态帧率控制】！！
        }

    }
    private bool changeFactor = false;
    private bool ScalableBufferManagerOpen = false;
    private void auto()
    {
        //StartCoroutine("autoFrame");
        changeFactor = true;
        QualitySettings.resolutionScalingFixedDPIFactor = 0.326f;
    }
    private void autoScalableBuffer()
    {
        //DynamicResolutionHandler.UpdateAndUseCamera(CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).Camera);


        //上面urp
        //下面自己写的

        ScalableBufferManagerOpen = true;
        m_widthScale = 1.0f;
        m_heightScale = 1.0f;
    }

/*    private IEnumerator autoFrame()
    {
        //这个拿不到真正的帧率：是可以的不过有的帧数时间太长了
       *//* while (ShowFps.Instance.fps <= 55)
        {
            yield return new WaitForSeconds(0.5f);
            QualitySettings.resolutionScalingFixedDPIFactor *= 0.95f;
            x.text = QualitySettings.resolutionScalingFixedDPIFactor.ToString();
        }*//*
    }*/


    string debugString;
    void OnGUI()
    {

        if (GUILayout.Button("关闭降分辨率", GUILayout.Height(40)))
        {
            SetDownScaleRendererRF(1f, 1f); // 恢复默认分辨率
            SetDownScaleRendererRF(false);
            //关闭降分辨率RF
        }
        if (GUILayout.Button("开启降分辨率", GUILayout.Height(40)))
        {
            SetDownScaleRendererRF(true);
            //GameObject.Destroy(gameObject);
        }
        GUILayout.Label("地表降分辨率");
        OpaqueDownScale = GUILayout.HorizontalSlider(OpaqueDownScale, 0, 1,GUILayout.Height(50));
        GUILayout.Label("水降分辨率");
        TransparentDownScale = GUILayout.HorizontalSlider(TransparentDownScale, 0, 1,GUILayout.Height(50));
        SetDownScaleRendererRF(OpaqueDownScale, TransparentDownScale);
        

        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.Space(50);
        
        
        if(GUILayout.Button("mipmap",GUILayout.Height(40)))
        {
            visualizationMode.ReplaceMipmapRenderers();
        }

        if(GUILayout.Button("cancle",GUILayout.Height(40)))
        {
            visualizationMode.RevertAllRenderers();
        }

        if(GUILayout.Button( "remove",GUILayout.Height(40)))
        {
            GameObject.Destroy(gameObject);
        }

        var color = GUI.color;
        GUI.color=Color.white;
        GUILayout.TextArea(debugString,GUILayout.Height(200));
        GUI.color = color;

        if (GUILayout.Button("清理中级", GUILayout.Height(40)))
        {
            ResourceFormalManager.Instance.ForceClearMiddleResourceCache();

        }

        if (GUILayout.Button("清理低级", GUILayout.Height(40)))
        {
            ResourceFormalManager.Instance.ForceClearLowerResourceCache();
        }
        if (GUILayout.Button("GC", GUILayout.Height(40)))
        {
            ResourceFormalManager.Instance.GC1();
        }
        if (GUILayout.Button("GC1", GUILayout.Height(40)))
        {
            ResourceFormalManager.Instance.GC2();
        }
        if (GUILayout.Button("RELEASE_RES_MGR", GUILayout.Height(40)))
        {
            ResourceFormalManager.Instance.OnModuleReleaseClearResourceCache();
        }
        if (GUILayout.Button( "streamingTextureDiscardUnusedMips",GUILayout.Height(40)))
        {
            Texture.streamingTextureDiscardUnusedMips = true;
        }
        if (GUILayout.Button( "增加budget32MB",GUILayout.Height(40)))
        {
            QualitySettings.streamingMipmapsMemoryBudget += 32;
        }
        if (GUILayout.Button( "减少budget32MB",GUILayout.Height(40)))
        {
            QualitySettings.streamingMipmapsMemoryBudget -= 32;
        }
      


        if (GUILayout.Button("高配",GUILayout.Height(40)))
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            QualitySettings.maximumLODLevel = 0;//Lod草地分三级，物件分成2级不可想用降级策略
            QualitySettings.masterTextureLimit = 0;//降低分辨率但是不修改ui
            /*QualitySettings.maximumLODLevel = 0;*///没问题：Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用物件0~1都是1级物件渲染策略：共享等级
            QualitySettings.lodBias = 1f;//1是向上取整,他不是0~1他之所以不能0是因为0次幂没法拉伸lod横线
            GameConfig.QualityForCameraClipNeighbor = 2;//慢速裁切都表现
            QualitySettings.resolutionScalingFixedDPIFactor = 0.326f;//0.46f;//接近无限高：30 16 479
            //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);
        }


        if (GUILayout.Button( "中配",GUILayout.Height(40)))
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            QualitySettings.maximumLODLevel = 1;//Lod草地分三级，物件分成2级不可想用降级策略
            QualitySettings.masterTextureLimit = 1;//通过设置设置最大精度
            GameConfig.QualityForCameraClipNeighbor = 1;//快速裁切
            /*QualitySettings.maximumLODLevel = 0;*///Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用物件0~1都是1级物件渲染策略：共享等级
            QualitySettings.lodBias = 0.6f;//可不是只显示1级的0.6是概念距离的缩减；就算是按照0.6倍数拉动距离条，但是裁切的lod是固定距离的所以表现会很不同
            QualitySettings.resolutionScalingFixedDPIFactor = 0.3f;//接近比例尺：人眼睛极限
            //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 60);
        }
        if (GUILayout.Button( "低配",GUILayout.Height(40)))
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            /*QualitySettings.maximumLODLevel = 0;*///Lod草地分三级，物件分成2级不可想用降级策略
            QualitySettings.masterTextureLimit = 2;//通过设置设置最大精度
            GameConfig.QualityForCameraClipNeighbor = 2;//快速裁切
            QualitySettings.maximumLODLevel = 2;//Lod草地分三级，物件分成2级不可想用降级策略；为了拓展需采用
            QualitySettings.lodBias = 0.5f;//0是向下取整  
                                           //物件0~1都是1级物件渲染策略：共享等级
            QualitySettings.resolutionScalingFixedDPIFactor = 0.25f;//接近比例尺：
            //Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, 30);
        }
        if (GUILayout.Button("Open Android SDK Example Scene",GUILayout.Height(40)))
        {
            OpenAndroidTestScene(true);
        }
        if (GUILayout.Button("Close Android SDK Example Scene",GUILayout.Height(40)))
        {
            OpenAndroidTestScene(false);
        }
         


        // One finger lowers the resolution
        if (GUILayout.Button("DownSceneViewRect", GUILayout.Height(40)))
        {
            m_heightScale = Mathf.Max(minResolutionHeightScale, m_heightScale - scaleHeightIncrement);
            m_widthScale = Mathf.Max(minResolutionWidthScale, m_widthScale - scaleWidthIncrement);
        }

        // Two fingers raises the resolution
        if (GUILayout.Button("UpSceneViewRect", GUILayout.Height(40)))
        {
            //特别大就1，小于1就是用的
            m_heightScale = Mathf.Min(maxResolutionHeightScale, m_heightScale + scaleHeightIncrement);
            m_widthScale = Mathf.Min(maxResolutionWidthScale, m_widthScale + scaleWidthIncrement);
        }
        GUILayout.EndScrollView();
    }

    private SceneInstance _sceneInstance;
    private void OpenAndroidTestScene(bool isOpen)
    {
        string scenename =
            "AndroidGoodiesExampleScene";
        if (isOpen)
        {
            var  loader=Addressables.LoadSceneAsync("map/"+scenename, LoadSceneMode.Additive);
            loader.Completed += ( _scene) =>
            {
                _sceneInstance = _scene.Result;
                var  scene= _sceneInstance.Scene;// SceneManager.GetSceneByName(scenename);
                SceneManager.SetActiveScene(scene);
            };

        }
        else
        {
            Addressables.UnloadSceneAsync(_sceneInstance);
        }
            
    }
    float oldWidthScale;
    float oldHeightScale;
    private void Update()
    {
        //
        //Setup();
        /*currentTextureMemory 纹理当前使用的内存量。
desiredTextureMemory 在应用纹理串流预算之前会先使用上述的纹理内存量。
nonStreamingTextureCount 非串流纹理的数量。
nonStreamingTextureMemory 非串流纹理正在使用的内存总量。
streamingMipmapUploadCount 由于纹理 Mipmap 串流而导致上传纹理的次数。
streamingRendererCount 在纹理串流系统中注册的渲染器数量。
streamingTextureCount 串流纹理的数量。
streamingTextureDiscardUnusedMips (这不是方法)强制串流纹理系统立即丢弃所有未使用的 Mipmap，而不是缓存这些 Mipmap 直到超出纹理内存预算。

streamingTextureForceLoadAll 强制串流纹理加载所有 Mipmap 级别。

streamingTextureLoadingCount 当前正在加载 Mipmap 的串流纹理的数量。
streamingTexturePendingLoadCount 要加载的未完成 Mipmap 的串流纹理数量。
targetTextureMemory 应用 Mipmap 串流和预算并且在加载完成后由纹理使用的内存量。
totalTextureMemory Mipmap 级别 0 的所有纹理将使用的内存总量。*/
        //Texture.streamingTextureForceLoadAll.   //You can use the UV distribution metric to calculate the mipmap level you need, based on the position of the Camera. See Mesh.GetUVDistributionMetric for example code.To override the system and force all mips to load, use


        //不同就设置，因为gui和update时机是不同的
        //https://docs.unity3d.com/ScriptReference/UnityEngine.CoreModule.html
        if (m_widthScale != oldWidthScale || m_heightScale != oldHeightScale)
        {  
            ScalableBufferManager.ResizeBuffers(m_widthScale, m_heightScale);
            oldWidthScale = m_widthScale; 
            oldHeightScale = m_heightScale;
        }
        DetermineResolution();
        int rezWidth = (int)Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
        int rezHeight = (int)Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
        screenText.text = string.Format("Scale: {0:F3}x{1:F3}\nResolution: {2}x{3}\nScaleFactor: {4:F3}x{5:F3}\nGPU: {6:F3} CPU: {7:F3} GPUAVG:{8:F3} CPUAVG:{9:F3}",
            m_widthScale,
            m_heightScale,
            rezWidth,
            rezHeight,
            ScalableBufferManager.widthScaleFactor,
            ScalableBufferManager.heightScaleFactor,
            m_gpuFrameTime,
            m_cpuFrameTime, m_gpuFrameTimeAVG, m_cpuFrameTimeAVG);




        debugString =
            "currentTextureMemory" + Texture.currentTextureMemory / 1024 / 1024 + "MB\n" +
            "desiredTextureMemory" + Texture.desiredTextureMemory / 1024 / 1024 + "\n" +
            "streamingTextureDiscardUnusedMips" + Texture.streamingTextureDiscardUnusedMips + "\n" +
            "targetTextureMemory" + Texture.targetTextureMemory / 1024 / 1024 + "MB\n" +
            "totalTextureMemory" + Texture.totalTextureMemory / 1024 / 1024 + "MB\n" +
            //"DPI:" + Screen.dpi +//假的没用
            "Screen.currentResolution.refreshRate" + Screen.currentResolution.refreshRate + "MB\n" +
#if !UNITY_ANDROID && !UNITY_IOS // 排除移动平台
            "Unity内存使用量"  +  System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 + "MB\n" +
#endif
            "Unity私有内存使用量：" + UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1024 / 1024 + "MB\n" +
            "当前场景内存使用量：" + UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024 + "MB\n" +
            "CPU数据：" + SystemInfo.processorCount + "_" + SystemInfo.processorFrequency;
       ;







    }
    bool FirstFastSBManagerOpenTag = true;
    float ScreenAutioParm =1;
    //在这里进行动态分辨率的制作
    //用不到Rt
    //ppi是根据1，用户习惯手机动态适配并不是我去【评判好坏】【高地机型不应该统一】【尊重硬件发展他们自己测试的硬件和系统之间的接口】
    //，2，机器型号gpu和屏幕像素比例尊重,通常低配机gpu不好但是像素点特别多刷新高这个机器本身就有问题，所以尊重类似4070适配2.5k240hz是梦幻联动，
    //3，英尺全套适配是尊重肉眼，所以高于分辨率意义，更英尺对应肉眼【如果屏幕切换折叠屏我可以重新刷一下326不过操作系统会调用我】

    /*    如果有多个相机， 可以为每个Camera单独设置他们
    可以为每个RenderTexture 设置
    当缩放>1 ，性能和负载也只会是原分辨率的
    无法在UnityEditor上进行测试。*/
    private void DetermineResolution()
    {
        //每间隔两次才采样
        ++m_frameCount;
        if (m_frameCount >= kNumFrameTimingsMAX)
        {
            //休息90帧数
            m_frameCount = 0;
            return;
        }
        else if(m_frameCount == kNumFrameTimings)//小于100
        {
            //到10的时候统计0~10帧数的平均
        }
        else
        {
            //10帧之内不统计 0~10的时候不统计
            return;
        }
        FrameTimingManager.CaptureFrameTimings();
        FrameTimingManager.GetLatestTimings(kNumFrameTimings, frameTimings);
        //if (frameTimings.Length < kNumFrameTimings)
        //{
        //    Debug.LogFormat("Skipping frame {0}, didn't get enough frame timings.",
        //        m_frameCount);

        //    return;
        //}

        m_gpuFrameTime = (double)frameTimings[0].gpuFrameTime;
        m_cpuFrameTime = (double)frameTimings[0].cpuFrameTime;
        totalGpuFrameTime = 0;
        totalCpuFrameTime = 0;
        for (int i = 0; i < frameTimings.Length; i++)
        {
            totalGpuFrameTime += frameTimings[i].gpuFrameTime;
            totalCpuFrameTime += frameTimings[i].cpuFrameTime;
        }
     /*   for (int i = frameTimings.Length - 1; i >= 0; i--)
        {
            frameTimings[i] = null;
        }*/
        m_gpuFrameTimeAVG = totalGpuFrameTime / frameTimings.Length;
        m_cpuFrameTimeAVG = totalCpuFrameTime / frameTimings.Length;
        //高地适配
        if (changeFactor)
        {  
            if (m_gpuFrameTimeAVG > GPU_SHOULD_TIME || m_cpuFrameTimeAVG > CPU_SHOULD_TIME)
            {
                QualitySettings.resolutionScalingFixedDPIFactor -= 0.01f;
                x.text = QualitySettings.resolutionScalingFixedDPIFactor.ToString();
            }
            else
            {
                changeFactor = false;
            }
        }

        //动态场景分辨率
        //平衡在50~60不管他
        if (ScalableBufferManagerOpen)
        {
            //提取缩放因子---假设不是顶点准备的问题，假设问题全来源于分辨率；随后的波动来源据调整CPU
            //但你排除不了顿卡的问题，和开始统计时机的问题如果是刚开始加载的问题
            //1这个可以找时机，2也可以排除顿卡

            if (FirstFastSBManagerOpenTag)
            {
                //1统计屏幕比例尺
                    ScreenAutioParm = Screen.width / Screen.height;
                //2大尺度缩放
                double p1 = ((m_cpuFrameTimeAVG + m_gpuFrameTimeAVG)/2f) / ((GPU_SHOULD_TIME + CPU_SHOULD_TIME)/2f);
                if (p1 > 1)//比我时间长
                {
                    //他时间是我要求的多少倍
                    m_heightScale = Mathf.Max(minResolutionHeightScale, m_heightScale / (float)p1);
                    m_widthScale = Mathf.Max(minResolutionWidthScale, m_widthScale / (float)p1);
                }
                //乘除法保持比例，加减法要手动添加比例
                //5/2 = 5/2 / 2/2
                //就如同欧美游戏和日韩游戏的区别一样


                FirstFastSBManagerOpenTag = false;
                return;
            }
          

            //手动自动都可以
            //消耗时间大于50帧数所需要时间
            if (m_gpuFrameTimeAVG > GPU_SHOULD_TIME || m_cpuFrameTimeAVG > CPU_SHOULD_TIME)
            {
                /*按照比例减少*/
                m_heightScale = Mathf.Max(minResolutionHeightScale, m_heightScale - scaleHeightIncrementAuto);
                m_widthScale = Mathf.Max(minResolutionWidthScale, m_widthScale - /*scaleWidthIncrementAuto*/ScreenAutioParm * scaleHeightIncrementAuto);
                //小于0.5 -0.5拦着，大于0.5 就0.5~1随便自由
            }
            else
            {
                //消耗时间都小于60fps所需要时间了
                //满足就关闭
                //ScalableBufferManagerOpen = false;

                if (m_gpuFrameTimeAVG < GPU_SHOULD_TIME_MAX || m_cpuFrameTimeAVG < CPU_SHOULD_TIME_MAX)
                {
                    m_heightScale = Mathf.Min(maxResolutionHeightScale, m_heightScale + scaleHeightIncrementAuto);
                    m_widthScale = Mathf.Min(maxResolutionWidthScale, m_widthScale + /*scaleWidthIncrementAuto*/ScreenAutioParm * scaleHeightIncrementAuto);
                }
            }
        }
    }
    
    //查找到当前柱主相机中名为"SGameDownScaleRenderer"的renderfeature.
    //如果没有开启则让其开启,并设置其中的参数DownScale(决定降低RT的值)
    private void SetDownScaleRendererRF(float opaqueDownScale,float transparentDownScale)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            // var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            //
            // var renderingFeatures = additionalCameraData.scriptableRenderer.supportedRenderingFeatures;
            //
            

            var renderer =
                (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(
                    (int)SGRenderingPaths.NewYokaRenderer);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures",BindingFlags.Instance | BindingFlags.NonPublic);
            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
            
            foreach (var feature in features)
            {
                if (feature.GetType() == typeof(SGameDownScaleRenderer))
                {
                    if(feature.isActive)
                    {
                        (feature as SGameDownScaleRenderer).DownScale = opaqueDownScale;
                        (feature as SGameDownScaleRenderer).TransparentDownScale = transparentDownScale;
                    }
                    
                }
            }

        }
    }

    private void SetDownScaleRendererRF(bool isActive)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            var renderer =
                (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(
                    (int)SGRenderingPaths.NewYokaRenderer);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures",BindingFlags.Instance | BindingFlags.NonPublic);
            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

            foreach (var feature in features)
            {
                if (feature.GetType() == typeof(SGameDownScaleRenderer))
                {
                    feature.SetActive(isActive);
                }
            }

        }
    }
}
//#endif