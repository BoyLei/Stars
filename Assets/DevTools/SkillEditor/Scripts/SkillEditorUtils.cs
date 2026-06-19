///--------------------------------------------------------------------
/// 文件名   :   SkillEditorUtils.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/14 16:53:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Newtonsoft.Json;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using StarProject;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static UnityEngine.ParticleSystem;
using Formatting = Newtonsoft.Json.Formatting;

namespace SkillEditor
{
    public static class SkillEditorUtils
    {

        static T DeepCopy<T>(T RealObject)
        {
            try
            {
                using (Stream objectStream = new MemoryStream())
                {
                    //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(objectStream, RealObject);
                    objectStream.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(objectStream);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            return default;
        }


        //public static T DeepCopy<T>(T obj)
        //{
        //    if (obj == null)
        //    {
        //        return default;
        //    }
        //    if (obj is string || obj.GetType().IsValueType)
        //        return obj;

        //    object retval = Activator.CreateInstance(obj.GetType());

        //    FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        //    foreach (var field in fields)
        //    {
        //        try
        //        {
        //            var fieldValue = field.GetValue(obj);
        //            if (fieldValue == null)
        //            {
        //                continue;
        //            }

        //            if(fieldValue is ICollection)
        //            {
        //                continue;
        //               // field.SetValue(retval, (fieldValue as ICollection).);
        //            }
        //            if (fieldValue is ICloneable)
        //            {
        //                field.SetValue(retval, (fieldValue as ICloneable).Clone());
        //            }
        //            else
        //            {
        //                field.SetValue(retval, DeepCopy(fieldValue));
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            Debug.LogException(exception);
        //            Debug.LogError($"field.Name={field.Name} field.FieldType={field.FieldType} Vaule={field.GetValue(obj)}");
        //        }
        //    }

        //    return (T)retval;
        //}

        static public List<StageJson> ExportStageJson(TrackAsset groupTrack, string infos,ref int effectIndex)
        {
#if UNITY_EDITOR
            if (groupTrack != null)
            {
                var outputs = groupTrack.GetChildTracks();
                if (outputs != null)
                {
                    List<StageTrack> stageTracks = new List<StageTrack>();
                    List<StarAnimatorTrack> animationTracks = new List<StarAnimatorTrack>();
                    List<StarControlTrack> controlTracks = new List<StarControlTrack>();
                    List<StarCinemachineTrack> cinemachineTracks = new List<StarCinemachineTrack>();
                    List<StarWWiseTrack> audioTracks = new List<StarWWiseTrack>();
                    List<EffectTrack> effectTracks = new List<EffectTrack>();
                    List<InputEffectTrack> inputeffectTracks = new List<InputEffectTrack>();
                    List<CameraShakeTrack> cameraShakeTracks = new List<CameraShakeTrack>();

                    foreach (var item in outputs)
                    {
                        if (item is StageTrack stagetrack)
                        {
                            stageTracks.Add(stagetrack);
                        }
                        else if (item is StarAnimatorTrack animationTrack)
                        {
                            animationTracks.Add(animationTrack);
                        }
                        else if (item is StarControlTrack controlTrack)
                        {
                            controlTracks.Add(controlTrack);
                        }
                        else if (item is StarCinemachineTrack cinemachineTrack)
                        {
                            cinemachineTracks.Add(cinemachineTrack);
                        }
                        else if (item is StarWWiseTrack audioTrack)
                        {
                            audioTracks.Add(audioTrack);
                        }
                        else if (item is EffectTrack effectTrack)
                        {
                            effectTracks.Add(effectTrack);
                        }
                        else if (item is InputEffectTrack inputEffectTrack)
                        {
                            inputeffectTracks.Add(inputEffectTrack);
                        }
                        else if (item is CameraShakeTrack cameraShake)
                        {
                            cameraShakeTracks.Add(cameraShake);
                        }
                    }

                    if (stageTracks.Count != 1)
                    {
                        Debug.LogError($"{infos}的阶段轴有且只有一个,当前阶段轴数量:{stageTracks.Count}" );
                        return null;
                    }
                    int Index = 0;
                    List<StageJson> stageJsons = new List<StageJson>();
                    bool isFaild = false;
                    int stageID = 0;
                    foreach (var item in stageTracks)
                    {
                        double start = 0;

                        List<TimelineClip> timelineClip = new List<TimelineClip>(item.GetClips());

                        timelineClip.Sort(delegate (TimelineClip x, TimelineClip y)
                        {
                            if (x.start > y.start)
                                return 1;
                            else
                                return -1;
                        });

                        foreach (var clip in timelineClip)
                        {
                            if (Mathf.Abs((int)(clip.start * 1000) - (int)(start * 1000)) > 2)
                            {
                                isFaild = true;
                                Debug.LogError($"阶段不连续，阶段开始在：{clip.start} {infos}");
                            }
                            else
                            {
                                StageJson stage = new StageJson();
                                stage.Start = (int)(clip.start * 1000);
                                stage.End = (int)(clip.end * 1000);
                                stage.Duration = (int)(clip.duration * 1000);
                                if (clip.asset is ClipStageAsset stageAsset)
                                {
                                    stage.StageNormal = stageAsset.template.data;
                                    stage.StageID = stage.StageNormal.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StageNormal.StageType;
                                }
                                else if (clip.asset is TriggerStageAsset triggerStage)
                                {
                                    stage.StageTrigger = triggerStage.template.data;
                                    stage.StageID = stage.StageTrigger.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StageTrigger.StageType;
                                }
                                else if (clip.asset is AddBuffStageAsset buffStageAsset)
                                {
                                    stage.StageBUFFStart = buffStageAsset.template.data;
                                    stage.StageID = stage.StageBUFFStart.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StageBUFFStart.StageType;
                                }
                                else if (clip.asset is BuffEndStageAsset endStageAsset)
                                {
                                    stage.StageBUFFEnd = endStageAsset.template.data;
                                    stage.StageID = stage.StageBUFFEnd.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StageBUFFEnd.StageType;
                                }
                                else if (clip.asset is BulletStageAsset bulletStageAsset)
                                {
                                    stage.StageBulletMotion = bulletStageAsset.template.data;
                                    stage.StageID = stage.StageBulletMotion.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StageBulletMotion.StageType;
                                }
                                else if (clip.asset is PassiveEndStageAsset passiveEndStageAsset)
                                {
                                    stage.StagePassiveEnd = passiveEndStageAsset.template.data;
                                    stage.StageID = stage.StagePassiveEnd.StageID;
                                    stageID = stage.StageID;
                                    stage.StageType = (StageType)stage.StagePassiveEnd.StageType;
                                }
                                stageJsons.Add(stage);
                            }

                            start = clip.end;
                        }
                    }

                    if (isFaild)
                    {
                        Debug.LogError($"阶段不连续：{stageID} {infos}");
                        return null;
                    }


                    //动画
                    foreach (var item in animationTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段{infos}");
                            }

                            AnimationJson animation = new AnimationJson();
                            animation.Start = (int)((clip.start) * 1000) - json.Start;
                            animation.End = (int)((clip.end) * 1000) - json.Start;
                            animation.Duration = (int)(clip.duration * 1000);
                            animation.ClipIn = (int)(clip.clipIn * 1000);
                            //animation.clipCaps = (int)clip.clipCaps;
                            animation.SpeedMultiplier = clip.timeScale;

                            ClipStarAnimatorAsset animationClip = (ClipStarAnimatorAsset)clip.asset;

                            animation.AnimatorCustomData = animationClip.template.AnimatorCustomData;
                            animation.FootIK = animationClip.applyFootIK;
                            animation.Loop = (int)animationClip.loop;
                            if (clip.animationClip != null)
                            {
                                animation.ClipName = clip.animationClip.name;
                                animation.ClipPath = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(clip.animationClip));
                            }
                            animation.Index = Index++;
                            json.Animations.Add(animation);
                        }
                    }


                    //特效
                    foreach (var item in controlTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段{infos}");
                            }

                            FXJson fX = new FXJson();
                            fX.Start = (int)((clip.start) * 1000) - json.Start;
                            fX.End = (int)((clip.end) * 1000) - json.Start;
                            fX.Duration = (int)(clip.duration * 1000);
                            fX.ClipIn = (int)(clip.clipIn * 1000);
                            //fX.clipCaps = (int)clip.clipCaps;
                            fX.SpeedMultiplier = clip.timeScale;
                            ClipStarControlAsset fxClip = (ClipStarControlAsset)clip.asset;

                            fX.config = fxClip.template.configEffect;
                            fX.ControlActivation = fxClip.active;
                            fX.postPlayback = (int)fxClip.postPlayback;
                            fX.ControlPlayableDirectors = fxClip.updateDirector;
                            fX.ControlParticleSystems = fxClip.updateParticle;
                            fX.ControlTimeControl = fxClip.updateITimeControl;
                            //fX.ControlChildren =fxClip.ch;
                            fX.RandomSpeed = fxClip.particleRandomSeed;
                            if (fxClip.prefabGameObject != null)
                            {
                                fX.FxDuration = 0;
                                ParticleSystem psFx = fxClip.prefabGameObject.GetComponent<ParticleSystem>();
                                if (psFx != null)
                                {
                                    // 如果 特效 是粒子的时候,将 粒子的时间导出来, 避免 技能做 效果恢复的时候，
                                    // 因为不知道 特效的长度,而要加载每个特效.
                                    fX.FxDuration = GetFxRootReallyDuration(psFx);

                                }
                                fX.EffectName = fxClip.prefabGameObject.name;
                                fX.EffectPath = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(fxClip.prefabGameObject));
                                fX.IsCameraImpulse = fX.EffectName.StartsWith("Impulse_");
                                fX.IsCamaeraOffset = fX.EffectName.StartsWith("CameraOffset_");
                            }

                            //   Debug.LogError($"fX.EffectName={fX.EffectName} StageID={json.StageID} StageStart={json.Start}  StageEnd={json.End} fX.Start={fX.Start} clip.start={clip.start} ");
                            fX.Index = Index++;
                            json.Fxs.Add(fX);
                        }
                    }


