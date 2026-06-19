using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AssetPosType
{
    Memory, AssetDatabase
}

public enum AssetCheckType
{
    Image, Text
}

public class ShowPopupWindow : EditorWindow
{
    string windowTitle = "Choose a asset: ";
    string newAssetName = string.Empty;
    private System.Action<object, AssetPosType, AssetCheckType> callBack;
    private bool tgMemoryType = true, tgAssetType = false;
    private bool atImage = true, atText = false;

    private AssetPosType assetPosType = AssetPosType.Memory;
    private AssetCheckType assetCheckType = AssetCheckType.Image;

    void OnGUI()
    {
        GUILayout.Space(5);

        newAssetName = EditorGUILayout.TextField(windowTitle, newAssetName);

        ///AssetPosType
        EditorGUI.BeginChangeCheck();
        tgMemoryType = EditorGUILayout.Toggle("Memory", tgMemoryType, EditorStyles.radioButton);
        if (EditorGUI.EndChangeCheck())
        {
            if (tgMemoryType) tgAssetType = false;
            else tgMemoryType = true;
        }

        EditorGUI.BeginChangeCheck();
        tgAssetType = EditorGUILayout.Toggle("AssetDatabase", tgAssetType, EditorStyles.radioButton);
        if (EditorGUI.EndChangeCheck())
        {
            if (tgAssetType) tgMemoryType = false;
            else tgMemoryType = true;
        }

        ////AssetCheckType
        GUILayout.Space(5);
        EditorGUI.BeginChangeCheck();
        atImage = EditorGUILayout.Toggle("Image", atImage);
        if (EditorGUI.EndChangeCheck())
        {
            if (atImage) atText = false;
            else atImage = true;
        }

        EditorGUI.BeginChangeCheck();
        atText = EditorGUILayout.Toggle("Text", atText);
        if (EditorGUI.EndChangeCheck())
        {
            if (atText) atImage = false;
            else atImage = true;
        }


        if (GUILayout.Button("Find new asset"))
        {
            assetPosType = tgMemoryType ? AssetPosType.Memory : AssetPosType.AssetDatabase;
            assetCheckType = atImage ? AssetCheckType.Image : AssetCheckType.Text;
            callBack?.Invoke(newAssetName, assetPosType, assetCheckType);
        }

        if (GUILayout.Button("Close"))
            Close();
    }

    public static void CreatePopupWindow(System.Action<object, AssetPosType, AssetCheckType> callBack)
    {
        var window = new ShowPopupWindow();
        window.callBack = callBack;
        window.ShowUtility();
    }
}

public class AutoTextureSettingForAstc : EditorWindow
{
    private string[] texturePaths;
    private int currentTextureIndex;
    private bool isModifying;



   
 


    [MenuItem("自动化工具/AutoTextureSettingForAstc")]
    private static void Init()
    {
        AutoTextureSettingForAstc window = (AutoTextureSettingForAstc)EditorWindow.GetWindow(typeof(AutoTextureSettingForAstc));
        window.Show();
    }

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

