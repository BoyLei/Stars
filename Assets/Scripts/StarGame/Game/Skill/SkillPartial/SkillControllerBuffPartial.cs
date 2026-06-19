using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using StarProject.Game.Entity.Factory;
using UnityEngine;
using System;
using StarProject.Service.LocalData;

namespace StarProject.Game.Skill
{
    public partial class SkillController
    {
        private Dictionary<ulong, SkillBuff> buffDic = new Dictionary<ulong, SkillBuff>();

        private List<SkillBuff> temp2ReleaseBuffs = new List<SkillBuff>();
        private List<SkillBuff> temp2EnterFrameBuffs = new List<SkillBuff>();

        public Action<SkillBuff, BuffCreateRet> ActionOnBuffCreate;

        public Action<SkillBuff, BuffEndRet> ActionOnBuffEnd;

        public Action<SkillBuff> ActionOnBuffUpdate;

        private void OnBuffRunStage(RunStageRet runStageRet)
        {
            ulong runtimeID = runStageRet.RuntimeID;
            // int stageID = runStageRet.StageID;
            // int loopIdx = runStageRet.StageLoop;

            // string stageIDStr = SkillStage.FormatStageIDStr(stageID, loopIdx);
            //SGF.Debuger.LogError($"{TagFlag} [server]  OnBuffRunStage runtimeID {runtimeID}, stageIDStr {stageIDStr}  ");

            if (!buffDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"{TagFlag} [server]  OnBuffRunStage runtimeID {runtimeID}, stageIDStr {stageIDStr} ,not find,error!!! ");
                return;
            }

            buffDic[runtimeID].OnRunStageRet(runStageRet);
        }

        /// <summary>
        /// 创建buff 对应的 SkillBuff 实体
        /// </summary>
        /// <param name="buffCreateRet"></param>
        public void OnBuffCreateRet(BuffCreateRet buffCreateRet)
        {
            SkillBuff buff = EntityFactory.InstanceEntity<SkillBuff>();
            RegisterBuffAction(buff);

            //SGF.Debuger.LogError($"[shadow]  {TagFlag} [server]  OnBuffCreateRet runtimeID {buffCreateRet.RuntimeID}  ");

            ulong runtimeID = buffCreateRet.RuntimeID;
            if (buffDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"{TagFlag} [server]  OnBuffCreateRet runtimeID {runtimeID}, repeated,error!!! ");
                ReleaseItemBuff(runtimeID);
            }
            buffDic.Add(runtimeID, buff);

