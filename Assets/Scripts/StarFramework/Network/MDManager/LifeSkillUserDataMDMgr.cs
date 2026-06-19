using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;
using StarProject;
using StarProject.Game;

namespace SGF.Network
{
    /// <summary>
    /// 玩家生活技能数据
    /// </summary>
    public class LifeSkillUserDataMDMgr : MDMgrInterface
    {
        private Dictionary<int, LifeSkillUserDataMD> map;
        private Dictionary<string, LifeSkillUserDataMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public LifeSkillUserDataMDMgr()
        {
            map = new Dictionary<int, LifeSkillUserDataMD>();
            mapli = new Dictionary<string, LifeSkillUserDataMD>();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LifeSkillUserDataMD GetData(int key)
        {
            return map[key];
        }

        /// <summary>
        /// 配方是否解锁
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsUnLock(int jobID, int id)
        {
            var data = GetData(jobID);
            if (data != null && data.Formulation != null && data.Formulation.CreateIdMap != null)
            {
                if (data.Formulation.CreateIdMap.ContainsKey(id))
                {
                    return data.Formulation.CreateIdMap[id];
                }

            }
            return false;
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
            var msgid = (int)MsgIDEnum.LifeSkillUserDataMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            LifeSkillUserDataMD gachaMD = (LifeSkillUserDataMD)msg;
            map[gachaMD.SkillId] = gachaMD;
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
                map.Remove(md.SkillId);
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
    }
}
