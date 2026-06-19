using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProjectDef;
namespace StarProject.Service.Battle
{

    /// <summary>
    /// 战斗相关的所有 模块数据
    /// </summary>
    [XLua.LuaCallCSharp]
    public class PowerAllData
    {
        /// <summary>
        /// 总战力
        /// </summary>
        public long AllPower = 0;

        /// <summary>
        /// 总战力 对应的 [大段位/小段位] 配置 
        /// </summary>
        public TotalPowerRankDataCell Cfg_PowerRankDataCell;

        /// <summary>
        /// 下一等级奖励的 配置
        /// </summary>
        public TotalPowerRankDataCell Cfg_NextLevelPowerRankDataCell;

        /// <summary>
        /// 总战力 大 段位对应的  配置
        /// </summary>
        public RankResDataCell Cfg_RankResDataCell;
        public RankResDataCell Cfg_NextLevelRankResDataCell;


        /// <summary>
        /// 推荐模块 数据
        /// </summary>
        public List<PowerSubDataModule> RecommendSubModules = new();

        /// <summary>
        /// 大的模块 数据
        /// </summary>
        public Dictionary<FightPowerModuleEnum, PowerBigDataModule> BigModules = new();


        private List<PowerSubDataModule> showRecommendSubModules = new();
        /// <summary>
        /// 得到推荐的前三个 模块
        /// </summary>
        public List<PowerSubDataModule> GetShowRecommendSubModules()
        {
            showRecommendSubModules.Clear();
            for (int i = 0; i < RecommendSubModules.Count && i < 3; i++)
            {
                showRecommendSubModules.Add(RecommendSubModules[i]);
            }

            return showRecommendSubModules;
        }

        private List<PowerBigDataModule> showBigModuleDatas = new();

        private FightPowerModuleEnum[] showModules = new FightPowerModuleEnum[] { FightPowerModuleEnum.Partner, FightPowerModuleEnum.Role, FightPowerModuleEnum.Equip, FightPowerModuleEnum.Skill };

        public List<PowerBigDataModule> GetShowBigModules()
        {

            showBigModuleDatas.Clear();

            // foreach (var item in showModules)
            // {
            //     if (BigModules.TryGetValue(item, out var bigModule))
            //     {
            //         showBigModuleDatas.Add(bigModule);
            //     }
            //     else
            //     {
            //         showBigModuleDatas.Add(null);
            //     }
            // }

            foreach (var item in BigModules)
            {
                showBigModuleDatas.Add(item.Value);
            }

            return showBigModuleDatas;
        }

        /// <summary>
        /// 刷新 战力数据
        /// </summary>
        public void Refresh()
        {

            AllPower = 0;
            foreach (var item in BigModules)
            {
                item.Value.Refresh();
                AllPower += item.Value.CurPower;
            }

            RefreshLevel();
        }

        public void RefreshLevel()
        {

            int openDay = GameManager.Instance.OpenShowServerDay();
            LocalDataManager.Instance.GetPowerRankLevelCfg(AllPower, openDay, out Cfg_PowerRankDataCell, out Cfg_RankResDataCell);

            Cfg_NextLevelPowerRankDataCell = LocalDataManager.Instance.GetNextTotalPowerRankDataCellByID(Cfg_PowerRankDataCell.Id);
            if (Cfg_NextLevelPowerRankDataCell != null)
            {
                Cfg_NextLevelRankResDataCell = LocalDataManager.Instance.GetRankResDataCell(Cfg_NextLevelPowerRankDataCell.ParentRank);
            }

        }

    }

}