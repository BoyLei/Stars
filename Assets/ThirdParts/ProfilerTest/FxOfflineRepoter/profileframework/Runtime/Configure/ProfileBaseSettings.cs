// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/23 18:15:8)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Yoka.Galaxy.Configure
{

    public abstract class ProfileBaseSettings : Settings
    {
        private const string BundleName = "profilesettings.bundle";
        protected const string FolderName = "Yoka.Settings/Profile";
        private const string TempPath = "Temp/" + FolderName;
        private const string StreamingPath = "Assets/StreamingAssets/" + FolderName;
        private static AssetBundle _bundle = null;

#if UNITY_EDITOR
        public static bool BundleMode
        {
            get => UnityEditor.EditorPrefs.GetBool($"{UnityEditor.PlayerSettings.productName}.Configure.ProfileBundleMode", false);
            set => UnityEditor.EditorPrefs.SetBool($"{UnityEditor.PlayerSettings.productName}.Configure.ProfileBundleMode", value);
        }
#endif

        internal static T LoadFromBundle<T>() where T : ProfileBaseSettings
        {
            Debug.Log("Debug LoadFromBundle");
            if (_bundle == null)
            {
                _bundle = AssetBundle.LoadFromFile(BundlePath());
            }
            Debug.LogFormat($"Debug LoadFromBundle _bundle.LoadAllAssets().Length={0}", _bundle.LoadAllAssets().Length);
            foreach (var debugAsset in _bundle.LoadAllAssets())
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
            var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(ProfileBaseSettings));
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

    public abstract class ProfileBaseSettings<T> : ProfileBaseSettings where T : ProfileBaseSettings
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

        public static string AssetPath
        {
            get
            {
                var folderPath = Path.Combine("Assets", FolderName);
                var filePath = Path.Combine(folderPath, $"{Name}.asset");
                return filePath;
            }
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

}
#endif