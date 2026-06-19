using Cinemachine;
using Cysharp.Threading.Tasks;
using ProtoMsg;
using SGF.Module.Framework;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProject.Service.Resource;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StarProject.Game
{
    /// <summary>
    /// 1，里世界会附属再表世界中，随着表世界加载加载，卸载卸载，甚至位置坐标都不能重合（设计上要如此）
    /// 2，里世界有一种快速切换，不会走进度条的隐藏加载机制
    /// 3，本质来说其实就是一个场景拆分两个，方便开发
    /// </summary>
    public class StarScenesManager : ServiceModule<StarScenesManager>
    {
        private const string SCENE_NAME_INNER_TAG = "_Inner";


        public string InnerSceneName = string.Empty;
        public string OuterSceneName = string.Empty;

        //public string curSceneName = string.Empty;


        private float lastValue = 0;
        private float loadingValue = 0;
        private float TempVaule = 0;
        private Camera mainCamera;
        public string OldSceneName = string.Empty;

        public Camera MainCamera
        {
            get
            {
                if (mainCamera == null)
                {
                    mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
                }

                return mainCamera;
            }
        }

        // 所有的BuildScene里场景名
        private List<string> m_AllBuildScenes = new();

        public long CurPartnerID = 0; // 当前上场的伙伴ID预加载资源用

        public bool CreateRoleLoadMap = false;

        private string preLoadMapName = string.Empty;
        private bool preLoadMapSuccess = false;

        public LoadSceneType loadSceneType = LoadSceneType.Default;

        private async UniTask LoadJumpScene()
        {
            var hanlder = Addressables.LoadSceneAsync("map/jumpscene", LoadSceneMode.Single, true);
            await hanlder.Task;
        }

        readonly static string[] HasSettingScenes =
        {
            "MainTown",
            "Map_2",
            "Map_Chenxgd",
            "Map_FB_MengJ",
            "Map_FB_MengJKJ",
            "Map_FB_XinSG",
            "Map_MengZJ",
            "Map_XinSC",
        };

        static bool IsSettingScenes(string scenename)
        {
            for(int i = 0;i < HasSettingScenes.Length;i++)
            {
                if (scenename.Equals(HasSettingScenes[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 加载带 Map_地图
        /// </summary>
        /// <param name="SceneName">场景名称</param>
        /// <param name="mapWeigth">权重占比</param>
        private async UniTask LoadMapScene(string SceneName, float mapWeigth, float process)
        {
            var mapname = "map/" + SceneName.ToLower();
            //下载之前先判断是否是dlc下载的场景
            //if(!DlcDownload.Instance.CheckSceneDownloaded(mapname))
            //{
            //    await DlcDownload.Instance.DownLoadScene(mapname);
            //}
            var handle = Addressables.LoadSceneAsync(mapname, LoadSceneMode.Single, true);
            handle.Completed += (AsyncOperationHandle<SceneInstance> obj) =>
            {
                if (obj.IsDone)
                {
                    process = mapWeigth;
                    //GameManager.Instance.Loading.OnProcess(process);
                    OldSceneName = SceneName;
                    //SGF.Debuger.LogWarning($"切图 进度 时间={SGF.Time.TimeUtils.ServerNow} ,00000000 percentage={handle.PercentComplete}");
                    // GameManager.Instance.Loading.OnProcessEnd();
                }
            };
            //SGF.Debuger.LogWarning($"切图 进度 时间={SGF.Time.TimeUtils.ServerNow} ,11111111111111percentage={handle.PercentComplete}");

            while (!handle.IsDone)
            {
                var percentage = handle.PercentComplete;
                process = percentage * mapWeigth;
                //SGF.Debuger.LogWarning($"切图 进度 时间={SGF.Time.TimeUtils.ServerNow} ,percentage={percentage},mapWeigth={mapWeigth},process={process}");
                //GameManager.Instance.Loading.OnProcess(process);
                await UniTask.NextFrame();
            }
        }
        /// <summary>
        /// 通过策划效果控制客户端单机切场景；新手关切换：暂时曲子没!!!!!清理Gc，也没跳转jumpScene!!!!,请走公用方法
        /// </summary>
        /// <param name="SceneName"></param>
        private void PreLoadMapScene(string SceneName)
        {
            var handle = Addressables.LoadSceneAsync("map/" + SceneName.ToLower(), LoadSceneMode.Single, true, 60);
            handle.Completed += (AsyncOperationHandle<SceneInstance> obj) =>
            {
                if (obj.IsDone)
                {
                    //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 预加载地图={SceneName} --------- 完成");
                    // 将主场景设置为活动场景（真正载入）（有时可能需要等待合适的时机，再主动调用）
                    preLoadMapSuccess = true;
                    handle.Result.ActivateAsync();
                    if (!string.IsNullOrEmpty(OldSceneName))
                    {
                        /*var lastScene = SceneManager.GetSceneByName(OldSceneName);*/
                        /*SceneManager.UnloadSceneAsync(lastScene);*/
                    }
                    OldSceneName = SceneName;
                    switch (loadSceneType)
                    {
                        case LoadSceneType.Default:
                        case LoadSceneType.PreLoadPV:
                            GameManager.Instance.Loading.OnProcessBlackEnd();
                            break;
                        case LoadSceneType.PreLoadFadeWhite:
                            {
                                SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 播放闪白  结束 ");
                                GameManager.Instance.Loading.OnProcessWhiteEnd();
                            }
                            break;
                        case LoadSceneType.PreLoadFadeBlack:
                            {
                                SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 播放闪黑  结束 ");
                                GameManager.Instance.Loading.OnProcessBlackEnd();
                            }
                            break;
                        default:
                            break;
                    }

                    if (OuterSceneName == OldSceneName)
                    {
                        loadSceneType = LoadSceneType.Default;
                        GlobalEvent.onSceneLoaded.Invoke(OuterSceneName, false);
                    }
                }
            };
        }

        /// <summary>
        ///         /// 异步加载场景
        /// 表世界通过外面加载，里世界附属帮你加载
        /// </summary>
        /// <param name="SceneName"></param>
        /// <param name="spaceType">现在不同的SceneType 都有loading</param>
        /// <returns></returns>
        public async UniTask LoadScene(string SceneName, SpaceType spaceType)
        {
            //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 aaaaaaaaaaaaaaaaaaaaaa ");

            OuterSceneName = SceneName;
            // 加载场景的时候 先设置 当前场景的名字
            //GameManager.Instance.Loading.OnProcessStart();
            bool isAlreadyPreLoad = preLoadMapName != string.Empty && preLoadMapName == SceneName && loadSceneType != LoadSceneType.Default;
            if (isAlreadyPreLoad)
            {
                SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 正常流程 地图={SceneName} ----------- 开始");
                SGF.Debuger.LogWarning($"初始登录 预加载切地图 锁住消息 ++++++++++++++++++++++++【锁住】");
                SGF.Network.NetworkManager.Instance.LockMessage(true);
            }
            else
            {
                if (CreateRoleLoadMap)
                {
                    SGF.Debuger.LogWarning($"初始登录 切地图 黑幕开始 -----------------------");
                    SGF.Network.NetworkManager.Instance.LockMessage(true);
                    if (spaceType == SpaceType.SpaceDefault)
                    {
                        GameManager.Instance.Loading.OnPlayBlackDOFade(true, 1f);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                    }
                    SGF.Debuger.LogWarning($"初始登录 切地图 黑幕 等待0.2s结束---------------");
                }
                else
                {
                    SGF.Debuger.LogWarning($"初始登录 切地图 -----------------------");
                    int mapID = 0;
                    if (GameManager.Instance.M_Map != null)
                    {
                        mapID = GameManager.Instance.M_Map.GetMapId();
                    }
                    GameManager.Instance.Loading.OnProcessStart(spaceType, mapID);
                    SGF.Debuger.LogWarning($"初始登录 切地图 锁住消息 ++++++++++++++++++++++++【锁住】");
                    SGF.Network.NetworkManager.Instance.LockMessage(true);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                }
            }

            lastValue = 0;
            loadingValue = 0;
            float process = 0;
            float mapWeigth = 1.0f;
            bool isPreLoading = false;
            if (PreLoading.Instance.CanPreLoad())
            {
                isPreLoading = true;
                mapWeigth = 0.7f;
            }

            //TO Map_场景
            if (SceneName.Contains("Map_", StringComparison.Ordinal))
            {
                //LRU和引用计数也是基于AA层的，其他确实都要清理，不然就丢在DoNotDestroy
                //if (!isAlreadyPreLoad)
                //{
                //    await LoadJumpScene();
                //}

                GlobalEvent.OnDiffMap_SceneChange?.Invoke(OldSceneName, SceneName); ///这里不会错误记录jumpScene，就是上一个map和下一个map
                //去Map一定要清理
                //OnChangeToMapSceneDeal();

                if (!isAlreadyPreLoad)
                {
                    await LoadMapScene(SceneName, mapWeigth, process);
                    process = GameManager.Instance.Loading.GetProcess();
                }
                //process = loader.GetAwaiter().GetResult();
            }
            else
            {
                if (!isAlreadyPreLoad)
                {
                    await LoadMapScene(SceneName, mapWeigth, process);
                    process = GameManager.Instance.Loading.GetProcess();
                }
            }

            //这里是没有进度条的========================================
            //await LoadingInnerScene(SceneName); //收集加载所有里世界场景？？？

            //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 正常流程1111111111 相机=Properties/VirtualCamera/{SceneName}_BlenderSettings ----------- ");
            CinemachineBrain cinemachine = MainCamera.GetComponent<Cinemachine.CinemachineBrain>();
            if (cinemachine != null && IsSettingScenes(SceneName))
            {
                //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 正常流程22222222222 相机=Properties/VirtualCamera/{SceneName}_BlenderSettings ----------- ");
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<CinemachineBlenderSettings>($"Properties/VirtualCamera/{SceneName}_BlenderSettings",
                (CinemachineBlenderSettings cbs) =>
                {
                    if (cbs == null)
                    {
                        return;
                    }
                    cinemachine.m_CustomBlends = cbs;
                });
            }

            //取得设置,变成配置，不过我目前不知道有什么需求，先写代码吧
            OnMainTownOrOtherSceneSetting(SceneName);

            if (isPreLoading)
            {
                float old = process;
                PreLoading.Instance.Loading((path, p) =>
                    {
                        process = old + (p * (1 - mapWeigth));
                        //GameManager.Instance.Loading.OnProcess(process); //Single
                    },
                    async () =>
                    {
                        SGF.Debuger.LogWarning($"初始登录 切地图 锁住消息 首次加载主角资源 -----------------------【解开】");
                        await PreLoadingSuccess(isAlreadyPreLoad);
                    });
            }
            else
            {
                if (!isAlreadyPreLoad)
                {
                    // 主角模型创建完毕，loadingView 才关闭（移位置）
                    SGF.Network.NetworkManager.Instance.LockMessage(false);
                    SGF.Debuger.LogWarning($"初始登录 切地图 锁住消息 -----------------------【解开】");
                    SceneLoadSuccess(isAlreadyPreLoad);
                    await UniTask.Delay(TimeSpan.FromSeconds(GameConfig.CHANAGE_MAP_DELAY_BLACKPANEL));
                    if (CreateRoleLoadMap)
                    {
                        GameManager.Instance.Loading.OnProcessBlackEnd();
                        CreateRoleLoadMap = false;
                    }
                    else
                    {
                        GameManager.Instance.Loading.OnProcessEnd(true);
                    }
                }
                else
                {
                    if (preLoadMapSuccess)
                    {
                        OldSceneName = SceneName;
                        switch (loadSceneType)
                        {
                            case LoadSceneType.Default:
                                break;
                            case LoadSceneType.PreLoadPV:
                                GameManager.Instance.Loading.OnProcessBlackEnd();
                                break;
                            case LoadSceneType.PreLoadFadeWhite:
                                GameManager.Instance.Loading.OnProcessWhiteEnd();
                                break;
                            case LoadSceneType.PreLoadFadeBlack:
                                GameManager.Instance.Loading.OnProcessBlackEnd();
                                break;
                            default:
                                break;
                        }
                        SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 正常流程 地图={SceneName} ----------- 结束");
                        SceneLoadSuccess(isAlreadyPreLoad);
                    }
                    await UniTask.Delay(TimeSpan.FromSeconds(GameConfig.CHANAGE_MAP_DELAY_BLACKPANEL));
                    SGF.Network.NetworkManager.Instance.LockMessage(false);
                    SGF.Debuger.LogWarning($"初始登录 预加载切地图 锁住消息 -----------------------【解开】");
                }
            }
        }

        private async UniTask PreLoadingSuccess(bool isAlreadyPreLoad)
        {
            SGF.Network.NetworkManager.Instance.LockMessage(false);
            SceneLoadSuccess(isAlreadyPreLoad);
            await UniTask.Delay(TimeSpan.FromSeconds(GameConfig.CHANAGE_MAP_DELAY_BLACKPANEL));
            if (CreateRoleLoadMap)
            {
                GameManager.Instance.Loading.OnProcessBlackEnd();
                CreateRoleLoadMap = false;
            }
            else
            {
                GameManager.Instance.Loading.OnProcessEnd(true);
            }
            //SGF.Debuger.LogWarning($"初始登录 切地图 锁住消息 首次加载主角资源 完成");
        }

        private void SceneLoadSuccess(bool isAlreadyPreLoad)
        {
            preLoadMapName = string.Empty;
            if (isAlreadyPreLoad && preLoadMapSuccess)
            {
                loadSceneType = LoadSceneType.Default;
                GlobalEvent.onSceneLoaded.Invoke(OuterSceneName, false);
            }
            //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 bbbbbbbbbbbbbbbbbbbbbbb ");
        }

        private readonly Color MainTownBackgroundColor = new(209f / 255f, 209f / 255f, 209f / 255f, 0f);
        private readonly Color OtherBackgroundColor = new(1f, 1f, 1f, 1f);
        private void OnMainTownOrOtherSceneSetting(string SceneName)
        {
            CameraBase cb = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam);
            if (SceneName == "MainTown")
            {
                Camera.main.backgroundColor = MainTownBackgroundColor; //给背景赋值
                cb.Camera.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                Camera.main.backgroundColor = OtherBackgroundColor; //给背景赋值
                cb.Camera.clearFlags = CameraClearFlags.Skybox;
            }
        }

        private static void OnLoginChangeToMapDeal()
        {
            //SGF.Debuger.Log("111111111111111====>>>>");
            ResourceFormalManager.Instance.ForceClearMiddleResourceCache("Login");

            //ResourceFormalManager.Instance.ReleaseGameobjectPool(); // 清理其他角色也在内存里面---
        }

        private static void OnChangeToMapSceneDeal()
        {
            ResourceFormalManager.Instance.GCResourceCache(ResourcePriority.Low);
            //ResourceFormalManager.Instance.GC4();
        }

        public void SetPreLoadScene(string mapName, LoadSceneType _loadSceneType)
        {
            loadSceneType = _loadSceneType;
            preLoadMapName = mapName;
            preLoadMapSuccess = false;
            switch (_loadSceneType)
            {
                case LoadSceneType.Default:
                case LoadSceneType.PreLoadPV:
                    GameManager.Instance.Loading.OnPlayBlackDOFade(true,0f);
                    break;
                case LoadSceneType.PreLoadFadeWhite:
                    GameManager.Instance.Loading.OnPlayWhiteDOFade(true, 1f);
                    break;
                case LoadSceneType.PreLoadFadeBlack:
                    GameManager.Instance.Loading.OnPlayBlackDOFade(true, 1f);
                    break;
                default:
                    break;
            }

            PreLoadMapScene(preLoadMapName);
        }

        //UIActiveSceneLoader
        //系统功能开启Scene融合：伙伴，【抽卡】
        //灯光，后效,
        //加载全放这里，对应具体和表现，逻辑自己处理
        public void LoadUISystemActiveScene(string sceneName, System.Action<SceneInstance> cb)
        {
            //MonoHelper.StartCoroutine(LoadScene(sceneName, cb));

            Addressables.LoadSceneAsync("map/" + sceneName.ToLower(), LoadSceneMode.Additive, true).Completed +=
                (AsyncOperationHandle<SceneInstance> obj) =>
                {
                    if (obj.IsDone)
                    {
                        cb.Invoke(obj.Result);
                    }
                };
        }

        /// <summary>
        /// Map_2表世界，Map_2_Inner是里世界：穿插没有进度条
        ///  Map_2表世界，Map2_Boss（或随意)：有进度条
        ///  Map2_Boss不参与表里世界（注意没有_2），编辑器MapName是找perfab（甚至支持多个Scene共享一个perfab）
        /// </summary>
        /// <param name="sceneName"></param>
        private async UniTask LoadingInnerScene(string sceneName)
        {
            //只有这样的场景才是战斗场景。
            //这里判断是不是【表世界】 maptrue && noInnertrue && noBosstrue
            //场景命名随意：未必需要Map_数字_拓展；只要注意避免关键词"_Inner"
            if (sceneName.Contains("Map_") && !sceneName.Contains(SCENE_NAME_INNER_TAG)) //确保一定是表世界配套里世界，里世界不能加载里世界导致场景加载出错
            {
                CurrentInnerTag = false; //重置内场景标记
                InnerSceneName = sceneName + SCENE_NAME_INNER_TAG;

                //并非所有场景都有inner设计，所以为了都进行inner加载，解决方法需要判断是否配置在BuildScene里
                if (m_AllBuildScenes.Contains(InnerSceneName))
                {
                    //MonoHelper.StartCoroutine(LoadScene());

                    var handler = Addressables.LoadSceneAsync("map/" + InnerSceneName.ToLower(), LoadSceneMode.Additive);
                    await handler.Task;
                }
                //InnerSceneName = string.Empty;
            }
            //Map_N_Inner
        }

        private void GetAllBuildSceneName()
        {
            int count = SceneManager.sceneCountInBuildSettings;
            //Debug.Log("Scene Count = " + count);
            //string[] scene_paths = new string[count];
            for (int i = 0; i < count; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string[] strs = scenePath.Split('/'); //需要注意场景命名里面不能带“/”
                string str = strs[strs.Length - 1];
                strs = str.Split('.');
                str = strs[0];
                m_AllBuildScenes.Add(str);
                //场景的名字
                //Debug.Log("场景的名字为：" + str);
            }
        }

        public void Init()
        {
            CheckSingleton();
            GetAllBuildSceneName();

            //第一次注册和加载
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged; //切换Active
            SceneManager.sceneLoaded += SceneManager_sceneLoaded; //首次加载,其他卸载
        }

        public bool CurrentInnerTag = false;

        public void SwitchInnerOuterWorld()
        {
            for (int i = 0; i < LoadGroup.Count; i++)
            {
                if (!CurrentInnerTag)
                {
                    if (LoadGroup[i].name == InnerSceneName)
                    {
                        GameManager.Instance.SetBattleCameraEffect(true, "Effects/BattleSceen/Fx_FB_MengJKJ_camera_002");
                        //表世界 ，里世界清理为了就是快，跟进房间一样也和处理波纹一样，不能load和GC
                        //CurSceneName = InnerSceneName; GlobalEvent.OnDiffMap_SceneChange?.Invoke(OldSceneName, SceneName);
                        SceneManager.SetActiveScene(LoadGroup[i]);
                        CurrentInnerTag = true;
                        break;
                    }
                }
                else
                {
                    if (LoadGroup[i].name == OuterSceneName)
                    {
                        GameManager.Instance.SetBattleCameraEffect(false);
                        //CurSceneName = OuterSceneName; GlobalEvent.OnDiffMap_SceneChange?.Invoke(OldSceneName, SceneName);
                        SceneManager.SetActiveScene(LoadGroup[i]);

                        CurrentInnerTag = false;
                        break;
                    }
                }
            }
        }

        public List<Scene> LoadGroup = new();

        //private bool Ftag = true;
        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //首次加载:隐藏
            if (arg1 == LoadSceneMode.Additive)
            {
                if (!string.IsNullOrEmpty(arg0.name) && !string.IsNullOrEmpty(arg0.path))
                {
                    if (arg0.name.Contains("Map_") && arg0.name.Contains(SCENE_NAME_INNER_TAG))
                    {
                        GameObject[] news = arg0.GetRootGameObjects();
                        for (int i = 0; i < news.Length; i++)
                        {
                            news[i].SetActive(false);
                        }

                        LoadGroup.Add(arg0);
                    }
                }
            }
            //其他卸载：设定--里世界属于附属者，表世界切换后里世界就卸载
            //1，波纹不会触发（因为不切换）
            //2，场景会触发，不想触发就不触发，但是要找到unload的时机（表里切换用Additive，其他用Single）
            //3，不会出现多个里世界（看设计，这个支持）
            else if (arg1 == LoadSceneMode.Single)
            {
                //重新启动，则重新加载分组，《Single》帮我们做了卸载
                LoadGroup.Clear();
                LoadGroup.Add(arg0);
            }
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            if (!string.IsNullOrEmpty(arg0.name) && !string.IsNullOrEmpty(arg0.path))
            {
                GameObject[] olds = arg0.GetRootGameObjects();
                for (int i = 0; i < olds.Length; i++)
                {
                    olds[i].SetActive(false);
                }
            }

            if (!string.IsNullOrEmpty(arg1.name) && !string.IsNullOrEmpty(arg1.path))
            {
                GameObject[] news = arg1.GetRootGameObjects();
                for (int i = 0; i < news.Length; i++)
                {
                    news[i].SetActive(true);
                }
            }
        }
        /// <summary>
        /// 场景迷雾开关
        /// </summary>
        /// <param name="isOpen"></param>
        public void SwitchFogAndCloud(int index,bool isOpen)
        {
            if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
            {
                Transform lightRoot = GameManager.Instance.M_Map.M_rootHelper.transform.Find("Light/Light_5set");
                if (lightRoot != null) 
                {
                    for (int i = 0; i < lightRoot.childCount; i++) 
                    {
                        Transform lightEffect = lightRoot.GetChild(i);
                        if (isOpen) 
                        {
                            if (i == index)
                            {
                                lightEffect.gameObject.SetActive(true);
                            }
                            else
                            {
                                lightEffect.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                lightEffect.gameObject.SetActive(true);
                            }
                            else
                            {
                                lightEffect.gameObject.SetActive(false);
                            }
                        }
                        
                    }
                }
            }
        }
    }
}