using Newtonsoft.Json;
using OfficeOpenXml;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SkillEditor;
using Task;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public partial class TaskWindow
{
    //事件

    #region 属性

    /// <summary>
    /// 右击菜单
    /// </summary>
    private GenericMenu RightClickMenu;

    /// <summary>
    /// 任务项上的菜单
    /// </summary>
    private GenericMenu MenuItemMenu;

    #endregion

    #region 自己定义的事件

    public event Action<Event> TrueeMouseRightClick;

    #endregion

    #region 系统

    public void CreateGUI()
    {
        LoadPrefs();
        InitExistsFile();
        TaskEnumUtils.Init();
        this.TrueeMouseRightClick += TaskWindow_TrueeMouseRightClick;
        this.RightClickMenu = new GenericMenu();
        this.RightClickMenu.AddItem(new GUIContent("复制到客户端..."), false, CopyToClient);
        this.RightClickMenu.AddItem(new GUIContent("复制到服务器..."), false, CopyToServer);
        this.RightClickMenu.AddSeparator("");
        this.RightClickMenu.AddItem(new GUIContent("首选项..."), false, OpenSettings);
        this.MenuItemMenu = new GenericMenu();
        this.MenuItemMenu.AddItem(new GUIContent("删  除"), false, Butdelete_clicked);
        this.MenuItemMenu.AddSeparator("");
        this.MenuItemMenu.AddItem(new GUIContent("新建章节"), false, Butnewchapter_clicked);
        this.MenuItemMenu.AddItem(new GUIContent("新建任务"), false, Butnewtask_clicked);
        this.MenuItemMenu.AddItem(new GUIContent("新建任务链"), false, ButnewChaintask_clicked);
        this.MenuItemMenu.AddItem(new GUIContent("复制任务"), false, ButCopyTask_clicked);
        VisualElement root = rootVisualElement;
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DevTools/TaskEditor/Editor/ToolBar.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);
            ToolbarButton but;
            but = root.Q<ToolbarButton>("butsave");
            but.clicked += Butsave_clicked;
            but = root.Q<ToolbarButton>("butdelete");
            but.clicked += Butdelete_clicked;
            but = root.Q<ToolbarButton>("butsvn");
            but.clicked += Butsvn_clicked;
            but = root.Q<ToolbarButton>("butloadcfg");
            but.clicked += Butloadcfg_clicked;
            but = root.Q<ToolbarButton>("butcopydata");
            but.clicked += Butbutcopydata_clicked;

            but = root.Q<ToolbarButton>("butmodifytaskid");
            but.clicked += Butmodifytaskid_clicked;
            but = root.Q<ToolbarButton>("butsplitfile");
            but.clicked += Butsplitfile_clicked;
            but = root.Q<ToolbarButton>("butforcheckdata");
            but.clicked += Butbutforcheckdata_clicked;
            but = root.Q<ToolbarButton>("butforexportprocessdata");
            but.clicked += Butbutforexportprocessdata_clicked;
            but = root.Q<ToolbarButton>("fixdata");
           // but.clicked += fixdata_clicked;
            //butmodifytaskid

            //butsplitfile
            //but = root.Q<ToolbarButton>("butnewchapter");
            //but.clicked += Butnewchapter_clicked;
            //but = root.Q<ToolbarButton>("butnewtask");
            //but.clicked += Butnewtask_clicked;

            var a = root.Q<ToolbarMenu>("butcreatemenu");
            a.menu.AppendAction("新建/章节", (a) => Butnewchapter_clicked());
            a.menu.AppendAction("新建/任务", (a) => Butnewtask_clicked());
            a.menu.AppendAction("新建/任务链", (a) => ButnewChaintask_clicked());
            a.menu.AppendSeparator("");
            if (ExistsFiles.Count > 0)
            {
                List<int> Types = new List<int>();
                foreach (var file in ExistsFiles)
                {
                    if (Enum.TryParse(file.Key, out TaskClassifyType type))
                    {
                        Types.Add((int)type);
                    }
                }
                Types.Sort();
                foreach (var type in Types)
                {
                    TaskClassifyType classifyType = (TaskClassifyType)type;
                    string typeName = $"{GetTaskTypeName(classifyType)}";

                    string typefileName = classifyType.ToString();
                    if (ExistsFiles.ContainsKey(typefileName))
                    {
                        foreach (var fl in ExistsFiles[typefileName])
                        {
                            a.menu.AppendAction($"打开/{typeName}/{fl}", (a) => OpenSingleFile(fl));
                        }
                    }


                }
            }

            a.menu.AppendAction("全部打开", (a) => OpenAllFiles());
        }
    }


    protected override void OnGUI()
    {
        string[] buttonNames = new string[] { "First", "Second", "Third" };
        GUILayout.Toolbar(0, buttonNames);
        base.OnGUI();
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
                break;
            case EventType.MouseUp:
                //鼠标抬起
                if (e.button == 1 &&
                    this.TrueeMouseRightClick != null)
                {
                    this.TrueeMouseRightClick(e);
                }

                break;
            case EventType.MouseMove:
                break;
            case EventType.MouseDrag:
                break;
            case EventType.KeyDown:
                break;
            case EventType.KeyUp:
                break;
            case EventType.ScrollWheel:
                break;
            case EventType.Repaint:
                break;
            case EventType.Layout:
                break;
            case EventType.DragUpdated:
                break;
            case EventType.DragPerform:
                break;
            case EventType.DragExited:
                break;
            case EventType.Ignore:
                break;
            case EventType.Used:
                break;
            case EventType.ValidateCommand:
                break;
            case EventType.ExecuteCommand:
                break;
            case EventType.ContextClick:
                break;
            case EventType.MouseEnterWindow:
                break;
            case EventType.MouseLeaveWindow:
                break;
            case EventType.TouchDown:
                break;
            case EventType.TouchUp:
                break;
            case EventType.TouchMove:
                break;
            case EventType.TouchEnter:
                break;
            case EventType.TouchLeave:
                break;
            case EventType.TouchStationary:
                break;

            default:
                break;
        }
    }

    #endregion


    #region 事件



    private void TaskWindow_TrueeMouseRightClick(Event obj)
    {
        //空白处右击
        this.RightClickMenu.DropDown(new Rect(obj.mousePosition, Vector2.zero));
    }

    private void Butsave_clicked()
    {
        ExportJson();
        ExportMapTextStrByMapEditorSave();
        CopyToServer();
        CopyToClient();
        Butcpdesign_clicked();
        if (CurrentSelect != null)
        {
            CurrentSelect.IsDirty = false;
        }
    }

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
            excel[row, index++].Value = Key;
            excel[row, index++].Value = Val;
            excel[row, index++].Value = TaskID;
            excel[row, index++].Value = Des;
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

    /// <summary>
    /// 任务编辑器保存时自动导出多语言xlsm文件，及生成对应的策划目录目录下的json明文对应的key
    /// </summary>
    public static void ExportMapTextStrByMapEditorSave()
    {
        Release();
        //先替换保存策划目录源数据
        ExportTaskText();
        //多语言excel生成
        WriteTaskExcel();
        Release();

        ////再保存客户端目录
        //string clientDir = Application.dataPath + "/Res/MapData";
        //ExportMapText(clientDir);
        //Release();
    }

    private static void ExportTaskText()
    {
        m_taskTextStr.Clear();
        //string[] files = Directory.GetFiles(m_TaskFilePath, "*.json", SearchOption.AllDirectories);
        //string[] files = Directory.GetFiles("E:/workspace/StarsProject_Design/trunk/配置文件/task_source/", "*.json", SearchOption.AllDirectories);
        string[] files = Directory.GetFiles(taskSettings.JsonPathDir, "*.json", SearchOption.AllDirectories);

        foreach (var item in files)
        {
            //var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
            //var TaskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<Task.TaskConfigs>(textAsset.text);

            var textAsset = File.ReadAllText(item);
            var TaskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<Task.TaskConfigs>(textAsset);

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
                    if (!string.IsNullOrEmpty(item1.Value.Base.ChapterName) && !string.IsNullOrWhiteSpace(item1.Value.Base.ChapterName))
                    {
                        {
                            string ChapterName = item1.Value.Base.ChapterName;
                            taskExport.SetChapterName($"{TaskID}_ChapterName", ChapterName, "章节名称");
                            item1.Value.Base.ChapterName_Key = $"{TaskID}_ChapterName";
                        }
                    }
                    else
                    {
                        item1.Value.Base.ChapterName_Key = null;
                    }


                    // 任务名称
                    if (!string.IsNullOrEmpty(item1.Value.Base.TaskName) && !string.IsNullOrWhiteSpace(item1.Value.Base.TaskName))
                    {
                        string TaskName = item1.Value.Base.TaskName;
                        taskExport.SetTaskName($"{TaskID}_TaskName", TaskName, "任务名称");
                        item1.Value.Base.TaskName_Key = $"{TaskID}_TaskName";
                    }
                    else
                    {
                        item1.Value.Base.TaskName_Key = null;
                    }


                    if (!string.IsNullOrEmpty(item1.Value.Base.SpecTaskDesc) && !string.IsNullOrWhiteSpace(item1.Value.Base.SpecTaskDesc))
                    // 特殊描述
                    {
                        string SpecTaskDesc = item1.Value.Base.SpecTaskDesc;
                        taskExport.SetSpecTaskDesc($"{TaskID}_SpecTaskDesc", SpecTaskDesc, "特殊描述");
                        item1.Value.Base.SpecTaskDesc_Key = $"{TaskID}_SpecTaskDesc";
                    }
                    else
                    {
                        item1.Value.Base.SpecTaskDesc_Key = null;
                    }

                    if (!string.IsNullOrEmpty(item1.Value.Base.Desc) && !string.IsNullOrWhiteSpace(item1.Value.Base.Desc))
                    // 任务描述
                    {
                        string Desc = item1.Value.Base.Desc;
                        taskExport.SetTaskDesc($"{TaskID}_TaskDesc", Desc, "任务描述");
                        item1.Value.Base.Desc_Key = $"{TaskID}_TaskDesc";
                    }
                    else
                    {
                        item1.Value.Base.Desc_Key = null;
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
                                if (!string.IsNullOrEmpty(ShowText) && !string.IsNullOrWhiteSpace(ShowText))
                                {
                                    taskExport.SetPickEffects_ShowText(DialogueID, Index, $"{TaskID}_PickEffects_Dialogue_{DialogueID}_{Index}", ShowText, $"承接效果组-对话效果-{DialogueID}");
                                    curEffect.GetBaseEffect().ShowText_Key = $"{TaskID}_PickEffects_Dialogue_{DialogueID}_{Index}";
                                    Index++;
                                }
                                else
                                {
                                    curEffect.GetBaseEffect().ShowText_Key = null;
                                }
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
                                if (!string.IsNullOrEmpty(ShowText) && !string.IsNullOrWhiteSpace(ShowText))
                                {
                                    taskExport.SetFinishEffects_ShowText(DialogueID, Index, $"{TaskID}_FinishEffects_Dialogue_{DialogueID}_{Index}", ShowText, $"交付效果组-对话效果-{DialogueID}");
                                    curEffect.GetBaseEffect().ShowText_Key = $"{TaskID}_FinishEffects_Dialogue_{DialogueID}_{Index}";
                                    Index++;
                                }
                                else
                                {
                                    curEffect.GetBaseEffect().ShowText_Key = null;
                                }
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
                                if (!string.IsNullOrEmpty(TargetDesc) && !string.IsNullOrWhiteSpace(TargetDesc))
                                {
                                    taskExport.SetTargetDesc(TargetID, $"{TaskID}_Targets_{TargetID}", TargetDesc, $"任务目标描述-{TargetID}");
                                    item2.Target.Desc_Key = $"{TaskID}_Targets_{TargetID}";
                                }
                                else
                                {
                                    item2.Target.Desc_Key = null;
                                }

                                // -- 效果目标ID
                                int Index = 1;
                                foreach (var curEffect in item2.Target.EditorEffects)
                                {
                                    //对话效果
                                    if (curEffect.EffectType == FunctionType.Dialogue)
                                    {
                                        int DialogueID = (curEffect.baseEffect as EffectDialogue).DialogueID;  // 对话ID
                                        string ShowText = curEffect.GetBaseEffect().ShowText;   // 服务显示文本
                                        if (!string.IsNullOrEmpty(ShowText) && !string.IsNullOrWhiteSpace(ShowText))
                                        {
                                            taskExport.SetTarget_ShowText(DialogueID, $"{IndexTargets}_{Index}", $"{TaskID}_Targets_{TargetID}_Dialogue_{DialogueID}_{Index}", ShowText, $"任务目标-对话效果-{DialogueID}");
                                            curEffect.GetBaseEffect().ShowText_Key = $"{TaskID}_Targets_{TargetID}_Dialogue_{DialogueID}_{Index}";
                                            Index++;
                                        }
                                        else
                                        {
                                            curEffect.GetBaseEffect().ShowText_Key = null;
                                        }
                                    }
                                }
                                IndexTargets++;
                            }
                        }
                    }

                    m_taskTextStr.Add(taskExport);
                }
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(TaskConfigs, setting);
            jsonstr = ConvertJsonString(jsonstr);

            //string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonAsset, Newtonsoft.Json.Formatting.Indented,);
            if (File.Exists(item))
            {
                File.Delete(item);
            }
            File.WriteAllText(item, jsonstr);

        }
    }

    static public string ConvertJsonString(string str)
    {
        //格式化json字符串
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }

    private static void WriteTaskExcel()
    {
        DirectoryInfo directoryInfo = new(taskSettings.JsonPathDir);
        if (directoryInfo.Parent == null)
        {
            Debug.LogError($"停止生成TaskWindow多语言表，路径错误");
            return;
        }

        DirectoryInfo parentDirectory = directoryInfo.Parent;
        string sourceFolderPath = parentDirectory.FullName + "\\多语言工具\\source";
        // 检查文件夹是否存在
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError($"停止生成多语言表，未找到\"\\多语言工具\\source\"文件夹");
            return;
        }
        
        //var path = EditorUtility.SaveFilePanel("选择导出任务文本路径", "", "任务编辑器文本_TaskEditorText.xlsx", "*.xlsx");
        var path = sourceFolderPath + "\\任务编辑器文本_TaskEditorText.xlsx";
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
                taskConfig.Cells[1, index++].Value = "多语言key";
                taskConfig.Cells[1, index++].Value = "文本";
                taskConfig.Cells[1, index++].Value = "任务ID";
                taskConfig.Cells[1, index++].Value = "描述";

                int row = 6;
                foreach (var item in m_taskTextStr)
                {
                    string taskID = $"{item.TaskID}";
                    // 章节名称
                    {
                        if (item.ChapterName.TaskID != 0)
                        {
                            item.ChapterName.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // 任务名称
                    {
                        if (item.TaskName.TaskID != 0)
                        {
                            item.TaskName.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // 特殊描述
                    {
                        if (item.SpecTaskDesc.TaskID != 0)
                        {
                            item.SpecTaskDesc.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    // 任务描述
                    {
                        if (item.TaskDesc.TaskID != 0)
                        {
                            item.TaskDesc.ExportExcel(row++, taskConfig.Cells);
                        }
                    }
                    //--承接效果组
                    // 服务显示文本
                    {
                        foreach (var item1 in item.PickEffects_ShowText)
                        {
                            if (item1.Value.TaskID != 0)
                            {
                                item1.Value.ExportExcel(row++, taskConfig.Cells);
                            } 
                        }
                    }
                    // -- 交付效果组
                    // 服务显示文本
                    {
                        foreach (var item1 in item.FinishEffects_ShowText)
                        {
                            if (item1.Value.TaskID != 0)
                            {
                                item1.Value.ExportExcel(row++, taskConfig.Cells);
                            }
                        }
                    }
                    // 任务目标
                    // 任务目标描述
                    {
                        foreach (var item1 in item.TargetDesc)
                        {
                            if (item1.Value.TaskID != 0)
                            {
                                item1.Value.ExportExcel(row++, taskConfig.Cells);
                            }
                        }
                    }
                    // -- 效果目标ID
                    // 服务显示文本
                    {
                        foreach (var item1 in item.Target_ShowText)
                        {
                            if (item1.Value.TaskID != 0)
                            {
                                item1.Value.ExportExcel(row++, taskConfig.Cells);
                            }
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

    private static void Release()
    {
        //m_prefabTextStr.Clear();
        //m_prefabTextStrCopy.Clear();
        //m_prefabTextStr2.Clear();

        //m_mapdataTextStr.Clear();
        //m_mapdataTextStr2.Clear();
        //m_mapTextStr.Clear();

        m_taskTextStr.Clear();

        //m_LuaScriptsText.Clear();
        //m_LuaScriptsText2.Clear();
        //m_LuaScriptsPath.Clear();
    }


    private void Butdelete_clicked()
    {
        DeleteSelectAction();
    }

    private void Butsvn_clicked()
    {
        CommitAction();
    }

    private void OpenSingleFile(string fileName)
    {
        LoadTaskTypeConfig();
        LoadTaskConfig(fileName);
    }

    private void OpenAllFiles()
    {
        LoadTaskTypeConfig();
        foreach (var item in ExistsFiles)
        {
            foreach (var file in item.Value)
            {
                LoadTaskConfig(file);
            }
        }
    }
    private void Butcpserver_clicked()
    {
        CopyToServer();
    }

    private void Butcpclient_clicked()
    {
        CopyToClient();
    }

    private void Butcpdesign_clicked()
    {
        // 检查taskSettings.DesignPath是否存在  
        if (!Directory.Exists(taskSettings.DesignPath))
        {
            // 如果不存在，则打开选择目录的选项框  
            var tmp = EditorUtility.SaveFolderPanel("选择客户端所在目录", "", "");
            if (tmp != "")
            {
                // 更新taskSettings.DesignPath  
                taskSettings.DesignPath = tmp;
                // 保存设置  
                EditorPrefs.SetString(settingsPath, JsonUtility.ToJson(taskSettings));

            }
        }
        // 执行复制操作  
        Copy(taskSettings.DesignPath, true, true);
        // 显示完成消息  
        EditorUtility.DisplayDialog("消息", "策划目录复制完成", "ok");
    }

    private void Butnewchapter_clicked()
    {
        var odin = this.MenuTree.Selection.LastOrDefault();
        if (odin == null)
        {
            Debug.LogWarning("请选择一个节点。");
            return;
        }

        TaskClassifyType classifyType;
        if (odin.Value is TaskConfigInfo tkcfg)
        {
            classifyType = tkcfg.Base.TaskType;
            //Debug.LogWarning(odin.Name+odin.Value);
        }
        else if (odin.Value is ChapterInfo cpinfo)
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            classifyType = cpinfo.taskClassify;
        }
        else
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            classifyType = GetTaskClassifyType(odin.Name, out bool find);
        }

        TaskPopMenu.AddChapter(classifyType, OnAddChapter);
    }

    private void ButnewChaintask_clicked()
    {
        var odin = this.MenuTree.Selection.LastOrDefault();
        if (odin == null)
        {
            Debug.LogWarning("请选择一个节点。");
            return;
        }

        TaskClassifyType classifyType;
        int chapterID = 0;
        string chapterName = "";
        if (odin.Value is TaskConfigInfo tkcfg)
        {
            classifyType = tkcfg.Base.TaskType;
            chapterID = tkcfg.Base.Chapter;
            chapterName = tkcfg.Base.ChapterName;
            //Debug.LogWarning(odin.Name+odin.Value);
        }
        else if (odin.Value is ChapterInfo cpinfo)
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            classifyType = cpinfo.taskClassify;
            chapterID = cpinfo.ID;
            chapterName = cpinfo.Name;
        }
        else if (odin.Value is ChainInfo chain)
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            classifyType = chain.taskClassify;
            chapterID = chain.ID;
            chapterName = chain.Name;
        }
        else
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            classifyType = GetTaskClassifyType(odin.Name, out bool find);
        }

        TaskPopMenu.AddChain(classifyType, chapterID, chapterName, OnAddChain);
    }

    private void Butnewtask_clicked()
    {
        var odin = this.MenuTree.Selection.LastOrDefault();
        if (odin == null)
        {
            Debug.LogWarning("请选择一个节点。");
            return;
        }

        ChainInfo info = getChapterByMenu(odin);
        if (info == null)
        {
            Debug.LogWarning("请选择一个任务链的节点。");
            return;
        }

        TaskPopMenu.AddTask(info.taskClassify, info.ID, info.Name, info.ChainID, OnAddTaskItem);
    }
    private void ButCopyTask_clicked()
    {
        var odin = this.MenuTree.Selection.LastOrDefault();
        if (odin == null)
        {
            Debug.LogWarning("请选择一个节点。");
            return;
        }

        TaskConfigInfo info = getTaskByMenu(odin);
        if (info == null)
        {
            Debug.LogWarning("请选择一个任务链的节点。");
            return;
        }
        OnCopyTaskItem(info);
    }
    private void Butloadcfg_clicked()
    {
        TaskEnumUtils.Init();
        EditorUtility.DisplayDialog("消息", "加载配置完成", "确  定");
    }

    public uint GetIndex(uint type, uint chap, uint id)
    {
        uint result = (type << 28) | (chap << 16) | (id << 0);
        return result;
    }

    public (uint type, uint chap, uint id) UnGetIndex(uint taskID)
    {
        uint type = (taskID >> 28) & 0xF;
        uint chap = (taskID >> 16) & 0xFFF;
        uint id = taskID & 0xFFFF;
        return (type, chap, id);
    }

    private void Butsplitfile_clicked()
    {
        Dictionary<string, List<TaskConfigInfo>> savemap = new Dictionary<string, List<TaskConfigInfo>>();
        foreach (var task in TaskMaps)
        {
            //任务类型+任务章节
            string fileName = $"Task_{task.Value.Base.TaskType}_{task.Value.Base.Chapter}";
            if (savemap.ContainsKey(fileName))
            {
                savemap[fileName].Add(task.Value);
            }
            else
            {
                List<TaskConfigInfo> list = new List<TaskConfigInfo>();
                list.Add(task.Value);
                savemap.Add(fileName, list);
            }
        }


        foreach (var item in savemap)
        {
            SaveTaskConfig(item.Key, item.Value);
        }

        AssetDatabase.Refresh();
    }

    private void Butmodifytaskid_clicked()
    {
        if (TaskMaps != null)
        {
            Dictionary<uint, Dictionary<uint, List<TaskConfigInfo>>> dic =
                new Dictionary<uint, Dictionary<uint, List<TaskConfigInfo>>>();
            foreach (var task in TaskMaps)
            {
                uint type = (uint)task.Value.Base.TaskType;
                uint ch = (uint)task.Value.Base.ChainID;
                if (dic.ContainsKey(type))
                {
                    if (dic[type].ContainsKey(ch))
                    {
                        dic[type][ch].Add(task.Value);
                    }
                    else
                    {
                        List<TaskConfigInfo> list = new List<TaskConfigInfo>();
                        list.Add(task.Value);
                        dic[type].Add(ch, list);
                    }
                }
                else
                {
                    List<TaskConfigInfo> list = new List<TaskConfigInfo>();
                    list.Add(task.Value);
                    Dictionary<uint, List<TaskConfigInfo>> cdic = new Dictionary<uint, List<TaskConfigInfo>>();
                    cdic.Add(ch, list);
                    dic.Add(type, cdic);
                }
                // task.Value.Base.TaskID = GetIndex(,(uint)task.Value.Base.ChainID,0);
            }

            Dictionary<uint, uint> HasMap = new Dictionary<uint, uint>();
            foreach (var items in dic)
            {
                foreach (var tasks in items.Value)
                {
                    uint index = 0;
                    foreach (var task in tasks.Value)
                    {
                        task.Base.UIndex = index;
                        uint oldid = task.Base.TaskID;
                        uint newid = GetIndex(items.Key, tasks.Key, index);
                        task.Base.TaskID = newid;
                        HasMap.Add(oldid, newid);
                        index++;
                    }
                    //task.Base.TaskID = GetIndex(,(uint)task.Value.Base.ChainID,0);
                }
            }

            foreach (var task in TaskMaps)
            {
                if (task.Value.Base.PreTaskID != 0 && HasMap.ContainsKey(task.Value.Base.PreTaskID))
                {
                    task.Value.Base.PreTaskID = HasMap[task.Value.Base.PreTaskID];
                }

                if (task.Value.Base.NextTaskID != 0 && HasMap.ContainsKey(task.Value.Base.NextTaskID))
                {
                    task.Value.Base.NextTaskID = HasMap[task.Value.Base.NextTaskID];
                }
            }
        }
    }

    private void Butbutforcheckdata_clicked()
    {
        var path = EditorUtility.SaveFilePanel("选择导出任务列表", "", "task.csv", "*.csv");

        if (path != "")
        {
            var data = new StringBuilder();
            foreach (var tasklist in Tasks)
            {
                foreach (var task in tasklist.Value)
                {
                    string target = "";
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        var tartype = task.Value.TaskFinishInfo.Targets[0].Target.EditorTaskType.TaskType;
                        foreach (var item in (ValueDropdownList<TaskType>)TaskEnumUtils._tasktypes)
                        {
                            if (item.Value == tartype)
                            {
                                target = item.Text;
                                break;
                            }
                        }
                    }

                    ////读取对话表
                    //string jsonText = System.IO.File.ReadAllText("Assets/Res/Config/Excel/CommonDialog.json");
                    //StarProjectDef.CommonDialogData commonDialogData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.CommonDialogData>(jsonText);
                    ////读取传讯表
                    //string jsonText2 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/TeleDialog.json");
                    //StarProjectDef.TeleDialogData teleDialogData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.TeleDialogData>(jsonText2);
                    ////读取黑幕表
                    //string jsonText3 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/BlackMovie.json");
                    //StarProjectDef.BlackMovieData blackMovieData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.BlackMovieData>(jsonText3);
                    ////读取怪物表
                    //string jsonText4 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Monster.json");
                    //StarProjectDef.MonsterData monsterData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.MonsterData>(jsonText4);
                    ////读取NPC表
                    //string jsonText5 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/NPC.json");
                    //StarProjectDef.NpcData npcData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.NpcData>(jsonText5);

                    //读取对话表
                    byte[] byte1 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/CommonDialog.bytes");
                    StarProjectDef.CommonDialogData commonDialogData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.CommonDialogData>(byte1);
                    //读取传讯表
                    byte[] byte2 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/TeleDialog.bytes");
                    StarProjectDef.TeleDialogData teleDialogData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.TeleDialogData>(byte2);
                    //读取黑幕表
                    byte[] byte3 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/BlackMovie.bytes");
                    StarProjectDef.BlackMovieData blackMovieData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.BlackMovieData>(byte3);
                    //读取怪物表
                    byte[] byte4 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Monster.bytes");
                    StarProjectDef.MonsterData monsterData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.MonsterData>(byte4);
                    //读取NPC表
                    byte[] byte5 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/NPC.bytes");
                    StarProjectDef.NpcData npcData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.NpcData>(byte5);

                    //任务描述
                    string taskDesc = string.IsNullOrEmpty(task.Value.Base.Desc) ? "" : task.Value.Base.Desc.Replace("\n", "").Replace("\r", "");

                    //任务目标
                    StringBuilder taskTargetSB = new StringBuilder();

                    //基础信息-特殊描述
                    if (!string.IsNullOrEmpty(task.Value.Base.SpecTaskDesc))
                    {
                        taskTargetSB.Append($"{task.Value.Base.SpecTaskDesc}|");
                    }
                    //任务目标-任务目标描述（List）
                    else if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                        {
                            if (!string.IsNullOrEmpty(curTarget.Target.Desc))
                            {
                                string desc = curTarget.Target.Desc;
                                //需要判断有没有特殊字符
                                if (desc.Contains("[MON]"))
                                {
                                    if (curTarget.Target.TaskType.TaskType == TaskType.KillMonster)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        foreach (var monid in curTarget.Target.TaskType.Pramas)
                                        {
                                            sb.Append(monsterData.StaticMonsterDatas[long.Parse(monid)].Name);
                                        }
                                        desc = desc.Replace("[MON]", sb.ToString());
                                    }
                                    else
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID}的某个任务目标填了[MON]但是任务目标不是杀怪");
                                    }
                                }
                                //需要判断有没有特殊字符
                                if (desc.Contains("[NPC]"))
                                {
                                    if (curTarget.Target.TargetType == EffectTargetType.NPC)
                                    {
                                        desc = desc.Replace("[NPC]", npcData.StaticNpcDatas[curTarget.Target.TargetID].Name);
                                    }
                                    else
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID}的某个任务目标填了[NPC]但是触发效果不是寻找NPC");
                                    }
                                }
                                taskTargetSB.Append($"{desc}|");
                            }
                        }
                    }
                    //基础信息-交付任务-服务显示文本（List）
                    if (task.Value.TaskFinishInfo.EditorFinishEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.TaskFinishInfo.EditorFinishEffects)
                        {
                            if (!string.IsNullOrEmpty(curEffect.GetBaseEffect().ShowText))
                            {
                                string desc = curEffect.GetBaseEffect().ShowText;
                                if (desc.Contains("[NPC]"))
                                {
                                    desc = desc.Replace("[NPC]", npcData.StaticNpcDatas[(int)task.Value.TaskFinishInfo.FinishTaskNPC].Name);
                                }
                                taskTargetSB.Append($"{desc}|");
                            }
                        }
                    }
                    //转化成字符串，需要去除最后一个竖线
                    string taskTarget = taskTargetSB.ToString();
                    if (taskTarget.Length > 0)
                    {
                        taskTarget = taskTarget.Remove(taskTarget.Length - 1);
                    }

                    //任务对话
                    StringBuilder taskTalkSB = new StringBuilder();

                    //基础信息-承接任务-效果类型-触发对话-对话ID（List）
                    if (task.Value.Condition.EditorPickEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.Condition.EditorPickEffects)
                        {
                            //对话效果
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                if (!GetAllDialog(ref taskTalkSB, commonDialogData, teleDialogData, (curEffect.baseEffect as EffectDialogue).DialogueID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的语言对话表ID{(curEffect.baseEffect as EffectDialogue).DialogueID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                            //黑幕效果
                            if (curEffect.EffectType == FunctionType.PlayBlack)
                            {
                                if (!GetAllBlack(ref taskTalkSB, blackMovieData, (curEffect.baseEffect as EffectPlayBlack).BlackID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的黑幕表ID{(curEffect.baseEffect as EffectPlayBlack).BlackID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                        }
                    }
                    //任务目标（List）-触发效果-效果类型-触发对话-对话ID（List）
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                        {
                            if (curTarget.Target.EditorEffects.Count > 0)
                            {
                                foreach (var curEffect in curTarget.Target.EditorEffects)
                                {
                                    if (curEffect.EffectType == FunctionType.Dialogue)
                                    {
                                        if (!GetAllDialog(ref taskTalkSB, commonDialogData, teleDialogData, (curEffect.baseEffect as EffectDialogue).DialogueID))
                                        {
                                            Debug.Log($"任务{task.Value.Base.TaskID}配置的语言对话表ID{(curEffect.baseEffect as EffectDialogue).DialogueID}不存在");
                                            taskTalkSB.Append("|");
                                        }
                                    }
                                    if (curEffect.EffectType == FunctionType.PlayBlack)
                                    {
                                        if (!GetAllBlack(ref taskTalkSB, blackMovieData, (curEffect.baseEffect as EffectPlayBlack).BlackID))
                                        {
                                            Debug.Log($"任务{task.Value.Base.TaskID}配置的黑幕表ID{(curEffect.baseEffect as EffectPlayBlack).BlackID}不存在");
                                            taskTalkSB.Append("|");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //交付任务-交付效果-效果类型-触发对话-对话ID（List
                    if (task.Value.TaskFinishInfo.EditorFinishEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.TaskFinishInfo.EditorFinishEffects)
                        {
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                if (!GetAllDialog(ref taskTalkSB, commonDialogData, teleDialogData, (curEffect.baseEffect as EffectDialogue).DialogueID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的语言对话表ID{(curEffect.baseEffect as EffectDialogue).DialogueID}不存在");
                                }
                                taskTalkSB.Append("|");
                            }
                            if (curEffect.EffectType == FunctionType.PlayBlack)
                            {
                                if (!GetAllBlack(ref taskTalkSB, blackMovieData, (curEffect.baseEffect as EffectPlayBlack).BlackID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的黑幕表ID{(curEffect.baseEffect as EffectPlayBlack).BlackID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                        }
                    }
                    string taskTalk = taskTalkSB.Replace(",", "，").ToString();
                    if (taskTalk.Length > 0)
                    {
                        taskTalk = taskTalk.Remove(taskTalk.Length - 1);
                    }

                    //特殊字符处理
                    taskDesc.Replace("@player", "[玩家名字]");
                    taskTarget.Replace("@player", "[玩家名字]");
                    taskTalk.Replace("@player", "[玩家名字]");
                    target.Replace("@player", "[玩家名字]");

                    data.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                        (int)task.Value.Base.TaskType * 10000000 + task.Value.Base.Chapter * 100000 +
                        task.Value.Base.Index,
                        task.Value.Base.TaskID,
                        task.Value.Base.ChapterName,
                        task.Value.Base.TaskName,
                        (int)task.Value.Base.TaskType,
                        task.Value.TaskFinishInfo.FinishTaskNPC,
                        taskDesc,
                        taskTarget,
                        taskTalk,
                        target);
                }
            }
#if UNITY_EDITOR

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(data.ToString());
            writer.Flush();
            writer.Close();
            writer.Dispose();
            //FileStream fs = new StreamWriter(path,(path, FileMode.CreateNew);
            //byte[] bytes = Encoding.Unicode.GetBytes(data.ToString());
            //fs.Write(bytes, 0, bytes.Length);
            //fs.Flush();
            //fs.Close();
            //fs.Dispose();
#endif
            EditorUtility.DisplayDialog("消息", "导出成功", "ok");
        }
    }

    /*private void fixdata_clicked()
    {
        if (TaskMaps != null)
        {
            foreach (var task in TaskMaps)
            {

                bool modify = false;
                if (task.Value.Base.GuideID > 0)
                {
                    if (task.Value.TaskFinishInfo != null)
                    {
                        if (task.Value.TaskFinishInfo.Targets != null && task.Value.TaskFinishInfo.Targets.Count > 0)
                        {
                            var tar=task.Value.TaskFinishInfo.Targets[0];
                            if (tar != null)
                            {
                                if (tar.Target!=null && tar.Target.FindPath != null)
                                {
                                    var OldType = tar.Target.FindPath.FindPathType;
                                    tar.Target.FindPath.FindPathType = E_FindPath.DoGuide;
                                    Debug.Log($"任务{task.Value.Base.TaskID}  原始类型={OldType} 配置的引导ID{task.Value.Base.GuideID}");
                                    tar.Target.FindPath.GuideID = task.Value.Base.GuideID;
                                    modify = true;
                                }
                            }
                        }
                    }
                    
                   // Debug.Log($"任务{task.Value.Base.TaskID} 配置的引导ID{task.Value.Base.GuideID}");
                }

                if (task.Value.Base.JumpID > 0)
                {
                    if (task.Value.TaskFinishInfo != null)
                    {
                        if (task.Value.TaskFinishInfo.Targets != null && task.Value.TaskFinishInfo.Targets.Count > 0)
                        {
                            var tar=task.Value.TaskFinishInfo.Targets[0];
                            if (tar != null)
                            {
                                if (tar.Target!=null && tar.Target.FindPath != null)
                                {
                                    var OldType = tar.Target.FindPath.FindPathType;
                                    tar.Target.FindPath.FindPathType = E_FindPath.JumpTo;
                                    if (modify)
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID} 被修改过 原始类型={OldType} 配置的跳转ID{task.Value.Base.JumpID}");
                                    }
                                    else
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID}  原始类型={OldType} 配置的跳转ID{task.Value.Base.JumpID}");
                                    }
                                    tar.Target.FindPath.JumpID = task.Value.Base.JumpID;
                                    modify = true;
                                }
                            }
                        }
                    }
                }
     
            }
        }
    }*/

    private void Butbutforexportprocessdata_clicked()
    {
        var path = EditorUtility.SaveFilePanel("选择导出任务列表", "", "process.csv", "*.csv");

        if (path != "")
        {
            var data = new StringBuilder();
            foreach (var tasklist in Tasks)
            {
                foreach (var task in tasklist.Value)
                {
                    string target = "";
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        var tartype = task.Value.TaskFinishInfo.Targets[0].Target.EditorTaskType.TaskType;
                        foreach (var item in (ValueDropdownList<TaskType>)TaskEnumUtils._tasktypes)
                        {
                            if (item.Value == tartype)
                            {
                                target = item.Text;
                                break;
                            }
                        }
                    }

                    ////读取对话表
                    //string jsonText = System.IO.File.ReadAllText("Assets/Res/Config/Excel/CommonDialog.json");
                    //StarProjectDef.CommonDialogData commonDialogData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.CommonDialogData>(jsonText);
                    ////读取传讯表
                    //string jsonText2 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/TeleDialog.json");
                    //StarProjectDef.TeleDialogData teleDialogData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.TeleDialogData>(jsonText2);
                    ////读取黑幕表
                    //string jsonText3 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/BlackMovie.json");
                    //StarProjectDef.BlackMovieData blackMovieData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.BlackMovieData>(jsonText3);
                    ////读取怪物表
                    //string jsonText4 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Monster.json");
                    //StarProjectDef.MonsterData monsterData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.MonsterData>(jsonText4);
                    ////读取NPC表
                    //string jsonText5 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/NPC.json");
                    //StarProjectDef.NpcData npcData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.NpcData>(jsonText5);
                    ////读取交互物表
                    //string jsonText6 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Interact.json");
                    //StarProjectDef.InteractData interactData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.InteractData>(jsonText6);
                    ////读取道具表
                    //string jsonText7 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Item.json");
                    //StarProjectDef.ItemData itemData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.ItemData>(jsonText7);

                    //读取对话表
                    byte[] byte1 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/CommonDialog.bytes");
                    StarProjectDef.CommonDialogData commonDialogData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.CommonDialogData>(byte1);
                    //读取传讯表
                    byte[] byte2 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/TeleDialog.bytes");
                    StarProjectDef.TeleDialogData teleDialogData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.TeleDialogData>(byte2);
                    //读取黑幕表
                    byte[] byte3 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/BlackMovie.bytes");
                    StarProjectDef.BlackMovieData blackMovieData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.BlackMovieData>(byte3);
                    //读取怪物表
                    byte[] byte4 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Monster.bytes");
                    StarProjectDef.MonsterData monsterData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.MonsterData>(byte4);
                    //读取NPC表
                    byte[] byte5 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/NPC.bytes");
                    StarProjectDef.NpcData npcData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.NpcData>(byte5);
                    //读取交互物表
                    byte[] byte6 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Interact.bytes");
                    StarProjectDef.InteractData interactData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.InteractData>(byte6);
                    //读取道具表
                    byte[] byte7 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Item.bytes");
                    StarProjectDef.ItemData itemData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.ItemData>(byte7);

                    //任务描述
                    string taskDesc = string.IsNullOrEmpty(task.Value.Base.Desc) ? "" : task.Value.Base.Desc.Replace("\n", "").Replace("\r", "");

                    //任务目标
                    StringBuilder taskTargetSB = new StringBuilder();

                    //基础信息-特殊描述
                    if (!string.IsNullOrEmpty(task.Value.Base.SpecTaskDesc))
                    {
                        taskTargetSB.Append($"{task.Value.Base.SpecTaskDesc}|");
                    }
                    //任务目标-任务目标描述（List）
                    else if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                        {
                            if (!string.IsNullOrEmpty(curTarget.Target.Desc))
                            {
                                string desc = curTarget.Target.Desc;
                                //需要判断有没有特殊字符
                                if (desc.Contains("[MON]"))
                                {
                                    if (curTarget.Target.TaskType.TaskType == TaskType.KillMonster)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        foreach (var monid in curTarget.Target.TaskType.Pramas)
                                        {
                                            sb.Append(monsterData.StaticMonsterDatas[long.Parse(monid)].Name);
                                        }
                                        desc = desc.Replace("[MON]", sb.ToString());
                                    }
                                    else
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID}的某个任务目标填了[MON]但是任务目标不是杀怪");
                                    }
                                }
                                //需要判断有没有特殊字符
                                if (desc.Contains("[NPC]"))
                                {
                                    if (curTarget.Target.TargetType == EffectTargetType.NPC)
                                    {
                                        desc = desc.Replace("[NPC]", npcData.StaticNpcDatas[curTarget.Target.TargetID].Name);
                                    }
                                    else
                                    {
                                        Debug.Log($"任务{task.Value.Base.TaskID}的某个任务目标填了[NPC]但是触发效果不是寻找NPC");
                                    }
                                }
                                taskTargetSB.Append($"{desc}|");
                            }
                        }
                    }
                    //基础信息-交付任务-服务显示文本（List）
                    if (task.Value.TaskFinishInfo.EditorFinishEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.TaskFinishInfo.EditorFinishEffects)
                        {
                            if (!string.IsNullOrEmpty(curEffect.GetBaseEffect().ShowText))
                            {
                                string desc = curEffect.GetBaseEffect().ShowText;
                                if (desc.Contains("[NPC]"))
                                {
                                    desc = desc.Replace("[NPC]", npcData.StaticNpcDatas[(int)task.Value.TaskFinishInfo.FinishTaskNPC].Name);
                                }
                                taskTargetSB.Append($"{desc}|");
                            }
                        }
                    }
                    //转化成字符串，需要去除最后一个竖线
                    string taskTarget = taskTargetSB.ToString();
                    if (taskTarget.Length > 0)
                    {
                        taskTarget = taskTarget.Remove(taskTarget.Length - 1);
                    }

                    //任务对话
                    StringBuilder taskTalkSB = new StringBuilder();

                    //基础信息-承接任务-效果类型-触发对话-对话ID（List）
                    if (task.Value.Condition.EditorPickEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.Condition.EditorPickEffects)
                        {
                            //对话效果
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                if (!GetAllDialog(ref taskTalkSB, commonDialogData, teleDialogData, (curEffect.baseEffect as EffectDialogue).DialogueID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的语言对话表ID{(curEffect.baseEffect as EffectDialogue).DialogueID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                            //黑幕效果
                            if (curEffect.EffectType == FunctionType.PlayBlack)
                            {
                                if (!GetAllBlack(ref taskTalkSB, blackMovieData, (curEffect.baseEffect as EffectPlayBlack).BlackID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的黑幕表ID{(curEffect.baseEffect as EffectPlayBlack).BlackID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                        }
                    }
                    //任务目标（List）-触发效果-效果类型-触发对话-对话ID（List）
                    StringBuilder taskTargetEffectSB = new StringBuilder();
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                        {
                            if (curTarget.Target.EditorEffects.Count > 0)
                            {
                                foreach (var curEffect in curTarget.Target.EditorEffects)
                                {
                                    GetEffectSB(ref taskTargetEffectSB, curEffect, npcData, interactData, monsterData, itemData, commonDialogData, teleDialogData);
                                }

                            }
                        }
                        if (taskTargetEffectSB.Length > 0)
                        {
                            taskTargetEffectSB.Remove(taskTargetEffectSB.Length - 1, 1);
                        }
                    }

                    string taskTargetEffect = taskTargetEffectSB.ToString();

                    //交付任务-交付效果-效果类型-触发对话-对话ID（List
                    if (task.Value.TaskFinishInfo.EditorFinishEffects.Count > 0)
                    {
                        foreach (var curEffect in task.Value.TaskFinishInfo.EditorFinishEffects)
                        {
                            if (curEffect.EffectType == FunctionType.Dialogue)
                            {
                                if (!GetAllDialog(ref taskTalkSB, commonDialogData, teleDialogData, (curEffect.baseEffect as EffectDialogue).DialogueID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的语言对话表ID{(curEffect.baseEffect as EffectDialogue).DialogueID}不存在");
                                }
                                taskTalkSB.Append("|");
                            }
                            if (curEffect.EffectType == FunctionType.PlayBlack)
                            {
                                if (!GetAllBlack(ref taskTalkSB, blackMovieData, (curEffect.baseEffect as EffectPlayBlack).BlackID))
                                {
                                    Debug.Log($"任务{task.Value.Base.TaskID}配置的黑幕表ID{(curEffect.baseEffect as EffectPlayBlack).BlackID}不存在");
                                    taskTalkSB.Append("|");
                                }
                            }
                        }
                    }
                    string taskTalk = taskTalkSB.Replace(",", "，").ToString();
                    if (taskTalk.Length > 0)
                    {
                        taskTalk = taskTalk.Remove(taskTalk.Length - 1);
                    }

                    //接任务NPC
                    string pickTaskNPC = task.Value.Condition.PickTaskNPC == 0 ? "0" : task.Value.Condition.PickTaskNPC + npcData.StaticNpcDatas[task.Value.Condition.PickTaskNPC].Name;
                    //接任务效果
                    StringBuilder pickTaskEffectSB = new StringBuilder();
                    foreach (var curEffect in task.Value.Condition.EditorPickEffects)
                    {
                        GetEffectSB(ref pickTaskEffectSB, curEffect, npcData, interactData, monsterData, itemData, commonDialogData, teleDialogData);
                    }
                    string pickTaskEffect = pickTaskEffectSB.Replace(",", "，").ToString();
                    if (pickTaskEffect.Length > 0)
                    {
                        pickTaskEffect = pickTaskEffect.Remove(pickTaskEffect.Length - 1);
                    }
                    //交任务NPC
                    string finishTaskNPC = task.Value.TaskFinishInfo.FinishTaskNPC == 0 ? "0" : task.Value.TaskFinishInfo.FinishTaskNPC + npcData.StaticNpcDatas[task.Value.TaskFinishInfo.FinishTaskNPC].Name;
                    //交任务效果
                    StringBuilder finishTaskEffectSB = new StringBuilder();
                    foreach (var curEffect in task.Value.TaskFinishInfo.EditorFinishEffects)
                    {
                        GetEffectSB(ref finishTaskEffectSB, curEffect, npcData, interactData, monsterData, itemData, commonDialogData, teleDialogData);
                    }
                    string finishTaskEffect = finishTaskEffectSB.Replace(",", "，").ToString();
                    if (finishTaskEffect.Length > 0)
                    {
                        finishTaskEffect = finishTaskEffect.Remove(finishTaskEffect.Length - 1);
                    }

                    //特殊字符处理
                    taskDesc.Replace("@player", "[玩家名字]");
                    taskTarget.Replace("@player", "[玩家名字]");
                    taskTalk.Replace("@player", "[玩家名字]");
                    target.Replace("@player", "[玩家名字]");

                    //任务参数
                    StringBuilder taskArgSB = new StringBuilder();
                    foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                    {
                        switch (curTarget.Target.EditorTaskType.TaskType)
                        {
                            case TaskType.Dialogue:
                                taskArgSB.Append($"对话：");
                                TaskTypeDialogue dialogueInfo = (TaskTypeDialogue)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Level:
                                taskArgSB.Append($"关卡：");
                                TaskTypeLevel levelInfo = (TaskTypeLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(levelInfo.LevelID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.KillMonster:
                                taskArgSB.Append($"杀怪：");
                                TaskTypeKillMonster killMonsterInfo = (TaskTypeKillMonster)curTarget.Target.EditorTaskType.TaskInfo;
                                foreach (var id in killMonsterInfo.MonsterID)
                                {
                                    taskArgSB.Append($"{GetMonsterName(id, monsterData)}，");
                                }
                                if (killMonsterInfo.MonsterID.Count > 0)
                                {
                                    taskArgSB.Remove(taskArgSB.Length - 1, 1);
                                }
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Collect:
                                taskArgSB.Append($"收集：");
                                TaskTypeCollect collectInfo = (TaskTypeCollect)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(GetItemName(collectInfo.ItemID, itemData));
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.UseItem:
                                taskArgSB.Append($"使用(道具)：");
                                TaskTypeUseItem useItemInfo = (TaskTypeUseItem)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(GetItemName(useItemInfo.ItemID, itemData));
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.UseSkill:
                                taskArgSB.Append($"使用(技能)：");
                                TaskTypeUseSkill useSkillInfo = (TaskTypeUseSkill)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(useSkillInfo.SkillID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Arrive:
                                taskArgSB.Append($"跑腿/到达：");
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Escort:
                                taskArgSB.Append($"护送：");
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.InterAction:
                                taskArgSB.Append($"交互：");
                                TaskTypeInterAction interActionInfo = (TaskTypeInterAction)curTarget.Target.EditorTaskType.TaskInfo;
                                foreach (var id in interActionInfo.InterID)
                                {
                                    taskArgSB.Append($"{GetInterName(id, interactData)}，");
                                }
                                if (interActionInfo.InterID.Count > 0)
                                {
                                    taskArgSB.Remove(taskArgSB.Length - 1, 1);
                                }
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Prologue:
                                taskArgSB.Append($"序章：");
                                TaskTypePrologue prologueInfo = (TaskTypePrologue)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(prologueInfo.Tips.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Shady:
                                taskArgSB.Append($"黑幕：");
                                TaskTypeShady shadyInfo = (TaskTypeShady)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(shadyInfo.Tips.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.KillMonsterGetItem:
                                taskArgSB.Append($"杀怪获得道具：");
                                TaskTypeKillMonsterGetItem killMonsterGetItemInfo = (TaskTypeKillMonsterGetItem)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(GetItemName(killMonsterGetItemInfo.TaskItemID, itemData));
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.InterGetItem:
                                taskArgSB.Append($"交互获得道具：");
                                TaskTypeInterGetItem interGetItemInfo = (TaskTypeInterGetItem)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(GetItemName(interGetItemInfo.TaskItemID, itemData));
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.GuideUseItem:
                                taskArgSB.Append($"引导使用道具：");
                                TaskTypeGuideUseItem guideUseItemInfo = (TaskTypeGuideUseItem)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(GetItemName(guideUseItemInfo.ItemID, itemData));
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.GuidePartnerInBattle:
                                taskArgSB.Append($"引导上阵伙伴：");
                                TaskTypeGuidePartnerInBattle guidePartnerInBattleInfo = (TaskTypeGuidePartnerInBattle)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(guidePartnerInBattleInfo.PartnerID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.Plays:
                                taskArgSB.Append($"玩法：");
                                TaskTypePlays playsInfo = (TaskTypePlays)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(playsInfo.playType.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.PlayerLevel:
                                taskArgSB.Append($"玩家等级：");
                                TaskTypePlayerLevel playerLevelInfo = (TaskTypePlayerLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(playerLevelInfo.PlayerLevel.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.AdventureLevel:
                                taskArgSB.Append($"玩家冒险等级：");
                                TaskTypeAdventureLevel adventureLevelInfo = (TaskTypeAdventureLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(adventureLevelInfo.AdventureLevel.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.SystemInteract:
                                taskArgSB.Append($"系统交互：");
                                TaskTypeSystemInteract systemInteractInfo = (TaskTypeSystemInteract)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(systemInteractInfo.playType.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.LoginToday:
                                taskArgSB.Append($"今日登录：");
                                TaskTypeLoginToday loginTodayInfo = (TaskTypeLoginToday)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.ShopBuyGoods:
                                taskArgSB.Append($"x商店购买道具x次：");
                                TaskTypeShopBuyGoods shopBuyGoodsInfo = (TaskTypeShopBuyGoods)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(shopBuyGoodsInfo.ShopID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.WantTaskCountStage:
                                taskArgSB.Append($"参与x阶通缉x次：");
                                TaskTypeWantTaskCountStage wantTaskCountStageInfo = (TaskTypeWantTaskCountStage)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(wantTaskCountStageInfo.wantedID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.PlayerSkillLevelUp:
                                taskArgSB.Append($"参与x阶通缉x次：");
                                TaskTypePlayerSkillLevelUp playerSkillLevelUpInfo = (TaskTypePlayerSkillLevelUp)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(playerSkillLevelUpInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.SkillMonsterSource:
                                taskArgSB.Append($"击杀怪物来源：");
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.SignInAward:
                                taskArgSB.Append($"签到领奖次数：");
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.FinishTask:
                                taskArgSB.Append($"完成指定id的任务：");
                                TaskTypeFinishTask finishTaskInfo = (TaskTypeFinishTask)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(finishTaskInfo.taskID.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.TotalIntensify:
                                taskArgSB.Append($"总计强化等级X：");
                                TaskTotalIntensify totalIntensifyInfo = (TaskTotalIntensify)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(totalIntensifyInfo.totalLevel.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.PartnerCount:
                                taskArgSB.Append($"N个X级以上佣兵：");
                                TaskPartnerCount partnerCountInfo = (TaskPartnerCount)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(partnerCountInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.SecretAreaLevel:
                                taskArgSB.Append($"秘境X层通关：");
                                TaskSecretAreaLevel secretAreaLevelInfo = (TaskSecretAreaLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(secretAreaLevelInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.SkillLevel:
                                taskArgSB.Append($"X类型技能X级：");
                                TaskSkillLevel skillLevelInfo = (TaskSkillLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(skillLevelInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.ShadowofDreamErosionPass:
                                taskArgSB.Append($"完成副本ID：");
                                TaskShadowofDreamErosionPass shadowofDreamErosionPassInfo = (TaskShadowofDreamErosionPass)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(shadowofDreamErosionPassInfo.id.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.TalismanQualityCount:
                                taskArgSB.Append($"激活N个X级以上Y品质以上护符：");
                                TaskTalismanQualityCount talismanQualityCountInfo = (TaskTalismanQualityCount)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(talismanQualityCountInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.HerldryCount:
                                taskArgSB.Append($"获取N个X级纹章：");
                                TaskHerldryCount herldryCountInfo = (TaskHerldryCount)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(herldryCountInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.PersonTowerLevel:
                                taskArgSB.Append($"爬塔X层：");
                                TaskPersonTowerLevel personTowerLevelInfo = (TaskPersonTowerLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(personTowerLevelInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.PartnerEquipCount:
                                taskArgSB.Append($"累计获取N件X类型道具：");
                                TaskPartnerEquipCount partnerEquipCountInfo = (TaskPartnerEquipCount)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(partnerEquipCountInfo.itemType.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.EquipmentTrainingLevel:
                                taskArgSB.Append($"N件X级装备洗练度达到Y：");
                                TaskEquipmentTrainingLevel equipmentTrainingLevelInfo = (TaskEquipmentTrainingLevel)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(equipmentTrainingLevelInfo.level.ToString());
                                taskArgSB.Append($"|");
                                break;
                            case TaskType.TalentCount:
                                taskArgSB.Append($"点亮天赋点X个：");
                                TaskTalentCount talentCountInfo = (TaskTalentCount)curTarget.Target.EditorTaskType.TaskInfo;
                                taskArgSB.Append(talentCountInfo.count.ToString());
                                taskArgSB.Append($"|");
                                break;
                            default:
                                break;
                        }
                    }
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        taskArgSB.Remove(taskArgSB.Length - 1, 1);
                    }
                    string taskArg = taskArgSB.ToString();

                    //寻路目标
                    Dictionary<E_FindPath, string> roadTypeDic = new Dictionary<E_FindPath, string>() {
                       { E_FindPath.None,"没有寻路" },
                       { E_FindPath.Spawer, "场景编辑器配置的Spanwer位置" },
                       { E_FindPath.NPC ,"到达具体NPC位置"},
                       { E_FindPath.InterAction , "到达具体交互物"},
                       { E_FindPath.Area , "到达具体区域位置" },
                       { E_FindPath.JumpTo , "跳转" }
                    };

                    StringBuilder roadSB = new StringBuilder();
                    foreach (var curTarget in task.Value.TaskFinishInfo.Targets)
                    {
                        switch (curTarget.Target.FindPath.FindPathType)
                        {
                            case E_FindPath.NPC:
                                roadSB.Append($"{roadTypeDic[curTarget.Target.FindPath.FindPathType]}:{GetNPCName(curTarget.Target.FindPath.ID, npcData)}|");
                                break;
                            case E_FindPath.InterAction:
                                roadSB.Append($"{roadTypeDic[curTarget.Target.FindPath.FindPathType]}:{GetInterName(curTarget.Target.FindPath.ID, interactData)}|");
                                break;
                            default:
                                roadSB.Append($"{roadTypeDic[curTarget.Target.FindPath.FindPathType]}:{curTarget.Target.FindPath.ID}|");
                                break;
                        }
                    }
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        roadSB.Remove(roadSB.Length - 1, 1);
                    }
                    string road = roadSB.ToString();

                    //效果列表

                    data.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}\n",
                    //任务序号
                    (int)task.Value.Base.TaskType * 10000000 + task.Value.Base.Chapter * 100000 + task.Value.Base.Index,
                    //任务ID
                    task.Value.Base.TaskID,
                    //章节名字
                    task.Value.Base.ChapterName,
                    //任务名字
                    task.Value.Base.TaskName,
                    //任务描述
                    taskDesc,
                    //任务目标
                    taskTarget,
                    //任务类型
                    task.Value.Base.TaskType.ToString(),
                    //接任务NPC
                    pickTaskNPC,
                    //接任务效果
                    pickTaskEffect,
                    //交任务NPC
                    finishTaskNPC,
                    //交任务效果
                    finishTaskEffect,
                    //任务参数
                    taskArg,
                    //寻路参数
                    road,
                    //效果参数
                    taskTargetEffect
                    );
                }
            }
#if UNITY_EDITOR

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(data.ToString());
            writer.Flush();
            writer.Close();
            writer.Dispose();
#endif
            EditorUtility.DisplayDialog("消息", "导出成功", "ok");
        }
    }

    private void Butbutcopydata_clicked()
    {
        var path = EditorUtility.SaveFilePanel("选择导出任务列表", "", "task.csv", "*.csv");

        if (path != "")
        {
            var data = new StringBuilder();
            foreach (var tasklist in Tasks)
            {
                foreach (var task in tasklist.Value)
                {
                    string target = "";
                    if (task.Value.TaskFinishInfo.Targets.Count > 0)
                    {
                        var tartype = task.Value.TaskFinishInfo.Targets[0].Target.EditorTaskType.TaskType;
                        foreach (var item in (ValueDropdownList<TaskType>)TaskEnumUtils._tasktypes)
                        {
                            if (item.Value == tartype)
                            {
                                target = item.Text;
                                break;
                            }
                        }
                    }

                    data.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",
                        (int)task.Value.Base.TaskType * 10000000 + task.Value.Base.Chapter * 100000 +
                        task.Value.Base.Index,
                        task.Value.Base.TaskID,
                        task.Value.Base.ChapterName,
                        task.Value.Base.TaskName,
                        (int)task.Value.Base.TaskType,
                        task.Value.TaskFinishInfo.FinishTaskNPC,
                        target);
                }
            }
#if UNITY_EDITOR

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(data.ToString());
            writer.Flush();
            writer.Close();
            writer.Dispose();
            //FileStream fs = new StreamWriter(path,(path, FileMode.CreateNew);
            //byte[] bytes = Encoding.Unicode.GetBytes(data.ToString());
            //fs.Write(bytes, 0, bytes.Length);
            //fs.Flush();
            //fs.Close();
            //fs.Dispose();
#endif
            EditorUtility.DisplayDialog("消息", "导出成功", "ok");
        }
    }

#if UNITY_EDITOR

    public void GetEffectSB(ref StringBuilder sb, Task.EffectSerialize curEffect, NpcData npcData, InteractData interactData, MonsterData monsterData, ItemData itemData, CommonDialogData commonDialogData, TeleDialogData teleDialogData)
    {
        switch (curEffect.EffectType)
        {
            case FunctionType.ReleaseSkill:
                sb.Append("释放技能:");
                sb.Append($"{(curEffect.baseEffect as EffectReleaseSkill).SkillID}|");
                break;
            case FunctionType.AddBuff:
                sb.Append("添加Buff:");
                sb.Append($"{(curEffect.baseEffect as EffectAddBuff).BuffID}|");
                break;
            case FunctionType.RemoveBuff:
                sb.Append("移除Buff:");
                sb.Append($"{(curEffect.baseEffect as EffectRemoveBuff).BuffID}|");
                break;
            case FunctionType.ReciveTask:
                sb.Append("接收任务:");
                sb.Append($"{(curEffect.baseEffect as EffectReciveTask).TaskID}|");
                break;
            case FunctionType.FinishTask:
                sb.Append("完成任务:");
                sb.Append($"{(curEffect.baseEffect as EffectFinishTask).TaskID}|");
                break;
            case FunctionType.EntityLevel:
                sb.Append("进入副本:");
                sb.Append($"{(curEffect.baseEffect as EffectEntityLevel).MapID}|");
                break;
            case FunctionType.Dialogue:
                sb.Append("触发对话:");
                sb.Append($"{GetDialogType((curEffect.baseEffect as EffectDialogue).DialogueID, commonDialogData, teleDialogData)}|");
                break;
            case FunctionType.PlayBlack:
                sb.Append("播放黑幕:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayBlack).BlackID}|");
                break;
            case FunctionType.TaskProcess:
                sb.Append("任务进度增加:");
                sb.Append($"{(curEffect.baseEffect as EffectTaskProcess).EventID}|");
                break;
            case FunctionType.PlayTimeline:
                sb.Append("播放Timeline:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayTimeline).TimelineID}|");
                break;
            case FunctionType.PlayPlot:
                sb.Append("播放章节效果:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayPlot).PlotID}|");
                break;
            case FunctionType.PlayImage:
                sb.Append("播放剧情图片:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayImage).ImageID}|");
                break;
            case FunctionType.OpenNpcShop:
                sb.Append("打开NPC商店:");
                sb.Append($"{(curEffect.baseEffect as EffectOpenNpcShop).ShopID}|");
                break;
            case FunctionType.SendCustomEvent:
                sb.Append("发送事件:");
                sb.Append($"{(curEffect.baseEffect as EffectSendCustomEvent).CustomKey}|");
                break;
            case FunctionType.SceneControlFlag:
                sb.Append("大场景标记位控制:");
                sb.Append($"{(curEffect.baseEffect as EffectSceneControlFlag).Flag}|");
                break;
            case FunctionType.SameSceneTranslate:
                sb.Append("同场景TP:");
                sb.Append($"{(curEffect.baseEffect as EffectSameSceneTranslate).X}，{(curEffect.baseEffect as EffectSameSceneTranslate).Y}，{(curEffect.baseEffect as EffectSameSceneTranslate).Z}|");
                break;
            case FunctionType.ChangeNpcState:
                sb.Append("切换NPC状态机:");
                sb.Append($"{(curEffect.baseEffect as EffectChangeNpcState).NpcID}|");
                break;
            case FunctionType.ClientSendModuleMsg:
                sb.Append("客户端发送事件:");
                sb.Append($"{(curEffect.baseEffect as EffectClientSendModuleMsg).EventName}|");
                break;
            case FunctionType.TeamEctypeChallenge:
                sb.Append("发起组队日常本挑战|");
                break;
            case FunctionType.TreasureFind:
                sb.Append("藏宝图挖宝|");
                break;
            case FunctionType.SendBattleOpenEvent:
                sb.Append("发送关卡开启事件|");
                break;
            case FunctionType.ActiveSceneCam:
                sb.Append("激活场景虚拟相机:");
                sb.Append($"{(curEffect.baseEffect as EffectActiveSceneCam).Index}|");
                break;
            case FunctionType.RiskLevelBreak:
                sb.Append("冒险等级突破效果|");
                break;
            case FunctionType.UpItem:
                sb.Append("加、减道具进背包:");
                foreach (var item in (curEffect.baseEffect as EffectUpItem).AddItems)
                {
                    sb.Append($"{item.ItemID}，");
                }
                if ((curEffect.baseEffect as EffectUpItem).AddItems.Count > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("|");
                break;
            case FunctionType.ModifyEntityTempVisibility:
                sb.Append("修改对象临时显隐|");
                break;
            case FunctionType.PlayAnimation:
                sb.Append("播放指定动画:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayAnimation).AnimationName}|");
                break;
            case FunctionType.PlayFx:
                sb.Append("播放指定特效:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayFx).FxName}|");
                break;
            case FunctionType.ChanageHide:
                sb.Append("切换显隐:");
                sb.Append($"{(curEffect.baseEffect as EffectChanageHide).HideType}|");
                break;
            case FunctionType.AddPassiveSkill:
                sb.Append("添加被动:");
                sb.Append($"{(curEffect.baseEffect as EffectAddPassiveSkill).PassiveSkillID}|");
                break;
            case FunctionType.TransJob:
                sb.Append("转职:");
                sb.Append($"{(curEffect.baseEffect as EffectTransJob).TransJobID}|");
                break;
            case FunctionType.TriggerGuide:
                sb.Append("触发引导:");
                sb.Append($"{(curEffect.baseEffect as EffectTriggerGuide).GuideID}|");
                break;
            case FunctionType.ShowMessage:
                sb.Append("显示飘字:");
                sb.Append($"{(curEffect.baseEffect as EffectShowMessage).ConfigID}|");
                break;
            case FunctionType.ChangeSceneObjState:
                sb.Append("改变场景对象状态:");
                sb.Append($"{(curEffect.baseEffect as EffectChangeSceneObjState).ID}|");
                break;
            case FunctionType.NpcMove:
                sb.Append("NPC移动:");
                sb.Append($"{(curEffect.baseEffect as EffectNpcMove).NpcIndex}|");
                break;
            case FunctionType.Translate:
                sb.Append("场景传送:");
                sb.Append($"{(curEffect.baseEffect as EffectTranslate).MapID}-{(curEffect.baseEffect as EffectTranslate).Spid}|");
                break;
            case FunctionType.Wait:
                sb.Append("等待|");
                break;
            case FunctionType.PassiveRemove:
                sb.Append("移除被动效果:");
                sb.Append($"{(curEffect.baseEffect as EffectPassiveRemove).PassiveID}|");
                break;
            case FunctionType.ScreenImpulse:
                sb.Append("屏幕震动:");
                sb.Append($"{(curEffect.baseEffect as EffectScreenImpulse).Path}|");
                break;
            case FunctionType.PlayPv:
                sb.Append("播放视频:");
                sb.Append($"{(curEffect.baseEffect as EffectPlayPv).ID}|");
                break;
            case FunctionType.ExitCurMap:
                sb.Append("播放视频|");
                break;
            case FunctionType.ServerModifyEntityVisible:
                sb.Append("服务器修改对象的显隐:");
                sb.Append($"{(curEffect.baseEffect as EffectServerModifyEntityVisible).MapID}：{(curEffect.baseEffect as EffectServerModifyEntityVisible).Index}");
                break;
            case FunctionType.CheckBlackBoard:
                sb.Append("副本黑板:");
                sb.Append($"{(curEffect.baseEffect as EffectCheckBlackBoard).Key}：{(curEffect.baseEffect as EffectCheckBlackBoard).Value}");
                break;
            case FunctionType.SystemGuide:
                sb.Append("系统引导:");
                sb.Append($"{(curEffect.baseEffect as EffectSystemGuide).guideID}|");
                break;
            case FunctionType.PartnerDialog:
                sb.Append("伙伴喊话:");
                sb.Append($"{(curEffect.baseEffect as EffectPartnerDialog).dialogID}|");
                break;
            case FunctionType.ChanageSceneTransitionData:
                sb.Append("切图过渡信息配置|");
                break;
            case FunctionType.FakePartnerUI:
                sb.Append("假伙伴UI|");
                break;
            case FunctionType.Transition:
                sb.Append("转场过渡|");
                break;
            default:
                break;
        }
    }

    public string GetNPCName(long id, NpcData npcData)
    {
        if (npcData.StaticNpcDatas.ContainsKey(id))
        {
            return id + npcData.StaticNpcDatas[id].Name;
        }
        return id.ToString();
    }

    public string GetInterName(long id, InteractData interactData)
    {
        if (interactData.StaticInteractDatas.ContainsKey(id))
        {
            return id + interactData.StaticInteractDatas[id].ModelName;
        }
        return id.ToString();
    }

    public string GetMonsterName(long id, MonsterData monsterData)
    {
        if (monsterData.StaticMonsterDatas.ContainsKey(id))
        {
            return id + monsterData.StaticMonsterDatas[id].Name;
        }
        return id.ToString();
    }

    public string GetItemName(int id, ItemData itemData)
    {
        if (itemData.StaticItemDatas.ContainsKey(id))
        {
            return id + itemData.StaticItemDatas[id].Name;
        }
        return id.ToString();
    }

    public string GetDialogType(int id, CommonDialogData commonDialogData, TeleDialogData teleDialogData)
    {
        if (commonDialogData.StaticCommonDialogDatas.ContainsKey(id))
        {
            switch (commonDialogData.StaticCommonDialogDatas[id].GetDialog_type())
            {
                case 1:
                    return id + "剧情对话";
                case 2:
                    return id + "3D角色冒泡";
                case 3:
                    return id + "氛围对话";
                case 4:
                    return id + "黑屏文字";
                case 5:
                    return id + "心灵传讯";
                case 6:
                    return id + "叙事旁白";
                case 7:
                    return id + "书信";
            }
        }
        else
        {
            if (teleDialogData.StaticTeleDialogDatas.ContainsKey(id))
            {
                switch (teleDialogData.StaticTeleDialogDatas[id].GetDialog_type())
                {
                    case 1:
                        return id + "剧情对话";
                    case 2:
                        return id + "3D角色冒泡";
                    case 3:
                        return id + "氛围对话";
                    case 4:
                        return id + "黑屏文字";
                    case 5:
                        return id + "心灵传讯";
                    case 6:
                        return id + "叙事旁白";
                    case 7:
                        return id + "书信";
                }
            }
        }
        return id.ToString();
    }

#endif
    private bool GetAllDialog(ref StringBuilder stringBuilder, StarProjectDef.CommonDialogData commonDialogData, TeleDialogData teleDialogDatat, int id, List<int> oldID = null)
    {
        if (oldID == null)
        {
            oldID = new List<int>();
        }
        if (commonDialogData.StaticCommonDialogDatas.ContainsKey(id))
        {
            foreach (var nextID in commonDialogData.StaticCommonDialogDatas[id].Next_dialog_id)
            {
                if (nextID == 0)
                {
                    if (!string.IsNullOrEmpty(commonDialogData.StaticCommonDialogDatas[id].Name))
                    {
                        stringBuilder.Append($"{commonDialogData.StaticCommonDialogDatas[id].Name}：");
                    }
                    if (!string.IsNullOrEmpty(commonDialogData.StaticCommonDialogDatas[id].Dialog_desc))
                    {
                        stringBuilder.Append($"{commonDialogData.StaticCommonDialogDatas[id].Dialog_desc.Replace("\r", "").Replace("\n", "")}。");
                    }
                    return true;
                }
                else
                {
                    if (!oldID.Contains(id))
                    {
                        if (!string.IsNullOrEmpty(commonDialogData.StaticCommonDialogDatas[id].Name))
                        {
                            stringBuilder.Append($"{commonDialogData.StaticCommonDialogDatas[id].Name}：");
                        }
                        if (!string.IsNullOrEmpty(commonDialogData.StaticCommonDialogDatas[id].Dialog_desc))
                        {
                            stringBuilder.Append($"{commonDialogData.StaticCommonDialogDatas[id].Dialog_desc.Replace("\r", "").Replace("\n", "")}；");
                        }
                        oldID.Add(id);
                        GetAllDialog(ref stringBuilder, commonDialogData, teleDialogDatat, nextID, oldID);
                    }
                    return true;
                }
            }
        }
        if (teleDialogDatat.StaticTeleDialogDatas.ContainsKey(id))
        {
            foreach (var nextID in teleDialogDatat.StaticTeleDialogDatas[id].Next_dialog_id)
            {
                if (nextID == 0)
                {
                    if (!string.IsNullOrEmpty(teleDialogDatat.StaticTeleDialogDatas[id].Name))
                    {
                        stringBuilder.Append($"{teleDialogDatat.StaticTeleDialogDatas[id].Name}：");
                    }
                    if (!string.IsNullOrEmpty(teleDialogDatat.StaticTeleDialogDatas[id].Dialog_desc))
                    {
                        stringBuilder.Append($"{teleDialogDatat.StaticTeleDialogDatas[id].Dialog_desc.Replace("\r", "").Replace("\n", "")}。");
                    }
                    return true;
                }
                else
                {
                    if (!oldID.Contains(id))
                    {
                        if (!string.IsNullOrEmpty(teleDialogDatat.StaticTeleDialogDatas[id].Name))
                        {
                            stringBuilder.Append($"{teleDialogDatat.StaticTeleDialogDatas[id].Name}：");
                        }
                        if (!string.IsNullOrEmpty(teleDialogDatat.StaticTeleDialogDatas[id].Dialog_desc))
                        {
                            stringBuilder.Append($"{teleDialogDatat.StaticTeleDialogDatas[id].Dialog_desc.Replace("\r", "").Replace("\n", "")}；");
                        }
                        oldID.Add(id);
                        GetAllDialog(ref stringBuilder, commonDialogData, teleDialogDatat, nextID, oldID);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private bool GetAllBlack(ref StringBuilder stringBuilder, StarProjectDef.BlackMovieData blackMovieData, int id, List<int> oldID = null, int index = 0)
    {
        if (oldID == null)
        {
            oldID = new List<int>();
        }
        if (blackMovieData.StaticBlackMovieDatas.ContainsKey(id))
        {
            int nextID = blackMovieData.StaticBlackMovieDatas[id].GetNext_id();

            if (nextID == 0)
            {
                if (!string.IsNullOrEmpty(blackMovieData.StaticBlackMovieDatas[id].Desc))
                {
                    stringBuilder.Append($"{blackMovieData.StaticBlackMovieDatas[id].Desc.Replace("\r", "").Replace("\n", "")}。");
                }
                return true;
            }
            else
            {
                if (!oldID.Contains(id))
                {
                    if (!string.IsNullOrEmpty(blackMovieData.StaticBlackMovieDatas[id].Desc))
                    {
                        stringBuilder.Append($"{blackMovieData.StaticBlackMovieDatas[id].Desc.Replace("\r", "").Replace("\n", "")}；");
                    }
                    oldID.Add(id);
                    GetAllBlack(ref stringBuilder, blackMovieData, nextID, oldID);
                }
                return true;
            }
        }
        return false;
    }

    private ChainInfo getChapterByMenu(OdinMenuItem odin)
    {
        if (odin == null) return null;
        if (odin.Value is ChainInfo result)
        {
            return result;
        }
        else
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            return getChapterByMenu(odin.Parent);
        }
    }
    private TaskConfigInfo getTaskByMenu(OdinMenuItem odin)
    {
        if (odin == null) return null;
        if (odin.Value is TaskConfigInfo result)
        {
            return result;
        }
        else
        {
            //Debug.LogWarning(odin.Name+odin.Value);
            return getTaskByMenu(odin.Parent);
        }
    }
    /// <summary>
    /// 任务身上右击菜单
    /// </summary>
    /// <param name="odin"></param>
    private void MenuRight_clicked(OdinMenuItem odin)
    {
        this.MenuItemMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
    }

    /// <summary>
    /// 章节数据更新生效
    /// </summary>
    /// <param name="odin"></param>
    /// <param name="info"></param>
    private void ButChapterUpdata_clicked(OdinMenuItem odin, ChapterInfo info)
    {
        foreach (var item in odin.ChildMenuItems)
        {
            int index = 0;
            foreach (var child in item.ChildMenuItems)
            {
                if (child.Value is TaskConfigInfo taskmd)
                {
                    taskmd.Base.Chapter = info.ID;
                    taskmd.Base.ChapterName = info.Name;
                    if (taskmd.Base.HasPre&& index != 0)
                    {
                        TaskConfigInfo preTaskInfo= item.ChildMenuItems[index - 1].Value as TaskConfigInfo;
                        taskmd.Base.PreTaskID = preTaskInfo.Base.TaskID;
                    }
                    if (taskmd.Base.HasNext && index != item.ChildMenuItems.Count-1)
                    {
                        TaskConfigInfo nextTaskInfo = item.ChildMenuItems[index + 1].Value as TaskConfigInfo;
                        taskmd.Base.NextTaskID = nextTaskInfo.Base.TaskID;
                    }
                }
                index++;    
            }

        }

        odin.Name = $"{info.ID}.{info.Name}";
    }

    private void ButChainUpdata_clicked(OdinMenuItem odin, ChainInfo info)
    {
        foreach (var item in odin.ChildMenuItems)
        {
            if (item.Value is TaskConfigInfo taskmd)
            {
                taskmd.Base.Chapter = info.ID;
                taskmd.Base.ChapterName = info.Name;
                taskmd.Base.ChainID = info.ChainID;
            }
        }

        odin.Name = $"{info.ChainID}";
    }

    #endregion
}