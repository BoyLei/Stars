using StarProject;
using System.Collections.Generic;
/// <summary>
/// 玩法相关的配置
/// </summary>
namespace StarProjectDef
{
    public static class GameConfig
    {
        //60，30
        //Every V Blank：帧率为60，Application.targetFrameRate无效
        public const byte UpdatePerFix = 2;
        public const byte FIX_TIME_PER_SEC = 30;
        public const float FIX_LOGIC_FRAME = 0.03333f;
        public const float FIX_RENDER_FRAME_SEC = 0.016666f;
        public const float C2S_MINI_MOVE_SYNC_DISTANCE = 0.1f;//客户端对服务器最小间隔位置同步(米)数
        public const float C2S_MINI_SYNC_ANGLE = 5f;    //客户端对服务器最小角度 

        /// <summary>
        /// 路点移动 距离的最小误差范围 0.1f
        /// </summary>
        public const float S2C_MINI_PATH_MOVE_SYNC_DISTANCE = 0.1f;//服务器对客户端最小设置米数，小于这个就忽略了

        public const float S2C_MINI_MOVE_SYNC_TIME = 1f;//服务器对客户端最小设置米数，小于这个就忽略了

        #region 移动动作比例配置：基于【美术动画规范】和【客户端驱动速度】 不包含【策划配置基础速度表 或者 动态速度运行时影响】

        //策划设定基于状态，并不基于速度（比如技能移动，高速移动，技能移动）
        public const float CLIENT_MONSTER_ANIM_BASE_WALK_SPEED = 2.0f;//美术对所有类型NPC（怪）标准统一按照200移动速度时的===1.0倍速动画播放标准
        public const float CLIENT_MONSTER_ANIM_BASE_RUN_SPEED = 4.0f;//美术对所有类型NPC（怪）标准统一按照200移动速度时的===1.0倍速动画播放标准

        //【我活着其他人【我和第三方玩家共享】:没有过度加速】两个状态都是跑
        public const float CLIENT_HERO_ANIM_BASE_WALK_SPEED = 2.5f;//美术对所有类型NPC（人）标准统一按照200移动速度时的===1.0倍速动画播放标准
        public const float CLIENT_HERO_ANIM_BASE_RUN_SPEED = 5.0f;//美术对所有类型NPC（人）标准统一按照200移动速度时的===1.0倍速动画播放标准

        #endregion

        #region 控制设置
        //public const float LONG_THOUCH_TIME = 1f;
        #endregion

        #region  玩家相关
        //public const float PLAYER_MOVE_SPEED_TOUCH = 5f;
        //public const float BLINK_LIMIT_TIME = 2f;//闪烁移动间隙
        //public const float PLAYER_MOVE_SPEED = 0.075f;//3.3f;//4.6f;
        //public const float PLAYER_RUN_SPEED = 0.15f;//6.6f;//跑动速度
        //public const float DOWN_SPEED = -9.8f;
        public const float PLAYER_SKIN_WIDTH = 0.01f;//预留的，给射线留有空间，意义是胶囊的顶点外拓延伸长度
        public const float PLAYER_GRI_CHECKER_ORGIN_HEIGHT = 0.01f;//预留的，给射线留有空间，意义是胶囊的顶点外拓延伸长度
        public const float TERRAIN_HOLE_MAX = 0.03f;//地表和坑洼和斜坡和地表旁边一个顶点相差的最大高度（因为地表不是绝平的）
        public const float TERRAIN_PERFORMANCE_TOLERANCE = 0.02f; //,另外也是一种性能容忍度如果卡这个值也要加大
        public const float PLAYER_GRI_CHECKER_LEN = PLAYER_GRI_CHECKER_ORGIN_HEIGHT + TERRAIN_HOLE_MAX + TERRAIN_PERFORMANCE_TOLERANCE;//射线检测长度

        public const float PLAYER_MOVE_SPEED_PER_SEC = 6;//1秒6米 -----600(server) / 100 是他们为了控制精度------6  / 30 是每秒30帧 = 0.2f
        public const float PLAYER_MOVE_SPEED_PER_FIXFRAME = 0.2f;//PLAYER_MOVE_SPEED_PER_SEC / FIX_TIME_PER_SEC

        public const float PLAYER_ROTATE_SPEED = 1080f;//1秒1080度：因为我们转向不会暂停，也就是没有光法那种（虽然dota也是移动角度分离），也就是转向就会伴随移动，会给人飘的感觉，所以要加速；1秒1080 30fps=1080； 10fps=  360 ；1帧可以36度最少也会36度

