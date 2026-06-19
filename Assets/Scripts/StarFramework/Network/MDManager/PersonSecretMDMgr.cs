
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 伙伴数据
    /// </summary>
    public class PersonSecretMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, PersonSecretMD> map;
        private Dictionary<string, PersonSecretMD> mapli;

        public PersonSecretMDMgr()
        {
            map = new Dictionary<ulong, PersonSecretMD>();
            mapli = new Dictionary<string, PersonSecretMD>();
        }
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public PersonSecretMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.PersonSecretMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            PersonSecretMD gachaMD = (PersonSecretMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            //SGF.Debuger.Log($"PersonSecretMDMgr Add() keyname={keyname},data={data}");

            return gachaMD;
        }

        public void Update(string keyname)
        {
            //SGF.Debuger.Log($"PersonSecretMDMgr Update() keyname={keyname}");
        }

        public IMessage Del(string keyname)
        {
            //SGF.Debuger.Log($"PersonSecretMDMgr Del() keyname={keyname}");

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

        public PersonSecretMD GetMD()
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

        public PersonSecretMD GetMDByPid(ulong pid)
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