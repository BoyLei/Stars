using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using StarProject.Service.Time.Base;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 被动技能实体
    /// 目前被动存放在SkillDispatcher.skillController中.
    /// note:   
    ///     子弹目前没有挂 SkillDispatcher,所以目前子弹没有被动
    ///     但子弹可以设计为一个召唤物,类似于LOL中大发明家召唤的炮台,
    ///     它没有人身上那么多的功能,但是它可以挂被动.
    ///     看后续需求是否需要在子弹上添加 被动
    /// </summary>
    public class PassiveSkillEntity : ServerControlStageEntityBase
    {
        private VitalSignData playerData;

        private PassiveInfo passiveInfo;
        public PassiveInfo PassiveInfo => passiveInfo;

        private int skillId = 0;
        private int RuntimeLv = 1;


        private PassiveDescDataCell passiveDescDataCell;
        public PassiveDescDataCell PassiveDescDataCell
        {
            get
            {
                if (passiveDescDataCell == null)
                {
                    passiveDescDataCell = LocalDataManager.Instance.GetPassiveDescDataCellBySkillIdAndLevel(skillId, RuntimeLv);
                }
                return passiveDescDataCell;
            }
        }


        /// <summary>
        /// 被动tick时, 需要播放的特效效果
        /// </summary>
        public Action<List<int>, ulong, string, float> ActionOnPlayEffects;

        /// <summary>
        /// 被动结束的Action
        ///     1.刷新被动的UI
        ///     2.移除被动的特效
        /// </summary>
        public Action<List<int>, string> ActionOnPassiveSkillEnd;

        public void Create(VitalSignData data, PassiveSkillUseRet passiveSkillUseRet, Transform parent)
        {
            Create(passiveSkillUseRet.RuntimeID, passiveSkillUseRet.OwnerID, passiveSkillUseRet.BuilderID);

            playerData = data;

            skillId = passiveSkillUseRet.SkillID;
            RuntimeLv = passiveSkillUseRet.RuntimeLv > 0 ? passiveSkillUseRet.RuntimeLv : 1;

            TagFlag = $"[{playerData.M_EntityID}] [Passive_{skillId}] RuntimeID[{RuntimeID}] ";

            EntityBlackBoard.Set(BaseBlackBoard.KEY_CFG_ID, skillId, E_BlackBoardTag.Client);

            CreateStageHandle();

            CreatePassiveInfo();

            // 处理被动创建时的 黑板数据
            HandleBlackList(passiveSkillUseRet.BlackList, "PassiveSkillUseRet", true);

            PlayCreateLoopEffects(passiveInfo.LoopEffectFxs, true);

            ExecuteStageStates(passiveInfo.States, true);
            passiveDescDataCell = null;
            //SGF.Debuger.Log($"{TagFlag}  Create passive runtimeID {RuntimeID}");
        }

        private void CreateStageHandle()
        {
            PassiveStageHandle handle = EntityFactory.InstanceEntity<PassiveStageHandle>();
            handle.SetVitalSignData(playerData);
            SetStageHandle(handle);
        }

        private void CreatePassiveInfo()
        {
            passiveInfo = EntityFactory.InstanceEntity<PassiveInfo>();
            passiveInfo.Init(skillId);
            baseConfigInfo = passiveInfo;

            if (passiveInfo.Cfg == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} 被动: [{skillId}] 找不到配置!!! call 高磊!!!");
                return;
            }
            PlayEffects(passiveInfo.Cfg.GlobalShows, skillId);
        }

        public void OnPassiveSkillEndRet(PassiveSkillEndRet passiveSkillEndRet)
        {
            // 收到服务器的被动结束, 
            state = E_EntityState.ServerClose;
            StopEffects();
            //SGF.Debuger.Log($"{TagFlag}  OnPassiveSkillEndRet runtimeID {RuntimeID}");
        }

        #region LUA层使用

        public string GetBuffIconPath()
        {
            string path = "";

            if (PassiveDescDataCell != null)
            {
                path = PassiveDescDataCell.IconPath;
            }

            return path;
        }

        public string GetBuffIcon2Path()
        {
            string path = "";

            if (PassiveDescDataCell != null)
            {
                path = PassiveDescDataCell.IconPath2;
            }

            return path;
        }

        public string GetBuffName()
        {
            string name = "";

            if (PassiveDescDataCell != null)
            {
                name = PassiveDescDataCell.SkillName;
            }

            return name;
        }

        public string GetBuffDecs()
        {
            string decs = "";

            if (PassiveDescDataCell != null)
            {
                decs = PassiveDescDataCell.Decs;
            }

            //if (string.IsNullOrEmpty(decs) || string.IsNullOrWhiteSpace(decs))
            //{
            //    if (BuffInfo != null && BuffInfo.Cfg != null)
            //    {
            //        decs = BuffInfo.Cfg.BuffDesc;
            //    }
            //}

            return decs;
        }

        public int GetBuffLevel()
        {
            int level = 1;
            if (PassiveDescDataCell != null)
            {
                level = PassiveDescDataCell.GetLevel();
            }
            return level;
        }

        #endregion


        protected override void Release()
        {
#if UNITY_EDITOR_WIN
            if (passiveInfo == null)
            {
                SGF.Debuger.LogWarning($"被动 : {skillId} 执行 release 找不到 passiveInfo 报错!!!");
                Debug.Break();
            }
#endif
            StopEffects();

            ExecuteStageStates(passiveInfo?.States, false);

            PlayCreateLoopEffects(passiveInfo?.LoopEffectFxs, true);



            playerData = null;

            EntityFactory.ReleaseEntity(passiveInfo);

            passiveInfo = null;

            skillId = 0;
            RuntimeLv = 0;


            ActionOnPlayEffects = null;

            ActionOnPassiveSkillEnd = null;

            base.Release();
        }
    }
}
