using MapEditor;
using OfficeOpenXml;
using StarProject.UI.LanguageUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Task;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 导出预制体中的文本字符
/// </summary>
public static class ExportTextValues
{
    private static string LOG_TAG = "[ExportTextValues]";

    private static readonly string m_TargetFilePath = "Assets/Editor/Localization/TextValues.txt";  // 文件保存路径

    #region 预制体

    public class PrefabExportArr
    {
        public string Key;
        public string Val;

        public void SetData(string key, string val)
        {
            Key = key;
            Val = val;
        }

        public void ExportExcel(int row, ExcelRange excel)
        {
            int index = 1;
            excel[row, index++].Value = Key;
            excel[row, index++].Value = Val;
        }
    }

    private static readonly string m_PrefabFilePath = "Assets/Res/UI/Wanted";  // 预制体所在文件夹路径
    private static Dictionary<string, List<string>> m_prefabTextStr = new();
    private static Dictionary<string, List<string>> m_prefabTextStrCopy = new();
    private static List<string> m_prefabTextStr2 = new();

    #endregion

    #region 地图
    public class MapExportArr
    {
        public int MapID;
        public string Key;
        public string Val;
        public string Des;

        public void SetData(int mapID, string key, string val, string des)
        {
            MapID = mapID;
            Key = key;
            Val = val;
            Des = des;
        }

        public void ExportExcel(int row, ExcelRange excel)
        {
            int index = 1;
            excel[row, index++].Value = MapID;
            excel[row, index++].Value = Des;
            excel[row, index++].Value = Key;
            excel[row, index++].Value = Val;
        }
    }
    private static readonly string m_MapDataFilePath = "Assets/Res/MapData";
    private static List<MapExportArr> m_mapTextStr = new();
    private static Dictionary<int, Dictionary<string, List<string>>> m_mapdataTextStr = new();
    private static List<string> m_mapdataTextStr2 = new();
    #endregion

    #region 任务
    public class TaskExportArr
    {
        public uint TaskID;
        public string Key;
        public string Val;
        public string Des;

        public void SetData(uint taskID, string key, string val, string des)
        {
            TaskID = taskID;
            Key = key;
            Val = val;
            Des = des;
        }

        public void ExportExcel(int row, ExcelRange excel)
        {
            int index = 1;
            excel[row, index++].Value = TaskID;
            excel[row, index++].Value = Des;
            excel[row, index++].Value = Key;
            excel[row, index++].Value = Val;
        }
    }
    public class TaskExport
    {
        public uint TaskID;
        // 章节名称
        public TaskExportArr ChapterName = new();
        public void SetChapterName(string key, string val, string des)
        {
            ChapterName.SetData(TaskID, key, val, des);
        }

        // 任务名称
        public TaskExportArr TaskName = new();
        public void SetTaskName(string key, string val, string des)
        {
            TaskName.SetData(TaskID, key, val, des);
        }

        // 特殊描述
        public TaskExportArr SpecTaskDesc = new();
        public void SetSpecTaskDesc(string key, string val, string des)
        {
            SpecTaskDesc.SetData(TaskID, key, val, des);
        }

        // 任务描述
        public TaskExportArr TaskDesc = new();
        public void SetTaskDesc(string key, string val, string des)
        {
            TaskDesc.SetData(TaskID, key, val, des);
        }

        //--承接效果组
        // 服务显示文本
        public Dictionary<string, TaskExportArr> PickEffects_ShowText = new();
        public void SetPickEffects_ShowText(int DialogueID, int index, string key, string val, string des)
        {
            string dicKey = $"{DialogueID}_{index}";
            if (PickEffects_ShowText.ContainsKey(dicKey))
            {
                Debug.LogWarning($"[承接效果组-对话效果] 已经有相同的对话ID={DialogueID},index={index}");
            }
            else
            {
                TaskExportArr taskExportArr = new();
                taskExportArr.SetData(TaskID, key, val, des);
                PickEffects_ShowText.Add(dicKey, taskExportArr);
            }
        }

        // -- 交付效果组
        // 服务显示文本
        public Dictionary<string, TaskExportArr> FinishEffects_ShowText = new();
        public void SetFinishEffects_ShowText(int DialogueID, int index, string key, string val, string des)
        {
            string dicKey = $"{DialogueID}_{index}";
            if (FinishEffects_ShowText.ContainsKey(dicKey))
            {
                Debug.LogWarning($"[交付效果组-对话效果] 已经有相同的对话ID={DialogueID},index={index}");
            }
            else
            {
                TaskExportArr taskExportArr = new();
                taskExportArr.SetData(TaskID, key, val, des);
                FinishEffects_ShowText.Add(dicKey, taskExportArr);
            }
        }

