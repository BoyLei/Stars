
///--------------------------------------------------------------------
/// 文件名   :   SkillEditorDefine
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By EnumDefine.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
namespace SkillEditor
{

    /// <summary>
    /// 布尔值
    /// </summary>
    public enum Boolean 
    {

        TRUE=1,                 //是
        FALSE=0,                 //否
    }

    /// <summary>
    /// 公式枚举
    /// </summary>
    public enum EffFormula 
    {

        EF_Damage_Phy=1,                 //物理伤害公式
        EF_Damage_Earth=11,                 //大地伤害
        EF_Damage_Fire=12,                 //烈焰伤害
        EF_Damage_Natura=13,                 //自然伤害
        EF_Damage_Toxic=14,                 //剧毒伤害
        EF_Damage_Light=15,                 //光明伤害
        EF_Damage_Dark=16,                 //黑暗伤害
        EF_Damage_TrueOne=17,                 //真实伤害1
        EF_Damage_TrueTwo=18,                 //真实伤害2
        EF_Damage_PerAttack=101,                 //攻击力的百分比
        EF_FixedValue=102,                 //固定数值
    }

    /// <summary>
    /// 比较符
    /// </summary>
    public enum CompOperator 
    {

        CO_Equal=1,                 //等于
        CO_NotEqual=2,                 //不等于
        CO_Greater=3,                 //大于
        CO_Less=4,                 //小于
        CO_EqualGreater=5,                 //大于等于
        CO_EqualLess=6,                 //小于等于
        CO_SET_SAME=7,                 //集合相同
        CO_SET_INCLUDE=8,                 //集合包含
        CO_SET_UNINCLUDE=9,                 //集合不包含
        CO_SET_HAVE=10,                 //在arr1中找arr2的成员，只要找到一个就成功
    }

    /// <summary>
    /// 技能施法方式
    /// </summary>
    public enum CastMethodType 
    {

        DirectCast=0,                 //按下释放，松开无事
        DirectCastOnLoosen=1,                 //按下无事，松开释放
        WheelCast=2,                 //按下选择，松开释放
        GatherWheelCast=3,                 //蓄力释放，蓄力期间有轮盘
        GatherWheelCastNoIndicator=4,                 //蓄力释放，蓄力期间无轮盘
    }

    /// <summary>
    /// 动作状态
    /// </summary>
    public enum AnimState 
    {

        Idle=1,                 //休闲待机
        BattleIdle=2,                 //战斗待机
        WanderMoving=3,                 //漫步移动
        SingleMoving=4,                 //休闲移动
        BattleMoving=5,                 //战斗移动
        Hurt=6,                 //受击
        Deading=7,                 //死亡
        WeaponRetractionIdle=8,                 //待机收刀
        WeaponRetractionMoving=9,                 //移动收刀
    }

    /// <summary>
    /// 技能输入类型
    /// </summary>
    public enum SkillInputType 
    {

        DirInput=1,                 //朝向输入
        PosInput=2,                 //坐标输入
        ObjInput=3,                 //对象输入
    }

    /// <summary>
    /// 特效放缩枚举
    /// </summary>
    public enum SpecEffectScale 
    {

        Origin=0,                 //永不跟随
        NoFollow=1,                 //创建跟随
        Follow=2,                 //永远跟随
    }

    /// <summary>
    /// 子弹高度跟随类型
    /// </summary>
    public enum BulletHeightType 
    {

        BornWithRole=0,                 //出生跟随主角
        FollowRole=1,                 //永远跟随主角
        BornCheckSpace=2,                 //出生紧贴地面
    }

    /// <summary>
    /// 技能条件
    /// </summary>
    public enum ConditionType 
    {

        Cond_Buff=1,                 //BUFF层数
        Cond_Attr=2,                 //属性值
        Cond_SkillTime=3,                 //前置技能时间
        Cond_HpPercent=4,                 //血量万分比
        Condition_CurSpectralElem=5,                 //末位量谱值
        Cond_TargetFaction=6,                 //敌人阵营
    }

    /// <summary>
    /// 碰撞盒修改方式
    /// </summary>
    public enum DynamicRangeType 
    {

        NoChange=1,                 //无动态增长
        ChargeTime=2,                 //根据蓄力时间
        ChargeNum=3,                 //根据蓄力层数
    }

    /// <summary>
    /// 原子状态
    /// </summary>
    public enum BattleState 
    {

        ForbidMove=1,                 //禁止主动移动
        ForbidDir=2,                 //禁止转向
        ForbidHurt=3,                 //禁止被伤害
        ForbidCure=4,                 //禁止被治疗
        ForbidAttack=5,                 //禁止普攻
        ForbidSkill=6,                 //禁止技能
        ForbidDisplacement=7,                 //禁止位移
        ForbidSelect=8,                 //禁止被选择器选中
        ForbidBreakSkill=9,                 //禁止打断技能
    }

    /// <summary>
    /// 形状
    /// </summary>
    public enum Shape 
    {

        None=0,                 //空
        Round=1,                 //圆形
        HollowCircle=2,                 //空心圆
        Sector=3,                 //扇形
        RingFan=4,                 //环扇形
        Rect=5,                 //矩形
        RotRoute=6,                 //朝向路径
        InputTarget=7,                 //技能选取目标
        Arrow=8,                 //箭头
        PosRoute=9,                 //点位路径
    }

    /// <summary>
    /// 目标选择类型
    /// </summary>
    public enum SelectType 
    {

        Friend=1,                 //友方
        Enemy=2,                 //敌方
        All=3,                 //全体
        FriendPlayer=4,                 //敌方玩家
        EnemyPlayer=5,                 //友方玩家
        AllPlayer=6,                 //全体玩家
        FriendBullet=7,                 //敌方子弹
        EnemyBullet=8,                 //友方子弹
        AllBullet=9,                 //全体子弹
        FriendlyTeam=10,                 //友方队友
    }

    /// <summary>
    /// 目标选择类型
    /// </summary>
    public enum TargetType 
    {

        RecentPerson=1,                 //最近的敌人
        FarPerson=2,                 //最远的敌人
    }

    /// <summary>
    /// Line绘制方式
    /// </summary>
    public enum LineTypeEnum 
    {

        Tiling=0,                 //平铺
        Stretching=1,                 //拉伸
    }

    /// <summary>
    /// Line链接方式
    /// </summary>
    public enum LinkTypeEnum 
    {

        Sequence=0,                 //连续
        Distribution=1,                 //分发
        NoFromSequence=2,                 //不带主人连续
    }

    /// <summary>
    /// 效果类型
    /// </summary>
    public enum EffectType 
    {

        Empty=1,                 //空效果
        CollisionBox=2,                 //碰撞盒
        TarGroup=3,                 //目标集合的操作
        CollisionBoxBullet=4,                 //子弹碰撞盒
        SpSelectTarget=5,                 //选择特殊目标
        Treat=21,                 //治疗
        Damage=22,                 //伤害
        MoveWithPos=23,                 //根据坐标位移
        MoveWithRot=24,                 //根据朝向位移
        Register=25,                 //黑洞效果
        ChangeCD=26,                 //修改CD
        BreakCurRuntime=27,                 //打断目标当前技能
        ChangeProp=28,                 //修改属性
        ChangeToward=29,                 //修改朝向
        ChangeMana=30,                 //修改蓝量
        AddBuff=31,                 //施加BUFF
        RemoveBuff=32,                 //移除BUFF
        AddPassive=33,                 //施加被动
        ChangeSpectral=34,                 //修改量谱值
        SpChangeSpectral=35,                 //昧光增加影能量
        AppendValueToSpectral=36,                 //特殊增加量谱值
        RemoveValueFromSpectral=37,                 //特殊移除量谱值
        SpectralStackCheck=38,                 //特殊判断量谱值
        CreateBullet=41,                 //创建子弹
        DestoryBullet=42,                 //销毁子弹
        DestoryBulletOwner=43,                 //销毁子弹自身
        ThrowBullet=44,                 //抛出子弹
        LaunchBullet=45,                 //发射子弹
        BezierBullet=46,                 //贝塞尔子弹
        UserInput=71,                 //用户输入
        ChargeInput=72,                 //蓄力输入
        BreakCurRuntimeInBullet=73,                 //打断指定类型阶段
        TriggerStage=74,                 //触发自定义阶段
        SummonMonster=81,                 //召唤物
        RandomPoint=91,                 //随机多个点位
        SetBulletTargetPos=92,                 //设置子弹目标点
        SingleRandomPoint=93,                 //随机单个点位
        GetFootPoint=94,                 //计算垂足点
        GetMirrorPoint=95,                 //计算对称点
        GetPointWithBuffTime=96,                 //随BUFF时间计算点位
        ChangeBulletSpeed=97,                 //设置子弹移动速率
        GetPointWithAToB=98,                 //从A指向B计算可达点
        SetCDKeyNum=101,                 //设置指定CDKey数值
        ThisSkillEnterCD=102,                 //当前技能进入CD
        IsNotEmpty=110,                 //判断目标Key是否不为空
        SelectHitFromKey=111,                 //对目标Key做命中处理
        SetIntKey=112,                 //设置int值到目标Key
        CheckIntKey=113,                 //检查目标Key的int值
        SetBuffTime=114,                 //重设BUFF剩余持续时间
        GetSqualDis2Pos=115,                 //获得两个点位间距离的平方
        SubBUFFLayer=116,                 //减去BUFF层数
        CheckJobID=151,                 //检查职业ID
        CheckTargetMonsterType=152,                 //检查目标怪物类型
        BuffAddCancel=201,                 //取消BUFF添加
        DamageSecond=202,                 //二次伤害
        DamageChange=203,                 //改变伤害
        AddShields=204,                 //增加护盾值
        DecShields=205,                 //减少护盾值
        CancelOffest=206,                 //取消移动
        CancelRota=207,                 //取消转向
        ShieldEffect=208,                 //护盾抵挡
        ChangeJobMassage=301,                 //转职消息
        SendMapMassage=302,                 //场景消息
        PlayEffectAtPoint=1001,                 //在点位播放特效
        PlayEffectAtTarget=1002,                 //在目标播放特效
        Stealth=1003,                 //隐身
        PlayEffectBetweenPoints=1004,                 //在两点间播放特效
        PlayEffectLineRenderer=1005,                 //连线
        ClientSummonAnim=1101,                 //客户端召唤物播放动作
        ClientSummonEffect=1102,                 //客户端召唤物播放特效
        ClientSummonTurnTo=1103,                 //客户端召唤物朝向目标
        ClientSummonRemove=1104,                 //客户端召唤物移除
        SetValueToKey=2001,                 //绝对数值产生器
        GetGroupIDToKey=2002,                 //组数值产生器
        RamdonValueToKey=2003,                 //随机数值产生器
        GetLengthFromListToKey=2004,                 //集合成员数值产生器
        OperationValueToKey=2005,                 //数值运算产生器
        GetTargetPropToKey=2006,                 //目标属性数值产生器
        GetCDKeyCDToKey=2007,                 //冷却时间数值产生器
        GetConfigIDToKey=2008,                 //配置ID数值产生器
        GetTowardFromEntityToKey=4001,                 //单位朝向产生器
        GetTowardFromPosToKey=4002,                 //点位朝向产生器
        CheckToward=7001,                 //1是否在2的角度范围内检查器
        CheckPassive=7101,                 //目标是否有指定被动检查器
        ChangeToAbsoluteToward=8001,                 //目标集合修改至绝对朝向
        ChangeToInputSpectral=8101,                 //目标集合量谱值修改
        ChangeToCDKeyCD=8102,                 //目标集合冷却时间修改
        CreateInteract=11001,                 //创建交互物
    }

    /// <summary>
    /// 作用目标集合源
    /// </summary>
    public enum Source 
    {

        InputPos=1,                 //输入点位
        Target=2,                 //输入目标
    }

    /// <summary>
    /// 效果条件
    /// </summary>
    public enum EffectCondition 
    {

        Damage=1,                 //伤害
        Treat=2,                 //治疗
        Hit=3,                 //命中
        Range=4,                 //随机
        Dodge=5,                 //闪避
        Critical=6,                 //暴击
        BUFFID=7,                 //BUFFID
        BUFFTag=8,                 //BUFFtag
        TargetLife=9,                 //目标血量
        TargetMana=10,                 //目标蓝量
        TargetNum=11,                 //命中人数
        TargetBUFFID=12,                 //目标BUFFTID
        TargetBUFFTAG=13,                 //目标BUFFTAG
        TargetBUFFNum=14,                 //目标BUFFF层数
    }

    /// <summary>
    /// 生效阶段
    /// </summary>
    public enum CheckState 
    {

