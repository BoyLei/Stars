///--------------------------------------------------------------------
///--------------------------------------------------------------------
/// 文件名   :   TaskWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/15 21:07:44
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using MapEditor;
using MessagePack;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using SkillEditor;
using StarProject.Service.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Task;
using UnityEditor;
using UnityEngine;

public partial class TaskWindow : OdinMenuEditorWindow
{
    [MenuItem("Tools/任务编辑器")]
    public static void OpenWindow()
    {
        TaskWindow window = GetWindow<TaskWindow>("任务");

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        //TaskMenuWindow.Open();
    }

    protected override void OnEnable()
    {
        LanguageManager.Instance.TaskEditorOpened = true;
    }

    protected override void OnDisable()
    {
        LanguageManager.Instance.TaskEditorOpened = false;
    }

    //运行时数据
    Dictionary<TaskClassifyType, Dictionary<uint, TaskConfigInfo>> Tasks =
        new Dictionary<TaskClassifyType, Dictionary<uint, TaskConfigInfo>>();

    //运行时数据
    Dictionary<uint, TaskConfigInfo> TaskMaps = new Dictionary<uint, TaskConfigInfo>();

    Dictionary<uint, TaskTypeInfo> TaskTypeList = new Dictionary<uint, TaskTypeInfo>();
    List<TaskClassifyType> taskClassifyTypes = new List<TaskClassifyType>();

    //备份数据用于 还原
    public Dictionary<uint, TaskConfigInfo> OldDatasMap = new Dictionary<uint, TaskConfigInfo>();

    Dictionary<uint, TaskMenuItem> OdinMenuItems = new Dictionary<uint, TaskMenuItem>();

    /*类型++链ID*/
    public Dictionary<uint, TaskIDGen> TaskIDs = new Dictionary<uint, TaskIDGen>();

    private Dictionary<string, List<string>> ExistsFiles = new Dictionary<string, List<string>>();

    private string TaskTypeFile = string.Empty;

    public static System.Action<int> TaskMenuWindowEvent;

    public uint StoreValues(ushort height, ushort low)
    {
        uint result = ((uint)(height << 16) | low);
        return result;
    }

    private TaskConfigInfo CurrentSelect = null;