                    //Cameras
                    foreach (var item in cinemachineTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段{infos}");
                            }

                            CameraJson camera = new CameraJson();
                            camera.Start = (int)((clip.start) * 1000) - json.Start;
                            camera.End = (int)((clip.end) * 1000) - json.Start;
                            camera.Duration = (int)(clip.duration * 1000);
                            camera.ClipIn = (int)(clip.clipIn * 1000);
                            //camera.clipCaps = (int)clip.clipCaps;
                            StarCinemachineShot cameraClip = (StarCinemachineShot)clip.asset;
                            if (cameraClip != null)
                            {
                                if (cameraClip.VirtualCamera.defaultValue != null)
                                {
                                    camera.ClipName = cameraClip.VirtualCamera.defaultValue.name;
                                    camera.ClipPath = UnityEditor.AssetDatabase.GetAssetPath(cameraClip.VirtualCamera.defaultValue);
                                }
                                camera.CameraCustomData = cameraClip.template.CameraCustomData;
                            }
                            camera.Index = Index++;
                            json.Cameras.Add(camera);
                        }
                    }


                    //音效
                    foreach (var item in audioTracks)
                    {
                        foreach (var marker in item.GetMarkers())
                        {
                            if (marker is WWiseEventMarker eventMarker)
                            {
                                bool instage = false;
                                StageJson json = null;
                                foreach (var stage in stageJsons)
                                {
                                    if (stage.InStage(eventMarker.time, item.isRight))
                                    {
                                        instage = true;
                                        json = stage;
                                        break;
                                    }
                                }

                                if (!instage || json == null)
                                {
                                    Debug.LogError($"不在任何阶段{infos}");
                                }

                                SoundJson sound = new SoundJson();
                                sound.Start = (int)((eventMarker.time) * 1000) - json.Start;
                                sound.End = sound.Start;
                                sound.EventName = eventMarker.akEvent.Name;
                                sound.SoundEventID = eventMarker.akEvent.Id;

                                sound.Index = Index++;
                                json.Sounds.Add(sound);
                            }
                        }
                    }
                    //效果
                    foreach (var item in effectTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段：{infos}");
                            }

                            EffectJosn effectJosn = new EffectJosn();

                            ClipEffectAsset effectAsset = (ClipEffectAsset)clip.asset;

                            //效果查重
                            List<int> EffectIDs = new List<int>();
                            List<int> NextEffectID = new List<int>();
                            for (int j = 0; j < effectAsset.template.frameEffect.Count; j++)
                            {
                                var source = effectAsset.template.frameEffect[j];
                                if (source != null)
                                {
                                    if (EffectIDs.Contains(source.EffectID))
                                    {
                                        Debug.LogError($"{infos}的{groupTrack.name} {item.name} {clip.displayName} 效果ID重复 {source.EffectID}");
                                    }
                                    else
                                    {
                                        EffectIDs.Add(source.EffectID);
                                        if (source.Next != null && source.Next.Length > 0)
                                        {
                                            foreach (var nt in source.Next)
                                            {
                                                NextEffectID.Add(nt);
                                            }
                                        }
                                    }
                                }

                            }

                            if (EffectIDs.Count > 1)
                            {
                                for (int i = 1; i < EffectIDs.Count; i++)
                                {
                                    if (!NextEffectID.Contains(EffectIDs[i]))
                                    {
                                        Debug.LogError($"{infos}的{groupTrack.name} {item.name} {clip.displayName}  {EffectIDs[i]} 没有被引用");
                                    }
                                }
                            }

                            if (NextEffectID.Count > 0)
                            {
                                for (int i = 0; i < NextEffectID.Count; i++)
                                {
                                    if (!EffectIDs.Contains(NextEffectID[i]))
                                    {
                                        Debug.LogError($"{infos}的{groupTrack.name} {item.name} {clip.displayName}  {NextEffectID[i]} 引用空数据");
                                    }
                                }
                            }


                            effectJosn.Start = (int)((clip.start) * 1000) - (json != null ? json.Start : 0);
                            effectJosn.Duration = (int)(clip.duration * 1000);
                            if (effectJosn.data == null)
                            {
                                effectJosn.data = new List<EffectData>();
                            }

                            for (int j = 0; j < effectAsset.template.frameEffect.Count; j++)
                            {
                                var source = effectAsset.template.frameEffect[j];
                                var it = SkillEditorUtils.DeepCopy<EffectData>(source);
                                if (it == null)
                                {
                                    continue;
                                }

                                //用外部传进来的索引进行计算
                                it.EffectID = effectIndex * 1000 + it.EffectID;
                                if (source.Next != null && source.Next.Length > 0)
                                {
                                    it.Next = new int[source.Next.Length];
                                    for (int i = 0; i < it.Next.Length; i++)
                                    {
                                        it.Next[i] = effectIndex * 1000 + source.Next[i];
                                    }
                                }
                                effectJosn.data.Add(it);
                            }
                            effectJosn.ModifyTime();
                            if (json != null)
                            {
                                json.Effects.Add(effectJosn);
                                effectIndex++;
                            }

                        }
                    }

                    //用户输入效果
                    foreach (var item in inputeffectTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段{infos}");
                            }

                            EffectJosn effectJosn = new EffectJosn();

                            ClipInputEffectAsset effectAsset = (ClipInputEffectAsset)clip.asset;
                            effectJosn.Start = (int)((clip.start) * 1000) - json.Start;
                            effectJosn.Duration = (int)(clip.duration * 1000);
                            if (effectJosn.data == null)
                            {
                                effectJosn.data = new List<EffectData>();
                            }
                            ClipInputEffectBehaviour effectBehaviour = effectAsset.template;
                            if (effectBehaviour != null)
                            {
                                if (effectBehaviour.inputEffect != null)
                                {
                                    var source = effectBehaviour.inputEffect;
                                    var effectData = SkillEditorUtils.DeepCopy<EffectData>(source);
                                    if (effectData != null)
                                    {
                                        effectData.EffectID = effectIndex * 1000 + source.EffectID;
                                        if (source.Next != null && source.Next.Length > 0)
                                        {
                                            effectData.Next = new int[source.Next.Length];
                                            for (int i = 0; i < source.Next.Length; i++)
                                            {
                                                effectData.Next[i] = effectIndex * 1000 + source.Next[i];
                                            }
                                        }
                                        if (effectData.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeUserInput))
                                        {
                                            (effectData.EffectArgs.BaseEffect as EffectTypeUserInput).OpenInput = true;
                                        }
                                        if (effectData.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeChargeInput))
                                        {
                                            (effectData.EffectArgs.BaseEffect as EffectTypeChargeInput).OpenInput = true;
                                        }


                                        effectJosn.data.Add(effectData);
                                    }
                                }

                                if (effectBehaviour.frameEffect != null && effectBehaviour.frameEffect.Count > 0)
                                {


                                    //效果查重
                                    List<int> EffectIDs = new List<int>();
                                    List<int> NextEffectID = new List<int>();
                                    for (int j = 0; j < effectBehaviour.frameEffect.Count; j++)
                                    {
                                        var source = effectAsset.template.frameEffect[j];
                                        if (source != null)
                                        {
                                            if (EffectIDs.Contains(source.EffectID) || source.EffectID == effectBehaviour.inputEffect.EffectID)
                                            {
                                                Debug.LogError($"{groupTrack.name} {item.name} {clip.displayName} 效果ID重复 {source.EffectID}");
                                            }
                                            else
                                            {
                                                EffectIDs.Add(source.EffectID);
                                                if (source.Next != null && source.Next.Length > 0)
                                                {
                                                    foreach (var nt in source.Next)
                                                    {
                                                        NextEffectID.Add(nt);
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    if (EffectIDs.Count > 1)
                                    {
                                        for (int i = 1; i < EffectIDs.Count; i++)
                                        {
                                            if (!NextEffectID.Contains(EffectIDs[i]))
                                            {
                                                Debug.LogError($"{groupTrack.name} {item.name} {clip.displayName}  {EffectIDs[i]} 没有被引用");
                                            }
                                        }
                                    }

                                    if (NextEffectID.Count > 0)
                                    {
                                        for (int i = 0; i < NextEffectID.Count; i++)
                                        {
                                            if (!EffectIDs.Contains(NextEffectID[i]))
                                            {
                                                Debug.LogError($"{groupTrack.name} {item.name} {clip.displayName}  {NextEffectID[i]} 引用空数据");
                                            }
                                        }
                                    }



                                    for (int j = 0; j < effectBehaviour.frameEffect.Count; j++)
                                    {
                                        var effect = effectBehaviour.frameEffect[j];
                                        if (effect == null)
                                        {
                                            continue;
                                        }
                                        var copyeffect = SkillEditorUtils.DeepCopy<EffectData>(effect);
                                        if (copyeffect != null)
                                        {
                                            copyeffect.EffectID = effectIndex * 1000 + copyeffect.EffectID;
                                            if (effect.Next != null && effect.Next.Length > 0)
                                            {
                                                copyeffect.Next = new int[effect.Next.Length];
                                                for (int i = 0; i < copyeffect.Next.Length; i++)
                                                {
                                                    copyeffect.Next[i] = effectIndex * 1000 + effect.Next[i];
                                                }
                                            }

                                            effectJosn.data.Add(copyeffect);
                                        }
                                    }
                                }
                                effectJosn.ModifyTime();
                                json.Effects.Add(effectJosn);
                                effectIndex++;

                            }

                            //bool endinstage = false;
                            //StageJson endjson = null;

                            //foreach (var stage in stageJsons)
                            //{
                            //    if (stage.InStage(clip.end))
                            //    {
                            //        endinstage = true;
                            //        endjson = stage;
                            //        break;
                            //    }
                            //}

                            //if (!endinstage || endjson == null)
                            //{
                            //    Debug.LogError("不在任何阶段");
                            //}

                            //if (json.StageType == StageType.NormalStage)
                            //{
                            //    if (json.StageNormal.LoopCount != 1 && endjson.StageID != json.StageID)
                            //    {
                            //        Debug.LogError("输入在循环阶段，结束不在同一个阶段");
                            //    }
                            //}
                            //else
                            //{
                            //    Debug.LogError("输入轴只能导入一般阶段");
                            //}

                            //EffectJosn endeffectJosn = new EffectJosn();

                            //ClipInputEffectAsset endeffectAsset = (ClipInputEffectAsset)clip.asset;
                            //endeffectJosn.Start = (int)((clip.start) * 1000) - endjson.Start;
                            //endeffectJosn.Duration = (int)(clip.duration * 1000);
                            //if (endeffectJosn.data == null)
                            //{
                            //    endeffectJosn.data = new List<EffectData>();
                            //}
                            //ClipInputEffectBehaviour endeffectBehaviour = effectAsset.template;
                            //if (endeffectBehaviour != null)
                            //{
                            //    if (endeffectBehaviour.inputEffect != null)
                            //    {
                            //        var endeffectData = SkillEditorUtils.DeepCopy<EffectData>(endeffectBehaviour.inputEffect);
                            //        if (endeffectData != null)
                            //        {
                            //            endeffectData.EffectID = endeffectAsset.Index * 1000 + endeffectData.EffectID;
                            //            if (endeffectData.Next != null && endeffectData.Next.Length > 0)
                            //            {
                            //                for (int i = 0; i < endeffectData.Next.Length; i++)
                            //                {
                            //                    endeffectData.Next[i] = endeffectAsset.Index * 1000 + endeffectData.Next[i];
                            //                }
                            //            }

                            //            endeffectData.EffectArgs.UserInput.OpenInput = false;
                            //            endeffectJosn.data.Add(endeffectData);
                            //        }
                            //    }

                            //    if (endeffectBehaviour.frameEffect != null && endeffectBehaviour.frameEffect.Count > 0)
                            //    {
                            //        for (int j = 0; j < endeffectBehaviour.frameEffect.Count; j++)
                            //        {
                            //            var effect = endeffectBehaviour.frameEffect[j];
                            //            if (effect == null)
                            //            {
                            //                continue;
                            //            }
                            //            var copyeffect = SkillEditorUtils.DeepCopy<EffectData>(effect);
                            //            if (copyeffect != null)
                            //            {
                            //                copyeffect.EffectID = endeffectAsset.Index * 1000 + copyeffect.EffectID;
                            //                if (copyeffect.Next != null && copyeffect.Next.Length > 0)
                            //                {
                            //                    for (int i = 0; i < copyeffect.Next.Length; i++)
                            //                    {
                            //                        copyeffect.Next[i] = endeffectAsset.Index * 1000 + copyeffect.Next[i];
                            //                    }
                            //                }

                            //                endeffectJosn.data.Add(copyeffect);
                            //            }
                            //        }
                            //    }

                            //    endeffectJosn.ModifyTime();
                            //    endjson.Effects.Add(endeffectJosn);
                            //}

                        }
                    }

                    //摄像机震动
                    foreach (var item in cameraShakeTracks)
                    {
                        foreach (var clip in item.GetClips())
                        {
                            bool instage = false;
                            StageJson json = null;
                            foreach (var stage in stageJsons)
                            {
                                if (stage.InStage(clip.start, item.isRight))
                                {
                                    instage = true;
                                    json = stage;
                                    break;
                                }
                            }

                            if (!instage || json == null)
                            {
                                Debug.LogError($"不在任何阶段{infos}");
                            }
                            CameraShakeJson shakeJson = new CameraShakeJson();
                            shakeJson.Start = (int)((clip.start) * 1000) - json.Start;
                            shakeJson.End = (int)((clip.end) * 1000) - json.Start;
                            shakeJson.Duration = (int)(clip.duration * 1000);
                            shakeJson.ClipIn = (int)(clip.clipIn * 1000);
                            //shakeJson.clipCaps = (int)clip.clipCaps;
                            shakeJson.SpeedMultiplier = clip.timeScale;
                            CameraShakeClip fxClip = (CameraShakeClip)clip.asset;

                            shakeJson.VirtualCameraName = fxClip.VirtualCameraName;
                            shakeJson.AmplitudeGain = fxClip.AmplitudeGain;
                            shakeJson.FrequencyGain = fxClip.FrequencyGain;

                            shakeJson.Index = Index++;
                            json.ShakeCameras.Add(shakeJson);
                        }
                    }

                    foreach (var item in stageJsons)
                    {
                        if (item.Animations.Count == 0 && item.Fxs.Count == 0)
                        {
                            if (item.StageType == StageType.NormalStage)
                            {
                                item.StageNormal.IsMsgClient = true;
                            }
                            else if (item.StageType == StageType.BulletStage)
                            {
                                item.StageBulletMotion.IsMsgClient = true;
                            }
                            else if (item.StageType == StageType.AddBuffStage)
                            {
                                item.StageBUFFStart.IsMsgClient = true;
                            }
                            else if (item.StageType == StageType.EndBuffStage)
                            {
                                item.StageBUFFEnd.IsMsgClient = true;
                            }
                            else if (item.StageType == StageType.EndPassiveStage)
                            {
                                item.StagePassiveEnd.IsMsgClient = true;
                            }
                            else if (item.StageType == StageType.TriggerStage)
                            {
                                item.StageTrigger.IsMsgClient = true;
                            }
                        }
                        else
                        {
                            if (item.StageType == StageType.NormalStage)
                            {
                                item.StageNormal.IsMsgClient = false;
                            }
                            else if (item.StageType == StageType.BulletStage)
                            {
                                item.StageBulletMotion.IsMsgClient = false;
                            }
                            else if (item.StageType == StageType.AddBuffStage)
                            {
                                item.StageBUFFStart.IsMsgClient = false;
                            }
                            else if (item.StageType == StageType.EndBuffStage)
                            {
                                item.StageBUFFEnd.IsMsgClient = false;
                            }
                            else if (item.StageType == StageType.EndPassiveStage)
                            {
                                item.StagePassiveEnd.IsMsgClient = false;
                            }
                            else if (item.StageType == StageType.TriggerStage)
                            {
                                item.StageTrigger.IsMsgClient = false;
                            }
                        }
                        item.Sort();
                    }

                    return stageJsons;
                }
            }