        Stage_AttackPro=1,                 //攻击前
        Stage_Attacked=2,                 //攻击后
        Stage_OnHurtPro=3,                 //受伤前
        Stage_OnHurted=4,                 //受伤后
        Stage_KillPro=5,                 //杀死目标前
        Stage_Killed=6,                 //杀死目标后
        Stage_OnDeadPro=7,                 //被杀前
        Stage_OnDeaded=8,                 //被杀后
        Stage_OnBuffAddPro=9,                 //buff添加前
        Stage_OnBuffAdded=10,                 //buff添加后
        Stage_OnHited=11,                 //命中后
        Stage_OndodgePro=12,                 //闪避前
        Stage_CurePro=13,                 //治疗前
        Stage_Cured=14,                 //治疗后
        Stage_OnCurePro=15,                 //被治疗前
        Stage_OnCured=16,                 //被治疗后
        Stage_RemoveBuffed=17,                 //移除buff后
        Stage_OnBuffRemoveed=18,                 //被移除buff后
        Stage_OnOffsetPro=19,                 //被位移前
        Stage_OnOffseted=20,                 //被位移后
        Stage_OffsetPro=21,                 //位移前
        Stage_Offseted=22,                 //位移后
        Stage_SpectralChanged=23,                 //量谱变化后
    }

    /// <summary>
    /// 位移标签
    /// </summary>
    public enum MoveLabel 
    {

        Normal=1,                 //无特效
        Sprint=2,                 //残影
        Flash=3,                 //隐身
    }

    /// <summary>
    /// 位移类型
    /// </summary>
    public enum MoveType 
    {

        DefaultOffset=0,                 //取消位移
        Dash=1,                 //冲撞(都不穿)
        Rush=2,                 //冲锋(不穿墙)
        Flash=3,                 //闪现(都穿)
        BeFly=4,                 //击飞
        BeRush=5,                 //击退
        BeMove=6,                 //被移动
    }

    /// <summary>
    /// 怪物类型
    /// </summary>
    public enum MonsterType 
    {

        Lackey=1,                 //小怪
        Elite=2,                 //精英
        BOSS=3,                 //BOSS
    }

    /// <summary>
    /// 伤害类型
    /// </summary>
    public enum DamageType 
    {

        El1=1,                 //元素伤害1
        El2=2,                 //元素伤害2
        El3=3,                 //元素伤害3
    }

    /// <summary>
    /// 修改方式
    /// </summary>
    public enum CDChangeType 
    {

        ChangeWithNum=1,                 //按照绝对值修改技能剩余cd
        ChangeWithPer=2,                 //按照百分比修改技能剩余cd
        ChangeAllWithNum=3,                 //设置技能模块技能总cd时间
        ChangeAlliWithPer=4,                 //按照百分比设置技能模块技能总cd时间
    }

    /// <summary>
    /// 计算坐标方式
    /// </summary>
    public enum CalcuType 
    {

        ChangeWithPer=2,                 //向坐标点方向移动
        ChangeAllWithNum=3,                 //角度移动
    }

    /// <summary>
    /// 集合操作方式
    /// </summary>
    public enum TarGroupOperType 
    {

        TarGroupOperType_1=1,                 //返回集合1不在集合2里的成员
        TarGroupOperType_2=2,                 //返回集合1在集合2里的成员
        TarGroupOperType_3=3,                 //返回所有成员
        TarGroupOperType_4=4,                 //返回不重合的成员
    }

    /// <summary>
    /// 阶段打断事件
    /// </summary>
    public enum InterruptEvent 
    {

        KillStage=1,                 //结束阶段
        KillRunTime=2,                 //结束运行时
    }

    /// <summary>
    /// 特效挂点
    /// </summary>
    public enum HangPoint 
    {

        Root=0,                 //根节点
        Hurt_D=1,                 //胸部受击
        BackWeapon_D=2,                 //后背部武器
        Wing_D=3,                 //后背部翅膀
        HandWeapon_D_L=4,                 //左手武器手部
        HandWeapon_D_R=5,                 //右手武器手部
        WeaponRoot_D_L=6,                 //左手武器根部
        WeaponRoot_D_R=7,                 //右手武器根部
        WeaponHurt_D_R=8,                 //右手或者双手武器特效
        WeaponHurt_D_L=9,                 //左手武器特效
        Top_D=10,                 //头顶特效
        Foot_D_R=11,                 //右脚脚底特效
        Foot_D_L=12,                 //左脚脚底特效
        UnitInfo_D=13,                 //头顶信息（气泡，公会，名字等）
        Root_D=14,                 //脚下特效节点
        Bip001=15,                 //尾巴骨挂点
        Mouth_D=16,                 //嘴巴挂点
        Mouth_D02=17,                 //嘴巴2挂点
        Mouth_D03=18,                 //嘴巴3挂点
    }

    /// <summary>
    /// 记录类型
    /// </summary>
    public enum EffectTypeWaitInputRecordType 
    {

        RecordTime=0,                 //记录时间
        RecordLayer=1,                 //记录层数
    }

    /// <summary>
    /// 目标
    /// </summary>
    public enum TargetKey 
    {

        TransObject=1,                 //默认Key
        TransPos=2,                 //默认坐标
        InputCoord=3,                 //输入的坐标
        InputRota=4,                 //输入的朝向
        InputTarget=5,                 //输入的目标
        BBEnergy=6,                 //蓄力
        BBEnergyTime=7,                 //蓄力时间
        Builder=8,                 //施法者
        Owner=9,                 //拥有者
        Victim=10,                 //受击者
        LiveTime=11,                 //存活时间
        StackCount=12,                 //BUFF层数
        ShieldVal=13,                 //护盾值
    }

    /// <summary>
    /// 是否取命中
    /// </summary>
    public enum TransTargetIsHit 
    {

        IsHit=1,                 //选择命中单位
        IsMiss=2,                 //选择未命中单位
        All=3,                 //选择所有单位
    }

    /// <summary>
    /// 量谱值
    /// </summary>
    public enum Spectral 
    {

        Spectral1=1,                 //量谱值1
        Spectral2=2,                 //量谱值2
        Spectral3=3,                 //量谱值3
    }

    /// <summary>
    /// 输入类型
    /// </summary>
    public enum InputType 
    {

        Immediately=1,                 //立即输入
        Prepare=2,                 //预输入
    }

    /// <summary>
    /// 输入方式
    /// </summary>
    public enum InputMode 
    {

        Press=1,                 //长按
        Tap=2,                 //单击
    }

    /// <summary>
    /// 阶段事件
    /// </summary>
    public enum StageEvent 
    {

        None=0,                 //什么都不做
        JumpStage1=1,                 //Tick结束跳转
        JumpStage2=2,                 //立即跳转
        KillSkill=3,                 //结束技能
        KillStage=4,                 //结束循环
        ReSet=5,                 //跳转下次循环
        EndStageReSet=6,                 //结束当前阶段跳转下次循环
    }

    /// <summary>
    /// 根据自身或者根据坐标
    /// </summary>
    public enum BuilderOrPos 
    {

        Builder=1,                 //自身
        Pos=2,                 //坐标
    }

    /// <summary>
    /// 根据自身或者根据坐标
    /// </summary>
    public enum TriggerTargetType 
    {

        Onwer=1,                 //拥有者
        Builder=2,                 //施法者
        Victim=3,                 //受击者
    }

    /// <summary>
    /// 阶段类型
    /// </summary>
    public enum StageType 
    {

        NormalStage=0,                 //一般阶段
        TriggerStage=1,                 //触发阶段
        AddBuffStage=2,                 //Buff添加阶段
        EndBuffStage=3,                 //Buff结束阶段
        BulletStage=4,                 //子弹移动阶段
        EndPassiveStage=5,                 //被动结束阶段
    }

    /// <summary>
    /// 触发阶段枚举
    /// </summary>
    public enum TriggerStageEnum 
    {

        Stage_AttackPro=101,                 //攻击前
        Stage_Attacked=102,                 //攻击后
        Stage_OnHurtPro=103,                 //受伤害前
        Stage_OnHurted=104,                 //受伤害后
        Stage_KillPro=105,                 //杀死目标前
        Stage_Killed=106,                 //杀死目标后
        Stage_OnDeadPro=107,                 //死亡前
        Stage_OnDeaded=108,                 //死亡后
        Stage_OnBuffAddProed=109,                 //buff被添加前
        Stage_OnBuffAdded=110,                 //buff被添加后
        Stage_OnHited=111,                 //命中后
        Stage_OndodgePro=112,                 //闪避前
        Stage_CurePro=113,                 //治疗前
        Stage_Cured=114,                 //治疗后
        Stage_OnCurePro=115,                 //被治疗前
        Stage_OnCured=116,                 //被治疗后
        Stage_RemoveBuffed=117,                 //移除buff后
        Stage_OnBuffRemoveed=118,                 //buff被移除后
        Stage_OnOffsetPro=119,                 //位移前
        Stage_OnOffseted=120,                 //位移后
        Stage_OffsetPro=121,                 //施加位移前
        Stage_Offseted=122,                 //施加位移后
        Stage_SpectralChanged=123,                 //量谱变化之后
        Stage_AtTargetPos=124,                 //移动到目标点时
        Stage_SkillStart=125,                 //任意技能运行时启动时
        Stage_SkillFirstSelect=126,                 //任意技能首次选择到目标后
        Stage_Customize1=127,                 //自定义触发阶段
        Stage_HurtEffected=128,                 //完全伤害结束阶段
        Stage_OnRotaPro=129,                 //转向前
        Stage_OnRotaed=130,                 //转向后
        Stage_RotaPro=131,                 //施加转向前
        Stage_Rotaed=132,                 //施加转向后
        Stage_OnBuffAddPro=133,                 //添加buff前
        Stage_OnBuffAdd=134,                 //添加buff后
        Stage_CureEffected=135,                 //完全治疗结束阶段
    }

    /// <summary>
    /// 触发器枚举
    /// </summary>
    public enum TriggerTypeEnum 
    {

        DataHurt=1,                 //临时伤害数值
        DataCure=2,                 //临时治疗数值
        DataHit=3,                 //临时命中结果
        Random=4,                 //临时随机数值
        DodgeAttack=5,                 //临时闪避结果
        CriticalHit=6,                 //临时暴击结果
        DataBuffID=7,                 //临时数据是buff运行时，判断id
        DataBuffTag=8,                 //临时数据是buff运行时，判断tag
        DataOffset=9,                 //临时数据是位移节点，判断位移状态
        DataBuffCount=10,                 //临时数据是buff运行时，判断层数
        RemainBlood=101,                 //对象当前生命值百分比
        RemainManaPer=102,                 //对象当前蓝量百分比
        RemainMana=103,                 //对象当前蓝量绝对值
        PropValue=104,                 //对象对应属性id的属性值
        BlackHit=201,                 //伤害节点-命中集合人数
        DamageType=202,                 //伤害节点-包含伤害类型
        CurrEffectID=301,                 //当前效果id
        CurrEffectType=302,                 //当前效果类型
        CurrEffectTag=303,                 //当前效果标签
        Backstab=304,                 //是否触发背刺
        IntNode=305,                 //检测当前运行时黑板上的数值类节点数据
        SkillTag=306,                 //当前技能标签
        RuntimeRelationship=401,                 //调用者与运行时对象关系
        BuffID=402,                 //对象的buffid
        BuffTag=403,                 //对象的bufftag
        BuffFloor=404,                 //对象的指定buff层数
        CoolTime=405,                 //对象的技能冷却
        State=406,                 //对象的原子状态
        Stage=407,                 //对象的阶段ID
        Battle=408,                 //Owner是否在战斗状态
        CheckTargetMonsterType=409,                 //检查目标怪物类型
        CanGetSAPoint=501,                 //是否能获得利箭点
    }

    /// <summary>
    /// Buff替换方式
    /// </summary>
    public enum BuffReplace 
    {

        AddTime=1,                 //时间累加
        Refresh=2,                 //刷新
        AddLayer=3,                 //层数和时间累加
        NoEffect=4,                 //互不影响
        Only=5,                 //旧的不去新的不来
        JustLayer=6,                 //层数叠加
        TimeRefresh=7,                 //仅刷新时间
    }

    /// <summary>
    /// Buff抵抗属性
    /// </summary>
    public enum BuffConfront 
    {

        NoEffect=0,                 //无类型
        Vertigo=1,                 //眩晕
        Confusion=2,                 //混乱
        Decelerate=3,                 //减速
        Freeze=4,                 //冰冻
        Silent=5,                 //沉默
        Blinding=6,                 //致盲
        Fear=7,                 //恐惧
        Paralysis=8,                 //麻痹
    }

    /// <summary>
    /// 触发器枚举
    /// </summary>
    public enum BattlePropEnum 
    {

