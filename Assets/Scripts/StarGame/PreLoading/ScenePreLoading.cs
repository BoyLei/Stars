using System;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using SkillEditor;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProject.Service.Resource;
using StarProjectDef;
using UnityEngine;

public class ScenePreLoading
{
    private string tag => $"[PreLoad-Scene] SceneID: {_sceneJsonData?.SceneID}";
    private SceneJsonData _sceneJsonData;

    public List<(string AssetType, string AssetPath)> PreLoadings = new();

    public void Start(SceneJsonData sceneJsonData)
    {
        Debug.Log($"{tag} start!!");
        _sceneJsonData = sceneJsonData;
        AnalysisScene(sceneJsonData);
        Loading(0.1f);
    }

    public void Start()
    {
        Debug.Log($"{tag} start!!");
        AnalysisScene(_sceneJsonData);
        Loading(0.1f);
    }


    public void End()
    {
        Debug.Log($"{tag} end!!");

        PreLoadings.Clear();
        DelayInvoker.CancelInvoke(this);
    }

    private void Loading(float delayTime)
    {
        int totalCount = PreLoadings.Count;
        int finishCount = 0;
        if (delayTime > 0)
        {
            for (int i = 0; i < PreLoadings.Count; i++)
            {
                DelayInvoker.DelayInvoke(this, delayTime * i, (args) =>
                {

                    var asset = ((string AssetType, string AssetPath))args[0];
                    LoaidngPreLoadingAssets((path) =>
                    {
                        finishCount++;
                        // Debug.Log($"{tag} loading: finishCount: {finishCount}, path {path} , success !!!");
                    }, asset);
                }, new object[] { PreLoadings[i] });
            }
        }
        else
        {
            for (int i = 0; i < PreLoadings.Count; i++)
            {
                LoaidngPreLoadingAssets((path) =>
                    {
                        finishCount++;
                        Debug.Log($"{tag} loading: finishCount: {finishCount}, path {path} , success !!!");
                    }, PreLoadings[i]);
            }
        }
    }

    private void LoaidngPreLoadingAssets(Action<string> itemLoadCb, (string AssetType, string AssetPath) item)
    {
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Object>(
            item.Item2, onComplete =>
                {
                    itemLoadCb?.Invoke(item.AssetPath);
                });

    }



    private void AnalysisScene(SceneJsonData sceneJsonData)
    {
        // 先看是否是新手关的 梦见战争 副本
        if (sceneJsonData.SceneID == 10004)
        {
            PreloadNewPlayerSkill();
        }

        List<long> monsters = new();

        foreach (var item in sceneJsonData.Monsters)
        {
            var monsterJsonData = item.Value;
            var monsterID = monsterJsonData.MonsterID;
            if (-1 == monsters.FindIndex((id) => id == monsterID))
            {
                monsters.Add(monsterID);
            }
        }

        PreloadMonsters(monsters);
    }

    private void PreloadNewPlayerSkill()
    {

        var PassiveID = 1100;
        var job = GameManager.Instance.HeroMD.Job;
        var config = LocalDataManager.Instance.GetJobDataCell(job);
        if (config == null)
        {
            return;
        }

        var avatarID = config.GetAvatarID();
        var avatarcfg = LocalDataManager.Instance.GetAvatarDataCell(avatarID);


        LocalDataManager.Instance.GetPassiveJson(PassiveID, (PassiveJson passiveJson) =>
        {
            if (passiveJson == null)
            {
                return;
            }

            var normals = passiveJson.Normals;
            for (int i = 0; i < normals.Count; i++)
            {
                var skillStage = normals[i];

                List<EffectJosn> effectJsons = skillStage.Effects;

                // 拿到阶段中 效果 对应的 dic
                var effectDatasDic = GetEffectDatasDic(effectJsons, $"[Passive_{PassiveID}_skillstage_{skillStage.StageID}");

                // 拿到指定的 检查职业id 的效果
                var checkJobEffect = GetCheckJobIDEffect(effectDatasDic, job);

                if (checkJobEffect == null)
                {
                    continue;
                }

                // 找到下一个 效果
                int nextID = checkJobEffect.Next.Length > 0 ? checkJobEffect.Next[0] : 0;

                while (nextID != 0)
                {
                    if (!effectDatasDic.ContainsKey(nextID))
                    {
                        nextID = 0;
                        continue;
                    }
                    SkillEditor.EffectData effect = effectDatasDic[nextID];

                    if (effect.EffectType != EffectType.AddPassive)
                    {
                        if (effect.Next.Length == 0)
                        {
                            nextID = 0;
                        }
                        else
                        {
                            nextID = effect.Next[0];
                        }
                    }
                    else
                    {
                        var addPassive = effectDatasDic[nextID].BaseEffect as EffectTypeAddPassive;

                        PreloadPassiveChangeSkill(addPassive.PassiveID, avatarcfg.AnimsPath, avatarcfg.EffectsPath);
                        return;
                    }


                }

            }

        });

    }

