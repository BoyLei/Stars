
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 伙伴数据
    /// </summary>
    public class UserSundryMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, UserSundryMD> map;
        private Dictionary<string, UserSundryMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public UserSundryMDMgr()
        {
            map = new Dictionary<ulong, UserSundryMD>();
            mapli = new Dictionary<string, UserSundryMD>();
        }

        public UserSundryMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.UserSundryMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            UserSundryMD gachaMD = (UserSundryMD)msg;
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

        public UserSundryMD GetUserSundryMD()
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

        public bool GetServerSign(SrvSignTypeEnum srvSignTypeEnum)
        {
            if (GetUserSundryMD() == null)
                return false;
            if (GetUserSundryMD().ServerSign == null)
                return false;
            if (GetUserSundryMD().ServerSign.Data == null)
                return false;
            int idx = ((int)srvSignTypeEnum);
            byte val = GetUserSundryMD().ServerSign.Data[0];
            int pos = 1 << idx;
            int res = val & pos;
            return res>0;


            /*
            var baseBinary = GetUserSundryMD().ServerSign;
            byte[] datas = baseBinary.Data.ToByteArray();
            int dlen = baseBinary.OneDataBitNum;
            int index = (int)srvSignTypeEnum;
            int ai = index / (8 / dlen);
            byte bitnum = (byte)(index % (8 / dlen) * dlen);
            if (ai >= baseBinary.ArrayLen)
            {
                return false;
            }

            byte d = datas[ai];
            if (dlen == 8)
            {
                return d > 0;
            }

            byte mask = (byte)((1 << dlen) - 1);
            var result = ((d & (mask << bitnum)) >> bitnum);
            return result > 0;
            */
        }

        public UserSundryMD GetUserSundryMDByPid(ulong pid)
        {
            foreach (var item in map)
            {
                if (item.Value != null && item.Value.PID == pid)
                {
                    return item.Value;
                }
            }
            return null;
        }

    }
}