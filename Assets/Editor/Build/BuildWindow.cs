///--------------------------------------------------------------------
/// 文件名   :   BuildWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/03 15:32:44
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using Debug = UnityEngine.Debug;

public class BuildWindow : OdinMenuEditorWindow
{
    //版本信息
    private VersionGUI versionGUI;

    //本地包
    private LocalPackGUI localPackGUI;

    //远程包
    private RemotePackGUI remotePackGUI;
    
    //版本号
    public static string Version => GameApp.Instance.Version;

    private GUIContent title = new GUIContent();
    public static void OpenWindow()
    {
        BuildWindow window = GetWindow<BuildWindow>(Version);

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }

    public void UpdateWindowName()
    {
        title.text = Version;
        this.titleContent = title;
    }
    public OdinMenuTree tree;
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


        //版本信息
        versionGUI = new VersionGUI();
        versionGUI.UpdateWindowName = UpdateWindowName;
        var versionInfo = new VersionMenuItem(tree, "版本", versionGUI);
        tree.AddMenuItemAtPath("版本信息", versionInfo);

        //本地包
        localPackGUI = new LocalPackGUI();
        var localPackInfo = new LocalPackMenuItem(tree, "本地", localPackGUI);
        tree.AddMenuItemAtPath("本地包", localPackInfo);

        //远程包
        remotePackGUI = new RemotePackGUI();
        remotePackGUI.UpdateWindowName = UpdateWindowName;
        var remotePackInfo = new RemotePackMenuItem(tree, "远程", remotePackGUI);
        tree.AddMenuItemAtPath("远程包", remotePackInfo);
        
        tree.EnumerateTree()
       .AddThumbnailIcons()
       .SortMenuItemsByName();

        return tree;
    }


    /*********************************版本信息************************************/

    [System.Serializable]
    public class VersionMenuItem : OdinMenuItem
    {
        private readonly VersionGUI instance;
        public string ItemName;

        public VersionMenuItem(OdinMenuTree tree, string name, VersionGUI version) : base(tree, name, version)
        {
            this.ItemName = name;
            this.instance = version;
        }



        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.button == 0)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<VersionGUI>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }
        }

        public override string SmartName { get { return ItemName; } }
    }

    [System.Serializable]
    public class VersionGUI
    {
        public System.Action UpdateWindowName;
        [ShowInInspector, EnableGUI, ReadOnly, LabelText("版本号")]
        public string Version => GameApp.Instance.Version;

        [Button("更新大版本号")]
        public void UpdateMainVersion()
        {
            GameApp.Instance.UpdateMainVersion();
            UpdateWindowName?.Invoke();
        }

        [Button("重置版本")]
        public void ResetVersion()
        {
            GameApp.Instance.ResetVersion();
            UpdateWindowName?.Invoke();
        }

    }


    /*******************************本地包**************************************/
    public class LocalPackMenuItem : OdinMenuItem
    {
        private readonly LocalPackGUI instance;
        public string ItemName;

        public LocalPackMenuItem(OdinMenuTree tree, string name, LocalPackGUI version) : base(tree, name, version)
        {
            this.ItemName = name;
            this.instance = version;
        }



        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.button == 0)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<VersionGUI>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }
        }

        public override string SmartName { get { return ItemName; } }
    }

    [System.Serializable]
    public class LocalPackGUI
    {

        [HideLabel]
        [InlineProperty]
        public DeBuggerPackGUI DeBugger;
        
        [Button("本地包")]
        public void LocalPack()
        {
            BuildPack.InnerBuildLocalPack(DeBugger.GetBuildOptions(),true,DeBugger.GetIsBuildRes());
        }
    }

    /********************************远程包***************************************/
    public class RemotePackMenuItem : OdinMenuItem
    {
        private readonly RemotePackGUI instance;
        public string ItemName;

        public RemotePackMenuItem(OdinMenuTree tree, string name, RemotePackGUI version) : base(tree, name, version)
        {
            this.ItemName = name;
            this.instance = version;
        }



        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            if (Event.current.type == EventType.KeyDown && Event.current.button == 0)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<VersionGUI>();

                if (selection.Any())
                {
                    Event.current.Use();
                }
            }
        }

        public override string SmartName { get { return ItemName; } }
    }

    [System.Serializable]
    public class RemotePackGUI
    {
        public System.Action UpdateWindowName;

        [ShowInInspector, EnableGUI, ReadOnly, LabelText("版本号")]
        public string Version => GameApp.Instance.Version;

        [LabelText("是否首包")]
        public bool IsFirst;

        [HideLabel]
        [ShowIf("IsFirstPack")]
        [InlineProperty]
        public DeBuggerPackGUI DeBugger;


        [Title("资源", TitleAlignment = TitleAlignments.Left)]
        [Button("更新补丁")]
        [ShowIf("IsHotFixPack")]
        public void BuildResource()
        {
            BuildPack.BuildResource();
        }
      
#if  ! (PLATFORM_ANDROID || PLATFORM_IOS)
        [Title("代码", TitleAlignment = TitleAlignments.Left)]
        [Button("2、编译代码")]
        [ShowIf("IsHotFixPack")]
        public void BuildCode()
        {
            IsFirst = false;
            BuildPack.BuildCode();
            UpdateWindowName?.Invoke();
            string buildDir = string.Format("{0}/{1}/{2}/{3}/code",
                                      GameApp.Instance.BuildDir,
                                      GameApp.Instance.publish.ToString(),
                                     GameApp.Instance.channel.ToString(),
                                      GameApp.Instance.buildtarget.ToString());

            GameApp.Instance.RunBat("TortoiseProc", $"/command:add /path:{buildDir} -m 修改Version 代码修改 --force /closeonend:4");
            GameApp.Instance.RunBat("TortoiseProc", $"/command:commit /path:{buildDir} -m 修改Version 代码修改 /closeonend:4");
        }
#endif

        
        [Title("打包", TitleAlignment = TitleAlignments.Left)]
        [Button("打包")]
        [ShowIf("IsFirstPack")]
        public void BuildRemotePack()
        {
            BuildPack.InnerBuildRemotePack(DeBugger.GetBuildOptions());
        }

        public bool IsFirstPack()
        {
            return IsFirst;
        }

        public bool IsHotFixPack()
        {
            return !IsFirst;
        }
    }
    
    
    
    /*********************************Debugger****************************************/
    [System.Serializable]
    [InlineProperty(LabelWidth = 205)]
    public class DeBuggerPackGUI
    {
        public bool DevelopmentBuild = false;
        public bool AutoConnectProfiler = false;
        public bool DeepProfiling =false;  
        public bool ScriptDebugging = false;
        public bool isCheckDependences = false;
        public bool isBuildRes = true;
        public BuildOptions GetBuildOptions()
        {
            BuildOptions Options = BuildOptions.None;

            if (DevelopmentBuild)
            {
                Options |= BuildOptions.Development;
            }
            if (AutoConnectProfiler)
            {
                Options |= BuildOptions.ConnectWithProfiler;
            }
            if (DeepProfiling)
            {
                Options |= BuildOptions.EnableDeepProfilingSupport;
            }
            if (ScriptDebugging)
            {
                Options |= BuildOptions.AllowDebugging;
            }
            return Options;
        }
        public bool GetAAFlag()
        {
            return isCheckDependences;
        }
        public bool GetIsBuildRes()
        {
            return isBuildRes;
        }
    }

}
