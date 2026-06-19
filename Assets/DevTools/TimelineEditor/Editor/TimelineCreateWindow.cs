///--------------------------------------------------------------------
/// 文件名   :   TimelineCreateWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/16 14:07:01
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;

public class TimelineCreateWindow : OdinMenuEditorWindow
{
    public CreateTimelineGUI m_CreateGUI;
    
    public OdinMenuTree tree;

    [MenuItem("Tools/Timeline编辑器/TimelineCreate")]
    public static void OpenWindow()
    {
        TimelineCreateWindow window = GetWindow<TimelineCreateWindow>("Timeline编辑器");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        window.Show();
    }

    public void CreateGUI()
    {
        TimelineConfigUtils.Init();

    }

    protected override OdinMenuTree BuildMenuTree()
    {
        tree = new OdinMenuTree(true);

        var customMenuStyle = new OdinMenuStyle
        {
            BorderPadding = 0f,
            AlignTriangleLeft = false,
            TriangleSize = 16f,
            TrianglePadding = 0f,
            Offset = 20f,
            Height = 23,
            IconPadding = 0f,
            BorderAlpha = 0.323f,
        };

        tree.AddAssetAtPath("配置", "Assets/DevTools/TimelineEditor/TimelineConfigs.asset");
        tree.AddAllAssetsAtPath("列表","Assets/DevTools/TimelineEditor/Configs");
        
        //新建
        m_CreateGUI = new CreateTimelineGUI();
        var versionInfo = new CreateMenuItem(tree, "新建Timeline", m_CreateGUI);
        tree.AddMenuItemAtPath("新建", versionInfo);
        tree.EnumerateTree()
            .AddThumbnailIcons()
            .SortMenuItemsByName();

        return tree;
    }
    
    [System.Serializable]
    public class CreateMenuItem : OdinMenuItem
    {
        private readonly CreateTimelineGUI instance;
        public string ItemName;

        public CreateMenuItem(OdinMenuTree tree, string name, CreateTimelineGUI version) : base(tree, name, version)
        {
            this.ItemName = name;
            this.instance = version;
        }

        public override string SmartName
        {
            get { return ItemName; }
        }
    }
}

