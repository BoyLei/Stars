using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using UnityEngine;
using StarProjectDef;
namespace StarProject.Game.Skill
{
    /// <summary>
    /// 用户预输入的缓存
    /// </summary>
    public class SkillInputCache
    {
        public int SkillID;
        public SkillUseReq SkillUseReq;
        public ulong RuntimeID;

        public bool HasInputCache => RuntimeID != 0 && SkillUseReq != null;

        /// <summary>
        /// 使用技能的类型
        /// </summary>
        public E_UseSkillType UseSkillType;

        public bool SendUseSkill = false;

        public void UpdateInputCache(SkillUseReq skillUseReq, E_UseSkillType useSkillType, bool sendUseSkill)
        {
            SkillID = skillUseReq.SkillID;
            RuntimeID = skillUseReq.RuntimeID;
            SkillUseReq = skillUseReq;
            UseSkillType = useSkillType;
            SendUseSkill = sendUseSkill;
        }

        public void Reset()
        {
            SkillID = 0;
            RuntimeID = 0;
            SkillUseReq = null;
            SendUseSkill = false;
        }
    }

    public class ServerInputCache
    {
        public ulong RuntimeID;
        public I_EffectParam EffectParam;
        public CustomBlackBoardNode BlackBoardNode;

        public PreUserInputData preUserInputData;

        public bool IsExcute => preUserInputData == null ? false : preUserInputData.IsExcute;

        public void UpdateCache(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode)
        {
            RuntimeID = runtimeID;
            EffectParam = effectParam;
            BlackBoardNode = blackBoardNode;
            preUserInputData = (PreUserInputData)blackBoardNode.Value;
        }

        public void Reset()
        {
            RuntimeID = 0;
            EffectParam = null;
            BlackBoardNode = null;
            preUserInputData = null;
        }
    }

    public class ServerPreSkillUseInputResult
    {
        /// <summary>
        /// 输入轴 用户预输入 服务器返回的 结果. 只有 服务器返回了一次成功,客户端 预报的时候,才会用本地数据预播
        /// </summary>
        public bool InputResult = false;
        PreSkillUseInputRet _preSkillUseInputRet;

        public void UpdateData(PreSkillUseInputRet preSkillUseInputRet)
        {
            if (_preSkillUseInputRet != null)
            {
                // 预输入操作的技能runtimeID 发生变化 或者 预输入操作的 输入轴 发生了变化, 那旧 的输入轴的 InputResult 为 默认false
                if (_preSkillUseInputRet.RuntimeID != preSkillUseInputRet.RuntimeID || _preSkillUseInputRet.EffectID != preSkillUseInputRet.EffectID)
                {
                    Reset();
                }
            }


            _preSkillUseInputRet = preSkillUseInputRet;

            InputResult |= preSkillUseInputRet.Ret == PreSkillUseInputResult.PreInputResultDefault;

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"[预输入] 预输入操作返回: {preSkillUseInputRet.Ret}, InputResult: {InputResult}");

        }

        public bool CheckUserInputResult(ulong runtimeID, EffectParam inputEffect)
        {
            if (_preSkillUseInputRet != null)
            {
                if (inputEffect.EffectID == _preSkillUseInputRet.EffectID && runtimeID == _preSkillUseInputRet.RuntimeID)
                {
                    return InputResult;
                }
            }


            return false;
        }

        public void Reset()
        {
            InputResult = false;
            _preSkillUseInputRet = null;
        }
    }
}