        Atk=3001,                 //攻击
        Defence=3002,                 //防御
        MDefence=3003,                 //魔抗
        Hp=3004,                 //生命
        Mp=3005,                 //法力
        RecHp=3006,                 //生命恢复值
        RecMp=3007,                 //法力恢复值
        AtkRate=3008,                 //万分比攻击加成
        DefRate=3009,                 //万分比防御加成
        MDefRate=3010,                 //万分比魔抗加成
        HpRate=3011,                 //万分比生命加成
        HitLv=3012,                 //命中等级
        DodgeLv=3013,                 //闪避等级
        CriLv=3014,                 //暴击等级
        CriDefLv=3015,                 //抗暴击等级
        CriDamLv=3016,                 //爆伤等级
        ExHit=3017,                 //额外命中率
        ExDodge=3018,                 //额外闪避率
        ExCri=3019,                 //额外暴击率
        ExDefCri=3020,                 //额外抗暴击率
        ExCriDam=3021,                 //额外爆伤率
        Pierce=3022,                 //破甲
        PierceRate=3023,                 //百分比破甲
        MPierce=3024,                 //法穿
        MPierceRate=3025,                 //百分比法穿
        Speed=3026,                 //移动速度
        SpeedAdd=3027,                 //移动速度加成
        AttackSpeed=3028,                 //攻击速度
        EnergySpeed=3029,                 //蓄力速度
        Spectral1=3030,                 //量谱槽1值
        Spectral2=3031,                 //量谱槽2值
        Spectral3=3032,                 //量谱槽3值
        SpectralRate1=3033,                 //量谱槽1万分比加成
        SpectralRate2=3034,                 //量谱槽2万分比加成
        SpectralRate3=3035,                 //量谱槽3万分比加成
        PhyDamAdd=3051,                 //物理伤害加成
        PhyDamDec=3052,                 //物理伤害削弱
        ElemDamAdd=3053,                 //元素伤害加成
        ElemDamDec=3054,                 //元素伤害削弱
        FinDamAdd=3055,                 //最终伤害加成
        FinDamDec=3056,                 //最终伤害减免
        Suck=3057,                 //吸血
        Thorns=3058,                 //反伤-荆棘
        BuffAddDamage=3059,                 //buff增伤万分比
        BuffVulnerable=3060,                 //buff易伤万分比
        ElemEarth=3200,                 //大地元素
        ElemFire=3201,                 //烈焰元素
        ElemIce=3202,                 //寒冰元素
        ElemNatura=3203,                 //自然元素
        ElemToxic=3204,                 //剧毒元素
        ElemLight=3205,                 //光明元素
        ElemDark=3206,                 //黑暗元素
        ElemAll=3220,                 //全元素攻击
        ResiEarth=3221,                 //大地抗性
        ResiFire=3222,                 //烈焰抗性
        ResiIce=3223,                 //寒冰抗性
        ResiNatura=3224,                 //自然抗性
        ResiToxic=3225,                 //剧毒抗性
        ResiLight=3226,                 //光明抗性
        ResiDark=3227,                 //黑暗抗性
        ElemEarthRate=3241,                 //大地元素百分比
        ElemFireRate=3242,                 //烈焰元素百分比
        ElemIceRate=3243,                 //寒冰元素百分比
        ElemNaturaRate=3244,                 //自然元素百分比
        ElemToxicRate=3245,                 //剧毒元素百分比
        ElemLightRate=3246,                 //光明元素百分比
        ElemDarkRate=3247,                 //黑暗元素百分比
        ElemAllRate=3260,                 //全元素百分比
        ResiDizz=3350,                 //抵抗眩晕
        ResiChaos=3351,                 //抵抗混乱
        ResiRetard=3352,                 //抵抗减速
        ResiFreeze=3353,                 //抵抗冰冻
        ResiSlience=3354,                 //抵抗沉默
        ResiBlind=3355,                 //抵抗致盲
        ResiFear=3356,                 //抵抗恐惧
        ResiNumb=3357,                 //抵抗麻痹
        ResiSuppress=3358,                 //抵抗压制
        ResiTodeter=3359,                 //抵抗震慑
        ResiOverheat=3360,                 //抵抗过热
        ResiStiff=3361,                 //抵抗僵直
        ResiTwine=3362,                 //抵抗缠绕
        ResiAbsent=3363,                 //抵抗失神
        ResiControl=3399,                 //控制抵抗
        EnhDizz=3300,                 //增强眩晕
        EnhChaos=3301,                 //增强混乱
        EnhRetard=3302,                 //增强减速
        EnhFreeze=3303,                 //增强冰冻
        EnhSlience=3304,                 //增强冰冻
        EnhBlind=3305,                 //增强致盲
        EnhFear=3306,                 //增强恐惧
        EnhNumb=3307,                 //增强麻痹
        EnhSuppress=3308,                 //增强压制
        EnhTodeter=3309,                 //增强震慑
        EnhOverheat=3310,                 //增强过热
        EnhStiff=3311,                 //增强僵直
        EnhTwine=3312,                 //增强缠绕
        EnhAbsent=3313,                 //增强失神
        EnhControl=3349,                 //控制增强
        TruthAtk=3400,                 //实际攻击力
        TruthHp=3401,                 //实际生命
        TruthDef=3402,                 //实际防御
        TruthMDef=3403,                 //实际魔抗
        TruthElemEarthRate=3404,                 //实际大地元素百分比
        TruthElemFireRate=3405,                 //实际烈焰元素百分比
        TruthElemIceRate=3406,                 //实际寒冰元素百分比
        TruthElemNaturaRate=3407,                 //实际自然元素百分比
        TruthElemToxicRate=3408,                 //实际剧毒元素百分比
        TruthElemLightRate=3409,                 //实际光明元素百分比
        TruthElemDarkRate=3410,                 //实际黑暗元素百分比
        TruthElemEarth=3411,                 //实际大地元素
        TruthElemFire=3412,                 //实际烈焰元素
        TruthElemIce=3413,                 //实际寒冰元素
        TruthElemNatura=3414,                 //实际自然元素
        TruthElemToxic=3415,                 //实际剧毒元素
        TruthElemLight=3416,                 //实际光明元素
        TruthElemDark=3417,                 //实际黑暗元素
        TruthSpeed=3418,                 //实际移动速度
        TruthHitRate=3419,                 //实际命中率
        TruthDodgeRate=3420,                 //实际闪避率
        TruthCriRate=3421,                 //实际暴击率
        TruthDefCirRate=3422,                 //实际抗暴击率
        TruthCirDamRate=3423,                 //实际爆伤率
        TruthMP=3424,                 //实际法力
        TruthAttackSpeed=3425,                 //实际攻速
        TruthEnergySpeed=3426,                 //实际蓄力速度
        TruthSpectral1=3427,                 //实际量谱槽1
        TruthSpectral2=3428,                 //实际量谱槽2
        TruthSpectral3=3429,                 //实际量谱槽3
        curHp=3450,                 //当前生命
        curMp=3451,                 //当前法力
        curSpectral1=3452,                 //当前量谱槽1
        curSpectral2=3453,                 //当前量谱槽2
        curSpectral3=3454,                 //当前量谱槽3
        HeroId=3501,                 //战斗单位的表格ID
        TargetBattleEntityID=3502,                 //当前盯着目标的编号
        BattleSubState=3503,                 //战斗状态下随机子状态
        State=3901,                 //战斗状态
        Job=3902,                 //职业
        Faction=3903,                 //玩家阵营
        PathPoses=3904,                 //寻路的坐标集合
        CurrPathIndex=3905,                 //寻路的坐标点下标，无效时为-1
    }

    /// <summary>
    /// 属性修改方式
    /// </summary>
    public enum PropChangeType 
    {

        ChangeWithValue=1,                 //根据数值增减
        ChangeWithPercent=2,                 //根据百分比增减
        ChangeToValue=3,                 //修改为指定数值
        ChangeToPercent=4,                 //修改为指定百分比
    }

    /// <summary>
    /// 属性修改方式
    /// </summary>
    public enum SummonDeathCondition 
    {

        ChangeWithPercent=1,                 //根据百分比修改
        ChangeWithValue=2,                 //根据数值增减
        ChangeToValue=3,                 //修改为指定数值
    }

    /// <summary>
    /// 运动阶段结束条件
    /// </summary>
    public enum MotionEndCondition 
    {

        NormalEnd=0,                 //正常结束
        MoveEnd=1,                 //运动到终点时结束阶段
    }

    /// <summary>
    /// 层数显示类型
    /// </summary>
    public enum LayerShowType 
    {

        All=1,                 //实际层数
        Offset=2,                 //减去最小层数显示
    }

    /// <summary>
    /// 算法枚举
    /// </summary>
    public enum OPEnum 
    {

        Set=1,                 //设置值到Key
        Add=2,                 //增加值到Key
        Sub=3,                 //减少值到Key
    }

    /// <summary>
    /// 技能消耗
    /// </summary>
    public enum IsIgnoreTarget 
    {

        IgnoreTarget=0,                 //忽略模型半径
        CheckTargetWithStart=1,                 //不忽略模型半径(根据起始点选择最近的交点)
        CheckTargetWithTargetCenter=2,                 //不忽略模型半径(根据BOSS原点选择最近的交点)
    }

    /// <summary>
    /// 动作播放条件
    /// </summary>
    public enum AnimConditionType 
    {

        Move=1,                 //移动
    }

    /// <summary>
    /// 触发运行时类型
    /// </summary>
    public enum TriRuntimeType 
    {

        Other=0,                 //触发者的运行时
        Self=1,                 //触发器的运行时
    }

    /// <summary>
    /// 通用表现标签
    /// </summary>
    public enum GlobalShowType 
    {

        BUFF_SpecialBUFFUI=0,                 //特殊BUFFUI显示
        BUFF_ShaderChange=1,                 //修改Shader
        BUFF_ShadowFollow=2,                 //附影
        BUFF_ChangeAnim=3,                 //状态动作修改
        BUFF_Paralysis=4,                 //动作暂停
        BUFF_Knock=5,                 //击倒
        BUFF_Hide=6,                 //隐身
        BUFF_HideUI=7,                 //隐藏UI
        BUFF_ClickUseSkill=9,                 //点击屏幕释放技能
        BUFF_ChangeColorShader=10,                 //修改整体颜色Shader
        BUFF_PlayEffectLineRenderer=11,                 //与主人连线
        BUFF_SpTimeShow=12,                 //特殊时间展示
        BUFF_AvatarChange=13,                 //修改化身
        BUFF_SkillSlotHide=14,                 //技能槽隐藏
        BUFF_SpectralChange=15,                 //量谱显示方式修改
        BUFF_TransChange=16,                 //透明改变
        BUFF_JobSkillChange=101,                 //新增职业技能
        BUFF_JobSkillCancel=102,                 //基础职业技能失效
        BUFF_ChangeFaction=103,                 //改变阵营
        BUFF_AutoDestroy=104,                 //移动摇杆自动删除
        Skill_SkillSlotSpShow=201,                 //技能槽特殊表现
        Global_CameraMove=1001,                 //摄像机移动
        Global_SpectralSkill=1002,                 //量谱图切换
        Global_AddClientSummon=1003,                 //召唤客户端召唤物
    }

    /// <summary>
    /// 摄像机移动方式
    /// </summary>
    public enum CameraMoveType 
    {

        MoveWithCha=0,                 //根据角色朝向平移
        MoveWithVal=1,                 //根据绝对朝向平移
        Zoom=2,                 //放缩（0向前、180向后）
    }

    /// <summary>
    /// 技能槽特殊表现
    /// </summary>
    public enum EnumSkillSlotSpShow 
    {

        BlueRing=1,                 //蓝色环绕
    }

    /// <summary>
    /// UI标签
    /// </summary>
    public enum UILabel 
    {

        Hide=0,                 //全屏技能隐藏
    }

    /// <summary>
    /// 技能基础类型
    /// </summary>
    public enum SkillTag 
    {

        Attack=1,                 //普通攻击
        Skill=2,                 //普通技能
        Aided=3,                 //辅助技能
        Ultimate=4,                 //终极技能
        Sprint=5,                 //冲刺技能
        PetSkill=6,                 //宠物技能
        Cleanse=7,                 //净化技能
    }

    /// <summary>
    /// 技能表现类型
    /// </summary>
    public enum SkillType 
    {

    }

    /// <summary>
    /// 技能标签
    /// </summary>
    public enum SkillLabel 
    {

        SpectralSkill=1,                 //量谱消耗技能
        NormalAttack=2,                 //普攻
        MuYTreat=4000001,                 //牧夜治疗
        MuYAttack=4000002,                 //牧夜强效
        QuanSOneA=509901,                 //拳师耗1点A
        QuanSTwoA=509902,                 //拳师耗2点A
        QuanSThrA=509903,                 //拳师耗3点A
        QuanSA=509904,                 //拳师A技能
        QuanSOneB=509911,                 //拳师耗1点B
        QuanSTwoB=509912,                 //拳师耗2点B
        QuanSThrB=509913,                 //拳师耗3点B
        QuanSB=509914,                 //拳师B技能
    }

    /// <summary>
    /// 技能消耗
    /// </summary>
    public enum ConsumeType 
    {

        Con_Hp=1,                 //生命值
        Con_Mp=2,                 //法力值
    }

    /// <summary>
    /// Buff基础类型
    /// </summary>
    public enum BuffTag 
    {

