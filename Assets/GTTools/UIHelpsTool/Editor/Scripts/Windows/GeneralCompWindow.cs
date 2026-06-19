/*
 * @Description: 通用件预览窗口
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    internal class GeneralCompWindow : OdinEditorWindow
    {
        [NonSerialized]
        public static GeneralCompWindow mainWindow;
        [NonSerialized]
        public OdinMenuItem mCurSelectItem;
        [NonSerialized]
        public string searchFile = "";
        private static int lastSelectIndex = -1;
        //预览列表
        private static List<GeneralCompTypeConfig> previewConfigs = new List<GeneralCompTypeConfig>();

        //绘制通用设置
        private PropertyTree generalSettingtree;

        [MenuItem("Tools/UI辅助工具 (v1.0.0)/Prefab预览管理", priority = 201)]
        public static void Open()
        {
            try
            {
                EditorUtility.DisplayProgressBar("提示", "加载数据中", 0);
                UIHelpsToolGlobalConfig.InitGeneralData();
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogErrorFormat("打开Prefab预览管理工具失败, {0}", e.ToString());
                return;
            }

            GeneralCompWindow window = GetWindow<GeneralCompWindow>("Prefab预览管理 (v1.0.0)");
            window.autoRepaintOnSceneChange = true;
            window.Show();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(1100f, 600f);
            window.minSize = new Vector2(330, 185);
        }

        public static GeneralCompWindow GetWindow()
        {
            if (mainWindow == null)
            {
                mainWindow = GetWindow<GeneralCompWindow>();
            }
            return mainWindow;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mainWindow = this;
            GTEditorWindowFocusMgr.Instance.PushEditorWindow(this);
        }

        protected override void OnDestroy()
        {
            searchFile = "";
            mainWindow = null;
            previewConfigs.Clear();
            GTEditorWindowFocusMgr.Instance.PopEditorWindow(this);
            AssetDatabase.SaveAssets();
            base.OnDestroy();
        }

        [NonSerialized]
        public OdinMenuTree menuTree;
        [NonSerialized]
        public int mMenuWidth = 200;
        [NonSerialized]
        public int mTopBarHeight = 30;
        [NonSerialized]
        public int mBottomBarHeight = 20;

        protected OdinMenuTree BuildMenuTree()
        {
            menuTree = new OdinMenuTree();
            menuTree.Selection.SupportsMultiSelect = false;
            menuTree.Config.DrawSearchToolbar = true;
            menuTree.Config.DefaultMenuStyle.IconSize = 16;
            menuTree.Config.DefaultMenuStyle.Height = 30;
            menuTree.Selection.SelectionChanged -= this.SelectionChanged;
            menuTree.Selection.SelectionChanged += this.SelectionChanged;
            this.UpdateMenuTree();
            return menuTree;
        }

        private void SelectionChanged(SelectionChangedType type = SelectionChangedType.SelectionCleared)
        {
            try
            {
                if (mainWindow == null || mainWindow.menuTree == null)
                {
                    return;
                }
                IEnumerable<OdinMenuItem> IE = mainWindow.menuTree.EnumerateTree().Where(x => x.IsSelected);
                if (IE == null)
                {
                    return;
                }
                var item = IE.FirstOrDefault();
                if (item == null)
                {
                    return;
                }
                if (mCurSelectItem != null && item == mCurSelectItem)
                {
                    return;
                }
                mCurSelectItem = item;
                searchFile = "";
                int selectIndex = -1;
                var itemList = menuTree.EnumerateTree().ToList();
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (mCurSelectItem == itemList[i])
                    {
                        selectIndex = i;
                        break;
                    }
                }
                lastSelectIndex = selectIndex;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

        }

        public void UpdateMenuTree()
        {
            previewConfigs.Clear();
            menuTree.MenuItems.Clear();

            //这里不直接使用loadAll了，因为要根据名称重新设置路径
            // menuTree.AddAllAssetsAtPath("", UIHelpsToolUtils.config_dir, typeof(GeneralCompTypeConfig));
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(UnityEngine.Object), new string[] { UIHelpsToolConfigure.ConfigDataPath.TrimEnd('/') });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (go is GeneralCompTypeConfig)
                {
                    GeneralCompTypeConfig cfg = go as GeneralCompTypeConfig;
                    previewConfigs.Add(cfg);
                    // menuTree.Add(cfg.typeName, cfg);
                    int left = cfg.typeName.LastIndexOf('/');
                    string itemName = cfg.typeName.Substring(left + 1);
                    OdinMenuItem item = new OdinMenuItem(menuTree, cfg.typeName.Substring(left + 1), cfg)
                    {
                        Value = cfg,
                        SearchString = cfg.typeName,
                        Name = cfg.typeName.Substring(left + 1)
                    };
                    menuTree.AddMenuItemAtPath("预览分类/" + cfg.typeName.Substring(0, cfg.typeName.Length - itemName.Length), item);
                }

            }
            menuTree.SortMenuItemsByName();

            var settingData = AssetDatabase.LoadAssetAtPath(UIHelpsToolConfigure.ConfigDataPath + "UIHelpsToolSetting.asset", typeof(UIHelpsToolSetting));
            menuTree.MenuItems.Insert(0, new OdinMenuItem(menuTree, "通用设置", settingData)
            {
                Value = settingData,
                SearchString = "通用设置",
                Name = "通用设置",
            });

            UpdateMenuItem();
            SetRightClickEvent(menuTree.MenuItems);

            //首次打开默认第一个
            if (lastSelectIndex == -1)
            {
                var firstDefaultItem = menuTree.EnumerateTree().First();
                if (firstDefaultItem != null)
                {
                    firstDefaultItem.Select();
                }
            }
            else
            {
                //每次更新保持上一次选中，避免页签跳来跳去
                var itemList = menuTree.EnumerateTree().ToList();
                var newIndex = lastSelectIndex;
                if (newIndex > itemList.Count - 1)
                {
                    newIndex = itemList.Count - 1;
                }
                var item = itemList[newIndex];
                if (item != null)
                {
                    item.Select();
                }
            }

        }

        private void UpdateMenuItem()
        {
            menuTree.EnumerateTree().Where(x => x.Value as GeneralCompTypeConfig).ForEach(UpdateMenuItemName);
            menuTree.EnumerateTree().AddIcons(GeneralCompWindow.GetMenuItemIcons);
        }

        //注册Icon
        private static Texture GetMenuItemIcons(OdinMenuItem item)
        {
            if (item.Value != null)
            {
                if (item.Value.GetType() == typeof(UIHelpsToolSetting))
                {
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_GeneralSettingIcon");
                }
                return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_PrefabIcon");
            }
            return item.Toggled ? GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_FolderOpenedIcon") : GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_FolderIcon");
        }

        //更新显示名称
        private void UpdateMenuItemName(OdinMenuItem menuItem)
        {
            if (menuItem.Value != null)
            {
                if (menuItem.Value.GetType() == typeof(GeneralCompTypeConfig))
                {
                    GeneralCompTypeConfig obj = menuItem.Value as GeneralCompTypeConfig;
                    obj.RegisterMenuItem(menuItem);
                    //去掉虚拟的目录名称
                    menuItem.Name = obj.typeName.Substring(obj.typeName.LastIndexOf('/') + 1);
                }
            }
        }

        //重新注册预览的菜单项的搜索信息
        private void UpdateMenuItemSearchStr(OdinMenuItem menuItem)
        {
            if (menuItem.Value != null)
            {
                if (menuItem.Value.GetType() == typeof(GeneralCompTypeConfig))
                {
                    GeneralCompTypeConfig obj = menuItem.Value as GeneralCompTypeConfig;
                    string searchStr = ""; //menuItem.Name;
                    //将prefab名称作为搜索范围
                    foreach (var child in obj.previewItems)
                    {
                        searchStr += child.prefabGo.name + ",";
                    }
                    menuItem.SearchString = searchStr;
                }
            }
        }

        //设置右键功能
        protected void SetRightClickEvent(List<OdinMenuItem> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    continue;
                }
                if (list[i].Value != null && list[i].Value is GeneralCompTypeConfig)
                {
                    list[i].OnRightClick -= PopupFileRightMenu;
                    list[i].OnRightClick += PopupFileRightMenu;
                }
                if (list[i].Value == null && list[i].Name != "预览分类")
                {
                    list[i].OnRightClick -= PopupFolderRightMenu;
                    list[i].OnRightClick += PopupFolderRightMenu;
                }
                if (list[i].ChildMenuItems.Count > 0)
                {
                    SetRightClickEvent(list[i].ChildMenuItems);
                }
            }
        }

        //OdinMenuItem右键功能
        protected void PopupFileRightMenu(OdinMenuItem item)
        {
            var ctrl = Event.current.modifiers == EventModifiers.Control;
            item.Select(ctrl);
            menu = new GTCommonMenuWindow();
            List<object> list = new List<object>();
            object window = this;
            List<object> deleteinfo = new List<object>();
            List<object> modifyItemID = new List<object>();
            deleteinfo.Add(menuTree);
            modifyItemID.Add(menuTree);
            menu.AddItem("修改名称", 0, UIHelpsToolUtils.ModifyPreviewTypeName, modifyItemID);
            menu.AddItem("删除", 0, UIHelpsToolUtils.DeleteFile, deleteinfo);
            Vector2 mousePosition = Event.current.mousePosition;
            Vector2 mouse_Position = mousePosition;
            mouse_Position.y = mouse_Position.y - menuTree.Config.ScrollPos.y + 60;
            menu.ShowAsContext(position, mouse_Position);
        }

        //目录的右键功能
        protected void PopupFolderRightMenu(OdinMenuItem item)
        {
            var ctrl = Event.current.modifiers == EventModifiers.Control;
            item.Select(ctrl);
            menu = new GTCommonMenuWindow();
            List<object> list = new List<object>();
            object window = this;
            List<object> modifyItemID = new List<object>();
            modifyItemID.Add(menuTree);
            menu.AddItem("修改目录名称", 0, UIHelpsToolUtils.ModifyPreviewFolderName, modifyItemID);
            Vector2 mousePosition = Event.current.mousePosition;
            Vector2 mouse_Position = mousePosition;
            mouse_Position.y = mouse_Position.y - menuTree.Config.ScrollPos.y + 60;
            menu.ShowAsContext(position, mouse_Position);
        }

        //绘制操作选项
        private GUIStyle seperator;
        protected string options1 = "操作";
        protected Rect buttonRect1 = new Rect(0, -5, "操作".Length * 25, 24);

        protected int selectindex = 0;

        private void SetupSeperator(int index = 1)
        {
            seperator = new GUIStyle();
            float weight = options1.Length * 25;
            seperator.fixedWidth = weight;
            seperator.fontSize = 14;
            seperator.fontStyle = FontStyle.Normal;
            seperator.hover.background = (Texture2D)GTCommonMenuConfig.menuhover_tex;

            seperator.alignment = TextAnchor.MiddleCenter;
            seperator.focused.background = null;
            seperator.onNormal.background = null;
            seperator.normal.background = selectindex == index ? (Texture2D)GTCommonMenuConfig.menuhover_tex : null;
            seperator.normal.textColor = selectindex == index ? new Color(0, 0, 0) : new Color(0.7f, 0.7f, 0.7f);
        }

        //预览图标的缩放比例
        private float mSizePercent = 2f;
        protected override void OnGUI()
        {
            if (this.menuTree == null)
            {
                this.BuildMenuTree();
            }
            EditorStyles.popup.fontSize = 12;
            EditorStyles.popup.fixedHeight = 18;
            EditorStyles.popup.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginHorizontal();
            {
                //绘制左侧导航栏
                SirenixEditorGUI.BeginBox(GUILayout.Width(mMenuWidth), GUILayout.Height(Screen.height - 2));
                {
                    GUILayout.BeginVertical();
                    {
                        // GUILayout.Space(5);
                        GUILayout.BeginHorizontal("box");
                        {
                            SetupSeperator(1);
                            if (EditorGUILayout.DropdownButton(new GUIContent(options1), FocusType.Passive, seperator, GUILayout.Width(options1.Length * 25), GUILayout.Height(24)))
                            {
                                CreatMenuOption();
                            }
                        }
                        GUILayout.EndHorizontal();
                        if (this.menuTree != null)
                            this.menuTree.DrawMenuTree();
                        base.OnGUI();
                    }
                    GUILayout.EndVertical();
                }
                SirenixEditorGUI.EndBox();

                //绘制右侧预览效果
                if (mCurSelectItem != null && mCurSelectItem.Value != null)
                {
                    SirenixEditorGUI.BeginBox(GUILayout.Width(Screen.width - mMenuWidth), GUILayout.Height(Screen.height - 2));
                    {
                        GUILayout.BeginVertical();
                        {
                            var selected = this.menuTree.Selection.FirstOrDefault();
                            //顶部提示区域
                            SirenixEditorGUI.BeginHorizontalToolbar(mTopBarHeight);
                            {
                                if (selected != null && selected.Value != null)
                                {
                                    var topstyle = new GUIStyle();
                                    topstyle.fontSize = 18;
                                    topstyle.fixedHeight = 35;
                                    topstyle.normal.textColor = Color.white;
                                    topstyle.alignment = TextAnchor.MiddleLeft;
                                    GUILayout.Space(5);
                                    if (mCurSelectItem.Value.GetType() == typeof(UIHelpsToolSetting))
                                    {
                                        GUILayout.Label("通用设置", topstyle);
                                    }
                                    else
                                    {
                                        GUILayout.Label((mCurSelectItem.Value as GeneralCompTypeConfig).typeName, topstyle);
                                        //绘制一个搜索框
                                        searchFile = SirenixEditorGUI.ToolbarSearchField(searchFile);
                                    }
                                }
                            }
                            SirenixEditorGUI.EndHorizontalToolbar();

                            //通用设置
                            if (mCurSelectItem.Value.GetType() == typeof(UIHelpsToolSetting))
                            {
                                if (UIHelpsToolGlobalConfig.generalSetting != null)
                                {
                                    this.generalSettingtree = (this.generalSettingtree ?? PropertyTree.Create(UIHelpsToolGlobalConfig.generalSetting));
                                    this.generalSettingtree.Draw(false);
                                }
                            }
                            else
                            {
                                //预览部分
                                (mCurSelectItem.Value as GeneralCompTypeConfig).Draw();

                                //底部显示区域
                                SirenixEditorGUI.BeginBox(GUILayout.Width(Screen.width - mMenuWidth), GUILayout.Height(mBottomBarHeight));
                                {
                                    GUILayout.Space(5);
                                    if (selected != null && selected.Value != null)
                                    {
                                        GeneralCompTypeConfig previewConfig = (selected.Value as GeneralCompTypeConfig);
                                        if (previewConfig.curSelectGo != null)
                                        {

                                            string path = AssetDatabase.GetAssetPath(previewConfig.curSelectGo);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                GUI.DrawTexture(new Rect(mMenuWidth + 15, Screen.height - 39, 14, 14), EditorGUIUtility.TrIconContent("d_Prefab Icon").image);
                                                GUI.Label(new Rect(mMenuWidth + 30, Screen.height - 39, Screen.width - mMenuWidth - 120 - 20, 14), path);
                                            }
                                        }
                                        mSizePercent = EditorGUI.Slider(new Rect(Screen.width - 115, Screen.height - 39, 110, 16), mSizePercent, 1, 6);
                                        previewConfig.OnSizePercentChange(mSizePercent);
                                    }
                                }
                                SirenixEditorGUI.EndBox();
                            }

                        }
                        GUILayout.EndVertical();
                    }
                    SirenixEditorGUI.EndBox();
                }
            }
            GUILayout.EndHorizontal();
        }

        //绘制预览效果
        private void DrawGeneralCompPriview()
        {

        }

        [NonSerialized]
        public GTCommonMenuWindow menu = null;
        protected void CreatMenuOption()
        {
            menu = ScriptableObject.CreateInstance<GTCommonMenuWindow>();
            menu.AddItem("新增分类", 0, this.Fun_AddNewPreviewType);
            menu.AddItem("打开文件提交目录", 0, this.Fun_OpenSubDir);
            menu.AddSeparator();
            menu.AddItem("详细说明文档", 0, this.Fun_OptionDoc);
            selectindex = 1;
            menu.DropDown(buttonRect1, position, delegate ()
            {
                selectindex = 0;
                menu = null;
            });
        }

        //操作-添加分类
        private void Fun_AddNewPreviewType()
        {
            CreateNewPreviewTypeWindow.OpenWindow();
        }

        private void Fun_OpenSubDir()
        {
            System.Diagnostics.Process.Start(UIHelpsToolConfigure.UIHelpsDataPath);
        }

        private void Fun_OptionDoc()
        {
            Application.OpenURL("https://www.baidu.com");
        }


        //确定添加分类
        public bool Do_AddNewPreviewType(string typeName)
        {
            foreach (var cfg in previewConfigs)
            {
                if (cfg.typeName == typeName)
                {
                    EditorUtility.DisplayDialog("提示", "类型名称已存在", "确定");
                    return false;
                }
            }
            GeneralCompTypeConfig obj = null;
            obj = GTHelper.CreateAsset<GeneralCompTypeConfig>(UIHelpsToolConfigure.ConfigDataPath + "/");
            obj.typeName = typeName;//typeName.Substring(typeName.LastIndexOf('/') + 1);
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            menuTree.Add(typeName, obj);
            UpdateMenuTree();
            //默认点击到新添加的分类
            int selectIndex = -1;
            var itemList = menuTree.EnumerateTree().ToList();
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i] != null && itemList[i].Value != null
                && (itemList[i].Value.GetType() == typeof(GeneralCompTypeConfig))
                && (itemList[i].Value as GeneralCompTypeConfig).typeName == obj.typeName)
                {
                    selectIndex = i;
                    lastSelectIndex = selectIndex;
                    itemList[i].Select();
                    break;
                }
            }

            return true;
        }

        public bool Do_ModityGeneralTypeName(GeneralCompTypeConfig typeCfg, string newTypeName)
        {
            foreach (var cfg in previewConfigs)
            {
                if (cfg.typeName == newTypeName)
                {
                    EditorUtility.DisplayDialog("提示", "类型名称已存在", "确定");
                    return false;
                }
            }
            typeCfg.typeName = newTypeName;
            EditorUtility.SetDirty(typeCfg);
            UpdateMenuTree();
            return true;
        }

        public bool Do_ModifyPreviewFolderName(OdinMenuItem menuItem, string preName, string newName)
        {
            DeepModifyPreviewFolder(menuItem, preName, newName);
            UpdateMenuTree();
            return true;
        }

        private void DeepModifyPreviewFolder(OdinMenuItem menuItem, string preName, string newName)
        {
            if (menuItem.Value != null && menuItem.Value is GeneralCompTypeConfig)
            {
                var cfg = menuItem.Value as GeneralCompTypeConfig;
                //这里不能直接用indexOf，因为会错误匹配，应该分割为目录名称全字匹配
                // int findIdx = cfg.typeName.IndexOf(preName);
                string[] folderNames = cfg.typeName.Split('/');
                for (var i = 0; i < folderNames.Length; i++)
                {
                    if (folderNames[i] == preName)
                    {
                        folderNames[i] = newName;
                        break;
                    }
                }
                //重新组合名称
                cfg.typeName = "";
                for (var i = 0; i < folderNames.Length; i++)
                {
                    if (i > 0)
                    {
                        cfg.typeName += '/';
                    }
                    cfg.typeName += folderNames[i];
                }
                EditorUtility.SetDirty(cfg);
            }
            if (menuItem.ChildMenuItems.Count > 0)
            {
                foreach (var child in menuItem.ChildMenuItems)
                {
                    DeepModifyPreviewFolder(child, preName, newName);
                }
            }
        }

    }
}