#endif
            return null;
        }


//        static public List<StageJson> ExportStageJson(PlayableDirector director)
//        {
//#if UNITY_EDITOR
//            if (director != null)
//            {
//                var outputs = director.playableAsset.outputs;

//                if (outputs != null)
//                {


//                    List<StageTrack> stageTracks = new List<StageTrack>();
//                    List<StarAnimatorTrack> animationTracks = new List<StarAnimatorTrack>();
//                    List<StarControlTrack> controlTracks = new List<StarControlTrack>();
//                    List<StarCinemachineTrack> cinemachineTracks = new List<StarCinemachineTrack>();
//                    List<StarWWiseTrack> audioTracks = new List<StarWWiseTrack>();
//                    List<EffectTrack> effectTracks = new List<EffectTrack>();
//                    List<InputEffectTrack> inputeffectTracks = new List<InputEffectTrack>();
//                    List<CameraShakeTrack> cameraShakeTracks = new List<CameraShakeTrack>();

//                    foreach (var item in outputs)
//                    {
//                        if (item.sourceObject is StageTrack stagetrack)
//                        {
//                            stageTracks.Add(stagetrack);
//                        }
//                        else if (item.sourceObject is StarAnimatorTrack animationTrack)
//                        {
//                            animationTracks.Add(animationTrack);
//                        }
//                        else if (item.sourceObject is StarControlTrack controlTrack)
//                        {
//                            controlTracks.Add(controlTrack);
//                        }
//                        else if (item.sourceObject is StarCinemachineTrack cinemachineTrack)
//                        {
//                            cinemachineTracks.Add(cinemachineTrack);
//                        }
//                        else if (item.sourceObject is StarWWiseTrack audioTrack)
//                        {
//                            audioTracks.Add(audioTrack);
//                        }
//                        else if (item.sourceObject is EffectTrack effectTrack)
//                        {
//                            effectTracks.Add(effectTrack);
//                        }
//                        else if (item.sourceObject is InputEffectTrack inputEffectTrack)
//                        {
//                            inputeffectTracks.Add(inputEffectTrack);
//                        }
//                        else if (item.sourceObject is CameraShakeTrack cameraShake)
//                        {
//                            cameraShakeTracks.Add(cameraShake);
//                        }
//                    }

