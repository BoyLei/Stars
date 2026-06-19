///--------------------------------------------------------------------
/// 文件名   :   CreateTimelineGUI.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/18 20:56:34
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class CreateTimelineGUI : ScriptableObject
{
    [LabelText("场景ID")] [ValueDropdown("GetDropdownMapList", DropdownHeight = 300)] [OnValueChanged("OnSelectScene")]
    public int MapID;

    [LabelText("资源组名")]
    [ValueDropdown("GetGroupType")] 
    public string GroupType="Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/MainStory";

    [LabelText("文件目录")]
    [ValueDropdown("GetGroupDir")] 
    public string GroupDir;
    
    
    
    [LabelText("是否使用玩家")] public bool UsePlayer = true;

    [LabelText(" 结束隐藏Timeline")] [FoldoutGroup("Timeline 配置文件")]
    public bool FinishHide;

    [LabelText(" 播放时隐藏UIe")] [FoldoutGroup("Timeline 配置文件")]
    public bool HideUI;

    [LabelText(" 播放时隐藏主角")] [FoldoutGroup("Timeline 配置文件")]
    public bool HideRole;

    [LabelText("播放时隐藏伙伴")] [FoldoutGroup("Timeline 配置文件")]
    public bool HidePartner;

    [LabelText("是否使用主角模型")] [FoldoutGroup("Timeline 配置文件")]
    public bool UseRoleModel;

    [LabelText("是否可移动")] [FoldoutGroup("Timeline 配置文件")]
    public bool IsCanMove;

    [LabelText("是否隐藏地图NPC")] [FoldoutGroup("Timeline 配置文件")]
    public bool HideMapNPC;

    [LabelText("是否隐藏其他玩家")] [FoldoutGroup("Timeline 配置文件")]
    public bool HideOtherPlayer;

    [LabelText("需要使用的NPC")] public List<CreateNpcInfo> Npcs;

    [LabelText("摄像机模板")] [ValueDropdown("GetCameraPath")]
    public List<string> CameraTemplete;

    [LabelText("Timeline名称")] public string TimelineName;

    [LabelText("Timeline描述")] public string Desc;
    
    [LabelText("playble文件")] [ReadOnly] public string TimelinePath;

    [LabelText("导演文件")] [ReadOnly] public string DirectorPath;

    [HideInInspector] public bool IsCreate;

    public IEnumerable GetCameraPath()
    {
        return TimelineConfigUtils._vcCameraTemplete;
    }

    public IEnumerable GetDropdownMapList()
    {
        return TimelineConfigUtils._maplist;
    }

    public IEnumerable GetGroupType()
    {
        return TimelineConfigUtils._groupTypes;
    }

    public IEnumerable GetGroupDir()
    {
        return TimelineConfigUtils._groupNames;
    }
    
    public void OnSelectScene()
    {
        TimelineCreateCache.MapID = MapID;
        if (TimelineCreateCache.SceneGameObject != null)
        {
            GameObject.DestroyImmediate(TimelineCreateCache.SceneGameObject);
        }

        TimelineConfigUtils.ClearScene();
        if (Npcs != null)
        {
            Npcs.Clear();
        }

        if (MapID > 0)
        {
            var path = string.Format("Assets/Res/Map/{0}.prefab",
                TimelineConfigUtils._SceneList[TimelineCreateCache.MapID].MapName);

            var sceneobj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (sceneobj == null)
            {
                Debug.LogError($"读取场景失败加载不到资源,{path}");
                return;
            }

            var SceneGameObject = GameObject.Instantiate(sceneobj);
            SceneGameObject.transform.position = Vector3.zero;
            SceneGameObject.transform.rotation = Quaternion.identity;
            SceneGameObject.transform.localScale = Vector3.one;
            SceneGameObject.hideFlags = HideFlags.NotEditable;
            TimelineCreateCache.SceneGameObject = SceneGameObject;
        }
    }

    private void CreatePlayerTrack(PlayableDirector Director, TimelineAsset Timeline, GroupTrack group, Transform parent,bool needCam,string trackName)
    {
        var animationTrack = Timeline.CreateTrack<AnimationTrack>(group, trackName);
        var player = GameObject.Instantiate(TimelineConfigUtils.PlayerTemplete);
        player.transform.SetParent(parent);
        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;
        player.name = player.name.Replace("(Clone)", "");
        if (Npcs != null && Npcs.Count > 0)
        {
            var position = Npcs[0].mModel.transform.position;
            var dir = Npcs[0].mModel.transform.forward;
            player.transform.position = position + dir * 3;
        }


        var AnimationAsset = animationTrack.CreateClip<AnimationPlayableAsset>();
        AnimationPlayableAsset asset = AnimationAsset.asset as AnimationPlayableAsset;
        if (asset != null)
        {
            asset.clip = TimelineConfigUtils.mIdleClip;
            asset.name = TimelineConfigUtils.mIdleClip.name;
            AnimationAsset.displayName = TimelineConfigUtils.mIdleClip.name;
            AnimationAsset.start = 0;
            AnimationAsset.duration = TimelineConfigUtils.mIdleClip.length;
        }

        Director.SetGenericBinding(animationTrack, player);
        CreateCamera(player, Director, Timeline, group, parent);
    }

    private void CreatePlayer(PlayableDirector Director, TimelineAsset Timeline, GroupTrack group, Transform parent)
    {
        //玩家
        if (UsePlayer)
        {
            //动画轨道
            CreatePlayerTrack(Director, Timeline, group, parent, true,"Player");
            
            //位移轨道
            CreatePlayerTrack(Director, Timeline, group, parent, false,"Move");
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


    public (string prefabPath,string playablePath,string VariantPath) GetTimelinePath(string timelineName)
    {
        string artDir = $"{GroupType}/{GroupDir}";
        var prefabPath = $"{artDir}/Prefab/{timelineName}.prefab";
        var playablePath = $"{artDir}/Timeline/{timelineName}.playable";
      
        var VariantPath = prefabPath.Replace("Assets/ArtWorkSpace/TimelineGroups","Assets/Res/TimeLine");
        var Variantdir = VariantPath.Replace($"/{timelineName}.prefab", string.Empty);
        
        if (System.IO.Directory.Exists(artDir))
        {
            if (!System.IO.Directory.Exists( $"{artDir}/Prefab"))
            {
                System.IO.Directory.CreateDirectory(artDir+"/Prefab"); 
            }
            
            if (!System.IO.Directory.Exists( $"{artDir}/Timeline"))
            {
                System.IO.Directory.CreateDirectory(artDir+"/Timeline"); 
            }
        }
        else
        {
            System.IO.Directory.CreateDirectory(artDir); 
            System.IO.Directory.CreateDirectory(artDir+"/Animations"); 
            System.IO.Directory.CreateDirectory(artDir+"/Materials"); 
            System.IO.Directory.CreateDirectory(artDir+"/Mesh"); 
            System.IO.Directory.CreateDirectory(artDir+"/Prefab"); 
            System.IO.Directory.CreateDirectory(artDir+"/Textures"); 
            System.IO.Directory.CreateDirectory(artDir+"/Timeline"); 
            
        }
        if (!System.IO.Directory.Exists(Variantdir))
        {
            System.IO.Directory.CreateDirectory(Variantdir); 
        }
        return (prefabPath,playablePath,VariantPath);
    }
    
    [Button("创建Timeline")]
    [HideIf("IsCreate")]
    public void CreateTimeline()
    {
#if UNITY_EDITOR
        var timelineName =
            $"Timeline_{TimelineCreateCache.MapID}_{System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss")}";
        if (!string.IsNullOrEmpty(TimelineName))
        {
            timelineName = TimelineName;
        }

        var timelinePath = GetTimelinePath(timelineName);
        GameObject go = new GameObject(timelineName);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var Director = go.AddComponent<PlayableDirector>();
        var receiver = go.AddComponent<CustomEventNotificationReceiver>();
        var timelineAsset = TimelineAsset.CreateInstance<TimelineAsset>();

       // string path = $"Assets/DevTools/TimelineEditor/Playables/{timelineName}.playable";
        UnityEditor.AssetDatabase.CreateAsset(timelineAsset, timelinePath.playablePath);
        Director.playableAsset = timelineAsset; //UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);


        //var PrefabPath = $"Assets/DevTools/TimelineEditor/Prefabs/{timelineName}.prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(go, timelinePath.prefabPath,
            UnityEditor.InteractionMode.AutomatedAction, out bool saveResult);


        GroupTrack group = timelineAsset.CreateTrack<GroupTrack>("ArtGroup");
        CreatePlayer(Director, timelineAsset, group, go.transform);

        //创建NPC 对应的动画

        if (Npcs != null && Npcs.Count > 0)
        {
            foreach (var npc in Npcs)
            {
                if (npc == null || npc.mModel == null)
                {
                    continue;
                }

                CreateNpc(Director, timelineAsset, group, go.transform, npc);
            }
        }


        //摄像机
        CreateCamera(Director, timelineAsset, group, go.transform);

        //事件
        GroupTrack eventGroupTrack = timelineAsset.CreateTrack<GroupTrack>("EventGroup");
        var EventTrack = timelineAsset.CreateTrack(typeof(CustomEventTrack), eventGroupTrack, "Events");
        Director.SetGenericBinding(EventTrack, receiver);


        TimelineName = timelineName;
        TimelinePath = timelinePath.playablePath;
        DirectorPath = timelinePath.prefabPath;

        string path_asset = $"Assets/DevTools/TimelineEditor/Configs/{timelineName}.asset";
        var vInstantiate = ScriptableObject.Instantiate(this);
        vInstantiate.IsCreate = true;
        UnityEditor.AssetDatabase.CreateAsset(vInstantiate, path_asset);
        UnityEditor.PrefabUtility.ApplyPrefabInstance(go,UnityEditor.InteractionMode.AutomatedAction);
  
        ///创建变体
       // var source = $"Assets/Res/TimeLine/GroupDisplay/{timelineName}.prefab";
       UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, timelinePath.VariantPath,out var  success);//直接保存该预制体到目录下，Unity会自动生成变体


        
        AddToTimelineExcel(timelinePath.VariantPath);

        UnityEditor.EditorUtility.SetDirty(timelineAsset);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void AddToTimelineExcel(string varpath)
    {
#if UNITY_EDITOR
        TimelineExcelInfo info = new TimelineExcelInfo();

        info.ID = 0;
        info.desc = Desc;
        info.TimelinePath = varpath.Replace("Assets/Res/", "").Replace(".prefab", "");
        info.TimelinePath = UnityEditor.FileUtil.GetLogicalPath(info.TimelinePath);
        info.FinishHide = this.FinishHide;
        info.HideUI = this.HideUI;
        info.HideRole = this.HideRole;
        info.HidePartner = info.HidePartner;
        info.UseRoleModel = info.UseRoleModel;
        info.IsCanMove = info.IsCanMove;
        info.HideMapNPC = info.HideMapNPC;
        info.HideOtherPlayer = info.HideOtherPlayer;
        TimelineConfigUtils.AddTimelineExcel(info);
#endif
    }
}
#endif