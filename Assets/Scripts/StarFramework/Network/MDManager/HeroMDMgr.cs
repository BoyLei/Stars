using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    /// <summary>
    /// 主角數據
    /// </summary>
    public class HeroMDMgr : MDMgrInterface
    {
        public HeroMDMgr()
        {
            map = new Dictionary<ulong, HeroMD>();
            mapli = new Dictionary<string, HeroMD>();
        }

        Dictionary<ulong, HeroMD> map;
        Dictionary<string, HeroMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public HeroMD GetData(ulong key)
        {
            return map[key];
        }

        //大退出重登入

        public void Init()
        {
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //通知
        }


        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.HeroMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            HeroMD gachaMD = (HeroMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            return gachaMD;
        }

        public void Update(string keyname)
        {
        }

        public IMessage Del(string keyname)
        {
            HeroMD md = mapli[keyname];
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


        public HeroMD GetHeroMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}