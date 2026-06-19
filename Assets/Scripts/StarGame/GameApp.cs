///--------------------------------------------------------------------
/// 文件名   :   GameApp.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/01 17:56:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif
using UnityEngine;


[CreateAssetMenu(menuName = "Assets/Create GameApp")]
public class GameApp : ScriptableObject
{

    private IEnumerable _channels = new ValueDropdownList<E_Channel>()
        {
            { "游卡", E_Channel.Yoka },
            { "版署", E_Channel.BanShu },
            { "腾讯", E_Channel.Tencent },
        };

    private IEnumerable _publishs = new ValueDropdownList<E_Publish>()
        {
            { "开发", E_Publish.DEV },
            { "测试", E_Publish.BETA },
            { "发布", E_Publish.RELEASE },
        };

    private IEnumerable _languageType = new ValueDropdownList<LanguageType>()
        {
            { "中文", LanguageType.Chinese},
            { "英文", LanguageType.English},
        };

    [LabelText("国内QA配置下载地址")]//Qa是beta；正式是release
    public string ConfigUrl;
    [LabelText("国内QA白名单IMEA")]//Qa是beta；正式是release
    public string ConfigUrlIMEA;

    [LabelText("欧洲QA配置下载地址")]
    public string EURConfigUrl;
    [LabelText("欧洲QA白名单IMEA")]//这个东西各个地址都需要自己的白名单，包括本地xml文件，imea模拟器能改，手机我帮你加
    public string EURConfigUrlIMEA;


    [LabelText("菲律宾QA配置下载地址")]
    public string SEAConfigUrl;
    [LabelText("菲律宾QA白名单IMEA")]
    public string SEAConfigUrlIMEA;

    [NonSerialized]
    [LabelText("渠道")]
    [ValueDropdown("_channels")]
    public E_Channel channel;

    [NonSerialized]
    [LabelText("环境")]
    [ValueDropdown("_publishs")]
    public E_Publish publish;

    [LabelText("默认语言")]
    [ValueDropdown("_languageType")]
    public LanguageType languageType = LanguageType.Chinese;

    [LabelText("运行平台")]
    public E_BuildTarget buildtarget;
    
    [LabelText("打包目录")]
    public string BuildDir;

    [LabelText("包体目录")]
    [FolderPath]
    public string PackDir;

    [LabelText("是否启动热更")]
    public bool IsUseUpdate;
    
    
    public bool CanUpdate
    {
        get
        {
#if UNITY_EDITOR
            return false;
#else
           return IsUseUpdate;
#endif
        }
    }
    public int Main { get { return _version.Main; } }

    /// <summary>
    /// 子版本号
    /// </summary>
    public int Tiny { get { return _version.Tiny; } }

    /// <summary>
    /// Fix版本
    /// </summary>
    public int Res { get { return _version.Res; } }

    /// <summary>
    ///
    /// </summary>

    //环境.Main.Code.Res
    public string Version { get { return $"{(int)publish}.{Main}.{Tiny}.{Res}"; } }

    /// <summary>
    ///  GoogleBundleCode，不包含在版本号里面中。打包的tab-name和我们自己管理的版本version都跟这个没关系。
    ///  这个版本号是上传谷歌版本号，这个号确实需要更高，且每次变化：
    ///  打包那块没bundleCode，也不应该有这个号关联，所以他不包含再version中，这个是涉及谷歌审核也有一种包体覆盖，但是不包含再我们版本管理，所以也不会放在版本号中，也不会影响发包工具，一切版本相关都跟这个无关。
    /// </summary>
    public int BundleCode { get { return _version.BundleCode; } }

    [ReadOnly]
    [LabelText("版本号")]
    public GameVersion _version;


    private static GameApp gameApp;
    public static GameApp Instance
    {
        get
        {
            if (gameApp == null)
            {
                gameApp = Resources.Load<GameApp>("GameApp");
                gameApp.Init();
            }
            
            return gameApp;
        }
    }

    public GameVersion LocalVersion;

    public GameVersion GetGameVersion()
    {
        return _version;
    }

    /// <summary>
    /// 重置版本号
    /// </summary> 
    [Button("取versionJson的值")]
    public void EditorInitVersion()
    {
        //安卓临时打开
        //_version = new GameVersion(1,1);
        //return;
        string path = Application.streamingAssetsPath + "/Version.json";
        string content = "";
        if (LoadVersion(path, out content))
        {
            _version = Newtonsoft.Json.JsonConvert.DeserializeObject<GameVersion>(content);

        }
        else
        {
            _version = new GameVersion(1, 1);
           // content = Newtonsoft.Json.JsonConvert.SerializeObject(_version);
           // WriteJson(content, path);
        }
    }

    public string GetVersion()
    {
        if (_version == null)
        {
            EditorInitVersion();
        }
        return _version.String().Replace("version_", string.Empty).Replace("_", ".");
    }

    //渠道，大版本号【决定热更】，小版本号【决定热更】，资源版本号（资源，（华佗C#））


    /// <summary>
    /// 重置版本号
    /// </summary> 
    [Button("重置版本号")]
    public void ResetVersion()
    {
#if UNITY_EDITOR
        var version = GameApp.Instance.GetGameVersion();
        version.Tiny = 0;
        version.Res = 0;
        version.Main = 0;
        version.BundleCode = 0;
        SaveVesion(version, "重置版本号");
        PlayerSettings.bundleVersion = "0.0.0.0";
        PlayerSettings.Android.bundleVersionCode = 0;
#endif
    }

