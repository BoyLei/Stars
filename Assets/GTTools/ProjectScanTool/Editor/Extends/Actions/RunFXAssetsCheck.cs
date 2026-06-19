/*
 * @Description: 粒子系统资源检查
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace CasualEngine.ProjectScanTool
{
    public class RunFXAssetsCheck : MethodHelper
    {
        private static int GetParticleEffectRuntimeMemorySize(GameObject go, out int textureCount)
        {
            var textures = new List<Texture>();
            textureCount = 0;
            int sumSize = 0;
            //粒子的贴图数据，注意这里要用ParticleSystemRenderer，用Renderer只能获取到一些最基本的数据
            var meshPsRendererlist = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystemRenderer item in meshPsRendererlist)
            {
                if (item.sharedMaterial)
                {
                    var shader = item.sharedMaterial.shader;
                    int propertyCount = ShaderUtil.GetPropertyCount(shader);
                    for (int i = 0; i < propertyCount; ++i)
                    {
                        var property = ShaderUtil.GetPropertyType(shader, i);
                        if (property == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            Texture tex = item.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                            if (!textures.Contains(tex))
                            {
                                if (tex != null)
                                {
                                    textureCount++;
                                    textures.Add(tex);
                                    sumSize = sumSize + TextureUtils.GetTextureStorageMemorySize(tex);
                                }
                            }
                        }
                    }
                }
            }

            //mesh的贴图数据
            var meshRendererlist = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer item in meshRendererlist)
            {
                if (item.sharedMaterial)
                {
                    var shader = item.sharedMaterial.shader;
                    int propertyCount = ShaderUtil.GetPropertyCount(shader);
                    for (int i = 0; i < propertyCount; ++i)
                    {
                        var property = ShaderUtil.GetPropertyType(shader, i);
                        if (property == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            Texture tex = item.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                            if (!textures.Contains(tex))
                            {
                                if (tex != null)
                                {
                                    textureCount++;
                                    textures.Add(tex);
                                    sumSize = sumSize + TextureUtils.GetTextureStorageMemorySize(tex);
                                }
                            }
                        }
                    }

                }
            }
            return sumSize;
        }

        #region 特效总贴图内存检查
        public static bool Do_ParticleEffectMemoryCheck(ParticleEffectMemoryCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;

                    var memorySize = GetParticleEffectRuntimeMemorySize(prefab, out int textureCount);
                    var fMemorySize = Math.Round(1.0f * memorySize / 1024 / 1024, 2);
                    if (fMemorySize > checkDetail.memoryLimit)
                    {
                        customRule.Record(prefab, methodName, $"特效总贴图内存过高({fMemorySize}M)", path);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 特效总贴图数量检查
        public static bool Do_ParticleEffectTextureCntCheck(ParticleEffectTextureCntCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;

                    var memorySize = GetParticleEffectRuntimeMemorySize(prefab, out int textureCount);
                    if (textureCount > checkDetail.textureCntLimit)
                    {
                        customRule.Record(prefab, methodName, $"特效总贴图数量过高({textureCount})", path);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 粒子系统组件总数检查
        public static bool Do_ParticleEffectPSComponentsCheck(ParticleEffectPSComponentsCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;

                    var particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
                    if (particleSystems.Length > checkDetail.psComponentCntLimit)
                    {
                        customRule.Record(prefab, methodName, $"特效粒子系统组件数量过多({particleSystems.Length})", path);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 特效播放时最大粒子数量检查
        public static bool Do_ParticleEffectParticlesCntCheck(ParticleEffectParticlesCntCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            //逻辑暂未完成
            // ParticleEffectAnalysis<ParticleEffectParticlesCntCheck, ParticleEffectParticlesCntCheckDetail>.Begin(customRule, EnumParticleEffectAnalysisType.eParticlesCnt, methodName);
            return true;
        }
        #endregion

        #region 检查粒子系统是否开启预热
        public static bool Do_ArtParticlePreWarmCheck(ParticlePreWarmCheck customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;
                    ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var fx in systems)
                    {
                        if (fx.main.prewarm != checkDetail.preWarm)
                        {
                            if (customRule.autoCorrection)
                            {
                                var main = fx.main;
                                main.prewarm = checkDetail.preWarm;
                                EditorUtility.SetDirty(prefab);
                            }
                            var nodePath = ProjectScanHelper.GetNodePath(fx.transform);
                            customRule.Record(prefab, methodName, $"粒子系统Prewarm选项设置错误，期望值是{checkDetail.preWarm}(节点{nodePath})", path, true);
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region 检查开启预热的粒子系统的生命周期时间是否过长
        public static bool Do_ArtParticleLifeTimeCheck(ParticleLifeTimeCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;
                    ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var fx in systems)
                    {
                        float time = 0;

                        if (!fx.main.prewarm)
                        {
                            continue;
                        }
                        if (fx.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                        {
                            time = Mathf.Max(fx.main.startLifetime.constant, time);
                        }
                        else if (fx.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                        {
                            time = Mathf.Max(fx.main.startLifetime.constantMin, time);
                            time = Mathf.Max(fx.main.startLifetime.constantMax, time);
                        }
                        if (time > checkDetail.lifeTimeLimit)
                        {
                            var nodePath = ProjectScanHelper.GetNodePath(fx.transform);
                            customRule.Record(prefab, methodName, $"粒子系统开启了预热且Start Lifetime设置值过大(节点{nodePath})", path);
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region 检查特效图片是否误引用图集
        public static bool Do_FXRefAtlasCheck(FXRefAtlasCheck customRule)
        {
            // Func<string, FXRefAtlasCheckDetail, bool> InAtlasDir = (texpath, detail) =>
            // {
            //     foreach (var atlasPath in detail.spriteAtlasDirs)
            //     {
            //         if (texpath.IndexOf(atlasPath) >= 0)
            //         {
            //             return true;
            //         }
            //     }
            //     return false;
            // };

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;

                    var images = prefab.GetComponentsInChildren<Image>(true);
                    foreach (var image in images)
                    {
                        if (image.mainTexture == null)
                        {
                            continue;
                        }
                        string texpath = AssetDatabase.GetAssetPath(image.mainTexture);
                        TextureImporter texImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
                        if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                        {
                            customRule.Record(prefab, methodName, "特效误引用了图集  " + texpath, path);
                        }
                        // if (!string.IsNullOrEmpty(texpath) && (InAtlasDir(texpath, checkDetail) || Path.GetExtension("texpath") == ".spriteatlas"))
                        // {
                        //     customRule.Record(prefab, methodName, path + "  误引用了图集  " + texpath, path);
                        // }
                    }
                }
            }
            return true;
        }
        #endregion

        #region spine参数检查
        public static bool Do_SpineAssetsCheck(SpineAssetsCheck customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            //先检查贴图
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
                    if (!texture)
                    {
                        continue;
                    }

                    TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!texImporter)
                        continue;

                    //check textureType
                    if (checkDetail.texCheck.mustbeDefault.enable && checkDetail.texCheck.mustbeDefault.value == false && texImporter.textureType != TextureImporterType.Default)
                    {
                        continue;
                    }

                    if (checkDetail.texCheck.mustbeDefault.enable && checkDetail.texCheck.mustbeDefault.value && texImporter.textureType != TextureImporterType.Default)
                    {
                        Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.textureType), TextureImporterType.Default, customRule);
                    }

                    RunTextureAssetsCheck.CheckDefaultTextureFormatParam(customRule, texture, methodName, path, texImporter, checkDetail.texCheck);
                    RunTextureAssetsCheck.CheckTextureCompressionFormat(customRule, texture, methodName, path, texImporter, checkDetail.texCheck.textureFormat);

                    //一定要记得将texImporter重新save
                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(texImporter);
                        texImporter.SaveAndReimport();
                    }
                }
            }

            //再检查材质
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.material, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                    if (mat == null)
                        continue;

                    if (checkDetail.matCheck.shader.enable && mat.shader != Shader.Find(checkDetail.matCheck.shader.value))
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            mat.shader = Shader.Find(checkDetail.matCheck.shader.value);
                        }
                        customRule.Record(mat, methodName, $"material的Shader设置错误，应该为{checkDetail.matCheck.shader}", path, true);
                    }

                    if (checkDetail.matCheck.straightAlphaInput.enable
                        && mat.HasProperty("_StraightAlphaInput")
                        && mat.GetInt("_StraightAlphaInput") != (checkDetail.matCheck.straightAlphaInput.value ? 1 : 0))
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            mat.SetInt("_StraightAlphaInput", checkDetail.matCheck.straightAlphaInput.value ? 1 : 0);
                        }
                        customRule.Record(mat, methodName, $"material的StraightAlphaTexture设置错误，应该为{checkDetail.matCheck.straightAlphaInput.value}", path, true);
                    }

                    if (checkDetail.matCheck.srcBlendMode.enable
                        && mat.HasProperty("_SrcBlend")
                        && mat.GetInt("_SrcBlend") != checkDetail.matCheck.srcBlendMode.value)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            mat.SetInt("_SrcBlend", checkDetail.matCheck.srcBlendMode.value);
                        }
                        customRule.Record(mat, methodName, $"material的SrcBlendMode属性设置错误，应该为{checkDetail.matCheck.srcBlendMode.value.ToString()}", path, true);
                    }

                    if (checkDetail.matCheck.dstBlendMode.enable
                        && mat.HasProperty("_DstBlend")
                        && mat.GetInt("_DstBlend") != checkDetail.matCheck.dstBlendMode.value)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            mat.SetInt("_DstBlend", checkDetail.matCheck.dstBlendMode.value);
                        }
                        customRule.Record(mat, methodName, $"material的DstBlendMode属性设置错误，应该为{checkDetail.matCheck.dstBlendMode.value.ToString()}", path, true);
                    }

                    //一定要记得将texImporter重新save
                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(mat);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 特效中引用的图片资源位置检查
        public static bool Do_FXRefImageLocalPathCheck(FXRefImageLocalPathCheck customRule)
        {
            Func<string, FXRefImageLocalPathCheckDetail, bool> InAtlasDir = (texpath, detail) =>
            {
                foreach (var atlasPath in detail.refImageDirs)
                {
                    if (texpath.IndexOf(atlasPath + "/") >= 0)
                    {
                        return true;
                    }
                }
                return false;
            };

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                if (checkDetail.refImageDirs.Count == 0)
                {
                    continue;
                }
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab == null)
                        continue;

                    var images = prefab.GetComponentsInChildren<Image>(true);
                    foreach (var image in images)
                    {
                        if (image.mainTexture != null)
                        {
                            string texpath = AssetDatabase.GetAssetPath(image.mainTexture);
                            if (!string.IsNullOrEmpty(texpath) && !InAtlasDir(texpath, checkDetail))
                            {
                                customRule.Record(prefab, methodName, "特效引用的贴图路径不是指定的目录，节点[" + ProjectScanHelper.GetNodePath(image.gameObject.transform) + "]", path);
                            }
                        }
                        if (image.material != null)
                        {
                            Material mat = image.material;
                            Object[] roots = new Object[] { mat };
                            Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                            foreach (Object dependObj in dependObjs)
                            {
                                if (dependObj.GetType() == typeof(Texture2D))
                                {
                                    string texpath = AssetDatabase.GetAssetPath(dependObj.GetInstanceID());
                                    if (!string.IsNullOrEmpty(texpath) && !InAtlasDir(texpath, checkDetail))
                                    {
                                        customRule.Record(prefab, methodName, "特效引用的贴图路径不是指定的目录，节点[" + ProjectScanHelper.GetNodePath(image.gameObject.transform) + "]", path);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region Animator检查
        internal class AnimatorTransitionRuleInfo<T> where T : CustomRule
        {
            public AnimatorController animator;
            public T customRule;
            public string methodName;
            public List<string> parames;
            public AnimatorControllerLayer layer;
            public string animatorPath;
        }

        public static bool Do_AnimatorAssetCheck(AnimatorAssetCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                //目前只有一个检查参数，这里直接判断是否检测，如果多个，则把把判断放到各个逻辑里面
                if (!checkDetail.transitionConditionCheck)
                {
                    continue;
                }
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.animatorController);
                foreach (var path in paths)
                {
                    AnimatorController animator = AssetDatabase.LoadAssetAtPath(path, typeof(AnimatorController)) as AnimatorController;
                    if (animator == null)
                    {
                        continue;
                    }

                    List<string> parames = new List<string>();
                    foreach (var p in animator.parameters)
                    {
                        if (!string.IsNullOrEmpty(p.name))
                        {
                            parames.Add(p.name);
                        }
                    }

                    //获取animator的每一个layers
                    AnimatorControllerLayer[] layers = animator.layers;
                    if (layers == null || layers.Length == 0)
                    {
                        continue;
                    }

                    foreach (var layer in layers)
                    {

                        AnimatorTransitionRuleInfo<AnimatorAssetCheck> ruleInfo = new RunFXAssetsCheck.AnimatorTransitionRuleInfo<AnimatorAssetCheck>()
                        {
                            animator = animator,
                            methodName = methodName,
                            customRule = customRule,
                            parames = parames,
                            layer = layer,
                            animatorPath = path
                        };

                        //获取层状态机
                        AnimatorStateMachine sm = layer.stateMachine;
                        //获得当前anystate的transitions
                        CheckAnimatorTransition(ruleInfo, "AnyState", sm.anyStateTransitions);
                        CheckAnimatorTransition(ruleInfo, "EntryState", sm.entryTransitions);
                        ChildAnimatorState[] ams = sm.states;   //获取该层状态机的子状态机

                        foreach (var childAms in ams)
                        {
                            //获得子状态机上面的组件
                            // var behaviours = childAms.state.behaviours;
                            //获取子状态机上面的transition
                            CheckAnimatorTransition(ruleInfo, childAms.state.name, childAms.state.transitions);
                        }
                    }
                }
            }
            return true;
        }

        internal static void CheckAnimatorTransition(AnimatorTransitionRuleInfo<AnimatorAssetCheck> ruleInfo, string stateName, AnimatorStateTransition[] transitions)
        {
            if (transitions == null || transitions.Length == 0)
            {
                return;
            }
            foreach (var item in transitions)
            {
                CheckAnimatorCondition(ruleInfo, stateName, item.destinationState.name, item.conditions);
            }
        }

        internal static void CheckAnimatorTransition(AnimatorTransitionRuleInfo<AnimatorAssetCheck> ruleInfo, string stateName, AnimatorTransition[] transitions)
        {
            if (transitions == null || transitions.Length == 0)
            {
                return;
            }
            foreach (var item in transitions)
            {
                CheckAnimatorCondition(ruleInfo, stateName, item.destinationState.name, item.conditions);
            }
        }

        internal static void CheckAnimatorCondition(AnimatorTransitionRuleInfo<AnimatorAssetCheck> ruleInfo, string stateName, string transitionName, AnimatorCondition[] conditions)
        {
            foreach (var item in conditions)
            {
                if (string.IsNullOrEmpty(item.parameter) || !ruleInfo.parames.Contains(item.parameter))
                {
                    ruleInfo.customRule.Record(ruleInfo.animator, ruleInfo.methodName, "Animator的transition中存在非法条件[" + ruleInfo.layer.name + "|" + stateName + "->" + transitionName + "]", ruleInfo.animatorPath);
                }
            }
        }
        #endregion

        #region Animation资源Legacy设置检查
        public static bool Do_AnimationLegacyCheck(AnimationLegacyCheck customRule)
        {
            string[] allPrefabGuids = ProjectScanHelper.GetAssetGuidsByType("Assets", AssetType.prefab);
            //被prefab引用到的animation路径
            List<string> refAnimationPath = new List<string>();
            foreach (var guid in allPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (prefab == null)
                    continue;
                Animation[] anis = prefab.GetComponentsInChildren<Animation>(true);
                foreach (var ani in anis)
                {
                    if (ani.clip != null)
                    {
                        string clipPath = AssetDatabase.GetAssetPath(ani.clip);
                        if (!string.IsNullOrEmpty(clipPath) && !refAnimationPath.Contains(clipPath))
                        {
                            refAnimationPath.Add(clipPath);
                        }
                    }
                }
            }

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.animation);
                foreach (var path in paths)
                {
                    if (!refAnimationPath.Contains(path))
                    {
                        continue;
                    }
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
                    if (clip == null)
                    {
                        continue;
                    }

                    if (checkDetail.legacy.enable && checkDetail.legacy.value != clip.legacy)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            clip.legacy = checkDetail.legacy.value;
                        }
                        customRule.Record(clip, methodName, $"被Prefab中组件Animation挂载的Animation资源Legacy设置不对，应该为{checkDetail.legacy.value}", path);
                    }

                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(clip);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 同一目录下Animator与Animation同名的检查
        public static bool Do_AnimationDuplicateNameCheck(AnimationDuplicateNameCheck customRule)
        {
            Dictionary<string, List<string>> refAnimationDic = GetAllAnimationBeRefDic();

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                List<string> animationPaths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.animation).ToList();
                foreach (var path in animationPaths)
                {

                    AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
                    string controllerPath = path.Replace(Path.GetExtension(path), PathExtensionDefine.controller);
                    if (checkDetail.mulRefCheck)
                    {
                        if (!refAnimationDic.ContainsKey(path))
                        {
                            continue;
                        }
                        refAnimationDic.TryGetValue(path, out List<string> refList);
                        if (refList == null || refList.Count < 2)
                        {
                            continue;
                        }
                        //且被多个引用了
                        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(controllerPath)))
                        {
                            customRule.Record(clip, methodName, $"Animation被多个Animator引用，且在同一个目录下，存在Controller与其同名", path);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(controllerPath)))
                        {
                            customRule.Record(clip, methodName, $"同一个目录下，存在Controller与Animation同名", path);
                        }
                    }
                }
            }

            return true;
        }

        //获得所有animation被引用关系数据
        public static Dictionary<string, List<string>> GetAllAnimationBeRefDic()
        {
            Dictionary<string, List<string>> refAnimationDic = new Dictionary<string, List<string>>();
            string[] allAnimatorGuids = ProjectScanHelper.GetAssetGuidsByType("Assets", AssetType.animatorController);
            //被animator引用到的animation路径
            foreach (var guid in allAnimatorGuids)
            {
                string animatorPath = AssetDatabase.GUIDToAssetPath(guid);
                AnimatorController animator = AssetDatabase.LoadAssetAtPath(animatorPath, typeof(AnimatorController)) as AnimatorController;
                if (animator == null)
                {
                    continue;
                }

                //获取animator的每一个layers
                AnimatorControllerLayer[] layers = animator.layers;
                if (layers == null || layers.Length == 0)
                {
                    continue;
                }

                foreach (var layer in layers)
                {
                    AnimatorStateMachine sm = layer.stateMachine;    //获取层状态机
                    ChildAnimatorState[] ams = sm.states;   //获取该层状态机的子状态机

                    foreach (var childAms in ams)
                    {
                        if (childAms.state.motion != null && !childAms.state.motion.legacy)
                        {
                            string aniPath = AssetDatabase.GetAssetPath(childAms.state.motion);
                            if (!string.IsNullOrEmpty(aniPath) && Path.GetExtension(aniPath) == PathExtensionDefine.anim)
                            {
                                if (refAnimationDic.TryGetValue(aniPath, out List<string> refList))
                                {
                                    if (!refList.Contains(animatorPath))
                                    {
                                        refList.Add(animatorPath);
                                    }
                                }
                                else
                                {
                                    if (refList == null)
                                    {
                                        refAnimationDic.Add(aniPath, new List<string>() { animatorPath });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return refAnimationDic;
        }
        #endregion

        #region Animation被多个Animator引用的检查
        public static bool Do_AnimationBeRefMulTimesCheck(AnimationBeRefMulTimesCheck customRule)
        {
            Dictionary<string, List<string>> refAnimationDic = GetAllAnimationBeRefDic();

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                List<string> animationPaths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.animation).ToList();
                foreach (var path in animationPaths)
                {

                    AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
                    if (!refAnimationDic.ContainsKey(path))
                    {
                        continue;
                    }
                    refAnimationDic.TryGetValue(path, out List<string> refList);
                    if (refList == null || refList.Count < 2)
                    {
                        continue;
                    }
                    //被多个引用了
                    customRule.Record(clip, methodName, $"Animation被多个Animator引用", path);
                }
            }

            return true;
        }
        #endregion

    }
}