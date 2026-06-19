using System.Collections.Generic;
using System.Diagnostics;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.Time;
using StarProjectDef;

namespace StarProject.Module
{
    /// <summary>
    ///  游戏逻辑
    /// </summary>
	public class StarWorldSocketMsg
    {
        private string LOG_TAG = "[SocketConnectMsg]";

        private SocketBase M_GameSocket;

        private static StarWorldSocketMsg instance;
        public static void Start()
        {
            if (instance == null)
            {
                instance = new();

                instance.Init();
            }
            instance.ConnectSocket();
        }

        private void Init()
        {
            if (M_GameSocket != null)
            {
                return;
            }
            M_GameSocket = NetworkManager.Instance.gameSocket;

            M_GameSocket.OnSocketClose += OnSocketClose;
            // M_GameSocket.OnSendMsgAction += OnSendMsgAction;
            M_GameSocket.OnSocketViewStateChange += OnSocketViewStateChange;

            //先Bind，Custom，PB
            BindStarWorldMsg();
        }

        private void BindStarWorldMsg()
        {
            //只有登录成功才会得到主角Ntt消息
            NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.ClientVerifySucceedRet, OnClientVerifySucceedRetMsgID, null);

            // 玩家重复登录的消息
            NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.UserDuplicateLoginNotify, OnUserDuplicateLoginNotifyMsgID, null);