//                    if (stageTracks.Count != 1)
//                    {
//                        Debug.LogError("阶段轴有且只有一个");
//                        return null;
//                    }
//                    int Index = 0;
//                    List<StageJson> stageJsons = new List<StageJson>();
//                    bool isFaild = false;
//                    foreach (var item in stageTracks)
//                    {
//                        double start = 0;
//                        foreach (var clip in item.GetClips())
//                        {
//                            if (Mathf.Abs((int)(clip.start * 1000) - (int)(start * 1000)) > 2)
//                            {
//                                isFaild = true;
//                                break;
//                            }
//                            else
//                            {
//                                StageJson stage = new StageJson();
//                                stage.Start = (int)(clip.start * 1000);
//                                stage.End = (int)(clip.end * 1000);
//                                stage.Duration = (int)(clip.duration * 1000);
//                                if (clip.asset is ClipStageAsset stageAsset)
//                                {
//                                    stage.StageNormal = stageAsset.template.data;
//                                }
//                                else if (clip.asset is TriggerStageAsset triggerStage)
//                                {
//                                    stage.StageTrigger = triggerStage.template.data;
//                                }
//                                else if (clip.asset is AddBuffStageAsset buffStageAsset)
//                                {
//                                    stage.StageBUFFStart = buffStageAsset.template.data;
//                                }
//                                else if (clip.asset is BuffEndStageAsset endStageAsset)
//                                {
//                                    stage.StageBUFFEnd = endStageAsset.template.data;
//                                }
//                                else if (clip.asset is BulletStageAsset bulletStageAsset)
//                                {
//                                    stage.StageBulletMotion = bulletStageAsset.template.data;
//                                }
//                                else if (clip.asset is PassiveEndStageAsset passiveEndStageAsset)
//                                {
//                                    stage.StagePassiveEnd = passiveEndStageAsset.template.data;
//                                }
//                                stageJsons.Add(stage);
//                                start = clip.end;
//                            }
//                        }
//                    }

