using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SkillEditor
{

    public static class FxGenera
    {
        [MenuItem("Build/生成技能配置")]
        public static void TestFx()
        {
            GeneraFxDetails();
        }

        public static void GeneraFxDetails()
        {
            Dictionary<string, int> fxFileDurationDic = new Dictionary<string, int>();

            string path = "Assets/Res/Effects";
            VisitPath(path, fxFileDurationDic);

            WriteJson(fxFileDurationDic);
        }

        public static void VisitPath(string path, in Dictionary<string, int> fxFileDurationDic)
        {
            // 先获取 path 目录下 prefab 的路径
            string[] prefabPaths = System.IO.Directory.GetFiles(path, "*.prefab");

            foreach (string prefabPath in prefabPaths)
            {
                VisitPrefab(prefabPath, fxFileDurationDic);
            }

            string[] filePaths = System.IO.Directory.GetDirectories(path);

            foreach (string filePath in filePaths)
            {
                VisitPath(filePath, fxFileDurationDic);
            }
        }

        public static void VisitPrefab(string prefabPath, in Dictionary<string, int> fxFileDurationDic)
        {
            GameObject gob = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            ParticleSystem ps = gob.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                return;
            }

            int fxDuration = SkillEditorUtils.GetFxRootReallyDuration(ps);

            prefabPath = prefabPath.Replace("Assets/Res/", "");
            prefabPath = prefabPath.Replace(".prefab", "");
            prefabPath = prefabPath.Replace("\\", "/");

            fxFileDurationDic.Add(prefabPath.ToLower(), fxDuration);

            // SGF.Debuger.LogError($"Visit Fx: {gob.name} , path : {prefabPath} , fxDuration: {fxDuration}");
        }

        public static void WriteJson(Dictionary<string, int> fxFileDurationDic)
        {
            FXDetailJson fXDetailJson = new FXDetailJson();
            fXDetailJson.fxFileDurationDic = fxFileDurationDic;
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(fXDetailJson);
            string path = string.Format(SkillEditorGlobal.Instance.FxJsonDetailPath + "/FxDetail.json");
            WriteJson(content, path);
        }

        public static void WriteJson(string content, string path)
        {
#if UNITY_EDITOR

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            FileStream fs = new FileStream(path, FileMode.CreateNew);
            byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}
