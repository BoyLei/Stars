using Cysharp.Threading.Tasks;
using SGF.Unity;
using StarProject;
using StarProject.Service.Language;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameUpdate : MonoBehaviour
{
    //远程版本号
    public GameVersion RemoteVersion;

    //本地版本号
    public GameVersion LocalVersion;
    private LoadingView GameProcess;

    private const string preExcel = "ExcelBytes/PreCSText";
    private static PreCSTextData preCSTextData = null;

    /// <summary>
    /// 是否重启游戏
    /// </summary>
    bool RestarGame = false;

    /// <summary>
    /// 是否覆盖版本号
    /// </summary>
    bool OverrideVersion = false;

    /// <summary>
    /// 是否有更新
    /// </summary>
    private bool update = false;

    /// <summary>
    /// 场景权重
    /// </summary>
    private float SceneWeight = 0.3f;

    private bool isSilentUpdate = false;
    private List<object> _updateKeys = new();
    private List<IResourceLocator> _updateLocators = new();
    private AsyncOperationHandle initHandle;

    /// <summary>
    /// 是否是 loading状态
    /// </summary>
    bool isLoadingState = false;

    //private Dictionary<int, (string content, float weight)> LoadingInfos = new()
    //{
    //    {1,($"..{GameConfig.LocalStr["HotDownload1"]}..",0.1f)},
    //    {2,($"..{GameConfig.LocalStr["HotDownload2"]}..",0.2f)},
    //    {3,($"..{GameConfig.LocalStr["HotDownload3"]}..",0.3f)},
    //    {4,($"..{GameConfig.LocalStr["HotDownload4"]}..",0.4f)},
    //    {5,($"..{GameConfig.LocalStr["HotDownload5"]}..",0.5f)},
    //    {6,($"..{GameConfig.LocalStr["HotDownload6"]}..",0.6f)},
    //    {7,($"..{GameConfig.LocalStr["HotDownload7"]}..",0.7f)},
    //    {8,($"..{GameConfig.LocalStr["HotDownload7"]}..",0.8f)},
    //    {9,($"..{GameConfig.LocalStr["HotDownload8"]}..",0.9f)},
    //};
    private Dictionary<int, (string content, float bigStepProcess)> LoadingInfos = new();

    public void CheckUpdate(LoadingView gameProcess)
    {
        GameProcess = gameProcess;
        LoadPreCSTextDataCfg();
        SGF.Debuger.Log("首场景 进度开始-------------------");
        GameProcess.OnProcessStart();
        StartCoroutine("UpdateGame");
        StartPostLoaingEvent();
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

        SGF.Debuger.LogWarning("首场景 111111111  加载首个配置 ----------完成");


        LoadingInfos.Clear();
        {
            {
                string tips = GetTipsTextByKey("HotDownload1");
                LoadingInfos.Add(1, ($"..{tips}..", 0.1f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload2");
                LoadingInfos.Add(2, ($"..{tips}..", 0.2f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload3");
                LoadingInfos.Add(3, ($"..{tips}..", 0.3f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload4");
                LoadingInfos.Add(4, ($"..{tips}..", 0.4f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload5");
                LoadingInfos.Add(5, ($"..{tips}..", 0.5f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload6");
                LoadingInfos.Add(6, ($"..{tips}..", 0.6f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload7");
                LoadingInfos.Add(7, ($"..{tips}..", 0.7f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload7");
                LoadingInfos.Add(8, ($"..{tips}..", 0.8f));
            }
            {
                string tips = GetTipsTextByKey("HotDownload8");
                LoadingInfos.Add(9, ($"..{tips}..", 0.9f));
            }
        }
    }

    public static string GetTipsTextByKey(string key)
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

    /// <summary>
    /// 不叫权重，这里是target用id分步去确定分步process.
    /// 因为权重是占比不突破型态，是以目标锚定，指需关心内心信息占比的设计。
    /// 另外写死的0.1和字典里面定的其实有约束关系都要一起修改也不好。
    /// 
    /// 目前设计是：分布式写各个步骤的结果，单独步骤的结果可以再次区分其细分步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tinyStepPercent">1就是小步填满</param>
    /// <param name="isLerp"></param>
    /// <param name="msg"></param>
    private void ShowProcessInfo(int id, float tinyStepPercent, bool isLerp, string msg = "")
    {
        if (GameProcess != null && GameProcess.gameObject != null && LoadingInfos != null)
        {
            if (LoadingInfos.ContainsKey(id))
            {
                var info = LoadingInfos[id];
                //0.1是分步设计
                GameProcess.OnProcess(info.bigStepProcess + (0.1f * tinyStepPercent), isLerp, LoadingState.GameUpdate);
                if (!string.IsNullOrEmpty(msg))
                {
                    GameProcess.OnShowText(msg);
                }
                else
                {
                    GameProcess.OnShowText(info.content);
                }
                //SGF.Debuger.LogWarning($"首场景 GameUpdate 预加载 进度={info.weight + 0.1f * weight},id={id},weight={weight}");
            }
        }
    }

    private async void UpdateGame()
    {
        isLoadingState = true;
        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.BEGIN_LOCAL_FILE);
        //读取本地版本号
        Debug.Log("==========读取本地版本号==========");
        ShowProcessInfo(1, 1, true);
        LoadLocalVersion();

        Debug.Log("==========检查热更==========");
        ShowProcessInfo(2, 1, true);
        update = false;//CheckUpdate();
        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.END_LOCAL_FILE);

        Debug.Log($"==========初始化本地配置==========");
        RemoteConfig.InitLocal();

        Debug.Log($"==========是否需要热更 {update}==========");
        //------
        if (update)
        {
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.YES_UPDATE_CONTENT);

            //读取远程配置
            Debug.Log("==========读取远程配置==========");
            ShowProcessInfo(3, 1, true);
            /*  await RemoteConfig.Init(GameApp.Instance.ConfigUrl);*/


            E_LoginCfgEnum e_LoginCfgEnum = SDKManager.Instance.GetLoginCfgEnum();
            string url = GameApp.Instance.ConfigUrl; //默认国内
            string whiteListURL = GameApp.Instance.ConfigUrlIMEA; //默认国内
            Debug.Log("GameApp.Instance.ConfigUrlIMEA:" + GameApp.Instance.ConfigUrlIMEA);
            switch (e_LoginCfgEnum)
            {
                //两个配置先合并成一个，beta和release定下来；全局加一下imea各个地区
                case E_LoginCfgEnum.Star_Dev:
                    break;
                case E_LoginCfgEnum.OutSide_Test:
                    break;
                case E_LoginCfgEnum.Version_Test:
                    break;
                case E_LoginCfgEnum.Romania:
                    url = GameApp.Instance.EURConfigUrl;
                    whiteListURL = GameApp.Instance.EURConfigUrlIMEA; //默认国内
                    break;
                case E_LoginCfgEnum.Philippines:
                    url = GameApp.Instance.SEAConfigUrl;
                    whiteListURL = GameApp.Instance.SEAConfigUrlIMEA; //默认国内
                    break;
                default:
                    break;
            }


            await WhiteListConfig.Init(whiteListURL);
            Debug.Log("SdkSoneId:" + SDKManager.Instance.SdkSoneId);

            //判断是不是
            //然后本地有xml
            //然后看热更两个地址
            //然后走海外流程 看看Qa对不对，建立文件夹
            //ShowFPS
            //海外不行就国内


            bool isQA = true;
            //处理qa还是正式
            isQA = WhiteListConfig.WhiteListsLocal.Contains(SDKManager.Instance.SdkSoneId);
            string betaORrelease;
            betaORrelease = isQA ? "beta" : "release";
            string finalurl = string.Format(url, betaORrelease);


            bool wait = true;

            int cnt = 0;

            void OnCallBack(bool result)
            {
                if (result)
                {
                    wait = false;
                }
                else
                {
                    UIAPI.ShowSystemMsgBox(GameUpdate.GetTipsTextByKey("NetworkTipName"),
                        GameUpdate.GetTipsTextByKey("NetworkTip"),
                        GameUpdate.GetTipsTextByKey("NetworkBtnFalse"), () => { Application.Quit(); },
                        GameUpdate.GetTipsTextByKey("NetworkBtnSure"), () =>
                        {
                            RemoteConfig.Init(finalurl, OnCallBack);
                            //重连
                        }
                    );
                    Debug.Log("远程配置初始化失败");
                    wait = true;
                }
            }

            await RemoteConfig.Init(finalurl, OnCallBack);

            while (wait)
            {
                await UniTask.Delay(100);
            }


            //读取远程版本号
            Debug.Log("==========读取远程版本号==========");
            await LoadRemoteVersion();


            if (LocalVersion.Main < RemoteVersion.Main || LocalVersion.Tiny < RemoteVersion.Tiny)
            {
                Debug.Log("local:main" + LocalVersion.Main + "ltiny:" + LocalVersion.Tiny + "rmain:" +
                          RemoteVersion.Main + "rtiny:" + RemoteVersion.Tiny);
                //OpenForceUpdateUI();
                return;
            }

            //约定：中版本号变更一定是新包
            if (LocalVersion.Res < RemoteVersion.Res)
            {
                OverrideVersion = true;
            }

            //资源热更
            Debug.Log("==========资源热更==========");
            await IEDoHotUpdate();
        }
        else
        {
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.NO_UPDATE_CONTENT);
            Debug.Log("==========异步加载Modules==========");
            PreLoadGlobalModule();
        }
    }

    private void OpenForceUpdateUI()
    {
        UIAPI.ShowMsgBoxNewAsync(69, (string v) => { Application.Quit(); });
    }

    private void LoadLocalVersion()
    {
        string path = Application.persistentDataPath + "/Version.json";
        string content = "";
        if (File.Exists(path))
        {
            content = File.ReadAllText(path, System.Text.Encoding.UTF8);
            LocalVersion = Newtonsoft.Json.JsonConvert.DeserializeObject<GameVersion>(content);
        }
        else
        {
            //如若首次读不到,就用buildPack中我缓存的，本地文件=playersetting；这样我就不用拷贝目录了
            LocalVersion = new GameVersion(1, 1);
            string[] parts = Application.version.Split('.');
            //int.Parse(parts[0]);巨大版本号
            LocalVersion.Main = int.Parse(parts[1]);
            LocalVersion.Tiny = int.Parse(parts[2]);
            LocalVersion.Res = int.Parse(parts[3]);
            LocalVersion.BundleCode = 0; //这里有信息查不过不作为对比
        }

        GameApp.Instance.LocalVersion = LocalVersion;
    }

    private void UpdateLocalVersion()
    {
        //对应上文读，这里可以写，但首次启动需要读版本号
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(LocalVersion);
        string path = Application.persistentDataPath + "/Version.json";

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileStream fs = new(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
    }

    public bool CheckUpdate()
    {
        return GameApp.Instance.CanUpdate;
    }


    IEnumerator LoadRemoteAssembly(string url)
    {
        Debug.Log($"================热更程序集============= {url}");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                //写入到本地文件
                string dllPath = Application.dataPath + "/Managed/Assembly-CSharp.dll";
                if (System.IO.File.Exists(dllPath))
                {
                    System.IO.File.Delete(dllPath);
                }

                FileStream fs = new(dllPath, FileMode.CreateNew);
                fs.Write(webRequest.downloadHandler.data, 0, webRequest.downloadHandler.data.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }
    }

    IEnumerator LoadRemoteVersion()
    {
        Debug.Log("=============加载远程版本号================");
        string remotepath = $"{RemoteConfig.ResUrl}/GameResRelease/Version.json";
        Debug.Log($"=============remotepath={remotepath}================");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(remotepath))
        {
            yield return webRequest.SendWebRequest();
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                RemoteVersion = null;
                Debug.Log(webRequest.error);
            }
            else
            {
                RemoteVersion =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<GameVersion>(webRequest.downloadHandler.text);
            }
        }
    }

    private void ReStart()
    {
        Debug.Log("=============开始重新启动游戏================");
        System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
        Application.Quit();
        Debug.Log("=============重新启动游戏结束================");
    }

    async UniTask GameUpdateToLoadInit(string SceneName = "Init")
    {
        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.LOAD_SCENE_RESOURCE);

        //ShowProcessInfo(9, 1, true);
        var loader = Addressables.LoadSceneAsync("map/" + SceneName.ToLower(), LoadSceneMode.Single, true);
        float process = 0;
        loader.ToUniTask(Progress.Create<float>(n => { ShowProcessInfo(9, Mathf.Ceil(n), false); }));

        loader.Completed += (scene) =>
        {
            if (GameProcess != null && GameProcess.gameObject != null)
            {
                SGF.Debuger.LogWarning($"首场景 666666666 进入Init场景 --------- 结束");

                ShowProcessInfo(9, 1, false);
                GameProcess.OnProcessEnd();
                //一致包裹到最后
                isLoadingState = false;
            }

            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.LOAD_SCENE_RESOURCE_SUC);


            SGF.Debuger.LogWarning($"首场景 666666666 进入Init场景 --------- 加载完成");
        };

        await loader;
    }


    string GetDownloadSize(long size)
    {
        int cnt = 0;
        float temp = size;
        while (temp > 1024)
        {
            temp /= 1024;
            ++cnt;
        }

        if (cnt >= 3)
        {
            return $"{temp.ToString("F2")}GB";
        }
        else if (cnt >= 2)
        {
            return $"{temp.ToString("F2")}MB";
        }
        else if (cnt >= 1)
        {
            return $"{temp.ToString("F2")}KB";
        }
        else
        {
            return $"{temp.ToString("F2")}B";
        }
    }


    async UniTask IEDoHotUpdate()
    {
        Debug.Log($"==============@@ 初始化资源 ==================");
        ShowProcessInfo(4, 1, true);
        initHandle = Addressables.InitializeAsync();
        initHandle.Completed += AddressableInitCompleted;
        await initHandle.Task;
    }

    async void AddressableInitCompleted(AsyncOperationHandle obj)
    {
        Debug.Log("InitializeAsync 初始化 完成" + obj);
        ShowProcessInfo(5, 1, true);
        //开始连接服务器检查更新
        var handle = Addressables.CheckForCatalogUpdates(false);
        await handle.Task;
        Debug.Log("check catalog status " + handle.Status);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _updateKeys.Clear();
            _updateLocators.Clear();
            List<string> catalogs = handle.Result;
            Debug.Log("CatLog Count" + catalogs.Count);
            if (catalogs != null && catalogs.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false); //更新catalog存储记录
                await updateHandle.Task;

                foreach (var item in updateHandle.Result)
                {
                    foreach (var key in item.Keys)
                    {
                        if (key is string)
                            _updateKeys.Add(key);
                    }
                }

                _updateLocators.AddRange(updateHandle.Result);
                Addressables.Release(updateHandle);
                Debug.Log("下载流程");
                await DownloadAsset();

                //延迟0.2秒 等待 释放完毕
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
            else
            {
                //UpdateCheckFinish();
                Debug.Log("dont need update catalogs");
            }
        }
        else
        {
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.UPDATE_FAIL);
        }

        Addressables.Release(handle);
        Debug.Log(
            $"=============update:{update} RemoteVersion:{RemoteVersion.Tiny}.{RemoteVersion.Res}  GameApp:{LocalVersion.Tiny}.{LocalVersion.Res}================");
        //------
        if (update)
        {
            //仅仅Windows 下需要热更dll
#if UNITY_STANDALONE_WIN
            //版本号小于在远程
            if (LocalVersion.Tiny < RemoteVersion.Tiny)
            {
                Debug.Log("=============代码热更================");

                string remotedllPath = string.Format("{0}/GameRes/{1}/{2}/{3}/code/Assembly-CSharp.dll",
                    RemoteConfig.ResUrl,
                    GameApp.Instance.publish.ToString(),
                    GameApp.Instance.channel.ToString(),
                    GameApp.Instance.buildtarget.ToString()
                );

                Debug.Log($"remotedllPath {remotedllPath}");
                Debug.Log("==========更新Dll==========");
                await LoadRemoteAssembly(remotedllPath);


                //重新启动游,此时的Assembly-CSharp.dll 已经在内存
                RestarGame = true;
            }
#endif
            if (OverrideVersion)
            {
                //更新本地版本号
                Debug.Log("=============更新本地版本号================");
                LocalVersion = RemoteVersion;
                GameApp.Instance.LocalVersion = RemoteVersion;
                UpdateLocalVersion();
            }

            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.UPDATE_SUC);
            if (RestarGame)
            {
                Debug.Log("==========重启游戏==========");
                ReStart();
            }
        }

        Debug.Log("==========异步加载Modules==========");
        PreLoadGlobalModule();
    }

    private void StagePerLoadAddressable()
    {
    }

    #region 预加载资源

    private void PreLoadGlobalModule()
    {
        SGF.Debuger.Log("首场景 2222222  加载GlobalModule ----------  开始");
        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.PRE_LOAD_GLOBAL_MODULE);

        var path = ResourceHelperMono.GlobalModule.ToLower();
        var loder = Addressables.InstantiateAsync(path);
        loder.Completed += (obj) =>
        {
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.PRE_LOAD_GLOBAL_MODULE_FINISH);

            SGF.Debuger.LogWarning("首场景 2222222  加载GlobalModule ----------  完成");

            SGF.Debuger.Log("首场景 3333 初始化 ResourceFormalManager 开始");
            StarProject.Service.Resource.ResourceFormalManager.Instance.Init();
            SGF.Debuger.LogWarning("首场景 3333 初始化 ResourceFormalManager 完成");


            //资源预加载
            PerLoadAddressable();
            //每一个返回检车数量满足后
            //lua预热 配置表 技能都预热 =loading都不等 SDK登录时间就很长，high就默认就当成同步不等，减小数据由于深度叠加广度带来的时间长度增加
        };
    }

    //【预加载代码资源部分--游戏启动部分】
    //shader是由变体部分处理
    //特效时由perwarm处理
    private async void PerLoadAddressable()
    {
        ShowProcessInfo(7, 1, true);

        {
            SGF.Debuger.Log("首场景 44444 预加载部分UI资源 开始");

            string[] preloadAssets = new string[]
            {
                "UI/StarWorld/StarWorldPage",
                //"UI/Common/UIMsgBox",
                //"UI/Common/UITips",
                "UI/Common/FormItem",
                "UI/Common/Prefab/PlayVideoWidget",
                "UI/StarWorld/Prefab/UnitPendantViewNew",
                "UI/StarWorld/Prefab/ObjectUnitPendantViewNew",
                "UI/Common/Prefab/PlayTimelineBlackWidget"
            };
            foreach (var item in preloadAssets)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(item,
                    null);
            }

            string[] preloadMaterials = new string[]
            {
                "Roles/Material/State/Common_Char_Ghost",
                "Roles/Material/State/Common_Char_ZwriteIn"
            };
            foreach (var item in preloadMaterials)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(item,
                    null);
            }

            SGF.Debuger.Log("首场景 44444 预加载部分UI资源 完成");
        }

        // 预加载配置文件
        {
            //Addressables.LoadAssetsAsync<UnityEngine.TextAsset>("config", null).Completed += AsyncLoadCSConfig;
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.PER_LOAD_ADDRESSABLE);

            SGF.Debuger.Log("首场景 55555555 预加载 配置文件 config 开始-------");
            //List<string> keys = new() { "config", "lua", "skillconfig" };
            List<string> keys = new() { "config", "lua" };
            //var list = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, null).WaitForCompletion();
            //if (list != null)
            //{
            //    CSConfigIsLoad = true;
            //    CSConfigCount = list.Count;
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        string path = list[i].ToString();
            //        path = path.Replace("Assets/Res/", string.Empty)
            //            .Replace(".json", string.Empty)
            //            .Replace(".txt", string.Empty)
            //            .Replace(".bytes", string.Empty)
            //            .Replace(".asset", string.Empty);
            //        path = path.ToLower();
            //        if (path.Contains("tutorialflagconfig"))
            //        {
            //            //尔东引导部分是自己通用接口，加载和使用，这个文件不走预热的加载和使用逻辑;
            //            AsyncLoadCSConfig(path, null);
            //        }
            //        else
            //        {
            //            //SGF.Debuger.Log($"预加载 配置文件 config 文件名=》{path}");
            //            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync(path,
            //                AsyncLoadCSConfig);
            //        }
            //    }

            //    Addressables.Release(list);
            //}

            {
                // 静默加载
                //参数一：资源名和标签名的组合
                //参数二：合并模式
                //参数三：资源类型
                AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, null);
                await handle;
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        CSConfigIsLoad = true;
                        CSConfigCount = obj.Result.Count;
                        //资源定位信息加载成功
                        foreach (var item in obj.Result)
                        {
                            string path = item.PrimaryKey;
                            path = path.Replace("Assets/Res/", string.Empty)
                                .Replace(".json", string.Empty)
                                .Replace(".txt", string.Empty)
                                .Replace(".bytes", string.Empty)
                                .Replace(".asset", string.Empty);
                            path = path.ToLower();
                            if (path.Contains("tutorialflagconfig"))
                            {
                                //尔东引导部分是自己通用接口，加载和使用，这个文件不走预热的加载和使用逻辑;
                                AsyncLoadCSConfig(path, null);
                            }
                            else
                            {
                                //SGF.Debuger.Log($"预加载 配置文件 config 文件名=》{path}");
                                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync(path,AsyncLoadCSConfig);
                            }
                        }
                    }
                };
                Addressables.Release(handle);
            }

            {
                // 静默加载
                //参数一：资源名和标签名的组合
                //参数二：合并模式
                //参数三：资源类型
                AsyncOperationHandle<IList<IResourceLocation>> handle2 = Addressables.LoadResourceLocationsAsync(new List<string>() { "skillconfig" }, Addressables.MergeMode.None, null);
                await handle2;
                handle2.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        //资源定位信息加载成功
                        foreach (var item in obj.Result)
                        {
                            //使用定位信息来加载资源
                            //我们可以利用定位信息 再去加载资源 （一些额外信息）
                            //SGF.Debuger.LogWarning(item.PrimaryKey);
                            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync(item.PrimaryKey, null);
                            //SGF.Debuger.LogWarning(item.InternalId);
                            //SGF.Debuger.LogWarning(item.ResourceType.Name);
                        }
                    }
                };
                Addressables.Release(handle2);
                //StarProject.Service.LocalData.LocalDataManager.Instance.LoadSkillConfigs();
            }
        }
    }

    private bool CSConfigIsLoad = false;
    private int CSConfigCount = 0;
    private int CSConfigIndex = 0;

    private void AsyncLoadCSConfig(string path, UnityEngine.TextAsset textAsset)
    {
        //if (textAsset == null)
        //{
        //    SGF.Debuger.LogError($"首场景 55555555 预加载 配置文件 config index={CSConfigIndex},path={path},textAsset={textAsset},资源加载失败了err!!!");
        //}

        CSConfigIndex++;

        //float process = LoadingInfos[8].weight + ((float)CSConfigIndex / (float)CSConfigCount * 0.1f);
        float process = (float)CSConfigIndex / (float)CSConfigCount;

        ShowProcessInfo(8, process, false);

        if (CSConfigIndex >= CSConfigCount)
        {
            SGF.Debuger.LogWarning($"首场景 55555555 预加载 配置文件 config 结束-------index={CSConfigIndex},count={CSConfigCount}");

            ShaderVariantLoading.Instance.Init();
            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.PER_LOAD_ADDRESSABLE_FINISH);

            TryLoadScene();
        }
    }

    private bool LuaTxtIsLoad = false;
    private int LuaTxtCount = 0;
    private int LuaTxtIndex = 0;

    private void AsyncLoadLuaTxtConfig(UnityEngine.TextAsset textAsset)
    {
        if (textAsset == null)
        {
            SGF.Debuger.LogWarning($"AsyncLoadLuaUIConfig() err!!!");
        }

        LuaTxtIndex++;
        if (LuaTxtIndex >= LuaTxtCount)
        {
            TryLoadScene();
        }
    }

    private void TryLoadScene()
    {
        if (CSConfigIsLoad)
        {
            if (CSConfigIndex < CSConfigCount)
            {
                return;
            }
        }

        if (LuaTxtIsLoad)
        {
            if (LuaTxtIndex < LuaTxtCount)
            {
                return;
            }
        }

        SGF.Debuger.Log($"首场景 666666666 进入Init场景 --------- 开始");

        GameUpdateToLoadInit();
    }

    #endregion


    public IEnumerator DownloadAsset()
    {
        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.BEGIN_CDN);

        SGF.Debuger.Log("开始下载");
        ShowProcessInfo(6, 1, true);
        float StartDownloadtime = Time.realtimeSinceStartup;
        IEnumerable<object> enumerable = _updateKeys;
        var downloadsize = Addressables.GetDownloadSizeAsync(enumerable); //获取要下载大小，需release
        yield return downloadsize;
        string realDownloadSize = GetDownloadSize(downloadsize.Result);
        SGF.Debuger.Log("本次下载资源大小:" + realDownloadSize);

        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.TIP_SIZE_UPDATE);

        float preocess = 0;
        if (downloadsize.Result > 0)
        {
            SGF.Debuger.Log("下载 download _updateKeys :" + _updateKeys);
            var downloadHandle =
                Addressables.DownloadDependenciesAsync(enumerable, Addressables.MergeMode.Union, false);
            string tips = GetTipsTextByKey("HotDownloadInfo");
            while (!downloadHandle.IsDone)
            {
                if (downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    yield break;
                }

                float elapsedTime = Time.realtimeSinceStartup - StartDownloadtime; // 获取经过的时间（秒）
                long downloadedBytes = downloadHandle.GetDownloadStatus().DownloadedBytes; // 获取已下载的字节数
                float downloadSpeed = downloadedBytes / 1024f / 1024f / elapsedTime; // 计算下载速度（字节/秒）
                // 构造要显示的消息
                //string msg = $"资源包大小{realDownloadSize} 当前下载速度{downloadSpeed:F2} MB/S";
                //string msg = string.Format(GameConfig.LocalStr["HotDownloadInfo"], realDownloadSize, $"{downloadSpeed:F2} MB/S");
                string msg = string.Format(tips, realDownloadSize, $"{downloadSpeed:F2} MB/S");

                // 更新进度条和显示信息
                //preocess = LoadingInfos[6].weight + (downloadHandle.GetDownloadStatus().Percent * SceneWeight);
                preocess = downloadHandle.GetDownloadStatus().Percent;

                SGF.Debuger.Log("下载 进度 :" + preocess);

                ShowProcessInfo(6, SceneWeight, false, msg);
                //GameProcess.OnProcess(preocess);
                yield return null;
            }

            Addressables.Release(downloadHandle);
        }

        Addressables.Release(downloadsize);
    }

    /// <summary>
    /// 开始 post 
    /// </summary>
    public void StartPostLoaingEvent()
    {
        DelayInvokerOnTime(5, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_5 });

        DelayInvokerOnTime(10, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_10 });

        DelayInvokerOnTime(15, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_15 });

        DelayInvokerOnTime(20, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_20 });

        // DelayInvoker.DelayInvokerOnTime(5, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_5 });

        // DelayInvoker.DelayInvokerOnTime(10, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_10 });

        // DelayInvoker.DelayInvokerOnTime(15, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_15 });

        // DelayInvoker.DelayInvokerOnTime(20, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_20 });

        // DelayInvoker.DelayTimeInvoke(5, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_5 });

        // DelayInvoker.DelayTimeInvoke(10, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_10 });

        // DelayInvoker.DelayTimeInvoke(15, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_15 });

        // DelayInvoker.DelayTimeInvoke(20, PostSdkLoadingEvent, new object[] { E_SDK_PreEvent.BEGIN_LOCAL_FILE_20 });
    }

    private void PostSdkLoadingEvent(object[] args)
    {
        if (!isLoadingState)
        {
            return;
        }

        int postEvent = (int)args[0];

        // Debug.LogError($"{SGF.Time.TimeUtils.ClientUtcNow} 开始 上报 loading 事件: {postEvent}");
        SDKManager.Instance.EventPreEvent(postEvent);
    }

    public void DelayInvokerOnTime(float delayTime, DelayFunction func, params object[] args)
    {
        StartCoroutine(DelayInvokerOnTimeWorker(delayTime, func, args));
    }

    private static IEnumerator DelayInvokerOnTimeWorker(float delayTime, DelayFunction func, params object[] args)
    {
        yield return new WaitForSeconds(delayTime);
        ;

        try
        {
            func(args);
        }
        catch (Exception e)
        {
            SGF.Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
        }
        //Profiler.EndSample();
    }
}