//                    if (isFaild)
//                    {
//                        Debug.LogError("阶段不连续");
//                        return null;
//                    }

//                    Index = 0;
//                    //动画
//                    foreach (var item in animationTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }

//                            AnimationJson animation = new AnimationJson();
//                            animation.Start = (int)((clip.start) * 1000) - json.Start;
//                            animation.End = (int)((clip.end) * 1000) - json.Start;
//                            animation.Duration = (int)(clip.duration * 1000);
//                            animation.ClipIn = (int)(clip.clipIn * 1000);
//                            //animation.clipCaps = (int)clip.clipCaps;
//                            animation.SpeedMultiplier = clip.timeScale;

//                            ClipStarAnimatorAsset animationClip = (ClipStarAnimatorAsset)clip.asset;

//                            animation.AnimatorCustomData = animationClip.template.AnimatorCustomData;
//                            animation.FootIK = animationClip.applyFootIK;
//                            animation.Loop = (int)animationClip.loop;
//                            if (clip.animationClip != null)
//                            {
//                                animation.ClipName = clip.animationClip.name;
//                                animation.ClipPath = UnityEditor.AssetDatabase.GetAssetPath(clip.animationClip);
//                            }
//                            animation.Index = Index++;
//                            json.Animations.Add(animation);
//                        }
//                    }

//                    Index = 0;
//                    //特效
//                    foreach (var item in controlTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }

//                            FXJson fX = new FXJson();
//                            fX.Start = (int)((clip.start) * 1000) - json.Start;
//                            fX.End = (int)((clip.end) * 1000) - json.Start;
//                            fX.Duration = (int)(clip.duration * 1000);
//                            fX.ClipIn = (int)(clip.clipIn * 1000);
//                            //fX.clipCaps = (int)clip.clipCaps;
//                            fX.SpeedMultiplier = clip.timeScale;
//                            ClipStarControlAsset fxClip = (ClipStarControlAsset)clip.asset;

