
using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 新手目标
    /// </summary>
    public class LobbyGamePlayMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, LobbyGamePlayMD> map;
        private Dictionary<string, LobbyGamePlayMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public LobbyGamePlayMDMgr()
        {
            map = new Dictionary<ulong, LobbyGamePlayMD>();
            mapli = new Dictionary<string, LobbyGamePlayMD>();
        }

        public LobbyGamePlayMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.LobbyGamePlayMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            LobbyGamePlayMD gachaMD = (LobbyGamePlayMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            //SGF.Debuger.Log($"GamePlayMDMgr Add() keyname={keyname},data={data}");

            return gachaMD;
        }

        public void Update(string keyname)
        {
            //SGF.Debuger.Log($"GamePlayMDMgr Update() keyname={keyname}");
        }

        public IMessage Del(string keyname)
        {
            //SGF.Debuger.Log($"GamePlayMDMgr Del() keyname={keyname}");

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

        public LobbyGamePlayMD GetMD()
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

        public LobbyGamePlayMD GetMDByPid(ulong pid)
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