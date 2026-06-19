using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;


[VolumeComponentEditor(typeof(ScreenFogAndCloudShadow))]
class SVolumeInspector_ScreenFogAndCloudShadow : VolumeComponentEditor
{
    SerializedDataParameter m_Intensity;

    public override void OnEnable()
    {
        base.OnEnable();
        SceneView.duringSceneGui += SceneView_duringSceneGui;
    }

    private void SceneView_duringSceneGui(SceneView obj)
    {
        var screenFogAndCloudShadowVolume = target as ScreenFogAndCloudShadow;
        var lightmapPosAndSize = screenFogAndCloudShadowVolume.FogLightmapPosAndSize.value;

        var size = new Vector3(lightmapPosAndSize.z, 0, lightmapPosAndSize.w);
        var center = new Vector3(lightmapPosAndSize.x + 0.5f * size.x, 0, lightmapPosAndSize.y + 0.5f * size.z);

        Handles.DrawWireCube(center, size);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneView.duringSceneGui -= SceneView_duringSceneGui;
    }

    public override void OnInspectorGUI()
    {
        var screenFogAndCloudShadowVolume = target as ScreenFogAndCloudShadow;

        var o = new PropertyFetcher<ScreenFogAndCloudShadow>(serializedObject);

        PropertyField(Unpack(o.Find(x => x.PassEvent)));

        EditorGUILayout.Space();

        var fogEnable = Unpack(o.Find(x => x.IsFogEnable));
        PropertyField(fogEnable);
        GUILayout.BeginVertical("GroupBox");
        {
            PropertyField(Unpack(o.Find(x => x.PreviewFogInSceneView)), new GUIContent("场景中预览？"));

            EditorGUILayout.LabelField("Fog Settings:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PropertyField(Unpack(o.Find(x => x.FadeToSky)));
            if (screenFogAndCloudShadowVolume.FadeToSky.value)
                PropertyField(Unpack(o.Find(x => x.SkyColor)));

            PropertyField(Unpack(o.Find(x => x.PlayerInteractivable)));
            if (screenFogAndCloudShadowVolume.PlayerInteractivable.value)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                PropertyField(Unpack(o.Find(x => x.InteractiveRadius)));
                PropertyField(Unpack(o.Find(x => x.InteractiveFalloff)));
                var previewInteractiveObj = EditorUtility.InstanceIDToObject(screenFogAndCloudShadowVolume.ScenePreviewInteractive.value) as Transform;
                previewInteractiveObj = EditorGUILayout.ObjectField("场景预览角色", previewInteractiveObj, typeof(Transform), true) as Transform;
                screenFogAndCloudShadowVolume.ScenePreviewInteractive.value = previewInteractiveObj ? previewInteractiveObj.GetInstanceID() : -1;
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }

            PropertyField(Unpack(o.Find(x => x.FogNoiseTexture)));
            PropertyField(Unpack(o.Find(x => x.FogNoiseContract)));
            PropertyField(Unpack(o.Find(x => x.FogNoiseDepthFalloff)));


            EditorGUILayout.LabelField("Height Fog:", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            PropertyField(Unpack(o.Find(x => x.FogNoiseSize1)));
            PropertyField(Unpack(o.Find(x => x.FogNoiseSize2)));
            PropertyField(Unpack(o.Find(x => x.FogNoiseMoveSpeed1)));
            PropertyField(Unpack(o.Find(x => x.FogNoiseMoveSpeed2)));


            EditorGUILayout.Space();
            PropertyField(Unpack(o.Find(x => x.FogHeightStart)));
            PropertyField(Unpack(o.Find(x => x.FogHeight)));

            PropertyField(Unpack(o.Find(x => x.HeightFogFalloff)));
            PropertyField(Unpack(o.Find(x => x.HeightFogDensity)));

            PropertyField(Unpack(o.Find(x => x.FogColor)));

            //PropertyField(Unpack(o.Find(x => x.FogSunColor)));
            //PropertyField(Unpack(o.Find(x => x.SunFalloff)));

            //PropertyField(Unpack(o.Find(x => x.SunLightAngle)));
            //PropertyField(Unpack(o.Find(x => x.SunLightVerticalAngle)));

            EditorGUI.BeginChangeCheck();
            screenFogAndCloudShadowVolume.TerrainMeshRenderer = EditorGUILayout.ObjectField("地表模型", screenFogAndCloudShadowVolume.TerrainMeshRenderer, typeof(MeshRenderer), true) as MeshRenderer;
            if (EditorGUI.EndChangeCheck())
            {
                if (screenFogAndCloudShadowVolume.TerrainMeshRenderer)
                {
                    Bounds bounds = screenFogAndCloudShadowVolume.TerrainMeshRenderer.bounds;

                    Vector3 min = bounds.min;
                    Vector3 max = bounds.max;

                    float minWorldX = min.x;
                    float maxWorldX = max.x;
                    float minWorldZ = min.z;
                    float maxWorldZ = max.z;
                    screenFogAndCloudShadowVolume.FogLightmapPosAndSize.value = new Vector4(minWorldX, minWorldZ, bounds.size.x, bounds.size.z);
                    //screenFogAndCloudShadowVolume.FogLightmapPosAndSize.value = new Vector4(bounds.center.x, bounds.center.z, bounds.size.x, bounds.size.z);
                }
            }
            PropertyField(Unpack(o.Find(x => x.FogLightmap)));
            PropertyField(Unpack(o.Find(x => x.FogLightmapPosAndSize)));
            PropertyField(Unpack(o.Find(x => x.FogLightmapColor)));
            PropertyField(Unpack(o.Find(x => x.FogLightmapFalloff)));
            PropertyField(Unpack(o.Find(x => x.FogLightmapDistanceFalloff)));
            PropertyField(Unpack(o.Find(x => x.FogLightmapDistanceMin)));
            //PropertyField(Unpack(o.Find(x => x.FogLightmapDistanceMax)));



            //EditorGUI.BeginChangeCheck();
            //var transmittanceLutSizeProp = Unpack(o.Find(x => x.TransmittanceLutSize));
            //PropertyField(transmittanceLutSizeProp);
            //var transmittanceLutGradiantProp = Unpack(o.Find(x => x.TransmittanceLutGradiant));
            //PropertyField(transmittanceLutGradiantProp);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    //var lutSize = transmittanceLutSizeProp.value.intValue;
            //    screenFogAndCloudShadowVolume.TransmittanceLut.value = ScreenFogAndCloudShadow.UpdateGradiantToTexture(GetGradient(transmittanceLutGradiantProp.value)
            //}

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Linear Fog:", EditorStyles.boldLabel);

            PropertyField(Unpack(o.Find(x => x.DepthFogFalloff)));
            PropertyField(Unpack(o.Find(x => x.DepthFogStart)));
            PropertyField(Unpack(o.Find(x => x.DepthFogEnd)));
            PropertyField(Unpack(o.Find(x => x.DepthFogDensity)));

            EditorGUILayout.LabelField("Blend:", EditorStyles.boldLabel);

            PropertyField(Unpack(o.Find(x => x.FogBlendMode)));
            if (screenFogAndCloudShadowVolume.FogBlendMode.value == ScreenFogBlendMode.Blend)
            {
                PropertyField(Unpack(o.Find(x => x.FogBlendFactor)));
            }
            EditorGUILayout.EndVertical();


            EditorGUI.indentLevel--;
        }
        GUILayout.EndVertical();

        var shadowCloudEnable = Unpack(o.Find(x => x.IsCloudShadowEnable));
        PropertyField(shadowCloudEnable);
        GUI.enabled = shadowCloudEnable.value.boolValue;
        GUILayout.BeginVertical("GroupBox");
        {
            PropertyField(Unpack(o.Find(x => x.PreviewCloudInSceneView)), new GUIContent("场景中预览？"));

            EditorGUILayout.LabelField("Cloud Shadow:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            PropertyField(Unpack(o.Find(x => x.CloudShadowTexture)));
            PropertyField(Unpack(o.Find(x => x.CloudShadowSize)));
            PropertyField(Unpack(o.Find(x => x.CloudShadowMoveSpeed)));
            PropertyField(Unpack(o.Find(x => x.CloudShadowColor)));

            EditorGUI.indentLevel--;
        }
        GUILayout.EndVertical();
    }

    public static Gradient GetGradient(SerializedProperty gradientProperty)
    {
        System.Reflection.PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (propertyInfo == null) { return null; }
        else { return propertyInfo.GetValue(gradientProperty, null) as Gradient; }
    }
}
