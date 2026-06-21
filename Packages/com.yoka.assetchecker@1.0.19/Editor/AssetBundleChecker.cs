using UnityEngine;
using System.Text;
using AssetChecker;
using AssetChecker.Interface;
using AssetChecker.Define;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetCheckor.AssetBundleAssetCheck
{
    public class AssetBundleChecker : AssetBaseCheck, IAssetChecker
    {
        const string blankStr = "      ";
        static FindAbType findAbType = FindAbType.Print;
        static DataSourceType dataSource = DataSourceType.ManifestFile;
        static StringBuilder sb;
        static AssetBundle abManifest;
        static AssetBundleManifest manifest;
        static int maxLayerCount = 25;    //超过25层就有问题了
        static bool useLayerNum = false;    //是否显示层数
        static List<string> loopDeps = new List<string>();
        static Dictionary<string, List<string>> manifestTable;

        public static void StartAssetCheck(DataSourceType _dataSourceType)
        {
            findAbType = FindAbType.Print;
            dataSource = _dataSourceType;
            InitAssetCheck();
            var (webroot, template) = PreprocAssets();
            var assetChecker = new AssetBundleChecker();
            assetChecker.DoAssetCheck(template, webroot, null);
        }

        public void DoAssetCheck(string template, string webroot, string[] tempAssetPaths)
        {
            var abs = GetAllAssetBundles();
            sb = new StringBuilder();
            for (int i = 0; i < abs.Length; i++)
            {
                var bundle = abs[i];
                sb.AppendLine("----------------------------------------------" + bundle + "-------------------------------------------");
                sb.AppendLine("name:" + bundle);
                var layerIndex = 0;
                FindAbDeps(bundle, null, bundle, string.Empty, ref layerIndex);
                sb.AppendLine();
            }
            var content = sb.ToString().Replace("\r\n", "<br>\r\n").Replace("   ", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            WriteReportFile(template, content, "AB关系链", webroot, "report_ablinks", GetExcelLink("report_ablinks"));
            SaveOverview(webroot, true);
        }

        static void InitAssetCheck()
        {
            if (dataSource == DataSourceType.MemDependent)
            {
                if (manifestTable == null)
                {
                    manifestTable = ProjectCreateABManifestTool.GetABManifest();
                }
            }
            else
            {
                if (abManifest != null)
                {
                    abManifest.Unload(true);
                }
                var manifestPath = Application.streamingAssetsPath + "/ABManifest";
                if (!File.Exists(manifestPath))
                {
                    Debug.LogError(manifestPath + " 文件不存在，请打AssetBundle或者从其他地方复制文件过来！");
                    return;
                }
                abManifest = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/ABManifest");
                manifest = abManifest.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            }
        }

        static bool FindAbDeps(string rootBundle, ABTreeNode parentNode, string bundle, string blankChar, ref int layerCount)
        {
            if (bundle.Contains("shader_") || bundle.EndsWith("shader"))
            {
                return true;
            }
            var depNames = GetDirectDependencies(bundle);
            if (depNames == null || depNames.Length == 0)
            {
                return true;
            }
            var currLayer = ++layerCount;
            var layerNum = blankChar.Split(new string[] { blankStr }, System.StringSplitOptions.None).Length;

            blankChar += blankStr;

            if (layerNum == maxLayerCount)
            {
                if (findAbType == FindAbType.Print)
                {
                    sb.Append(blankChar);
                    sb.AppendLine("Too many references!!!!===============================!!!!");

                    if (loopDeps.Count > 0)
                    {
                        var joinstrs = string.Join<string>("<br>", loopDeps);
                        sb.AppendLine("[循环依赖]:<br><font color='red'>" + joinstrs + "</font>");
                    }
                    loopDeps.Clear();
                }
                return false;
            }

            var currNode = new ABTreeNode() { _name = bundle, _parent = parentNode };

            foreach (var depName in depNames)
            {
                if (depName.Contains("shader_") || depName.EndsWith("shader"))
                {
                    continue;
                }
                if (findAbType == FindAbType.Print)
                {
                    sb.Append(blankChar);
                    if (useLayerNum)
                    {
                        sb.Append(layerNum + ":");
                    }
                    sb.AppendLine(depName);
                }
                if (rootBundle == depName)     //发现依赖中有跟根AB相同的AB，存在循环依赖
                {
                    loopDeps.Clear();
                    loopDeps.Add(depName);

                    var depNode = currNode;
                    while (depNode._parent != null)
                    {
                        loopDeps.Add(depNode._name);
                        depNode = depNode._parent;
                    }
                }
                var result = FindAbDeps(rootBundle, currNode, depName, blankChar, ref currLayer);
                if (!result) return false;
            }
            return true;
        }

        public static (string, string[]) GetDepAssetBundle(string bundle, DataSourceType _dataSourceType)
        {
            InitAssetCheck();
            var fullBundle = string.Empty;
            var abs = GetAllAssetBundles();
            foreach (var dep in abs)
            {
                if (dep.Contains(bundle))
                {
                    fullBundle = dep;
                    break;
                }
            }
            if (string.IsNullOrEmpty(fullBundle))
            {
                return (fullBundle, null);
            }
            var deps = GetDirectDependencies(fullBundle);
            return (fullBundle, deps);
        }

        public static (string, string[]) GetLoopDependency(string bundle, DataSourceType _dataSourceType)
        {
            InitAssetCheck();
            var fullBundle = string.Empty;
            var abs = GetAllAssetBundles();
            foreach (var dep in abs)
            {
                if (dep.Contains(bundle))
                {
                    fullBundle = dep;
                    break;
                }
            }
            if (string.IsNullOrEmpty(fullBundle))
            {
                return (fullBundle, null);
            }
            findAbType = FindAbType.Pull;
            var layerIndex = 0;
            loopDeps.Clear();
            FindAbDeps(fullBundle, null, fullBundle, string.Empty, ref layerIndex);
            if (loopDeps.Count > 0)
            {
                loopDeps.Reverse();
            }
            return (fullBundle, loopDeps.ToArray());
        }

        static string[] GetAllAssetBundles()
        {
            if (dataSource == DataSourceType.ManifestFile)
            {
                return manifest?.GetAllAssetBundles();
            }
            else
            {
                return GetAllAssetBundlesByDirTable();
            }
        }

        static string[] GetAllAssetBundlesByDirTable()
        {
            if (manifestTable != null)
            {
                var abList = new List<string>();
                foreach (var item in manifestTable)
                {
                    abList.Add(item.Key);
                }
                return abList.ToArray();
            }
            return null;
        }

        static string[] GetDirectDependencies(string bundle)
        {
            if (dataSource == DataSourceType.ManifestFile)
            {
                return manifest?.GetDirectDependencies(bundle);
            }
            else 
            {
                return GetDirectDependenciesByDirTable(bundle);
            }
        }

        static string[] GetDirectDependenciesByDirTable(string bunle)
        {
            if (manifestTable != null)
            {
                foreach (var item in manifestTable)
                {
                    if (item.Key == bunle)
                    {
                        return item.Value.ToArray();
                    }
                }
            }
            return null;
        }
    }
}