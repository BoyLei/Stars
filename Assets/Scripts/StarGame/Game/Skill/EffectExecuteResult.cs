using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Skill;
using StarProjectDef;
using UnityEngine;
namespace StarProject.Game.Skill
{
    /// <summary>
    /// 效果的执行结果数据, 由 客户端线 和 服务器线 共同产生和 维护 
    /// 某个效果 执行的结果.
    /// eg:
    ///     比如在某个点 播放特效 , 目前是由 客户端和服务器 共同触发这个效果的播放,
    ///     但只要 客户端/服务器 一方 执行了这个效果,另一方就不执行。
    ///     对于这种情况, 只需要将效果 的执行结果 存放在 黑板中, 服务器客户端共同维护即可。
    ///     
    ///     同时，这种 前后端共同维护的数据,单独提出来, 而不是放在 客户端/服务器的 CustomBlackBoardNode 结构中,
    ///     减少没必要的 实例内存消耗。
    /// </summary>
    public class EffectExecuteResult
    {
        /// <summary>
        /// 效果 执行的 结果, 比如某个客户端效果, 效果的执行依赖 对应的 key，如果存在key,返回执行result为true,否则为false
        /// 此时, ExecuteResult 就是 这个效果执行返回的结果.
        /// 对于 有些 由 客户端和服务器 共同触发且只需要执行一次的效果,
        /// 这个 结果能够 记录 客户端 和服务器 是否 执行过这个效果.
        /// 如果执行过, 那 对应的 效果 就不需要再次执行
        /// </summary>
        public bool ExecuteResult => GetTagExecuteResult(E_BlackBoardTag.Client) || GetTagExecuteResult(E_BlackBoardTag.Server);

        /// <summary>
        /// 不同 标签 tag 的效果 执行的 结果
        /// </summary>
        /// <typeparam name="E_BlackBoardTag"></typeparam>
        /// <typeparam name="EffectExecuteResultData"></typeparam>
        /// <returns></returns>
        private Dictionary<E_BlackBoardTag, EffectExecuteResultData> tagExecuteResult = new Dictionary<E_BlackBoardTag, EffectExecuteResultData>();

        /// <summary>
        /// 2022/12/23
        /// 新增一个 等待 服务器线效果执行,才触发注册 next 效果的效果实现
        /// 
        /// 是否等待服务器线 注册 next 效果.
        /// note:
        ///     1.目前 策划需要有 一些判断效果, 判断效果 由服务器 线 触发,
        ///       客户端执行到这个效果的时候,这个效果的 串行效果线 暂停,
        ///       不继续执行它的next, 而是等到 服务器线这条效果 来的时候,
        ///       再去 继续执行它的 Next 串行效果;
        ///     
        ///     2.基于上面的需求,再 每个效果的 执行结果中(EffectExecuteResult),
        ///       记录一个 是否等待 服务器 线 注册 next 效果 的变量(waitServerRegNextEffect)，
        ///       当执行 到一个效果 需要等待服务器 线注册时, 先让客户端线 
        ///       先执行(客户端线线执行,来注册这个效果的服务器线),同时,标记这个效果 需要等待服务器
        ///       线 来触发 next, 客户端线 不 提前注册next.
        ///       当等到 服务器线 触发这个效果的时候, 检查这个变量,如果有,先执行 服务器线,
        ///       同时, 由服务器线提前 注册 这个效果的 next.
        /// </summary>
        private bool waitServerRegNextEffect = false;


        /// <summary>
        /// 获取 tag 对应的 执行 数据, 如果tag 的数据不存在，就创建一个 默认的tag 数据,
        /// note:
        ///     一般只有 update 数据的时候，才需要调用这个接口. 其它的检查接口, 都是调用  ContainsKey 检查有没有
        /// </summary>
        /// <param name="tag"></param>
        private EffectExecuteResultData GetEnsureEffectExecuteResultData(E_BlackBoardTag tag)
        {
            EffectExecuteResultData data = null;
            if (!tagExecuteResult.ContainsKey(tag))
            {
                data = new EffectExecuteResultData(tag);
                tagExecuteResult.Add(tag, data);
            }
            data = tagExecuteResult[tag];
            return data;
        }

        public void UpdateExecuteResult(E_BlackBoardTag tag, bool result)
        {
            EffectExecuteResultData data = GetEnsureEffectExecuteResultData(tag);

            data.UpdateExecuteResult(result);
        }

        public bool GetTagExecuteResult(E_BlackBoardTag tag)
        {
            if (tagExecuteResult.ContainsKey(tag))
            {
                return tagExecuteResult[tag].ExecuteResult;
            }
            return false;
        }

        /// <summary>
        /// 更新 tag 标签对应的 效果执行结果 的状态
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="state"></param>
        /// <param name="forceSet">是否 是强制设置状态, 如果是强制设置状态, 状态设置 使用 = 设置，否者 使用 |= 的方式设置 </param>
        public void UpdateExecuteResultState(E_BlackBoardTag tag, E_EffectExecuteResultState state, bool forceSet = false)
        {
            EffectExecuteResultData data = GetEnsureEffectExecuteResultData(tag);

            if (forceSet)
            {
                data.State = state;
            }
            else
            {
                data.State |= state;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CheckHasExecuteState(E_BlackBoardTag tag, E_EffectExecuteResultState state)
        {
            EffectExecuteResultData data = GetEnsureEffectExecuteResultData(tag);

            return data.CheckHasExecuteState(state);
        }

        /// <summary>
        /// 得到 tag 标签 对应的效果 执行的 时间
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public long GetExecuteTime(E_BlackBoardTag tag)
        {
            if (tagExecuteResult.ContainsKey(tag))
            {
                return tagExecuteResult[tag].ExecuteTime;
            }
            return 0;
        }

        /// <summary>
        /// 注册 是否时 等待服务器线 效果 来 注册 next 效果
        /// </summary>
        /// <param name="isServerRegNext"></param>
        public void RegIsWaitServerRegNext(bool isServerRegNext)
        {
            waitServerRegNextEffect = isServerRegNext;
        }

        public bool IsWaitServerRegNext()
        {
            return waitServerRegNextEffect;
        }
    }

    /// <summary>
    /// 执行结果数据,目前保存了 它的执行结果/ 执行的时间等
    /// </summary>
    public class EffectExecuteResultData
    {
        public E_BlackBoardTag Tag;
        /// <summary>
        /// 执行的时间戳 (ms)
        /// </summary>
        public long ExecuteTime;
        /// <summary>
        /// 执行 是成功还是 失败的结果
        /// </summary>
        public bool ExecuteResult;
        /// <summary>
        /// 效果执行的状态,详情 看枚举注释
        /// </summary>
        public E_EffectExecuteResultState State;

        public EffectExecuteResultData(E_BlackBoardTag tag)
        {
            Tag = tag;
        }

        public void UpdateExecuteResult(bool executeResult)
        {
            ExecuteResult = executeResult;
            ExecuteTime = SGF.Time.TimeUtils.ClientNowStampMilli;
        }

        public bool CheckHasExecuteState(E_EffectExecuteResultState state)
        {
            return (State & state) == state;
        }
    }
}
