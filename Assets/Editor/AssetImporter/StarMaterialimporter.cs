///--------------------------------------------------------------------
/// 文件名   :   StarMaterialimporter.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/13 17:43:26
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StarMaterialimporter
{
    [MenuItem("Assets/一键勾选GPU Instance", false, 2)]
    static void GpuInstance()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if(string.IsNullOrEmpty(path))
        {
            return;
        }
        if (path.Contains("."))
        {
            path = System.IO.Path.GetDirectoryName(path);
        }
        string[] files = AssetDatabase.FindAssets("t:Material", new string[] { path });
        foreach (var item in files)
        {
            string mPath = AssetDatabase.GUIDToAssetPath(item);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(mPath);
            if (material != null)
            {
                material.enableInstancing = true;
                UnityEditor.EditorUtility.SetDirty(material);
            }

        }
        AssetDatabase.SaveAssets();
    }
 /*   [MenuItem("Assets/一键FromShader", false, 2)]
    static void GRenderQueueFromShader()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        if (path.Contains("."))
        {
            path = System.IO.Path.GetDirectoryName(path);
        }
        string[] files = AssetDatabase.FindAssets("t:Material", new string[] { path });
        foreach (var item in files)
        {
            string mPath = AssetDatabase.GUIDToAssetPath(item);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(mPath);
            if (material != null)
            {
                material.renderQueue
                UnityEditor.EditorUtility.SetDirty(material);
            }

        }
        AssetDatabase.SaveAssets();
    }
*/
}
