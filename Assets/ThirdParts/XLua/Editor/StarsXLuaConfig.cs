///--------------------------------------------------------------------
/// 文件名   :   StarsXLuaConfig.cs
/// 内  容   :   XLua 配置文件
/// 说  明   :  1、LuaCallCSharp ;2、CSharpCallLua 3、BlackList  4、Hotfix
/// 创建日期 :   2023/02/01 09:48:48
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using XLua;

public static class StarsXLuaConfig
{
    #region LuaCallCSharp

    /// <summary>
    /// 通过类名配置
    /// </summary>
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp_Class = new List<Type>()
    {
        // ======================== begin 从XLuaCustomExport 复制===================
        //typeof(CameraMMO),
        //typeof(BrokenItem),
        //typeof(MapItemMgr),
        //typeof(MapDataStyle),
        //typeof(MapItemMono),
        //typeof(MapItemPos),
        //typeof(MapDataStyle.ItemChunk),
        //typeof(MapItemPrefabData),
        //typeof(Frame.MList),
        //typeof(Frame.XLuaList),
        //typeof(Frame.ScrollList),
        //typeof(Frame.LoopList),
        //typeof(MItem),
        //typeof(MUIFunction),
        //typeof(DragMe),
        //typeof(FOWLogic),
        //typeof(FOWSystem),
        //typeof(FOWSystem.Setting),
        //typeof(Fire.Utils),
        //typeof(RoleControl),
        //typeof(MResUpdate),
        //typeof(VersionStyle),
        //typeof(VersionManager),
        //typeof(SystemInfoUtil),
        //typeof(GameStartUp),
        //typeof(EventTrigger3D),
        //typeof(ApiTools),
        //typeof(SpriteRenderBox),
        //typeof(Frame.NodeControl),
        //typeof(LuaChunkPool),
        //typeof(LuaItem),
        //typeof(LuaItemPos),
        //typeof(WWWHttpHelper),
        //typeof(WWWHttpData),
        //typeof(ILogger),
        //typeof(GFrameDebuger),
        //typeof(ProfilerTest),
        //typeof(SDK.QGameSDK),
        //typeof(SDK.SDKUser),
      // ======================== end 从XLuaCustomExport 复制===================


        //-------------------UnityEngine--------------------------
        typeof(UnityEngine.Profiling.Profiler),
        typeof(UnityEngine.Object),
        typeof(Vector2Int),
        typeof(Vector3Int),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Matrix4x4),
        typeof(Vector4),
        typeof(Quaternion),
        typeof(Color),
        typeof(Ray),
        typeof(Bounds),
        typeof(Ray2D),
        typeof(Time),
        typeof(GameObject),
        typeof(Component),
        typeof(Behaviour),
        typeof(Transform),
        typeof(RectTransform),
        typeof(TrailRenderer),
        typeof(Rect),
        typeof(Resources),
        typeof(TextAsset),
        typeof(Keyframe),
        typeof(AnimationCurve),
        typeof(AnimationClip),
        typeof(MonoBehaviour),
        typeof(ParticleSystem),
        typeof(SkinnedMeshRenderer),
        typeof(Renderer),
        typeof(WWW),
        /*typeof(RequestHttpWebRequest),*/
        typeof(Mathf),
        typeof(UnityEngine.Debug),
        typeof(UnityEngine.AI.NavMeshHit),
        typeof(UnityEngine.AI.NavMesh),
        typeof(RaycastHit),
        typeof(RectTransformUtility),
        typeof(AudioSource),
        typeof(QualitySettings),
        typeof(Animator),

        //-------------------ThirdParts---------------
        typeof(TMPro.TextMeshPro),

        //--------------------System-------------------
        typeof(System.Object),
        typeof(System.Collections.Generic.List<int>),
        typeof(List<GameObject>),

        //-------------------CustomClass---------------
        /* typeof(UITools),*/
        typeof(KEngineExtensions),
        typeof(SuperScrollViewController),
        /* typeof(ShaderCircle),*/
        //-------------------Delegate/Event------------------
        typeof(UnityAction< UnityEngine.RectTransform,int>),
        typeof(System.Action<int,System.Object[]>),
        typeof(System.Action<int, LuaTable>),
        typeof(System.Action<int, UnityEngine.RectTransform>),
        typeof(System.Action<int, UnityEngine.Vector2>),
        typeof(System.Action<Vector3>),

