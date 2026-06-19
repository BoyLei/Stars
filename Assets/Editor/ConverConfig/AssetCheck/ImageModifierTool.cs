using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class ImageModifierTool : EditorWindow
{
    [MenuItem("自动化工具/贴图都修改成normal")]
    static void OpenWindow()
    {
        ImageModifierTool window = GetWindow<ImageModifierTool>("Image Modifier");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("normal", EditorStyles.boldLabel);

        if (GUILayout.Button(" 2normal Images"))
        {
            ModifyImages();
        }



        GUILayout.Label("Default", EditorStyles.boldLabel);

        if (GUILayout.Button("2Default Images"))
        {
            ModifyImages1();
        }
    }

    private void ModifyImages()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);

        foreach (Object selectedObject in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // 设置纹理为法线贴图格式
            importer.textureType = TextureImporterType.NormalMap;

            AssetDatabase.ImportAsset(assetPath);
           
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //Debug.Log("Images modified successfully!");
    }

    [MenuItem("自动化工具/贴图都修改成Default")]
    static void OpenWindow1()
    {
        ImageModifierTool window = GetWindow<ImageModifierTool>("Image Modifier");
        window.Show();
    }

    
    private void ModifyImages1()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);

        foreach (Object selectedObject in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // 设置纹理为法线贴图格式
            importer.textureType = TextureImporterType.Default;

            AssetDatabase.ImportAsset(assetPath);

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //Debug.Log("Images modified successfully!");
    }
}