        NoEffect=0,                 //无类型
        Vertigo=1,                 //眩晕
        Confusion=2,                 //混乱
        Decelerate=3,                 //减速
        Freeze=4,                 //冰冻
        Silent=5,                 //沉默
        Blinding=6,                 //致盲
        Fear=7,                 //恐惧
        Paralysis=8,                 //麻痹
    }

    /// <summary>
    /// Shader枚举
    /// </summary>
    public enum ShaderEnum 
    {

        Freeze=1,                 //冰冻
        RedPatches=2,                 //GVE红色侵蚀
        Drench=3,                 //浸湿
        Crazy=4,                 //狂暴
    }

    /// <summary>
    /// 特殊展示类型
    /// </summary>
    public enum SpTimeShowTypeEnum 
    {

        OverheadClock=1,                 //头顶时钟
        ProgressBelowHead=2,                 //头顶血条下方读条
        MuYTreat=11,                 //牧夜治疗
        MuYAttack=12,                 //牧夜强攻
    }

    /// <summary>
    /// Buff标签
    /// </summary>
    public enum BuffLabel 
    {

        Debuffs=1,                 //减益
        Bleed=2,                 //流血
        Shield=3,                 //护盾
        Decelerate=4,                 //减速
        Freeze=5,                 //冰冻
        KnockDown=6,                 //击倒
        ShadowFollow=7,                 //附影
        ChangeAnim=8,                 //状态动作修改
        invisible=9,                 //隐身
        Paralysis=10,                 //麻痹
        NoTimeDisplay=11,                 //不显示时间
        Drench=12,                 //浸湿
        DeadWithMap=10000,                 //地图让死就死BUFF 
    }

    /// <summary>
    /// BUFFUI显示位置标签
    /// </summary>
    public enum BUFFUIShowPosEnum 
    {

        HUDHpUp=1,                 //HUD血条上方 
        ThreeDHPUp=2,                 //3D血条上方 
        SecretBUFF=11,                 //秘境BUFF
    }

    /// <summary>
    /// 子弹标签
    /// </summary>
    public enum BulletLabel 
    {

        Shadow=1,                 //影子
        BackShadow=2,                 //返回影子
        LinStreatball=910011,                 //琳赛治疗球
        MuYTreatRing=4099011,                 //牧夜治疗圈
        MuYHurtRing=4099012,                 //牧夜伤害圈
        DeadWithMap=10000,                 //地图让死就死子弹
    }

    /// <summary>
    /// 效果标签
    /// </summary>
    public enum EffectLabel 
    {

        NoTriggerDamage=1,                 //不会触发伤害
        SpBullet1Skill=2,                 //特殊子弹1
        SpBullet2Skill=3,                 //特殊子弹2
        SpBullet3Skill=4,                 //特殊子弹3
        Backstab=5,                 //背刺
        NoBackstab=6,                 //不会背刺
        AddBackstabPoint=7,                 //增加背刺点
        Catch201=2011,                 //抓捕201
        DailyDungeonFireDamage=30561,                 //日常副本火焰伤害
        ShiRomRush=103062,                 //噬子弹伤害
        ShiLighting=103071,                 //噬吼叫阶段
        ShiLoopHit=103511,                 //噬持续伤害
        ZZHitAddBuff=110511,                 //中指击中施加BUFF
        ZZHitWithBuff=110511,                 //中指根据BUFF判断攻击
        LinSHitAddBullet=910011,                 //琳赛普攻概率召唤子弹
        LinSHitAddBuff=910211,                 //琳赛血球增加攻击力
        HuoLCreateBullet=922011,                 //霍莉召唤子弹
        HaSTCreateBullet=912011,                 //哈森特召唤子弹
        KnifeManAttack=2000011,                 //刺客普通攻击
        ShadowTime=200001,                 //刺客影子操作
        CiKBoomPassive=2099011,                 //刺客被动爆炸
        ShadowDamage=2002111,                 //刺客影子伤害
        ChargeShadowDamage=2002112,                 //刺客蓄影伤害
        ShadowMoreDamage=2004111,                 //刺客有影增伤
        ShadowToBall=2107011,                 //刺客附影换球
        ShadowToBoom=2108111,                 //刺客附影额外爆炸
        ShadowToLoopBoom=2109111,                 //刺客附影额外放波
        KnifeFly=2110111,                 //刺客发射匕首
        ShadowToShadow=2111211,                 //刺客附影额外放雾
        GunGirlAttack=3000011,                 //女枪普通攻击
        GunGirlBoom=3000001,                 //女枪技能引爆点燃
        CriticalHitElectricShock=3106211,                 //女枪暴击感电
        GunGirlBigBoom=3111111,                 //女枪大招引爆点燃
        BreakDADADA=3105011,                 //结束交叉射击
        ElectricShockAgain=3105211,                 //重复感电
        FireFire=3107011,                 //引爆易燃
        MuY_Treat=4000001,                 //牧夜治疗
        MuY_SpHit=4000002,                 //牧夜强效
        MuY_TreatTrigger=4000003,                 //牧夜治疗通用触发
        MuY_SpHitTrigger=4000004,                 //牧夜强效通用触发
        QuanSSkill4Charge=500401,                 //拳师技能四伤害
        QuanSSkill6Charge=500801,                 //拳师技能六伤害
        QuanSOneA=509901,                 //拳师耗1点A
    }


    public static class EnumDefineMap    
    {
        static public IEnumerable _boolean = new ValueDropdownList<Boolean>()
        {
            {"是",Boolean.TRUE},
            {"否",Boolean.FALSE},
        };

        static public IEnumerable _effformula = new ValueDropdownList<EffFormula>()
        {
            {"物理伤害公式",EffFormula.EF_Damage_Phy},
            {"大地伤害",EffFormula.EF_Damage_Earth},
            {"烈焰伤害",EffFormula.EF_Damage_Fire},
            {"自然伤害",EffFormula.EF_Damage_Natura},
            {"剧毒伤害",EffFormula.EF_Damage_Toxic},
            {"光明伤害",EffFormula.EF_Damage_Light},
            {"黑暗伤害",EffFormula.EF_Damage_Dark},
            {"真实伤害1",EffFormula.EF_Damage_TrueOne},
            {"真实伤害2",EffFormula.EF_Damage_TrueTwo},
            {"攻击力的百分比",EffFormula.EF_Damage_PerAttack},
            {"固定数值",EffFormula.EF_FixedValue},
        };

        static public IEnumerable _compoperator = new ValueDropdownList<CompOperator>()
        {
            {"等于",CompOperator.CO_Equal},
            {"不等于",CompOperator.CO_NotEqual},
            {"大于",CompOperator.CO_Greater},
            {"小于",CompOperator.CO_Less},
            {"大于等于",CompOperator.CO_EqualGreater},
            {"小于等于",CompOperator.CO_EqualLess},
            {"集合相同",CompOperator.CO_SET_SAME},
            {"集合包含",CompOperator.CO_SET_INCLUDE},
            {"集合不包含",CompOperator.CO_SET_UNINCLUDE},
            {"在arr1中找arr2的成员，只要找到一个就成功",CompOperator.CO_SET_HAVE},
        };

        static public IEnumerable _castmethodtype = new ValueDropdownList<CastMethodType>()
        {
            {"按下释放，松开无事",CastMethodType.DirectCast},
            {"按下无事，松开释放",CastMethodType.DirectCastOnLoosen},
            {"按下选择，松开释放",CastMethodType.WheelCast},
            {"蓄力释放，蓄力期间有轮盘",CastMethodType.GatherWheelCast},
            {"蓄力释放，蓄力期间无轮盘",CastMethodType.GatherWheelCastNoIndicator},
        };

        static public IEnumerable _animstate = new ValueDropdownList<AnimState>()
        {
            {"休闲待机",AnimState.Idle},
            {"战斗待机",AnimState.BattleIdle},
            {"漫步移动",AnimState.WanderMoving},
            {"休闲移动",AnimState.SingleMoving},
            {"战斗移动",AnimState.BattleMoving},
            {"受击",AnimState.Hurt},
            {"死亡",AnimState.Deading},
            {"待机收刀",AnimState.WeaponRetractionIdle},
            {"移动收刀",AnimState.WeaponRetractionMoving},
        };

        static public IEnumerable _skillinputtype = new ValueDropdownList<SkillInputType>()
        {
            {"朝向输入",SkillInputType.DirInput},
            {"坐标输入",SkillInputType.PosInput},
            {"对象输入",SkillInputType.ObjInput},
        };

        static public IEnumerable _speceffectscale = new ValueDropdownList<SpecEffectScale>()
        {
            {"永不跟随",SpecEffectScale.Origin},
            {"创建跟随",SpecEffectScale.NoFollow},
            {"永远跟随",SpecEffectScale.Follow},
        };

        static public IEnumerable _bulletheighttype = new ValueDropdownList<BulletHeightType>()
        {
            {"出生跟随主角",BulletHeightType.BornWithRole},
            {"永远跟随主角",BulletHeightType.FollowRole},
            {"出生紧贴地面",BulletHeightType.BornCheckSpace},
        };

        static public IEnumerable _conditiontype = new ValueDropdownList<ConditionType>()
        {
            {"BUFF层数",ConditionType.Cond_Buff},
            {"属性值",ConditionType.Cond_Attr},
            {"前置技能时间",ConditionType.Cond_SkillTime},
            {"血量万分比",ConditionType.Cond_HpPercent},
            {"末位量谱值",ConditionType.Condition_CurSpectralElem},
            {"敌人阵营",ConditionType.Cond_TargetFaction},
        };

        static public IEnumerable _dynamicrangetype = new ValueDropdownList<DynamicRangeType>()
        {
            {"无动态增长",DynamicRangeType.NoChange},
            {"根据蓄力时间",DynamicRangeType.ChargeTime},
            {"根据蓄力层数",DynamicRangeType.ChargeNum},
        };

        static public IEnumerable _battlestate = new ValueDropdownList<BattleState>()
        {
            {"禁止主动移动",BattleState.ForbidMove},
            {"禁止转向",BattleState.ForbidDir},
            {"禁止被伤害",BattleState.ForbidHurt},
            {"禁止被治疗",BattleState.ForbidCure},
            {"禁止普攻",BattleState.ForbidAttack},
            {"禁止技能",BattleState.ForbidSkill},
            {"禁止位移",BattleState.ForbidDisplacement},
            {"禁止被选择器选中",BattleState.ForbidSelect},
            {"禁止打断技能",BattleState.ForbidBreakSkill},
        };

        static public IEnumerable _shape = new ValueDropdownList<Shape>()
        {
            {"空",Shape.None},
            {"圆形",Shape.Round},
            {"空心圆",Shape.HollowCircle},
            {"扇形",Shape.Sector},
            {"环扇形",Shape.RingFan},
            {"矩形",Shape.Rect},
            {"朝向路径",Shape.RotRoute},
            {"技能选取目标",Shape.InputTarget},
            {"箭头",Shape.Arrow},
            {"点位路径",Shape.PosRoute},
        };

        static public IEnumerable _selecttype = new ValueDropdownList<SelectType>()
        {
            {"友方",SelectType.Friend},
            {"敌方",SelectType.Enemy},
            {"全体",SelectType.All},
            {"敌方玩家",SelectType.FriendPlayer},
            {"友方玩家",SelectType.EnemyPlayer},
            {"全体玩家",SelectType.AllPlayer},
            {"敌方子弹",SelectType.FriendBullet},
            {"友方子弹",SelectType.EnemyBullet},
            {"全体子弹",SelectType.AllBullet},
            {"友方队友",SelectType.FriendlyTeam},
        };

        static public IEnumerable _targettype = new ValueDropdownList<TargetType>()
        {
            {"最近的敌人",TargetType.RecentPerson},
            {"最远的敌人",TargetType.FarPerson},
        };

        static public IEnumerable _linetypeenum = new ValueDropdownList<LineTypeEnum>()
        {
            {"平铺",LineTypeEnum.Tiling},
            {"拉伸",LineTypeEnum.Stretching},
        };

        static public IEnumerable _linktypeenum = new ValueDropdownList<LinkTypeEnum>()
        {
            {"连续",LinkTypeEnum.Sequence},
            {"分发",LinkTypeEnum.Distribution},
            {"不带主人连续",LinkTypeEnum.NoFromSequence},
        };

