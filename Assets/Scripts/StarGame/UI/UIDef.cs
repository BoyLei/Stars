
namespace StarProjectDef
{
    /// <summary>
    /// UI命名:驼峰
    /// 解释什么是各种分类，分线Window/Tips挂件正确，
    /// 其他const常量命名：ABC_DCE_DDD
    /// </summary>
    [XLua.LuaCallCSharp]
    public static class UIDef
    {
        #region 公用UI名

        public const string UIMsgBox = "Common/UIMsgBox";

        public const string UICommonMsgBox = "Common/Prefab/CommonBoxWidget_";
        #endregion


        #region PageUI
        /// Page 注定是全屏的
        /// Page 全局唯一
        public const string UILoginPage = "Login/Prefab/UILoginPage";
        public const string StarWorldPage = "StarWorld/StarWorldPage";

        public const string ScreenUIEffects_Dream = "ScreenUIEffects/Dream";
        public const string ScreenUIEffects_Gve = "ScreenUIEffects/Gve";
        public const string ScreenUIEffects_FX_XinSC_Global3 = "ScreenUIEffects/FX_XinSC_Global3";
        public const string ScreenUIEffects_FX_XinSC_Global4 = "ScreenUIEffects/FX_XinSC_Global4";


        //      public const string UIPVEGamePage = "PVE/UIPVEGamePage";
        //      public const string UIPVEPage = "PVE/UIPVEPage";

        //      public const string UIPVPGamePage = "PVP/UIPVPGamePage";
        //      public const string UIPVPRoomPage = "PVP/UIPVPRoomPage";
        //      =public const string UIRoomFindWnd = "PVP/UIRoomFindWnd";

        //      public const string UIHostWnd = "Host/UIHostWnd";
        #endregion



        #region Window模块

        ///Window 通常是具有稍复杂且独立的强交互性质：全局唯一是什么（Logic对应View，View的唯一），功能复杂且内聚！
        ///Window 可以一对多么
        ///Window [通常全局只能开放一个，不过目前并未限定]！
        public const string UIAreaListWindow = "Login/Prefab/AreaListWindow";
        public const string UICreateRoleWindow = "Login/Prefab/CreateRoleWindow";
        public const string UIDaysSignWindow = "DaysSign/SevenDaysSignWnd";
        public const string UIWaitToRebornWindow = "StarWorld/WaitToRebornWindow";  // 复活界面

        public const string WorldMapWindow = "WorldMap/Prefab/WorldMapWindow";  // 世界地图
        public const string MiniMapWindow = "WorldMap/Prefab/MiniMapWindow";    // 小地图舱口



        public const string PlayerAttrWindow = "PlayerAttrWindow/Prefabs/PlayerAttrWindow";

        public const string LevelUpgradeWidget = "StarWorld/LevelUpgradeWidget";


        public const string ChatChannelWindow = "Chat/Prefab/ChatChannelWindow"; // 聊天界面

        public const string SettingWindow = "SettingWindow/Prefab/SettingWindow"; // 聊天界面

        public const string TransJobWindow = "TransJob/Prefab/TransJobWindow"; // 转职界面
        public const string TransJobFinishWindow = "TransJob/Prefab/TransJobFinishWindow"; // 转职完成界面

        public const string RingTaskWindow = "RingTask/RingTaskWindow"; // 环任务界面
        public const string RingTaskRandomWidget = "RingTask/RingTaskRandomWidget"; // 环任务转盘界面

        public const string GamePlayInfoWindow = "Common/GamePlayInfoWindow"; // 玩法界面

        public const string UniversalSettlementWindow = "Common/Prefab/UniversalSettlementWindow"; // 通用结算

        public static string AchievementTipsWidget = "Common/Prefab/AchievementTipsWidget";


        public const string GamePlayChoiceWindow = "Ectype/Prefab/GamePlayChoiceWindow";   //玩法界面

        public const string MedicineWindow = "Medicine/Prefab/Medicine"; // 药品界面

        public const string SystemMsgBox = "SystemMsgBox";


        // public const string AnnouncementWindow=""


        #endregion


        #region Widget模块 
        ///通用性;弱交互性！
        ///什么是挂件？他是挂载再各个页面窗口中的复合组件，所以池化的Tips也是池化挂件。
        ///挂件可以开放多个，= + 其他

