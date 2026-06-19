using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering.Universal;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using static UnityEngine.UI.Image;


public partial class CustomShadowBase
{
    void DisableEditor()
    {
        if (Application.isPlaying || _light == null)
            return;

        _light.shadows = LightShadows.Soft;
    }
}
