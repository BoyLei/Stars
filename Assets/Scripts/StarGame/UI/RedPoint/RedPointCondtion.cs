using StarProjectDef;
using System.Collections.Generic;

/// <summary>
/// 红点的条件类。
/// 对于 红点的条件，应该分为2中:
/// 1. 配置表中 策划配置的 红点条件;
/// 2. 约定 的 代码中 添加的类型. (不好通过配置表配置的类型)
/// </summary>
public class RedPointCondtion
{
    public RedPointConditionType ConditionType;
    public object value;

    public RedPointCondtion(RedPointConditionType conditionType, object arg = null)
    {
        ConditionType = conditionType;
        value = arg;
    }
}

/// <summary>
/// 自己 定义的 红点类型 配置, 本想让策划 配置 一个 condition 表 和 红点类型 对应的 条件组 表。
/// 后面 想想 让策划配 策划不一定 好配(条件类型 程序更清楚一点). 
/// 所以 条件类型 就由 客户端 自己定义
/// </summary>
public class RedPointCfgCondtion
{
    public DictionaryEx<RedPointType, List<RedPointCondtion>> redPointType2Conditions = new();

    public void Init()
    {
        // 好友 Friend类型的 条件
        {
            var friendConditions = new List<RedPointCondtion>();

            // {
            // Friend 类型的条件 默认为 判断 Friend 的 红点数 是否 > 0;
            // friendConditions.Add(new RedPointCondtion(RedPointConditionType.Friend, null));
            // }

            /// 好友的 条件类型 配置可以分 2中:
            /// 1. 直接 配置 Friend 的条件类型,配置如上, 主界面红点 的数量 需要 系统自己 计算 FriendNotice + FriendRequest 的红点数
            /// 2. 配置 Friend 红点 关联的 FriendNotice 和 FriendRequest 类型,  红点系统 自己计算 Friend 条件关联的 红点数量. 如下：

            // FriendNotice 类型的条件 默认为 判断 FriendNotice 的 红点数 是否 > 0;
            friendConditions.Add(new RedPointCondtion(RedPointConditionType.FriendNotice, null));

            // FriendRequest 类型的条件 默认为 判断 FriendRequest 的 红点数 是否 > 0;
            friendConditions.Add(new RedPointCondtion(RedPointConditionType.FriendRequest, null));

            redPointType2Conditions.Add(RedPointType.Friend, friendConditions);
        }

        // 好友模块内部的 FriendNotice 和  FriendRequest 红点类型.
        // note:
        // 按我的想法,UI模块 内部的 红点应该是UI自己的UI状态逻辑，　红点系统应该不需要关心．
        // 此处 增加以下两种 红点, 只是按照 策划的 设计, 增加的 使用示例。(各系统自己选择 是否需要)
        {
            // FriendNotice 对应的类型的 条件
            {
                var conditions = new List<RedPointCondtion>();


                // FriendNotice 类型的条件 默认为 判断 FriendNotice 的 红点数 是否 > 0;
                conditions.Add(new RedPointCondtion(RedPointConditionType.FriendNotice, null));

                redPointType2Conditions.Add(RedPointType.FriendNotice, conditions);
            }

            // FriendRequest 对应的类型的 条件
            {
                var conditions = new List<RedPointCondtion>();

                // FriendRequest 类型的条件 默认为 判断 FriendRequest 的 红点数 是否 > 0;
                conditions.Add(new RedPointCondtion(RedPointConditionType.FriendRequest, null));

                redPointType2Conditions.Add(RedPointType.FriendRequest, conditions);
            }
        }

        // 拍卖类型 相关 的条件配置. 目前拍卖 都是侧边栏的 红点,所以条件来说 不太需要红点数据
        {
            {
                var conditions = new List<RedPointCondtion>();

                conditions.Add(new RedPointCondtion(RedPointConditionType.Auction_Union, null));

                redPointType2Conditions.Add(RedPointType.Auction_Union, conditions);
            }

            {
                var conditions = new List<RedPointCondtion>();

                conditions.Add(new RedPointCondtion(RedPointConditionType.Auction_World, null));

                redPointType2Conditions.Add(RedPointType.Auction_World, conditions);
            }

            {
                var conditions = new List<RedPointCondtion>();

                conditions.Add(new RedPointCondtion(RedPointConditionType.Auction_New, null));

                redPointType2Conditions.Add(RedPointType.Auction_New, conditions);
            }

        }

        // 抽卡相关
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Gacha1, null));
                redPointType2Conditions.Add(RedPointType.Gacha1, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Gacha2, null));
                redPointType2Conditions.Add(RedPointType.Gacha2, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Gacha3, null));
                redPointType2Conditions.Add(RedPointType.Gacha3, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Gacha4, null));
                redPointType2Conditions.Add(RedPointType.Gacha4, conditions);
            }
        }

        //药品相关
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Medicine1, null));
                redPointType2Conditions.Add(RedPointType.Medicine1, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Medicine2, null));
                redPointType2Conditions.Add(RedPointType.Medicine2, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Medicine3, null));
                redPointType2Conditions.Add(RedPointType.Medicine3, conditions);
            }
        }

        //转职
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.TransJob, null));
                redPointType2Conditions.Add(RedPointType.TransJob, conditions);
            }
        }

        //技能书
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.SkillBook, null));
                redPointType2Conditions.Add(RedPointType.SkillBook, conditions);
            }
        }

        //排行榜
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Rank, null));
                redPointType2Conditions.Add(RedPointType.Rank, conditions);
            }
        }


        //队伍
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.TeamGroup, null));
                redPointType2Conditions.Add(RedPointType.TeamGroup, conditions);
            }
        }
        //冒险等级
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Adventure, null));
                redPointType2Conditions.Add(RedPointType.Adventure, conditions);
            }
        }

        //公会
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Union_Apply, null));
                redPointType2Conditions.Add(RedPointType.Union_Apply, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Union_Notice, null));
                redPointType2Conditions.Add(RedPointType.Union_Notice, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Union_ChangeFunc, null));
                redPointType2Conditions.Add(RedPointType.Union_ChangeFunc, conditions);
            }
        }

        //商业化
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.MoonCard, null));
                redPointType2Conditions.Add(RedPointType.MoonCard, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.FirstCharge, null));
                redPointType2Conditions.Add(RedPointType.FirstCharge, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.BattlePass, null));
                redPointType2Conditions.Add(RedPointType.BattlePass, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.BattlePass_Task, null));
                redPointType2Conditions.Add(RedPointType.BattlePass_Task, conditions);
            }
        }

        //背包
        /*
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Bag_Uequiped, null));
                redPointType2Conditions.Add(RedPointType.Bag_Uequiped, conditions);
            }
        }
        */

        //符文
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Amulet_Uequiped, null));
                redPointType2Conditions.Add(RedPointType.Amulet_Uequiped, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Emb_Uequiped, null));
                redPointType2Conditions.Add(RedPointType.Emb_Uequiped, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.Amulet_Polish, null));
                redPointType2Conditions.Add(RedPointType.Amulet_Polish, conditions);
            }
        }


        //伙伴目标
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.PartnerTask, null));
                redPointType2Conditions.Add(RedPointType.PartnerTask, conditions);
            }
        }

        //个人爬塔
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.PersonTowerAward, null));
                redPointType2Conditions.Add(RedPointType.PlayRedPointPersonTowerAward, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.PersonTowerLevel, null));
                redPointType2Conditions.Add(RedPointType.PlayRedPointPersonTowerLevel, conditions);
            }
        }

        //环任务
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.RingTaskNotReceive, null));
                redPointType2Conditions.Add(RedPointType.PlayRedPointRingTaskNotReceive, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.DailyAct, null));
                redPointType2Conditions.Add(RedPointType.DailyAct, conditions);
            }


        }
        //个人秘境
        {

        }

        //装备相关
        {
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.EquipCanUpgrade, null));
                redPointType2Conditions.Add(RedPointType.PlayRedPointEquipCanUpgrade, conditions);
            }
            {
                var conditions = new List<RedPointCondtion>();
                conditions.Add(new RedPointCondtion(RedPointConditionType.EquipCanRefine, null));
                redPointType2Conditions.Add(RedPointType.PlayRedPointEquipCanRefine, conditions);
            }
        }

        //伙伴红点
        RegisterPartnerConditon();

        RegisterMailRedPointConditon();
        InitTest();
    }

    public List<RedPointCondtion> emptyConditions = new();

    public List<RedPointCondtion> GetPointTypeConditions(RedPointType redPointType)
    {
        if (!redPointType2Conditions.ContainsKey(redPointType))
        {
            return emptyConditions;
        }

        return redPointType2Conditions[redPointType];
    }

    #region 伙伴红点

    private void RegisterPartnerConditon()
    {


        //新伙伴
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_New, null));
            redPointType2Conditions.Add(RedPointType.Partner_New, conditions);

        }

        //Partner_CanInBattle
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanInBattle, null));
            redPointType2Conditions.Add(RedPointType.Partner_CanInBattle, conditions);
        }

        //Partner_CanAssist
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanAssist, null));
            redPointType2Conditions.Add(RedPointType.Partner_CanAssist, conditions);
        }

        //Partner_Team
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanInBattle, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanAssist, null));
            redPointType2Conditions.Add(RedPointType.Partner_Team, conditions);
        }

        //Partner_Level
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Level, null));
            redPointType2Conditions.Add(RedPointType.Partner_Level, conditions);
        }


        //Partner_Equip
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Equip, null));
            redPointType2Conditions.Add(RedPointType.Partner_Equip, conditions);
        }


        //Partner_Upgrade
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_UpGrade, null));
            redPointType2Conditions.Add(RedPointType.Partner_Upgrade, conditions);
        }


        //Partner_Construct
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Construct, null));
            redPointType2Conditions.Add(RedPointType.Partner_Construct, conditions);
        }

        //Partner
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanInBattle, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_CanAssist, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Level, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Equip, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_UpGrade, null));
            conditions.Add(new RedPointCondtion(RedPointConditionType.Partner_Construct, null));
            redPointType2Conditions.Add(RedPointType.Partner, conditions);
        }
    }


    private void RegisterMailRedPointConditon()
    {
        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Mail_New, null));
            redPointType2Conditions.Add(RedPointType.Mail_New, conditions);
        }

        {
            var conditions = new List<RedPointCondtion>();
            conditions.Add(new RedPointCondtion(RedPointConditionType.Mail_Red, null));
            redPointType2Conditions.Add(RedPointType.Mail_Red, conditions);
        }
    }
    #endregion


    /// <summary>
    /// 测试的 红点数据
    /// 
    /// </summary>
    private void InitTest()
    {
        {
            var testConditions = new List<RedPointCondtion>();
            testConditions.Add(new RedPointCondtion(RedPointConditionType.Entrance, null));

            redPointType2Conditions.Add(RedPointType.Entrance, testConditions);
        }

        {
            var testConditions = new List<RedPointCondtion>();
            testConditions.Add(new RedPointCondtion(RedPointConditionType.SubType_Node1, null));

            redPointType2Conditions.Add(RedPointType.SubType_Node1, testConditions);
        }

        // SubType_Node2 --> [SubType_Node2,SubType5]
        {
            var testConditions = new List<RedPointCondtion>();
            testConditions.Add(new RedPointCondtion(RedPointConditionType.SubType_Node2, null));


            testConditions.Add(new RedPointCondtion(RedPointConditionType.SubType5, true));

            redPointType2Conditions.Add(RedPointType.SubType_Node2, testConditions);
        }

        // SubType3 ---> SubType3
        {
            var testConditions = new List<RedPointCondtion>();
            testConditions.Add(new RedPointCondtion(RedPointConditionType.SubType3, null));

            redPointType2Conditions.Add(RedPointType.SubType3, testConditions);
        }

        // SubType4 ---> SubType4
        {
            var testConditions = new List<RedPointCondtion>();
            testConditions.Add(new RedPointCondtion(RedPointConditionType.SubType4, null));

            redPointType2Conditions.Add(RedPointType.SubType4, testConditions);
        }

        // 初始化 红点数据
        RedPointManager.Instance.RefreshRedPointCount(RedPointType.Entrance, 10);
        RedPointManager.Instance.RefreshRedPointCount(RedPointType.SubType_Node1, 1);
        RedPointManager.Instance.RefreshRedPointCount(RedPointType.SubType_Node2, 0);



        RedPointManager.Instance.RefreshRedPointCount(RedPointType.SubType3, 1);
        RedPointManager.Instance.RefreshRedPointCount(RedPointType.SubType4, 0);

    }
}
;