//                            fX.config = fxClip.template.configEffect;
//                            fX.ControlActivation = fxClip.active;
//                            fX.postPlayback = (int)fxClip.postPlayback;
//                            fX.ControlPlayableDirectors = fxClip.updateDirector;
//                            fX.ControlParticleSystems = fxClip.updateParticle;
//                            fX.ControlTimeControl = fxClip.updateITimeControl;
//                            //fX.ControlChildren =fxClip.ch;
//                            fX.RandomSpeed = fxClip.particleRandomSeed;
//                            if (fxClip.prefabGameObject != null)
//                            {
//                                fX.FxDuration = 0;
//                                ParticleSystem psFx = fxClip.prefabGameObject.GetComponent<ParticleSystem>();
//                                if (psFx != null)
//                                {
//                                    // 如果 特效 是粒子的时候,将 粒子的时间导出来, 避免 技能做 效果恢复的时候，
//                                    // 因为不知道 特效的长度,而要加载每个特效.
//                                    fX.FxDuration = GetFxRootReallyDuration(psFx);
//                                }
//                                fX.EffectName = fxClip.prefabGameObject.name;
//                                fX.EffectPath = UnityEditor.AssetDatabase.GetAssetPath(fxClip.prefabGameObject);
//                            }
//                            fX.IsCameraImpulse = fX.EffectName.StartsWith("Impulse_");
//                            fX.IsCamaeraOffset = fX.EffectName.StartsWith("CameraOffset_");
//                            //   Debug.LogError($"fX.EffectName={fX.EffectName} StageID={json.StageID} StageStart={json.Start}  StageEnd={json.End} fX.Start={fX.Start} clip.start={clip.start} ");
//                            fX.Index = Index++;
//                            json.Fxs.Add(fX);
//                        }
//                    }

//                    Index = 0;
//                    //Cameras
//                    foreach (var item in cinemachineTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }

//                            CameraJson camera = new CameraJson();
//                            camera.Start = (int)((clip.start) * 1000) - json.Start;
//                            camera.End = (int)((clip.end) * 1000) - json.Start;
//                            camera.Duration = (int)(clip.duration * 1000);
//                            camera.ClipIn = (int)(clip.clipIn * 1000);
//                            //camera.clipCaps = (int)clip.clipCaps;
//                            StarCinemachineShot cameraClip = (StarCinemachineShot)clip.asset;
//                            if (cameraClip != null)
//                            {
//                                if (cameraClip.VirtualCamera.defaultValue != null)
//                                {


//                                    camera.ClipName = cameraClip.VirtualCamera.defaultValue.name;
//                                    camera.ClipPath =
//                                        UnityEditor.AssetDatabase.GetAssetPath(cameraClip.VirtualCamera.defaultValue);
//                                }
//                                camera.CameraCustomData = cameraClip.template.CameraCustomData;
//                            }
//                            camera.Index = Index++;
//                            json.Cameras.Add(camera);
//                        }
//                    }

//                    Index = 0;

//                    //音效
//                    foreach (var item in audioTracks)
//                    {
//                        foreach (var marker in item.GetMarkers())
//                        {
//                            if (marker is WWiseEventMarker eventMarker)
//                            {
//                                bool instage = false;
//                                StageJson json = null;
//                                foreach (var stage in stageJsons)
//                                {
//                                    if (stage.InStage(eventMarker.time, item.isRight))
//                                    {
//                                        instage = true;
//                                        json = stage;
//                                        break;
//                                    }
//                                }

//                                if (!instage || json == null)
//                                {
//                                    Debug.LogError("不在任何阶段");
//                                }

//                                SoundJson sound = new SoundJson();
//                                sound.Start = (int)((eventMarker.time) * 1000) - json.Start;
//                                sound.End = sound.Start;
//                                sound.EventName = eventMarker.akEvent.Name;
//                                sound.Index = Index++;
//                                json.Sounds.Add(sound);
//                            }
//                        }
//                    }
//                    //效果
//                    foreach (var item in effectTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }




//                            EffectJosn effectJosn = new EffectJosn();

//                            ClipEffectAsset effectAsset = (ClipEffectAsset)clip.asset;
//                            ClipEffectBehaviour effectBehaviour = effectAsset.template;
//                            if (effectBehaviour != null)
//                            {
//                                effectJosn.Start = (int)((clip.start) * 1000) - json.Start;
//                                effectJosn.Duration = (int)(clip.duration * 1000);
//                                if (effectJosn.data == null)
//                                {
//                                    effectJosn.data = new List<EffectData>();
//                                }

//                                for (int j = 0; j < effectAsset.template.frameEffect.Count; j++)
//                                {
//                                    var source = effectAsset.template.frameEffect[j];
//                                    var it = SkillEditorUtils.DeepCopy<EffectData>(source);
//                                    if (it == null)
//                                    {
//                                        continue;
//                                    }

//                                    it.EffectID = effectAsset.Index * 1000 + it.EffectID;
//                                    if (source.Next != null && source.Next.Length > 0)
//                                    {
//                                        it.Next = new int[source.Next.Length];
//                                        for (int i = 0; i < it.Next.Length; i++)
//                                        {
//                                            it.Next[i] = effectAsset.Index * 1000 + source.Next[i];
//                                        }
//                                    }
//                                    effectJosn.data.Add(it);
//                                }
//                            }
//                            effectJosn.ModifyTime();
//                            json.Effects.Add(effectJosn);
//                        }
//                    }

//                    //用户输入效果
//                    foreach (var item in inputeffectTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }

//                            EffectJosn effectJosn = new EffectJosn();

//                            ClipInputEffectAsset effectAsset = (ClipInputEffectAsset)clip.asset;
//                            effectJosn.Start = (int)((clip.start) * 1000) - json.Start;
//                            effectJosn.Duration = (int)(clip.duration * 1000);
//                            if (effectJosn.data == null)
//                            {
//                                effectJosn.data = new List<EffectData>();
//                            }
//                            ClipInputEffectBehaviour effectBehaviour = effectAsset.template;
//                            if (effectBehaviour != null)
//                            {
//                                if (effectBehaviour.inputEffect != null)
//                                {
//                                    var source = effectBehaviour.inputEffect;
//                                    var effectData = SkillEditorUtils.DeepCopy<EffectData>(source);
//                                    if (effectData != null)
//                                    {
//                                        effectData.EffectID = effectAsset.Index * 1000 + source.EffectID;
//                                        if (source.Next != null && source.Next.Length > 0)
//                                        {
//                                            effectData.Next = new int[source.Next.Length];
//                                            for (int i = 0; i < source.Next.Length; i++)
//                                            {
//                                                effectData.Next[i] = effectAsset.Index * 1000 + source.Next[i];
//                                            }
//                                        }
//                                        (effectData.BaseEffect as EffectTypeUserInput).OpenInput = true;
//                                        effectJosn.data.Add(effectData);
//                                    }
//                                }

