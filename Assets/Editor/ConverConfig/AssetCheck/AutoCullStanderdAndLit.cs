using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using OfficeOpenXml;

public class AutoCullStanderdAndLit : MonoBehaviour
{
    static Dictionary<string, string> shaderMapping = new Dictionary<string, string>()
    {
        { "Standard", "SGAME/SGAME_Scene_01" },
    };
    
    [MenuItem("自动化工具/自动排除Standard Shader并同时输出Lit Shader路径")]
    private static void ReplaceStandardToDiffuse()
    {
        //批量输出使用LitShader的材质球路径到excel文件 英文使用Lit的材质球需要具体分析
        //Excel
        var package = new ExcelPackage();
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
        int shaderPathCount = 1;
        
        var paths = AssetDatabase.GetAllAssetPaths();
        
        foreach (var path in paths) 
        {
            //批量替换Standard到Scene01 因为Standard在urp下肯定报紫
            foreach (var mapping in shaderMapping )
            {
                var srcShader = Shader.Find(mapping.Key);
                var destShader = Shader.Find(mapping.Value);
                
                //lit
                var LitShader = Shader.Find("Universal Render Pipeline/Lit");

                if (path.EndsWith(".mat"))
                {
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat != null && mat.shader == srcShader )
                    {
                        mat.shader = destShader;
                        Debug.LogError("StandardShader替换: "+ path);
                        EditorUtility.SetDirty(mat);
                    }
                    if (mat != null && mat.shader == LitShader )
                    {
                        worksheet.Cells["A" + shaderPathCount.ToString()].Value = "Lit Shader: " + path;
                        shaderPathCount++;
                        //Debug.LogError("Lit Shader: "+path);
                        EditorUtility.SetDirty(mat);
                    }
                }
            }
        }
        //保存Excel
        var excelFile = new FileInfo(Application.dataPath + "/LitShaderPath.xlsx");
        Debug.LogError("LitShader路径输出至Excel文档, 目录为: "+Application.dataPath + "/LitShaderPath.xlsx");
        package.SaveAs(excelFile);
                
        AssetDatabase.SaveAssets();
    }
}
