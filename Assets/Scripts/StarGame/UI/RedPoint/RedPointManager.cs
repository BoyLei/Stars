using System.Collections.Generic;
using SGF.Module.Framework;
using StarProjectDef;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProject.Service.Battle;
using StarProject.Service.SystemOpen;
using UnityEngine.SocialPlatforms;
using UnityEngine;

[XLua.LuaCallCSharp]
public class RedPointManager : ServiceModule<RedPointManager>
{
    /// <summary>
    /// 条件对应的 RedPoint 点
    /// </summary>
    private Dictionary<RedPointConditionType, HashSet<RedPoint>> conditionType2Points = new();

    /// <summary>
    /// 红点类型 对应的 RedPoint 组
    /// </summary>
    private Dictionary<RedPointType, HashSet<RedPoint>> redPointType2Posints = new();

    /// <summary>
    /// 红点数量
    /// </summary>
    private Dictionary<RedPointType, int> RedPointCount = new();

    private RedPointCfgCondtion redPointCfgCondtion = new RedPointCfgCondtion();

    /// <summary>
    /// 可以存在 RedPointManager中, 也可以 存在外部(比如 本地缓存)，在checkCondition中检查.
    /// </summary>
    public DictionaryEx<RedPointConditionType, bool> recordBool = new();

    private bool _init = false;

    public void Init()
    {
        if (_init)
        {
            return;
        }

        _init = true;
        redPointCfgCondtion.Init();
    }

    public void Clear()
    {
        RedPointCount.Clear();
    }

    /// <summary>
    /// 增加 condition 对应的 redPoint
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="redPoint"></param>
    private void AddCondition2RedPoint(RedPointCondtion condition, RedPoint redPoint)
    {
        RedPointConditionType conditionType = condition.ConditionType;

        if (!conditionType2Points.TryGetValue(conditionType, out var redPointsSet))
        {
            redPointsSet = new HashSet<RedPoint>();
            conditionType2Points.Add(conditionType, redPointsSet);
        }

        if (!redPointsSet.Contains(redPoint))
        {
            redPointsSet.Add(redPoint);
        }
    }

    /// <summary>
    /// 取消 condition 对应的 redPoint
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="redPoint"></param>
    private void RemoveConditionRedPoint(RedPointCondtion condition, RedPoint redPoint)
    {
        RedPointConditionType conditionType = condition.ConditionType;

        if (!conditionType2Points.TryGetValue(conditionType, out var redPointsSet))
        {
            return;
        }

        if (redPointsSet.Contains(redPoint))
        {
            redPointsSet.Remove(redPoint);
        }
    }

    public List<RedPointCondtion> GetConfigRedPointConditions(RedPointType redPointType)
    {
        return redPointCfgCondtion.GetPointTypeConditions(redPointType);
    }

    /// <summary>
    /// 注册 redPoint 的 监听
    /// </summary>
    /// <param name="redPoint"></param>
    public void RegisterRedPoint(RedPoint redPoint)
    {
        if (!_init)
        {
            Init();
        }

        RegiseterPointTypeCondition2RedPoint(redPoint.redPointType, redPoint);

        RegisterPointType2RedPoint(redPoint.redPointType, redPoint);
    }

    /// <summary>
    /// 取消 redPoint 相关的监听
    /// </summary>
    /// <param name="redPoint"></param>
    public void UnRegisterRedPoint(RedPoint redPoint)
    {
        UnRegiseterPointTypeCondition2RedPoint(redPoint.redPointType, redPoint);

        UnRegisterPointType2RedPoint(redPoint.redPointType, redPoint);
    }

    /// <summary>
    /// 注册 redPointType 的 cfg condition 到 redPoint
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="redPoint"></param>
    public void RegiseterPointTypeCondition2RedPoint(RedPointType redPointType, RedPoint redPoint)
    {
        if (redPointType == RedPointType.None || redPointType == RedPointType.Group)
        {
            return;
        }

        var conditions = GetConfigRedPointConditions(redPointType);
        conditions.ForEach((condition) => { AddCondition2RedPoint(condition, redPoint); });
    }

    /// <summary>
    /// 取消 红点类型的 cfg conditon 对应的 redPoint
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="redPoint"></param>
    public void UnRegiseterPointTypeCondition2RedPoint(RedPointType redPointType, RedPoint redPoint)
    {
        if (redPointType == RedPointType.None || redPointType == RedPointType.Group)
        {
            return;
        }

        var conditions = GetConfigRedPointConditions(redPointType);
        conditions.ForEach((condition) => { RemoveConditionRedPoint(condition, redPoint); });
    }

