using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AutoTextureSettingAlphaSourceNone : EditorWindow
{
    private string[] texturePaths;
    private int currentTextureIndex;
    private bool isModifying;

    private string texturesInput;
    private void OnGUI()
    {
        GUILayout.Label("Input Texture Paths (One per line)");
        texturesInput = EditorGUILayout.TextArea(texturesInput, GUILayout.Height(200));

        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChanges(texturesInput);
        }
    }

    [MenuItem("自动化工具/AutoTextureSettingAlphaSourceNone")]
    private static void Init()
    {
        AutoTextureSettingAlphaSourceNone window = (AutoTextureSettingAlphaSourceNone)EditorWindow.GetWindow(typeof(AutoTextureSettingAlphaSourceNone));
        window.Show();
    }

    private void ApplyChanges(string texturesInput)
    {
        string input = texturesInput;
        string pattern = @"Assets.*\.(png|jpg|jpeg|tga|bmp)";

        MatchCollection matches = Regex.Matches(input, pattern);
        int index = 0;
        texturePaths = new string[matches.Count];
        foreach (Match match in matches)
        {
            texturePaths[index] = match.Value;
            index++;
            Debug.Log(match.Value);
            ProcessTextures(match.Value);
        }
    }

    private void ProcessTextures(string value)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(value);

        if (importer != null)
        {
            importer.alphaSource = TextureImporterAlphaSource.None;

            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + value);
        }
    }
}