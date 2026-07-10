/*
 * @Description: 封装一些辅助接口
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using GameTechTools.CommonLibs.CommonExtends;

namespace CasualEngine.ProjectScanTool
{
    public class ProjectScanHelper
    {
        /// <summary>
        /// 获得编辑器菜单选择项的注册的模块
        /// </summary>
        public static EnumScanModes GetScanModeForMenuItem(System.Object obj)
        {
            if (obj.GetType().BaseType == typeof(CustomScanMode))
            {
                return (obj as CustomScanMode).scanMode;
            }
            if (obj.GetType() == typeof(ScanModeDrawItem))
            {
                return (obj as ScanModeDrawItem).scanMode;
            }
            Debug.LogErrorFormat("错误的扫描类型{0}", obj.ToString());
            return EnumScanModes.通用设置;
        }

        /// <summary>
        /// 获得自定义扫描规则注册的模块
        /// </summary>
        public static EnumScanModes GetCustomRuleBindMode(UnityEngine.Object obj)
        {
            if (obj.GetType().BaseType != typeof(CustomRule))
            {
                Debug.LogErrorFormat("错误的扫描类型{0}", obj.ToString());
                return EnumScanModes.通用设置;
            }
            if (obj.GetType().IsDefined(typeof(CustomScanTypeAttribute), true))
            {
                CustomScanTypeAttribute attrib = (CustomScanTypeAttribute)Attribute.GetCustomAttribute((System.Reflection.MemberInfo)obj.GetType(), typeof(CustomScanTypeAttribute));
                if (attrib != null)
                {
                    return attrib.scanMode;
                }
            }
            Debug.LogErrorFormat("错误的扫描类型{0}", obj.ToString());
            return EnumScanModes.通用设置;
        }

        /// <summary>
        /// 获得自定义扫描规则注册的权值
        /// </summary>
        public static int GetCustomRulePriority(UnityEngine.Object obj)
        {
            if (obj.GetType().BaseType != typeof(CustomRule))
            {
                Debug.LogErrorFormat("错误的扫描类型{0}", obj.ToString());
                return 0;
            }
            if (obj.GetType().IsDefined(typeof(CustomScanTypeAttribute), true))
            {
                CustomScanTypeAttribute attrib = (CustomScanTypeAttribute)Attribute.GetCustomAttribute((System.Reflection.MemberInfo)obj.GetType(), typeof(CustomScanTypeAttribute));
                if (attrib != null)
                {
                    return attrib.Priority;
                }
            }
            Debug.LogErrorFormat("错误的扫描类型{0}", obj.ToString());
            return 0;
        }

        /// <summary>
        /// 执行某个自定的检查规则
        /// </summary>
        public static void DoCustomRuleCheck<T>(Func<T, bool> func, T customRule) where T : CustomRule
        {
            try
            {
                ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eNormal, $"正在进行[{customRule.ruleTitle}]检查......");
                EditorUtility.DisplayProgressBar("提示", $"正在进行[{customRule.ruleTitle}]检查......", 0);
                customRule.BeginScan();
                func(customRule);
                customRule.EndScan();
                if (customRule.logDetails.Count > 0)
                {
                    ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eError, $"[{customRule.ruleTitle}]检查完成，详细资源异常情况查看工作台日志");
                }
                else
                {
                    ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eCorrect, $"[{customRule.ruleTitle}]检查完成，资源无异常");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 执行某个自定的检查规则
        /// </summary>
        public static void DoCustomRuleCheck<T>(Func<T, string[], bool> func, T customRule, string[] assetPostprocessorPath = null) where T : CustomRule
        {
            try
            {
				//后处理就不弹提示了，避免体验不好
				if (assetPostprocessorPath == null)
				{
					ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eNormal, $"正在进行[{customRule.ruleTitle}]检查......");
					EditorUtility.DisplayProgressBar("提示", $"正在进行[{customRule.ruleTitle}]检查......", 0);
				}
                
                customRule.BeginScan();
                func(customRule, assetPostprocessorPath);
                customRule.EndScan();
                if (assetPostprocessorPath == null && customRule.logDetails.Count > 0)
                {
                    ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eError, $"[{customRule.ruleTitle}]检查完成，详细资源异常情况查看工作台日志");
                }
                else
                {
                    ProjectScanWindow.checkLog = FormatScanLog(LogDisplayLevel.eCorrect, $"[{customRule.ruleTitle}]检查完成，资源无异常");
                }
            }
            finally
            {
				if (assetPostprocessorPath == null)
					EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 格式化日志
        /// </summary>
        private static string FormatScanLog(LogDisplayLevel logLv, string detail)
        {
            string logColor = "#ffffff";
            switch (logLv)
            {
                case LogDisplayLevel.eCorrect:
                    logColor = "#00ff00";
                    break;
                case LogDisplayLevel.eWarning:
                    logColor = "#ffa500";
                    break;
                case LogDisplayLevel.eError:
                    logColor = "#ff0000";
                    break;
            }
            return string.Format("<color={0}>{1}</color>", logColor, "  " + DateTime.Now.ToString() + "\t" + detail);
        }

        /// <summary>
        /// clear日志文件
        /// </summary>
        public static void ClearScanLogFils()
        {
            if (Directory.Exists(ProjectScanGlobalConfig.EditorLogDir))
            {
                var files = Directory.GetFiles(ProjectScanGlobalConfig.EditorLogDir);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// 执行应用到后处理的自定义规则
        /// </summary>
        public static void DoCustomRuleForPostProccesser(string[] assetsPath)
        {
            ProjectScanGlobalConfig.openAutoCorrection = true;
            List<CustomRule> autoPostProcessorRules = ProjectScanGlobalConfig.autoPostProcessorRules;
            if (autoPostProcessorRules == null || autoPostProcessorRules.Count == 0)
            {
                return;
            }
            foreach (var rule in autoPostProcessorRules)
            {
                var methods = rule.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.IsDefined<CustomScanActionAttribute>())
                    {
                        if (method.GetParameters().Length == 0)
                        {
                            continue;
                        }
                        method.Invoke(rule, new object[] { assetsPath });
                    }
                }
            }
        }

        /// <summary>
        /// 击customToggle上调试按钮相应的事件，此时由于手动执行会附带修正功能，则可以禁用掉后处理功能
        /// </summary>
        /// <param name="customRule">执行的检查规则</param>
        /// <param name="isSignleDebug">是否是单个执行的调试，如果是则清理日志</param>
        /// <param name="isJenkins">是否是jenkins后台执行</param>
        public static void DoScanCustomRuleByEntry(UnityEngine.Object customRule, bool isSignleDebug = false, bool isJenkins = false)
        {
            try
            {
                if (isSignleDebug)
                {
                    ProjectScanLogsMgr.BeginStatistics();
                }
                if (isJenkins == false)
                {
                    ProjectScanGlobalConfig.openAutoCorrection = true;
                }
                ProjectScanGlobalConfig.openCustomPostProccesser = false;
                bool flag = false;
                var methods = customRule.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.IsDefined<CustomScanActionAttribute>())
                    {
                        flag = true;
                        int parameCnt = 0;
                        if (method.GetParameters() != null)
                            parameCnt = method.GetParameters().Length;
                        List<object> args = new List<object>();
                        //点击执行的操作都是默认的检查规则，不传入任何参数
                        while (parameCnt-- > 0)
                        {
                            args.Add(null);
                        }
                        method.Invoke(customRule, args.ToArray());
                    }
                }

                if (!flag)
                {
                    EditorUtility.DisplayDialog("提示", $"{customRule.GetType()}没有实现任何检查函数或没有给相应检查函数添加CustomScanAction特性", "确定");
                }
                if (isSignleDebug)
                {
                    ProjectScanLogsMgr.EndStatistics();
                }
            }
            finally
            {
                ProjectScanGlobalConfig.openCustomPostProccesser = true;
            }
        }

        /// <summary>
        /// 写入并打开文本
        /// </summary>
        public static void WriteAndOpenFile<T>(StringBuilder builder, T customRule, bool autoOpen = false) where T : CustomRule
        {
            if (!ProjectScanGlobalConfig.generalSetting.logSetting.printLogToFile.enable)
            {
                return;
            }
            string logPath = ProjectScanGlobalConfig.EditorLogDir + "/" + customRule.ruleTitle.Trim().Replace("/", "&") + ".log";
            File.WriteAllText(logPath, builder.ToString());
            if (autoOpen)
            {
                System.Diagnostics.Process.Start(logPath);
            }
        }

        /// <summary>
        /// 写入并打开文本
        /// </summary>
        public static void WriteAndOpenFile(StringBuilder builder, string fileName = "log")
        {
            string logPath = ProjectScanGlobalConfig.EditorLogDir + "/" + fileName + ".log";
            File.WriteAllText(logPath, builder.ToString());
            System.Diagnostics.Process.Start(logPath);
        }

        /// <summary>
        /// 将类似 d:\Projects\Assets\xxx\yy 路径转换成 Assets/ 开头路径
        /// </summary>
        public static string TrimAssetPath(string originPath)
        {
            originPath = originPath.Replace("\\", "/");
            int index = originPath.IndexOf("Assets/", StringComparison.Ordinal);
            if (index == -1)
                throw new Exception(string.Format("originPath {0} not contains Assets/", originPath));

            return originPath.Substring(index);
        }

        /// <summary>
        /// 是否是子目录
        /// </summary>
        public static bool IsSubDirectory(string srcPath, string dstPath)
        {
            DirectoryInfo srcDir = new DirectoryInfo(srcPath);
            DirectoryInfo dstDir = new DirectoryInfo(dstPath);
            if (srcDir == null || dstDir == null)
            {
                return false;
            }
            if (TrimAssetPath(srcDir.FullName + "/").StartsWith(TrimAssetPath(dstDir.FullName + "/")))
            {
                return true;
            }
            //子目录太多，递归获取树结构太慢了
            //foreach (var subDir in dstDir.GetDirectories("*", SearchOption.AllDirectories))
            //{
            //    if (subDir.FullName == srcDir.FullName)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        /// <summary>
        /// 是否在配置的忽略文件夹里面
        /// </summary>
        public static bool InIgnoreDir(string path, CustomCheckDetail customDetail)
        {
            List<string> ignoreDirs;
            if (customDetail.useSingleDir.enable)
            {
                ignoreDirs = customDetail.ignoreDirs;
            }
            else
            {
                ignoreDirs = ProjectScanGlobalConfig.generalSetting.ignoreDirs;
            }
            if (ignoreDirs == null || ignoreDirs.Count == 0)
            {
                return false;
            }

            string dirPath = path;
            //如果是文件取父目录
            if (File.Exists(path))
            {
                dirPath = Path.GetDirectoryName(path);
            }

            foreach (var item in ignoreDirs)
            {
                if (IsSubDirectory(dirPath, item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
		/// 根据扫描规则获得某一类型资源guid集合, 规则包括单独的目标文件夹，忽略文件夹和忽略文件，如果没有启用单独的目标文件夹，则使用通用设置里面的文件夹配置
		/// </summary>
		public static string[] GetAssetPathsByType<T, U>(T customRule, U detail, string type = "texture", string[] assetPostprocessorPath = null) where T : CustomRule where U : CustomCheckDetail
        {
            if (detail.GetType() != typeof(CustomCheckDetail) && detail.GetType().BaseType != typeof(CustomCheckDetail))
            {
                Debug.LogError($"{detail}不是自定义的扫描规则，需要继承CustomCheckDetail");
                return new string[] { };
            }
            CustomCheckDetail customDetail = detail as CustomCheckDetail;
            string[] guids = { };
            if (customDetail.useSingleDir.enable && customDetail.targetDirs.Count > 0)
            {
                guids = AssetDatabase.FindAssets("t:" + type, customDetail.targetDirs.ToArray());
            }
            else if (ProjectScanGlobalConfig.generalSetting.targetDirs.Count > 0)
            {
                guids = AssetDatabase.FindAssets("t:" + type, ProjectScanGlobalConfig.generalSetting.targetDirs.ToArray());
            }
            else
            {
                guids = AssetDatabase.FindAssets("t:" + type, new string[] { "Assets" });
            }
            if (guids.Length == 0)
            {
                return new string[] { };
            }
            List<string> paths = new List<string>();
            for (int i = 0, len = guids.Length; i < len; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                //在忽略列表里面
                if (InIgnoreDir(path, customDetail))
                {
                    continue;
                }
                //在白名单里面
                if (customDetail.whiteListPath.Contains(path))
                {
                    continue;
                }
                //并不是图片
                if (type == AssetType.texture && !TextureUtils.IsImage(path))
                {
                    continue;
                }
                //忽略psd文件
                if (path.EndsWith(".psd"))
                {
                    continue;
                }
                //在需要查找的文件夹里面
                if (customDetail.checkPath != null && customDetail.checkPath.Count > 0 )
                {
                    bool include = false;
                    foreach (var checkKey in customDetail.checkPath)
                    {
                        if (!string.IsNullOrEmpty(checkKey)&& path.Contains(checkKey)) 
                        {
                            include = true;
                        }
                    }
                    if (include)
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        continue;
                    }
                }
                else 
                {
                    paths.Add(path);
                }
            }

            //没有指定后处理路径，则返回所有筛选的配置路径
            if (assetPostprocessorPath == null)
            {
                customRule.scanAssetsCnt += paths.Count;
                return paths.ToArray();
            }
            //找出符合要后处理的文件路径
            if (assetPostprocessorPath.Length > 0)
            {
                List<string> ret = new List<string>();
                for (var i = 0; i < assetPostprocessorPath.Length; i++)
                {
                    if (paths.Contains(assetPostprocessorPath[i]))
                    {
                        ret.Add(assetPostprocessorPath[i]);
                    }
                }
                customRule.scanAssetsCnt += ret.Count;
                return ret.ToArray();
            }

            return new string[] { };
        }

        /// <summary>
		/// 根据扫描规则获得某一类型资源guid集合, 规则包括单独的目标文件夹，忽略文件夹和忽略文件，如果没有启用单独的目标文件夹，则使用通用设置里面的文件夹配置
		/// </summary>
		public static string[] GetAllAssetPaths<T, U>(T customRule, U detail) where T : CustomRule where U : CustomCheckDetail
        {
            if (detail.GetType() != typeof(CustomCheckDetail) && detail.GetType().BaseType != typeof(CustomCheckDetail))
            {
                Debug.LogError($"{detail}不是自定义的扫描规则，需要继承CustomCheckDetail");
                return new string[] { };
            }
            CustomCheckDetail customDetail = detail as CustomCheckDetail;
            string[] targetDirs = { };

            if (customDetail.useSingleDir.enable && customDetail.targetDirs.Count > 0)
            {
                targetDirs = customDetail.targetDirs.ToArray();
            }
            else if (ProjectScanGlobalConfig.generalSetting.targetDirs.Count > 0)
            {
                targetDirs = ProjectScanGlobalConfig.generalSetting.targetDirs.ToArray();
            }
            else
            {
                targetDirs = new string[] { "Assets" };
            }
            if (targetDirs.Length == 0)
            {
                return new string[] { };
            }
            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            List<string> paths = new List<string>();
            for (int i = 0, len = allPaths.Length; i < len; i++)
            {
                string path = allPaths[i];
                bool bFind = false; //检查是否在目标目录里面
                foreach (var item in targetDirs)
                {
                    if (path.StartsWith(item + "/"))
                    {
                        bFind = true;
                        break;
                    }
                }
                if (!bFind)
                {
                    continue;
                }
                //在忽略列表里面
                if (InIgnoreDir(path, customDetail))
                {
                    continue;
                }
                //在白名单里面
                if (customDetail.whiteListPath.Contains(path))
                {
                    continue;
                }
                paths.Add(path);
            }
            customRule.scanAssetsCnt += paths.Count;
            return paths.ToArray();
        }

        /// <summary>
		/// 获得指定目录下某一类型资源guid集合
		/// </summary>
		public static string[] GetAssetGuidsByType(string resFolder = "Assets", string type = "texture")
        {
            return AssetDatabase.FindAssets("t:" + type, new string[] { resFolder });
        }

        /// <summary>
        /// PrintCheckLog
        /// </summary>
        public static void PrintProjectScanLog(string method, string detail, string path)
        {
            Debug.LogErrorFormat("<ProjectScanLog>[Method]: {0}; [ErrorDetail]: {1}; [Path]: {2}", method, detail, path);
        }

        /// <summary>
        /// PrintCheckLog
        /// </summary>
        public static void PrintProjectScanLog(UnityEngine.Object context, string ruleTitle, string method, string detail, string path)
        {
            Debug.LogErrorFormat(context, "<ProjectScanLog>[Rule]: {0};[Method]: {1}; [ErrorDetail]: {2}; [Path]: {3}", ruleTitle, method, detail, path);
        }

        /// <summary>
        /// 获得变量名称，不接受二次函数传递，否则在第一次函数传递时就会变成第一次函数传参的变量名了
        /// </summary>
        public static string GetVarName(Expression<Func<System.Object>> expr)
        {
            Expression e = expr.Body;
            MemberExpression me = e as MemberExpression;
            if (me == null)
            {
                UnaryExpression ue = e as UnaryExpression;
                me = ue.Operand as MemberExpression;
            }
            return me.Member.Name;
        }

        /// <summary>
        /// 深Copy
        /// </summary>
        public static object DeepCopy<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                List<UnityEngine.Object> unityReferences;
                Sirenix.Serialization.SerializationUtility.SerializeValue(obj, stream, DataFormat.Binary, out unityReferences);
                stream.Position = 0;
                return Sirenix.Serialization.SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences);
            }
        }

        /// <summary>
        /// 根据instanceID获得guid
        /// </summary>
        static public string GetGuidByInstanceID(int instance_id)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instance_id, out string guid, out long local_id);
            return guid;
        }

        /// <summary>
        /// 创建序列化对象
        /// </summary>
        static public T CreateAsset<T>(string outPath, string newName = "") where T : ScriptableObject
        {
            var scriptableObj = ScriptableObject.CreateInstance<T>();

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
                AssetDatabase.Refresh();
            }

            string filePath = outPath + GetGuidByInstanceID(scriptableObj.GetInstanceID()) + ".asset";

            AssetDatabase.CreateAsset(scriptableObj, filePath);

            if (string.IsNullOrEmpty(newName))
            {
                AssetDatabase.RenameAsset(filePath, GetGuidByInstanceID(scriptableObj.GetInstanceID()));
            }
            else
            {
                AssetDatabase.RenameAsset(filePath, newName);
            }
            AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return scriptableObj;
        }

        /// <summary>
        /// 创建序列化对象
        /// </summary>
        static public ScriptableObject CreateScriptableObjectAsset(string outPath, Type assetType, string newName = "")
        {
            var scriptableObj = ScriptableObject.CreateInstance(assetType);

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
                AssetDatabase.Refresh();
            }

            string filePath = outPath + GetGuidByInstanceID(scriptableObj.GetInstanceID()) + ".asset";

            AssetDatabase.CreateAsset(scriptableObj, filePath);

            if (string.IsNullOrEmpty(newName))
            {
                AssetDatabase.RenameAsset(filePath, GetGuidByInstanceID(scriptableObj.GetInstanceID()));
            }
            else
            {
                AssetDatabase.RenameAsset(filePath, newName);
            }
            AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return scriptableObj;
        }

        /// <summary>
        /// 资源名同时检查是否有中文和空格，资源内部只检查中文
        /// </summary>
        public static bool isResHasChineseOrSpace(string path, bool checkChild = true)
        {
            if (isStringHasChineseOrSpace(path, true))
                return true;
            if (!checkChild)
            {
                return false;
            }
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj != null)
            {
                Transform[] trans = obj.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < trans.Length; i++)
                {
                    if (isStringHasChineseOrSpace(trans[i].name, false))
                        return true;
                }
            }
            return false;
        }

        private static bool isStringHasChineseOrSpace(string path, bool checkspace = false)
        {
            for (int i = 0; i < path.Length; i++)
            {
                int a = path[i];
                if (((int)a >= 0x4e00 && (int)a <= 0x9fbb) || (a == ' ' && checkspace))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获得节点路径
        /// </summary>
        public static string GetNodePath(Transform node)
        {
            if (node == null)
            {
                Debug.LogErrorFormat("GetNodePath error, node is null");
                return "";
            }
            string path = node.name;
            Transform parentNode = node.parent;
            while (parentNode != null)
            {
                path = parentNode.name + "/" + path;
                parentNode = parentNode.parent;
            }

            return path;
        }

        private static List<string> editorLogs = new List<string>();
        private static void HandleEditorLog(string message, string stackTrace, LogType type)
        {
            editorLogs.Add(message);
        }

        /// <summary>
        /// 获得捕获的Editor控制台日志
        /// </summary>
        public static List<string> GetEditorLog()
        {
            return editorLogs;
        }

        /// <summary>
        /// 开始捕获Editor控制台日志
        /// </summary>
        public static void BeginReceiveEditorLog()
        {
            ProjectScanHelper.editorLogs.Clear();
            Application.logMessageReceived -= ProjectScanHelper.HandleEditorLog;
            Application.logMessageReceived += ProjectScanHelper.HandleEditorLog;
        }

        /// <summary>
        /// 结束捕获Editor控制台日志
        /// </summary>
        public static void EndReceiveEditorLog()
        {
            Application.logMessageReceived -= ProjectScanHelper.HandleEditorLog;
        }

        /// <summary>
        /// 返回一个枚举的list，提供给相应类型进行绘制
        /// </summary>
        public static ValueDropdownList<int> GetEnumPages(Type type)
        {
            ValueDropdownList<int> ret = new ValueDropdownList<int>();
            if (type.IsEnum)
            {
                var names = Enum.GetNames(type);
                for (var i = 0; i < names.Length; i++)
                {
                    ret.Add(names[i], (int)Enum.Parse(type, names[i]));
                }
            }
            else
            {
                Debug.LogError($"{type} is not enum");
            }

            return ret;
        }

    }
}
