///--------------------------------------------------------------------
/// 文件名   :   BattleRecord.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/02 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BattleDebug
{
    public class BattleRecord
    {
        /// <summary>
        /// 本地时间
        /// </summary>
        public DateTime LocalTime { get; private set; }


        private Dictionary<int, BattleSkillDebug> Skills = null;

        private Dictionary<DEBUGTYPE, BattleDebug> DebugeMap = null;

        private Dictionary<int, Dictionary<StageEnum, TrackAsset>> GroupTracks = null;

        private Dictionary<int, Dictionary<string, TimelineClip>> DebugClips = null;

        private PlayableDirector Director;


        private TimelineAsset Timeline;
        public BattleRecord()
        {
#if UNITY_EDITOR
           

            LocalTime = System.DateTime.Now;
            if (Director == null)
            {
                GameObject go = new GameObject("DebugTimeline_"+ LocalTime.ToString("yyyy_MM_dd HH_mm_ss"));
                go.transform.localPosition = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                Director = go.AddComponent<PlayableDirector>();
            }
            Timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            Director.playableAsset = Timeline;
            string path = $"Assets/BattleDebug/Runtime/Playables/{LocalTime.ToString("yyyy_MM_dd HH_mm_ss")}-timeline.playable";
            UnityEditor.AssetDatabase.CreateAsset(Timeline, path);
#endif
            Skills = new Dictionary<int, BattleSkillDebug>();
            DebugeMap = new Dictionary<DEBUGTYPE, BattleDebug>()
        {
            {DEBUGTYPE.DEBUG_SKILL,OnSkillEvent },
            {DEBUGTYPE.DEBUG_ANIMATION,OnAnimationEvent },
            {DEBUGTYPE.DEBUG_EFFECT,OnEffectEvent },
            {DEBUGTYPE.DEBUG_AUDIO,OnAudioEvent },
            {DEBUGTYPE.DEBUG_CLIENT_EVENT,OnClientEvent },
            {DEBUGTYPE.DEBUG_SERVER_EVENT,OnServerEvent },
        };
            GroupTracks = new Dictionary<int, Dictionary<StageEnum, TrackAsset>>();
            DebugClips = new Dictionary<int, Dictionary<string, TimelineClip>>();
            BattleDebugHelper.SetDelegate(OnReciveDeBug);
            BattleDebugHelper.SetBattleRecord(this);
        }

        ~BattleRecord()
        {
            GameObject.DestroyImmediate(Director.gameObject);
            BattleDebugHelper.SetDelegate(null);
        }


        private void OnReciveDeBug(DebugData debugData)
        {
            if (DebugeMap.ContainsKey(debugData.DeBugType))
            {
                DebugeMap[debugData.DeBugType].Invoke(debugData);
            }
        }



        private bool ContainsSkill(int SkillID)
        {
            return Skills.ContainsKey(SkillID);
        }

        private bool CheckEvent(DEBUGTYPE EventID, DEBUGTYPE TargetID)
        {
            if (EventID != TargetID)
            {
                Debug.LogError($"事件ID不匹配 EventID={EventID} TargetID={TargetID}");
                return false;
            }
            return true;
        }



        private void CreatSkillGroup(int SkillID)
        {

            if (!GroupTracks.ContainsKey(SkillID))
            {
                GroupTrack group = Timeline.CreateTrack<GroupTrack>(SkillID.ToString());

                Dictionary<StageEnum, TrackAsset> TrackAssets = new Dictionary<StageEnum, TrackAsset>();
                var animationTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "动作");
                TrackAssets.Add(StageEnum.Animation, animationTrack);


                var EffectTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "特效");
                TrackAssets.Add(StageEnum.Effect, EffectTrack);

                var AudioTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "音效");
                TrackAssets.Add(StageEnum.Audio, AudioTrack);


                var ClientTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "客户端执行效果");
                TrackAssets.Add(StageEnum.ClientEvent, ClientTrack);

                var ServerTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "服务器效果");
                TrackAssets.Add(StageEnum.ServerEvent, ServerTrack);

                GroupTracks.Add(SkillID, TrackAssets);

                DebugClips.Add(SkillID, new Dictionary<string, TimelineClip>());

#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(Director);
                UnityEditor.Selection.activeGameObject = Director.transform.gameObject;
