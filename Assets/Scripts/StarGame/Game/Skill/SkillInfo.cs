using ProtoMsg;
using SGF.Module.Framework;
using SGF.Time;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Game.Skill
{
    public class SkillInfo : EntityRemoteStatic
    {

        private string TagFlag
        {
            get => $"[{playerData?.M_EntityID}] [SkillInfo] skillID {skillId} ";
        }

        private VitalSignData playerData;
        public int skillId = 0;

        public SkillConfig cfg;

        public SkillWheelInfo skillWheelInfo = new();

        public float MinCastDistance => cfg.MinCastDistance;

        public float AICastDistance => cfg.AICastDistance;

        /// <summary>
        /// 技能的 自动释放 距离, 目前 先采用 最小/最大 释放距离的中心点
        /// </summary>
        public float SkillAutoUseDistance => (cfg.MinCastDistance + cfg.AICastDistance) / 2f;

        public List<StageJson> stageJsons;

        public List<StageJson> otherStageJsons;

        private List<EnumSkillSlotSpShow> m_SkillSlotSpShow = new();

        /// <summary>
        /// 阶段 stageID 对 stageJson 的映射
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="StageJson"></typeparam>
        /// <returns></returns>
        public Dictionary<int, StageJson> StageJsonsDic = new();

        /// <summary>
        /// 释放技能时,是否自动转向
        /// </summary>
        public bool IsAutoTurnToTarget => cfg != null ? cfg.IsAutoTurnToTarget : true;

        private int _curStageIdx = 0;

        // 当前阶段是否需要抢占活跃
        public bool NeedActive => CurStageJson.StageType == StageType.NormalStage && CurStageJson.StageNormal.Active;

        public int CDKey => cfg.SkillCDKey;

        private StageJson CurStageJson
        {
            get => stageJsons[_curStageIdx];
        }

        public SkillContainer skillContainer;


        private TimeLineStageInfos timeLineStageInfos;

        /// <summary>
        /// 其它阶段的 timeline 信息
        /// </summary>
        private TimeLineStageInfos timeLineOtherStageInfos;


        // 进入CD是否抬起按钮
        public bool IsCDCloseTouch => cfg.IsCDCloseTouch;

        private int skillLevel = 1;

        private int serverSkillLevel = 1;


        /// <summary>
        /// 技能的类型.  普工/宠物 等 . 本质上 只会在 技能槽中存在.
        /// 在技能槽 填充技能的时候, 设置技能 的 类型
        /// </summary>
        private E_SkillSetType skillSetType;


        /// <summary>
        /// 技能的优先级,肯定是配置表中配置所得
        /// </summary>
        /// <value></value>
        public int Priority
        {
            get => cfg.Priority;
        }

        /// <summary>
        /// 技能标签类型, 目前 对于 伙伴技能而言, 客户端主动使用伙伴技能 不需要预播(因为此时 伙伴可能是服务器再使用技能等),
        /// 所以 客户端使用伙伴技能, 每次释放 都是 给服务器 发送释放协议即可, 同时 与服务器约定 释放 协议的runtimeID 为0
        /// </summary>
        public SkillTag SkillTag => cfg.SkillTag;

        private bool isCleanse = false;
        /// <summary>
        /// 是否是 进化技能
        /// </summary>
        public bool IsCleanse => isCleanse;

        /// <summary>
        /// 技能转向的 最大时间
        /// </summary>
        public int SkillTurnAroundTime => cfg.SkillTurnAroundTime;

        public bool IsShow => !cfg.IsHidden;

        private List<ConsumeConfig> consumes;

        public string SkillComplexConfig => $"skill_{skillId}_[{GameConfig.SKILL_BTN_TYPE_ENUM}]";

        public List<int> Labs = new();

        public void Create(int _skillId, VitalSignData _playerData, SkillContainer _skillContainer, bool reload = false)
        {
            playerData = _playerData;

            skillContainer = _skillContainer;
            timeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();

            timeLineOtherStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();

            skillId = _skillId;
            Labs.Clear();

            LocalDataManager.Instance.GetSkillJson(_skillId, (SkillJson skillJson) =>
             {
                 if (skillJson == null)
                 {
                     //DB_Close   SGF.Debuger.LogError($"{TagFlag} skillId : {skillId}  token cfg error!!!");
                     return;
                 }

                 cfg = skillJson.config;
                 skillWheelInfo.Init(cfg);

                 consumes = cfg.Consume;

                 stageJsons = skillJson.Normals;

                 otherStageJsons = skillJson.Others;

                 isCleanse = cfg.SkillTag == SkillTag.Cleanse;

                 InitStageJsonDic(stageJsons);
                 InitStageJsonDic(otherStageJsons);
                 InitSkillSlotSpShow();

                 timeLineStageInfos.InitFrameEvents(stageJsons, _skillId);
                 timeLineOtherStageInfos.InitFrameEvents(otherStageJsons, _skillId);


                 List<int> skillLabels = cfg.SkillLabels.ConvertAll((lab) => (int)lab);

                 Labs.KUnion(skillLabels, (a, b) => a == b);

                 UIConfigDataCell dataCell = LocalDataManager.Instance.GetUIConfigDataCell(GameConfig.SKILL_BTN_TYPE_ENUM);
                 if (dataCell != null)
                 {
                     Labs.KUnion(dataCell.Labels, (a, b) => a == b);
                 }

                 InitSkillArea();

             }, reload);


        }

        private void InitStageJsonDic(List<StageJson> stages)
        {
            stages.ForEach((StageJson stage) =>
            {
                int stageID = stage.StageID;
                if (StageJsonsDic.ContainsKey(stageID))
                {
                    return;
                }
                StageJsonsDic.Add(stageID, stage);
            });
        }

        public StageJson GetStageJson(int stageID)
        {
            if (StageJsonsDic.ContainsKey(stageID))
            {
                return StageJsonsDic[stageID];
            }
            return null;
        }

        public TimeLineStage GetTimeLineStage(int stageID, int loop)
        {
            return timeLineStageInfos.GetTimeLineStage(stageID, loop);
        }

        public TimeLineStage GetOtherTimeLineStage(int stageID)
        {
            return timeLineOtherStageInfos.GetTimeLineStage(stageID, 0);
        }


        private int GetConsumeCost(ConsumeType costType)
        {
            int cost = 0;
            if (consumes.Count == 0)
            {
                return cost;
            }
            ConsumeConfig consume = consumes.Find((ConsumeConfig consumeConfig) => { return consumeConfig.CostType == costType; });
            if (consume != null)
            {
                cost = consume.Cost;
            }

            return cost;
        }

        private int GetReallyMpConsume()
        {
            int consumeMp = 0;


            if (cfg.Consume.Count == 0)
            {
                return consumeMp;
            }

            //计算真实的蓝耗,需要综合玩家属性判断,目前只是简单的读表
            consumeMp = GetConsumeCost(ConsumeType.Con_Mp);

            return consumeMp;
        }

        /// <summary>
        /// 只检查技能消耗
        /// </summary>
        /// <returns>E_UseSkillResult</returns>
        public E_UseSkillResult CheckCanUseSkill()
        {
            // 判断MP是否足够
            int mp = playerData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.curMp);
            int consumeMp = GetReallyMpConsume();
            if (mp < consumeMp)
            {
                return E_UseSkillResult.MP_Not_Enough;
            }
            // 如果是净化技能, 就不考虑是否被原子锁锁住
            if (isCleanse)
            {
                return E_UseSkillResult.Succeed;
            }

            bool isNormalSkill = IsNormalSkill();

            // 如果是普工, 就判断是否被原子锁 锁住普工
            if (isNormalSkill && playerData.myOwnerNtt.Data.IsForbidAttack)
            {
                return E_UseSkillResult.ForbidAttack;
            }

            // 如果不是普工, 就看 原子锁 是否锁住 技能, 锁住技能 也是返回 E_UseSkillResult.ForbidAttack 
            if (!isNormalSkill && playerData.myOwnerNtt.Data.IsForbidSkill)
            {
                return E_UseSkillResult.ForbidAttack;
            }

            return E_UseSkillResult.Succeed;
        }

        public List<SkillCondition> GetSkillCondtions()
        {
            return cfg.Conditions;
        }

        /// <summary>
        /// 获得当前技能 不满足 属性的 条件
        /// </summary>
        public ConditionAttr NotMatchAtrrCondition(out bool isOver)
        {
            isOver = false;
            if (cfg == null)
            {
                return null;
            }

            foreach (SkillCondition skillCondition in cfg.Conditions)
            {
                ConditionTypeSerialize condition = skillCondition.ConditionParams;

                ConditionType conditionType = condition.ConditionType;

                if (ConditionType.Cond_Attr == conditionType)
                {

                    {
                        ConditionAttr conditionAttr = condition.Cond_Attr;
                        VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)conditionAttr.ID);
                        if (vitalSignAOIClientAttrs != null)
                        {
                            long val = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, vitalSignAOIClientAttrs.Name);
                            if (val > conditionAttr.Max)
                            {
                                isOver = true;
                                return conditionAttr;
                            }
                            if (val < conditionAttr.Min)
                            {
                                isOver = false;
                                return conditionAttr;
                            }
                        }
                    }
                    break;
                }
            }

            return null;
        }

        public string GetAttrTips()
        {
            var attr = NotMatchAtrrCondition(out var isOver);
            if (attr == null)
            {
                return string.Empty;
            }
            // return $"属性:{attr.ID} {isOver ? '大于':'小于'} ";
            return "";
        }


        public bool CheckConditions()
        {
            var skillController = skillContainer.skillUnitController.skillDispatcher.SkillController;
            if (cfg == null)
            {
                return false;
            }
            /// SkillCondition 表中字段注释:
            /// ConditionType:
            ///     1: buff 层数,  KeyParam 表示 buff ID, RangeParams [0,2] 表示  该条件buff在 [0-2]层 期间可以释放该技能
            ///     2: 属性 要求,   KeyParam 表示 属性 ID, RangeParams [0,2] 表示  该条件属性在 [0-2]   期间可以释放该技能
            ///     3: 前置 条件,   KeyParam 表示 技能 ID, RangeParams [0,2] 表示  该条件技能在 [0-2]s  期间可以释放该技能
            foreach (SkillCondition skillCondition in cfg.Conditions)
            {
                ConditionTypeSerialize condition = skillCondition.ConditionParams;

                ConditionType conditionType = condition.ConditionType;

                switch (conditionType)
                {
                    case ConditionType.Cond_Buff:
                        {
                            ConditionBuff conditionBuff = condition.Cond_Buff;
                            int buffID = conditionBuff.ID;
                            var skillBuff = skillController.GetSkillBuffWithBuffID(buffID);
                            int val = skillBuff == null ? 0 : skillBuff.GetBuffStackCount();
                            // SGF.Debuger.LogError($"skillId: {skillId} check buff: {buffID} , curCount: {val}, max: {conditionBuff.Max}, min: {conditionBuff.Min} , result: {val > conditionBuff.Max || val < conditionBuff.Min}");
                            if (val > conditionBuff.Max || val < conditionBuff.Min)
                            {
                                return false;
                            }
                        }
                        break;
                    case ConditionType.Cond_Attr:
                        {
                            ConditionAttr conditionAttr = condition.Cond_Attr;
                            VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)conditionAttr.ID);
                            if (vitalSignAOIClientAttrs != null)
                            {
                                long val = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, vitalSignAOIClientAttrs.Name);
                                if (val > conditionAttr.Max || val < conditionAttr.Min)
                                {
                                    return false;
                                }
                            }
                        }
                        break;
                    case ConditionType.Cond_SkillTime:
                        {
                            ConditionSkill conditionSkill = condition.Cond_SkillTime;

                            long startCDTime = skillContainer.skillUnitController.GetSkillCDStartTime(conditionSkill.ID);
                            long val = TimeUtils.ClientNowStampMilli - startCDTime;
                            if (val > conditionSkill.Max || val < conditionSkill.Min)
                            {
                                return false;
                            }
                        }
                        break;
                    case ConditionType.Cond_HpPercent:
                        {
                            ConditionHpPercent conditionSkill = condition.Cond_HpPercent;

                            long truthHp = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);
                            long curHp = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
                            float hpPrechent = curHp * 1.0f / truthHp;

                            if (hpPrechent > conditionSkill.Max || hpPrechent < conditionSkill.Min)
                            {
                                return false;
                            }
                        }
                        break;
                    case ConditionType.Condition_CurSpectralElem:
                        {
                            ConditionCurSpectralElem conditionSkill = condition.Condition_CurSpectralElem;

                            int CheckValue = conditionSkill.CheckValue;
                            long spectralValue = 0;

                            switch (conditionSkill.Spectral)
                            {
                                case Spectral.Spectral1:
                                    {
                                        spectralValue = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral1);
                                    }
                                    break;
                                case Spectral.Spectral2:
                                    {
                                        spectralValue = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral2);
                                    }
                                    break;
                                case Spectral.Spectral3:
                                    {
                                        spectralValue = playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curSpectral3);
                                    }
                                    break;
                                default:
                                    {
                                        //DB_Close   SGF.Debuger.LogError($"{TagFlag} CheckConditions cfg {cfg.ID} , type {conditionType}, Spectral: {conditionSkill.Spectral}, checkValue: {CheckValue} no handle!!!");
                                    }
                                    break;
                            }

                            // 先假定 个 int64 的值
                            int high8 = Fire.Utils.GetInt64High2Low8(spectralValue);

                            //SGF.Debuger.Log($"{TagFlag} CheckConditions cfg {cfg.ID}, type {conditionType}, Spectral: {conditionSkill.Spectral}, CheckValue: {CheckValue}, spectralValue: {spectralValue} , hig8: {high8}, 是等于: {CheckValue == high8} ");
                            if (CheckValue != high8)
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        {
                            //DB_Close   SGF.Debuger.LogError($"{TagFlag} CheckConditions cfg {cfg.ID} , type {conditionType} , no handle!!!");

                        }
                        break;
                }
            }

            return true;
        }

        internal override void EnterFrame()
        {
            if (leastCD <= 0)
            {
                return;
            }
            leastCD -= TimeUtils.FixedDeltaTime;
            if (leastCD < 0)
            {
                leastCD = 0;
            }
        }


        /// <summary>
        /// 剩余的cd时间
        /// </summary>
        private float leastCD = 0;

        /// <summary>
        /// 技能槽的CD剩余时间
        /// </summary>
        public float LeastCD
        {
            get { return leastCD; }
        }

        /// <summary>
        /// 刷新CD
        /// </summary>
        public Action<SkillInfo, float> ActionOnSkillRefreshCD;

        public void ServerStartCD(CDData cDData, int cd)
        {
            // 所有技能cd 共享相同 cdKey 的技能, 所以此此处 不单独计算cd
            {
                //技能的CD有几种情况
                //使用完技能后，收到职业技能CD协议回复，此时需要考虑
                //  1.如果存在 客户度开始记录CD的时间点(说明不是走断线重连):
                //    [1]: 计算客户度从开始记录CD到收到cd协议回复的时间间隔 0.15s
                //    [2]: 计算协议回复的Cd实际 多久，比如cd 0.1s
                //    [3]: 计算服务器返回的CD结束时间是否 > serverNow ， 说明cd还没结束。
                //             如果有摇杆间隔 1s , 显示的CD 为:  摇杆间隔(1-0.15)s + 0.1cd
                //             如果没有摇杆间隔  , 显示的CD 为： 0.1cd (此时忽视几十ms级的网络延迟，体验应该更好) 
                //    [4]: 计算服务器返回的CD结束时间是否 < serverNow ， 网络延迟波动很大,本地时间跑到服务器后面
                //             如果有摇杆间隔 1s , 显示的CD 为:  摇杆间隔(1-0.15)s + 0.1cd
                //             如果没有摇杆间隔  , 显示的CD 为： 0.1cd (此时忽视几十ms级的网络延迟，体验应该更好) 
                //
                //  2.如果不存在客户端开始记录CD的时间点(说明 是走断线重连):
                //    [1]: 如果服务器的 cdStartTime <= serverNow &&  serverNow < cdEndTime, 说明此时在技能CD中, 
                //         此时客户度无法确定服务器到底在什么时候开始, 就忽略延迟,
                //         显示的CD 为: cdEndTime - serverNow ;
                //    [2]: 如果服务器的 cdStartTime > serverNow ,说明收到了未来时间
                //         此时客户度无法确定服务器到底在什么时候开始, 就忽略延迟,
                //         显示的CD 为: cdEndTime - cdStartTime ;
                //    [3]: 如果服务器的 cdEndTime > serverNow ,说明 已经无法说明了,
                //         那就不进入CD

                // long cdStartTime = cDData.StartCDTime;
                // long cdEndTime = cDData.EndCDTime;

                // long cdTime = cdEndTime - cdStartTime;

                // long serverNow = TimeUtils.ServerNowStampMilli;

                // // 暂时不处理， 目前还没遇到这么极端的情况
                // {
                //     //客户度如果加载卡顿,可能出现cd协议到了，但是本地技能还未在cd阶段
                //     //那么为了处理这种加载卡帧导致网络先到的问题
                //     //可以在收到协议后，就开启摇杆cd
                //     //if (isClientUseSkill)
                //     //{
                //     //    leastJoyInterval = joyInterval;
                //     //    isStartJoyInterval = true;
                //     //}
                // }
                // //普工的情况
                // if (cdTime <= 0)
                // {
                //     leastCD = 0;
                // }
                // else
                // {
                //     if (cdStartTime <= serverNow && serverNow < cdEndTime)
                //     {
                //         leastCD = cdEndTime - serverNow;
                //     }
                //     else if (cdStartTime > serverNow)
                //     {
                //         leastCD = cdTime;
                //     }
                //     else
                //     {
                //         leastCD = 0;
                //     }
                // }
            }

            RefreshCD(cd);
            ActionOnSkillRefreshCD?.Invoke(this, cd);
            //SGF.Debuger.Log($"{TagFlag} [技能CD] [ServerStartCD] leastCD : {leastCD} ");
        }


        public void RefreshCD(int cd)
        {
            leastCD = cd;

        }

        public float GetCD()
        {
            return LeastCD;
        }


        public int GetRellyCD()
        {
            int cd = 0;
            if (cfg != null)
            {
                cd = cfg.CD;
            }
            return cd;
        }

        public bool HasCfgCD()
        {
            return GetRellyCD() > 0;
        }

        public void SetSkillLevel(int level)
        {
            skillLevel = level;
        }

        public void SetSkillType(E_SkillSetType e_SkillSetType)
        {
            skillSetType = e_SkillSetType;
        }

        public bool IsNormalSkill()
        {
            return skillSetType == E_SkillSetType.Normal;
        }

        public string GetSkillIconPath()
        {
            string path = "";
            if (playerData.EntityType == E_EntityType.Player)
            {
                path = GetPlayerSkillIconPath();
            }
            else if (playerData.EntityType == E_EntityType.Partner)
            {
                path = GetPartnerSkillCfgIconPath();
            }
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".png", string.Empty);
            return path;
        }

        private string GetPlayerSkillIconPath()
        {
            string path = "";

            JobSkillDescDataCell jobSkillDescDataCell = LocalDataManager.Instance.GetJobSkillDescDataCellBySkillIdAndLevel(skillId, skillLevel);
            if (jobSkillDescDataCell != null)
            {
                path = jobSkillDescDataCell.IconPath;
            }

            return path;
        }

        public int GetPlayerSkillConsume()
        {
            int num = 0;

            JobSkillDescDataCell jobSkillDescDataCell = LocalDataManager.Instance.GetJobSkillDescDataCellBySkillIdAndLevel(skillId, skillLevel);
            if (jobSkillDescDataCell != null)
            {
                num = jobSkillDescDataCell.GetSkillConsume();
            }

            return num;
        }

        private string GetPartnerSkillCfgIconPath()
        {
            string path = "";

            int skillLevel = 1;
            if (playerData != null && playerData.myOwnerNtt != null)
            {
                int curPartnerStar = PartnerManager.Instance.GetPartnerStarById((int)playerData.myOwnerNtt.ConfigIndex);
                var m_ParSkillDataCell = LocalDataManager.Instance.GetParSkillDataCell((int)playerData.myOwnerNtt.ConfigIndex, curPartnerStar);
                if (m_ParSkillDataCell != null)
                {
                    skillLevel = m_ParSkillDataCell.GetParSkillLV();
                }
            }

            PartnerSkillDescDataCell partnerSkillDescDataCell = LocalDataManager.Instance.GetPartnerSkillDescDataCellBySkillIdAndLevel(skillId, skillLevel);
            if (partnerSkillDescDataCell != null)
            {
                path = partnerSkillDescDataCell.IconPath;
            }

            return path;
        }

        public string GetSkillName()
        {
            string name = "";

            JobSkillDescDataCell jobSkillDescDataCell = LocalDataManager.Instance.GetJobSkillDescDataCellBySkillIdAndLevel(skillId, skillLevel);
            if (jobSkillDescDataCell != null)
            {
                name = jobSkillDescDataCell.SkillName;
            }

            return name;
        }

        private double minSkillAra = 0;
        private double maxSkillAra = 0;

        private void InitSkillArea()
        {

            minSkillAra = Math.Pow(cfg.MinCastDistance / 100f, 2);

            maxSkillAra = Math.Pow(cfg.AICastDistance / 100f, 2);
        }

        public bool CheckIsSkillArea(double powDis)
        {
            return minSkillAra <= powDis && maxSkillAra >= powDis;
        }

        /// <summary>
        /// 判定是否在 技能范围内
        /// </summary>
        /// <param name="powDis">目标点 距离技能重点的 距离平方</param>
        /// <param name="tagetRadius">目标半径</param>
        /// <returns></returns>
        public bool CheckIsSkillArea(double powDis, float targetRadius)
        {
            // 如果对象输入的技能， 不计算目标的半径
            if (skillWheelInfo.SkillInputType == SkillInputType.ObjInput)
            {
                targetRadius = 0;
            }
            // targetRadius只扩大 最大的技能范围
            return minSkillAra <= powDis && GetSkillAra(cfg.AICastDistance / 100f, targetRadius) >= powDis;
        }

        public bool CheckIsInAutoBattleSkilArea(double powDis, float targetRadius)
        {
            return minSkillAra <= powDis && GetSkillAra(maxSkillAra / 100f, targetRadius) >= powDis;
        }

        private double GetSkillAra(double skillDistance, float targetRadius)
        {
            return Math.Pow(skillDistance + targetRadius, 2);
        }


        private void InitSkillSlotSpShow()
        {
            m_SkillSlotSpShow.Clear();
            if (cfg == null)
            {
                return;
            }
            foreach (var item in cfg.GlobalShows)
            {
                if (item.GlobalShowType == GlobalShowType.Skill_SkillSlotSpShow)
                {
                    foreach (var item1 in item.Skill_SkillSlotSpShow.SpShow)
                    {
                        m_SkillSlotSpShow.Add(item1);
                    }
                }
            }
        }

        public List<EnumSkillSlotSpShow> GetSkillSlotSpShow()
        {
            return m_SkillSlotSpShow;
        }

        public void Reset()
        {
            playerData = null;
            skillId = 0;
            serverSkillLevel = 1;
            skillLevel = 1;

            cfg = null;
            skillWheelInfo.Reset();
            stageJsons = null;
            otherStageJsons = null;
            _curStageIdx = 0;
            isCleanse = false;

            skillContainer = null;
            consumes = null;

            leastCD = 0;

            ActionOnSkillRefreshCD = null;

            m_SkillSlotSpShow.Clear();

            EntityFactory.ReleaseEntity(timeLineStageInfos);
            timeLineStageInfos = null;

            EntityFactory.ReleaseEntity(timeLineOtherStageInfos);
            timeLineOtherStageInfos = null;
        }

        protected override void Release()
        {
            base.Release();
            Reset();
        }

    }
}


