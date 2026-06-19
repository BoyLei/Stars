using Google.Protobuf;
using ProtoMsg;
using SGF.Module.Framework;
using System.Collections.Generic;

namespace SGF.Network
{

    [XLua.LuaCallCSharp]
    public delegate void ThirdMessageDelegate(string msgName, object data);

    /// <summary>
    /// 服务器中转第三方的协议统一处理
    /// </summary>
    [XLua.LuaCallCSharp]
    public class Client2ThirdMsgManager : ServiceModule<Client2ThirdMsgManager>
    {
        public string flagKey = "[Client2ThirdMsgManager]";

        private Dictionary<MsgIDEnum, ThirdMessageDelegate> key2MessageHandleDic = new();

        internal void Init()
        {

        }

        public override void Release()
        {
            base.Release();
            this.Log("Release() Client2ThirdMsgManager");
        }

        public void OnMessageEnum(MsgIDEnum msgIDEnum, ThirdMessageDelegate callback)
        {
            if (key2MessageHandleDic.ContainsKey(msgIDEnum))
            {
                key2MessageHandleDic[msgIDEnum] += callback;
            }
            else
            {
                key2MessageHandleDic.Add(msgIDEnum, callback);
            }
        }

        private void OffMessageEnum(MsgIDEnum msgIDEnum, ThirdMessageDelegate callback)
        {
            if (key2MessageHandleDic.ContainsKey(msgIDEnum))
            {
                key2MessageHandleDic[msgIDEnum] -= callback;
                if (key2MessageHandleDic[msgIDEnum] == null)
                {
                    key2MessageHandleDic.Remove(msgIDEnum);
                }
            }
        }

        public void HandleMessageCS(Client2ThirdRet dataRet)
        {
            if (dataRet == null)
            {
                return;
            }

            // 判断有无监听
            string MsgName = dataRet.MsgName;

            // 构建协议名对应的结构体
            var msgid = ProtoDic.Instance.GetCMDByName(MsgName); ;
            IMessage message = SGF.Network.ProtoUtils.Deserialize(msgid, dataRet.MsgData.ToByteArray());
            if (message == null)
            {
                SGF.Debuger.LogError($"{flagKey} handleMessageCS MsgName={MsgName},msgid={msgid}, Deserialize error");
                return;
            }

            key2MessageHandleDic[MsgIDEnum.Client2ThirdRetID](MsgName, message);
        }
    }
}

