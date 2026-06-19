using Google.Protobuf;
using ProtoMsg;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;

namespace StarProject.Game.Data
{
    //属性
    //1空間是持久配置空間
    //2屬性pb是pb管理，且按照屬性釋放
    //3内存臨時持久數據（
    //[ProtoContract]
    [XLua.LuaCallCSharp]
    public class VitalSignAttrData
    {
        public string LOG_TAG = "VitalSignAttrData";

        private DictionaryEx<string, int> intAttrsMaps = new();
        private DictionaryEx<string, long> longAttrsMaps = new();
        private DictionaryEx<string, uint> uIntAttrsMaps = new();
        private DictionaryEx<string, ulong> uLongAttrsMaps = new();
        private DictionaryEx<string, float> floatAttrsMaps = new();
        private DictionaryEx<string, bool> boolAttrsMaps = new();
        private DictionaryEx<string, string> strAttrsMaps = new();
        private DictionaryEx<string, object> protoAttrsMaps = new();


        private DictionaryEx<string, Action<object>> protoAttrsFunMaps = new();

        public VitalSignAttrData()
        {

            protoAttrsFunMaps.Add(AOIAttrDefine.StorageDrugs, CacheStorageDrugs);

            protoAttrsFunMaps.Add(AOIAttrDefine.ExAmuletInfo, CacheExAmuletInfo);

            protoAttrsFunMaps.Add(AOIAttrDefine.PlayerGuildInfo, CacheLGInfo);

            protoAttrsFunMaps.Add(AOIAttrDefine.RiskInfo, CacheRiskInfo);

            protoAttrsFunMaps.Add(AOIAttrDefine.WorldLineRewardData, CacheWorldLineRewardData);

            protoAttrsFunMaps.Add(AOIAttrDefine.GVEBonus, CacheGVEBonus);

            protoAttrsFunMaps.Add(AOIAttrDefine.TreasureMonid, CacheTreasureMonid);

            protoAttrsFunMaps.Add(AOIAttrDefine.CurTreasure, CacheCurTreasure);

            protoAttrsFunMaps.Add(AOIAttrDefine.WantTaskInfo, CacheWantTaskInfo);

            protoAttrsFunMaps.Add(AOIAttrDefine.GNGData, CacheGNGData);



        }

