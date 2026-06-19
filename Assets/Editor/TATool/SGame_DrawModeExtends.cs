using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public static class SGame_DrawModeExtends
{
    static string ModeName_BakedOcclusion = "Baked Occlusion ";

    static Shader Shader_BakedOcclusion;
    static SceneView currentSceneView;
    //static const string texturePath = "Assets/Plugins/Base.png";

    static SGame_DrawModeExtends()
    {
        EditorApplication.update += OnUpdateEditor;
        Shader_BakedOcclusion = Shader.Find("SGAME/Editor_Preview_Occlusion");
        SceneView.AddCameraMode(ModeName_BakedOcclusion, "Yoka");

    }

    static void OnDrawModeChanged(SceneView.CameraMode mode)
    {
        if (mode.name == ModeName_BakedOcclusion)
            DrawBakedOcclusion();
        else
            Reset();
    }

    public static void DrawBakedOcclusion()
    {
        currentSceneView.SetSceneViewShaderReplace(Shader_BakedOcclusion, null);
    }
    public static void Reset()
    {
        //Reset to null.
        currentSceneView.SetSceneViewShaderReplace(null, null);
    }

    public static void OnUpdateEditor()
    {
        if (SceneView.lastActiveSceneView != currentSceneView)
        {
            if (currentSceneView)
                currentSceneView.onCameraModeChanged -= OnDrawModeChanged;
            if (SceneView.lastActiveSceneView)
            {
                currentSceneView = SceneView.lastActiveSceneView;
                currentSceneView.onCameraModeChanged += OnDrawModeChanged;
            }
        }
    }
}