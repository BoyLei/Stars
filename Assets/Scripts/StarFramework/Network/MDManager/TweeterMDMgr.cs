using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;
using UnityEngine;

namespace SGF.Network
{
    /// <summary>
    /// µÀ¾ß¹ÜÀíÆ÷
    /// </summary>
    public class TweeterMDMgr : MDMgrInterface
    {

        public TweeterMDMgr()
        {
            map = new Dictionary<int, TweeterMD>();
            mapli = new Dictionary<string, TweeterMD>();
        }
        Dictionary<int, TweeterMD> map;
        Dictionary<string, TweeterMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public TweeterMD GetData(int key)
        {
            return map[key];
        }

        public void Init()
        {

        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //Í¨Öª


        }


        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.TweeterMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            TweeterMD mD = (TweeterMD)msg;
            map[mD.Index] = mD;
            mapli[keyname] = mD;
            return mD;
        }

        public void Update(string keyname)
        {
            var Tweeter = (TweeterMD)mapli[keyname];
        }

        public IMessage Del(string keyname)
        {
            Debug.LogFormat("TweeterMD delete Keyname:{0};", keyname);
            if (mapli.TryGetValue(keyname, out var md))
            {
                map.Remove(md.Index);
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


        public TweeterMD GetTweeterByBaseId(int baseID)
        {
            foreach (var it in map)
            {
                if (it.Value.Index == baseID)
                {
                    return it.Value;
                }
            }

            return null;
        }


        public void Release()
        {

        }



    }

}