    static T DeepCopy<T>(T RealObject)
    {
        try
        {
            using (Stream objectStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        return default;
    }

    private void InitExistsFile()
    {
        ExistsFiles.Clear();
        TaskTypeFile = taskTypeName;
        var files = System.IO.Directory.GetFiles(taskSettings.JsonPathDir);

        foreach (var file in files)
        {
            if (file.EndsWith(".meta"))
            {
                continue;
            }

            var fileName = System.IO.Path.GetFileName(file);
            if (fileName.StartsWith("Task_"))
            {
                var sp = fileName.Split("_");
                var key = string.Join(sp[0], sp[1]);

                if (ExistsFiles.ContainsKey(key))
                {
                    ExistsFiles[key].Add(fileName);
                }
                else
                {
                    var ls = new List<string>();
                    ls.Add(fileName);
                    ExistsFiles.Add(key, ls);
                }
            }
        }
    }

    //初始化
    protected override void Initialize()
    {
        base.Initialize();

        TaskIDs.Clear();
        Tasks.Clear();
        TaskMaps.Clear();
        TaskMenuWindowEvent = OnTaskMenuWindowEvent;
        LoadPrefs();
    }
    private void LoadTaskTypeConfig()
    {
        if (!string.IsNullOrEmpty(TaskTypeFile))
        {
            TaskTypeList.Clear();
            if (File.Exists($"{taskSettings.JsonPathDir}/{TaskTypeFile}.json"))
            {
                var jsonText = System.IO.File.ReadAllText($"{taskSettings.JsonPathDir}/{TaskTypeFile}.json");
                if (jsonText != null)
                {
                    var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskConfigs>(jsonText);
                    if (taskConfigs != null && taskConfigs.typelist != null)
                    {
                        foreach (var item in taskConfigs.typelist)
                        {
                            TaskTypeInfo info = DeepCopy<TaskTypeInfo>(item.Value);
                            TaskTypeList.Add((uint)info.TaskType, info);
                        }
                        return;
                    }
                }
            }
            Array values = Enum.GetValues(typeof(TaskClassifyType));

            // 遍历数组  
            foreach (int value in values)
            {
                TaskTypeInfo info = new TaskTypeInfo()
                {
                    TaskType = value,
                    CanDelete = false,
                    CanAgain = false,
                    GetTips = false,
                    FinishTips = true,
                    ShowProcess = false,
                    IconPath = string.Empty
                };
                TaskTypeList.Add((uint)value, info);
            }
        }
    }
    private void LoadTaskConfig(string fileName)
    {
        //var jsonText = AssetDatabase.LoadAssetAtPath<TextAsset>($"{taskSettings.JsonPathDir}/{fileName}");
        var jsonText = System.IO.File.ReadAllText($"{taskSettings.JsonPathDir}/{fileName}");
        if (jsonText != null)
        {
            var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskConfigs>(jsonText);
            List<TaskClassifyType> containTypeList = new List<TaskClassifyType>();
            if (taskConfigs != null && taskConfigs.list != null)
            {
                foreach (var item in taskConfigs.list)
                {
                    TaskConfigInfo info = DeepCopy<TaskConfigInfo>(item.Value);
                    TaskConfigInfo runingInfo = DeepCopy<TaskConfigInfo>(item.Value);
                    if (!containTypeList.Contains(info.Base.TaskType))
                    {
                        containTypeList.Add(info.Base.TaskType);
                    }

                    uint t_ckey = StoreValues((ushort)info.Base.TaskType, info.Base.ChainID);
                    //初始化
                    if (!TaskIDs.ContainsKey(t_ckey))
                    {
                        TaskIDs.Add(t_ckey, new TaskIDGen(info.Base.TaskType, info.Base.ChainID));
                    }

                    uint index = info.Base.UIndex;

                    var msg = TaskIDs[t_ckey].SetData(index, 1, false);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        ShowNotification(new GUIContent(msg));
                    }

                    if (!TaskMaps.ContainsKey(runingInfo.Base.TaskID))
                    {
                        TaskMaps.Add(runingInfo.Base.TaskID, runingInfo);
                    }

                    if (OldDatasMap.ContainsKey(info.Base.TaskID))
                    {
                        OldDatasMap[info.Base.TaskID] = info;
                    }
                    else
                    {
                        OldDatasMap.Add(info.Base.TaskID, info);
                    }

                    if (Tasks.ContainsKey(runingInfo.Base.TaskType))
                    {
                        if (!Tasks[runingInfo.Base.TaskType].ContainsKey(runingInfo.Base.TaskID))
                        {
                            Tasks[runingInfo.Base.TaskType].Add(runingInfo.Base.TaskID, runingInfo);
                        }
                    }
                    else
                    {
                        var infos = new Dictionary<uint, TaskConfigInfo>();
                        infos.Add(runingInfo.Base.TaskID, runingInfo);
                        Tasks.Add(runingInfo.Base.TaskType, infos);
                    }
                    if (!info.Base.CanCustomTypeConfig)//如果没有用自定义 则赋值
                    {
                        if (TaskTypeList.TryGetValue((uint)info.Base.TaskType, out var t))
                        {
                            info.Base.CanDelete = t.CanDelete;
                            info.Base.CanAgain = t.CanAgain;
                            info.Base.GetTips = t.GetTips;
                            info.Base.ShowProcess = t.ShowProcess;
                            info.Base.FinishTips = t.FinishTips;
                        }
                    }
                    if (!runingInfo.Base.CanCustomTypeConfig)//如果没有用自定义 则赋值
                    {
                        if (TaskTypeList.TryGetValue((uint)runingInfo.Base.TaskType, out var t))
                        {
                            runingInfo.Base.CanDelete = t.CanDelete;
                            runingInfo.Base.CanAgain = t.CanAgain;
                            runingInfo.Base.GetTips = t.GetTips;
                            runingInfo.Base.ShowProcess = t.ShowProcess;
                            runingInfo.Base.FinishTips = t.FinishTips;
                        }
                    }
                }
                foreach (var taskType in containTypeList)
                {
                    InitTaskByType(taskType);
                }
                tree.SortMenuItemsByName(false);
            }
        }
    }

    private void OnTaskMenuWindowEvent(int EventType)
    {
        switch (EventType)
        {
            case 1:
                ExportJson();
                break;
            case 2:
                CommitAction();
                break;
            case 3:
                DeleteSelectAction();
                break;
            case 4:
                CopyToClient();
                break;
            case 5:
                CopyToServer();
                break;
        }
    }

    public void CopyToClient()
    {
        // 检查taskSettings.ClientPath是否存在  
        if (!Directory.Exists(taskSettings.ClientPath))
        {
            // 如果不存在，则打开选择目录的选项框  
            var tmp = EditorUtility.SaveFolderPanel("选择客户端所在目录", "", "");
            if (tmp != "")
            {
                // 更新taskSettings.ClientPath  
                taskSettings.ClientPath = tmp;
                // 保存设置  
                EditorPrefs.SetString(settingsPath, JsonUtility.ToJson(taskSettings));

            }
        }
        // 执行复制操作  
        Copy(taskSettings.ClientPath, false, true);
        // 显示完成消息  
        EditorUtility.DisplayDialog("消息", "客户端目录复制完成", "ok");
    }
    public void CopyToServer()
    {
        // 检查taskSettings.ServerPath  
        if (!Directory.Exists(taskSettings.ServerPath))
        {
            // 如果不存在，则打开选择目录的选项框  
            var tmp = EditorUtility.SaveFolderPanel("选择服务器所在目录", "", "");
            if (tmp != "")
            {
                // 更新taskSettings.ServerPath  
                taskSettings.ServerPath = tmp;
                // 保存设置  
                EditorPrefs.SetString(settingsPath, JsonUtility.ToJson(taskSettings));

            }
        }
        // 执行复制操作  
        Copy(taskSettings.ServerPath, true, false);
        // 显示完成消息  
        EditorUtility.DisplayDialog("消息", "服务器目录复制完成", "ok");
    }

    public void Copy(string TargetPath, bool saveJson, bool saveBinary)
    {
        if (string.IsNullOrEmpty(TargetPath))
        {
            return;
        }

        if (!System.IO.Directory.Exists(TargetPath))
        {
            System.IO.Directory.CreateDirectory(TargetPath);
        }

        string filePath = TargetPath + "/TaskConfigs.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }


        var files = System.IO.Directory.GetFiles(taskSettings.JsonPathDir);

        List<string> loadings = new List<string>();
        foreach (var file in files)
        {
            if (file.EndsWith(".meta"))
            {
                continue;
            }

            var fileName = System.IO.Path.GetFileName(file);
            if (fileName.StartsWith("Task_") || fileName.Contains(taskTypeName))
            {
                loadings.Add(file);
            }
        }

        TaskConfigs configs = new TaskConfigs();
        configs.list.Clear();
        configs.typelist.Clear();
        foreach (var load in loadings)
        {
            var text = File.ReadAllText(load);
            var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskConfigs>(text);
            if (taskConfigs != null && taskConfigs.list != null)
            {
                foreach (var cfg in taskConfigs.list)
                {
                    if (configs.list.ContainsKey(cfg.Key))
                    {
                        configs.list[cfg.Key] = cfg.Value;
                    }
                    else
                    {
                        configs.list.Add(cfg.Key, cfg.Value);
                    }
                }
            }
            if (taskConfigs != null && taskConfigs.typelist != null)
            {
                foreach (var cfg in taskConfigs.typelist)
                {
                    if (configs.typelist.ContainsKey(cfg.Key))
                    {
                        configs.typelist[cfg.Key] = cfg.Value;
                    }
                    else
                    {
                        configs.typelist.Add(cfg.Key, cfg.Value);
                    }
                }
            }
        }

        JsonSerializerSettings setting = new JsonSerializerSettings();
        setting.NullValueHandling = NullValueHandling.Ignore;
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(configs, setting);
        if (saveJson)
        {
            WriteJson(content, filePath);
        }

        if (saveBinary)
        {
            // 保存任务MessagePack二进制文件
            byte[] byteArrary = MessagePackSerializer.Serialize<TaskConfigs>(configs);
            string filePathBinary = TargetPath + "/TaskConfigsBin.bytes";
            System.IO.File.WriteAllBytes(filePathBinary, byteArrary);
        }

        //var soure = File.ReadAllBytes(taskSettings.JsonPath);
        //File.WriteAllBytes(filePath, soure);
        AssetDatabase.Refresh();
    }

    public bool HasTask(uint taskID)
    {
        return TaskMaps.ContainsKey(taskID);
    }

    public void DeleteTask(TaskClassifyType TaskType, uint taskID)
    {
        DeleteTaskInner(TaskType, taskID);
        ForceMenuTreeRebuild();
    }


    public void DeleteTaskInner(TaskClassifyType TaskType, uint taskID)
    {
        if (TaskMaps.ContainsKey(taskID))
        {
            TaskMaps.Remove(taskID);
        }

        if (Tasks.ContainsKey(TaskType))
        {
            if (Tasks[TaskType].ContainsKey(taskID))
            {
                uint PreID = Tasks[TaskType][taskID].Base.PreTaskID;
                uint NextID = Tasks[TaskType][taskID].Base.NextTaskID;


                //前置任务的后置 修改成 NextID
                if (HasTask(PreID))
                {
                    if (TaskMaps[PreID].Base.HasNext)
                    {
                        TaskMaps[PreID].Base.NextTaskID = NextID;
                    }
                }

                //后置任务的前置 修改成 PreID
                if (HasTask(NextID))
                {
                    if (TaskMaps[NextID].Base.HasPre)
                    {
                        TaskMaps[NextID].Base.PreTaskID = PreID;
                    }
                }

                var cfg = Tasks[TaskType][taskID];
                if (cfg != null)
                {
                    var key = StoreValues((ushort)cfg.Base.TaskType, cfg.Base.ChainID);

                    var msg = TaskIDs[key].SetData(cfg.Base.UIndex, 0);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        ShowNotification(new GUIContent(msg));
                    }
                }

                Tasks[TaskType].Remove(taskID);
            }
        }
    }