        static public IEnumerable _effecttype = new ValueDropdownList<EffectType>()
        {
            {"空效果",EffectType.Empty},
            {"碰撞盒",EffectType.CollisionBox},
            {"目标集合的操作",EffectType.TarGroup},
            {"子弹碰撞盒",EffectType.CollisionBoxBullet},
            {"选择特殊目标",EffectType.SpSelectTarget},
            {"治疗",EffectType.Treat},
            {"伤害",EffectType.Damage},
            {"根据坐标位移",EffectType.MoveWithPos},
            {"根据朝向位移",EffectType.MoveWithRot},
            {"黑洞效果",EffectType.Register},
            {"修改CD",EffectType.ChangeCD},
            {"打断目标当前技能",EffectType.BreakCurRuntime},
            {"修改属性",EffectType.ChangeProp},
            {"修改朝向",EffectType.ChangeToward},
            {"修改蓝量",EffectType.ChangeMana},
            {"施加BUFF",EffectType.AddBuff},
            {"移除BUFF",EffectType.RemoveBuff},
            {"施加被动",EffectType.AddPassive},
            {"修改量谱值",EffectType.ChangeSpectral},
            {"昧光增加影能量",EffectType.SpChangeSpectral},
            {"特殊增加量谱值",EffectType.AppendValueToSpectral},
            {"特殊移除量谱值",EffectType.RemoveValueFromSpectral},
            {"特殊判断量谱值",EffectType.SpectralStackCheck},
            {"创建子弹",EffectType.CreateBullet},
            {"销毁子弹",EffectType.DestoryBullet},
            {"销毁子弹自身",EffectType.DestoryBulletOwner},
            {"抛出子弹",EffectType.ThrowBullet},
            {"发射子弹",EffectType.LaunchBullet},
            {"贝塞尔子弹",EffectType.BezierBullet},
            {"用户输入",EffectType.UserInput},
            {"蓄力输入",EffectType.ChargeInput},
            {"打断指定类型阶段",EffectType.BreakCurRuntimeInBullet},
            {"触发自定义阶段",EffectType.TriggerStage},
            {"召唤物",EffectType.SummonMonster},
            {"随机多个点位",EffectType.RandomPoint},
            {"设置子弹目标点",EffectType.SetBulletTargetPos},
            {"随机单个点位",EffectType.SingleRandomPoint},
            {"计算垂足点",EffectType.GetFootPoint},
            {"计算对称点",EffectType.GetMirrorPoint},
            {"随BUFF时间计算点位",EffectType.GetPointWithBuffTime},
            {"设置子弹移动速率",EffectType.ChangeBulletSpeed},
            {"从A指向B计算可达点",EffectType.GetPointWithAToB},
            {"设置指定CDKey数值",EffectType.SetCDKeyNum},
            {"当前技能进入CD",EffectType.ThisSkillEnterCD},
            {"判断目标Key是否不为空",EffectType.IsNotEmpty},
            {"对目标Key做命中处理",EffectType.SelectHitFromKey},
            {"设置int值到目标Key",EffectType.SetIntKey},
            {"检查目标Key的int值",EffectType.CheckIntKey},
            {"重设BUFF剩余持续时间",EffectType.SetBuffTime},
            {"获得两个点位间距离的平方",EffectType.GetSqualDis2Pos},
            {"减去BUFF层数",EffectType.SubBUFFLayer},
            {"检查职业ID",EffectType.CheckJobID},
            {"检查目标怪物类型",EffectType.CheckTargetMonsterType},
            {"取消BUFF添加",EffectType.BuffAddCancel},
            {"二次伤害",EffectType.DamageSecond},
            {"改变伤害",EffectType.DamageChange},
            {"增加护盾值",EffectType.AddShields},
            {"减少护盾值",EffectType.DecShields},
            {"取消移动",EffectType.CancelOffest},
            {"取消转向",EffectType.CancelRota},
            {"护盾抵挡",EffectType.ShieldEffect},
            {"转职消息",EffectType.ChangeJobMassage},
            {"场景消息",EffectType.SendMapMassage},
            {"在点位播放特效",EffectType.PlayEffectAtPoint},
            {"在目标播放特效",EffectType.PlayEffectAtTarget},
            {"隐身",EffectType.Stealth},
            {"在两点间播放特效",EffectType.PlayEffectBetweenPoints},
            {"连线",EffectType.PlayEffectLineRenderer},
            {"客户端召唤物播放动作",EffectType.ClientSummonAnim},
            {"客户端召唤物播放特效",EffectType.ClientSummonEffect},
            {"客户端召唤物朝向目标",EffectType.ClientSummonTurnTo},
            {"客户端召唤物移除",EffectType.ClientSummonRemove},
            {"绝对数值产生器",EffectType.SetValueToKey},
            {"组数值产生器",EffectType.GetGroupIDToKey},
            {"随机数值产生器",EffectType.RamdonValueToKey},
            {"集合成员数值产生器",EffectType.GetLengthFromListToKey},
            {"数值运算产生器",EffectType.OperationValueToKey},
            {"目标属性数值产生器",EffectType.GetTargetPropToKey},
            {"冷却时间数值产生器",EffectType.GetCDKeyCDToKey},
            {"配置ID数值产生器",EffectType.GetConfigIDToKey},
            {"单位朝向产生器",EffectType.GetTowardFromEntityToKey},
            {"点位朝向产生器",EffectType.GetTowardFromPosToKey},
            {"1是否在2的角度范围内检查器",EffectType.CheckToward},
            {"目标是否有指定被动检查器",EffectType.CheckPassive},
            {"目标集合修改至绝对朝向",EffectType.ChangeToAbsoluteToward},
            {"目标集合量谱值修改",EffectType.ChangeToInputSpectral},
            {"目标集合冷却时间修改",EffectType.ChangeToCDKeyCD},
            {"创建交互物",EffectType.CreateInteract},
        };
        public static Dictionary<EffectType, System.Type> EffectTypeDic = new Dictionary<EffectType, System.Type>()
        {
            {EffectType.Empty,typeof(EffectTypeEmpty)},
            {EffectType.CollisionBox,typeof(EffectTypeCollisionBox)},
            {EffectType.TarGroup,typeof(EffectTypeTarGroup)},
            {EffectType.CollisionBoxBullet,typeof(EffectTypeCollisionBoxBullet)},
            {EffectType.SpSelectTarget,typeof(EffectTypeSpSelectTarget)},
            {EffectType.Treat,typeof(EffectTypeTreat)},
            {EffectType.Damage,typeof(EffectTypeDamage)},
            {EffectType.MoveWithPos,typeof(EffectTypeMoveWithPos)},
            {EffectType.MoveWithRot,typeof(EffectTypeMoveWithRot)},
            {EffectType.Register,typeof(EffectTypeRegister)},
            {EffectType.ChangeCD,typeof(EffectTypeChangeCD)},
            {EffectType.BreakCurRuntime,typeof(EffectTypeBreakCurRuntime)},
            {EffectType.ChangeProp,typeof(EffectTypeChangeProp)},
            {EffectType.ChangeToward,typeof(EffectTypeChangeToward)},
            {EffectType.ChangeMana,typeof(EffectTypeChangeMana)},
            {EffectType.AddBuff,typeof(EffectTypeAddBuff)},
            {EffectType.RemoveBuff,typeof(EffectTypeRemoveBuff)},
            {EffectType.AddPassive,typeof(EffectTypeAddPassive)},
            {EffectType.ChangeSpectral,typeof(EffectTypeChangeSpectral)},
            {EffectType.SpChangeSpectral,typeof(EffectTypeSpChangeSpectral)},
            {EffectType.AppendValueToSpectral,typeof(EffectTypeAppendValueToSpectral)},
            {EffectType.RemoveValueFromSpectral,typeof(EffectTypeRemoveValueFromSpectral)},
            {EffectType.SpectralStackCheck,typeof(EffectTypeSpectralStackCheck)},
            {EffectType.CreateBullet,typeof(EffectTypeCreateBullet)},
            {EffectType.DestoryBullet,typeof(EffectTypeDestoryBullet)},
            {EffectType.DestoryBulletOwner,typeof(EffectTypeDestoryBulletOwner)},
            {EffectType.ThrowBullet,typeof(EffectTypeThrowBullet)},
            {EffectType.LaunchBullet,typeof(EffectTypeLaunchBullet)},
            {EffectType.BezierBullet,typeof(EffectTypeBezierBullet)},
            {EffectType.UserInput,typeof(EffectTypeUserInput)},
            {EffectType.ChargeInput,typeof(EffectTypeChargeInput)},
            {EffectType.BreakCurRuntimeInBullet,typeof(EffectTypeBreakCurRuntimeInBullet)},
            {EffectType.TriggerStage,typeof(EffectTypeTriggerStage)},
            {EffectType.SummonMonster,typeof(EffectTypeSummonMonster)},
            {EffectType.RandomPoint,typeof(EffectTypeRandomPoint)},
            {EffectType.SetBulletTargetPos,typeof(EffectTypeSetBulletTargetPos)},
            {EffectType.SingleRandomPoint,typeof(EffectTypeSingleRandomPoint)},
            {EffectType.GetFootPoint,typeof(EffectTypeGetFootPoint)},
            {EffectType.GetMirrorPoint,typeof(EffectTypeGetMirrorPoint)},
            {EffectType.GetPointWithBuffTime,typeof(EffectTypeGetPointWithBuffTime)},
            {EffectType.ChangeBulletSpeed,typeof(EffectTypeChangeBulletSpeed)},
            {EffectType.GetPointWithAToB,typeof(EffectTypeGetPointWithAToB)},
            {EffectType.SetCDKeyNum,typeof(EffectTypeSetCDKeyNum)},
            {EffectType.ThisSkillEnterCD,typeof(EffectTypeThisSkillEnterCD)},
            {EffectType.IsNotEmpty,typeof(EffectTypeIsNotEmpty)},
            {EffectType.SelectHitFromKey,typeof(EffectTypeSelectHitFromKey)},
            {EffectType.SetIntKey,typeof(EffectTypeSetIntKey)},
            {EffectType.CheckIntKey,typeof(EffectTypeCheckIntKey)},
            {EffectType.SetBuffTime,typeof(EffectTypeSetBuffTime)},
            {EffectType.GetSqualDis2Pos,typeof(EffectTypeGetSqualDis2Pos)},
            {EffectType.SubBUFFLayer,typeof(EffectTypeSubBUFFLayer)},
            {EffectType.CheckJobID,typeof(EffectTypeCheckJobID)},
            {EffectType.CheckTargetMonsterType,typeof(EffectTypeCheckTargetMonsterType)},
            {EffectType.BuffAddCancel,typeof(EffectTypeBuffAddCancel)},
            {EffectType.DamageSecond,typeof(EffectTypeDamageSecond)},
            {EffectType.DamageChange,typeof(EffectTypeDamageChange)},
            {EffectType.AddShields,typeof(EffectTypeAddShields)},
            {EffectType.DecShields,typeof(EffectTypeDecShields)},
            {EffectType.CancelOffest,typeof(EffectTypeCancelOffest)},
            {EffectType.CancelRota,typeof(EffectTypeCancelRota)},
            {EffectType.ShieldEffect,typeof(EffectTypeShieldEffect)},
            {EffectType.ChangeJobMassage,typeof(EffectTypeChangeJobMassage)},
            {EffectType.SendMapMassage,typeof(EffectTypeSendMapMassage)},
            {EffectType.PlayEffectAtPoint,typeof(EffectTypePlayEffectAtPoint)},
            {EffectType.PlayEffectAtTarget,typeof(EffectTypePlayEffectAtTarget)},
            {EffectType.Stealth,typeof(EffectTypeStealth)},
            {EffectType.PlayEffectBetweenPoints,typeof(EffectTypePlayEffectBetweenPoints)},
            {EffectType.PlayEffectLineRenderer,typeof(EffectTypePlayEffectLineRenderer)},
            {EffectType.ClientSummonAnim,typeof(EffectTypeClientSummonAnim)},
            {EffectType.ClientSummonEffect,typeof(EffectTypeClientSummonEffect)},
            {EffectType.ClientSummonTurnTo,typeof(EffectTypeClientSummonTurnTo)},
            {EffectType.ClientSummonRemove,typeof(EffectTypeClientSummonRemove)},
            {EffectType.SetValueToKey,typeof(EffectTypeSetValueToKey)},
            {EffectType.GetGroupIDToKey,typeof(EffectTypeGetGroupIDToKey)},
            {EffectType.RamdonValueToKey,typeof(EffectTypeRamdonValueToKey)},
            {EffectType.GetLengthFromListToKey,typeof(EffectTypeGetLengthFromListToKey)},
            {EffectType.OperationValueToKey,typeof(EffectTypeOperationValueToKey)},
            {EffectType.GetTargetPropToKey,typeof(EffectTypeGetTargetPropToKey)},
            {EffectType.GetCDKeyCDToKey,typeof(EffectTypeGetCDKeyCDToKey)},
            {EffectType.GetConfigIDToKey,typeof(EffectTypeGetConfigIDToKey)},
            {EffectType.GetTowardFromEntityToKey,typeof(EffectTypeGetTowardFromEntityToKey)},
            {EffectType.GetTowardFromPosToKey,typeof(EffectTypeGetTowardFromPosToKey)},
            {EffectType.CheckToward,typeof(EffectTypeCheckToward)},
            {EffectType.CheckPassive,typeof(EffectTypeCheckPassive)},
            {EffectType.ChangeToAbsoluteToward,typeof(EffectTypeChangeToAbsoluteToward)},
            {EffectType.ChangeToInputSpectral,typeof(EffectTypeChangeToInputSpectral)},
            {EffectType.ChangeToCDKeyCD,typeof(EffectTypeChangeToCDKeyCD)},
            {EffectType.CreateInteract,typeof(EffectTypeCreateInteract)}
        };
        static public IEnumerable _source = new ValueDropdownList<Source>()
        {
            {"输入点位",Source.InputPos},
            {"输入目标",Source.Target},
        };