//                                if (effectBehaviour.frameEffect != null && effectBehaviour.frameEffect.Count > 0)
//                                {
//                                    for (int j = 0; j < effectBehaviour.frameEffect.Count; j++)
//                                    {
//                                        var effect = effectBehaviour.frameEffect[j];
//                                        if (effect == null)
//                                        {
//                                            continue;
//                                        }
//                                        // var source = effectBehaviour.inputEffect;

//                                        var copyeffect = SkillEditorUtils.DeepCopy<EffectData>(effect);
//                                        if (copyeffect != null)
//                                        {
//                                            copyeffect.EffectID = effectAsset.Index * 1000 + copyeffect.EffectID;
//                                            if (effect.Next != null && effect.Next.Length > 0)
//                                            {
//                                                copyeffect.Next = new int[effect.Next.Length];
//                                                for (int i = 0; i < copyeffect.Next.Length; i++)
//                                                {
//                                                    copyeffect.Next[i] = effectAsset.Index * 1000 + effect.Next[i];
//                                                }
//                                            }

//                                            effectJosn.data.Add(copyeffect);
//                                        }
//                                    }
//                                }
//                                effectJosn.ModifyTime();
//                                json.Effects.Add(effectJosn);

//                            }

//                            bool endinstage = false;
//                            StageJson endjson = null;

//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.end, item.isRight))
//                                {
//                                    endinstage = true;
//                                    endjson = stage;
//                                    break;
//                                }
//                            }

//                            if (!endinstage || endjson == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }


//                            if (json.StageType == StageType.NormalStage)
//                            {
//                                if (json.StageNormal.LoopCount != 1 && endjson.StageID != json.StageID)
//                                {
//                                    Debug.LogError("输入在循环阶段，结束不在同一个阶段");
//                                }
//                            }
//                            else
//                            {
//                                Debug.LogError("输入轴只能导入一般阶段");
//                            }


//                            EffectJosn endeffectJosn = new EffectJosn();

//                            ClipInputEffectAsset endeffectAsset = (ClipInputEffectAsset)clip.asset;
//                            endeffectJosn.Start = (int)((clip.start) * 1000) - endjson.Start;
//                            endeffectJosn.Duration = (int)(clip.duration * 1000);
//                            if (endeffectJosn.data == null)
//                            {
//                                endeffectJosn.data = new List<EffectData>();
//                            }
//                            ClipInputEffectBehaviour endeffectBehaviour = effectAsset.template;
//                            if (endeffectBehaviour != null)
//                            {
//                                if (endeffectBehaviour.inputEffect != null)
//                                {
//                                    var endeffectData = SkillEditorUtils.DeepCopy<EffectData>(endeffectBehaviour.inputEffect);
//                                    if (endeffectData != null)
//                                    {
//                                        endeffectData.EffectID = endeffectAsset.Index * 1000 + endeffectData.EffectID;
//                                        if (endeffectData.Next != null && endeffectData.Next.Length > 0)
//                                        {
//                                            for (int i = 0; i < endeffectData.Next.Length; i++)
//                                            {
//                                                endeffectData.Next[i] = endeffectAsset.Index * 1000 + endeffectData.Next[i];
//                                            }
//                                        }

//                                        (endeffectData.BaseEffect as EffectTypeUserInput).OpenInput = true;
//                                        endeffectJosn.data.Add(endeffectData);
//                                    }
//                                }

//                                if (endeffectBehaviour.frameEffect != null && endeffectBehaviour.frameEffect.Count > 0)
//                                {
//                                    for (int j = 0; j < endeffectBehaviour.frameEffect.Count; j++)
//                                    {
//                                        var effect = endeffectBehaviour.frameEffect[j];
//                                        if (effect == null)
//                                        {
//                                            continue;
//                                        }
//                                        var copyeffect = SkillEditorUtils.DeepCopy<EffectData>(effect);
//                                        if (copyeffect != null)
//                                        {
//                                            copyeffect.EffectID = endeffectAsset.Index * 1000 + copyeffect.EffectID;
//                                            if (copyeffect.Next != null && copyeffect.Next.Length > 0)
//                                            {
//                                                for (int i = 0; i < copyeffect.Next.Length; i++)
//                                                {
//                                                    copyeffect.Next[i] = endeffectAsset.Index * 1000 + copyeffect.Next[i];
//                                                }
//                                            }

//                                            endeffectJosn.data.Add(copyeffect);
//                                        }
//                                    }
//                                }

//                                endeffectJosn.ModifyTime();
//                                endjson.Effects.Add(endeffectJosn);
//                            }

//                        }
//                    }

//                    //摄像机震动
//                    foreach (var item in cameraShakeTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {
//                            bool instage = false;
//                            StageJson json = null;
//                            foreach (var stage in stageJsons)
//                            {
//                                if (stage.InStage(clip.start, item.isRight))
//                                {
//                                    instage = true;
//                                    json = stage;
//                                    break;
//                                }
//                            }

//                            if (!instage || json == null)
//                            {
//                                Debug.LogError("不在任何阶段");
//                            }
//                            CameraShakeJson shakeJson = new CameraShakeJson();
//                            shakeJson.Start = (int)((clip.start) * 1000) - json.Start;
//                            shakeJson.End = (int)((clip.end) * 1000) - json.Start;
//                            shakeJson.Duration = (int)(clip.duration * 1000);
//                            shakeJson.ClipIn = (int)(clip.clipIn * 1000);
//                            //shakeJson.clipCaps = (int)clip.clipCaps;
//                            shakeJson.SpeedMultiplier = clip.timeScale;
//                            CameraShakeClip fxClip = (CameraShakeClip)clip.asset;

//                            shakeJson.VirtualCameraName = fxClip.VirtualCameraName;
//                            shakeJson.AmplitudeGain = fxClip.AmplitudeGain;
//                            shakeJson.FrequencyGain = fxClip.FrequencyGain;

//                            shakeJson.Index = Index++;
//                            json.ShakeCameras.Add(shakeJson);
//                        }
//                    }

//                    foreach (var item in stageJsons)
//                    {
//                        item.Sort();
//                    }

//                    return stageJsons;
//                }
//            }
//#endif
//            return null;
//        }

//        static public List<MotionJson> ExportMotionJson(PlayableDirector director)
//        {
//#if UNITY_EDITOR
//            if (director != null)
//            {
//                var outputs = director.playableAsset.outputs;