        typeof(System.Func<int>),
        typeof(UnityAction),
        typeof(UnityAction<int, int>),
        typeof(UnityAction<int, int,int>),
        typeof(UnityAction<int, int,int,int>),
        typeof(UnityAction<float, float,float,float>),
        typeof(UnityAction<float,float,float>),
        typeof(UnityAction<float,float>),
        typeof(UnityAction<float>),
        typeof(UnityAction<ulong>),
        typeof(UnityAction<long,long>),
        typeof(UnityAction<int, System.Object>),
        typeof(System.Action<GameObject>),
        typeof(Action<string>),
        typeof(UnityAction<int, string>),
        typeof(UnityAction<E_EventDefine,string>),
        typeof(UnityAction<AgainLoginType>),

        typeof(SGF.UI.Framework.LuaUIWindow),   //modify by lijun08
        typeof(StarProjectDef.AnnouncementData),
        typeof(StarProjectDef.Announcements),

        typeof(StarProject.Game.Skill.SkillEntity),
        typeof(StarProjectDef.E_SkillExitType),
        typeof(UnityEngine.Events.UnityAction<StarProjectDef.SystemOpenType, bool>),
        typeof(UnityEngine.Events.UnityAction<StarProjectDef.SystemOpenType>),
        typeof(PlayableDirector),