        //举个粒子，胶囊由球体演变而来，高度也是2r，宽度也是2r。高度只能是2r + n ， 所以算半径如果用高度算回来的话最少除以2，所以取值范围是2~n
        //但是宽度本质是自由的，navmesh是0.15，人的宽度应该也是0.15；暂时别表现碰撞的真实性了
        public const float MODEL_RADIUS_FACTOR = 3f; // 模型碰撞盒胶囊体【半径】计算的系数，规定是【2~N】，目前取用3

        /// <summary>
        /// 有三种关系：服务器全面积，配置是0.2|【【服务器0.2面积已经是客户端给的了】，【配置是0】 +++ 【客户端校验速度位置是否合理】 +++【 0.2不合法需要是半径 + 0.01容错的碰撞 + 碰撞时效性0.3秒*一移动速度 = 0.35最小】（TODO算切线+速度分量要算出来 或者 算最近的点？）（所有场景0.2生成一次策划确保）（服务器确保配置是0）】| 要看是否能到边界的navmove|【最小的缝隙不能小于0.7m寻路】
        /// </summary>
        public const float MODEL_RADIUS = 0.5f;//0.40f;//0.35f;//0.2不合法需要是半径 + 0.01容错的碰撞 + 碰撞时效性0.33秒*移动速度 = 0.35最小
        public const float NAVMESH_EXTEND = 0.01f;
        public const float NAVMESH_STEPHEIGHT = 2.4f;//如果0.4下不去的话就调大就行了
        public const float NAVMESH_RADIUS = 0.2f;
        //策略1：立刻中断模式，只依据网络最新协议，抛弃过程，即刻更新，当前位置不完成
        //策略2：还原快照模式，依据缓存客户端加速执行过程，执行过程，如被中断立刻结束
        //计算取得最大推出时间，移动和转角
        //转角变成第二优先级【意识是如被中断立刻准确完成同步】
        //用于DoTween,true就是第二策略被更新直接完成
        //拥堵很多快速对齐
        public const bool RotSyncMode = true;

        //怪物移动路店是提前预测的
        //实际情况很可能由于，其他玩家导致怪物停止/怪物朝向最新目标移动
        //人要走完----------怪物不用走完-----------(不用新消息立刻完成：可能中断)：路点保持目标
        //怪物是暂停后向原来目标继续了。人也是原来目标；双方都会按照移动过去只不过是加速所以都是false
        //拥堵很多加速逐一完成
        public const bool MoveSyncMode = false;


        //加速是个什么逻辑？【1，首先就是正常异步时序处理；2，如果拥堵就加速优先处理这种异步的（不重要，还要执行，不是直接到，快点到）；】
        //3，网络通常固定帧数（属性赋值）
        //4，网络也可能拥堵；5，通过服务器逻辑帧处理消息，6，理论上帧帧都会发生变化也要帧加速但，每一个人，每一个属性，并不在每一帧都会变化，服务器没有属性变化这个属性不发，全属性没变化本消息不发。
        //所以按照帧去驱动理论上是很快可以追上的
        /*路店要加速码?【要】:首先处理消息缓存不阻塞不论移动还是其他消息：消息缓存就意味着网络拥堵；既然拥堵加速
            是为了下一个移动消息到了（缓存中）处理能快一点
            但是问题是加速移动和其他aoi肯定对不上111--本质是过程化的快速表达（弱意义，过程化）；属性的快速堵盖（弱意义，直接覆盖）这个过程还原目前可以忽略，拥堵导致的移动过程中哪一点开始掉血本质无所谓；但是原子锁会生效---会屏蔽掉后续处理这个大流程问题不纯在
            强同步效力变低了222--只有主角没问题
            技能会有原子会告诉在属性同步中后续消息会抛弃掉了---所以技能没问题
            如果强同步--墙同步也要缓存，让压缩饼干（消息缓存机制和强制机制同样被同时刻压缩）---但是强同步只有主角同样没问题
            逻辑帧处理就是效仿服务器的机制接入到客户端---也是没问题逻辑思考简单化*/

