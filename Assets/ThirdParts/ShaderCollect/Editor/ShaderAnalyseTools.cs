using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.IO;
using GameDLL;
using UnityEditor.SceneManagement;
using GameEditor.Tools.Statistics;
using System.Text.RegularExpressions;

namespace GameEditor.Tools
{
    class ShaderAnalyseResult : ScriptableObject
    {
        [Serializable]
        public class KeywordReference
        {
            public string keyword;
            public Shader[] refShaders;
        }

        [Serializable]
        public class UnsupportedKeywordReference
        {
            public string keyword;
            public Material[] refMaterials;
        }

        public Shader[] unusingShaders;
        public Shader[] usingShaders;
        public KeywordReference[] keywordsReference;
        public UnsupportedKeywordReference[] warningReference;
        public UnsupportedKeywordReference[] unsupportedReference;

        public void Clear()
        {
            unusingShaders = null;
            usingShaders = null;
            keywordsReference = null;
        }
    }

    [Serializable]
    public class ShaderAnalyseToolsConfig
    {
        [Serializable]
        public class KeywordCombination
        {
            public string[] keywords;
        }
        [Serializable]
        public class KeywordCombinationPattern
        {
            public string[] shaderNames;
            public KeywordCombination[] fixedCombination;
            public int fixedCombinationIndexLimit;
            public KeywordCombination[] flexibleCombination;
        }

        public string keepSuffix;
        public string[] keywordWipeOffPattern;
        public KeywordCombinationPattern combinationPattern;
    }


    class ShaderAnalyseTools
    {
        const string outputResultPath = "Assets/Analyse/shaderAnalyse.asset";
        const string jsonFilePath = "Assets/Editor Default Resources/ShaderAnalyseTools.json";
        const string shaderExt = ".shader";
        const string materialExt = ".mat";

        public enum KeywordType
        {
            Global = 1,
            Local = 2,
            Both = Global | Local,
        }

        public static string[] GetShaderKeywords(Shader shader, KeywordType keywordType)
        {
            Type shaderUtilType = typeof(ShaderUtil);
            object obj1 = shaderUtilType.InvokeMember("GetShaderGlobalKeywords",
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { shader });
            object obj2 = shaderUtilType.InvokeMember("GetShaderLocalKeywords",
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { shader });
            string[] keywords = obj1 as string[];
            string[] localKeywords = obj2 as string[];
            List<string> keywordList = new List<string>();
            if((keywordType & KeywordType.Global) > 0)
            {
                keywordList.AddRange(keywords);
            }
            if((keywordType & KeywordType.Local) > 0)
            {
                keywordList.AddRange(localKeywords);
            }
            return keywordList.ToArray();
        }

        public static ShaderAnalyseToolsConfig GetConfig()
        {
            ShaderAnalyseToolsConfig config = null;
            using (FileStream fs = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    config = JsonUtility.FromJson<ShaderAnalyseToolsConfig>(content);
                }
            }
            return config;
        }

