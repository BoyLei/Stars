///--------------------------------------------------------------------
/// 文件名   :   CreateNpcInfo.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/18 20:58:39
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CreateNpcInfo
{
    [LabelText("AvaterID")] [ValueDropdown("GetDropdownNpcList")] [OnValueChanged("OnSelectNpc")]
    public int AvaterID;

    [LabelText("模型路径")] [ReadOnly] public string ModlePath;

    [LabelText("对应的动画")] [ValueDropdown("GetClips")]
    public List<AnimationPreview> Animations;

    private ValueDropdownList<AnimationPreview> AnimationClips;

    [NonSerialized] public GameObject mModel;

    public IEnumerable GetClips()
    {
        return AnimationClips;
    }

    public IEnumerable GetDropdownNpcList()
    {
        return TimelineConfigUtils._avatars;
    }

    public void OnSelectNpc()
    {
#if UNITY_EDITOR
        if (TimelineConfigUtils._avatarData != null &&
            TimelineConfigUtils._avatarData.StaticAvatarDatas.Count > 0)
        {
            if (TimelineConfigUtils._avatarData.StaticAvatarDatas.TryGetValue(AvaterID, out var avatar))
            {
                if (MapEditor.EditorConfigUtils.ModelCfgs != null &&
                    MapEditor.EditorConfigUtils.ModelCfgs.Models != null &&
                    MapEditor.EditorConfigUtils.ModelCfgs.Models.Count > 0)
                {
                    if (MapEditor.EditorConfigUtils.ModelCfgs.Models.TryGetValue(avatar.GetModelId(),
                            out var model))
                    {
                        ModlePath = model;
                        mModel = GameObject.Instantiate(
                            UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModlePath));

                        Vector3 postion=Vector3.zero;
                        bool suc = MapEditor.MapEditorUtils.MousePosition(UnityEditor.SceneView.lastActiveSceneView, out postion);
                        mModel.transform.position = postion;
                        mModel.transform.rotation = Quaternion.identity;
                        mModel.transform.localScale = Vector3.one;
                        mModel.name = mModel.name.Replace("(Clone)", "");

                        if (UnityEditor.SceneView.lastActiveSceneView != null &&
                            UnityEditor.SceneView.lastActiveSceneView.camera != null && mModel != null)
                        {
                            UnityEditor.SceneView.lastActiveSceneView.camera.transform.position =
                                mModel.transform.position;
                        }
                    }
                }
            }


            //AnimsPath
            string dirpath = "Assets/Res/" + avatar.AnimsPath;
            if (!Directory.Exists(dirpath))
            {
                Debug.LogError($"文件路径不存在{dirpath}");
                return;
            }

            var files = Directory.GetFiles(dirpath, "*.anim");
            foreach (var file in files)
            {
                var path = UnityEditor.FileUtil.GetLogicalPath(file);

                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (AnimationClips == null)
                {
                    AnimationClips = new ValueDropdownList<AnimationPreview>();
                }

                var prew = new AnimationPreview();
                prew.AnimationClip = clip;
                prew.AnimationPath = path;
                prew.AnimationName = Path.GetFileNameWithoutExtension(path);
                prew.mModel = mModel;
                AnimationClips.Add(prew.AnimationName, prew);
            }
        }
#endif
    }
}
#endif