        /// <summary>
        /// 自己，这个类型信息的，消息缓存 的拥堵加速
        /// 第三人称单路点加速
        /// 现在全局加速了
        /// 怪物和thd都会加速了，都有自己，消息类型，消息缓存数量了所以都可以加速了
        /// 【逻辑帧分发到移动上路点，和单点拥堵导致的加速】因为到移动不至于直接处理结束，移动要过程化表达，甚至thd要都走到所以还是有这个二级缓存，要过程化慢慢处理流程消息对垒（只是执行过程要加速）
        /// </summary>
        public const float THD_PERSION_CLIENT_SIM_SPEED_PARA = 1.1f;//1.5f;


        //可以优化的点在于可以两个值做和

        public const float AOI_CLIENT_SIM_SPEED_PARA = 1.02f;//1.5f;


        ///// <summary>
        ///// 自己，这个类型信息的，消息缓存 的拥堵加速
        ///// [全局消息会因为【这个类型，自己，拥堵数量】而加速
        ///// </summary>
        //public const float G_CLIENT_SIM_SPEED_PARA = 1.3f;//1.5f;
        #endregion

        #region  场景相关
        //public const float GROUND_HEIGHT = 0f;
        //public const float PLAY_HEIGHT_3D = 0f;
        //public const float PLAY_HEIGHT_2D = -6f;
        //public const float CAMERA_HEIGHT_2D = -100f;

        public const float CHANAGE_MAP_DELAY_BLACKPANEL = 1f; // 切地图延迟黑幕面板时间 秒

        #endregion

        #region  NPC碰撞相关
        //public const float PYH_COLLIDER_R = 3.2f;//碰撞触发范围
        //public const float LARGE_COLLIDER_R = 3f;//场景大触发范围
        //public const float SMALL_COLLIDER_R = 2f;//场景小触发范围
        //public const float LARGE_TASK_NPC_R = 5f;//场景小触发范围
        //public const float SMALL_TASK_NPC_R = 1.1f;//场景小触发范围
        //public const float SMALL_EXTEND_ENV_ITEM = 5f;//场景小触发范围
        //public const float LARGE_EXTEND_ENV_ITEM = 5.1f;//场景小触发范围


        //public const float LARGE_SMALLBUILD_R = 6f;//小微建筑大距离
        //public const float SMALL_SMALLBUILD_R = 3f;//小微建筑小距离
        #endregion

        #region  标准定义相关
        //public const int RESOLUTION_RATIO = 128;
        //public const int MAP_IMAGE_SIZE = 1024;
        //public const int MAP_MASK_SIZE = 512;
        #endregion

        #region  摄像机相关 - 配置 json

        /////------------------------------- 虚拟相机配置 ----------------------------
        public const float SCALE_FACTOR = 1f;                   // 缩放系数
        public const float DEFAULT_DISTANCE = 10f;              // 默认距离
        public const float MAX_DISTANCE = 14f;                  // 最大距离
        public const float THRESHOLD_DISTANCE = 1f;             // 阀值距离
        public const float MIN_DISTANCE = 3f;                 // 最小距离
        public const float MAX_ANGLE_OF_PITCH = 43f;            // 最大俯视角
        public const float MIN_ANGLE_OF_PITCH = 18f;            // 最小俯视角
        public const float MAX_MANUAL_ANGLE_OF_PITCH = 55f;     // 手动操作最大俯视角
        public const float MIN_MANUAL_ANGLE_OF_PITCH = 25f;     // 手动操作最小俯视角
        /////------------------------------- 虚拟相机配置 ----------------------------


        #endregion

        #region  UI相关
        //public const int TASK_BAG_MAX_COUNT = 4;
        //public const int TASK_STATE_MAX_COUNT = 4;
        /// <summary>
        /// 系统消息，在逻辑帧中，平均[多少个]逻辑帧显示一个
        /// </summary>
        public static int SPECIAL_MESSAGE_FIX_SPACE_DELAY = 5;

        /// <summary>
        /// 成就消息，播放间隔（秒）
        /// </summary>
        public static float ACHIEVEMENT_MESSAGE_SPACE_DELAY = 2.1f;

        /// <summary>
        /// 系统消息，播放间隔（秒）
        /// </summary>
        public static float SYSTEM_MESSAGE_SPACE_DELAY = 2f;

        /// <summary>
        /// UI BUFF闪烁最小时间
        /// </summary>
        public static float UI_BUFF_FADE_MIN_TIME = 1.0f;

        #endregion

