using Google.Protobuf;
using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Service.LocalData;
using StarProjectDef;


namespace StarProject.Module
{

    [XLua.LuaCallCSharp]
    public class ItemBaseData
    {
        public long BaseID;//表格ID

        public int UpdatePoint;//强化点数

        public EquipPropList EquipProps;//装备属性

        public CombatSkillList CombatSkills;//战技

        public GemSlotList GemSlots;//护符纹章槽

        public int ExtractNum;//萃取次数

        public int Durability;//耐久度

        public bool IsBind;//是否已绑定
    }

    [XLua.LuaCallCSharp]
    public class ItemData : ItemBaseData
    {
        /// <summary>
        /// 所有ntt都应具有位置属性 ：  public virtual Vector3 Position()
        /// </summary>
        public VitalSignAttrData Attrs;

        /*
        public EuipBasePropList Baseprop;//基础属性

        public EuipExPropList Exprop;//额外属性

        public EuipSpecialPropList Specialprop;//特殊属性

        public EuipBasePropList Updatebaseprop;//基础属性
        */

        public ulong EntityID;//唯一ID
        public long Num;//数量
        public int SpaceId;//所属背包ID 
        public uint Level;//加成等级
        public uint Quality;//品阶
        public long CDTime;//使用CD
        public long GetTime;//获得道具时间
        public ulong BelongEquipID;//归属于的装备ID(宝石用的)
        public ulong Heroid;//当前使用该武器的英雄
        public bool IsNew;//是否是新增
        public long CD;//道具CD（下次可以使用的时间戳毫秒）


        public ItemData(ulong Eid)
        {
            CreateItemData(Eid);
        }

        //public ItemData(ItemPropSyncList itemPropSyncList, ulong EItemID)
        //{
        //    CreateItemData(EItemID);


        //    UpdatePropList(itemPropSyncList.Prop);
        //}

        private void CreateItemData(ulong Eid)
        {
            Attrs = new VitalSignAttrData();
            EntityID = Eid;
        }

        public void RefeshItem()
        {
            if (Attrs.CheckCacheIntMap(AOIAttrDefine.SpaceId))
            {
                SpaceId = Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SpaceId);
            }

            if (Attrs.CheckCacheLongMap(AOIAttrDefine.BaseID))
            {
                BaseID = Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.BaseID);
            }

