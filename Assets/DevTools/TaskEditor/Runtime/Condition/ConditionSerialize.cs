///--------------------------------------------------------------------
/// 文件名   :   ConditionSerialize.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/17 17:14:02
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class ConditionSerialize
    {

        [LabelText("条件类型")]
        [ValueDropdown("GetConditionTypes")]
        public ConditionType ConditionType = ConditionType.LevelLimit;

        [LabelText("等级限制")]
        [SerializeField]
        [ShowIf("ShouldSerializeLeveLimit")]
        public LevelCondition LeveLimit;

        [LabelText("任务是否完成")]
        [SerializeField]
        [ShowIf("ShouldSerializeTaskFinish")]
        public TaskFinishCondition TaskFinish;


        [LabelText("任务是否在进行")]
        [SerializeField]
        [ShowIf("ShouldSerializeTaskRunning")]
        public TaskRunningCondition TaskRunning;


        [LabelText("任务目标是否完成")]
        [SerializeField]
        [ShowIf("ShouldSerializeTaskEvent")]
        public TaskEventFinishCondition TaskEvent;

        [LabelText("任务是否已提交")]
        [SerializeField]
        [ShowIf("ShouldSerializeTaskCommit")]
        public TaskCommitCondition TaskCommit;

        [LabelText("道具数量限制")]
        [SerializeField]
        [ShowIf("ShouldSerializeItemLimit")]
        public ItemLimitCondition ItemLimit;

        //队伍检查 
        [LabelText("队伍检查")]
        [SerializeField]
        [ShowIf("ShouldSerializeTeamCheck")]
        public TeamCheckCondition TeamCheck;

        //公会等级检查
        [LabelText("公会等级检查")]
        [SerializeField]
        [ShowIf("ShouldSerializeGuildLevel")]
        public GuildLevelLimitCondition GuildLevel;

        //是否在挖宝
        [LabelText("是否在挖宝")]
        [SerializeField]
        [ShowIf("ShouldSerializeTreasureActived")]
        public TreasureActivedCondition TreasureActived;

        //开服天数 
        [LabelText("开服天数")]
        [SerializeField]
        [ShowIf("ShouldSerializeServerOpendDay")]
        public ServerOpendDayCondition ServerOpendDay;

        //通关指定关卡 
        [LabelText("通关个人秘密层数")]
        [SerializeField]
        [ShowIf("ShouldSerializeFinishLevel")]
        public FinishLevelCondition FinishLevel;

        [LabelText("冒险等级")]
        [SerializeField]
        [ShowIf("ShouldSerializeRiskLevelLimit")]
        public RiskLevelLimitCondition RiskLevelLimit;

        [LabelText("玩家职业")]
        [SerializeField]
        [ShowIf("ShouldSerializePlayerJob")]
        public PlayerJobCondition PlayerJob;


        [LabelText("检查玩家身上Buff")]
        [SerializeField]
        [ShowIf("ShouldSerializeCheckBuff")]
        public CheckBuffCondition CheckBuff;

        [LabelText("队伍等级限制")]
        [SerializeField]
        [ShowIf("ShouldSerializeTeamLevelLimt")]
        public TeamLevelLimtCondition TeamLevelLimt;

        [LabelText("通缉任务是否接取")]
        [SerializeField]
        [ShowIf("ShouldSerializeWantedAccepted")]
        public WantedAcceptedCondition WantedAccepted;

        [LabelText("通缉任务是否接取")]
        [SerializeField]
        [ShowIf("ShouldSerializeEquipIntensify")]
        public EquipIntensifyCondition EquipIntensify;

        [LabelText("通缉任务是否接取")]
        [SerializeField]
        [ShowIf("ShouldSerializePartnerCount")]
        public PartnerCountCondition PartnerCount;

        [LabelText("通缉任务是否接取")]
        [SerializeField]
        [ShowIf("ShouldSerializeAmuletPolishing")]
        public AmuletPolishingCondition AmuletPolishing;

        [LabelText("穿戴了X个品质为Y的装备")]
        [SerializeField]
        [ShowIf("ShouldSerializeEquipQualityCount")]
        public EquipQualityCondition EquipQualityCount;

        [LabelText("职业技能总等级")]
        [SerializeField]
        [ShowIf("ShouldSerializeJobSkillLevelCount")]
        public JobSkillLevelCountCondition JobSkillLevelCount;

        [LabelText("通关指定单人日常本")]
        [SerializeField]
        [ShowIf("ShouldSerializePersonDailyPass")]
        public PersonDailyPassCondition PersonDailyPass;

        [LabelText("通关指定组队日常本")]
        [SerializeField]
        [ShowIf("ShouldSerializeTeamDailyPass")]
        public TeamDailyPassCondition TeamDailyPass;

        [LabelText("指定系统解锁")]
        [SerializeField]
        [ShowIf("ShouldSerializeAppointSystemOpen")]
        public AppointSystemOpenCondition AppointSystemOpen;

        [LabelText("副本黑板")]
        [SerializeField]
        [ShowIf("ShouldSerializeCheckBlackBoard")]
        public CheckBlackBoardCondition CheckBlackBoard;

        [LabelText("战力模块检查")]
        [SerializeField]
        [ShowIf("ShouldSerializePowerModuleCheck")]
        public PowerModuleCheckCondition PowerModuleCheck;

        [LabelText("角色累计获得技能点数")]
        [SerializeField]
        [ShowIf("ShouldSerializeSkillDotTotal")]
        public SkillDotTotalCondition SkillDotTotal;

        [LabelText("冒险旅程阶段检查")]
        [SerializeField]
        [ShowIf("ShouldSerializeAdventureCheck")]
        public AdventureCheckCondition AdventureCheck;
        public CMCondition GetMCondition()
        {
            switch (ConditionType)
            {
                case ConditionType.LevelLimit:
                    return LeveLimit;
                case ConditionType.TaskIsFinish:
                    return TaskFinish;
                case ConditionType.TaskIsRunning:
                    return TaskRunning;
                case ConditionType.TaskEventIsFinish:
                    return TaskEvent;
                case ConditionType.TaskIsCommit:
                    return TaskCommit;
                case ConditionType.ItemLimit:
                    return ItemLimit;
                case ConditionType.TeamCheck:
                    return TeamCheck;
                case ConditionType.GuildLevelLimit:
                    return GuildLevel;
                case ConditionType.TreasureActived:
                    return TreasureActived;
                case ConditionType.ServerOpendDay:
                    return ServerOpendDay;
                case ConditionType.FinishLevel:
                    return FinishLevel;
                case ConditionType.RiskLevelLimit:
                    return RiskLevelLimit;
                case ConditionType.PlayerJob:
                    return PlayerJob;
                case ConditionType.TeamLevelLimt:
                    return TeamLevelLimt;
                case ConditionType.CheckBuff:
                    return CheckBuff;
                case ConditionType.WantedAccepted:
                    return WantedAccepted;
                case ConditionType.EquipIntensify:
                    return EquipIntensify;
                case ConditionType.PartnerCount:
                    return PartnerCount;
                case ConditionType.AmuletPolishing:
                    return AmuletPolishing;
                case ConditionType.EquipQualityCount:
                    return EquipQualityCount;
                case ConditionType.JobSkillLevelCount:
                    return JobSkillLevelCount;
                case ConditionType.PersonDailyPass:
                    return PersonDailyPass;
                case ConditionType.TeamDailyPass:
                    return TeamDailyPass;
                case ConditionType.AppointSystemOpen:
                    return AppointSystemOpen;
                case ConditionType.CheckBlackBoard:
                    return CheckBlackBoard;
                case ConditionType.PowerModuleCheck:
                    return PowerModuleCheck;
                case ConditionType.SkillDotTotal:
                    return SkillDotTotal;
                case ConditionType.AdventureCheck:
                    return AdventureCheck;
            }
            return default;
        }

        public ConditionSerialize(JsonConditon jsonConditon)
        {
            ConditionType = jsonConditon.ConditionType;
            switch (ConditionType)
            {
                case ConditionType.LevelLimit:
                    LeveLimit = new LevelCondition();
                    LeveLimit.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TaskIsFinish:
                    TaskFinish = new TaskFinishCondition();
                    TaskFinish.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TaskIsRunning:
                    TaskRunning = new TaskRunningCondition();
                    TaskRunning.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TaskEventIsFinish:
                    TaskEvent = new TaskEventFinishCondition();
                    TaskEvent.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TaskIsCommit:
                    TaskCommit = new TaskCommitCondition();
                    TaskCommit.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.ItemLimit:
                    ItemLimit = new ItemLimitCondition();
                    ItemLimit.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TeamCheck:
                    TeamCheck = new TeamCheckCondition();
                    TeamCheck.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.GuildLevelLimit:
                    GuildLevel = new GuildLevelLimitCondition();
                    GuildLevel.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TreasureActived:
                    TreasureActived = new TreasureActivedCondition();
                    TreasureActived.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.ServerOpendDay:
                    ServerOpendDay = new ServerOpendDayCondition();
                    ServerOpendDay.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.FinishLevel:
                    FinishLevel = new FinishLevelCondition();
                    FinishLevel.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.RiskLevelLimit:
                    RiskLevelLimit = new RiskLevelLimitCondition();
                    RiskLevelLimit.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.PlayerJob:
                    PlayerJob = new PlayerJobCondition();
                    PlayerJob.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TeamLevelLimt:
                    TeamLevelLimt = new TeamLevelLimtCondition();
                    TeamLevelLimt.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.CheckBuff:
                    CheckBuff = new CheckBuffCondition();
                    CheckBuff.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.WantedAccepted:
                    WantedAccepted = new WantedAcceptedCondition();
                    WantedAccepted.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.EquipIntensify:
                    EquipIntensify = new EquipIntensifyCondition();
                    EquipIntensify.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.PartnerCount:
                    PartnerCount = new PartnerCountCondition();
                    PartnerCount.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.AmuletPolishing:
                    AmuletPolishing = new AmuletPolishingCondition();
                    AmuletPolishing.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.EquipQualityCount:
                    EquipQualityCount = new EquipQualityCondition();
                    EquipQualityCount.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.JobSkillLevelCount:
                    JobSkillLevelCount = new JobSkillLevelCountCondition();
                    JobSkillLevelCount.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.PersonDailyPass:
                    PersonDailyPass = new PersonDailyPassCondition();
                    PersonDailyPass.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.TeamDailyPass:
                    TeamDailyPass = new TeamDailyPassCondition();
                    TeamDailyPass.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.AppointSystemOpen:
                    AppointSystemOpen = new AppointSystemOpenCondition();
                    AppointSystemOpen.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.CheckBlackBoard:
                    CheckBlackBoard = new CheckBlackBoardCondition();
                    CheckBlackBoard.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.PowerModuleCheck:
                    PowerModuleCheck = new PowerModuleCheckCondition();
                    PowerModuleCheck.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.SkillDotTotal:
                    SkillDotTotal = new SkillDotTotalCondition();
                    SkillDotTotal.OnDeSerializd(jsonConditon);
                    break;
                case ConditionType.AdventureCheck:
                    AdventureCheck = new AdventureCheckCondition();
                    AdventureCheck.OnDeSerializd(jsonConditon);
                    break;
                default:
                    break;
            }
        }

        public bool ShouldSerializeLeveLimit()
        {
            return this.ConditionType == ConditionType.LevelLimit;
        }
        public bool ShouldSerializeTaskFinish()
        {
            return this.ConditionType == ConditionType.TaskIsFinish;
        }

        public bool ShouldSerializeTaskRunning()
        {
            return this.ConditionType == ConditionType.TaskIsRunning;
        }

        public bool ShouldSerializeTaskEvent()
        {
            return this.ConditionType == ConditionType.TaskEventIsFinish;
        }

        public bool ShouldSerializeItemLimit()
        {
            return this.ConditionType == ConditionType.ItemLimit;
        }

        public bool ShouldSerializeTaskCommit()
        {
            return this.ConditionType == ConditionType.TaskIsCommit;
        }

        public bool ShouldSerializeTeamCheck()
        {
            return ConditionType == ConditionType.TeamCheck;
        }
        public bool ShouldSerializeGuildLevel()
        {
            return ConditionType == ConditionType.GuildLevelLimit;
        }

        public bool ShouldSerializeTreasureActived()
        {
            return ConditionType == ConditionType.TreasureActived;
        }

        public bool ShouldSerializeServerOpendDay()
        {
            return ConditionType == ConditionType.ServerOpendDay;
        }
        public bool ShouldSerializeFinishLevel()
        {
            return ConditionType == ConditionType.FinishLevel;
        }

        public bool ShouldSerializeRiskLevelLimit()
        {
            return ConditionType == ConditionType.RiskLevelLimit;
        }

        public bool ShouldSerializePlayerJob()
        {
            return ConditionType == ConditionType.PlayerJob;
        }

        public bool ShouldSerializeTeamLevelLimt()
        {
            return ConditionType == ConditionType.TeamLevelLimt;
        }

        public bool ShouldSerializeCheckBuff()
        {
            return ConditionType == ConditionType.CheckBuff;
        }

        public bool ShouldSerializeWantedAccepted()
        {
            return ConditionType == ConditionType.WantedAccepted;
        }

        public bool ShouldSerializeEquipIntensify()
        {
            return ConditionType == ConditionType.EquipIntensify;
        }

        public bool ShouldSerializePartnerCount()
        {
            return ConditionType == ConditionType.PartnerCount;
        }

        public bool ShouldSerializeAmuletPolishing()
        {
            return ConditionType == ConditionType.AmuletPolishing;
        }
        public bool ShouldSerializeEquipQualityCount()
        {
            return ConditionType == ConditionType.EquipQualityCount;
        }
        public bool ShouldSerializeJobSkillLevelCount()
        {
            return ConditionType == ConditionType.JobSkillLevelCount;
        }
        public bool ShouldSerializePersonDailyPass()
        {
            return ConditionType == ConditionType.PersonDailyPass;
        }
        public bool ShouldSerializeTeamDailyPass()
        {
            return ConditionType == ConditionType.TeamDailyPass;
        }
        public bool ShouldSerializeAppointSystemOpen()
        {
            return ConditionType == ConditionType.AppointSystemOpen;
        }
        public bool ShouldSerializeCheckBlackBoard()
        {
            return ConditionType == ConditionType.CheckBlackBoard;
        }
        public bool ShouldSerializeAdventureCheck()
        {
            return ConditionType == ConditionType.AdventureCheck;
        }
        public bool ShouldSerializePowerModuleCheck()
        {
            return ConditionType == ConditionType.PowerModuleCheck;
        }
        public bool ShouldSerializeSkillDotTotal()
        {
            return ConditionType == ConditionType.SkillDotTotal;
        }
        public IEnumerable GetConditionTypes()
        {
            return TaskEnumUtils._conditiontypetypes;
        }


    }
}