    private Dictionary<int, SkillEditor.EffectData> GetEffectDatasDic(List<EffectJosn> effectJsons, string tag)
    {
        // 效果轴 id 是唯一,不管一个阶段有多少的效果轴
        Dictionary<int, SkillEditor.EffectData> effectDataDic = new();

        for (int i = 0; i < effectJsons.Count; i++)
        {
            EffectJosn effect = effectJsons[i];

            List<SkillEditor.EffectData> effectDatas = effect.data;

            for (int j = 0; j < effectDatas.Count; j++)
            {
                SkillEditor.EffectData effectData = effectDatas[j];

                var effectID = effectData.EffectID;

                if (!effectDataDic.ContainsKey(effectID))
                {
                    effectDataDic.Add(effectID, effectData);
                }
                else
                {
                    Debug.LogError($"{tag} 效果 存在重复的效果id: {effectID} ");
                    effectDataDic[effectID] = effectData;
                }
            }
        }
        return effectDataDic;
    }

    private SkillEditor.EffectData GetCheckJobIDEffect(Dictionary<int, SkillEditor.EffectData> effectDatasDic, int jobID)
    {
        return GetEffectData(effectDatasDic, (SkillEditor.EffectData effectDtat) =>
                {
                    if (effectDtat.EffectType != EffectType.CheckJobID)
                    {
                        return false;
                    }
                    var checkEffect = effectDtat.BaseEffect as EffectTypeCheckJobID;

                    if (checkEffect != null && checkEffect.JobID == jobID)
                    {
                        return true;
                    }

                    return false;
                });
    }