    public OdinMenuTree tree;

    public string GetTaskTypeName(TaskClassifyType TaskType)
    {
        string typeName = "";
        switch (TaskType)
        {
            case TaskClassifyType.MainLine:
                typeName = $"{(int)TaskType}.主线";
                break;
            case TaskClassifyType.SubbranchLine:
                typeName = $"{(int)TaskType}.支线";
                break;
            case TaskClassifyType.Biography:
                typeName = $"{(int)TaskType}.传记";
                break;
            case TaskClassifyType.Play:
                typeName = $"{(int)TaskType}.日常";
                break;
            case TaskClassifyType.Guide:
                typeName = $"{(int)TaskType}.引导";
                break;
            case TaskClassifyType.Other:
                typeName = $"{(int)TaskType}.其他";
                break;
            case TaskClassifyType.Challenge:
                typeName = $"{(int)TaskType}.挑战";
                break;
        }

        return typeName;
    }

    public TaskClassifyType GetTaskClassifyType(string type, out bool find)
    {
        find = false;
        TaskClassifyType classifyType = TaskClassifyType.MainLine;
        switch (type)
        {
            case "主线":
                classifyType = TaskClassifyType.MainLine;
                find = true;
                break;
            case "支线":
                classifyType = TaskClassifyType.SubbranchLine;
                find = true;
                break;
            case "传记":
                classifyType = TaskClassifyType.Biography;
                find = true;
                break;
            case "日常":
                classifyType = TaskClassifyType.Play;
                find = true;
                break;
            case "引导":
                classifyType = TaskClassifyType.Guide;
                find = true;
                break;
            case "其他":
                classifyType = TaskClassifyType.Other;
                find = true;
                break;
            case "挑战":
                classifyType = TaskClassifyType.Challenge;
                find = true;
                break;
        }

        return classifyType;
    }

