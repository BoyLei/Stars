// -----------------------------------------------------------------------
// This file is part of framework.
// 
// (c) goricher <wangwei06@dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER

using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Yoka.Galaxy.Configure
{
    /// <summary>
    /// 抽象Unity配置数据对象。
    /// </summary>
    public abstract class Settings : ScriptableObject, ISerializationCallbackReceiver
    {
        public virtual void OnInit()
        { }

        public virtual void OnBeforeSerialize()
        { }

        public virtual void OnAfterDeserialize()
        { }
    }

#if UNITY_EDITOR

    public static class SettingsMenu
    {
        [UnityEditor.MenuItem("Assets/Create/创建Settings", false, 20)]
        public static void FetchSettings()
        {
            var setting = UnityEditor.Selection.activeObject as UnityEditor.MonoScript;
            var type = setting.GetClass();
            if (type.IsSubclassOf(typeof(Settings)))
            {
                var fetch = type.GetMethod("Fetch", 
                    System.Reflection.BindingFlags.Static | 
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.FlattenHierarchy);
                fetch?.Invoke(null, null);
            }
        }
    }

#endif

    public abstract class RuntimeSettings : Settings
    {
        private const string BundleName = "runtimesettings.bundle";
        protected const string FolderName = "Yoka.Settings/Runtime";
        private const string TempPath = "Temp/" + FolderName;
        private const string StreamingPath = "Assets/StreamingAssets/" + FolderName;
        private static AssetBundle _bundle = null;

#if UNITY_EDITOR
        public static bool BundleMode
        {
            get => UnityEditor.EditorPrefs.GetBool($"{UnityEditor.PlayerSettings.productName}.Configure.RuntimeBundleMode", false);
            set => UnityEditor.EditorPrefs.SetBool($"{UnityEditor.PlayerSettings.productName}.Configure.RuntimeBundleMode", value);
        }
#endif

        internal static T LoadFromBundle<T>() where T : RuntimeSettings
        {
            Debug.Log("Debug LoadFromBundle");
            if (_bundle == null)
            {
                _bundle = AssetBundle.LoadFromFile(BundlePath());
            }
            Debug.LogFormat($"Debug LoadFromBundle _bundle.LoadAllAssets().Length={0}", _bundle.LoadAllAssets().Length);
            foreach(var debugAsset in _bundle.LoadAllAssets())
            {
                Debug.LogFormat($"Debug LoadFromBundle debugAsset={0}", debugAsset.name);
            }
            return _bundle.LoadAsset<T>(typeof(T).Name);
        }

        private static string BundlePath()
        {
            var relativePath = Path.Combine(FolderName, BundleName);
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
            {
                return persistentPath;
            }

            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

#if UNITY_EDITOR
        public static void Build(UnityEditor.BuildTarget platform)
        {
            var assets = new List<string>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(RuntimeSettings));
            for (var i = 0; i < guids.Length; ++i)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                assets.Add(path);
            }

            var build = new UnityEditor.AssetBundleBuild();
            build.assetBundleName = BundleName;
            build.assetNames = assets.ToArray();

            try
            {
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                UnityEditor.BuildPipeline.BuildAssetBundles(TempPath, new[] { build }, UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression, platform);

                if (!Directory.Exists(StreamingPath))
                {
                    Directory.CreateDirectory(StreamingPath);
                }

                var sourcePath = Path.Combine(TempPath, BundleName);
                var targetPath = Path.Combine(StreamingPath, BundleName);
                File.Copy(sourcePath, targetPath, true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw ex;
            }
        }
#endif
    }

    public abstract class RuntimeSettings<T> : RuntimeSettings where T : RuntimeSettings
    {
        private static T _instance = default;

        public static string Name => typeof(T).Name;

        public static T Fetch()
        {
            Init();

            return _instance;
        }

        public static void Flush()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Fetch());
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public static void Clean()
        {
            _instance = default;
        }

        private static void Init()
        {
            if (_instance == null)
            {
#if UNITY_EDITOR
                var folderPath = Path.Combine("Assets", FolderName);
                var filePath = Path.Combine(folderPath, $"{Name}.asset");
                if (BundleMode && Application.isPlaying)
                {
                    _instance = LoadFromBundle<T>();
                }
                else
                {
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePath);
                }
#else
                _instance = LoadFromBundle<T>();
#endif

#if UNITY_EDITOR
                if (_instance == null)
                {
                    _instance = CreateInstance<T>();

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    UnityEditor.AssetDatabase.CreateAsset(_instance, filePath);
                }

                _instance.OnInit();
#endif
                if (_instance == null)
                {
                    throw new InvalidDataException($"{Name} instance is invalid");
                }
            }
        }
    }

#if UNITY_EDITOR
    public abstract class EditorSettings : Settings
    { }

    /// <summary>
    /// 泛化抽象Unity配置数据对象。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EditorSettings<T> : EditorSettings where T : EditorSettings
    {
        private const string FolderPath = "Assets/Yoka.Settings/Editor";

        private static T _instance = default;

        public static string Name => typeof(T).Name;

        public static T Fetch()
        {
            Init();

            return _instance;
        }

        public static void Flush()
        {
            UnityEditor.EditorUtility.SetDirty(Fetch());
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public static void Clean()
        {
            _instance = default;
        }

        private static void Init()
        {
            if (_instance == null)
            {
                var filePath = Path.Combine(FolderPath, $"{Name}.asset");
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePath);
                if (_instance == null)
                {
                    if (File.Exists(filePath))
                    {
                        Debug.LogError($"{filePath}文件已存在, 但是未进入AssetDatabase");
                        return;
                    }

                    try
                    {
                        _instance = CreateInstance<T>();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    UnityEditor.AssetDatabase.CreateAsset(_instance, filePath);
                }

                _instance.OnInit();
            }
        }
    }
#endif
}

#endif