        #region  配置相关
        //public const int SPECIAL_MESSAGE_DELAY = 5;
        //public const int ITEM_FLY_DELAY = 5;
        //public const int MAX_GAME_TYPE_COUNT = 200;
        //public  const int E_AttrNameStar = 100;
        public const float SEC_RATE = 1f;
        public const float TEN_SEC_RATE = 10f;
        public const float MINUTES_RATE = 60f;

        #endregion

        #region 网络相关
        //public const string IP = "127.0.0.1";
        //public const int Port = 9876;
        #endregion

        #region 【system】配置表的 规则 先写这里，后面多了就动态读json,GameConfig里面的东西都挪到配置表中

        public const float System_BattleStateCountDown = 6.0f;
        /// <summary> 技能切遥感位移的【强制时间（毫秒）】 </summary>
        public const float System_ChangeMoveForceTime = 0.05f;
        /// <summary> 遥感位移聊天的显示时间【时间（毫秒）】 </summary>
        public const float System_ChangeMoveForceTimeChat = 5f;
        public const float SYSTEM_MOVE_SPEED_MAX = 600f;//600cm/s
        public const float SYSTEM_MOVE_SPEED_MIN = 60f;//60cm/s
        /// <summary> 技能按下释放判断【时间（毫秒）】 </summary>
        public const float System_Skill_Down_Time = 0.067f;
        // 死亡倒计时
        public const float System_Die_Count_Down = 2.0f;
        #endregion

        #region 属性相关的配置
        /// <summary>
        /// 百分比属性的基数(服务器都是万分比,数据都放大了100倍)
        /// </summary>
        public const int AttrMaxRateValue = 10000;

        #endregion

        #region  技能相关配置




        public const string RES_CONFIG_JSON_PATH = "Config/Skill/";

        /// <summary>
        /// 技能的 网络 延迟 误差范围
        /// </summary>
        public const int SKILL_LAG_TIME = 100;

        /// <summary>
        /// 技能阶段 的 网络 误差 范围
        /// </summary>
        public const int SKILL_STAGE_NET_LAG_TIME = 100;

        /// <summary>
        /// 静态的 技能发送协议 网络延迟 基础参数
        /// </summary>
        public const float STATIC_SKILL_LAG_TIME = 15;//ms
        /// <summary>
        /// 动态调整的 技能发送协议 网络延迟的 参数影子
        /// </summary>
        public const float DYNAMIC_FORBID_RATE = 1.1f;//ms

        public const string SKILL_BUILDER = "Builder";
        public const string SKILL_OWNER = "Owner";

        // 使用技能黑板key常量
        public const string SKILL_INPUTTYPE_DIRINPUT = "InputRota";
        public const string SKILL_INPUTTYPE_POSINPUT = "InputCoord";
        public const string SKILL_INPUTTYPE_OBJINPUT = "InputTarget";



        /// <summary>
        /// 目前跟 策划约定的 伙伴的 技能位  写死的 100；
        /// </summary>        
        public const int SKILL_PARTNER_POS_ID = 100;

        #endregion

        #region 各阵营区分颜色

        // 同队伍
        // 主角
        public const string COLOR_TITLE_TEAM = "#A0FF76";
        public const string OUTLINE_TITLE_TEAM = "#3A7243";
        public const int OUTLINE_TITLE_Font_TEAM = 1;
        public const string COLOR_NAME_TEAM = "#FFFFFFFF";
        public const string OUTLINE_NAME_TEAM = "#050404FF";
        public const int OUTLINE_NAME_Font_TEAM = 1;

        // 友方
        public const string COLOR_NAME_FRIEND = "#3BD1CAFF";
        public const string OUTLINE_NAME_FRIEND = "#050404FF";
        public const int OUTLINE_NAME_Font_FRIEND = 1;

        // 非好友
        public const string COLOR_NAME_NOFRIEND = "#d3d3d3";
        public const string OUTLINE_NAME_NOFRIEND = "#5d5d5d";
        public const int OUTLINE_NAME_Font_NOFRIEND = 1;

        // 同工会
        public const string COLOR_NAME_LABOR_UNION = "#ffd35d";
        public const string OUTLINE_NAME_LABOR_UNION = "#a76d34";
        public const int OUTLINE_NAME_Font_LABOR_UNION = 1;

        // 敌对玩家
        public const string COLOR_NAME_ENEMY = "#EE2523FF";
        public const string COLOR_NAME_ENEMY2 = "#C8AD45FF";
        public const string OUTLINE_NAME_ENEMY = "#050404FF";
        public const int OUTLINE_NAME_Font_ENEMY = 1;

