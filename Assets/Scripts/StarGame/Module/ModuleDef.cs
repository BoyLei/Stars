using System;
using System.Collections.Generic;

namespace StarProjectDef
{
    [XLua.LuaCallCSharp]
    public static class ModuleDef
    {
        [XLua.LuaCallCSharp]
        public enum ModuleDefType
        {
            None,
            CsModuleDef,
            LuaModuleDef,
        }
        [XLua.LuaCallCSharp]
        /// <summary>
        /// 枚举查找对应的值的性能较好，因为枚举是通过整数值与其对应的命名常量进行关联。因此，只需将枚举值转换为整数后，可以直接通过索引来获取对应的值。这是一种高效的查找方式，不涉及函数调用和运行时的额外开销。

        //反射查找则需要在运行时动态获取类型信息，并使用反射API来查找类型的成员信息。这包括查找枚举类型、获取枚举成员、获取成员的值等。由于反射涉及到运行时的类型分析和函数调用，因此通常比直接的枚举查找方式耗费更多的时间和资源。
        /// </summary>
        public enum Name
        {
            None,
            //ModuleA,
            //ModuleB,
            LoginModule,                              //登录模块
            //SevenDaysSignModule,                      //七日签到模块
            StarWorldModule,                          //场景模块
            TriggerModule,                            //触发器模块
            InterActionModule,                        //交互模块
            //DragonBallModule,                         //龙珠模块
            //LuaModuleTest,                            //七日签到模块
            RayCheckModule,                           //射线检测模块
            RingTaskModule,                           //环任务
                                                      //public const string HostModule =                 "HostModule";                                        //
            ItemControllerModule,                     //道具模块
            WorldMapModule,                           //世界地图模块
            PlayerLocalCache,                         //玩家缓存数据
            EctypeModule,                             //副本模块
            TutorialModule,                            //引导模块
            ScenePlayModule,                           //场景玩法模块
            LuaModuleType = 5000,           //-LuaModule分割器--------------LuaCs理论上不可能一样----- 

            AvgLuaModule,                             //AVG模块
            TaskModule,                               //任务模块
            EquipModule,                              //装备模块
            ShopModule,                               //商店模块
            RunHorseModule,                           //跑马灯模块
            ItemUseModule,                            //道具使用弹框
            ItemBuyModule,                            //道具购买弹框
            GuildModule,                              //工会模块
            BagModule,                                //背包模块
            ItemTipsModule,                           //道具Tips模块
            FuncTipsModule,                           //功能Tips模块
            DropInfoListModule,                       //掉落UI模块
            QuickEquipModule,                         //快捷穿戴模块
            ObjectInteractiveModule,                  //交互列表模块
            DropControllerModule,                     //掉落模块
            RewardsPopModule,                         //奖励弹窗UI模块
            ItemResolveModule,                        //道具分解模块
            BattleTeamModule,                         //伙伴出战模块
            EquipSlotControllerModule,                //装备槽位强化模块
            EquipUpgradeModule,                       //装备强化模块

            SkillWindowModule,                        //技能整体的模块
            AmuletModule,                             //符文模块
            InscriptionModule,                        //鸣器模块
            ChatModule,                               //聊天模块
            MedicineModule,                           //药品模块
            GamePlayChoiceModule,                     //副本玩法选择
            EctypeEntranceModule,                     //副本入口
            EctypeBagModule,                          //副本背包
            EctypePopModule,                          //副本弹窗
            LootControllerModule,                     //掉落模块
            EctypeSettleModule,                       //副本出战
            SecretAreaModule,                         //个人秘境
            MailModule,                               //邮件系统
            RechargeModule,                           // 交易 module
            DailyTeamModule,                          //组队日常本
            TeamModule,                               //组队模块
            FriendModule,                             // 好友模块
            TreasureModule,                           //藏宝图玩法
            AdventureLevelModule,                     //冒险等级模块
            DrawCardModule,                           //抽卡系统
            EventModule,                              //活动系统
            EventBossRankModule,                      //活动boss排行
            WantedModule,                             // 通缉玩法模块

