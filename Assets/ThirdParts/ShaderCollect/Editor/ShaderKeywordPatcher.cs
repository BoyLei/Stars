using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Assets/ShaderKeywordPatcher.asset", menuName = "Shader Keyword Patcher")]
public class ShaderKeywordPatcher : SerializedScriptableObject
{
    [Serializable]
    public class ShaderKeywordInfo
    {
        public PassType passType;
        public List<string> keywords = new List<string>();
    }

    public string shaderVariantPath;

    public Dictionary<Shader, List<ShaderKeywordInfo>> shaderKeywords = new Dictionary<Shader, List<ShaderKeywordInfo>>();

    public bool TryAddShaderVariant()
    {
        var files = Directory.GetFiles(shaderVariantPath, "*.shadervariants");
        foreach (var file in files)
        {
            var shaderVariants = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(file);
            if (shaderVariants != null)
            {
                foreach (var shaderInfo in shaderKeywords)
                {
                    foreach (var keywordInfo in shaderInfo.Value)
                    {
                        var shaderVariant = new ShaderVariantCollection.ShaderVariant();
                        shaderVariant.shader = shaderInfo.Key;
                        shaderVariant.passType = keywordInfo.passType;
                        shaderVariant.keywords = keywordInfo.keywords.ToArray();
                        shaderVariants.Add(shaderVariant);
                    }
                }
                AssetDatabase.SaveAssetIfDirty(shaderVariants);
            }
        }
        AssetDatabase.Refresh();
        return true;
    }

    [MenuItem("Tools/Proc Shader Keyword Patch")]
    public static void ProcShaderKeywordPatch()
    {
        var asset = AssetDatabase.LoadAssetAtPath<ShaderKeywordPatcher>("Assets/ShaderKeywordPatcher.asset");
        if (asset == null)
        {
            throw new Exception("Assets/ShaderKeywordPatcher.asset not found!!!");
        }
        if (string.IsNullOrEmpty(asset.shaderVariantPath) || !Directory.Exists(asset.shaderVariantPath))
        {
            throw new Exception("ShaderKeywordPatcher config file error!!!");
        }
        if (asset.shaderKeywords.Count == 0)
        {
            throw new Exception("Shader Keyword Config error!!!");
        }
        asset.TryAddShaderVariant();

        Debug.Log("Proc Shader Keyword Patch OK!");
    }
}
