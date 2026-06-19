using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

public static class ParticleCheck
{
    [MenuItem("Assets/资源检查/特效/检查选中特效", false, 1)]
    public static void CheckSelect()
    {
        GameObject obj = (GameObject)Selection.activeObject;
        if (obj != null)
        {
            if (!IsParticle(obj))
            {
                var check = CheckAsset(obj, true);
                if (check.result && !string.IsNullOrEmpty(check.message))
                {
                    Debug.LogError(check.message);
                }
            }
        }
    }

    private static (bool result, string message) CheckAsset(GameObject go, bool useModify = false)
    {
        ParticleSystem[] ps = go.transform.GetComponentsInChildren<ParticleSystem>();
        if (go == null || ps == null || ps.Length < 1)
        {
            return (false, null);
        }

        StringBuilder sb = new StringBuilder();
        //粒子材质数量
        //粒子最大数量
        //粒子Prewarm 是否开启
        List<string> list = new List<string>();
        sb.AppendLine($"粒子路径:{AssetDatabase.GetAssetPath(go)}");
        sb.AppendLine($"子粒子数量:{ps.Length}");
        int maxCount = 0;
        int tCount = 0;
        foreach (var p in ps)
        {
            var renderer = p.gameObject.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                maxCount += p.main.maxParticles;
                var main = p.main;
                if (main.loop && !main.prewarm)
                {
                    sb.AppendLine($"{renderer.gameObject.name} 的prewarm 未开启");
                }

                if (p.emission.enabled && p.emission.rateOverTime.constant > 15)
                {
                    sb.AppendLine($"{renderer.gameObject.name} 粒子发射速率不能高于15");
                }

                if (renderer.mesh != null)
                {
                    if (renderer.mesh.vertices.Length > 17000)
                    {
                        sb.AppendLine($"{renderer.gameObject.name}  模型的顶点数大于17000");
                    }

                    if (renderer.mesh.triangles.Length > 25000)
                    {
                        sb.AppendLine($"{renderer.gameObject.name}  模型的三角面数大于25000");
                    }
                }

                if (renderer.sharedMaterial == null)
                {
                    sb.AppendLine($"{renderer.gameObject.name} 材质为空");
                    continue;
                }


                if (!list.Contains(renderer.sharedMaterial.name))
                    list.Add(renderer.sharedMaterial.name);

                int textureCount = ShaderUtil.GetPropertyCount(renderer.sharedMaterial.shader);

                for (int i = 0; i < textureCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(renderer.sharedMaterial.shader, i) ==
                        ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(renderer.sharedMaterial.shader, i);
                        Texture texture = renderer.sharedMaterial.GetTexture(propertyName);

                        if (texture != null)
                        {
                            if (texture.width > 1024)
                            {
                                sb.AppendLine(
                                    $"{renderer.gameObject.name} 节点下 {renderer.sharedMaterial.name} 贴图 {texture.name} 宽超过 1024");
                            }

                            if (texture.height > 1024)
                            {
                                sb.AppendLine(
                                    $"{renderer.gameObject.name} 节点下 {renderer.sharedMaterial.name} 贴图 {texture.name} 高超过 1024");
                            }

                            tCount++;
                        }
                    }
                }
            }
        }
        sb.AppendLine($"粒子贴图数量:{tCount}");
        if (maxCount > 100)
        {
            sb.AppendLine($"子粒子数量:{maxCount}");
        }

        sb.AppendLine($"材质数量{list.Count}");

        if (useModify)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("检查信息", $"粒子存在不合规范，是否自动修复 {sb.ToString()} ", "确定", "取消"))
            {
                FixParticle(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        return (true, sb.ToString());
    }

    private static bool IsParticle(GameObject go)
    {
        if (go == null)
        {
            return false;
        }

        ParticleSystem[] ps = go.transform.GetComponentsInChildren<ParticleSystem>();
        if (ps == null || ps.Length < 1)
        {
            return false;
        }

        return true;
    }

    private static void FixParticle(GameObject go)
    {
        if (go != null)
        {
            ParticleSystem[] ps = go.transform.GetComponentsInChildren<ParticleSystem>();
            if (ps != null && ps.Length > 0)
            {
                foreach (var p in ps)
                {
                    var main = p.main;
                    if (main.loop && !main.prewarm)
                    {
                        main.prewarm = true;
                    }

                    if (p.emission.enabled && p.emission.rateOverTime.constant > 15)
                    {
                        var emission = p.emission;
                        var rateOverTime = emission.rateOverTime;
                        rateOverTime.constant = 15;
                        emission.rateOverTime = rateOverTime;
                    }
                }
            }
            UnityEditor.EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("自动化工具/资源检查/特效/检查所有特效", false, 1)]
    public static void FxCheck()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(CheckAll());
    }

    [MenuItem("自动化工具/资源检查/特效/修复所有特效", false, 2)]
    public static void FixAllFx()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(FixAll());
    }

    static IEnumerator FixAll()
    {
        List<string> paths = new List<string>();
        string[] files = AssetDatabase.GetAllAssetPaths();
        foreach (var path in files)
        {
            if (path.StartsWith("Assets/Resources/Map"))
            {
                continue;
            }
            if ((path.Contains("ArtWorkSpace") || path.Contains("Res") || path.Contains("Resources")) &&
                path.EndsWith(".prefab"))
            {
                paths.Add(path);
            }
        }

        EditorUtility.ClearProgressBar();
        for (int i = 0; i < paths.Count; i++)
        {
            EditorUtility.DisplayProgressBar($"修复特效({i}/{paths.Count})", $"{paths[i]}", i * 1.0f / (paths.Count - 1));
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (go != null)
            {
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(go);

                if (prefabType == PrefabAssetType.Regular)
                {
                    if (IsParticle(go))
                    {
                        FixParticle(go);
                    }
                }
            }
            yield return null;
        }
        yield return null;
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static IEnumerator CheckAll()
    {
        StringBuilder sb = new StringBuilder();
        List<string> paths = new List<string>();
        string[] files = AssetDatabase.GetAllAssetPaths();
        foreach (var path in files)
        {
            if (path.StartsWith("Assets/Resources/Map"))
            {
                continue;
            }
            if ((path.Contains("ArtWorkSpace") || path.Contains("Res") || path.Contains("Resources")) &&
                path.EndsWith(".prefab"))
            {
                paths.Add(path);
            }
        }

        EditorUtility.ClearProgressBar();

        for (int i = 0; i < paths.Count; i++)
        {
            EditorUtility.DisplayProgressBar($"检查特效({i}/{paths.Count})", $"{paths[i]}", i * 1.0f / (paths.Count - 1));
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (go != null)
            {
                if (IsParticle(go))
                {
                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(go);

                    if (prefabType == PrefabAssetType.Regular)
                    {
                        var check = CheckAsset(go);
                        if (check.result && !string.IsNullOrEmpty(check.message))
                        {
                            sb.AppendLine(check.message);
                        }
                    }
                }
            }

            yield return null;
        }

        yield return null;
        EditorUtility.ClearProgressBar();
        string filePath = Application.dataPath + "/../特效检查报告.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        File.WriteAllText(filePath, sb.ToString());
        EditorUtility.OpenWithDefaultApp(filePath);
    }
}