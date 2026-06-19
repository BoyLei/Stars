using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    public static class EffectResultUtils
    {
        /// <summary>
        /// 更新 服务器 黑板的 result, 默认服务器效果的result为true(只要有数据就表示效果执行了)
        /// 而对于预输入 这种效果，存在超时逻辑, 超时服务器会返回 false, 所以 对于预输入 这种类型的
        /// 效果, 需要单独处理 它们的result
        /// </summary>
        public static void UpdateServerEffectResult(I_EffectParam effectParam, CustomBlackBoardNode serverCustomBoardNode)
        {
            // 目前只有预输入轴效果 需要做效果的延迟处理,存在跟服务器效果不一致的情况
            // 对于不一致的情况,才需要做效果检查处理,其它情况,默认返回true
            serverCustomBoardNode.OutputResult = true;

            // note:
            // 后续有其它情况再一个个效果加

            // 用户输入轴的效果
            if (effectParam.SkillEffectType == E_SkillEffect.UserInput || effectParam.SkillEffectType == E_SkillEffect.Energy)
            {
                // string key = effectParam.EffectIDStr;
                /// 2023/4/25
                /// 服务器的 输入轴效果的 数据结构由 bool 改为 PreUserInputData
                PreUserInputData preUserInputData = (PreUserInputData)serverCustomBoardNode.Value;
                serverCustomBoardNode.OutputResult = preUserInputData.IsExcute;
            }
        }

        public static EffectExecuteResult GetEffectExecuteResult(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam)
        {
            string key = i_EffectParam.EffectIDStr;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            object blackBoardExuecuteResult = baseBlackBoard.Get(key, E_BlackBoardTag.ExecuteResult);
            if (blackBoardExuecuteResult == null)
            {
                blackBoardExuecuteResult = new EffectExecuteResult();
            }

            EffectExecuteResult effectExecuteResult = (EffectExecuteResult)blackBoardExuecuteResult;

            return effectExecuteResult;
        }

        /// <summary>
        ///  取得 黑板中 的 效果的 执行数据, 如果 effeID 没有的时候， 返回的null. 否则如果黑板中没有,就在黑板中创建一份 默认的数据返回
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <returns></returns>
        private static EffectExecuteResult GetEnsureEffectExecuteResult(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam)
        {
            /// 2023/3/8
            ///     目前 的 效果监听 可以 由 inputKeys 或者 outputKey 共同触发, 如 效果 [1,2] , 1.outputKey = A; 2.outputKey = C, inputKeys =[A,B].
            ///     如上 , 对于 A 的 outputKey , 就同时 有效果 [1,2] 的监听.
            ///     所以 使用 outputKey 作为 执行 结果的 key 就不再 合适, 进而改为 用效果 EffectID 来处理。
            // string outPutKey = i_EffectParam.OutputKey;

            string key = i_EffectParam.EffectIDStr;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            EffectExecuteResult effectExecuteResult = EffectResultUtils.GetEffectExecuteResult(baseBlackBoard, i_EffectParam);
            baseBlackBoard.Set(key, effectExecuteResult, E_BlackBoardTag.ExecuteResult);

            return effectExecuteResult;
        }

        /// <summary>
        /// 更新 效果 执行的 结果(ExecuteResult) 
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isClient"></param>
        /// <param name="executeResult"></param>
        public static void UpdateEffectExecuteResult(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, bool isClient, bool executeResult)
        {
            EffectExecuteResult effectExecuteResult = GetEnsureEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return;
            }
            E_BlackBoardTag tag = isClient ? E_BlackBoardTag.Client : E_BlackBoardTag.Server;

            // 效果的执行 结果的状态, 此处是 通用的 客户端/服务器 线执行状态，会根据是否是 客户端，将状态设置为 ClientRun / ServerRun 
            E_EffectExecuteResultState effectExecuteResultState = isClient ? E_EffectExecuteResultState.ClientRun : E_EffectExecuteResultState.ServerRun;

            effectExecuteResult.UpdateExecuteResult(tag, executeResult);
            effectExecuteResult.UpdateExecuteResultState(tag, effectExecuteResultState);

        }

        /// <summary>
        /// 检查 效果 是否已经执行 成功的 ExecuteResult。
        /// note:
        ///     1.ExecuteResult 默认为 false, 所以 检查条件一般是检查 ExecuteResult == True 才有价值.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="tag"> client/server/Reg 黑板的标签 </param>
        public static bool CheckEffectExecuteResult(I_EffectParam effectParam, BaseBlackBoard baseBlackBoard, E_BlackBoardTag tag)
        {
            EffectExecuteResult effectExecuteResult = EffectResultUtils.GetEffectExecuteResult(baseBlackBoard, effectParam);
            if (effectExecuteResult == null)
            {
                return false;
            }
            return effectExecuteResult.GetTagExecuteResult(tag);
        }

        /// <summary>
        /// 同步 效果执行结果的 状态，客户端/服务器 效果 默认会同步 设置他们的状态为 ClientRun / ServerRun
        /// 单个效果 可以自己根据自己的 逻辑, update 效果的状态
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="tag"></param>
        /// <param name="effectExecuteResultState"></param>
        public static void UpdateEffectExecuteResultState(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, E_BlackBoardTag tag, E_EffectExecuteResultState effectExecuteResultState)
        {
            EffectExecuteResult effectExecuteResult = GetEnsureEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return;
            }

            effectExecuteResult.UpdateExecuteResultState(tag, effectExecuteResultState);
        }

        /// <summary>
        /// 检查 对应 tag 有没有 执行对应的状态
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="tag"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public static bool CheckEffectHasExecuteState(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, E_BlackBoardTag tag, E_EffectExecuteResultState checkState)
        {
            EffectExecuteResult effectExecuteResult = EffectResultUtils.GetEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return false;
            }
            return effectExecuteResult.CheckHasExecuteState(tag, checkState);
        }

        /// <summary>
        /// 得到对应 tag 的效果 的执行时间
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="tag"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public static long GetEffectExecuteTime(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, E_BlackBoardTag tag)
        {
            EffectExecuteResult effectExecuteResult = EffectResultUtils.GetEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return 0;
            }
            return effectExecuteResult.GetExecuteTime(tag);
        }


        /// <summary>
        /// 注册 是否时 等待服务器线 效果 来 注册 next 效果
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isNeedServerRegNext">是否需要服务器线注册 next效果</param>
        public static void RegIsWaitServerRegNext(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, bool isNeedServerRegNext)
        {
            EffectExecuteResult effectExecuteResult = GetEnsureEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return;
            }

            effectExecuteResult.RegIsWaitServerRegNext(isNeedServerRegNext);
        }

        /// <summary>
        /// 判断是否 注册了 等待服务器 效果 注册 Next
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <returns></returns>
        public static bool IsWaitServerRegNext(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam)
        {
            EffectExecuteResult effectExecuteResult = EffectResultUtils.GetEffectExecuteResult(baseBlackBoard, i_EffectParam);
            if (effectExecuteResult == null)
            {
                return false;
            }
            return effectExecuteResult.IsWaitServerRegNext();
        }

        /// <summary>
        /// 是否 需要 注册 Next 分支
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isNextTrue">是 NextTrue 分支还是NextFalse分支</param>
        /// <returns></returns>
        public static bool IsRegNext(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, bool isNextTrue)
        {
            // 首先判断 是否 需要等待服务器线 注册Next.
            bool needServerRegNext = IsWaitServerRegNext(baseBlackBoard, i_EffectParam);
            // 如果不需要服务器线注册Next, 那就是客户端线注册Next,那就直接返回True,都需要注册
            if (!needServerRegNext)
            {
                return true;
            }

            E_BlackBoardTag tag = E_BlackBoardTag.Server;

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                /// 本地服的黑板数据 都是存在客户端黑板中, 所以 检查是否需要注册next 效果的 黑板也是 客户端黑板
                tag = E_BlackBoardTag.Client;
            }
