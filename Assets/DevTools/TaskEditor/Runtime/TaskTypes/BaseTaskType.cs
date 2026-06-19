///--------------------------------------------------------------------
/// 文件名   :   BaseTaskType.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 13:32:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Task
{

    [System.Serializable]
    public class BaseTaskType
    {

        //public BaseTaskType(int mapid)
        //{
        //    this.MapID = mapid;
        //}

        [HideInInspector]
        public int MapID;

        public virtual void OnSerialized(List<string> pramas)
        {

        }

        public virtual void OnDeSerialized(List<string> pramas)
        {

        }

        public int ToInt(string arg)
        {
            int v = 0;
            System.Int32.TryParse(arg, out v);
            return v;
        }

    }

    /// <summary>
    /// 对话 1
    /// </summary>
    [System.Serializable]
    public class TaskTypeDialogue : BaseTaskType
    {

        public override void OnSerialized(List<string> pramas)
        {
        }

        public override void OnDeSerialized(List<string> pramas)
        {
        }
    }

    /// <summary>
    /// 关卡 2
    /// </summary>
    [System.Serializable]
    public class TaskTypeLevel : BaseTaskType
    {
        //public TaskTypeLevel(int mapid) : base(mapid)
        //{

        //}
        [LabelText("关卡ID")]
        public int LevelID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(LevelID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                LevelID = ToInt(pramas[0]);
            }
        }
    }

    /// <summary>
    /// 杀怪 3
    /// </summary>
    [System.Serializable]
    public class TaskTypeKillMonster : BaseTaskType
    {
        //public TaskTypeKillMonster(int mapid) : base(mapid)
        //{

        //}
        [LabelText("怪物ID")]
#if UNITY_EDITOR
        [ValueDropdown("GetFindPathID", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "目标")]
#endif
        public List<long> MonsterID = new List<long>();


#if UNITY_EDITOR
        [XLua.BlackList]
        public IEnumerable GetFindPathID()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.MapID, out var scfg))
            {
                return scfg._monster;

            }

            return null;
        }
#endif

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(string.Join(',', MonsterID));
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                if (!string.IsNullOrEmpty(pramas[0]))
                {
                    MonsterID = pramas[0].Split(',').Select(long.Parse).ToList();
                }

                //MonsterID = ToInt(pramas[0]);
            }
        }
    }

    /// <summary>
    /// 收集 4
    /// </summary>
    [System.Serializable]
    public class TaskTypeCollect : BaseTaskType
    {
        //public TaskTypeCollect(int mapid) : base(mapid)
        //{

        //}
        [LabelText("道具ID")]
        public int ItemID;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(ItemID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                ItemID = ToInt(pramas[0]);
            }
        }
    }

    /// <summary>
    /// 使用道具 5
    /// </summary>
    [System.Serializable]
    public class TaskTypeUseItem : BaseTaskType
    {
        //public TaskTypeUseItem(int mapid) : base(mapid)
        //{

        //}
        [LabelText("道具ID")]
        public int ItemID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(ItemID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                ItemID = ToInt(pramas[0]);
            }
        }
    }

    /// <summary>
    /// 使用技能 6
    /// </summary>
    [System.Serializable]
    public class TaskTypeUseSkill : BaseTaskType
    {
        //public TaskTypeUseSkill(int mapid) : base(mapid)
        //{

        //}
        [LabelText("技能ID")]
        public int SkillID;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(SkillID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                SkillID = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 跑腿/到达 7
    /// </summary>
    [System.Serializable]
    public class TaskTypeArrive : BaseTaskType
    {
        //public TaskTypeArrive(int mapid) : base(mapid)
        //{

        //}
    }

    /// <summary>
    /// 护送 8
    /// </summary>
    [System.Serializable]
    public class TaskTypeEscort : BaseTaskType
    {
        //public TaskTypeEscort(int mapid) : base(mapid)
        //{

        //}
        ////NPCID
    }

    /// <summary>
    /// 交互 9
    /// </summary>
    [System.Serializable]
    public class TaskTypeInterAction : BaseTaskType
    {
        //public TaskTypeInterAction(int mapid) : base(mapid)
        //{

        //}
        //交互物ID
        [LabelText("交互物ID")]
#if UNITY_EDITOR
        [ValueDropdown("GetFindPathID", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle = "目标")]
#endif
        public List<long> InterID = new List<long>();

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(string.Join(',', InterID));
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                if (!string.IsNullOrEmpty(pramas[0]))
                {
                    InterID = pramas[0].Split(',').Select(long.Parse).ToList();
                }
            }
        }


#if UNITY_EDITOR
        [XLua.BlackList]
        public IEnumerable GetFindPathID()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.MapID, out var scfg))
            {
                return scfg._interlist;

            }

            return null;
        }
