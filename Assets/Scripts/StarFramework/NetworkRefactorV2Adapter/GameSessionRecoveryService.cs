using System;
using System.Threading;
using System.Threading.Tasks;
using StarProjectDef;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// 重连前 HTTP 重新选择角色（ChooseHeroHandler）的请求委托。注入它可在测试中用 Fake 替代真实 HTTP，
    /// 覆盖“HTTP 重新登录 -> 拿到新 Token -> 构造重连会话”的集成逻辑，而无需触网。
    /// </summary>
    public delegate void ChooseHeroRequester(ChooseHeroReq req, Action<ChossHeroAck, bool> onCompleted);

    /// <summary>
    /// 会话恢复服务：实现 V2 的 ISessionRecoveryService，对应 NetworkUiPolicy.md 中
    /// RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect 的设计意图——在重连前通过
    /// HTTP 重新登录（ChooseHeroHandler）拿到新的 Token，而不是像当前旧网络层那样复用旧 Token。
    /// ConnectionRecoveryCoordinator 在 RecoveryPolicy == RefreshSessionBeforeReconnect 时会回调本服务。
    /// </summary>
    public sealed class GameSessionRecoveryService : ISessionRecoveryService
    {
        private readonly ChooseHeroRequester _chooseHero;

        /// <summary>
        /// 默认用真实 HTTP（HttpUtils.SendRetryLoginHeaderHttpReq）。传入自定义委托即可在测试中替换为 Fake。
        /// </summary>
        public GameSessionRecoveryService(ChooseHeroRequester chooseHero = null)
        {
            _chooseHero = chooseHero ?? DefaultChooseHero;
        }

        internal static void DefaultChooseHero(ChooseHeroReq req, Action<ChossHeroAck, bool> onCompleted)
        {
            HttpUtils.SendRetryLoginHeaderHttpReq<ChooseHeroReq, ChossHeroAck>(
                GameLoginInfo.GameServerIpAddress,
                HttpUtils.CHOOSE_HERO_HANDLER,
                req,
                onCompleted);
        }

        public Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ReconnectSessionData>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 重连前先通过 HTTP 重新选择角色，刷新 Token（与旧网络注释掉的 ChooseHeroHandler 逻辑一致）
            _chooseHero(GameLoginInfo.M_ChooseHeroReq,
                (ack, ok) =>
                {
                    if (!ok || ack == null || ack.Result > 1)
                    {
                        tcs.TrySetException(new InvalidOperationException($"[V2Adapter] 会话恢复 HTTP 重新登录失败 (socket={socketName}, ok={ok}, result={ack?.Result})"));
                        return;
                    }

                    // 写回新的 Token，供后续验证包使用
                    GameLoginInfo.M_ChossHeroAck = ack;

                    var session = new ReconnectSessionData(
                        GameLoginInfo.GameServerIpAddress,
                        GameLoginInfo.M_ChooseHeroReq.PID,
                        ack.Token,
                        true);
                    tcs.TrySetResult(session);
                });

            // 不用 using：注册需在任务生存期内保持有效，否则 return 后立即 Dispose，取消信号永远传不到 tcs
            cancellationToken.Register(() => tcs.TrySetCanceled());
            return tcs.Task;
        }
    }
}