        // NPC
        public const string COLOR_NAME_NPC = "#3BD1CAFF";

        //PVP
        public const string COLOR_NAME_BLUE = "#64C3FF";
        public const string COLOR_NAME_RED = "#FF6464";

        #endregion

        #region 优化设定相关
        public static byte QualityForCameraClipNeighbor = 0;
        public static bool AllowDynamicResolution = true;
        private static MachineQualityLevel machineQualityLevel = MachineQualityLevel.TopLevel; //0是顶级，是高配机，对于内存检测和系统设定来降低
        public static MachineQualityLevel MachineQualityLevel
        {
            get => machineQualityLevel;
            set
            {
                if (machineQualityLevel != value)
                {
                    machineQualityLevel = value;
                    GlobalEvent.OnQualChangeTinyModuleReflesh.Invoke((int)machineQualityLevel);//快速刷新
                }

            }
        }
        /// <summary>
        /// 就为了角色选择的时候是高配的这种设定单独做的
        /// </summary>
        public static MachineQualityLevel FirstMachineCheckCache;
        public static void FleshMachineQualityLevel()
        {
            GlobalEvent.OnQualChangeTinyModuleReflesh.Invoke((int)machineQualityLevel);//快速刷新
        }
        //总有一些不会随着设置立刻生效，甚至游戏开启本次都不会变化，dota设定也如此，有很多没办法的
        //比如你是加载地模地图uv都不一样你贴图换了模型也要换但是地图不可能立刻换了，
        //比如角色加载也不可能换让内存有两份
        public static MachineQualityLevel InitQualityLevel;

        internal static bool isEmulator;

        public static float UIFxScale;
        #endregion

        #region

        public static bool UseSGSR = false;
        public static ModelQualityLevel modelType = ModelQualityLevel.Low;
        public static float SGSR_EdgeSharpness = 1.0f;

        #endregion

        #region 一些 通用的 触发事件的 枚举

        public const string HIDDEN_UI_EVENT = "hidden_ui_event";


        /// <summary>
        /// 点击事件
        /// </summary>
        public const string TOUCH_EVENT = "touch_event";

        /// <summary>
        /// 点击 屏幕使用 技能的 事件
        /// </summary>
        public const string TOUCH_USE_SKILL_EVENT = "touch_use_skill_event";

        #endregion

        #region C# 定义的【UI】类型枚举


        /// <summary>
        /// 固定的 约定 好的 技能 按钮 基础  类型 枚举, 策划 想要 一个 基础按钮类型 , 配置 所有的 技能按钮默认的 labs
        /// 然后 根据 每个 技能 自己 独有的 labs, 组合成一个 当前 技能槽 技能的 合集 labs
        /// </summary>
        public const int SKILL_BTN_TYPE_ENUM = 10000;

        public const int HUD_MINI_MAP = 100001;

        public const int BUFF_SHOW_MAX_COUNT = 5;

        #endregion

        #region 大地图配置

        public const string MAP_COMMON_CACHE = "WorldMapCommonCache";
        public const string MAP_COMMON_MAP_CACHE = "WorldMapCommonCache_";

        #endregion

        #region 本地缓存数据
        public const string SYSTEM_OPEN_All = "SystemOpenAll";

        public const string SYSTEM_CACHE = "SystemCache";

        // PV播放
        public const string SETTING_PV = "SettingPV";

        #region 设置缓存key

        public const string SETTING_KEY = "SettingKey";
        public const string SETTING_GRAPHKEY = "SettingGraphKey";
        public const string SETTING_GRAPHKEY_LEVEL = "SettingGraphKeyLevel";
        // 总音效
        public const string SETTING_ALL_MUSIC = "SettingAllMusic";
        // 背景音效
        public const string SETTING_BACKGROUND_MUSIC = "SettingBackgroundMusic";
        // 系统音效
        public const string SETTING_SYSTEAM_MUSIC = "SettingSysteamMusic";
        // 环境音效
        public const string SETTING_SURROUNDINGS_MUSIC = "SettingSurroundingsMusic";
        // 语言缓存
        public const string SETTING_LANGUAGE = "SettingLanguage";

        // 画质品质
        public const string SETTING_GRAPH = "SettingGraph";

        // 战斗
        public const string SETTING_BATTLE = "SettingBattle";
        public const string SETTING_BATTLE_PARTNER = "SettingBattle_Partner_New";

