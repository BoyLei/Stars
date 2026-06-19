//using UnityEngine;
//using UnityEditor;
//using System.Linq;

//public class SetMaterialNames : EditorWindow
//{
//    [MenuItem("Tools/Set Material Names")]
//    private static void SetMaterialsNames()
//    {
//        // 获取所有选中的FBX文件路径
//        string[] selectedFbxPaths = Selection.GetFiltered(typeof(Object), SelectionMode.Assets)
//            .Select(fbx => AssetDatabase.GetAssetPath(fbx))
//            .Where(path => path.EndsWith(".fbx"))
//            .ToArray();

//        foreach (string fbxPath in selectedFbxPaths)
//        {
//            // 导入FBX文件
//            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
//            if (modelImporter != null)
//            {
//                // 获取所有Materials的导入配置
//                string[] materialNames = modelImporter.GetMaterialNames();
//                for (int i = 0; i < materialNames.Length; i++)
//                {
//                    // 设置Material名称
//                    modelImporter.SetMaterialName(i, "Material_" + i);
//                }

//                // 应用修改
//                modelImporter.SaveAndReimport();
//            }
//        }

//        Debug.Log("Material names have been set for selected FBX files.");
//    }
//}