        // 任务目标
        // 任务目标描述
        public Dictionary<int, TaskExportArr> TargetDesc = new();
        public void SetTargetDesc(int TargetID, string key, string val, string des)
        {
            if (TargetDesc.ContainsKey(TargetID))
            {
                Debug.LogWarning($"[任务目标描述] 已经有相同的目标ID={TargetID}");
            }
            else
            {
                TaskExportArr taskExportArr = new();
                taskExportArr.SetData(TaskID, key, val, des);
                TargetDesc.Add(TargetID, taskExportArr);
            }
        }

        // -- 效果目标ID
        // 服务显示文本
        public Dictionary<string, TaskExportArr> Target_ShowText = new();
        public void SetTarget_ShowText(int DialogueID, string index, string key, string val, string des)
        {
            string dicKey = $"{DialogueID}_{index}";
            if (Target_ShowText.ContainsKey(dicKey))
            {
                Debug.LogWarning($"[任务目标-对话效果] 已经有相同的对话ID={DialogueID},index={index}");
            }
            else
            {
                TaskExportArr taskExportArr = new();
                taskExportArr.SetData(TaskID, key, val, des);
                Target_ShowText.Add(dicKey, taskExportArr);
            }
        }
    }
    private static readonly string m_TaskFilePath = "Assets/Res/Config/Task";   // 任务配置
    private static List<TaskExport> m_taskTextStr = new();
    #endregion

    #region LUA

    private static readonly string m_LuaScriptsFilePath = "Assets/Res/LuaScripts";  // Lua所在文件夹路径
    private static List<string> m_LuaScriptsText = new();
    private static List<string> m_LuaScriptsText2 = new();

    private static Dictionary<string, Dictionary<string, List<string>>> m_LuaScriptsPath = new();
    private static int index = 1;

    #endregion


    #region 预制体文本

    #region 给所有预制体文本加多语言组件

    [MenuItem("自动化工具/多语言/预制体/预制体文本加多语言组件", false, 1)]
    public static void AddAllPrefabTextComp()
    {
        List<string> list = new();

        string[] prefabFiles = Directory.GetFiles(m_PrefabFilePath, "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab.name.StartsWith("gm", true, null))
            {
                continue;
            }
            // 遍历预制体内的Text组件
            Text[] texts = prefab.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                if (!string.IsNullOrEmpty(text.text) && !string.IsNullOrWhiteSpace(text.text))
                {
                    //if (!IsNumber(text.text))
                    {
                        if (HasChinese(text.text))
                        {
                            LocalizationText comp = text.gameObject.GetComponent<LocalizationText>();
                            if (comp == null)
                            {
                                var localizationText = text.gameObject.AddComponent<LocalizationText>();
                                string log = string.Format("为{0}下的节点{1}文本内容是[{2}]添加了多语言文本组件！", prefab.name, text.name, text.text);
                                list.Add(log);
                            }
                        }
                    }
                }
            }
            TextMeshPro[] textMeshPro = prefab.GetComponentsInChildren<TextMeshPro>(true);
            foreach (TextMeshPro text in textMeshPro)
            {
                if (!string.IsNullOrEmpty(text.text) && !string.IsNullOrWhiteSpace(text.text))
                {
                    //if (!IsNumber(text.text))
                    {
                        if (HasChinese(text.text))
                        {
                            LocalizationText comp = text.gameObject.GetComponent<LocalizationText>();
                            if (comp == null)
                            {
                                var localizationText = text.gameObject.AddComponent<LocalizationText>();
                                string log = string.Format("为{0}下的节点{1}文本内容是[{2}]添加了多语言文本组件！", prefab.name, text.name, text.text);
                                list.Add(log);
                            }
                        }
                    }
                }
            }
            PrefabUtility.SavePrefabAsset(prefab);
        }

        StringBuilder csv = new();
        foreach (var item in list)
        {
            csv.AppendLine(item);
        }

        Debug.Log($"总条数=》{list.Count}");

