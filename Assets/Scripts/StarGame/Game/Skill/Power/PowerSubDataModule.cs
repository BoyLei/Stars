using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProjectDef;
namespace StarProject.Service.Battle
{

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 子模块战力数据
    /// </summary>
    public class PowerSubDataModule : PowerDataModule
    {

        /// <summary>
        /// 战力模块的 推荐值
        /// </summary>
        public float RecommendValue;





        public FightPowerModuleEnum ParentModule;
        public int ParentModuleID;

        /// <summary>
        /// 系统模块是否开启
        /// </summary>
        public bool IsOpen = true;

        public override void Create(FightPowerModuleEnum powerModule, long curPower, int openDay)
        {
            base.Create(powerModule, curPower, openDay);


            ModuleLimitPower = LocalDataManager.Instance.GetModuleUpperLimitPower(PowerModuleID, openDay);

            // 推荐值 = (当前战力/模块上限战力 ) / (模块上限战力 / 所有模块上限战力 )
            ParentModule = (FightPowerModuleEnum)PowerModuleDataCellCfg.FromPart;
            ParentModuleID = PowerModuleDataCellCfg.FromPart;
            if (PowerModuleDataCellCfg.SystemJump > 0)
            {
                IsOpen = SystemOpenManager.Instance.CheckCanSystemJump(PowerModuleDataCellCfg.SystemJump);
            }
            else
            {
                // 如果都没配置跳转, 那就表示开启
                // IsOpen = true;
                IsOpen = false;
            }
#if UNITY_EDITOR
            // 测试的逻辑，开放所有子模块
            // IsOpen = true;
#endif
            UpdatePower(curPower);
        }

        public void UpdatePower(long power)
        {
            if (power != CurPower)
            {
                return;
            }

            CurPower = power;
            if (ModuleLimitPower == 0 || TotalLimitPower == 0)
            {
                SGF.Debuger.LogWarning($"[Power] {E_FightPowerModule} ModuleLimitPower: {ModuleLimitPower}, TotalLimitPower: {TotalLimitPower} 上限为0!!!");

                return;
            }

            // 子模块成长的百分比
            Progress = CurPower * 1.0f / ModuleLimitPower;

            if (Progress > 1)
            {
#if UNITY_EDITOR
                SGF.Debuger.LogWarning($"[Power] {E_FightPowerModule} ModuleLimitPower: {ModuleLimitPower} CurPower: {CurPower}, 模块超上限为!!!");

#endif
                Progress = 1;

            }


            RecommendValue = Progress / (1.0f * ModuleLimitPower / TotalLimitPower);





            CurProgressModuleEvalueCfg = LocalDataManager.Instance.GetModuleIDEvalue(PowerModuleID, (int)Math.Ceiling(Progress * 10000));
        }

        public override void Log()
        {
            SGF.Debuger.Log($"[Power] {E_FightPowerModule} 战力: {CurPower}, 推荐值: {RecommendValue}, 极限战力: {ModuleLimitPower}, 总极限战力: {TotalLimitPower}, IsOpen: {IsOpen} ");
        }



    }

}