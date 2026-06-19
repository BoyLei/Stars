using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Network;
using UnityEngine;

public static class SkillMsgUtils
{
    public static PreSkillUseInputReq preSkillUseInputReq = new PreSkillUseInputReq();
    /// <summary>
    /// 发送预输入 操作 的协议
    /// </summary>
    /// <param name="runtimeID"></param>
    /// <param name="skillUseReq"></param>
    public static void SendPreSkillUseInput(ulong runtimeID, SkillUseReq skillUseReq, int inputEffectID)
    {
        if (skillUseReq == null)
        {
            //DB_Close    SGF.Debuger.LogError($"发送 技能 预输入操作 SendPreSkillUseInput  ----发发发发发----- runtimeID: {runtimeID} 但是没有 skillUseReq ");
            return;
        }
        preSkillUseInputReq.SkillID = skillUseReq.SkillID;
        preSkillUseInputReq.RuntimeID = runtimeID;
        preSkillUseInputReq.Pos = skillUseReq.Pos;
        // 施法者朝向
        preSkillUseInputReq.Rot = skillUseReq.Rot;
        preSkillUseInputReq.LockTargetID = skillUseReq.LockTargetID;
        preSkillUseInputReq.EffectID = inputEffectID;
        preSkillUseInputReq.BlackList.Clear();

        // 如果技能预输入黑板中有朝向, 那就是 告诉 服务器 技能 预输入的朝向
        preSkillUseInputReq.BlackList.Add(skillUseReq.BlackList.Clone());
        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, preSkillUseInputReq, false);
        //SGF.Debuger.LogError($"发送 技能 预输入操作 SendPreSkillUseInput  ----发发发发发----- runtimeID: {runtimeID} Rot={skillUseReq.Rot}");

        LogUtils.LogError(LogUtils.LogEnum.Skill, $"[SkillMsgUtils] 发送预输入操作 SendPreSkillUseInput skillID {skillUseReq.SkillID} runtimeID {runtimeID} ");

        //SGF.Debuger.LogError($"SkillMsgUtils 技能 [send] 预输入操作 SendPreSkillUseInput  preSkillUseInputReq {preSkillUseInputReq}");

    }


}