        /// <summary>
        /// 药品相关
        /// </summary>
        /// <param name="value"></param>
        private void CacheStorageDrugs(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.StorageDrugsMdID, msgData);
            value = (StorageDrugsMd)pbMessage;

            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.StorageDrugs))
            {
                protoAttrsMaps[AOIAttrDefine.StorageDrugs] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.StorageDrugs, value);
            }
        }

        /// <summary>
        /// 装备洗练待保存的词条
        /// </summary>
        /// <param name="value"></param>
        private void CacheExAmuletInfo(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.ExtractaAmuletMDID, msgData);
            value = (ExtractaAmuletMD)pbMessage;

            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.ExAmuletInfo))
            {
                protoAttrsMaps[AOIAttrDefine.ExAmuletInfo] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.ExAmuletInfo, value);
            }
        }

        private void CacheCurTreasure(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.TreasureDataID, msgData);
            value = (TreasureData)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.CurTreasure))
            {
                protoAttrsMaps[AOIAttrDefine.CurTreasure] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.CurTreasure, value);
            }
        }

        private void CacheWantTaskInfo(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.WantTaskMDID, msgData);
            value = (WantTaskMD)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.WantTaskInfo))
            {
                protoAttrsMaps[AOIAttrDefine.WantTaskInfo] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.WantTaskInfo, value);
            }
        }

        /// <summary>
        /// 世界线数据
        /// </summary>
        /// <param name="value"></param>
        private void CacheWorldLineRewardData(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.BaseDataID, msgData);
            value = (BaseData)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.WorldLineRewardData))
            {
                protoAttrsMaps[AOIAttrDefine.WorldLineRewardData] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.WorldLineRewardData, value);
            }
        }

        /// <summary>
        /// 冒险等级相关
        /// </summary>
        /// <param name="value"></param>
        private void CacheRiskInfo(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.RiskLevelMDID, msgData);
            value = (RiskLevelMD)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.RiskInfo))
            {
                protoAttrsMaps[AOIAttrDefine.RiskInfo] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.RiskInfo, value);
            }
        }

        /// <summary>
        /// GVE积分信息
        /// </summary>
        /// <param name="value"></param>
        private void CacheGVEBonus(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.GVEBonusID, msgData);
            value = (GVEBonus)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.GVEBonus))
            {
                protoAttrsMaps[AOIAttrDefine.GVEBonus] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.GVEBonus, value);
            }
        }
        /// <summary>
        /// 午间GVE
        /// </summary>
        /// <param name="value"></param>
        private void CacheGNGData(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.GNGUserDataID, msgData);
            value = (GNGUserData)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.GNGData))
            {
                protoAttrsMaps[AOIAttrDefine.GNGData] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.GNGData, value);
            }
        }

        /// <summary>
        /// GVE 藏宝图
        /// </summary>
        public void CacheTreasureMonid(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.TreMonIDID, msgData);
            value = (TreMonID)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.TreasureMonid))
            {
                protoAttrsMaps[AOIAttrDefine.TreasureMonid] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.TreasureMonid, value);
            }
        }

        private void CacheLGInfo(object value)
        {
            ByteString msg = (ByteString)value;
            byte[] msgData = msg.ToByteArray();
            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int)MsgIDEnum.PlayerGuildInfoID, msgData);
            value = (PlayerGuildInfo)pbMessage;
            if (protoAttrsMaps.ContainsKey(AOIAttrDefine.PlayerGuildInfo))
            {
                protoAttrsMaps[AOIAttrDefine.PlayerGuildInfo] = value;
            }
            else
            {
                protoAttrsMaps.Add(AOIAttrDefine.PlayerGuildInfo, value);
            }
        }


        #region 【缓存属性信息】
        public void CacheValueByKey(VitalSignAOIClientAttrs attrsType, ref object value)
        {
            //UnityEngine.Debug.Log("CacheValueByKey:" + attrsType.Name + value.ToString());
            switch (attrsType.type)
            {
                case "int8":
                case "int16":
                case "int32":
                    {
                        CacheIntMap(attrsType.Name, Convert.ToInt32(value));
                    }
                    break;
                case "int64":
                    {
                        CacheLongMap(attrsType.Name, Convert.ToInt64(value));
                    }
                    break;
                case "uint8":
                case "uint16":
                case "uint32":
                    {
                        CacheUIntMap(attrsType.Name, Convert.ToUInt32(value));
                    }
                    break;
                case "uint64":
                    {
                        CacheULongMap(attrsType.Name, Convert.ToUInt64(value));
                    }
                    break;
                case "float32":
                case "float64":
                    {
                        CacheFloatMap(attrsType.Name, Convert.ToSingle(value));
                    }
                    break;
                case "bool":
                    {
                        CacheBoolMap(attrsType.Name, Convert.ToBoolean(value));
                    }
                    break;
                case "string":
                    {
                        CacheStrMap(attrsType.Name, Convert.ToString(value));
                    }
                    break;
                case "protoMsg":
                    {
                        CacheProtoMap(attrsType.Name, ref value);
                    }
                    break;
                default:
                    {
                        CacheProtoMap(attrsType.Name, ref value);
                        //SGF.Debuger.LogError($"{LOG_TAG} CashValueByKey attrName={attrsType.Name},value={value},attrsType={attrsType.type},attrsType is unknown");
                    }
                    break;
            }


        }

        public void ClearAllCache()
        {
            intAttrsMaps.Clear();
            longAttrsMaps.Clear();
            uIntAttrsMaps.Clear();
            uLongAttrsMaps.Clear();
            floatAttrsMaps.Clear();
            boolAttrsMaps.Clear();
            strAttrsMaps.Clear();
            protoAttrsMaps.Clear();
        }


        //没有缓存机制，只是对照填进
        public void CacheIntMap(string key, int value)
        {
            if (intAttrsMaps.ContainsKey(key))
            {
                intAttrsMaps[key] = value;
            }
            else
            {
                intAttrsMaps.Add(key, value);
            }
        }
        public void CacheLongMap(string key, long value)
        {
            CacheIntoMap<long>(key, value, longAttrsMaps);
        }
        public void CacheUIntMap(string key, uint value)
        {
            CacheIntoMap<uint>(key, value, uIntAttrsMaps);
        }
        public void CacheULongMap(string key, ulong value)
        {
            CacheIntoMap<ulong>(key, value, uLongAttrsMaps);
        }
        public void CacheFloatMap(string key, float value)
        {
            CacheIntoMap<float>(key, value, floatAttrsMaps);
        }
        public void CacheBoolMap(string key, bool value)
        {
            CacheIntoMap<bool>(key, value, boolAttrsMaps);
        }
        public void CacheStrMap(string key, string value)
        {
            CacheIntoMap<string>(key, value, strAttrsMaps);
        }
        public void CacheProtoMap(string key, ref object value)
        {
            //接收就解开的PB
            if (protoAttrsFunMaps.ContainsKey(key))
            {
                protoAttrsFunMaps[key].Invoke(value);
                //if (key == AOIAttrDefine.StorageDrugs)
                //{
                //    ByteString msg = (ByteString)value;
                //    byte[] msgData = msg.ToByteArray();
                //    IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize((int) MsgIDEnum.StorageDrugsMdID, msgData);
                //    value = (StorageDrugsMd)pbMessage;
                //}
                return;
            }


            if (protoAttrsMaps.ContainsKey(key))
            {
                protoAttrsMaps[key] = value;
            }
            else
            {
                protoAttrsMaps.Add(key, value);
            }
        }

        public void CacheProtoMap(string key, ByteString value)
        {
            //接收就解开的PB
            if (protoAttrsFunMaps.ContainsKey(key))
            {
                protoAttrsFunMaps[key].Invoke(value);
                return;
            }

            if (protoAttrsMaps.ContainsKey(key))
            {
                protoAttrsMaps[key] = value;
            }
            else
            {
                protoAttrsMaps.Add(key, value);
            }
        }

        private void CacheIntoMap<T>(string key, T value, DictionaryEx<string, T> dic)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }
        #endregion

        #region 【判断属性是否存在数据层】

        public bool CheckCacheAttrName(VitalSignAOIClientAttrs attrsType)
        {
            bool res = false;
            switch (attrsType.type)
            {
                case "int8":
                case "int16":
                case "int32":
                    {
                        res = CheckCacheIntMap(attrsType.Name);
                    }
                    break;
                case "int64":
                    {
                        res = CheckCacheLongMap(attrsType.Name);
                    }
                    break;
                case "uint8":
                case "uint16":
                case "uint32":
                    {
                        res = CheckCacheUIntMap(attrsType.Name);
                    }
                    break;
                case "uint64":
                    {
                        res = CheckCacheULongMap(attrsType.Name);
                    }
                    break;
                case "float32":
                case "float64":
                    {
                        res = CheckCacheFloatMap(attrsType.Name);
                    }
                    break;
                case "bool":
                    {
                        res = CheckCacheBoolMap(attrsType.Name);
                    }
                    break;
                case "string":
                    {
                        res = CheckCacheStrMap(attrsType.Name);
                    }
                    break;
                case "protoMsg":
                    {
                        res = CheckCacheProtoMap(attrsType.Name);
                    }
                    break;
                default:
                    {
                        res = CheckCacheProtoMap(attrsType.Name);
                    }
                    break;
            }
            return res;
        }

        private bool CheckHasCacheInMap<T>(string key, DictionaryEx<string, T> dic)
        {
            if (dic.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        //没有缓存机制，只是对照填进
        public bool CheckCacheIntMap(string key)
        {
            return CheckHasCacheInMap<int>(key, intAttrsMaps);
        }
        public bool CheckCacheLongMap(string key)
        {
            return CheckHasCacheInMap<long>(key, longAttrsMaps);
        }
        public bool CheckCacheUIntMap(string key)
        {
            return CheckHasCacheInMap<uint>(key, uIntAttrsMaps);
        }
        public bool CheckCacheULongMap(string key)
        {
            return CheckHasCacheInMap<ulong>(key, uLongAttrsMaps);
        }
        public bool CheckCacheFloatMap(string key)
        {
            return CheckHasCacheInMap<float>(key, floatAttrsMaps);
        }
        public bool CheckCacheBoolMap(string key)
        {
            return CheckHasCacheInMap<bool>(key, boolAttrsMaps);
        }
        public bool CheckCacheStrMap(string key)
        {
            return CheckHasCacheInMap<string>(key, strAttrsMaps);
        }
        public bool CheckCacheProtoMap(string key)
        {
            return CheckHasCacheInMap<object>(key, protoAttrsMaps);
        }

        #endregion

        #region 【对比数据是否有变化】

        //public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, object newVal)
        //{
        //    //解-比-存-通知（比）
        //    bool isChanage = true;
        //    string key = attrsType.Name;
        //    string type = attrsType.type;
        //    switch (type)
        //    {
        //        case "int8":
        //        case "int16":
        //        case "int32":
        //            {
        //                int val = Convert.ToInt32(newVal);
        //                isChanage = !GetIntValue(key).Equals(val);
        //                //if (isChanage)
        //                //{
        //                //    CacheIntMap(key, val);
        //                //}
        //            }
        //            break;
        //        case "int64":
        //            {
        //                long val = Convert.ToInt64(newVal);
        //                isChanage = !GetLongValue(key).Equals(val);
        //            }
        //            break;
        //        case "uint8":
        //        case "uint16":
        //        case "uint32":
        //            {
        //                uint val = Convert.ToUInt32(newVal);
        //                isChanage = !GetUIntValue(key).Equals(val);
        //            }
        //            break;
        //        case "uint64":
        //            {
        //                ulong val = Convert.ToUInt64(newVal);
        //                isChanage = !GetULongValue(key).Equals(val);
        //            }
        //            break;
        //        case "float32":
        //        case "float64":
        //            {
        //                float val = Convert.ToSingle(newVal);
        //                isChanage = !GetFloatValue(key).Equals(val);
        //            }
        //            break;
        //        case "bool":
        //            {
        //                bool val = Convert.ToBoolean(newVal);
        //                isChanage = !GetBoolValue(key).Equals(val);
        //            }
        //            break;
        //        case "string":
        //            {
        //                string val = Convert.ToString(newVal);
        //                isChanage = !GetStrValue(key).Equals(val);
        //            }
        //            break;
        //        case "protoMsg":
        //            {
        //                // 如果是proto字节流的数据类型
        //                // 拿出来转成-》字符串后再转-》md5 进行对比 不行[转md5会全变""空字符串]
        //                // 直接对比字节流
        //                ByteString oldVal = (ByteString)GetProtoValue(attrsType.Name);
        //                string oldStrVal = oldVal.ToBase64();
        //                //string oldMd5Val = CompareMD5.GetMD5HashFromFile(oldStrVal);

        //                ByteString newValue = (ByteString)newVal;
        //                string newStrVal = newValue.ToBase64();
        //                //string newMd5Val = CompareMD5.GetMD5HashFromFile(newStrVal);
        //                isChanage = !oldStrVal.Equals(newStrVal);

        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() ------------------------111111111111111111111111");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() proto对比  key={key},type={type},isChanage={isChanage}");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() proto对比  oldStrVal={oldStrVal},newStrVal={newStrVal}");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() ------------------------2222222222222222222222222");
        //            }
        //            break;
        //        default:
        //            {
        //                // 如果是proto字节流的数据类型
        //                // 拿出来转成-》字符串后再转-》md5 进行对比 不行[转md5会全变""空字符串]
        //                // 直接对比字节流
        //                //UnityEngine.Debug.LogError(attrsType.Name);
        //                ByteString oldVal = (ByteString)GetProtoValue(attrsType.Name);
        //                string oldStrVal = oldVal.ToBase64();
        //                //string oldMd5Val = CompareMD5.GetMD5HashFromFile(oldStrVal);

        //                ByteString newValue = (ByteString)newVal;
        //                string newStrVal = newValue.ToBase64();
        //                //string newMd5Val = CompareMD5.GetMD5HashFromFile(newStrVal);
        //                isChanage = !oldStrVal.Equals(newStrVal);

        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() 111 ------------------------111111111111111111111111");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() proto对比  key={key},type={type},isChanage={isChanage}");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() proto对比  oldStrVal={oldStrVal},newStrVal={newStrVal}");
        //                //SGF.Debuger.LogError($"属性同步创建 GetValIsChanageByAttr() 1111 ------------------------2222222222222222222222222");
        //            }
        //            break;
        //    }

        //    return isChanage;
        //}

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, int newVal)
        {
            return !GetIntValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, long newVal)
        {
            return !GetLongValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, uint newVal)
        {
            return !GetUIntValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, ulong newVal)
        {
            return !GetULongValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, float newVal)
        {
            return !GetFloatValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, bool newVal)
        {
            return !GetBoolValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, string newVal)
        {
            return !GetStrValue(attrsType.Name).Equals(newVal);
        }

        public bool GetValIsChanageByAttr(VitalSignAOIClientAttrs attrsType, ByteString newVal)
        {
            ByteString oldVal = (ByteString)GetProtoValue(attrsType.Name);
            string oldStrVal = oldVal.ToBase64();
            //string oldMd5Val = CompareMD5.GetMD5HashFromFile(oldStrVal);

            string newStrVal = newVal.ToBase64();
            //string newMd5Val = CompareMD5.GetMD5HashFromFile(newStrVal);
            return !oldStrVal.Equals(newStrVal);
        }


        #endregion

        #region 【获取属性信息】
        /// <summary>
        /// 获取基础数据类型
        /// </summary>
        public T GetAoiValue<T>(EnumAOIType enumAOIType, string key) where T : IComparable, IConvertible
        {
            switch (enumAOIType)
            {
                case EnumAOIType.Int:
                    {
                        int value = 0;
                        if (intAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Long:
                    {
                        long value = 0;
                        if (longAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Uint:
                    {
                        uint value = 0;
                        if (uIntAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Ulong:
                    {
                        ulong value = 0;
                        if (uLongAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Float:
                    {
                        float value = 0;
                        if (floatAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Bool:
                    {
                        bool value = false;
                        if (boolAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.String:
                    {
                        string value = "";
                        if (strAttrsMaps.TryGetValue(key, out value))
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                    break;
                case EnumAOIType.Proto://vector2=object
                    {
                        // 需要调用其他接口
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
            return default(T);
        }


        [XLua.LuaCallCSharp]
        public System.Object GetLuaAoiValue(int index)
        {
            VitalSignAOIClientAttrs attrType = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)index);
            if (attrType == null)
            {
                return null;
            }
            return GetLuaAoiValue(attrType);
        }
        [XLua.LuaCallCSharp]
        public System.Object GetLuaAoiValue(VitalSignAOIClientAttrs vitalSignAOIClientAttrs)
        {
            string key = vitalSignAOIClientAttrs.Name;
            string type = vitalSignAOIClientAttrs.type;
            switch (type)
            {
                case "int8":
                case "int16":
                case "int32":
                    {
                        return GetIntValue(key);
                    }
                case "int64":
                    {
                        return GetLongValue(key);
                    }
                case "uint8":
                case "uint16":
                case "uint32":
                    {
                        return GetUIntValue(key);
                    }
                case "uint64":
                    {
                        return GetULongValue(key);
                    }
                case "float32":
                case "float64":
                    {
                        return GetFloatValue(key);
                    }
                case "bool":
                    {
                        return GetBoolValue(key);

                    }
                case "string":
                    {
                        return GetStrValue(key);
                    }
                case "protoMsg":
                    {
                        return GetProtoValue(key);
                    }
                default:
                    {
                        return GetProtoValue(key);
                    }
            }
        }

        [XLua.LuaCallCSharp]
        public System.Object GetLuaAoiValue(EnumAOIType enumAOIType, string key)
        {
            switch (enumAOIType)
            {
                case EnumAOIType.Int:
                    {
                        return GetIntValue(key);
                    }
                case EnumAOIType.Long:
                    {
                        return GetLongValue(key);
                    }
                case EnumAOIType.Uint:
                    {
                        return GetUIntValue(key);
                    }
                case EnumAOIType.Ulong:
                    {
                        return GetULongValue(key);
                    }
                case EnumAOIType.Float:
                    {
                        return GetFloatValue(key);
                    }
                case EnumAOIType.Bool:
                    {
                        return GetBoolValue(key);
                    }
                case EnumAOIType.String:
                    {
                        return GetStrValue(key);
                    }
                case EnumAOIType.Proto://vector2=object
                    {
                        return GetProtoValue(key);
                    }
                default:
                    {
                        return GetProtoValue(key);
                    }
            }
        }
        public int GetIntValue(string key)
        {
            int value = 0;
            if (intAttrsMaps.ContainsKey(key))
            {
                value = intAttrsMaps[key];
            }
            return value;
        }
        public long GetLongValue(string key)
        {
            long value = 0;
            if (longAttrsMaps.ContainsKey(key))
            {
                value = longAttrsMaps[key];
            }
            return value;
        }
        public uint GetUIntValue(string key)
        {
            uint value = 0;
            if (uIntAttrsMaps.ContainsKey(key))
            {
                value = uIntAttrsMaps[key];
            }
            return value;
        }
        public ulong GetULongValue(string key)
        {
            ulong value = 0;
            if (uLongAttrsMaps.ContainsKey(key))
            {
                value = uLongAttrsMaps[key];
            }
            return value;
        }
        public float GetFloatValue(string key)
        {
            float value = 0;
            if (floatAttrsMaps.ContainsKey(key))
            {
                value = floatAttrsMaps[key];
            }
            return value;
        }
        public bool GetBoolValue(string key)
        {
            bool value = false;
            if (boolAttrsMaps.ContainsKey(key))
            {
                value = boolAttrsMaps[key];
            }
            return value;
        }
        public string GetStrValue(string key)
        {
            string value = "";
            if (strAttrsMaps.ContainsKey(key))
            {
                value = strAttrsMaps[key];
            }
            return value;
        }
        /// <summary>
        /// 获取proto结构数据
        /// </summary>
        [XLua.LuaCallCSharp]
        public object GetProtoValue(string key)
        {
            object value = null;
            if (protoAttrsMaps.ContainsKey(key))
            {
                value = protoAttrsMaps[key];
            }
            return value;
        }

        #endregion
    }
}
