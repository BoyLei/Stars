using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoMsg;
using StarProject.Game.Entity.Factory;

using StarProject.Game.Data;
using System;
using StarProjectDef;
using StarProject.Service.LocalData;
using SGF.Time;
using StarProject.Game.Entity;
using SkillEditor;
using SGF.Module.Framework;
using StarProject.Service.Battle;
using StarProject.Service.SystemOpen;
using StarProject.Service.Business;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能槽，一个技能槽可能包含多个技能位
    /// </summary>
    public class SkillContainer : EntityRemoteStatic
    {
        private string TagFlag
        {
            get => $"[{playerData.M_EntityID}] [SkillContainer]";
        }

        private VitalSignData playerData;

        public SkillUnitController skillUnitController;

        public SkillDispatcher skillDispatcher;


        /// <summary>
        /// 当前技能所在技能组的位置，多段技能使用的时候，根据这个变量可以确定当前的技能是啥
        /// </summary>
        private int curSkillIdx = 0;

        /// <summary>
        /// 当前技能位的技能id
        /// </summary>
        public int CurSkillId
        {
            get
            {
                if (curSkillIdx >= ShowSkillCount)
                {
                    return 0;
                }
                return ShowSkillInfos[curSkillIdx].skillId;
            }
        }

        /// <summary>
        /// 技能槽位
        /// </summary>
        public int PosID
        {
            get
            {
                if (skillPos != null)
                {
                    return skillPos.PosID;
                }
                return 0;
            }
        }

        /// <summary>
        /// 技能槽中 所有的战斗技能skillInfo
        /// 2023/3/10
        /// note:
        ///     技能槽 中的战斗 技能信息 不应该直接  暴力给外面使用. 
        ///     目前 看到 外面 为了 修改技能,导致 直接引用 skillInfos, 这其实是不太合适的.
        /// </summary>
        private List<SkillInfo> skillInfos = new List<SkillInfo>();

        /// <summary>
        /// 可以用作显示的 skillInfo 集合,  是满足 显示条件的 技能信息的集合
        /// </summary>
        /// <typeparam name="SkillInfo"></typeparam>
        /// <returns></returns>
        public List<SkillInfo> ShowSkillInfos = new List<SkillInfo>();

        public int ShowSkillCount => ShowSkillInfos.Count;

        /// <summary>
        /// 当前技能位的技能信息, 当前的技能槽 没有满足的 技能时,当前的技能信息 为null
        /// </summary>
        public SkillInfo CurSkillInfo
        {
            get
            {
                if (curSkillIdx >= ShowSkillCount)
                {
                    return null;
                }
                return ShowSkillInfos[curSkillIdx];
            }
        }

        /// <summary>
        /// 当前显示的 技能信息, 用来显示 当前技能槽的技能信息. 如果没有技能时，默认用第一个 技能信息
        /// </summary>
        public SkillInfo CurShowSkillInfo
        {
            get
            {
                if (ShowSkillInfos.Count > 0)
                {
                    return ShowSkillInfos[0];
                }
                if (skillInfos.Count > 0)
                {
                    return skillInfos[0];
                }
                return null;
            }
        }

        public SkillInfo FirstSkillInfo
        {
            get
            {
                if (skillInfos.Count > 0)
                {
                    return skillInfos[0];
                }
                return null;
            }
        }



        /// <summary>
        /// 是否有显示的 技能信息
        /// </summary>
        public bool HasShowSkill => ShowSkillInfos.Count > 0;

        /// <summary>
        /// 技能槽是否有技能
        /// </summary>
        public bool HasSkill => skillInfos.Count > 0;

        /// <summary>
        /// 刷新等待时间的 Action.  
        /// 第一个参数 表明 是否 (开True)/(关False)
        /// 第二个参数 表明 最大时间 ms
        /// 第一个参数 表明 当前时间 ms
        /// </summary>
        public Action<SkillContainer, bool, int, int> ActionOnRefreshWaitInterval;

        /// <summary>
        /// 技能发送改变时, 执行的刷新技能 Action
        /// </summary>
        public Action<SkillInfo, SkillInfo> ActionRefreshSkill;

        /// <summary>
        /// 开始CD 的Action
        /// </summary>
        public Action<float, bool> ActionOnStartCD;

        /// <summary>
        /// 结束CD之后的Action
        /// </summary>
        public Action ActionOnEndCD;


        /// <summary>
        /// 刷新CD
        /// </summary>
        public Action<float> ActionOnRefreshCD;

        /// <summary>
        /// 技能摇杆 等待时间开启的Action
        /// </summary>
        public Action<float> ActionOnStartJoyInterval;



        /// <summary>
        /// 技能摇杆 等待时间结束的Action
        /// </summary>
        public Action ActionOnEndJoyInterval;

        /// <summary>
        /// 是否开启摇杆间隔倒计时
        /// </summary>
        private bool isStartJoyInterval = false;
        /// <summary>
        /// 是否开启
        /// </summary>
        private bool isStartCD = false;




        /// <summary>
        /// 摇杆的技能等待时间。
        /// </summary>
        private float joyInterval = 0;

        /// <summary>
        /// 剩余的摇杆间隔时间
        /// </summary>
        private float leastJoyInterval = 0;

        /// <summary>
        /// 客户端记录的开启cd时间
        /// </summary>
        private long clientStartCDTime = 0;

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
        /// 技能槽 按钮 cd
        /// </summary>
        private float containerBtnCD = 0;

        /// <summary>
        /// 技能 预报后，开启的 技能槽cd, 目前 主要是为了 防止 服务器技能cd未到，客户端 就开启预输入技能的问题
        /// </summary>
        public float ContainerBtnCD => containerBtnCD;

        /// <summary>
        /// 技能槽的 技能位信息
        /// 
        /// message SkillPos {
        ///     //技能位 
        ///     required int32 PosID = 1;
        ///     //绑定（职业）技能
        ///     required int32 JobSkillID = 2;
        ///     //是否锁定
        ///     required bool Islocked = 3;
        ///}
        /// </summary>
        public SkillPos skillPos;

        /// <summary>
        /// 当前技能槽的 玩家职业技能 信息
        /// </summary>
        private SingleSkillInfo singleSkillInfo;

        /// <summary>
        /// 当前技能槽的职业技能配置信息
        /// </summary>
        //public JobSkillDataCell jobSkillCfg;

        /// <summary>
        /// 当前职业技能对应的天赋配置
        /// </summary>
        public TalentDataCell talentDataCell;

        private E_SkillSetType curSkillType = E_SkillSetType.Conventional;



        /// <summary>
        /// 技能槽是否填充技能
        /// </summary>
        private bool _isPadding = false;
        public bool IsPadding
        {
            get { return _isPadding; }
        }

        private bool _isChangeSkillSlot = false;

        /// <summary>
        /// 是否是 变身刷新的技能槽
        /// </summary>
        public bool IsChangeSkillSlot
        {
            get { return _isChangeSkillSlot; }
        }

        // 万能技能位，应用于：其他人和怪物
        public void Create(List<int> skills, VitalSignData data, SkillDispatcher dispacher, Transform container)
        {
            Reset();
            playerData = data;
            skillUnitController = dispacher.SkillUnitController;
            skillDispatcher = dispacher;

            for (int i = 0; i < skills.Count; i++)
            {
                int battleSkillID = skills[i];
                CreateSkillInfo(battleSkillID);
            }
            RefreshShowSkillInfos();

            //重置当前的技能idx。
            ChangeCurSkillInfo(0);

            _isPadding = true;
        }
        // 根据服务器下发的携带的技能信息，创建技能位数据
        public void Create(SkillPos skillPos1, VitalSignData data, SkillDispatcher dispacher, Transform container)
        {
            Reset();
            playerData = data;
            skillUnitController = dispacher.SkillUnitController;
            skillDispatcher = dispacher;
            skillPos = skillPos1;

            _isPadding = RefreshJobSkill(skillPos.JobSkillID);

        }

        public void Create(TalentAndSlot talentAndSlot, VitalSignData data, SkillDispatcher dispacher, Transform container)
        {
            Reset();
            playerData = data;
            skillUnitController = dispacher.SkillUnitController;
            skillDispatcher = dispacher;

            _isChangeSkillSlot = true;
            _isPadding = RefreshJobSkillWithJobSlot(talentAndSlot);
        }

        public bool RefreshJobSkill(int jobSkillID)
        {
            // 如果没有变身技能的 效果标签, 那就直接拿 玩家身上的 技能信息
            // if (!playerData.HasChangeSkillSlot)
            // {
            //     singleSkillInfo = playerData.GetSingleSkillInfo(jobSkillID);
            // }

            singleSkillInfo = playerData.GetSingleSkillInfo(jobSkillID);

            if (singleSkillInfo == null)
            {
                _isPadding = false;
                SGF.Debuger.LogWarning($"{TagFlag} 技能信息 Create JobSkillID {jobSkillID} error!!!");
                return false;
            }



            // SGF.Debuger.Log($"{TagFlag} Create JobSkillID {jobSkillID} curSkillType {curSkillType}");



            var skillPosSetCfg = LocalDataManager.Instance.GetSkillPosSetDataCell(skillPos.PosID);
            if (skillPosSetCfg != null)
            {
                curSkillType = (E_SkillSetType)skillPosSetCfg.GetSkillType();
            }

            if (singleSkillInfo.TalentID == 0)
            {
                singleSkillInfo.TalentID = 1;
                SGF.Debuger.LogWarning($"{TagFlag} Create JobSkillID {jobSkillID} curSkillType {curSkillType} , 服务器发过来的 TalentID = 0 , error!!!");

            }
            RefreshTalent(singleSkillInfo);

            return true;
        }

        public bool RefreshJobSkillWithJobSlot(TalentAndSlot talentAndSlot)
        {
            talentDataCell = LocalDataManager.Instance.GetTalentDataCell(talentAndSlot.TalentID);

            int jobSkillID = talentDataCell.GetBelongSkill();

            skillPos = new SkillPos();
            skillPos.PosID = talentAndSlot.SlotID;
            skillPos.Islocked = false;

            // jobSkill 职业技能id
            skillPos.JobSkillID = jobSkillID;

            // 如果没有变身技能的 效果标签, 那就直接拿 玩家身上的 技能信息
            singleSkillInfo = new SingleSkillInfo();
            // dbID 服务器记的是 职业技能id
            singleSkillInfo.SkillDBID = jobSkillID;
            singleSkillInfo.SkillLevel = 1;
            singleSkillInfo.TalentID = talentAndSlot.TalentID;

            // SGF.Debuger.Log($"{TagFlag} Create JobSkillID {jobSkillID} curSkillType {curSkillType}");


            var skillPosSetCfg = LocalDataManager.Instance.GetSkillPosSetDataCell(skillPos.PosID);
            if (skillPosSetCfg != null)
            {
                curSkillType = (E_SkillSetType)skillPosSetCfg.GetSkillType();
            }


            RefreshTalent(singleSkillInfo);

            return true;
        }

        /// <summary>
        /// 刷新天赋 ID, 目前 一个天赋 对应多个 战斗技能 battleID
        /// </summary>
        /// <param name="talentID"></param>
        public void RefreshTalent(SingleSkillInfo singleSkillInfo)
        {
            int talentID = singleSkillInfo.TalentID;

            talentDataCell = LocalDataManager.Instance.GetTalentDataCell(talentID);

            if (talentDataCell != null)
            {
                //天赋对应的战斗技能列表
                List<int> battleSkilList = talentDataCell.BattleActive;

                for (int i = 0; i < battleSkilList.Count; i++)
                {
                    int battleSkillID = battleSkilList[i];
                    CreateSkillInfo(battleSkillID);
                }
            }
            else
            {
                int battleSkillID = singleSkillInfo.SkillDBID;
                CreateSkillInfo(battleSkillID);
            }
            RefreshShowSkillInfos();

            //重置当前的技能idx。
            ChangeCurSkillInfo(0);
        }

        private SkillInfo InstanceSkillInfo()
        {
            SkillInfo skillInfo = new SkillInfo();
            return skillInfo;
        }

        private void CreateSkillInfo(int battleSkillID)
        {
            SkillInfo skillInfo = InstanceSkillInfo();
            skillInfo.Create(battleSkillID, playerData, this);
            if (skillInfo.cfg == null)
            {
                return;
            }
            int skillLevel = 1;
            if (singleSkillInfo != null)
            {
                skillLevel = singleSkillInfo.SkillLevel;
            }
            skillInfo.SetSkillLevel(skillLevel);

            skillInfo.SetSkillType(curSkillType);

            // 注册 每个skillInfo 的 cd 刷新action
            skillInfo.ActionOnSkillRefreshCD = OnSkillInfoRefreshCD;

            //技能info加入skillInfos
            skillInfos.Add(skillInfo);
        }

        private bool RefreshShowSkillInfos()
        {
            // 如果有 正在使用的 技能预输入,那就不刷新这个技能
            if (CanUseUserInput())
            {
                return false;
            }

            bool changeSkill = false;

            ShowSkillInfos.Clear();
            ClearSkillConditionReg();

            int originCurSkillID = CurSkillId;
            skillInfos.ForEach((SkillInfo skillInfo) =>
            {
                if (skillInfo.CheckConditions() && skillInfo.IsShow)
                {
                    ShowSkillInfos.Add(skillInfo);
                }
                RegisterSkillCondition(skillInfo);
            });

            // 当 当前技能 信息 由于 技能 condtion条件变化导致 技能刷新时,
            // 通知 外层
            if (originCurSkillID != CurSkillId)
            {
                changeSkill = true;
            }

            if (ShowSkillInfos.Count == 0)
            {
                changeSkill = true;
            }
            return changeSkill;
        }

        Dictionary<ConditionType, List<SkillCondition>> skillConditions = new Dictionary<ConditionType, List<SkillCondition>>();
        private void RegisterSkillCondition(SkillInfo skillInfo)
        {
            List<SkillCondition> conditions = skillInfo.GetSkillCondtions();
            conditions.ForEach((SkillCondition condition) =>
            {
                ConditionType conditionType = condition.ConditionParams.ConditionType;
                List<SkillCondition> conditionList;
                if (!this.skillConditions.ContainsKey(conditionType))
                {
                    conditionList = new List<SkillCondition>();
                }
                else
                {
                    conditionList = this.skillConditions[conditionType];
                }
                conditionList.Add(condition);
                this.skillConditions[conditionType] = conditionList;
            });
            RegConditionsListner();
        }

        /// <summary>
        /// 注册 技能槽的 技能条件 condition 监听
        /// </summary>
        private void RegConditionsListner()
        {
            ClearSkillConditionReg();
            foreach (KeyValuePair<ConditionType, List<SkillCondition>> item in skillConditions)
            {
                ConditionType conditionType = item.Key;
                List<SkillCondition> conditions = item.Value;

                conditions.ForEach((SkillCondition condition) =>
                {
                    RegItemConditionListener(conditionType, condition);
                });
            }
        }

        private void OnTriggleCondition(object value)
        {
            bool changeSkill = RefreshShowSkillInfos();
            // 如果 技能操 技能发生变化,那就 通知外层 技能发生了变化
            if (changeSkill)
            {
                ChangeCurSkillInfo(0);
            }
        }

        private void RegItemConditionListener(ConditionType conditionType, SkillCondition condition)
        {
            switch (conditionType)
            {
                case ConditionType.Cond_Buff:
                    {
                        ConditionBuff conditionBuff = condition.ConditionParams.Cond_Buff;
                        int buffID = conditionBuff.ID;
                        // 监听 buff
                        playerData.RegBuffListener(buffID, OnTriggleCondition, this.GetHashCode().ToString());
                    }
                    break;
                case ConditionType.Cond_Attr:
                    {
                        // 监听 属性变化
                        ConditionAttr conditionAttr = condition.ConditionParams.Cond_Attr;
                        VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)conditionAttr.ID);
                        if (vitalSignAOIClientAttrs != null)
                        {
                            playerData.RegChangeListener(vitalSignAOIClientAttrs.Name, OnTriggleCondition, this.GetHashCode().ToString());
                        }
                    }
                    break;
                case ConditionType.Cond_SkillTime:
                    {
                        // 技能时间？
                        // 目前跟 gl 约定 这个条件 先不弄, 技能icon的显示 通过 监听技能事件 判断能不能使用 没太大必要
                        // 实在不行, 弄成  一个buff 一段事件 不允许显示某个技能也行
                        //DB_Close   SGF.Debuger.Log($"{TagFlag} Cond_SkillTime 条件 不需要再 技能ui刷新时 处理");

                    }
                    break;
                case ConditionType.Cond_HpPercent:
                    {
                        // 监听生命值的百分比
                        playerData.RegChangeListener(AOIAttrDefine.curHp, OnTriggleCondition, this.GetHashCode().ToString());
                        playerData.RegChangeListener(AOIAttrDefine.TruthHp, OnTriggleCondition, this.GetHashCode().ToString());

                    }
                    break;
                case ConditionType.Condition_CurSpectralElem:
                    {

                        // 特殊枚举类型 的值？？  类似 量普？？
                        ConditionCurSpectralElem SpectralElem = condition.ConditionParams.Condition_CurSpectralElem;

                        switch (SpectralElem.Spectral)
                        {
                            case Spectral.Spectral1:
                                {
                                    playerData.RegChangeListener(AOIAttrDefine.curSpectral1, OnTriggleCondition, this.GetHashCode().ToString());
                                }
                                break;
                            case Spectral.Spectral2:
                                {
                                    playerData.RegChangeListener(AOIAttrDefine.curSpectral2, OnTriggleCondition, this.GetHashCode().ToString());
                                }
                                break;
                            case Spectral.Spectral3:
                                {
                                    playerData.RegChangeListener(AOIAttrDefine.curSpectral3, OnTriggleCondition, this.GetHashCode().ToString());
                                }
                                break;
                            default:
                                {
                                    //DB_Close   SGF.Debuger.LogError($"{TagFlag} no handle with conditionType: {conditionType} , value : {SpectralElem.Spectral}");
                                }
                                break;
                        }
                    }
                    break;
                default:
                    {
                        //DB_Close   SGF.Debuger.LogError($"{TagFlag} no handle with conditionType: {conditionType}");
                    }
                    break;
            }
        }

        /// <summary>
        /// 取消 技能 的所有 条件注册
        /// </summary>
        private void ClearSkillConditionReg()
        {
            playerData?.UnRegAllTagChangeListeners(this.GetHashCode().ToString());
        }

        private long now = 0;

        /// <summary>
        /// 刷新摇杆间隔时间
        /// 技能创建或者服务器使用技能后，需要根据当前的技能，来刷新技能的间隔时间
        /// </summary>
        private void RefreshJoyInterval()
        {
            //1.判断当前职业技能是不是普工攻击
            //joyInterval = IsNormalSkill() ? CurSkillInfo.cfg.JoyStickInterval : 0f;
            joyInterval = 0f;

            long cost = 0;
            if (now != 0)
            {
                cost = TimeUtils.ClientNowStampMilli - now;
            }
            now = TimeUtils.ClientNowStampMilli;

            //DB_Close   SGF.Debuger.Log($"{TagFlag} RefreshJoyInterval skillId {CurSkillId} , IsNormalSkill {IsNormalSkill()} , , joyInterval {joyInterval} , cost : {cost} ms");
        }

        /// <summary>
        ///  技能槽 应该只能判断技能 释放条件是否满足
        /// </summary>
        /// <returns></returns>
        public E_UseSkillResult CheckCanUseSkill(int skillId, E_UseSkillType usingSkillType)
        {
            /// 2023/10/25
            /// 高磊新提的需求: 要求 在技能按钮操作期间, 技能槽哪怕被刷新了, 也是能够操作这个技能.
            ///     所以 在检查使用技能的时候, 增加一个条件, 当这个技能是使用效果的时候(蓄力技能就是输入效果),
            ///     哪怕这个技能槽被 刷新掉了, 也是能够使用

            if (usingSkillType == E_UseSkillType.ExecuteEffect)
            {
                return E_UseSkillResult.Succeed;
            }

            if (IsCD())
            {
                // 2023/10/27
                // gl 的需求
                // 如果有 正在是使用 并且有效的用户输入效果, 那么这个按钮cd  也是可以点击使用
                if (!CanUseUserInput())
                {
                    // SGF.Debuger.LogError($"[CheckCanUseSkill] ---> CD_Not_Enough");
                    return E_UseSkillResult.CD_Not_Enough;
                }
                else
                {
                    // 2023/10/27
                    // gl 的需求
                    // 如果有 正在是使用 并且有效的用户输入效果, 那么这个按钮cd  也是可以点击使用
                    // 所以 此处啥都不干,继续走后续节能能否使用的判定
                    // SGF.Debuger.LogError($"[CheckCanUseSkill] ---> 有正在使用的 预输入,cd 不拦截");

                }
            }
            if (IsButtonCD())
            {
                return E_UseSkillResult.Button_CD_Not_Enough;
            }
            if (CurSkillInfo == null)
            {
                return E_UseSkillResult.Cur_Not_UseSkill;
            }
            if (skillId != CurSkillInfo.skillId)
            {
                return E_UseSkillResult.Cur_Not_UseSkill;
            }
            return CurSkillInfo.CheckCanUseSkill();
        }

        public bool CheckCanUse()
        {
            var result = CheckCanUseSkill(CurSkillId, E_UseSkillType.UsingSkill);
            //SGF.Debuger.Log($" 检查是否可以使用技能 skillid={CurSkillId},result: {result}");

            // 检查是否可以使用技能， 默认使用技能为 技能按钮按下的操作
            return CurSkillInfo != null && CurSkillId != 0 && CheckCanUseSkill(CurSkillId, E_UseSkillType.UsingSkill) == E_UseSkillResult.Succeed;
        }

        /// <summary>
        /// 获得当前 槽位技能 不匹配的 属性条件
        /// </summary>
        /// <param name="isOver"></param>
        public ConditionAttr GetCurNotMatchAttrCondition(out bool isOver)
        {
            var attrCondition = CurShowSkillInfo.NotMatchAtrrCondition(out isOver);

            return attrCondition;
        }

        /// <summary>
        /// 检查 是否在技能的释放范围内
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool CheckIsInSkillArea(float distance)
        {
            return CurShowSkillInfo.MinCastDistance <= distance && CurShowSkillInfo.AICastDistance >= distance;
        }

        /// <summary>
        /// 找到skillId 在技能位的位置idx ( 找的是战斗技能id 的  skillInfo)
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public int FindSkillIdx(int skillId)
        {
            int idx = skillInfos.FindIndex((SkillInfo skillInfo) =>
            {
                return skillInfo.skillId == skillId;
            });
            return idx;
        }

        /// <summary>
        /// 查找 战斗技能id 对应的 skillInfo
        /// </summary>
        /// <param name="battleSkillId"></param>
        /// <returns></returns>
        public SkillInfo FindSkillInfo(int battleSkillId)
        {
            return skillInfos.Find((SkillInfo skillInfo) =>
             {
                 return skillInfo.skillId == battleSkillId;
             });
        }

        /// <summary>
        /// 查找技能skillID 在 能够显示的技能 ShowSkillInfos 中的 idx
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        private int FindShowSkillIdx(int skillId)
        {
            int idx = ShowSkillInfos.FindIndex((SkillInfo skillInfo) =>
            {
                return skillInfo.skillId == skillId;
            });
            return idx;
        }

        public bool EqualJobSkillID(int jobSkillId)
        {
            if (singleSkillInfo != null)
            {
                return singleSkillInfo.SkillDBID == jobSkillId;
            }
            return false;
        }

        public bool EqualSkillCDKey(int cdKey)
        {
            return CurShowSkillInfo?.CDKey == cdKey;
        }

        public int GetNextSkillIdx()
        {
            //跳动到下一个技能
            int idx = curSkillIdx + 1;

            //说明curSkillIdx已经超过了技能的总数，那么就回到第一个技能处
            if (idx >= ShowSkillCount)
            {
                idx = 0;
            }
            return idx;
        }

        public bool IsNormalSkill()
        {
            return curSkillType == E_SkillSetType.Normal;
        }

        public bool CanGetNextSkill()
        {
            //如果是普工，则可以无限循环技能
            if (IsNormalSkill())
            {
                return ShowSkillCount > 0;
            }
            else
            {
                return curSkillIdx + 1 >= ShowSkillCount;
            }

        }


        /// <summary>
        /// 服务器使用技能后，找到对应的技能位，执行对应技能位的OnUseSkill
        /// </summary>
        /// <param name="skillId"></param>
        public SkillInfo ServerUseSkill(SkillUseRet skillUseRet)
        {

            //如果技能槽的当前技能不是服务器下发的技能，那首先将当前技能切换到服务器下发的技能
            int skillId = skillUseRet.SkillID;


            if (skillId != CurSkillId)
            {

                //找到服务器使用的技能idx，将最新的技能位置curSkillIdx替换为服务器使用的技能点
                int idx = FindShowSkillIdx(skillId);
                if (-1 != idx)
                {
                    ChangeCurSkillInfo(idx);
                }
                else
                {
                    {
                        // 如果技能 id 在 显示技能 列表中 也找不到, 这个时候 就取找 skillInfos
                        // 如果 能 在 skillInfos 中找到, 就将 skillInfo 放入 ShowSkillInfos 中.
                        // 如果 找不到, 就生成一个 skillInfo 放入 SkillInfos 和 ShowSkillInfos中
                        // idx = FindSkillIdx(skillId);

                        // if (-1 == idx)
                        // {
                        //     CreateSkillInfo(skillId);
                        // }
                        // ShowSkillInfos.Add(skillInfos[skillInfos.Count - 1]);
                        // ChangeCurSkillInfo(ShowSkillCount - 1);
                    }

                    // 如果技能 找不到,  那就可能技能被 刷没了， 此时 重新RefreshShowSkillInfos 
                    bool change = RefreshShowSkillInfos();
                    // if (change)
                    // {
                    //     //重置当前的技能idx。
                    //     ChangeCurSkillInfo(0);
                    // }
                }

            }




            //刷新下一个技能信息，目前策划说就放在使用技能后刷新就行，后面有需要再去移动到合适的位置
            {
                //SGF.Debuger.Log($"{TagFlag} OnUseSkill skillId {skillId} CurSkillId {CurSkillId} nextSkillIdx {GetNextSkillIdx()} nextSkill {skillList[GetNextSkillIdx()]}");

                //跳动到下一个技能
                ChangeCurSkillInfo(GetNextSkillIdx());
            }

            SkillInfo serverUeSkillInfo = FindSkillInfo(skillId);
            return serverUeSkillInfo;
        }

        /// <summary>
        /// 客户端的 技能 开始 技能槽cd
        /// </summary>
        /// <param name="cfg"></param>
        public void ClientStartContainerCD(SkillConfig cfg)
        {
            containerBtnCD = cfg.ButtonCD;
        }


        /// <summary>
        /// 客户端开启技能cd
        /// </summary>
        public void ClientStartCD(SkillConfig cfg)
        {
            // 客户端开启 CD
            return;

            ///note：
            ///    理论上客户端 StartSkillCD 比服务器 要快
            ///    所以基本流程是 客户端 先StartSkillCD ,记录 leastJoyInterval
            ///    服务器返回后，计算出实际的cd时间和 leastJoyInterval时间
            /// 
            /// but:
            ///    遥感间隔时间,直接采用技能 运行时中cfg的间隔时间,
            ///    这样可以避免在 技能槽中当前技能变化,导致遥感间隔时间不一致的bug

            if (cfg == null)
            {
                //DB_Close   SGF.Debuger.LogError($"{TagFlag} ClientStartCD cfg=null");
                return;
            }
            if (IsCD())
            {
                return;
            }
            // //DB_Close   SGF.Debuger.LogError($"{TagFlag} ClientStartCD  , skillID : {cfg.ID} , cd : {cfg.CD}");
            {
                clientStartCDTime = TimeUtils.ClientNowStampMilli;
                //leastJoyInterval = cfg.JoyStickInterval;
                leastJoyInterval = 0;
                isStartJoyInterval = true;
                isStartCD = true;
                leastCD = cfg.CD;
            }
            skillUnitController?.ClientStartCD(cfg.CD);
            //SGF.Debuger.Log($"{TagFlag} StartSkillCD clientStartCDTime   {clientStartCDTime} , leastJoyInterval {leastJoyInterval}");
        }

        /// <summary>
        /// 收到服务器协议回复后，客户端才应该开启cd流程
        /// </summary>
        /// <param name="cDData"></param>
        private void ServerStartCD(CDData cDData, int cd)
        {
            // 刷新 这个槽里面的 所有 cdKey 一致的 skillInfo
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.CDKey == cDData.CDKey)
                {
                    skillInfo.ServerStartCD(cDData, cd);
                }
            }
            return;

            // 旧的 CD 刷新逻辑
            if (false)
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

                long cdStartTime = cDData.StartCDTime;
                long cdEndTime = cDData.EndCDTime;

                long cdTime = cdEndTime - cdStartTime;

                long serverNow = TimeUtils.ServerNowStampMilli;

                // 暂时不处理， 目前还没遇到这么极端的情况
                {
                    //客户度如果加载卡顿,可能出现cd协议到了，但是本地技能还未在cd阶段
                    //那么为了处理这种加载卡帧导致网络先到的问题
                    //可以在收到协议后，就开启摇杆cd
                    //if (isClientUseSkill)
                    //{
                    //    leastJoyInterval = joyInterval;
                    //    isStartJoyInterval = true;
                    //}
                }
                //普工的情况
                if (cdTime == 0)
                {
                    leastCD = 0;
                    isStartCD = false;
                    //DB_Close       SGF.Debuger.Log($"{TagFlag} [ServerStartCD] leastCD : {leastCD}  leastJoyInterval {leastJoyInterval} , isStartCD {isStartCD} ");
                    ActionOnEndCD?.Invoke();
                    return;
                }


                if (clientStartCDTime > 0)
                {

                    //摇杆间隔时间如果存在，在 enterFrame 中会每帧处理
                    //所以此处就不需要专门去 处理 leastJoyInterval
                    {
                        //不用删，留着捋顺逻辑
                        // float clientCDCost = TimeUtils.ClientNowStampMilli - clientStartCDTime;
                        // if (leastJoyInterval > 0)
                        // {
                        //     leastJoyInterval -= clientCDCost;
                        //     leastJoyInterval = leastJoyInterval < 0 ? 0 : leastJoyInterval;
                        // }
                    }

                    leastCD = cdTime;
                    clientStartCDTime = 0;
                }
                else
                {
                    if (cdStartTime <= serverNow && serverNow < cdEndTime)
                    {
                        leastCD = cdEndTime - serverNow;
                    }
                    else if (cdStartTime > serverNow)
                    {
                        leastCD = cdTime;
                    }
                    else
                    {
                        leastCD = 0;
                    }
                }
                isStartCD = true;
                ActionOnRefreshCD?.Invoke(leastCD);
                //DB_Close       SGF.Debuger.Log($"{TagFlag} [ServerStartCD] leastCD : {leastCD}  leastJoyInterval {leastJoyInterval} ");

            }
        }

        // note:
        // 夏哥墙裂，俩个伙伴技能id【绝对不可能公用一个技能id】
        // 如果不是主角的技能，说明是伙伴的技能CD信息-》发送给伙伴
        //---- 这里是获取到主角技能数据层的cd，设置到伙伴技能层的cd
        public void SetSkillCD(float cd)
        {
            RefreshCD(cd);
        }

        private void ChangeCurSkillInfo(int skillIdx, bool isCDUpdateSkill = false)
        {
            UnRegSkillLabs();

            curSkillIdx = skillIdx;


            //LogUtils.Log(LogUtils.LogEnum.Skill,$"[技能CD] {GetHashCode()} 切换技能 curSkill: {CurSkillInfo?.skillId} ");
            // SGF.Debuger.LogError($"[技能CD] {GetHashCode()} 切换技能 curSkill: {CurSkillInfo?.skillId} , curShowSkill: {CurShowSkillInfo?.skillId} ");

            //当前技能发送改变时,通知刷新技能
            ActionRefreshSkill?.Invoke(CurSkillInfo, CurShowSkillInfo);

            RegSkillLabs();

            if (CurShowSkillInfo != null)
            {
                int cd = 0;
                if (skillUnitController != null)
                {
                    cd = skillUnitController.GetLeastCD(CurShowSkillInfo.CDKey);
                }
                CurShowSkillInfo.RefreshCD(cd);
                // 当技能槽的 当前技能发生变化时,通知 UI 当前技能的 CD刷新
                RefreshCD(cd);
            }

        }

        private void UnRegSkillLabs()
        {
            if (CurShowSkillInfo == null)
            {
                return;
            }
            // 当不是主角的时候, 没有 它的 ui 按钮表现逻辑
            if (!playerData.isMainPlayer)
            {
                return;
            }

            LocalDataManager.Instance.UnRegComplexUIConfigLabs(CurShowSkillInfo.SkillComplexConfig, CurShowSkillInfo.Labs);
        }

        private void RegSkillLabs()
        {
            if (CurShowSkillInfo == null)
            {
                return;
            }
            // 当不是主角的时候, 没有 它的 ui 按钮表现逻辑
            if (!playerData.isMainPlayer)
            {
                return;
            }

            //LocalDataManager.Instance.RegComplexUIConfigLabs(CurShowSkillInfo.SkillComplexConfig, CurShowSkillInfo.Labs);
            LocalDataManager.Instance.RegComplexUIConfigLabs(CurShowSkillInfo.SkillComplexConfig, new List<int>() { 0 });

        }

        /// <summary>
        /// 刷新技能CD的接口
        /// note1:
        ///     目前服务器技能的CD 绑定在职业技能 上，而不是在战斗技能上。
        ///     所以技能槽上的CD 由 职业技能确定。
        ///     
        /// note2:
        ///     CD需要计算当前时间，应该需要考虑延迟的问题
        /// </summary>
        /// <param name="cDData"></param>
        public void RefreshCD(CDData cDData, int cd)
        {
            //开启CD
            ServerStartCD(cDData, cd);
        }

        /// <summary>
        /// 技能cd
        /// </summary>
        public bool IsCD()
        {
            return LeastCD > 0;
        }

        /// <summary>
        /// 是否是 技能预播 导致的 按钮cd
        /// </summary>
        public bool IsButtonCD()
        {
            return ContainerBtnCD > 0;
        }

        private float battleCD = 0;
        public void MarkAutoBattleCD()
        {
            // 普攻 需要连击, 默认为 普攻永远可以释放
            if (IsNormalSkill())
            {
                battleCD = 50;
            }
            else
            {
                battleCD = BattleManager.Instance.IsPrePlayClientSkill ? 150 : 300;
            }

        }

        public bool IsAutoBattleCD()
        {
            return battleCD > 0;
        }

        internal override void EnterFrame()
        {
            foreach (SkillInfo item in skillInfos)
            {
                item.EnterFrame();
            }

            if (isStartJoyInterval)
            {
                isStartJoyInterval = false;
                ActionOnStartJoyInterval?.Invoke(leastJoyInterval);
            }

            if (containerBtnCD > 0)
            {
                containerBtnCD -= TimeUtils.FixedDeltaTime;
                if (containerBtnCD < 0)
                {
                    containerBtnCD = 0;

                    if (playerData.isMainPlayer)
                    {
                        GameManager.Instance.TriggerEvent(TriggerEventType.EndButtonCD, null);
                    }

                }
            }


            //有cd的技能,走技能cd逻辑
            if (leastCD > 0)
            {
                if (isStartCD)
                {
                    isStartCD = false;
                    //执行技能cd的Action
                    // SGF.Debuger.LogError($"[CheckCanUseSkill] startCD: {leastCD}");
                    ActionOnStartCD?.Invoke(leastCD, CurShowSkillInfo.IsCDCloseTouch);
                }
                leastCD -= TimeUtils.FixedDeltaTime;
                if (leastCD < 0)
                {
                    // 倒计时结束, 此时需要去刷新技能.
                    // 而刷新技能的时候，会去查找这个技能的cdKey 对应的cd.
                    // 所以 leastCD > 0|| leastCD < 0 都表示 正在走 cd倒计时的逻辑.
                    // 此处就不 设置 leastCD = 0 ,在 RefreshCD 的时候 会设置
                    // leastCD = 0;

                    ChangeCurSkillInfo(0, true);
                }
            }

            if (battleCD > 0)
            {
                battleCD -= TimeUtils.FixedDeltaTime;
                if (battleCD < 0)
                {
                    battleCD = 0;
                }
            }

            // 2023/4/25
            // 策划新需求,技能槽将不再共享CD,每个 技能单独记录它独有的CD
            // 同时, 不再需要 技能 A ---> B ----> C 这种逻辑，也不需要 CD 结束后回到 第一个技能的逻辑
            if (false)
            {
                if (leastJoyInterval > 0)
                {
                    leastJoyInterval -= TimeUtils.FixedDeltaTime;
                    //摇杆间隔时间中，直接return
                    return;
                }
                if (leastJoyInterval < 0)
                {
                    //归零,并且进入CD，所以不return
                    leastJoyInterval = 0;
                    //执行摇杆等待时间 结束的Action
                    ActionOnEndJoyInterval?.Invoke();

                    ChangeCurSkillInfo(0);
                }

                //有cd的技能,走技能cd逻辑
                if (leastCD > 0 && leastJoyInterval == 0)
                {
                    if (isStartCD)
                    {
                        isStartCD = false;
                        //执行技能cd的Action
                        ActionOnStartCD?.Invoke(leastCD, CurShowSkillInfo.IsCDCloseTouch);
                    }
                    leastCD -= TimeUtils.FixedDeltaTime;
                    if (leastCD < 0)
                    {
                        leastCD = 0;

                        ChangeCurSkillInfo(0);

                        //CD结束
                        ActionOnEndCD?.Invoke();
                    }
                }
            }


        }

        private void Reset()
        {
            /// fix bug: 屏蔽不知道为啥 走 release 的时候, 发现 skillInfo 的cfg 已经被 release,但是 ShowSkillInfos 未被情况的bug
            /// 2024/1/26
            ShowSkillInfos.Clear();


            ChangeCurSkillInfo(0);

            ClearSkillConditionReg();

            foreach (SkillInfo item in skillInfos)
            {
                EntityFactory.ReleaseEntity(item);
            }
            skillInfos.Clear();

            // 技能清空之后 再次通知一次 ui
            ActionRefreshSkill?.Invoke(CurSkillInfo, CurShowSkillInfo);

            ActionRefreshSkill = null;
            ActionOnStartCD = null;
            ActionOnEndCD = null;

            joyInterval = 0;
            leastJoyInterval = 0;
            clientStartCDTime = 0;
            leastCD = 0;

            battleCD = 0;

            skillPos = null;
            _isPadding = false;

            _isChangeSkillSlot = false;

            UserInputRecord = string.Empty;

        }

        protected override void Release()
        {
            base.Release();
            Reset();
            skillUnitController = null;
        }

        public bool CanUseUserInput()
        {
            if (CurSkillInfo != null && CurSkillInfo.cfg != null)
            {
                return skillDispatcher.SkillController.HasRunningSkillUserInput(CurSkillInfo.skillId);
            }
            return false;
        }

        public bool IsCanUseUserInputNormalSkill()
        {
            return IsNormalSkill() && CanUseUserInput();
        }


        public bool IsCanUseSkill()
        {
            // 判断时候在CD中
            if (IsCD())
            {
                // 2023/10/27
                // gl 的需求
                // 如果有 正在是使用 并且有效的用户输入效果, 那么这个按钮cd  也是可以点击使用
                if (CanUseUserInput())
                {
                    return true;
                }
                return false;
            }
            if (IsButtonCD())
            {
                return false;
            }
            // 查看是不是净化技能
            bool isCleanse = false;
            if (CurSkillInfo != null && CurSkillInfo.cfg != null)
            {
                isCleanse = CurSkillInfo.cfg.SkillTag == SkillTag.Cleanse;
            }
            // 是否原子锁了技能
            if (!IsNormalSkill())
            {
                bool isForbidSkill = playerData.myOwnerNtt.Data.CheckStateIsForbid(E_BattleStateType.BattleState_ForbidSkill);
                if (isForbidSkill && !isCleanse)
                {
                    //DB_Close       SGF.Debuger.Log($"{TagFlag} GetSkillRelease OnPointerDown CheckStateIsForbid IsForbidSkill=true");
                    return false;
                }
            }
            else
            {
                // 是否原子锁了普攻
                bool IsForbidAttack = playerData.myOwnerNtt.Data.CheckStateIsForbid(E_BattleStateType.BattleState_ForbidAttack);
                if (IsForbidAttack && !isCleanse)
                {
                    //DB_Close       SGF.Debuger.Log($"{TagFlag} GetSkillRelease OnPointerDown CheckStateIsForbid IsForbidAttack=true");
                    return false;
                }
            }
            return true;
        }

        public void OnSkillInfoRefreshCD(SkillInfo skillInfo, float cd)
        {
            // SGF.Debuger.LogError($"[技能CD] {GetHashCode()} OnSkillInfoRefreshCD : {cd} curSkill: {CurSkillInfo?.skillId}  , curShowSkill: {CurShowSkillInfo.skillId} , skillInfo: {skillInfo.skillId} [cd_key]: {skillInfo.CDKey} ");

            // 2023/8/23 
            // 跟高磊 沟通确定,  技能cd 显示的是 当前显示技能的cd
            if (skillInfo != CurShowSkillInfo)
            {
                // SGF.Debuger.LogError($"[技能CD] {GetHashCode()} OnSkillInfoRefreshCD : {cd} return!!! ");
                return;
            }

            RefreshCD(cd);
        }

        /// <summary>
        /// 刷新 cd, cd为0  或者 技能刷新 的时候都会刷新cd
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="isCDUpdateSkill">是否是 cd 刷新更新的技能, cd 刷新为 0 的时候， 告诉外面 cd 结束</param>
        private void RefreshCD(float cd, bool isCDUpdateSkill = false)
        {
            // SGF.Debuger.LogError($"[技能CD] {GetHashCode()} RefreshCD : {cd} curSkill: {CurSkillInfo?.skillId}  , curShowSkill: {CurShowSkillInfo.skillId}");
            bool isCDIng = leastCD != 0 || isCDUpdateSkill;
            leastCD = cd;
            isStartCD = leastCD > 0;

            if (isStartCD)
            {
                ActionOnRefreshCD?.Invoke(leastCD);
            }
            else
            {
                //CD结束
                // 如果 本来是 cd 中, 然后刷新了一个新的技能cd 设置为0, 就通知表现出 cd 结束
                if (isCDIng)
                {
                    if (playerData.isMainPlayer)
                    {
                        // 主角cd 结束的通知
                        GameManager.Instance.TriggerEvent(TriggerEventType.M_SkillEndCD, CurSkillId);
                    }


                    ActionOnEndCD?.Invoke();
                }
            }


        }

        public bool IsNeedActive()
        {
            if (CurSkillInfo == null)
            {
                return false;
            }
            return CurSkillInfo.NeedActive;
        }

        public void OnUserInputStart(I_EffectParam effectParam)
        {
        }

        private string UserInputRecord = string.Empty;
        public void OnUserInputSteteChange(SkillEntity skillEntity, int effectId, bool isOpen, int totalTime, int curTime)
        {
            // 用户输入状态发生改变的时候, 刷新一次技能条件
            RefreshShowSkillInfos();

            // SGF.Debuger.LogError($"[CD] 刷新技能等待间隔: CurSkillId {CurSkillId} , curUserInputId: {curUserInputId},----> effectId: {effectId}, totalTime: {totalTime}, curTime: {curTime}, isOpen: {isOpen}  ");

            // 关闭输入轴的时候, 需要判定是不是当前输入轴. 而打开的时候, 则不需要
            string key = $"{skillEntity.skillId}_{effectId}";
            if (!isOpen && UserInputRecord.Equals(key))
            {
                UserInputRecord = string.Empty;
                ActionOnRefreshWaitInterval?.Invoke(this, false, 0, 0);
                return;
            }

            UserInputRecord = key;
            ActionOnRefreshWaitInterval?.Invoke(this, isOpen, totalTime, curTime);

        }

        /// <summary>
        /// 判定 一个技能槽 按钮能否点击
        /// 1.如果 技能槽 未填充技能, 技能槽 关闭;
        /// 2.如果 技能槽 技能未达到 系统开放等级， 技能槽关闭;
        /// 3.如果 玩家 身上有 这个槽位的 变身技能效果标签, 技能槽 显示;
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSkillCanClick()
        {
            if (!IsSystemOpen())
            {
                return false;
            }

            if (!HasSkill)
            {
                return false;
            }

            /// 如果存在技能, 判断这个技能 是否是 变身效果填充的技能.
            /// 目前高磊的 说法是 变身效果的技能如果存在, 这个槽位不管开不开起, 都需要显示
            /// 比如
            ///     新手引导的变身. 玩家一开始的技能槽是 不满足系统开放条件 并且 被 效果标签隐藏的。
            ///     但是对于 特定的技能槽，策划会配置 特定的变身技能。这个槽位也需要显示
            if (IsChangeSkillSlot)
            {
                string mapSubType = GameManager.Instance.GetMapSubType();
                if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                {
                    return BusinessManager.Instance.GetXinSGStateDic(PosID.ToString());
                }
                else
                {
                    return true;
                }

            }
            return true;
        }

        private bool IsSystemOpen()
        {

            switch (PosID)
            {
                case 3:
                    {
                        return SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.SkillOne);

                    }
                    break;
                case 4:
                    {
                        return SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.SkillTwo);
                    }
                    break;
                case 5:
                    {
                        return SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.SkillThree);
                    }
                    break;
                case 6:
                    {
                        return SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.SkillFour);
                    }
                    break;
                case 7:
                    {
                        return SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Ult);

                    }
                    break;

                default: return true;
            }
            return true;
        }


#if UNITY_EDITOR
        public void RefreshSkillInfo(int skillID)
        {
            for (int index = 0; index < ShowSkillCount; index++)
            {
                if (ShowSkillInfos[index].cfg.ID == skillID)
                {
                    SkillInfo skillInfo = InstanceSkillInfo();
                    skillInfo.Create(skillID, playerData, this, true);
                    int skillLevel = 1;
                    if (singleSkillInfo != null)
                    {
                        skillLevel = singleSkillInfo.SkillLevel;
                    }
                    skillInfo.SetSkillLevel(skillLevel);
                    ShowSkillInfos[index] = skillInfo;
                    break;
                }
            }
        }

        /// <summary>
        /// 替换 第一个 显示的 技能信息
        /// </summary>
        /// <param name="skillID"></param>
        public void ReplaceFirstSkillInfo(int skillID)
        {
            SkillInfo skillInfo = InstanceSkillInfo();
            skillInfo.Create(skillID, playerData, this, true);
            int skillLevel = 1;
            if (singleSkillInfo != null)
            {
                skillLevel = singleSkillInfo.SkillLevel;
            }
            skillInfo.SetSkillLevel(skillLevel);
            ShowSkillInfos[0] = skillInfo;
        }
#endif
    }
}