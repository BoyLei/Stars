using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSSDK
{
    public class GSUnity
    {

        public static void Init(Dictionary<string, object> data, Action<string, string, string> callbcak)
        {
            GSPlatform.GetInstance().SetGSPlatform(GetSDKPlatform());

            GSSDKMainThreadDispatcher.Instance();

            GSPlatform.GetInstance().gsNativeCallback = callbcak;

            GSPlatform.GetInstance().GetPlatform().RegistNaviteCompletionHandler();
            GSPlatform.GetInstance().GetPlatform().Init(JsonMapper.ToJson(data));
        }

        public static void Call(string module, string func, Dictionary<string, object> parameters)
        {
            if (module == "crash" && func == "enableExceptionHandler")
            {
                GSUncaughtExceptionHandler.EnableExceptionHandler();
            }
            else
            {
                GSPlatform.GetInstance().GetPlatform().Call(module, func, JsonMapper.ToJson(parameters));
            }
        }
        public static bool CallBool(string module, string func, Dictionary<string, object> parameters)
        {
            return GSPlatform.GetInstance().GetPlatform().CallBool(module, func, JsonMapper.ToJson(parameters));
        }
        public static string CallString(string module, string func, Dictionary<string, object> parameters)
        {
            return GSPlatform.GetInstance().GetPlatform().CallString(module, func, JsonMapper.ToJson(parameters));
        }
        public static int CallInt(string module, string func, Dictionary<string, object> parameters)
        {
            return GSPlatform.GetInstance().GetPlatform().CallInt(module, func, JsonMapper.ToJson(parameters));
        }
        public static void Call(string module, string func)
        {
            Call(module, func, new Dictionary<string, object>());
        }

        public static bool CallBool(string module, string func)
        {
            return CallBool(module, func, new Dictionary<string, object>());
        }
        public static string CallString(string module, string func)
        {
            return CallString(module, func, new Dictionary<string, object>());
        }
        public static int CallInt(string module, string func)
        {
            return CallInt(module, func, new Dictionary<string, object>());
        }

        private static IGSPlatform platform;

        private static readonly object platformLock = new object();

        internal static IGSPlatform GetSDKPlatform()
        {
            if (platform == null)
            {
                lock (platformLock)
                {
                    if (platform == null)
                    {
#if UNITY_IOS && !UNITY_EDITOR
                        platform = new GSiOS();
#elif UNITY_ANDROID && !UNITY_EDITOR
                        platform = new GSAndroid();
#else
                        platform = new GSUniversalPlatform();
#endif
                    }
                    return platform;
                }
            }
            return platform;
        }
    }
}