    private void InitTaskByType(TaskClassifyType classifyType, bool tilie = false)
    {
        if (!taskClassifyTypes.Contains(classifyType))
        {
            //类型
            string Name = GetTaskTypeName(classifyType);
            OdinMenuItem odin = new OdinMenuItem(tree, Name, Name);
            odin.OnRightClick = this.MenuRight_clicked;
            if (TaskTypeList.TryGetValue((uint)classifyType, out var t))
            {
                odin.Value = t;
            }
            tree.MenuItems.Add(odin);
            taskClassifyTypes.Add(classifyType);
        }

        if (Tasks.ContainsKey(classifyType))
        {
            foreach (var item in Tasks[classifyType])
            {
                item.Value.Base.OnFreshItemName = OnFreshItemName;
                item.Value.ContinesTask = HasTask;
                item.Value.Base.ModifyIndexAction = ModifyIndex;
                item.Value.DeleteTaskCall = DeleteTask;
                TaskMenuItem odin = new TaskMenuItem(tree, item.Value);
                odin.OnRightClick = this.MenuRight_clicked;
                if (!OdinMenuItems.ContainsKey(item.Key))
                {
                    OdinMenuItems.Add(item.Key, odin);
                }

                var li = tree.AddMenuItemAtPath(item.Value.GetTaskTypeName(), odin).ToList();
                foreach (var modin in li)
                {
                    if (modin.Value != null) continue;
                    //如果value为null表示是新建的节点，需要设置value；
                    if (modin.Name == $"{item.Value.Base.Chapter}.{item.Value.Base.ChapterName}")
                    {
                        modin.OnRightClick = this.MenuRight_clicked;
                        var c = new ChapterInfo()
                        {
                            taskClassify = classifyType,
                            ID = item.Value.Base.Chapter,
                            Name = item.Value.Base.ChapterName,
                        };
                        c.OnButSave_Clicked += (obj) => { ButChapterUpdata_clicked(modin, obj); };
                        modin.Value = c;
                    }
                    else if (modin.Name == $"{item.Value.Base.ChainID}")
                    {
                        modin.OnRightClick = this.MenuRight_clicked;
                        var v = new ChainInfo()
                        {
                            taskClassify = classifyType,
                            ID = item.Value.Base.Chapter,
                            Name = item.Value.Base.ChapterName,
                            ChainID = item.Value.Base.ChainID
                        };
                        v.OnButSave_Clicked += (obj) => { ButChainUpdata_clicked(modin, obj); };
                        modin.Value = v;
                    }
                }

                odin.BindCall(OnAddTaskItem);
            }
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        tree = new OdinMenuTree(true);
        OdinMenuItems.Clear();
        var customMenuStyle = new OdinMenuStyle
        {
            BorderPadding = 0f,
            AlignTriangleLeft = false,
            TriangleSize = 16f,
            TrianglePadding = 0f,
            Offset = 30,
            Height = 23,
            IconPadding = 0f,
            BorderAlpha = 0.323f,
        };
        tree.DefaultMenuStyle = customMenuStyle;
        tree.Config.DrawSearchToolbar = true;
        foreach (var taskType in taskClassifyTypes)
        {
            InitTaskByType(taskType);
        }
        tree.Selection.SelectionChanged += SelectionChanged;

        tree.EnumerateTree()
            .AddThumbnailIcons()
            .SortMenuItemsByName(false);
        return tree;
    }


    private void CommitAction()
    {
        MapEditorUtils.RunBat(EditorConfigUtils.TortoiseProc,
            string.Format($"/command:commit /path:{taskSettings.JsonPathDir} -m 修改 {taskSettings.JsonPathDir}"));
    }

    public void DeleteSelectAction()
    {
        if (tree != null && tree.Selection.SelectedValues != null)
        {
            StringBuilder taskstrli = new StringBuilder();
            foreach (var item in tree.Selection.SelectedValues)
            {
                if (item is TaskConfigInfo task)
                {
                    taskstrli.AppendLine(task.TaskTile);
                }
            }

            if (UnityEditor.EditorUtility.DisplayDialog("删除确定", $"是否确定删除选中的任务：\n{taskstrli.ToString()}", "确定", "取消"))
            {
                foreach (var item in tree.Selection.SelectedValues)
                {
                    if (item is TaskConfigInfo task && task != null)
                    {
                        DeleteTaskInner(task.Base.TaskType, task.Base.TaskID);
                    }
                }

                ForceMenuTreeRebuild();
            }
        }
    }


    private void SelectionChanged(SelectionChangedType changedType)
    {
        if (changedType == SelectionChangedType.ItemAdded)
        {
            if (tree.Selection.SelectedValue is TaskConfigInfo info)
            {
                if (CurrentSelect != null)
                {
                    if (CurrentSelect.IsDirty)
                    {
                        if (!UnityEditor.EditorUtility.DisplayDialog("提示", $"任务{CurrentSelect.Base.TaskID}有修改是否保存",
                                "保存", "不保存"))
                        {
                            if (OldDatasMap.ContainsKey(CurrentSelect.Base.TaskID))
                            {
                                var temp = info = DeepCopy<TaskConfigInfo>(OldDatasMap[CurrentSelect.Base.TaskID]);
                                if (Tasks.ContainsKey(CurrentSelect.Base.TaskType))
                                {
                                    if (Tasks[CurrentSelect.Base.TaskType].ContainsKey(CurrentSelect.Base.TaskID))
                                    {
                                        Tasks[CurrentSelect.Base.TaskType][CurrentSelect.Base.TaskID] = temp;
                                    }
                                }

                                if (OdinMenuItems.ContainsKey(CurrentSelect.Base.TaskID))
                                {
                                    OdinMenuItems[CurrentSelect.Base.TaskID].Value = temp;
                                    OdinMenuItems[CurrentSelect.Base.TaskID].Name = temp.TaskTile;
                                }
                            }
                            else
                            {
                                DeleteTask(CurrentSelect.Base.TaskType, CurrentSelect.Base.TaskID);
                            }
                        }
                        CurrentSelect.IsDirty = false;

                    }
                }

                CurrentSelect = info;
            }
        }
    }

    public void ExportJson()
    {
        string Log = string.Empty;
        Dictionary<string, List<TaskConfigInfo>> savemap = new Dictionary<string, List<TaskConfigInfo>>();
        foreach (var item in Tasks)
        {
            foreach (var it in item.Value)
            {
                string fileName = $"Task_{it.Value.Base.TaskType}_{it.Value.Base.Chapter}";
                if (savemap.ContainsKey(fileName))
                {
                    savemap[fileName].Add(it.Value);
                }
                else
                {
                    List<TaskConfigInfo> list = new List<TaskConfigInfo>();
                    list.Add(it.Value);
                    savemap.Add(fileName, list);
                }

                string log = it.Value.CheckTask();
                if (!string.IsNullOrEmpty(log))
                {
                    Log += log;
                }
            }
        }
        //保存任务类型json
        var cfg = new TaskConfigs();
        cfg.typelist = TaskTypeList;
        JsonSerializerSettings setting = new JsonSerializerSettings();
        setting.NullValueHandling = NullValueHandling.Ignore;
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(cfg, setting);
        WriteJson(content, $"{taskSettings.JsonPathDir}/{taskTypeName}.json");

        foreach (var file in savemap)
        {
            SaveTaskConfig(file.Key, file.Value);
        }

        WriteLog(Log, taskSettings.LogPath);
        if (!string.IsNullOrEmpty(Log))
        {
            UnityEditor.EditorUtility.DisplayDialog("错误日志", Log, "确定");
            EditorUtility.OpenWithDefaultApp(taskSettings.LogPath);
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("消息", "保存成功。", "确定");
        }

        AssetDatabase.Refresh();
    }


    public void SaveTaskConfig(string fileName, List<TaskConfigInfo> list)
    {
        var cfg = new TaskConfigs();
        cfg.list.Clear();
        foreach (var info in list)
        {
            cfg.list.Add(info.Base.TaskID, info);
            //根据任务类型覆盖每个任务的类型配置
            if (!info.Base.CanCustomTypeConfig)//如果没有用自定义 则赋值
            {
                if (TaskTypeList.TryGetValue((uint)info.Base.TaskType, out var t))
                {
                    info.Base.CanDelete = t.CanDelete;
                    info.Base.CanAgain = t.CanAgain;
                    info.Base.GetTips = t.GetTips;
                    info.Base.ShowProcess = t.ShowProcess;
                    info.Base.FinishTips = t.FinishTips;
                }
            }
        }

        JsonSerializerSettings setting = new JsonSerializerSettings();
        setting.NullValueHandling = NullValueHandling.Ignore;
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(cfg, setting);
        WriteJson(content, $"{taskSettings.JsonPathDir}/{fileName}.json");
    }

    public void WriteLog(string content, string path)
    {
#if UNITY_EDITOR

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
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void WriteJson(string content, string path)
    {
#if UNITY_EDITOR

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileStream fs = new FileStream(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void OnAddTaskItem(TaskClassifyType classifyType, int ChapterID, string ChapterName, int TaskIndex,
        string TaskName, string TaskDesc, ushort chaiID)
    {
        TaskConfigInfo info = new TaskConfigInfo();
        info.Base.Index = TaskIndex;
        info.Base.Chapter = ChapterID;

        info.Base.ChainID = chaiID;
        info.Base.ChapterName = ChapterName;
        uint idKey = StoreValues((ushort)classifyType, chaiID);
        if (!TaskIDs.ContainsKey(idKey))
        {
            TaskIDs.Add(idKey, new TaskIDGen(classifyType, chaiID));
        }

        var data = TaskIDs[idKey].GetNextIndex();
        if (!data.result)
        {
            ShowNotification(new GUIContent("添加任务失败，已经超任务链上限"));
            return;
        }

        info.Base.UIndex = data.Index;
        info.Base.TaskType = classifyType;
        info.Base.TaskID = GetIndex((uint)info.Base.TaskType, info.Base.ChainID, info.Base.UIndex);
        info.Base.TaskName = TaskName;
        info.Base.Desc = TaskDesc;
        info.Base.HasPre = true;
        //info.Base.PreTaskID = info.Base.TaskID - 1;
        info.Base.OnFreshItemName = OnFreshItemName;
        info.ContinesTask = HasTask;
        info.Base.ModifyIndexAction = ModifyIndex;
        info.DeleteTaskCall = DeleteTask;
        var msg = TaskIDs[idKey].SetData(data.Index, 1);
        if (!string.IsNullOrEmpty(msg))
        {
            ShowNotification(new GUIContent(msg));
        }

        TaskMenuItem odin = new TaskMenuItem(tree, info);
        if (!OdinMenuItems.ContainsKey(info.Base.TaskID))
        {
            OdinMenuItems.Add(info.Base.TaskID, odin);
        }

        if (Tasks.ContainsKey(classifyType))
        {
            Tasks[classifyType].Add(info.Base.TaskID, info);
        }
        else
        {
            var types = new Dictionary<uint, TaskConfigInfo>();
            types.Add(info.Base.TaskID, info);
            Tasks.Add(classifyType, types);
        }

        if (!TaskMaps.ContainsKey(info.Base.TaskID))
        {
            TaskMaps.Add(info.Base.TaskID, info);
        }

        tree.AddMenuItemAtPath(info.GetTaskTypeName(), odin);
        //odin.BindCall(OnAddTaskItem);
        // ModifyIndex(info.Base.TaskType, info.Base.TaskID,true);

        SortItems();

        // int PreID = Tasks[TaskType][TaskID].Base.PreTaskID;
        //  int NextID = Tasks[TaskType][TaskID].Base.NextTaskID;

        //更改
        if (OdinMenuItems.ContainsKey(info.Base.TaskID))
        {
            var list = tree.EnumerateTree().ToList();
            var item = OdinMenuItems[info.Base.TaskID];
            int index = list.IndexOf(item);
            int pre = index - 1;
            int next = index + 1;

            TaskMaps[info.Base.TaskID].Base.PreTaskID = 0;
            if (pre > -1 && pre < list.Count)
            {
                if (list[pre] is TaskWindow.TaskMenuItem skill && skill.Value is TaskConfigInfo configInfo)
                {
                    if (TaskMaps[info.Base.TaskID].Base.HasPre)
                    {
                        TaskMaps[info.Base.TaskID].Base.PreTaskID = configInfo.Base.TaskID;
                    }

                    if (HasTask(configInfo.Base.TaskID))
                    {
                        if (TaskMaps[configInfo.Base.TaskID].Base.HasNext)
                        {
                            TaskMaps[configInfo.Base.TaskID].Base.NextTaskID = info.Base.TaskID;
                        }
                    }
                }
            }

            TaskMaps[info.Base.TaskID].Base.NextTaskID = 0;
            if (next > -1 && next < list.Count)
            {
                if (list[next] is TaskWindow.TaskMenuItem skill && skill.Value is TaskConfigInfo configInfo)
                {
                    if (TaskMaps[info.Base.TaskID].Base.HasNext)
                    {
                        TaskMaps[info.Base.TaskID].Base.NextTaskID = configInfo.Base.TaskID;
                    }

                    if (HasTask(configInfo.Base.TaskID))
                    {
                        if (TaskMaps[configInfo.Base.TaskID].Base.HasPre)
                        {
                            TaskMaps[configInfo.Base.TaskID].Base.PreTaskID = info.Base.TaskID;
                        }
                    }
                }
            }
        }
    }
    private void OnCopyTaskItem(TaskConfigInfo taskInfo)
    {
        TaskClassifyType taskType = taskInfo.Base.TaskType;
        uint taskID = taskInfo.Base.TaskID;
        TaskConfigInfo info = null;
        var jsonText = System.IO.File.ReadAllText($"{taskSettings.JsonPathDir}/Task_{taskInfo.Base.TaskType.ToString()}_{taskInfo.Base.Chapter}.json");
        if (jsonText != null)
        {
            var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskConfigs>(jsonText);
            if (taskConfigs != null && taskConfigs.list != null)
            {
                foreach (var item in taskConfigs.list)
                {
                    if (taskInfo.Base.TaskID == item.Value.Base.TaskID)
                    {
                        info = DeepCopy<TaskConfigInfo>(item.Value);
                        break;
                    }
                }
            }
        }
        info.ContinesTask = HasTask;
        if (info == null) return;
        uint idKey = StoreValues((ushort)info.Base.TaskType, info.Base.ChainID);
        if (!TaskIDs.ContainsKey(idKey))
        {
            TaskIDs.Add(idKey, new TaskIDGen(info.Base.TaskType, info.Base.ChainID));
        }

        var data = TaskIDs[idKey].GetNextIndex();
        if (!data.result)
        {
            ShowNotification(new GUIContent("添加任务失败，已经超任务链上限"));
            return;
        }
        info.Base.UIndex = data.Index;
        info.Base.TaskID = GetIndex((uint)info.Base.TaskType, info.Base.ChainID, info.Base.UIndex);

        var msg = TaskIDs[idKey].SetData(data.Index, 1);
        if (!string.IsNullOrEmpty(msg))
        {
            ShowNotification(new GUIContent(msg));
        }

        TaskMenuItem odin = new TaskMenuItem(tree, info);
        if (!OdinMenuItems.ContainsKey(info.Base.TaskID))
        {
            OdinMenuItems.Add(info.Base.TaskID, odin);
        }

        if (Tasks.ContainsKey(info.Base.TaskType))
        {
            Tasks[info.Base.TaskType].Add(info.Base.TaskID, info);
        }
        else
        {
            var types = new Dictionary<uint, TaskConfigInfo>();
            types.Add(info.Base.TaskID, info);
            Tasks.Add(info.Base.TaskType, types);
        }

        if (!TaskMaps.ContainsKey(info.Base.TaskID))
        {
            TaskMaps.Add(info.Base.TaskID, info);
        }

        tree.AddMenuItemAtPath(info.GetTaskTypeName(), odin);

        SortItems();
        //更改
        if (OdinMenuItems.ContainsKey(info.Base.TaskID))
        {
            var list = tree.EnumerateTree().ToList();
            var item = OdinMenuItems[info.Base.TaskID];
            int index = list.IndexOf(item);
            int pre = index - 1;
            int next = index + 1;

            TaskMaps[info.Base.TaskID].Base.PreTaskID = 0;
            TaskMaps[info.Base.TaskID].Base.NextTaskID = 0;
        }
    }
    private void OnClickMenu(OdinMenuItem menuItem)
    {
        if (!string.IsNullOrEmpty(menuItem.Name))
        {
            TaskClassifyType classifyType = GetTaskClassifyType(menuItem.Name, out bool find);
            if (find)
            {
                TaskPopMenu.AddChapter(classifyType, OnAddChapter);
            }
        }
    }

    private void OnAddChain(TaskClassifyType /*任务类型*/ classifyType, int /*章节ID*/ChapterID,
        string /*章节名称*/ ChapterName, ushort chainID)
    {
        string Name = GetTaskTypeName(classifyType);
        OdinMenuItem odin = new OdinMenuItem(tree, $"{chainID}", null);
        tree.AddMenuItemAtPath($"{Name}/{ChapterID}.{ChapterName}", odin);
        odin.OnRightClick = MenuRight_clicked;
        var v = new ChainInfo() { taskClassify = classifyType, ID = ChapterID, Name = ChapterName, ChainID = chainID };
        v.OnButSave_Clicked += (obj) => { this.ButChainUpdata_clicked(odin, obj); };
        odin.Value = v;
        SortItems();
    }

    private void OnAddChapter(TaskClassifyType /*任务类型*/ classifyType, int /*章节ID*/ChapterID,
        string /*章节名称*/ ChapterName)
    {
        string Name = GetTaskTypeName(classifyType);
        OdinMenuItem odin = new OdinMenuItem(tree, $"{ChapterID}.{ChapterName}", null);
        tree.AddMenuItemAtPath($"{Name}", odin);
        odin.OnRightClick = MenuRight_clicked;
        var v = new ChapterInfo() { taskClassify = classifyType, ID = ChapterID, Name = ChapterName };
        v.OnButSave_Clicked += (obj) => { this.ButChapterUpdata_clicked(odin, obj); };
        odin.Value = v;
        SortItems();
    }

    private void OnChapterRightClick(OdinMenuItem obj)
    {
        if (obj.IsSelected)
        {
            ChainInfo info = obj.Value as ChainInfo;
            if (info != null)
            {
                TaskPopMenu.AddTask(info.taskClassify, info.ID, info.Name, info.ChainID, OnAddTaskItem);
            }
        }
    }

    private void SortItems()
    {
        tree.SortMenuItemsByName(false);
    }

    public void ModifyIndex(TaskClassifyType TaskType, uint TaskID)
    {
        //更改
        if (OdinMenuItems.ContainsKey(TaskID))
        {
            //排序前
            var oldlist = tree.EnumerateTree().ToList();
            var olditem = OdinMenuItems[TaskID];
            int oldindex = oldlist.IndexOf(olditem);
            SortItems();


            //排序后
            uint PreID = Tasks[TaskType][TaskID].Base.PreTaskID;
            uint NextID = Tasks[TaskType][TaskID].Base.NextTaskID;


            var list = tree.EnumerateTree().ToList();
            var item = OdinMenuItems[TaskID];
            int index = list.IndexOf(item);
            //索引没有变化的就不需要调整
            if (oldindex == index)
            {
                return;
            }

            int pre = index - 1;
            int next = index + 1;

            TaskMaps[TaskID].Base.PreTaskID = 0;
            if (pre > -1 && pre < list.Count)
            {
                if (list[pre] is TaskWindow.TaskMenuItem skill && skill.Value is TaskConfigInfo configInfo)
                {
                    if (TaskMaps[TaskID].Base.HasPre)
                    {
                        TaskMaps[TaskID].Base.PreTaskID = configInfo.Base.TaskID;
                    }


                    //前置任务的后置 修改成 NextID
                    if (HasTask(configInfo.Base.TaskID))
                    {
                        if (TaskMaps[configInfo.Base.TaskID].Base.HasNext)
                        {
                            TaskMaps[configInfo.Base.TaskID].Base.NextTaskID = TaskID;
                        }
                    }
                }
            }

            TaskMaps[TaskID].Base.NextTaskID = 0;
            if (next > -1 && next < list.Count)
            {
                if (list[next] is TaskWindow.TaskMenuItem skill && skill.Value is TaskConfigInfo configInfo)
                {
                    if (TaskMaps[TaskID].Base.HasNext)
                    {
                        TaskMaps[TaskID].Base.NextTaskID = configInfo.Base.TaskID;
                    }

                    //后置任务的前置 修改成 PreID
                    if (HasTask(configInfo.Base.TaskID))
                    {
                        if (TaskMaps[configInfo.Base.TaskID].Base.HasPre)
                        {
                            TaskMaps[configInfo.Base.TaskID].Base.PreTaskID = TaskID;
                        }
                    }
                }
            }

            //前置任务的后置 修改成 NextID
            if (HasTask(PreID))
            {
                if (TaskMaps[PreID].Base.HasNext)
                {
                    TaskMaps[PreID].Base.NextTaskID = NextID;
                }
            }

            //后置任务的前置 修改成 PreID
            if (HasTask(NextID))
            {
                if (TaskMaps[NextID].Base.HasPre)
                {
                    TaskMaps[NextID].Base.PreTaskID = PreID;
                }
            }
        }
    }

    public class TaskMenuItem : OdinMenuItem
    {
        private readonly TaskConfigInfo instance;

        private System.Action<TaskClassifyType /*任务类型*/, int /*章节ID*/, string /*章节名称*/, int /*索引*/, string /*任务名称*/
            , string /*任务描述*/, ushort /*任务链ID*/> OnAddTaskItemCall;

        public TaskMenuItem(OdinMenuTree tree, TaskConfigInfo instance) : base(tree, instance.Base.TaskID.ToString(),
            instance)
        {
            this.instance = instance;
            SearchString = SmartName;
        }

        public void BindCall(
            System.Action<TaskClassifyType /*任务类型*/, int /*章节ID*/, string /*章节名称*/, int /*索引*/, string /*任务名称*/
                , string /*任务描述*/, ushort /*任务链ID*/> action)
        {
            if (Parent != null)
            {
                Parent.OnRightClick = OnParentRightClick;
            }

            OnAddTaskItemCall = action;
        }

        //System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string /*章节名称*/, int /*索引*/ , string/*任务名称*/, string/*任务描述*/ >

        private void OnParentRightClick(OdinMenuItem obj)
        {
            if (!obj.IsSelected)
            {
                return;
            }

            if (obj.Name == Parent.Name)
            {
                TaskPopMenu.AddTask(instance.Base.TaskType, instance.Base.Chapter, instance.Base.ChapterName,
                    instance.Base.ChainID,
                    OnAddTaskItem);
            }
        }

        private void OnAddTaskItem(TaskClassifyType classifyType, int ChapterID, string ChapterName, int TaskIndex,
            string TaskName, string TaskDesc, ushort chainID)
        {
            OnAddTaskItemCall?.Invoke(classifyType, ChapterID, ChapterName, TaskIndex, TaskName, TaskDesc, chainID);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<TaskConfigInfo>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }
        }

        public override string SmartName
        {
            get { return (this.instance.TaskTile); }
        }
    }


    public class ExportMenuItem : OdinMenuItem
    {
        public readonly ExportData instance;
        private bool isClick;

        public ExportMenuItem(OdinMenuTree tree, ExportData export) : base(tree, "导出", export)
        {
            instance = export;
            SearchString = SmartName;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<ExportData>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (!isClick && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isClick = true;
                    if (isClick)
                    {
                        instance.ExportJsonAction?.Invoke();
                    }
                }
            }


            if (Event.current.type == EventType.MouseUp)
            {
                isClick = false;
            }
        }
    }


    public class DeleteSelectMenuItem : OdinMenuItem
    {
        public readonly DeleteSelectData instance;
        private bool isClick;

        public DeleteSelectMenuItem(OdinMenuTree tree, DeleteSelectData export) : base(tree, "删除选中任务", export)
        {
            instance = export;
            SearchString = SmartName;
        }


        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<DeleteSelectData>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (!isClick && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isClick = true;
                    if (isClick)
                    {
                        instance.DeleteSelectAction?.Invoke();
                    }
                }
            }


            if (Event.current.type == EventType.MouseUp)
            {
                isClick = false;
            }
        }
    }

