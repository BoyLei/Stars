
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// pvp报名信息
    /// </summary>
    public class B10registerMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, Battle10RegisterMD> map;
        private Dictionary<string, Battle10RegisterMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }

        public B10registerMDMgr()
        {
            map = new Dictionary<ulong, Battle10RegisterMD>();
            mapli = new Dictionary<string, Battle10RegisterMD>();
        }

        public Battle10RegisterMD GetData(ulong key)
        {
            return map[key];
        }

        public void Init()
        {
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //通知
        }

        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.Battle10RegisterMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            Battle10RegisterMD gachaMD = (Battle10RegisterMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;

            return gachaMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            if (mapli.TryGetValue(keyname, out var md))
            {
                map.Remove(md.PID);
                mapli.Remove(keyname);
                return md;
            }
            else
            {
                return null;
            }
        }

        public IMessage GetMsg(string keyname)
        {
            return mapli[keyname];
        }

        public void Release()
        {
        }

        public Battle10RegisterMD GetBattle10RegisterMD()
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