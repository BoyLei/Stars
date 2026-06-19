using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OfficeOpenXml;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using SkillEditor;
using StarProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialWindow : OdinMenuEditorWindow
{
    [LabelText("文件路径")] public string SavePath = "Assets/Res/Config/Guide/TutorialConfig.json";

    public int MaxID;

    private OdinMenuTree tree;

    Dictionary<int, TutorialMenuItem> OdinMenuItems = new();

    private Dictionary<int, TutorialConfig> Configs = new();

    private string CacheExportLanguagePath = "Assets/DevTools/TutorialEditor/Editor/CacheExportLanguage.txt";
    private UnityEngine.UIElements.TextField textField;
    public string LanguageElsxPath
    {
        get => textField.value;
        set => textField.value = value;
    }


    [MenuItem("Tools/引导配置")]
    public static void OpenWindow()
    {
        var window = GetWindow<TutorialWindow>("引导配置");
        window.Show();
    }

    public void OnSaveSavePath()
    {
        EditorPrefs.SetString("TutorialWindow_SavePath", SavePath);
    }

    public void OnInitSavePath()
    {
        SavePath = EditorPrefs.GetString("TutorialWindow_SavePath");
    }

    private void OnEnable()
    {
        Configs.Clear();
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(SavePath);
        if (textAsset != null)
        {
            var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TutorialConfigList>(textAsset.text);
            foreach (var item in taskConfigs.list)
            {
                if (!Configs.ContainsKey(item.Key))
                {
                    Configs.Add(item.Key, item.Value);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Configs.Clear();
    }

    public void CreateGUI()
    {
        this.MenuItemMenu = new GenericMenu();
        this.MenuItemMenu.AddItem(new GUIContent("删  除"), false, Butdelete_clicked);
        this.MenuItemMenu.AddSeparator("");
        this.MenuItemMenu.AddItem(new GUIContent("新建引导"), false, Butnew_clicked);


        this.TrueeMouseRightClick += TaskWindow_TrueeMouseRightClick;
        this.RightClickMenu = new GenericMenu();
        this.RightClickMenu.AddItem(new GUIContent("新建引导"), false, Butnew_clicked);
        this.RightClickMenu.AddItem(new GUIContent("保存"), false, Butdelete_Save);

        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DevTools/TutorialEditor/Editor/ToolBar.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        
        ToolbarButton but;
        but = root.Q<ToolbarButton>("butsave");
        but.clicked += Butdelete_Save;
        but = root.Q<ToolbarButton>("butnew");
        but.clicked += Butnew_clicked;
        Button but1 = root.Q<UnityEngine.UIElements.Button>("btnselectionlanguagefield");
        but1.clicked += Btnselectionlanguagefield_clicked;

        textField = root.Q<UnityEngine.UIElements.TextField>("textfieldlanguagepath");

        string mTemplate = File.ReadAllText(CacheExportLanguagePath, System.Text.Encoding.UTF8);

        textField.value = mTemplate;
    }

    private void Butdelete_Save()
    {
        //Debug.Log($"保存 {pathSelectionField.Path}");
        JsonSerializerSettings setting = new();
        setting.NullValueHandling = NullValueHandling.Ignore;
        TutorialConfigList list = new();
        if (Configs != null)
        {
            foreach (var item in Configs)
            {
                if (!list.list.ContainsKey(item.Value.ID))
                {
                    list.list.Add(item.Value.ID, item.Value);
                }
            }
        }

        string content = Newtonsoft.Json.JsonConvert.SerializeObject(list, setting);
        WriteJson(content, SavePath);

        ExportTutorialTextStrByMapEditorSave();
    }

    public void WriteJson(string content, string path)
    {
#if UNITY_EDITOR

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileStream fs = new(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void Butdelete_clicked()
    {
        if (tree != null && tree.Selection != null && tree.Selection.SelectedValues != null)
        {
            foreach (var item in tree.Selection.SelectedValues)
            {
                if (item is TutorialConfig confiog)
                {
                    if (confiog != null)
                    {
                        if (OdinMenuItems.ContainsKey(confiog.ID))
                        {
                            OdinMenuItems.Remove(confiog.ID);
                        }

                        if (Configs.ContainsKey(confiog.ID))
                        {
                            Configs.Remove(confiog.ID);
                        }
                    }
                }
            }

            ForceMenuTreeRebuild();
        }
        /*Debug.Log("删除");
        int id = MaxID++;
        TutorialConfig config = new TutorialConfig();
        config.ID = id;
        TutorialMenuItem odin = new TutorialMenuItem(tree, config);
        odin.OnRightClick = this.MenuRight_clicked;
        if (!OdinMenuItems.ContainsKey(id))
        {
            OdinMenuItems.Add(id, odin);
        }
        tree.MenuItems.Add(odin);*/
    }

    public int GetID()
    {
        MaxID++;
        return MaxID;
    }

    private void Butnew_clicked()
    {
        TutorialConfig oldConfig = null;
        string Name = "";
        if (tree != null && tree.Selection != null && tree.Selection.SelectedValues != null)
        {
            foreach (var item in tree.Selection.SelectedValues)
            {
                if (item is TutorialConfig confiog)
                {
                    if (confiog != null)
                    {
                        oldConfig = confiog;
                        Name = confiog.Name;
                        break;
                    }
                }
            }
        }

        int id = GetID();
        TutorialConfig config = new();
        config.ID = id;
        config.Name = Name;
        if (oldConfig != null)
        {

            config.FlagIndex= oldConfig.FlagIndex;
            config.GroupID = oldConfig.GroupID;
        }
        config.NextID = id+1;
        TutorialMenuItem odin = new(tree, config);
        odin.OnRightClick = this.MenuRight_clicked;
        odin.Name = config.GetViewName();
        if (!OdinMenuItems.ContainsKey(id))
        {
            OdinMenuItems.Add(id, odin);
        }

        if (!Configs.ContainsKey(id))
        {
            Configs.Add(id, config);
        }
        tree.AddMenuItemAtPath(config.GroupID+config.Name, odin);
        //tree.MenuItems.Add(odin);
    }

    private void Btnselectionlanguagefield_clicked()
    {
        string path = EditorUtility.OpenFolderPanel("选择导出引导多语言配置路径", Application.dataPath, "");
        if (!string.IsNullOrEmpty(path))
        {
            if (textField != null && textField.text != null)
            {
                string TutorialEditorTextPath = $"{path}/引导编辑器文本_TutorialEditorText.xlsx";
                textField.value = TutorialEditorTextPath;

                File.WriteAllText(CacheExportLanguagePath, TutorialEditorTextPath);
            }
        }
    }

    private void OnDisable()
    {
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
        MaxID = 0;

        tree.AddAssetAtPath("服务器标记配置", "Assets/Res/Config/Guide/TutorialFlagConfig.asset");


        if (Configs != null)
        {
            var list = Configs.OrderBy(x => x.Value.GroupID).ThenBy(x => x.Value.ID);
            foreach (var item in list)
            {
                TutorialMenuItem odin = new(tree, item.Value);
                odin.OnRightClick = this.MenuRight_clicked;
                if (!OdinMenuItems.ContainsKey(item.Value.ID))
                {
                    OdinMenuItems.Add(item.Value.ID, odin);
                }

                if (item.Value.ID > MaxID)
                {
                    MaxID = item.Value.ID;
                }
                tree.AddMenuItemAtPath(item.Value.GroupID+item.Value.Name, odin);
                // tree.MenuItems.Add(odin);
            }
        }

        tree.EnumerateTree()
            .AddThumbnailIcons()
            .SortMenuItemsByName(false);
        return tree;
    }

    /// <summary>
    /// 右击菜单
    /// </summary>
    private GenericMenu MenuItemMenu;

    /// <summary>
    /// 右击菜单
    /// </summary>
    private GenericMenu RightClickMenu;

    public event Action<Event> TrueeMouseRightClick;

    /// <summary>
    /// 任务身上右击菜单
    /// </summary>
    /// <param name="odin"></param>
    private void MenuRight_clicked(OdinMenuItem odin)
    {
        this.MenuItemMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
    }

    private void OnAddItem(int id)
    {
        Debug.LogError(id);
    }

    private void TaskWindow_TrueeMouseRightClick(Event obj)
    {
        //空白处右击
        this.RightClickMenu.DropDown(new Rect(obj.mousePosition, Vector2.zero));
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


    #region 多语言导出
    public class TutorialExportArr
    {
        public int ID;
        public string Key;
        public string Val;
        public string Des;

        public void SetData(int id, string key, string val, string des)
        {
            ID = id;
            Key = key;
            Val = val;
            Des = des;
        }

        public void ExportExcel(int row, ExcelRange excel)
        {
            int index = 1;
            excel[row, index++].Value = Key;
            excel[row, index++].Value = Val;
            excel[row, index++].Value = ID;
            excel[row, index++].Value = Des;
        }
    }

    private void Butdelete_ExportLanguage()
    {
        Debug.Log("导出引导多语言");
        TutorialConfigList list = new();
        if (Configs != null)
        {
            foreach (var item in Configs)
            {
                if (!list.list.ContainsKey(item.Value.ID))
                {
                    list.list.Add(item.Value.ID, item.Value);
                }
            }
        }

        List<TutorialExportArr> arr = new();
        foreach (var item in list.list)
        {
            if (!string.IsNullOrEmpty(item.Value.TipText))
            {
                TutorialExportArr tutorialExportArr = new();
                tutorialExportArr.SetData(item.Value.ID, $"TutorialEditor_Tips_{item.Value.ID}", item.Value.TipText, item.Value.Name);
                arr.Add(tutorialExportArr);
            }
        }

        WriteLanguageExcel(arr);
    }

    private void WriteLanguageExcel(List<TutorialExportArr> arr)
    {
        //string path = "D:\\stars\\StarsProject_Design\\trunk\\配置文件\\编辑器文本\\引导编辑器文本_TutorialEditorText.xlsx";
        //var path = EditorUtility.SaveFilePanel("选择导出引导文本路径", "", "引导编辑器文本_TutorialEditorText.xlsx", "*.xlsx");
        if (LanguageElsxPath != "")
        {
            FileInfo newFile = new(LanguageElsxPath);
            if (newFile.Exists)
            {
                //创建一个新的excel文件
                newFile.Delete();
                newFile = new FileInfo(LanguageElsxPath);
            }
            //通过ExcelPackage打开文件
            using (ExcelPackage package = new(newFile))
            {
                //在excel空文件添加新sheet
                ExcelWorksheet config = package.Workbook.Worksheets.Add("TutorialEditorText");
                int index = 1;
                config.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                config.Cells[1, index++].Value = "多语言key";
                config.Cells[1, index++].Value = "文本";
                config.Cells[1, index++].Value = "编号";
                config.Cells[1, index++].Value = "描述";

                int row = 6;
                foreach (var item in arr)
                {
                    item.ExportExcel(row++, config.Cells);
                }

                config.Cells.AutoFitColumns();
                //保存excel
                package.Save();

                EditorUtility.DisplayDialog("消息", $"一共:{arr.Count}条\n\n\t导出成功", "您辛苦了");
            }
        }
    }

    static public string ConvertJsonString(string str)
    {
        //格式化json字符串
        JsonSerializer serializer = new();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new();
            JsonTextWriter jsonWriter = new(textWriter)
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

    /// <summary>
    /// 地图编辑器保存时自动导出多语言xlsm文件，及把对应的策划目录和客户端目录下的json明文全部替换为key
    /// </summary>
    public void ExportTutorialTextStrByMapEditorSave()
    {
        //先替换保存策划目录源数据
        ExportTutorialText();
        ////多语言excel生成
        Butdelete_ExportLanguage();
    }

    private void ExportTutorialText()
    {
        List<TutorialExportArr> arr = new();
        var textAsset = File.ReadAllText(SavePath);
        if (textAsset != null)
        {
            var taskConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<TutorialConfigList>(textAsset);
            foreach (var item in taskConfigs.list)
            {
                if (!string.IsNullOrEmpty(item.Value.TipText))
                {
                    TutorialExportArr tutorialExportArr = new();
                    string key = $"TutorialEditor_Tips_{item.Value.ID}";
                    tutorialExportArr.SetData(item.Value.ID, key, item.Value.TipText, item.Value.Name);
                    arr.Add(tutorialExportArr);
                    item.Value.TipText_Key = key;
                }
                else
                {
                    item.Value.TipText_Key = "";
                }
            }
            JsonSerializerSettings setting = new();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(taskConfigs, setting);
            jsonstr = ConvertJsonString(jsonstr);

            //string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonAsset, Newtonsoft.Json.Formatting.Indented,);
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            File.WriteAllText(SavePath, jsonstr);
        }
    }

    #endregion


}

public class TutorialMenuItem : OdinMenuItem
{
    private readonly TutorialConfig instance;


    public TutorialMenuItem(OdinMenuTree tree, TutorialConfig instance) : base(tree, instance.ID.ToString(),
        instance)
    {
        this.instance = instance;
        SearchString = SmartName;
    }

    protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
        {
            var selection = this.MenuTree.Selection
                .Select(x => x.Value)
                .OfType<TutorialConfig>();

            if (selection.Any())
            {
                Event.current.Use();
            }
        }
    }

    public override string SmartName
    {
        get { return $"{this.instance.GroupID}_{this.instance.ID}_{this.instance.Name}"; }
    }
}