            buff.Create(buffCreateRet, playerData);
            ActionOnBuffCreate?.Invoke(buff, buffCreateRet);
        }

        public void OnBuffRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            //SGF.Debuger.Log($"{TagFlag} [server]  OnBuffRuntimeSync runtimeID {runtimeSyncRet.RuntimeID}  ");

            SkillBuff skillBuff = GetSkillBuff(runtimeSyncRet.RuntimeID);
            if (skillBuff == null)
            {
                return;
            }
            skillBuff.OnRuntimeSync(runtimeSyncRet);
        }

        /// <summary>
        /// 所有的buff
        /// </summary>
        public Dictionary<ulong, SkillBuff> GetSkillBuffs()
        {
            return buffDic;
        }

        private SkillBuff GetSkillBuff(ulong runtimeID)
        {
            SkillBuff skillBuff;
            if (buffDic.TryGetValue(runtimeID, out skillBuff))
            {
                return skillBuff;
            }
            return null;
        }

        /// <summary>
        /// 根据 buffID 获取 skillBuff
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public SkillBuff GetSkillBuffWithBuffID(int buffID)
        {
            foreach (KeyValuePair<ulong, SkillBuff> item in buffDic)
            {
                if (item.Value.BuffID == buffID)
                {
                    return item.Value;
                }
            }
            return null;
        }

        private void RegisterBuffAction(SkillBuff buff)
        {

            // buff.FuncOnSkillTrySetActiveSkill = TrySetActiveSkill;
            // buff.ActionOnSkillExit = OnActionSkillExit;
            buff.ActionOnRefreshStates = OnActionRefreshStates;
            buff.ActionOnPlayAnim = OnActionPlayAnim;
            buff.ActionOnPlayFx = OnActionPlayFx;
            buff.ActionOnPlayAudio = OnActionPlayAudio;      //目前假设音频一个事件对应多个音频的制作方式

            buff.FuncOnTryPlayClientEffect = StageTryPlayClientEffect;
            buff.FuncOnTryPlayServerEffect = OnFuncStageTryPlayServerEffect;
            buff.ActionOnStopFx = OnActionStopFx;
            // buff.ActionOnStageTryPlayCameraShake = OnActionPlayCameraShake;

            buff.ActionOnUpdateBuff = OnActionUpdateBuff;
        }

        public void OnBuffEndRet(BuffEndRet buffEndRet)
        {
            ulong runtimeID = buffEndRet.RuntimeID;
            //SGF.Debuger.LogError($" [shadow] {TagFlag} [server]  OnBuffEndRet runtimeID {buffEndRet.RuntimeID}  ");

            if (!buffDic.ContainsKey(runtimeID))
            {
                //SGF.Debuger.LogError($"[shadow] {TagFlag} [server]  OnBuffEndRet runtimeID {runtimeID},BuffID={buffEndRet.BuffID},EndType={buffEndRet.EndType}, not find,error!!! ");
                return;
            }
            SkillBuff skillBuff = buffDic[runtimeID];
            skillBuff.OnBuffEndRet(buffEndRet);

            ActionOnBuffEnd?.Invoke(skillBuff, buffEndRet);

            // ReleaseItemBuff(runtimeID);
        }

        public void OnActionUpdateBuff(SkillBuff skillBuff)
        {
            ActionOnBuffUpdate.Invoke(skillBuff);
        }

        private void EnterFrameBuff()
        {

            temp2EnterFrameBuffs.Clear();
            temp2ReleaseBuffs.Clear();
            foreach (KeyValuePair<ulong, SkillBuff> item in buffDic)
            {
                if (item.Value.IsRunning)
                {
                    temp2EnterFrameBuffs.Add(item.Value);
                }
                else
                {
                    temp2ReleaseBuffs.Add(item.Value);
                }
            }

            // 先释放旧的 buff 运行时;
            ReleaseTempBuffs();

            // 再运行 buff 运行时
            EnterFrameRunnintBuff();
        }

        private void EnterFrameRunnintBuff()
        {
            for (int i = 0; i < temp2EnterFrameBuffs.Count; i++)
            {
                SkillBuff buff = temp2EnterFrameBuffs[i];
                buff.EnterFrame();
            }

            temp2ReleaseBuffs.Clear();
        }

        private void ReleaseItemBuff(ulong runtimeID)
        {
            if (buffDic.ContainsKey(runtimeID))
            {
                EntityFactory.ReleaseEntity(buffDic[runtimeID]);
                buffDic.Remove(runtimeID);
            }
        }




        private void ReleaseTempBuffs()
        {
            for (int i = 0; i < temp2ReleaseBuffs.Count; i++)
            {
                SkillBuff buff = temp2ReleaseBuffs[i];
                ulong runtimeID = buff.RuntimeID;
                ReleaseItemBuff(runtimeID);
            }

            temp2ReleaseBuffs.Clear();
        }

        private void ReleaseBuffAction()
        {
            ActionOnBuffCreate = null;
            ActionOnBuffEnd = null;
            ActionOnBuffUpdate = null;
        }

        private void ResetBuff()
        {
            foreach (KeyValuePair<ulong, SkillBuff> item in buffDic)
            {
                ActionOnBuffEnd?.Invoke(item.Value, null);

                EntityFactory.ReleaseEntity(item.Value);
            }
            buffDic.Clear();

            ReleaseTempBuffs();


        }

        private int cancelBuffId = 0;
        private bool CheckJoyStickCancleBuff()
        {
            if (cancelBuffId == 0)
            {
                var cfg = LocalDataManager.Instance.GetSystemDataCell("Task_FastFast");
                if (cfg != null)
                {
                    cancelBuffId = cfg.Value;
                }

            }

            if (cancelBuffId == 0)
            {
                return false;
            }
            foreach (var item in buffDic)
            {
                if (item.Value.BuffID == cancelBuffId)
                {

                    return true;
                }
            }
            return false;
        }

        private void JoyStickCancleBuff()
        {
            TaskHelper.SendReqSpeedBuff(false);
        }
    }
}
