using ProtoBuf;
using SGF;
using StarProject.Service.UserManager.Data;
using UnityEngine;

/// <summary>
/// 软件相关的配置
/// </summary>
namespace StarProject
{
    /// <summary>
    /// App的配置定义
    /// </summary>
    [ProtoContract]
    public class AppConfig
    {
        /// <summary> 
        /// 主用户数据
        /// </summary>
        [ProtoMember(1)] public UserData mainUserData = new UserData();
        [ProtoMember(2)] public bool enableBgMusic = true;
        [ProtoMember(3)] public bool enableSoundEffect = true;

        public static bool IsFirstLogin = true;

        //============================================================================
        private static AppConfig m_Value = new AppConfig();
        public static AppConfig Value { get { return m_Value; } }

        /// <summary>
        /// gm 操作时,存储的一份 数据
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public static DictionaryEx<string, string> GMData = new DictionaryEx<string, string>();

        private static string _Path;

        public static string Path
        {
            get
            {
                if (string.IsNullOrEmpty(_Path))
                {
#if UNITY_EDITOR
                    _Path = Application.persistentDataPath + "/AppConfig_Editor.data";
#else
                    _Path = Application.persistentDataPath + "/AppConfig.data";
#endif
                }
                return _Path;
            }
        }
        public static void Init()
        {
            Debuger.Log("AppConfig", "Init() Path = " + Path);

            byte[] data = FileUtils.ReadFile(Path);
            if (data != null && data.Length > 0)
            {
                AppConfig cfg = (AppConfig)PBSerializer.NDeserialize(data, typeof(AppConfig));
                if (cfg != null)
                {
                    m_Value = cfg;
                }
            }
        }

        public static void Save()
        {
            Debuger.LogWarning("AppConfig", "Save() Value = " + Value);

            if (Value != null)
            {

                byte[] data = PBSerializer.NSerialize(Value);
                FileUtils.SaveFile(Path, data);
            }
        }
        /// <summary>
        /// 开发过程测试，静态参数，看打包标记
        /// </summary>
        /// <returns></returns>
        public static bool IsDev()
        {
#if STAR_DEV
            return true;// Dev 是开 Log，是测试版本，让本地测试包可以log信息调试
#else
       return false;//Release 是开 上报，是上线了不打log，读写不在本地
#endif
        }

        public static bool IsGM()
        {
#if GM
            return true;
#else
       return false;
#endif
        }
        public static bool IsRelease()
        {
#if STAR_RELEASE
            return true;
#else
            return false;
#endif
        }


        public static bool IsHotfix()
        {
            return false;
        }

#if UNITY_EDITOR
        private static System.Collections.Generic.List<string> languageEditorNameList = new System.Collections.Generic.List<string>() { "(TaskWindow)", "(TutorialWindow)", "(ExplorerWindow)", "(MapEditor)" };
        [XLua.BlackList]
        public static bool IsLanguageEditorOpen() 
        {
            var allWindows = Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>();
            foreach (var window in allWindows)
            {
                if (languageEditorNameList.Contains(window.ToString().Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        [XLua.BlackList]
        public static bool IsLanguageEditorOpen(string editWindow)
        {
            var allWindows = Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>();
            foreach (var window in allWindows)
            {
                if (editWindow.Contains(window.ToString().Trim()))
                {
                    if (languageEditorNameList.Contains(window.ToString().Trim()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#endif
        public static int IsEditConfig = 0;


    }


}
