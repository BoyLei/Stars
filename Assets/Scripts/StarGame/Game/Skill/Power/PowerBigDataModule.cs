using System;
using System.Collections.Generic;
using StarProject.Service.LocalData;
using StarProjectDef;
namespace StarProject.Service.Battle
{

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 大模块战力数据
    /// </summary>
    public class PowerBigDataModule : PowerDataModule
    {

        /// <summary>
        /// 历史最大战力, 这个只有服务器才会存储
        /// </summary>
        public long HistoryMaxPower;

        /// <summary>
        /// 大模块 所包含的 子模块数据
        /// </summary> 
        public List<PowerSubDataModule> SubPowerDatas = new();

        public override void Create(FightPowerModuleEnum powerModule, long curPower, int openDay)
        {
            base.Create(powerModule, curPower, openDay);

            ModuleLimitPower = LocalDataManager.Instance.GetBigModuleUpperLimetPower(PowerModuleID, openDay);

        }

        /// <summary>
        /// 对于大模块而言, 战力数据 是所有 子模块战力数据的 总和
        /// </summary>
        public void Refresh()
        {
            long power = 0;
            foreach (var item in SubPowerDatas)
            {
                power += item.CurPower;
            }
            RefreshPower(power);
        }

        public void RefreshPower(long power, long historyMax = 0)
        {
            CurPower = power;
            HistoryMaxPower = historyMax;
            // 当前 大模块的进度
            if (ModuleLimitPower == 0)
            {
                SGF.Debuger.LogWarning($"[Power] {E_FightPowerModule} ModuleLimitPower: {ModuleLimitPower} 模块上限为0!!!");
                return;
            }
            Progress = CurPower * 1.0f / ModuleLimitPower;

            if (Progress > 1)
            {
#if UNITY_EDITOR
                //SGF.Debuger.LogWarning($"[Power] {E_FightPowerModule} ModuleLimitPower: {ModuleLimitPower} CurPower: {CurPower}, 模块超上限为!!!");
#endif
                Progress = 1;

            }

            // 刷新 大模块战力 对应的评价
            CurProgressModuleEvalueCfg = LocalDataManager.Instance.GetModuleIDEvalue(PowerModuleID, (int)Math.Ceiling(Progress * 1000));

        }

        /// <summary>
        /// 增加战力子模块 数据
        /// </summary>
        /// <param name="subDataModule"></param>
        public void AddSubModule(PowerSubDataModule subDataModule)
        {
            SubPowerDatas.Add(subDataModule);
        }

        public PowerSubDataModule FindPowerSubDataModule(FightPowerModuleEnum powerModule)
        {
            return SubPowerDatas.Find((item) =>
            {
                return item.E_FightPowerModule == E_FightPowerModule;
            });
        }

        public PowerSubDataModule UpdateSubModule(FightPowerModuleEnum powerModule, long power)
        {
            PowerSubDataModule subModule = FindPowerSubDataModule(powerModule);
            if (subModule == null)
            {
                subModule = Create<PowerSubDataModule>(powerModule, power);
                SubPowerDatas.Add(subModule);

            }

            subModule.UpdatePower(power);

            // 刷新整个大模块的 战力数据
            Refresh();

            return subModule;
        }

        private List<PowerSubDataModule> showSubModules = new();
        public List<PowerSubDataModule> GetShowSubModules()
        {
            showSubModules.Clear();
            foreach (var item in SubPowerDatas)
            {
                if (item.IsOpen)
                {
                    showSubModules.Add(item);
                }
            }

            return showSubModules;
        }

        public override void Log()
        {

            SGF.Debuger.Log($"[Power] {E_FightPowerModule} 战力: {CurPower}, 进度: {Progress}, 极限战力: {ModuleLimitPower}, 总极限战力: {TotalLimitPower} ");

            SGF.Debuger.Log("[Power] =============================================== ");


            foreach (var item in SubPowerDatas)
            {
                item.Log();
            }
        }

    }

}