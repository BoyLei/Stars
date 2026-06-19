///--------------------------------------------------------------------
/// 文件名   :   BuildMenu.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/13 09:50:09
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using CSObjectWrapEditor;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System;
using Debug = UnityEngine.Debug;

public class BuildMenu
{

    [MenuItem("Build/发包")]
    public static void OpenBuildWindow()
    {
        BuildWindow.OpenWindow();
    }

    [MenuItem("Build/编译代码")]
    public static void BuildScript()
    {
        foreach (var item in  StarsXLuaConfig.LuaCallCSharp_NameSpace)
        {
            Debug.Log(item.FullName);
        }
       // BuildPack.BuildScript();
    }

    [MenuItem("Build/资源/构建资源")]
    public static bool BuildAddressables()
    {
        FreshAssetGroup();
        return BuildRes.BuildAddressables();
    }
    
    [MenuItem("Build/资源/异步刷新Addressable分组")]
    public static void FreshAssetGroupAync()
    {
        AssestConfig.Instance.FreshGroupAync();
    }
    
    [MenuItem("Build/资源/刷新Addressable分组")]
    public static void FreshAssetGroup()
    {
        BuildRes.SetUseAssetDataBase();
        AssestConfig.Instance.SetAddressableLocal();
        BuildRes.FreshAssetGroup();
        //Log(" 开始 生成 AssetsToDefine : ");
        AssetsToDefine.GenerateAssetsToDefine();
        //Log(" 生成 AssetsToDefine  成功 ");
        UIQueueToDefine.GenerateUIQueueToDefine();
    }

    [MenuItem("Build/资源/刷新Addressable设置")]
    public static void FreshAssetSetting()
    {
        BuildRes.FreshAssetSetting();
    }

    [MenuItem("Build/资源/清理Addressable分组")]
    public static void ClearAddressablesGroup()
    {
        BuildRes.ClearAddressablesGroup();
    }

    public static void BuildCurrent()
    {
        BuildPack.IsSendXiaoShan = false;
        BuildPack.BuildCurrent();
    }

    //[MenuItem("Build/发包/一键Android包")]
    //public static void BuildAndroid()
    //{
    //    BuildPack.IsSendXiaoShan = false;
    //    BuildPack.BuildAndroid();
    //}

    //[MenuItem("Build/发包/一键Windows")]
    //public static void BuildWindows()
    //{
    //    BuildPack.IsSendXiaoShan = false;
    //    BuildPack.BuildWindows();
    //}

    //[MenuItem("Build/发包/一键IOS")]
    //public static void BuildIOS()
    //{
    //    //System.Environment.GetCommandLineArgs().Add("-datatime");
    //    //string data = System.DateTime.Now.ToString("yyyyMMddHHmm");
    //    //System.Environment.GetCommandLineArgs().Add(data);
    //}

    [MenuItem("Build/配置/更新Excel")]
    public static void UpdateExcel()
    {
        StarClient.UpdateExcel();
    }

    [MenuItem("Build/配置/Excel转表")]
    public static void ExecuteExcel()
    {
        StarClient.ExecuteExcel();
    }

    [MenuItem("Build/配置/更新Excel&&转表")]
    public static void UpdateExecuteExcel()
    {
        StarClient.UpdateExcel();
        StarClient.ExecuteExcel();
    }

    [MenuItem("Build/协议/更新协议")]
    public static void UpdateProtoBuff()
    {
        StarClient.UpdateProtoBuff();
    }

    [MenuItem("Build/协议/转协议")]
    public static void ExecuteProtoBuff()
    {
        StarClient.ExecuteProtoBuff();
    }

    [MenuItem("Build/协议/更新协议&&转协议")]
    public static void UpdateExecuteProtoBuff()
    {
        StarClient.UpdateProtoBuff();
        StarClient.ExecuteProtoBuff();
    }

    //普通不用编译，打包用点这个
    [MenuItem("Build/MessagePack/GenerateCode(il2cpp+android+.net6.0sdkrt)")]
    public static void MsgPackGenerateCode()
    {
        var commnadLineArguments = "-i ..\\Assets -o Scripts/StarGame/Service/LocalDataManager/MessagePackGen/GeneratedResolver.cs";
        UnityEngine.Debug.Log("Generate MessagePack Files, command:" + commnadLineArguments);
        MsgProcessHelper.InvokeProcessStartAsync("mpc", commnadLineArguments);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }

    internal static class MsgProcessHelper
    {
        public static void InvokeProcessStartAsync(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Application.dataPath
            };

            Process p;
            try
            {
                p = Process.Start(psi);
                p.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.Log("Info:" + e.Data);
                };

                p.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.LogError("Error:" + e.Data);
                };

                p.WaitForExit();
                p.Close();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }
    }
}