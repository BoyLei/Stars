using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

namespace SkillEditor
{
    public class CameraShakeBehavior : PlayableBehaviour
    {
        public AnimationCurve AmplitudeGain;
        public AnimationCurve FrequencyGain;
        public string VirtualCameraName;
    }
}

