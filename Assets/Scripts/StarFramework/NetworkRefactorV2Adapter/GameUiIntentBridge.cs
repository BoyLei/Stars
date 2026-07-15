using SGF.Network;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// UI 意图桥接：把 V2 的 ConnectionUiIntent（策略层产出）映射到旧网络的 SocketViewState（UI 表现层）。
    /// 映射关系与旧网络 Ping.SwitchPingState / SocketBase 状态机保持一致：
    ///   None                -> None
    ///   WeakLoading         -> Connecting        (弱网菊花)
    ///   FakeReconnectDialog -> PingReconnectBox  (假重连弹窗，不断 socket)
    ///   RealReconnectDialog -> ReconnectBox      (真重连弹窗)
    ///   ReturnLoginDialog   -> RetrunLoginBox    (返回登录弹窗)
    /// 该桥接仅为映射工具，未来由 AppMain 把 SocketViewStateChanged 接到 StarWorldSocketMsg。
    /// </summary>
    public static class GameUiIntentBridge
    {
        public static SocketViewState ToSocketViewState(ConnectionUiIntent intent)
        {
            switch (intent)
            {
                case ConnectionUiIntent.WeakLoading: return SocketViewState.Connecting;
                case ConnectionUiIntent.FakeReconnectDialog: return SocketViewState.PingReconnectBox;
                case ConnectionUiIntent.RealReconnectDialog: return SocketViewState.ReconnectBox;
                case ConnectionUiIntent.ReconnectRetryDialog: return SocketViewState.ReconnectBox;
                case ConnectionUiIntent.ReturnLoginDialog: return SocketViewState.RetrunLoginBox;
                case ConnectionUiIntent.None:
                default: return SocketViewState.None;
            }
        }
    }
}
