using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using GSSDKEditor;
using GSSDK;

[CustomEditor(typeof(GSSDKParams))]
public class GSSDKParamsEditor : Editor
{
    const string title = "GSSDK参数配置模块";

    SerializedProperty appId;
    SerializedProperty appKey;
    SerializedProperty buglyAppId;
    SerializedProperty isNewLogin;

    const string DispatcherName = "GSSDKMainThreadDispatcher";
    private class Styles
    {
        public static readonly GUIContent AppIdDesc = new GUIContent("GSSDK AppID[?]", "GSSDK AppID参数用于处理业务层面的内容，必须输入");
        public static readonly GUIContent AppKeyDesc = new GUIContent("GSSDK AppKey[?]", "GSSDK AppKey参数用于处理业务层面的内容，必须输入");
        public static readonly GUIContent BuyAppIdDesc = new GUIContent("GSSDK BuyAppID[?]", "GSSDK BuglyAppID，输入此参数，默认打开BUGLY DEBUG日志");
        public static readonly GUIContent IsNewLoginDesc = new GUIContent("GSSDK IsNewLogin[?]", "GSSDK 是不是新的登录接口，用于返回不同的登录结构");
        public static readonly GUIContent ExportSettingsDesc = new GUIContent("导出配置");
    }

    void OnEnable()
    {
        appId = serializedObject.FindProperty("m_appId");
        appKey = serializedObject.FindProperty("m_appKey");
        buglyAppId = serializedObject.FindProperty("m_bugly_appId");
        isNewLogin = serializedObject.FindProperty("m_isNewLogin");
    }

    void OnDisable()
    {
        AssetDatabase.SaveAssetIfDirty(target);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        appId.stringValue = EditorGUILayout.TextField(Styles.AppIdDesc, appId.stringValue);
        EditorGUILayout.Space();

        appKey.stringValue = EditorGUILayout.TextField(Styles.AppKeyDesc, appKey.stringValue);
        EditorGUILayout.Space();

        buglyAppId.stringValue = EditorGUILayout.TextField(Styles.BuyAppIdDesc, buglyAppId.stringValue);
        EditorGUILayout.Space();

        isNewLogin.boolValue = EditorGUILayout.Toggle(Styles.IsNewLoginDesc, isNewLogin.boolValue);
        EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button(Styles.ExportSettingsDesc))
        {
            SaveConfig();
        }
        EditorGUILayout.Space();
    }

    private void SaveConfig()
    {
        Dictionary<string, string> paramsDic = new Dictionary<string, string>();

        paramsDic.Add("GS_APPID", appId.stringValue);
        paramsDic.Add("GS_APPKEY", appKey.stringValue);
        paramsDic.Add("GS_PlatformID", "1");
        paramsDic.Add("GS_Channel", "310003");
        paramsDic.Add("GS_Store_Channel", "310003");
        paramsDic.Add("GS_Sub_Channel", "310003");
        paramsDic.Add("GSSERVER_URL", "https://gssdk-api.dobest.cn");
        paramsDic.Add("yoka_app_id", appId.stringValue);
        paramsDic.Add("yoka_is_online", "true");
        paramsDic.Add("bugly_bugly_appid", buglyAppId.stringValue);
        paramsDic.Add("bugly_debug", "true");
        paramsDic.Add("yoka_qq_appid", "false");
        paramsDic.Add("yoka_wechat_appid", "false");
        paramsDic.Add("yoka_is_off_login_guest", "true");
        paramsDic.Add("yoka_is_off_login_qq", "true");
        paramsDic.Add("yoka_is_off_login_weixin", "true");
        paramsDic.Add("yoka_is_off_pay_weixin", "false");
        paramsDic.Add("yoka_is_off_pay_alipay", "false");
        paramsDic.Add("yoka_is_off_login_yoka", "false");
        paramsDic.Add("isNewLogin", isNewLogin.boolValue.ToString().ToLower());
        paramsDic.Add("GSSDK_ADMIN_SERVER_URL", "https://sdk-admin-api.dobest.cn");

        string configPath = GSSDKParams.ConfigPath;
        string directoryPath = Path.GetDirectoryName(configPath);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        GSParamsTools.CreateAndWritePropertiesFile(configPath, paramsDic);

        AssetDatabase.Refresh();
    }

    public static void AddGSSDKMainThreadDispatcherIfNotExists()
    {
#if UNITY_2023_1_OR_NEWER
		GSSDKMainThreadDispatcher dispatcher = FindFirstObjectByType<GSSDKMainThreadDispatcher>(FindObjectsInactive.Include);
#elif UNITY_2020_1_OR_NEWER
        GSSDKMainThreadDispatcher dispatcher = FindObjectOfType<GSSDKMainThreadDispatcher>(true);
#else
		GSSDKMainThreadDispatcher dispatcher = FindObjectOfType<GSSDKMainThreadDispatcher>();
#endif
        if (dispatcher == null)
        {
            GameObject container = GameObject.Find(DispatcherName);
            if (container == null)
            {
                container = new GameObject(DispatcherName);

                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(container, "Create " + DispatcherName);
                    if (Selection.activeTransform != null)
                        container.transform.parent = Selection.activeTransform;
                    Selection.activeObject = container;
                }
            }

            dispatcher = container.AddComponent<GSSDKMainThreadDispatcher>();

        }
    }

    [SettingsProvider]
    static SettingsProvider CreateProjectSettingsProvider()
    {
        GSSDKParams param = AssetDatabase.LoadAssetAtPath<GSSDKParams>(GSSDKParams.AssetPath);
        if (param == null)
        {
            string directoryPath = Path.GetDirectoryName(GSSDKParams.AssetPath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            param = GSSDKParams.CreateInstance<GSSDKParams>();
            AssetDatabase.CreateAsset(param, GSSDKParams.AssetPath);
            EditorUtility.SetDirty(param);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        AssetSettingsProvider provider = AssetSettingsProvider.CreateProviderFromAssetPath(
            GSSDKParams.SettingsWindowPath, GSSDKParams.AssetPath,
            SettingsProvider.GetSearchKeywordsFromGUIContentProperties<Styles>());
        provider.label = title;
        return provider;
    }
}
