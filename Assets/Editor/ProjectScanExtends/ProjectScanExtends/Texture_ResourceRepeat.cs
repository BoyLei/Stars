//在ProjectScanDef.cs中添加新的模块类型 (如果需要添加新模块的话，否则直接添加检查项即可)

//扫描类型唯一标识
//public enum EnumScanModes
//{
//    通用设置 = 0,
//    基本资源检查,
//    贴图资源检查,
//    音频资源检查,
//    动效资源检查,
//    场景检查,
//    eExampleCheck
//}

//自定义某个检测项
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CasualEngine.ProjectScanTool
{
    [Serializable] 
    public class Texture_ResourceRepeatCheckDetail : CustomCheckDetail
    {
       
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_ResourceRepeat : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图是否重复";
            ruleDescription = "[说 明]:脚本通过计算每个贴图的MD5哈希值来检查它们是否重复。它遍历所有的贴图资源，并为每个贴图计算一个哈希值。如果两个或更多贴图的哈希值相同，这意呀着它们的内容是一样的，即使它们的文件名不同。";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查贴图是否重复")]
        public List<Texture_ResourceRepeatCheckDetail> checkDetailList = new List<Texture_ResourceRepeatCheckDetail>() { new Texture_ResourceRepeatCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_TextureResourceRepeatCheck, this);
        }

        #region 贴图是否重复统计
        public static bool Do_TextureResourceRepeatCheck(Texture_ResourceRepeat customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Dictionary<string,List<string>> textureHashes = new Dictionary<string, List<string>>();
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    string textureHash = GetHashCode(texture);
                    if (!textureHashes.ContainsKey(textureHash))
                    {
                        List<string> pathList = new List<string>();
                        textureHashes.Add(textureHash,pathList);
                    }
                    textureHashes[textureHash].Add(path);

                }
            }
            foreach (var item in textureHashes)
            {
                if (item.Value.Count > 1)
                {
                    foreach (var path in item.Value)
                    {
                        Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                        if (texture == null)
                            continue;
                        customRule.Record(texture, methodName, $"贴图重复 path:{path} ", path);
                    }
                }
            }
           
            return true;
        }
        private static string GetHashCode(Texture2D texture)
        {
            byte[] textureBytes = texture.GetRawTextureData();
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(textureBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        private static byte[] GetAssetData<T>(T asset) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(Texture2D))
            {
                return ImageConversion.EncodeToPNG(asset as Texture2D);
            }
            else if (typeof(T) == typeof(AudioClip))
            {
                // 这里需要自己定义如何将AudioClip转换为byte[]
                return new byte[0]; // 这里是一个占位符
            }
            // 根据资源的类型适配不同的处理方法
            return new byte[0];
        }

        private static string ComputeHash(byte[] assetData)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(assetData);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        #endregion
    }
}