    /// <summary>
    /// 注册 redPointType 类型关联的 redPoint
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="redPoint"></param>
    public void RegisterPointType2RedPoint(RedPointType redPointType, RedPoint redPoint)
    {
        if (redPointType == RedPointType.None || redPointType == RedPointType.Group)
        {
            return;
        }

        if (!redPointType2Posints.ContainsKey(redPointType))
        {
            redPointType2Posints.Add(redPointType, new HashSet<RedPoint>());
        }

        if (!redPointType2Posints[redPointType].Contains(redPoint))
        {
            redPointType2Posints[redPointType].Add(redPoint);
        }
    }

    /// <summary>
    /// 取消 redPointType 类型关联的 redPoint
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="redPoint"></param>
    public void UnRegisterPointType2RedPoint(RedPointType redPointType, RedPoint redPoint)
    {
        if (redPointType == RedPointType.None || redPointType == RedPointType.Group)
        {
            return;
        }

        if (!redPointType2Posints.ContainsKey(redPointType))
        {
            return;
        }

        if (redPointType2Posints[redPointType].Contains(redPoint))
        {
            redPointType2Posints[redPointType].Remove(redPoint);
        }
    }

    /// <summary>
    /// 刷新 数字类型红点的数量
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="count"></param> 
    /// <summary>
    public void RefreshRedPointCount(RedPointType redPointType, int count)
    {
        RedPointCount[redPointType] = count;

        // 红点数字 刷新的时候, 通知 redPointType 关联的 所有类型 redPoint
        if (redPointType2Posints.ContainsKey(redPointType))
        {
            foreach (var redPoint in redPointType2Posints[redPointType])
            {
                redPoint.MarkDirty();
            }
        }
    }

    public int GetRedPointTypeCount(RedPointType redPointType)
    {
        if (RedPointCount.ContainsKey(redPointType))
        {
            return RedPointCount[redPointType];
        }

        return 0;
    }


    /// <summary>
    /// 系统触发 红点的条件检查. 条件 并不一定对应数量, 可能这个条件 是个 范围检查.
    /// 所以 某个条件 改变值的时候， 触发这个条件 相关的 所有红点的 条件检查 
    /// </summary>
    /// <param name="redPointConditionType"></param> 
    public void TriggerConditionType(RedPointConditionType redPointConditionType)
    {
        if (!conditionType2Points.ContainsKey(redPointConditionType))
        {
            return;
        }

        var redPoints = conditionType2Points[redPointConditionType];

        foreach (var redPoint in redPoints)
        {
            // 通知 conditionType 对应的红点刷新
            redPoint.MarkDirty();
        }
    }


    /// <summary>
    /// 触发 条件类型的 bool 值
    /// </summary>
    /// <param name="redPointConditionType"></param>
    /// <param name="value"></param>
    public void TriggerConditionTypeBoolValue(RedPointConditionType redPointConditionType, bool value)
    {
        recordBool[redPointConditionType] = value;
        TriggerConditionType(redPointConditionType);
    }

    /// <summary>
    /// 如果 条件类型 存储了 true 值, 那就返回这个条件 为 true
    /// </summary>
    /// <param name="redPointConditionType"></param>
    public bool HasRecordBool(RedPointConditionType redPointConditionType)
    {
        if (recordBool.ContainsKey(redPointConditionType) && recordBool[redPointConditionType])
        {
            return true;
        }

        return false;
    }


    public void CloseAllConditionTypeBoolValue(RedPointType redPointType)
    {
        if (redPointType == RedPointType.None || redPointType == RedPointType.Group)
        {
            return;
        }

        var conditions = GetConfigRedPointConditions(redPointType);
        conditions.ForEach((condition) => { recordBool[condition.ConditionType] = false; });
    }


    /// <summary>
    /// 检查 红点类型 对应 的 条件是否满足
    /// </summary>
    /// <param name="redPointType"></param>
    public bool CheckRedPointTypeCfgConditions(RedPointType redPointType)
    {
        var conditions = GetConfigRedPointConditions(redPointType); //根据红点类型，获取红点condition
        foreach (var item in conditions)
        {
            if (CheckCondition(redPointType, item)) //根据红点类别和红点条件类别，去获取count或者bool
            {
                return true;
            }
        }

        // 如果未配置条件, 那就直接走 红点数 或者 红点记录判定
        if (conditions.Count == 0)
        {
            // 如果 这个类型的红点数 > 0, 那就直接 return true;
            if (GetRedPointTypeCount(redPointType) > 0)
            {
                return true;
            }
        }

        return false;
    }


