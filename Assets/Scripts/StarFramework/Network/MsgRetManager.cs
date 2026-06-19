using SGF.Module.Framework;
using StarProject.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;

namespace SGF.Network
{


    /// <summary>
    /// 简单消息回复处理管理器
    /// </summary>
    [XLua.LuaCallCSharp]
    public partial class MsgRetManager : ServiceModule<MsgRetManager>
    {
        /// <summary>
        /// 事件处理表
        /// </summary>
        private Dictionary<int, MsgRetDelegate> MsgIDToEvents = new Dictionary<int, MsgRetDelegate>();

        public void Init()
        {

        }

        public void Close()
        {

        }

        public override void Release()
        {
            base.Release();
            this.Log("Release() MsgRet Manager");

        }




        /// <summary>
        /// 注册回调事件
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="msgids">消息号列表</param>
        // public void OnMessage(MsgRetDelegate callback, params int[] msgids)
        // {
        //     foreach (var msgid in msgids)
        //     {

        //         if (MsgIDToEvents.ContainsKey(msgid))
        //         {
        //             MsgIDToEvents[msgid] += callback;
        //         }
        //         else
        //         {
        //             MsgIDToEvents.Add(msgid, callback);
        //         }

        //         /*
        //         if (this.MsgIDToEvents.TryGetValue(msgid, out var eventf))
        //         {
        //             eventf += callback;
        //         }
        //         else
        //         {
        //             this.MsgIDToEvents[msgid] = callback;
        //         }*/
        //     }
        // }

        public void OnMessageEnum(MsgRetDelegate callback, params MsgIDEnum[] msgids)
        {
            foreach (var msgIDEnum in msgids)
            {
                var msgid = (int)msgIDEnum;

                if (MsgIDToEvents.ContainsKey(msgid))
                {
                    MsgIDToEvents[msgid] += callback;
                }
                else
                {
                    MsgIDToEvents.Add(msgid, callback);
                }


            }
        }



        public void OffMessageEnum(MsgRetDelegate callback, params MsgIDEnum[] msgIDEnums)
        {
            foreach (var item in msgIDEnums)
            {

                var msgID = (int)item;

                if (MsgIDToEvents.ContainsKey(msgID))
                {
                    MsgIDToEvents[msgID] -= callback;
                    if (MsgIDToEvents[msgID] == null)
                    {
                        MsgIDToEvents.Remove(msgID);
                    }
                }
            }
        }


        /// <summary>
        /// 收到简单消息回复
        /// </summary>
        /// <param name="data"></param>
        public void OnMsgRet(MessageHandleData data)
        {
            ProtoMsg.MsgRet msgRet = (ProtoMsg.MsgRet)data.data;
            MsgRetArgs args = new MsgRetArgs();
            args.MsgRet = msgRet;
            args.IsDialog = false;
            int messageCmd = (int)msgRet.RetMsgID;

            if (this.MsgIDToEvents.TryGetValue(messageCmd, out var eventf))
            {
                eventf(args);
            }

            // 通知gameManager
            GameManager.Instance.OnRetMsg(messageCmd);

            // 服务器返回错误码这边只做【找表然后显示报错提示信息】
            if (!args.IsDialog && args.MsgRet.RetCode != 0)
            {
                Frame.Util.ShowMessageByMsgRet(msgRet);
            }
        }
    }

    /// <summary>
    /// 简单消息回复事件参数
    /// </summary>
    [XLua.LuaCallCSharp]
    public class MsgRetArgs
    {
        public ProtoMsg.MsgRet MsgRet { get; set; }
        /// <summary>
        /// 是否已在UI上表现过
        /// </summary>
        public bool IsDialog { get; set; }
    }

    /// <summary>
    /// 简单回复消息处理
    /// </summary>
    /// <param name="msgRet"></param>
    [XLua.LuaCallCSharp]
    public delegate void MsgRetDelegate(MsgRetArgs args);
}