            //玩家被顶号发送通知
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TickNtfID, OnTickNtfMsg, this);

        }

        /// <summary>
        /// 连接 socket
        /// </summary>
        private void ConnectSocket()
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} 连接SOCKET CreateSocketNSendMsg");

            // ClientVerifyReq clientVerifyReq = new();
            // clientVerifyReq.Source = 0; //  Source: 消息来源, 分客户端或者服务器(ClientMSG/服务器类型)，客户端填0
            // clientVerifyReq.PID = GameLoginInfo.M_ChooseHeroReq.PID;
            // clientVerifyReq.Token = GameLoginInfo.M_ChossHeroAck.Token;
            // clientVerifyReq.SessState = 1;  // SessState: 客户端登录时需要传送当前状态 1 新链接 2 断线重连
            // // 先发送 验证消息, 这样不管是 正常连接 还是 重连, 都是先将消息放入 未发送队列, 从而保证 验证消息永远第一条
            // M_GameSocket.SetSocketVerifyMsg((int)CustomMsgID.ClientVerifyReq, clientVerifyReq, false);

            //Lobby链接地址
            string lobbyAddr = GameLoginInfo.M_ChossHeroAck.LobbyAddr;
            // 服务器 lobby 发过来的是 域名:地址,  下面的解析，在 dns解析出 ipv6 的情况下,会 在connect 中产生异常
            {
                // string[] split = GameLoginInfo.M_ChossHeroAck.LobbyAddr.Split(":");
                // if (split.Length > 0)
                // {
                //     string domainName = split[0];
                //     if (Fire.Utils.isDomin(domainName))
                //     {
                //         lobbyAddr = string.Concat(Fire.Utils.GetIp(domainName), ":", split[1]);
                //     }
                // }

            }
            SGF.Debuger.LogWarning($"{LOG_TAG} 连接SOCKET CreateSocketNSendMsg lobbyAddr={lobbyAddr}");
            // M_GameSocket.Connect(lobbyAddr);
            M_GameSocket.Start(lobbyAddr, GameLoginInfo.M_ChooseHeroReq.PID, GameLoginInfo.M_ChossHeroAck.Token);
            // SGF.Debuger.LogWarning($"{LOG_TAG} 连接SOCKET CreateSocketNSendMsg end lobbyAddr={lobbyAddr}");
        }

        #region 自定义协议返回

        private void OnClientVerifySucceedRetMsgID(MessageHandleData data)
        {
            // 连接成功-》创建
            ModuleManager.Instance.CreateModule(ModuleDef.Name.StarWorldModule);         //世界战斗
            GameManager.Instance.IsLockGameMsgHandle = true;
            ModuleManager.Instance.ShowModule(ModuleDef.Name.StarWorldModule);           //启动界面
        }

        // (跟服务器一起看过了，不用了)
        //private  void OnBattleClientVerifyRetID(MessageHandleData data)
        //{
        //    SGF.Debuger.LogWarning($"{LOG_TAG} 连接SOCKET OnBattleClientVerifyRetID : {data.socketName}");
        //}



        /// <summary>
        /// 同账号UID并且同角色PID重复登录，被服务器断开，客户端返回登录界面
        /// </summary>
        /// <param name="data"></param>
        private void OnUserDuplicateLoginNotifyMsgID(MessageHandleData data)
        {
            //UserDuplicateLoginNotify clientVerifySucceedRet = (UserDuplicateLoginNotify)data.data;
            SGF.Debuger.LogWarning($"{LOG_TAG} 连接SOCKET OnUserDuplicateLoginNotifyMsgID 同账号UID并且同角色PID重复登录，被服务器断开，客户端返回登录界面, 准备 断开网络");

            M_GameSocket.End();
            OnDuplicateLogin(LanguageManager.Instance.GetLanguageByKey("TopNotice2"));
        }

        #endregion

        #region Socket网络状态变化回调
        private void OnSocketViewStateChange(SocketViewState socketViewState)
        {
            switch (socketViewState)
            {
                case SocketViewState.None:
                    {
                        // 正常连接状态, 需要清空所有 网络状态菊花/弹窗
                        GameManager.Instance.CloseNsgLoading();
                        GameManager.Instance.CloseMsgBox();
                    }
                    break;
                case SocketViewState.Connecting:
                    {
                        GameManager.Instance.CloseMsgBox();
                        // 弱网状态,显示菊花
                        GameManager.Instance.ShowMsgLoading(LoadingWidgetTypeEnum.WeakConnect1);
                    }
                    break;
                case SocketViewState.PingReconnectBox:
                    {
                        GameManager.Instance.CloseNsgLoading();

                        // 重连弹窗
                        ShowPingReLoginMsgBox();
                    }
                    break;
                case SocketViewState.ReconnectBox:
                    {
                        GameManager.Instance.CloseNsgLoading();

                        // 重连弹窗
                        ShowReLoginMsgBox(true);
                    }
                    break;
                case SocketViewState.RetrunLoginBox:
                    {
                        GameManager.Instance.CloseNsgLoading();

                        // 返回登录弹窗
                        ShowReLoginMsgBox(false);
                    }
                    break;

                default: break;
            }
        }

        private void ShowPingReLoginMsgBox()
        {
            GameManager.Instance.ShowReLoginMsgBox(true,
                (string v) =>
                {
                    SGF.Debuger.Log($"ShowPingReLoginMsgBox :  isPingMsgBox: {true}, v: {v}");
                    //返回登录，
                    if (v == "CANCLE")
                    {
                        OnClickReLogin();
                    }
                    else if (v == "SURE")
                    {
                        SGF.Debuger.Log($"ShowReLoginMsgBox : ReStartPing");

                        M_GameSocket.ReStartPing();

                    }
                }, true);
        }

        private void ShowReLoginMsgBox(bool reConnect)
        {
            GameManager.Instance.ShowReLoginMsgBox(reConnect,
                (string v) =>
                {
                    SGF.Debuger.Log($"ShowReLoginMsgBox : reConnect {reConnect} , v: {v}");
                    if (reConnect)
                    {
                        //返回登录，
                        if (v == "CANCLE")
                        {
                            OnClickReLogin();
                        }
                        else if (v == "SURE")
                        {
                            SGF.Debuger.Log($"ShowReLoginMsgBox : ReStart");
                            // 继续尝试
                            M_GameSocket.ReStart();

                        }
                    }
                    else
                    {
                        //返回登录，

                        OnClickReLogin();
                    }
                }, false);
        }

        /// <summary>
        /// 点击重新登录
        /// </summary>
        /// <param name="arg"></param>
        private void OnClickReLogin()
        {
            SGF.Debuger.Log($"ShowReLoginMsgBox : OnClickReLogin");

            UIManager.Instance.ReLogin(AgainLoginType.AgainLogin);
        }

        /// <summary>
        /// 关闭Socket
        /// </summary>
        /// <param name="forceClose"></param>
        private void OnSocketClose(bool forceClose)
        {
            // socket 不管是主动关闭还是被动关闭,都清除 超时等待消息的压花
            GameManager.Instance.ClearMsgRetListen();
        }

        /// <summary>
        /// 发送 cmd 结果的回调
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="result"></param> 
        private void OnSendMsgAction(int cmd, bool result)
        {
            if (!result)
            {
                GameManager.Instance.OnSendMsgError(cmd);
            }
        }

        #endregion

        #region 玩家被顶号发送通知

        private void OnTickNtfMsg(MessageHandleData data)
        {
            ProtoMsg.TickNtf tickNtf = (ProtoMsg.TickNtf)data.data;

            StarProjectDef.RetMsgDataCell retM =
                StarProject.Service.LocalData.LocalDataManager.Instance.GetRetMsgDataCell(tickNtf.RetCode);
            string msgtext;
            int msgtype = 2;
            if (retM == null)
            {
                msgtext = string.Format("顶号通知 未知错误码：{1}", tickNtf.RetCode);
            }
            else
            {
                msgtype = retM.GetMsgType();
                if (retM.ClientStr != "")
                {
                    List<string> li = new();
                    foreach (var item in tickNtf.Params)
                    {
                        switch (item.ParamValueCase)
                        {
                            case RespParam.ParamValueOneofCase.None:
                                break;
                            case RespParam.ParamValueOneofCase.Int32Value:
                                li.Add(item.Int32Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Uint32Value:
                                li.Add(item.Uint32Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Int64Value:
                                li.Add(item.Int64Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Uint64Value:
                                li.Add(item.Uint64Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.FloatValue:
                                li.Add(item.FloatValue.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.DoubleValue:
                                li.Add(item.DoubleValue.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.StringValue:
                                li.Add(item.StringValue);
                                break;
                            default:
                                break;
                        }
                    }

                    //string[] param = string.Format(retM.ParamsKey, li.ToArray()).Split(',');
                    //msgtext = string.Format(retM.ClientStr, param);
                    //msgtext = RichTextUtils.ParseMailText(retM.ClientStr, string.Format(retM.ParamsKey, li.ToArray()));
                    msgtext = RichTextUtils.ParseRichText(retM.ClientStr, li);
                }
                else
                {
                    msgtext = retM.Id.ToString();
                }
            }

            UnityEngine.Debug.Log($"[socket] 被服务器踢下线 : {retM.ToString()}, 准备关闭 网络");

            M_GameSocket.End();

            switch (retM.EnumName)
            {
                case "User_Not_Adult":
                case "User_Game_Time_End":
                    {
                        UnityEngine.Debug.Log($"[SDK] 未成年 游戏时常结束被服务器踢下线, 客户端切换 login 弹出 确认弹窗 ");
                        UnityEngine.Debug.Log($"[SDK] 未成年 , 准备登出sdk");
                        // 登出sdk
                        Service.SDK.SDKManager.Instance.LoginOut();

                        // 防成谜弹出, 点击确认 会关闭游戏
                        UIAPI.ShowMsgBox(44, (string v) =>
                        {
                            // 返回登录module
                            UIManager.Instance.ReLogin(AgainLoginType.AgainLogin);
                        }, new object[] { });
                    }
                    break;
                default:
                    {
                        OnDuplicateLogin(msgtext);
                    }
                    break;
            }
            //   SGF.Debuger.LogError($"{LOG_TAG} OnTickNtfMsg() 顶号通知");
            //Frame.Util.ShowSystemMessage("顶号通知");
        }

        private void OnTickCB(string arg)
        {
            //if ((int)arg == 0)
            {

                // 返回登录module
                UIManager.Instance.ReLogin(AgainLoginType.AgainLogin);
            }
        }

        public void OnDuplicateLogin(string msgtext)
        {
            //UIAPI.ShowMsgBox(GameConfig.LocalStr["TopNotice"], msgtext, GameConfig.LocalStr["BtnSure"], OnTickCB);
            UIAPI.ShowMsgBox(LanguageManager.Instance.GetLanguageByKey("TopNotice"), msgtext, LanguageManager.Instance.GetLanguageByKey("BtnSure"), OnTickCB, MsgBoxRootType.System);
        }

        #endregion
    }
}
