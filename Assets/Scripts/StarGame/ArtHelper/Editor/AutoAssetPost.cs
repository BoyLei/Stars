using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

public class AutoAssetPost : AssetPostprocessor
{
    static string SPRITES_DIR = "Assets/Res/UI/Textures/";

    public static TextureImporterSettingGroup settinggroup;

    public static TextureImporterSettingGroup settingGroup
    {
        get
        {
            if (settinggroup == null)
            {
                settinggroup = AssetDatabase.LoadAssetAtPath<TextureImporterSettingGroup>("Assets/Editor/AssetImporter/TextureImporterSetting/TextureImporterSetting.asset");
            }
            return settinggroup;
        }
    }
    //void OnPostprocessTexture(Texture2D texture)
    //{
    //    TextureImporter textureImporter = assetImporter as TextureImporter;
    //    if (textureImporter == null)
    //        return;
    //}

    /* void OnPostprocessTexture(Texture2D texture)
     {
         if (assetPath.EndsWith(".jpg") || assetPath.EndsWith(".png"))
         {
             string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

             if (fileName.EndsWith("_A"))
             {
                 foo();
             }
         }
     }

     void foo()
     {
         // 执行你希望在文件名以 "_A" 结尾的图片导入时执行的逻辑
     }*/

    static public bool InBlockList(string fileName)
    {
        return settingGroup.InBlockList(fileName);
    }
    //尔东功能的快捷文件夹设定
    [MenuItem("自动化工具/将选择文件夹的贴图设置成规定格式", false, 22)]
    static void SetSelectTextures()
    {
        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < selects.Length; ++i)
        {
            Object selected = selects[i];
            string path = AssetDatabase.GetAssetPath(selected);
            AssetImporter asset = AssetImporter.GetAtPath(path);
            TextureImporter textureImporter = asset as TextureImporter;
            TextureImporterSetting setting = settingGroup.GetSetting(path);
            if (textureImporter != null && setting != null)
            {
                SetTextureFormat(textureImporter, setting);
                // SetTexturePackingTag(textureImporter, path);

            }
        }
        AssetDatabase.Refresh();
    }

    public static void SetTextureFormat(TextureImporter textureImporter, TextureImporterSetting setting)
    {
        if (setting.IsThisUSE == false)
        {
            return;
        }
        //textureImporter.textureType = setting.TextureType;//这个不应该设置，没用修改原来的格式，原来一定是有用的
        if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            textureImporter.spritePixelsPerUnit = 72;
        }
        textureImporter.isReadable = setting.isReadable;
        textureImporter.mipmapEnabled = setting.mipmapEnabled;
        if (!textureImporter.streamingMipmaps)//原图是False，一定没设定优先级，根据宏设定来驱动他
        {
            textureImporter.streamingMipmaps = setting.streamMipmapEnabled;
            textureImporter.streamingMipmapsPriority = setting.streamMipmapLevel;
        }
        else//如果是流式加载开启过
        {
            if (setting.ForceRefStreamPro)//同意就强刷
            {
                textureImporter.streamingMipmaps = setting.streamMipmapEnabled;
                textureImporter.streamingMipmapsPriority = setting.streamMipmapLevel;
            }
        }

        textureImporter.sRGBTexture = setting.sRGBTexture;
        textureImporter.npotScale = setting.TextureImporterNPOTScale;
        if (setting.alphaIsTransparencyForce == true)
        {
            textureImporter.alphaIsTransparency = setting.alphaIsTransparency;
        }

        textureImporter.wrapMode = setting.WrapMode;
        textureImporter.filterMode = setting.FilterMode;
        textureImporter.anisoLevel = setting.AnisoLevel;

        TextureImporterPlatformSettings ios = textureImporter.GetPlatformTextureSettings("iPhone");
        ios.overridden = setting.IPhoneSettings.overridden;
        ios.format = setting.IPhoneSettings.ImporterFormat;
        ios.maxTextureSize = setting.IPhoneSettings.maxTextureSize;
        ios.resizeAlgorithm = setting.IPhoneSettings.resizeAlgorithm;
        ios.textureCompression = setting.IPhoneSettings.textureCompression;
        textureImporter.SetPlatformTextureSettings(ios);

        TextureImporterPlatformSettings android = textureImporter.GetPlatformTextureSettings("Android");
        android.overridden = setting.AndroidSettings.overridden;
        android.format = setting.AndroidSettings.ImporterFormat;
        android.maxTextureSize = setting.AndroidSettings.maxTextureSize;
        android.resizeAlgorithm = setting.AndroidSettings.resizeAlgorithm;
        android.textureCompression = setting.AndroidSettings.textureCompression;
        textureImporter.SetPlatformTextureSettings(android);

        TextureImporterPlatformSettings pc = textureImporter.GetPlatformTextureSettings("Standalone");
        pc.overridden = setting.StandaloneSettings.overridden;
        pc.format = setting.StandaloneSettings.ImporterFormat;
        pc.maxTextureSize = setting.StandaloneSettings.maxTextureSize;
        pc.resizeAlgorithm = setting.StandaloneSettings.resizeAlgorithm;
        pc.textureCompression = setting.StandaloneSettings.textureCompression;
        textureImporter.SetPlatformTextureSettings(pc);

        textureImporter.SaveAndReimport();
    }

    private static bool IsNeedAtlas(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        if (height > 1024 & width > 512)
        {
            return false;
        }
        else if (width > 1024 & height > 512)
        {
            return false;
        }
        else if (width <= 2048 & height <= 2048)
        {
            return true;
        }
        return false;
    }


    public static bool SetTexturePackingTag(TextureImporter textureImporter, string path)
    {
        bool needChange = false;
        string tag = GetSpritePackingTag(path);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (!IsNeedAtlas(texture))
        {
            tag = "";
        }
        if (tag != textureImporter.spritePackingTag)
        {
            textureImporter.spritePackingTag = tag;
            needChange = true;
        }
        return needChange;
    }

    public static string GetSpritePackingTag(string path)
    {
        string packingTag = path.ToLower();
        packingTag = Path.GetDirectoryName(packingTag);
        packingTag = packingTag.Replace("\\", "/");
        packingTag = packingTag.Replace(SPRITES_DIR.ToLower(), "");
        packingTag = packingTag.Replace('/', '_');
        return packingTag;
    }

    void OnPreprocessTexture()
    {
        //既然每次导入图片都要设定，就要根据类型变成更全的设定，这里是被动就依赖各个类型的通用全局/简单处理
        //要么就不设定，根据主动尔东编辑器apply或者快捷设定
        //因为每个文件夹，场景和ui的设定不同，所以要主动设置其根据文件夹设定而不同的设定，并且之后也要加入对类似场景中法线和信息图的处理格式类型分门别类处理
        /*TextureImporter textureImporter = (TextureImporter)assetImporter;
        if (textureImporter != null)
        {
            string fileName = System.IO.Path.GetFileName(assetImporter.assetPath);
            if(!InBlockList(fileName))
            {
                TextureImporterSetting setting = settingGroup.GetSetting(assetImporter.assetPath);
                if (setting != null)
                {
                    SetTextureFormat(textureImporter, setting);
                }
            }

        }*/

        #if ASSET_CHECKER

        TextureImporter textureImporter = assetImporter as TextureImporter;
        if (textureImporter == null)
            return;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        if (fileName.EndsWith("_A"))
        {
            ProcessTexturesA(assetPath);
        }
        else if (fileName.EndsWith("_M"))
        {
            ProcessTexturesM(assetPath);
        }
        else if (fileName.EndsWith("_N"))
        {
            ProcessTexturesN(assetPath);
        }
        else if (fileName.EndsWith("_D"))
        {
            ProcessTexturesD(assetPath);
        }
        else if (fileName.EndsWith("_X"))
        {
            ProcessTexturesX(assetPath);
        }
        else if (fileName.EndsWith("_SDM"))
        {
            ProcessTexturesSDM(assetPath);
        }
        else if (fileName.EndsWith("_R"))
        {
            ProcessTexturesR(assetPath);
        }else if (assetPath.Contains("Splatmap"))
        {
            ProcessTexturesSplatmap(assetPath);
        }else if (assetPath.Contains("Res/UI")) // 检查贴图是否在"Res/UI/"文件夹下
        {
            ProcessTexturesUIAtlas(assetPath);
        }else
        {
            string fileNameEx = System.IO.Path.GetFileName(assetPath);
            if (fileNameEx.EndsWith(".exr"))
            {
                ProcessTexturesLightMap(assetPath);
            }
            else if (assetPath.Contains("Effects/Textures"))
            {
                //ProcessTexturesEffect(assetPath);
            }
            else
            {
                ProcessTexturesNormal(assetPath);
            }

        }
       
        if (assetPath.Contains("@@@"))
        {
            // 更改导入设置，使用Unity自带算法平滑模型，会自动合并重合顶点
            ModelImporter model = assetImporter as ModelImporter;
            model.importNormals = ModelImporterNormals.Calculate;
            model.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
            model.normalSmoothingAngle = 180.0f;
            model.importAnimation = false;
            model.materialImportMode = ModelImporterMaterialImportMode.None;
        }

        //textureImporter.mipmapEnabled = false;
#endif
    }
    Color[] ComputeSmoothedNormal(Mesh smoothedMesh, Mesh originalMesh, int maxOverlapvertices = 10)
    {
        Vector3[] normals = new Vector3[originalMesh.vertexCount];
        Vector4[] tangents = new Vector4[originalMesh.vertexCount];
        Color[] colors = new Color[originalMesh.vertexCount];
        //Vector3[] smoothedNormals ;
        //将SmoothMesh模型的顶点和法线数据加入Dictionary
        Dictionary<Vector3, Vector3> smoothVerNorDictionary = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < smoothedMesh.vertexCount; i++)
        {
            if (smoothVerNorDictionary.ContainsKey(smoothedMesh.vertices[i]))
            {
                Vector3 v3T = smoothVerNorDictionary[smoothedMesh.vertices[i]];
                v3T = v3T + smoothedMesh.normals[i];
                smoothVerNorDictionary.Remove(smoothedMesh.vertices[i]);
                smoothVerNorDictionary.Add(smoothedMesh.vertices[i], v3T);
                //Debug.Log("smoothVerNorDictionary add"+smoothedMesh.vertices[i] + v3T);
            }
            else
            {
                smoothVerNorDictionary.Add(smoothedMesh.vertices[i], smoothedMesh.normals[i]);
                //Debug.Log("smoothVerNorDictionary add"+smoothedMesh.vertices[i] + smoothedMesh.normals[i]);
            }

        }
        /*for (int i = 0; i < result.Length; i++)
        {
            if (result[i].normal != Vector3.zero)
                smoothedNormals += result[i].normal;
            else
                break;
        }*/
        //smoothedNormals = smoothedNormals.normalized;
        //smoothedNormals = smoothedMesh.normals;
        normals = originalMesh.normals;
        tangents = originalMesh.tangents;
        for (int index = 0; index < originalMesh.vertexCount; index++)
        {
            //对于OriginalMesh内的每一个顶点 查找SmoothMesh内对应的顶点的法线
            if (smoothVerNorDictionary.ContainsKey(originalMesh.vertices[index]))
            {
                //Debug.Log("查找到顶点:"+index+"的位置为:"+originalMesh.vertices[index]);
                Vector3 smoothNormal = smoothVerNorDictionary[originalMesh.vertices[index]];
                smoothNormal = smoothNormal.normalized;
                var binormal = (Vector3.Cross(normals[index], tangents[index]) * tangents[index].w).normalized;

                var tbn = new Matrix4x4(
                    tangents[index],
                    binormal,
                    normals[index],
                    Vector4.zero);
                tbn = tbn.transpose;

                var bakedNormal = tbn.MultiplyVector(/*smoothedNormals[index]*/smoothNormal).normalized;

                Color color = new Color();
                color.r = (bakedNormal.x * 0.5f) + 0.5f;
                color.g = (bakedNormal.y * 0.5f) + 0.5f;
                color.b = colors[index].b;
                color.a = colors[index].a;

                colors[index] = color;
            }
            else
            {
                //11Debug.Log("无法找2到顶点:"+index+"的位置为:"+originalMesh.vertices[index]);
            }
        }

        /*Debug.Log("平滑模型顶点数为："+smoothedMesh.vertexCount);
        Debug.Log("原模型顶点数为："+originalMesh.vertexCount);*/
        return colors;
    }

    // 在GameObject生成后调用，对GameObject的修改会影响生成结果，但引用不会保留
    void OnPostprocessModel(GameObject g)
    {
//#if STAR_CHECK
        string fileNameEx = Path.GetFileName(assetPath);
        if (!string.IsNullOrEmpty(fileNameEx))
        {
            ProcessFBX(assetPath);
        }
        //Debug.Log("开始生产剩余的模型！！！！！！！！！！！！");
        if ((!g.name.Contains("R_DP") && !g.name.Contains("R_WD")) || g.name.Contains("@@@"))
            return;

        Debug.Log("法线平滑工具检测到命名带R_DP或者R_WD的模型,名称为:" + g.name + "  平滑法线工具开始运行");
        ModelImporter model = assetImporter as ModelImporter;

        string src = model.assetPath;
        // string dst = Path.GetDirectoryName(src) + "/@@@" + Path.GetFileName(src);

        // 复制一个模型用unity的算法生成描边法线，此处ImportAsset完之后会再次导入此asset进入else分支(仅2019.3.1+)
        // if (!File.Exists(Application.dataPath + "/" + dst.Substring(7)))
        // {
        //     AssetDatabase.CopyAsset(src, dst);
        //     AssetDatabase.ImportAsset(dst);
        // }
        // else
        {
            //var go = AssetDatabase.LoadAssetAtPath<GameObject>(dst);

            Dictionary<string, Mesh> originalMesh = GetMesh(g)/*, smoothedMesh = GetMesh(go)*/;

            foreach (var item in originalMesh)
            {
                var m = item.Value;
                Color[] colorTemp;
                colorTemp = WirteAverageNormalToTangent(m);
                //把平滑的法线也存储到uv2,uv3中去.
                Vector2[] uv2s = new Vector2[colorTemp.Length];
                for (int i = 0; i < colorTemp.Length; i++)
                {
                    uv2s[i].x = colorTemp[i].r;
                    uv2s[i].y = colorTemp[i].g;
                }
                m.uv2 = uv2s;
                uv2s = null;

                //shader中可以用平滑法线xy还原z,因此省去uv3
                Vector2[] uv3s = new Vector2[colorTemp.Length];
                for (int i = 0; i < colorTemp.Length; i++)
                {
                    uv3s[i].x = colorTemp[i].b;
                }
                m.uv3 = uv3s;

                //uv2s.;
            }
            Debug.Log("模型" + g.name + "的平滑法线已经导入至uv2和uv3中.");
        }
//#endif
    }

    Dictionary<string, Mesh> GetMesh(GameObject go)
    {
        Dictionary<string, Mesh> dic = new Dictionary<string, Mesh>();
        foreach (var item in go.GetComponentsInChildren<MeshFilter>())
            dic.Add(item.name, item.sharedMesh);
        if (dic.Count == 0)
            foreach (var item in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                dic.Add(item.name, item.sharedMesh);
        return dic;
    }


    private static Vector3[] GetTangentSpaceNormal(Vector3[] smoothedNormals, Mesh srcMesh)
    {
        Vector3[] normals = srcMesh.normals;
        Vector4[] tangents = srcMesh.tangents;

        Vector3[] smoothedNormals_TS = new Vector3[smoothedNormals.Length];

        for (int i = 0; i < smoothedNormals_TS.Length; i++)
        {
            Vector3 normal = normals[i];
            Vector4 tangent = tangents[i];

            Vector3 tangentV3 = new Vector3(tangent.x, tangent.y, tangent.z);

            var bitangent = Vector3.Cross(normal, tangentV3) * tangent.w;
            bitangent = bitangent.normalized;

            var TBN = new Matrix4x4(tangentV3, bitangent, normal, Vector4.zero);
            TBN = TBN.transpose;

            var smoothedNormal_TS = TBN.MultiplyVector(smoothedNormals[i]).normalized;


            smoothedNormals_TS[i] = smoothedNormal_TS;
        }

        return smoothedNormals_TS;
    }
    Color[] ComputeSmmothedNormalFromOriginalMesh(Mesh originalMesh)
    {
        Color[] colors = new Color[originalMesh.vertexCount];
        Dictionary<Vector3, Vector3> smoothNormalDic = new Dictionary<Vector3, Vector3>();
        //遍历顶点,把相同坐标顶点的法线作和并归一化获得平滑法线
        for (int i = 0; i < originalMesh.vertexCount; i++)
        {
            if (smoothNormalDic.ContainsKey(originalMesh.vertices[i]))
            {
                Vector3 normalTemp = smoothNormalDic[originalMesh.vertices[i]];
                smoothNormalDic.Remove(originalMesh.vertices[i]);
                normalTemp = (normalTemp + originalMesh.normals[i]).normalized;
                smoothNormalDic.Add(originalMesh.vertices[i], normalTemp);
            }
            else
            {
                smoothNormalDic.Add(originalMesh.vertices[i], originalMesh.normals[i]);
            }
        }
        //遍历顶点,把Dictionary内的平滑法线取出赋予原本的顶点.
        for (int i = 0; i < originalMesh.vertexCount; i++)
        {
            if (smoothNormalDic.ContainsKey(originalMesh.vertices[i]))
            {
                Vector3 normalTemp = smoothNormalDic[originalMesh.vertices[i]];
                colors[i].r = normalTemp.x;
                colors[i].g = normalTemp.y;
                colors[i].b = normalTemp.z;
            }
        }

        return colors;
    }

    private static Color[] WirteAverageNormalToTangent(Mesh mesh)
    {
        Color[] colors = new Color[mesh.vertexCount];
        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] =
                    (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }

        var averageNormals = new Vector3[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
        }

        Vector3[] averageNormalTangent = GetTangentSpaceNormal(averageNormals, mesh);
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            colors[i].r = (averageNormalTangent[i].x * 0.5f) + 0.5f;
            colors[i].g = (averageNormalTangent[i].y * 0.5f) + 0.5f;
            colors[i].b = (averageNormalTangent[i].z * 0.5f) + 0.5f;
            /*colors[i].b = colors[i].b;
            colors[i].a = colors[i].a;*/
            /*colors[i].r = averageNormalTangent[i].x;
            colors[i].g = averageNormalTangent[i].y;
            colors[i].b = averageNormalTangent[i].z;*/
        }

        return colors;
    }


    public static string GetFileNameWithoutExtension(string filePath)
    {
        int lastIndex = filePath.LastIndexOfAny(new char[] { '/', '\\' });

        if (lastIndex >= 0 && lastIndex < filePath.Length - 1)
        {
            string fileName = filePath.Substring(lastIndex + 1);
            int dotIndex = fileName.LastIndexOf('.');

            if (dotIndex > 0 && dotIndex < fileName.Length - 1)
            {
                return fileName.Substring(0, dotIndex);
            }
        }

        return null;
    }

    private static void ProcessTexturesA(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            if (importer.assetPath.Contains("/La/"))//判断是否是地表贴图
            {
                //仅有在配置不正确的时候 对配置进行修改
                if (!(platformSettings.overridden == true &&
                      platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                      platformSettings.maxTextureSize == 512 &&
                      importer.sRGBTexture == true &&
                      importer.streamingMipmaps == true &&
                      importer.mipmapEnabled == true &&
                      importer.textureType == TextureImporterType.Default)
                   )
                {
                    platformSettings.overridden = true;
                    platformSettings.format = TextureImporterFormat.ASTC_6x6;
                    //存在两个情况1，美术命名不规范；2角色可能也用_A没有区分开
                    platformSettings.maxTextureSize = 512;
                    importer.sRGBTexture = true;
                    importer.streamingMipmaps = true;
                    importer.mipmapEnabled = true;
                    importer.textureType = TextureImporterType.Default;
    
                
    
                    importer.SetPlatformTextureSettings(platformSettings);
                    //alpha is transparency 不妨碍A通道读取 不管他
                    importer.SaveAndReimport();
                }
            }
            else
            {
                //仅有在配置不正确的时候 对配置进行修改
                if (!(platformSettings.overridden == true &&
                      platformSettings.format == TextureImporterFormat.ASTC_8x8 &&
                      /*platformSettings.maxTextureSize == 512 &&*/
                      importer.sRGBTexture == true &&
                      importer.streamingMipmaps == true &&
                      importer.mipmapEnabled == true &&
                      importer.textureType == TextureImporterType.Default)
                   )
                {
                    platformSettings.overridden = true;
                    platformSettings.format = TextureImporterFormat.ASTC_8x8;
                    //存在两个情况1，美术命名不规范；2角色可能也用_A没有区分开
                    /* platformSettings.maxTextureSize = 512;*/
                    importer.sRGBTexture = true;
                    importer.streamingMipmaps = true;
                    importer.mipmapEnabled = true;
                    importer.textureType = TextureImporterType.Default;
    
                
    
                    importer.SetPlatformTextureSettings(platformSettings);
                    //alpha is transparency 不妨碍A通道读取 不管他
                    importer.SaveAndReimport();
                }
            }
            

            TextureImporterPlatformSettings platformSettingsWin = importer.GetPlatformTextureSettings("Standalone");

            if (importer.assetPath.Contains("/La/")) //判断是否是地表贴图
            {
                 if (!(platformSettingsWin.overridden == true &&
                       platformSettingsWin.format == TextureImporterFormat.DXT5 &&
                       platformSettingsWin.maxTextureSize == 512))
                 {
                    platformSettingsWin.overridden = true;
                    platformSettingsWin.format = TextureImporterFormat.DXT5;
                    //存在两个情况1，美术命名不规范；2角色可能也用_A没有区分开
                    platformSettingsWin.maxTextureSize = 512;
    
                    importer.SetPlatformTextureSettings(platformSettingsWin);
                    //alpha is transparency 不妨碍A通道读取 不管他
                    importer.SaveAndReimport();
                 }
            }
            else
            {
                if (!(platformSettingsWin.overridden == true &&
                      platformSettingsWin.format == TextureImporterFormat.DXT5 
                      //&& platformSettingsWin.maxTextureSize == 512
                      ))
                {
                    platformSettingsWin.overridden = true;
                    platformSettingsWin.format = TextureImporterFormat.DXT5;
                    //存在两个情况1，美术命名不规范；2角色可能也用_A没有区分开
                    //platformSettingsWin.maxTextureSize = 512;
                
                    importer.SetPlatformTextureSettings(platformSettingsWin);
                    //alpha is transparency 不妨碍A通道读取 不管他
                    importer.SaveAndReimport();
                }
            }
           
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }
    private static void ProcessTexturesM(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            // 获取图片的名字
            string textureName = GetFileNameWithoutExtension(texturePath);

            // 判断图片名字中是否包含 "Terrain"，如果包含则开启 sRGB，否则关闭
            bool IfContainTerr = textureName.Contains("Terr", System.StringComparison.Ordinal);

            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                  importer.streamingMipmaps == true &&
                  importer.mipmapEnabled == true &&
                  importer.textureType == TextureImporterType.Default &&
                  ((IfContainTerr == true && importer.sRGBTexture == true) ||
                   (IfContainTerr == false && importer.sRGBTexture == false))
                ))
            {
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
                
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.Default;
    
                if (IfContainTerr)
                {
                    importer.sRGBTexture = true;
                }
                else
                {
                    importer.sRGBTexture = false;
                }
    
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
            
            
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    private static void ProcessTexturesN(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");

            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.overridden == true &&
                  platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                  importer.sRGBTexture == false &&
                  importer.streamingMipmaps == true &&
                  importer.mipmapEnabled == true &&
                  importer.textureType == TextureImporterType.NormalMap
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
    
        /*        if (importer.textureType == TextureImporterType.NormalMap)
                {
    
                }
                else if (importer.textureType == TextureImporterType.Default)
                {
                    importer.sRGBTexture = false;
                }*/
                importer.sRGBTexture = false;
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.NormalMap;
                
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();

            }
            TextureImporterPlatformSettings platformSettingsWin = importer.GetPlatformTextureSettings("Standalone");

            if (!(platformSettingsWin.overridden == true &&
                platformSettingsWin.format == TextureImporterFormat.DXT5))
            {
                platformSettingsWin.overridden = true;
                platformSettingsWin.format = TextureImporterFormat.DXT5;

                importer.SetPlatformTextureSettings(platformSettingsWin);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    private static void ProcessTexturesD(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;
        
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");

            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.overridden == true &&
                  platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                  importer.sRGBTexture == true &&
                  importer.streamingMipmaps == true &&
                  importer.mipmapEnabled == true &&
                  importer.textureType == TextureImporterType.Default
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
    
    
                importer.sRGBTexture = true;
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.Default;
    
    
    
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
            

        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    private static void ProcessTexturesX(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            
            // 获取图片的名字
            string textureName = GetFileNameWithoutExtension(texturePath);
            
            // 判断图片名字中是否包含 "Terrain"，如果包含则开启 sRGB，否则关闭
            bool IfContainTerr = textureName.Contains("Terr", System.StringComparison.Ordinal);

            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.overridden == true &&
                  platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                  importer.sRGBTexture == false &&
                  importer.streamingMipmaps == true &&
                  importer.textureType == TextureImporterType.Default &&
                  ((IfContainTerr == true && importer.sRGBTexture == false) || (IfContainTerr == false && importer.sRGBTexture == true))
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
                
                importer.sRGBTexture = false;
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.Default;
                
                // 判断图片名字中是否包含 "Terrain"，如果包含则开启 sRGB，否则关闭
                if (IfContainTerr)
                {
                    importer.sRGBTexture = false;
                }
                else
                {
                    importer.sRGBTexture = true;
                }
    
                /*importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();*/
                
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
            TextureImporterPlatformSettings platformSettingsWin = importer.GetPlatformTextureSettings("Standalone");

            if (!(platformSettingsWin.overridden == true &&
                platformSettingsWin.format == TextureImporterFormat.DXT5))
            {
                platformSettingsWin.overridden = true;
                platformSettingsWin.format = TextureImporterFormat.DXT5;

                importer.SetPlatformTextureSettings(platformSettingsWin);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }


        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    private static void ProcessTexturesSDM(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            
            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.overridden == true && 
                platformSettings.format ==  TextureImporterFormat.ASTC_6x6&&
                importer.sRGBTexture == false &&
                importer.streamingMipmaps == true&&
                importer.mipmapEnabled == true&&
                importer.textureType == TextureImporterType.Default
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
    
    
                importer.sRGBTexture = false;
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.Default;
    
    
    
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }

    private static void ProcessTexturesR(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            ;
            
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            //仅有在配置不正确的时候 对配置进行修改
            if ((platformSettings.overridden == true &&
                platformSettings.format == TextureImporterFormat.ASTC_6x6 &&
                importer.sRGBTexture == true &&
                importer.streamingMipmaps == true &&
                importer.mipmapEnabled == true &&
                importer.textureType == TextureImporterType.Default
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
    
    
                importer.sRGBTexture = true;
                importer.streamingMipmaps = true;
                importer.mipmapEnabled = true;
                importer.textureType = TextureImporterType.Default;
    
                
                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
            
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }
    private static void ProcessTexturesSplatmap(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");
            //仅有在配置不正确的时候 对配置进行修改
            if (!(platformSettings.overridden == true &&
                platformSettings.format == TextureImporterFormat.RGBA32 &&
                importer.mipmapEnabled == false &&
                importer.textureType == TextureImporterType.Default
                ))
            {
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.RGBA32;
                importer.mipmapEnabled = false;
                importer.textureType = TextureImporterType.Default;


                importer.SetPlatformTextureSettings(platformSettings);
                //alpha is transparency 不妨碍A通道读取 不管他
                importer.SaveAndReimport();
            }
            TextureImporterPlatformSettings platformSettingsWin = importer.GetPlatformTextureSettings("Standalone");

            if (!(platformSettingsWin.overridden == true &&
                platformSettingsWin.format == TextureImporterFormat.DXT5))
            {
                platformSettingsWin.overridden = true;
                platformSettingsWin.format = TextureImporterFormat.DXT5;

                importer.SetPlatformTextureSettings(platformSettingsWin);
                importer.SaveAndReimport();
            }
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }
    }
    //Dither4444的改进算法
    void SetOnPostprocessTexture(Texture2D texture)
    {
        if (assetPath.Contains("_dither565"))
        {
            var texw = texture.width;
            var texh = texture.height;

            var pixels = texture.GetPixels();
            var offs = 0;

            var k1Per31 = 1.0f / 31.0f;

            var k1Per32 = 1.0f / 32.0f;
            var k5Per32 = 5.0f / 32.0f;
            var k11Per32 = 11.0f / 32.0f;
            var k15Per32 = 15.0f / 32.0f;

            var k1Per63 = 1.0f / 63.0f;

            var k3Per64 = 3.0f / 64.0f;
            var k11Per64 = 11.0f / 64.0f;
            var k21Per64 = 21.0f / 64.0f;
            var k29Per64 = 29.0f / 64.0f;

            var k_r = 32; //R&B压缩到5位，所以取2的5次方
            var k_g = 64; //G压缩到6位，所以取2的6次方

            for (var y = 0; y < texh; y++)
            {
                for (var x = 0; x < texw; x++)
                {
                    float r = pixels[offs].r;
                    float g = pixels[offs].g;
                    float b = pixels[offs].b;

                    var r2 = Mathf.Clamp01(Mathf.Floor(r * k_r) * k1Per31);
                    var g2 = Mathf.Clamp01(Mathf.Floor(g * k_g) * k1Per63);
                    var b2 = Mathf.Clamp01(Mathf.Floor(b * k_r) * k1Per31);

                    var re = r - r2;
                    var ge = g - g2;
                    var be = b - b2;

                    var n1 = offs + 1;
                    var n2 = offs + texw - 1;
                    var n3 = offs + texw;
                    var n4 = offs + texw + 1;

                    if (x < texw - 1)
                    {
                        pixels[n1].r += re * k15Per32;
                        pixels[n1].g += ge * k29Per64;
                        pixels[n1].b += be * k15Per32;
                    }

                    if (y < texh - 1)
                    {
                        pixels[n3].r += re * k11Per32;
                        pixels[n3].g += ge * k21Per64;
                        pixels[n3].b += be * k11Per32;

                        if (x > 0)
                        {
                            pixels[n2].r += re * k5Per32;
                            pixels[n2].g += ge * k11Per64;
                            pixels[n2].b += be * k5Per32;
                        }

                        if (x < texw - 1)
                        {
                            pixels[n4].r += re * k1Per32;
                            pixels[n4].g += ge * k3Per64;
                            pixels[n4].b += be * k1Per32;
                        }
                    }

                    pixels[offs].r = r2;
                    pixels[offs].g = g2;
                    pixels[offs].b = b2;

                    offs++;
                }
            }

            texture.SetPixels(pixels);
            EditorUtility.CompressTexture(texture, TextureFormat.RGB565, (int)TextureCompressionQuality.Best);
        }
    }
    private static void ProcessTexturesUIAtlas(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            if (texturePath.Contains("Atlas") || texturePath.Contains("/Texture"))//后面看 图集和散图是否要区分处理？
            {
                if (texturePath.Contains("Atlas")) {
                    importer.textureType = TextureImporterType.Sprite;
                }
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                TextureImporterPlatformSettings platformSettingsAndroid = importer.GetPlatformTextureSettings("Android");
                platformSettingsAndroid.overridden = true;
                platformSettingsAndroid.format = TextureImporterFormat.ASTC_6x6;
                platformSettingsAndroid.maxTextureSize = 2048;
                platformSettingsAndroid.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
                importer.SetPlatformTextureSettings(platformSettingsAndroid);
                importer.SaveAndReimport();
            }
            else 
            {
                ProcessTexturesNormal(texturePath);
            }
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }

    }
    private static void ProcessTexturesLightMap(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettingsAndroid = importer.GetPlatformTextureSettings("Android");
            platformSettingsAndroid.overridden = true;
            platformSettingsAndroid.format = TextureImporterFormat.ASTC_5x5;
            importer.SetPlatformTextureSettings(platformSettingsAndroid);
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }

    }
    private static void ProcessFBX(string fbxPath)
    {
        ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);

        if (importer != null)
        {
            // 设置FBX模型的导入选项  
            importer.materialImportMode =  ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load FBX model at path: " + fbxPath);
        }
    }
    private static void ProcessTexturesEffect(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettingsAndroid = importer.GetPlatformTextureSettings("Android");
            platformSettingsAndroid.overridden = true;
            platformSettingsAndroid.maxTextureSize = 256;
            platformSettingsAndroid.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(platformSettingsAndroid);
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }

    }
    private static void ProcessTexturesNormal(string texturePath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

        if (importer != null)
        {
            TextureImporterPlatformSettings platformSettingsAndroid = importer.GetPlatformTextureSettings("Android");
            platformSettingsAndroid.overridden = true;
            platformSettingsAndroid.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(platformSettingsAndroid);
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + texturePath);
        }

    }
    //武文说卡
    /*  static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
      {
          foreach (string s in importedAssets)
          {
              if (s.EndsWith(".dds"))
              {
                  UnityEngine.Debug.LogError("禁止使用.dds贴图文件:" + s);
                  File.Delete(s);
              }
              //if (s.EndsWith(".tga") || s.EndsWith(".jpg"))
              if (s.EndsWith(".png") || s.EndsWith(".jpg"))
              {
                  UnityEngine.Debug.LogError("贴图文件请使用.tga格式:" + s);
              }
          }
          //  AssetDatabase.Refresh();
      }*/

    private void OnPostprocessTexture(Texture2D texture)
    {
        string texturePath = assetPath.ToLower();
        if (texturePath.EndsWith(".dds"))
        {
            UnityEngine.Debug.LogError("禁止使用.dds贴图文件:" + texturePath);
        }
        if (texturePath.EndsWith(".png") || texturePath.EndsWith(".jpg"))
        {
            Debug.LogError("贴图文件请使用.tga格式: " + texturePath);
        }
    }

    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        //modelImporter.materialImportMode = false;
        string modelName = GetFileNameWithoutExtension(assetImporter.assetPath);
        if(modelName.Contains("$", System.StringComparison.Ordinal))
        {
            modelImporter.meshCompression = ModelImporterMeshCompression.Low;
        }
        else
        {
            modelImporter.meshCompression = ModelImporterMeshCompression.Medium;
        }
        

        //modelImporter.importTangents = ModelImporterTangents.None;
    }


    public void fooA()
    {

    }
}

//public class AudioClipAssetPost : AssetPostprocessor
//{
//    void OnPostprocessAudio(AudioClip clip)
//    {
//        if (assetPath.EndsWith(".mp3"))
//        {
//            AudioImporter imp = assetImporter as AudioImporter;
//            AudioImporterSampleSettings set = imp.defaultSampleSettings;
//            set.loadType = AudioClipLoadType.Streaming;
//            imp.defaultSampleSettings = set;
//        }
//    }
//}

