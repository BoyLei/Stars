
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 个人爬塔数据
    /// </summary>
    public class PersonTowerMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, PersonTowerMD> map;
        private Dictionary<string, PersonTowerMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public PersonTowerMDMgr()
        {
            map = new Dictionary<ulong, PersonTowerMD>();
            mapli = new Dictionary<string, PersonTowerMD>();
        }

        public PersonTowerMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.PersonTowerMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            PersonTowerMD gachaMD = (PersonTowerMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            //SGF.Debuger.Log($"PersonTowerMDMgr Add() keyname={keyname},data={data}");

            return gachaMD;
        }

        public void Update(string keyname)
        {
            //SGF.Debuger.Log($"PersonTowerMDMgr Update() keyname={keyname}");
        }

        public IMessage Del(string keyname)
        {
            //SGF.Debuger.Log($"PersonTowerMDMgr Del() keyname={keyname}");

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

        public PersonTowerMD GetMD()
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

        public PersonTowerMD GetMDByPid(ulong pid)
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