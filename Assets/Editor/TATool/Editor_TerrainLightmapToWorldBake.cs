using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class Editor_TerrainLightmapToWorldBake : EditorWindow
{
    public int WorldTextureSize = 512;
    public GameObject TargetTerrain = null;

    Texture2D FinalTexture = null;
    Material BakeMaterial;

    [MenuItem("自动化工具/TA工具/地形Lightmap导出")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(Editor_TerrainLightmapToWorldBake));
        window.titleContent = new GUIContent("地形Lightmap导出");
        window.Show();
        window.position = new Rect(12, 12, 300, 500);
    }


    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneView_duringSceneGui;
    }

    private void SceneView_duringSceneGui(SceneView sceneView)
    {
        if (!TargetTerrain)
            return;

        var terrainMeshFilter = TargetTerrain.GetComponent<MeshFilter>();
        var terrainMeshRenderer = TargetTerrain.GetComponent<MeshRenderer>();
        if (!terrainMeshFilter || !terrainMeshRenderer)
            return;


        var bounds = terrainMeshRenderer.bounds;
        Handles.DrawWireCube(bounds.center, bounds.size);

    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneView_duringSceneGui;
    }

    private void OnGUI()
    {
        //BakeMaterial = EditorGUILayout.ObjectField(BakeMaterial, typeof(Material), false) as Material;
        BakeMaterial = Resources.Load<Material>("Unlit_ExportWorldLightmap");

        EditorGUI.BeginChangeCheck();
        TargetTerrain = EditorGUILayout.ObjectField(TargetTerrain, typeof(GameObject), true) as GameObject;
        if (!TargetTerrain)
        {
            EditorGUI.EndChangeCheck();
            EditorGUILayout.HelpBox(new GUIContent("请指认场景里面的地形"));
            return;
        }

        var terrainMeshFilter = TargetTerrain.GetComponent<MeshFilter>();
        var terrainMeshRenderer = TargetTerrain.GetComponent<MeshRenderer>();
        if (!terrainMeshFilter || !terrainMeshRenderer)
            return;

        if (LightmapSettings.lightmaps.Length == 0 || terrainMeshRenderer.lightmapIndex > LightmapSettings.lightmaps.Length)
        {
            EditorGUI.EndChangeCheck();
            EditorGUILayout.HelpBox(new GUIContent("场景LightMap有问题，请检查"));
            return;
        }

        var lightmapTexture = LightmapSettings.lightmaps[terrainMeshRenderer.lightmapIndex];
        WorldTextureSize = EditorGUILayout.IntField("导出尺寸", WorldTextureSize);
        if (EditorGUI.EndChangeCheck())
        {
            if (FinalTexture)
            {
                GameObject.DestroyImmediate(FinalTexture);
                FinalTexture = null;
            }

            if (mTempRt)
            {
                mTempRt.Release();
                mTempRt = null;
            }
        }

        if (GUILayout.Button("Bake"))
        {
            if (FinalTexture)
            {
                GameObject.DestroyImmediate(FinalTexture);
                FinalTexture = null;
            }

            FinalTexture = new Texture2D(WorldTextureSize, WorldTextureSize, TextureFormat.RGBAFloat, false);
            LightmapWorldMapping(terrainMeshFilter, terrainMeshRenderer);
        }

        if (mTempRt)
        {
            GUILayout.Box(new GUIContent(mTempRt), GUILayout.MaxHeight(300));
            if (GUILayout.Button("Export"))
            {
                RenderTexture.active = mTempRt;
                FinalTexture.ReadPixels(new Rect(0, 0, WorldTextureSize, WorldTextureSize), 0, 0);
                FinalTexture.Apply();
                RenderTexture.active = null;

                // 上下翻转
                Color[] pixels = FinalTexture.GetPixels();
                for (int y = 0; y < WorldTextureSize / 2; y++)
                {
                    for (int x = 0; x < WorldTextureSize; x++)
                    {
                        int topIndex = y * WorldTextureSize + x;
                        int bottomIndex = (WorldTextureSize - y - 1) * WorldTextureSize + x;

                        // 交换上下像素
                        Color temp = pixels[topIndex];
                        pixels[topIndex] = pixels[bottomIndex];
                        pixels[bottomIndex] = temp;
                    }
                }

                // 将翻转后的像素重新应用到Texture2D对象中
                FinalTexture.SetPixels(pixels);
                FinalTexture.Apply();

                var sceneLightmapFullPath = AssetDatabase.GetAssetPath(lightmapTexture.lightmapColor);

                var lightmapTexturePath = Path.GetDirectoryName(sceneLightmapFullPath);
                var expoortedTextureName = lightmapTexture.lightmapColor.name + "_WorldMapping";

                byte[] bytes = FinalTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

                var targetPath = lightmapTexturePath + "/" + expoortedTextureName + ".exr";
                System.IO.File.WriteAllBytes(targetPath, bytes);
                UnityEditor.AssetDatabase.Refresh();
                // 重新导入资源
                var assetPath = lightmapTexturePath + "/" + expoortedTextureName + ".exr";
                UnityEditor.AssetDatabase.ImportAsset(assetPath);

                var reloadedTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
                Selection.activeObject = reloadedTexture;
                EditorGUIUtility.PingObject(reloadedTexture);
            }
        }
    }

    RenderTexture mTempRt = null;

    void LightmapWorldMapping(MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        var mesh = meshFilter.sharedMesh;
        var preRenderTarget = RenderTexture.active;
        var lightmapTexture = LightmapSettings.lightmaps[meshRenderer.lightmapIndex].lightmapColor;
        var localToWorldMatrix = meshFilter.transform.localToWorldMatrix;
        var bounds = meshRenderer.bounds;

        BakeMaterial.SetVector("_TerrainWorldInfo", new Vector4(bounds.center.x, bounds.center.z, 0.5f * bounds.size.x, 0.5f * bounds.size.z));
        BakeMaterial.SetVector("_LightmapScaleOffset", meshRenderer.lightmapScaleOffset);
        BakeMaterial.SetTexture("_TerrainWorldLightmap", lightmapTexture);

        // 创建临时的 RenderTexture
        if (!mTempRt)
        {
            mTempRt = new RenderTexture(WorldTextureSize, WorldTextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat, 0);
            mTempRt.Create();
        }

        Camera.SetupCurrent(null);
        RenderTexture.active = mTempRt;
        GL.Clear(true, true, Color.black);
        GL.PushMatrix();
        BakeMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, localToWorldMatrix);
        GL.PopMatrix();
        RenderTexture.active = preRenderTarget;
    }
}
