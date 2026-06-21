using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDLL;

public class SGameRunner : ShaderVariantRunner
{
    protected override void QuitRunning()
    {
#if UNITY_EDITOR || UNITY_EDITOR_OSX
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        base.QuitRunning();
    }
}