    public bool CheckCondition(RedPointType redPointType, RedPointCondtion redPointCondtion)
    {
        // 如果 这个类型的红点数 > 0, 那就直接 return true;
        if (GetRedPointTypeCount(redPointType) > 0)
        {
            return true;
        }

        if (HasRecordBool(redPointCondtion.ConditionType))
        {
            return true;
        }

        switch (redPointType)
        {
            case RedPointType.Email:
            case RedPointType.FriendNotice:
            case RedPointType.FriendRequest:
            case RedPointType.Auction_Union:
            case RedPointType.Auction_World:
            case RedPointType.Auction_New:
            case RedPointType.TeamGroup:


            {
                // 默认就是判断 > 0, 上面已经判定, 此处就不需要处理
            }
                break;

            #region 商业化

            case RedPointType.FirstCharge:
            case RedPointType.MoonCard:
            case RedPointType.BattlePass:
            case RedPointType.BattlePass_Task:

            #endregion

            case RedPointType.Gacha1:
            case RedPointType.Gacha2:
            case RedPointType.Gacha3:
            case RedPointType.Gacha4:

            #region 公会

            case RedPointType.Union_Apply:
            case RedPointType.Union_Notice:
            case RedPointType.Union_ChangeFunc:

            #endregion

            case RedPointType.Adventure:


            case RedPointType.Medicine1:
            case RedPointType.Medicine2:
            case RedPointType.Medicine3:
            {
                // 默认的条件 recordBool 值判定, 上面已经处理，此处不用干
            }
                break;
            case RedPointType.Friend:
            {
                /// 红点类型 的条件判断 有2种情况:
                /// 1. 直接判断 RedPointCount 中 是否有 Friend 类型的红点数. 
                ///    这种需要 ui 模块在 收到 FriendRequest 或者 FriendNotice 的时候,
                ///    自己去 更新 同步  Friend 类型的红点数.
                /// 
                /// 2. 红点系统 自己去 计算 Friend 关联的 FriendRequest 和 FriendNotice 的红点数.
                /// 
                /// 示例采用 2 的方式判定 好友入口的 红点是否需要显示
                /// 
                /// note:
                /// 好友最外层的 入口 红点 有两种配置方法(假设FriendRequest 和 FriendNotice 红点在弹窗上, 所以没办法直接关联 redPointGroup[子红点组]):
                /// 1. 红点类型 为 Friend + redPointTypeGroup[关联类型组 {FriendRequest,FriendNotice}] ;
                /// 2. 红点类型 为 Group + redPointTypeGroup[关联类型组 {FriendRequest,FriendNotice}]  ;
                /// 
                /// 对于1, 入口红点 走的是 Friend类型 配置的 类型判定条件: Friend; 然后进入 此处判断逻辑;
                /// 对于2, 入口红点 走的是 关联的 红点类型的判断条件 : FriendRequest 和 FriendNotice, 并不会走 Friend 的条件检查.
                /// 
                /// 以上两种情况都可以,看 各个模块自己意愿
                if (GetRedPointTypeCount(RedPointType.FriendRequest) > 0)
                {
                    return true;
                }

                if (GetRedPointTypeCount(RedPointType.FriendNotice) > 0)
                {
                    return true;
                }
            }
                break;

            case RedPointType.TransJob:
            {
                // 转职
                return GetIsShowTransJobRedpoint();
            }
            //break;

            #region 接下来几种 condition 都是测试用的condition

            case RedPointType.Entrance:
            {
                if (GetRedPointTypeCount(RedPointType.Entrance) > 0)
                {
                    return true;
                }
            }
                break;
            case RedPointType.SubType_Node1:
            {
                if (GetRedPointTypeCount(RedPointType.SubType_Node1) > 0)
                {
                    return true;
                }
            }
                break;
            case RedPointType.SubType_Node2:
            {
                // 如下， SubType_Node2 是 2个条件 。 怎么判定 2个条件 由 系统模块自己定义写法。 
                if (GetRedPointTypeCount(RedPointType.SubType_Node2) > 0 &&
                    recordBool[RedPointConditionType.SubType5] == true)
                {
                    return true;
                }
            }
                break;
            case RedPointType.SubType3:
            {
                if (GetRedPointTypeCount(RedPointType.SubType3) > 0)
                {
                    return true;
                }
            }
                break;
            case RedPointType.SubType4:
            {
                if (GetRedPointTypeCount(RedPointType.SubType4) > 0)
                {
                    return true;
                }
            }
                break;
            case RedPointType.Partner_New:
            {
                return LocalCache.GetPartnerRedPointData.NewPartners.Count > 0;
                //break;
            }

            case RedPointType.Partner_CanInBattle:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    if (partnerMgr.FreePartnerCount() > 0)
                    {
                        if (partnerMgr.GetInBattleCount() < 3)
                        {
                            return true;
                        }
                    }
                }

                break;
            }

