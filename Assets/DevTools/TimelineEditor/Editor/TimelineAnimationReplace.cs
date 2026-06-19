///--------------------------------------------------------------------
/// 文件名   :   TimelineAnimationReplace.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/07/17 17:26:18
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class TimelineAnimationReplace
{
    [MenuItem("Assets/动画/替换Timeline Clip动画")]
    static public void Execute()
    {
        var go = Selection.activeObject as GameObject;

        if (go == null)
        {
            return;
        }
        var director = go.GetComponent<PlayableDirector>();
        if (director != null)
        {
            foreach (var item in director.playableAsset.outputs)
            {
                if (item.sourceObject != null)
                {
                    if (item.sourceObject is AnimationTrack track)
                    {
                        foreach (var timelineClip in track.GetClips())
                        {
                            var SourcePath = AssetDatabase.GetAssetPath(timelineClip.animationClip);
                            if (SourcePath.Contains("ArtWorkSpace/Roles/World"))
                            {
                                var newPath = SourcePath.Replace("Animation/", "");
                                newPath = newPath.Replace("ArtWorkSpace/Roles/World", "Res/Animation/Roles");

                                string[] temps = newPath.Split('/');
                                var temp = temps[temps.Length - 1].Split("@");
                                var clipName = temp[1].Replace(".FBX", ".anim");

                                string p = string.Empty;
                                for (int i = 0; i < temps.Length - 1; i++)
                                {
                                    p += temps[i] + "/";
                                }

                                newPath = p + clipName;

                                Debug.Log($" clipName={clipName} OldPath={SourcePath}  NewPath={newPath}");

                                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newPath);
                                if (clip != null)
                                {
                                    //
                                    Debug.Log($"加载到新的资源");

                                    AnimationPlayableAsset asset = timelineClip.asset as AnimationPlayableAsset;
                                    if (asset != null)
                                    {
                                        asset.clip = clip;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(go);
        EditorUtility.SetDirty(director.playableAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}