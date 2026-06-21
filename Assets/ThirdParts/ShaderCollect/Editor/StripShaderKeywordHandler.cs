using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class StripShaderKeywordHandler : IPreprocessShaders
{
    private List<ShaderKeyword> m_includeShaderKeywords = null;
    private List<ShaderKeyword> m_excludeShaderKeywords = null;
    private bool m_EnableShaderStrip = false;
    private List<string> m_ShaderPaths = null;
    private string[] m_IgnoreShaders = null;
    private string[] m_ShaderStripWhitelist = null;

    public int callbackOrder => 0;

    public StripShaderKeywordHandler()
    {
#if STAR_CHECK
        var globalSettings = AssetDatabase.LoadAssetAtPath<ShaderCheckerDefine>("Assets/ShaderCheckerDefine.asset");
        if (globalSettings == null)
        {
            Debug.LogError("初始化全局配置失败，请检查再执行！");
            return;
        }
        m_EnableShaderStrip = globalSettings.enableShaderStrip;

        m_ShaderPaths = globalSettings.shaderPaths;
        m_ShaderStripWhitelist = globalSettings.ShaderStripWhitelist;

        m_IgnoreShaders = globalSettings.IgnoreShaders;

        InitKeywords(globalSettings.includeShaderKeywords, globalSettings.excludeShaderKeywords);
#endif
    }

    private void InitKeywords(List<string> includeShaderKeywords, List<string> excludeShaderKeyworkds)
    {
        m_includeShaderKeywords = new List<ShaderKeyword>();
        foreach (var keyword in excludeShaderKeyworkds)
        {
            m_includeShaderKeywords.Add(new ShaderKeyword(keyword));
        }

        m_excludeShaderKeywords = new List<ShaderKeyword>();
        foreach (var keyword in excludeShaderKeyworkds)
        {
            m_excludeShaderKeywords.Add(new ShaderKeyword(keyword));
        }
    }

    private bool IsDisableKeyword(ShaderKeywordSet kwSet)
    {
        if (m_includeShaderKeywords != null)
        {
            foreach (var keyword in m_includeShaderKeywords)
            {
                if (kwSet.IsEnabled(keyword))
                    return false;
            }
        }

        if (m_excludeShaderKeywords != null)
        {
            foreach (var keyword in m_excludeShaderKeywords)
            {
                if (kwSet.IsEnabled(keyword))
                    return true;
            }
        }
        return false;
    }

    private bool IsVaildShader(Shader shader)
    {
        if (m_ShaderStripWhitelist != null)
        {
            foreach (var item in m_ShaderStripWhitelist)
            {
                if (item.Equals(shader.name))
                    return true;
            }
        }

        if (m_IgnoreShaders != null)
        {
            foreach (var item in m_IgnoreShaders)
            {
                if (item.Equals(shader.name)) 
                    return false;
            }
        }

        if (shader.name.StartsWith("Hidden/"))
        {
            return true;
        }
        if (m_ShaderPaths != null)
        {
            var shaderPath = AssetDatabase.GetAssetPath(shader);
            foreach (var item in m_ShaderPaths)
            {
                if (shaderPath.Contains(item))
                    return true;
            }
        }
        return false;
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (!m_EnableShaderStrip)
            return;

        if (!IsVaildShader(shader))
        {
            data.Clear();
            return;
        }
        for (int i = data.Count - 1; i >= 0; --i)
        {
            var kwSet = data[i].shaderKeywordSet;
            if (IsDisableKeyword(kwSet))
                data.RemoveAt(i);
        }
    }
}
