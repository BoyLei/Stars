using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    public class DailyActivityMDMgr : MDMgrInterface
    {
        public DailyActivityMDMgr()
        {
            map = new Dictionary<ulong, DailyActivityMD>();
            mapli = new Dictionary<string, DailyActivityMD>();
        }

        Dictionary<ulong, DailyActivityMD> map;
        Dictionary<string, DailyActivityMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public DailyActivityMD GetData(ulong key)
        {
            return map[key];
        }

        public void Init()
        {
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
        }


        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.DailyActivityMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            DailyActivityMD dailyActivityMD = (DailyActivityMD)msg;
            map[dailyActivityMD.PID] = dailyActivityMD;
            mapli[keyname] = dailyActivityMD;
            return dailyActivityMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            DailyActivityMD md = mapli[keyname];
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


        public DailyActivityMD GetDailyActivityMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}