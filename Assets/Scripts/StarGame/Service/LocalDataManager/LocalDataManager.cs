using MessagePack;
using ProtoMsg;
using SGF.Module.Framework;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static RenderHeads.Media.AVProVideo.MediaPlayer;

//记录：修改为字典方便【Lua，Go,】外面包一层DicsName
//那就要注意 ：导表工具是1开始的 ：Lua没问题，CS小心点是1开始的：【接收器Key，【是int，String都可以自己改】
//当然了数据，正常可以for循环，但是策划数据可能断层，所以还是，枚举器，迭代器，foreach了
//
/// <summary>
/// Done            我还需要1解开 数量判断数量不够自动加载。
/// TODO            unity菜单处理生成代码。
/// TODO            优化，拆表工具关卡替换1，拆开数据可以重新替换，2，应用不同关卡数据替换，3，关卡变更自动替换（如任务）。
/// Done            3“json”自动添加。
/// Done            5服务器导出json我们生成cs
/// Done            枚举解耦：差不多   ；不去掉转义符依然可以解码，和utf-8
/// TODO Cs手动修改完毕，Json要校验，
/// </summary>
namespace StarProject.Service.LocalData
{
    /// <summary>
    /// AOI中 某一个具体的属性类型
    /// </summary>
    public class VitalSignAOIAttrs
    {
        public int index { set; get; } //查找关系的
        public string type { set; get; } //反射具体类型的
        public string save { set; get; } //服务器自己用的
        public string desc { set; get; } //描述界面的

        //true是要更新界面，然後就關閉
        //public bool needUate { set; get; }//客户端自己用更新的
    }

    /// <summary>
    /// AOI中 某一个具体的客户端保存的属性类型
    /// </summary>
    public class VitalSignAOIClientAttrs
    {
        public string Name { set; get; }
        public int index { set; get; }
        public string type { set; get; }
        public string save { set; get; }
        public string desc { set; get; }
    }

    public class MapCfgData
    {
        public int ID;
        public string MapName;
        public int MapBaseID;
        public string LevelCollider;
        public int ReviveTid;
        public string SubType;
        public bool AutoBattle;
        public SpaceType MapType;
        public bool IsShowEffect;
        public string EnterTip;
        public int OpenRoleLv;
        public int OpenComTask;
        public bool IsHidePartner;
        public bool IsCameraRotate;
        public bool IsCameraTeleport;

        public MapCfgData(int id, string mapName, int mapBaseID, SpaceType mapType, string subType, string levelCollider, int reviveTid, bool autoBattle, bool isHidePartner, bool isCameraRotate, bool isCameraTeleport = false, bool isShowEffect = false, string enterTip = "", int openRoleLv = 1, int openComTask = 0)
        {
            ID = id;
            MapName = mapName;
            MapBaseID = mapBaseID;
            LevelCollider = levelCollider;
            ReviveTid = reviveTid;
            SubType = subType;
            AutoBattle = autoBattle;
            MapType = mapType;
            IsShowEffect = isShowEffect;
            EnterTip = enterTip;
            OpenRoleLv = openRoleLv;
            OpenComTask = openComTask;
            IsHidePartner = isHidePartner;
            IsCameraRotate = isCameraRotate;
            IsCameraTeleport = isCameraTeleport;
        }
    }


    [XLua.LuaCallCSharp]
    /// <summary>
    /// 注册和管理移动节点
    /// </summary>
    public class LocalDataManager : ServiceModule<LocalDataManager>
    {
        private const string LOG_TAG = "LocalDataManager";

        public string _folder = "Config/ExcelBytes/";

        public string _aoiFolder = "Config/AOIJson/";

        //这里需要一个Loading
        public void Init()
        {
            CheckSingleton();

            InitMessagePackResolver();
            //// 提前加载AOI属性配置表
            //InitAoiAttrPropertiesNMap();
        }

        /// <summary>
        /// register messagepack default resolver
        /// </summary>
        private void InitMessagePackResolver()
        {
            MessagePack.Resolvers.StaticCompositeResolver.Instance.Register(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance
             );

            var option = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
            //var option = MessagePackSerializerOptions.Standard.WithResolver(
            //    MessagePack.Resolvers.CompositeResolver.Create(
            //        new IMessagePackFormatter[] { new StringInterningFormatter() },
            //        new IFormatterResolver[] { MessagePack.Resolvers.StaticCompositeResolver.Instance })
            //    );

            MessagePackSerializer.DefaultOptions = option;

            //自定义resolver
            //PolymorphicRegister.Register();
            //PolymorphicResolver.Instance.Init();
        }

        //        private string appPath = string.Empty;
        //        public string AppPath
        //        {

        //            get
        //            {
        //#if UNITY_EDITOR || UNITY_STANDALONE
        //                appPath = Application.dataPath + "/StreamingAssets/";

        //#elif UNITY_IPHONE
        //            appPath = Application.dataPath +"/Raw/";

        //#elif UNITY_ANDROID
        //           //   appPath = "jar:file://" + Application.dataPath + "!/assets/";   
        //           //   appPath = Application.streamingAssetsPath + "/";
        //           //   appPath = "file://" + Application.streamingAssetsPath + "/";

        //                  appPath =  Application.streamingAssetsPath + "/";
        //                //appPath =  "file:///data/app/com.xGame.Caryon_Shinchan/base.apk!/assets/";


        //#endif
        //                return appPath;
        //            }

        //        }


        //        public delegate void LoadJsonFinish<T>(T data);


        //        private void LoadingJson<T>(string jsonFullName,LoadJsonFinish<T> finish)
        //        {

        //#if UNITY_EDITOR
        //            string filepath = Application.persistentDataPath + "/Json/" + jsonFullName;
        //#elif UNITY_IPHONE
        //    string filepath = Application.dataPath + "/Raw/Json/";
        //#elif UNITY_ANDROID
        //    string filepath = "jar:file://" + Application.dataPath + "!/assets/Json/" + jsonFullName;
        //#endif
        //            //new Task(Loading.LoadJson<List<MusicDataItem>>(filepath, LoadJsonFinish));
        //            MonoHelper.StartCoroutine(LoadJson<T>(filepath,finish));
        //        }

        //        public static IEnumerator LoadJson<T>(string path, LoadJsonFinish<T> finish)
        //        {
        //            WWW www = new WWW(path);
        //            yield return www;
        //            while (www.isDone == false)
        //            {
        //                yield return new WaitForEndOfFrame();
        //            }
        //            string skipBom = Encoding.UTF8.GetString(www.bytes, 3, www.bytes.Length - 3);
        //            T t = JsonConvert.DeserializeObject<T>(skipBom);
        //            finish(t);
        //            yield return www;

        //        }


        //        1.android 平台下 StreamingAssets 文件夹是只读的 没有写入权限

        //2. 

        //android平台StreamingAssets 文件路径 注意前面"jar:file://"

        //      #elif UNITY_ANDROID

        //      path= "jar:file://" + Application.dataPath + "!/assets/;

        //    #else

        //3.这个路径不是绝对路径所以不能用System.IO.File/Directory 来操作 ，比如File.Exist 会一直返回False 即使文件存在，同理 File.Read也不能使用

        //4.需要使用 WWW（Unity弃用）或者UnityWebRequest（Unity推荐） 来访问这个文件夹下的资源

        //5.WWW 返回 404 Not Found 问题 ，首先查看文件是否存在，再看路径前是否加了 “file://” ，然后要看后缀名，一定要区分大小写，区分大小写,因为这个坑浪费了    我两个小时，貌似只有在Android 平台需要区分后缀名大小写，其他平台不用。

        //6.建议使用Application.persistentDataPath 这个目录 ，这个目录是可读可写的，可以使用File/Directory来操作。

        /// <summary>
        /// 删除路径下所有文件和文件夹
        /// </summary>
        /// <param name="path"></param>
        void DeleteFell(string path)
        {
            DirectoryInfo dir = new(path);
            FileSystemInfo[] fileSystems = dir.GetFileSystemInfos();
            foreach (var item in fileSystems)
            {
                if (item is DirectoryInfo) //判断是否是文件夹
                {
                    DirectoryInfo directory = new(item.FullName);
                    directory.Delete(true);
                }
                else
                {
                    File.Delete(item.FullName); //删除这个文件
                }
            }
        }

        ////创建文件
        //var fs = File.Create(path + "/" + downloadVideoName + ".mp4"); //path为你想保存文件的路径。downloadVideoName 为文件名
        //        fs.Write(www.bytes, 0, www.bytes.Length);
        //                fs.Close();

        //[Obsolete]//标记该方法已弃用
        /// <summary>
        /// JsonLit：不能解析字典  目前的用法是 这个方法只解析AOI的json配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonName"></param>
        /// <returns></returns>
        //private T LoadNMapDatasLit<T>(string jsonName) where T : class
        //{
        //    string path = _aoiFolder + jsonName;
        //    // T t = LitJson.JsonMapper.ToObject<T>(jsonData);
        //    return LoadCfgJsonSync<T>(path);
        //}

        private T LoadNMapDatasLitAsync<T>(string jsonName) where T : class
        {
            string path = _aoiFolder + jsonName;
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (jsonAsset != null)
            {

                var aoiJson = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonAsset.text);
                StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(path);
                return aoiJson;
            }
            return null;
        }

        /// <summary>
        /// NewTonJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonName"></param>
        /// <returns></returns>
        private void LoadNMapDatasNewtonAsync1<T>(string jsonName, ref T t) where T : class
        {
            //string jsonFullName = jsonName + ".json";
            //LoadingJson<T>(jsonName + ".json", (T resultClass) =>
            //{
            //    t = resultClass;

            //});

            //t = GetJsonData<T>(jsonName);

            // string jsonData;
            // string jsonData = GetJsonData(jsonName);

            // t = JsonConvert.DeserializeObject<T>(jsonData);
            //JsonReader js = new JsonReader(jsonData); 
            //return JsonMapper.ToObject<T>(js);


            //GameStageManager sgm = GameStageManager.Instance;
            //int _stageId = sgm.GetCurrentFBStage();
            //int _levelId = sgm.GetCurrentFBLevel();
            //M_GameSceneItemData.DealCurrentUsefulData(_stageId, _levelId);
        }

        //白名单，任务也直接写里面

        private void LoadNMapDatasNewtonAsync<T>(string jsonName, ref T t) where T : class
        {
            string path = _folder + jsonName;
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (jsonAsset != null)
            {
                t = MessagePack.MessagePackSerializer.Deserialize<T>(jsonAsset.bytes);
                StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(path);
            }

#if UNITY_EDITOR
            // 当Unity编辑器处于非运行模式时，编辑器读取多语言支持
            if (!PlayModeChangeDetector.IsPlaying)
            {
                path = PlayModeChangeDetector.DataPath + $"\\Res\\Config\\ExcelBytes\\{jsonName}.bytes";
                var bytes = File.ReadAllBytes(path);
                if (bytes != null)
                {
                    t = MessagePack.MessagePackSerializer.Deserialize<T>(bytes);
                }
            }
#endif
        }

        [Obsolete]//标记该方法已弃用
        /// <summary>
        /// 加载分为
        /// 1，主动式，全局准备，分摊时间（目前这个策略优先级特别低）
        /// 2，被动式，需要则加载，||可分情况释放 1，分表分数据 2，不用时候卸载 3，切换场景时---stageMgr通知---切换替换数据
        /// </summary>
        /// <param name="jsonName"></param>
        /// <returns></returns>
        //public T GetJsonData<T>(string jsonName) where T : class
        //{
        //    string path = _folder + jsonName;

        //    return LoadCfgJsonSync<T>(path); ;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileLocation">文件迁移。、,读取的关键选项</param>
        /// <returns></returns>
        //public string GetData(string filePath, FileLocation fileLocation = FileLocation.RelativeToPersistentDataFolder)
        //{
        //    string result = "";

        //    if (!string.IsNullOrEmpty(filePath))
        //    {
        //        var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadAssetSync<TextAsset>(filePath);
        //        if (jsonAsset != null)
        //        {
        //            result = jsonAsset.text;
        //        }
        //        else
        //        {
        //            SGF.Debuger.LogError($"读取json 文件失败 {filePath}"); //, this);
        //        }
        //    }
        //    else
        //    {
        //        SGF.Debuger.LogError("[File] No subtitle file path specified"); //, this);
        //    }

        //    return result;
        //}


        #region Platform and Path

        public static Platform GetPlatform()
        {
            Platform result = Platform.Unknown;

            // Setup for running in the editor (Either OSX, Windows or Linux)
#if UNITY_EDITOR
#if UNITY_EDITOR_OSX && UNITY_EDITOR_64
			result = Platform.MacOSX;
#elif UNITY_EDITOR_WIN
            result = Platform.Windows;
#endif
#else
            // Setup for running builds
#if UNITY_STANDALONE_WIN
			result = Platform.Windows;
#elif UNITY_STANDALONE_OSX
			result = Platform.MacOSX;
#elif UNITY_IPHONE || UNITY_IOS
			result = Platform.iOS;
#elif UNITY_TVOS
			result = Platform.tvOS;
#elif UNITY_ANDROID
			result = Platform.Android;
#elif UNITY_WP8 || UNITY_WP81 || UNITY_WINRT_8_1
			result = Platform.WindowsPhone;
#elif UNITY_WSA_10_0
			result = Platform.WindowsUWP;
#elif UNITY_WEBGL
			result = Platform.WebGL;
#elif UNITY_PS4
			result = Platform.PS4;
#endif

#endif
            return result;
        }

        #endregion


        private string GetPlatformFilePath(Platform platform, ref string filePath, ref FileLocation fileLocation)
        {
            string result = string.Empty;

            // Replace file path and location if overriden by platform options
            if (platform != Platform.Unknown)
            {
                PlatformOptions options = GetCurrentPlatformOptions();
                if (options != null)
                {
                    if (options.overridePath)
                    {
                        filePath = options.path;
                        fileLocation = options.pathLocation;
                    }
                }
            }

            result = GetFilePath(filePath, fileLocation);

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
            // Handle very long file paths by converting to DOS 8.3 format
            if (result.Length > 200 && !result.Contains("://"))
            {
                const string pathToken = @"\\?\";
                result = pathToken + result.Replace("/", "\\");
                int length = GetShortPathName(result, null, 0);
                if (length > 0)
                {
                    System.Text.StringBuilder sb = new(length);
                    if (0 != GetShortPathName(result, sb, length))
                    {
                        result = sb.ToString().Replace(pathToken, "");
                        SGF.Debuger.LogWarning("[AVProVideo] Long path detected. Changing to DOS 8.3 format");
                    }
                }
            }
#endif

            return result;
        }

        public static string GetFilePath(string path, FileLocation location)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(path))
            {
                switch (location)
                {
                    case FileLocation.AbsolutePathOrURL:
                        result = path;
                        break;
                    case FileLocation.RelativeToDataFolder:
                    case FileLocation.RelativeToPersistentDataFolder:
                    case FileLocation.RelativeToProjectFolder:
                    case FileLocation.RelativeToStreamingAssetsFolder:
                        result = System.IO.Path.Combine(GetPath(location), path);
                        break;
                }
            }

            return result;
        }

        public PlatformOptions GetCurrentPlatformOptions()
        {
            PlatformOptions result = null;

#if UNITY_EDITOR
#if UNITY_EDITOR_OSX && UNITY_EDITOR_64
			result = _optionsMacOSX;
#elif UNITY_EDITOR_WIN
            result = _optionsWindows;
#endif
#else
            // Setup for running builds

#if UNITY_STANDALONE_WIN
			result = _optionsWindows;
#elif UNITY_STANDALONE_OSX
			result = _optionsMacOSX;
#elif UNITY_IPHONE || UNITY_IOS
			result = _optionsIOS;
#elif UNITY_TVOS
			result = _optionsTVOS;
#elif UNITY_ANDROID
			result = _optionsAndroid;
#elif UNITY_WP8 || UNITY_WP81 || UNITY_WINRT_8_1
			result = _optionsWindowsPhone;
#elif UNITY_WSA_10_0
			result = _optionsWindowsUWP;
#elif UNITY_WEBGL
			result = _optionsWebGL;
#elif UNITY_PS4
			result = _optionsPS4;
#endif

#endif
            return result;
        }

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
        [System.Runtime.InteropServices.DllImport("kernel32.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint = "GetShortPathNameW",
            SetLastError = true)]
        private static extern int GetShortPathName(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pathName,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            System.Text.StringBuilder shortName,
            int cbShortName);
