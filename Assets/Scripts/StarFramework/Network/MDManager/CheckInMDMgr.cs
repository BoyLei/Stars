using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    public class CheckInMDMgr : MDMgrInterface
    {
        public CheckInMDMgr()
        {
            map = new Dictionary<ulong, CheckInMD>();
            mapli = new Dictionary<string, CheckInMD>();
        }

        Dictionary<ulong, CheckInMD> map;
        Dictionary<string, CheckInMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public CheckInMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.CheckInMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            CheckInMD checkInMD = (CheckInMD)msg;
            map[checkInMD.PID] = checkInMD;
            mapli[keyname] = checkInMD;
            return checkInMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            CheckInMD md = mapli[keyname];
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


        public CheckInMD GetCheckInMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}