    public class CommitMenuItem : OdinMenuItem
    {
        public readonly CommitData instance;
        private bool isClick;

        public CommitMenuItem(OdinMenuTree tree, CommitData export) : base(tree, "提交", export)
        {
            instance = export;
            SearchString = SmartName;
        }


        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<CommitData>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (!isClick && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isClick = true;
                    if (isClick)
                    {
                        instance.CommitAction?.Invoke();
                    }
                }
            }


            if (Event.current.type == EventType.MouseUp)
            {
                isClick = false;
            }
        }
    }

    public void OnFreshItemName(uint id)
    {
        if (OdinMenuItems.ContainsKey(id) && TaskMaps.ContainsKey(id))
        {
            OdinMenuItems[id].Name = TaskMaps[id].TaskTile;
        }
    }
}

[System.Serializable]
public class ChapterInfo
{
    private IEnumerable _taskclassifytypes = new ValueDropdownList<TaskClassifyType>()
    {
        { "主线", TaskClassifyType.MainLine },
        { "支线", TaskClassifyType.SubbranchLine },
        { "传记", TaskClassifyType.Biography },
        { "日常", TaskClassifyType.Play },
        { "引导", TaskClassifyType.Guide },
        { "其他", TaskClassifyType.Other },
        { "挑战", TaskClassifyType.Challenge },
    };

