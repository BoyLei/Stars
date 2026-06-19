using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    /// <summary>
    /// �鿨������
    /// </summary>
    public class GachaMDMgr : MDMgrInterface
    {
        public GachaMDMgr()
        {
            map = new Dictionary<ulong, GachaMD>();
            mapli = new Dictionary<string, GachaMD>();
        }

        Dictionary<ulong, GachaMD> map;
        Dictionary<string, GachaMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public GachaMD GetData(ulong key)
        {
            return map[key];
        }

        //���˳��ص���

        public void Init()
        {
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //֪ͨ
        }


        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.GachaMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            GachaMD gachaMD = (GachaMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            return gachaMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            GachaMD md = mapli[keyname];
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


        public GachaMD GetGachaMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}