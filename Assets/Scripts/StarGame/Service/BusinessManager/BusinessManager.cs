using DG.Tweening;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using SkillEditor;
using StarProject.ArtHelper;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.Skill.Utils;
using StarProject.Module;
using StarProject.Service.Battle;
using StarProject.Service.LocalData;
using StarProject.Service.ServerService;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Service.Business
{
    [XLua.LuaCallCSharp]
    public class BusinessManager : ServiceModule<BusinessManager>
    {
        private string LOG_TAG = "BusinessManager";

        public EntityCtrlBase M_MainPlayerCtrlBase
        {
            get { return GameManager.Instance.M_MainPlayerCtrlBase; }
        }

        public ulong GetUserPID()
        {
            return StarProject.Service.User.UserManager.Instance.MainUserData.playerRoleId;
        }

        public ulong GetAccountID()
        {
            return StarProject.Service.User.UserManager.Instance.MainUserData.accountID;
        }

        public uint GetAreaID()
        {
            return StarProject.Service.User.UserManager.Instance.MainUserData.groupID;
        }

        public string GetAreaName()
        {
            return StarProject.Service.User.UserManager.Instance.MainUserData.groupName;
        }

        public string GetChannelID()
        {
            // if(SDK.SDKManager.Instance.sdkChannelInfo != null)
            // {
            //     return SDK.SDKManager.Instance.sdkChannelInfo.channelId;
            // }
            return "0";
        }

        private BankPlayerMD m_BankPlayerMD;

        public BankPlayerMD M_BankPlayerMD
        {
            get
            {
                if (m_BankPlayerMD == null)
                {
                    var md = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.BankPlayer) as BankPlayerMDMgr;
                    if (md != null)
                    {
                        m_BankPlayerMD = md.GetBankPlayerMD();
                    }
                }

                return m_BankPlayerMD;
            }
        }

        private UserSundryMD m_UserSundryMD;

        public UserSundryMD M_UserSundryMD
        {
            get
            {
                if (m_UserSundryMD == null)
                {
                    var md = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.UserSundry) as UserSundryMDMgr;
                    if (md != null)
                    {
                        m_UserSundryMD = md.GetUserSundryMD();
                    }
                }

                return m_UserSundryMD;
            }
        }

        /// <summary>
        /// 生活技能差量数据数据
        /// </summary>
        private LifeSkillUserDataMDMgr m_LifeSkillUserDataMDMgr;

        public LifeSkillUserDataMDMgr M_LifeSkillUserDataMDMgr
        {
            get
            {
                if (m_LifeSkillUserDataMDMgr == null)
                {
                    m_LifeSkillUserDataMDMgr =
                        FixMessageManager.Instance.GetMDMgr(FixUpdateDef.UserLifeSkills) as LifeSkillUserDataMDMgr;
                }

                return m_LifeSkillUserDataMDMgr;
            }
        }

        public LifeSkillUserDataMD GetLifeJobSkill(int jobid)
        {
            if (M_LifeSkillUserDataMDMgr != null)
            {
                var info = M_LifeSkillUserDataMDMgr.GetData(jobid);
                return info;
            }

            return null;
        }


        public int DigTreasureEndNtfCacheType = -1;

        /// <summary>
        /// 判断自己是否拥有以下权限
        /// </summary>
        /// <param name="powerID"></param>
        /// <returns></returns>
        public bool IsHaveGuildPower(int powerID)
        {
            /*
            1 = 审批入会
        
            2 = 踢人
        
            3 = 禁言
        
            4 = 任职
        
            5 = 冒险团改名、修改公告、修改宣言、修改标签
        
            8 = 修改招人等级
        
            10 = 邀请入会
            */

            //没有冒险团
            if (GetGuildID() == 0)
            {
                return false;
            }

            int posLevelID = GetGuildPosLevel();

            var powerList = LocalDataManager.Instance.GetGuildTitleDataCell(posLevelID).Power;

            for (int i = 0; i < powerList.Count; i++)
            {
                if (powerList[i] == powerID)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetMineUseCount(int mapID, int mineIndex)
        {
            if (M_UserSceneLogicDataMD != null)
            {
                var data = M_UserSceneLogicDataMD.GatherData;
                if (data != null)
                {
                    if (data.MapData.ContainsKey(mapID))
                    {
                        if (data.MapData[mapID].Mines != null)
                        {
                            if (data.MapData[mapID].Mines.ContainsKey(mineIndex))
                            {
                                return data.MapData[mapID].Mines[mineIndex].UsedReserve;
                            }
                        }
                    }
                }
            }

            return 0;
        }


        /// <summary>
        ///  获取服务器可见的NPC
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <param name="configID">配置表ID</param>
        /// <returns></returns>
        public bool GetServerNpcVisiable(int mapID, int configID, out bool isFind)
        {
            bool result = false;
            isFind = false;
            if (M_UserSceneLogicDataMD != null)
            {
                var data = M_UserSceneLogicDataMD.ShowHideData;
                if (data != null)
                {
                    if (data.MapNpcHideData.ContainsKey(mapID))
                    {
                        if (data.MapNpcHideData[mapID].HideMap.ContainsKey(configID))
                        {
                            isFind = true;
                            result = !data.MapNpcHideData[mapID].HideMap[configID];
                            //SGF.Debuger.Log($"服务器记录{mapID}地图 NpcConfigID={configID} 结果={result}");
                        }
                    }

                    //if (!bfind)
                    //{
                    //    var cfg = LocalDataManager.Instance.GetNPCDataCell(configID);
                    //    if (cfg != null)
                    //    {
                    //        result = cfg.GetIsShow();
                    //        SGF.Debuger.Log($"服务器没有记录 NpcConfigID={configID} 默认配置 结果={result}");
                    //    }
                    //    else
                    //    {
                    //        SGF.Debuger.Log($"读取配置失败NpcConfigID={configID}");
                    //    }
                    //}
                }
            }

            return result;
        }

        public bool GetServerNpcVisiable(int mapID, int configID)
        {
            bool result = false;
            bool bfind = false;
            if (M_UserSceneLogicDataMD != null)
            {
                var data = M_UserSceneLogicDataMD.ShowHideData;
                if (data != null)
                {
                    if (data.MapNpcHideData.ContainsKey(mapID))
                    {
                        if (data.MapNpcHideData[mapID].HideMap.ContainsKey(configID))
                        {
                            bfind = true;
                            result = !data.MapNpcHideData[mapID].HideMap[configID];
                            //SGF.Debuger.Log($"服务器记录{mapID}地图 NpcConfigID={configID} 结果={result}");
                        }
                    }

                    if (!bfind)
                    {
                        var cfg = LocalDataManager.Instance.GetNPCDataCell(configID);
                        if (cfg != null)
                        {
                            result = cfg.GetIsShow();
                            SGF.Debuger.Log($"服务器没有记录 NpcConfigID={configID} 默认配置 结果={result}");
                        }
                        else
                        {
                            SGF.Debuger.Log($"读取配置失败NpcConfigID={configID}");
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///  获取服务器可见的物件
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <param name="index">唯一索引</param>
        /// <returns></returns>
        public bool GetServerMineVisiable(int mapID, int configID, out bool isFind)
        {
            bool result = false;
            isFind = false;
            if (M_UserSceneLogicDataMD != null)
            {
                var data = M_UserSceneLogicDataMD.ShowHideData;
                if (data != null)
                {
                    isFind = false;
                    if (data.MapInterHideData.ContainsKey(mapID))
                    {
                        if (data.MapInterHideData[mapID].HideMap.ContainsKey(configID))
                        {
                            isFind = true;
                            result = !data.MapInterHideData[mapID].HideMap[configID];
                        }
                    }

                    //if (!bfind)
                    //{
                    //    var cfg = LocalDataManager.Instance.GetInteractDataCell(configID);
                    //    if (cfg != null)
                    //    {
                    //        result = cfg.GetIsShow();
                    //        SGF.Debuger.Log($"服务器没有记录 InterID={configID} 默认配置 结果={result}");
                    //    }
                    //    else
                    //    {
                    //        SGF.Debuger.Log($"读取配置失败InterID={configID}");
                    //    }
                    //}
                }
            }

            return result;
        }

        public bool GetServerMineVisiable(int mapID, int configID)
        {
            bool result = false;
            bool isFind = false;
            if (M_UserSceneLogicDataMD != null)
            {
                var data = M_UserSceneLogicDataMD.ShowHideData;
                if (data != null)
                {
                    isFind = false;
                    if (data.MapInterHideData.ContainsKey(mapID))
                    {
                        if (data.MapInterHideData[mapID].HideMap.ContainsKey(configID))
                        {
                            isFind = true;
                            result = !data.MapInterHideData[mapID].HideMap[configID];
                        }
                    }

                    if (!isFind)
                    {
                        var cfg = LocalDataManager.Instance.GetInteractDataCell(configID);
                        if (cfg != null)
                        {
                            result = cfg.GetIsShow();
                            SGF.Debuger.Log($"服务器没有记录 InterID={configID} 默认配置 结果={result}");
                        }
                        else
                        {
                            SGF.Debuger.Log($"读取配置失败InterID={configID}");
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 地图玩家数据
        /// </summary>
        private UserSceneLogicDataMD m_UserSceneLogicDataMD;

        public UserSceneLogicDataMD M_UserSceneLogicDataMD
        {
            get
            {
                if (m_UserSceneLogicDataMD == null)
                {
                    var md =
                        FixMessageManager.Instance.GetMDMgr(FixUpdateDef.UserSceneLogicData) as UserSceneLogicDataMDMgr;
                    if (md != null)
                    {
                        m_UserSceneLogicDataMD = md.GetUserMapData();
                    }
                }

                return m_UserSceneLogicDataMD;
            }
        }


        public void OnBeginCreating(int job, string animation)
        {
            Game.Player.PlayerCtrlGroup mainPlayerCtrl =
                (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (mainPlayerCtrl != null)
            {
                mainPlayerCtrl.OnBeginCreating(job, animation);
            }
        }

        public void OnEndCreating()
        {
            Game.Player.PlayerCtrlGroup mainPlayerCtrl =
                (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (mainPlayerCtrl != null)
            {
                mainPlayerCtrl.OnEndCreating();
            }
        }

        public bool IsCreating()
        {
            Game.Player.PlayerCtrlGroup mainPlayerCtrl =
                (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (mainPlayerCtrl != null)
            {
                return mainPlayerCtrl.IsCreating();
            }

            return false;
        }

        #region 道具相关

        //是否初始化所有道具
        public bool IsInitAllItem = false;

        /// <summary>
        /// 获取服务器道具
        /// </summary>
        /// <param name="ItemInfo"></param>
        /// <returns></returns>
        //public List<StarProject.Module.ItemData> GetItemEntitys(
        //    RepeatedField<global::ProtoMsg.ItemPropSyncList> ItemInfo)
        //{
        //    //List<StarProject.Module.ItemData> items = new();
        //    //for (int i = 0; i < ItemInfo.Count; i++)
        //    //{
        //    //    ProtoMsg.ItemPropSyncList it = ItemInfo[i];
        //    //    StarProject.Module.ItemData itemData = CreateItemData(it, it.ItemEntityID);
        //    //    items.Add(itemData);
        //    //}

        //    return items;
        //}

        /// <summary>
        /// 获得货币数量
        /// </summary>
        /// <returns></returns>
        public long GetCurrencyNum(long id, bool onlyBind = false)
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null ||
                M_MainPlayerCtrlBase.Data.Attrs == null)
            {
                return 0;
            }

            if (id == 1)
            {
                return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Silver);
            }
            else if (id == 2)
            {
                return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Coin);
            }
            else if (id == 3)
            {
                return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.ChargeDiamond);
            }
            else if (id == 4)
            {
                return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.BindDiamond);
            }
            else if (id == 116)
            {
                return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.CollectEnergy);
            }
            else
            {
                //夏崇尚定的规则：
                //道具配置表没有Dealid 表示 就是非绑定的道具
                //道具配置表有Dealid 那么第一列就是非绑定的道具，第二列是非绑定道具
                //服务器只下发非绑定道具的配置表id
                //配置表消耗如果填的是非绑定id那么就是找绑定和非绑定的道具数量
                //配置表消耗如果填的是绑定id那么就是找绑定的道具数量(没这规则)

                long num = 0;

                var items = GetAllItemsByItemBaseId(id);
                for (int i = 0; i < items.Count; i++)
                {
                    num = num + items[i].Num;
                }

                return num;
                /*
                if (num>0)
                {
                    //表示传了绑定的道具（找全部）
                    return num;
                }
                else
                {
                    //表示传了绑定的道具（只找绑定）
                    var cfg= LocalDataManager.Instance.GetItemDataCell(id);
                    if(cfg!=null)
                    {
                        long cfgId = cfg.GetDealid();
                        if (cfgId > 0)
                        {
                            var it = GetBindItemByItemBaseId(cfgId);
                            if (it != null)
                            {
                                return it.Num;
                            }

                        }
                    }

                    

                }

                */


                /*
                if (!onlyBind)
                {
                    long num = 0;
                    //仅仅找绑定的
                    var items = GetAllItemsByItemBaseId(id);
                    for (int i = 0; i< items.Count; i++)
                    {
                        num = num + items[i].Num;
                    }
                    return num;
                }
                else
                {

                    var it = GetItemByItemBaseId(id);
                    if (it != null)
                    {
                        return it.Num;
                    }

                }
                */
            }

            //return 0;
        }

        /// <summary>
        /// 得到 道具 item  是否 足够
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool IsEnoughCurrency(long itemID, int count)
        {
            long curCount = GetCurrencyNum(itemID);

            return curCount >= count;
        }


        public void ClearData()
        {
            itemMDMgr = null;
            partnerMDMgr = null;
            m_BankPlayerMD = null;
            m_UserSundryMD = null;
            m_LifeSkillUserDataMDMgr = null;
            m_UserSceneLogicDataMD = null;
        }

        private ItemMDMgr itemMDMgr;

        public ItemMDMgr GetItemMDMgr()
        {
            if (itemMDMgr == null)
            {
                itemMDMgr = (ItemMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Items);
            }

            return itemMDMgr;
        }

        private PartnerMDMgr partnerMDMgr;

        public PartnerMDMgr GetPartnerMDMgr()
        {
            //if (partnerMDMgr == null)
            //{
            //    partnerMDMgr = (PartnerMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Partner);
            //}

            return (PartnerMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Partner);
        }

        public ProtoMsg.ItemMD GetHeroEquipedAmuletByEntity(ulong entityID)
        {
            var equip = GetItemByItemEntityId(entityID);
            var cfg = LocalDataManager.Instance.GetItemDataCell(equip.BaseID);
            return GetHeroEquipedAmulet(cfg.GetSubType());
        }

        public ProtoMsg.ItemMD GetHeroEquipedAmulet(int subId)
        {
            ulong playerRoleId = Service.User.UserManager.Instance.MainUserData.playerRoleId; //玩家的唯一ID不是实体ID

            return GetItemMDMgr().GetHeroEquipedAmulet(playerRoleId, subId);
        }

        public ProtoMsg.ItemMD GetHeroEquipedEquipByEntity(ulong entityID)
        {
            var equip = GetItemByItemEntityId(entityID);
            var cfg = LocalDataManager.Instance.GetItemDataCell(equip.BaseID);
            return GetHeroEquipedEquip(cfg.GetSubType());
        }

        /// <summary>
        /// 判断装备是否可以装备
        /// </summary>
        /// <returns></returns>
        public bool CheckIsEquipCanEquiped(int baseID)
        {
            return GetItemMDMgr().CheckIsEquipCanEquiped(baseID);
        }

        /// <summary>
        /// 判断是否有可以装备的装备
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckIsHaveCanEquipedEquip(int type)
        {
            return GetItemMDMgr().CheckIsHaveCanEquipedEquip(type);
        }


        /// <summary>
        /// 检查身上是否有可以装备的装备
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public void CheckIsHaveCanEquipedEquipRedpoint()
        {

            for (int i = 1; i <= 8; i++)
            {
                var equip = GetHeroEquipedEquip(i);
                if (equip == null)
                {
                    bool isHave = CheckIsHaveCanEquipedEquip(i);
                    if (isHave)
                    {
                        RedPointManager.Instance.RefreshRedPointCount(StarProjectDef.RedPointType.Bag_Uequiped, 1);
                        //RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.Bag_Uequiped, true);
                        return;
                    }
                }
            }

            RedPointManager.Instance.RefreshRedPointCount(StarProjectDef.RedPointType.Bag_Uequiped, 0);
            //RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.Bag_Uequiped, false);
        }


        public ProtoMsg.ItemMD GetHeroEquipedEquip(int type)
        {
            ulong playerRoleId = Service.User.UserManager.Instance.MainUserData.playerRoleId; //玩家的唯一ID不是实体ID
            return GetItemMDMgr().GetHeroEquipedEquip(playerRoleId, type);
        }

        public ProtoMsg.ItemMD GetPartnerEquipedEquip(ulong entityID, int type)
        {
            return GetItemMDMgr().GetPartnerEquipedEquip(entityID, type);
        }

        public List<ProtoMsg.ItemMD> GetItemsByItemType(int type, int subtype)
        {
            return GetItemMDMgr().GetItemsByItemType(type, subtype);
        }

        public List<ProtoMsg.ItemMD> GetDescendItemsByItemType(int type, int subtype)
        {
            var items = GetItemsByItemType(type, subtype);
            items.Sort((item1, item2) => item2.BaseID.CompareTo(item1.BaseID));
            return items;
        }

        public List<ProtoMsg.ItemMD> GetItemsByItemSpaceId(int id)
        {
            return GetItemMDMgr().GetItemsByItemSpaceId(id);
        }

        /// <summary>
        /// 获取所有没有检查快捷穿戴的装备
        /// </summary>
        /// <returns></returns>
        public List<ProtoMsg.ItemMD> GetAllNotCheckQuickEquip()
        {
            return GetItemMDMgr().GetAllNotCheckQuickEquip();
        }

        public void RemoveItemsBySpaceId(int id)
        {
            GetItemMDMgr().RemoveItemsBySpaceId(id);
        }

        //获取所有可以分解的装备
        public List<ProtoMsg.ItemMD> GetItemsCanResolve()
        {
            return GetItemMDMgr().GetItemsCanResolve();
        }


        public ProtoMsg.ItemMD GetItemByItemEntityId(ulong id)
        {
            return GetItemMDMgr().GetItemByItemEntityId(id);
        }


        public ProtoMsg.ItemMD GetItemByItemBaseId(long jid)
        {
            return GetItemMDMgr().GetItemByItemBaseId(jid);
        }

        public ProtoMsg.ItemMD GetNoBindItemByItemBaseId(long jid)
        {
            return GetItemMDMgr().GetNoBindItemByItemBaseId(jid);
        }

        public ProtoMsg.ItemMD GetBindItemByItemBaseId(long jid)
        {
            return GetItemMDMgr().GetBindItemByItemBaseId(jid);
        }

        public List<ProtoMsg.ItemMD> GetAllItemsByItemBaseId(long jid)
        {
            return GetItemMDMgr().GetAllItemsByItemBaseId(jid);
        }

        public void ClearTempBagItems()
        {
            GetItemMDMgr().ClearTempBagItems();
        }

        /// <summary>
        /// TODO: 胡哥哥 
        /// 根据 ItemPropSyncList 创建 一个 ItemData 实例, 每次都是New 出来的 , 后续可以考虑弄个 工厂和 池
        /// </summary>
        /// <param name="itemPropSyncList"></param>
        /// <returns></returns>
        //public Module.ItemData CreateItemData(ProtoMsg.ItemPropSyncList itemPropSyncList, ulong EItemID)
        //{
        //    return new Module.ItemData(itemPropSyncList, EItemID);
        //}
        public void UnNewAllItems()
        {
            GetItemMDMgr().UnNewAllItems();
        }

        public bool IsHaveNewItem()
        {
            return GetItemMDMgr().IsHaveNew();
        }

        public void SendUseItem(ulong eid, int spaceId, int count)
        {
            ProtoMsg.ItemUseReq itemUseReq = new();
            itemUseReq.ItemEntityID = eid;
            itemUseReq.ItemSpaceId = spaceId;
            itemUseReq.Count = count;

            NetworkManager.Instance.battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, itemUseReq, false);
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, itemUseReq, false);
        }

        /// <summary>
        /// 判断是否
        /// </summary>
        /// <param name="quality"></param>
        /// <param name="num"></param>
        public bool CheckWearEquipQualityCount(int quality, int num)
        {
            int matchNum = GetWearEquipQualityCount(quality);

            return matchNum >= num;
        }

        /// <summary>
        /// 获得总计强化等级
        /// </summary>
        public int GetWearEquipTotalLv()
        {
            int totalLv = 0;
            int maxEquipNum = 10; //装备槽位上限，之后读表
            for (int i = 0; i < maxEquipNum; i++)
            {
                var equipSlotData = GameManager.Instance.GetEquipSlotData(i + 1);
                if (equipSlotData != null)
                {
                    totalLv += equipSlotData.UpLv;
                }
            }

            return totalLv;
        }


        /// <summary>
        /// 获取某种类型,子类型的道具总量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="SubType"></param>
        /// <returns></returns>
        public int GetItemNumByType(int type, int subtype = 0)
        {
            int totalNum = 0;

            totalNum = GetItemMDMgr().GetItemNumByType(type, subtype);

            return totalNum;
        }

        /// <summary>
        /// 获取某个背包里的道具数量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="SubType"></param>
        /// <returns></returns>
        public int GetItemNumBySpaceID(int spaceid)
        {
            int totalNum = 0;

            totalNum = GetItemMDMgr().GetItemNumBySpaceID(spaceid);

            return totalNum;
        }

        public int GetWearEquipQualityCount(int quality)
        {
            var wearItems = GetItemsByItemSpaceId(1);

            int matchNum = 0;

            for (int i = 0; i < wearItems.Count; i++)
            {
                var equip = wearItems[i];
                var cfg = LocalDataManager.Instance.GetItemDataCell(equip.BaseID);
                if (cfg.GetQuality() >= quality)
                {
                    matchNum++;
                }
            }

            return matchNum;
        }

        #endregion

        #region 环任务

        private RingTaskModule ringTaskModule;

        /// <summary>
        /// 获取所有环任务
        /// </summary>
        /// <returns></returns>
        public RepeatedField<int> GetAllRingTaskIDs()
        {
            //if (ringTaskModule == null)
            {
                ringTaskModule = ModuleManager.Instance.GetModule(ModuleDef.Name.RingTaskModule) as RingTaskModule;
            }


            return ringTaskModule.GetTaskIDs();
        }

        public bool IsGetRingTaskExRew()
        {
            //if (ringTaskModule == null)
            {
                ringTaskModule = ModuleManager.Instance.GetModule(ModuleDef.Name.RingTaskModule) as RingTaskModule;
            }

            return ringTaskModule.CheckIsGetExRew();
        }

        //发送领取环奖励请求
        public void RingTaskExReq()
        {
            var message = new ProtoMsg.RingTaskExReq();
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, message, false);
        }

        #endregion

        #region 工会相关

        /// <summary>
        /// 获取公会名
        /// </summary>
        /// <returns></returns>
        public string GetGuildName()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return "";
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.PlayerGuildInfo);
            if (info == null)
            {
                return "";
            }
            else
            {
                ProtoMsg.PlayerGuildInfo lGuildInfoMD = (ProtoMsg.PlayerGuildInfo)info;
                return lGuildInfoMD.GuName;
            }
        }

        /// <summary>
        /// 获得当前加入的工会ID
        /// </summary>
        /// <returns></returns>
        public ulong GetGuildID()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.PlayerGuildInfo);
            if (info == null)
            {
                return 0;
            }
            else
            {
                ProtoMsg.PlayerGuildInfo lGuildInfoMD = (ProtoMsg.PlayerGuildInfo)info;
                return lGuildInfoMD.GuildID;
            }
        }

        /// <summary>
        /// 获得当前加入的工会职位
        /// </summary>
        /// <returns></returns>
        public int GetGuildPosLevel()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.PlayerGuildInfo);
            if (info == null)
            {
                return 0;
            }
            else
            {
                ProtoMsg.PlayerGuildInfo lGuildInfoMD = (ProtoMsg.PlayerGuildInfo)info;
                return (int)lGuildInfoMD.GuPlayer.PlayerPosLv;
            }
        }

        /// <summary>
        /// 获得当前加入的工会等级
        /// </summary>
        /// <returns></returns>
        public int GetGuildLevel()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.PlayerGuildInfo);
            if (info == null)
            {
                return 0;
            }
            else
            {
                ProtoMsg.PlayerGuildInfo lGuildInfoMD = (ProtoMsg.PlayerGuildInfo)info;
                return lGuildInfoMD.GuLevel;
            }
        }

        /// <summary>
        /// 获得工会禁言结束的时间
        /// </summary>
        /// <returns></returns>
        public long GetGuildProhibEndTime()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.PlayerGuildInfo);
            if (info == null)
            {
                return 0;
            }
            else
            {
                ProtoMsg.PlayerGuildInfo lGuildInfoMD = (ProtoMsg.PlayerGuildInfo)info;
                return lGuildInfoMD.GuPlayer.BanChatTime;
            }
        }

        /// <summary>
        /// 退出公会时间
        /// </summary>
        /// <returns></returns>
        public long GetOutGuildTime()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.OutGuildTime);
        }

        #endregion

        #region 交互相关

        public bool InEctype()
        {
            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as EctypeModule;
            if (module != null)
            {
                return module.InEctype();
            }

            return false;
        }


        /// <summary>
        /// 更新每日提醒
        /// </summary>
        public void SaveNotice(string key)
        {
            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.PlayerLocalCache) as PlayerLocalCache;
            if (module != null)
            {
                module.SaveNotice(key);
            }
        }

        /// <summary>
        /// 是否存在每日提醒
        /// </summary>
        /// <returns></returns>
        public bool HasNotice(string key)
        {
            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.PlayerLocalCache) as PlayerLocalCache;
            if (module != null)
            {
                return module.HasNotice(key);
            }

            return false;
        }


        /// <summary>
        /// 获取NPC其他服务
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public List<ServerService.ServerService> GetNpcOtherService(uint npcID)
        {
            var services = ServerServiceManager.Instance.GetServices(ServerServiceType.NPC, npcID);

            return services;
        }

        public List<ServerService.ServerService> GetObjOtherService(uint npcID)
        {
            var services = ServerServiceManager.Instance.GetServices(ServerServiceType.InterAction, npcID);

            return services;
        }


        /// <summary>
        /// 判断NPC是否在服务
        /// </summary>
        /// <returns></returns>
        public bool IsNpcCanService(uint baseID)
        {
            return GetNpcOtherService(baseID).Count > 0;
        }

        public EctypeData EctypeTargetData()
        {
            EctypeModule module = ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as EctypeModule;
            if (module != null)
            {
                return module.EctypeTargetData;
            }

            return null;
        }

        public bool IsObjectCanService(uint baseID)
        {
            return GetObjOtherService(baseID).Count > 0;
        }

        public void SetNpcIsVisiable(GameNPCCtrlGroup entity)
        {
            uint npcBaseID = (entity.M_Curr as GameNPCEntityBase).ConfigIndex;
            var npcCfg = LocalDataManager.Instance.GetNPCDataCell(npcBaseID);
            if (npcCfg == null)
            {
                return;
            }

            int showType = npcCfg.GetShowType();

            //条件显示
            if (showType == 0)
            {
                entity.SetVisiable(GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition()));
            }
            //服务显示
            else if (showType == 1)
            {
                entity.SetVisiable(IsNpcCanService(npcBaseID));
            }
            //条件或服务显示
            else if (showType == 2)
            {
                entity.SetVisiable(GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition()) ||
                                   IsNpcCanService(npcBaseID));
            }
        }

        public bool GetNPCVisiable(long npcConfigID)
        {
            bool isShow = false;
            var npcCfg = LocalDataManager.Instance.GetNPCDataCell(npcConfigID);
            if (npcCfg == null)
            {
                return isShow;
            }

            int showType = npcCfg.GetShowType();

            //条件显示
            if (showType == 0)
            {
                isShow = GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition());
            }
            //服务显示
            else if (showType == 1)
            {
                isShow = IsNpcCanService((uint)npcConfigID);
            }
            //条件或服务显示
            else if (showType == 2)
            {
                isShow = GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition()) ||
                       IsNpcCanService((uint)npcConfigID);
            }

            bool isFind = false;
            bool temp = GetServerNpcVisiable(GameManager.Instance.GetCurMapId(), (int)npcConfigID, out isFind);
            if (isFind)
            {
                isShow = temp;
            }

            return isShow;
        }

        public void SetObjectIsVisiable(ObjectCtrlGroup entity)
        {
            int npcBaseID = entity.ConfigID;
            var npcCfg = LocalDataManager.Instance.GetInteractDataCell(npcBaseID);
            if (npcCfg == null)
            {
                SGF.Debuger.LogError($"{LOG_TAG} SetObjectIsVisiable npcBaseID={npcBaseID},npcCfg=null,,,err!!!");
                return;
            }

            int showType = npcCfg.GetShowType();


            bool hasservice = IsObjectCanService((uint)npcBaseID);

            //entity
            //条件显示
            if (showType == 0)
            {
                entity.SetVisiable(GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition(), entity.Index));
            }
            //服务显示
            else if (showType == 1)
            {
                entity.SetVisiable(hasservice);
            }
            //条件或服务显示
            else if (showType == 2)
            {
                entity.SetVisiable(GameManager.Instance.IsConditionMete(npcCfg.GetShowCondition(), entity.Index) ||
                                   hasservice);
            }

            entity.OnServiceChange(hasservice);
        }

        #endregion

        #region 通用消耗

        private InterActionModule _InterActionModule;

        private InterActionModule m_InterActionModule
        {
            get
            {
                //if (_InterActionModule == null)
                //{
                //    _InterActionModule = ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
                //}

                //return _InterActionModule;

                return ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
            }
        }

        public bool HasCostNotice()
        {
            if (m_InterActionModule != null)
            {
                return m_InterActionModule.HasCostNotice();
            }

            return false;
        }

        /// <summary>
        /// 更新存储
        /// </summary>
        public void SaveCostNotice()
        {
            if (m_InterActionModule != null)
            {
                m_InterActionModule.SaveCostNotice();
            }
        }

        /// <summary>
        /// 打开通用消耗界面
        /// </summary>
        /// <param name="costID">消耗ID</param>
        /// <param name="sureCallBack">确定回调</param>
        /// <param name="cancelCallBack">取消回调</param>
        public void OpenCostWidget(int costID, System.Action sureCallBack = null, System.Action cancelCallBack = null)
        {
            //UIManager.Instance.OpenWidget("InterAction/CommonCostWidget", false, new object[] { costID, sureCallBack, cancelCallBack });
            UIManager.Instance.OpenWidgetAsync(UIDef.CommonCostWidget, null, false,
                new object[] { costID, sureCallBack, cancelCallBack });
        }

        #endregion

        #region 业务模型展示相关

        public GameObject ShowModel(int index, int avatarID, Vector3 localPos, string defaultAnim)
        {
            AvatarDataCell avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(avatarID);
            GameObject obj =
                Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(UIDef.DisplayModelPath,
                    E_AssetType.Roles);
            obj.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
            var modelViewDisplay = obj.GetComponent<RoleViewDisplay>();
            //string animPath = "idle_01";
            UIManager.Instance.SetModel2UI2(
                modelViewDisplay.gameObject,
                avatarDataCell,
                ThreeDModelPos.Raw,
                scale: Vector3.one,
                WorleRotate: Vector3.zero,
                localRotate: Vector3.zero,
                localPos: localPos,
                defaultAnim
            );

            obj.SetActive(true);
            return obj;
        }

        public void CloseModel(int index, GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            GameObject.Destroy(obj);
            // obj.SetActive(false);
        }

        //public GameObject ShowBgModel(string path, Vector3 pos, Vector3 rot, Vector3 scale)
        //{
        //    GameObject obj = StarProject.Service.Resource.ResourceManager.Instance.LoadGameObject(path);
        //    UIManager.Instance.SetBgModel(obj, pos, rot, scale);
        //    obj.SetActive(true);
        //    return obj;
        //}

        public void CloseBgModel(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            GameObject.Destroy(obj);
        }

        public void CloseModel2UI(List<GameObject> objs)
        {
            if (objs == null)
            {
                return;
            }

            foreach (var obj in objs)
            {
                obj.SetActive(false);
            }
        }

        #endregion

        #region 药品相关

        public NPCEntityBase GetMainPlayerEntity()
        {
            Game.Player.PlayerCtrlGroup m_MainPlayerCtrl =
                (Game.Player.PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            return (NPCEntityBase)m_MainPlayerCtrl.M_Curr;
        }

        public List<bool> GetUserSundryData(ProtoMsg.BaseBinary data)
        {
            List<bool> r = new();
            var arr = data.Data.ToByteArray();
            for (int i = 0; i < arr.Length; i++)
            {
                var d = arr[i];
                int tmp = 1;
                for (int j = 0; j < 8; j++)
                {
                    r.Add((d & tmp) >= 1);
                    tmp *= 2;
                }
            }

            return r;
        }

        public void RefreshMedicineBtn()
        {
            GameInput.Instance?.RefreshMedicineBtn();
        }

        #endregion

        #region 客户端NPC

        public void ResumeLast(int Index)
        {
            ClientNpc.ClientNpcManager.Instance.ResumeLast(Index);
        }

        public void TranslateState(int Index, int state)
        {
            ClientNpc.ClientNpcManager.Instance.TranslateState(Index, (ClientNpc.StateEnum)state);
        }

        #endregion

        #region 寻路相关

        /// <summary>
        /// 转向实体
        /// </summary>
        /// <param name="entityID"></param>
        public void TurnToEntity(ulong entityID)
        {
            var entity = GameManager.Instance.GetEntityCtr(entityID);
            if (entity == null)
            {
                return;
            }

            Vector3 pos = Vector3.zero;
            if (entity.EntityType == E_EntityType.Interact)
            {
                ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                if (ctrlGroup != null)
                {
                    pos = ctrlGroup.GetPosition();
                }
                else
                {
                    pos = GameManager.Instance.GetEntityPosById(entityID);
                }

                Vector3 playerPos = GameManager.Instance.GetEntityPosById(GameManager.Instance.mainPlayerId);
                playerPos.y = pos.y;
                Vector3 dir = pos - playerPos;
                dir.Normalize();
                M_MainPlayerCtrlBase.M_Curr.ClientSetRotationByDir(dir, true, 0);
            }
        }

        /// <summary>
        /// 走到实体面前
        /// </summary>
        /// <param name="entityID"></param>
        public void MoveToEntity(ulong entityID, Action<bool> onfinish = null)
        {
            var entity = GameManager.Instance.GetEntityCtr(entityID);
            if (entity == null)
            {
                return;
            }

            Vector3 pos = Vector3.zero;
            if (entity.EntityType == E_EntityType.Interact)
            {
                ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                if (ctrlGroup != null)
                {
                    pos = ctrlGroup.GetPosition();
                }
            }
            else
            {
                pos = GameManager.Instance.GetEntityPosById(entityID);
            }

            FindPath(pos, onfinish);
        }

        /// <summary>
        /// 寻找路径
        /// </summary>
        /// <param name="postion"></param>
        public bool FindPath(Vector3 postion, Action<bool> onfinish = null, float maxDistance = 1.0f)
        {
            if (M_MainPlayerCtrlBase != null)
            {
                return (M_MainPlayerCtrlBase as PlayerCtrlGroup).ClientNavFindPath(postion, maxDistance, onfinish);
            }

            return false;
        }

        public void FindNpcPath(int mapID, int npcID, float range = 1, System.Action<bool> action = null)
        {
            TaskHelper.FindPathByNpc(mapID, npcID, range, action);
        }

        public bool FindPathByPosition(Vector3 postion, float range = 1, System.Action<bool> action = null)
        {
            var MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (MainPlayer != null)
            {
                return MainPlayer.ClientNavFindPath(postion, range, action);
            }

            return false;
        }

        public bool FindPathByPosition(int mapID, Vector3 postion, float range = 1, System.Action<bool> action = null,
            bool isChanageFindPath = false, bool recordOnStateForbid = false)
        {
            var MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            if (MainPlayer != null)
            {
                TaskHelper.FindPostion(mapID, postion, range, action, isChanageFindPath, recordOnStateForbid);
                return true;
            }

            return false;
        }

        #region 秘境寻路

        // 秘境寻路到boss坐标点

        public void SecretAreaFindPathByChoseMonsterPos(System.Action<bool> action = null)
        {
            if (M_MainPlayerCtrlBase != null)
            {
                var curAtkEntity = SkillUtils.GetClosestTargetIdByTargets(M_MainPlayerCtrlBase.M_Curr.EntityId,
                    M_MainPlayerCtrlBase.M_Curr.Faction, M_MainPlayerCtrlBase.M_Curr.Position(), 10000,
                    SelectType.Enemy, false);
                if (curAtkEntity != null)
                {
                    Action<bool> cb = (bool res) =>
                    {
                        action?.Invoke(true);
                        // BattleManager.Instance.RecordAutoBattleStartPos(true);
                    };
                    FindPathByPosition(curAtkEntity.Position(), 1f, cb);
                    return;
                }
            }

            action?.Invoke(false);
        }

        /// <summary>
        /// 获取一个点周围的可以移动的点
        /// </summary>
        /// <param name="pos"></param>
        public Vector3 GetRandomPos(Vector3 pos, float radius = 3.0f)
        {
            float angles = UnityEngine.Random.Range(-45, 45) + 10;
            Vector3 point = Fire.Utils.PosMoveBySeverRota(pos, angles, radius);
            Vector3 outpos = pos;
            Service.FindPath.FindPathManager.Instance.FindValidPointNearby(point, out outpos);
            return outpos;
        }

        // 秘境寻路到boss坐标点
        public void SecretAreaFindPathByBossPos(System.Action<bool> action = null)
        {

            bool result = GetSecretBossPos(out Vector3 pos);

            if (!result)
            {
                action?.Invoke(false);
                return;
            }

            Action<bool> cb = (bool res) =>
            {
                action?.Invoke(true);
            };
            FindPathByPosition(pos, 1f, cb);
        }

        public bool GetSecretBossPos(out Vector3 pos)
        {
            pos = Vector3.zero;
            if (GameMap.sceneJsonData == null)
            {
                return false;
            }

            if (GameMap.sceneJsonData.RandomMonsters == null)
            {
                return false;
            }

            if (GameMap.sceneJsonData.RandomMonsters.Count <= 0)
            {
                return false;
            }

            foreach (var item in GameMap.sceneJsonData.RandomMonsters)
            {
                if (item.Value.MonsterType == 3)
                {
                    pos = item.Value.Position.Convert();
                    return true;
                }
            }

            return false;
        }
        public bool GetSecretAreaBuffPos(out Vector3 v3)
        {
            v3 = Vector3.zero;
            if (GameMap.sceneJsonData == null)
            {
                return false;
            }

            if (GameMap.sceneJsonData.Mines == null)
            {
                return false;
            }

            if (GameMap.sceneJsonData.Mines.Count <= 0)
            {
                return false;
            }

            foreach (var item in GameMap.sceneJsonData.Mines)
            {
                if (item.Value.MineID == 900000)
                {
                    v3 = item.Value.Position.Convert();
                    return true;
                }
            }

            return false;
        }

        // 秘境寻路到buff坐标点
        public void SecretAreaFindPathByBuffPos(System.Action<bool> action = null)
        {
            if (GameMap.sceneJsonData == null)
            {
                return;
            }

            if (GameMap.sceneJsonData.Mines == null)
            {
                return;
            }

            if (GameMap.sceneJsonData.Mines.Count <= 0)
            {
                return;
            }

            foreach (var item in GameMap.sceneJsonData.Mines)
            {
                if (item.Value.MineID == 900000)
                {
                    Action<bool> cb = (bool res) =>
                    {
                        action?.Invoke(true);
                        // BattleManager.Instance.RecordAutoBattleStartPos(true);
                    };
                    FindPathByPosition(item.Value.Position.Convert(), 1f, cb);
                    return;
                }
            }
            action?.Invoke(false);
        }

        // 秘境寻路到传送门坐标点
        public void SecretAreaFindPathByGatewayEntityPos(System.Action<bool> action = null)
        {
            var localEntityList = GameManager.Instance.M_LocalEntitys;
            if (localEntityList != null)
            {
                for (int i = 0; i < localEntityList.Count; i++)
                {
                    var child = localEntityList[i];
                    if (child != null && child.Type == E_LocalEntityType.Gateway)
                    {
                        Action<bool> cb = (bool res) =>
                        {
                            action?.Invoke(true);
                            // BattleManager.Instance.RecordAutoBattleStartPos(true);
                        };
                        FindPathByPosition(child.Position(), 0.5f, cb);
                        return;
                    }
                }
            }
            action?.Invoke(false);
        }

        #endregion

        #region 获取Y轴高度

        private Vector3 rayStartPoint;

        public float GetGroundHeight(float x, float z)
        {
            rayStartPoint.x = x;
            rayStartPoint.y = 200f;
            rayStartPoint.z = z;

            RaycastHit[] allHit = Physics.RaycastAll(rayStartPoint, Vector3.down);
            for (int i = 0; i < allHit.Length; i++)
            {
                var item = allHit[i];
                if (item.transform.gameObject.layer == (int)E_LayerType.Ground)
                {
                    return item.point.y;
                }
                //Debug.LogError($"y={item.point.y},layer={item.transform.gameObject.layer}");
            }

            return 0f;
        }

        #endregion

        #endregion

        /*1，同场景切换，loading要固定时间
        2，服务器有无敌状态
        3，客户端此时不会自动寻路
        4，除了波纹没有loading，其他策划全要loading
        ==============这些只是一部分，要案子。*/
        public void ShowLoading(float time = 1.0f, System.Action callBack = null)
        {
            GameManager.Instance.Loading.OnProcessStart();
            Tweener tweener = DOTween.To(() => 0, (v) =>
            {
                //GameManager.Instance.Loading.OnProcess(v); //Single
            }, time * 1.0f, time);
            tweener.onComplete = () =>
            {
                GameManager.Instance.Loading.OnProcessEnd(true);
                callBack?.Invoke();
            };
            ////GameManager.Instance.Loading.OnProcessStart();
            //GameManager.Instance.Loading.OnPlayBlackDOFade(true, 1.0f);
            //Tweener tweener = DOTween.To(() => 0, (v) =>
            //{
            //    //GameManager.Instance.Loading.OnProcess(v); //Single
            //}, time * 1.0f, time);
            //tweener.onComplete = () =>
            //{
            //    //GameManager.Instance.Loading.OnProcessEnd();
            //    GameManager.Instance.Loading.OnProcessBlackEnd();
            //    callBack?.Invoke();
            //};
        }

        #region 藏宝图

        /// <summary>
        /// 藏宝图数据
        /// </summary>
        /// <returns></returns>
        public ProtoMsg.TreasureData GetTreasureData()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return null;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.CurTreasure) as ProtoMsg.TreasureData;
        }


        //public ProtoMsg.TDDatasNtf GetTDDatasNtf()
        //{
        //    if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
        //    {
        //        return null;
        //    }
        //    return M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.CurTDInfo) as ProtoMsg.TDDatasNtf;
        //}

        /// <summary>
        /// 是否在挖宝藏
        /// </summary>
        /// <returns></returns>
        public bool IsTreasuring()
        {
            var data = GetTreasureData();
            if (data == null)
            {
                return false;
            }

            if (data.TreasureEID == 0)
            {
                return false;
            }

            if (data.TreasureMapID == 0)
            {
                return false;
            }

            if (data.TreasureInterID == 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 组队信息

        private ProtoMsg.TDEntryOpenNtf tDEntryOpenNtf = null; // 组队 玩法入口开启
        private Action<ProtoMsg.TDEntryOpenNtf> TDEntryOpenNtfChanageAction = null;

        private ProtoMsg.TDMemberJoinNtf tDMemberJoinNtf = null; // 组队日常本人数信息变化 通知
        private Action<ProtoMsg.TDMemberJoinNtf> TDMemberJoinNtfChanageAction = null;

        private void OnTeamInfoSrvNtf(MessageHandleData data)
        {
            TeamInfoSrvNtf msg = (TeamInfoSrvNtf)data.data;

            tDEntryOpenNtf = msg.TInfo.Entry;
            tDMemberJoinNtf = msg.TInfo.CallInfo;

            OnInformTeamAction();
        }

        private void OnSignOutTeamRet(MessageHandleData data)
        {
            SignOutTeamRet msg = (SignOutTeamRet)data.data;
            if (msg.Ret == SignOutTeamRet.Types.Result.Success)
            {
                //退出队伍清空入口信息
                if (msg.OutEid == GameManager.Instance.mainPlayerId)
                {
                    tDEntryOpenNtf = null;
                    TDEntryOpenNtfChanageAction?.Invoke(null);
                }
            }
        }

        private void OnInformTeamAction()
        {
            TDEntryOpenNtfChanageAction?.Invoke(tDEntryOpenNtf);
            TDMemberJoinNtfChanageAction?.Invoke(tDMemberJoinNtf);
        }

        public ProtoMsg.TDEntryOpenNtf GetTDEntryOpenNtf()
        {
            return tDEntryOpenNtf;
        }

        public ProtoMsg.TDMemberJoinNtf GetTDMemberJoinNtf()
        {
            return tDMemberJoinNtf;
        }

        public void RegisterTDEntryOpenNtfChanageAction(Action<ProtoMsg.TDEntryOpenNtf> action)
        {
            TDEntryOpenNtfChanageAction += action;
        }

        public void UnRegisterTDEntryOpenNtfChanageAction(Action<ProtoMsg.TDEntryOpenNtf> action)
        {
            TDEntryOpenNtfChanageAction -= action;
        }

        public void RegisterTDMemberJoinNtfChanageAction(Action<ProtoMsg.TDMemberJoinNtf> action)
        {
            TDMemberJoinNtfChanageAction += action;
        }

        public void UnRegisterTDMemberJoinNtfChanageAction(Action<ProtoMsg.TDMemberJoinNtf> action)
        {
            TDMemberJoinNtfChanageAction -= action;
        }

        #endregion

        #region 副本黑板UI显示消息

        private Dictionary<string, bool> m_XinSGStateDic = new();

        [XLua.BlackList]
        public bool GetXinSGStateDic(string key)
        {
            bool state = false;
            if (m_XinSGStateDic.TryGetValue(key, out state))
            {

            }
            return state;
        }

        [XLua.BlackList]
        public void SetXinSGStateDic(string key, bool val)
        {
            if (m_XinSGStateDic.ContainsKey(key))
            {
                m_XinSGStateDic[key] = val;
            }
            else
            {
                m_XinSGStateDic.Add(key, val);
            }
        }

        [XLua.BlackList]
        public void ClearXinSGStateDic()
        {
            m_XinSGStateDic.Clear();
        }

        #endregion


        public void Init()
        {
            OnEventMessage();
            GlobalEvent.OnBackLogin.AddListener(OnReLogin);
        }

        /// <summary>
        /// 返回登录
        /// </summary>
        /// <param name="arg0"></param>
        private void OnReLogin(AgainLoginType arg0)
        {
            //清理缓存
            ClearData();
        }

        private void OnEventMessage()
        {
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TeamInfoSrvNtfID, OnTeamInfoSrvNtf, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SignOutTeamRetID, OnSignOutTeamRet, this);
        }

        private void OffEventMessage()
        {
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TeamInfoSrvNtfID, OnTeamInfoSrvNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SignOutTeamRetID, OnSignOutTeamRet, this);
        }


        public bool CheckSysOpen(int cfgID)
        {
            var config = LocalDataManager.Instance.GetPlayOpenConditionsDataCell(cfgID);
            if (config != null)
            {
                if (config.GetPlayerGuidance() != 0)
                {
                    var cfg = LocalDataManager.Instance.GetGuidSysOpenDataCell(config.GetPlayerGuidance());
                    if (cfg != null)
                    {
                        //玩家等级
                        var playerLevel = GameManager.Instance.GetPlayerLevel();
                        if (cfg.GetNeedLv() > playerLevel)
                        {
                            return false;
                        }

                        if (GameManager.Instance.M_MainPlayerCtrlBase == null)
                        {
                            return false;
                        }

                        //冒险等级
                        var RiskInfo =
                            GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(StarProjectDef
                                .AOIAttrDefine.RiskInfo);
                        if (RiskInfo != null)
                        {
                            ProtoMsg.RiskLevelMD lRiskInfoMD = (ProtoMsg.RiskLevelMD)RiskInfo;
                            var curRiskLevel = lRiskInfoMD.CurRiskLevel;

                            if (cfg.GetNeedAdvLv() > curRiskLevel)
                            {
                                return false;
                            }
                        }


                        //任务完成情况
                        if (cfg.GetNeedQuestCom() > 0)
                        {
                            if (!TaskHelper.IsTaskFinsh((uint)cfg.GetNeedQuestCom()))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 系统是否开放
        /// </summary>
        /// <param name="systemOpenType"></param>
        /// <returns></returns>
        public bool SystemIsOpen(SystemOpenType systemOpenType)
        {
            return SystemOpenManager.Instance.SystemIsOpen(systemOpenType);
        }

        public void PlayWaveEffect()
        {
            Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetScreenShockyRenderPassFeature(true);
            GlobalEvent.OnPlayWaveEffect?.Invoke(true);
            DelayInvoker.DelayInvoke(this, 0.8f,
                (object[] args) =>
                {
                    Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetScreenShockyRenderPassFeature(false);
                    GlobalEvent.OnPlayWaveEffect?.Invoke(false);
                });
        }

        public override void Release()
        {
            base.Release();
            OffEventMessage();
        }


        public void EntityLevel(int id)
        {

            MapChangeReq mapChangeReqMsg = new();
            mapChangeReqMsg.MapID = id;
            mapChangeReqMsg.ServerID = 0;
            mapChangeReqMsg.Reason = ChangeReason.Instance;
            mapChangeReqMsg.SpType = SpaceType.SpaceDefault;
            mapChangeReqMsg.SpaceID = 0;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, mapChangeReqMsg, false);

        }


        public string ReplacePlayerName(string tex, string color = "f7cf46")
        {
            var targetID = GameManager.Instance.mainPlayerId;
            if (targetID > 0)
            {
                string playerName = GameManager.Instance.GetEntityNameById(targetID);
                if (string.IsNullOrEmpty(color))
                {
                    return tex.Replace("@player", playerName);
                }
                else
                {
                    var pname = string.Format("<color=#{0}>{1}</color>", color, playerName);
                    return tex.Replace("@player", pname);
                }

            }
            return tex;
        }
    }
}