#endif

        // TODO: move these to a Setup object
        [SerializeField] OptionsWindows _optionsWindows = new();
        [SerializeField] OptionsMacOSX _optionsMacOSX = new();
        [SerializeField] OptionsIOS _optionsIOS = new();
        [SerializeField] OptionsTVOS _optionsTVOS = new();
        [SerializeField] OptionsAndroid _optionsAndroid = new();
        [SerializeField] OptionsWindowsPhone _optionsWindowsPhone = new();
        [SerializeField] OptionsWindowsUWP _optionsWindowsUWP = new();
        [SerializeField] OptionsWebGL _optionsWebGL = new();
        [SerializeField] OptionsPS4 _optionsPS4 = new();


        #region 【AOI属性同步】

        /// <summary>
        /// [需分离数据，AttrData是相当大量的，配置必须独立]，服务器id，对应配置  
        /// </summary>
        private DictionaryEx<ushort, VitalSignAOIClientAttrs> m_playerIndexAttrMaps =
            new();

        #region 【玩家基础属性信息】

        private PlayerInfo m_PlayerInfoData;

        public PlayerInfo M_PlayerInfoData
        {
            get
            {
                if (m_PlayerInfoData == null)
                {
                    m_PlayerInfoData = LoadNMapDatasLitAsync<PlayerInfo>("Player");
                }

                //缓存机制
                return m_PlayerInfoData;
            }
            set { m_PlayerInfoData = value; }
        }

        #endregion

        #region 【道具基础属性信息】

        private ItemInfo m_ItemInfoData;

        public ItemInfo M_ItemInfoData
        {
            get
            {
                if (m_ItemInfoData == null)
                {
                    m_ItemInfoData = LoadNMapDatasLitAsync<ItemInfo>("Item");
                }

                //缓存机制
                return m_ItemInfoData;
            }
            set { m_ItemInfoData = value; }
        }

        #endregion

        #region 【玩家战斗属性信息】

        private BattleInfo m_BattleInfoData;

        public BattleInfo M_BattleInfoData
        {
            get
            {
                if (m_BattleInfoData == null)
                {
                    m_BattleInfoData = LoadNMapDatasLitAsync<BattleInfo>("Battle");
                }

                //缓存机制
                return m_BattleInfoData;
            }
            set { m_BattleInfoData = value; }
        }

        #endregion

        #region 【玩家实体属性信息，不知道怎么翻译了0.0】

        private TinyEntityInfo m_TinyEntityInfoData;

        public TinyEntityInfo M_TinyEntityInfoData
        {
            get
            {
                if (m_TinyEntityInfoData == null)
                {
                    m_TinyEntityInfoData = LoadNMapDatasLitAsync<TinyEntityInfo>("TinyEntity");
                }

                //缓存机制
                return m_TinyEntityInfoData;
            }
            set { m_TinyEntityInfoData = value; }
        }

        #endregion

        #region 【玩家状态属性信息】

        private GameStateInfo m_GameStateInfoData;

        public GameStateInfo M_GameStateInfoData
        {
            get
            {
                if (m_GameStateInfoData == null)
                {
                    m_GameStateInfoData = LoadNMapDatasLitAsync<GameStateInfo>("GameState");
                }

                //缓存机制
                return m_GameStateInfoData;
            }
            set { m_GameStateInfoData = value; }
        }

        #endregion

        private void InitAoiAttrPropertiesNMap()
        {
            m_playerIndexAttrMaps.Clear();
            InitPlayerAttrPropertiesNMap();
            InitItemAttrPropertiesNMap();
            InitBattleAttrPropertiesNMap();
            InitTinyEntityAttrPropertiesNMap();
            InitGameStateAttrPropertiesNMap();
        }

        /// <summary>
        /// Server通過屬性id，屬性類型，屬性值
        /// 【中間】Json提供，名字，*索引，類型
        /// 找到屬性Name
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VitalSignAOIClientAttrs GetAttrPropByAttrIdx(ushort index)
        {
            if (m_playerIndexAttrMaps.Count == 0)
            {
                InitAoiAttrPropertiesNMap();
            }

            if (m_playerIndexAttrMaps.ContainsKey(index))
            {
                return m_playerIndexAttrMaps[index];
            }

            return null;
        }

        public ushort GetAttrPropIdxByName(string name)
        {
            if (m_playerIndexAttrMaps.Count == 0)
            {
                InitAoiAttrPropertiesNMap();
            }

            foreach (var item in m_playerIndexAttrMaps)
            {
                if (item.Value.Name == name)
                {
                    return item.Key;
                }
            }

            return 0;
        }

        public VitalSignAOIClientAttrs GetAttrPropByName(string name)
        {
            if (m_playerIndexAttrMaps.Count == 0)
            {
                InitAoiAttrPropertiesNMap();
            }

            foreach (var item in m_playerIndexAttrMaps)
            {
                if (item.Value.Name == name)
                {
                    return item.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// 初始化PlayerMaps
        /// </summary>
        private void InitPlayerAttrPropertiesNMap()
        {
            //LoadConfig1
            Type t = M_PlayerInfoData.props.GetType();
            //InitMaps2
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                VitalSignAOIAttrs oneAttrs = (VitalSignAOIAttrs)item.GetValue(M_PlayerInfoData.props);
                if (oneAttrs != null)
                {
                    ushort num = (ushort)((ushort)oneAttrs.index + M_PlayerInfoData.startIndex);
                    VitalSignAOIClientAttrs data = new();
                    data.Name = name;
                    data.index = num;
                    data.save = oneAttrs.save;
                    data.desc = oneAttrs.desc;
                    data.type = oneAttrs.type;
                    m_playerIndexAttrMaps.Add(num, data);
                }
                else
                {
                    SGF.Debuger.LogWarning($"InitPlayerAttrPropertiesNMap() name={name},props=null!!!");
                }
            }
        }

        /// <summary>
        /// 初始化ItemMaps
        /// </summary>
        private void InitItemAttrPropertiesNMap()
        {
            //LoadConfig1
            Type t = M_ItemInfoData.props.GetType();
            //InitMaps2
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                VitalSignAOIAttrs oneAttrs = (VitalSignAOIAttrs)item.GetValue(M_ItemInfoData.props);
                if (oneAttrs != null)
                {
                    ushort num = (ushort)((ushort)oneAttrs.index + M_ItemInfoData.startIndex);
                    VitalSignAOIClientAttrs data = new();
                    data.Name = name;
                    data.index = num;
                    data.save = oneAttrs.save;
                    data.desc = oneAttrs.desc;
                    data.type = oneAttrs.type;
                    m_playerIndexAttrMaps.Add(num, data);
                }
                else
                {
                    SGF.Debuger.LogWarning($"InitItemAttrPropertiesNMap() name={name},props=null!!!");
                }
            }
        }

        /// <summary>
        /// 初始化battleMaps
        /// </summary>
        private void InitBattleAttrPropertiesNMap()
        {
            //LoadConfig1
            Type t = M_BattleInfoData.props.GetType();
            //InitMaps2
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                VitalSignAOIAttrs oneAttrs = (VitalSignAOIAttrs)item.GetValue(M_BattleInfoData.props);
                if (oneAttrs != null)
                {
                    ushort num = (ushort)((ushort)oneAttrs.index + M_BattleInfoData.startIndex);
                    VitalSignAOIClientAttrs data = new();
                    data.Name = name;
                    data.index = num;
                    data.save = oneAttrs.save;
                    data.desc = oneAttrs.desc;
                    data.type = oneAttrs.type;
                    m_playerIndexAttrMaps.Add(num, data);
                }
                else
                {
                    SGF.Debuger.LogWarning($"InitBattleAttrPropertiesNMap() name={name},props=null!!!");
                }
            }
        }

        /// <summary>
        /// 初始化TinyEntityMaps
        /// </summary>
        private void InitTinyEntityAttrPropertiesNMap()
        {
            //LoadConfig1
            Type t = M_TinyEntityInfoData.props.GetType();
            //InitMaps2
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                VitalSignAOIAttrs oneAttrs = (VitalSignAOIAttrs)item.GetValue(M_TinyEntityInfoData.props);
                if (oneAttrs != null)
                {
                    ushort num = (ushort)((ushort)oneAttrs.index + M_TinyEntityInfoData.startIndex);
                    VitalSignAOIClientAttrs data = new();
                    data.Name = name;
                    data.index = num;
                    data.save = oneAttrs.save;
                    data.desc = oneAttrs.desc;
                    data.type = oneAttrs.type;
                    m_playerIndexAttrMaps.Add(num, data);
                }
                else
                {
                    SGF.Debuger.LogWarning($"InitTinyEntityAttrPropertiesNMap() name={name},props=null!!!");
                }

            }
        }

        /// <summary>
        /// 初始化GameStatemaps
        /// </summary>
        private void InitGameStateAttrPropertiesNMap()
        {
            //LoadConfig1
            Type t = M_GameStateInfoData.props.GetType();
            //InitMaps2
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                VitalSignAOIAttrs oneAttrs = (VitalSignAOIAttrs)item.GetValue(M_GameStateInfoData.props);
                ushort num = (ushort)((ushort)oneAttrs.index + M_GameStateInfoData.startIndex);
                VitalSignAOIClientAttrs data = new();
                data.Name = name;
                data.index = num;
                data.save = oneAttrs.save;
                data.desc = oneAttrs.desc;
                data.type = oneAttrs.type;
                m_playerIndexAttrMaps.Add(num, data);
            }
        }

        #endregion


        #region 属性添加区块---映射由工具生成---Json放StreamAsset中---WrapData放Cs脚本

        #region 模型表

        private ModelData m_ModelData;

        public ModelData M_ModelData
        {
            get
            {
                if (m_ModelData == null || m_ModelData.StaticModelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ModelData>("Model", ref m_ModelData);
                }

                //缓存机制
                return m_ModelData;
            }
        }

        public ModelDataCell GetModelDataCell(int modelID)
        {
            ModelDataCell cfg;
            if (!M_ModelData.StaticModelDatas.TryGetValue(modelID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 化身表

        private AvatarData m_AvatarData;

        public AvatarData M_AvatarData
        {
            get
            {
                if (m_AvatarData == null || m_AvatarData.StaticAvatarDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AvatarData>("Avatar", ref m_AvatarData);
                }

                //缓存机制
                return m_AvatarData;
            }
        }

        public AvatarDataCell GetAvatarDataCell(int id)
        {
            AvatarDataCell cfg;
            if (!M_AvatarData.StaticAvatarDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 头像表

        private ModelHeadData m_ModelHeadData;

        public ModelHeadData M_ModelHeadData
        {
            get
            {
                if (m_ModelHeadData == null || m_ModelHeadData.StaticModelHeadDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ModelHeadData>("ModelHead", ref m_ModelHeadData);
                }

                //缓存机制
                return m_ModelHeadData;
            }
        }

        public ModelHeadDataCell GetModelHeadDataCell(int modelID)
        {
            ModelHeadDataCell cfg;
            if (!M_ModelHeadData.StaticModelHeadDatas.TryGetValue(modelID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 交互物件配置

        private InteractData m_InteractData;

        public InteractData M_InteractData
        {
            get
            {
                if (m_InteractData == null || m_InteractData.StaticInteractDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<InteractData>("Interact", ref m_InteractData);

                }

                //缓存机制
                return m_InteractData;
            }
        }

        public InteractDataCell GetInteractDataCell(long interactId)
        {
            if (M_InteractData == null)
            {
                return null;
            }

            InteractDataCell interactDataCell;
            if (M_InteractData.StaticInteractDatas.TryGetValue(interactId, out interactDataCell))
            {
                return interactDataCell;
            }

            return null;
        }
        private LifeSkillMineData m_LifeSkillMineData;

        public LifeSkillMineData M_LifeSkillMineData
        {
            get
            {
                if (m_LifeSkillMineData == null || m_LifeSkillMineData.StaticLifeSkillMineDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LifeSkillMineData>("LifeSkillMine", ref m_LifeSkillMineData);
                }

                //缓存机制
                return m_LifeSkillMineData;
            }
        }

        public LifeSkillMineDataCell GetLifeSkillMineDataCell(int interactId)
        {
            if (M_LifeSkillMineData == null)
            {
                return null;
            }
            LifeSkillMineDataCell interactDataCell;
            if (M_LifeSkillMineData.StaticLifeSkillMineDatas.TryGetValue(interactId, out interactDataCell))
            {
                return interactDataCell;
            }

            return null;
        }

        private LifeSkillCreateData m_LifeSkillCreateData;

        public LifeSkillCreateData M_LifeSkillCreateData
        {
            get
            {
                if (m_LifeSkillCreateData == null || m_LifeSkillCreateData.StaticLifeSkillCreateDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LifeSkillCreateData>("LifeSkillCreate", ref m_LifeSkillCreateData);
                }

                //缓存机制
                return m_LifeSkillCreateData;
            }
        }

        public LifeSkillCreateDataCell GetLifeSkillCreateDataCell(int interactId)
        {
            if (M_LifeSkillCreateData == null)
            {
                return null;
            }
            LifeSkillCreateDataCell interactDataCell;
            if (M_LifeSkillCreateData.StaticLifeSkillCreateDatas.TryGetValue(interactId, out interactDataCell))
            {
                return interactDataCell;
            }

            return null;
        }


        private LifeSkillJobData m_LifeSkillJobData;

        public LifeSkillJobData M_LifeSkillJobData
        {
            get
            {
                if (m_LifeSkillJobData == null || m_LifeSkillJobData.StaticLifeSkillJobDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LifeSkillJobData>("LifeSkillJob", ref m_LifeSkillJobData);
                }

                //缓存机制
                return m_LifeSkillJobData;
            }
        }

        public LifeSkillJobDataCell GetLifeSkillJobDataCell(int jobID, int level)
        {
            if (M_LifeSkillJobData != null && M_LifeSkillJobData.StaticLifeSkillJobDatas != null)
            {
                foreach (var item in M_LifeSkillJobData.StaticLifeSkillJobDatas)
                {
                    if (item.Value.GetSkillID() == jobID && item.Value.GetLevel() == level)
                    {
                        return item.Value;
                    }
                }
            }


            return null;
        }

        /// <summary>
        /// 通用效果，事件，服务
        /// </summary>
        private CMeffectData m_CommonEffectData;

        public CMeffectData M_CommonEffectData
        {
            get
            {
                if (m_CommonEffectData == null || m_CommonEffectData.StaticCMeffectDatas.Count == 0)
                {
                    /*m_InteractEffectData = */
                    LoadNMapDatasNewtonAsync<CMeffectData>("CMeffect", ref m_CommonEffectData);
                }

                //缓存机制
                return m_CommonEffectData;
            }
        }

        public CMeffectDataCell GetCMeffectDataCell(int Id)
        {
            CMeffectDataCell cMeffectDataCell;
            if (M_CommonEffectData.StaticCMeffectDatas.TryGetValue(Id, out cMeffectDataCell))
            {
                return cMeffectDataCell;
            }

            return null;
        }

        /// <summary>
        /// 通用条件
        /// </summary>
        private CMconditionData m_CommonConditionData;

        public CMconditionData M_CommonConditionData
        {
            get
            {
                if (m_CommonConditionData == null || m_CommonConditionData.StaticCMconditionDatas.Count == 0)
                {
                    /*m_InteractConditionData = */
                    LoadNMapDatasNewtonAsync<CMconditionData>("CMcondition", ref m_CommonConditionData);
                }

                //缓存机制
                return m_CommonConditionData;
            }
        }

        public CMconditionDataCell GetCMconditionDataCell(int Id)
        {
            CMconditionDataCell cMconditionDataCell;
            if (M_CommonConditionData.StaticCMconditionDatas.TryGetValue(Id, out cMconditionDataCell))
            {
                return cMconditionDataCell;
            }

            return null;
        }

        private Dictionary<int, List<CMconditionDataCell>> groupConditions = new();
        public List<CMconditionDataCell> GetGroupConditions(int groupId)
        {
            // 初始化 一份数据
            if (groupConditions.Count == 0)
            {
                M_CommonConditionData.StaticCMconditionDatas.ForEach((item) =>
                {
                    var cGroupID = item.Value.GetGroupID();
                    if (!groupConditions.ContainsKey(cGroupID))
                    {
                        groupConditions.Add(cGroupID, new List<CMconditionDataCell>());
                    }

                    groupConditions[cGroupID].Add(item.Value);
                });
            }
            if (groupConditions.TryGetValue(groupId, out var v))
            {
                return v;
            }

            return null;
        }

        #endregion

        #region 交互物件地图显示

        private InteractMapListData m_InteractMapList;

        public InteractMapListData M_InteractMapList
        {
            get
            {
                if (m_InteractMapList == null || m_InteractMapList.StaticInteractMapListDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<InteractMapListData>("InteractMapList", ref m_InteractMapList);
                }

                //缓存机制
                return m_InteractMapList;
            }
        }

        public InteractMapListDataCell GetInteractMapListDataCell(long interactId)
        {
            if (M_InteractMapList == null)
            {
                return null;
            }
            InteractMapListDataCell data;
            if (M_InteractMapList.StaticInteractMapListDatas.TryGetValue(interactId, out data))
            {
                return data;
            }

            return null;
        }

        #endregion

        #region 【职业技能信息】

        //----------------- 职业配置技能
        private JobData m_JobData;

        public JobData M_JobData
        {
            get
            {
                if (m_JobData == null || m_JobData.StaticJobDatas.Count == 0)
                {
                    /*m_JobData = */
                    LoadNMapDatasNewtonAsync<JobData>("Job", ref m_JobData);
                }

                //缓存机制
                return m_JobData;
            }
            set { m_JobData = value; }
        }

        public JobDataCell GetJobDataCell(int jobId)
        {
            JobDataCell jobDataCell;
            if (M_JobData.StaticJobDatas.TryGetValue(jobId, out jobDataCell))
            {
                return jobDataCell;
            }

            return null;
        }

        //----------------- 职业配置技能
        private JobSkillData m_JobSkillData;

        public JobSkillData M_JobSkillData
        {
            get
            {
                if (m_JobSkillData == null || m_JobSkillData.StaticJobSkillDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<JobSkillData>("JobSkill", ref m_JobSkillData);
                }

                return m_JobSkillData;
            }
            set { m_JobSkillData = value; }
        }

        public JobSkillDataCell GetJobSkillDataCell(int jobSkillID)
        {
            JobSkillDataCell jobSkillDataCell;
            if (M_JobSkillData.StaticJobSkillDatas.TryGetValue(jobSkillID, out jobSkillDataCell))
            {
                return jobSkillDataCell;
            }

            return null;
        }

        //----------------- 职业天赋配置
        private TalentData m_TalentData;

        public TalentData M_TalentData
        {
            get
            {
                if (m_TalentData == null || m_TalentData.StaticTalentDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TalentData>("Talent", ref m_TalentData);
                }

                return m_TalentData;
            }
            set { m_TalentData = value; }
        }

        public TalentDataCell GetTalentDataCell(int talentID)
        {
            TalentDataCell TalentDataCell;
            if (M_TalentData.StaticTalentDatas.TryGetValue(talentID, out TalentDataCell))
            {
                return TalentDataCell;
            }

            return null;
        }

        // ---------------- 职业技能 对应的 天赋List
        private Dictionary<int, List<TalentDataCell>> m_JobSkillID2Talent = new();

        /// <summary>
        /// 获取 jobSkillID 对应的 天赋 配置
        /// </summary>
        /// <param name="jobSkillID"></param>
        public List<TalentDataCell> GetJobSkillId2TalentDataCell(int jobSkillID)
        {
            if (m_JobSkillID2Talent.Count == 0)
            {
                M_TalentData.StaticTalentDatas.ForEach((item) =>
                {
                    TalentDataCell talentDataCell = item.Value;
                    int belongSkillID = talentDataCell.GetBelongSkill();
                    if (!m_JobSkillID2Talent.ContainsKey(belongSkillID))
                    {
                        m_JobSkillID2Talent.Add(belongSkillID, new List<TalentDataCell>());
                    }

                    m_JobSkillID2Talent[belongSkillID].Add(talentDataCell);
                });
            }

            if (!m_JobSkillID2Talent.ContainsKey(jobSkillID))
            {
                return null;
            }

            return m_JobSkillID2Talent[jobSkillID];
        }


        //----------------- 职业技能按钮默认配置
        private SkillPosSetData m_SkillPosSetData;

        public SkillPosSetData M_SkillPosSetData
        {
            get
            {
                if (m_SkillPosSetData == null || m_SkillPosSetData.StaticSkillPosSetDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SkillPosSetData>("SkillPosSet", ref m_SkillPosSetData);
                }

                return m_SkillPosSetData;
            }
            set { m_SkillPosSetData = value; }
        }

        private List<KeyValuePair<int, SkillPosSetDataCell>> m_SkillPosSetDataList;

        public List<KeyValuePair<int, SkillPosSetDataCell>> M_SkillPosSetDataList
        {
            get
            {
                if (m_SkillPosSetDataList == null || m_SkillPosSetDataList.Count == 0)
                {
                    m_SkillPosSetDataList = M_SkillPosSetData.StaticSkillPosSetDatas.ToList();
                }

                //缓存机制
                return m_SkillPosSetDataList;
            }
        }

        public SkillPosSetDataCell GetSkillPosSetDataCell(int skillPosSetID)
        {
            SkillPosSetDataCell skillPosSetDataCell;
            if (M_SkillPosSetData.StaticSkillPosSetDatas.TryGetValue(skillPosSetID, out skillPosSetDataCell))
            {
                return skillPosSetDataCell;
            }

            return null;
        }

        public SkillPosSetDataCell GetSkillPosSetDataCell(int skillPosSetID, int jobId)
        {
            if (M_SkillPosSetDataList == null)
            {
                return null;
            }

            for (int i = 0; i < M_SkillPosSetDataList.Count; i++)
            {
                //var item = M_SkillPosSetDataList[i].Value;

                //if (item.GetSort() == skillPosSetID && item.GetJob() == jobId)
                //{
                //    return item;
                //}
            }

            return null;
        }

        #endregion

        #region 【转职任务配置信息】

        private TransferConditionData m_TransferConditionData;

        public TransferConditionData M_TransferConditionData
        {
            get
            {
                if (m_TransferConditionData == null || m_TransferConditionData.StaticTransferConditionDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TransferConditionData>("TransferCondition", ref m_TransferConditionData);
                }

                return m_TransferConditionData;
            }
            set { m_TransferConditionData = value; }
        }

        public TransferConditionDataCell GetTransferConditionDataCell(int talentID)
        {
            TransferConditionDataCell TalentDataCell;
            if (M_TransferConditionData.StaticTransferConditionDatas.TryGetValue(talentID, out TalentDataCell))
            {
                return TalentDataCell;
            }

            return null;
        }

        #endregion

        #region 【声音管理配置信息】

        private AudioData m_AudioData;

        public AudioData M_AudioData
        {
            get
            {
                if (m_AudioData == null)
                {
                    /*  m_AudioData = */
                    LoadNMapDatasNewtonAsync<AudioData>("Audio", ref m_AudioData);
                }

                //缓存机制
                return m_AudioData;
            }
            set { m_AudioData = value; }
        }

        #endregion

        #region 【实体阵营信息】

        private FactionRelationData m_FactionRelationData;

        public FactionRelationData M_FactionRelationData
        {
            get
            {
                if (m_FactionRelationData == null || m_FactionRelationData.StaticFactionRelationDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<FactionRelationData>("FactionRelation", ref m_FactionRelationData);
                }

                //缓存机制
                return m_FactionRelationData;
            }
            set { m_FactionRelationData = value; }
        }

        public FactionRelationDataCell GetFactionRelationDataCell(int factionID)
        {
            FactionRelationDataCell cfg;
            if (!M_FactionRelationData.StaticFactionRelationDatas.TryGetValue(factionID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【怪物信息】

        private MonsterData m_MonsterData;

        public MonsterData M_MonsterData
        {
            get
            {
                if (m_MonsterData == null || m_MonsterData.StaticMonsterDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<MonsterData>("Monster", ref m_MonsterData);
                }

                //缓存机制
                return m_MonsterData;
            }
            set { m_MonsterData = value; }
        }

        public MonsterDataCell GetMonsterDataCell(long monsterID)
        {
            MonsterDataCell cfg;
            if (!M_MonsterData.StaticMonsterDatas.TryGetValue(monsterID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【怪物AI信息】

        private AITemplateData m_AITemplateData;

        public AITemplateData M_AITemplateData
        {
            get
            {
                if (m_AITemplateData == null || m_AITemplateData.StaticAITemplateDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AITemplateData>("AITemplate", ref m_AITemplateData);
                }

                //缓存机制
                return m_AITemplateData;
            }
            set { m_AITemplateData = value; }
        }

        public AITemplateDataCell GetAITemplateDataCell(int id)
        {
            AITemplateDataCell cfg;
            if (!M_AITemplateData.StaticAITemplateDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【NPC地图显示信息】

        private MonsetrMapListData m_MonsetrMapList;

        public MonsetrMapListData M_MonsetrMapList
        {
            get
            {
                if (m_MonsetrMapList == null || m_MonsetrMapList.StaticMonsetrMapListDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<MonsetrMapListData>("MonsetrMapList", ref m_MonsetrMapList);
                }

                //缓存机制
                return m_MonsetrMapList;
            }
            set { m_MonsetrMapList = value; }
        }

        public MonsetrMapListDataCell GetMonsetrMapListDataCell(long npcID)
        {
            MonsetrMapListDataCell cfg;
            if (!M_MonsetrMapList.StaticMonsetrMapListDatas.TryGetValue(npcID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion


        #region 【机器人信息】

        private RobotData m_RobotData;

        public RobotData M_RobotData
        {
            get
            {
                if (m_RobotData == null || m_RobotData.StaticRobotDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<RobotData>("Robot", ref m_RobotData);
                }

                //缓存机制
                return m_RobotData;
            }
            set { m_RobotData = value; }
        }

        public RobotDataCell GetRobotDataCell(long monsterID)
        {
            RobotDataCell cfg;
            if (!M_RobotData.StaticRobotDatas.TryGetValue(monsterID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion


        #region 【怪物属性信息】

        //private System.Collections.Generic.Dictionary<int, MonsterAttrDataCell> MonsterAttrDataDic = null;
        //public MonsterAttrDataCell GetMonsterAttrDataCellByUseTemplate(int useTemplate)
        //{
        //    if (MonsterAttrDataDic == null)
        //    {
        //        MonsterAttrDataDic = new System.Collections.Generic.Dictionary<int, MonsterAttrDataCell>();
        //        foreach (var item in M_MonsterAttrData.StaticMonsterAttrDatas)
        //        {
        //            MonsterAttrDataDic.Add(item.Value.GetUseTemplate(), item.Value);
        //        }
        //    }
        //    MonsterAttrDataCell cfg;
        //    if (!MonsterAttrDataDic.TryGetValue(useTemplate, out cfg))
        //    {
        //        return null;
        //    }
        //    return cfg;
        //}

        #endregion

        #region 【召唤物信息】

        private SummonData m_SummonData;

        public SummonData M_SummonData
        {
            get
            {
                if (m_SummonData == null || m_SummonData.StaticSummonDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SummonData>("Summon", ref m_SummonData);
                }

                //缓存机制
                return m_SummonData;
            }
            set { m_SummonData = value; }
        }

        public SummonDataCell GetSummonDataCell(int summonID)
        {
            SummonDataCell cfg;
            if (!M_SummonData.StaticSummonDatas.TryGetValue(summonID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【召唤物属性信息】

        private SummonAttrData m_SummonAttrData;

        public SummonAttrData M_SummonAttrData
        {
            get
            {
                if (m_SummonAttrData == null || m_SummonAttrData.StaticSummonAttrDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SummonAttrData>("SummonAttr", ref m_SummonAttrData);
                }

                //缓存机制
                return m_SummonAttrData;
            }
            set { m_SummonAttrData = value; }
        }

        public SummonAttrDataCell GetSummonAttrDataCell(int summonID)
        {
            SummonAttrDataCell cfg;
            if (!M_SummonAttrData.StaticSummonAttrDatas.TryGetValue(summonID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        //private System.Collections.Generic.Dictionary<int, SummonAttrDataCell> SummonAttrDataDic = null;
        //public SummonAttrDataCell GetSummonAttrDataCellByUseTemplate(int useTemplate)
        //{
        //    if (SummonAttrDataDic == null)
        //    {
        //        SummonAttrDataDic = new System.Collections.Generic.Dictionary<int, SummonAttrDataCell>();
        //        foreach (var item in M_SummonAttrData.StaticSummonAttrDatas)
        //        {
        //            SummonAttrDataDic.Add(item.Value.GetUseTemplate(), item.Value);
        //        }
        //    }
        //    SummonAttrDataCell cfg;
        //    if (!SummonAttrDataDic.TryGetValue(useTemplate, out cfg))
        //    {
        //        return null;
        //    }
        //    return cfg;
        //}

        #endregion

        #region 【伙伴信息】

        private PartnerData m_PartnerData;

        public PartnerData M_PartnerData
        {
            get
            {
                if (m_PartnerData == null || m_PartnerData.StaticPartnerDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerData>("Partner", ref m_PartnerData);
                }

                //缓存机制
                return m_PartnerData;
            }
            set { m_PartnerData = value; }
        }

        public PartnerDataCell GetPartnerDataCell(long monsterID)
        {
            PartnerDataCell cfg;
            if (!M_PartnerData.StaticPartnerDatas.TryGetValue(monsterID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        private PartnerNatureData m_PartnerNatureData;
        public PartnerNatureData M_PartnerNatureData
        {
            get
            {
                if (m_PartnerNatureData == null || m_PartnerNatureData.StaticPartnerNatureDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerNatureData>("PartnerNature", ref m_PartnerNatureData);
                }

                //缓存机制
                return m_PartnerNatureData;
            }
        }

        public PartnerNatureDataCell GetPartnerNatureDataCell(int group, int level)
        {

            foreach (var item in M_PartnerNatureData.StaticPartnerNatureDatas)
            {
                PartnerNatureDataCell cfg = item.Value;
                if (cfg.Group == group && cfg.Level == level)
                {
                    return cfg;
                }
            }

            return null;
        }

        private ParConversionData m_ParConversionData;
        private ParConversionData M_ParConversionData
        {
            get
            {
                if (m_ParConversionData == null || m_ParConversionData.StaticParConversionDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ParConversionData>("ParConversion", ref m_ParConversionData);

                }

                //缓存机制
                return m_ParConversionData;
            }
        }


        private QualificationsData m_QualificationData;

        public QualificationsData M_QualificationData
        {
            get
            {
                if (m_QualificationData == null)
                {
                    LoadNMapDatasNewtonAsync<QualificationsData>("Qualifications", ref m_QualificationData);

                }
                return m_QualificationData;
            }
        }

        public QualificationsDataCell GetQualificationsData(int id, int breakStep)
        {

            if (M_QualificationData != null && M_QualificationData.StaticQualificationsDatas.Count > 0)
            {
                foreach (var item in M_QualificationData.StaticQualificationsDatas)
                {
                    if (item.Value.GetQualificationID() == id && item.Value.GetBreakStep() == breakStep)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

        private QualificationsExpendData m_QualificationsExpendData;

        public QualificationsExpendData M_QualificationsExpendData
        {
            get
            {
                if (m_QualificationsExpendData == null)
                {
                    LoadNMapDatasNewtonAsync<QualificationsExpendData>("QualificationsExpend", ref m_QualificationsExpendData);

                }
                return m_QualificationsExpendData;
            }
        }

        public QualificationsExpendDataCell GetQualificationsExpendDataCell(int id, int breakStep)
        {
            if (M_QualificationsExpendData != null &&
                M_QualificationsExpendData.StaticQualificationsExpendDatas.Count > 0)
            {
                foreach (var item in M_QualificationsExpendData.StaticQualificationsExpendDatas)
                {
                    if (item.Value.GetQualificationsExpendID() == id && item.Value.GetBreakStep() == breakStep)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

        private DictionaryEx<int, Dictionary<int, List<ParConversionDataCell>>> qualificationID2Cfgs = new();

        private void InitQualificationID2Cfgs()
        {

            if (qualificationID2Cfgs.Count == 0)
            {
                foreach (var item in M_ParConversionData.StaticParConversionDatas)
                {
                    var cfg = item.Value;
                    var qualificationID = cfg.QualificationID;

                    if (!qualificationID2Cfgs.TryGetValue(qualificationID, out var nature2Cfgs))
                    {
                        nature2Cfgs = new Dictionary<int, List<ParConversionDataCell>>();
                        qualificationID2Cfgs.Add(qualificationID, nature2Cfgs);
                    }
                    var nature = cfg.Nature;
                    if (!nature2Cfgs.TryGetValue(nature, out var cfgLists))
                    {
                        cfgLists = new();
                        nature2Cfgs.Add(nature, cfgLists);
                    }
                    cfgLists.Add(cfg);
                }
            }
        }
        /// <summary>
        /// 获得当前 资质 所在的资质区间的配置 以及下一个资质
        /// </summary>
        /// <param name="curValue">当前的资质</param>
        public List<ParConversionDataCell> GetParConversionDataCell(int qualificationID, int curValue)
        {
            InitQualificationID2Cfgs();


            if (!qualificationID2Cfgs.TryGetValue(qualificationID, out var nature2Cfgs))
            {
                return null;
            }

            List<ParConversionDataCell> results = new();

            foreach (var item in nature2Cfgs)
            {
                var cfgs = item.Value;
                for (int i = 0; i < cfgs.Count; i++)
                {
                    // 已经是最后一个了, 直接就是 最高等级了
                    if (i == cfgs.Count - 1)
                    {
                        results.Add(cfgs[i]);
                        break;
                    }
                    else
                    {
                        var nextParConversion = cfgs[i + 1];

                        // 当前战力大于本级战力
                        if (cfgs[i].Range <= curValue)
                        {
                            // 如果大于当前等级，且小于下一个等级, 那就定位到当前等级
                            if (curValue < nextParConversion.Range)
                            {
                                results.Add(cfgs[i]);
                                break;
                            }
                            else
                            {
                                // 如果还是大于后面一个, 那就继续往后面找
                                continue;
                            }
                        }
                        else
                        {
                            // 当前战力小于 本级战力, 理论上这种情况不存在
                            results.Add(cfgs[i]);
                            break;
                        }

                    }
                }
            }

            return results;

        }



        private StarExpendData m_StarExpendData;

        public StarExpendData M_StarExpendData
        {
            get
            {
                if (m_StarExpendData == null || m_StarExpendData.StaticStarExpendDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<StarExpendData>("StarExpend", ref m_StarExpendData);
                }

                //缓存机制
                return m_StarExpendData;
            }
        }

        public StarExpendDataCell GetStarExpendDataCell(int quality, int star)
        {
            foreach (var item in M_StarExpendData.StaticStarExpendDatas)
            {
                if (item.Value.GetQuality() == quality && item.Value.GetStar() == star)
                {
                    return item.Value;
                }
            }
            return null;
        }


        #endregion

        #region 【伙伴技能信息】

        private ParSkillData m_ParSkillData;

        public ParSkillData M_ParSkillData
        {
            get
            {
                if (m_ParSkillData == null || m_ParSkillData.StaticParSkillDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ParSkillData>("ParSkill", ref m_ParSkillData);
                }

                //缓存机制
                return m_ParSkillData;
            }
            set { m_ParSkillData = value; }
        }

        private List<KeyValuePair<int, ParSkillDataCell>> m_ParSkillDataList;

        public List<KeyValuePair<int, ParSkillDataCell>> M_ParSkillDataList
        {
            get
            {
                if (m_ParSkillDataList == null || m_ParSkillDataList.Count == 0)
                {
                    m_ParSkillDataList = M_ParSkillData.StaticParSkillDatas.ToList();
                }

                //缓存机制
                return m_ParSkillDataList;
            }
        }

        public ParSkillDataCell GetParSkillDataCell(int id)
        {
            ParSkillDataCell cfg;
            if (!M_ParSkillData.StaticParSkillDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        public ParSkillDataCell GetParSkillDataCell(int parId, int starId)
        {
            if (M_ParSkillDataList == null)
            {
                return null;
            }

            for (int i = 0; i < M_ParSkillDataList.Count; i++)
            {
                var item = M_ParSkillDataList[i].Value;

                if (item.GetPar() == parId && item.GetStar() == starId)
                {
                    return item;
                }
            }

            return null;
        }

        public List<ParSkillDataCell> GetParSkillDataCellByParID(long parId)
        {
            List<ParSkillDataCell> list = new();
            if (M_ParSkillDataList == null)
            {
                return list;
            }

            for (int i = 0; i < M_ParSkillDataList.Count; i++)
            {
                var item = M_ParSkillDataList[i].Value;

                if (item.GetPar() == parId)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public bool GetPartnerSkillIDISHaveCfg(int parId, int skillID)
        {
            List<ParSkillDataCell> list = GetParSkillDataCellByParID(parId);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetParSkill() == skillID)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 【飘字信息】

        private DamageTextData m_DamageTextData;

        public DamageTextData M_DamageTextData
        {
            get
            {
                if (m_DamageTextData == null || m_DamageTextData.StaticDamageTextDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<DamageTextData>("DamageText", ref m_DamageTextData);
                }

                //缓存机制
                return m_DamageTextData;
            }
            set { m_DamageTextData = value; }
        }

        public DamageTextDataCell GetDamageTextDataCell(int id)
        {
            DamageTextDataCell cfg;
            if (!M_DamageTextData.StaticDamageTextDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【头顶名字颜色】

        private HeadNameColorData m_HeadNameColorData;

        public HeadNameColorData M_HeadNameColorData
        {
            get
            {
                if (m_HeadNameColorData == null || m_HeadNameColorData.StaticHeadNameColorDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<HeadNameColorData>("HeadNameColor", ref m_HeadNameColorData);
                }

                //缓存机制
                return m_HeadNameColorData;
            }
            set { m_HeadNameColorData = value; }
        }

        public HeadNameColorDataCell GetHeadNameColorDataCell(int id)
        {
            HeadNameColorDataCell cfg;
            if (!M_HeadNameColorData.StaticHeadNameColorDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【创角信息】

        private CharacterData m_CharacterData;

        public CharacterData M_CharacterData
        {
            get
            {
                if (m_CharacterData == null || m_CharacterData.StaticCharacterDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CharacterData>("Character", ref m_CharacterData);
                }

                //缓存机制
                return m_CharacterData;
            }
            set { m_CharacterData = value; }
        }

        public CharacterDataCell GetCharacterDataCell(int id)
        {
            CharacterDataCell cfg;
            if (!M_CharacterData.StaticCharacterDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        ///////////// 新的------------------------------------
        private CharacterCreateData m_CharacterCreateData;

        public CharacterCreateData M_CharacterCreateData
        {
            get
            {
                if (m_CharacterCreateData == null || m_CharacterCreateData.StaticCharacterCreateDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CharacterCreateData>("CharacterCreate", ref m_CharacterCreateData);
                }

                //缓存机制
                return m_CharacterCreateData;
            }
            set { m_CharacterCreateData = value; }
        }

        public CharacterCreateDataCell GetCharacterCreateDataCell(int id)
        {
            CharacterCreateDataCell cfg;
            if (!M_CharacterCreateData.StaticCharacterCreateDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        public CharacterCreateDataCell GetCharacterCreateDataCellBy(int id)
        {
            foreach (var item in M_CharacterCreateData.StaticCharacterCreateDatas)
            {
                if (item.Value.GetJobID() == id)
                {
                    return item.Value;
                }
            }

            return null;
        }

        #endregion

        #region camera相关

        #region camera震动配置

        private CameraShakeData m_CameraShakeData;

        public CameraShakeData M_CameraShakeData
        {
            get
            {
                if (m_CameraShakeData == null || m_CameraShakeData.StaticCameraShakeDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CameraShakeData>("CameraShake", ref m_CameraShakeData);
                }

                //缓存机制
                return m_CameraShakeData;
            }
            set { m_CameraShakeData = value; }
        }

        public CameraShakeDataCell GetCameraShakeDataCell(int effectID)
        {
            CameraShakeDataCell cameraShakeDataCell;
            if (M_CameraShakeData.StaticCameraShakeDatas.TryGetValue(effectID, out cameraShakeDataCell))
            {
                return cameraShakeDataCell;
            }

            return null;
        }

        #endregion

        #region cameraZoom 配置

        private CameraZoomData m_CameraZoomData;

        public CameraZoomData M_CameraZoomData
        {
            get
            {
                if (m_CameraZoomData == null || m_CameraZoomData.StaticCameraZoomDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CameraZoomData>("CameraZoom", ref m_CameraZoomData);
                }

                //缓存机制
                return m_CameraZoomData;
            }
            set { m_CameraZoomData = value; }
        }

        public CameraZoomDataCell GetCameraZoomDataCell(int effectID)
        {
            CameraZoomDataCell cameraZoomDataCell;
            if (M_CameraZoomData.StaticCameraZoomDatas.TryGetValue(effectID, out cameraZoomDataCell))
            {
                return cameraZoomDataCell;
            }

            return null;
        }

        #endregion

        #endregion

        #region 所有地图

        #region 大场景地图

        private SceneMapData m_SceneMapDataData;

        public SceneMapData M_SceneMapDataData
        {
            get
            {
                if (m_SceneMapDataData == null || m_SceneMapDataData.StaticSceneMapDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SceneMapData>("SceneMap", ref m_SceneMapDataData);
                }

                return m_SceneMapDataData;
            }
            set { m_SceneMapDataData = value; }
        }

        #region 国度区分

        private WorldMapListData m_WorldMapList;

        public WorldMapListData M_WorldMapList
        {
            get
            {
                if (m_WorldMapList == null || m_WorldMapList.StaticWorldMapListDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<WorldMapListData>("WorldMapList", ref m_WorldMapList);
                }

                return m_WorldMapList;
            }
            set { m_WorldMapList = value; }
        }

        public List<WorldMapListDataCell> GetWorldMapListDataCellList(int nationID)
        {
            List<WorldMapListDataCell> list = new();

            foreach (var item in M_WorldMapList.StaticWorldMapListDatas)
            {
                if (item.Value.GetFromNation() == nationID)
                {
                    list.Add(item.Value);
                }
            }

            return list;
        }

        public int GetMapIDFromeNation(int mapID)
        {
            foreach (var item in M_WorldMapList.StaticWorldMapListDatas)
            {
                if (item.Value.GetMapID() == mapID)
                {
                    return item.Value.GetFromNation();
                }
            }

            return -1;
        }

        #endregion

        #endregion

        #region 日常副本

        private DailyLevelData m_DailyLevel;

        public DailyLevelData M_DailyLevel
        {
            get
            {
                if (m_DailyLevel == null || m_DailyLevel.StaticDailyLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<DailyLevelData>("DailyLevel", ref m_DailyLevel);
                }

                return m_DailyLevel;
            }
        }

        #endregion

        #region 秘境副本

        private SecretLevelData m_SecretLevel;

        public SecretLevelData M_SecretLevel
        {
            get
            {
                if (m_SecretLevel == null || m_SecretLevel.StaticSecretLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SecretLevelData>("SecretLevel", ref m_SecretLevel);
                }

                return m_SecretLevel;
            }
        }

        #endregion

        #region 镜像副本

        private MirrorLevelData m_MirrorLevel;

        public MirrorLevelData M_MirrorLevel
        {
            get
            {
                if (m_MirrorLevel == null || m_MirrorLevel.StaticMirrorLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<MirrorLevelData>("MirrorLevel", ref m_MirrorLevel);
                }

                return m_MirrorLevel;
            }
        }

        #endregion

        #region 镜像副本

        private PlotLevelData m_PlotLevel;

        public PlotLevelData M_PlotLevel
        {
            get
            {
                if (m_PlotLevel == null || m_PlotLevel.StaticPlotLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PlotLevelData>("PlotLevel", ref m_PlotLevel);
                }

                return m_PlotLevel;
            }
        }

        #endregion

        #region 副本目标

        private EctypeTargetData m_EctypeTarget;

        public EctypeTargetData M_EctypeTarget
        {
            get
            {
                if (m_EctypeTarget == null || m_EctypeTarget.StaticEctypeTargetDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EctypeTargetData>("EctypeTarget", ref m_EctypeTarget);
                }

                return m_EctypeTarget;
            }
        }

        public EctypeTargetDataCell GetEctypeTargetDataCell(int id)
        {
            if (M_EctypeTarget.StaticEctypeTargetDatas.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }

        #endregion

        #region 组队-日常副本

        private TeamDailyLevelData m_TeamDailyLevel;

        public TeamDailyLevelData M_TeamDailyLevel
        {
            get
            {
                if (m_TeamDailyLevel == null || m_TeamDailyLevel.StaticTeamDailyLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TeamDailyLevelData>("TeamDailyLevel", ref m_TeamDailyLevel);
                }

                return m_TeamDailyLevel;
            }
        }

        #endregion

        #region 通缉玩法场景

        private WantedLevelData m_WantedLevel;

        public WantedLevelData M_WantedLevel
        {
            get
            {
                if (m_WantedLevel == null || m_WantedLevel.StaticWantedLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<WantedLevelData>("WantedLevel", ref m_WantedLevel);
                }

                return m_WantedLevel;
            }
        }

        #endregion

        #region GNG玩法场景

        private GNGLevelData m_GNGLevel;

        public GNGLevelData M_GNGLevel
        {
            get
            {
                if (m_GNGLevel == null || m_GNGLevel.StaticGNGLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GNGLevelData>("GNGLevel", ref m_GNGLevel);
                }

                return m_GNGLevel;
            }
        }

        #endregion

        #region 爬塔

        private TowerLevelData m_TowerLevel;

        public TowerLevelData M_TowerLevel
        {
            get
            {
                if (m_TowerLevel == null || m_TowerLevel.StaticTowerLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TowerLevelData>("TowerLevel", ref m_TowerLevel);
                }

                return m_TowerLevel;
            }
        }

        #endregion

        #region 10v10战场

        private BFLevelData m_BFLevel;

        public BFLevelData M_BFLevel
        {
            get
            {
                if (m_BFLevel == null || m_BFLevel.StaticBFLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<BFLevelData>("BFLevel", ref m_BFLevel);
                }

                return m_BFLevel;
            }
        }

        #endregion


        #region 地图基础配置表

        private MapBaseData m_MapBase;

        public MapBaseData M_MapBase
        {
            get
            {
                if (m_MapBase == null || m_MapBase.StaticMapBaseDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<MapBaseData>("MapBase", ref m_MapBase);
                }

                return m_MapBase;
            }
        }

        public MapBaseDataCell GetMapBaseDataCell(int id)
        {
            MapBaseDataCell data;
            if (M_MapBase.StaticMapBaseDatas.TryGetValue(id, out data))
            {
                return data;
            }

            return null;
        }

        #endregion


        private Dictionary<int, MapCfgData> m_MapCfgDataDic = new();
        private void InitMapCfgData()
        {
            m_MapCfgDataDic.Clear();

            // 0.SceneMap 大场景/默认/公会领地
            var sceneMap = M_SceneMapDataData.StaticSceneMapDatas;
            foreach (var item in sceneMap)
            {
                var v = item.Value;
                MapCfgData mapCfgData = new(
                    id: v.GetID(),
                    mapName: v.SceneName,
                    mapBaseID: v.GetBaseID(),
                    levelCollider: string.Empty,
                    reviveTid: 0,
                    subType: string.Empty,
                    autoBattle: false,
                    mapType: (SpaceType)v.GetSceneType(),
                    enterTip: v.EnterTip,
                    openRoleLv: v.GetOpenRoleLv(),
                    openComTask: v.GetOpenComTask(),
                    isHidePartner: v.IsHidePartner,
                    isCameraRotate: v.IsCameraRotate,
                    isCameraTeleport: v.IsCameraTeleport
                    );
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 1.levelDatas 日常副本
            var levelDatas = M_DailyLevel.StaticDailyLevelDatas;
            foreach (var item in levelDatas)
            {
                var v = item.Value;
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: 0, subType: string.Empty, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: SpaceType.SpaceDaily);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 2.secretDatas 单人秘境
            var secretDatas = M_SecretLevel.StaticSecretLevelDatas;
            foreach (var item in secretDatas)
            {
                var v = item.Value;
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: 0, subType: string.Empty, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: SpaceType.SpaceSercet);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 3.MirrorLevel 镜像和藏宝图
            var mirrorDatas = M_MirrorLevel.StaticMirrorLevelDatas;
            foreach (var item in mirrorDatas)
            {
                var v = item.Value;
                SpaceType MapType = GetMapSpaceType(v.SubType);
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: v.SubType, isShowEffect: v.GetIsShowEffect(), autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: MapType);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 4.PlotLevel 剧情
            var plotDatas = M_PlotLevel.StaticPlotLevelDatas;
            foreach (var item in plotDatas)
            {
                var v = item.Value;
                SpaceType MapType = GetMapSpaceType(v.SubType);
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: v.SubType, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: MapType);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 5.TeamDailyLevel 组队日常本
            var teamDailyDatas = M_TeamDailyLevel.StaticTeamDailyLevelDatas;
            foreach (var item in teamDailyDatas)
            {
                var v = item.Value;
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: 0, subType: string.Empty, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: SpaceType.SpaceTeamDaily);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 6.TowerLevel 爬塔本
            var towerLevel = M_TowerLevel.StaticTowerLevelDatas;
            foreach (var item in towerLevel)
            {
                var v = item.Value;
                SpaceType MapType = GetMapSpaceType(v.SubType);
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: v.SubType, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: MapType);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 7.WantedLevel 通缉本
            var wantedLevel = M_WantedLevel.StaticWantedLevelDatas;
            foreach (var item in wantedLevel)
            {
                var v = item.Value;
                SpaceType MapType = GetMapSpaceType(v.SubType);
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: v.SubType, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: MapType);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 8.GNGLevel 午间副本
            var gNGLevel = M_GNGLevel.StaticGNGLevelDatas;
            foreach (var item in gNGLevel)
            {
                var v = item.Value;
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: string.Empty, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: SpaceType.SpaceGng);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
            // 9.GNGLevel 竞技场/战场
            var bFLevel = M_BFLevel.StaticBFLevelDatas;
            foreach (var item in bFLevel)
            {
                var v = item.Value;
                SpaceType MapType = GetMapSpaceType(v.SubType);
                MapCfgData mapCfgData = new(id: v.GetID(), mapName: v.LevelName, mapBaseID: v.GetBaseID(), levelCollider: v.LevelCollider, reviveTid: v.GetReviveTid(), subType: v.SubType, autoBattle: v.GetAutoBattle(), isHidePartner: v.IsHidePartner, isCameraRotate: v.IsCameraRotate, mapType: MapType);
                AddMapCfgDataDic(v.GetID(), mapCfgData);
            }
        }

        private void AddMapCfgDataDic(int id, MapCfgData mapCfgData)
        {
            if (m_MapCfgDataDic.ContainsKey(id))
            {
                SGF.Debuger.LogWarning($"封装地图数据有重复的id={id},err!!!");
            }
            else
            {
                m_MapCfgDataDic.Add(id, mapCfgData);
            }
        }

        private SpaceType GetMapSpaceType(string subType)
        {
            SpaceType MapType = SpaceType.SpaceDefault;
            switch (subType)
            {
                case GameConfig.INSTANCE_NORMAL:
                case GameConfig.INSTANCE_PERSON_DAILY:
                case GameConfig.INSTANCE_PERSON_SERCET:
                case GameConfig.INSTANCE_TEAM_DAILY:
                case GameConfig.INSTANCE_WTASK:
                case GameConfig.INSTANCE_GUILD_NOON:
                case GameConfig.INSTANCE_GuildTerritory:
                    MapType = SpaceType.SpaceDefault;
                    break;
                case GameConfig.INSTANCE_PLOT:
                case GameConfig.INSTANCE_PLOT_FIRST:
                case GameConfig.INSTANCE_PLOT_SECOND:
                case GameConfig.INSTANCE_PLOT_THIRD:
                    MapType = SpaceType.SpacePlot;
                    break;
                case GameConfig.INSTANCE_MIRROR:
                    MapType = SpaceType.SpaceMirror;
                    break;
                case GameConfig.INSTANCE_PLOT_TREASURE:
                    MapType = SpaceType.SpacePlotTreasure;
                    break;
                case GameConfig.INSTANCE_MIRROR_TREASURE:
                    MapType = SpaceType.SpaceMirrorTreasure;
                    break;
                case GameConfig.INSTANCE_10v10:
                    MapType = SpaceType.Space10V10;
                    break;
                case GameConfig.INSTANCE_1v1:
                    MapType = SpaceType.SpaceArena;
                    break;
                case GameConfig.INSTANCE_PERSON_TOWER:
                    MapType = SpaceType.SpacePersonTower;
                    break;
                default:
                    MapType = SpaceType.SpaceDefault;
                    break;
            }
            return MapType;
        }

        public MapCfgData GetMapCfgData(int mapID)
        {
            if (m_MapCfgDataDic == null || m_MapCfgDataDic.Count <= 0)
            {
                InitMapCfgData();
            }
            MapCfgData mapCfgData = null;
            if (m_MapCfgDataDic.TryGetValue(mapID, out mapCfgData))
            {

            }
            return mapCfgData;
        }

        #endregion

        #region 帧事件相关配置

        private FramEventData m_FramEventData;

        public FramEventData M_FramEventData
        {
            get
            {
                if (m_FramEventData == null || m_FramEventData.StaticFramEventDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<FramEventData>("FramEvent", ref m_FramEventData);
                }

                //缓存机制
                return m_FramEventData;
            }
            set { m_FramEventData = value; }
        }

        public FramEventDataCell GetFramEventDataCell(int id)
        {
            FramEventDataCell framEventDataCell;
            if (M_FramEventData.StaticFramEventDatas.TryGetValue(id, out framEventDataCell))
            {
                return framEventDataCell;
            }

            return null;
        }

        private VideoData m_VideoData;

        public VideoData M_VideoData
        {
            get
            {
                if (m_VideoData == null || m_VideoData.StaticVideoDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<VideoData>("Video", ref m_VideoData);
                }

                //缓存机制
                return m_VideoData;
            }
            set { m_VideoData = value; }
        }

        public VideoDataCell GetVideoDataCell(int id)
        {
            VideoDataCell framEventDataCell;
            if (M_VideoData.StaticVideoDatas.TryGetValue(id, out framEventDataCell))
            {
                return framEventDataCell;
            }

            return null;
        }

        #endregion

        #region AVG 相关

        private CommonDialogData m_CommonDialogData;

        public CommonDialogData M_CommonDialogData
        {
            get
            {
                if (m_CommonDialogData == null || m_CommonDialogData.StaticCommonDialogDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CommonDialogData>("CommonDialog", ref m_CommonDialogData);
                }

                //缓存机制
                return m_CommonDialogData;
            }
            set { m_CommonDialogData = value; }
        }

        public CommonDialogDataCell GetCommonDialogDataCell(int dialogID)
        {
            CommonDialogDataCell commonDialogDataCell;
            if (M_CommonDialogData.StaticCommonDialogDatas.TryGetValue(dialogID, out commonDialogDataCell))
            {
                return commonDialogDataCell;
            }

            return null;
        }

        #endregion

        #region 【NPC信息】

        private NpcData m_NpcData;

        public NpcData M_NpcData
        {
            get
            {
                if (m_NpcData == null || m_NpcData.StaticNpcDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<NpcData>("Npc", ref m_NpcData);
                }

                //缓存机制
                return m_NpcData;
            }
            set { m_NpcData = value; }
        }

        public NpcDataCell GetNPCDataCell(long npcID)
        {
            NpcDataCell cfg;
            if (!M_NpcData.StaticNpcDatas.TryGetValue(npcID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        public NpcDataCell GetNPCDataCell(int npcID)
        {
            NpcDataCell cfg;
            if (!M_NpcData.StaticNpcDatas.TryGetValue(npcID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 【NPC地图显示信息】

        private NpcMapListData m_NpcMapList;

        public NpcMapListData M_NpcMapList
        {
            get
            {
                if (m_NpcMapList == null || m_NpcMapList.StaticNpcMapListDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<NpcMapListData>("NpcMapList", ref m_NpcMapList);
                }

                //缓存机制
                return m_NpcMapList;
            }
            set { m_NpcMapList = value; }
        }

        public NpcMapListDataCell GetNpcMapListDataCell(long npcID)
        {
            NpcMapListDataCell cfg;
            if (!M_NpcMapList.StaticNpcMapListDatas.TryGetValue(npcID, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion


        #region GVEBoss表

        private GVEBossSelectData m_GVEBossSelectData;

        public GVEBossSelectData M_GVEBossSelectData
        {
            get
            {
                if (m_GVEBossSelectData == null || m_GVEBossSelectData.StaticGVEBossSelectDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GVEBossSelectData>("GVEBossSelect", ref m_GVEBossSelectData);
                }

                //缓存机制
                return m_GVEBossSelectData;
            }
            set { m_GVEBossSelectData = value; }
        }

        public GVEBossSelectDataCell GetGVEBossSelectDataCell(int itemID)
        {
            GVEBossSelectDataCell gVEBossSelectDataCell;
            if (M_GVEBossSelectData.StaticGVEBossSelectDatas.TryGetValue(itemID, out gVEBossSelectDataCell))
            {
                return gVEBossSelectDataCell;
            }

            return null;
        }

        public int GetGVEBossType(long bossID, int playMode)
        {
            foreach (var kv in M_GVEBossSelectData.StaticGVEBossSelectDatas)
            {
                if (kv.Value.GetPlayMode() == playMode && kv.Value.GetUseBoss() == bossID)
                {
                    return kv.Value.GetBossType();
                }
            }

            return 0;
        }

        #endregion

        #region 道具配置表

        private ItemData m_ItemData;

        public ItemData M_ItemData
        {
            get
            {
                if (m_ItemData == null || m_ItemData.StaticItemDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ItemData>("Item", ref m_ItemData);
                }

                //缓存机制
                return m_ItemData;
            }
            set { m_ItemData = value; }
        }

        public ItemDataCell GetItemDataCell(long itemID)
        {
            ItemDataCell itemDataCell;
            if (M_ItemData.StaticItemDatas.TryGetValue(itemID, out itemDataCell))
            {
                return itemDataCell;
            }

            return null;
        }

        public long GetItemIdByDealid(long Dealid)
        {
            foreach (var kv in M_ItemData.StaticItemDatas)
            {
                if (kv.Value.GetDealid() == Dealid)
                {
                    return kv.Key;
                }
            }

            return 0;
        }

        #endregion

        #region 伙伴装备表
        private PartnerEquipData m_PartnerEquipData;
        private PartnerEquipData M_PartnerEquipData
        {
            get
            {
                if (m_PartnerEquipData == null || m_PartnerEquipData.StaticPartnerEquipDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerEquipData>("PartnerEquip", ref m_PartnerEquipData);
                }

                //缓存机制
                return m_PartnerEquipData;
            }
            set { m_PartnerEquipData = value; }
        }

        public PartnerEquipDataCell GetPartnerEquipDataCell(long id)
        {
            if (M_PartnerEquipData.StaticPartnerEquipDatas.TryGetValue(id, out PartnerEquipDataCell cfg))
            {
                return cfg;
            }

            return null;
        }

        private PartnerExpData m_PartnerExpData;

        public PartnerExpData M_PartnerExpData
        {
            get
            {
                if (m_PartnerExpData == null || m_PartnerExpData.StaticPartnerExpDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerExpData>("PartnerExp", ref m_PartnerExpData);

                }
                return m_PartnerExpData;
            }
        }


        public PartnerExpDataCell GetPartnerExpDataCell(int id)
        {
            if (M_PartnerExpData != null && M_PartnerExpData.StaticPartnerExpDatas.Count > 0)
            {
                foreach (var item in M_PartnerExpData.StaticPartnerExpDatas)
                {
                    if (item.Value.Level == id)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }


        #endregion

        #region 装备配置表

        private EquipData m_EquipData;

        public EquipData M_EquipData
        {
            get
            {
                if (m_EquipData == null || m_EquipData.StaticEquipDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EquipData>("Equip", ref m_EquipData);
                }

                //缓存机制
                return m_EquipData;
            }
            set { m_EquipData = value; }
        }

        public EquipDataCell GetEquipDataCell(int equipID)
        {
            EquipDataCell equipDataCell;
            if (M_EquipData.StaticEquipDatas.TryGetValue(equipID, out equipDataCell))
            {
                return equipDataCell;
            }

            return null;
        }

        private EquipSubData m_EquipSubData;

        public EquipSubData M_EquipSubData
        {
            get
            {
                if (m_EquipSubData == null || m_EquipSubData.StaticEquipSubDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EquipSubData>("EquipSub", ref m_EquipSubData);
                }

                //缓存机制
                return m_EquipSubData;
            }
            set { m_EquipSubData = value; }
        }

        public EquipSubDataCell GetEquipSubDataCell(int equipID)
        {
            EquipSubDataCell equipSubDataCell;
            if (M_EquipSubData.StaticEquipSubDatas.TryGetValue(equipID, out equipSubDataCell))
            {
                return equipSubDataCell;
            }

            return null;
        }

        private SubCurveData m_SubCurveData;

        public SubCurveData M_SubCurveData
        {
            get
            {
                if (m_SubCurveData == null || m_SubCurveData.StaticSubCurveDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SubCurveData>("SubCurve", ref m_SubCurveData);
                }

                //缓存机制
                return m_SubCurveData;
            }
            set { m_SubCurveData = value; }
        }

        private DictionaryEx<int, List<SubCurveDataCell>> curveId2Cells = new();
        public SubCurveDataCell GetSubCurveDataCell(int curve, int level)
        {
            if (curveId2Cells.Count == 0)
            {
                foreach (var item in M_SubCurveData.StaticSubCurveDatas)
                {
                    var v = item.Value;
                    List<SubCurveDataCell> lists = curveId2Cells[v.Curve];
                    if (lists == null)
                    {
                        lists = new List<SubCurveDataCell>();
                        curveId2Cells[v.Curve] = lists;
                    }

                    lists.Add(v);
                }

            }

            var cells = curveId2Cells[curve];
            for (int i = 0; i < cells.Count; i++)
            {
                // 已经是最后一个了
                if (i == cells.Count - 1)
                {
                    return cells[i];
                }
                // 大于当前的 小于下一个
                if (level >= cells[i].Level && level < cells[i + 1].Level)
                {
                    return cells[i];
                }
            }

            return null;
        }


        private EquipSlotIntensifyData m_EquipSlotIntensifyData;

        public EquipSlotIntensifyData M_EquipSlotIntensifyData
        {
            get
            {
                if (m_EquipSlotIntensifyData == null || m_EquipSlotIntensifyData.StaticEquipSlotIntensifyDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EquipSlotIntensifyData>("EquipSlotIntensify", ref m_EquipSlotIntensifyData);
                }

                //缓存机制
                return m_EquipSlotIntensifyData;
            }
            set { m_EquipSlotIntensifyData = value; }
        }

        private DictionaryEx<int, DictionaryEx<int, EquipSlotIntensifyDataCell>> equipSlots = new();

        public EquipSlotIntensifyDataCell GetEquipSlotIntensifyData(int slot_id, int level)
        {
            // 数据初始化
            if (equipSlots.Count == 0)
            {
                foreach (var item in M_EquipSlotIntensifyData.StaticEquipSlotIntensifyDatas)
                {
                    var equipSlotCell = item.Value;

                    if (equipSlots[equipSlotCell.Slot_id] == null)
                    {
                        equipSlots[equipSlotCell.Slot_id] = new DictionaryEx<int, EquipSlotIntensifyDataCell>();
                    }

                    equipSlots[equipSlotCell.Slot_id][equipSlotCell.Level] = equipSlotCell;
                }

            }

            if (equipSlots[slot_id] == null)
            {
                return null;
            }

            if (equipSlots[slot_id][level] == null)
            {
                return null;
            }

            return equipSlots[slot_id][level];

        }


        private EquipSlotResonanceData m_EquipSlotResonanceData;

        private EquipSlotResonanceData M_EquipSlotResonanceData
        {
            get
            {
                if (m_EquipSlotResonanceData == null || m_EquipSlotResonanceData.StaticEquipSlotResonanceDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EquipSlotResonanceData>("EquipSlotResonance", ref m_EquipSlotResonanceData);
                }

                //缓存机制
                return m_EquipSlotResonanceData;
            }
            set { m_EquipSlotResonanceData = value; }
        }

        private DictionaryEx<int, int> Job2ResonanceLvMax = new();

        public int GetJobMaxResonanceLv(int jobID)
        {
            if (Job2ResonanceLvMax.Count == 0)
            {
                var maxLv = 0;
                foreach (var item in M_EquipSlotResonanceData.StaticEquipSlotResonanceDatas)
                {
                    var cfg = item.Value;
                    cfg.Job_id.ForEach((jobID) =>
                    {
                        if (!Job2ResonanceLvMax.TryGetValue(jobID, out var lv))
                        {
                            Job2ResonanceLvMax.Add(jobID, cfg.ResonanceLv);
                        }

                        maxLv = Math.Max(Job2ResonanceLvMax[jobID], cfg.ResonanceLv);
                        // 把职业对应的 最大共鸣等级 存下来
                        Job2ResonanceLvMax[jobID] = maxLv;
                    });
                }
            }

            if (!Job2ResonanceLvMax.ContainsKey(jobID))
            {
                return 0;
            }

            return Job2ResonanceLvMax[jobID];

        }

        /// <summary>
        /// 得到 装备强化等级对应的 共鸣配置
        /// </summary>
        /// <param name="resonanceLv">共鸣等级</param>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public EquipSlotResonanceDataCell GetEquipSlotResonanceDataCell(int resonanceLv, int jobID)
        {
            foreach (var item in M_EquipSlotResonanceData.StaticEquipSlotResonanceDatas)
            {
                var cfg = item.Value;
                if (cfg.ResonanceLv != resonanceLv)
                {
                    continue;
                }

                var idx = cfg.Job_id.FindIndex((cfgJobId) =>
                {
                    return cfgJobId == jobID;
                });
                if (-1 != idx)
                {
                    return cfg;
                }

            }
            return null;
        }

        private ComposeData m_ComposeData;
        /// <summary>
        /// 合成表
        /// </summary>
        private ComposeData M_ComposeData;

        public ComposeDataCell GetComposeDataCell(int itemID)
        {
            if (M_ComposeData.StaticComposeDatas.TryGetValue(itemID, out var cfg))
            {
                return cfg;
            }
            return null;
        }


        #endregion

        #region 常量配置表

        private SystemData m_SystemData;

        public SystemData M_SystemData
        {
            get
            {
                if (m_SystemData == null || m_SystemData.StaticSystemDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SystemData>("System", ref m_SystemData);
                }

                //缓存机制
                return m_SystemData;
            }
            set { m_SystemData = value; }
        }

        public SystemDataCell GetSystemDataCell(int id)
        {
            SystemDataCell systemDataCell;
            if (M_SystemData.StaticSystemDatas.TryGetValue(id, out systemDataCell))
            {
                return systemDataCell;
            }

            return null;
        }

        public SystemDataCell GetSystemDataCell(string enumName)
        {
            foreach (var item in M_SystemData.StaticSystemDatas)
            {
                if (item.Value.EnumName == enumName)
                {
                    return item.Value;
                }
            }

            return null;
        }


        #endregion

        #region 道具使用表

        private ItemUseData m_ItemUseData;

        public ItemUseData M_ItemUseData
        {
            get
            {
                if (m_ItemUseData == null || m_ItemUseData.StaticItemUseDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ItemUseData>("ItemUse", ref m_ItemUseData);
                }

                //缓存机制
                return m_ItemUseData;
            }
            set { m_ItemUseData = value; }
        }

        public ItemUseDataCell GetItemUseDataCell(long itemUseID)
        {
            ItemUseDataCell itemUseDataCell;
            if (M_ItemUseData.StaticItemUseDatas.TryGetValue(itemUseID, out itemUseDataCell))
            {
                return itemUseDataCell;
            }

            return null;
        }

        #endregion

        #region 背包配置表

        private ItemSpaceData m_ItemSpaceData;

        public ItemSpaceData M_ItemSpaceData
        {
            get
            {
                if (m_ItemSpaceData == null || m_ItemSpaceData.StaticItemSpaceDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ItemSpaceData>("ItemSpace", ref m_ItemSpaceData);
                }

                //缓存机制
                return m_ItemSpaceData;
            }
            set { m_ItemSpaceData = value; }
        }

        public ItemSpaceDataCell GetItemSpaceDataCell(int itemSpaceID)
        {
            ItemSpaceDataCell itemSpaceDataCell;
            if (M_ItemSpaceData.StaticItemSpaceDatas.TryGetValue(itemSpaceID, out itemSpaceDataCell))
            {
                return itemSpaceDataCell;
            }

            return null;
        }

        #endregion

        #region 背包配置表

        private CurrencyCfgData m_CurrencyCfg;

        public CurrencyCfgData M_CurrencyCfg
        {
            get
            {
                if (m_CurrencyCfg == null || m_CurrencyCfg.StaticCurrencyCfgDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CurrencyCfgData>("CurrencyCfg", ref m_CurrencyCfg);
                }

                //缓存机制
                return m_CurrencyCfg;
            }
            set { m_CurrencyCfg = value; }
        }

        public CurrencyCfgDataCell GetCurrencyCfgDataCell(int itemid)
        {
            CurrencyCfgDataCell currencyCfgDataCell;
            if (M_CurrencyCfg.StaticCurrencyCfgDatas.TryGetValue(itemid, out currencyCfgDataCell))
            {
                return currencyCfgDataCell;
            }

            return null;
        }

        #endregion

        #region 任务配置

        private TaskRewardData m_taskRewardData;

        public TaskRewardData M_TaskRewardData
        {
            get
            {
                if (m_taskRewardData == null || m_taskRewardData.StaticTaskRewardDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TaskRewardData>("TaskReward", ref m_taskRewardData);
                }

                return m_taskRewardData;
            }
        }

        public TaskRewardDataCell GetTaskRewardDataCell(int taskid)
        {
            if (M_TaskRewardData != null)
            {
                if (M_TaskRewardData.StaticTaskRewardDatas.TryGetValue(taskid, out var task))
                {
                    return task;
                }
            }

            return null;
        }

        private TaskData m_taskData;

        public TaskData M_taskData
        {
            get
            {
                if (m_taskData == null || m_taskData.StaticTaskDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TaskData>("Task", ref m_taskData);
                }

                return m_taskData;
            }
        }

        private TaskEventData m_taskEventData;

        public TaskEventData M_TaskEventData
        {
            get
            {
                if (m_taskEventData == null || m_taskEventData.StaticTaskEventDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TaskEventData>("TaskEvent", ref m_taskEventData);
                }

                return m_taskEventData;
            }
        }

        private PlotData m_PlotData;

        public PlotData M_PlotData
        {
            get
            {
                if (m_PlotData == null || m_PlotData.StaticPlotDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PlotData>("Plot", ref m_PlotData);
                }

                return m_PlotData;
            }
        }

        #endregion

        #region 通用错误码消息提示表（服务器）

        private RetMsgData m_RetMsgData;

        public RetMsgData M_RetMsgData
        {
            get
            {
                if (m_RetMsgData == null || m_RetMsgData.StaticRetMsgDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<RetMsgData>("RetMsg", ref m_RetMsgData);
                }

                //缓存机制
                return m_RetMsgData;
            }
            set { m_RetMsgData = value; }
        }

        public RetMsgDataCell GetRetMsgDataCell(int messageID)
        {
            RetMsgDataCell retMsgDataCell;
            if (M_RetMsgData.StaticRetMsgDatas.TryGetValue(messageID, out retMsgDataCell))
            {
                return retMsgDataCell;
            }

            return null;
        }

        #endregion

        #region 通用错误码消息提示表（客户端）

        private CRetMsgData m_CRetMsgData;

        public CRetMsgData M_CRetMsgData
        {
            get
            {
                if (m_CRetMsgData == null || m_CRetMsgData.StaticCRetMsgDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<CRetMsgData>("CRetMsg", ref m_CRetMsgData);
                }

                //缓存机制
                return m_CRetMsgData;
            }
            set { m_CRetMsgData = value; }
        }

        public CRetMsgDataCell GetCRetMsgDataCell(CRetMsgEnum cRetMsgEnum)
        {
            CRetMsgDataCell retMsgDataCell;
            if (M_CRetMsgData.StaticCRetMsgDatas.TryGetValue((int)cRetMsgEnum, out retMsgDataCell))
            {
                return retMsgDataCell;
            }

            return null;
        }

        public CRetMsgDataCell GetCRetMsgDataCell(int id)
        {
            CRetMsgDataCell retMsgDataCell;
            if (M_CRetMsgData.StaticCRetMsgDatas.TryGetValue(id, out retMsgDataCell))
            {
                return retMsgDataCell;
            }

            return null;
        }

        #endregion

        #region 通用弹窗

        private WindowData m_window;

        public WindowData M_Window
        {
            get
            {
                if (m_window == null || m_window.StaticWindowDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<WindowData>("Window", ref m_window);
                }

                return m_window;
            }
        }

        public WindowDataCell GetWindowDataCell(int id)
        {
            if (M_Window.StaticWindowDatas.TryGetValue(id, out var data))
            {
                return data;
            }

            return null;
        }

        #endregion

        #region 模型动作 配置

        private ModelAnimancerData m_ModelAnimancerData;

        public ModelAnimancerData M_ModelAnimancerData
        {
            get
            {
                if (m_ModelAnimancerData == null || m_ModelAnimancerData.StaticModelAnimancerDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ModelAnimancerData>("ModelAnimancer", ref m_ModelAnimancerData);
                }

                return m_ModelAnimancerData;
            }
        }

        public ModelAnimancerDataCell GetModelAnimancerDataCell(int id)
        {
            if (M_ModelAnimancerData.StaticModelAnimancerDatas.TryGetValue(id, out var data))
            {
                return data;
            }

            return null;
        }

        #endregion

        #region 副本复活配置表

        private LevelReviveData m_LevelReviveData;

        public LevelReviveData M_LevelReviveData
        {
            get
            {
                if (m_LevelReviveData == null || m_LevelReviveData.StaticLevelReviveDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LevelReviveData>("LevelRevive", ref m_LevelReviveData);
                }

                //缓存机制
                return m_LevelReviveData;
            }
            set { m_LevelReviveData = value; }
        }

        public LevelReviveDataCell GetLevelReviveDataCell(int levelReviveDataCellID)
        {
            LevelReviveDataCell levelReviveDataCell;
            if (M_LevelReviveData.StaticLevelReviveDatas.TryGetValue(levelReviveDataCellID, out levelReviveDataCell))
            {
                return levelReviveDataCell;
            }

            return null;
        }

        #endregion

        #region 单服副本

        private SingleLevelData m_SingleLevelData;

        public SingleLevelData M_SingleLevelData
        {
            get
            {
                if (m_SingleLevelData == null || m_SingleLevelData.StaticSingleLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SingleLevelData>("SingleLevel", ref m_SingleLevelData);
                }

                //缓存机制
                return m_SingleLevelData;
            }
            set { m_SingleLevelData = value; }
        }

        public SingleLevelDataCell GetSingleLevelDataCell(int singleLevelDataCellID)
        {
            SingleLevelDataCell singleLevelDataCell;
            if (M_SingleLevelData.StaticSingleLevelDatas.TryGetValue(singleLevelDataCellID, out singleLevelDataCell))
            {
                return singleLevelDataCell;
            }

            return null;
        }

        #endregion

        #region gm配置

        private GMData m_GMData;

        public GMData M_GMData
        {
            get
            {
                if (m_GMData == null || m_GMData.StaticGMDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GMData>("GM", ref m_GMData);
                }

                //缓存机制
                return m_GMData;
            }
            set { m_GMData = value; }
        }

        public GMDataCell GetGMDataCell(int gmId)
        {
            GMDataCell gMDataCell;
            if (M_GMData.StaticGMDatas.TryGetValue(gmId, out gMDataCell))
            {
                return gMDataCell;
            }

            return null;
        }

        #endregion

        #region 动画融合时间配置

        private AnimancerEnterTimeData m_animEnterTimeData;

        private AnimancerEnterTimeData M_animEnterTimeData
        {
            get
            {
                if (m_animEnterTimeData == null || m_animEnterTimeData.StaticAnimancerEnterTimeDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AnimancerEnterTimeData>("AnimancerEnterTime", ref m_animEnterTimeData);
                }

                return m_animEnterTimeData;
            }
            set { m_animEnterTimeData = value; }
        }

        /// <summary>
        /// 通过状态名 获取对应状态的 通用动画融合配置
        /// note:
        ///     目前表中配置的是通用动画融合时间,
        /// </summary>
        /// <param name="StateName"></param>
        /// <returns></returns>
        public AnimancerEnterTimeDataCell GetAnimancerEnterTimeDataCell(string StateName)
        {
            foreach (var item in M_animEnterTimeData.StaticAnimancerEnterTimeDatas)
            {
                var dateCell = item.Value;
                bool isSame = dateCell.StateName.Equals(StateName, StringComparison.CurrentCultureIgnoreCase);
                if (isSame)
                {
                    return dateCell;
                }
            }

            return null;
        }

        #endregion

        #region 战斗文本信息表_DescText

        #region 角色文本信息表【JobSkillDesc】

        private JobSkillDescData m_JobSkillDescData;

        public JobSkillDescData M_JobSkillDescData
        {
            get
            {
                if (m_JobSkillDescData == null || m_JobSkillDescData.StaticJobSkillDescDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<JobSkillDescData>("JobSkillDesc", ref m_JobSkillDescData);
                }

                //缓存机制
                return m_JobSkillDescData;
            }
            set { m_JobSkillDescData = value; }
        }


        public Dictionary<int, Dictionary<int, JobSkillDescDataCell>> m_JobSkillDescDataDic;

        public Dictionary<int, Dictionary<int, JobSkillDescDataCell>> M_JobSkillDescDataDic
        {
            get
            {
                if (m_JobSkillDescDataDic == null || m_JobSkillDescDataDic.Count <= 0)
                {
                    m_JobSkillDescDataDic = new Dictionary<int, Dictionary<int, JobSkillDescDataCell>>();

                    List<KeyValuePair<int, JobSkillDescDataCell>> m_JobSkillDescDataList =
                        M_JobSkillDescData.StaticJobSkillDescDatas.KToList();

                    foreach (var item in m_JobSkillDescDataList)
                    {
                        JobSkillDescDataCell child = item.Value;
                        int skillId = child.GetSkillID();
                        int skillLevel = child.GetLevel();
                        if (!m_JobSkillDescDataDic.ContainsKey(skillId))
                        {
                            Dictionary<int, JobSkillDescDataCell> keyValuePairs =
                                new();
                            keyValuePairs.Add(skillLevel, child);
                            m_JobSkillDescDataDic.Add(skillId, keyValuePairs);
                        }
                        else
                        {
                            m_JobSkillDescDataDic[skillId].Add(skillLevel, child);
                        }
                    }
                }

                //缓存机制
                return m_JobSkillDescDataDic;
            }
        }

        public JobSkillDescDataCell GetJobSkillDescDataCell(int id)
        {
            JobSkillDescDataCell dataCell;
            if (M_JobSkillDescData.StaticJobSkillDescDatas.TryGetValue(id, out dataCell))
            {
                return dataCell;
            }

            return null;
        }

        public JobSkillDescDataCell GetJobSkillDescDataCellBySkillIdAndLevel(int skillId, int skillLevel)
        {
            JobSkillDescDataCell dataCell = null;

            if (M_JobSkillDescDataDic.ContainsKey(skillId))
            {
                if (M_JobSkillDescDataDic[skillId].ContainsKey(skillLevel))
                {
                    dataCell = M_JobSkillDescDataDic[skillId][skillLevel];
                }
            }

            return dataCell;
        }

        private SkillLevelData m_SkillLevelData;
        public SkillLevelData M_SkillLevelData
        {
            get
            {
                if (m_SkillLevelData == null || m_SkillLevelData.StaticSkillLevelDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SkillLevelData>("SkillLevel", ref m_SkillLevelData);
                }

                //缓存机制
                return m_SkillLevelData;
            }
            set { m_SkillLevelData = value; }
        }

        private DictionaryEx<int, DictionaryEx<int, SkillLevelDataCell>> skillLevels = new();
        public SkillLevelDataCell GetSkillLevelDataCell(int jobSkillID, int skillLevel)
        {
            if (skillLevels.Count == 0)
            {
                foreach (var item in M_SkillLevelData.StaticSkillLevelDatas)
                {
                    SkillLevelDataCell levelCfg = item.Value;
                    if (skillLevels[levelCfg.SkillID] == null)
                    {
                        skillLevels[levelCfg.SkillID] = new();
                    }
                    skillLevels[levelCfg.SkillID][levelCfg.Skill_Level] = levelCfg;
                }
            }

            if (skillLevels[jobSkillID] == null)
            {
                return null;
            }

            return skillLevels[jobSkillID][skillLevel];
        }

        #endregion

        #region 伙伴技能文本信息表【PartnerSkillDesc】

        private PartnerSkillDescData m_PartnerSkillDescData;

        public PartnerSkillDescData M_PartnerSkillDescData
        {
            get
            {
                if (m_PartnerSkillDescData == null || m_PartnerSkillDescData.StaticPartnerSkillDescDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerSkillDescData>("PartnerSkillDesc", ref m_PartnerSkillDescData);
                }

                //缓存机制
                return m_PartnerSkillDescData;
            }
            set { m_PartnerSkillDescData = value; }
        }


        public Dictionary<int, Dictionary<int, PartnerSkillDescDataCell>> m_PartnerSkillDescDataDic;

        public Dictionary<int, Dictionary<int, PartnerSkillDescDataCell>> M_PartnerSkillDescDataDic
        {
            get
            {
                if (m_PartnerSkillDescDataDic == null || m_PartnerSkillDescDataDic.Count <= 0)
                {
                    m_PartnerSkillDescDataDic = new Dictionary<int, Dictionary<int, PartnerSkillDescDataCell>>();

                    List<KeyValuePair<int, PartnerSkillDescDataCell>> m_PartnerSkillDescDataCellList =
                        M_PartnerSkillDescData.StaticPartnerSkillDescDatas.KToList();

                    foreach (var item in m_PartnerSkillDescDataCellList)
                    {
                        PartnerSkillDescDataCell child = item.Value;
                        int skillId = child.GetSkillID();
                        int skillLevel = child.GetLevel();
                        if (!m_PartnerSkillDescDataDic.ContainsKey(skillId))
                        {
                            Dictionary<int, PartnerSkillDescDataCell> keyValuePairs =
                                new();
                            keyValuePairs.Add(skillLevel, child);
                            m_PartnerSkillDescDataDic.Add(skillId, keyValuePairs);
                        }
                        else
                        {
                            m_PartnerSkillDescDataDic[skillId].Add(skillLevel, child);
                        }
                    }
                }

                //缓存机制
                return m_PartnerSkillDescDataDic;
            }
        }

        public PartnerSkillDescDataCell GetPartnerSkillDescDataCell(int id)
        {
            PartnerSkillDescDataCell dataCell;
            if (M_PartnerSkillDescData.StaticPartnerSkillDescDatas.TryGetValue(id, out dataCell))
            {
                return dataCell;
            }

            return null;
        }

        public PartnerSkillDescDataCell GetPartnerSkillDescDataCellBySkillIdAndLevel(int skillId, int skillLevel)
        {
            PartnerSkillDescDataCell dataCell = null;

            if (M_PartnerSkillDescDataDic.ContainsKey(skillId))
            {
                if (M_PartnerSkillDescDataDic[skillId].ContainsKey(skillLevel))
                {
                    dataCell = M_PartnerSkillDescDataDic[skillId][skillLevel];
                }
            }

            return dataCell;
        }

        #endregion

        #region Buff信息表【PartnerSkillDesc】

        private BuffDescData m_BuffDescData;

        public BuffDescData M_BuffDescData
        {
            get
            {
                if (m_BuffDescData == null || m_BuffDescData.StaticBuffDescDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<BuffDescData>("BuffDesc", ref m_BuffDescData);
                }

                //缓存机制
                return m_BuffDescData;
            }
            set { m_BuffDescData = value; }
        }


        public Dictionary<int, Dictionary<int, BuffDescDataCell>> m_BuffDescDataDic;

        public Dictionary<int, Dictionary<int, BuffDescDataCell>> M_BuffDescDataDic
        {
            get
            {
                if (m_BuffDescDataDic == null || m_BuffDescDataDic.Count <= 0)
                {
                    m_BuffDescDataDic = new Dictionary<int, Dictionary<int, BuffDescDataCell>>();

                    List<KeyValuePair<int, BuffDescDataCell>> m_BuffDescDataCellList =
                        M_BuffDescData.StaticBuffDescDatas.KToList();

                    foreach (var item in m_BuffDescDataCellList)
                    {
                        BuffDescDataCell child = item.Value;
                        int skillId = child.GetSkillID();
                        int skillLevel = child.GetLevel();
                        if (!m_BuffDescDataDic.ContainsKey(skillId))
                        {
                            Dictionary<int, BuffDescDataCell> keyValuePairs = new();
                            keyValuePairs.Add(skillLevel, child);
                            m_BuffDescDataDic.Add(skillId, keyValuePairs);
                        }
                        else
                        {
                            m_BuffDescDataDic[skillId].Add(skillLevel, child);
                        }
                    }
                }

                //缓存机制
                return m_BuffDescDataDic;
            }
        }

        public BuffDescDataCell GetBuffDescDataCell(int id)
        {
            BuffDescDataCell dataCell;
            if (M_BuffDescData.StaticBuffDescDatas.TryGetValue(id, out dataCell))
            {
                return dataCell;
            }

            return null;
        }

        public BuffDescDataCell GetBuffDescDataCellBySkillIdAndLevel(int skillId, int skillLevel)
        {
            BuffDescDataCell dataCell = null;

            if (M_BuffDescDataDic.ContainsKey(skillId))
            {
                if (M_BuffDescDataDic[skillId].ContainsKey(skillLevel))
                {
                    dataCell = M_BuffDescDataDic[skillId][skillLevel];
                }
            }

            return dataCell;
        }

        #endregion

        #region Buff信息表【PartnerSkillDesc】

        private PassiveDescData m_PassiveDescData;

        public PassiveDescData M_PassiveDescData
        {
            get
            {
                if (m_PassiveDescData == null || m_PassiveDescData.StaticPassiveDescDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PassiveDescData>("PassiveDesc", ref m_PassiveDescData);
                }

                //缓存机制
                return m_PassiveDescData;
            }
            set { m_PassiveDescData = value; }
        }


        public Dictionary<int, Dictionary<int, PassiveDescDataCell>> m_PassiveDescDataDic;

        public Dictionary<int, Dictionary<int, PassiveDescDataCell>> M_PassiveDescDataDic
        {
            get
            {
                if (m_PassiveDescDataDic == null || m_PassiveDescDataDic.Count <= 0)
                {
                    m_PassiveDescDataDic = new Dictionary<int, Dictionary<int, PassiveDescDataCell>>();

                    List<KeyValuePair<int, PassiveDescDataCell>> m_PassiveDescDataCellList =
                        M_PassiveDescData.StaticPassiveDescDatas.KToList();

                    foreach (var item in m_PassiveDescDataCellList)
                    {
                        PassiveDescDataCell child = item.Value;
                        int skillId = child.GetSkillID();
                        int skillLevel = child.GetLevel();
                        if (!m_PassiveDescDataDic.ContainsKey(skillId))
                        {
                            Dictionary<int, PassiveDescDataCell> keyValuePairs = new();
                            keyValuePairs.Add(skillLevel, child);
                            m_PassiveDescDataDic.Add(skillId, keyValuePairs);
                        }
                        else
                        {
                            m_PassiveDescDataDic[skillId].Add(skillLevel, child);
                        }
                    }
                }

                //缓存机制
                return m_PassiveDescDataDic;
            }
        }

        public PassiveDescDataCell GetPassiveDescDataCell(int id)
        {
            PassiveDescDataCell dataCell;
            if (M_PassiveDescData.StaticPassiveDescDatas.TryGetValue(id, out dataCell))
            {
                return dataCell;
            }

            return null;
        }

        public PassiveDescDataCell GetPassiveDescDataCellBySkillIdAndLevel(int skillId, int skillLevel)
        {
            PassiveDescDataCell dataCell = null;

            if (M_PassiveDescDataDic.ContainsKey(skillId))
            {
                if (M_PassiveDescDataDic[skillId].ContainsKey(skillLevel))
                {
                    dataCell = M_PassiveDescDataDic[skillId][skillLevel];
                }
            }

            return dataCell;
        }

        public int GetPassiveDescMaxLv(int skillId)
        {
            int maxLv = 1;
            foreach (var kv in M_PassiveDescData.StaticPassiveDescDatas)
            {
                var ese = kv.Value;
                if (ese.Level > maxLv && ese.SkillID == skillId)
                {
                    maxLv = ese.Level;
                }
            }
            return maxLv;
        }

        #endregion


        #region 职业技能描述

        #endregion

        #endregion

        #region TimelineConfig配置

        private TimelineConfigData m_TimelineConfigData;

        public TimelineConfigData M_TimelineConfigData
        {
            get
            {
                if (m_TimelineConfigData == null || m_TimelineConfigData.StaticTimelineConfigDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TimelineConfigData>("TimelineConfig", ref m_TimelineConfigData);
                }

                //缓存机制
                return m_TimelineConfigData;
            }
        }

        public TimelineConfigDataCell GetTimelineConfigDataCell(int timelineID)
        {
            if (M_TimelineConfigData.StaticTimelineConfigDatas.TryGetValue(timelineID, out var data) && data != null)
            {
                return data;
            }

            return null;
        }

        #endregion

        #region ui标签

        private UIConfigData m_UIConfigData;

        public UIConfigData M_UIConfigData
        {
            get
            {
                if (m_UIConfigData == null || m_UIConfigData.StaticUIConfigDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<UIConfigData>("UIConfig", ref m_UIConfigData);
                    InitUIConfigLabs();
                }

                //缓存机制
                return m_UIConfigData;
            }
            set { m_UIConfigData = value; }
        }

        public UIConfigDataCell GetUIConfigDataCell(int id)
        {
            UIConfigDataCell dataCell;
            if (M_UIConfigData.StaticUIConfigDatas.TryGetValue(id, out dataCell))
            {
                return dataCell;
            }

            return null;
        }

        /// <summary>
        /// UIConfig 类型 对应的 ---> Labs Set<int> 
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, HashSet<int>> UIConfigLabs = new();

        private void InitUIConfigLabs()
        {
            UIConfigData uiConfig = LocalDataManager.Instance.M_UIConfigData;
            foreach (KeyValuePair<int, UIConfigDataCell> item in uiConfig.StaticUIConfigDatas)
            {
                int key = item.Key;
                UIConfigDataCell uiConfigCell = item.Value;

                UIConfigLabs.Add(key, uiConfigCell.Labels.KToSet());
            }
        }

        /// <summary>
        /// 检查 uiConfigID 中 是否 配置了 对应的标签
        /// </summary>
        /// <param name="uiConfigID"></param>
        /// <param name="lab"></param>
        /// <returns></returns>
        public bool CheckUIConfigHasLab(int uiConfigID, int lab)
        {
            if (!UIConfigLabs.ContainsKey(uiConfigID))
            {
                return false;
            }

            return UIConfigLabs[uiConfigID].Contains(lab);
        }

        /// <summary>
        /// 标签 lab 对应的 ui 类型Set
        /// </summary>
        private Dictionary<int, HashSet<int>> lab2UIConfigs = new();

        /// <summary>
        /// 通过 标签 lab 获取到 与它相关 的 ui 类型 Set
        /// </summary>
        public HashSet<int> GetLabConfigs(int lab)
        {
            if (lab2UIConfigs.Count == 0)
            {
                UIConfigData uiConfig = LocalDataManager.Instance.M_UIConfigData;
                foreach (KeyValuePair<int, UIConfigDataCell> item in uiConfig.StaticUIConfigDatas)
                {
                    int key = item.Key;
                    UIConfigDataCell uiConfigCell = item.Value;
                    List<int> labs = uiConfigCell.Labels;

                    labs.ForEach((int uiLab) =>
                    {
                        if (!lab2UIConfigs.ContainsKey(uiLab))
                        {
                            lab2UIConfigs.Add(uiLab, new HashSet<int>());
                        }

                        HashSet<int> configs = lab2UIConfigs[uiLab];
                        configs.Add(key);
                    });
                }
            }

            if (!lab2UIConfigs.ContainsKey(lab))
            {
                return null;
            }

            return UIConfigLabs[lab];
        }

        /// <summary>
        /// 根据 labs 获取 int 类型的 ui类型set
        /// </summary>
        /// <param name="labs"></param>
        /// <returns></returns>
        public HashSet<int> GetLabsConfigs(List<int> labs)
        {
            HashSet<int> resultConfigs = new();

            labs.ForEach((lab) =>
            {
                HashSet<int> configs = GetLabConfigs(lab);
                if (configs == null || configs.Count == 0)
                {
                    return;
                }

                resultConfigs.UnionWith(configs);
            });
            return resultConfigs;
        }

        public HashSet<int> GetLabsConfigs(List<int> labs, CompOperator compOperator)
        {
            HashSet<int> resultConfigs = new();
            if (labs.Count == 0)
            {
                return resultConfigs;
            }

            HashSet<int> labsSet = labs.KToSet();
            switch (compOperator)
            {
                case CompOperator.CO_Equal:
                    break;
                case CompOperator.CO_NotEqual:
                    break;
                case CompOperator.CO_Greater:
                    break;
                case CompOperator.CO_Less:
                    break;
                case CompOperator.CO_EqualGreater:
                    break;
                case CompOperator.CO_EqualLess:
                    break;
                case CompOperator.CO_SET_SAME:
                    {
                        //集合相同
                        foreach (KeyValuePair<int, HashSet<int>> item in UIConfigLabs)
                        {
                            if (labsSet.IsSubsetOf(item.Value) && item.Value.IsSubsetOf(labsSet))
                            {
                                resultConfigs.Add(item.Key);
                            }
                        }
                    }
                    break;
                case CompOperator.CO_SET_INCLUDE:
                    {
                        //集合包含
                        // 就是 arr2 中 必须 完全包含 arr1 才可以
                        foreach (KeyValuePair<int, HashSet<int>> item in UIConfigLabs)
                        {
                            if (labsSet.IsSubsetOf(item.Value))
                            {
                                resultConfigs.Add(item.Key);
                            }
                        }
                    }
                    break;
                case CompOperator.CO_SET_UNINCLUDE:
                    {
                    }
                    break;
                case CompOperator.CO_SET_HAVE:
                    {
                        // 在 arr2 中 只要包含 一个 arr1 即 成功
                        // 此处 直接 找 uiLabls 中 所有 lab 相关的 ui 即可
                        resultConfigs = GetLabsConfigs(labs);
                    }
                    break;
                default:
                    break;
            }

            return resultConfigs;
        }

        /// <summary>
        /// 标签lab 对应的 复杂类型的 UI type
        /// 对于 技能按钮 这种UI
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, HashSet<string>> lab2ComplexUIConfigs = new();

        /// <summary>
        /// 注册 新的 复杂类型 ui类型 的 标签 labs
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="labs"></param>
        public void RegComplexUIConfigLabs(string configKey, List<int> labs)
        {
            labs.ForEach((int lab) =>
            {
                if (!lab2ComplexUIConfigs.ContainsKey(lab))
                {
                    lab2ComplexUIConfigs.Add(lab, new HashSet<string>());
                }

                HashSet<string> complexUIConfigLabs = lab2ComplexUIConfigs[lab];
                complexUIConfigLabs.Add(configKey);
            });
        }

        /// <summary>
        /// 取消 复杂类型 UI 注册的 labs
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="labs"></param>
        public void UnRegComplexUIConfigLabs(string configKey, List<int> labs)
        {
            labs.ForEach((int lab) =>
            {
                if (!lab2ComplexUIConfigs.ContainsKey(lab))
                {
                    return;
                }

                HashSet<string> complexUIConfigLabs = lab2ComplexUIConfigs[lab];
                complexUIConfigLabs.Remove(configKey);
            });
        }

        /// <summary>
        /// 获得 标签 lab 注册的 复杂类型 uiCfong 
        /// </summary>
        /// <param name="lab"></param>
        /// <returns></returns>
        public HashSet<string> GetLabComplexConfigs(int lab)
        {
            if (!lab2ComplexUIConfigs.ContainsKey(lab))
            {
                return null;
            }

            return lab2ComplexUIConfigs[lab];
        }

        /// <summary>
        /// 根据 labs 获取 int 类型的 ui类型set
        /// </summary>
        /// <param name="labs"></param>
        /// <returns></returns>
        public HashSet<string> GetLabsComplexConfigs(List<int> labs)
        {
            HashSet<string> resultConfigs = new();

            labs.ForEach((lab) =>
            {
                HashSet<string> configs = GetLabComplexConfigs(lab);
                if (configs == null || configs.Count == 0)
                {
                    return;
                }

                resultConfigs.UnionWith(configs);
            });
            return resultConfigs;
        }

        #endregion

        #region 公会配置

        #region 公会事件配置

        private GuildEventData m_GuildEventData;

        public GuildEventData M_GuildEventData
        {
            get
            {
                if (m_GuildEventData == null || m_GuildEventData.StaticGuildEventDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GuildEventData>("GuildEvent", ref m_GuildEventData);
                }

                //缓存机制
                return m_GuildEventData;
            }
        }

        public GuildEventDataCell GetGuildEventDataCell(int id)
        {
            if (M_GuildEventData.StaticGuildEventDatas.TryGetValue(id, out var data) && data != null)
            {
                return data;
            }

            return null;
        }

        #endregion

        #region 公会成员职位

        private GuildTitleData m_GuildTitleData;

        public GuildTitleData M_GuildTitleData
        {
            get
            {
                if (m_GuildTitleData == null || m_GuildTitleData.StaticGuildTitleDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GuildTitleData>("GuildTitle", ref m_GuildTitleData);
                }

                //缓存机制
                return m_GuildTitleData;
            }
        }

        public GuildTitleDataCell GetGuildTitleDataCell(int id)
        {
            if (M_GuildTitleData.StaticGuildTitleDatas.TryGetValue(id, out var data) && data != null)
            {
                return data;
            }

            return null;
        }

        #endregion

        #endregion

        #region 通缉玩法

        private TeamWantedData m_TeamWanted;

        public TeamWantedData M_TeamWanted
        {
            get
            {
                if (m_TeamWanted == null || m_TeamWanted.StaticTeamWantedDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TeamWantedData>("TeamWanted", ref m_TeamWanted);
                }

                //缓存机制
                return m_TeamWanted;
            }
            set { m_TeamWanted = value; }
        }

        public TeamWantedDataCell GetTeamWantedDataCell(int id)
        {
            TeamWantedDataCell cfg;
            if (!M_TeamWanted.StaticTeamWantedDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 玩法开启表

        private PlayOpenConditionsData m_PlayOpenConditions;

        public PlayOpenConditionsData M_PlayOpenConditions
        {
            get
            {
                if (m_PlayOpenConditions == null || m_PlayOpenConditions.StaticPlayOpenConditionsDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PlayOpenConditionsData>("PlayOpenConditions", ref m_PlayOpenConditions);
                }

                //缓存机制
                return m_PlayOpenConditions;
            }
            set { m_PlayOpenConditions = value; }
        }

        public PlayOpenConditionsDataCell GetPlayOpenConditionsDataCell(int id)
        {
            PlayOpenConditionsDataCell cfg;
            if (!M_PlayOpenConditions.StaticPlayOpenConditionsDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 新手引导开启表


        public GuidSysOpenDataCell GetGuidSysOpenDataCell(int id)
        {
            GuidSysOpenDataCell cfg;
            if (!M_GuidSysOpenData.StaticGuidSysOpenDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region 掉落显示表

        private DropShowData m_DropShow;

        public DropShowData M_DropShow
        {
            get
            {
                if (m_DropShow == null || m_DropShow.StaticDropShowDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<DropShowData>("DropShow", ref m_DropShow);
                }

                //缓存机制
                return m_DropShow;
            }
            set { m_DropShow = value; }
        }

        private Dictionary<int, List<DropShowDataCell>> m_DropShowDic = new();

        private void InitDropShowDic()
        {
            foreach (var item in M_DropShow.StaticDropShowDatas)
            {
                int indexID = item.Value.GetIndexID();
                if (m_DropShowDic.ContainsKey(indexID))
                {
                    m_DropShowDic[indexID].Add(item.Value);
                }
                else
                {
                    List<DropShowDataCell> list = new() { item.Value };
                    m_DropShowDic.Add(indexID, list);
                }
            }
        }

        public DropShowDataCell GetDropShowDataCell(int id)
        {
            DropShowDataCell cfg;
            if (!M_DropShow.StaticDropShowDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        public List<DropShowDataCell> GetDropShowList(int id)
        {
            if (m_DropShowDic == null || m_DropShowDic.Count <= 0)
            {
                InitDropShowDic();
            }
            if (m_DropShowDic.TryGetValue(id, out var data))
            {

            }
            return null;
        }

        #endregion

        #region 多语言表

        private LanguageCNData m_LanguageCNData;

        public LanguageCNData M_LanguageCNData
        {
            get
            {
                if (m_LanguageCNData == null || m_LanguageCNData.LanguageContentDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LanguageCNData>("LanguageCN", ref m_LanguageCNData);
                }

                //缓存机制
                return m_LanguageCNData;
            }
            set { m_LanguageCNData = value; }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 仅为编辑器多语言清cache服务
        /// </summary>
        [XLua.BlackList]
        public void ClearEditorLanguageCache()
        {
            //只在编辑器 编辑模式下生效
            if (!Application.isPlaying)
            {
                M_LanguageCNData = null;
                M_LanguageENData = null;
            }
        }
#endif

        /// <summary>
        /// 多语言key -> int 内部实现的质数值必须和导表工具同步，不允许单方面修改！
        /// </summary>
        /// <param name="languageKey"></param>
        /// <returns></returns>
        private int ComputeHash(string languageKey)
        {
            int hash = 397;
            foreach (char num in languageKey)
            {
                hash = unchecked(hash * 31);
                hash ^= num; // 使用异或运算
            }
            return hash;
        }

        public string GetCNLanguageVal(string key)
        {
            if (M_LanguageCNData != null)
            {
                int hashCode = ComputeHash(key);
                uint dataIndex;
                //先用hash找，找不到用字符串key找
                if (M_LanguageCNData.HashToDataIndexMap.TryGetValue(hashCode, out dataIndex))
                {
                    return M_LanguageCNData.LanguageContentDatas[(int)dataIndex];
                }
                else
                {
                    if (M_LanguageCNData.KeyToDataIndexMap.TryGetValue(key, out dataIndex))
                    {
                        return M_LanguageCNData.LanguageContentDatas[(int)dataIndex];
                    }
                }
            }

            return string.Empty;
        }

        private LanguageENData m_LanguageENData;

        public LanguageENData M_LanguageENData
        {
            get
            {
                if (m_LanguageENData == null || m_LanguageENData.LanguageContentDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LanguageENData>("LanguageEN", ref m_LanguageENData);
                }

                //缓存机制
                return m_LanguageENData;
            }
            set { m_LanguageENData = value; }
        }

        public string GetENLanguageVal(string key)
        {
            if (M_LanguageENData != null)
            {
                int hashCode = ComputeHash(key);
                uint dataIndex;
                //先用hash找，找不到用字符串key找
                if (M_LanguageENData.HashToDataIndexMap.TryGetValue(hashCode, out dataIndex))
                {
                    return M_LanguageENData.LanguageContentDatas[(int)dataIndex];
                }
                else
                {
                    if (M_LanguageENData.KeyToDataIndexMap.TryGetValue(key, out dataIndex))
                    {
                        return M_LanguageENData.LanguageContentDatas[(int)dataIndex];
                    }
                }
            }

            return string.Empty;
        }

        #endregion


        #region Loading切图

        private LoadingFuncData m_LoadingFuncData;

        public LoadingFuncData M_LoadingFuncData
        {
            get
            {
                if (m_LoadingFuncData == null || m_LoadingFuncData.StaticLoadingFuncDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LoadingFuncData>("LoadingFunc", ref m_LoadingFuncData);
                }

                //缓存机制
                return m_LoadingFuncData;
            }
            set { m_LoadingFuncData = value; }
        }

        public LoadingFuncDataCell GetLoadingFuncDataCell(int id)
        {
            if (M_LoadingFuncData == null)
            {
                return null;
            }

            LoadingFuncDataCell cfg = null;
            if (!M_LoadingFuncData.StaticLoadingFuncDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        public LoadingFuncDataCell GetLoadingFuncDataCell(int gamePlayID, int mapID)
        {
            if (M_LoadingFuncData == null)
            {
                return null;
            }

            foreach (var item in M_LoadingFuncData.StaticLoadingFuncDatas)
            {
                if (gamePlayID != 0 && item.Value.GetGamePlayID() == gamePlayID)
                {
                    return item.Value;
                }
                else if (item.Value.GetMapID() == mapID)
                {
                    return item.Value;
                }
            }

            return null;
        }

        public LoadingFuncDataCell GetLoadingFuncDataCell(SpaceType spaceType, int mapID)
        {
            if (M_LoadingFuncData == null)
            {
                return null;
            }
            int gamePlayID = (int)spaceType;
            if (spaceType == SpaceType.SpaceScene || spaceType == SpaceType.SpaceDefault)
            {
                gamePlayID = 0;
            }
            return GetLoadingFuncDataCell(gamePlayID, mapID);
        }

        #region loading图

        private LoadingGalleryData m_LoadingGalleryData;

        public LoadingGalleryData M_LoadingGalleryData
        {
            get
            {
                if (m_LoadingGalleryData == null || m_LoadingGalleryData.StaticLoadingGalleryDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LoadingGalleryData>("LoadingGallery", ref m_LoadingGalleryData);
                }

                //缓存机制
                return m_LoadingGalleryData;
            }
            set { m_LoadingGalleryData = value; }
        }

        private Dictionary<int, List<LoadingGalleryDataCell>> loadingGalleryDataDic = null;

        private void InitLoadingGalleryDataDicDic()
        {
            loadingGalleryDataDic = new();
            foreach (var item in M_LoadingGalleryData.StaticLoadingGalleryDatas)
            {
                if (loadingGalleryDataDic.ContainsKey(item.Value.GetGalleryID()))
                {
                    loadingGalleryDataDic[item.Value.GetGalleryID()].Add(item.Value);
                }
                else
                {
                    List<LoadingGalleryDataCell> list = new() { item.Value };
                    loadingGalleryDataDic.Add(item.Value.GetGalleryID(), list);
                }
            }
        }

        public List<LoadingGalleryDataCell> GetLoadingGalleryDataCellList(int id)
        {
            if (loadingGalleryDataDic == null)
            {
                InitLoadingGalleryDataDicDic();
            }

            List<LoadingGalleryDataCell> list = new();
            if (loadingGalleryDataDic.TryGetValue(id, out list))
            {

            }

            return list;
        }

        public LoadingGalleryDataCell GetLoadingGalleryDataCell(int id)
        {
            if (M_LoadingGalleryData == null)
            {
                return null;
            }

            LoadingGalleryDataCell cfg = null;
            if (!M_LoadingGalleryData.StaticLoadingGalleryDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #region loading文本

        private LoadingTipsData m_LoadingTipsData;

        public LoadingTipsData M_LoadingTipsData
        {
            get
            {
                if (m_LoadingTipsData == null || m_LoadingTipsData.StaticLoadingTipsDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LoadingTipsData>("LoadingTips", ref m_LoadingTipsData);
                }

                //缓存机制
                return m_LoadingTipsData;
            }
            set { m_LoadingTipsData = value; }
        }

        private Dictionary<int, List<LoadingTipsDataCell>> loadingTipsDataDic = null;

        private void InitLoadingTipsDataDic()
        {
            loadingTipsDataDic = new();
            foreach (var item in M_LoadingTipsData.StaticLoadingTipsDatas)
            {
                if (loadingTipsDataDic.ContainsKey(item.Value.GetTipsID()))
                {
                    loadingTipsDataDic[item.Value.GetTipsID()].Add(item.Value);
                }
                else
                {
                    List<LoadingTipsDataCell> list = new() { item.Value };
                    loadingTipsDataDic.Add(item.Value.GetTipsID(), list);
                }
            }
        }

        public List<LoadingTipsDataCell> GetLoadingTipsDataCellList(int id)
        {
            if (loadingTipsDataDic == null)
            {
                InitLoadingTipsDataDic();
            }

            List<LoadingTipsDataCell> list = new();
            if (loadingTipsDataDic.TryGetValue(id, out list))
            {

            }

            return list;
        }

        public LoadingTipsDataCell GetLoadingTipsDataCell(int id)
        {
            if (M_LoadingTipsData == null)
            {
                return null;
            }

            LoadingTipsDataCell cfg = null;
            if (!M_LoadingTipsData.StaticLoadingTipsDatas.TryGetValue(id, out cfg))
            {
                return null;
            }

            return cfg;
        }

        #endregion

        #endregion


        #endregion


        #region 新的技能配置

        private DictionaryEx<int, SkillJson> skillCfgs = new();
        private DictionaryEx<int, BuffJson> buffCfgs = new();
        private DictionaryEx<int, BulletJson> bulletCfgs = new();
        private DictionaryEx<int, PassiveJson> passiveCfgs = new();

        /// <summary>
        /// 加载 配置表的 泛型接口, 此处先用的 资源同步接口, 后面 跟博哥确定如何 采用他的 异步接口
        /// 【技能】
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <param name="callBack"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadCfgJson<T>(string jsonPath, Action<T> callBack) where T : class
        {
            Resource.ResourceFormalManager.Instance.LoadConfigAsync<T>(jsonPath, callBack);
        }

        [Obsolete("请使用 ", false)]//标记该方法已弃用
        /// <summary>
        /// 【同步接口】
        /// </summary>
       /* public T LoadCfgJsonSync<T>(string jsonPath) where T : class
        {
            return Resource.ResourceFormalManager.Instance.LoadConfig<T>(jsonPath);

        }*/

        private void CacheSkillCfgJson<T>(string path, int id, T jsonDOdata, SkillCfgType skillCfgType) where T : class
        {
            if (jsonDOdata == null)
            {
                return;
            }

            switch (skillCfgType)
            {
                case SkillCfgType.Skill:
                    {
                        SkillJson skill = jsonDOdata as SkillJson;
                        skillCfgs[id] = skill; //取后面的数字，遍历所有元素
                    }
                    break;
                case SkillCfgType.Buff:
                    {
                        BuffJson buff = jsonDOdata as BuffJson;
                        buffCfgs[id] = buff;
                    }
                    break;
                case SkillCfgType.Passive:
                    {
                        PassiveJson passive = jsonDOdata as PassiveJson;
                        passiveCfgs[id] = passive;
                    }
                    break;
                case SkillCfgType.Bullet:
                    {
                        BulletJson bullet = jsonDOdata as BulletJson;
                        bulletCfgs[id] = bullet;
                    }
                    break;
                case SkillCfgType.FxDetail:
                    {
                        fXDetailJson = jsonDOdata as FXDetailJson;
                    }
                    break;
                default:
                    SGF.Debuger.LogError($"[LocalDataManager] CacheSkillCfgJson no handle with path: {path}");
                    break;
            }
        }

        public void GetSkillJson(int skillID, Action<SkillJson> callBack, bool reload = false)
        {
            if (skillCfgs.ContainsKey(skillID) && !reload)
            {
                callBack?.Invoke(skillCfgs[skillID]);
                return;
            }

            string key = $"Skill_{skillID}";
            string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + key;

            LoadCfgJson<SkillJson>(jsonFilePath, (SkillJson jsonDOdata) =>
            {
                if (jsonDOdata == null)
                {
                    callBack?.Invoke(null);
                    return;
                }

                CacheSkillCfgJson(jsonFilePath, skillID, jsonDOdata, SkillCfgType.Skill);
                callBack?.Invoke(jsonDOdata);
            });
        }

        // 根据技能ID获取是否自动转向
        public bool GetIsAutoRotBySkillId(int skillID)
        {
            bool res = false;
            if (!skillCfgs.ContainsKey(skillID))
            {
                SGF.Debuger.LogWarning($"GetIsAutoRotBySkillId() skillID={skillID},not find err!!!");
                //string key = $"Skill_{skillID}";
                //string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + key;
                //SkillJson skillJson = LoadCfgJsonSync<SkillJson>(jsonFilePath);
                //if (skillJson != null)
                //{
                //    skillCfgs[skillID] = skillJson;
                //}
            }

            if (skillCfgs[skillID] != null)
            {
                res = skillCfgs[skillID].config.IsAutoTurnToTarget;
            }

            return res;
        }

        private FXDetailJson fXDetailJson;

        /// <summary>
        /// 特效 细节的 json配置
        /// </summary>
        /// <param name="callBack"></param>
        public void GetFxDetailJson(Action<FXDetailJson> callBack)
        {
            if (fXDetailJson != null)
            {
                callBack?.Invoke(fXDetailJson);
                return;
            }

            string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + "FxDetail";

            LoadCfgJson<FXDetailJson>(jsonFilePath, (FXDetailJson jsonDOdata) =>
            {
                if (jsonDOdata == null)
                {
                    callBack?.Invoke(null);
                    return;
                }

                CacheSkillCfgJson(jsonFilePath, -1, jsonDOdata, SkillCfgType.FxDetail);
                callBack?.Invoke(fXDetailJson);
            });
        }

        public bool CehckHasFxDetail(string path, E_AssetType e_AssetType)
        {
            bool result = false;
            GetFxDetailJson((FXDetailJson fXDetailJson) =>
            {
                result = fXDetailJson.fxFileDurationDic.ContainsKey(path);
            });
            return result;
        }

        public void GetBuffJson(int buffID, Action<BuffJson> callBack, bool reload = false)
        {
            if (buffCfgs.ContainsKey(buffID) && !reload)
            {
                callBack.Invoke(buffCfgs[buffID]);
                return;
            }

            string key = $"Buff_{buffID}";
            string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + key;

            LoadCfgJson<BuffJson>(jsonFilePath, (BuffJson jsonDOdata) =>
            {
                if (jsonDOdata == null)
                {
                    callBack?.Invoke(null);
                    return;
                }

                CacheSkillCfgJson(jsonFilePath, buffID, jsonDOdata, SkillCfgType.Buff);
                callBack?.Invoke(jsonDOdata);
            });
        }

        public void GetBulletJson(int bulletID, Action<BulletJson> callBack, bool reload = false)
        {
            if (bulletCfgs.ContainsKey(bulletID) && !reload)
            {
                callBack.Invoke(bulletCfgs[bulletID]);
                return;
            }

            string key = $"Bullet_{bulletID}";
            string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + key;

            LoadCfgJson<BulletJson>(jsonFilePath, (BulletJson jsonDOdata) =>
            {
                if (jsonDOdata == null)
                {
                    callBack?.Invoke(null);
                    return;
                }

                CacheSkillCfgJson(jsonFilePath, bulletID, jsonDOdata, SkillCfgType.Bullet);
                callBack?.Invoke(jsonDOdata);
            });
        }

        public void GetPassiveJson(int id, Action<PassiveJson> callBack, bool reload = false)
        {
            if (passiveCfgs.ContainsKey(id) && !reload)
            {
                callBack.Invoke(passiveCfgs[id]);
                return;
            }

            string key = $"Passive_{id}";
            string jsonFilePath = GameConfig.RES_CONFIG_JSON_PATH + key;

            LoadCfgJson<PassiveJson>(jsonFilePath, (PassiveJson jsonDOdata) =>
            {
                if (jsonDOdata == null)
                {
                    callBack?.Invoke(null);
                    return;
                }

                CacheSkillCfgJson(jsonFilePath, id, jsonDOdata, SkillCfgType.Passive);
                callBack?.Invoke(jsonDOdata);
            });
        }

        public SkillConfig GetSkillConfig(int id)
        {
            if (skillCfgs.ContainsKey(id))
            {
                return skillCfgs[id].config;
            }

            return null;
        }

        #endregion


        #region 玩法

        private RecyclePlayData m_RecyclePlayData;

        public RecyclePlayData M_RecyclePlayData
        {
            get
            {
                if (m_RecyclePlayData == null || m_RecyclePlayData.StaticRecyclePlayDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<RecyclePlayData>("RecyclePlay", ref m_RecyclePlayData);
                }

                //缓存机制
                return m_RecyclePlayData;
            }
            set { m_RecyclePlayData = value; }
        }

        public RecyclePlayDataCell GetRecyclePlayDataCell(long eID)
        {
            RecyclePlayDataCell RecyclePlayDataCell;
            if (M_RecyclePlayData.StaticRecyclePlayDatas.TryGetValue(eID, out RecyclePlayDataCell))
            {
                return RecyclePlayDataCell;
            }

            return null;
        }


        #endregion

        #region 纹章

        private HeraldryEquipData m_HeraldryEquipData;

        public HeraldryEquipData M_HeraldryEquipData
        {
            get
            {
                if (m_HeraldryEquipData == null || m_HeraldryEquipData.StaticHeraldryEquipDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<HeraldryEquipData>("HeraldryEquip", ref m_HeraldryEquipData);
                }

                //缓存机制
                return m_HeraldryEquipData;
            }
            set { m_HeraldryEquipData = value; }
        }

        public HeraldryEquipDataCell GetHeraldryEquipDataCell(long eID)
        {
            HeraldryEquipDataCell HeraldryEquipDataCell;
            if (M_HeraldryEquipData.StaticHeraldryEquipDatas.TryGetValue(eID, out HeraldryEquipDataCell))
            {
                return HeraldryEquipDataCell;
            }

            return null;
        }


        #endregion

        #region 护符技能







        private EtchUPData m_EtchUPData;

        public EtchUPData M_EtchUPData
        {
            get
            {
                if (m_EtchUPData == null || m_EtchUPData.StaticEtchUPDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EtchUPData>("EtchUP", ref m_EtchUPData);
                }

                //缓存机制
                return m_EtchUPData;
            }
            set { m_EtchUPData = value; }
        }

        public EtchUPDataCell GetEtchUPDataCell(int eID)
        {
            EtchUPDataCell etchUPDataCell;
            if (M_EtchUPData.StaticEtchUPDatas.TryGetValue(eID, out etchUPDataCell))
            {
                return etchUPDataCell;
            }

            return null;
        }



        #endregion

        #region 铭器







        private EtchData m_EtchData;

        public EtchData M_EtchData
        {
            get
            {
                if (m_EtchData == null || m_EtchData.StaticEtchDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<EtchData>("Etch", ref m_EtchData);
                }

                //缓存机制
                return m_EtchData;
            }
            set { m_EtchData = value; }
        }

        public EtchDataCell GetEtchDataCell(int eID)
        {
            EtchDataCell EtchDataCell;
            if (M_EtchData.StaticEtchDatas.TryGetValue(eID, out EtchDataCell))
            {
                return EtchDataCell;
            }

            return null;
        }



        #endregion


        #region 商业化

        private SignInData m_SignInData;

        public SignInData M_SignInData
        {
            get
            {
                if (m_SignInData == null || m_SignInData.StaticSignInDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SignInData>("SignIn", ref m_SignInData);
                }

                //缓存机制
                return m_SignInData;
            }
            set { m_SignInData = value; }
        }

        public SignInDataCell GetSignInDataCell(int nodeID)
        {
            SignInDataCell signInDataCellCell;
            if (M_SignInData.StaticSignInDatas.TryGetValue(nodeID, out signInDataCellCell))
            {
                return signInDataCellCell;
            }

            return null;
        }

        public int GetFirstPayMaxDay()
        {
            int num = 0;
            foreach (var kv in M_SignInData.StaticSignInDatas)
            {
                if (kv.Value.GetGroup() == 1 && kv.Value.GetSignInType() == 1)
                {
                    num = num + 1;
                }
            }
            return num;
        }


        #endregion

        #region 天赋树配置

        private AttriTTNodeData m_AttriTTNodeData;

        public AttriTTNodeData M_AttriTTNodeData
        {
            get
            {
                if (m_AttriTTNodeData == null || m_AttriTTNodeData.StaticAttriTTNodeDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AttriTTNodeData>("AttriTTNode", ref m_AttriTTNodeData);
                }

                //缓存机制
                return m_AttriTTNodeData;
            }
            set { m_AttriTTNodeData = value; }
        }

        public AttriTTNodeDataCell GetAttriTTNodeDataCell(int nodeID)
        {
            AttriTTNodeDataCell attriTTNodeDataCell;
            if (M_AttriTTNodeData.StaticAttriTTNodeDatas.TryGetValue(nodeID, out attriTTNodeDataCell))
            {
                return attriTTNodeDataCell;
            }

            return null;
        }

        #endregion


        #region 系统开启表

        private GuidSysOpenData m_GuidSysOpenData;

        public GuidSysOpenData M_GuidSysOpenData
        {
            get
            {
                if (m_GuidSysOpenData == null || m_GuidSysOpenData.StaticGuidSysOpenDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<GuidSysOpenData>("GuidSysOpen", ref m_GuidSysOpenData);
                }

                //缓存机制
                return m_GuidSysOpenData;
            }
        }

        #endregion

        #region Holiday 假期表, 目前 服务器 和 客户端根据这张表 判断是否是 节假日

        private HolidayData m_HolidayData;

        public HolidayData M_HolidayData
        {
            get
            {
                if (m_HolidayData == null || m_HolidayData.StaticHolidayDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<HolidayData>("Holiday", ref m_HolidayData);
                }

                //缓存机制
                return m_HolidayData;
            }
        }

        private DictionaryEx<string, bool> holidayMap = new();
        public bool IsHoliday(string day)
        {
            if (holidayMap.Count == 0)
            {
                M_HolidayData.StaticHolidayDatas.ForEach((item) =>
                {
                    holidayMap[item.Value.Date] = item.Value.GetHoliday();
                });
            }
            if (holidayMap.ContainsKey(day))
            {
                return holidayMap[day];
            }
            return false;
        }

        #endregion

        #region 引导配置

        //TutorialConfigList
        private TutorialConfigList m_TutorialConfig;

        public TutorialConfigList M_TutorialConfig
        {
            get
            {
                if (m_TutorialConfig == null || m_TutorialConfig.list.Count == 0)
                {
                    string path = "Config/Guide/TutorialConfig";
                    //m_TutorialConfig = LoadCfgJsonSync<TutorialConfigList>(path);
                    var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
                    if (jsonAsset != null)
                    {
                        m_TutorialConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<TutorialConfigList>(jsonAsset.text);
                        StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(path);
                    }
                }

                //缓存机制
                return m_TutorialConfig;
            }
        }

        #endregion

        #region 冒险等级
        private AdvGradeExpData m_AdvGradeExpData;

        public AdvGradeExpData M_AdvGradeExpData
        {
            get
            {
                if (m_AdvGradeExpData == null || m_AdvGradeExpData.StaticAdvGradeExpDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AdvGradeExpData>("AdvGradeExp", ref m_AdvGradeExpData);
                }

                //缓存机制
                return m_AdvGradeExpData;
            }
            set { m_AdvGradeExpData = value; }
        }

        public AdvGradeExpDataCell GetAdvGradeExpDataCell(int levelID)
        {
            AdvGradeExpDataCell advGradeExpDataCell;
            if (M_AdvGradeExpData.StaticAdvGradeExpDatas.TryGetValue(levelID, out advGradeExpDataCell))
            {
                return advGradeExpDataCell;
            }

            return null;
        }

        private long maxPower = 0;
        public long GetAdvGradeExpMaxPower()
        {
            if (maxPower > 0)
            {
                return maxPower;
            }


            foreach (var item in M_AdvGradeExpData.StaticAdvGradeExpDatas)
            {
                maxPower = Math.Max(maxPower, item.Value.Power);
            }

            return maxPower;
        }
        #endregion


        #region PV表
        private PvListData m_PvListData;

        public PvListData M_PvListData
        {
            get
            {
                if (m_PvListData == null || m_PvListData.StaticPvListDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PvListData>("PvList", ref m_PvListData);
                }

                //缓存机制
                return m_PvListData;
            }
            set { m_PvListData = value; }
        }

        public PvListDataCell GetPvListDataCell(int id)
        {
            PvListDataCell pvListDataCell;
            if (M_PvListData.StaticPvListDatas.TryGetValue(id, out pvListDataCell))
            {
                return pvListDataCell;
            }

            return null;
        }
        #endregion


        #region 护符

        private AmuletPointData m_AmuletPointData;

        public AmuletPointData M_AmuletPointData
        {
            get
            {
                if (m_AmuletPointData == null || m_AmuletPointData.StaticAmuletPointDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AmuletPointData>("AmuletPoint", ref m_AmuletPointData);
                }

                //缓存机制
                return m_AmuletPointData;
            }
            set { m_AmuletPointData = value; }
        }



        public int GetMaxPolishNum(int rarity)
        {
            int maxNum = 0;

            foreach (var kv in M_AmuletPointData.StaticAmuletPointDatas)
            {
                var v = kv.Value;
                if (v.GetAmuletLevel() == 1 && v.GetAmuletQuality() == rarity && v.GetPoint() > maxNum)
                {
                    maxNum = v.GetPoint();
                }
            }
            return maxNum;
        }

        #endregion


        #region 玩家等级
        private LevelExpData m_LevelExpData;
        private LevelExpData M_LevelExpData
        {
            get
            {
                if (m_LevelExpData == null || m_LevelExpData.StaticLevelExpDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LevelExpData>("LevelExp", ref m_LevelExpData);
                }

                //缓存机制
                return m_LevelExpData;
            }
        }

        public LevelExpDataCell GetLevelExpDataCell(int level)
        {
            if (M_LevelExpData.StaticLevelExpDatas.TryGetValue(level, out var v))
            {
                return v;
            }

            return null;
        }

        private long levelExpMaxPower = 0;

        public long GetLevelExpMaxPower()
        {
            if (levelExpMaxPower == 0)
            {
                foreach (var item in M_LevelExpData.StaticLevelExpDatas)
                {
                    levelExpMaxPower = Math.Max(levelExpMaxPower, item.Value.Power);
                }
            }

            return levelExpMaxPower;
        }


        #endregion

        #region Power 战力相关表
        private PowerPartData m_PowerModuleData;
        private PowerPartData M_PowerModuleData
        {
            get
            {
                if (m_PowerModuleData == null || m_PowerModuleData.StaticPowerPartDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PowerPartData>("PowerPart", ref m_PowerModuleData);
                }

                //缓存机制
                return m_PowerModuleData;
            }
        }

        private PartUpperLimitData m_ModuleUpperLimitData;
        private PartUpperLimitData M_ModuleUpperLimitData
        {
            get
            {
                if (m_ModuleUpperLimitData == null || m_ModuleUpperLimitData.StaticPartUpperLimitDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartUpperLimitData>("PartUpperLimit", ref m_ModuleUpperLimitData);
                }

                //缓存机制
                return m_ModuleUpperLimitData;
            }
        }

        private PartEvalueData m_ModuleEvalueData;
        private PartEvalueData M_ModuleEvalueData
        {
            get
            {
                if (m_ModuleEvalueData == null || m_ModuleEvalueData.StaticPartEvalueDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartEvalueData>("PartEvalue", ref m_ModuleEvalueData);
                }

                //缓存机制
                return m_ModuleEvalueData;
            }
        }

        private TotalPowerRankData m_TotalPowerRankData;
        private TotalPowerRankData M_TotalPowerRankData
        {
            get
            {
                if (m_TotalPowerRankData == null || m_TotalPowerRankData.StaticTotalPowerRankDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TotalPowerRankData>("TotalPowerRank", ref m_TotalPowerRankData);
                }

                //缓存机制
                return m_TotalPowerRankData;
            }
        }

        private RankResData m_RankResData;
        private RankResData M_RankResData
        {
            get
            {
                if (m_RankResData == null || m_RankResData.StaticRankResDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<RankResData>("RankRes", ref m_RankResData);
                }

                //缓存机制
                return m_RankResData;
            }

        }


        private Dictionary<int, List<PowerPartDataCell>> BigModules = new();
        private Dictionary<int, List<PartUpperLimitDataCell>> MoudleID2LimitPower = new();
        private Dictionary<int, List<PartEvalueDataCell>> MoudleID2EvalueCell = new();

        public void InitPowerModules()
        {
            foreach (var item in M_PowerModuleData.StaticPowerPartDatas)
            {
                var cfg = item.Value;
                int modelID = cfg.ID;
                int bigModuleID = cfg.FromPart;

                // 如果是0, 那就是 大模块
                if (bigModuleID == 0)
                {
                    BigModules.Add(modelID, new());
                }
                else
                {
                    // 如果是小模块, 那就 把小模块添加进大模块数据中
                    if (!BigModules.TryGetValue(bigModuleID, out var itemModules))
                    {
                        itemModules = new();
                        BigModules.Add(modelID, itemModules);
                    }

                    itemModules.Add(cfg);
                }
            }

            foreach (var item in M_ModuleUpperLimitData.StaticPartUpperLimitDatas)
            {
                var cfg = item.Value;
                var moduleID = cfg.PartID;
                if (!MoudleID2LimitPower.TryGetValue(moduleID, out var limitDataCells))
                {
                    limitDataCells = new();
                    MoudleID2LimitPower.Add(moduleID, limitDataCells);
                }

                limitDataCells.Add(cfg);
            }

            foreach (var item in M_ModuleEvalueData.StaticPartEvalueDatas)
            {
                var cfg = item.Value;
                var moduleID = cfg.PartID;

                if (!MoudleID2EvalueCell.TryGetValue(moduleID, out var evalueCells))
                {
                    evalueCells = new();
                    MoudleID2EvalueCell.Add(moduleID, evalueCells);
                }

                evalueCells.Add(cfg);
            }

        }

        /// <summary>
        /// 获得 模块对应的配置
        /// </summary>
        /// <param name="moduleID"></param>
        public PowerPartDataCell GetPowerModuleDataCell(int moduleID)
        {
            if (M_PowerModuleData.StaticPowerPartDatas.TryGetValue(moduleID, out var cfg))
            {
                return cfg;
            }

            return null;
        }

        /// <summary>
        /// 获得大的模块 id 对应的 总模块战力和上限
        /// </summary>
        /// <param name="bigModuleID">大的模块ID</param>
        /// <param name="openDay">开服天数</param>
        public long GetBigModuleUpperLimetPower(int bigModuleID, int openDay)
        {
            if (!BigModules.ContainsKey(bigModuleID))
            {
                return 0;
            }

            long power = 0;
            foreach (var item in BigModules[bigModuleID])
            {
                power += GetModuleUpperLimitPower(item.ID, openDay);
            }

            return power;
        }


        /// <summary>
        /// 获得 模块 对应的战力上限
        /// </summary>
        /// <param name="moduleID"></param>
        public long GetModuleUpperLimitPower(int moduleID, int openDay)
        {
            if (!MoudleID2LimitPower.ContainsKey(moduleID))
            {
                return 0;
            }

            PartUpperLimitDataCell cfg = null;
            for (int i = 0; i < MoudleID2LimitPower[moduleID].Count; i++)
            {

                if (openDay < MoudleID2LimitPower[moduleID][i].OpenDay)
                {
                    break;
                }
                cfg = MoudleID2LimitPower[moduleID][i];
            }
            // 找不到 就找第一个
            if (cfg == null && MoudleID2LimitPower[moduleID].Count > 0)
            {
                cfg = MoudleID2LimitPower[moduleID][0];
            }

            return cfg.UpperLimitPower;
        }

        /// <summary>
        /// 得到所有模块 当前开服天数所对应的 极限总战力
        /// </summary>
        /// <param name="openDay"></param>
        public long GetAllModulesTotalUpperLimitPower(int openDay)
        {
            long power = 0;
            foreach (var item in MoudleID2LimitPower)
            {
                var moduleID = item.Key;
                var modulePower = GetModuleUpperLimitPower(moduleID, openDay);

                power += modulePower;
            }

            return power;
        }

        /// <summary>
        /// 得到 moduleId 成长百分比 对于的 等级的配置
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="percent"></param>
        public PartEvalueDataCell GetModuleIDEvalue(int moduleID, int percent)
        {
            if (!MoudleID2EvalueCell.ContainsKey(moduleID))
            {
                return null;
            }

            PartEvalueDataCell cfg = null;
            for (int i = 0; i < MoudleID2EvalueCell[moduleID].Count; i++)
            {

                if (percent < MoudleID2EvalueCell[moduleID][i].Percent)
                {
                    break;
                }
                cfg = MoudleID2EvalueCell[moduleID][i];
            }

            if (cfg == null && MoudleID2EvalueCell[moduleID].Count > 0)
            {
                cfg = MoudleID2EvalueCell[moduleID][0];
            }

            return cfg;
        }

        /// <summary>
        /// 得到总战力对于的段位评分配置
        /// </summary>
        /// <param name="totalPower"></param>
        /// <param name="openDay"></param>
        public TotalPowerRankDataCell GetTotalPowerRankDataCell(long totalPower, int openDay)
        {
            TotalPowerRankDataCell cfg = null;
            foreach (var item in M_TotalPowerRankData.StaticTotalPowerRankDatas)
            {
                // 如果开服天数小于 配置的生效天数, 那就直接采用之前的 配置
                if (openDay < item.Value.OpenDay)
                {
                    break;
                }
                cfg = item.Value;
                // 当前的总分数 比 配置表的分数小, 那就是这一个段位
                if (totalPower <= cfg.TotalPower)
                {
                    break;
                }

            }

            return cfg;
        }

        private List<TotalPowerRankDataCell> totalPowerRankDataCells = new();

        /// <summary>
        /// 获取一个 当前开服天数下 对应的 战力等级队列
        /// </summary>
        /// <param name="openDay"></param>
        public List<TotalPowerRankDataCell> GetTotalPowerRankDataCells(int openDay)
        {
            if (totalPowerRankDataCells.Count == 0)
            {
                TotalPowerRankDataCell cfg = null;
                Dictionary<int, TotalPowerRankDataCell> rank2cfg = new();


                foreach (var item in M_TotalPowerRankData.StaticTotalPowerRankDatas)
                {
                    var key = (item.Value.ParentRank * 10) + item.Value.SubRank;

                    cfg = item.Value;
                    // 第一个没有的话，加入
                    if (!rank2cfg.ContainsKey(key))
                    {
                        rank2cfg.Add(key, cfg);
                    }
                    else
                    {
                        // 如果开服天数小于 配置的生效天数, 那就直接采用之前的 配置
                        if (openDay < item.Value.OpenDay)
                        {
                            rank2cfg[key] = cfg;
                            continue;
                        }
                    }
                }

                foreach (var item in rank2cfg)
                {
                    totalPowerRankDataCells.Add(item.Value);
                }
            }

            return totalPowerRankDataCells;
        }


        public TotalPowerRankDataCell GetTotalPowerRankDataCellByID(int id)
        {
            if (M_TotalPowerRankData.StaticTotalPowerRankDatas.TryGetValue(id, out var cfg))
            {
                return cfg;
            }

            return null;

        }

        /// <summary>
        /// 得到 段位 对应的 战力段位配置表
        /// </summary>
        /// <param name="parentRank"></param>
        /// <param name="subRank"></param>
        /// <returns></returns>
        public TotalPowerRankDataCell GetTotalPowerRankDataCellByRank(int parentRank, int subRank)
        {
            foreach (var item in M_TotalPowerRankData.StaticTotalPowerRankDatas)
            {
                var cfg = item.Value;
                if (cfg.ParentRank == parentRank && cfg.SubRank == subRank)
                {
                    return cfg;
                }
            }
            return null;
        }

        public TotalPowerRankDataCell GetNextTotalPowerRankDataCellByID(int curId)
        {

            if (M_TotalPowerRankData.StaticTotalPowerRankDatas.TryGetValue(curId, out var curCfg))
            {
                // 当前大段位ID
                int curParentRankID = curCfg.ParentRank;
                // 当前子段位ID
                int curSubRankID = curCfg.SubRank;

                // 找下一个 字段为对应的 战力段位配置
                var nextCfg = GetTotalPowerRankDataCellByRank(curParentRankID, curSubRankID + 1);

                if (nextCfg != null)
                {
                    return nextCfg;
                }

                // 如果 子段位配置找不到,找 大段位+1 的 配置
                nextCfg = GetTotalPowerRankDataCellByRank(curParentRankID + 1, 1);

                return nextCfg;
            }

            return null;

        }

        public RankResDataCell GetRankResDataCell(int rankLevel)
        {
            if (M_RankResData.StaticRankResDatas.TryGetValue(rankLevel, out var cfg))
            {
                return cfg;
            }

            return null;
        }

        /// <summary>
        /// 得到战力 对应的 战力等级相关的配置
        /// </summary>
        /// <param name="power"></param>
        /// <param name="openDay">开服天数</param>
        /// <param name="totalPowerRankDataCell">总站力对应的 段位阶段奖励相关配置</param>
        /// <param name="rankResDataCell">总站力对应的 大段位 段位名称相关的配置</param>
        public void GetPowerRankLevelCfg(long power, int openDay, out TotalPowerRankDataCell totalPowerRankDataCell, out RankResDataCell rankResDataCell)
        {

            totalPowerRankDataCell = LocalDataManager.Instance.GetTotalPowerRankDataCell(power, openDay);
            rankResDataCell = LocalDataManager.Instance.GetRankResDataCell(totalPowerRankDataCell.ParentRank);
        }


        /// <summary>
        /// 获得战力 对应的奖励配置
        /// </summary>
        /// <param name="power"></param>
        /// <param name="openDay"></param>
        /// <returns></returns>
        public TotalPowerRankDataCell GetPowerRewardRankLevelCfg(long power, int openDay)
        {

            TotalPowerRankDataCell cfg = null;
            foreach (var item in M_TotalPowerRankData.StaticTotalPowerRankDatas)
            {
                // 如果开服天数小于 配置的生效天数, 那就直接采用之前的 配置
                if (openDay < item.Value.OpenDay)
                {
                    break;
                }

                // 当前的总分数 比 配置表的分数小, 那就是这一个段位
                if (power < item.Value.TotalPower)
                {
                    break;
                }
                cfg = item.Value;

            }

            return cfg;

        }
        #endregion


        private PartnerAssistSlotData m_PartnerAssistSlotData;

        public PartnerAssistSlotData M_PartnerAssistSlotData
        {
            get
            {
                if (m_PartnerAssistSlotData == null || m_PartnerAssistSlotData.StaticPartnerAssistSlotDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<PartnerAssistSlotData>("PartnerAssistSlot", ref m_PartnerAssistSlotData);
                }

                //缓存机制
                return m_PartnerAssistSlotData;
            }
        }

        public int GetPartnerAssistSlotCount(uint job)
        {
            int count = 0;
            foreach (var item in M_PartnerAssistSlotData.StaticPartnerAssistSlotDatas)
            {
                if (item.Value.GetJob_id() == job)
                {
                    var islocked = GameManager.Instance.IsConditionMete(item.Value.GetCondition());
                    if (islocked)
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        private TimelineDialogData m_TimelineDialogData;

        public TimelineDialogData M_TimelineDialogData
        {
            get
            {
                if (m_TimelineDialogData == null || m_TimelineDialogData.StaticTimelineDialogDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TimelineDialogData>("TimelineDialog", ref m_TimelineDialogData);
                }

                //缓存机制
                return m_TimelineDialogData;
            }
        }

        public TimelineDialogDataCell GetTimelineDialogDataCell(int id)
        {
            if (M_TimelineDialogData != null && M_TimelineDialogData.StaticTimelineDialogDatas.Count > 0)
            {
                if (M_TimelineDialogData.StaticTimelineDialogDatas.TryGetValue(id, out var dataCell))
                {
                    return dataCell;
                }
            }

            return null;
        }


        private AssistData m_AssistData;

        public AssistData M_AssistData
        {
            get
            {
                if (m_AssistData == null || m_AssistData.StaticAssistDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<AssistData>("Assist", ref m_AssistData);

                }

                //缓存机制
                return m_AssistData;
            }
        }

        /// <summary>
        /// 伙伴助战槽位配置列表
        /// </summary>
        /// <param name="job"></param>
        /// <param name="slot"></param>
        public List<AssistDataCell> GetAssistDataCells(int job, int slot)
        {
            List<AssistDataCell> cfgs = new();

            foreach (var item in M_AssistData.StaticAssistDatas)
            {
                var cfg = item.Value;
                if (cfg.Job_id == job && cfg.Slot == slot)
                {
                    cfgs.Add(item.Value);
                }
            }

            return cfgs;
        }

        public string ReadFromJson(string jsonName, string tableName, int id)
        {
            //var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync($"Config/Excel/{jsonName}");
            //if (jsonAsset != null)
            //{
            //    JObject jObject = JObject.Parse(jsonAsset.text);
            //    var root = (JObject)jObject[$"Static{jsonName}Datas"];
            //    if (root != null && root.ContainsKey(id.ToString()))
            //    {
            //        var p = root.Property(id.ToString());
            //        if (p != null && p.Value != null)
            //        {
            //            return p.Value[tableName].ToString();
            //        }

            //    }
            //}

            Type type = LocalDataManager.Instance.GetType();
            string methodName = $"Get{jsonName}DataCell";
            MethodInfo method = type.GetMethod(methodName);
            if (method != null)
            {
                var result = method.Invoke(LocalDataManager.Instance, new object[] { id });
                if (result != null)
                {
                    Type resultType = result.GetType();
                    FieldInfo field = resultType.GetField(tableName, BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        return field.GetValue(result).ToString();
                    }
                }
            }

            return string.Empty;
        }


        private SystemJumpData m_SystemJumpData;

        public SystemJumpData M_SystemJumpData
        {
            get
            {
                if (m_SystemJumpData == null || m_SystemJumpData.StaticSystemJumpDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<SystemJumpData>("SystemJump", ref m_SystemJumpData);
                }

                //缓存机制
                return m_SystemJumpData;
            }
        }

        private RankCommonData m_RankCommonData;

        public RankCommonData M_RankCommonData
        {
            get
            {
                if (m_RankCommonData == null || m_RankCommonData.StaticRankCommonDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<RankCommonData>("RankCommon", ref m_RankCommonData);
                }

                //缓存机制
                return m_RankCommonData;
            }
            set { m_RankCommonData = value; }
        }

        public RankCommonDataCell GetRankCommonDataCell(int id)
        {
            if (M_RankCommonData.StaticRankCommonDatas.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }

        public SystemJumpDataCell GetSystemJumpDataCell(int id)
        {
            if (M_SystemJumpData.StaticSystemJumpDatas.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }


        private NoticeData m_NoticeData;

        public NoticeData M_NoticeData
        {
            get
            {
                if (m_NoticeData == null || m_NoticeData.StaticNoticeDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<NoticeData>("Notice", ref m_NoticeData);
                }

                //缓存机制
                return m_NoticeData;
            }
            set { m_NoticeData = value; }
        }

        public NoticeDataCell GetNoticeDataCell(int id)
        {
            if (M_NoticeData.StaticNoticeDatas.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }

        private TrackPlayData m_TrackPlayData;

        public TrackPlayData M_TrackPlayData
        {
            get
            {
                if (m_TrackPlayData == null || m_TrackPlayData.StaticTrackPlayDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<TrackPlayData>("TrackPlay", ref m_TrackPlayData);
                }
                return m_TrackPlayData;
            }
        }

        private FirstNameData m_FirstNamePack;

        public FirstNameData FirstName
        {
            get
            {
                if (m_FirstNamePack == null || m_FirstNamePack.StaticFirstNameDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<FirstNameData>("FirstName", ref m_FirstNamePack);
                }

                //缓存机制
                return m_FirstNamePack;
            }
        }

        private LastNameData m_LastName;

        public LastNameData LastName
        {
            get
            {
                if (m_LastName == null || m_LastName.StaticLastNameDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<LastNameData>("LastName", ref m_LastName);
                }

                //缓存机制
                return m_LastName;
            }
        }

        private ResourcePackData m_PackData;

        public ResourcePackData PackData
        {
            get
            {
                if (m_PackData == null || m_PackData.StaticResourcePackDatas.Count == 0)
                {
                    LoadNMapDatasNewtonAsync<ResourcePackData>("ResourcePack", ref m_PackData);
                }

                //缓存机制
                return m_PackData;
            }
        }

        public enum Platform
        {
            Windows,
            MacOSX,
            iOS,
            tvOS,
            Android,
            WindowsPhone,
            WindowsUWP,
            WebGL,
            PS4,
            Count = 9,
            Unknown = 100,
        }
    }
}