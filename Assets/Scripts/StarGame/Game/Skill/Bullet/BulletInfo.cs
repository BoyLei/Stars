using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using UnityEngine;

namespace StarProject.Game.Skill
{
    public class BulletInfo : EntityRemoteStatic
    {
        private string TagFlag
        {
            get => $"[BulletInfo_{BulletID}] ";
        }
        public int BulletID;
        public SkillEditor.BulletConfig Cfg;

        /// <summary>
        /// buff普通阶段的配置,不能clear
        /// </summary>
        public List<StageJson> NormalStageJsons;

        public List<StageJson> BulletStageJsons;

        public List<StageJson> OtherStageJsons;

        private TimeLineStageInfos normalTimeLineStageInfos;

        private TimeLineStageInfos bulletTimeLineStageInfos;

        protected TimeLineStageInfos otherTimeLineStageInfos;

        public List<FXJson> LoopEffectFxs = new List<FXJson>();

        public void Init(int bulletID, System.Action<BulletInfo> finishCb)
        {
            BulletID = bulletID;

            normalTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
            bulletTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
            otherTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();

            // 配置理论上应该用立即同步逻辑,
            LocalDataManager.Instance.GetBulletJson(bulletID, (BulletJson bulletJson) =>
            {
                if (bulletJson == null)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} bulletID : {bulletID}  token cfg error!!!");
                    return;
                }

                // SGF.Debuger.Log($"{TagFlag} bulletID : {bulletID}  init");

                Cfg = bulletJson.config;

                InitLoopEffect();

                NormalStageJsons = bulletJson.Normals;
                BulletStageJsons = bulletJson.Bullets;
                OtherStageJsons = bulletJson.Others;

                normalTimeLineStageInfos.InitFrameEvents(NormalStageJsons, bulletID);
                bulletTimeLineStageInfos.InitFrameEvents(BulletStageJsons, bulletID);
                otherTimeLineStageInfos.InitFrameEvents(OtherStageJsons, bulletID);

                finishCb?.Invoke(this);
            });
        }

        private void InitLoopEffect()
        {
            List<EffectTypeHitEffect> loopEffectscfg = Cfg.LoopEffectInEditors;
            if (loopEffectscfg == null)
            {
                return;
            }

            ConverFx.LoopEffects2FxJsons(Cfg.LoopEffectInEditors, LoopEffectFxs);
        }
        /// <summary>
        /// 获取buff 基于timeLine时间轴上的阶段事件帧数据
        /// </summary>
        /// <param name="stageJson"></param>
        public TimeLineStage GetTimeLineStage(StageJson stageJson)
        {
            switch (stageJson.StageType)
            {
                case StageType.NormalStage:
                    {
                        return normalTimeLineStageInfos.GetTimeLineStage(stageJson.StageID, 0);
                    }
                case StageType.BulletStage:
                    {
                        return bulletTimeLineStageInfos.GetTimeLineStage(stageJson.StageID, 0);
                    }
                case StageType.TriggerStage:
                    {
                        return otherTimeLineStageInfos.GetTimeLineStage(stageJson.StageID, 0);
                    }
                default:
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} 子弹获取 未处理 阶段类型 StageType: {stageJson.StageType}, error!!!");
                    }
                    break;
            }
            return null;
        }

        public StageJson FindStageJson(List<StageJson> stageJsons, int stageId)
        {
            if (stageJsons == null)
            {
                return null;
            }
            return stageJsons.Find((StageJson stageJson) =>
            {
                return stageJson.StageID == stageId;
            });
        }

        /// <summary>
        /// 获取 normalStage中的 stageJson
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public StageJson GetStageJson(int stageId)
        {
            StageJson stageJson = FindStageJson(NormalStageJsons, stageId);

            if (stageJson != null)
            {
                return stageJson;
            }
            stageJson = FindStageJson(BulletStageJsons, stageId);
            if (stageJson != null)
            {
                return stageJson;
            }
            stageJson = FindStageJson(OtherStageJsons, stageId);
            return stageJson;
        }

        protected override void Release()
        {
            base.Release();

            BulletID = 0;
            Cfg = null;

            NormalStageJsons = null;
            BulletStageJsons = null;
            OtherStageJsons = null;

            EntityFactory.ReleaseEntity(normalTimeLineStageInfos);
            normalTimeLineStageInfos = null;

            EntityFactory.ReleaseEntity(bulletTimeLineStageInfos);
            bulletTimeLineStageInfos = null;

            EntityFactory.ReleaseEntity(otherTimeLineStageInfos);
            otherTimeLineStageInfos = null;

            LoopEffectFxs.Clear();
        }

    }
}
