using UnityEditor;
using UnityEngine;

public static class AssetPicSettingTools
{
    [MenuItem("自动化工具/Select TextureImporterSetting")]
    private static void SelectTextureImporterSetting()
    {
        string guid = "0b1baf6b74e2a164f80c7793032326b5";
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        Selection.activeObject = asset;
    }
}
