using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProjectDef;

namespace SGF.Network
{
    /// <summary>
    /// 伙伴数据
    /// </summary>
    [XLua.LuaCallCSharp]
    public class PartnerMDMgr : MDMgrInterface
    {
        private Dictionary<long, PartnerMD> map;
        private Dictionary<string, PartnerMD> mapli;

        private Dictionary<long /*伙伴ID*/, (long /*升星道具ID*/, bool)> PartnerStarRed;

        private Dictionary<long, PartnerMD> CurInBattlePartnerDic = new();

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
            PartnerStarRed.Clear();
            CurInBattlePartnerDic.Clear();
        }

        public PartnerMDMgr()
        {
            map = new Dictionary<long, PartnerMD>();
            mapli = new Dictionary<string, PartnerMD>();
            PartnerStarRed = new Dictionary<long, (long, bool)>();
            Init();
        }

        public PartnerMD GetPartnerByIndex(long index)
        {
            if (map.ContainsKey(index))
            {
                return map[index];
            }
            return null;
        }

        public PartnerMD GetPartnerMD(ulong id)
        {

            if (map != null && map.Count > 0)
            {
                foreach (var item in map)
                {

                    if (item.Value.ID == id)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }

        public PartnerMD GetData(long key)
        {
            return map[key];
        }

        public void Init()
        {
            GlobalEvent.OnItemChange.AddListener(OnItemChangeHandler);
        }

        public long GetPartnerIDByUpStarItem(long id)
        {
            if (PartnerStarRed != null && PartnerStarRed.Count > 0)
            {
                foreach (var item in PartnerStarRed)
                {
                    if (item.Value.Item1 == id)
                    {
                        return item.Key;
                    }
                }
            }
            return 0;
        }
        
        private void OnItemChangeHandler(long configID, long Num)
        {
            bool ignored = true;
            foreach (var star in PartnerStarRed)
            {
                if (star.Value.Item1 == configID)
                {
                    ignored = false;
                    break;
                }
            }

            if (!ignored)
            {
                UpdateAllPartnerStar();
            }

            if (configID == 104011 || configID == 104012 || configID == 104013)
            {
                RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_UpGrade);
            }
        }

        private void UpdateAllPartnerStar()
        {
            var list = new Dictionary<long, (long, bool)>();
            foreach (var star in PartnerStarRed)
            {
                if (map.ContainsKey(star.Key))
                {
                    var data = map[star.Key];
                    var red = UpdatePartnerStarRed(data);
                    list.Add(star.Key, red);
                    //PartnerStarRed[star.Key] = red;
                }
            }

            foreach (var temp in list)
            {
                PartnerStarRed[temp.Key] = temp.Value;
            }
        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //通知
        }

        public IMessage Add(string keyname, DBDataModel data)
        {
            var msgid = (int)MsgIDEnum.PartnerMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            PartnerMD gachaMD = (PartnerMD)msg;
            map[gachaMD.Index] = gachaMD;
            mapli[keyname] = gachaMD;

            var red = UpdatePartnerStarRed(gachaMD);
            if (!PartnerStarRed.ContainsKey(gachaMD.Index))
            {
                PartnerStarRed.Add(gachaMD.Index, red);
            }
            else
            {
                PartnerStarRed[gachaMD.Index] = red;
            }

            return gachaMD;
        }


        public (long, bool) UpdatePartnerStarRed(PartnerMD partnerMD)
        {
            var config = LocalDataManager.Instance.GetPartnerDataCell(partnerMD.Index);
            if (config != null)
            {
                if (partnerMD.CurStar >= 6)
                {
                    return (config.GetUpStarItem(), false);
                }

                long currentCnt = BusinessManager.Instance.GetCurrencyNum(config.GetUpStarItem());
                long needCnt = 999999;
                var cfg = LocalDataManager.Instance.GetStarExpendDataCell(config.GetQuality(), partnerMD.CurStar + 1);
                if (cfg != null)
                {
                    needCnt = cfg.GetItem_num();
                }

                return (config.GetUpStarItem(), (partnerMD.CurStar < 6 && currentCnt >= needCnt));
            }

            return (0, false);
        }

        public bool HasPartnerEquipRed()
        {
            if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_Equip))
            {
                return false;
            }
            //拿到所有的伙伴装备
            var allitems_1 = BusinessManager.Instance.GetItemsByItemType(4, 1);
            var allitems_2 = BusinessManager.Instance.GetItemsByItemType(4, 2);
            var allitems_3 = BusinessManager.Instance.GetItemsByItemType(4, 3);

            foreach (var partner in map)
            {
                var data = BusinessManager.Instance.GetPartnerEquipedEquip((ulong)partner.Key, 1);
                if (data == null && allitems_1.Count > 0)
                {
                    return true;
                }

                var data1 = BusinessManager.Instance.GetPartnerEquipedEquip((ulong)partner.Key, 2);
                if (data1 == null && allitems_2.Count > 0)
                {
                    return true;
                }

                var data2 = BusinessManager.Instance.GetPartnerEquipedEquip((ulong)partner.Key, 3);
                if (data2 == null && allitems_3.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPartnerStarRed()
        {
            if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_StarUP))
            {
                return false;
            }
            UpdateAllPartnerStar();
            foreach (var star in PartnerStarRed)
            {
                if (star.Value.Item2)
                {
                    return true;
                }
            }

            return false;
        }

