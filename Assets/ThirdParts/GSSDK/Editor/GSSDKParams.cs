using UnityEngine;
using UnityEditor;

namespace GSSDKEditor
{
    public class GSSDKParams : ScriptableObject
    {
        public const string SDKConfigFileName = "sdk_config.properties";

        public static string AssetPath
        {
            get
            {
                return "Assets/GSSDK/Editor/Params/GSSDKParams.asset";
            }
        }

        public static string ConfigPath
        {
            get
            {
                return Application.streamingAssetsPath + "/" + SDKConfigFileName;
            }
        }

        public const string SettingsWindowPath = "Project/GS SDK";

        [SerializeField]
        private string m_appId = "";

        [SerializeField]
        private string m_appKey = "";

        [SerializeField]
        private string m_bugly_appId = "";

        [SerializeField]
        private bool m_isNewLogin = false;

        public string AppID { get { return m_appId; } set { SetAppId(value); } }

        public string AppKey { get { return m_appKey; } set { SetAppKey(value); } }

        public string BuglyAppID { get { return m_bugly_appId; } set { SetBuyAppID(value); } }

        public bool IsNewLogin { get { return m_isNewLogin; } set { SetIsNewLogin(value); } }

        public void SetAppId(string strValue)
        {
            if (m_appId != strValue)
            {
                m_appId = strValue;

                Save();
            }
        }

        public void SetBuyAppID(string strValue)
        {
            if (m_bugly_appId != strValue)
            {
                m_bugly_appId = strValue;

                Save();
            }
        }

        public void SetAppKey(string strValue)
        {
            if (m_appKey != strValue)
            {
                m_appKey = strValue;

                Save();
            }
        }

        public void SetIsNewLogin(bool isNewLogin)
        {
            if (m_isNewLogin != isNewLogin)
            {
                m_isNewLogin = isNewLogin;

                Save();
            }
        }

        void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

    }
}
