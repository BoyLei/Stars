using System.Collections.Generic;

namespace StarProject.Service.SDK
{
    /// <summary>
    /// sdk 预处理打点枚举
    /// </summary>
    [XLua.LuaCallCSharp]
    public class E_SDK_PreEvent
    {
        public const int INIT_SDK = 10;//初始化 SDK。

        public const int BEGIN_LOCAL_FILE = 20;//开始校验本地文件
        public const int BEGIN_LOCAL_FILE_5 = 22;//加载5s
        public const int BEGIN_LOCAL_FILE_10 = 24;//加载10s
        public const int BEGIN_LOCAL_FILE_15 = 26;//加载15s
        public const int BEGIN_LOCAL_FILE_20 = 28;//加载20s


        public const int END_LOCAL_FILE = 30;//完成校验本地文件

        public const int NO_UPDATE_CONTENT = 40;//表示没有可用的内容更新。

        public const int YES_UPDATE_CONTENT = 50;//表示有可用的内容更新。
        public const int BEGIN_CDN = 60;//开始 CDN 下载
        public const int SUC_CDN = 70;//CDN 操作成功。
        public const int TIP_SIZE_UPDATE = 80;//提示更新大小
        public const int UPDATE_FAIL = 90;//更新失败。
        public const int UPDATE_SUC = 100;//更新成功。
        public const int PRE_LOAD_GLOBAL_MODULE = 105;//加载Moules
        public const int PRE_LOAD_GLOBAL_MODULE_FINISH = 106;//加载Moules完成
        public const int PER_LOAD_ADDRESSABLE = 107;//加载配置
        public const int PER_LOAD_ADDRESSABLE_FINISH = 108;//加载配置

        public const int LOAD_SCENE_RESOURCE = 110;//加载场景资源。
        public const int LOAD_SCENE_RESOURCE_SUC = 115;//场景资源加载成功。

        public const int SHOW_ANNOUNCEMENT = 120;//显示公告。
        public const int SHUT_ANNOUNCEMENT = 130;//关闭公告。

        public const int CHK_PRIVACY_1 = 140;//勾选用户协议和隐私条例 1

        public const int CLICK_ENTER_GAME = 150;//点击"Enter Game"登录游戏

        public const int CHK_PRIVACY_2 = 152;//提示勾选用户协议 2

        public const int CLICK_SERVER_AREA = 154;//点击选择区服
        public const int CLICK_RETURN_LOGIN = 156;//点击返回进入游戏界面

        public const int CLICK_SERVER_AREA_ENTER_GAME = 158;//点击区服登录游戏

        public const int CLICK_STAR_ENTER_GAME = 160;//点击"Start"进入游戏

        public const int CLICK_CONFIRM_ENTER_GAME = 170;//输入昵称点击"Confirm"进入游戏

        public const int LOGIN_SUC = 180;//登录成功。
        public const int LOGIN_FAIL = 185;//登录失败。

        public const int CLICK_SIGNINWINDOW = 200;//点击冒险旅程

        public const int SHOW_MOBILE_WINDOW = 1900;//显示手机窗口。
        public const int SHUT_MOBILE_WINDOW = 2000;//关闭手机窗口。
        public const int INPUT_MOBILE = 2100;//输入手机号。
        public const int CLICK_VERIFICATION_CODE = 2200;//点击验证码。
        public const int INPUT_VERIFICATION_CODE = 2300;//输入验证码。
        public const int SHOW_REALNAME_WINDOW = 2400;//显示实名窗口。
        public const int SHUT_REALNAME_WINDOW = 2500;//关闭实名窗口。
        public const int INPUT_REALNAME = 2600;//输入真实姓名。
        public const int REALNAME_VERIFICATION_FAIL = 2700;//实名认证失败。
        public const int REALNAME_VERIFICATION_SUC = 2800;//实名认证成功。
        public const int NET_WORK_ERROR_1 = 999999;//远程配置拉取失败。

        private static Dictionary<int, string> stepDesc = new();

        public static string GetStepDesc(int stepId)
        {
            InitStepDesc();

            if (stepDesc.TryGetValue(stepId, out var v))
            {
                return v;
            }

            return "";
        }