        File.WriteAllText(m_TargetFilePath, csv.ToString());

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("消息", "添加成功", "您辛苦了");
    }

    #endregion

    #region 刷新文本key

    [MenuItem("自动化工具/多语言/预制体/刷新文本key", false, 2)]
    public static void RefreshTextKey()
    {
        List<string> list = new();

        string[] prefabFiles = Directory.GetFiles(m_PrefabFilePath, "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab.name.StartsWith("gm", true, null))
            {
                continue;
            }
            LocalizationText[] texts = prefab.GetComponentsInChildren<LocalizationText>(true);
            int index = 1;
            foreach (LocalizationText text in texts)
            {
                text.key = $"{prefab.name}_{index++}";
                string log = string.Format("为[{0}]下的节点[{1}]设置多语言key=[{2}]", prefab.name, text.name, text.key);
                list.Add(log);
            }
            PrefabUtility.SavePrefabAsset(prefab);
        }

        StringBuilder csv = new();
        foreach (var item in list)
        {
            csv.AppendLine(item);
        }

        Debug.Log($"总条数=》{list.Count}");

        File.WriteAllText(m_TargetFilePath, csv.ToString());
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("消息", $"一共:{list.Count}条\n\n\t刷新成功", "您辛苦了");
    }

    #endregion

    #region 导出预制体文本

    [MenuItem("自动化工具/多语言/预制体/导出预制体文本", false, 3)]
    public static void ExportPrefabTextStr()
    {
        ExportPrefabText();

        Release();
    }

    public static void ExportPrefabText()
    {
        string[] prefabFiles = Directory.GetFiles(m_PrefabFilePath, "*.prefab", SearchOption.AllDirectories);
        List<PrefabExportArr> list = new();
        int ErrCount = 0;
        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            LocalizationText[] texts = prefab.GetComponentsInChildren<LocalizationText>(true);
            foreach (LocalizationText text in texts)
            {
                string content = "";
                Text TextComp = text.gameObject.GetComponent<Text>();
                if (TextComp != null)
                {
                    content = TextComp.text;
                }
                TextMeshPro TextMeshProComp = text.gameObject.GetComponent<TextMeshPro>();
                if (TextMeshProComp != null)
                {
                    content = TextMeshProComp.text;
                }
                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogError($"出错了，导出的多语言内容文本是空的。。 预制体={prefab.name},节点={text.name},内容={content}");
                    ErrCount++;
                    continue;
                }
                if (!string.IsNullOrEmpty(content) && !string.IsNullOrWhiteSpace(content))
                {
                    if (!HasChinese(content))
                    {
                        Debug.LogError($"出错了，导出的多语言内容文本不包含中文的。。 预制体={prefab.name},节点={text.name},内容={content}");
                        ErrCount++;
                        continue;
                    }
                }
                string key = text.key;
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError($"出错了，导出的多语言key是空的。。 预制体={prefab.name},节点={text.name},内容={content}");
                    ErrCount++;
                    continue;
                }

                PrefabExportArr prefabExportArr = new();
                prefabExportArr.SetData(key, content);
                list.Add(prefabExportArr);
            }
        }

        Debug.Log($"导出的多语言key 总文件数=》{prefabFiles.Length}");

        WritePrefabExcel(list, ErrCount);
    }

    private static void WritePrefabExcel(List<PrefabExportArr> list, int ErrCount)
    {
        var path = EditorUtility.SaveFilePanel("选择导出预制体文本路径", "", "预制体文本_PrefabText.xlsx", "*.xlsx");
        if (path != "")
        {
            FileInfo newFile = new(path);
            if (newFile.Exists)
            {
                //创建一个新的excel文件
                newFile.Delete();
                newFile = new FileInfo(path);
            }

            //通过ExcelPackage打开文件
            using (ExcelPackage package = new(newFile))
            {
                //在excel空文件添加新sheet
                ExcelWorksheet taskConfig = package.Workbook.Worksheets.Add("PrefabText");
                int index = 1;
                taskConfig.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                taskConfig.Cells[1, index++].Value = "多语言key";
                taskConfig.Cells[1, index++].Value = "文本";

                int row = 6;
                foreach (var item in list)
                {
                    item.ExportExcel(row++, taskConfig.Cells);
                }

                taskConfig.Cells.AutoFitColumns();
                //保存excel
                package.Save();

                EditorUtility.DisplayDialog("导出完成", $"成功导出:{list.Count}条\n\n失败了:{ErrCount}条", "您辛苦了");
            }
        }
    }

    public static void ExportPrefabText222()
    {
        string[] prefabFiles = Directory.GetFiles(m_PrefabFilePath, "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab.name.StartsWith("gm", true, null))
            {
                continue;
            }
            // 遍历预制体内的Text组件
            Text[] texts = prefab.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                if (!string.IsNullOrEmpty(text.text) && !string.IsNullOrWhiteSpace(text.text))
                {
                    //if (!IsNumber(text.text))
                    {
                        if (HasChinese(text.text))
                        {
                            AddStr(prefab.name, text.gameObject, $"{text.text}");
                        }
                    }
                }
            }
            TextMeshPro[] textMeshPro = prefab.GetComponentsInChildren<TextMeshPro>(true);
            foreach (TextMeshPro text in textMeshPro)
            {
                if (!string.IsNullOrEmpty(text.text) && !string.IsNullOrWhiteSpace(text.text))
                {
                    //if (!IsNumber(text.text))
                    {
                        if (HasChinese(text.text))
                        {
                            AddStr(prefab.name, text.gameObject, $"{text.text}");
                        }
                    }
                }
            }
        }
        Debug.Log($"总文件数=》{prefabFiles.Length}");

        WriteText();
    }

    private static string GetPath(this GameObject obj)
    {
        if (obj == null)
            return null;

        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = $"{obj.name}/{path}";
        }

        return path;
    }

    private static void AddStr(string prefabName, GameObject obj, string str)
    {
        str = str.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty);

        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        bool isHave = false;
        string gobName = obj.name;
        string content = $"{gobName}￥{str}";
        if (m_prefabTextStrCopy.TryGetValue(prefabName, out var list))
        {
            if (list.Contains(content))
            {
                isHave = true;
            }
            else
            {
                m_prefabTextStrCopy[prefabName].Add(content);
            }
        }
        else
        {
            m_prefabTextStrCopy.Add(prefabName, new());
            m_prefabTextStrCopy[prefabName].Add(content);
        }

        if (!isHave)
        {
            string pathtext = $"{GetPath(obj)}￥{gobName}￥{str}";
            if (!m_prefabTextStr.ContainsKey(prefabName))
            {
                m_prefabTextStr.Add(prefabName, new());
            }
            m_prefabTextStr[prefabName].Add(pathtext);
        }


    }

    private static void WriteText()
    {
        foreach (var item in m_prefabTextStr)
        {
            string prefabName = $"{item.Key}";
            //if (!m_prefabTextStr2.Contains(prefabName))
            {
                //m_prefabTextStr2.Add(prefabName);
            }
            for (int i = 0; i < item.Value.Count; i++)
            {
                string child = item.Value[i];
                //if (!m_prefabTextStr2.Contains(child))
                //{
                //    m_prefabTextStr2.Add(child);
                //}
                string str = $"{prefabName}￥{child}";
                if (!m_prefabTextStr2.Contains(str))
                {
                    m_prefabTextStr2.Add(str);
                }
            }
        }

        StringBuilder csv = new();
        foreach (var item in m_prefabTextStr2)
        {
            csv.AppendLine(item);
        }

        Debug.Log($"总条数=》{m_prefabTextStr2.Count}");

        File.WriteAllText(m_TargetFilePath, csv.ToString());

        AssetDatabase.Refresh();
    }

    #endregion

    #endregion

    #region 导出地图文本

    [MenuItem("自动化工具/多语言/导出地图文本", false, 2)]
    public static void ExportMapTextStr()
    {
        ExportMapText();

        //WriteMapDataText();
        WriteMapExcel();
        Release();
    }

    private static void ExportMapText()
    {
        m_mapdataTextStr.Clear();
        string[] files = Directory.GetFiles(m_MapDataFilePath, "*.json", SearchOption.AllDirectories);
        foreach (var item in files)
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
            var jsonAsset = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(textAsset.text);
            if (jsonAsset != null)
            {
                //{
                //    foreach (var item1 in jsonAsset.Monsters)
                //    {
                //        if (!string.IsNullOrEmpty(item1.Value.Alias) && !string.IsNullOrWhiteSpace(item1.Value.Alias))
                //        {
                //            AddMapDataStr(jsonAsset.SceneID, item1.Value.Index.ToString(), item1.Value.Alias);
                //            //Debug.Log($"怪物id={item1.Value.MonsterID},Alias={item1.Value.Alias}");
                //        }
                //        if (!string.IsNullOrEmpty(item1.Value.Title) && !string.IsNullOrWhiteSpace(item1.Value.Title))
                //        {
                //            AddMapDataStr(jsonAsset.SceneID, item1.Value.Index.ToString(), item1.Value.Title);
                //            //Debug.Log($"怪物id={item1.Value.MonsterID},Title={item1.Value.Title}");
                //        }
                //    }
                //}
                {
                    foreach (var item1 in jsonAsset.Areas)
                    {
                        if (item1.Value != null && item1.Value.areaType == (int)AreaType.Jurisdiction)
                        {
                            if (!string.IsNullOrEmpty(item1.Value.AreaName) && !string.IsNullOrWhiteSpace(item1.Value.AreaName))
                            {
                                MapExportArr mapExportArr = new();
                                mapExportArr.SetData(jsonAsset.SceneID, $"AreaName_{jsonAsset.SceneID}_{item1.Value.AreaID}", item1.Value.AreaName, "地图区域名");
                                m_mapTextStr.Add(mapExportArr);
                                //AddMapDataStr(jsonAsset.SceneID, item1.Value.Index.ToString(), item1.Value.AreaName);
                                //Debug.Log($"区域id={item1.Value.AreaID} ,AreaName={item1.Value.AreaName}");
                            }
                        }
                    }
                }
            }
        }
    }

    private static void AddMapDataStr(int mapid, string prefabName, string str)
    {
        str = str.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty);

        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        if (!m_mapdataTextStr.ContainsKey(mapid))
        {
            m_mapdataTextStr.Add(mapid, new());
        }

        if (m_mapdataTextStr.TryGetValue(mapid, out var dic))
        {

        }

        if (!dic.ContainsKey(prefabName))
        {
            dic.Add(prefabName, new());
        }

        if (dic.TryGetValue(prefabName, out var list))
        {

        }

        list.Add(str);
    }

    private static void WriteMapDataText()
    {
        foreach (var item in m_mapdataTextStr)
        {
            string mapID = $"{item.Key}";
            foreach (var item2 in item.Value)
            {
                string index = $"{item2.Key}";
                for (int i = 0; i < item2.Value.Count; i++)
                {
                    string str = $"{mapID}￥{index}￥{item2.Value[i]}";
                    if (!m_mapdataTextStr2.Contains(str))
                    {
                        m_mapdataTextStr2.Add(str);
                    }
                }
            }
        }

        StringBuilder csv = new();
        foreach (var item in m_mapdataTextStr2)
        {
            csv.AppendLine(item);
        }

        Debug.Log($"总条数=》{m_mapdataTextStr2.Count}");

        File.WriteAllText(m_TargetFilePath, csv.ToString());

        AssetDatabase.Refresh();
    }

    private static void WriteMapExcel()
    {
        var path = EditorUtility.SaveFilePanel("选择导出地图文本路径", "", "地图编辑器文本_MapEditorText.xlsx", "*.xlsx");
        if (path != "")
        {
            FileInfo newFile = new(path);
            if (newFile.Exists)
            {
                //创建一个新的excel文件
                newFile.Delete();
                newFile = new FileInfo(path);
            }

            //通过ExcelPackage打开文件
            using (ExcelPackage package = new(newFile))
            {
                //在excel空文件添加新sheet
                ExcelWorksheet taskConfig = package.Workbook.Worksheets.Add("MapEditorText");
                int index = 1;
                taskConfig.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                taskConfig.Cells[1, index++].Value = "地图ID";
                taskConfig.Cells[1, index++].Value = "描述";
                taskConfig.Cells[1, index++].Value = "多语言key";
                taskConfig.Cells[1, index++].Value = "文本";

                int row = 6;
                foreach (var item in m_mapTextStr)
                {
                    item.ExportExcel(row++, taskConfig.Cells);
                }

                taskConfig.Cells.AutoFitColumns();
                //保存excel
                package.Save();

                EditorUtility.DisplayDialog("消息", $"一共:{m_mapTextStr.Count}条\n\n\t导出成功", "您辛苦了");
            }
        }
    }

    #endregion

    #region 导出任务文本

    [MenuItem("自动化工具/多语言/导出任务文本", false, 2)]
    public static void ExportTaskTextStr()
    {
        ExportTaskText();

        //WriteTaskDataText();

        WriteTaskExcel();
        Release();
    }

    private static void ExportTaskText()
    {
        m_mapdataTextStr.Clear();
        string[] files = Directory.GetFiles(m_TaskFilePath, "*.json", SearchOption.AllDirectories);
        foreach (var item in files)
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
            var TaskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<Task.TaskConfigs>(textAsset.text);

            if (TaskConfigs != null && TaskConfigs.list != null && TaskConfigs.list.Count > 0)
            {
                foreach (var item1 in TaskConfigs.list)
                {
                    TaskExport taskExport = new();
                    // 基础信息
                    //int Chapter = item1.Value.Base.Chapter; // 章节ID
                    uint TaskID = item1.Value.Base.TaskID; // 任务ID
                    taskExport.TaskID = TaskID;
                    // 章节名称
                    {
                        string ChapterName = item1.Value.Base.ChapterName;
                        taskExport.SetChapterName($"{TaskID}_ChapterName", ChapterName, "章节名称");
                    }
                    // 任务名称
                    {
                        string TaskName = item1.Value.Base.TaskName;
                        taskExport.SetTaskName($"{TaskID}_TaskName", TaskName, "任务名称");
                    }
                    // 特殊描述
                    {
                        string SpecTaskDesc = item1.Value.Base.SpecTaskDesc;
                        taskExport.SetSpecTaskDesc($"{TaskID}_SpecTaskDesc", SpecTaskDesc, "特殊描述");
                    }
                    // 任务描述
                    {
                        string Desc = item1.Value.Base.Desc;
                        taskExport.SetTaskDesc($"{TaskID}_TaskDesc", Desc, "任务描述");
                    }
                    // -- 承接效果组
                    if (item1.Value.Condition.EditorPickEffects.Count > 0)
                    {
                        int Index = 1;
                        foreach (var curEffect in item1.Value.Condition.EditorPickEffects)
                        {
                            //对话效果
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                int DialogueID = (curEffect.baseEffect as EffectDialogue).DialogueID;  // 对话ID
                                string ShowText = curEffect.GetBaseEffect().ShowText;   // 服务显示文本
                                taskExport.SetPickEffects_ShowText(DialogueID, Index, $"{TaskID}_PickEffects_Dialogue_{DialogueID}_{Index}", ShowText, $"承接效果组-对话效果-{DialogueID}");
                                Index++;
                            }
                        }
                    }
                    // -- 交付效果组
                    if (item1.Value.TaskFinishInfo.EditorFinishEffects.Count > 0)
                    {
                        int Index = 1;
                        foreach (var curEffect in item1.Value.TaskFinishInfo.EditorFinishEffects)
                        {
                            //对话效果
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                int DialogueID = (curEffect.baseEffect as EffectDialogue).DialogueID;  // 对话ID
                                string ShowText = curEffect.GetBaseEffect().ShowText;   // 服务显示文本
                                taskExport.SetFinishEffects_ShowText(DialogueID, Index, $"{TaskID}_FinishEffects_Dialogue_{DialogueID}_{Index}", ShowText, $"交付效果组-对话效果-{DialogueID}");
                                Index++;
                            }
                        }
                    }
                    // 任务目标
                    {
                        List<Task.TaskTargetBase> Targets = item1.Value.TaskFinishInfo.Targets;
                        if (Targets != null && Targets.Count > 0)
                        {
                            int IndexTargets = 1;

                            foreach (var item2 in Targets)
                            {
                                int TargetID = item2.Target.ID;    // 目标ID
                                string TargetDesc = item2.Target.Desc; // 任务目标描述
                                //if (TargetDesc.Contains("[NPC]"))
                                //{
                                //    if (item2.EditorTaskType != null && item2.EditorTaskType.TaskType == TaskType.Dialogue)
                                //    {
                                //        TargetDesc = TargetDesc.Replace("[NPC]", "");
                                //    }
                                //}
                                //else if (TargetDesc.Contains("[MON]"))
                                //{
                                //    if (item2.EditorTaskType != null && item2.EditorTaskType.TaskType == TaskType.KillMonster)
                                //    {
                                //        TargetDesc = TargetDesc.Replace("[MON]", "");
                                //    }
                                //}
                                taskExport.SetTargetDesc(TargetID, $"{TaskID}_Targets_{TargetID}", TargetDesc, $"任务目标描述-{TargetID}");
                                // -- 效果目标ID
                                int Index = 1;
                                foreach (var curEffect in item2.Target.EditorEffects)
                                {
                                    //对话效果
                                    if (curEffect.EffectType == FunctionType.Dialogue)
                                    {
                                        int DialogueID = (curEffect.baseEffect as EffectDialogue).DialogueID;  // 对话ID
                                        string ShowText = curEffect.GetBaseEffect().ShowText;   // 服务显示文本
                                        taskExport.SetTarget_ShowText(DialogueID, $"{IndexTargets}_{Index}", $"{TaskID}_Targets_{TargetID}_Dialogue_{DialogueID}_{Index}", ShowText, $"任务目标-对话效果-{DialogueID}");
                                        Index++;
                                    }
                                }
                                IndexTargets++;
                            }
                        }
                    }

                    m_taskTextStr.Add(taskExport);
                }
            }
        }
    }

    private static void WriteTaskExcel()
    {
        var path = EditorUtility.SaveFilePanel("选择导出任务文本路径", "", "任务编辑器文本_TaskEditorText.xlsx", "*.xlsx");
        if (path != "")
        {
            FileInfo newFile = new(path);
            if (newFile.Exists)
            {
                //创建一个新的excel文件
                newFile.Delete();
                newFile = new FileInfo(path);
            }

            //通过ExcelPackage打开文件
            using (ExcelPackage package = new(newFile))
            {
                //在excel空文件添加新sheet
                ExcelWorksheet taskConfig = package.Workbook.Worksheets.Add("TaskEditorText");
                int index = 1;
                taskConfig.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                taskConfig.Cells[1, index++].Value = "任务ID";
                taskConfig.Cells[1, index++].Value = "描述";

                taskConfig.Cells[1, index++].Value = "多语言key";

                taskConfig.Cells[1, index++].Value = "文本";

                int row = 6;
                foreach (var item in m_taskTextStr)
                {
                    string taskID = $"{item.TaskID}";
                    // 章节名称
                    {
                        item.ChapterName.ExportExcel(row++, taskConfig.Cells);
                    }
                    // 任务名称
                    {
                        item.TaskName.ExportExcel(row++, taskConfig.Cells);
                    }
                    // 特殊描述
                    {
                        item.SpecTaskDesc.ExportExcel(row++, taskConfig.Cells);
                    }
                    // 任务描述
                    {
                        item.TaskDesc.ExportExcel(row++, taskConfig.Cells);
                    }
                    //--承接效果组
                    // 服务显示文本
                    {
                        foreach (var item1 in item.PickEffects_ShowText)
                        {
                            item1.Value.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // -- 交付效果组
                    // 服务显示文本
                    {
                        foreach (var item1 in item.FinishEffects_ShowText)
                        {
                            item1.Value.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // 任务目标
                    // 任务目标描述
                    {
                        foreach (var item1 in item.TargetDesc)
                        {
                            item1.Value.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // -- 效果目标ID
                    // 服务显示文本
                    {
                        foreach (var item1 in item.Target_ShowText)
                        {
                            item1.Value.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                }

                taskConfig.Cells.AutoFitColumns();
                //保存excel
                package.Save();

                EditorUtility.DisplayDialog("消息", $"一共:{row - 6}条\n\n\t导出成功", "您辛苦了");
            }
        }
    }

    #endregion

    #region 文本去重

    [MenuItem("自动化工具/多语言/导出文本去重", false, 2)]
    public static void ExportRepetitiveTextStr()
    {
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(m_TargetFilePath);
        string[] strs = textAsset.text.Split("\r\n");
        Debug.Log($"总条数=》{strs.Length}");

        HashSet<string> hashSet = new();
        foreach (var item in strs)
        {
            string str = KeepOnlyChinese(item);
            if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            hashSet.Add(str);
        }
        Debug.Log($"只保留中文后 总条数=》{hashSet.Count}");
        //HashSet<string> hashSet = new HashSet<string>(strs);
        string[] uniqueArray = hashSet.ToArray();

        StringBuilder csv = new();
        foreach (var item in uniqueArray)
        {
            if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            csv.AppendLine(item);
        }

        File.WriteAllText(m_TargetFilePath, csv.ToString());

        AssetDatabase.Refresh();

        Debug.Log($"去重总条数=》{uniqueArray.Length}");
    }

    #endregion

    #region 导出Lua文本

    [MenuItem("自动化工具/多语言/导出Lua文本", false, 2)]
    public static void ExportLuaTextStr()
    {
        ExportLuaText();

        Release();
    }

    private static void ExportLuaText()
    {
        m_LuaScriptsText.Clear();
        m_LuaScriptsText2.Clear();
        m_LuaScriptsPath.Clear();
        index = 1;

        string[] luaFiles = Directory.GetFiles(m_LuaScriptsFilePath, "*.txt", SearchOption.AllDirectories);
        foreach (string luaPath in luaFiles)
        {
            //TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(luaPath);
            ReadAllLines(luaPath);
        }

        ReplaceLocalStr();

        Debug.Log($"总文件数=》{luaFiles.Length}");

        WriteLuaText();
    }

    private static void ReadAllLines(string luaPath)
    {
        var lines = File.ReadAllLines(luaPath, System.Text.Encoding.UTF8);
        Debug.LogWarning($"读取文件路径={luaPath}");
        //AddLuaText(luaPath);
        try
        {
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                {
                    bool isHave_ = line.StartsWith("--");
                    bool isHaveOver = line.EndsWith(";");
                    bool isHaveLocalStr = line.Contains("Local_Str_");
                    {
                        //if (!isHave_ && isHaveOver && !isHaveLocalStr)
                        //{
                        //    bool isHaveText = line.Contains(".text =") || line.Contains(".text=");
                        //    if (isHaveText)
                        //    {
                        //        string[] strs = null;
                        //        if (line.Split(".text =").Length > 0)
                        //        {
                        //            strs = line.Split(".text =");
                        //        }
                        //        else if (line.Split(".text=").Length > 0)
                        //        {
                        //            strs = line.Split(".text=");
                        //        }
                        //        if (strs != null && strs.Length >= 1)
                        //        {
                        //            var textVal = strs[1];
                        //            // 第一种
                        //            //{
                        //            //    // 1. tostring(self.m_HPCur).."/"..tostring(self.m_HPSum)
                        //            //    // 2.percent.."%"
                        //            //    // 3."x"..self.m_CurHpRowCount
                        //            //    bool isHaveString = textVal.Contains("\"") || textVal.Contains("\'");
                        //            //    bool islianjie = textVal.Contains("..");
                        //            //    if (isHaveString && islianjie)
                        //            //    {
                        //            //        AddLuaText(textVal);
                        //            //    }
                        //            //}
                        //            // 第二种【匹配全部中文】
                        //            //{
                        //            //    bool isHaveString = textVal.Contains("\"") || textVal.Contains("\'");
                        //            //    bool isAllChinese = IsAllChinese(textVal);
                        //            //    if (isHaveString && isAllChinese)
                        //            //    {
                        //            //        string OnlyChinese = KeepOnlyChinese(textVal);
                        //            //        string attrName = $"Local_Str_{OnlyChinese}";
                        //            //        string LocalStr = $"{attrName} = \"{OnlyChinese}\";";
                        //            //        AddLuaPath(luaPath, textVal, attrName);
                        //            //        AddLuaText(LocalStr);
                        //            //    }
                        //            //}
                        //            // 第三种【后面加localstr】
                        //            {
                        //                bool isHaveString = textVal.Contains("\"") || textVal.Contains("\'");
                        //                bool islianjie = textVal.Contains("..");
                        //                if (isHaveString && islianjie)
                        //                {
                        //                    AddLuaText(textVal);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }

                    // 匹配 Util.ShowMessage("
                    {
                        bool isHaveShowMessage = line.Contains("Util.ShowMessage(\"") || line.Contains("Util.ShowMessage(\'");
                        if (!isHave_ && !isHaveLocalStr && isHaveShowMessage)
                        {
                            string pattern = "(\"([^\"]+)\")";

                            Match match = Regex.Match(line, pattern);
                            if (match.Success)
                            {
                                // 使用match.Groups[1].Value获取匹配到的内容，不包括双引号
                                string textVal = match.Groups[1].Value;
                                bool isAllChinese = IsAllChinese(textVal);
                                if (isAllChinese)
                                {
                                    string attrName = $"Local_Str_Auto_Tips_{index}";
                                    string LocalStr = $"{attrName} = \"{textVal}\";";
                                    textVal = textVal.Replace("\"", string.Empty);
                                    bool res = AddLuaText2(LocalStr, textVal);
                                    if (res)
                                    {
                                        AddLuaPath(luaPath, textVal, attrName);
                                        Debug.Log($"index={index},luaPath={luaPath},attrName={attrName},LocalStr={LocalStr}");
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred: " + e.Message);
        }
    }

    private static void AddLuaPath(string filePath, string replaceStr, string newStr)
    {
        if (!m_LuaScriptsPath.ContainsKey(filePath))
        {
            m_LuaScriptsPath.Add(filePath, new());
        }
        if (m_LuaScriptsPath.TryGetValue(filePath, out var dic))
        {
        }

        if (!dic.ContainsKey(newStr))
        {
            dic.Add(newStr, new());
        }
        if (dic.TryGetValue(newStr, out var list))
        {
        }

        if (!list.Contains(replaceStr))
        {
            list.Add(replaceStr);
        }
    }

    private static void AddLuaText(string str)
    {
        str = str.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Replace(";", string.Empty);

        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        if (m_LuaScriptsText.Contains(str))
        {
            return;
        }
        m_LuaScriptsText.Add(str);
    }

    private static bool AddLuaText2(string str, string str2)
    {
        str = str.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Replace(";", string.Empty);
        str2 = str2.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Replace(";", string.Empty);

        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        if (string.IsNullOrEmpty(str2) || string.IsNullOrWhiteSpace(str2))
        {
            return false;
        }

        //if (m_LuaScriptsText2.Contains(str2))
        //{
        //    return false;
        //}

        if (m_LuaScriptsText.Contains(str))
        {
            return false;
        }
        m_LuaScriptsText.Add(str);
        m_LuaScriptsText2.Add(str2);

        return true;
    }

    private static void ReplaceLocalStr()
    {
        foreach (var path in m_LuaScriptsPath)
        {
            string fileText = File.ReadAllText(path.Key);
            foreach (var newStrItem in path.Value)
            {
                string newStr = newStrItem.Key;
                foreach (var replaceStrItem in newStrItem.Value)
                {
                    fileText = Regex.Replace(fileText, replaceStrItem, newStr);
                }
            }
            File.WriteAllText(path.Key, fileText);
        }
    }

    private static void WriteLuaText()
    {
        StringBuilder csv = new();
        foreach (var item in m_LuaScriptsText)
        {
            csv.AppendLine(item);
        }

        Debug.Log($"总条数=》{m_LuaScriptsText.Count}");

        File.WriteAllText(m_TargetFilePath, csv.ToString());

        AssetDatabase.Refresh();
    }

    #endregion

    #region Lua文本去重

    [MenuItem("自动化工具/多语言/Lua文本去重", false, 2)]
    public static void CheckLuaLocalStr()
    {
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(m_TargetFilePath);
        string[] strs = textAsset.text.Split("\r\n");
        Debug.Log($"总条数=》{strs.Length}");

        List<string> str2 = new();
        List<string> str3 = new();

        foreach (var item in strs)
        {
            if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            string[] strings = item.Split(" = ");
            if (str2.Contains(strings[0]))
            {
                Debug.LogWarning($"有重复的key={strings[0]}");
            }
            else
            {
                str2.Add(strings[0]);
            }
            string key = strings[1];
            key.Replace(";", string.Empty);
            if (str3.Contains(key))
            {
                Debug.LogWarning($"有重复的值={key}");
            }
            else
            {
                str3.Add(key);
            }
        }

    }

    #endregion

    private static string KeepOnlyChinese(string input)
    {
        return Regex.Replace(input, @"[^\u4e00-\u9fa5]", "");
    }

    private static bool IsAllChinese(string str)
    {
        str = str.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Replace(";", string.Empty).Replace("\"", string.Empty).Replace("\\", string.Empty).Replace(" ", string.Empty);
        str.Trim();
        return Regex.IsMatch(str, @"^[\u4e00-\u9fa5]+$");
    }

    /// <summary>
    /// 判断字符串中是否包含中文
    /// </summary>
    /// <param name="str">需要判断的字符串</param>
    /// <returns>判断结果</returns>
    public static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }

    /// <summary>
    /// 通过正则表达式判定判断字符串是否为全数字
    /// </summary>
    private static bool IsNumber(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        //const string pattern = "^[0-9]*$";
        const string pattern = @"^[\d!￥$?@%#+-~/*()（）=：:.!！ⅠⅡⅢⅣⅥⅦⅧⅨⅩ  ]*$";
        Regex rx = new(pattern);
        return rx.IsMatch(s);
    }

    private static void Release()
    {
        m_prefabTextStr.Clear();
        m_prefabTextStrCopy.Clear();
        m_prefabTextStr2.Clear();

        m_mapdataTextStr.Clear();
        m_mapdataTextStr2.Clear();
        m_mapTextStr.Clear();

        m_taskTextStr.Clear();

        m_LuaScriptsText.Clear();
        m_LuaScriptsText2.Clear();
        m_LuaScriptsPath.Clear();
    }

}