    [LabelText("任务类型")]
    [ReadOnly]
    [ValueDropdown("_taskclassifytypes")]
    public TaskClassifyType taskClassify;

    [LabelText("章节ID")]
    //[DisplayAsString]
    public int ID;

    [ShowInInspector]
    [LabelText("章节名称")]
    //[DisplayAsString]
    //public string Name;
    public string Name
    {
        get
        {
#if UNITY_EDITOR
            //编辑器模式直接返回中文
            if (!Application.isPlaying)
            {
                return name;
            }
#endif
            //非编辑器模式返回key对应的语言文本
            return LanguageManager.Instance.GetLanguageByKey(Name_Key);
        }
        set { name = value; }
    }
    private string name;
    [HideInInspector]
    public string Name_Key;


    public event Action<ChapterInfo> OnButSave_Clicked;

    [Button("更  新")]
    public void OnSaveEvent()
    {
        if (this.OnButSave_Clicked != null)
        {
            this.OnButSave_Clicked(this);
        }
    }
}


[System.Serializable]
public class ChainInfo
{
    private IEnumerable _taskclassifytypes = new ValueDropdownList<TaskClassifyType>()
    {
        { "主线", TaskClassifyType.MainLine },
        { "支线", TaskClassifyType.SubbranchLine },
        { "传记", TaskClassifyType.Biography },
        { "日常", TaskClassifyType.Play },
        { "引导", TaskClassifyType.Guide },
        { "其他", TaskClassifyType.Other },
        { "挑战", TaskClassifyType.Challenge },
    };