        static public IEnumerable _effectcondition = new ValueDropdownList<EffectCondition>()
        {
            {"伤害",EffectCondition.Damage},
            {"治疗",EffectCondition.Treat},
            {"命中",EffectCondition.Hit},
            {"随机",EffectCondition.Range},
            {"闪避",EffectCondition.Dodge},
            {"暴击",EffectCondition.Critical},
            {"BUFFID",EffectCondition.BUFFID},
            {"BUFFtag",EffectCondition.BUFFTag},
            {"目标血量",EffectCondition.TargetLife},
            {"目标蓝量",EffectCondition.TargetMana},
            {"命中人数",EffectCondition.TargetNum},
            {"目标BUFFTID",EffectCondition.TargetBUFFID},
            {"目标BUFFTAG",EffectCondition.TargetBUFFTAG},
            {"目标BUFFF层数",EffectCondition.TargetBUFFNum},
        };

        static public IEnumerable _checkstate = new ValueDropdownList<CheckState>()
        {
            {"攻击前",CheckState.Stage_AttackPro},
            {"攻击后",CheckState.Stage_Attacked},
            {"受伤前",CheckState.Stage_OnHurtPro},
            {"受伤后",CheckState.Stage_OnHurted},
            {"杀死目标前",CheckState.Stage_KillPro},
            {"杀死目标后",CheckState.Stage_Killed},
            {"被杀前",CheckState.Stage_OnDeadPro},
            {"被杀后",CheckState.Stage_OnDeaded},
            {"buff添加前",CheckState.Stage_OnBuffAddPro},
            {"buff添加后",CheckState.Stage_OnBuffAdded},
            {"命中后",CheckState.Stage_OnHited},
            {"闪避前",CheckState.Stage_OndodgePro},
            {"治疗前",CheckState.Stage_CurePro},
            {"治疗后",CheckState.Stage_Cured},
            {"被治疗前",CheckState.Stage_OnCurePro},
            {"被治疗后",CheckState.Stage_OnCured},
            {"移除buff后",CheckState.Stage_RemoveBuffed},
            {"被移除buff后",CheckState.Stage_OnBuffRemoveed},
            {"被位移前",CheckState.Stage_OnOffsetPro},
            {"被位移后",CheckState.Stage_OnOffseted},
            {"位移前",CheckState.Stage_OffsetPro},
            {"位移后",CheckState.Stage_Offseted},
            {"量谱变化后",CheckState.Stage_SpectralChanged},
        };

        static public IEnumerable _movelabel = new ValueDropdownList<MoveLabel>()
        {
            {"无特效",MoveLabel.Normal},
            {"残影",MoveLabel.Sprint},
            {"隐身",MoveLabel.Flash},
        };

        static public IEnumerable _movetype = new ValueDropdownList<MoveType>()
        {
            {"取消位移",MoveType.DefaultOffset},
            {"冲撞(都不穿)",MoveType.Dash},
            {"冲锋(不穿墙)",MoveType.Rush},
            {"闪现(都穿)",MoveType.Flash},
            {"击飞",MoveType.BeFly},
            {"击退",MoveType.BeRush},
            {"被移动",MoveType.BeMove},
        };

        static public IEnumerable _monstertype = new ValueDropdownList<MonsterType>()
        {
            {"小怪",MonsterType.Lackey},
            {"精英",MonsterType.Elite},
            {"BOSS",MonsterType.BOSS},
        };

        static public IEnumerable _damagetype = new ValueDropdownList<DamageType>()
        {
            {"元素伤害1",DamageType.El1},
            {"元素伤害2",DamageType.El2},
            {"元素伤害3",DamageType.El3},
        };

        static public IEnumerable _cdchangetype = new ValueDropdownList<CDChangeType>()
        {
            {"按照绝对值修改技能剩余cd",CDChangeType.ChangeWithNum},
            {"按照百分比修改技能剩余cd",CDChangeType.ChangeWithPer},
            {"设置技能模块技能总cd时间",CDChangeType.ChangeAllWithNum},
            {"按照百分比设置技能模块技能总cd时间",CDChangeType.ChangeAlliWithPer},
        };

        static public IEnumerable _calcutype = new ValueDropdownList<CalcuType>()
        {
            {"向坐标点方向移动",CalcuType.ChangeWithPer},
            {"角度移动",CalcuType.ChangeAllWithNum},
        };

        static public IEnumerable _targroupopertype = new ValueDropdownList<TarGroupOperType>()
        {
            {"返回集合1不在集合2里的成员",TarGroupOperType.TarGroupOperType_1},
            {"返回集合1在集合2里的成员",TarGroupOperType.TarGroupOperType_2},
            {"返回所有成员",TarGroupOperType.TarGroupOperType_3},
            {"返回不重合的成员",TarGroupOperType.TarGroupOperType_4},
        };

        static public IEnumerable _interruptevent = new ValueDropdownList<InterruptEvent>()
        {
            {"结束阶段",InterruptEvent.KillStage},
            {"结束运行时",InterruptEvent.KillRunTime},
        };

        static public IEnumerable _hangpoint = new ValueDropdownList<HangPoint>()
        {
            {"根节点",HangPoint.Root},
            {"胸部受击",HangPoint.Hurt_D},
            {"后背部武器",HangPoint.BackWeapon_D},
            {"后背部翅膀",HangPoint.Wing_D},
            {"左手武器手部",HangPoint.HandWeapon_D_L},
            {"右手武器手部",HangPoint.HandWeapon_D_R},
            {"左手武器根部",HangPoint.WeaponRoot_D_L},
            {"右手武器根部",HangPoint.WeaponRoot_D_R},
            {"右手或者双手武器特效",HangPoint.WeaponHurt_D_R},
            {"左手武器特效",HangPoint.WeaponHurt_D_L},
            {"头顶特效",HangPoint.Top_D},
            {"右脚脚底特效",HangPoint.Foot_D_R},
            {"左脚脚底特效",HangPoint.Foot_D_L},
            {"头顶信息（气泡，公会，名字等）",HangPoint.UnitInfo_D},
            {"脚下特效节点",HangPoint.Root_D},
            {"尾巴骨挂点",HangPoint.Bip001},
            {"嘴巴挂点",HangPoint.Mouth_D},
            {"嘴巴2挂点",HangPoint.Mouth_D02},
            {"嘴巴3挂点",HangPoint.Mouth_D03},
        };

        static public IEnumerable _effecttypewaitinputrecordtype = new ValueDropdownList<EffectTypeWaitInputRecordType>()
        {
            {"记录时间",EffectTypeWaitInputRecordType.RecordTime},
            {"记录层数",EffectTypeWaitInputRecordType.RecordLayer},
        };

        static public IEnumerable _targetkey = new ValueDropdownList<TargetKey>()
        {
            {"默认Key",TargetKey.TransObject},
            {"默认坐标",TargetKey.TransPos},
            {"输入的坐标",TargetKey.InputCoord},
            {"输入的朝向",TargetKey.InputRota},
            {"输入的目标",TargetKey.InputTarget},
            {"蓄力",TargetKey.BBEnergy},
            {"蓄力时间",TargetKey.BBEnergyTime},
            {"施法者",TargetKey.Builder},
            {"拥有者",TargetKey.Owner},
            {"受击者",TargetKey.Victim},
            {"存活时间",TargetKey.LiveTime},
            {"BUFF层数",TargetKey.StackCount},
            {"护盾值",TargetKey.ShieldVal},
        };

        static public IEnumerable _transtargetishit = new ValueDropdownList<TransTargetIsHit>()
        {
            {"选择命中单位",TransTargetIsHit.IsHit},
            {"选择未命中单位",TransTargetIsHit.IsMiss},
            {"选择所有单位",TransTargetIsHit.All},
        };

        static public IEnumerable _spectral = new ValueDropdownList<Spectral>()
        {
            {"量谱值1",Spectral.Spectral1},
            {"量谱值2",Spectral.Spectral2},
            {"量谱值3",Spectral.Spectral3},
        };

        static public IEnumerable _inputtype = new ValueDropdownList<InputType>()
        {
            {"立即输入",InputType.Immediately},
            {"预输入",InputType.Prepare},
        };

        static public IEnumerable _inputmode = new ValueDropdownList<InputMode>()
        {
            {"长按",InputMode.Press},
            {"单击",InputMode.Tap},
        };

        static public IEnumerable _stageevent = new ValueDropdownList<StageEvent>()
        {
            {"什么都不做",StageEvent.None},
            {"Tick结束跳转",StageEvent.JumpStage1},
            {"立即跳转",StageEvent.JumpStage2},
            {"结束技能",StageEvent.KillSkill},
            {"结束循环",StageEvent.KillStage},
            {"跳转下次循环",StageEvent.ReSet},
            {"结束当前阶段跳转下次循环",StageEvent.EndStageReSet},
        };

        static public IEnumerable _builderorpos = new ValueDropdownList<BuilderOrPos>()
        {
            {"自身",BuilderOrPos.Builder},
            {"坐标",BuilderOrPos.Pos},
        };

        static public IEnumerable _triggertargettype = new ValueDropdownList<TriggerTargetType>()
        {
            {"拥有者",TriggerTargetType.Onwer},
            {"施法者",TriggerTargetType.Builder},
            {"受击者",TriggerTargetType.Victim},
        };

        static public IEnumerable _stagetype = new ValueDropdownList<StageType>()
        {
            {"一般阶段",StageType.NormalStage},
            {"触发阶段",StageType.TriggerStage},
            {"Buff添加阶段",StageType.AddBuffStage},
            {"Buff结束阶段",StageType.EndBuffStage},
            {"子弹移动阶段",StageType.BulletStage},
            {"被动结束阶段",StageType.EndPassiveStage},
        };

        static public IEnumerable _triggerstageenum = new ValueDropdownList<TriggerStageEnum>()
        {
            {"攻击前",TriggerStageEnum.Stage_AttackPro},
            {"攻击后",TriggerStageEnum.Stage_Attacked},
            {"受伤害前",TriggerStageEnum.Stage_OnHurtPro},
            {"受伤害后",TriggerStageEnum.Stage_OnHurted},
            {"杀死目标前",TriggerStageEnum.Stage_KillPro},
            {"杀死目标后",TriggerStageEnum.Stage_Killed},
            {"死亡前",TriggerStageEnum.Stage_OnDeadPro},
            {"死亡后",TriggerStageEnum.Stage_OnDeaded},
            {"buff被添加前",TriggerStageEnum.Stage_OnBuffAddProed},
            {"buff被添加后",TriggerStageEnum.Stage_OnBuffAdded},
            {"命中后",TriggerStageEnum.Stage_OnHited},
            {"闪避前",TriggerStageEnum.Stage_OndodgePro},
            {"治疗前",TriggerStageEnum.Stage_CurePro},
            {"治疗后",TriggerStageEnum.Stage_Cured},
            {"被治疗前",TriggerStageEnum.Stage_OnCurePro},
            {"被治疗后",TriggerStageEnum.Stage_OnCured},
            {"移除buff后",TriggerStageEnum.Stage_RemoveBuffed},
            {"buff被移除后",TriggerStageEnum.Stage_OnBuffRemoveed},
            {"位移前",TriggerStageEnum.Stage_OnOffsetPro},
            {"位移后",TriggerStageEnum.Stage_OnOffseted},
            {"施加位移前",TriggerStageEnum.Stage_OffsetPro},
            {"施加位移后",TriggerStageEnum.Stage_Offseted},
            {"量谱变化之后",TriggerStageEnum.Stage_SpectralChanged},
            {"移动到目标点时",TriggerStageEnum.Stage_AtTargetPos},
            {"任意技能运行时启动时",TriggerStageEnum.Stage_SkillStart},
            {"任意技能首次选择到目标后",TriggerStageEnum.Stage_SkillFirstSelect},
            {"自定义触发阶段",TriggerStageEnum.Stage_Customize1},
            {"完全伤害结束阶段",TriggerStageEnum.Stage_HurtEffected},
            {"转向前",TriggerStageEnum.Stage_OnRotaPro},
            {"转向后",TriggerStageEnum.Stage_OnRotaed},
            {"施加转向前",TriggerStageEnum.Stage_RotaPro},
            {"施加转向后",TriggerStageEnum.Stage_Rotaed},
            {"添加buff前",TriggerStageEnum.Stage_OnBuffAddPro},
            {"添加buff后",TriggerStageEnum.Stage_OnBuffAdd},
            {"完全治疗结束阶段",TriggerStageEnum.Stage_CureEffected},
        };

