#if UNITY_EDITOR   
//公用的东西不能放在editor下，Sobj不能放。
//Editor其他文件夹内容容易被访问不到。
//被包裹就是当成类型能访问到，Editor下的特性也能访问到。兼容editor的特性+只在editor下运行+能被访问
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public class SceneOpenListener
{
    static SceneOpenListener()
    {
        // 获取场景名称列表配置的 Scriptable 对象实例
        SceneNameListConfig sceneNameListConfig = GetSceneNameListConfig();

        EditorSceneManager.sceneOpened += (scene, mode) => OnSceneOpened(scene, mode, sceneNameListConfig);
    }

    public static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode, SceneNameListConfig sceneNameListConfig)
    {
        foreach (string targetSceneName in sceneNameListConfig.sceneNames)
        {
            if (scene.name == targetSceneName && scene.name.Length == targetSceneName.Length)
            {
                Debug.Log("Scene opened: " + targetSceneName);

                MeshRenderer[] meshRenderers = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    if (meshRenderer.gameObject.layer == LayerMask.NameToLayer("AkEvents")//音频组一定隐藏
                        ||
                        meshRenderer.gameObject.layer == LayerMask.NameToLayer("Wall")//collider一定隐藏
                        )
                    {
                        //一定策划的
                        meshRenderer.enabled = false;


                    }
                    else
                    {

                        if (meshRenderer.gameObject.layer == LayerMask.NameToLayer("Ground") && meshRenderer.gameObject.transform.parent.name.Contains("canrun", System.StringComparison.OrdinalIgnoreCase))//unicode忽略大小写包含canrun就是策划一定隐藏
                        //帮美术显示正常，憋给策划给覆盖了)
                        {
                            //canrun是策划的
                            meshRenderer.enabled = false;
                        }
                        else
                        {
                            //美术也会用ground ，其他都是美术的
                            meshRenderer.enabled = true;
                        }


                    }
                
                }
            }
        }
    }

    public static SceneNameListConfig GetSceneNameListConfig()
    {
        // 修改 "SceneNameListConfig" 为您自己的 Scriptable 对象实例的名称
        string sceneNameListConfigAssetPath = "Assets/ArtWorkSpace/Tools/SceneNameListConfig.asset";
        SceneNameListConfig sceneNameListConfig = AssetDatabase.LoadAssetAtPath<SceneNameListConfig>(sceneNameListConfigAssetPath);

        if (sceneNameListConfig == null)
        {
            Debug.LogError("Failed to load SceneNameListConfig at path: " + sceneNameListConfigAssetPath);
        }

        return sceneNameListConfig;
    }
}
#endif
