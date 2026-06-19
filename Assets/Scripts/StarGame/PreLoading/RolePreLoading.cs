using ProtoMsg;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProject.Service.Resource;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class RolePreLoading
{
    public bool IsLoad;
    public List<(string AssetType, string AssetPath)> PreLoadings = new();

    public void Init()
    {
        Reset();
    }

    public void Reset()
    {
        PreLoadings.Clear();
        IsLoad = false;
    }

    public bool CanPreLoad()
    {
        if (GameManager.Instance.HeroMD == null)
        {
            return false;
        }

        if (!IsLoad)
        {
            Prepare();
            return true;
        }

        return false;
    }

    public void Loading(System.Action<string, float> process, System.Action complete)
    {
        int totalCount = PreLoadings.Count;
        int finishCount = 0;
        if (totalCount > 0)
        {

            // bool isLoadedTemplete = false;
            // Action totleComplete = () =>
            // {
            //     if (isLoadedTemplete && finishCount >= totalCount)
            //     {
            //         complete.Invoke();
            //     }
            // };

            // PreloadHeroTemplate(totleComplete);

            // loading preloading asset
            LoaidngPreLoadingAssets((string assetPath) =>
            {
                finishCount++;
                RefreshLoaidng(finishCount, totalCount, assetPath, process, complete);
            });



            // 加载 skill cfg

            // foreach (var item in PreLoadings)
            // {
            //     StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Object>(
            //         item.AssetPath,
            //         onComplete =>
            //         {

            //             float p = finishCount * 1.0f / totalCount;
            //             process?.Invoke(item.AssetPath, p);
            //             if (finishCount == totalCount)
            //             {
            //                 complete?.Invoke();
            //                 IsLoad = true;
            //             }


            //         });
            // }
        }
        else
        {
            complete?.Invoke();
            IsLoad = true;
        }
    }

    private void LoaidngPreLoadingAssets(Action<string> itemLoadCb)
    {
        foreach (var item in PreLoadings)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Object>(
                item.AssetPath,
                onComplete =>
                {
                    itemLoadCb?.Invoke(item.AssetPath);
                });
        }

    }

    private void RefreshLoaidng(int finishCount, int totalCount, string assetPath, System.Action<string, float> process, System.Action complete)
    {
        float p = finishCount * 1.0f / totalCount;
        // SGF.Debuger.Log($"{finishCount}_{totalCount}");
        process?.Invoke(assetPath, p);
        if (finishCount == totalCount)
        {
            complete?.Invoke();
            IsLoad = true;
        }

    }

    private void Prepare()
    {
        PreLoadings.Clear();
        //主角预处理
        HeroPrepare();

        //伙伴预处理
        PartnerPrepare();

        PreloadHeroTemplate();

    }

    private void PartnerPrepare()
    {
        var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
        if (partnerMgr != null)
        {
            var list = partnerMgr.GetInPlayedPartners();
            if (list != null && list.Count > 0)
            {
                foreach (var partner in list)
                {
                    PartnerPrepare(partner);
                }
                return;
            }
        }

        if (StarScenesManager.Instance.CurPartnerID > 0)
        {
            PartnerPrepare(StarScenesManager.Instance.CurPartnerID);
        }
    }

    private void PartnerPrepare(PartnerMD partnerMD)
    {
        PartnerPrepare(partnerMD.Index);
    }

    private void PartnerPrepare(long partnerID)
    {
        var config = LocalDataManager.Instance.GetPartnerDataCell(partnerID);
        if (config != null)
        {
            var avatarID = config.GetAvatarID();

            var avatarcfg = LocalDataManager.Instance.GetAvatarDataCell(avatarID);
            if (avatarcfg != null)
            {
                //模型处理
                HeroModel(avatarcfg.GetModelId(), avatarcfg.GetHighModelId());
                // 基础动作
                if (!string.IsNullOrEmpty(config.ModelAnima))
                {
                    string path = GetRealPath($"{avatarcfg.AnimsPath}/{config.ModelAnima}", E_AssetType.Animation);
                    if (!string.IsNullOrEmpty(path))
                    {
                        PreLoadings.Add(("UnityEngine.AnimationClip", path));
                    }
                }

                if (!string.IsNullOrEmpty(config.AppearAnima))
                {
                    string path = GetRealPath($"{avatarcfg.AnimsPath}/{config.AppearAnima}", E_AssetType.Animation);
                    if (!string.IsNullOrEmpty(path))
                    {
                        PreLoadings.Add(("UnityEngine.AnimationClip", path));
                    }
                }

                //基础动作处理
                HeroBaseAniamation(avatarcfg.GetBaseAnims(), avatarcfg.AnimsPath);
                //图像处理
                HeroHeadIcon(avatarcfg.GetHeadID());
                //技能
                var skillConfig = LocalDataManager.Instance.GetParSkillDataCellByParID(partnerID);
                if (skillConfig != null && skillConfig.Count > 0)
                {
                    foreach (var item in skillConfig)
                    {
                        PartnerSkill(item, avatarcfg.AnimsPath, avatarcfg.EffectsPath);
                    }
                }
            }
        }
    }

    private void HeroPrepare()
    {
        var config = LocalDataManager.Instance.GetJobDataCell(GameManager.Instance.HeroMD.Job);
        if (config != null)
        {
            var avatarID = config.GetAvatarID();

            var avatarcfg = LocalDataManager.Instance.GetAvatarDataCell(avatarID);
            if (avatarcfg != null)
            {
                //模型处理
                HeroModel(avatarcfg.GetModelId(), avatarcfg.GetHighModelId());
                //基础动作处理
                HeroBaseAniamation(avatarcfg.GetBaseAnims(), avatarcfg.AnimsPath);
                //图像处理
                HeroHeadIcon(avatarcfg.GetHeadID());
                //技能
                HeroSkill(avatarcfg.AnimsPath, avatarcfg.EffectsPath);
            }
        }
    }

    private void HeroHeadIcon(int headid)
    {
        var config = LocalDataManager.Instance.GetModelHeadDataCell(headid);
        if (config != null)
        {
            //if (!string.IsNullOrEmpty(config.HalfDrawing))
            //    PreLoadings.Add(("UnityEngine.Texture", config.HalfDrawing));
            if (!string.IsNullOrEmpty(config.HeadAtlasName))
                PreLoadings.Add(("UnityEngine.Texture", config.HeadAtlasName));
        }
    }

    private void HeroModel(int ModelId, int HightModelId)
    {
        var model = LocalDataManager.Instance.GetModelDataCell(ModelId);
        if (model != null)
        {
            string path = GetRealPath(model.ModelsPath, E_AssetType.Roles);
            if (!string.IsNullOrEmpty(path))
            {
                PreLoadings.Add(("UnityEngine.GameObject", path));
            }
        }

        var highmodel = LocalDataManager.Instance.GetModelDataCell(HightModelId);
        if (highmodel != null)
        {
            string path = GetRealPath(highmodel.ModelsPath, E_AssetType.Roles);
            if (!string.IsNullOrEmpty(path))
            {
                PreLoadings.Add(("UnityEngine.GameObject", path));
            }
        }
    }

    private string GetRealPath(string path, E_AssetType tyep)
    {
        return ResourceFormalManager.Instance.GetRecursionLoadAssetPath(path, tyep);
    }

    private void HeroBaseAniamation(int baseid, string AnimsPath)
    {
        var config = LocalDataManager.Instance.GetModelAnimancerDataCell(baseid);
        if (config != null)
        {
            if (!string.IsNullOrEmpty(config.Idle))
            {
                string path = GetRealPath($"{AnimsPath}/{config.Idle}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.BattleIdle))
            {
                string path = GetRealPath($"{AnimsPath}/{config.BattleIdle}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.WanderMoving))
            {
                string path = GetRealPath($"{AnimsPath}/{config.WanderMoving}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.SingleMoving))
            {
                string path = GetRealPath($"{AnimsPath}/{config.SingleMoving}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.BattleMoving))
            {
                string path = GetRealPath($"{AnimsPath}/{config.BattleMoving}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.Hurt))
            {
                string path = GetRealPath($"{AnimsPath}/{config.Hurt}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.Deading))
            {
                string path = GetRealPath($"{AnimsPath}/{config.Deading}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.Stand))
            {
                string path = GetRealPath($"{AnimsPath}/{config.Stand}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.WeaponRetractionIdle))
            {
                string path = GetRealPath($"{AnimsPath}/{config.WeaponRetractionIdle}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }

            if (!string.IsNullOrEmpty(config.WeaponRetractionMoving))
            {
                string path = GetRealPath($"{AnimsPath}/{config.WeaponRetractionMoving}", E_AssetType.Animation);
                if (!string.IsNullOrEmpty(path))
                {
                    PreLoadings.Add(("UnityEngine.AnimationClip", path));
                }
            }
        }
    }

    private void HeroSkill(string AnimsPath, string EffectsPath)
    {
        var HeroMD = GameManager.Instance.HeroMD;
        var current = HeroMD.JobSkillModel.SkillCases[HeroMD.JobSkillModel.CurSkillCaseID - 1];
        if (current != null)
        {
            List<int> Skills = new();
            foreach (var skill in current.PosList)
            {
                foreach (var item in HeroMD.JobSkillModel.AllSkill)
                {
                    if (item.SkillDBID == skill.JobSkillID)
                    {
                        var ctcfg = LocalDataManager.Instance.GetTalentDataCell(item.TalentID);

                        if (ctcfg != null)
                        {
                            Skills.AddRange(ctcfg.BattleActive);
                        }

                        break;
                    }
                }
            }

            if (Skills != null && Skills.Count > 0)
            {
                foreach (var skill in Skills)
                {
                    LocalDataManager.Instance.GetSkillJson(skill, (skillcfg) =>
                    {
                        if (skillcfg == null) { return; }
                        //  string effectName = $"{effectPathPrefix}/{fxParam.EffectPath}";
                        if (skillcfg.Normals != null && skillcfg.Normals.Count > 0)
                        {
                            foreach (var item in skillcfg.Normals)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.GameObject", path));
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                        }
                                    }
                                }
                            }

                            foreach (var item in skillcfg.Others)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.GameObject", path));
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                        }
                                    }
                                }
                            }

                            foreach (var item in skillcfg.Bullets)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.GameObject", path));
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }
    }

    private void PartnerSkill(ParSkillDataCell config, string AnimsPath, string EffectsPath)
    {
        if (config != null)
        {
            List<int> Skills = new();
            if (config.SelfSkill != null && config.SelfSkill.Count > 0)
            {
                Skills.AddRange(config.SelfSkill);
            }

            if (config.GetParSkill() > 0)
            {
                Skills.Add(config.GetParSkill());
            }

            foreach (var skill in Skills)
            {
                LocalDataManager.Instance.GetSkillJson(skill, (skillcfg) =>
                {
                    if (skillcfg == null)
                    {
                        SGF.Debuger.LogError($"RolePreLoading PartnerSkill() skill={skill},skillcfg=null err!!!");
                    }
                    else
                    {
                        //  string effectName = $"{effectPathPrefix}/{fxParam.EffectPath}";
                        if (skillcfg.Normals != null && skillcfg.Normals.Count > 0)
                        {
                            foreach (var item in skillcfg.Normals)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        if (!string.IsNullOrEmpty(effect.EffectName))
                                        {
                                            string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.GameObject", path));
                                            }
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        if (!string.IsNullOrEmpty(anima.ClipName))
                                        {
                                            string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var item in skillcfg.Others)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        if (!string.IsNullOrEmpty(effect.EffectName))
                                        {
                                            string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.GameObject", path));
                                            }
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        if (!string.IsNullOrEmpty(anima.ClipName))
                                        {
                                            string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var item in skillcfg.Bullets)
                            {
                                if (item.Fxs != null && item.Fxs.Count > 0)
                                {
                                    foreach (var effect in item.Fxs)
                                    {
                                        if (!string.IsNullOrEmpty(effect.EffectName))
                                        {
                                            string path = GetRealPath($"{EffectsPath}/{effect.EffectName}", E_AssetType.Effects);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.GameObject", path));
                                            }
                                        }
                                    }
                                }

                                if (item.Animations != null && item.Animations.Count > 0)
                                {
                                    foreach (var anima in item.Animations)
                                    {
                                        if (!string.IsNullOrEmpty(anima.ClipName))
                                        {
                                            string path = GetRealPath($"{AnimsPath}/{anima.ClipName}", E_AssetType.Animation);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                PreLoadings.Add(("UnityEngine.AnimationClip", path));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }
    }

    /// <summary>
    /// 预加载 壳子对应的 模板
    /// </summary>
    private void PreloadHeroTemplate(Action compeleteCb = null)
    {
        List<string> paths = new();
        paths.Add("Roles/Template/Bullet_Model2");
        paths.Add("Roles/Template/Character_Model");
        paths.Add("Roles/Template/Character_Model2");
        paths.Add("Roles/Template/Character_Model_Shadow");
        paths.Add("Roles/Template/Display_Model");
        paths.Add("Roles/Template/Interact_Model");
        paths.Add("Roles/Template/Local_Gateway_Model");
        paths.Add("Roles/Template/Local_Summon_Model");
        paths.Add("Roles/Template/Local_TreasureBox_Model");
        paths.Add("Roles/Template/Local_Wanted_Model");
        paths.Add("Roles/Template/Monster_Model2");
        paths.Add("Roles/Template/NPC_Model2");
        paths.Add("Roles/Template/Partner_Model2");
        paths.Add("Roles/Template/Summon_Model2");

        paths.ForEach((path) =>
        {
            PreLoadings.Add(("UnityEngine.GameObject", path));
        });

        // string folderName = "roles";
        // LoadFolderAsset<GameObject>(folderName, (result, items) =>
        // {
        //     compeleteCb?.Invoke();
        // });
    }

    private void LoadFolderAsset<T>(string folderName, Action<bool, IList<T>> compeleteCb, Action<T> itemLoadCb = null)
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(folderName, null, Addressables.MergeMode.None);
        handle.Completed += (AsyncOperationHandle<IList<T>> handle) =>
        {

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<T> assets = handle.Result;
                foreach (T obj in assets)
                {
                    if (itemLoadCb != null)
                    {
                        itemLoadCb?.Invoke(obj);
                    }
                }
                compeleteCb?.Invoke(true, assets);
            }
            else
            {
                compeleteCb?.Invoke(true, null);
                Debug.LogError("Failed to load assets: " + handle.OperationException);
            }
        };
    }
}