#endif
            }

        }
        private Color TranslateColor(StageEnum stage)
        {
            Color color = Color.green;
            switch (stage)
            {
                case StageEnum.Animation:
                    ColorUtility.TryParseHtmlString("#00F5FF", out color);
                    break;

                case StageEnum.Effect:
                    ColorUtility.TryParseHtmlString("#FFDAB9", out color);
                    break;
                case StageEnum.Audio:
                    ColorUtility.TryParseHtmlString("#54FF9F", out color);
                    break;
                case StageEnum.ClientEvent:
                    ColorUtility.TryParseHtmlString("#FF6A6A", out color);
                    break;
                case StageEnum.ServerEvent:
                    ColorUtility.TryParseHtmlString("#E066FF", out color);
                    break;
            }
            return color;
        }

        public void CreateClip(int SkillID, StageEnum stage, string UniqueID, double start, double end, List<string> Args)
        {
            if (DebugClips.ContainsKey(SkillID) && GroupTracks.ContainsKey(SkillID))
            {
                if (!DebugClips[SkillID].ContainsKey(UniqueID) && GroupTracks[SkillID].ContainsKey(stage))
                {

                    var clip = GroupTracks[SkillID][stage].CreateClip<BattleDebugAsset>();
                    clip.displayName = $"[{stage.ToString()}]";
                    clip.start = start;
                    clip.duration = end - start;
                    clip.CustomColor = TranslateColor(stage);
                    if (clip.asset is BattleDebugAsset debugAsset)
                    {
                        if (Args != null && Args.Count > 0)
                        {
                            debugAsset.template.Args.Clear();
                            foreach (var item in Args)
                            {
                                debugAsset.template.Args.Add(item);
                            }
                        }
                    }
                    DebugClips[SkillID].Add(UniqueID, clip);
#if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.PingObject(Director);
                    UnityEditor.Selection.activeGameObject = Director.transform.gameObject;
#endif
                }
            }
        }

        public void ModifyClip(int SkillID, StageEnum stage, string UniqueID, double duration, List<string> Args)
        {
            if (DebugClips.ContainsKey(SkillID) && GroupTracks.ContainsKey(SkillID))
            {
                if (DebugClips[SkillID].ContainsKey(UniqueID) && GroupTracks[SkillID].ContainsKey(stage))
                {
                    var clip = DebugClips[SkillID][UniqueID];
                    clip.duration = duration;
                    if (clip.asset is BattleDebugAsset debugAsset)
                    {
                        if (Args != null && Args.Count > 0)
                        {
                            debugAsset.template.Args.Clear();
                            foreach (var item in Args)
                            {
                                debugAsset.template.Args.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private void OnSkillEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_SKILL))
            {
                return;
            }
            SkillDebugData skillDebug = debugData as SkillDebugData;
            if (skillDebug == null)
            {
                return;
            }


            bool exist = ContainsSkill(debugData.SkillID);
            //新增
            if (skillDebug.IsStart)
            {
                if (exist)
                {
                    Debug.LogError($"{debugData.SkillID} 已经存在 不能重复添加");
                }
                else
                {
                    BattleSkillDebug battleSkill = new BattleSkillDebug(debugData.SkillID);
                    Skills.Add(debugData.SkillID, battleSkill);
                    CreatSkillGroup(debugData.SkillID);
                }
            }
            else
            {
                if (exist)
                {
                    //修改
                    Skills[debugData.SkillID].OnEnd();
                }
                else
                {
                    Debug.LogError($"技能{debugData.SkillID} 不存在 ");
                }
            }

        }

        private void OnAnimationEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_ANIMATION))
            {
                return;
            }

            AnimationDebugData animationDebug = debugData as AnimationDebugData;
            if (animationDebug == null)
            {
                return;
            }


            if (!ContainsSkill(debugData.SkillID))
            {
                Debug.LogError($"不存在技能{debugData.SkillID}");
                return;
            }

            Skills[debugData.SkillID].OnModify(StageEnum.Animation, debugData);
            var clipData = Skills[debugData.SkillID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(debugData.SkillID, StageEnum.Animation, debugData.UniqueID, clipData.StarTime, clipData.EndTime, clipData.Args);
                }
                else
                {
                    ModifyClip(debugData.SkillID, StageEnum.Animation, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
                }
            }

        }

        private void OnEffectEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_EFFECT))
            {
                return;
            }

            EffectDebugData effectDebug = debugData as EffectDebugData;
            if (effectDebug == null)
            {
                return;
            }


            if (!ContainsSkill(debugData.SkillID))
            {
                Debug.LogError($"不存在技能{debugData.SkillID}");
                return;
            }
            Skills[debugData.SkillID].OnModify(StageEnum.Effect, debugData);
            var clipData = Skills[debugData.SkillID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(debugData.SkillID, StageEnum.Effect, debugData.UniqueID, clipData.StarTime, clipData.EndTime, clipData.Args);
                }
                else
                {
                    ModifyClip(debugData.SkillID, StageEnum.Effect, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
                }
            }

        }

        private void OnAudioEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_AUDIO))
            {
                return;
            }

            AudioDebugData audioDebug = debugData as AudioDebugData;
            if (audioDebug == null)
            {
                return;
            }


            if (!ContainsSkill(debugData.SkillID))
            {
                Debug.LogError($"不存在技能{debugData.SkillID}");
                return;
            }
            Skills[debugData.SkillID].OnModify(StageEnum.Audio, debugData);
            var clipData = Skills[debugData.SkillID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(debugData.SkillID, StageEnum.Audio, debugData.UniqueID, clipData.StarTime, clipData.EndTime, clipData.Args);
                }
                else
                {
                    ModifyClip(debugData.SkillID, StageEnum.Audio, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
                }
            }
        }
        private void OnClientEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_CLIENT_EVENT))
            {
                return;
            }


            if (!ContainsSkill(debugData.SkillID))
            {
                Debug.LogError($"不存在技能{debugData.SkillID}");
                return;
            }



            Skills[debugData.SkillID].OnModify(StageEnum.ClientEvent, debugData);
            var clipData = Skills[debugData.SkillID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                CreateClip(debugData.SkillID, StageEnum.ClientEvent, debugData.UniqueID, clipData.StarTime, clipData.EndTime, clipData.Args);
            }
        }
        private void OnServerEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_SERVER_EVENT))
            {
                return;
            }

            if (!ContainsSkill(debugData.SkillID))
            {
                Debug.LogError($"不存在技能{debugData.SkillID}");
                return;
            }
            Skills[debugData.SkillID].OnModify(StageEnum.ServerEvent, debugData);
            var clipData = Skills[debugData.SkillID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                CreateClip(debugData.SkillID, StageEnum.ServerEvent, debugData.UniqueID, clipData.StarTime, clipData.EndTime, clipData.Args);
            }
        }
    }
}