            case RedPointType.Partner_CanAssist:
            {
                bool isOpen = SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_Assist);
                if (isOpen)
                {
                    var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                    if (partnerMgr != null)
                    {
                        if (partnerMgr.FreePartnerCount() > 0)
                        {
                            uint job = GameManager.Instance.GetPlayerJob();
                            int assistCnt = LocalDataManager.Instance.GetPartnerAssistSlotCount(job);
                            if (partnerMgr.GetInAssistCount() < assistCnt)
                            {
                                return true;
                            }
                        }
                    }
                }


                break;
            }

            case RedPointType.Partner_Team:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    if (partnerMgr.FreePartnerCount() > 0)
                    {
                        if (partnerMgr.GetInBattleCount() < 3)
                        {
                            return true;
                        }


                        bool isOpen = SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Partner_Assist);
                        if (isOpen)
                        {
                            uint job = GameManager.Instance.GetPlayerJob();
                            int assistCnt = LocalDataManager.Instance.GetPartnerAssistSlotCount(job);
                            if (partnerMgr.GetInAssistCount() < assistCnt)
                            {
                                return true;
                            }
                        }
                    }
                }

                break;
            }

            case RedPointType.Partner_Level:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    return partnerMgr.HasPartnerStarRed();
                }
            }
                break;
            case RedPointType.Partner_Equip:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    return partnerMgr.HasPartnerEquipRed();
                }

                break;
            }

            case RedPointType.Partner_Upgrade:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    return partnerMgr.HasUpgradeRed();
                }

                break;
            }
            case RedPointType.Partner_Construct:
            {
                var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMgr != null)
                {
                    return partnerMgr.HasConstructRed();
                }

                break;
            }

            #endregion


            #region 邮件红点类型

            case RedPointType.Mail_New:
            {
                if (GetRedPointTypeCount(RedPointType.Mail_New) > 0)
                {
                    return true;
                }
            }
                break;

            case RedPointType.Mail_Red:
            {
                if (recordBool[redPointCondtion.ConditionType] == true)
                {
                    return true;
                }
            }
                break;

            #endregion


            default: break;
        }

        return false;
    }

    /// <summary>
    /// TODO: 曲
    /// 小曲加的代码， 抽个时间挪掉，管理类不应该关心任何一个具体的 逻辑实现.
    /// </summary>

    #region 转职红点逻辑

    private bool GetIsShowTransJobRedpoint()
    {
        // 是否已经转职完成了
        uint mainPlayerJob = GameManager.Instance.GetPlayerJob();
        var cfg = LocalDataManager.Instance.GetJobDataCell((int)mainPlayerJob);
        if (cfg != null && cfg.TransferJob.Count <= 0)
        {
            return false;
        }

        // 判断转职任务都领取了
        var HeroMD = GameManager.Instance.HeroMD;
        // 给曲加的判定逻辑, 返回登录后 这玩意可能为null, 但是返回登录后有些定时器没被干掉(定时器依赖于界面，界面没有被destroy)
        if (HeroMD == null)
        {
            return false;
        }

        var TransJobRewardList = HeroMD.TransJobRewardList;
        var getTaskCount = TransJobRewardList.List.Count;
        var taskCount = LocalDataManager.Instance.M_TransferConditionData.StaticTransferConditionDatas.Count;
        if (getTaskCount >= taskCount)
        {
            if (cfg.TransferJob.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 逐个判断
        foreach (var item in LocalDataManager.Instance.M_TransferConditionData.StaticTransferConditionDatas)
        {
            CMconditionDataCell cMconditionDataCell =
                LocalDataManager.Instance.GetCMconditionDataCell(item.Value.GetConditionID());
            if (cMconditionDataCell == null)
            {
                continue;
            }

            bool isGet = GetTaskIsGet(item.Key);
            if (isGet)
            {
                continue;
            }

            int conditionCfgCount = 1; // 条件数量
            int conditionCfgID = 1; // 条件配置ID
            int.TryParse(cMconditionDataCell.Args1, out conditionCfgID);
            if (cMconditionDataCell.Args2 != "")
            {
                int.TryParse(cMconditionDataCell.Args2, out conditionCfgCount);
            }
            else if (cMconditionDataCell.Args1 != "")
            {
                conditionCfgCount = conditionCfgID;
            }

            long curCount = 0;
            if (cMconditionDataCell.GetConditionType() == 1)
            {
                curCount = GameManager.Instance.GetPlayerLevel();
            }
            else if (cMconditionDataCell.GetConditionType() == 18)
            {
                int maxEquipNum = 8; // 装备槽位上限，之后读表
                for (int i = 1; i <= maxEquipNum; i++)
                {
                    var equipSlotData = GameManager.Instance.GetEquipSlotData(i);
                    if (equipSlotData != null)
                    {
                        if (equipSlotData.UpLv >= conditionCfgCount)
                        {
                            curCount += 1;
                        }
                    }
                }
            }
            else if (cMconditionDataCell.GetConditionType() == 19)
            {
                var partnerMDMgr = BusinessManager.Instance.GetPartnerMDMgr();
                if (partnerMDMgr != null)
                {
                    curCount = partnerMDMgr.GetLevelPartnerCount(conditionCfgID);
                }
            }
            else if (cMconditionDataCell.GetConditionType() == 20)
            {
                var allitems = BusinessManager.Instance.GetItemsByItemType(5, 0);
                if (allitems != null)
                {
                    for (int i = 0; i < allitems.Count; i++)
                    {
                        var it = allitems[i];
                        var combatSkills = it.CombatSkills;
                        int curPolish = combatSkills.CurPolish; //已经打磨的次数
                        var itemCfg = LocalDataManager.Instance.GetItemDataCell(it.BaseID);
                        int maxNum = LocalDataManager.Instance.GetMaxPolishNum(itemCfg.GetQuality()); //打磨次数的上限

                        if (curPolish == maxNum)
                        {
                            curCount += 1;
                        }
                    }
                }
            }
            else if (cMconditionDataCell.GetConditionType() == 21)
            {
                curCount = BusinessManager.Instance.GetWearEquipQualityCount(conditionCfgID);
            }
            else if (cMconditionDataCell.GetConditionType() == 22)
            {
                curCount = GameManager.Instance.HeroSkillTotalLevel();
            }
            else if (cMconditionDataCell.GetConditionType() == 27)
            {
                int args3 = 1; // 条件配置ID
                int.TryParse(cMconditionDataCell.Args3, out args3);
                curCount = FightPowerManager.Instance.GetSeverPowerByModuleType(conditionCfgID, args3 == 0);
            }
            else if (cMconditionDataCell.GetConditionType() == 28)
            {
                var skillPoint = GameManager.Instance.GetSkillPoint();
                var skillPointUsed = GameManager.Instance.GetSkillPointUsed();
                curCount = skillPoint + skillPointUsed;
            }

            if (conditionCfgCount != 0 && curCount >= conditionCfgCount)
            {
                return true;
            }
        }

        return false;
    }

    private bool GetTaskIsGet(int taskID)
    {
        var HeroMD = GameManager.Instance.HeroMD;
        var TransJobRewardList = HeroMD.TransJobRewardList;
        for (int i = 0; i < TransJobRewardList.List.Count; i++)
        {
            int item = TransJobRewardList.List[i];
            if (taskID == item)
            {
                return true;
            }
        }

        return false;
    }

    #endregion


    /// <summary>
    /// 测试红点类型 对应的 数量变化的 流程
    /// </summary>
    /// <param name="redPointType"></param>
    /// <param name="add"></param> 
    public void TestCount(RedPointType redPointType, bool add)
    {
        var count = GetRedPointTypeCount(redPointType);
        count = add ? count + 1 : count - 1;
        count = count < 0 ? 0 : count;

        // SGF.Debuger.LogError($"[Test] update redPointType: {redPointType} add: {add}, count: {count}");
        RedPointManager.Instance.RefreshRedPointCount(redPointType, count);
    }


    /// <summary>
    /// 测试 监听某个 套件的bool 值 变化的 流程 
    /// </summary>
    /// <param name="RedPointConditionType"></param>
    /// <param name="result"></param>
    public void TestBool(RedPointConditionType conditionType, bool result)
    {
        recordBool[conditionType] = result;
        // SGF.Debuger.LogError($"[Test] conditionType: {conditionType} result: {result}");

        TriggerConditionType(conditionType);
    }
}