        public const string BenameWidget = "Login/Prefab/BenameWidget";
        public const string DeleteHeroWidget = "Login/Prefab/DeleteHeroWidget";

        public const string EnemyInfoWidget = "StarWorld/EnemyInfoWidget";
        public const string GVEBossRewardsWidget = "Event/GVEBossRewardsWidget";
        public const string AfternoonGveFinishWidget = "Event/AfternoonGveFinishWidget";
        public const string PlayerInfoWidget = "StarWorld/Prefab/PlayerInfoWidget1";
        public const string TaskWidget = "TaskWindow/Prefab/HudTaskWidget";
        public const string PlayerInfoWidget2 = "StarWorld/Prefab/PlayerInfoWidget2";

        public const string AvgShowTitleWidget = "Avg/AvgShowTitleWidget";

        public const string MiniMapWidget = "StarWorld/MiniMapWidget";
        public const string ArrowLockAtWidget = "StarWorld/ArrowLockAtWidget";
        public const string HudMenuWidget = "HudMenu/Prefab/HudMenuWidget";

        public const string FirstPayWidget = "Shop/FirstPayWidget";
        public const string MoonCardWidget = "Shop/MoonCardWidget";
        public const string BattlePassWidget = "Shop/BattlePassWidget";



        public const string TipsWidget = "Common/UITips";

        public const string PartnerWidget = "StarWorld/PartnerWidget";
        public const string PartnerAppearedWidget = "StarWorld/Prefab/PartnerAppearedWidget";

        public const string GmWidget = "Gm/Prefab/GmWidget"; // GM 界面
        public const string LocalServerTestWidget = "Gm/Prefab/LocalServerTestWidget"; // 本地服测试 界面

        public const string TestRedPointWidget = "Gm/Prefab/RedPointTest";
        public const string ChanageLineListWidget = "StarWorld/Prefab/ChanageLineListWidget";  // 换线界面：[通用]，[也可以是唯一性]，看策划需求目前正确=

        public const string InterActionObject = "InterAction/InterActionObject";
        public const string RedeemCodeWidget = "HudMenu/Prefab/RedeemCodeWidget";   // 兑换码界面


        public const string CommonCostWidget = "InterAction/CommonCostWidget";   // 通用消耗界面
        public const string ShowAreaNameWidget = "StarWorld/ShowAreaNameWidget";   // 切地图地图名显示界面
        public const string RequestLoadingWidget = "Common/Prefab/RequestLoadingWidget";   // 等待菊花
        public const string RequestLoadingWidget2 = "Common/Prefab/RequestLoadingWidget2";   // 等待菊花


        public const string TutorialWidget = "Tutorial/Prefab/TutorialWidget";   // 引导
        public const string QTEWidget = "Tutorial/Prefab/QTEWidget";   // 引导
        

        public const string AnnouncementWidget = "Announcement/AnnouncementWindow"; 
        public const string PlayVideoWidget = "Common/Prefab/PlayVideoWidget"; // 播放视频
        public const string PlayTimelineBlackWidget = "Common/Prefab/PlayTimelineBlackWidget"; // 播放视频


        public const string BuyCashWidget = "Shop/BuyCashWidget"; // 打开充值

        public const string PartnerDialogWidget = "Tutorial/Prefab/PartnerDialogWidget";
        public const string FakePartnerWidget = "Tutorial/Prefab/FakePartnerWidget";

        public const string PartnerJoinWidget = "StarWorld/Prefab/PartnerJoinWidget";



        public const string WorldLvUpWidget = "StarWorld/Prefab/WorldLvUpWidget";







        #endregion

        #region 其他

        public const string MobileController = "UI/StarWorld/Prefab/MobileController";

        public const string TimeLineToHideHud = "TimeLineToHideHud";
        public const string VirtualCameraToHideHud = "VirtualCameraToHideHud";
        
        public const string LoadingViewToHideHud = "LoadingViewToHideHud";

        public const string GMPanelToHideHud = "GMPanelToHideHud";


        public const string DisplayModelPath = "Roles/Template/Display_Model";      // 模型展示路径

        #endregion

        #region UI特效

        public const string FX_UI_UILoginPage = "Effects/UI/LoadingView/FX_UI_UILoginPage";

        public const string Mvp = "UI/StarWorld/Prefab/Mvp";



        #endregion
    }
}
