using System;
using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能中, 子弹/buff/被动 等 配置信息info的基类
    /// </summary>
    public class BaseConfigInfo : EntityRemoteStatic
    {
        protected int ID = 0;

        public List<StageJson> NormalStageJsons;
        public List<StageJson> OtherStageJsons;
        public List<StageJson> BulletStageJsons;

        protected TimeLineStageInfos normalTimeLineStageInfos;
        protected TimeLineStageInfos otherTimeLineStageInfos;
        protected TimeLineStageInfos bulletTimeLineStageInfos;


        public List<FXJson> LoopEffectFxs = new List<FXJson>();

        protected List<int> states = new List<int>();

        /// <summary>
        /// 原子状态:
        /// note: 
        ///     原子状态 _states 需要在子类中赋值,但不能直接 引用配置,
        ///     因为在 Release 时, 需要将 _states.Clear().
        ///     直接引用 配置原始数据,会导致 数据报错
        /// </summary>
        public List<int> States
        {
            get
            {
                return states;
            }
        }

        public virtual void Init(int id)
        {
            ID = id;
            normalTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
            otherTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
            bulletTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
        }


        /// <summary>
        /// 获取 基于timeLine时间轴上的阶段事件帧数据
        /// </summary>
        /// <param name="stageJson"></param>
        public TimeLineStage GetTimeLineStage(StageJson stageJson)
        {
            int stageID = stageJson.StageID;
            switch (stageJson.StageType)
            {
                case StageType.NormalStage:
                    {
                        return normalTimeLineStageInfos.GetTimeLineStage(stageID, 0);
                    }
                case StageType.BulletStage:
                    {
                        return bulletTimeLineStageInfos.GetTimeLineStage(stageID, 0);
                    }
                case StageType.AddBuffStage:
                    {
                        return otherTimeLineStageInfos.GetTimeLineStage(stageID, 0);
                    }
                case StageType.EndBuffStage:
                    {
                        return otherTimeLineStageInfos.GetTimeLineStage(stageID, 0);
                    }
                case StageType.TriggerStage:
                    {
                        return otherTimeLineStageInfos.GetTimeLineStage(stageID, 0);
                    }
                default:
                    {
                        //DB_Close    SGF.Debuger.LogError($"[BaseConfigInfo] GetTimeLineStage [{stageID}] StageType : {stageJson.StageType} no handle!!!");
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
        /// 获取 配置中的 stageJson
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

            stageJson = FindStageJson(OtherStageJsons, stageId);

            if (stageJson != null)
            {
                return stageJson;
            }

            stageJson = FindStageJson(BulletStageJsons, stageId);
            return stageJson;
        }

        protected override void Release()
        {
            base.Release();
            ID = 0;

            NormalStageJsons = null;
            OtherStageJsons = null;
            BulletStageJsons = null;

            LoopEffectFxs.Clear();

            states.Clear();

            EntityFactory.ReleaseEntity(normalTimeLineStageInfos);
            normalTimeLineStageInfos = null;

            EntityFactory.ReleaseEntity(otherTimeLineStageInfos);
            otherTimeLineStageInfos = null;

            EntityFactory.ReleaseEntity(bulletTimeLineStageInfos);
            bulletTimeLineStageInfos = null;
        }
    }
}
