using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Time;

using ProtoMsg;
using SGF.Network;
using StarProjectDef;
using StarProject.Game.Player;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// SkillController 的 msg 部分
    /// </summary>
    public partial class SkillController
    {
        /// <summary>
        /// 预施法协议
        ///     note：
        ///         0.服务器只存在一个预施法技能
        ///         1.服务器收到预施法后，会缓存下来，在技能结束后，修改释放技能时间，然后才执行请求的技能
        ///         2.服务器认为预施法只适用于客户端认为技能结束，而服务器可能未结束的情况
        ///         3.由上，以及沟通之后的综合了解，如果客户端认为技能需要打断，然后释放预施法，
        ///             那客户端应该去区分技能打断是否发送了打断请求，如果实际发送了打断协议请求，
        ///             那其实只需要发送 普通的使用技能请求 (skillUseReq) 就可以了.
        ///             如果没有发送打断，那就发送(PreSkillUseReq);
        /// </summary>
        /// <param name="skillUseReq"></param>
        private void SendPreUseSkillReq(SkillUseReq skillUseReq)
        {
            //每次发送技能使用请求，都会修正技能请求的时间
            skillUseReq.RunTime = (ulong)TimeUtils.ServerNowStampMilli;


            if (playerData.EntityType == E_EntityType.Partner)
            {
                PartnerCtrlGroup partnerCtrlGroup = (PartnerCtrlGroup)playerData.myOwnerNttGroup;
                PartnerSkillUseReq partnerSkillUseReq = new PartnerSkillUseReq();
                partnerSkillUseReq.Index = (int)partnerCtrlGroup.GetCfgId();
                partnerSkillUseReq.Msg = skillUseReq;
                NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, partnerSkillUseReq, false);
                //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [client] [input] PartnerSkillUseReq SendRPCMsg skillID {skillUseReq.SkillID} , runtimeID {skillUseReq.RuntimeID}  ");


            }
            else
            {
                PreSkillUseReq preSkillUseReq = new PreSkillUseReq();
                preSkillUseReq.Msg = skillUseReq;
                NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, preSkillUseReq, false);
                //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 预输入技能 SendPreUseSkillReq skillUseReq {skillUseReq}  ");
            }
            //SGF.Debuger.LogError($"{TagFlag} 技能 [send] 自动转向 SendPreUseSkillReq  ----发发发发发----- skillUseReq {skillUseReq}");

        }

        private void SendUseSkillReq(SkillUseReq skillUseReq)
        {
            //每次发送技能使用请求，都会修正技能请求的时间
            skillUseReq.RunTime = (ulong)TimeUtils.ServerNowStampMilli;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, skillUseReq, false);
            //SGF.Debuger.LogError($"{TagFlag} 技能 [send] skillUseReq {skillUseReq}");

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag}  SendUseSkillReq SendRPCMsg skillID {skillUseReq.SkillID}  ");
        }

        /// <summary>
        /// 发送蓄力结束通知。
        /// 1.打断技能的时候，主动发；
        /// 2.主动结束蓄力发蓄力结束协议，蓄力结束不打断技能。
        /// </summary>
        /// <param name="runtimeID"></param>
        private void SendEnergyEndNotice(SkillEntity CurSkillEntity)
        {
            // SGF.Debuger.Log($"{TagFlag}  SendEnergyEndNotice  runTimeId {CurSkillEntity.runtimeID} serverTime {TimeUtils.ServerNowStampMilli}");

            //energyEndNotice.RuntimeID = CurSkillEntity.runtimeID;
            //ulong time = (ulong)TimeUtils.ServerNowStampMilli;

            //bool forceChangeStage = false;

            ////note:
            ////   如果蓄力技能，是前摇阶段就发送蓄力结束通知的话，这里要加一个endtime为1.技能创建时间+前摇时间+（蓄力阶段默认层数*蓄力阶段的时间）
            //if (CurSkillEntity.CurStage == E_ULayerSubState.Stage_SkillPro)
            //{
            //    // TODO DL
            //    // 修改 方式

            //    // SGF.Debuger.Log($"{TagFlag}  ClientBreakEnergySkill on Stage_SkillPro");
            //    // SkillDataCell cfg = CurSkillEntity.cfg;
            //    // time = CurSkillEntity.createTime + (ulong)CurSkillEntity.GetCurEnergyTime();

            //    forceChangeStage = true;
            //}

            //if (CurSkillEntity.CurStage == E_ULayerSubState.Stage_SkillEnergy)
            //{
            //    forceChangeStage = true;
            //}

            //energyEndNotice.EndTime = time;

            //if (forceChangeStage)
            //{
            //    CurSkillEntity.ClientBreakEnergyStage(energyEndNotice.EndTime);
            //}
            //NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, energyEndNotice, false);
        }

        private void SendSkillQuit(SkillEntity skillEntity)
        {
            ulong runtimeID = skillEntity.RuntimeID;
            ProtoMsg.SkillQuitReq skillQuitReq = new ProtoMsg.SkillQuitReq();
            skillQuitReq.RuntimeID = runtimeID;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, skillQuitReq, false);
            //SGF.Debuger.LogError($"{TagFlag} 技能 [send] 取消 SendSkillQuit {skillQuitReq}");

        }

        private PreSkillUseInputCancelReq preSkillUseInputCancelReq = new PreSkillUseInputCancelReq();
        private void sendPreSkillUseInputCancelReq(ulong runtimeID, int skillId)
        {
            preSkillUseInputCancelReq.SkillID = skillId;
            preSkillUseInputCancelReq.RuntimeID = runtimeID;

            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, preSkillUseInputCancelReq, false);
            //SGF.Debuger.LogError($"{TagFlag} 技能 [send] 预输入取消 runtimeID={runtimeID},,skillId={skillId}");

            StarDebug.LogTag(StarDebug.LogTagEnum.Skill, $"{TagFlag} 发送预输入取消 preSkillUseInputCancelReq skillID {skillId} runtimeID {runtimeID} ");
        }


    }
}
