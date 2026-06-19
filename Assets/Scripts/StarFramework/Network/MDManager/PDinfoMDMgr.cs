using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    public class PDinfoMDMgr : MDMgrInterface
    {
        public PDinfoMDMgr()
        {
            map = new Dictionary<ulong, PDinfoMD>();
            mapli = new Dictionary<string, PDinfoMD>();
        }

        Dictionary<ulong, PDinfoMD> map;
        Dictionary<string, PDinfoMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }

        public PDinfoMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.PDinfoMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            PDinfoMD pdInfoMD = (PDinfoMD)msg;
            map[pdInfoMD.PID] = pdInfoMD;
            mapli[keyname] = pdInfoMD;
            return pdInfoMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            PDinfoMD md = mapli[keyname];
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


        public PDinfoMD GetPDinfoMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}