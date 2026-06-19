/*
 * @Description: 扩展工具辅助接口，GameTech开发人员维护，各项目组谨慎修改
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.CommonLibs.CommonExtends
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public class AssetType
    {
        public static string texture = "texture";
        public static string material = "material";
        public static string prefab = "prefab";
        public static string scene = "scene";
        public static string animation = "animation";
        public static string animatorController = "AnimatorController";
        public const string audioclip = "audioclip";
        public const string textAsset = "textAsset";
        public const string spriteAtlas = "spriteAtlas";
        public const string asset = "asset";
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public class DeviceTypeDefine
    {
        public const string Default = "Default";
        public const string DefaultTexturePlatform = "DefaultTexturePlatform";
        public const string Win = "Standalone";
        public const string IOS = "IOS";
        public const string Android = "Android";
        public const string iPhone = "iPhone";

    }

    /// <summary>
    /// 文件路径后缀类型
    /// </summary>
    public class PathExtensionDefine
    {
        public const string anim = ".anim";
        public const string controller = ".controller";
        public const string spriteatlas = ".spriteatlas";
    }

    public class GTHelper
    {
        /// <summary>
        /// 是否是后台命令行启动的unity
        /// </summary>
        public static bool LoadWithBatchmode
        {
            get
            {
                return Environment.CommandLine.IndexOf("-batchmode") >= 0;
            }
        }

        /// <summary>
        /// 根据instanceID获得guid
        /// </summary>
        static public string GetGuidByInstanceID(int instance_id)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instance_id, out string guid, out long local_id);
            return guid;
        }

        /// <summary>
        /// 创建序列化对象
        /// </summary>
        static public T CreateAsset<T>(string outPath, string newName = "") where T : ScriptableObject
        {
            var scriptableObj = ScriptableObject.CreateInstance<T>();

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
                AssetDatabase.Refresh();
            }

            string filePath = outPath + GetGuidByInstanceID(scriptableObj.GetInstanceID()) + ".asset";

            AssetDatabase.CreateAsset(scriptableObj, filePath);

            if (string.IsNullOrEmpty(newName))
            {
                AssetDatabase.RenameAsset(filePath, GetGuidByInstanceID(scriptableObj.GetInstanceID()));
            }
            else
            {
                AssetDatabase.RenameAsset(filePath, newName);
            }
            AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return scriptableObj;
        }

        /// <summary>
        /// 获得指定目录下某一类型资源guid集合
        /// </summary>
        public static string[] GetAssetGuidsByType(string resFolder = "Assets", string type = "texture")
        {
            return AssetDatabase.FindAssets("t:" + type, new string[] { resFolder });
        }

        /// <summary>
        /// 根据guid加载GameObject
        /// </summary>
        public static GameObject GetAssetObjByGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            return obj;
        }

        static public string ObjectToGUID(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return (!string.IsNullOrEmpty(path)) ? AssetDatabase.AssetPathToGUID(path) : "";
        }

        static MethodInfo s_GetInstanceIDFromGUID;
        static public UnityEngine.Object GUIDToObject(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            // if (s_GetInstanceIDFromGUID == null)
            //     s_GetInstanceIDFromGUID = typeof(AssetDatabase).GetMethod("GetInstanceIDFromGUID", BindingFlags.Static | BindingFlags.NonPublic);

            // int id = (int)s_GetInstanceIDFromGUID.Invoke(null, new object[] { guid });
            // if (id != 0) return EditorUtility.InstanceIDToObject(id);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }

        static public T GUIDToObject<T>(string guid) where T : UnityEngine.Object
        {
            UnityEngine.Object obj = GUIDToObject(guid);
            if (obj == null) return null;

            System.Type objType = obj.GetType();
            if (objType == typeof(T) || objType.IsSubclassOf(typeof(T))) return obj as T;

            if (objType == typeof(GameObject) && typeof(T).IsSubclassOf(typeof(Component)))
            {
                GameObject go = obj as GameObject;
                return go.GetComponent(typeof(T)) as T;
            }
            return null;
        }

        public static string GetNodePath(Transform node)
        {
            if (node == null)
            {
                Debug.LogError("GetNodePath error, node is null");
                return "";
            }
            string path = node.name;
            Transform parentNode = node.parent;
            while (parentNode != null)
            {
                path = parentNode.name + "/" + path;
                parentNode = parentNode.parent;
            }

            return path;
        }

        public static string GetObjDeepPath(GameObject root, Transform obj)
        {
            //资源路径
            List<string> parentPath = new List<string>();
            string refPath = "";
            parentPath.Clear();
            parentPath.Add(obj.name);
            FindUpParentName(root, obj.transform, ref parentPath);
            parentPath.Reverse();
            for (var i = 0; i < parentPath.Count; i++)
            {
                if (i > 0)
                    refPath += "/";
                refPath += parentPath[i];
            }
            return refPath;
        }

        private static Transform FindUpParentName(GameObject root, Transform obj, ref List<string> parentPath)
        {
            if (obj.parent == null || obj.parent.gameObject == root)
                return obj;
            else
            {
                parentPath.Add(obj.parent.name);
                return FindUpParentName(root, obj.parent, ref parentPath);
            }
        }

    }
}