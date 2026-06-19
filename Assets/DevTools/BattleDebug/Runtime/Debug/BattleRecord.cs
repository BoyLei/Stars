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

        private Dictionary<string, BattleSkillDebug> Skills = null;

        private Dictionary<DEBUGTYPE, BattleDebug> DebugeMap = null;

        private Dictionary<string, Dictionary<StageEnum, TrackAsset>> GroupTracks = null;

        private Dictionary<string, Dictionary<string, TimelineClip>> DebugClips = null;

        private PlayableDirector Director;


        private TimelineAsset Timeline;
        public BattleRecord()
        {
#if UNITY_EDITOR

            if (!System.IO.Directory.Exists("Assets/BattleDebug/Runtime/Playables"))
            {
                System.IO.Directory.CreateDirectory("Assets/BattleDebug/Runtime/Playables");
            }
            LocalTime = System.DateTime.Now;
            if (Director == null)
            {
                GameObject go = new GameObject("DebugTimeline_" + LocalTime.ToString("yyyy_MM_dd HH_mm_ss"));
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
            Skills = new Dictionary<string, BattleSkillDebug>();
            DebugeMap = new Dictionary<DEBUGTYPE, BattleDebug>()
        {
            {DEBUGTYPE.DEBUG_SKILL,OnSkillEvent },
            {DEBUGTYPE.DEBUG_ANIMATION,OnAnimationEvent },
            {DEBUGTYPE.DEBUG_EFFECT,OnEffectEvent },
            {DEBUGTYPE.DEBUG_AUDIO,OnAudioEvent },
            {DEBUGTYPE.DEBUG_CLIENT_EVENT,OnClientEvent },
            {DEBUGTYPE.DEBUG_SERVER_EVENT,OnServerEvent },
            {DEBUGTYPE.DEBUG_SKILL_STATE_EVENT,OnSkillStateEvent },


        };
            GroupTracks = new Dictionary<string, Dictionary<StageEnum, TrackAsset>>();
            DebugClips = new Dictionary<string, Dictionary<string, TimelineClip>>();
            BattleDebugHelper.SetDelegate(OnReciveDeBug);
            BattleDebugHelper.SetBattleRecord(this);
        }

        public void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Timeline);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
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



        private bool ContainsSkill(string UniqueID)
        {
            return Skills.ContainsKey(UniqueID);
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



        private void CreatSkillGroup(string SkillUID, int skillID)
        {

            if (!GroupTracks.ContainsKey(SkillUID))
            {
                GroupTrack group = Timeline.CreateTrack<GroupTrack>($"{skillID}_{SkillUID}");



                Dictionary<StageEnum, TrackAsset> TrackAssets = new Dictionary<StageEnum, TrackAsset>();

                var SkillStateTrack = Timeline.CreateTrack(typeof(BattleDebugTrack), group, "技能状态");
                TrackAssets.Add(StageEnum.SkillState, SkillStateTrack);

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

                GroupTracks.Add(SkillUID, TrackAssets);

                DebugClips.Add(SkillUID, new Dictionary<string, TimelineClip>());

#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(Director);
                UnityEditor.Selection.activeGameObject = Director.transform.gameObject;
#endif
            }

        }
        private Color TranslateColor(StageEnum stage, DebugData debugData)
        {
            Color color = Color.green;
            switch (stage)
            {
                case StageEnum.SkillState:
                    SkillStateDebugData skillStateDebug = debugData as SkillStateDebugData;
                    if (skillStateDebug != null)
                    {
                        if (skillStateDebug.IsEnd)
                        {
                            ColorUtility.TryParseHtmlString("#FF0000", out color);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#00FF00", out color);
                        }
                    }
                    break;
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

        public string GetClipName(StageEnum stage, DebugData debugData)
        {
            if (stage == StageEnum.SkillState)
            {
                SkillStateDebugData skillStateDebug = debugData as SkillStateDebugData;
                if (skillStateDebug != null)
                {
                    string tile = skillStateDebug.IsClient ? "客户端" : "服务器";
                    string end = skillStateDebug.IsEnd ? "结束" : "开始";
                    return $"{tile}技能{end}";
                }
            }

            return debugData.DisplayName;
        }
        public void CreateClip(StageEnum stage, DebugData debugData, StageDebug stageDebug)
        {
            if (DebugClips.ContainsKey(debugData.SkillUniqueID) && GroupTracks.ContainsKey(debugData.SkillUniqueID))
            {
                if (!DebugClips[debugData.SkillUniqueID].ContainsKey(debugData.UniqueID) && GroupTracks[debugData.SkillUniqueID].ContainsKey(stage))
                {

                    var clip = GroupTracks[debugData.SkillUniqueID][stage].CreateClip<BattleDebugAsset>();
                    clip.displayName = GetClipName(stage, debugData);
                    clip.start = stageDebug.StarTime;
                    clip.duration = stageDebug.EndTime - stageDebug.StarTime;
                    clip.CustomColor = TranslateColor(stage, debugData);
                    if (clip.asset is BattleDebugAsset debugAsset)
                    {
                        if (stageDebug.Args != null && stageDebug.Args.Count > 0)
                        {
                            debugAsset.template.Args.Clear();
                            foreach (var item in stageDebug.Args)
                            {
                                debugAsset.template.Args.Add(item);
                            }
                        }
                    }
                    DebugClips[debugData.SkillUniqueID].Add(debugData.UniqueID, clip);
                }

            }
        }


        public void ModifyClip(string SkillUID, StageEnum stage, string UniqueID, double duration, List<string> Args)
        {
            if (DebugClips.ContainsKey(SkillUID) && GroupTracks.ContainsKey(SkillUID))
            {
                if (DebugClips[SkillUID].ContainsKey(UniqueID) && GroupTracks[SkillUID].ContainsKey(stage))
                {
                    var clip = DebugClips[SkillUID][UniqueID];
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


            bool exist = ContainsSkill(debugData.SkillUniqueID);
            //新增
            if (skillDebug.IsStart)
            {
                if (exist)
                {
                    Debug.LogError($"{skillDebug.SkillUniqueID} 已经存在 不能重复添加");
                }
                else
                {
                    BattleSkillDebug battleSkill = new BattleSkillDebug(skillDebug.SkillID, skillDebug.SkillUniqueID);
                    Skills.Add(debugData.SkillUniqueID, battleSkill);
                    CreatSkillGroup(skillDebug.SkillUniqueID, skillDebug.SkillID);
                }
            }
            else
            {
                if (exist)
                {
                    //修改
                    Skills[debugData.SkillUniqueID].OnEnd();
                }
                else
                {
                    Debug.LogError($"技能{skillDebug.SkillID} 不存在 ");
                }
            }
        }


        private void OnSkillStateEvent(DebugData debugData)
        {
            if (debugData == null)
            {
                return;
            }
            if (!CheckEvent(debugData.DeBugType, DEBUGTYPE.DEBUG_SKILL_STATE_EVENT))
            {
                return;
            }

            SkillStateDebugData skillStateDebug = debugData as SkillStateDebugData;
            if (skillStateDebug == null)
            {
                return;
            }


            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }

            Skills[debugData.SkillUniqueID].OnModify(StageEnum.SkillState, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(StageEnum.SkillState, debugData, clipData);
                }
                else
                {
                    ModifyClip(debugData.SkillUniqueID, StageEnum.SkillState, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
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


            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }

            Skills[debugData.SkillUniqueID].OnModify(StageEnum.Animation, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(StageEnum.Animation, debugData, clipData);
                }
                else
                {
                    ModifyClip(debugData.SkillUniqueID, StageEnum.Animation, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
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


            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }
            Skills[debugData.SkillUniqueID].OnModify(StageEnum.Effect, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(StageEnum.Effect, debugData, clipData);
                }
                else
                {
                    ModifyClip(debugData.SkillUniqueID, StageEnum.Effect, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
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


            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }
            Skills[debugData.SkillUniqueID].OnModify(StageEnum.Audio, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                if (debugData.IsStart)
                {
                    CreateClip(StageEnum.Audio, debugData, clipData);
                }
                else
                {
                    ModifyClip(debugData.SkillUniqueID, StageEnum.Audio, debugData.UniqueID, clipData.EndTime - clipData.StarTime, clipData.Args);
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


            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }

            Skills[debugData.SkillUniqueID].OnModify(StageEnum.ClientEvent, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                CreateClip(StageEnum.ClientEvent, debugData, clipData);
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

            if (!ContainsSkill(debugData.SkillUniqueID))
            {
                Debug.LogError($"不存在技能{debugData.SkillUniqueID}");
                return;
            }
            Skills[debugData.SkillUniqueID].OnModify(StageEnum.ServerEvent, debugData);
            var clipData = Skills[debugData.SkillUniqueID].GetStageDebug(debugData.UniqueID);
            if (clipData != null)
            {
                CreateClip(StageEnum.ServerEvent, debugData, clipData);
            }
        }
    }
}