        // SGSR开关
        public const string SETTING_SGSR = "SettingSGSR";

        // SGSR锐度
        public const string SETTING_SGSR_EdgeSharpness = "SettingSGSREdgeSharpness";

        // 是否屏蔽他人特效
        public const string SETTING_SGSR_Fx = "SettingFx";
        
        // 相机选择设置
        public const string SETTING_CAMERA = "Setting_Camera";

        #endregion

        #endregion

        #region 主角脚印路径

        public const string MAIN_PLAYER_FOOTPRINT_PATH = "Effects/cm/FX_UI_Jiaoying";

        public const int FOOTPRINT_MAP_ID = 27;

        #endregion

        #region 副本子类型

        //普通副本
        public const string INSTANCE_NORMAL = "Normal";
        //个人日常本
        public const string INSTANCE_PERSON_DAILY = "PersonDaily";
        //个人秘境
        public const string INSTANCE_PERSON_SERCET = "PersonSecret";
        //剧情
        public const string INSTANCE_PLOT = "Plot";
        //镜像
        public const string INSTANCE_MIRROR = "Mirror";
        //剧情宝图
        public const string INSTANCE_PLOT_TREASURE = "PlotTreasure";
        //镜像宝图
        public const string INSTANCE_MIRROR_TREASURE = "MirrorTreasure";
        //组队日常本
        public const string INSTANCE_TEAM_DAILY = "TeamDaily";
        //通缉任务
        public const string INSTANCE_WTASK = "WTaskSpace";
        //公会午间玩法
        public const string INSTANCE_GUILD_NOON = "GNG";
        //随机战场玩法
        public const string INSTANCE_10v10 = "10v10";
        //异步竞技场
        public const string INSTANCE_1v1 = "1v1";
        //个人爬塔
        public const string INSTANCE_PERSON_TOWER = "PersonTower";
        //新手关
        public const string INSTANCE_PLOT_FIRST = "PlotFirst";
        //新手关2
        public const string INSTANCE_PLOT_SECOND = "PlotSecond";
        //新手关3
        public const string INSTANCE_PLOT_THIRD = "PlotThird";
        // 公会领地
        public const string INSTANCE_GuildTerritory = "GuildTerritory";

        #endregion

        #region 场景实体的最大值

        public const int SceneVisibleMax = 50;


        #endregion

        #region 通用次数ID定义

        // 通用跨天,一般指的是五点
        public const int EComCount_Day_1 = 1;
        // 通用跨周
        public const int EComCount_Week = 2;
        //免费复活对应的表ID
        public const int ComCount_Revive = 1000;
        // 日常资源本挑战次数  GLOBAL_DAILY_INSTANCE_ID = 8
        public const int EComCount_DailyResInstance_8 = 8;
        // 组队日常本的领奖次数
        public const int EComCount_DailyTeamReward_11 = 11;
        // 公会商店重置时间
        public const int EComCount_GuildShop = 14;
        //公会午间活动挑战次数
        public const int EComCount_DailyGng_15 = 15;
        //银行每日兑换钻石数量
        public const int EComCount_Trade_Diamond = 16;
        // 竞技场可用挑战次数
        public const int EComCount_ArenaLeftFightCount_17 = 17;
        // 野外boss领奖次数
        public const int EComCount_WildBossRewardCount_18 = 18;
        // 派对时刻挂机奖励领奖次数
        public const int EComCount_PartyTimeHangUpRewardCount_20 = 20;
        // 每日玩家获取经验值数量
        public const int EComCount_DailyPlayerGetExp_24 = 24;


        #endregion

        #region url 地址相关
        // // 用户隐私协议
        // public const string URL_THE_STAR_PRIVACY = "https://sites.google.com/view/thestarprivacy";
        // // 用户协议地址
        // public const string URL_THE_STAR_LICENSE = "https://sites.google.com/view/thestarlicense";


        // 用户隐私协议
        public const string URL_THE_STAR_PRIVACY = "https://www.zhoushanlianqing.com/qws-mxzs/privacy-policy.html";
        // 用户协议地址
        public const string URL_THE_STAR_LICENSE = "https://www.zhoushanlianqing.com/qws-mxzs/user-agreement.html";


        public const string URL_THE_STAR_CUS_SERVICE = "https://mkf.zhoushanlianqing.com/front/index/templet-show?id=171";
        #endregion

    }
}