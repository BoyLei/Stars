///--------------------------------------------------------------------
/// 文件名   :   InputKeyExtensions.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/20 10:38:34
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;


namespace SkillEditor
{

    public partial class InputKey
    {
#if UNITY_EDITOR
        public IEnumerable GetOutputKey()
        {
            List<string> outputKeys = new List<string>();
            foreach (var track in UnityEditor.Timeline.TimelineEditor.timelineAsset.GetOutputTracks())
            {
                if (track.GetType() is EffectTrack)
                {
                    continue;
                }
                foreach (var clip in track.GetClips())
                {
                    if (clip.parentTrack.GetType() == typeof(EffectTrack))
                    {
                        ClipEffectAsset clipEffectAsset = (ClipEffectAsset)clip.asset;
                        foreach (var data in clipEffectAsset.template.frameEffect)
                        {
                            foreach (var outputKey in data.EffectArgs.GetOutputKey())
                            {
                                outputKeys.Add(outputKey);
                            }
                        }
                    }
                    if (clip.parentTrack.GetType() == typeof(InputEffectTrack))
                    {
                        ClipInputEffectAsset clipEffectAsset = (ClipInputEffectAsset)clip.asset;
                        foreach (var outputKey in clipEffectAsset.template.inputEffect.EffectArgs.GetOutputKey())
                        {
                            outputKeys.Add(outputKey);
                        }
                        foreach (var data in clipEffectAsset.template.frameEffect)
                        {
                            foreach (var outputKey in data.EffectArgs.GetOutputKey())
                            {
                                outputKeys.Add(outputKey);
                            }
                        }
                    }
                }
            }
            int ID = int.Parse(UnityEditor.Timeline.TimelineEditor.timelineAsset.name.Split("_")[1]);
            string type = UnityEditor.Timeline.TimelineEditor.timelineAsset.name.Split("_")[0];
            if (type == "Bullet")
            {
                foreach (var key in SkillEditorData.Bullets[ID].bulletConfig.BulletCopyData)
                {
                    outputKeys.Add(key.Result);
                }
            }
            if (type == "Buff")
            {
                foreach (var key in SkillEditorData.Buffs[ID].buffConfig.BuffCopyData)
                {
                    outputKeys.Add(key.Result);
                }
            }
            if (type == "Passive")
            {
                foreach (var key in SkillEditorData.Passives[ID].passiveSkillConfig.PassiveCopyData)
                {
                    outputKeys.Add(key.Result);
                }
            }
            foreach (var key in SkillEditorGlobal.DefualtKeys)
            {
                outputKeys.Add(key);
            }
            return outputKeys;
        }

        public string GetInputKey()
        {
            return Result;
        }
#endif
    }
}