        public bool UpdatePartnerConstructRed(PartnerMD partnerMD)
        {

            var config = LocalDataManager.Instance.GetPartnerDataCell(partnerMD.Index);
            if (config != null)
            {
                var qualificate =
                    LocalDataManager.Instance.GetQualificationsData(config.GetQualificationID(),
                        partnerMD.AptitudeBreakStep);

                //等级
                if (partnerMD.Level< qualificate.GetLevelGrade())
                {
                    return false;
                }
                
                var totalold = 0;
                var max = 0;
                if (qualificate != null)
                {
                    for (int i = 0; i < qualificate.QualiFications.Count; i++)
                    {
                        max += qualificate.Value_max[i];
                        var t = partnerMD.AMD.Aptitudes[qualificate.QualiFications[i]];
                        totalold += t.BaseValue;
                    }

                    var isFull = max == totalold;
                    if (isFull)
                    {
                        if (partnerMD.CurStar >= qualificate.Star)
                        {
                            return true;
                        }
                    }
                    else
                    {

                        var eCost = LocalDataManager.Instance.GetQualificationsExpendDataCell(
                            config.QualificationsExpendID,
                            partnerMD.AptitudeBreakStep);
                        if (eCost != null)
                        {
                            var money = BusinessManager.Instance.GetCurrencyNum(eCost.Money[0]);
                            if (money < eCost.Money[1])
                            {
                                return false;
                            }

                            var itemSuit = true;
                            for (int i = 0; i < eCost.Item_id.Count; i++)
                            {
                                var cnt = BusinessManager.Instance.GetCurrencyNum(eCost.Item_id[i]);
                                if (cnt < eCost.Item_num[i])
                                {
                                    itemSuit = false;
                                    break;

                                }
                            }

                            return itemSuit;
                        }

                    }

                    return false;
                }
            }

            return false;
        }
        
        public bool HasConstructRed()
        {
            if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_Qualification))
            {
                return false;
            }
            var hasred = false;
            if (map != null && map.Count > 0)
            {
                foreach (var item in map)
                {
                    hasred = UpdatePartnerConstructRed(item.Value);
                    if (hasred)
                    {
                        break;
                    }
                }
            }
            return hasred;
        }

        public bool HasUpgradeRed()
        {
            if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_LevelUp))
            {
                return false;
            }
            var cnt = BusinessManager.Instance.GetCurrencyNum(104011);
            cnt+=BusinessManager.Instance.GetCurrencyNum(104012);
            cnt+=BusinessManager.Instance.GetCurrencyNum(104013);
            //是否有升级的道具
            if (cnt > 0)
            {
                var playerLv = GameManager.Instance.GetPlayerLevel();
                foreach (var item in map)
                {
                   var  cfg=LocalDataManager.Instance.GetPartnerExpDataCell(item.Value.Level + 1);
                   if (cfg != null)
                   {
                       if (cfg.UnlockLevel <= playerLv)
                       {
                           return true;
                       }
                   }
                }
            }
            return false;
        }

        /*//构筑
        Partner_Construct,
        //伙伴升级
        Partner_Upgrade,*/
        public void Update(string keyname)
        {
            //红点刷新
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_CanInBattle);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_CanAssist);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_Team);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_Level);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_Equip);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_UpGrade);
            RedPointManager.Instance.TriggerConditionType(RedPointConditionType.Partner_Construct);
        }
        
        
        

        public IMessage Del(string keyname)
        {
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

        public void Release()
        {
        }

        public Dictionary<long, PartnerMD> GetInPlayedPartnerDic()
        {
            Dictionary<long, PartnerMD> dic = new();
            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.InBattle || item.Value.State == PartnerState.Concretization)
                {
                    dic.Add(item.Value.Index, item.Value);
                }
            }

            return dic;
        }

        public List<PartnerMD> GetPartners()
        {
            List<PartnerMD> partners = new();
            foreach (var item in map)
            {
                partners.Add(item.Value);
            }
            return partners;
        }

        /// <summary>
        /// 获取XX等级以上的伙伴
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns>数量</returns>
        public int GetPartnerCountByLevel(int level)
        {
            int count = 0;
            foreach (var item in map)
            {
                if (item.Value.Level >= level)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 获取出战的伙伴
        /// </summary>
        /// <returns></returns>
        public List<PartnerMD> GetInPlayedPartners()
        {
            List<PartnerMD> list = new();

            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.InBattle || item.Value.State == PartnerState.Concretization)
                {
                    list.Add(item.Value);
                }
            }

            return list;
        }

        /// <summary>
        /// 获得助战的伙伴
        /// </summary>
        public List<PartnerMD> GetInAssistPartners()
        {
            List<PartnerMD> list = new();
            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.Assist)
                {
                    list.Add(item.Value);
                }
            }

            return list;
        }



        public int GetInBattleCount()
        {
            int count = 0;
            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.InBattle || item.Value.State == PartnerState.Concretization)
                {
                    count++;
                }
            }

            return count;
        }

        public List<PartnerMD> GetInBattlePatner()
        {
            List<PartnerMD> list = new List<PartnerMD>();
            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.InBattle)
                {
                    list.Add(item.Value);
                }
            }

            return list;
        }

        public int GetInAssistCount()
        {
            int count = 0;
            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.Assist)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 获取出战的伙伴
        /// </summary>
        /// <returns></returns>
        public int FreePartnerCount()
        {
            int count = 0;

            foreach (var item in map)
            {
                if (item.Value.State == PartnerState.DefaultState)
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsMyPartner(ulong entityID)
        {
            if (map != null)
            {
                foreach (var item in map)
                {
                    if (item.Value.ID == entityID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsMyPartnerByEid(ulong entityID)
        {
            if (map != null)
            {
                foreach (var item in map)
                {
                    if (item.Value.ID == entityID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取等级伙伴数量
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetLevelPartnerCount(int level)
        {
            int count = 0;
            if (map != null)
            {
                foreach (var item in map)
                {
                    if (item.Value.Level >= level)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}