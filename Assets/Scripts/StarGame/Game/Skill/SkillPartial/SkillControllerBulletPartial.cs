using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StarProject.Game.Entity.Factory;
using ProtoMsg;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// SkillController 的 Bullet 部分
    /// </summary>
    public partial class SkillController
    {
        string TagFlagBullet => "[Bullet_skillController]";
        /// <summary>
        /// 目前 服务器和 策划 所定义的 子弹,其实是 客户端定义的 召唤物SummonEntity.
        /// 相应的,一个 BulletSummon(子弹召唤物) 只有一个 skillBullet 运行时.
        /// skillBullet 与 skillEntity 同一层级
        /// </summary>
        private SkillBullet skillBullet;

        private void EnterFrameBullet()
        {
            if (skillBullet != null)
            {
                skillBullet.EnterFrame();
            }
        }

        public void OnBulletCreateRet(BulletCreateRet bulletCreateRet)
        {
            if (skillBullet != null && bulletCreateRet.RuntimeID == skillBullet.RuntimeID)
            {
                return;
            }
            //DB_Close       SGF.Debuger.Log($"{TagFlagBullet} [server]  OnBulletCreateRet runtimeID {bulletCreateRet.RuntimeID}  ");
            skillBullet = EntityFactory.InstanceEntity<SkillBullet>();
            RegisterBulletAction(skillBullet);
            skillBullet.Create(bulletCreateRet);
        }

        /// <summary>
        /// 注册 skillBullet 对于的事件
        /// </summary>
        /// <param name="skillBullet"></param>
        private void RegisterBulletAction(SkillBullet skillBullet)
        {
            skillBullet.ActionOnPlayAnim = OnActionPlayAnim;
            skillBullet.ActionOnPlayFx = OnActionPlayFx;
            skillBullet.ActionOnPlayAudio = OnActionPlayAudio;      //目前假设音频一个事件对应多个音频的制作方式

            skillBullet.FuncOnTryPlayClientEffect = StageTryPlayClientEffect;
            skillBullet.FuncOnTryPlayServerEffect = OnFuncStageTryPlayServerEffect;
            skillBullet.ActionOnStopFx = OnActionStopFx;

            skillBullet.ActionOnBulletOffectY = OnActionOnBulletOffectY;
            skillBullet.ActionOnHidden = OnActionHidden;
        }

        public void OnBulletEndRet(BulletEndRet bulletEndRet)
        {
            //SGF.Debuger.LogError($"{TagFlagBullet} [server]  OnBulletEndRet runtimeID {bulletEndRet.RuntimeID}  ");

            if (skillBullet == null)
            {
                return;
            }
            skillBullet.OnBulletEndRet(bulletEndRet);
            ReleaseBullet();
        }

        public void OnBulletCreatCusBlackBoard(BaseBlackBoard baseBlackBoard)
        {
            if (skillBullet == null)
            {
                return;
            }
            skillBullet.OnBulletCreatCusBlackBoard(baseBlackBoard);

        }

        private void OnBulletRunStage(RunStageRet runStageRet)
        {

            ulong runtimeID = runStageRet.RuntimeID;
            // string stageIdStr = SkillStage.FormatStageIDStr(runStageRet.StageID, runStageRet.StageLoop);
            // SGF.Debuger.LogError($"{TagFlagBullet} [server]  OnBulletRunStage runtimeID {runtimeID} , stageIdStr: [{stageIdStr}]");
            //for (int i = 0; i < runStageRet.BlackList.Count; i++)
            //{
            //    SGF.Debuger.LogError($"{TagFlagBullet} [server]  OnBulletRunStage runtimeID {runtimeID} , stageIdStr: [{stageIdStr}] key : [{runStageRet.BlackList[i].Key}] ");
            //}

            if (skillBullet == null)
            {
                //SGF.Debuger.LogError($"{TagFlagBullet} [server] !!!!!!!!!!!! OnBulletRunStage runtimeID {runtimeID} , stageIdStr: [{stageIdStr}]  not find bullet ,error!!!");

                for (int i = 0; i < runStageRet.BlackList.Count; i++)
                {
                    //SGF.Debuger.LogError($"{TagFlagBullet} [server]  OnBulletRunStage runtimeID {runtimeID} , stageIdStr: [{stageIdStr}] key : [{runStageRet.BlackList[i].Key}] , not find bullet ,error!!!");
                }
                return;
            }
            if (skillBullet.RuntimeID != runtimeID)
            {
                return;
            }
            skillBullet.OnRunStageRet(runStageRet);
        }

        public void OnBulletRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            //DB_Close       SGF.Debuger.Log($"{TagFlagBullet} [server]  OnBulletRuntimeSync runtimeID {runtimeSyncRet.RuntimeID}  ");

            ulong runtimeID = runtimeSyncRet.RuntimeID;
            if (skillBullet == null)
            {
                SGF.Debuger.LogError($"{TagFlagBullet} 子弹 RuntimeSyncRet {runtimeID} 不存在这个子弹实体, 遇到喊 孔磊大爷!!! error!!!");
#if (UNITY_EDITOR)
                {
                    // Debug.Break();
                }
#endif
                return;
            }

            if (skillBullet.RuntimeID != runtimeID)
            {
                return;
            }
            skillBullet.OnBuffRuntimeSync(runtimeSyncRet);
        }

        private void ReleaseBulletAction()
        {

        }

        private void ReleaseBullet()
        {
            EntityFactory.ReleaseEntity(skillBullet);
            skillBullet = null;
        }

        private void ResetBullet()
        {
            ReleaseBullet();
        }


    }
}