            if (Attrs.CheckCacheLongMap(AOIAttrDefine.Num))
            {
                if (Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Num) > Num)
                {
                    IsNew = true;
                }
                Num = Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Num);
            }

            if (Attrs.CheckCacheUIntMap(AOIAttrDefine.Level))
            {
                Level = Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Level);
            }

            if (Attrs.CheckCacheUIntMap(AOIAttrDefine.Quality))
            {
                Quality = Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Quality);
            }

            if (Attrs.CheckCacheIntMap(AOIAttrDefine.UpdatePoint))
            {
                UpdatePoint = Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.UpdatePoint);
            }

            if (Attrs.CheckCacheLongMap(AOIAttrDefine.CDTime))
            {
                CDTime = Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.CDTime);
            }

            if (Attrs.CheckCacheLongMap(AOIAttrDefine.GetTime))
            {
                GetTime = Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.GetTime);
            }

            if (Attrs.CheckCacheIntMap(AOIAttrDefine.Durability))
            {
                Durability = Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.Durability);
            }

            if (Attrs.CheckCacheULongMap(AOIAttrDefine.Heroid))
            {
                Heroid = Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.Heroid);
            }

            if (Attrs.CheckCacheBoolMap(AOIAttrDefine.IsBind))
            {
                IsBind = Attrs.GetAoiValue<bool>(EnumAOIType.Bool, AOIAttrDefine.IsBind);
            }

            object v = Attrs.GetProtoValue(AOIAttrDefine.EquipProps);
            if (v != null)
            {
                ByteString msg = (ByteString)v;
                byte[] msgData = msg.ToByteArray();
                IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.EquipPropListID, msgData);
                EquipPropList euipProps = (EquipPropList)pbMessage;
                if (euipProps != null)
                {
                    EquipProps = euipProps;
                }
            }

            object v2 = Attrs.GetProtoValue(AOIAttrDefine.CombatSkills);
            if (v2 != null)
            {
                ByteString msg = (ByteString)v2;
                byte[] msgData = msg.ToByteArray();
                IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.CombatSkillListID, msgData);
                CombatSkillList combatSkills = (CombatSkillList)pbMessage;
                if (combatSkills != null)
                {
                    CombatSkills = combatSkills;
                }
            }

            object v3 = Attrs.GetProtoValue(AOIAttrDefine.GemSlots);
            if (v3 != null)
            {
                ByteString msg = (ByteString)v3;
                byte[] msgData = msg.ToByteArray();
                IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.GemSlotListID, msgData);
                GemSlotList gemSlots = (GemSlotList)pbMessage;
                if (gemSlots != null)
                {
                    GemSlots = gemSlots;
                }
            }

            if (Attrs.CheckCacheIntMap(AOIAttrDefine.ExtractNum))
            {
                ExtractNum = Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.ExtractNum);
            }

            if (Attrs.CheckCacheULongMap(AOIAttrDefine.BelongEquipID))
            {
                BelongEquipID = Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.BelongEquipID);
            }


            /*
            EuipBasePropList baseprop = Attrs.GetProtoValue(AOIAttrDefine.Baseprop) as EuipBasePropList;
            if (baseprop != null)
            {
                Baseprop = baseprop;
            }

            EuipExPropList exprop = Attrs.GetProtoValue(AOIAttrDefine.Exprop) as EuipExPropList;
            if (exprop != null)
            {
                Exprop = exprop;
            }

            EuipSpecialPropList specialprop = Attrs.GetProtoValue(AOIAttrDefine.Specialprop) as EuipSpecialPropList;
            if (specialprop != null)
            {
                Specialprop = specialprop;
            }

            EuipBasePropList updatebaseprop = Attrs.GetProtoValue(AOIAttrDefine.Updatebaseprop) as EuipBasePropList;
            if (updatebaseprop != null)
            {
                Updatebaseprop = updatebaseprop;
            }
            */

        }

        public void DelItem()
        {
            Attrs = null;
            /*
            Baseprop = null;
            Exprop = null;
            Specialprop = null;
            Updatebaseprop = null;
            */
        }

        public void UpdatePropList(PropBaseSyncList pbsAttrList)
        {
            //清光所有缓存
            Attrs.ClearAllCache();

            //场景 && 人
            for (int i = 0; i < pbsAttrList.Prop.Count; i++)
            {
                SyncBaseInfo syncBaseInfo = pbsAttrList.Prop[i];
                HandleProperty(syncBaseInfo);

            }
            RefeshItem();

        }


        private void HandleProperty(SyncBaseInfo syncBaseInfo)
        {
            VitalSignAOIClientAttrs attrType = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);

            if (attrType == null)
            {
                //SGF.Debuger.LogError($" {TagFlag} HandleProperty id=>{M_EntityID}  属性===》{syncBaseInfo.Index} 值===》 {syncBaseInfo}  error!!!");
                return;
            }

            //Debug.LogError("ItemSpaceSyncListMsg.attrType.Name " + attrType.Name);

            //object val = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString()).GetValue(syncBaseInfo, null);
            ////最终都是同步属性
            //Attrs.CacheValueByKey(attrType, ref val);

            string key = attrType.Name;
            switch (syncBaseInfo.PropValueCase)
            {
                case SyncBaseInfo.PropValueOneofCase.None:
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int32Value:
                    {
                        Attrs.CacheIntMap(key, syncBaseInfo.Int32Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                    {
                        Attrs.CacheUIntMap(key, syncBaseInfo.Uint32Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int64Value:
                    {
                        Attrs.CacheLongMap(key, syncBaseInfo.Int64Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                    {
                        Attrs.CacheULongMap(key, syncBaseInfo.Uint64Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.FloatValue:
                    {
                        Attrs.CacheFloatMap(key, syncBaseInfo.FloatValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                    {
                        Attrs.CacheFloatMap(key, (float)syncBaseInfo.FloatValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.StringValue:
                    {
                        Attrs.CacheStrMap(key, syncBaseInfo.StringValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.BoolValue:
                    {
                        Attrs.CacheBoolMap(key, syncBaseInfo.BoolValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.MsgValue:
                    {
                        Attrs.CacheProtoMap(key, syncBaseInfo.MsgValue);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
