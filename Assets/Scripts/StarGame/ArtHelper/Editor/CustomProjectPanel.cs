/*using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CustomProjectPanel
{
    static CustomProjectPanel()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
    }

    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        // 在这里可以访问项目中的资源，并进行条件判断或其他操作
        // 例如，你可以获取资源的类型，并根据类型进行不同的处理

        // 示例：绘制一个自定义的预览图
        Texture2D previewTexture = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;
        if (previewTexture != null)
        {
            Rect previewRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);
            EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
        }

        // 如果你还想实现其他项目面板的行为，可以在这里添加代码
    }
}*/