#endif

            // 如果需要 服务器 来注册 这个效果的 next, 那就需要检查 黑板中是否有这个 黑板数据
            object v = baseBlackBoard.Get(i_EffectParam.OutputKey.ToString(), tag);

            // 如果 找不到,那就返回false
            // (目前的设计,如果 这个效果是服务器 来 触发注册,那触发 next 效果分支的时候需要依赖这个服务器数据;
            // 如果是客户线触发,也就不需要 走这个流程
            if (v == null)
            {
                return false;
            }

            CustomBlackBoardNode customBlackBoardNode = (CustomBlackBoardNode)v;
            // 如果需要服务器线注册Next,就需要找到 这个服务器效果传过来的数据
            bool serverResult = (bool)customBlackBoardNode.Value;

            // 由服务器返回的结果 决定 是否NextTrue/NextFalse 分支
            return serverResult == isNextTrue;
        }



        public static bool HandleClientRegEffect(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            RegIsWaitServerRegNext(blackBoard, effectParam, true);
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleClientIsNotEmpty  RegIsWaitServerRegNext : true");
            return true;
        }

        public static bool HandleServerRegEffect(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            RegIsWaitServerRegNext(blackBoard, effectParam, true);
            if (!blackBoard.Contain(effectParam.OutputKey, E_BlackBoardTag.Server))
            {
                return false;
            }
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleServerIsNotEmpty RegIsWaitServerRegNext : true");
            return true;
        }
    }
}