        typeof(UnityEngine.Canvas),
        typeof(UnityEngine.Canvas.WillRenderCanvases),
        typeof(UnityEngine.Random),
        typeof(UnityEngine.Random.State),
        typeof(UnityEngine.Color32),
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Application.AdvertisingIdentifierCallback),
        typeof(UnityEngine.Application.LowMemoryCallback),
        typeof(UnityEngine.Application.LogCallback),
        typeof(UnityEngine.PlayerPrefs),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Animation),
        typeof(UnityEngine.CanvasGroup),
        typeof(UnityEngine.Texture2D),
        typeof(UnityEngine.Texture2D.EXRFlags),
        typeof(TMPro.TextMeshProUGUI),
        typeof(UnityEngine.ParticleSystemRenderer),
        typeof(UnityEngine.TextAnchor),
        //typeof(Frame.Util),
        typeof(StarProject.Service.Function.GlobalFunctionManager),
        typeof(StarProject.Service.Function.GlobalFunctionManager.InterActionConditionDelegate),
        typeof(StarProject.Service.Function.GlobalFunctionManager.ConditionData),
        typeof(StarProject.Service.DisplayProcess.DisplayProcessDispenser),
        typeof(SGF.UI.Framework.LuaUICell),
        typeof(EasyTweenPosition),
        typeof(LocalDropData),
        typeof(StarDebug),
        typeof(StarDebug.LogTagEnum),
        typeof(StarProjectDef.E_SlotModule),
        typeof(StarProject.Module.SlotUpdateActionType),
        typeof(StarProjectDef.CRetMsgEnum),
        typeof(UnityEngine.GUIUtility),
        typeof(SystemConstConfigs),
        typeof(Coffee.UIExtensions.UIParticle),
        typeof(Coffee.UIExtensions.UIParticle.MeshSharing),
        typeof(Coffee.UIExtensions.UIParticle.PositionMode),
        typeof(UnityEngine.SpriteRenderer),
        typeof(StarProject.OffLine.PanelOffLineData),
        typeof(SGF.Module.Framework.OnEvent),
        typeof(TMPro.TMP_Text),
        typeof(UnityEngine.UI.Graphic),
        typeof(UnityEngine.EventSystems.UIBehaviour),
        typeof(ProtoMsg.AuctionType),
        typeof(ProtoMsg.GetTargetTeamInfoReq),
        typeof(SGF.Network.SocketBase),
        typeof(ProtoMsg.BankQueryListReq),
        typeof(ProtoMsg.BankTypeListReq),
        typeof(ProtoMsg.BankItemListReq),
        typeof(ProtoMsg.BankSubTypeListReq),
        typeof(LuaPanel),

        typeof(UnityEngine.Events.UnityEvent<System.Object>),
        typeof(UnityEngine.Events.UnityEvent<System.Int64,System.Int64>),
        typeof(UnityEngine.Events.UnityEvent<System.UInt64, System.UInt64>),
        typeof(UnityEngine.Events.UnityEvent<System.String,System.Boolean>),
        typeof(UnityEngine.Events.UnityEvent<StarProjectDef.AgainLoginType>),
        typeof(UnityEngine.Events.UnityEvent<System.UInt64,System.UInt64,System.Int32,System.Int32>),
        typeof(UnityEngine.Events.UnityEvent<System.Int32,System.Int64,System.String,System.String>),
        typeof(UnityEngine.Events.UnityEvent<System.Boolean>),
        typeof(UnityEngine.Events.UnityEvent<StarProjectDef.SystemOpenType,System.Boolean>),
        typeof(UnityEngine.Events.UnityEvent<System.UInt64>),
        typeof(UnityEngine.Events.UnityEvent<System.Int32,System.Int32>),
        typeof(UnityEngine.Events.UnityEvent<System.Int32,System.Boolean>),
        typeof(UnityEngine.Events.UnityEvent<System.Int32>),

        typeof(SGF.Module.Framework.ModuleEvent<System.Int64,System.Int64>),
        typeof(SGF.Module.Framework.ModuleEvent<System.String,System.Boolean>),
        typeof(SGF.Module.Framework.ModuleEvent<StarProjectDef.AgainLoginType>),
        typeof(SGF.Module.Framework.ModuleEvent<System.UInt64,System.UInt64,System.Int32,System.Int32>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Int32,System.Int64,System.String,System.String>),
        typeof(SGF.Module.Framework.ModuleEvent<StarProjectDef.SystemOpenType,System.Boolean>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Int64>),
        typeof(SGF.Module.Framework.ModuleEvent<System.UInt64>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Boolean>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Int32,System.Int32>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Int32,System.Boolean>),
        typeof(SGF.Module.Framework.ModuleEvent<System.Int32 >),

        typeof(SGF.Module.Framework.ServiceModule<StarProject.Service.Language.LanguageManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.Module.Framework.ModuleManager>),
        typeof(SGF.Module.Framework.ServiceModule<StarProject.Service.Resource.ResourceFormalManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.Network.NetworkManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.Network.FixMessageManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.Network.MsgRetManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.Network.Client2ThirdMsgManager>),
        typeof(SGF.Module.Framework.ServiceModule<RedPointManager>),
        typeof(SGF.Module.Framework.ServiceModule<StarProject.Game.GameManager>),
        typeof(SGF.Module.Framework.ServiceModule<StarProject.Service.Business.BusinessManager>),
        typeof(List<StarProjectDef.AnnouncementData>),
        typeof(SGF.Module.Framework.ServiceModule<LocalCacheManager>),
        typeof(SGF.Module.Framework.ServiceModule<SGF.UI.Framework.UIManager>),

        typeof(List<System.String>),
        typeof(SGF.Unity.MonoSingletonEx<EntityRoot>)

    };

    /// <summary>
    /// 命名空间配置
    /// </summary>
    [LuaCallCSharp]
    public static IEnumerable<Type> LuaCallCSharp_NameSpace
    {
        get
        {
            List<string> namespaces = new List<string>() // 在这里添加名字空间
            {
                //  "UnityEngine",
                "UnityEngine.UI"
            };

            var unityTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                              where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                              from type in assembly.GetExportedTypes()
                              where type.Namespace != null && namespaces.Contains(type.Namespace)
                                  && type.FullName != "UnityEngine.UI.GraphicRebuildTracker" && type.FullName != "UnityEngine.UI.Graphic"
                                  && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                              select type);

            return unityTypes;
        }
    }

    /// <summary>
    /// 通过程序集配置
    /// </summary>
    [LuaCallCSharp]
    public static IEnumerable<Type> LuaCallCSharp_Assemblys
    {
        get
        {
            string[] customAssemblys = new string[] {
                //"DOTween",
               // "DOTweenPro",
            };
            var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                               from type in assembly.GetExportedTypes()
                               where type.Namespace == null || !type.Namespace.StartsWith("XLua")
                                       && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                               select type);

            return customTypes;
        }
    }

    /// <summary>
    /// dotween的擴展方法在lua中調用
    /// </summary>
    [LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> dotween_lua_call_cs_list = new List<Type>()
    {
            typeof(DG.Tweening.AutoPlay),
            typeof(DG.Tweening.AxisConstraint),
            typeof(DG.Tweening.Ease),
            typeof(DG.Tweening.LogBehaviour),
            typeof(DG.Tweening.LoopType),
            typeof(DG.Tweening.PathMode),
            typeof(DG.Tweening.PathType),
            typeof(DG.Tweening.RotateMode),
            typeof(DG.Tweening.ScrambleMode),
            typeof(DG.Tweening.TweenType),
            typeof(DG.Tweening.UpdateType),
            typeof(DG.Tweening.DOTweenModuleUI),
            typeof(DG.Tweening.DOTween),
            typeof(DG.Tweening.DOVirtual),
            typeof(DG.Tweening.EaseFactory),
            typeof(DG.Tweening.Tweener),
            typeof(DG.Tweening.Tween),
            typeof(DG.Tweening.Sequence),
            typeof(DG.Tweening.TweenParams),
            typeof(DG.Tweening.Core.ABSSequentiable),

            typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

            typeof(DG.Tweening.TweenCallback),
            typeof(DG.Tweening.TweenExtensions),
            typeof(DG.Tweening.TweenSettingsExtensions),
            typeof(DG.Tweening.ShortcutExtensions),
    };
    #endregion

    #region CSharpCallLua
    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(Action),
        typeof(Func<double, double, double>),
        typeof(Func<bool>),
        typeof(Action<int>),
        typeof(Action<string>),
        typeof(Action<double>),
        typeof(Action<float>),
        typeof(Action<bool>),
        typeof(Action<bool,bool>),
        typeof(UnityAction<int,string>),
        typeof(UnityAction<E_EventDefine,string>),
        typeof(UnityAction<AgainLoginType>),
        typeof(UnityAction<int>),
        typeof(UnityAction<int,int>),
        typeof(UnityAction<int,bool>),
        typeof(UnityAction<int,int,int>),
        typeof(UnityAction<int,int,int,int>),
        typeof(UnityAction<int,int,string,string>),
        typeof(UnityAction<ulong,ulong,int,int>),
        typeof(UnityAction<long,long>),
        typeof(UnityAction<int,long,string,string>),
        typeof(UnityAction<UInt64,UInt64,int,int>),
        typeof(UnityAction<string, bool>),
        typeof(UnityAction<ulong>),
        typeof(System.Action<int,System.Object[]>),
        typeof(System.Action<int, LuaTable>),
        typeof(UnityEngine.Events.UnityAction< UnityEngine.RectTransform,int>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<Vector2>),
        typeof(System.Collections.IEnumerator),
        typeof(System.Collections.Generic.List<int>),
        typeof(System.Collections.Generic.List<UnityEngine.Component>),
        typeof(int[]),
        typeof(string[]),
        typeof(OnFreshGrid),
        typeof(UnityEngine.Events.UnityAction<StarProjectDef.SystemOpenType, bool>),
        typeof(UnityEngine.Events.UnityAction<StarProjectDef.SystemOpenType>),
        typeof(System.Action<Vector3>)

    };
    #endregion

    #region BlackList

    //第0个是命名空间.类名 1 方法名 /属性名/ 成员变量名   2+ 后面依次是 方法参数类型
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()
    {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
                new List<string>(){"UnityEngine.Light", "SetLightDirty"},

    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){typeof(UnityEngine.UI.Graphic).FullName, "OnRebuildRequested"},
                new List<string>(){typeof(UnityEngine.UI.Text).FullName, "OnRebuildRequested"},
                new List<string>(){typeof(UnityEngine.UI.DefaultControls).FullName, "factory" },
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "PlayOnGamepad", "System.Int32"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "DisableGamepadOutput"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "SetGamepadSpeakerMixLevel", "System.Int32", "System.Int32"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "SetGamepadSpeakerMixLevelDefault", "System.Int32"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "SetGamepadSpeakerRestrictedAudio", "System.Int32","System.Boolean"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "GamepadSpeakerSupportsOutputType", "UnityEngine.GamepadSpeakerOutputType"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "gamepadSpeakerOutputType"},
                new List<string>(){typeof(UnityEngine.AudioSource).FullName, "GamepadSpeakerOutputType","UnityEngine.GamepadSpeakerOutputType"},
                new List<string>(){typeof(UnityEngine.QualitySettings).FullName, "GetAllRenderPipelineAssetsForPlatform", "System.String","System.Collections.Generic.List`1[UnityEngine.Rendering.RenderPipelineAsset]&"},
                new List<string>(){typeof(SGF.UI.Framework.UIPanel).FullName, "AddCanvasGroup"},

                new List<string> {"UnityEngine.Input", "IsJoystickPreconfigured", "System.String"},
                new List<string> {"UnityEngine.Texture", "imageContentsHash"},
                new List<string> {"UnityEngine.UI.Graphic", "OnRebuildRequested"},
                new List<string> {"UnityEngine.UI.Text", "OnRebuildRequested"},
                new List<string> { "UnityEngine.ParticleSystemRenderer", "supportsMeshInstancing"},
    };