        [MenuItem("Tools/StatiticsTools/Shader/ShaderAnalyseTools", false, 303)]
        static void DoScan()
        {
            HashSet<string> inProjectShader = new HashSet<string>();
            HashSet<string> inABShader = new HashSet<string>();
            HashSet<string> inABMaterial = new HashSet<string>();

            ShaderAnalyseToolsConfig config = GetConfig();
            if (config==null)
            {
                EditorUtility.DisplayDialog("Error", string.Format("config file not exist:\n{0}", jsonFilePath), "Ok");
                return;
            }

            string directory = System.IO.Path.GetDirectoryName(outputResultPath);
            if (!Directory.Exists(directory))
            {
                //创建分析目录
                Directory.CreateDirectory(directory);
            }
            ShaderAnalyseResult result = AssetDatabase.LoadAssetAtPath<ShaderAnalyseResult>(outputResultPath);
            if(result==null)
            {
                result = new ShaderAnalyseResult();
                AssetDatabase.CreateAsset(result, outputResultPath);
            }
            result.Clear();

            EditorUtility.ClearProgressBar();
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            List<string> firstPass = new List<string>();
            int collectIndex = 0, bundleCount = assetBundleNames.Length;
            foreach (var assetBundleName in assetBundleNames)
            {
                Tools.StatisticsTools.ProgressBar("collecting inABShader", assetBundleName, collectIndex, bundleCount);
                collectIndex++;

                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                firstPass.AddRange(assetPaths);
            }
            Tools.StatisticsTools.ProgressBar("fetching dependencies", string.Empty, 0, 1);
            HashSet<string> allAssets = new HashSet<string>(firstPass);
            HashSet<string> secondPass = new HashSet<string>( AssetDatabase.GetDependencies(firstPass.ToArray()));
            allAssets.UnionWith(secondPass);
            int filterIndex = 0, assetsCount = allAssets.Count;
            foreach(var assetPath in allAssets)
            {
                Tools.StatisticsTools.ProgressBar("filtering inABShader", assetPath, filterIndex, assetsCount);
                filterIndex++;

                string ext = Path.GetExtension(assetPath);
                switch(ext)
                {
                    case shaderExt:
                        inABShader.Add(assetPath);
                        break;
                    case materialExt:
                        inABMaterial.Add(assetPath);
                        break;
                }
            }

            EditorUtility.ClearProgressBar();
            List<String> fileList = Directory.GetFiles(Application.dataPath, "*"+ shaderExt, SearchOption.AllDirectories).ToList();
            List<string> assetFileList = new List<string>();
            foreach (var systemPath in fileList)
            {
                string assetPath = StatisticsTools.ConvertSystemPathToAssetPath(systemPath);
                assetFileList.Add(assetPath);
            }
            string[] allPackagePath = StatisticsTools.GetAllPackagePath();
            if(allPackagePath==null)
            {
                GLog.Error("Package is not ready");
            }
            else
            {
                foreach(var packagePath in allPackagePath)
                {
                    string[] packageFiles = Directory.GetFiles(packagePath, "*" + shaderExt, SearchOption.AllDirectories);
                    foreach(var systemPath in packageFiles)
                    {
                        string assetPath = StatisticsTools.ConvertSystemPathToPackagePath(systemPath, packagePath);
                        assetFileList.Add(assetPath);
                    }
                }
            }
            foreach (var assetPath in assetFileList)
            {
                //string assetPath = StatisticsTools.ConvertSystemPathToAssetPath(systemPath);
                Tools.StatisticsTools.ProgressBar("filtering inProjectShader", assetPath, filterIndex, assetsCount);
                inProjectShader.Add(assetPath);
                if(assetPath.ToLower().Contains("resources"))
                {
                    inABShader.Add(assetPath);
                }
            }

            HashSet<string> exceptionSet = new HashSet<string>(inProjectShader);    //备份出来，方便对比
            exceptionSet.ExceptWith(inABShader);
            List<Shader> unusingShaderList = new List<Shader>();
            foreach(var exp in exceptionSet)
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(exp);
                unusingShaderList.Add(shader);
            }
            result.unusingShaders = unusingShaderList.ToArray();
            List<Shader> usingShaderList = new List<Shader>();
            var runtimeAssembly = typeof(ShaderUtil).Assembly;
            Type shaderUtilType = typeof(ShaderUtil);
            Dictionary<string, HashSet<Shader>> keywordToShaderSetDict = new Dictionary<string, HashSet<Shader>>();
            Dictionary<Shader, HashSet<string>> shaderToLocalKeywordsDict = new Dictionary<Shader, HashSet<string>>();
            foreach (var s in inABShader)
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(s);
                usingShaderList.Add(shader);

