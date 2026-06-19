using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;
using StarProject;
using StarProject.Game;

namespace SGF.Network
{
    /// <summary>
    /// 玩家场景数据
    /// </summary>
    public class UserSceneLogicDataMDMgr : MDMgrInterface
    {
        private Dictionary<ulong, UserSceneLogicDataMD> map;
        private Dictionary<string, UserSceneLogicDataMD> mapli;

        public UserSceneLogicDataMDMgr()
        {
            map = new Dictionary<ulong, UserSceneLogicDataMD>();
            mapli = new Dictionary<string, UserSceneLogicDataMD>();
        }
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public UserSceneLogicDataMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.UserSceneLogicDataMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            UserSceneLogicDataMD gachaMD = (UserSceneLogicDataMD)msg;
            map[gachaMD.PID] = gachaMD;
            mapli[keyname] = gachaMD;
            SGF.Debuger.Log($"PersonSecretMDMgr Add() keyname={keyname},data={data}");

            return gachaMD;
        }

        public void Update(string keyname)
        {
            GlobalEvent.OnMapMineChange?.Invoke(0);
            GlobalEvent.OnFreshServerEntityVisiable?.Invoke(0);
            var data = GetLifeSkillOneMapMineData(GameManager.Instance.GetCurMapId());
            if (data != null)
            {
                foreach (var mine in data.Mines)
                {
                    StarDebug.Log(StarDebug.Orange, "矿点信息同步", "InterId", mine.Value.InterId, "MineId", mine.Value.MineId, "MineIndex", mine.Value.MineIndex, "UsedReserve", mine.Value.UsedReserve);
                }
            }
            SGF.Debuger.Log($"PersonSecretMDMgr Update() keyname={keyname}");
        }

        public IMessage Del(string keyname)
        {
            SGF.Debuger.Log($"PersonSecretMDMgr Del() keyname={keyname}");

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

        public UserSceneLogicDataMD GetUserMapData()
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

        public LifeSkillOneMapMineData GetLifeSkillOneMapMineData(int mapID)
        {
            var data = GetUserMapData();
            if (data != null)
            {
                if (data.GatherData != null && data.GatherData.MapData != null)
                {
                    if (data.GatherData.MapData.ContainsKey(mapID))
                    {
                        return data.GatherData.MapData[mapID];
                    }
                }
            }

            return null;
        }

    }
}