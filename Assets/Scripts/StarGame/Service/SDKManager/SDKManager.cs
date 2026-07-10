using Frame;
using GSSDK;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SGF.Time;
using StarProject.Service.Language;
using StarProject.Service.Time;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Service.SDK
{

    /*    一种是jar封装，然后接入androidstudio-目前是这样-TODOJenkins
    一种是unitypackage
    一种是直接拖拽APK生成

    1，默认启动sdk，得到init回调信息
    2，login弹出默认窗口，得到数据，发送服务器，等待服务器验证响应
    3，服务器验证成功后可以点击游戏内的进入游戏
    */
    [XLua.LuaCallCSharp]
    public class SDKManager /*: *//*ServiceModule*//*MonoSingletonEx<SDKManager>*/
    {

        private static readonly object _lock = new();
        private static SDKManager _instance = null;

        private SDKManager() { }

        public static SDKManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SDKManager();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }


        private bool isInit = false;
        public bool IsInit => isInit;

        private string loginResult;
        // sdk 登录的 结果

        public string LoginResult
        {
            set
            {
                if (value == null || value == string.Empty)
                {
                    sdkLoginResult = null;
                }
                else
                {
                    sdkLoginResult = new SKDLoginResult(value);
                }

                loginResult = value;
            }
            get
            {
                return loginResult;
            }
        }


        public string SdkUserID
        {
            get
            {
                // 如果能拿到登录数据，那就返回 sdk 登录的 userID
                if (sdkLoginResult != null)
                {
                    return sdkLoginResult.userId;
                }

                // 如果拿不到, 那就返回默认值
                return "$_1";
            }
        }

        public SKDLoginResult sdkLoginResult;


        //账号，游客，手机；都会指定这个id，且不会变了；但这个不是最终绑定id
        //用途上他类似，一个机器的imea锁定设备的，换设备其实就变化了
        //设计上他，是提前生成的，是数据部和sdk联合生成的唯一id
        public string SdkSoneId
        {
            get
            {
                if (sdkDeviceInfo != null)
                {
                    return sdkDeviceInfo.soneid;
                }

                // 如果拿不到, 那就返回默认值
                return "$_1";
            }
        }

        /// <summary>
        /// 从 sdk 中 拿到的 设备相关数据信息
        /// </summary>
        private SDKDeviceInfo sdkDeviceInfo;
        /// <summary>
        /// 从 sdk 中 拿到的 渠道相关数据
        /// </summary>
        private SDKChannelInfo sdkChannelInfo;

        /// <summary>
        /// 需要发送给服务器的 sdk 相关数据
        /// </summary>
        public PostServerSDKData postServerSDKData = new();

        private bool isLogined = false;


        public bool IsLogined => isLogined;

        /// <summary>
        /// 是否实名
        /// </summary>
        private bool isRealName = false;
        public bool IsRealName => isRealName;

        /// <summary>
        /// 是否成年
        /// </summary>
        private bool isAdult = false;
        public bool IsAdult => isAdult;

        private string sdkLanguage;
        public string SdkLanguage => sdkLanguage;

        /// <summary>
        /// 默认是大陆
        /// </summary>
        private bool isMainLand = true;

        public bool IsMainLand => isMainLand;

        private Action onSdkInitCallBack;
        private void Init()
        {
            //CheckSingleton();
            Debug.Log($"[sdk] : OpenSDK remoteSetting={RemoteConfig.OpenSDK},IsHasOpenSDK={RemoteConfig.IsHasOpenSDK}");

            // 创建一个 空数据的实例
            sdkChannelInfo = new SDKChannelInfo();
            // 创建一个 空数据的实例
            sdkDeviceInfo = new SDKDeviceInfo();

            if (RemoteConfig.IsHasOpenSDK)
            {
                if (!RemoteConfig.OpenSDK)
                {
                    Debug.Log($"[sdk] : OpenSDK 被远程配置关闭了");

                    return;
                }
            }

            // Test();

            if (Application.isEditor)
            {
                return;
            }


            // 根据sdk标签决定走不走sdk 登录
#if STAR_SDK
             TryingSdkInit();
#endif


        }

        bool isTryingInit = false;
        /// <summary>
        /// 尝试 sdk 重新的初始化
        /// </summary>
        private void TryingSdkInit()
        {
            Debug.Log($"[sdk] 开始 init");
            isTryingInit = true;

            // 马彬彬 说 这个参数暂时可以不填
            Dictionary<string, object> info = new();


            GSUnity.Init(info, (string moduleName, string funcName, string result) =>
            {

                try
                {
                    OnGSCallback(moduleName, funcName, result);
                }
                catch (System.Exception e)
                {

                    Debug.LogError($"[sdk] OnGSCallback moduleName:{moduleName}, funcName: {funcName} , result: {result} 发生异常 \\n  捕获异常: {e.Message} , {e.StackTrace}");
                }

            });
        }

        private void OnGSCallback(string moduleName, string funcName, string result)
        {
            Debug.Log($"[sdk] OnGSCallback moduleName: {moduleName}, funcName: {funcName}, result: {result}");

            JsonData resultData = JsonMapper.ToObject(result);
            int code = (int)resultData["code"];
            if (moduleName == GSUnityConstants.Module.Platform)
            {
                if (funcName == GSUnityConstants.Func.Init)
                {
                    OnSDKInitCall(code, result);
                }
                else if (funcName == GSUnityConstants.Func.Platform.Login)
                {
                    OnSDKLoginCall(code, result);
                }
                else if (funcName == GSUnityConstants.Func.Platform.Logout)
                {
                    OnSDKLoginOutCall(code, result);
                }
                else if (funcName == GSUnityConstants.Func.Platform.ExitApp)
                {
                    OnSDKExitAppCall(code, result);
                }
                else if (funcName == GSUnityConstants.Func.Platform.BindAccount)
                {
                    OnSDKBindCall(code, result);
                }
                else if (funcName == GSUnityConstants.Func.Platform.SwitchAccount)
                {
                    OnSDKSwitchCall(code, result);
                }
                else
                {
                    // OnSDKExitAppCall(code, result);
                }


            }
            else if (moduleName == "permission" && funcName == "request")
            {
                if (code == 0)
                {
                    // 表示权限全部被允许

                }
                else if (code == 10026)
                {
                    // 说明有权限被禁止，CP可以根据data回调内容做逻辑处理
                }
            }
        }

        private void OnSDKInitCall(int code, string result)
        {
            if (code == 0)
            {
                isInit = true;
                Debug.Log($"[sdk] init 成功.");
                if (onSdkInitCallBack != null)
                {
                    Debug.Log($"[sdk] 准备执行 sdkInitCallBack ");

                    onSdkInitCallBack?.Invoke();
                }
                OnSdkInit();
            }
            else
            {
                Debug.Log($"[sdk] init 失败, result: {result}");
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKErrorTips"));

            }
        }

        private void OnSDKLoginCall(int code, string result)
        {
            if (code == 0)
            {
                Debug.Log($"[sdk] login 成功, result: {result}");
                LoginResult = result;

                InitDeviceInfo();
                InitPostServerSDKData();


                RefreshLoginStatus();
                GlobalEvent.OnSDKEvent?.Invoke("login", true);
            }
            else
            {
                isLogined = false;

                GlobalEvent.OnSDKEvent?.Invoke("login", false);
            }
        }

        private void OnSDKLoginOutCall(int code, string result)
        {
            if (code == 0)
            {
                Debug.Log($"[sdk] logout 成功, code: {code}, result: {result}");
                GlobalEvent.OnSDKEvent?.Invoke("loginOut", true);
                isLogined = false;
                LoginResult = string.Empty;

                // 设置一个唯一的值，这样 后面拿的值就永远不一样了
                LocalCacheManager.Instance.SetValueType("PrivacyCheck", TimeUtils.ClientNowStampMilli.ToString());
            }
            else
            {
                Debug.Log($"[sdk] logout 失败, code: {code}, result: {result}");
                //Util.ShowMessage(GameConfig.LocalStr["SDKErrorTips2"]);
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKErrorTips2"));
            }
        }


        private void OnSDKExitAppCall(int code, string result)
        {
            if (code == 0)
            {
                Debug.Log($"[sdk] ExipApp 成功, code: {code}, result: {result}");
                //退出游戏
                Application.Quit();
            }
            else
            {
                Debug.Log($"[sdk] ExipApp 失败, code: {code}, result: {result}");
            }
        }

        private void OnSDKBindCall(int code, string result)
        {
            if (code == 0)
            {
                Debug.Log($"[sdk] OnSDKBindCall 成功, code: {code}, result: {result}");

                EventPreEvent(E_SDK_PreEvent.REALNAME_VERIFICATION_SUC);
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKBindSuccessed"));
            }
            else
            {
                Debug.Log($"[sdk] OnSDKBindCall 失败, code: {code}, result: {result}");

                EventPreEvent(E_SDK_PreEvent.REALNAME_VERIFICATION_FAIL);
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKBindFailed"));
            }
        }

        private void OnSDKSwitchCall(int code, string result)
        {
            if (code == 0)
            {
                Debug.Log($"[sdk] OnSDKSwitchCall 成功, code: {code}, result: {result}");

                // 切换账号成功后, 需要返回到登录界面
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKSwitchSuccessed"));

                // 切换账号成功，客户端 重新调用登录 接口
                // Login();
            }
            else
            {
                Debug.Log($"[sdk] OnSDKSwitchCall 失败, code: {code}, result: {result}");
                //Util.ShowMessage(GameConfig.LocalStr["SDKSwitchFailed"]);
                Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("SDKSwitchFailed"));
            }
        }

        /// <summary>
        /// 游戏从登录场景或创角场景进入游戏主场景时（即游戏选服创建角色之后），需要调用该方法。
        /// </summary>
        /// <param name="ServerId">必传，服务器ID</param>
        /// <param name="ServerName">必传，服务器名称</param>
        /// <param name="RoleId">必传，游戏创建的角色ID（game_uid）</param>
        /// <param name="RoleName">必传，角色名称</param>
        /// <param name="RoleLevel">必传，角色等级</param>
        /// <param name="CreateTime">必传，角色创建时间</param>
        /// <param name="RealServer">可选，合服后玩家角色当前区服</param>
        /// <param name="ServerZone">服务器所在时区（海外）</param>
        public void ReportSDKEnterGame(string ServerId, string ServerName, string RoleId, string RoleName,
            string RoleLevel, string CreateTime, string RealServer = "", string ServerZone = "")
        {
            if (!IsInit)
            {
                return;
            }
            Dictionary<string, object> parameters = new()
            {
                { GSUnityConstants.ParameterKey.ServerId, ServerId }, // 必传，服务器ID
                { GSUnityConstants.ParameterKey.ServerName, ServerName }, // 必传，服务器名称
                { GSUnityConstants.ParameterKey.RoleId, RoleId }, // 必传，游戏创建的角色ID（game_uid）
                {  GSUnityConstants.ParameterKey.RoleName, RoleName }, // 必传，角色名称
                { GSUnityConstants.ParameterKey.RoleLevel, RoleLevel }, // 必传，角色等级
                { GSUnityConstants.ParameterKey.CreateTime, CreateTime }, // 必传，角色创建时间
            };

            if (RealServer != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.RealServer, RealServer);
            }

            if (ServerZone != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.ServerZone, ServerZone);
            }

            parameters.Add("FPS:", ShowFps.FPS);
            parameters.Add("ping:", TimeManager.Instance.AveragePing + "ms");
            Debug.Log("[sdk] ReportSDKEnterGame ");
            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.EnterGame, parameters);
        }

        /// <summary>
        /// 游戏角色第一次创建成功后，需要调用该方法。
        /// </summary>
        /// <param name="ServerId">必传，服务器ID</param>
        /// <param name="ServerName">必传，服务器名称</param>
        /// <param name="RoleId">必传，游戏创建的角色ID（game_uid）</param>
        /// <param name="RoleName">必传，角色名称</param>
        /// <param name="RoleLevel">必传，角色等级</param>
        /// <param name="CreateTime">必传，角色创建时间</param>
        /// <param name="RealServer">可选，合服后玩家角色当前区服</param>
        /// <param name="ServerZone">服务器所在时区（海外）</param>
        public void ReportSDKCreateRole(string ServerId, string ServerName, string RoleId, string RoleName,
            string RoleLevel, string CreateTime, string RealServer = "", string ServerZone = "")
        {
            if (!IsInit)
            {
                return;
            }
            Dictionary<string, object> parameters = new()
            {
                { GSUnityConstants.ParameterKey.ServerId, ServerId }, // 必传，服务器ID
                { GSUnityConstants.ParameterKey.ServerName, ServerName }, // 必传，服务器名称
                { GSUnityConstants.ParameterKey.RoleId, RoleId }, // 必传，游戏创建的角色ID（game_uid）
                {  GSUnityConstants.ParameterKey.RoleName, RoleName }, // 必传，角色名称
                { GSUnityConstants.ParameterKey.RoleLevel, RoleLevel }, // 必传，角色等级
                { GSUnityConstants.ParameterKey.CreateTime, CreateTime }, // 必传，角色创建时间
            };

            if (RealServer != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.RealServer, RealServer);
            }

            if (ServerZone != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.ServerZone, ServerZone);
            }

            parameters.Add("FPS:", ShowFps.FPS);
            parameters.Add("ping:", TimeManager.Instance.AveragePing + "ms");
            Debug.Log("[sdk] ReportSDKCreateRole ");

            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.CreateRole, parameters);
        }

        public void ReportSDKLevelUp(string ServerId, string ServerName, string RoleId, string RoleName,
            string RoleLevel, string CreateTime, string RealServer = "", string ServerZone = "")
        {
            if (!IsInit)
            {
                return;
            }
            Dictionary<string, object> parameters = new()
            {
                { GSUnityConstants.ParameterKey.ServerId, ServerId }, // 必传，服务器ID
                { GSUnityConstants.ParameterKey.ServerName, ServerName }, // 必传，服务器名称
                { GSUnityConstants.ParameterKey.RoleId, RoleId }, // 必传，游戏创建的角色ID（game_uid）
                {  GSUnityConstants.ParameterKey.RoleName, RoleName }, // 必传，角色名称
                { GSUnityConstants.ParameterKey.RoleLevel, RoleLevel }, // 必传，角色等级
                { GSUnityConstants.ParameterKey.CreateTime, CreateTime }, // 必传，角色创建时间
            };

            if (RealServer != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.RealServer, RealServer);
            }

            if (ServerZone != "")
            {
                parameters.Add(GSUnityConstants.ParameterKey.ServerZone, ServerZone);
            }

            parameters.Add("FPS:", ShowFps.FPS);
            parameters.Add("ping:", TimeManager.Instance.AveragePing + "ms");

            Debug.Log("[sdk] ReportSDKLevelUp ");

            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.LevelUp, parameters);
        }

        private void RefreshLoginStatus()
        {
            if (!string.IsNullOrEmpty(LoginResult))
            {
                isLogined = true;

                isRealName = sdkLoginResult.IsRealName;

                isAdult = sdkLoginResult.IsAdult;

                Debug.Log($"[sdk] 刷新 登录凭证状态: isRealName {isRealName} , isAdult: {isAdult}");

            }
            else
            {
                isLogined = false;
                isRealName = false;
                isAdult = false;
            }
        }

        public void Test()
        {
            SDKManager.Instance.LoginResult =
             @"{
                'code': 0,
                'data': {
                    'userId': '10001_310153892574',
                    'isAdult': 1,
                    'userName': '310153892574',
                    'userNick': '',
                    'channelId': 410004,
                    'appId': 10100078,
                    'isGuest': 0,
                    'extension': '',
                    'sign': '4306d3162d2be360d193f23b13f11972',
                    'userAge': 0,
                    'realNameStatus': 1
                },
                'msg': 'success',
                'dataType': 0
            }";

            RefreshLoginStatus();

            SGF.Debuger.LogError($"[sdk] dataString : {sdkLoginResult.dataString}");
            SGF.Debuger.LogError($"[sdk] ChannelID : {sdkLoginResult.ChannelID}");

        }

        public void SetOnSdkInit(Action ac)
        {
            Debug.Log($"[sdk] SetOnSdkInit 准备 设置回调 ");


#if UNITY_EDITOR
            // 编译器的模式直接 执行回调，没有sdk.
            ac?.Invoke();
            return;
#endif

#if STAR_SDK
            if (IsInit)
            {
                Debug.Log($"[sdk] SetOnSdkInit sdk 已经 Init, 立即执行回调 ");
                ac?.Invoke();
            }
            else
            {

                Debug.Log($"[sdk] SetOnSdkInit 设置回调 ");

                onSdkInitCallBack = ac;
            }
#else
            ac?.Invoke();
#endif

        }

        private void OnSdkInit()
        {
            isTryingInit = false;

            EventPreEvent(E_SDK_PreEvent.INIT_SDK);

            InitSDKBuglyReport();

            InitDeviceInfo();

            InitChannelInfo();

            InitSDKLanguage();

            InitPermission();
        }

        /// <summary>
        /// 初始化 sdk 上报接口
        /// </summary>
        private void InitSDKBuglyReport()
        {
            Debug.Log($"[sdk] InitSDKBuglyReport  初始化 sdk bugly 上报接口");

            GSUnity.Call("crash", "enableExceptionHandler");
        }
        private void InitDeviceInfo()
        {
            string deveviceInfo = GSUnity.CallString(GSUnityConstants.Module.Tools, "getDeviceInfo");
            Debug.Log($"[sdk] : InitDeviceInfo: {deveviceInfo}");

            if (deveviceInfo != null && deveviceInfo != string.Empty)
            {
                sdkDeviceInfo = JsonConvert.DeserializeObject<SDKDeviceInfo>(deveviceInfo);
            }

        }

        private void InitChannelInfo()
        {
            string channelInfo = GSUnity.CallString(GSUnityConstants.Module.Tools, "getChannelInfo");
            Debug.Log($"[sdk] : InitChannelInfo: {channelInfo}");

            if (channelInfo != null && channelInfo != string.Empty)
            {
                sdkChannelInfo = JsonConvert.DeserializeObject<SDKChannelInfo>(channelInfo);

                var appid = sdkChannelInfo.appId;
                var area = appid.Substring(1, 2);


                if (area.Equals("01"))
                {
                    isMainLand = true;
                }
                else
                {
                    isMainLand = false;
                }

                Debug.Log($"[sdk] : appid: {appid} , area: {area}, 是否是大陆 isMainLand: {isMainLand} ");
            }

        }

        private void InitSDKLanguage()
        {
            // 马斌斌 说这个接口已经不维护了

            // sdkLanguage = GSDevice.GetLanguage();

            sdkLanguage = sdkDeviceInfo == null ? "zh" : sdkDeviceInfo.language;
            Debug.Log($"[sdk] : InitSDKLanguage: {sdkLanguage}");
        }

        private void InitPermission()
        {
            Debug.Log($"[sdk] : InitPermission 准备初始化权限");
            string readStorage = "android.permission.READ_EXTERNAL_STORAGE";


            List<Dictionary<string, object>> permissionList = new();

            // 请求读的权限
            if (!CheckPermission(readStorage))
            {
                permissionList.Add(new Dictionary<string, object> {
                    { "permission_name", readStorage}
                });
            }

            string writeStorage = "android.permission.WRITE_EXTERNAL_STORAGE";

            // 请求写的权限
            if (!CheckPermission(writeStorage))
            {
                permissionList.Add(new Dictionary<string, object> {
                    { "permission_name", writeStorage}
                });
            }

            if (permissionList.Count == 0)
            {
                return;
            }

            RequestPermissionList(permissionList);

        }

        public string GetChannelID()
        {
            if (sdkChannelInfo == null)
            {
                return "999";
            }
            return sdkChannelInfo.channelId;
        }


        public E_LoginCfgEnum GetLoginCfgEnum()
        {

            if (sdkChannelInfo != null)
            {
                switch (sdkChannelInfo.channelId)//GooglePlay全球之后很可能是
                {
                    case "320184":
                        {
                            // Googleplay-菲律宾
                            return E_LoginCfgEnum.Philippines;
                        }
                        break;
                    case "310221":
                        {
                            //Googleplay-罗马尼亚
                            return E_LoginCfgEnum.Romania;
                        }
                        break;

                    default: return E_LoginCfgEnum.Star_Dev;
                }

            }
#if STAR_DEV
            return E_LoginCfgEnum.Star_Dev;
#else
            return E_LoginCfgEnum.Philippines;
#endif

        }





        private void InitPostServerSDKData()
        {
            Debug.Log($"[sdk] : InitPostServerSDKData");

            postServerSDKData.one_id = sdkDeviceInfo.soneid;
            postServerSDKData.device_id = sdkDeviceInfo.device_id;
            postServerSDKData.app_package_name = sdkDeviceInfo.appPackageName;
            postServerSDKData.device_model = sdkDeviceInfo.deviceModel;
            postServerSDKData.os_version = sdkDeviceInfo.os_version;
            postServerSDKData.cpsid = sdkDeviceInfo.cpsid;
            postServerSDKData.language = sdkDeviceInfo.language;
            postServerSDKData.channel = sdkChannelInfo.channelId;
            postServerSDKData.os_type = sdkDeviceInfo.os_type;
            postServerSDKData.app_version = sdkDeviceInfo.appVersionName;
            postServerSDKData.app_version_code = sdkDeviceInfo.appVersionCode;
            postServerSDKData.user_id = sdkDeviceInfo.user_id;
            postServerSDKData.yoka_id = sdkDeviceInfo.yoka_id;
            postServerSDKData.user_type = sdkDeviceInfo.user_type;

            string str = Newtonsoft.Json.JsonConvert.SerializeObject(postServerSDKData);
            Debug.Log($"[sdk] : InitPostServerSDKData {postServerSDKData} , ===> {str}");

        }


        public void Login()
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log("[sdk] 登录开始");
            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.Login);
        }

        public void SwitchAccount()
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log("[sdk] 切换账号开始");
            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.SwitchAccount);
        }

        public bool HasHelpCenter()
        {
            // 是否有客服功能
            bool hasUserCenter = GSUnity.CallBool("help", "hasHelpCenter");
            if (hasUserCenter)
            {
                // 游戏设置中，显示打开客服按钮
                return true;
            }
            else
            {
                // 游戏设置中，隐藏打开客服按钮
                return false;
            }
        }

        public void OpenCustomeService()
        {
            if (!IsInit)
            {
                return;
            }
            // Debug.Log("[sdk] 打开客服");
            string url = GameConfig.URL_THE_STAR_CUS_SERVICE;
            Debug.Log($" [sdk] [客服] open Service: {url}");
            SDKManager.Instance.RequestOpenWebView(url);
            // GSUnity.Call("help", "openHelpCenter");
            // Application.OpenURL(url);
        }

        public void LoginOut()
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log("[sdk] 注销账号开始");

            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.Logout);
        }

        public void Exit()
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log("[sdk] 准备退出游戏");
            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.ExitApp);
        }

        // 
        public void BindIDCard()
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log("[sdk] 开始SDK 绑定");
            // note: 海外sdk 没有实名认证, 所以bind 不再是绑定身份证, 而是 绑定 fb 账号等等. 
            GSUnity.Call(GSUnityConstants.Module.Platform, GSUnityConstants.Func.Platform.BindAccount);
        }

        public void Pay()
        {
            Debug.Log("[sdk] 开始 执行支付逻辑");

            // TopPayParams topPayParams = new();

        }

        /// <summary>
        /// 查询 订单信息
        /// </summary>
        /// <param name="products">订单 列表</param>
        public void QueryPruductDetails(List<string> products)
        {
            Debug.Log("[sdk] 开始 查询订单 信息");

        }


        /// <summary>
        /// 上报 bugly 日志 一些异常日志
        /// </summary>
        public void ReportBuglyLog(Dictionary<string, object> logInfo)
        {
            if (!IsInit)
            {
                return;
            }

            GSUnity.Call("crash", "reportException", logInfo);
        }

        private Dictionary<string, string> extra = new();
        private Dictionary<string, object> info = new();


        /// <summary>
        /// 上报自定义事件类型的接口, 包含参数
        /// </summary>
        public void PostEventCustom(string eventKey, Dictionary<string, string> parameters)
        {

            if (parameters == null)
            {
                parameters = new();
            }
            var roleID = StarProject.Service.User.UserManager.Instance.MainUserData.playerRoleId;
            if (roleID != 0 && !parameters.ContainsKey("roleId"))
            {
                parameters.Add("roleId", roleID.ToString());
            }

            Debug.Log($"[SDK] 上报自定义事件 PostEventCustom: {JsonConvert.SerializeObject(parameters)} ");
            if (!IsInit)
            {
                return;
            }

            info.Clear();
            info.Add("type", "0");
            info.Add("event", "100001");     // 必传；自定义打点的event从10000 起往后添加
            info.Add("eventName", "CLICK_SIGNINWINDOW");
            info.Add("extra", parameters);   //必传

            Debug.LogWarning($"[SDK] 上报 上报自定义事件 eventKey: {eventKey} , info {info} ");

            GSUnity.Call(GSUnityConstants.Module.Analytics, GSUnityConstants.Func.Analytics.Track, info);
        }

        public void PostPriceEvent(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                parameters = new();
            }
            Debug.Log($"[SDK] 上报 price 事件 PostPriceEvent: {JsonConvert.SerializeObject(parameters)} ");
            if (!IsInit)
            {
                return;
            }


            info.Clear();
            info.Add("type", "0");
            info.Add("event", "90001");     // 必传；自定义打点的event从10000 起往后添加
            info.Add("eventName", "eventPurchase");
            info.Add("extra", parameters);   //必传

            Debug.LogWarning($"[SDK] 上报 price  , info {JsonConvert.SerializeObject(info)} ");

            GSUnity.Call(GSUnityConstants.Module.Analytics, GSUnityConstants.Func.Analytics.Track, info);
        }

        /// <summary>
        /// 预处理事件打点
        /// </summary>
        /// <param name="stepId">E_SDK_PreEvent 枚举</param>
        public void EventPreEvent(int stepId, string extraStr = "")
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log($"[SDK] 上报 预处理事件打点 EventPreEvent: {stepId} ");

            extra.Clear();
            extra.Add("stepId", stepId.ToString());
            //extra.Add("stepName", E_SDK_PreEvent.GetStepDesc(stepId));

            info.Clear();
            info.Add("FPS", ShowFps.FPS);
            info.Add("ping", TimeManager.Instance.AveragePing);

            extra.Add("stepName", JsonMapper.ToJson(info));

            info.Clear();
            info.Add("type", "1");      // 必传，前置事件的type固定为 1
            info.Add("extra", extra);   //必传

            Debug.LogWarning($"[SDK] 上报 预处理事件打点 EventPreEvent: {stepId} , info {JsonMapper.ToJson(info)} ");

            GSUnity.Call(GSUnityConstants.Module.Analytics, GSUnityConstants.Func.Analytics.Track, info);
        }

        /// <summary>
        /// 打点接口
        /// </summary>
        /// <param name="stepId"></param>
        /// <param name="typeID">1: 进游戏前的前置埋点, 2 进游戏收的新手引导埋点</param>
        public void EventPreNewPlayerEvent(string stepId, int typeID = 2)
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log($"[SDK] 上报 预处理事件打点 EventPreEvent: {stepId} ");

            extra.Clear();
            extra.Add("stepId", stepId.ToString());

            info.Clear();
            info.Add("FPS", ShowFps.FPS);
            info.Add("ping", TimeManager.Instance.AveragePing);

            extra.Add("stepName", JsonMapper.ToJson(info));

            info.Clear();
            info.Add("type", typeID.ToString());      // 必传，前置事件的type固定为 1
            info.Add("extra", extra);   //必传

            Debug.LogWarning($"[SDK] 上报 预处理事件打点 EventPreEvent: {stepId} , info {JsonMapper.ToJson(info)} ");

            GSUnity.Call(GSUnityConstants.Module.Analytics, GSUnityConstants.Func.Analytics.Track, info);
        }

        /// <summary>
        /// 新手打点， TODO
        /// </summary>
        /// <param name="stepId"></param> 
        public void PostNewPlayerEvent(int stepId)
        {
            if (!IsInit)
            {
                return;
            }
            Debug.Log($"[SDK] 上报 新手引导事件大带你 EventPreEvent: {stepId} ");

            extra.Clear();
            extra.Add("stepId", stepId.ToString());
            // TODO 新手打点
            // extra.Add("stepName",E_SDK_PreEvent.GetStepDesc(stepId));;


            info.Clear();
            info.Add("FPS", ShowFps.FPS);
            info.Add("ping", TimeManager.Instance.AveragePing);

            extra.Add("stepName", JsonMapper.ToJson(info));

            info.Clear();
            info.Add("type", "2");      // 必传，前置事件的type固定为 1
            info.Add("extra", extra);   //必传

            Debug.LogWarning($"[SDK] 上报 预处理事件打点 EventPreEvent: {stepId} , info {JsonMapper.ToJson(info)} ");

            GSUnity.Call(GSUnityConstants.Module.Analytics, GSUnityConstants.Func.Analytics.Track, info);

        }

        /// <summary>
        /// 支付相关打点
        /// </summary>
        /// <param name="gsEvent"></param>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <param name="money"></param>
        /// <param name="currency"></param>
        public static void EventWithPurchase(GSEvent gsEvent, string orderId, string productId, string money, string currency)
        {
            if (!SDKManager.Instance.IsInit)
            {
                return;
            }

            GSAnalytics.EventWithPurchase(gsEvent, orderId, productId, money, currency);
        }

        /// <summary>
        /// 检查权限是否授权
        /// </summary>
        public bool CheckPermission(string androidPermissionKey)
        {
            info.Clear();
            info.Add("permission_name", androidPermissionKey);

            bool hasPermission = GSUnity.CallBool("permission", "checkPermission", info); ;

            Debug.Log($"[sdk] : 检查权限 CheckPermission: {androidPermissionKey} , hasPermission: {hasPermission}");

            return hasPermission;
        }

        /// <summary>
        /// 请求 权限
        /// </summary>
        public void RequestPermission(string androidPermissionKey)
        {
            info.Clear();
            info.Add("permission_name", androidPermissionKey);
            Debug.LogWarning($"[SDK] 请求权限:  {androidPermissionKey}, info: {info}");

            GSUnity.Call("permission", "request", info);
        }

        public void RequestPermissionList(List<Dictionary<string, object>> permissionList)
        {
            info.Clear();
            info.Add("permissions", permissionList);
            Debug.LogWarning($"[SDK] 请求权限:  , info: {info}");

            GSUnity.Call("permission", "request", info);
        }
        /// <summary>
        /// 内置浏览器
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="animation"></param>
        /// <param name="navigationBarType"></param>
        public void RequestOpenWebView(string url, string title = "", string animation = "1", string navigationBarType = "3")
        {
            Debug.LogWarning($"[SDK] 请求权限:  {url}, info: {url}");
            Dictionary<string, object> parameters = new()
            {
                { "navigationBarType", navigationBarType },
                { "url", url },
                { "animation", animation },
                { "title", title }
             };
            GSUnity.Call("web", "openWebView", parameters);
        }


        // 定时获取电量和网络状态信息
        public SDKDeviceInfo GetDeviceInfo()
        {
            if (IsInit == false)
            {
                return null;
            }

            InitDeviceInfo();
            return sdkDeviceInfo;
        }
    }

    /// <summary>
    /// gssdk 登录后 返回的结构
    //    {
    //     "code": 0,
    //     "data": {
    //         "userId": "10001_310153892574",
    //         "isAdult": 1,
    //         "userName": "310153892574",
    //         "userNick": "",
    //         "channelId": 410004,
    //         "appId": 10100078,
    //         "isGuest": 0,
    //         "extension": "",
    //         "sign": "4306d3162d2be360d193f23b13f11972",
    //         "userAge": 0,
    //         "realNameStatus": 1
    //     },
    //     "msg": "success",
    //     "dataType": 0
    //    }
    /// </summary>
    public class SKDLoginResult
    {
        public int code;
        public string dataString;
        private SDKLoginData data;
        public string msg;
        public int dataType;

        private string _channelID;

        public string ChannelID
        {
            get
            {
                return _channelID;
            }
        }
        private string _sdkUserName;
        public string SdkUserName
        {
            get
            {
                return _sdkUserName;
            }
        }

        private string _userId;
        public string userId
        {
            get
            {
                return _userId;
            }
        }
        private int _userAge;

        public int userAge
        {
            get
            {
                return _userAge;
            }
        }

        public bool _isRealName = false;
        public bool IsRealName => _isRealName;

        public bool _isAdult = false;
        public bool IsAdult => _isAdult;

        /// <summary>
        /// sdk 服务器返回的 json 结构有时候会修改, 客户端没办法按一个完全定义好的类型来解析;
        /// </summary>
        /// <param name="json"></param>
        public SKDLoginResult(string json)
        {
            var jsonObj = JObject.Parse(json);

            Debug.Log($"[sdk] SKDLoginResult 准备 parse json: {jsonObj}");

            if (jsonObj["code"] != null)
            {
                code = (int)jsonObj["code"];
            }

            if (jsonObj["msg"] != null)
            {
                msg = jsonObj["msg"].ToString();
            }

            if (jsonObj["dataType"] != null)
            {
                dataType = (int)jsonObj["dataType"];
            }

            dataString = jsonObj["data"].ToString();

            data = new SDKLoginData(dataString);

            _channelID = data.channelId;

            _sdkUserName = data.userName;
            _userId = data.userId;
            _userAge = data.userAge;

            _isRealName = data.IsRealName;
            _isAdult = data.IsAdult;
        }
    }

    public class SDKLoginData
    {
        public string userId;
        public string userName;
        public string userNick;
        public int realNameStatus;
        public int isAdult;
        public int isGuest;
        public string extension;
        public string sign;
        public string appId;
        public string channelId;
        public int userAge;

        // public string antiAddiction;

        public bool IsRealName;
        public bool IsAdult;

        public SDKLoginData(string json)
        {
            var jsonObj = JObject.Parse(json);

            Debug.Log($"[sdk] SDKLoginData 准备 parse json: {jsonObj}");

            if (jsonObj["userId"] != null)
            {
                userId = jsonObj["userId"].ToString();
            }

            if (jsonObj["userName"] != null)
            {
                userName = jsonObj["userName"].ToString();
            }

            if (jsonObj["userNick"] != null)
            {
                userNick = jsonObj["userNick"].ToString();
            }

            if (jsonObj["realNameStatus"] != null)
            {
                realNameStatus = (int)jsonObj["realNameStatus"];
            }

            if (jsonObj["isAdult"] != null)
            {
                isAdult = (int)jsonObj["isAdult"];
            }

            if (jsonObj["isGuest"] != null)
            {
                isGuest = (int)jsonObj["isGuest"];
            }

            if (jsonObj["extension"] != null)
            {
                extension = jsonObj["extension"].ToString();
            }

            if (jsonObj["sign"] != null)
            {
                sign = jsonObj["sign"].ToString();
            }

            if (jsonObj["appId"] != null)
            {
                appId = jsonObj["appId"].ToString();
            }

            if (jsonObj["channelId"] != null)
            {
                channelId = jsonObj["channelId"].ToString();
            }


            if (jsonObj["userAge"] != null)
            {
                userAge = (int)jsonObj["userAge"];
            }

            IsRealName = realNameStatus == 1;
            IsAdult = isAdult == 1;
        }
    }

    public class PostServerSDKData
    {
        /// <summary>
        /// 设备标识
        /// </summary>
        public string one_id;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string device_id;
        /// <summary>
        /// App包体名称
        /// </summary>
        public string app_package_name;
        /// <summary>
        /// 设备型号
        /// </summary>
        public string device_model;
        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string os_version;
        /// <summary>
        /// 渠道分包ID
        /// </summary>
        public string cpsid;
        /// <summary>
        /// 语言
        /// </summary>
        public string language;
        /// <summary>
        /// 用户渠道
        /// </summary>
        public string channel;
        /// <summary>
        /// 操作系统
        /// </summary>
        public string os_type;
        /// <summary>
        /// App版本(与客户端一致)
        /// </summary>
        public string app_version;
        /// <summary>
        /// App版本CODE(细分版本)
        /// </summary>
        public string app_version_code;
        /// <summary>
        /// GSSDK账号ID
        /// </summary>
        public string user_id;
        /// <summary>
        /// 渠道测用户ID
        /// </summary>
        public string yoka_id;
        /// <summary>
        /// 账号类型\注册方式等(微信注册/手机注册/游客登陆等)
        /// </summary>
        public string user_type;

    }

    /// <summary>
    /// sdk 返回的渠道数据信息
    /// 
    /// {
    //     "appId": "10100078",
    //     "humeChannel": "",
    //     "appKey": "e098801642b2594ce64dce3966a9d811",
    //     "subChannel": "",
    //     "channelId": 410004,
    //     "channelVersion": ""
    // }
    /// InitChannelInfo: {"appId":"10100078","subChannel":"","channel":"GSSDK","humeChannel":"","appKey":"e098801642b2594ce64dce3966a9d811","channelVersion":"1","channelId":"310003"}
    /// </summary>
    public class SDKChannelInfo
    {
        public string appId;
        public string humeChannel;
        public string appKey;
        public string channelId;
    }

    /// <summary>
    /// sdk 返回的 DeviceInfo
    //{
    //    "coneid": "fa89082784539288",
    //    "isEmulator": "false",
    //    "yoka_id": "210424217296",
    //    "language": "zh",
    //    "appTargetSdkVersion": "32",
    //    "appVersionCode": "1",
    //    "sdkVersionCode": "34",
    //    "manufacturer": "Xiaomi",
    //    "isDeviceRooted": "false",
    //    "user_type": "GSSDK",
    //    "isTablet": "false",
    //    "idfv": "",
    //    "isConnectVPN": "false",
    //    "androidId": "86777d656f26c69c",
    //    "sdkVersionName": "14",
    //    "soneid": "os_android_ec8f0dcc-0bff-4aaf-abdd-6118d69bc229",
    //    "appMinSdkVersion": "22",
    //    "device_id": "U-29a09ca6-d8a5-477e-ba2d-03e8dae8fe52",
    //    "appName": "gssdk-unity",
    //    "cpsid": "1001310212",
    //    "idfa": "",
    //    "os_version": "34",
    //    "appVersionName": "1.0",
    //    "user_id": "10001_210424217296",
    //    "os_type": "android",
    //    "appPackageName": "com.DefaultCompany.gssdkunity",
    //    "net_type":"wifi",
    //    "battery_capacity":"55",
    //    "deviceModel": "23116PN5BC"
    //}
    /// </summary>
    [XLua.LuaCallCSharp]
    public class SDKDeviceInfo
    {
        public string coneid;
        public bool isEmulator;
        public string yoka_id;
        public string language;
        public string appTargetSdkVersion;
        public string appVersionCode;
        public string sdkVersionCode;
        public string manufacturer;
        public string isDeviceRooted;
        public string user_type;
        public bool isTablet;
        public string idfv;
        public string isConnectVPN;
        public string androidId;

        public string sdkVersionName;
        public string soneid;
        public string appMinSdkVersion;

        public string device_id;
        public string appName;
        public string cpsid;
        public string idfa;
        public string os_version;
        public string appVersionName;
        public string user_id;
        public string os_type;
        public string appPackageName;
        public string deviceModel;

        public string net_type; // 网络类型
        public string battery_capacity; // 电池电量 真机：【常规电量范围0-100】 模拟器：【-100】
    }
}