    private void ApplyChanges(string texturesInput)
    {
        Debug.LogError(texturesInput);
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
    public static string GetFileNameWithoutExtension(string filePath)
    {
        int lastIndex = filePath.LastIndexOfAny(new char[] { '/', '\\' });

        if (lastIndex >= 0 && lastIndex < filePath.Length - 1)
        {
            string fileName = filePath.Substring(lastIndex + 1);
            int dotIndex = fileName.LastIndexOf('.');

            if (dotIndex > 0 && dotIndex < fileName.Length - 1)
            {
                return fileName.Substring(0, dotIndex);
            }
        }

        return null;
    }
    private void ProcessTextures(string value)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(value);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6; // 设置压缩算法为ASTC 4x4
            importer.SetPlatformTextureSettings(platformSettings);

            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + value);
        }
    }


    [MenuItem("自动化工具/_A图形的设置_选中的图片设置")]
    private static void ApplyProcessTexturesToSelected()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesA(texturePath);
            }
        }
    }

    [MenuItem("自动化工具/查查查查查查查查查查查查查查查查查查查查查查查查查查查查ClearHideFlags")]
    public static void ClearHideFlags()
    {
        Shader shader = Shader.Find("Hidden/InternalErrorShader");
        shader.hideFlags = HideFlags.None;
        EditorUtility.SetDirty(shader);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("自动化工具/清清清清清清清清清清清清理Window缓存")]
    public static void CleanCache()
    {
        string cachePath;
        string userName = System.Environment.UserName;
        cachePath = "C:\\Users\\" + userName + "\\AppData\\LocalLow\\Yoka";

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
            Debug.Log("缓存数据已成功清理。");
        }
        else
        {
            Debug.Log("未找到缓存数据目录：" + cachePath);
        }
    }
    [MenuItem("自动化工具/清清清清清清清清清清清清理平台无用音频")]
    private static void ClearPlatformUnuseRes()
    {
        var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        string path = "Assets/StreamingAssets/Audio/GeneratedSoundBanks";

        // 只有在目标路径存在时，才进行清理
        if (System.IO.Directory.Exists(path))
        {
            string[] platformsToRemove;

            if (target == BuildTarget.Android)
            {
                platformsToRemove = new[] { "iOS", "Windows" };
            }
            else if (target == BuildTarget.iOS)
            {
                platformsToRemove = new[] { "Android", "Windows" };
            }
            else if (target == BuildTarget.StandaloneWindows)
            {
                platformsToRemove = new[] { "Android", "iOS" };
            }
            else
            {
                // 其他平台不处理
                Debug.Log("当前平台不需要清理资源: " + target);
                return;
            }

            // 循环清理不需要的文件夹
            foreach (string platform in platformsToRemove)
            {
                string platformPath = System.IO.Path.Combine(path, platform);
                if (System.IO.Directory.Exists(platformPath))
                {
                    System.IO.Directory.Delete(platformPath, true);
                    Debug.Log($"{platform} 文件夹已清理");
                }
            }
        }
        else
        {
            Debug.LogWarning("资源路径不存在: " + path);
        }

        Debug.Log("已清理完毕所有打包无用资源");
    }
    [MenuItem("Build/清清清清清清清清清清清清理本版本不用场景")]
    private static void ClearUnuseArtScene()
    {
        UnuseMapsThisVersion unuseMapsThisVersion;
        string path = "Assets/Res/Map";
        unuseMapsThisVersion = Resources.Load<UnuseMapsThisVersion>("UnuseMapsThisVersion");
        // 只有在目标路径存在时，才进行清理
        if (Directory.Exists(path))
        {
            // 从 ScriptableObject 读取平台名称
            string[] platformsToRemove = unuseMapsThisVersion.removeName;

            foreach (string platform in platformsToRemove)
            {
                string platformPath = Path.Combine(path, platform);

                // 删除文件夹
                if (Directory.Exists(platformPath))
                {
                    Directory.Delete(platformPath, true);
                    Debug.Log($"{platform} 文件夹已清理");
                }

                // 删除对应的.prefab文件（如果存在）
                string prefabPath = Path.Combine(path, platform + ".prefab");
                if (File.Exists(prefabPath))
                {
                    File.Delete(prefabPath);
                    Debug.Log($"{platform}.prefab 文件已删除");
                }

                // 删除对应的.unity场景文件（如果存在）
                string scenePath = Path.Combine(path, platform + ".unity");
                if (File.Exists(scenePath))
                {
                    File.Delete(scenePath);
                    Debug.Log($"{platform}.unity 文件已删除");
                }
            }
        }
        else
        {
            Debug.LogWarning("资源路径不存在: " + path);
        }
        // 刷新编辑器窗口，确保资源状态更新
        AssetDatabase.Refresh();
        Debug.Log("已清理完毕所有打包无用资源");
    }


    private static void ProcessTexturesA(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_8x8;
            //存在两个情况1，美术命名不规范；2角色可能也用_A没有区分开
            platformSettings.maxTextureSize = 512;
            importer.sRGBTexture = true;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;
            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    [MenuItem("自动化工具/_M图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedM()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesM(texturePath);
            }
        }
    }

    private static void ProcessTexturesM(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;

            // 获取图片的名字
            string textureName = GetFileNameWithoutExtension(texturePath);

            // 判断图片名字中是否包含 "Terrain"，如果包含则开启 sRGB，否则关闭
            if (textureName.Contains("Terr", System.StringComparison.Ordinal))
            {
                importer.sRGBTexture = true;
            }
            else
            {
                importer.sRGBTexture = false;
            }

            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    [MenuItem("自动化工具/_N图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedN()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesN(texturePath);
            }
        }
    }

    private static void ProcessTexturesN(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.sRGBTexture = false;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.NormalMap;

            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    [MenuItem("自动化工具/_D图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedD()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesD(texturePath);
            }
        }
    }

    private static void ProcessTexturesD(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.sRGBTexture = true;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;

            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    [MenuItem("自动化工具/_X图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedX()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesD(texturePath);
            }
        }
    }

    private static void ProcessTexturesX(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.sRGBTexture = false;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;


            // 获取图片的名字
            string textureName = GetFileNameWithoutExtension(texturePath);

            // 判断图片名字中是否包含 "Terrain"，如果包含则开启 sRGB，否则关闭
            if (texturePath.Contains("Roles", System.StringComparison.Ordinal))
            {
                importer.sRGBTexture = true;
            }
            else
            {
                importer.sRGBTexture = false;
            }




            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }


    [MenuItem("自动化工具/_SDM图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedSDM()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesSDM(texturePath);
            }
        }
    }

    private static void ProcessTexturesSDM(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.sRGBTexture = false;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;

            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    [MenuItem("自动化工具/_R图形的设置_选中的图片压缩格式变成6*6")]
    private static void ApplyProcessTexturesToSelectedR()
    {
        // 获取编辑器中选中的所有对象
        Object[] selectedObjects = Selection.objects;

        // 遍历每个选中的对象
        foreach (Object selectedObject in selectedObjects)
        {
            // 检查选中的对象是否是 Texture2D
            if (selectedObject is Texture2D)
            {
                // 获取选择的图片的路径
                string texturePath = AssetDatabase.GetAssetPath(selectedObject);

                // 应用 ProcessTextures 逻辑
                ProcessTexturesR(texturePath);
            }
        }
    }

    private static void ProcessTexturesR(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            platformSettings.format = TextureImporterFormat.ASTC_6x6;

            importer.sRGBTexture = false;
            importer.streamingMipmaps = true;
            importer.mipmapEnabled = true;
            importer.textureType = TextureImporterType.Default;

            importer.SetPlatformTextureSettings(platformSettings);
            //alpha is transparency 不妨碍A通道读取 不管他
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }























































    static string targetShader = "Universal Render Pipeline/Lit";
    static List<string> ignoreGameObjs = new List<string>()
    {
        "Map_FB_KuiLSYS"
    };

    private static void LogicPrint(object msg)
    {
        if (msg.ToString().ToLower().Contains("resources") == true)
        {
            Debug.LogError($"<color=red>" + msg + " </color> found");
        }
        else
        {
            Debug.Log($"<color=white>" + msg +" </color> found");
        }
    }


    [MenuItem("自动化工具/骏哥工具/FixChecker/Find Target Shader")]
    private static void FindTargetMaterials()
    {
        var sd = Shader.Find(targetShader);
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.EndsWith(".mat"))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader == sd)
                {
                    LogicPrint(path);
                }
            }
        }
    }

    [MenuItem("自动化工具/骏哥工具/FixChecker/Find FBX Shader")]
    private static void SearchTargetShader()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".fbx"))
            {
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in objs)
                {
                    var gameObj = obj as GameObject;
                    if (gameObj != null && !ignoreGameObjs.Contains(gameObj.name))
                    {
                        var render = gameObj.GetComponent<Renderer>();
                        if (render != null && render.sharedMaterials != null)
                        {
                            foreach (var item in render.sharedMaterials)
                            {
                                if (item != null && item.shader != null)
                                {
                                    if (item.shader.name == targetShader)
                                    {
                                        LogicPrint(render.name + " " + path);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    [MenuItem("自动化工具/骏哥工具/FixChecker/Find Shader Text")]
    private static void SearchTargetShaderText()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".shader"))
            {
                var fullPath = Path.GetFullPath(path);
                var lines = File.ReadAllLines(fullPath);
                foreach (var line in lines)
                {
                    if (line.Contains(targetShader))
                    {
                        LogicPrint(line);
                        LogicPrint(fullPath);
                        break;
                    }
                }
            }
        }
    }

    [MenuItem("自动化工具/骏哥工具/FixChecker/Find Prefab Material")]
    private static void SearchDefaultMaterial()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".prefab"))
            {
                var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (gameObj != null)
                {
                    var renders = gameObj.GetComponentsInChildren<Renderer>(true);
                    if (renders != null && !ignoreGameObjs.Contains(gameObj.name))
                    {
                        foreach (var renderer in renders)
                        {
                            foreach (var mate in renderer.sharedMaterials)
                            {
                                if (mate != null && mate.shader != null)
                                {
                                    if (mate.shader.name == targetShader)
                                    {
                                        WriteToFile("F:/PrefabMaterial.txt", mate, renderer, gameObj, path);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("自动化工具/骏哥工具/FixChecker/Find Scene Material")]
    private static void SearchSceneMaterial()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.StartsWith("Assets/Res/Map") && path.EndsWith(".unity"))
            {
                EditorSceneManager.OpenScene(path);
                var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
                foreach (var renderer in renderers)
                {
                    foreach (var mate in renderer.sharedMaterials)
                    {
                        if (mate != null && mate.shader != null)
                        {
                            if (mate.shader.name == targetShader)
                            {
                                LogicPrint(renderer.name + " " + path);
                            }
                        }
                    }
                }
            }
        }
    }


    private static void WriteToFile(string logPath, Material mate, Renderer renderer, GameObject gameObj, string path1)
    {
        LogicPrint("mate:>" + mate + " render.name:>" + renderer + " GameObj:>" + gameObj + " Path:>" + path1);
        var _BaseMap = mate.GetTexture("_BaseMap");
        var _BumpMap = mate.GetTexture("_BumpMap");
        var lines = new[] { "材质名:>" + mate + " _BaseMap:>" + _BaseMap + " _BumpMap:>" + _BumpMap + " 节点名:>" + renderer.name + " 文件路径:>" + path1 };
        File.AppendAllLines(logPath, lines);
    }

    [MenuItem("自动化工具/骏哥工具/FixChecker/Find All Assets")]
    private static void FindAllTextures()
    {
        ShowPopupWindow.CreatePopupWindow(OnDoFindAllAsset);
    }

    static void OnDoFindAllAsset(object obj, AssetPosType type, AssetCheckType assetType)
    {
        if (string.IsNullOrEmpty(obj.ToString()))
        {
            return;
        }
        var keyword = obj.ToString().ToLower();
        System.Type currType = null;
        switch (assetType)
        {
            case AssetCheckType.Image:
                currType = typeof(Image);
                break;
            case AssetCheckType.Text:
                currType = typeof(Text);
                break;
        }
        var objs = Resources.FindObjectsOfTypeAll(currType);
        foreach (var assetObj in objs)
        {
            switch (assetType)
            {
                case AssetCheckType.Image:
                    var image = assetObj as Image;
                    if (image != null && image.sprite != null)
                    {
                        OnCheckImageAsset(image, type, keyword);
                    }
                    break;
                case AssetCheckType.Text:
                    var text = assetObj as Text;
                    if (text != null && text.font != null)
                    {
                        OnCheckTextAsset(text, type, keyword);
                    }
                    break;
            }
        }
    }

    private static void OnCheckTextAsset(Text text, AssetPosType type, string keyword)
    {
        var rootPath = GetObjRootPath(text.transform);
        if (IsConditionOK(rootPath, type))
        {
            if (text.font.name.ToLower().Contains(keyword))
            {
                var assetPath = AssetDatabase.GetAssetPath(text.font);
                Debug.LogError("Transform.Path:>" + rootPath + "  Font.Name:>" + text.font.name + "  Font.Path:>" + assetPath);
            }
        }
    }

    static void OnCheckImageAsset(Image image, AssetPosType type, string keyword)
    {
        var rootPath = GetObjRootPath(image.transform);
        if (IsConditionOK(rootPath, type))
        {
            var assetPath = AssetDatabase.GetAssetPath(image.sprite);
            if (assetPath.ToLower().Contains(keyword))
            {
                Debug.LogError("Transform.Path:>" + rootPath + "  Sprite.Name:>" + image.sprite.name + "  Atlas.Path:>" + assetPath);
            }
        }
    }

    /// <summary>
    /// 自定义判断规则
    /// </summary>
    static bool IsConditionOK(string path, AssetPosType type)
    {
        switch (type)
        {
            case AssetPosType.Memory:
                return path.StartsWith("Canvas") || path.StartsWith("Modules/RenderRoot/UICamera");
            case AssetPosType.AssetDatabase:
                return true;
        }
        return false;
    }

    static string GetObjRootPath(Transform trans)
    {
        var objNode = trans;
        var list = new List<string>();
        while (objNode != null)
        {
            list.Add(objNode.name);
            objNode = objNode.parent;
            if (objNode == null) break;
        }
        list.Reverse();
        return string.Join<string>("/", list);
    }


    const string SGAME_Lit = "SGAME/Item/SGAME_Lit";
    const string Pandavfx_v2 = "VFX/Pandavfx_v2.3_URP";



    [MenuItem("自动化工具/骏哥工具/FixChecker/Replace Material Shader Assets")]
    private static void ReplaceMateShader()
    {
        var dstShaderPath = SGAME_Lit;
        var destShader = Shader.Find(dstShaderPath);
        if (destShader == null)
        {
            Debug.LogError(dstShaderPath + " not exist!!!");
            return;
        }
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (/*!path.ToLower().Contains("/resources/") && */path.EndsWith(".mat"))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader.name == targetShader)
                {
                    mat.shader = destShader;
                    EditorUtility.SetDirty(mat);
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.LogError(dstShaderPath + " replace OK!!!");
    }

    [MenuItem("自动化工具/骏哥工具/FixChecker/Replace Prefab Shader Assets")]
    private static void ReplaceShaderAssets()
    {
        var dstShaderPath = Pandavfx_v2;
        var destShader = Shader.Find(dstShaderPath);
        if (destShader == null)
        {
            Debug.LogError(dstShaderPath + " not exist!!!");
            return;
        }
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".prefab"))
            {
                var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (gameObj != null)
                {
                    var isDirty = false;
                    var renders = gameObj.GetComponentsInChildren<Renderer>(true);
                    if (renders != null)
                    {
                        foreach (var renderer in renders)
                        {
                            foreach (var mate in renderer.sharedMaterials)
                            {
                                if (mate != null && mate.shader != null)
                                {
                                    if (mate.shader.name == targetShader)
                                    {
                                        isDirty = true;
                                        mate.shader = destShader;
                                        Debug.LogError(renderer.name + " " + gameObj);
                                    }
                                }
                            }
                        }
                    }
                    if (isDirty)
                    {
                        EditorUtility.SetDirty(gameObj);
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.LogError(dstShaderPath + " replace OK!!!");
    }
}