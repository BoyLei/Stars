using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class Es_ToolbarOtherUtils
{
    public static void OnToolbarGUI()
    {
        //设置按钮底色
        var backgroundoldColor = GUI.backgroundColor;
        GUI.backgroundColor = Es_ToolbarStyle.EstoolbarGraphicSettingButtonBGColor;
        var rect = GUILayoutUtility.GetRect(new GUIContent("其他快捷入口"), Es_ToolbarStyle.EstoolbarOtherUtilsButton);
        if (GUI.Button(rect, new GUIContent("其他快捷入口"), Es_ToolbarStyle.EstoolbarOtherUtilsButton))
        {
            PopupWindow.Show(rect, new Es_ToolbarOtherUtilsPopup());
        }
        GUI.backgroundColor = backgroundoldColor;
    }
}

public class Es_ToolbarOtherUtilsPopup : PopupWindowContent
{
    static MachineQualityLevel editorCurrentLevel = MachineQualityLevel.TopLevel;
    static class Style
    {
        private static GUIStyle s_MenuItem;
        public static GUIStyle sMenuItem => s_MenuItem ?? (s_MenuItem = "MenuItem");
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(200, 120);
    }
    private const string renderDataListFieldName = "m_RendererDataList";

    public override void OnGUI(Rect rect)
    {
        if (GUILayout.Button(new GUIContent("选择Pipeline Asset")))
        {
            var currentAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (currentAsset != null)
            {
                Selection.activeObject = currentAsset;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.Log("No Universal Render Pipeline Asset is currently assigned.");
            }
        }
        if (GUILayout.Button(new GUIContent("选择当前Pipeline Data")))
        {
            var currentAsset = UniversalRenderPipeline.asset as UniversalRenderPipelineAsset;
            if (currentAsset != null)
            {
                Selection.activeObject = currentAsset;
                ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                                                                                            .GetField(renderDataListFieldName
                                                                                                        , BindingFlags.NonPublic | BindingFlags.Instance)
                                                                                            .GetValue(UniversalRenderPipeline.asset);

                int defaultIndex = GetDefaultRendererIndex(UniversalRenderPipeline.asset);
                var defaultRendererData = m_rendererDataList[defaultIndex];

                Selection.activeObject = defaultRendererData;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.Log("No Universal Render Pipeline Asset is currently assigned.");
            }
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button(new GUIContent("选择场景Volume")))
        {
            var volume = GameObject.FindAnyObjectByType<Volume>();
            if (volume)
            {
                Selection.activeObject = volume;
            }
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button(new GUIContent("导出场景烘焙信息")))
        {
            ExportSceneLightMaps();
        }

        if (GUILayout.Button(new GUIContent("加载场景的烘焙信息")))
        {
            ApplySceneLightMaps();
        }
    }

    public static void ExportSceneLightMaps()
    {
        SGameSceneLightmapData sceneLightmapDataRoot = GameObject.FindAnyObjectByType<SGameSceneLightmapData>();


        var currentScene = SceneManager.GetActiveScene();
        if (currentScene == null)
        {
            if (EditorUtility.DisplayDialog("导出失败", "当前场景没保存？保存下再操作。", "中！"))
                return;
        }
        if (!sceneLightmapDataRoot)
        {
            if (EditorUtility.DisplayDialog("导出失败", "未找到SceneLightmapData，请给美术根节点添加【SGameSceneLightmapData】脚本!", "中！"))
                return;
        }
        else
        {
            SGameSceneLightmapData.Export(sceneLightmapDataRoot, currentScene.name);
            if (PrefabUtility.IsAnyPrefabInstanceRoot(sceneLightmapDataRoot.gameObject))
            {
                PrefabUtility.ApplyPrefabInstance(sceneLightmapDataRoot.gameObject, InteractionMode.AutomatedAction);
            }
        }
    }

    public static void ApplySceneLightMaps()
    {
        SGameSceneLightmapData sceneLightmapDataRoot = GameObject.FindAnyObjectByType<SGameSceneLightmapData>();

        if (!sceneLightmapDataRoot)
        {
            EditorUtility.DisplayDialog("导入失败", "场景未找到SceneLightmapData,请检查场景Prefab是否为最新,地编是否有把光照贴图信息导入至Prefab中!",
                "中！");

        }
        else
        {
            if (sceneLightmapDataRoot.MeshRenderLightDatas.Count == 0)
            {
                EditorUtility.DisplayDialog("导入失败", "Prefab中没有光照贴图数据,请检查场景Prefab是否为最新,地编是否有把光照贴图信息导入至Prefab中!", "中！");
            }
            else
                sceneLightmapDataRoot.ApplyToMeshRenderers();
        }
    }


    static int GetDefaultRendererIndex(UniversalRenderPipelineAsset asset)
    {
        return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset); ;
    }
}