#if UNITY_2018_1_OR_NEWER
    [BlackList]
    public static Func<MemberInfo, bool> MethodFilter = (memberInfo) =>
    {
        if (memberInfo.DeclaringType.IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (memberInfo.MemberType == MemberTypes.Constructor)
            {
                ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length > 0)
                {
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfos[0].ParameterType))
                    {
                        return true;
                    }
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Method)
            {
                var methodInfo = memberInfo as MethodInfo;
                if (methodInfo.Name == "TryAdd" || methodInfo.Name == "Remove" && methodInfo.GetParameters().Length == 2)
                {
                    return true;
                }
            }
        }
        return false;
    };
#endif
    #endregion

    #region Hotfix

    // [Hotfix]
    //static IEnumerable<Type> HotfixInject;
    //{
    //    get
    //    {
    //        return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
    //                where type.Namespace == null || !type.Namespace.StartsWith("XLua")
    //                select type);
    //    }
    //}

    //static bool hasGenericParameter(Type type)
    //{
    //    if (type.IsGenericTypeDefinition) return true;
    //    if (type.IsGenericParameter) return true;
    //    if (type.IsByRef || type.IsArray)
    //    {
    //        return hasGenericParameter(type.GetElementType());
    //    }
    //    if (type.IsGenericType)
    //    {
    //        foreach (var typeArg in type.GetGenericArguments())
    //        {
    //            if (hasGenericParameter(typeArg))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //static bool typeHasEditorRef(Type type)
    //{
    //    if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
    //    {
    //        return true;
    //    }
    //    if (type.IsNested)
    //    {
    //        return typeHasEditorRef(type.DeclaringType);
    //    }
    //    if (type.IsByRef || type.IsArray)
    //    {
    //        return typeHasEditorRef(type.GetElementType());
    //    }
    //    if (type.IsGenericType)
    //    {
    //        foreach (var typeArg in type.GetGenericArguments())
    //        {
    //            if (typeArg.IsGenericParameter)
    //            {
    //                //skip unsigned type parameter
    //                continue;
    //            }
    //            if (typeHasEditorRef(typeArg))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //static bool delegateHasEditorRef(Type delegateType)
    //{
    //    if (typeHasEditorRef(delegateType)) return true;
    //    var method = delegateType.GetMethod("Invoke");
    //    if (method == null)
    //    {
    //        return false;
    //    }
    //    if (typeHasEditorRef(method.ReturnType)) return true;
    //    return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
    //}

    //// 配置某Assembly下所有涉及到的delegate到CSharpCallLua下，Hotfix下拿不准那些delegate需要适配到lua function可以这么配置
    //[CSharpCallLua]
    //static IEnumerable<Type> AllDelegate
    //{
    //    get
    //    {
    //        BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    //        List<Type> allTypes = new List<Type>();
    //        var allAssemblys = new Assembly[]
    //        {
    //            Assembly.Load("Assembly-CSharp")
    //        };
    //        foreach (var t in (from assembly in allAssemblys from type in assembly.GetTypes() select type))
    //        {
    //            var p = t;
    //            while (p != null)
    //            {
    //                allTypes.Add(p);
    //                p = p.BaseType;
    //            }
    //        }
    //        allTypes = allTypes.Distinct().ToList();
    //        var allMethods = from type in allTypes
    //                         from method in type.GetMethods(flag)
    //                         select method;
    //        var returnTypes = from method in allMethods
    //                          select method.ReturnType;
    //        var paramTypes = allMethods.SelectMany(m => m.GetParameters()).Select(pinfo => pinfo.ParameterType.IsByRef ? pinfo.ParameterType.GetElementType() : pinfo.ParameterType);
    //        var fieldTypes = from type in allTypes
    //                         from field in type.GetFields(flag)
    //                         select field.FieldType;
    //        return (returnTypes.Concat(paramTypes).Concat(fieldTypes)).Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t)).Distinct();
    //    }
    //}
    #endregion
}
