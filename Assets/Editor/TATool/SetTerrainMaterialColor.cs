#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using StylizedWater2;

public class SetTerrainMaterialColor : EditorWindow
{
    [MenuItem("自动化工具/TA工具/给所有Terrain材质刷新Color的数值")]
    static void ExtractMaterials()
    {
        List<Material> objects =  new List<Material>();
        GameObject[] objects1 = GameObject.FindObjectsOfType<GameObject>();
        
        string[] allMaterials  =  AssetDatabase.FindAssets("t:Material");

        foreach (string materialGUID in allMaterials)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(materialGUID);
            if (materialPath.Contains("Map") && materialPath.Contains("Terrain"))
            {
                Material tempMat = (Material)AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                objects.Add((Material)AssetDatabase.LoadAssetAtPath<Material>(materialPath));
            }
        }

        int matCount = 0;
        foreach (Material mat in objects)
        {
            if (mat.name.Contains("Map") && mat.name.Contains("Terrain"))
            {
                Material material = mat;
                if (material != null && material.shader.name.Contains("SGame/Terrain/TerrainLit_TArray"))
                {
                    // 获取值A
                    //将面板中的Color转换成Diffuse Remap
                    Vector4 Color0 = material.GetVector("_Color0");
                    Vector4 Color1 = material.GetVector("_Color1");
                    Vector4 Color2 = material.GetVector("_Color2");
                    Vector4 Color3 = material.GetVector("_Color3");
                    //
                    // // 修改值B
                    material.SetVector("_DiffuseRemapScale0", Color0);
                    material.SetVector("_DiffuseRemapScale1", Color1);
                    material.SetVector("_DiffuseRemapScale2", Color2);
                    material.SetVector("_DiffuseRemapScale3", Color3);
                    //
                    // // 保存修改
                    // EditorUtility.SetDirty(material);
                    Debug.Log("刷新材质球:"+mat.name+" 每个地表贴图的Color");
                    matCount++;
                }
            }
        }
        
        Debug.Log("给"+matCount+"个地表材质球刷新Color数值");

    }
}
#endif