//                if (outputs != null)
//                {
//                    List<StarMotionTrack> motionTracks = new List<StarMotionTrack>();
//                    foreach (var item in outputs)
//                    {
//                        if (item.sourceObject is StarMotionTrack motionTrack)
//                        {
//                            motionTracks.Add(motionTrack);
//                        }
//                    }
//                    List<MotionJson> motionJsons = new List<MotionJson>();
//                    int Index = 0;
//                    foreach (var item in motionTracks)
//                    {
//                        foreach (var clip in item.GetClips())
//                        {

//                            MotionJson motion = new MotionJson();
//                            motion.Start = (int)((clip.start) * 1000);
//                            motion.End = (int)((clip.end) * 1000);
//                            motion.Duration = (int)(clip.duration * 1000);
//                            ClipStarMotionAsset motionAsset = (ClipStarMotionAsset)clip.asset;
//                            motion.config = motionAsset.template.motionData;
//                            motion.Index = Index++;
//                            motionJsons.Add(motion);
//                        }
//                    }
//                    return motionJsons;
//                }
//            }
//#endif
//            return null;
//        }
        static public string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        static public string GeneraGUID()
        {
            string guid = string.Empty;
#if UNITY_EDITOR
            guid = UnityEditor.GUID.Generate().ToString();
#endif
            return guid;
        }

        static public int GetFxRootReallyDuration(ParticleSystem ps)
        {
            ParticleSystem[] childPs = ps.GetComponentsInChildren<ParticleSystem>();

            float maxDuration = 0;
            foreach (ParticleSystem item in childPs)
            {
                if (item == ps)
                {
                    continue;
                }
                ParticleSystemRenderer rendererModule = item.GetComponent<ParticleSystemRenderer>();
                if (rendererModule == null || !rendererModule.enabled)
                {
                    // Debug.LogError($"skip fx : {ps.name}");
                    continue;
                }
                GetFxItemReallyDuration(item, out float delayTime, out float startLifeTime);
                maxDuration = Math.Max(maxDuration, delayTime + startLifeTime);
            }
            // Debug.LogError($" fx : {ps.name}  maxDuration : {maxDuration}");

            return (int)(maxDuration * 1000);
        }

        static public void GetFxItemReallyDuration(ParticleSystem ps, out float delayTime, out float startLifeTime)
        {
            MainModule main = ps.main;
            float duration = main.duration;

            ParticleSystemCurveMode delayMode = main.startDelay.mode;

            delayTime = GetMinMaxCurveValue(delayMode, main.startDelay, duration);

            ParticleSystemCurveMode lifeMode = main.startLifetime.mode;
            startLifeTime = GetMinMaxCurveValue(lifeMode, main.startLifetime, duration);
            // Debug.LogError($"fx : {ps.name}  delayMode: {delayMode}, delayTime: {delayTime} ,startLifeTime: {startLifeTime}");
        }

        public static float GetMinMaxCurveValue(ParticleSystemCurveMode mode, MinMaxCurve curve, float duration)
        {
            float value = 0;

            switch (mode)
            {
                case ParticleSystemCurveMode.Constant:
                    {
                        value = curve.constant;
                    }
                    break;
                case ParticleSystemCurveMode.Curve:
                    {
                        value = curve.curve.Evaluate(duration);
                    }
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    {
                        value = curve.constantMax;
                    }
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    {
                        value = curve.curveMax.Evaluate(duration);
                    }
                    break;
                default: break;
            }

            return value;
        }

        public static ParticleSystem GetRootPs(GameObject gob)
        {
            ParticleSystem ps = null;
            GameObject findGob = gob;
            do
            {
                if (findGob == null)
                {
                    break;
                }
                ps = findGob.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    break;
                }
                Transform child = findGob.transform.GetChild(0);
                if (child == null)
                {
                    break;
                }
                findGob = child.gameObject;
            } while (true);

            return ps;
        }


        public static Dictionary<uint, (string Name, string Path)> Infos = new();

        public static List<string> Paths = new();

        private static bool IsInit = false;
        static void Init()
        {
            Infos.Clear();
            Paths.Clear();

            Infos.Add(0, ("", ""));
            Paths.Add("");

            TextAsset textAsset = ResourceHelperMono.LoadWwiseSoundBankInfo("SoundbanksInfo");
            if (textAsset != null && textAsset.text != null)
            {
                XmlDocument document = new();
                document.LoadXml(textAsset.text);

                XmlNode soundbankInfos = document.SelectSingleNode("SoundBanksInfo");
                XmlNode soundbanks = soundbankInfos.SelectSingleNode("SoundBanks");
                if (soundbanks != null && soundbanks.ChildNodes.Count > 0)
                {
                    foreach (var soundbank in soundbanks.ChildNodes) //SoundBank
                    {
                        XmlElement _XsoundBank = soundbank as XmlElement;
                        if (_XsoundBank == null || _XsoundBank.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        XmlNode IncludedEvents = _XsoundBank.SelectSingleNode("IncludedEvents");
                        if (IncludedEvents == null)
                        {
                            continue;
                        }

                        string bankName = _XsoundBank.SelectSingleNode("ShortName").InnerText; //节点.包含值
                        foreach (var @event in IncludedEvents.ChildNodes) //SoundBank配置
                        {
                            XmlElement _event = @event as XmlElement;
                            if (_event == null || _event.NodeType == XmlNodeType.Comment)
                            {
                                continue;
                            }

                            string eventName = _event.GetAttribute("Name"); //节点 属性

                            if (string.IsNullOrEmpty(eventName))
                            {
                                continue;
                            }

                            string ObjectPath = _event.GetAttribute("ObjectPath"); //节点 属性
                            ObjectPath = ObjectPath.Replace("\\", "/");
                            ObjectPath = ObjectPath.Remove(0, 1);
                            uint eventId = System.Convert.ToUInt32(_event.GetAttribute("Id"));
                            if (!Infos.ContainsKey(eventId))
                            {
                                Infos.Add(eventId, (eventName, ObjectPath));
                                Paths.Add(ObjectPath);
                            }
                            else
                            {
                                //Debug.LogError($"事件重复 eventID={eventId} eventName={eventName} ObjectPath={ObjectPath}");
                            }
                            // 
                        }
                    }
                }
            }

            IsInit = true;
        }

        static public List<string> GetSounds()
        {
            if (!IsInit)
            {
                Init();
            }
            return Paths;
        }

        static public (uint eventID, string eventName) GetEventID(string path)
        {
            if (!IsInit)
            {
                Init();
            }

            foreach (var item in Infos)
            {
                if (item.Value.Path == path)
                {
                    return (item.Key, item.Value.Name);
                }
            }
            return (0, string.Empty);
        }
    }
}

