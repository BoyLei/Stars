using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [Serializable]
    public class CameraShakeClip : PlayableAsset, ITimelineClipAsset
    {
        public CameraShakeBehavior template = new CameraShakeBehavior();

        public AnimationCurve AmplitudeGain = AnimationCurve.Constant(0f,1f,0f);
        public AnimationCurve FrequencyGain = AnimationCurve.Constant(0f,1f,0f);
        public string VirtualCameraName;
        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CameraShakeBehavior>.Create(graph, template);

            CameraShakeBehavior behaviour = playable.GetBehaviour();
            behaviour.AmplitudeGain = AmplitudeGain;
            behaviour.FrequencyGain = FrequencyGain;
            
            return playable;
        }
    }
}