    /// <summary>
    /// 更新代码版本号，Addressable Build之后
    /// </summary>
    [Button("更新主版本号")]
    public void UpdateMainVersion()
    {
#if UNITY_EDITOR
        _version.Main++;//GameApp升级1
        _version.Tiny = 0;
        _version.Res = 0;
        _version.BundleCode++;
        SaveVesion(_version, "更新主版本号"); //Version.json升级2
        PlayerSettingVersion();
#endif
    }



    /// <summary>
    /// 更新中版本号，Addressable Build之后
    /// </summary>
    [Button("更新中版本号")]
    public void UpdateCodeVersion()
    {
#if UNITY_EDITOR
        _version.Tiny++;
        _version.Res = 0;
        _version.BundleCode++;
        SaveVesion(_version, "更新代码版本号");
        PlayerSettingVersion();
#endif
    }

    /// <summary>
    /// 更新资源版本号
    /// </summary> 
    [Button("更新资源版本号")]
    public void UpdateResVersion()
    {

#if UNITY_EDITOR
        _version.Res++;
        _version.BundleCode++;
        SaveVesion(_version, "更新资源版本号");//1txt写入，2本代码控制
        PlayerSettingVersion();
#endif

    }
    public void PlayerSettingVersion()
    {
#if UNITY_EDITOR
        //3，4两个version是一样的
       // PlayerSettings.bundleVersion = "0." + _version.Main + "." + _version.Tiny + "." + _version.Res;

        //上下的version是一样的


        //不能合并，因为不能变小；上限10万，不能限制最小不刷新，也不能限制最小100级，最高层到10也就崩10万了
        //5code要自增
        /*我觉得要读取*/
        PlayerSettings.Android.bundleVersionCode = _version.BundleCode;


        // 修改 AddressableAssetSettings 中的 PlayerVersionOverride
/*        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
        {
            // 找到 PlayerVersionOverride 选项
            settings.OverridePlayerVersion = "0." + _version.Main + "." + _version.Tiny + "." + _version.Res;
        }*/
#endif
    }

    public void SaveVesion(GameVersion version, string reason = "")
    {
#if UNITY_EDITOR
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(version);

        //更新到工程本地
        string path = Application.streamingAssetsPath + "/Version.json";
        WriteJson(content, path);

        if (!Directory.Exists(GameApp.Instance.BuildDir))
        {
            Directory.CreateDirectory(GameApp.Instance.BuildDir);
        }
        
        //更新到远程服务器
        string remotepath = string.Format("{0}/Version.json",
            GameApp.Instance.BuildDir);
        WriteJson(content, remotepath);

        //RunBat("TortoiseProc", $"/command:commit /path:{path} -m 修改Version {reason} /closeonend:4");
        //RunBat("TortoiseProc", $"/command:commit /path:{remotepath} -m 修改Version {reason} /closeonend:4");
#endif
    }


    private void Init()
    {
        EditorInitVersion();
    }

    private bool LoadVersion(string path, out string content)
    {
        content = string.Empty;
        if (File.Exists(path))
        {
            content = File.ReadAllText(path, System.Text.Encoding.UTF8);
            return true;
        }
        return false;

    }

    private void WriteJson(string content, string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        

        FileStream fs = new FileStream(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void RunBat(string program, string parm)
    {
        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = string.Format(parm);//this is argument
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.Log("Info:" + e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogError("Error:" + e.Data);
            };

            process.Start();
            process.StandardInput.AutoFlush = true;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
        }
    }

}

[System.Serializable]
public class GameVersion
{
    /*巨大版本号*/
    /*开发0，测试1，发布2*/
    /*public E_Publish publish;*/

    /*大版本号*/
    [ReadOnly]
    [LabelText("主版本号")]
    public int Main;

    /*中版本号 */
    [ReadOnly]
    [LabelText("中版本")]
    public int Tiny;

    //Fix 
    [ReadOnly]
    [LabelText("Fix版本")]
    public int Res;


    //跟版本没关系 
    [ReadOnly]
    [LabelText("Google Bundle Code")]
    public int BundleCode;

    public GameVersion(int code, int res)
    {
        this.Main = 0;
        this.BundleCode = 0;
        this.Tiny = code;
        this.Res = res;
    }

    public string String()
    {
        return $"version_{this.Main}_{this.Tiny}_{this.Res}";
    }

    public string Tostring()
    {
        return $"{this.Main}.{BundleCode}.{this.Tiny}.{this.Res}";
    }
}
/// <summary>
/// 渠道枚举
/// </summary>
public enum E_Channel
{
    /// <summary>
    /// 游卡渠道
    /// </summary>
    Yoka = 1,

    /// <summary>
    /// 版署
    /// </summary>
    BanShu = 2,

    /// <summary>
    /// 腾讯
    /// </summary>
    Tencent = 3,
}

public enum E_Publish
{
    /// <summary>
    /// 开发
    /// </summary>
    DEV = 1,
    /// <summary>
    /// 测试
    /// </summary>
    BETA = 2,
    /// <summary>
    /// 发布
    /// </summary>
    RELEASE = 3,
}

public enum E_BuildTarget
{
    Windows = 0,
    Android = 1,
    IOS = 2,
}