        static public IEnumerable _triggertypeenum = new ValueDropdownList<TriggerTypeEnum>()
        {
            {"临时伤害数值",TriggerTypeEnum.DataHurt},
            {"临时治疗数值",TriggerTypeEnum.DataCure},
            {"临时命中结果",TriggerTypeEnum.DataHit},
            {"临时随机数值",TriggerTypeEnum.Random},
            {"临时闪避结果",TriggerTypeEnum.DodgeAttack},
            {"临时暴击结果",TriggerTypeEnum.CriticalHit},
            {"临时数据是buff运行时，判断id",TriggerTypeEnum.DataBuffID},
            {"临时数据是buff运行时，判断tag",TriggerTypeEnum.DataBuffTag},
            {"临时数据是位移节点，判断位移状态",TriggerTypeEnum.DataOffset},
            {"临时数据是buff运行时，判断层数",TriggerTypeEnum.DataBuffCount},
            {"对象当前生命值百分比",TriggerTypeEnum.RemainBlood},
            {"对象当前蓝量百分比",TriggerTypeEnum.RemainManaPer},
            {"对象当前蓝量绝对值",TriggerTypeEnum.RemainMana},
            {"对象对应属性id的属性值",TriggerTypeEnum.PropValue},
            {"伤害节点-命中集合人数",TriggerTypeEnum.BlackHit},
            {"伤害节点-包含伤害类型",TriggerTypeEnum.DamageType},
            {"当前效果id",TriggerTypeEnum.CurrEffectID},
            {"当前效果类型",TriggerTypeEnum.CurrEffectType},
            {"当前效果标签",TriggerTypeEnum.CurrEffectTag},
            {"是否触发背刺",TriggerTypeEnum.Backstab},
            {"检测当前运行时黑板上的数值类节点数据",TriggerTypeEnum.IntNode},
            {"当前技能标签",TriggerTypeEnum.SkillTag},
            {"调用者与运行时对象关系",TriggerTypeEnum.RuntimeRelationship},
            {"对象的buffid",TriggerTypeEnum.BuffID},
            {"对象的bufftag",TriggerTypeEnum.BuffTag},
            {"对象的指定buff层数",TriggerTypeEnum.BuffFloor},
            {"对象的技能冷却",TriggerTypeEnum.CoolTime},
            {"对象的原子状态",TriggerTypeEnum.State},
            {"对象的阶段ID",TriggerTypeEnum.Stage},
            {"Owner是否在战斗状态",TriggerTypeEnum.Battle},
            {"检查目标怪物类型",TriggerTypeEnum.CheckTargetMonsterType},
            {"是否能获得利箭点",TriggerTypeEnum.CanGetSAPoint},
        };

        static public IEnumerable _buffreplace = new ValueDropdownList<BuffReplace>()
        {
            {"时间累加",BuffReplace.AddTime},
            {"刷新",BuffReplace.Refresh},
            {"层数和时间累加",BuffReplace.AddLayer},
            {"互不影响",BuffReplace.NoEffect},
            {"旧的不去新的不来",BuffReplace.Only},
            {"层数叠加",BuffReplace.JustLayer},
            {"仅刷新时间",BuffReplace.TimeRefresh},
        };

        static public IEnumerable _buffconfront = new ValueDropdownList<BuffConfront>()
        {
            {"无类型",BuffConfront.NoEffect},
            {"眩晕",BuffConfront.Vertigo},
            {"混乱",BuffConfront.Confusion},
            {"减速",BuffConfront.Decelerate},
            {"冰冻",BuffConfront.Freeze},
            {"沉默",BuffConfront.Silent},
            {"致盲",BuffConfront.Blinding},
            {"恐惧",BuffConfront.Fear},
            {"麻痹",BuffConfront.Paralysis},
        };

        static public IEnumerable _battlepropenum = new ValueDropdownList<BattlePropEnum>()
        {
            {"攻击",BattlePropEnum.Atk},
            {"防御",BattlePropEnum.Defence},
            {"魔抗",BattlePropEnum.MDefence},
            {"生命",BattlePropEnum.Hp},
            {"法力",BattlePropEnum.Mp},
            {"生命恢复值",BattlePropEnum.RecHp},
            {"法力恢复值",BattlePropEnum.RecMp},
            {"万分比攻击加成",BattlePropEnum.AtkRate},
            {"万分比防御加成",BattlePropEnum.DefRate},
            {"万分比魔抗加成",BattlePropEnum.MDefRate},
            {"万分比生命加成",BattlePropEnum.HpRate},
            {"命中等级",BattlePropEnum.HitLv},
            {"闪避等级",BattlePropEnum.DodgeLv},
            {"暴击等级",BattlePropEnum.CriLv},
            {"抗暴击等级",BattlePropEnum.CriDefLv},
            {"爆伤等级",BattlePropEnum.CriDamLv},
            {"额外命中率",BattlePropEnum.ExHit},
            {"额外闪避率",BattlePropEnum.ExDodge},
            {"额外暴击率",BattlePropEnum.ExCri},
            {"额外抗暴击率",BattlePropEnum.ExDefCri},
            {"额外爆伤率",BattlePropEnum.ExCriDam},
            {"破甲",BattlePropEnum.Pierce},
            {"百分比破甲",BattlePropEnum.PierceRate},
            {"法穿",BattlePropEnum.MPierce},
            {"百分比法穿",BattlePropEnum.MPierceRate},
            {"移动速度",BattlePropEnum.Speed},
            {"移动速度加成",BattlePropEnum.SpeedAdd},
            {"攻击速度",BattlePropEnum.AttackSpeed},
            {"蓄力速度",BattlePropEnum.EnergySpeed},
            {"量谱槽1值",BattlePropEnum.Spectral1},
            {"量谱槽2值",BattlePropEnum.Spectral2},
            {"量谱槽3值",BattlePropEnum.Spectral3},
            {"量谱槽1万分比加成",BattlePropEnum.SpectralRate1},
            {"量谱槽2万分比加成",BattlePropEnum.SpectralRate2},
            {"量谱槽3万分比加成",BattlePropEnum.SpectralRate3},
            {"物理伤害加成",BattlePropEnum.PhyDamAdd},
            {"物理伤害削弱",BattlePropEnum.PhyDamDec},
            {"元素伤害加成",BattlePropEnum.ElemDamAdd},
            {"元素伤害削弱",BattlePropEnum.ElemDamDec},
            {"最终伤害加成",BattlePropEnum.FinDamAdd},
            {"最终伤害减免",BattlePropEnum.FinDamDec},
            {"吸血",BattlePropEnum.Suck},
            {"反伤-荆棘",BattlePropEnum.Thorns},
            {"buff增伤万分比",BattlePropEnum.BuffAddDamage},
            {"buff易伤万分比",BattlePropEnum.BuffVulnerable},
            {"大地元素",BattlePropEnum.ElemEarth},
            {"烈焰元素",BattlePropEnum.ElemFire},
            {"寒冰元素",BattlePropEnum.ElemIce},
            {"自然元素",BattlePropEnum.ElemNatura},
            {"剧毒元素",BattlePropEnum.ElemToxic},
            {"光明元素",BattlePropEnum.ElemLight},
            {"黑暗元素",BattlePropEnum.ElemDark},
            {"全元素攻击",BattlePropEnum.ElemAll},
            {"大地抗性",BattlePropEnum.ResiEarth},
            {"烈焰抗性",BattlePropEnum.ResiFire},
            {"寒冰抗性",BattlePropEnum.ResiIce},
            {"自然抗性",BattlePropEnum.ResiNatura},
            {"剧毒抗性",BattlePropEnum.ResiToxic},
            {"光明抗性",BattlePropEnum.ResiLight},
            {"黑暗抗性",BattlePropEnum.ResiDark},
            {"大地元素百分比",BattlePropEnum.ElemEarthRate},
            {"烈焰元素百分比",BattlePropEnum.ElemFireRate},
            {"寒冰元素百分比",BattlePropEnum.ElemIceRate},
            {"自然元素百分比",BattlePropEnum.ElemNaturaRate},
            {"剧毒元素百分比",BattlePropEnum.ElemToxicRate},
            {"光明元素百分比",BattlePropEnum.ElemLightRate},
            {"黑暗元素百分比",BattlePropEnum.ElemDarkRate},
            {"全元素百分比",BattlePropEnum.ElemAllRate},
            {"抵抗眩晕",BattlePropEnum.ResiDizz},
            {"抵抗混乱",BattlePropEnum.ResiChaos},
            {"抵抗减速",BattlePropEnum.ResiRetard},
            {"抵抗冰冻",BattlePropEnum.ResiFreeze},
            {"抵抗沉默",BattlePropEnum.ResiSlience},
            {"抵抗致盲",BattlePropEnum.ResiBlind},
            {"抵抗恐惧",BattlePropEnum.ResiFear},
            {"抵抗麻痹",BattlePropEnum.ResiNumb},
            {"抵抗压制",BattlePropEnum.ResiSuppress},
            {"抵抗震慑",BattlePropEnum.ResiTodeter},
            {"抵抗过热",BattlePropEnum.ResiOverheat},
            {"抵抗僵直",BattlePropEnum.ResiStiff},
            {"抵抗缠绕",BattlePropEnum.ResiTwine},
            {"抵抗失神",BattlePropEnum.ResiAbsent},
            {"控制抵抗",BattlePropEnum.ResiControl},
            {"增强眩晕",BattlePropEnum.EnhDizz},
            {"增强混乱",BattlePropEnum.EnhChaos},
            {"增强减速",BattlePropEnum.EnhRetard},
            {"增强冰冻",BattlePropEnum.EnhFreeze},
            {"增强冰冻",BattlePropEnum.EnhSlience},
            {"增强致盲",BattlePropEnum.EnhBlind},
            {"增强恐惧",BattlePropEnum.EnhFear},
            {"增强麻痹",BattlePropEnum.EnhNumb},
            {"增强压制",BattlePropEnum.EnhSuppress},
            {"增强震慑",BattlePropEnum.EnhTodeter},
            {"增强过热",BattlePropEnum.EnhOverheat},
            {"增强僵直",BattlePropEnum.EnhStiff},
            {"增强缠绕",BattlePropEnum.EnhTwine},
            {"增强失神",BattlePropEnum.EnhAbsent},
            {"控制增强",BattlePropEnum.EnhControl},
            {"实际攻击力",BattlePropEnum.TruthAtk},
            {"实际生命",BattlePropEnum.TruthHp},
            {"实际防御",BattlePropEnum.TruthDef},
            {"实际魔抗",BattlePropEnum.TruthMDef},
            {"实际大地元素百分比",BattlePropEnum.TruthElemEarthRate},
            {"实际烈焰元素百分比",BattlePropEnum.TruthElemFireRate},
            {"实际寒冰元素百分比",BattlePropEnum.TruthElemIceRate},
            {"实际自然元素百分比",BattlePropEnum.TruthElemNaturaRate},
            {"实际剧毒元素百分比",BattlePropEnum.TruthElemToxicRate},
            {"实际光明元素百分比",BattlePropEnum.TruthElemLightRate},
            {"实际黑暗元素百分比",BattlePropEnum.TruthElemDarkRate},
            {"实际大地元素",BattlePropEnum.TruthElemEarth},
            {"实际烈焰元素",BattlePropEnum.TruthElemFire},
            {"实际寒冰元素",BattlePropEnum.TruthElemIce},
            {"实际自然元素",BattlePropEnum.TruthElemNatura},
            {"实际剧毒元素",BattlePropEnum.TruthElemToxic},
            {"实际光明元素",BattlePropEnum.TruthElemLight},
            {"实际黑暗元素",BattlePropEnum.TruthElemDark},
            {"实际移动速度",BattlePropEnum.TruthSpeed},
            {"实际命中率",BattlePropEnum.TruthHitRate},
            {"实际闪避率",BattlePropEnum.TruthDodgeRate},
            {"实际暴击率",BattlePropEnum.TruthCriRate},
            {"实际抗暴击率",BattlePropEnum.TruthDefCirRate},
            {"实际爆伤率",BattlePropEnum.TruthCirDamRate},
            {"实际法力",BattlePropEnum.TruthMP},
            {"实际攻速",BattlePropEnum.TruthAttackSpeed},
            {"实际蓄力速度",BattlePropEnum.TruthEnergySpeed},
            {"实际量谱槽1",BattlePropEnum.TruthSpectral1},
            {"实际量谱槽2",BattlePropEnum.TruthSpectral2},
            {"实际量谱槽3",BattlePropEnum.TruthSpectral3},
            {"当前生命",BattlePropEnum.curHp},
            {"当前法力",BattlePropEnum.curMp},
            {"当前量谱槽1",BattlePropEnum.curSpectral1},
            {"当前量谱槽2",BattlePropEnum.curSpectral2},
            {"当前量谱槽3",BattlePropEnum.curSpectral3},
            {"战斗单位的表格ID",BattlePropEnum.HeroId},
            {"当前盯着目标的编号",BattlePropEnum.TargetBattleEntityID},
            {"战斗状态下随机子状态",BattlePropEnum.BattleSubState},
            {"战斗状态",BattlePropEnum.State},
            {"职业",BattlePropEnum.Job},
            {"玩家阵营",BattlePropEnum.Faction},
            {"寻路的坐标集合",BattlePropEnum.PathPoses},
            {"寻路的坐标点下标，无效时为-1",BattlePropEnum.CurrPathIndex},
        };

