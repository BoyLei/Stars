using System.Collections.Generic;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public static class TimelineTools
    {
        public static (List<TimelineClip> otherTracks, List<TimelineClip> curTracks) GetCurTimelineClips<T>(GetTrackType type,double time)
        {
            //非当前位置的块
            List<TimelineClip> otherTracks = new List<TimelineClip>();
            //处于当前位置的块
            List<TimelineClip> curTracks = new List<TimelineClip>();
#if UNITY_EDITOR
            //拿到当前的时间线
            TimelineAsset timelineAsset = UnityEditor.Timeline.TimelineEditor.timelineAsset;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (clip.parentTrack.GetType() == typeof(T))
                    {
                        if (clip.start < time)
                        {
                            if (clip.end < time)
                            {
                                if (type == GetTrackType.Before)
                                {
                                    otherTracks.Add(clip);
                                }
                            }
                            else
                            {
                                curTracks.Add(clip);
                            }
                        }
                        else if (clip.end > time)
                        {
                            if (type == GetTrackType.After)
                            {
                                otherTracks.Add(clip);
                            }
                        }
                        else
                        {
                            curTracks.Add(clip);
                        }
                    }
                }
            }
#endif
            return (otherTracks,curTracks);
        }
    }

    public enum GetTrackType
    {
        AllTracks = 0,
        Before = 1,
        After = 2,
    }
}
