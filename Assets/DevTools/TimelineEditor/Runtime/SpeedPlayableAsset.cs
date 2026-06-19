using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class SpeedPlayableAsset : PlayableAsset
{
   
    [LabelText("ChangeTimelineSpeed")]
    public SpeedPlayableBehaviour template = new SpeedPlayableBehaviour();

   
        
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }
    
    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<SpeedPlayableBehaviour>.Create(graph, template);
        return playable;
    }
}
