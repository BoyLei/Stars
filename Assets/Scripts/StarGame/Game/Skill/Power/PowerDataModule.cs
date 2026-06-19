using System.Collections.Generic;
using System.Text;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProjectDef;
namespace StarProject.Service.Battle
{

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 子模块战力数据
    /// </summary>
    public abstract class PowerDataModule
    {
        /// <summary>
        /// 模块的类型
        /// </summary>
        public FightPowerModuleEnum E_FightPowerModule;
        /// <summary>
        /// 模块类型对应的 枚举ID
        /// </summary>
        public int PowerModuleID;
        public PowerPartDataCell PowerModuleDataCellCfg;



        /// <summary>
        /// 模块 成长百分比
        /// </summary>
        public float Progress;

        /// <summary>
        /// 当前进度模块 对应的 评价配置
        /// </summary>
        public PartEvalueDataCell CurProgressModuleEvalueCfg;

        /// <summary>
        /// 当前战力
        /// </summary>
        public long CurPower = 0;
        /// <summary>
        /// 当前模块 战力上限
        /// </summary>
        public long ModuleLimitPower = 0;
        /// <summary>
        /// 总极限战力
        /// </summary>
        public long TotalLimitPower = 0;



        public virtual void Create(FightPowerModuleEnum powerModule, long curPower, int openDay)
        {
            E_FightPowerModule = powerModule;
            PowerModuleID = (int)powerModule;

            CurPower = curPower;
            PowerModuleDataCellCfg = LocalDataManager.Instance.GetPowerModuleDataCell(PowerModuleID);

            TotalLimitPower = LocalDataManager.Instance.GetAllModulesTotalUpperLimitPower(openDay);
        }


        public abstract void Log();

        public static StringBuilder sb = new();
        public static T Create<T>(FightPowerModuleEnum powerModule, long curPower) where T : PowerDataModule, new()
        {
            T t = new();
            int openDay = GameManager.Instance.OpenShowServerDay();
            t.Create(powerModule, curPower, openDay);
            return t;
        }
    }

}