        static public IEnumerable _propchangetype = new ValueDropdownList<PropChangeType>()
        {
            {"根据数值增减",PropChangeType.ChangeWithValue},
            {"根据百分比增减",PropChangeType.ChangeWithPercent},
            {"修改为指定数值",PropChangeType.ChangeToValue},
            {"修改为指定百分比",PropChangeType.ChangeToPercent},
        };

        static public IEnumerable _summondeathcondition = new ValueDropdownList<SummonDeathCondition>()
        {
            {"根据百分比修改",SummonDeathCondition.ChangeWithPercent},
            {"根据数值增减",SummonDeathCondition.ChangeWithValue},
            {"修改为指定数值",SummonDeathCondition.ChangeToValue},
        };

        static public IEnumerable _motionendcondition = new ValueDropdownList<MotionEndCondition>()
        {
            {"正常结束",MotionEndCondition.NormalEnd},
            {"运动到终点时结束阶段",MotionEndCondition.MoveEnd},
        };

        static public IEnumerable _layershowtype = new ValueDropdownList<LayerShowType>()
        {
            {"实际层数",LayerShowType.All},
            {"减去最小层数显示",LayerShowType.Offset},
        };

        static public IEnumerable _openum = new ValueDropdownList<OPEnum>()
        {
            {"设置值到Key",OPEnum.Set},
            {"增加值到Key",OPEnum.Add},
            {"减少值到Key",OPEnum.Sub},
        };

        static public IEnumerable _isignoretarget = new ValueDropdownList<IsIgnoreTarget>()
        {
            {"忽略模型半径",IsIgnoreTarget.IgnoreTarget},
            {"不忽略模型半径(根据起始点选择最近的交点)",IsIgnoreTarget.CheckTargetWithStart},
            {"不忽略模型半径(根据BOSS原点选择最近的交点)",IsIgnoreTarget.CheckTargetWithTargetCenter},
        };

        static public IEnumerable _animconditiontype = new ValueDropdownList<AnimConditionType>()
        {
            {"移动",AnimConditionType.Move},
        };

        static public IEnumerable _triruntimetype = new ValueDropdownList<TriRuntimeType>()
        {
            {"触发者的运行时",TriRuntimeType.Other},
            {"触发器的运行时",TriRuntimeType.Self},
        };

        static public IEnumerable _globalshowtype = new ValueDropdownList<GlobalShowType>()
        {
            {"特殊BUFFUI显示",GlobalShowType.BUFF_SpecialBUFFUI},
            {"修改Shader",GlobalShowType.BUFF_ShaderChange},
            {"附影",GlobalShowType.BUFF_ShadowFollow},
            {"状态动作修改",GlobalShowType.BUFF_ChangeAnim},
            {"动作暂停",GlobalShowType.BUFF_Paralysis},
            {"击倒",GlobalShowType.BUFF_Knock},
            {"隐身",GlobalShowType.BUFF_Hide},
            {"隐藏UI",GlobalShowType.BUFF_HideUI},
            {"点击屏幕释放技能",GlobalShowType.BUFF_ClickUseSkill},
            {"修改整体颜色Shader",GlobalShowType.BUFF_ChangeColorShader},
            {"与主人连线",GlobalShowType.BUFF_PlayEffectLineRenderer},
            {"特殊时间展示",GlobalShowType.BUFF_SpTimeShow},
            {"修改化身",GlobalShowType.BUFF_AvatarChange},
            {"技能槽隐藏",GlobalShowType.BUFF_SkillSlotHide},
            {"量谱显示方式修改",GlobalShowType.BUFF_SpectralChange},
            {"透明改变",GlobalShowType.BUFF_TransChange},
            {"新增职业技能",GlobalShowType.BUFF_JobSkillChange},
            {"基础职业技能失效",GlobalShowType.BUFF_JobSkillCancel},
            {"改变阵营",GlobalShowType.BUFF_ChangeFaction},
            {"移动摇杆自动删除",GlobalShowType.BUFF_AutoDestroy},
            {"技能槽特殊表现",GlobalShowType.Skill_SkillSlotSpShow},
            {"摄像机移动",GlobalShowType.Global_CameraMove},
            {"量谱图切换",GlobalShowType.Global_SpectralSkill},
            {"召唤客户端召唤物",GlobalShowType.Global_AddClientSummon},
        };

        static public IEnumerable _cameramovetype = new ValueDropdownList<CameraMoveType>()
        {
            {"根据角色朝向平移",CameraMoveType.MoveWithCha},
            {"根据绝对朝向平移",CameraMoveType.MoveWithVal},
            {"放缩（0向前、180向后）",CameraMoveType.Zoom},
        };

        static public IEnumerable _enumskillslotspshow = new ValueDropdownList<EnumSkillSlotSpShow>()
        {
            {"蓝色环绕",EnumSkillSlotSpShow.BlueRing},
        };

        static public IEnumerable _uilabel = new ValueDropdownList<UILabel>()
        {
            {"全屏技能隐藏",UILabel.Hide},
        };

        static public IEnumerable _skilltag = new ValueDropdownList<SkillTag>()
        {
            {"普通攻击",SkillTag.Attack},
            {"普通技能",SkillTag.Skill},
            {"辅助技能",SkillTag.Aided},
            {"终极技能",SkillTag.Ultimate},
            {"冲刺技能",SkillTag.Sprint},
            {"宠物技能",SkillTag.PetSkill},
            {"净化技能",SkillTag.Cleanse},
        };

        static public IEnumerable _skilltype = new ValueDropdownList<SkillType>()
        {
        };

        static public IEnumerable _skilllabel = new ValueDropdownList<SkillLabel>()
        {
            {"量谱消耗技能",SkillLabel.SpectralSkill},
            {"普攻",SkillLabel.NormalAttack},
            {"牧夜治疗",SkillLabel.MuYTreat},
            {"牧夜强效",SkillLabel.MuYAttack},
            {"拳师耗1点A",SkillLabel.QuanSOneA},
            {"拳师耗2点A",SkillLabel.QuanSTwoA},
            {"拳师耗3点A",SkillLabel.QuanSThrA},
            {"拳师A技能",SkillLabel.QuanSA},
            {"拳师耗1点B",SkillLabel.QuanSOneB},
            {"拳师耗2点B",SkillLabel.QuanSTwoB},
            {"拳师耗3点B",SkillLabel.QuanSThrB},
            {"拳师B技能",SkillLabel.QuanSB},
        };

        static public IEnumerable _consumetype = new ValueDropdownList<ConsumeType>()
        {
            {"生命值",ConsumeType.Con_Hp},
            {"法力值",ConsumeType.Con_Mp},
        };

        static public IEnumerable _bufftag = new ValueDropdownList<BuffTag>()
        {
            {"无类型",BuffTag.NoEffect},
            {"眩晕",BuffTag.Vertigo},
            {"混乱",BuffTag.Confusion},
            {"减速",BuffTag.Decelerate},
            {"冰冻",BuffTag.Freeze},
            {"沉默",BuffTag.Silent},
            {"致盲",BuffTag.Blinding},
            {"恐惧",BuffTag.Fear},
            {"麻痹",BuffTag.Paralysis},
        };

        static public IEnumerable _shaderenum = new ValueDropdownList<ShaderEnum>()
        {
            {"冰冻",ShaderEnum.Freeze},
            {"GVE红色侵蚀",ShaderEnum.RedPatches},
            {"浸湿",ShaderEnum.Drench},
            {"狂暴",ShaderEnum.Crazy},
        };

        static public IEnumerable _sptimeshowtypeenum = new ValueDropdownList<SpTimeShowTypeEnum>()
        {
            {"头顶时钟",SpTimeShowTypeEnum.OverheadClock},
            {"头顶血条下方读条",SpTimeShowTypeEnum.ProgressBelowHead},
            {"牧夜治疗",SpTimeShowTypeEnum.MuYTreat},
            {"牧夜强攻",SpTimeShowTypeEnum.MuYAttack},
        };

        static public IEnumerable _bufflabel = new ValueDropdownList<BuffLabel>()
        {
            {"减益",BuffLabel.Debuffs},
            {"流血",BuffLabel.Bleed},
            {"护盾",BuffLabel.Shield},
            {"减速",BuffLabel.Decelerate},
            {"冰冻",BuffLabel.Freeze},
            {"击倒",BuffLabel.KnockDown},
            {"附影",BuffLabel.ShadowFollow},
            {"状态动作修改",BuffLabel.ChangeAnim},
            {"隐身",BuffLabel.invisible},
            {"麻痹",BuffLabel.Paralysis},
            {"不显示时间",BuffLabel.NoTimeDisplay},
            {"浸湿",BuffLabel.Drench},
            {"地图让死就死BUFF ",BuffLabel.DeadWithMap},
        };

        static public IEnumerable _buffuishowposenum = new ValueDropdownList<BUFFUIShowPosEnum>()
        {
            {"HUD血条上方 ",BUFFUIShowPosEnum.HUDHpUp},
            {"3D血条上方 ",BUFFUIShowPosEnum.ThreeDHPUp},
            {"秘境BUFF",BUFFUIShowPosEnum.SecretBUFF},
        };

        static public IEnumerable _bulletlabel = new ValueDropdownList<BulletLabel>()
        {
            {"影子",BulletLabel.Shadow},
            {"返回影子",BulletLabel.BackShadow},
            {"琳赛治疗球",BulletLabel.LinStreatball},
            {"牧夜治疗圈",BulletLabel.MuYTreatRing},
            {"牧夜伤害圈",BulletLabel.MuYHurtRing},
            {"地图让死就死子弹",BulletLabel.DeadWithMap},
        };

        static public IEnumerable _effectlabel = new ValueDropdownList<EffectLabel>()
        {
            {"不会触发伤害",EffectLabel.NoTriggerDamage},
            {"特殊子弹1",EffectLabel.SpBullet1Skill},
            {"特殊子弹2",EffectLabel.SpBullet2Skill},
            {"特殊子弹3",EffectLabel.SpBullet3Skill},
            {"背刺",EffectLabel.Backstab},
            {"不会背刺",EffectLabel.NoBackstab},
            {"增加背刺点",EffectLabel.AddBackstabPoint},
            {"抓捕201",EffectLabel.Catch201},
            {"日常副本火焰伤害",EffectLabel.DailyDungeonFireDamage},
            {"噬子弹伤害",EffectLabel.ShiRomRush},
            {"噬吼叫阶段",EffectLabel.ShiLighting},
            {"噬持续伤害",EffectLabel.ShiLoopHit},
            {"中指击中施加BUFF",EffectLabel.ZZHitAddBuff},
            {"中指根据BUFF判断攻击",EffectLabel.ZZHitWithBuff},
            {"琳赛普攻概率召唤子弹",EffectLabel.LinSHitAddBullet},
            {"琳赛血球增加攻击力",EffectLabel.LinSHitAddBuff},
            {"霍莉召唤子弹",EffectLabel.HuoLCreateBullet},
            {"哈森特召唤子弹",EffectLabel.HaSTCreateBullet},
            {"刺客普通攻击",EffectLabel.KnifeManAttack},
            {"刺客影子操作",EffectLabel.ShadowTime},
            {"刺客被动爆炸",EffectLabel.CiKBoomPassive},
            {"刺客影子伤害",EffectLabel.ShadowDamage},
            {"刺客蓄影伤害",EffectLabel.ChargeShadowDamage},
            {"刺客有影增伤",EffectLabel.ShadowMoreDamage},
            {"刺客附影换球",EffectLabel.ShadowToBall},
            {"刺客附影额外爆炸",EffectLabel.ShadowToBoom},
            {"刺客附影额外放波",EffectLabel.ShadowToLoopBoom},
            {"刺客发射匕首",EffectLabel.KnifeFly},
            {"刺客附影额外放雾",EffectLabel.ShadowToShadow},
            {"女枪普通攻击",EffectLabel.GunGirlAttack},
            {"女枪技能引爆点燃",EffectLabel.GunGirlBoom},
            {"女枪暴击感电",EffectLabel.CriticalHitElectricShock},
            {"女枪大招引爆点燃",EffectLabel.GunGirlBigBoom},
            {"结束交叉射击",EffectLabel.BreakDADADA},
            {"重复感电",EffectLabel.ElectricShockAgain},
            {"引爆易燃",EffectLabel.FireFire},
            {"牧夜治疗",EffectLabel.MuY_Treat},
            {"牧夜强效",EffectLabel.MuY_SpHit},
            {"牧夜治疗通用触发",EffectLabel.MuY_TreatTrigger},
            {"牧夜强效通用触发",EffectLabel.MuY_SpHitTrigger},
            {"拳师技能四伤害",EffectLabel.QuanSSkill4Charge},
            {"拳师技能六伤害",EffectLabel.QuanSSkill6Charge},
            {"拳师耗1点A",EffectLabel.QuanSOneA},
        };

    }

}