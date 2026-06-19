
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 战力数据
    /// </summary>
    public class FightMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, FightMD> map;
        private Dictionary<string, FightMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public FightMDMgr()
        {
            map = new Dictionary<ulong, FightMD>();
            mapli = new Dictionary<string, FightMD>();
        }

        public FightMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.FightMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            FightMD gachaMD = (FightMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            //SGF.Debuger.Log($"FightMDMgr Add() keyname={keyname},data={data}");

            return gachaMD;
        }

        public void Update(string keyname)
        {
            //SGF.Debuger.Log($"FightMDMgr Update() keyname={keyname}");
        }

        public IMessage Del(string keyname)
        {
            //SGF.Debuger.Log($"FightMDMgr Del() keyname={keyname}");

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

        public FightMD GetMD()
        {
            //  对于战力数据来说， 表的 第一个就是玩家自己的战力MD,且只有 一个
            foreach (var item in map)
            {
                if (item.Value != null)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public FightMD GetMDByPid(ulong pid)
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