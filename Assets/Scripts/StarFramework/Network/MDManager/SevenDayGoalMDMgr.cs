using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    public class SevenDayGoalMDMgr : MDMgrInterface
    {
        public SevenDayGoalMDMgr()
        {
            map = new Dictionary<ulong, SevenDayGoalMD>();
            mapli = new Dictionary<string, SevenDayGoalMD>();
        }

        Dictionary<ulong, SevenDayGoalMD> map;
        Dictionary<string, SevenDayGoalMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public SevenDayGoalMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.SevenDayGoalMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            SevenDayGoalMD sevenDayGoalMD = (SevenDayGoalMD)msg;
            map[sevenDayGoalMD.PID] = sevenDayGoalMD;
            mapli[keyname] = sevenDayGoalMD;
            return sevenDayGoalMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            SevenDayGoalMD md = mapli[keyname];
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


        public SevenDayGoalMD GetSevenDayGoalMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}