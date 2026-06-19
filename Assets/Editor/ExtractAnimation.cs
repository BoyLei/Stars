///--------------------------------------------------------------------
/// 文件名   :   ExtractAnimation.cs
/// 内  容   :   提取动画
/// 说  明   :  
/// 创建日期 :   2024/04/23 11:32:42
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExtractAnimation
{
    [MenuItem("Assets/动画/提取动画", false, 2)]
    public static void ExtractAnimationToRes()
    {
        var selection = Selection.activeObject;
        if (selection == null)
        {
            return;
        }

        string artRolePath = AssetDatabase.GetAssetPath(selection);
        if (!artRolePath.Contains("Assets/ArtWorkSpace/Roles/World"))
        {
            Debug.LogError("请选择Assets/ArtWorkSpace/Roles/World 下的文件目录");
            return;
        }

        var dir = artRolePath.Replace("Assets/ArtWorkSpace/Roles/World", string.Empty);
        Debug.Log($"当前选中的目录 {artRolePath}");
        //美术文件目录
        //资源文件目录
        string resRolePath = "Assets/Res/Animation/Roles" + dir;

        //拿到所有FBX和Anim
        List<string> fbxs = new();
        var fbxConfigs = Directory.GetFiles(artRolePath, "*.FBX", SearchOption.AllDirectories);
        var clipConfigs = Directory.GetFiles(artRolePath, "*.anim", SearchOption.AllDirectories);
        if (fbxConfigs != null)
        {
            foreach (var item in fbxConfigs)
            {
                //如果FBX的路径不在Animation下面，就不用导了
                if (item.Contains("\\Animation\\", StringComparison.OrdinalIgnoreCase))
                {
                    fbxs.Add(item);
                }
            }
        }


        //遍历所有FBX，不考虑目标文件，直接导出到目标文件里面，如果有同名的就覆盖一下
        foreach (var fbx in fbxs)
        {
            //读取FBX下所有动作Clip
            var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(fbx.ToString()).OfType<AnimationClip>().ToArray();
            if (objs != null && objs.Length != 0)
            {
                //先判断有没有路径，没有就要创建一个路径

                string curTargetPath = fbx.Replace(artRolePath, resRolePath)
                    .Replace($"\\Animation\\{Path.GetFileName(fbx)}", "");
                curTargetPath = FileUtil.GetPhysicalPath(curTargetPath);
                if (!File.Exists(curTargetPath))
                {
                    var str = curTargetPath;
                    if (!Directory.Exists(curTargetPath))
                    {
                        Directory.CreateDirectory(curTargetPath);
                    }
                }

                //遍历所有obj
                foreach (var obj in objs)
                {
                    //如果是Anim就需要拷贝过去
                    if (obj.name != "__preview__Take 001")
                    {
                        try
                        {
                            AnimationClip clip = new();
                            UnityEditor.EditorUtility.CopySerialized(obj, clip);
                            UnityEditor.AssetDatabase.CreateAsset(clip, $"{artRolePath}/{obj.name}.anim");
                            FileInfo go = new($"{artRolePath}/{obj.name}.anim");
                            go.CopyTo($"{curTargetPath}/{obj.name}.anim", true);
                            UnityEditor.AssetDatabase.DeleteAsset($"{artRolePath}/{obj.name}.anim");
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        //遍历所有anim
        foreach (var clipconfig in clipConfigs)
        {
            //如果clip的路径不在Animation下面，就不用导了
            if (!clipconfig.Contains("\\Animation\\", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            //针对每个clip单独处理
            AnimationClip curClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(clipconfig);
            //先判断有没有路径，没有就要创建一个路径
            string curTargetPath = clipconfig.Replace(artRolePath, resRolePath)
                .Replace($"\\Animation\\{Path.GetFileName(clipconfig)}", "");
            curTargetPath = FileUtil.GetPhysicalPath(curTargetPath);
            if (!File.Exists(curTargetPath))
            {
                var str = curTargetPath;
                if (!Directory.Exists(curTargetPath))
                {
                    Directory.CreateDirectory(curTargetPath);
                }
            }

            //如果是Anim就需要拷贝过去
            if (curClip.name != "__preview__Take 001")
            {
                try
                {
                    AnimationClip newClip = new();
                    UnityEditor.EditorUtility.CopySerialized(curClip, newClip);
                    UnityEditor.AssetDatabase.CreateAsset(newClip, $"{artRolePath}/{curClip.name}.anim");
                    FileInfo go = new($"{artRolePath}/{curClip.name}.anim");
                    go.CopyTo($"{curTargetPath}/{curClip.name}.anim", true);
                    UnityEditor.AssetDatabase.DeleteAsset($"{artRolePath}/{curClip.name}.anim");
                }
                catch
                {
                    continue;
                }
            }
        }

        Debug.Log("动画提取成功");

        UnityEditor.AssetDatabase.Refresh();
    }
}