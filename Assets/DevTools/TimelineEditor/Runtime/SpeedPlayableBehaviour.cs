using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
[System.Serializable]
public class SpeedPlayableBehaviour : PlayableBehaviour
{

    [SerializeField]
    [LabelText("动画曲线")]
    public AnimationCurve Clip;

    private double NormalSpeed;

    private Playable RootPlayable;

    private float RunnigTime;

    private double Duration;

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        RootPlayable = playable.GetGraph().GetRootPlayable(0);
        if (RootPlayable.IsNull())
        {
            return;
        }
        NormalSpeed = RootPlayable.GetSpeed();
        RunnigTime = 0;
        Duration=playable.GetDuration();
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (RootPlayable.IsNull())
        {
            return;
        }
        RootPlayable.SetSpeed(NormalSpeed);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (RootPlayable.IsNull())
        {
            return;
        }
        RunnigTime += info.deltaTime;
        float normalizedTime = (float)(RunnigTime / Duration);
        var speed = Clip.Evaluate(normalizedTime) * NormalSpeed;
        RootPlayable.SetSpeed(speed);
    }
}