#endif
    }

    /// <summary>
    /// 序章 10
    /// </summary>
    [System.Serializable]
    public class TaskTypePrologue : BaseTaskType
    {
        //public TaskTypePrologue(int mapid) : base(mapid)
        //{

        //}
        [DisplayAsString]
        [LabelText("提示")]
        [Newtonsoft.Json.JsonIgnore]
        public string Tips = "效果里实现";
    }


    /// <summary>
    /// 黑幕 11
    /// </summary>
    [System.Serializable]
    public class TaskTypeShady : BaseTaskType
    {
        //public TaskTypeShady(int mapid) : base(mapid)
        //{

        //}
        [DisplayAsString]
        [LabelText("提示")]
        [Newtonsoft.Json.JsonIgnore]
        public string Tips = "效果里实现";
    }

    /// <summary>
    /// 杀怪获得道具
    /// </summary>
    [System.Serializable]
    public class TaskTypeKillMonsterGetItem : TaskTypeKillMonster
    {
        //public TaskTypeKillMonsterGetItem(int mapid) : base(mapid)
        //{

        //}
        [BoxGroup("任务道具")]
        [LabelText("任务道具ID")]
        public int TaskItemID;

        [BoxGroup("任务道具")]
        [LabelText("任务道具数量最小值")]
        public int TaskItemMin;

        [BoxGroup("任务道具")]
        [LabelText("任务道具数量最大值")]
        public int TaskItemMax;

        [BoxGroup("任务道具")]
        [LabelText("触发概率")]
        [SuffixLabel("10000最大值")]
        public int Prob;

        public override void OnSerialized(List<string> pramas)
        {
            base.OnSerialized(pramas);
            pramas.Add(string.Join(',',
                       this.TaskItemID, this.TaskItemMin, this.TaskItemMax, this.Prob));
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            base.OnDeSerialized(pramas);
            if (pramas != null && pramas.Count >= 2)
            {
                var li = pramas[1].Split(',').Select(int.Parse).ToList();
                if (li.Count >= 4)
                {
                    this.TaskItemID = li[0];
                    this.TaskItemMin = li[1];
                    this.TaskItemMax = li[2];
                    this.Prob = li[3];
                }
            }
        }
    }
    /// <summary>
    /// 交互获得道具
    /// </summary>
    [System.Serializable]
    public class TaskTypeInterGetItem : TaskTypeInterAction
    {
        //public TaskTypeKillMonsterGetItem(int mapid) : base(mapid)
        //{

        //}
        [BoxGroup("任务道具")]
        [LabelText("任务道具ID")]
        public int TaskItemID;

        [BoxGroup("任务道具")]
        [LabelText("任务道具数量最小值")]
        public int TaskItemMin;

        [BoxGroup("任务道具")]
        [LabelText("任务道具数量最大值")]
        public int TaskItemMax;

        [BoxGroup("任务道具")]
        [LabelText("触发概率")]
        [SuffixLabel("10000最大值")]
        public int Prob;

        public override void OnSerialized(List<string> pramas)
        {
            base.OnSerialized(pramas);
            pramas.Add(string.Join(',',
                       this.TaskItemID, this.TaskItemMin, this.TaskItemMax, this.Prob));
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            base.OnDeSerialized(pramas);
            if (pramas != null && pramas.Count >= 2)
            {
                var li = pramas[1].Split(',').Select(int.Parse).ToList();
                if (li.Count >= 4)
                {
                    this.TaskItemID = li[0];
                    this.TaskItemMin = li[1];
                    this.TaskItemMax = li[2];
                    this.Prob = li[3];
                }
            }
        }
    }

    [System.Serializable]
    public class TaskTypeGuideUseItem : BaseTaskType
    {
        [LabelText("道具ID")]
        public int ItemID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(ItemID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                ItemID = ToInt(pramas[0]);
            }
        }
    }
    [System.Serializable]
    public class TaskTypeGuidePartnerInBattle : BaseTaskType
    {
        [LabelText("伙伴ID")]
        public int PartnerID;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(PartnerID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                PartnerID = ToInt(pramas[0]);
            }
        }
    }

    [System.Serializable]
    public class TaskTypePlays : BaseTaskType
    {
        [LabelText("玩法")]
        [ValueDropdown("GetPlayEnums")]
        public PlayEnums playType;

        [LabelText("玩法参数")]
        [ShowIf("ShouldParams")]
        public List<long> paramList = new List<long>();

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(((int)playType).ToString());
            if (paramList.Count == 0)
            {
                pramas.Add("0");
            }
            else
            {
                pramas.Add(string.Join(',', paramList));
            }
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                playType = (PlayEnums)ToInt(pramas[0]);
                if (pramas.Count > 1)
                {
                    if (!string.IsNullOrEmpty(pramas[1]))
                    {
                        paramList = pramas[1].Split(',').Select(long.Parse).ToList();
                    }
                }
            }

        }

        public IEnumerable GetPlayEnums()
        {
            return TaskEnumUtils._taskplayenums;
        }
        public bool ShouldParams()
        {
            return playType == PlayEnums.TeamWanted;
        }
    }

    /// <summary>
    /// 玩家等级
    /// </summary>
    [System.Serializable]
    public class TaskTypePlayerLevel : BaseTaskType
    {
        [LabelText("玩家等级")]
        public int PlayerLevel;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(PlayerLevel.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                PlayerLevel = ToInt(pramas[0]);
            }
        }
    }

    /// <summary>
    /// 玩家等级
    /// </summary>
    [System.Serializable]
    public class TaskTypeAdventureLevel : BaseTaskType
    {
        [LabelText("玩家冒险等级")]
        public int AdventureLevel;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(AdventureLevel.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                AdventureLevel = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 系统交互
    /// </summary>
    [System.Serializable]
    public class TaskTypeSystemInteract : BaseTaskType
    {
        [LabelText("系统交互")]
        [ValueDropdown("GeSystemInteractEnums")]
        public SystemInteract playType;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(((int)playType).ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                playType = (SystemInteract)ToInt(pramas[0]);
            }
        }

        public IEnumerable GeSystemInteractEnums()
        {
            return TaskEnumUtils._systemInteractenums;
        }
    }

    /// <summary>
    /// 今日登录
    /// </summary>
    [System.Serializable]
    public class TaskTypeLoginToday : BaseTaskType
    {
    }
    /// <summary>
    /// x商店购买道具x次
    /// </summary>
    [System.Serializable]
    public class TaskTypeShopBuyGoods : BaseTaskType
    {
        [LabelText("商店ID")]
        public int ShopID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(ShopID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                ShopID = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 参与x阶通缉x次
    /// </summary>
    [System.Serializable]
    public class TaskTypeWantTaskCountStage : BaseTaskType
    {
        [LabelText("通缉阶段ID")]
        public int wantedID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(wantedID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                wantedID = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 玩家技能升级
    /// </summary>
    [System.Serializable]
    public class TaskTypePlayerSkillLevelUp : BaseTaskType
    {
        [LabelText("技能ID")]
        public int skillID;
        [LabelText("技能等级")]
        public int level;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.skillID.ToString());
            pramas.Add(this.level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                skillID = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 签到领奖次数
    /// </summary>
    [System.Serializable]
    public class TaskTypePlayerSignInAward : BaseTaskType
    {

    }
    /// <summary>
    /// 击杀场景或者副本怪物x次
    /// </summary>
    [System.Serializable]
    public class TaskTypeSkillAppointMonster : BaseTaskType
    {
        [LabelText("击杀场景或者副本怪物x次")]
        [ValueDropdown("GetMonsterSourceEnums")]
        public MonsterSourceEnum sourceType;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(((int)sourceType).ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                sourceType = (MonsterSourceEnum)ToInt(pramas[0]);
            }
        }

        public IEnumerable GetMonsterSourceEnums()
        {
            return TaskEnumUtils._monstersourceenums;
        }
    }
    /// <summary>
    /// 完成指定任务
    /// </summary>
    [System.Serializable]
    public class TaskTypeFinishTask : BaseTaskType
    {
        [LabelText("任务ID")]
        public int taskID;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(taskID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                taskID = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 总计强化等级X
    /// </summary>
    [System.Serializable]
    public class TaskTotalIntensify : BaseTaskType
    {
        [LabelText("总计强化等级X")]
        public int totalLevel;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(totalLevel.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                totalLevel = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// N个X级以上佣兵
    /// </summary>
    [System.Serializable]
    public class TaskPartnerCount : BaseTaskType
    {
        [LabelText("数量")]
        public int count;
        [LabelText("等级")]
        public int level;
        [LabelText("星级")]
        public int star;
        [LabelText("品质")]
        public int quality;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
            pramas.Add(this.level.ToString());
            pramas.Add(this.star.ToString());
            pramas.Add(this.quality.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 4)
            {
                count = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
                star = ToInt(pramas[2]);
                quality = ToInt(pramas[3]);
            }
        }
    }
    /// <summary>
    /// 秘境X层通关
    /// </summary>
    [System.Serializable]
    public class TaskSecretAreaLevel : BaseTaskType
    {
        [LabelText("秘境X层通关")]
        public int level;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                level = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 奥义技能X级
    /// </summary>
    [System.Serializable]
    public class TaskSkillLevel : BaseTaskType
    {
        [LabelText("技能等级")]
        public int level;
        [LabelText("技能类型(SkillType)")]
        [ValueDropdown("GetSkillType")]
        public SkillType skillType = SkillType.Anything;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(((int)skillType).ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                level = ToInt(pramas[0]);
                skillType = (SkillType)ToInt(pramas[1]);
            }
        }

        public IEnumerable GetSkillType()
        {
            return TaskEnumUtils._skilltypeenums;
        }
    }
    /// <summary>
    /// 完成副本ID(蚀梦之影)
    /// </summary>
    [System.Serializable]
    public class TaskShadowofDreamErosionPass : BaseTaskType
    {
        [LabelText("完成副本ID(蚀梦之影)")]
        public int id;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(id.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                id = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 养成/打磨完成X个Y等阶Z品质纹章
    /// </summary>
    [System.Serializable]
    public class TaskTalismanQualityCount : BaseTaskType
    {
        [LabelText("数量")]
        public int count;
        [LabelText("等级")]
        public int level;
        [LabelText("品质")]
        public int quality;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
            pramas.Add(this.level.ToString());
            pramas.Add(this.quality.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 3)
            {
                count = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
                quality = ToInt(pramas[2]);
            }
        }
    }
    /// <summary>
    /// 获取N个X级纹章
    /// </summary>
    [System.Serializable]
    public class TaskHerldryCount : BaseTaskType
    {
        [LabelText("数量")]
        public int count;
        [LabelText("等级")]
        public int level;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
            pramas.Add(this.level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                count = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 爬塔X层
    /// </summary>
    [System.Serializable]
    public class TaskPersonTowerLevel : BaseTaskType
    {
        [LabelText("层数")]
        public int level;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                level = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 累计获取N件X类型道具
    /// </summary>
    [System.Serializable]
    public class TaskPartnerEquipCount : BaseTaskType
    {
        [LabelText("数量")]
        public int count;
        [LabelText("道具类型")]
        [ValueDropdown("GetItemType")]
        public ItemType itemType;
        [LabelText("品质")]
        public int quality;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
            pramas.Add(((int)itemType).ToString());
            pramas.Add(this.quality.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 3)
            {
                count = ToInt(pramas[0]);
                itemType = (ItemType)ToInt(pramas[1]);
                quality = ToInt(pramas[2]);
            }
        }

        public IEnumerable GetItemType()
        {
            return TaskEnumUtils._itemtypeenums;
        }
    }
    /// <summary>
    /// N件X级装备洗练度达到Y
    /// </summary>
    [System.Serializable]
    public class TaskEquipmentTrainingLevel : BaseTaskType
    {
        [LabelText("数量")]
        public int count;
        [LabelText("等级")]
        public int level;
        [LabelText("完美度")]
        public int perfection;
        [LabelText("品质")]
        public int refinement;


        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
            pramas.Add(this.level.ToString());
            pramas.Add(this.perfection.ToString());
            pramas.Add(this.refinement.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 4)
            {
                count = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
                perfection = ToInt(pramas[2]);
                refinement = ToInt(pramas[3]);
            }
        }
    }
    /// <summary>
    /// 点亮天赋点X个
    /// </summary>
    [System.Serializable]
    public class TaskTalentCount : BaseTaskType
    {
        [LabelText("数量")]
        public int count;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.count.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count > 0)
            {
                count = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 某位置(1，2，3代表出战，助战，任意位置)伙伴任意资质超过X
    /// </summary>
    [System.Serializable]
    public class TaskPartnerQualification : BaseTaskType
    {
        [LabelText("位置")]
        public int index;

        [LabelText("资质")]
        public int qualification;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.index.ToString());
            pramas.Add(this.qualification.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                index = ToInt(pramas[0]);
                qualification = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// X模块信标评分达到(X需要支持总模块)
    /// </summary>
    [System.Serializable]
    public class TaskModuleRating : BaseTaskType
    {
        [LabelText("模块ID")]
        public int id;

        [LabelText("评分")]
        public int rating;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
            pramas.Add(this.rating.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                id = ToInt(pramas[0]);
                rating = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 任意呜器达到x级
    /// </summary>
    [System.Serializable]
    public class TaskBuzzerLevel : BaseTaskType
    {
        [LabelText("等级")]
        public int level;

        [LabelText("数量")]
        public int num;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(this.num.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null)
            {
                level = pramas.Count > 0 ? ToInt(pramas[0]) : 0;
                num = pramas.Count > 1 ? ToInt(pramas[1]) : 0;
            }
        }
    }
    /// <summary>
    /// z频道发送r条消息
    /// </summary>
    [System.Serializable]
    public class TaskChannelMessages : BaseTaskType
    {
        [LabelText("频道ID")]
        public int id;

        [LabelText("消息数量")]
        public int num;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
            pramas.Add(this.num.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                id = ToInt(pramas[0]);
                num = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 获取z级以上品质以上装备I件
    /// </summary>
    [System.Serializable]
    public class TaskGetEquipQuality : BaseTaskType
    {
        [LabelText("等级")]
        public int level;

        [LabelText("品质")]
        public int quality;

        [LabelText("数量")]
        public int count;

        [LabelText("部位")]
        public int part;

        [LabelText("职业")]//（填-1代表本职业装备）
        public int job;

        [LabelText("副属性词条品质")]
        public int subQuality;

        [LabelText("获取途径")]
        public int AccessMethods;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(this.quality.ToString());
            pramas.Add(this.count.ToString());
            pramas.Add(this.part.ToString());
            pramas.Add(this.job.ToString());
            pramas.Add(this.subQuality.ToString());
            pramas.Add(this.AccessMethods.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 7)
            {
                level = ToInt(pramas[0]);
                quality = ToInt(pramas[1]);
                count = ToInt(pramas[2]);
                part = ToInt(pramas[3]);
                job = ToInt(pramas[4]);
                subQuality = ToInt(pramas[5]);
                AccessMethods = ToInt(pramas[6]);
            }
        }
    }
    /// <summary>
    /// 穿戴x级以上品质以上装备件
    /// </summary>
    [System.Serializable]
    public class TaskWearingEquipQuality : BaseTaskType
    {
        [LabelText("等级")]
        public int level;

        [LabelText("品质")]
        public int quality;

        [LabelText("数量")]
        public int count;

        [LabelText("装备id数组")]
        public string idList;

        [LabelText("部位")]
        public int part;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(this.quality.ToString());
            pramas.Add(this.count.ToString());
            pramas.Add(this.idList);
            pramas.Add(this.part.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null)
            {
                level = pramas.Count > 0 ? ToInt(pramas[0]) : 0;
                quality = pramas.Count > 1 ? ToInt(pramas[1]) : 0;
                count = pramas.Count > 2 ? ToInt(pramas[2]) : 0;
                idList = pramas.Count > 3 ? pramas[3] : "";
                part = pramas.Count > 4 ? ToInt(pramas[4]) : 0;
            }
        }
    }
    /// <summary>
    /// X商店累计消耗货币y数量N
    /// </summary>
    [System.Serializable]
    public class TaskShopSpendCurrency : BaseTaskType
    {
        [LabelText("商店ID")]
        public int id;

        [LabelText("货币ID")]
        public int currencyID;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
            pramas.Add(this.currencyID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null)
            {
                id = pramas.Count > 0 ? ToInt(pramas[0]) : 0;
                currencyID = pramas.Count > 1 ? ToInt(pramas[1]) : 0;
            }
        }
    }
    /// <summary>
    /// 激活I个z级战技
    /// </summary>
    [System.Serializable]
    public class TaskActivationWar : BaseTaskType
    {
        [LabelText("等级")]
        public int level;

        [LabelText("数量")]
        public int num;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(this.num.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null)
            {
                level = pramas.Count > 0 ? ToInt(pramas[0]) : 0;
                num = pramas.Count > 1 ? ToInt(pramas[1]) : 0;
            }
        }
    }
    /// <summary>
    /// 指定邮件标题领奖次数
    /// </summary>
    [System.Serializable]
    public class TaskMailRewarded : BaseTaskType
    {
        [LabelText("邮件标题")]
        public string title;

        [LabelText("奖励ID")]
        public int rewardID;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.title);
            pramas.Add(this.rewardID.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                title = pramas[0];
                rewardID = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 所有装备强化至XXXX级
    /// </summary>
    [System.Serializable]
    public class TaskTotalEquipIntensify : BaseTaskType
    {
        [LabelText("强化等级")]
        public int level;


        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 1)
            {
                level = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 穿戴x级以上品质以上护符
    /// </summary>
    [System.Serializable]
    public class TaskWearingQualityAmulet : BaseTaskType
    {
        [LabelText("等级")]
        public int level;

        [LabelText("品质")]
        public int quality;

        [LabelText("数量")]
        public int count;

        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.level.ToString());
            pramas.Add(this.quality.ToString());
            pramas.Add(this.count.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 3)
            {
                level = ToInt(pramas[0]);
                quality = ToInt(pramas[1]);
                count = ToInt(pramas[2]);
            }
        }
    }
    /// <summary>
    /// 装配指定ID鸣器
    /// </summary>
    [System.Serializable]
    public class TaskWearingTweeter : BaseTaskType
    {
        [LabelText("鸣器ID 0任意")]
        public int id;


        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 1)
            {
                id = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 活跃度
    /// </summary>
    [System.Serializable]
    public class TaskActivitylevel : BaseTaskType
    {
        [LabelText("活跃度")]
        public int value;


        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.value.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 1)
            {
                value = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 进入指定地图
    /// </summary>
    [System.Serializable]
    public class TaskEnterMap : BaseTaskType
    {
        [LabelText("地图ID")]
        public int id;


        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 1)
            {
                id = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 创建或加入商会
    /// </summary>
    [System.Serializable]
    public class CreateOrJoinGuild : BaseTaskType
    {
    }
    /// <summary>
    /// 养成/指定部位强化等级达到X级
    /// </summary>
    [System.Serializable]
    public class PartsIntensifyLevel : BaseTaskType
    {
        [LabelText("部位 0置空")]
        public int part;

        [LabelText("等级")]
        public int level;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.part.ToString());
            pramas.Add(this.level.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                part = ToInt(pramas[0]);
                level = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 养成/切换X槽位技能天赋
    /// </summary>
    [System.Serializable]
    public class ChangeSkillTalent : BaseTaskType
    {
        [LabelText("槽位 对应posset的1-7")]
        public int slot;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.slot.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 1)
            {
                slot = ToInt(pramas[0]);
            }
        }
    }
    /// <summary>
    /// 养成/指定位置装配数量X伙伴
    /// </summary>
    [System.Serializable]
    public class PosEquipPartner : BaseTaskType
    {
        [LabelText("位置（123，出战/助战/任意）")]
        public int pos;

        [LabelText("数量")]
        public int num;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.pos.ToString());
            pramas.Add(this.num.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                pos = ToInt(pramas[0]);
                num = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 养成/Z类型纹章镶嵌X星次石
    /// </summary>
    [System.Serializable]
    public class EmblemSetStarStone : BaseTaskType
    {
        [LabelText("纹章槽位（012，任意/攻击/防御）")]
        public int pos;

        [LabelText("数量")]
        public int num;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.pos.ToString());
            pramas.Add(this.num.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null && pramas.Count >= 2)
            {
                pos = ToInt(pramas[0]);
                num = ToInt(pramas[1]);
            }
        }
    }
    /// <summary>
    /// 玩法/通关X组Y难度时序残响
    /// </summary>
    [System.Serializable]
    public class PersonDailyPass : BaseTaskType
    {
        [LabelText("副本id")]
        public ulong id;

        [LabelText("难度")]
        public ulong diff;
        public override void OnSerialized(List<string> pramas)
        {
            pramas.Add(this.id.ToString());
            pramas.Add(this.diff.ToString());
        }

        public override void OnDeSerialized(List<string> pramas)
        {
            if (pramas != null)
            {
                id = Convert.ToUInt64(pramas.Count > 0 ? ToInt(pramas[0]) : 0);
                diff = Convert.ToUInt64(pramas.Count > 1 ? ToInt(pramas[1]) : 0);
            }
        }
    }
}
