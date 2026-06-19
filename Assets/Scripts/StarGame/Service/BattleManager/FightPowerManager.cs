using System;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Service.Battle
{

    [XLua.LuaCallCSharp]
    public class FightPowerManager : ServiceModule<FightPowerManager>
    {
        public void Init()
        {
            LocalDataManager.Instance.InitPowerModules();
        }

        public override void Release()
        {
            base.Release();
        }

        #region 玩家战力 相关的接口
        public FightMD GetFightMD
        {
            get
            {
                var mgr = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.FightMD) as FightMDMgr;
                if (mgr != null)
                {
                    return mgr.GetMD();
                }
                return null;
            }
        }



        /// <summary>
        /// 定义一个战力相关的类.
        /// 战力相关的 接口 都可以封装在这个类里面
        /// </summary>

        #region  角色
        /// <summary>
        /// 冒险等级战力
        /// </summary>
        public long GetAdvGradeExpPower()
        {

            //    SGF.Debuger.Log($"[Power] 开始计算 冒险等级 战力:");
            var riskInfo = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(StarProjectDef.AOIAttrDefine.RiskInfo) as RiskLevelMD;
            if (riskInfo == null)
            {
                return 0;
            }

            var curAdvanceLevel = riskInfo.CurRiskLevel;

            AdvGradeExpDataCell cfg = LocalDataManager.Instance.GetAdvGradeExpDataCell(curAdvanceLevel);
            if (cfg == null)
            {
                return 0;
            }
            //    SGF.Debuger.Log($"[Power] 冒险等级 战力: {cfg.Power}");

            return cfg.Power;
        }

        /// <summary>
        /// 玩家等级战力
        /// </summary>
        public long GetLevelExpPower()
        {
            var level = GameManager.Instance.GetPlayerLevel();
            //    SGF.Debuger.Log($"[Power] 开始计算 玩家等级 战力:");

            if (level == 0)
            {
                return 0;
            }

            var cfg = LocalDataManager.Instance.GetLevelExpDataCell(level);
            if (cfg == null)
            {
                return 0;
            }
            //    SGF.Debuger.Log($"[Power] 玩家等级 战力: {cfg.Power}");

            return cfg.Power;
        }

        #endregion

        /// <summary>
        /// 得到天赋树 相关的 战力
        /// </summary>
        public long GetTalentTreeAttack(out long power1, out long power2, out long power3)
        {
            var tree = GameManager.Instance.HeroMD.TalentTree.AllTree;
            long value = 0;
            power1 = 0;
            power2 = 0;
            power3 = 0;
            //    SGF.Debuger.Log($"[Power] 开始计算 天赋树 战力:");

            foreach (var item in tree)
            {
                var heroTree = item.Value.Tree;
                foreach (var treeItem in heroTree)
                {
                    int attriTTID = treeItem.Key;
                    int attriTTLevel = treeItem.Value;
                    AttriTTNodeDataCell attriCfg = LocalDataManager.Instance.GetAttriTTNodeDataCell(attriTTID);
                    if (attriCfg != null)
                    {
                        long addValue = 0;
                        if (attriTTLevel == 1)
                        {
                            addValue = attriCfg.Power1;
                            power1 += attriCfg.Power1;

                        }
                        else if (attriTTLevel == 2)
                        {
                            addValue = attriCfg.Power2;
                            power2 += attriCfg.Power2;

                        }
                        else
                        {
                            addValue = attriCfg.Power3;
                            power3 += attriCfg.Power3;

                        }
                        value += addValue;
                        //    SGF.Debuger.Log($"[Power] 天赋树 id: {attriTTID}, 天赋战力: {addValue}");

                    }
                }
            }
            //    SGF.Debuger.Log($"[Power] 天赋树 总战力: {value}, power1: {power1}, power2: {power2}, power3: {power3} ");

            return value;
        }

        #region  装备养成线战力
        /// <summary>
        /// 得到纹章战力,包含共鸣战力
        /// </summary>
        /// 

        /// <summary>
        /// 得到 纹章战力,包含共鸣战力
        /// </summary>
        /// <param name="slotPower">所有纹章 战力</param>
        /// <param name="resonanceSlot">所有 共鸣战力</param>
        /// <returns>纹章+共鸣 总战力</returns>
        public long GetHeraldryEquipTotalPower(out long slotPower, out long resonanceSlot)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 纹章 战力:");

            long powerAll = 0;

            // 装备的纹章战力
            slotPower = 0;
            // 装备的共鸣战力
            resonanceSlot = 0;

            // 护符上面有多个槽;
            // 槽位上面放了纹章
            // 当纹章的共鸣槽位 与护肤槽位一样时，产生共鸣, 采用共鸣战力
            // 当 没有共鸣时, 采用 纹章基础战力
            for (int i = 1; i <= 2; i++)
            {
                ItemMD equip = BusinessManager.Instance.GetHeroEquipedAmulet(i);
                if (equip == null)
                {
                    continue;
                }

                //显示纹章信息
                var gemSlots = equip.GemSlots;
                if (gemSlots == null)
                {
                    continue;
                }
                //查看纹章
                for (int j = 0; j < gemSlots.List.Count; j++)
                {
                    //刷新纹章槽位信息
                    var gemSlot = gemSlots.List[j];

                    int slotID = gemSlot.SlotID;
                    ulong gemID = gemSlot.GemID;

                    if (gemID != 0)
                    {
                        var itemData = BusinessManager.Instance.GetItemByItemEntityId(gemID);


                        //宝石的共鸣
                        var heraldryEquipCfg = LocalDataManager.Instance.GetHeraldryEquipDataCell(itemData.BaseID);

                        slotPower += heraldryEquipCfg.Power1;
                        //    SGF.Debuger.Log($"[Power] 纹章 槽位: {i}, slotID: {slotID}, 装备id: {itemData.BaseID}, 战力: {heraldryEquipCfg.Power1}");

                        //判断是是否触发了共鸣
                        if (slotID == heraldryEquipCfg.ResonanceSlot)
                        {
                            //    SGF.Debuger.Log($"[Power] 纹章 产生的 共鸣战力 : {heraldryEquipCfg.Power2}");

                            resonanceSlot += heraldryEquipCfg.Power2;
                        }
                    }

                }
            }

            // 总站力 = 纹章战力 + 共鸣战力
            powerAll = (slotPower + resonanceSlot);
            //    SGF.Debuger.Log($"[Power] 纹章 产生的 总战力 : {powerAll}");

            return powerAll;
        }

        /// <summary>
        /// 装备主属性战力
        /// </summary>
        /// <returns></returns>
        public long GetEquipPropsPower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 装备主属性 战力:");

            long power = 0;
            //1. 玩家有8个装备位
            for (int i = 1; i <= 8; i++)
            {
                ItemMD equip = BusinessManager.Instance.GetHeroEquipedEquip(i);
                if (equip == null)
                {
                    continue;
                }

                // 装备
                long equipPower = GetEquipMDPropsPower(equip);

                SGF.Debuger.Log($"[Power] 装备 槽位: {i}, 装备id: {equip.BaseID}, 主属性 战力: {equipPower}");

                power += equipPower;
            }
            SGF.Debuger.Log($"[Power] 装备 主属性 总战力: {power}");

            return power;
        }

        /// <summary>
        /// 装备词条战力
        /// </summary>
        public long GetEquipSubPower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 装备词条 战力:");

            long power = 0;
            //1. 玩家有8个装备位
            for (int i = 1; i <= 8; i++)
            {
                ItemMD equip = BusinessManager.Instance.GetHeroEquipedEquip(i);
                if (equip == null)
                {
                    continue;
                }

                // 词条战力
                long subPower = GetEquipSubAttack(equip);

                SGF.Debuger.Log($"[Power] 装备 槽位: {i}, 装备id: {equip.BaseID}, 词条 战力: {subPower}");

                power += subPower;
            }
            SGF.Debuger.Log($"[Power] 装备 词条 总战力: {power}");

            return power;
        }

        /// <summary>
        /// 装备主属性战力
        /// </summary>
        public long GetEquipMDPropsPower(ItemMD equip)
        {
            if (equip == null)
            {
                return 0;
            }
            var equipID = equip.BaseID;
            //    SGF.Debuger.Log($"[Power] 开始计算 单个装备 战力:");

            var itemDataCell = LocalDataManager.Instance.GetItemDataCell(equipID);

            long power = 0;

            // 2是装备, 4是 伙伴装备 , 5 是纹章.
            if (itemDataCell.ItemType == 2)
            {
                var eCfg = LocalDataManager.Instance.GetEquipDataCell((int)equipID);



                if (equip.EquipProps == null)
                {
                    return 0;
                }
                var mainProps = equip.EquipProps.MainPropList;

                // 服务器下发的装备的洗练完美度
                var perfection = equip.EquipProps.Perfection;

                // 装备战力 = 洗练完美都 * 配置战力
                power += (int)MathF.Floor(perfection * eCfg.Power / 100);
                //    SGF.Debuger.Log($"[Power] 单个装备 {equipID} , 洗练完美度: {perfection}, 战力: {power}");

            }
            else if (itemDataCell.ItemType == 4)
            {

                var eCfg = LocalDataManager.Instance.GetPartnerEquipDataCell(itemDataCell.Id);

                if (eCfg != null)
                {
                    return eCfg.Power;
                }

            }



            // TODO: 胡哥哥
            // 如何区分 玩家装备还是 伙伴装备，这个 不知道


            return power;
        }

        /// <summary>
        /// 获得词条战力
        /// </summary>
        /// <param name="equip"></param>
        public long GetEquipSubAttack(ItemMD equip)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 单个装备 {equip.BaseID} 词条 战力:");

            long power = 0;
            if (equip.EquipProps == null)
            {
                return power;
            }

            // 装备子属性词条
            var subPropList = equip.EquipProps.SubPropList;

            var equipCfg = LocalDataManager.Instance.GetEquipDataCell((int)equip.BaseID);

            for (int i = 0; i < subPropList.Count; i++)
            {
                var subProp = subPropList[i];
                var quality = subProp.Quality;
                var equipSubID = subProp.CfgID;

                EquipSubDataCell subEquipCfg = LocalDataManager.Instance.GetEquipSubDataCell(equipSubID);
                SubCurveDataCell curveCfg = LocalDataManager.Instance.GetSubCurveDataCell(subEquipCfg.Curve, equipCfg.Level);
                int idx = quality - 1;

                int count = subEquipCfg.Power.Count;
                if (count <= idx)
                {
                    idx = count - 1;
                }
                var subPower = subEquipCfg.Power[idx];
                var powerRate = curveCfg.PowerRate;
                var subReallyPower = (int)MathF.Floor(subEquipCfg.Power[idx] * curveCfg.PowerRate / 100);

                // 词条战力  = 词条品质对应战力*(SubCurve表中此词条在当前装备穿戴等级下对应的战力倍率)
                power += subReallyPower;

                //    SGF.Debuger.Log($"[Power] 词条 equipSubID: {equipSubID}, quality: {quality}, 倍率: {powerRate}, 等级: {equipCfg.Equip_Level},  曲线ID: {curveCfg.Id} subPower: {subPower},  词条 战力: {subReallyPower}");

            }
            //    SGF.Debuger.Log($"[Power] 装备词条 总战力: {power}");

            return power;
        }


        /// <summary>
        ///  获得单个槽位的战力
        /// </summary>
        /// <param name="slotPower">槽位战力</param>
        /// <param name="slotResonancePower">共鸣战力</param>
        public long GetEquipSlotsPowerBySlotID(int slotID)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 单个 装备槽位 战力:");


            EquipSlotData equitSlot = GameManager.Instance.GetEquipSlotData(slotID);
            if (equitSlot == null)
            {
                return 0;
            }
            var slotCfg = LocalDataManager.Instance.GetEquipSlotIntensifyData(equitSlot.SlotID, equitSlot.UpLv);
            if (slotCfg == null)
            {
                return 0;
            }


            return slotCfg.Power;
        }




        /// <summary>
        ///  装备基础战力: 即装备槽位战力 + 装备槽 共鸣战力
        /// </summary>
        /// <param name="slotPower">槽位战力</param>
        /// <param name="slotResonancePower">共鸣战力</param>
        public long GetEquipSlotsPower(out long slotPower, out long slotResonancePower)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 装备槽位 战力:");

            long power = 0;
            // 装备战力
            slotPower = 0;
            // 装备槽位的 共鸣战力 =  所有槽位等级总和 对应的 EquipSlotResonance 对应的 共鸣战力
            slotResonancePower = 0;

            int totalLv = 0;

            // 首先拿到 8个装备槽位
            for (int i = 0; i <= 8; i++)
            {
                EquipSlotData equitSlot = GameManager.Instance.GetEquipSlotData(i + 1);
                if (equitSlot == null)
                {
                    continue;
                }
                var slotCfg = LocalDataManager.Instance.GetEquipSlotIntensifyData(equitSlot.SlotID, equitSlot.UpLv);
                if (slotCfg == null)
                {
                    continue;
                }
                SGF.Debuger.Log($"[Power] 槽位: {i} SlotID: {equitSlot.SlotID}, UpLv: {equitSlot.UpLv}, 战力: {slotCfg.Power}");

                slotPower += slotCfg.Power;
                totalLv += equitSlot.UpLv;
            }
            SGF.Debuger.Log($"[Power] 装备槽位 战力: {slotPower}");

            // TODO: 胡哥哥
            // 需要计算装备 的 共鸣战力, 兰君的意思是
            {
                // 计算共鸣的 等级
                int resonanceLv = GameManager.Instance.GetEquipSlotAvgResonateLv();

                int jobID = (int)GameManager.Instance.GetPlayerJob(); ;


                if (resonanceLv > 0)
                {
                    // 取的 共鸣等级对应的 共鸣配置
                    var cfg = LocalDataManager.Instance.GetEquipSlotResonanceDataCell(resonanceLv, jobID);
                    if (cfg != null)
                    {
                        slotResonancePower = cfg.Power;
                    }
                }
                SGF.Debuger.Log($"[Power] 装备槽位 共鸣战力: {slotResonancePower}");

            }

            power = slotPower + slotResonancePower;
            SGF.Debuger.Log($"[Power] 装备 总战力: {power}");

            return power;
        }

        /// <summary>
        /// 已经装配的 技能战力
        /// </summary>
        public long GetSkillsPower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 职业技能 战力:");

            long power = 0;
            var allSkills = GameManager.Instance.HeroMD.JobSkillModel.AllSkill;
            // GameManager.Instance.HeroMD.
            foreach (SkillPos item in BattleManager.Instance.allSkillNotice.Skillposlist)
            {
                if (item.Islocked)
                {
                    continue;
                }

                // 拿到槽内的 职业技能id
                var jobSkillID = item.JobSkillID;

                // 拿到玩家 这个槽位  对应的 技能信息
                var singleSkillInfo = GameManager.Instance.M_MainPlayerCtrlBase.Data.GetSingleSkillInfo(jobSkillID);

                if (singleSkillInfo == null)
                {
                    continue;
                }

                int jobSkillLevel = singleSkillInfo.SkillLevel;

                // 然后找到对应的天赋 
                SkillLevelDataCell skillLevelDataCell = LocalDataManager.Instance.GetSkillLevelDataCell(jobSkillID, jobSkillLevel);
                if (skillLevelDataCell == null)
                {
                    continue;
                }
                //    SGF.Debuger.Log($"[Power] 职业技能  jobSkillID: {jobSkillID}, level: {jobSkillLevel},  战力: {skillLevelDataCell.Power}");

                power += skillLevelDataCell.Power;
            }

            //    SGF.Debuger.Log($"[Power] 职业技能  总战力: {power}");

            return power;
        }

        #endregion


        /// <summary>
        /// 获得单个战技的战力
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetPassivePowerBySkillId(int skillID, int lvNow)
        {

            var cfg = LocalDataManager.Instance.GetPassiveDescDataCellBySkillIdAndLevel(skillID, lvNow);

            if (cfg != null)
            {
                return cfg.Power;
            }

            return 0;
        }

        /// <summary>
        /// 获得单个战技的战力
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetPassivePowerBySkillIdAndPoints(int skillID, int points)
        {



            int maxLv = LocalDataManager.Instance.GetPassiveDescMaxLv(skillID);


            int num = points;

            //每一级的点数
            int perLevelPoint = 5;


            //计算技能等级
            int lvNext = 1;
            for (int i = 1; i <= (maxLv - 1); i++)
            {
                if (lvNext * perLevelPoint <= num)
                {
                    lvNext = lvNext + 1;
                }
            }
            int lvNow = lvNext - 1;


            long power = 0;

            if (lvNow > 0)
            {
                power = GetPassivePowerBySkillId(skillID, lvNow);
            }

            return power;
        }
        /// <summary>
        /// 获得战技 被动增加的战力
        /// </summary>
        public long GetPassivePower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 战技 被动技能 战力:");

            Dictionary<int, int> allPoints = new Dictionary<int, int>();

            //护符
            for (int i = 1; i <= 2; i++)
            {
                ItemMD itemData = BusinessManager.Instance.GetHeroEquipedAmulet(i);

                if (itemData != null)
                {
                    var combatSkills = itemData.CombatSkills;

                    for (int j = 0; j < combatSkills.List.Count; j++)
                    {
                        var cs = combatSkills.List[j];

                        if (!allPoints.ContainsKey(cs.ID))
                        {
                            allPoints.Add(cs.ID, 0);
                        }

                        int num = Frame.Util.CountOnes(cs.Points);
                        allPoints[cs.ID] = allPoints[cs.ID] + num;
                    }
                }
            }

            //铭器
            var M_EntityBase = GameManager.Instance.M_MainPlayerCtrlBase;
            var tweeter1 = M_EntityBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.Tweeter1);
            var tweeter2 = M_EntityBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.Tweeter2);

            if (tweeter1 > 0)
            {

                //鸣器的点数
                var data = (FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Tweeter) as TweeterMDMgr).GetTweeterByBaseId(tweeter1);
                //默认是1级别
                int lv = 1;
                if (data != null)
                {
                    lv = data.Level;

                }
                var cfg2 = LocalDataManager.Instance.GetEtchDataCell(tweeter1);
                if (!allPoints.ContainsKey(cfg2.BattleSkillID))
                {
                    allPoints.Add(cfg2.BattleSkillID, 0);
                }
                var cfg = LocalDataManager.Instance.GetEtchUPDataCell(lv);

                allPoints[cfg2.BattleSkillID] = allPoints[cfg2.BattleSkillID] + cfg.BattleSkillIPoint;

            }

            if (tweeter2 > 0)
            {
                //鸣器的点数
                var data = (FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Tweeter) as TweeterMDMgr).GetTweeterByBaseId(tweeter2);
                //默认是1级别
                int lv = 1;
                if (data != null)
                {
                    lv = data.Level;

                }

                var cfg2 = LocalDataManager.Instance.GetEtchDataCell(tweeter2);
                if (!allPoints.ContainsKey(cfg2.BattleSkillID))
                {
                    allPoints.Add(cfg2.BattleSkillID, 0);
                }
                var cfg = LocalDataManager.Instance.GetEtchUPDataCell(lv);
                allPoints[cfg2.BattleSkillID] = allPoints[cfg2.BattleSkillID] + cfg.BattleSkillIPoint;
            }

            //总战力
            long allPower = 0;

            //计算技能等级
            foreach (var kv in allPoints)
            {
                int maxLv = LocalDataManager.Instance.GetPassiveDescMaxLv(kv.Key);


                int skillID = kv.Key;
                int num = kv.Value;

                //每一级的点数
                int perLevelPoint = 5;


                //计算技能等级
                int lvNext = 1;
                for (int i = 1; i <= (maxLv - 1); i++)
                {
                    if (lvNext * perLevelPoint <= num)
                    {
                        lvNext = lvNext + 1;
                    }
                }
                int lvNow = lvNext - 1;


                long power = 0;

                if (lvNow > 0)
                {
                    power = GetPassivePowerBySkillId(skillID, lvNow);
                    //    SGF.Debuger.Log($"[Power] 被动技能: {skillID}  技能等级: {lvNow},  战力: {power}");
                    allPower += power;
                }

            }



            return allPower;
        }


        #region 伙伴战力
        // 伙伴出战战力=（等级战力+资质转化表战力）*星级缩放系数+伙伴技能战力+伙伴装备战力
        // 伙伴助战战力= 助战槽位战力+伙伴装备战力

        /// <summary>
        /// 获得所有出战伙伴的 总战力
        /// </summary>
        public long GetPlayedPartnersPower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 出战伙伴总战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                List<PartnerMD> list = partnerMgr.GetInPlayedPartners();

                foreach (var partner in list)
                {
                    var index = partner.Index;
                    var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);
                    var levelPower = GetPartnerLevelPower(partner, partnerCfg);
                    var conversionPower = GetPartnerConversionPower(partner);

                    var skillPower = GetPartnerSkillPower(partner);

                    var equipPower = GetPartnerEquipedEquipPower(partner);

                    var starExpendPower = GetStarExpendPower(partner, partnerCfg);

                    var itemPartnerPower = (levelPower + conversionPower + starExpendPower + skillPower + equipPower);
                    // var itemPartnerPower = (levelPower + conversionPower + starExpendPower + equipPower);
                    //    SGF.Debuger.Log($"[Power] 伙伴: {partner.Index} levelPower: {levelPower}, conversionPower: {conversionPower},  equipPower: {equipPower}, starExpendPower: {starExpendPower}, 技能战力: {skillPower},  单个伙伴总战力: {itemPartnerPower}  ");

                    power += itemPartnerPower;
                }

            }
            //    SGF.Debuger.Log($"[Power] 出战 伙伴总  战力: {power}");

            return power;
        }

        /// <summary>
        /// 伙伴出战的 模块战力, 包含 伙伴审计战力 +  伙伴资质战力
        /// </summary>
        /// <returns></returns>
        public long GetPlayedPartnersModulePower()
        {
            // SGF.Debuger.Log($"[Power] 开始计算  伙伴出战 [伙伴升级+伙伴资质] 总战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                var list = partnerMgr.GetInPlayedPartners();

                foreach (var partner in list)
                {
                    var index = partner.Index;
                    var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);
                    var levelPower = GetPartnerLevelPower(partner, partnerCfg);
                    var conversionPower = GetPartnerConversionPower(partner);


                    var itemPartnerPower = levelPower + conversionPower;
                    // SGF.Debuger.Log($"[Power] 伙伴: {partner.Index} levelPower: {levelPower}, conversionPower: {conversionPower}, 单个伙伴 基础战力: {itemPartnerPower}  ");

                    power += itemPartnerPower;
                }

            }
            // SGF.Debuger.Log($"[Power] 出战 伙伴基础总  战力: {power}");

            return power;
        }

        /// <summary>
        /// 计算出战伙伴 进阶模块 总站力 = 伙伴进阶战力 + 伙伴技能战力
        /// </summary>
        public long GetPlayedPartnersStarExpendModulePower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 出战伙伴 进阶模块 总战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                List<PartnerMD> list = partnerMgr.GetInPlayedPartners();

                foreach (var partner in list)
                {
                    var index = partner.Index;
                    var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);
                    var starExpendPower = GetStarExpendPower(partner, partnerCfg);
                    var skillPower = GetPartnerSkillPower(partner);

                    power += (starExpendPower + skillPower);

                    //    SGF.Debuger.Log($"[Power] 伙伴: {partner.Index} 升星战力: {starExpendPower}, 技能战力: {skillPower}, 进阶模块战力: {starExpendPower + skillPower}  ");


                }

            }
            //    SGF.Debuger.Log($"[Power] 出战 伙伴进阶总  战力: {power}");

            return power;
        }

        /// <summary>
        /// 助战伙伴 总战力 = 伙伴助战战力 + 伙伴装备战力
        /// </summary>
        /// <returns></returns>
        public long GetAssiantPartnersPower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 助战伙伴总战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                var list = partnerMgr.GetInAssistPartners();
                foreach (var partner in list)
                {
                    var index = partner.Index;
                    var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);

                    var equipPower = GetPartnerEquipedEquipPower(partner);
                    var assitPower = GetAssistPower(partner, partnerCfg);
                    //    SGF.Debuger.Log($"[Power] 助战 伙伴: {index}, equipPower: {equipPower}, assitPower: {assitPower} , 助战伙伴战力: {assitPower + equipPower} ");

                    power += (assitPower + equipPower);
                }

            }
            //    SGF.Debuger.Log($"[Power] 助战伙伴 总战力: {power} ");

            return power;
        }

        /// <summary>
        /// 伙伴助战模块总战力 = 伙伴助战战力
        /// </summary>
        public long GetAssiantPartnersModulePower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 助战 伙伴模块 战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                var list = partnerMgr.GetInAssistPartners();
                foreach (var partner in list)
                {
                    var index = partner.Index;
                    var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);


                    var assitPower = GetAssistPower(partner, partnerCfg);
                    //    SGF.Debuger.Log($"[Power] 助战 伙伴: {index}, assitPower: {assitPower} , 助战伙伴战力: {assitPower} ");

                    power += assitPower;
                }

            }
            //    SGF.Debuger.Log($"[Power] 伙伴助战 基础 总战力: {power} ");

            return power;
        }

        /// <summary>
        /// 计算伙伴的装备模块战力[包含:出战/助战]
        /// </summary>
        public long GetPartnersEquipModulePower()
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴装备模块 战力:");

            long power = 0;
            var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMgr != null)
            {
                var list = partnerMgr.GetInAssistPartners();
                foreach (var partner in list)
                {
                    var index = partner.Index;

                    var equipPower = GetPartnerEquipedEquipPower(partner);
                    //    SGF.Debuger.Log($"[Power] 助战 伙伴: {index}, 装备战力: {equipPower} ");

                    power += equipPower;
                }

                var list1 = partnerMgr.GetInPlayedPartners();
                foreach (var partner in list1)
                {
                    var index = partner.Index;

                    var equipPower = GetPartnerEquipedEquipPower(partner);
                    //    SGF.Debuger.Log($"[Power] 出战 伙伴: {index}, 装备战力: {equipPower} ");

                    power += equipPower;
                }


            }
            //    SGF.Debuger.Log($"[Power] 伙伴装备模块 总战力: {power} ");

            return power;

        }


        /// <summary>
        /// 先计算 伙伴等级战力
        /// 	伙伴升级战力=读取此伙伴属性组在当前等级的战力字段
        /// </summary>
        public long GetPartnerLevelPower(PartnerMD partnerMD, PartnerDataCell partnerDataCell)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 等级 战力:");

            //伙伴属性组
            int nature = partnerDataCell.Nature;
            // 伙伴等级
            int level = partnerMD.Level;

            var cfg = LocalDataManager.Instance.GetPartnerNatureDataCell(nature, level);
            if (cfg == null)
            {
                return 0;
            }
            //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index}  等级  总战力: {cfg.Power} ");

            return cfg.Power;

        }

        /// <summary>
        /// 获得伙伴的所有资质 战力和
        /// </summary>
        /// <param name="partnerMD"></param>
        /// <returns></returns>
        public long GetPartnerConversionPower(PartnerMD partnerMD)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 资质总 战力:");

            long power = 0;
            // 伙伴资质列表
            var aptitudes = partnerMD.AMD.Aptitudes;

            foreach (var item in aptitudes)
            {

                var partnerAMD = item.Value;

                // 资质id
                var aptiudeID = partnerAMD.AptiudeID;

                long aptiudePower = 0;

                // 伙伴资质 = 资质基础值+资质等级值
                var aptiudeValue = partnerAMD.BaseValue + partnerAMD.LevelValue;

                var curParConversionCfgs = LocalDataManager.Instance.GetParConversionDataCell(aptiudeID, aptiudeValue);

                for (int i = 0; i < curParConversionCfgs.Count; i++)
                {
                    var curParConversionCfg = curParConversionCfgs[i];

                    //    SGF.Debuger.Log($"[Power] 资质: {aptiudeID},  BaseValue: {partnerAMD.BaseValue}, LevelValue: {partnerAMD.LevelValue}, 总资质: {aptiudeValue} ");

                    // 如果找到了 当前 资质
                    if (curParConversionCfg != null)
                    {
                        aptiudePower = curParConversionCfg.Power + (long)(Math.Floor((aptiudeValue - curParConversionCfg.Range) * curParConversionCfg.PowerCo / 10000f));
                        //    SGF.Debuger.Log($"[Power] 资质: {aptiudeID}  资质战力 Power: {aptiudePower}");

                        power += aptiudePower;
                    }
                }

            }
            //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} 资质总 战力: {power}");

            return power;
        }


        /// <summary>
        /// 计算伙伴的技能战力
        /// </summary>
        /// <param name="partnerMD"></param>
        public long GetPartnerSkillPower(PartnerMD partnerMD)
        {
            long power = 0;
            // 伙伴id
            int id = (int)partnerMD.Index;
            var star = partnerMD.CurStar;
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 技能总 战力:");

            var parSkillCfg = LocalDataManager.Instance.GetParSkillDataCell(id, star);
            if (parSkillCfg != null)
            {
                var skills = parSkillCfg.SelfSkill;
                for (int i = 0; i < skills.Count; i++)
                {
                    var skillID = skills[i];
                    var skillLevel = parSkillCfg.SelfSkillLV[i];

                    var skillDesc = LocalDataManager.Instance.GetPartnerSkillDescDataCellBySkillIdAndLevel(skillID, skillLevel);
                    if (skillDesc != null)
                    {
                        var skillDescPower = skillDesc.Power;
                        //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} skillID: {skillID}, skillLevel: {skillLevel}, 战力: {skillDescPower}");

                        power += skillDescPower;
                    }
                }

                // 登场技
                int parSkill = parSkillCfg.ParSkill;
                // 登场技 等级
                int parSkillLv = parSkillCfg.ParSkillLV;

                var parSkillDesc = LocalDataManager.Instance.GetPartnerSkillDescDataCellBySkillIdAndLevel(parSkill, parSkillLv);
                if (parSkillDesc != null)
                {
                    var parSkillDescPower = parSkillDesc.Power;
                    //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} 登场 skillID: {parSkill}, skillLevel: {parSkillLv}, 战力: {parSkillDescPower}");

                    power += parSkillDescPower;
                }
            }
            //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} 技能总战力: {power}");

            return power;
        }

        /// <summary>
        /// 获得伙伴的 已经装备的战力
        /// </summary>
        /// <param name="partnerMD"></param>
        /// <returns></returns>
        public long GetPartnerEquipedEquipPower(PartnerMD partnerMD)
        {
            long power = 0;

            ulong partnerID = partnerMD.ID;
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 装备总 战力:");

            // 伙伴有2个装备位置
            for (int i = 1; i <= 3; i++)
            {
                ItemMD equip = BusinessManager.Instance.GetPartnerEquipedEquip(partnerID, i);
                // kl 说伙伴装备 没有 纹章/词条/强化, 所以只考虑 装备主属性 战力
                var equipPower = GetEquipMDPropsPower(equip);
                //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} 装备位: {i} , 装备战力: {equipPower}");

                power += equipPower;
            }
            //    SGF.Debuger.Log($"[Power] 伙伴: {partnerMD.Index} 装备总战力: {power}");

            return power;
        }

        /// <summary>
        /// 等到伙伴的 升星战力
        /// </summary>
        /// <param name="quality"></param>
        /// <param name="star"></param>
        /// <returns></returns>
        public long GetStarExpendPower(PartnerMD partnerMD, PartnerDataCell partnerDataCell)
        {
            int quality = partnerDataCell.Quality;
            int star = partnerMD.CurStar;
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 升星总 战力:");

            var starExpendCfg = LocalDataManager.Instance.GetStarExpendDataCell(quality, star);

            long power = 0;

            if (starExpendCfg != null)
            {
                power = (GetPartnerLevelPower(partnerMD, partnerDataCell) + GetPartnerConversionPower(partnerMD)) * starExpendCfg.Power / 100;
                //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} quality: {quality}, star: {star},  升星战力: {starExpendCfg.Power} --> 伙伴进阶战力: {power}");

                return power;
            }

            return power;
        }


        /// <summary>
        /// 获得伙伴助战战力
        /// </summary>
        /// <param name="partnerMD"></param>
        /// <param name="partnerDataCell"></param>
        /// <returns></returns>
        public long GetAssistPower(PartnerMD partnerMD, PartnerDataCell partnerDataCell)
        {
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴: {partnerMD.Index} 助战总 战力:");

            long power = 0;

            var seat = partnerMD.CurSeat;
            // 伙伴职业 取的是 玩家的职业
            var job = GameManager.Instance.GetPlayerJob();

            // 伙伴助战槽位配置表
            var cfgs = LocalDataManager.Instance.GetAssistDataCells((int)job, seat);
            //    SGF.Debuger.Log($"[Power] 开始计算 伙伴职业 job: {job}, 助战槽位: {seat} ");

            // 资质的 起始id， 找到一个资质的战力后，后续就不再继续找这个资质对应的战力
            int jStart = 0;

            for (int i = 0; i < cfgs.Count; i++)
            {
                var assistCfg = cfgs[i];
                // 槽位资质
                var slotQualifications = assistCfg.Qualifications;

                var qualificationCount = slotQualifications.Count;

                for (int j = jStart; j < qualificationCount; j++)
                {
                    // 槽位配置的资质
                    var qualification = slotQualifications[j];

                    // 如果 不存在这个槽位资质, 结束 这个资质的查找( j = qualificationCount ), 跳到下一个资质 jStart ++ ，从新开始查找下一个资质(i=0)
                    if (!partnerMD.AMD.Aptitudes.ContainsKey(qualification))
                    {
                        jStart++;
                        j = qualificationCount;
                        i = 0;
                        continue;
                    }
                    var attiMD = partnerMD.AMD.Aptitudes[qualification];

                    // 资质的 总值
                    var value = attiMD.BaseValue + attiMD.LevelValue;

                    var curRequireValue = assistCfg.Require[j];

                    bool curIsTargeLevel = false;

                    // 如果已经是最后一个了, 那最后一个就是当前等级
                    if (i == cfgs.Count - 1)
                    {
                        curIsTargeLevel = true;
                    }
                    else
                    {
                        // 如过不是i最后一个, 那就判断 当前等级是否就是 当前资质对应的等级
                        var nexLevelCfg = cfgs[i + 1];
                        var nextRequreValue = nexLevelCfg.Require[j];

                        // 此时 已经找到了 这个资质 对应的 等级
                        if (curRequireValue <= value && value < nextRequreValue)
                        {
                            curIsTargeLevel = true;
                        }
                        else
                        {
                            // 如果不是, 那就继续往下面找
                            curIsTargeLevel = false;
                        }
                    }

                    // 如果找到了目标等级, 就按照当前等级 计算战力
                    if (curIsTargeLevel)
                    {
                        // 当前阶段的 对应 资质的 power 
                        long curLevelPower = assistCfg.Power + (long)Math.Floor((value - curRequireValue) * assistCfg.PowerCo / 10000f);

                        power += curLevelPower;

                        //    SGF.Debuger.Log($"[Power] 助战伙伴 包含 资质: {qualification}, baseValue: {attiMD.BaseValue}, levelValue: {attiMD.LevelValue}, 当前资质值: {value}, 资质要求: {curRequireValue}");
                        //    SGF.Debuger.Log($"[Power] 助战伙伴 当前阶段: {i} , 资质数据j: {j}, 阶段战力: {curLevelPower}");

                        // 找到对应阶段战力后,  判断 资质的 资质是否已经全计算完
                        // 如果已经完全计算完了 资质, 那就 直接结束  ( i = cfgs.Count )
                        if (jStart >= qualificationCount - 1)
                        {
                            i = cfgs.Count;
                        }
                        else
                        {
                            // 如果没有计算完, 那就 资质 jStart ++ , 从新开始查找这个资质的等级 i = 10

                            jStart++;
                            i = 0;
                        }
                    }
                    else
                    {
                        // 如果没找到, 那就 跳过 后续资质 j 的循环, 继续往后面等级查找
                        j = qualificationCount;
                    }

                    continue;

                    // // 如果存在下一个登录的 助战资质配置
                    // if (i + 1 < cfgs.Count)
                    // {
                    //     var nexLevelCfg = cfgs[i + 1];
                    //     var nextRequreValue = nexLevelCfg.Require[j];

                    //     // 此时 已经找到了 这个资质 对应的 等级
                    //     if (curRequireValue <= value && value < nextRequreValue)
                    //     {
                    //         // 当前阶段的 对应 资质的 power 
                    //         long curLevelPower = assistCfg.Power + (long)Math.Floor((value - curRequireValue) * assistCfg.PowerCo / 10000f);

                    //         power += curLevelPower;

                    //     //    SGF.Debuger.Log($"[Power] 助战伙伴 包含 资质: {qualification}, baseValue: {attiMD.BaseValue}, levelValue: {attiMD.LevelValue}, 当前资质值: {value}, 资质要求: {curRequireValue}");
                    //     //    SGF.Debuger.Log($"[Power] 助战伙伴 当前阶段: {i} , 资质数据j: {j}, 阶段战力: {curLevelPower}");

                    //         // 找到对应阶段战力后,  判断 资质的 资质是否已经全计算完
                    //         // 如果已经完全计算完了 资质, 那就 直接结束  ( i = cfgs.Count )
                    //         if (jStart >= qualificationCount - 1)
                    //         {
                    //             i = cfgs.Count;
                    //         }
                    //         else
                    //         {
                    //             // 如果没有计算完, 那就 资质 jStart ++ , 从新开始查找这个资质的等级 i = 10

                    //             jStart++;
                    //             i = 0;
                    //         }

                    //     }
                    //     else
                    //     {
                    //         // 下一阶段 资质战力存在，那就继续往下一个阶段查找
                    //         // 所以此处先 结束 当前资质 j 的循环
                    //         j = qualificationCount;

                    //     }
                    //     continue;

                    // }
                    // else
                    // {
                    //     // 如果已经是最后 一个等级了, 那最后一个就是 它的当前等级
                    //     // 当前阶段的 对应 资质的 power 
                    //     long curLevelPower = assistCfg.Power + (long)Math.Floor((value - curRequireValue) * assistCfg.PowerCo / 10000f);

                    //     power += curLevelPower;

                    // //    SGF.Debuger.Log($"[Power] 助战伙伴 包含 资质: {qualification}, baseValue: {attiMD.BaseValue}, levelValue: {attiMD.LevelValue}, 当前资质值: {value}, 资质要求: {curRequireValue}");
                    // //    SGF.Debuger.Log($"[Power] 助战伙伴 当前阶段: {i} , 资质数据j: {j}, 阶段战力: {curLevelPower}");

                    //     // 找到对应阶段战力后,  判断 资质的 资质是否已经全计算完
                    //     // 如果已经完全计算完了 资质, 那就 直接结束  ( i = cfgs.Count )
                    //     if (jStart >= qualificationCount - 1)
                    //     {
                    //         i = cfgs.Count;
                    //     }
                    //     else
                    //     {
                    //         // 如果没有计算完, 那就 资质 jStart ++ , 从新开始查找这个资质的等级 i = 10

                    //         jStart++;
                    //         i = 0;
                    //     }
                    // }
                }
            }

            return power;


        }



        #endregion


        #region 各个界面 模块显示战力的接口. 显示的战力 可能跟实际战力不一样， 比如装备槽位上的装备战力, 只需要显示 装备主属性战力+词条战力, 不考虑对应的槽位战力
        /// <summary>
        /// 装备战力= 主属性战力 + 装备词条战力 
        /// [不包含 纹章战力和共鸣战力]
        /// </summary>
        /// <param name="equip"></param>
        public long GetShowEquipPower(ItemMD equip)
        {
            long equipPower = GetEquipMDPropsPower(equip);
            long subPower = GetEquipSubAttack(equip);

            // 装备显示战力 =  装备主属性战力 + 装备词条战力
            long power = equipPower + subPower;

            return power;
        }

        /// <summary>
        /// 获得单个纹章战力 = 战力字段+if(有共鸣,共鸣战力字段,0)
        /// </summary>
        /// <param name="equip"></param>
        public long GetShowHeraldryPowerByGemID(long gemID, bool isEcho = false)
        {

            long power = 0;

            if (gemID != 0)
            {

                //宝石的共鸣
                var heraldryEquipCfg = LocalDataManager.Instance.GetHeraldryEquipDataCell(gemID);
                power += heraldryEquipCfg.Power1;

                //判断是是否触发了共鸣
                if (isEcho)
                {
                    //    SGF.Debuger.Log($"[Power] 纹章 产生的 共鸣战力 : {heraldryEquipCfg.Power2}");

                    power += heraldryEquipCfg.Power2;
                }
            }

            return power;
        }



        /// <summary>
        /// 得到装备上的 纹章战力 = 战力字段+if(有共鸣,共鸣战力字段,0)
        /// </summary>
        /// <param name="equip"></param>
        public long GetShowHeraldryPower(ItemMD equip)
        {

            long power = 0;
            //显示纹章信息
            var gemSlots = equip.GemSlots;
            if (gemSlots == null)
            {
                return power;
            }
            //查看纹章
            for (int j = 0; j < gemSlots.List.Count; j++)
            {
                //刷新纹章槽位信息
                var gemSlot = gemSlots.List[j];

                int slotID = gemSlot.SlotID;
                ulong gemID = gemSlot.GemID;

                if (gemID != 0)
                {
                    var itemData = BusinessManager.Instance.GetItemByItemEntityId(gemID);


                    //宝石的共鸣
                    var heraldryEquipCfg = LocalDataManager.Instance.GetHeraldryEquipDataCell(itemData.BaseID);
                    power += heraldryEquipCfg.Power1;
                    //    SGF.Debuger.Log($"[Power] 纹章 slotID: {slotID}, 装备id: {itemData.BaseID}, 战力: {heraldryEquipCfg.Power1}");

                    //判断是是否触发了共鸣
                    if (slotID == heraldryEquipCfg.ResonanceSlot)
                    {
                        //    SGF.Debuger.Log($"[Power] 纹章 产生的 共鸣战力 : {heraldryEquipCfg.Power2}");

                        power += heraldryEquipCfg.Power2;
                    }
                }

            }

            return power;
        }



        /// <summary>
        /// 得到背包道具 显示的战力
        ///     道具类型为装备 战力 = 主属性战力 + 副属性战力
        ///     道具类型为纹章 战力 = 纹章战力
        /// </summary>
        /// <param name="equip"></param>
        /// <returns></returns>
        public long GetBagItemShowPower(ItemMD equip)
        {
            var itemID = equip.BaseID;

            // 胡哥哥语录: item 先去 道具表中查, 道具表中有个 itemType, 如果是 装备类型,就去找 equip
            var itemDataCell = LocalDataManager.Instance.GetItemDataCell(itemID);

            if (itemDataCell == null)
            {
                return 0;
            }
            // 胡哥哥说 2是 道具, 5是护符
            if (itemDataCell.ItemType == 2)
            {
                return GetShowEquipPower(equip);
            }
            else if (itemDataCell.ItemType == 5)
            {
                return GetShowHeraldryPower(equip);
            }


            return 0;
        }

        /// <summary>
        /// 得到装备上的 战技战力----> 轮契
        /// </summary>
        /// <param name="equip"></param>
        /// <returns></returns>
        public long GetShowEquipPassivePower(ItemMD itemData)
        {
            long allPower = 0;

            Dictionary<int, int> allPoints = new Dictionary<int, int>();

            if (itemData != null)
            {
                var combatSkills = itemData.CombatSkills;

                for (int j = 0; j < combatSkills.List.Count; j++)
                {
                    var cs = combatSkills.List[j];

                    if (!allPoints.ContainsKey(cs.ID))
                    {
                        allPoints.Add(cs.ID, 0);
                    }

                    int num = Frame.Util.CountOnes(cs.Points);
                    allPoints[cs.ID] = allPoints[cs.ID] + num;
                }
            }



            //计算技能等级
            foreach (var kv in allPoints)
            {
                int maxLv = LocalDataManager.Instance.GetPassiveDescMaxLv(kv.Key);


                int skillID = kv.Key;
                int num = kv.Value;

                //每一级的点数
                int perLevelPoint = 5;


                //计算技能等级
                int lvNext = 1;
                for (int i = 1; i <= (maxLv - 1); i++)
                {
                    if (lvNext * perLevelPoint <= num)
                    {
                        lvNext = lvNext + 1;
                    }
                }
                int lvNow = lvNext - 1;


                long power = 0;

                if (lvNow > 0)
                {
                    power = GetPassivePowerBySkillId(skillID, lvNow);
                    //    SGF.Debuger.Log($"[Power] 被动技能: {skillID}  技能等级: {lvNow},  战力: {power}");
                    allPower += power;
                }

            }


            return allPower;
        }


        /// <summary>
        /// 静态战力 (伙伴详情/魂石伙伴)
        ///     东烨 说静态战力 =  出战战力, 跟兰君的不一样, 所以先按 东烨 的出战战力算
        /// </summary>
        /// <returns></returns>
        public long GetShowPartnerStaticPower(PartnerMD partner)
        {
            long power = 0;
            var index = partner.Index;
            var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);
            var levelPower = GetPartnerLevelPower(partner, partnerCfg);
            var conversionPower = GetPartnerConversionPower(partner);

            var skillPower = GetPartnerSkillPower(partner);

            var equipPower = GetPartnerEquipedEquipPower(partner);

            var starExpendPower = GetStarExpendPower(partner, partnerCfg);

            power = (levelPower + conversionPower + starExpendPower + skillPower + equipPower);
            return power;
        }

        /// <summary>
        /// 魂石 静态战力
        /// </summary>
        /// <param name="equip"></param>
        /// <param name="nextPower">返回下一个等级魂石的战力</param>
        /// <returns></returns>
        public long GetShowPartnerStaticEquipPower(ItemMD equip, out long nextPower)
        {
            var equipID = (int)equip.BaseID;
            var eCfg = LocalDataManager.Instance.GetEquipDataCell(equipID);
            var composeDataCell = LocalDataManager.Instance.GetComposeDataCell(equipID);

            var nextCfg = composeDataCell == null ? null : LocalDataManager.Instance.GetEquipDataCell((int)composeDataCell.TargeID);

            nextPower = nextCfg == null ? 0 : nextCfg.Power;
            return eCfg.Power;


        }

        /// <summary>
        /// 伙伴编队的总站力 = 出战战力+助战战力
        /// </summary>
        public long GetPartTotalPower(List<PartnerMD> playedPartners, List<PartnerMD> assiantPartners)
        {
            long power = 0;
            // 所有伙伴出战的战力
            foreach (var item in playedPartners)
            {
                power += GetShowPartnerStaticPower(item);
            }

            // 所有伙伴助战的战力
            foreach (var item in assiantPartners)
            {
                var index = item.Index;
                var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(index);

                var equipPower = GetPartnerEquipedEquipPower(item);
                var assitPower = GetAssistPower(item, partnerCfg);

                power += (equipPower + assitPower);
            }

            return power;

        }

        /// <summary>
        /// 得到显示的 道具战力
        /// </summary>
        /// <param name="itemMD"></param>
        /// <param name="nextPower"></param>
        /// <returns></returns>
        public long GetShowItemMDPower(ItemMD itemMD, out long nextPower)
        {
            var equipID = (int)itemMD.BaseID;
            var eCfg = LocalDataManager.Instance.GetEquipDataCell(equipID);

            var composeDataCell = LocalDataManager.Instance.GetComposeDataCell(equipID);

            var nextCfg = composeDataCell == null ? null : LocalDataManager.Instance.GetEquipDataCell((int)composeDataCell.TargeID);

            nextPower = nextCfg == null ? 0 : nextCfg.Power;
            return eCfg.Power;
        }

        #endregion




        /// <summary>
        /// 计算战力 推荐比
        /// </summary>
        public PowerAllData GetPowerModuleDatas()
        {
            // 养成线当前战力/养成线极限战力/(养成线极限战力/总极限战力)
            var powerAllData = new PowerAllData();
            // 推荐的子模块 数据
            List<PowerSubDataModule> recommendSubModules = powerAllData.RecommendSubModules;

            Dictionary<FightPowerModuleEnum, PowerBigDataModule> bigModules = powerAllData.BigModules;


            Action<FightPowerModuleEnum, long> createModule = (FightPowerModuleEnum powerModule, long power) =>
            {
                PowerSubDataModule powerData = PowerDataModule.Create<PowerSubDataModule>(powerModule, power);

                // 只有开启了, 才加入推荐
                if (powerData.IsOpen)
                {
                    // 增加 模块推荐数据
                    recommendSubModules.Add(powerData);
                }


                if (!bigModules.TryGetValue(powerData.ParentModule, out var powerBigDataModule))
                {
                    powerBigDataModule = PowerDataModule.Create<PowerBigDataModule>(powerData.ParentModule, 0);
                    bigModules.Add(powerData.ParentModule, powerBigDataModule);
                }

                // 将子模块数据 加入 大模块数据中
                powerBigDataModule.AddSubModule(powerData);
            };

            // Role_GrowUp = 101, // 冒险成长    AdvGradeExp	LevelExp
            {
                // 2024/8/1
                // 策划要求冒险成长战力 只计算玩家等级战力
                // long advGradeExpPower = GetAdvGradeExpPower();
                long levelExpPower = GetLevelExpPower();

                // long curPower = advGradeExpPower + levelExpPower;
                long curPower = levelExpPower;

                // powerData.Log("角色");
                createModule.Invoke(FightPowerModuleEnum.Role_GrowUp, curPower);

            }

            // Role_AttriTT = 102, // 星灵     AttriTTNode
            {
                long curPower = GetTalentTreeAttack(out long power1, out long power2, out long power3);

                // powerData.Log("星灵");
                createModule.Invoke(FightPowerModuleEnum.Role_AttriTT, curPower);

            }



            // //  = #103,饰品主属性
            // //  = #104,饰品副属性
            // //  = #105,饰品套装属性


            // Equip_Slot = 201, //  强化槽位+共鸣战力     EquipSlotIntensify
            {
                long slopPower = GetEquipSlotsPower(out long slotPower, out long slotResonancePower);

                // 装备强化 = 强化槽位+共鸣战力  
                createModule.Invoke(FightPowerModuleEnum.Equip_Slot, slopPower);
            }


            // Equip_Sub = 202, // 主属性战力      EquipSub	SubCurve
            {
                // 装备主属性战力
                long propPower = GetEquipPropsPower();


                createModule.Invoke(FightPowerModuleEnum.Equip_Sub, propPower);
            }

            // 装备 重构战力 
            {
                // 装备 重构战力 =  装备 副属性词条
                long subPower = GetEquipSubPower();

                createModule.Invoke(FightPowerModuleEnum.Equip_Rebuild, subPower);
            }

            // Equip_Heraldry = 204, // 日月纹章      HeraldryEquip
            {
                long equipHeraldyPower = GetHeraldryEquipTotalPower(out long heraldyPower, out long resonanceheraldyPower);


                // 所有纹章 战力
                createModule.Invoke(FightPowerModuleEnum.Equip_Heraldry, equipHeraldyPower);
            }

            // Skill_Level = 301, // 角色技能      SkillLevel
            {
                long curPower = GetSkillsPower();

                // powerData.Log("角色技能");
                createModule.Invoke(FightPowerModuleEnum.Skill_Level, curPower);

            }

            // Skill_Passive = 302, // 鸣魄战技      PassiveDesc
            {
                long curPower = GetPassivePower();

                // powerData.Log("鸣魄战技");
                createModule.Invoke(FightPowerModuleEnum.Skill_Passive, curPower);

            }

            // Partner_Playing = 401, // 伙伴出战      	ParConversion
            {
                long curPower = GetPlayedPartnersModulePower();

                // powerData.Log("伙伴出战");
                createModule.Invoke(FightPowerModuleEnum.Partner_Playing, curPower);

            }
            // Partner_Assist = 402, // 伙伴助战      Assist
            {
                long curPower = GetAssiantPartnersModulePower();

                // powerData.Log("伙伴助战");
                createModule.Invoke(FightPowerModuleEnum.Partner_Assist, curPower);

            }

            // Partner_Star = 403, // 伙伴进阶      StarExpend	PartnerSkillDesc
            {
                long curPower = GetPlayedPartnersStarExpendModulePower();

                // powerData.Log("伙伴进阶");
                createModule.Invoke(FightPowerModuleEnum.Partner_Star, curPower);

            }
            // Partner_Equip = 404, // 伙伴装备      PartnerEquip
            {
                long curPower = GetPartnersEquipModulePower();

                createModule.Invoke(FightPowerModuleEnum.Partner_Equip, curPower);
                // powerData.Log("伙伴装备");

            }


            powerAllData.Refresh();


            //    SGF.Debuger.Log($"[Power] ---------------------------------------------------------");
            //    SGF.Debuger.Log($"[Power] 开始打印 模块数据");
            foreach (var item in bigModules)
            {
                // 模块战力 刷新
                item.Value.Log();
            }

            //    SGF.Debuger.Log($"[Power] 打印 模块数据结束 ");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");

            //    SGF.Debuger.Log($"[Power] ********************************************************");
            //    SGF.Debuger.Log($"[Power] 开始打印 推荐数据");
            recommendSubModules.Sort((powerModuleA, powerModuleB) =>
            {
                if (powerModuleA.RecommendValue != powerModuleB.RecommendValue)
                {
                    return powerModuleA.RecommendValue.CompareTo(powerModuleB.RecommendValue);
                }
                else
                {
                    return powerModuleA.PowerModuleID.CompareTo(powerModuleB.PowerModuleID);
                }
            });

            foreach (var item in recommendSubModules)
            {
                item.Log();
            }
            //    SGF.Debuger.Log($"[Power] 打印 推荐数据结束");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");

            return powerAllData;
        }

        /// <summary>
        /// 得到玩家的 总站力
        /// </summary>
        public long GetPlayerTotalPower()
        {
            long power = 0;
            //    SGF.Debuger.Log($"[Power] [总] 开始计算 玩家 总战力:");
            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 冒险等级战力 ==========:");
                // long temp = GetAdvGradeExpPower();
                // power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 冒险等级战力: {temp}  总战力: {power}");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 玩家等级战力 ==========:");
                long temp = GetLevelExpPower();
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 玩家等级战力: {temp}  总战力: {power}");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 天赋树战力 ==========:");
                long temp = GetTalentTreeAttack(out long power1, out long power2, out long power3);
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 天赋树战力: {temp}  总战力: {power}");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 纹章战力 ==========:");
                long temp = GetHeraldryEquipTotalPower(out long heraldryPower, out long resonanceJeraldryPower);
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 纹章总战力: {temp}, 纹章共鸣总站力: {resonanceJeraldryPower}   总战力: {power}");
            }

            {
                SGF.Debuger.Log($"[Power] [总] ======== 装备主属性战力 ==========:  ");
                long temp = GetEquipPropsPower();
                power += temp;
                SGF.Debuger.Log($"[Power] [总] + 装备主属性战力: {temp}  总战力: {power} --------------");
            }

            {
                SGF.Debuger.Log($"[Power] [总] ======== 装备词条战力 ==========:");
                long temp = GetEquipSubPower();
                power += temp;
                SGF.Debuger.Log($"[Power] [总] + 装备词条战力: {temp}  总战力: {power} --------------");
            }


            {
                SGF.Debuger.Log($"[Power] [总] ======== 装备槽位战力 ==========:");
                long temp = GetEquipSlotsPower(out long slotPower, out long slotResonancePower);
                power += temp;
                SGF.Debuger.Log($"[Power] [总] + 装备槽位战力: {temp} , 槽位共鸣战力: {slotResonancePower}, 总战力: {power} --------------");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 职业技能 战力 ==========:");
                long temp = GetSkillsPower();
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 职业技能 战力: {temp}  总战力: {power}");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 被动技能 战力 ==========:");
                long temp = GetPassivePower();
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 被动技能 战力: {temp}  总战力: {power}");
            }


            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 出战伙伴总战力 ==========:");
                long temp = GetPlayedPartnersPower();
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 出战伙伴总战力: {temp}  总战力: {power}");
            }

            {
                //    SGF.Debuger.Log($"[Power] [总] ======== 助战伙伴总战力 ==========:");
                long temp = GetAssiantPartnersPower();
                power += temp;
                //    SGF.Debuger.Log($"[Power] [总] + 助战伙伴总战力: {temp}  总战力: {power}");

            }
            //    SGF.Debuger.Log($"[Power] [总] 总站力计算结束");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");
            //    SGF.Debuger.Log($"[Power] ");

            return 0;
        }

        /// <summary>
        /// 得到 玩家不同模块类型的 战力
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="isHistroy">是否事历史最高战力</param>
        /// <returns></returns>
        public long GetSeverPowerByModuleType(int modelType, bool isHistroy)
        {
            long power = 0;
            if (modelType == 0)
            {
                power = isHistroy ? FightPowerManager.Instance.GetServerPowerAllData().HistoryPowerMax : FightPowerManager.Instance.GetServerPowerAllData().AllPower;
            }
            else
            {
                FightPowerModuleEnum module = FightPowerModuleEnum.Equip;
                switch (modelType)
                {

                    case 1:
                        {
                            module = FightPowerModuleEnum.Equip;
                        }
                        break;
                    case 2:
                        {
                            module = FightPowerModuleEnum.Partner;
                        }
                        break;
                    case 3:
                        {
                            module = FightPowerModuleEnum.Role;
                        }
                        break;
                    case 4:
                        {
                            module = FightPowerModuleEnum.Skill;
                        }
                        break;

                    default: break;
                }
                PowerBigDataModule moudleData = FightPowerManager.Instance.GetServerPowerAllData().PowerModuleDatas[module];
                power = isHistroy ? moudleData.HistoryMaxPower : moudleData.CurPower;

            }


            return power;
        }

        public ServerPowerData ServerPowerAllData;

        public ServerPowerData GetServerPowerAllData()
        {
            if (ServerPowerAllData == null)
            {
                ServerPowerAllData = new();
            }

            if (GetFightMD != null)
            {
                ServerPowerAllData.Refresh(GetFightMD);
            }

            return ServerPowerAllData;
        }
        public ServerPowerData OtherServerPowerAllData;

        public ServerPowerData GetOtherServerPowerAllData(ProtoMsg.FightPowerModel fightPowerModule)
        {
            if (OtherServerPowerAllData == null)
            {
                OtherServerPowerAllData = new();
            }

            if (fightPowerModule != null)
            {
                OtherServerPowerAllData.RefreshOhterPower(fightPowerModule);
            }

            return OtherServerPowerAllData;
        }


        // public 
        /// <summary>
        /// 检查是否有可以领取的 level 奖励
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool HasSeverRankLevelReward()
        {
            var serverData = GetServerPowerAllData();

            var History_Cfg_Receive_PowerRankDataCell = serverData.History_Cfg_Receive_PowerRankDataCell;

            if (History_Cfg_Receive_PowerRankDataCell == null)
            {
                return false;
            }

            var levelId = History_Cfg_Receive_PowerRankDataCell.Id;

            // 如果能从奖励列表中拿到, 那就表明 奖励已经领取
            if (GetFightMD.RewardList.List.TryGetValue(levelId, out bool value))
            {
                return false;
            }

            // 如果都没有领取记录,那就返回 true,标识有奖励
            return true;

        }


    }




    #endregion


}
