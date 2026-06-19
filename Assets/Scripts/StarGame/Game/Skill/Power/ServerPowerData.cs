using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProjectDef;
using ProtoMsg;
namespace StarProject.Service.Battle
{
    /// <summary>
    /// 服务器战斗相关的所有 模块数据
    /// </summary>
    [XLua.LuaCallCSharp]
    public class ServerPowerData
    {
        /// <summary>
        /// 总战力
        /// </summary>
        public long AllPower = 0;

        /// <summary>
        /// 历史最高战力,领取奖励采用的是历史最高战力
        /// </summary>
        public long HistoryPowerMax = 0;

        /// <summary>
        /// 总战力 对应的 [大段位/小段位] 配置 
        /// </summary>
        public TotalPowerRankDataCell Cfg_PowerRankDataCell;

        /// <summary>
        /// 下一等级奖励的 配置
        /// </summary>
        public TotalPowerRankDataCell Cfg_NextLevelPowerRankDataCell;


        /// <summary>
        /// 历史最高战力 对应的 等级配置
        /// </summary>
        public TotalPowerRankDataCell History_Cfg_PowerRankDataCell;

        /// <summary>
        /// 历史最高战力 下一等级奖励的 配置
        /// </summary>
        public TotalPowerRankDataCell History_Cfg_NextLevelPowerRankDataCell;

        /// <summary>
        /// 历史最高战力 可以领取的 等级配置
        /// note:
        ///     最高战力达到 2 级,但是没 达到奖励领取需求. 此时 就只能领取 1级的奖励
        /// </summary>
        public TotalPowerRankDataCell History_Cfg_Receive_PowerRankDataCell;


        /// <summary>
        /// 总战力 大 段位对应的  配置
        /// </summary>
        public RankResDataCell Cfg_RankResDataCell;
        /// <summary>
        /// 总战力 对应的 下一级 大 段位配置
        /// </summary>
        public RankResDataCell Cfg_NextLevelRankResDataCell;

        /// <summary>
        /// 历史最高战力对应的 大段位配置
        /// </summary>
        public RankResDataCell History_Cfg_RankResDataCell;
        /// <summary>
        /// 历史最高战力对应的 下一个大段位配置
        /// </summary>
        public RankResDataCell History_Cfg_NextLevelRankResDataCell;


        public Dictionary<FightPowerModuleEnum, PowerBigDataModule> PowerModuleDatas = new();


        public void Refresh(FightMD fightMD)
        {


            HistoryPowerMax = fightMD.HistoryMax;

            RefresBigModuleData(FightPowerModuleEnum.Role, fightMD.Role, fightMD.HistoryRole);
            RefresBigModuleData(FightPowerModuleEnum.Skill, fightMD.Skill, fightMD.HistorySkill);
            RefresBigModuleData(FightPowerModuleEnum.Equip, fightMD.Equip, fightMD.HistoryEquip);
            RefresBigModuleData(FightPowerModuleEnum.Partner, fightMD.Partner, fightMD.HistoryPartner);

            AllPower = fightMD.All;
            // 刷新战力相关的等级
            RefreshLevel();
        }

        /// <summary>
        /// 刷新其他人的战力, 孔大爷 居然区分了 自己和其他人的 战力数据结构
        /// </summary>
        /// <param name="fightPowerModule"></param>
        public void RefreshOhterPower(ProtoMsg.FightPowerModel fightPowerModule)
        {
            HistoryPowerMax = 0;

            RefresBigModuleData(FightPowerModuleEnum.Role, fightPowerModule.Role, 0);
            RefresBigModuleData(FightPowerModuleEnum.Skill, fightPowerModule.Skill, 0);
            RefresBigModuleData(FightPowerModuleEnum.Equip, fightPowerModule.Equip, 0);
            RefresBigModuleData(FightPowerModuleEnum.Partner, fightPowerModule.Partner, 0);

            AllPower = fightPowerModule.All;
            // 刷新战力相关的等级
            RefreshLevel();
        }

        /// <summary>
        /// 刷新每一个 服务器大模块的数据
        /// </summary>
        /// <param name="fightPowerModule"></param>
        /// <param name="power"></param>
        private void RefresBigModuleData(FightPowerModuleEnum fightPowerModule, long power, long historyMax)
        {
            if (!PowerModuleDatas.TryGetValue(fightPowerModule, out var bigMoudleData))
            {
                bigMoudleData = PowerDataModule.Create<PowerBigDataModule>(fightPowerModule, power);
                PowerModuleDatas.Add(fightPowerModule, bigMoudleData);
            }

            bigMoudleData.RefreshPower(power, historyMax);
        }

        /// <summary>
        /// 刷新总体战力对应的 等级数据
        /// </summary>
        private void RefreshLevel()
        {

            int openDay = GameManager.Instance.OpenShowServerDay();

            // 计算当前战力对应的 段位等级以及 下一段位等级对应的配置
            {

                LocalDataManager.Instance.GetPowerRankLevelCfg(AllPower, openDay, out Cfg_PowerRankDataCell, out Cfg_RankResDataCell);

                Cfg_NextLevelPowerRankDataCell = LocalDataManager.Instance.GetNextTotalPowerRankDataCellByID(Cfg_PowerRankDataCell.Id);
                if (Cfg_NextLevelPowerRankDataCell != null)
                {
                    Cfg_NextLevelRankResDataCell = LocalDataManager.Instance.GetRankResDataCell(Cfg_NextLevelPowerRankDataCell.ParentRank);
                }
            }

            // 计算历史最高战力对应的 段位等级以及 下一段位对应的配置, 对于奖励预览来说, 段位预览奖励需要采用历史最高战力
            {
                LocalDataManager.Instance.GetPowerRankLevelCfg(HistoryPowerMax, openDay, out History_Cfg_PowerRankDataCell, out History_Cfg_RankResDataCell);

                History_Cfg_NextLevelPowerRankDataCell = LocalDataManager.Instance.GetNextTotalPowerRankDataCellByID(History_Cfg_PowerRankDataCell.Id);
                if (History_Cfg_NextLevelPowerRankDataCell != null)
                {
                    History_Cfg_NextLevelRankResDataCell = LocalDataManager.Instance.GetRankResDataCell(History_Cfg_NextLevelPowerRankDataCell.ParentRank);
                }

                History_Cfg_Receive_PowerRankDataCell = LocalDataManager.Instance.GetPowerRewardRankLevelCfg(HistoryPowerMax, openDay);
            }
        }

        /// <summary>
        /// 是否 匹配要求的战力
        /// </summary>
        /// <param name="modelType">战力模块(0总, 1装备，2伙伴，3角色，4技能)</param>
        /// <param name="requirePower">要求的战力</param>
        /// <param name="isHistroy">是否采用历史最高战力</param>
        /// <returns></returns>
        public bool IsMatchPower(int modelType, long requirePower, bool isHistroy)
        {
            long power = 0;
            if (modelType == 0)
            {
                power = isHistroy ? HistoryPowerMax : AllPower;
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
                PowerBigDataModule moudleData = PowerModuleDatas[module];
                power = isHistroy ? moudleData.HistoryMaxPower : moudleData.CurPower;

            }


            return power >= requirePower;
        }

    }

}