                object obj1 = shaderUtilType.InvokeMember("GetShaderGlobalKeywords",
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { shader });
                object obj2 = shaderUtilType.InvokeMember("GetShaderLocalKeywords",
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { shader });
                //var globalKeywords = ShaderUtil.GetShaderGlobalKeywords(shader);
                //var localKeywords = ShaderUtil.GetShaderLocalKeywords(shader);
                string[] keywords = obj1 as string[];
                foreach(var keyword in keywords)
                {
                    HashSet<Shader> shaderSet = null;
                    if(!keywordToShaderSetDict.TryGetValue(keyword, out shaderSet))
                    {
                        shaderSet = new HashSet<Shader>();
                        keywordToShaderSetDict.Add(keyword, shaderSet);
                    }
                    shaderSet.Add(shader);
                }
                string[] localKeywords = obj2 as string[];
                HashSet<string> localKeyword = new HashSet<string>(localKeywords);
                shaderToLocalKeywordsDict.Add(shader, localKeyword);
            }
            result.usingShaders = usingShaderList.ToArray();

            List<ShaderAnalyseResult.KeywordReference> krList = new List<ShaderAnalyseResult.KeywordReference>();
            HashSet<string> supportedGlobalKeywords = new HashSet<string>();
            foreach(var kv in keywordToShaderSetDict)
            {
                string keyword = kv.Key;
                Shader[] shaders = kv.Value.ToArray();
                ShaderAnalyseResult.KeywordReference kr = new ShaderAnalyseResult.KeywordReference();
                kr.keyword = keyword;
                kr.refShaders = shaders;
                krList.Add(kr);

                supportedGlobalKeywords.Add(keyword);
            }
            result.keywordsReference = krList.ToArray();

            Dictionary<string, HashSet<Material>> unsupported = new Dictionary<string, HashSet<Material>>();
            Dictionary<string, HashSet<Material>> warning = new Dictionary<string, HashSet<Material>>();
            foreach(var matPath in inABMaterial)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if(!mat)
                {
                    GLog.ErrorFormat("mat invalid: {0}", matPath);
                    continue;
                }
                string[] keywords = mat.shaderKeywords;
                foreach(var keyword in keywords)
                {
                    bool supported = false;
                    if(supportedGlobalKeywords.Contains(keyword))
                    {
                        supported = true;
                    }
                    else
                    {
                        HashSet<string> localKeywords = null;
                        if(shaderToLocalKeywordsDict.TryGetValue(mat.shader, out localKeywords))
                        {
                            if(localKeywords!=null && localKeywords.Contains(keyword))
                            {
                                supported = true;
                            }
                        }
                    }
                    if(!supported)
                    {
                        HashSet<Material> matSet = null;
                        if (keyword.StartsWith(config.keepSuffix))
                        {
                            if(!warning.TryGetValue(keyword, out matSet))
                            {
                                matSet = new HashSet<Material>();
                                warning.Add(keyword, matSet);
                            }
                        }
                        else
                        {
                            if (!unsupported.TryGetValue(keyword, out matSet))
                            {
                                matSet = new HashSet<Material>();
                                unsupported.Add(keyword, matSet);
                            }
                        }
                        matSet.Add(mat);
                    }
                }
            }
            List<ShaderAnalyseResult.UnsupportedKeywordReference> warningList = new List<ShaderAnalyseResult.UnsupportedKeywordReference>();
            foreach (var kv in warning)
            {
                string keyword = kv.Key;
                HashSet<Material> matSet = kv.Value;
                ShaderAnalyseResult.UnsupportedKeywordReference ukr = new ShaderAnalyseResult.UnsupportedKeywordReference();
                ukr.keyword = keyword;
                ukr.refMaterials = matSet.ToArray();
                warningList.Add(ukr);
            }
            result.warningReference = warningList.ToArray();
            List<ShaderAnalyseResult.UnsupportedKeywordReference> unsupportList = new List<ShaderAnalyseResult.UnsupportedKeywordReference>();
            foreach(var kv in unsupported)
            {
                string keyword = kv.Key;
                HashSet<Material> matSet = kv.Value;
                ShaderAnalyseResult.UnsupportedKeywordReference ukr = new ShaderAnalyseResult.UnsupportedKeywordReference();
                ukr.keyword = keyword;
                ukr.refMaterials = matSet.ToArray();
                unsupportList.Add(ukr);
            }
            result.unsupportedReference = unsupportList.ToArray();

            /*
            string tester = @"ZG_LIGHTCOLOR_TINT4_ON _USENORMALMAP_YES _RENDERING_TRANSPARENT _SMOOTHSTEP_ON LIGHTMAP_OFF SPOT VERTEXLIGHT_ON ZG__PICK_HEIGHT_OFF _REBUILD_NORMAL_OFF ZG_METALLIC_SRC_ALBEDO_A LIGHTPROBE_SH ZG_HALF_OFF ZG_LIGHTCOLOR_TINT_ON _ALPHADIS_ON _ALPHATEXUV2_ON _LIGHTMAP_SHADOW_MIX_OFF USE_CUTOUT UNITY_COLORSPACE_GAMMA TONEMAPPING_NEUTRAL USE_NOISE_DISTORTION FXAA_LOW ZG_MTL_3_EYEBALL USE_CUTOUT_THRESHOLD _SUNDISK_HIGH_QUALITY ZG_LIGHTCOLOR_TINT5_OFF DISTORT_OFF _EMISSION ZG_WORLDMAP_BLEND_OFF _METALLIC_SRC_NORMAL_A ZG_METALLIC_SRC_VALUE ZG_LIGHTCOLOR_TINT2_OFF ZG_MTL_MASK_OFF ZG_LIGHTCOLOR_TINT4_OFF _MASKE01EFFECTDIFFUSE_ON BILLBOARD_FACE_CAMERA_POS ZG_LIGHTMAP_SHADOW_MIX_OFF _FLIPBOOK_BLENDING NEED_MASK CHROMATIC_ABERRATION _BLENDMODE_MUL _DISTORTIONINFLUENCESOFT_ON _ROUGHNESS_SRC_NORMAL_B ZG_ANISO_OFF ZG_BLENDMODE_MUL _NEEDGAMMAFIX_ON _SPEED_ON APPLY_FORWARD_FOG UNITY_UI_CLIP_RECT SOURCE_GBUFFER _ALPHAR_ON DIRECTIONAL_COOKIE ZG_AMBIENTCOLOR_GLOBAL SHADOWS_SHADOWMASK ZG_TERRAIN_RISE_ON GRAIN _GLOW_ON COLOR_GRADING_HDR ZG_MTL_4_SILK _AMBIENTCOLOR_GLOBAL FXAA ZG_OBJTYPE_WORLD_CHAR ZG_MTL_3_SILK LIGHTMAP_ON ZG_LIGHTCOLOR_TINT_OFF _ALPHATEST_ON INSTANCING_ON COLOR_GRADING_LDR_2D TONEMAPPING_ACES ZG_BLENDMODE_BLEND ZG_ROUGHNESS_SRC_NORMAL_B BLOOM DIRLIGHTMAP_COMBINED ZG_HALF_ON _OBJTYPE_WORLD_OBJ UNITY_SINGLE_PASS_STEREO _DEPTHFADE_ON _FRESNEL_ON STEREO_INSTANCING_ON SHADOWS_SOFT ZG_LIGHTCOLOR_TINT2_ON ZG_PICK_HEIGHT_ON ZG_MTL_COUNT_M1 _SOFTDISSOLVESWITCH_ON ZG_INVERSE_NORMAL_Y_ON ZG_OBJTYPE_NORMAL ZG_SHAKEANIM_NO ZG_BLENDMODE_ADD _HEIGHTMAP USE_WORLD_SPACE_UV SPECULAR_REFLECTION_OFF ZG_ANISO_BLEND_OFF _ALPHAUSEGRAY_ON _MASK_ON _ALPHAUSE_NONE USE_FRESNEL_FADING _OPACITYMULTALPHA_ON SHADOWS_SINGLE_CASCADE USE_SCRIPT_FRAMEBLENDING ZG_MTL_2_EYEBALL CHROMATIC_ABERRATION_LOW ZG_4S_ON ZG_MTL_1_SILK ZG_SHOW_DEFAULT FXAA_KEEP_ALPHA VIGNETTE AUTO_EXPOSURE VERTICAL_FLIP ZG_BLEND_REFLECT_LAYER_ON ZGAME_LIGHTCOLOR_TINT_OFF _WORLDMAP_BLEND_ON ZG_RENDERMODE_TRANSPARENT _INVERSE_NORMAL_Y_ON ZG_ROUGHNESS_SRC_VALUE SHADOWS_CUBE FOG_EXP _RENDERMODE_CUTOUT ZG_SPECULAROCCULUSION_OFF USE_FRESNEL ZG_AMBIENTCOLOR_CUSTOM ZG_METALLIC_SRC_ALBEDO ZG_OBJTYPE_WORLD_TERRAIN ZG_MTL_COUNT_M4 _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A _ANISO_ON ZG_LIGHTMAP_SHADOW_MIX_ON FOG_LINEAR SHADOWS_SCREEN ZG_SHAKEANIM_YES PROCEDURAL_INSTANCING_ON ZG_LIGHTCOLOR_TINT3_ON ZG_ANISO_BLEND_ON ZG_OBJTYPE_WORLD_OBJ OFFSCREEN LIGHTMAP_SHADOW_MIXING ZG_LIGHTCOLOR_TINT5_ON ZG_MTL_3_SKIN_PREINTERGRATED ZG_METALLIC_SRC_NORMAL_A _SHOW_DEFAULT ZG_BLEND_REFLECT_LAYER_OFF _ROUGHNESSSOURCE_VALUE USE_FRAME_BLENDING ETC1_EXTERNAL_ALPHA ZG_PARALLAXMAP_OFF _BLEND_REFLECT_LAYER_ON _NORMALMAP SHADOWS_SPLIT_SPHERES ZG_MTL_4_SKIN_PREINTERGRATED ZG_ANISO_ON UNITY_UI_ALPHACLIP _ISWORLDMAPOBJ_YES USE_HEIGHT ZG_INV_NORMAL_Y_ON ZG_SPECULAROCCULUSION_ON _USEMAINTEXALPHA_ON STEREO_MULTIVIEW_ON DIRLIGHTMAP_OFF UNITY_HDR_ON NEED_GAMMAFIX _ZWRITE_ON _SPECGLOSSMAP ZGAME_PARALLAXMAP_ON FINALPASS ZG_WORLDMAP_BLEND_ON ZG_MTL_4_EYEBALL ZG_TERRAINLAYER_WATER_OFF DIRECTIONAL SOFTMASK_EDITOR ZG_MTL_COUNT_M3 POINT ZG_PARALLAXMAP_ON USE_CUTOUT_TEX _DISSOLVEOFF_ON _METALLICGLOSSMAP _DISTORTIONINFLUENCEGRADIENT_ON _ALPHABLEND_ON ZG_MTL_3_FABRIC _ZWRITEMODE_ON COLOR_GRADING_HDR_3D ZG_LAYER2_OFF _BLEND_REFLECT_LAYER_OFF ZG_MTL_MASK_ON STEREO_CUBEMAP_RENDER_ON _GLOSSYREFLECTIONS_OFF SHOW_COLOR_ALL USE_ALPHA_CLIPING STEREO_DOUBLEWIDE_TARGET ZG_INVERSE_NORMAL_Y_OFF POINT_COOKIE _VERTEXCOLORINFLUENCESOFTDISSOLVE_ON _UVSEC_UV1 _FADING_ON _ALPHAPREMULTIPLY_ON _CUSTOMDATAUV2XINFLUENCESOFTDISSOLVE_ON USE_QUAD_DECAL ZG_LAYER2_ON USE_REFRACTIVE EDITOR_VISUALIZATION _OBJTYPE_WORLD_CHAR TONEMAPPING_CUSTOM _LIGHTMAPPING_DYNAMIC_LIGHTMAPS _WORLDMAP_BLEND_OFF ZG_ROUGHNESS_SRC_ALBEDO_A SHADOWS_DEPTH ZG_DETAILMAP_OFF FOG_EXP2 ZG__PICK_HEIGHT_ON _BLENDING_ALPHABLEND SOFTMASK_IGNORE ZG_TERRAINLAYER_WATER_ON _EMISSIVE_ON LOD_FADE_CROSSFADE _SHAKEANIM_NO SPECULAR_REFLECTION_ON ZG_MTL_1_EYEBALL _CLIPDYNAMIC_ON ZG_MTL_1_FABRIC COLOR_GRADING_HDR_2D ZG_MTL_COUNT_M2 SOFTPARTICLES_ON _ROUGHNESSSOURCE_TEXTURE DISTORT _MAPPING_LATITUDE_LONGITUDE_LAYOUT _USECRICLEMASK_ON TRANSPARENCY_ON _ANISO_BLEND_OFF _BRIGHTNESSDYNAMIC_ON ZG_LIGHTCOLOR_TINT3_OFF DIRLIGHTMAP_SEPARATE ZG_MTL_1_SKIN_PREINTERGRATED _USENORMALMAP_NO _MASK_UV2_ON _CUSTOMON_ON BLOOM_LOW ZG_MTL_2_FABRIC _NEEDUVANI_ON USE_ALPHA_POW ZG_INV_NORMAL_Y_OFF DYNAMICLIGHTMAP_ON NEED_UVANI _DISTORTIONINFLUENCEOFFSET_ON ZG_MTL_1_ANISO _BLENDMODE_BLEND _OBJTYPE_NORMAL ZG_MTL_4_FABRIC ZG_RENDERMODE_CUTOUT ZG_RENDERMODE_OPAQUE _DETAILMAP_OFF ZG_DETAILMAP_ON ZG_4S_OFF ZG_MTL_2_SKIN_PREINTERGRATED ZG_MTL_2_SILK STEREO_INSTANCING_ENABLED _MATERIAL_2_FABRIC _LAYER2_OFF _ZWRITE_OFF _DISTORTION2UV_ON";
            string[] splitedTester = tester.Split(' ');
            foreach(var t in splitedTester)
            {
                if(supportedGlobalKeywords.Contains(t))
                {
                    GLog.LogFormat("supported global: {0}", t);
                }
                else if(unsupported.ContainsKey(t))
                {
                    GLog.LogFormat("unsuppoted global: {0}", t);
                }
                else
                {
                    bool foundInLocal = false;
                    foreach(var localKv in shaderToLocalKeywordsDict)
                    {
                        var localKey = localKv.Key;
                        var localSet = localKv.Value;
                        foreach(var localKeyword in localSet)
                        {
                            if(t==localKeyword)
                            {
                                GLog.ErrorFormat("supported local: {0} in {1}", t, localKey.name);
                                foundInLocal = true;
                                break;
                            }
                        }
                    }
                    if(!foundInLocal)
                    {
                        GLog.ErrorFormat("found nowhere: {0}", t);
                    }
                }
            }
            */

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(result);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Finish", "ShaderAnalyseTools process finished", "Ok");
        }


        static public void DoFix(ShaderAnalyseResult result)
        {
            GLog.Log("begin DoFix");
            foreach(var shader in result.unusingShaders)
            {
                if(shader)
                {
                    string path = AssetDatabase.GetAssetPath(shader);
                    if(string.IsNullOrEmpty(path))
                    {
                        GLog.LogFormat("shader path not exist: {0}", shader.name);
                    }
                    else
                    {
                        GLog.LogFormat("delete shader: {0} at path: {1}", shader.name, path);

                        //不再删除，因为有shader是editor使用的，避免误删除
                        //AssetDatabase.DeleteAsset(path);
                        //AssetDatabase.MoveAssetToTrash(path);
                    }
                }
                else
                {
                    GLog.Log("shader disappear");
                }
            }
            AssetDatabase.Refresh();

            HashSet<string> unsupportedKeywords = new HashSet<string>();
            HashSet<Material> fixingMaterialSet = new HashSet<Material>();
            foreach(var warning in result.warningReference)
            {
                unsupportedKeywords.Add(warning.keyword);
                foreach(var mat in warning.refMaterials)
                {
                    fixingMaterialSet.Add(mat);
                }
            }
            foreach(var unsupported in result.unsupportedReference)
            {
                unsupportedKeywords.Add(unsupported.keyword);
                foreach(var mat in unsupported.refMaterials)
                {
                    fixingMaterialSet.Add(mat);
                }
            }

            foreach(var mat in fixingMaterialSet)
            {
                var keywords = mat.shaderKeywords;
                List<string> filteredKeywordList = new List<string>();
                string keywordstring = string.Join(" ", keywords);
                GLog.LogFormat("begin material {0}, keywords: {1}", mat.name, keywordstring);
                foreach (var keyword in keywords)
                {
                    if (!unsupportedKeywords.Contains(keyword))
                    {
                        filteredKeywordList.Add(keyword);
                    }
                }
                mat.shaderKeywords = filteredKeywordList.ToArray();
                keywords = mat.shaderKeywords;
                keywordstring = string.Join(" ", keywords);
                GLog.LogFormat("end material {0}, keywords: {1}", mat.name, keywordstring);
                EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GLog.Log("end DoFix");
        }

        public static void DoFixGlobal()
        {
            ShaderAnalyseToolsConfig config = GetConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Error", string.Format("config file not exist:\n{0}", jsonFilePath), "Ok");
                return;
            }
            var matSet = ShaderVariantDifferenceTools.GetBuildingPath();
            foreach(var matPath in matSet)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                StatisticsTools.CheckWastedMaterailTexture(config, matPath, mat, false, null);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DoTest()
        {
            string pattern = @"ZG_(.+)_OFF";
            //string pattern = @"ZG_SHOW_(.+)";
            Regex regex = new Regex(pattern);
            string testerLong = "DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_LINEAR LIGHTMAP_ON ZG_AMBIENTCOLOR_GLOBAL ZG_METALLIC_SRC_NORMAL_A ZG_OBJTYPE_NORMAL ZG_RENDERMODE_OPAQUE ZG_ROUGHNESS_SRC_NORMAL_B ZG_SHOW_DEFAULT DIRECTIONAL ZG_HAhH_OFF";
            string[] tester = testerLong.Split(' ');
            foreach(var t in tester)
            {
                bool match = regex.IsMatch(t);
                Debug.LogFormat("result: {0} {1}", t, match.ToString());
            }
        }
    }

    [CustomEditor(typeof(ShaderAnalyseResult))]
    public class ShaderAnalyseResultEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if(GUILayout.Button("Fix"))
            {
                ShaderAnalyseTools.DoFix(target as ShaderAnalyseResult);
            }
            if(GUILayout.Button("FixGlobal"))
            {
                ShaderAnalyseTools.DoFixGlobal();
            }
            //if(GUILayout.Button("DoTest"))
            //{
            //    ShaderAnalyseTools.DoTest();
            //}
            base.OnInspectorGUI();
        }
    }



    class ShaderVariantDifference : ScriptableObject
    {
        [Serializable]
        public class VariantKeywordsComposite
        {
            public string hash;
            public string[] sortedKeywords;
            public List<Material> materials;
            public void Clear(string h, string[] k)
            {
                hash = h;
                sortedKeywords = k;
                materials = new List<Material>();
            }
        }

        [Serializable]
        public class ShaderType
        {
            public string shaderName;
            public VariantKeywordsComposite[] composites;
            private Dictionary<string, VariantKeywordsComposite> compositeDict;

            public void Clear()
            {
                composites = null;
                compositeDict = new Dictionary<string, VariantKeywordsComposite>();
            }

            public void RecieveMaterial(string hash, string[] keywords, Material material)
            {
                VariantKeywordsComposite vkc = null;
                if(!compositeDict.TryGetValue(hash, out vkc))
                {
                    vkc = new VariantKeywordsComposite();
                    compositeDict.Add(hash, vkc);
                    vkc.Clear(hash, keywords);
                }
                vkc.materials.Add(material);
            }

            public void Serialize()
            {
                composites = compositeDict.Values.ToArray();
            }
        }

        public ShaderType[] shaderVariants;
        private Dictionary<string, ShaderType> shaderVariantsDict;

        public void Clear()
        {
            shaderVariants = null;
            shaderVariantsDict = new Dictionary<string, ShaderType>();
        }

        public void RecieveMaterial(string hash, string[] keywords, Material material)
        {
            ShaderType st = null;
            if(!shaderVariantsDict.TryGetValue(material.shader.name, out st))
            {
                st = new ShaderType();
                shaderVariantsDict.Add(material.shader.name, st);
                st.shaderName = material.shader.name;
                st.Clear();
            }
            st.RecieveMaterial(hash, keywords, material);
        }

        public void Serialize()
        {
            foreach(var st in shaderVariantsDict.Values)
            {
                st.Serialize();
            }
            shaderVariants = shaderVariantsDict.Values.ToArray();
        }
    }

    class ShaderVariantDifferenceTools
    {
        const string outputResultPath = "Assets/Analyse/ShaderVariant.asset";

        public static HashSet<string> GetBuildingPath()
        {
            string[] filterExt = new string[] { ".mat" };
            List<string> allUITextureAssets = AutoAssignDependentABTools.GetAllBuildingAssets(filterExt, false, ABAssetPass.All);
            return new HashSet<string>(allUITextureAssets);
        }

        static string GetKeywordsHash(string[] keywords)
        {
            List<string> l = new List<string>(keywords);
            l.Sort();
            string longKeywords = string.Join(" ", l);
            return longKeywords;
        }

        [MenuItem("Tools/StatiticsTools/Shader/ShaderVariantDifferenceTool", false, 304)]
        static void Do()
        {

            string directory = System.IO.Path.GetDirectoryName(outputResultPath);
            if (!Directory.Exists(directory))
            {
                //创建分析目录
                Directory.CreateDirectory(directory);
            }
            ShaderVariantDifference result = AssetDatabase.LoadAssetAtPath<ShaderVariantDifference>(outputResultPath);
            if (result == null)
            {
                result = new ShaderVariantDifference();
                AssetDatabase.CreateAsset(result, outputResultPath);
            }
            result.Clear();
            Tools.StatisticsTools.ProgressBar("ShaderVariantDifferenceTool", "initiating", 0, 1);

            HashSet<string> matPaths = GetBuildingPath();
            int objectIndex = 0;
            foreach (var matPath in matPaths)
            {
                Tools.StatisticsTools.ProgressBar("analysing material", matPath, objectIndex, matPaths.Count);
                ++objectIndex;

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                string[] keywords = mat.shaderKeywords;
                string hash = GetKeywordsHash(keywords);
                result.RecieveMaterial(hash, keywords, mat);
            }
            result.Serialize();
            EditorUtility.SetDirty(result);

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "ShaderVariantDifferenceTool process finished", "Ok");
        }
    }

}
