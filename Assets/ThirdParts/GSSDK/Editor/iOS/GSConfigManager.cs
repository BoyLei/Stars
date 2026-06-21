using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace GSSDKEditor
{
    internal class GSConfigManager
    {

        private static Dictionary<string, object> localConfig;
        internal static Dictionary<string, object> LocalConfig
        {
            get
            {
                if (localConfig == null)
                {
                    StreamReader reader = null;
                    string str = null;
                    try
                    {
                        string path;
#if UNITY_ANDROID
                        path = Path.Combine(Application.dataPath, GSPathName.SDK_CHANNELS, "Resources/Android", GSFileName.SDK_CONFIG_JSON);
#else
                        path = Path.Combine(Application.dataPath, GSPathName.SDK_CHANNELS, "Resources/iOS", GSFileName.SDK_CONFIG_JSON);
#endif
                        if (File.Exists(path))
                        {
                            reader = new StreamReader(path);
                            str = reader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                    reader?.Close();
                }
                return localConfig;
            }
        }





        internal static object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            Dictionary<string, object> dictionary = LocalConfig;

            if (dictionary == null || !dictionary.ContainsKey(key))
            {
                return null;
            }

            return dictionary[key];
        }

        internal static object GetValue(string module, string key)
        {
            if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(key))
            {
                return null;
            }

            Dictionary<string, object> dictionary = LocalConfig;

            if (dictionary == null || !dictionary.ContainsKey(module))
            {
                return null;
            }

            Dictionary<string, object> mdictionary = dictionary[module] as Dictionary<string, object>;

            if (mdictionary == null || !mdictionary.ContainsKey(key))
            {
                return null;
            }

            return mdictionary[key];
        }
    }

    internal class GSPathName
    {
        public const string SDK_CHANNELS = "GSSDK/Channels";

        public const string SDK_XPLUGINS = "GSSDK/Plugins";
    }

    internal class GSFileName
    {
        public const string SDK_CONFIG_JSON = "sdkconfig.json";

        public const string GOOGLE_SERVICE_INFO_PLIST = "GoogleService-Info.plist";

        public const string GOOGLE_SERVICE_JSON = "google-services.json";

        public const string ANALYTICS_EVENT_JSON = "AnalyticsEvent.json";

        public const string MCHANNEL_INFOS_PLIST = "mchannelinfos.plist";

        public const string YKHW_RESOURSE_BUNDLE = "YKHWResourse.bundle";
    }
}