        private static void InitStepDesc()
        {
            if (stepDesc.Count > 0)
            {
                return;
            }

            stepDesc.Add(INIT_SDK, "初始化 SDK。");

            stepDesc.Add(BEGIN_LOCAL_FILE, "开始校验本地文件");
            stepDesc.Add(BEGIN_LOCAL_FILE_5, "加载5s");
            stepDesc.Add(BEGIN_LOCAL_FILE_10, "加载10s");
            stepDesc.Add(BEGIN_LOCAL_FILE_15, "加载15s");
            stepDesc.Add(BEGIN_LOCAL_FILE_20, "加载20s");


            stepDesc.Add(END_LOCAL_FILE, "完成校验本地文件");

            stepDesc.Add(NO_UPDATE_CONTENT, "表示没有可用的内容更新。");
            stepDesc.Add(YES_UPDATE_CONTENT, "表示有可用的内容更新。");

            stepDesc.Add(BEGIN_CDN, "开始 CDN 下载");
            stepDesc.Add(SUC_CDN, "CDN 操作成功。");
            stepDesc.Add(TIP_SIZE_UPDATE, "提示更新大小");
            stepDesc.Add(UPDATE_FAIL, "更新失败。");
            stepDesc.Add(UPDATE_SUC, "/更新成功。");

            stepDesc.Add(PRE_LOAD_GLOBAL_MODULE, "/加载Moules。");
            stepDesc.Add(PRE_LOAD_GLOBAL_MODULE_FINISH, "/加载Moules完成。");
            stepDesc.Add(PER_LOAD_ADDRESSABLE, "/加载配置。");
            stepDesc.Add(PER_LOAD_ADDRESSABLE_FINISH, "/加载配置完成。");

            stepDesc.Add(LOAD_SCENE_RESOURCE, "/加载场景资源。");
            stepDesc.Add(LOAD_SCENE_RESOURCE_SUC, "/场景资源加载成功。");

            stepDesc.Add(SHOW_ANNOUNCEMENT, "/显示公告。");
            stepDesc.Add(SHUT_ANNOUNCEMENT, "/关闭公告。");

            stepDesc.Add(CHK_PRIVACY_1, "/勾选用户协议和隐私条例 1");

            stepDesc.Add(CLICK_ENTER_GAME, "/点击 Enter Game 登录游戏");

            stepDesc.Add(CHK_PRIVACY_2, "/提示勾选用户协议 2");

            stepDesc.Add(CLICK_SERVER_AREA, "/点击选择区服");
            stepDesc.Add(CLICK_RETURN_LOGIN, "/点击返回进入游戏界面");

            stepDesc.Add(CLICK_SERVER_AREA_ENTER_GAME, "/点击区服登录游戏");

            stepDesc.Add(CLICK_STAR_ENTER_GAME, "/点击 Start 进入游戏");

            stepDesc.Add(CLICK_CONFIRM_ENTER_GAME, "/输入昵称点击 Confirm 进入游戏");

            stepDesc.Add(LOGIN_SUC, "/登录成功。");
            stepDesc.Add(LOGIN_FAIL, "/登录失败。");

            stepDesc.Add(CLICK_SIGNINWINDOW, "/点击冒险旅程");
            
            stepDesc.Add(SHOW_MOBILE_WINDOW, "//显示手机窗口。");
            stepDesc.Add(SHUT_MOBILE_WINDOW, "//关闭手机窗口。");
            stepDesc.Add(INPUT_MOBILE, "//输入手机号。");
            stepDesc.Add(CLICK_VERIFICATION_CODE, "//点击验证码。");
            stepDesc.Add(INPUT_VERIFICATION_CODE, "//输入验证码。");
            stepDesc.Add(SHOW_REALNAME_WINDOW, "//显示实名窗口。");
            stepDesc.Add(SHUT_REALNAME_WINDOW, "//关闭实名窗口。");
            stepDesc.Add(INPUT_REALNAME, "//输入真实姓名。");
            stepDesc.Add(REALNAME_VERIFICATION_FAIL, "//实名认证失败。");
            stepDesc.Add(REALNAME_VERIFICATION_SUC, "//实名认证成功。");
            stepDesc.Add(NET_WORK_ERROR_1, "//远程拉取失败");
        }
    }
}