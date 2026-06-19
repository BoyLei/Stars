using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.IO;

public class SetShadowToUnStatic : EditorWindow
{
    private static string folderPath = "Assets/ArtWorkSpace/Scenes";
    private static string BlackListPath = "Assets/ArtWorkSpace/Scenes\\CommonObstacle\\Collection";
    private static string shadowShadeName = "SGAME/SimpleShadowShader";
    private bool refreshRenderQueue = false;
    private static string statusMessage = "";

    [MenuItem("自动化工具/TA工具/刷新所有ArtWorkSpace-Scenes目录下Prefab中的代理阴影为UnStatic并设置Static的设置")]
    public static void ShowWindow()
    {
        GetWindow<SetShadowToUnStatic>("Set Shadow to UnStatic");
    }

    private void OnGUI()
    {
        GUILayout.Label("设置选项", EditorStyles.boldLabel);
        
        GUILayout.Label("该插件会刷新ArtWorkSpace-Scenes下Prefab中的代理阴影为Unstatic并刷新Static的设置", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Prefab目录路径", folderPath);
        
        GUILayout.Label("黑名单目录为Assets/ArtWorkSpace/Scenes/CommonObstacle/Collection", EditorStyles.boldLabel);
        
        GUILayout.Space(30);
        refreshRenderQueue = EditorGUILayout.Toggle("刷新带AlphaClip的渲染队列", refreshRenderQueue);

        if (GUILayout.Button("开始刷新"))
        {
            SetShadowToStatic();
        }

        GUILayout.Space(10);
        GUILayout.Label(statusMessage, EditorStyles.helpBox);
    }

    private static void SetShadowToStatic()
    {
        statusMessage = "正在刷新，请稍候...";
        EditorUtility.DisplayProgressBar("刷新中", "正在处理 Prefab...", 0);

        try
        {
            // 获取项目中所有的 Prefab 路径
            string[] prefabPaths = Directory.GetFiles(folderPath, "*_Pfb.prefab", SearchOption.AllDirectories);
            // 用于存放过滤后的文件列表
            List<string> filteredFiles = new List<string>();
            
            foreach (var file in prefabPaths)
            {
                // 检查文件是否在黑名单目录中
                if (!file.StartsWith(BlackListPath) )
                {
                    filteredFiles.Add(file);
                }
            }
            

            for (int i = 0; i < filteredFiles.Count; i++)
            {
                string prefabPath = filteredFiles[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError("无法加载 Prefab：" + prefabPath);
                    continue;
                }
            
                Transform[] allTransforms = prefab.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    Renderer renderer = t.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // 处理投影代理模型
                        if (t.name.EndsWith("_Shadow"))
                        {
                            // 检查材质是否为投影材质
                            if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader.name.Contains(shadowShadeName))
                            {
                                // 取消 static 设置
                                renderer.transform.gameObject.isStatic = false;
                            }
                        }
                        // 处理非投影代理模型
                        else
                        {
                            var flags = StaticEditorFlags.ContributeGI |
                                        StaticEditorFlags.OccludeeStatic |
                                        StaticEditorFlags.OccluderStatic |
                                        StaticEditorFlags.OffMeshLinkGeneration |
                                        StaticEditorFlags.ReflectionProbeStatic;
                            GameObjectUtility.SetStaticEditorFlags(renderer.transform.gameObject, flags);
                        }
                    }
                }
                PrefabUtility.SavePrefabAsset(prefab);
            
                // 更新进度条
                EditorUtility.DisplayProgressBar("刷新中", "正在处理 Prefab: " + prefabPath, (float)i / filteredFiles.Count);
            }

            if (instance.refreshRenderQueue)
            {
                RefreshRenderQueue(folderPath);
            }

            //AssetDatabase.Refresh();
            statusMessage = "刷新完成：" + folderPath;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("处理过程中出现错误：" + ex.Message);
            statusMessage = "刷新失败：" + ex.Message;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log(statusMessage);
    }

    private static void RefreshRenderQueue(string path)
    {
        EditorUtility.DisplayProgressBar("刷新中", "正在处理材质...", 0);

        try
        {
            string[] materialPaths = Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories);
            
            // 用于存放过滤后的文件列表
            List<string> filteredFiles = new List<string>();
            
            foreach (var file in materialPaths)
            {
                // 检查文件是否在黑名单目录中
                if (!file.StartsWith(BlackListPath) )
                {
                    filteredFiles.Add(file);
                }
            }
            
            for (int i = 0; i < filteredFiles.Count; i++)
            {
                string materialPath = filteredFiles[i];
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null) continue;

                Shader shader = material.shader;
                if (shader.name == "SGAME/SGAME_Scene_01" ||
                    shader.name == "GPUInstancer/SGAME/SGAME_Scene_01" ||
                    shader.name == "GPUInstancer/SGAME/SGAME_Scene_leaf"||
                    shader.name == "SGAME/SGAME_Scene_leaf")
                {
                    if (material.HasProperty("_Cutoff") && material.GetFloat("_Cutoff") > 0 &&
                        material.IsKeywordEnabled("_ALPHACLIP"))
                    {
                        material.renderQueue = 2450;
                        EditorUtility.SetDirty(material);
                        Debug.Log("设置渲染队列： " + materialPath);
                    }
                }

                // 确保保存所有更改
                AssetDatabase.SaveAssets();
                // 更新进度条
                EditorUtility.DisplayProgressBar("刷新中", "正在处理材质: " + materialPath, (float)i / filteredFiles.Count);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("处理材质时出现错误：" + ex.Message);
            statusMessage = "处理材质失败：" + ex.Message;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static SetShadowToUnStatic instance
    {
        get { return (SetShadowToUnStatic)GetWindow(typeof(SetShadowToUnStatic), false, "Set Shadow to UnStatic"); }
    }
}