    [LabelText("任务类型")]
    [ReadOnly]
    [ValueDropdown("_taskclassifytypes")]
    public TaskClassifyType taskClassify;

    [LabelText("章节ID")][DisplayAsString] public int ID;

    [LabelText("章节名称")]
    [DisplayAsString]
    public string Name;

    [LabelText("任务链ID")] public ushort ChainID;
    public event Action<ChainInfo> OnButSave_Clicked;

    [Button("更  新")]
    public void OnSaveEvent()
    {
        if (this.OnButSave_Clicked != null)
        {
            this.OnButSave_Clicked(this);
        }
    }
}

public class ExportData
{
    public System.Action ExportJsonAction;
}

public class DeleteSelectData
{
    public System.Action DeleteSelectAction;
}

public class CommitData
{
    public System.Action CommitAction;
}

public class TaskIDGen
{
    public TaskClassifyType TaskType;
    public uint ChainID;


    public TaskIDGen(TaskClassifyType type, uint id)
    {
        TaskType = type;
        ChainID = id;
    }

    public byte[] Table = new byte[64];


    public string SetData(uint index, byte v, bool check = true)
    {
        string message = string.Empty;
        if (index > -1 && index < 64)
        {
            if (check && Table[index] == v)
            {
                message = $"{TaskType} {ChainID} {index} 重复设置{v}";
            }
            Table[index] = v;
        }
        else
        {
            message = $"{TaskType} {ChainID} {index} 索引不在[0-63]之间";
        }

        if (!string.IsNullOrEmpty(message))
        {
            Debug.LogError(message);
        }

        return message;
    }


    public (bool result, byte Index) GetNextIndex()
    {
        byte index = 0;
        while (Table[index] == 1)
        {
            index++;
            if (index >= Table.Length)
            {
                Debug.LogError($"{TaskType} {ChainID}索引已经超上限");
                return (false, index);
            }
        }

        return (true, index);
    }
}