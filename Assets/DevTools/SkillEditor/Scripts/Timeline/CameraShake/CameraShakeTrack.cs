
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [Serializable]
    [TrackClipType(typeof(CameraShakeClip))]
    [TrackColor(0.53f, 0.0f, 0.08f)]
    public class CameraShakeTrack : TrackAsset
    {
        public bool isRight;
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixerPlayable = ScriptPlayable<CameraShakeMixer>.Create(graph);
            mixerPlayable.SetInputCount(inputCount);
            return mixerPlayable;
        }
    }
}
