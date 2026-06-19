using System.Linq;
using StarProject;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisallowMultipleComponent]
    public class CustomEventNotificationReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification != null)
            {
                if (notification is CustomEventMarker phase)
                {
                    if (phase.Events != null && phase.Events.Count > 0)
                    {
                        foreach (var evt in phase.Events)
                        {
                            GlobalEvent.OnTimelineEvent.Invoke(phase.Trigger,GetComponent<PlayableDirector>(),evt,(float)System.Math.Round(phase.time,3));
                        }
                    }
                }
            }

        }

        [ContextMenu("Test")]
        public void Test()
        {
            var dirctor = gameObject.GetComponent<PlayableDirector>();

            var TimelineAsset = dirctor.playableAsset as TimelineAsset;
            var tracks = TimelineAsset.GetRootTracks();
            foreach (var item in tracks)
            {
                foreach (var clip in item.GetMarkers())
                {
                    if (clip is CustomEventMarker marker)
                    {
                        foreach (var e in marker.Events)
                        {
                            if (e.EventType == TimelineEventDefine.Dialogue)
                            {
                                Debug.LogError( clip.time+"                   "+System.Math.Round(clip.time,2));
                            }
                        }
                    }

                }

                var childs = item.GetChildTracks();

                foreach (var child in childs)
                {
                    foreach (var clip in child.GetMarkers())
                    {
                        if (clip is CustomEventMarker marker)
                        {
                            foreach (var e in marker.Events)
                            {
                                if (e.EventType == TimelineEventDefine.Dialogue)
                                {
                                    Debug.LogError( clip.time+"                   "+System.Math.Round(clip.time,3));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
