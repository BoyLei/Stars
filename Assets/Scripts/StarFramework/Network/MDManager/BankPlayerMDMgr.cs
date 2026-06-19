using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    public class BankPlayerMDMgr : MDMgrInterface
    {
        public BankPlayerMDMgr()
        {
            map = new Dictionary<ulong, BankPlayerMD>();
            mapli = new Dictionary<string, BankPlayerMD>();
        }

        Dictionary<ulong, BankPlayerMD> map;
        Dictionary<string, BankPlayerMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public BankPlayerMD GetData(ulong key)
        {
            return map[key];
        }

        //大退出重登入

        public void Init()
        {
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //通知
        }


        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.BankPlayerMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            BankPlayerMD gachaMD = (BankPlayerMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;

            return gachaMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            BankPlayerMD md = mapli[keyname];
            map.Remove(md.PID);
            mapli.Remove(keyname);
            return md;
        }

        public IMessage GetMsg(string keyname)
        {
            return mapli[keyname];
        }

        public void Release()
        {
        }


        public BankPlayerMD GetBankPlayerMD()
        {
            foreach (var item in map)
            {
                if (item.Value != null)
                {
                    return item.Value;
                }
            }

            return null;
        }
    }
}