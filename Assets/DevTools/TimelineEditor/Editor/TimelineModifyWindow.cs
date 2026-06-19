using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineModifyWindow : OdinEditorWindow
{
    [Tooltip("只支持Project下的Timeline文件")]
    [OnValueChanged("OnSetGameObject")]
    [LabelText("Timeline")] public GameObject TimelineObject;

    [LabelText("需要使用的NPC")] public List<CreateNpcInfo> Npcs;
    
    [LabelText("摄像机模板")] [ValueDropdown("GetCameraPath")]
    public List<string> CameraTemplete;
    
    public IEnumerable GetCameraPath()
    {
        return TimelineConfigUtils._vcCameraTemplete;
    }
    private void OnSetGameObject()
    {
        if (TimelineObject != null)
        {
            if (TimelineObject.scene.isLoaded)
            {
                ShowNotification(new GUIContent("只支持Project下的Timeline文件"));
                TimelineObject = null;
            }
        }
    }
    public void CreateGUI()
    {
        TimelineConfigUtils.Init();
    }

    [MenuItem("Tools/Timeline编辑器/TimelineModify")]
    public static void OpenWindow()
    {
        var window = GetWindow<TimelineModifyWindow>("Timeline编辑器");
        window.Show();
    }

    [Button("修改Timeline")]
    void OnModify()
    {
        if (TimelineObject != null && Npcs != null && Npcs.Count > 0)
        {

            var timelinePath = UnityEditor.AssetDatabase.GetAssetPath(TimelineObject);

            var instance=PrefabUtility.InstantiatePrefab(TimelineObject) as GameObject;
            
            PlayableDirector Director = instance.GetComponent<PlayableDirector>();

            if (Director != null)
            {
                var timelineAsset = Director.playableAsset as TimelineAsset;

                if (timelineAsset == null)
                {
                    return;
                }

                var rootTracks = timelineAsset.GetRootTracks().ToDictionary(x => x.name, x => x);

                GroupTrack group = null;
                if (rootTracks.ContainsKey("ArtGroup"))
                {
                    group = rootTracks["ArtGroup"] as GroupTrack;
                }
                if (group == null)
                {
                    group = timelineAsset.CreateTrack<GroupTrack>("ArtGroup");
                }
                
                foreach (var npc in Npcs)
                {
                    if (npc == null || npc.mModel == null)
                    {
                        continue;
                    }
                    CreateNpc(Director, timelineAsset, group, instance.transform, npc);
                }

                CreateCamera(Director, timelineAsset, group, instance.transform);

                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(instance, timelinePath,
                    UnityEditor.InteractionMode.AutomatedAction, out bool saveResult);

                GameObject.DestroyImmediate(instance);
  
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                ShowNotification(new GUIContent("修改成功"));
            }
        }
    }

    private void CreateNpc(PlayableDirector Director, TimelineAsset Timeline, GroupTrack group, Transform parent,
        CreateNpcInfo npc)
    {
        npc.mModel.transform.SetParent(parent);
        var animationTrack = Timeline.CreateTrack(typeof(AnimationTrack), group, npc.mModel.name);
        Director.SetGenericBinding(animationTrack, npc.mModel);
        double start = 0;
        foreach (var anim in npc.Animations)
        {
            var AnimationAsset = animationTrack.CreateClip<AnimationPlayableAsset>();

            AnimationPlayableAsset asset = AnimationAsset.asset as AnimationPlayableAsset;
            if (asset != null)
            {
                asset.clip = anim.AnimationClip;
                asset.name = anim.AnimationClip.name;
                AnimationAsset.displayName = anim.AnimationClip.name;
                AnimationAsset.start = start;
                AnimationAsset.duration = start + anim.AnimationClip.length;
                start += AnimationAsset.duration;
            }
        }

        CreateCamera(npc.mModel, Director, Timeline, group, parent);
    }

    private void CreateCamera(GameObject model, PlayableDirector Director, TimelineAsset Timeline, GroupTrack group,
        Transform parent)
    {
        var vc = GameObject.Instantiate(TimelineConfigUtils.Tl_CameraTemplete);
        vc.transform.SetParent(parent);

        vc.transform.localScale = Vector3.one;
        vc.name = vc.name.Replace("(Clone)", "");

        var position = model.transform.position;
        var dir = model.transform.forward;
        vc.transform.position = position + dir * 2 + new Vector3(0, 2, 0);
        vc.transform.LookAt(model.transform.position + new Vector3(0, 1.5f, 0));
        vc.name = model.name + "_Cam";
    }
    
    
     private void CreateCamera(PlayableDirector Director, TimelineAsset Timeline, GroupTrack group, Transform parent)
    {
        //摄像机
        if (CameraTemplete!=null && CameraTemplete.Count > 0)
        {
            int index = 0;
            foreach (var path in CameraTemplete)
            {
                var cameraclip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                GameObject cam = new GameObject($"CommonCam_{index++}");
                cam.transform.SetParent(parent);
                cam.transform.position = Vector3.zero;
                cam.transform.rotation = Quaternion.identity;
                cam.transform.localScale = Vector3.one;
                if (Npcs != null && Npcs.Count > 0)
                {
                    var position = Npcs[0].mModel.transform.position;
                    var dir = Npcs[0].mModel.transform.forward;
                    cam.transform.position = position + dir * 3;
                }

                var vc = GameObject.Instantiate(TimelineConfigUtils.Tl_CameraTemplete);
                vc.transform.SetParent(cam.transform);
                vc.transform.localPosition = Vector3.zero;
                vc.transform.rotation = Quaternion.identity;
                vc.transform.localScale = Vector3.one;
                vc.name = vc.name.Replace("(Clone)", "");
                var animationTrack = Timeline.CreateTrack(typeof(AnimationTrack), group, $"VirtualCamera_{index}");

                var AnimationAsset = animationTrack.CreateClip<AnimationPlayableAsset>();
                AnimationPlayableAsset asset = AnimationAsset.asset as AnimationPlayableAsset;
                if (asset != null)
                {
                    asset.clip = cameraclip;
                    asset.name = cameraclip.name;
                    AnimationAsset.displayName = cameraclip.name;
                    AnimationAsset.start = 0;
                    AnimationAsset.duration = cameraclip.length;
                }

                Director.SetGenericBinding(animationTrack, vc);
            }
            
        }
    }

}