    private SkillEditor.EffectData GetEffectData(Dictionary<int, SkillEditor.EffectData> effectDatasDic, Func<SkillEditor.EffectData, bool> checkEffect)
    {
        foreach (var item in effectDatasDic)
        {
            if (checkEffect.Invoke(item.Value))
            {
                return item.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// 预加载 被动改变的 技能
    /// </summary>
    private void PreloadPassiveChangeSkill(int passiveID, string AnimsPath, string EffectsPath)
    {
        LocalDataManager.Instance.GetPassiveJson(passiveID, (PassiveJson passiveJson) =>
        {
            if (passiveJson == null)
            {
                return;
            }
            var globalShows = passiveJson.config.GlobalShows;

            for (int i = 0; i < globalShows.Count; i++)
            {
                var globalShow = globalShows[i];
                if (globalShow.GlobalShowType == GlobalShowType.BUFF_JobSkillChange)
                {
                    var talent = globalShow.BUFF_JobSkillChange.TalentAndSlot;

                    for (int j = 0; j < talent.Count; j++)
                    {
                        int talentID = talent[i].TalentID;

                        PreloadTalentBattleActive(talentID, AnimsPath, EffectsPath);

                    }
                }
            }
        }, false);
    }
    private void PreloadTalentBattleActive(int talentID, string AnimsPath, string EffectsPath)
    {
        var cfg = LocalDataManager.Instance.GetTalentDataCell(talentID);
        if (cfg == null)
        {
            return;
        }

        var battleSkill = cfg.BattleActive;
        for (int i = 0; i < battleSkill.Count; i++)
        {
            var skill = battleSkill[i];

            PreloadSkill(skill, AnimsPath, EffectsPath);

        }

    }

    private void PreloadMonsters(List<long> monsters)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            PreloadMonster(monsters[i], i * 1.0f);
        }
    }



    private void PreloadMonster(long monsterId, float delayTime)
    {

        MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(monsterId);
        if (monsterDataCell == null)
        {
            Debug.Log($"{tag} PreloadMonster : {monsterId} 找不到 monsterDataCell "); ;

            return;
        }
        var avatarcfg = LocalDataManager.Instance.GetAvatarDataCell(monsterDataCell.GetAvatarID());



        if (avatarcfg == null)
        {
            Debug.Log($"{tag} PreloadMonster : {monsterId} 找不到  avatar : {monsterDataCell.GetAvatarID()}"); ;

            return;
        }

        PreloadModel(avatarcfg.GetModelId());
        PreloadBaseAnim(avatarcfg.GetBaseAnims(), avatarcfg.AnimsPath);
        PreloadSkills(monsterDataCell.ActiveSkills, avatarcfg.AnimsPath, avatarcfg.EffectsPath);

    }

    /// <summary>
    /// 场景中 显示的 怪物 基本只需要显示 标准模型, 而伙伴，在伙伴抽卡/展示这种界面，才需要显示高模
    /// </summary>
    private void PreloadModel(int modelID)
    {
        ModelDataCell model = LocalDataManager.Instance.GetModelDataCell(modelID);
        if (model == null)
        {
            return;
        }
        string path = FormateRealPath(model.ModelsPath, E_AssetType.Roles);
        if (!string.IsNullOrEmpty(path))
        {
            PreLoadings.Add(("UnityEngine.GameObject", path));
        }
    }

    private void PreloadBaseAnim(int baseAnimID, string basePath)
    {
        var config = LocalDataManager.Instance.GetModelAnimancerDataCell(baseAnimID);
        if (config == null)
        {
            return;
        }

        TryPreloadPath(config.Idle, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.BattleIdle, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.WanderMoving, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.SingleMoving, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.BattleMoving, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.Hurt, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.Deading, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.Stand, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.WeaponRetractionIdle, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);
        TryPreloadPath(config.WeaponRetractionMoving, E_AssetType.Animation, "UnityEngine.AnimationClip", basePath);

    }

    private void PreloadSkills(List<int> skills, string animsBasePath, string effectsBasePath)
    {
        skills.ForEach(skill => PreloadSkill(skill, animsBasePath, effectsBasePath));
    }

    private void PreloadSkill(int skillID, string animsBasePath, string effectsBasePath)
    {
        LocalDataManager.Instance.GetSkillJson(skillID, (skillcfg) =>
            {
                if (skillcfg == null)
                {
                    return;
                }

                if (skillcfg.Normals != null && skillcfg.Normals.Count > 0)
                {
                    foreach (var item in skillcfg.Normals)
                    {
                        if (item.Fxs != null && item.Fxs.Count > 0)
                        {
                            foreach (var effect in item.Fxs)
                            {
                                TryPreloadPath(effect.EffectName, E_AssetType.Effects, "UnityEngine.GameObject", effectsBasePath);
                            }
                        }

                        if (item.Animations != null && item.Animations.Count > 0)
                        {
                            foreach (var anima in item.Animations)
                            {
                                TryPreloadPath(anima.ClipName, E_AssetType.Animation, "UnityEngine.AnimationClip", animsBasePath);
                            }
                        }
                    }

                    foreach (var item in skillcfg.Others)
                    {
                        if (item.Fxs != null && item.Fxs.Count > 0)
                        {
                            foreach (var effect in item.Fxs)
                            {
                                TryPreloadPath(effect.EffectName, E_AssetType.Effects, "UnityEngine.GameObject", effectsBasePath);
                            }
                        }

                        if (item.Animations != null && item.Animations.Count > 0)
                        {
                            foreach (var anima in item.Animations)
                            {
                                TryPreloadPath(anima.ClipName, E_AssetType.Animation, "UnityEngine.AnimationClip", animsBasePath);
                            }
                        }
                    }

                    foreach (var item in skillcfg.Bullets)
                    {
                        if (item.Fxs != null && item.Fxs.Count > 0)
                        {
                            foreach (var effect in item.Fxs)
                            {
                                TryPreloadPath(effect.EffectName, E_AssetType.Effects, "UnityEngine.GameObject", effectsBasePath);
                            }
                        }

                        if (item.Animations != null && item.Animations.Count > 0)
                        {
                            foreach (var anima in item.Animations)
                            {
                                TryPreloadPath(anima.ClipName, E_AssetType.Animation, "UnityEngine.AnimationClip", animsBasePath);
                            }
                        }
                    }
                }
            });

    }

    private void TryPreloadPath(string path, E_AssetType tyep, string loadingType, string basePath = "")
    {
        string finalPath = FormateRealPath(path, tyep, basePath);
        if (string.IsNullOrEmpty(finalPath))
        {
            return;
        }

        PreLoadings.Add((loadingType, finalPath));

    }


    private string FormateRealPath(string path, E_AssetType tyep, string basePath = "")
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        if (basePath == "")
        {
            return GetRealPath(path, tyep);
        }

        return GetRealPath($"{basePath}/{path}", tyep);
    }

    private string GetRealPath(string path, E_AssetType tyep)
    {
        return ResourceFormalManager.Instance.GetRecursionLoadAssetPath(path, tyep);
    }
}
