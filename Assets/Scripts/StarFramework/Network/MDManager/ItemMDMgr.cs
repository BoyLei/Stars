using Google.Protobuf;
using ProtoMsg;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using System.Collections.Generic;
using UnityEngine;

namespace SGF.Network
{
    /// <summary>
    /// µÀ¾ß¹ÜÀíÆ÷
    /// </summary>
    public class ItemMDMgr : MDMgrInterface
    {

        public ItemMDMgr()
        {
            map = new Dictionary<ulong, ItemMD>();
            mapli = new Dictionary<string, ItemMD>();
        }
        Dictionary<ulong, ItemMD> map;
        Dictionary<string, ItemMD> mapli;

        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }

        public ItemMD GetData(ulong key)
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
            var msgid = (int)MsgIDEnum.ItemMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            ItemMD mD = (ItemMD)msg;
            //药品cd要读取服务器配置 不然退出再进 或者顶号会有问题
            mD.CD = mD.CDTime;
            if (BusinessManager.Instance.IsInitAllItem && !map.ContainsKey(mD.ID))
            {
                mD.IsNew = true;
            }
            map[mD.ID] = mD;
            mapli[keyname] = mD;







            mD.OldNum = mD.Num;

            //装备
            if (mD.SpaceID == 2 || mD.SpaceID == 1)
            {


                BusinessManager.Instance.CheckIsHaveCanEquipedEquipRedpoint();
                //GlobalEvent.OnHaveNewEquip.Invoke(mD.EntityID);
            }
            if (mD.SpaceID == 2 && mD.SpaceID == 2)
            {
                if (BusinessManager.Instance.IsInitAllItem && mD.IsNew)
                {
                    mD.IsNeedCheckQuickEquip = true;
                }
            }