            AfternoonGveModule,                       //午间GVE
            TalentModule,                             //天赋模块
            SelectorModule,                           //选择器模块
            BankModule,                               //银行模块
            ExchangeModule,                           //交易行
            SystemOpenTipsModule,                     //系统功能开放模块
            PvpModule,                                //pvp模块
            ArenaModule,                              //异步竞技场
            AnnouncementModule,                       // 公告module
            CommercializationModule,                  // 商业化module
            WildBossModule,                           // 野外boss
            DonateModule,                             // 公会捐献module
            LivingSkillsModule,                       //生活技能

            ActivityModule,                           //签到
            PersonalTowerModule,                      //个人爬塔

            DailyActModule,                           //每日活动
            WorldLineModule,                          //世界线模块
            GamePlayCalendarModule,                   //玩法日历
            FightPowerModule,                         // 战力模块

            OnlineRewardModule,                       //在线奖励

            PartyTimeModule,                          //公会篝火
            RankModule,                               //排行榜模块
            PlayerPreviewModule,                      //玩家信息预览
            SkillUnlockTipsModule,                    //技能解锁弹窗表现
            BeginnerTargetModule,                     //新手目标
            MindRepairModule,                         //心灵修复小游戏
            DownLoadModule,                     //分包下载模块
        }


        private static Dictionary<Name,string> m_ModuleNameToEnumDict = new Dictionary<Name, string>();
        private static Dictionary<string,Name> m_EnumToModuleNameDict = new Dictionary<string, Name>();

        public static void Init()
        {
            m_ModuleNameToEnumDict.Clear();
            m_EnumToModuleNameDict.Clear();
            
            foreach (Name moduleEnum in Enum.GetValues(typeof(Name)))
            {
                if (moduleEnum!= Name.None && moduleEnum!= Name.LuaModuleType)
                {
                    string moduleName = moduleEnum.ToString();
                    m_ModuleNameToEnumDict.Add(moduleEnum,moduleName);
                    m_EnumToModuleNameDict.Add(moduleName, moduleEnum);
                }
            }
            
        }
        
        public static Name GetModuleName(string ModuleName)
        {
            if (m_EnumToModuleNameDict.ContainsKey(ModuleName))
            {
                return m_EnumToModuleNameDict[ModuleName];
            }

            return Name.None;
            /*Name E_ModuleName = Name.None;
            //isDef是确定 枚举类型 和具体枚举实例的
            if (Enum.TryParse<Name>(ModuleName, out E_ModuleName))
            {

            }
            else
            {
                E_ModuleName = Name.None;
            }
            return E_ModuleName;*/
        }



        public static string GetModuleName(Name E_ModuleName)
        {
            if (m_ModuleNameToEnumDict.ContainsKey(E_ModuleName))
            {
                return m_ModuleNameToEnumDict[E_ModuleName];
            }
            return string.Empty;
            /*string ModuleName = "";
            //isDef是确定 枚举类型 和具体枚举实例的
            if (Enum.IsDefined(typeof(Name), E_ModuleName))
            {
                ModuleName = E_ModuleName.ToString();
            }
            return ModuleName;*/
        }

        /// <summary>
        /// 节省了lua和Cs的查找，现在在Enum里面查找了
        /// </summary>
        /// <param name="ModuleName"></param>
        /// <returns></returns>
        public static ModuleDefType GetModuleDefState(Name E_ModuleName)
        {
            ModuleDefType moduleDefType = ModuleDefType.None;
            /// Xlua是int ，是Xlua强转过来的，所以比如写排错用来排除Xlua自己瞎写一个Int
            /// 如果出错就一定会在强转调用时候出错   （Name)9000 一定出错 因为没定义 所以不必拦截
            if (E_ModuleName == Name.None || E_ModuleName == Name.LuaModuleType /*|| !Enum.IsDefined(typeof(Name), E_ModuleName)*/)
            {
                moduleDefType = ModuleDefType.None;
                return moduleDefType;
            }


            if ((int)E_ModuleName > (int)Name.LuaModuleType)
            {
                moduleDefType = ModuleDefType.LuaModuleDef;
            }
            else
            {
                moduleDefType = ModuleDefType.CsModuleDef;
            }

            return moduleDefType;
        }


    }
}
