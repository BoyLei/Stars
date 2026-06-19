using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;

namespace SGF.Network
{
    /// <summary>
    /// װ������������
    /// </summary>
    public class EqRecastMDMgr : MDMgrInterface
    {
        public EqRecastMDMgr()
        {
            map = new Dictionary<ulong, EqRecastMD>();
            mapli = new Dictionary<string, EqRecastMD>();
        }

        Dictionary<ulong, EqRecastMD> map;
        Dictionary<string, EqRecastMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public EqRecastMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.EqRecastMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            EqRecastMD eqRecastMD = (EqRecastMD)msg;
            map[eqRecastMD.PID] = eqRecastMD;
            mapli[keyname] = eqRecastMD;
            CheckRedPoint();
            return eqRecastMD;
        }

        public void Update(string keyname)
        {
            CheckRedPoint();
        }


        public void CheckRedPoint()
        {
            bool isRed = false;
            var eqRecastMD = GetEqRecastMD();
            if (eqRecastMD != null)
            {
                var itemInfos = eqRecastMD.RecastEqs.ItemInfos;
                isRed = (itemInfos.Count > 0);
            }

            RedPointManager.Instance.RefreshRedPointCount(StarProjectDef.RedPointType.PlayRedPointEquipCanRecast, isRed ? 1 : 0);
        }

        public IMessage Del(string keyname)
        {
            EqRecastMD md = mapli[keyname];
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


        public EqRecastMD GetEqRecastMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }
}