            return mD;
        }

        public void Update(string keyname)
        {
            var item = (ItemMD)mapli[keyname];
            //¸Ä±äÅÐ¶ÏÊÇ·ñÊÇnew
            if (item.Num > item.OldNum && BusinessManager.Instance.IsInitAllItem)
            {
                item.IsNew = true;
                item.OldNum = item.Num;
            }
            GlobalEvent.OnItemChange.Invoke(item.BaseID, item.Num);
        }

        public IMessage Del(string keyname)
        {
            Debug.LogFormat("ItemMD delete Keyname:{0};", keyname);
            if (mapli.TryGetValue(keyname, out var md))
            {
                map.Remove(md.ID);
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

        /// <summary>
        /// 清空临时背包
        /// </summary>
        public void ClearTempBagItems()
        {
            List<ulong> keyList = new List<ulong>();
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 22)
                {
                    keyList.Add(it.Key);
                }
            }

            for (int i = 0; i < keyList.Count; i++)
            {
                map.Remove(keyList[i]);
            }

            List<string> keyList2 = new List<string>();
            foreach (var it in mapli)
            {
                if (it.Value.SpaceId == 22)
                {
                    keyList2.Add(it.Key);
                }
            }

            for (int i = 0; i < keyList2.Count; i++)
            {
                mapli.Remove(keyList2[i]);
            }

        }


        public ItemMD GetHeroEquipedAmulet(ulong heroid, int subtype)
        {
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 13 && it.Value.Heroid == heroid)
                {
                    var cfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);

                    if (cfg.GetSubType() == subtype)
                    {

                        return it.Value;
                    }

                }
            }

            return null;
        }

        public ItemMD GetPartnerEquipedEquip(ulong heroid, int type)
        {
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 20 && it.Value.Heroid == heroid)
                {
                    var cfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);

                    if (cfg.GetSubType() == type)
                    {

                        return it.Value;
                    }

                }
            }

            return null;
        }

        /// <summary>
        /// 判断装备是否可以装备
        /// </summary>
        /// <returns></returns>
        public bool CheckIsEquipCanEquiped(int baseID)
        {
            var equipCfg = LocalDataManager.Instance.GetEquipDataCell(baseID);
            if (equipCfg == null)
            {
                return false;
            }


            //等级不足无法穿戴
            int lvLimit = equipCfg.Level;
            int playerLv = GameManager.Instance.GetPlayerLevel();
            if (lvLimit > playerLv)
            {
                return false;
            }

            //职业不同无法穿戴
            int JobType = equipCfg.JobBaseType;
            int playerJob = GameManager.Instance.GetPlayJobBaseID();
            if (!(JobType == 0 || playerJob == JobType))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断背包里是否有玩家可以装备的装备
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckIsHaveCanEquipedEquip(int type)
        {
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 2)
                {
                    var cfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);
                    if (cfg.GetSubType() == type)
                    {
                        bool canEquip = CheckIsEquipCanEquiped((int)it.Value.BaseID);
                        //找到就返回
                        if (canEquip)
                            return true;
                    }

                }
            }
            return false;
        }

        public ItemMD GetHeroEquipedEquip(ulong heroid, int type)
        {
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 1 && it.Value.Heroid == heroid)
                {
                    var cfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);

                    if (cfg.GetSubType() == type)
                    {

                        return it.Value;
                    }

                }
            }

            return null;
        }

        public List<ItemMD> GetAllItemsByItemBaseId(long jid)
        {
            List<ItemMD> allItem = new();
            foreach (var it in map)
            {
                if (it.Value.BaseID == jid)
                {
                    allItem.Add(it.Value);
                }
            }

            return allItem;
        }

        public ItemMD GetNoBindItemByItemBaseId(long jid)
        {
            foreach (var it in map)
            {
                if (it.Value.BaseID == jid && !it.Value.IsBind)
                {
                    return it.Value;
                }
            }

            return null;
        }

        public ItemMD GetBindItemByItemBaseId(long jid)
        {
            foreach (var it in map)
            {
                if (it.Value.BaseID == jid && it.Value.IsBind)
                {
                    return it.Value;
                }
            }

            return null;
        }

        public ItemMD GetItemByItemBaseId(long jid)
        {
            foreach (var it in map)
            {
                if (it.Value.BaseID == jid)
                {
                    return it.Value;
                }
            }

            return null;
        }

        public ItemMD GetItemByItemEntityId(ulong id)
        {
            foreach (var it in map)
            {
                if (it.Value.EntityID == id)
                {
                    return it.Value;


                }
            }

            return null;


        }
        public List<ItemMD> GetAllNotCheckQuickEquip()
        {
            List<ItemMD> temp = new();
            foreach (var it in map)
            {
                if (it.Value.SpaceId == 2 && it.Value.IsNeedCheckQuickEquip)
                {
                    if (it.Value.Num > 0)
                        temp.Add(it.Value);
                }
            }
            return temp;
        }
        public List<ItemMD> GetItemsByItemSpaceId(int spaceId)
        {
            List<ItemMD> temp = new();
            foreach (var it in map)
            {
                if (it.Value.SpaceId == spaceId || (spaceId == 0 && (it.Value.SpaceId == 2 || it.Value.SpaceId == 15 || it.Value.SpaceId == 14 || it.Value.SpaceId == 16 || it.Value.SpaceId == 19 || it.Value.SpaceId == 20 || it.Value.SpaceId == 17 || it.Value.SpaceId == 11 || it.Value.SpaceId == 24)))
                {
                    if (it.Value.Num > 0)
                        temp.Add(it.Value);
                }
            }
            return temp;
        }

        public void RemoveItemsBySpaceId(int spaceId)
        {
            Dictionary<ulong, ItemMD> temp = new();
            foreach (var it in map)
            {
                if (it.Value.SpaceId != spaceId)
                {
                    temp[it.Key] = it.Value;
                }
            }
            map = temp;
        }

        public List<ItemMD> GetItemsCanResolve()
        {
            List<ItemMD> temp = new();
            foreach (var it in map)
            {
                if (it.Value.Heroid == 0 && it.Value.Num > 0)
                {
                    var cfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);
                    if (cfg.GetBackItem() > 0)
                    {
                        temp.Add(it.Value);
                    }
                }
            }
            return temp;
        }


        public int GetItemNumBySpaceID(int spaceid)
        {
            int totalNum = 0;

            foreach (var it in map)
            {


                if (it.Value.SpaceId == spaceid )
                {
                    if (it.Value.Num > 0)
                        totalNum++;
                }
            }
            return totalNum;
        }

        public int GetItemNumByType(int type, int subtype)
        {
            int totalNum = 0;

            foreach (var it in map)
            {

                var itCfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);

                if (itCfg.GetItemType() == type && (itCfg.GetSubType() == subtype || subtype == 0))
                {
                    if (it.Value.Num > 0)
                        totalNum++;
                }
            }
            return totalNum;
        }

        public List<ItemMD> GetItemsByItemType(int type, int subtype)
        {
            List<ItemMD> temp = new();
            foreach (var it in map)
            {

                var itCfg = LocalDataManager.Instance.GetItemDataCell(it.Value.BaseID);

                if (itCfg.GetItemType() == type && (itCfg.GetSubType() == subtype || subtype == 0))
                {
                    if (it.Value.Num > 0)
                        temp.Add(it.Value);
                }
            }
            return temp;
        }

        public void UnNewAllItems()
        {

            foreach (var it in map)
            {
                it.Value.IsNew = false;
            }

        }

        public bool IsHaveNew()
        {
            bool haveNew = false;

            foreach (var it in map)
            {
                if (it.Value.IsNew)
                {
                    haveNew = true;
                    break;
                }
            }

            return haveNew;
        }

        /// <summary>
        /// Ë¢ÐÂCD
        /// </summary>
        /// <param name="data"></param>
        public void OnUseItemCDInfoRefesh(MessageHandleData data)
        {
            UseItemCDInfoNtf UseItemCDInfoMsg = (UseItemCDInfoNtf)data.data;

            foreach (var info in UseItemCDInfoMsg.CdInfo)
            {
                foreach (var it in map)
                {
                    var item = it.Value;
                    var cfg = LocalDataManager.Instance.GetItemDataCell(item.BaseID);
                    long useID = cfg.GetUseID();
                    if (useID > 0)
                    {
                        var useCfg = LocalDataManager.Instance.GetItemUseDataCell(useID);
                        if (useCfg != null)
                        {
                            int groupID = useCfg.GetGroup();
                            if (groupID == info.Key)
                            {
                                item.CD = info.Value;
                            }
                        }

                    }
                }
            }
        }

        public void Release()
        {

        }



    }

}