using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoMsg;
using StarProject.Game.Entity.Factory;
using System;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// SkillController 的 Passive 部分
    /// </summary>
    public partial class SkillController
    {

        private DictionaryEx<ulong, PassiveSkillEntity> passiveSkillDic = new DictionaryEx<ulong, PassiveSkillEntity>();

        public Action<PassiveSkillEntity, PassiveSkillUseRet> ActionOnPassiveCreate;


        public Action<PassiveSkillEntity, PassiveSkillEndRet> ActionOnPassiveEnd;


        private void EnterFramePassive()
        {
            foreach (KeyValuePair<ulong, PassiveSkillEntity> item in passiveSkillDic)
            {
                item.Value.EnterFrame();
            }
        }

        private void ResetPassive()
        {
            foreach (KeyValuePair<ulong, PassiveSkillEntity> item in passiveSkillDic)
            {
                EntityFactory.ReleaseEntity(item.Value);
            }
            passiveSkillDic.Clear();
        }

        /// <summary>
        /// 所有的被动
        /// </summary>
        public Dictionary<ulong, PassiveSkillEntity> GetPassiveSkills()
        {
            return passiveSkillDic;
        }


        public void OnPassiveSkillUseRet(PassiveSkillUseRet passiveSkillUseRet)
        {
            PassiveSkillEntity passiveSkillEntity;
            ulong runtimeID = passiveSkillUseRet.RuntimeID;
            if (passiveSkillDic.ContainsKey(runtimeID))
            {
                passiveSkillEntity = passiveSkillDic[runtimeID];
                //SGF.Debuger.LogError($"{TagFlag} OnPassiveSkillUseRet runtimeID {runtimeID} , skillID {passiveSkillUseRet.SkillID} same");
                return;
            }
            else
            {
                passiveSkillEntity = EntityFactory.InstanceEntity<PassiveSkillEntity>();
                passiveSkillDic.Add(runtimeID, passiveSkillEntity);
                RegisterPassiveAction(passiveSkillEntity);
            }

            passiveSkillEntity.Create(playerData, passiveSkillUseRet, container);
            ActionOnPassiveCreate?.Invoke(passiveSkillEntity, passiveSkillUseRet);
        }

        /// <summary>
        /// 注册 passive 的事件
        /// </summary>
        /// <param name="passive"></param>
        private void RegisterPassiveAction(PassiveSkillEntity passive)
        {
            passive.ActionOnPlayAnim = OnActionPlayAnim;
            passive.ActionOnPlayFx = OnActionPlayFx;
            passive.ActionOnPlayAudio = OnActionPlayAudio;      //目前假设音频一个事件对应多个音频的制作方式
            passive.ActionOnRefreshStates = OnActionRefreshStates;

            passive.FuncOnTryPlayClientEffect = StageTryPlayClientEffect;
            passive.FuncOnTryPlayServerEffect = OnFuncStageTryPlayServerEffect;
            passive.ActionOnStopFx = OnActionStopFx;
        }

        public void OnPassiveRunStageRet(RunStageRet runStageRet)
        {
            ulong runtimeID = runStageRet.RuntimeID;
            // string stageIdStr = SkillStage.FormatStageIDStr(runStageRet.StageID, runStageRet.StageLoop);

            if (!passiveSkillDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"{TagFlag} OnPassiveRunStageRet not Create runtimeID {runtimeID} , stage : {stageIdStr} error!!!");
                return;
            }
            PassiveSkillEntity passiveSkillEntity = passiveSkillDic[runtimeID];
            passiveSkillEntity.OnRunStageRet(runStageRet);
        }

        public void OnPassiveRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            ulong runtimeID = runtimeSyncRet.RuntimeID;

            if (!passiveSkillDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"{TagFlag} OnPassiveRuntimeSync not Create runtimeID {runtimeID} error!!!");
                return;
            }
            PassiveSkillEntity passiveSkillEntity = passiveSkillDic[runtimeID];
            passiveSkillEntity.OnRuntimeSync(runtimeSyncRet);
        }

        public void OnPassiveSkillEndRet(PassiveSkillEndRet passiveSkillEndRet)
        {
            ulong runtimeID = passiveSkillEndRet.RuntimeID;
            if (!passiveSkillDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"{TagFlag} OnPassiveSkillEndRet not Create runtimeID {runtimeID} , skillID {passiveSkillEndRet.SkillID} error");
                return;
            }
            //    SGF.Debuger.LogError($"{TagFlag} OnPassiveSkillEndRet  runtimeID {runtimeID} , skillID {passiveSkillEndRet.SkillID} error");

            PassiveSkillEntity passiveSkillEntity = passiveSkillDic[runtimeID];
            ActionOnPassiveEnd?.Invoke(passiveSkillEntity, passiveSkillEndRet);
            passiveSkillEntity.OnPassiveSkillEndRet(passiveSkillEndRet);

            EntityFactory.ReleaseEntity(passiveSkillEntity);
            passiveSkillDic.Remove(runtimeID);

        }

        private void ReleasePassiveAction()
        {
            ActionOnPassiveCreate = null;
            ActionOnPassiveEnd = null;
        }
    }
}