using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Time;
using SkillEditor;
using StarProject.Game.Entity;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    public partial class SkillEntity : EntityRemoteStatic
    {
        /// <summary>
        /// 原始的 技能阶段的配置数据,由于存在循环阶段的原因,
        /// 技能实体创建的 stageDic 包含循环阶段的配置.
        /// 所以此处 增加一个 配置的原始数据
        /// </summary>
        public List<StageJson> cfgStageJsons;
        /// <summary>
        /// 当前阶段 基于配置表的 阶段的idx
        /// </summary>
        private int _curStageIdx = 0;
        /// <summary>
        /// 当前配置表中的 阶段原始配置
        /// </summary>
        private StageJson CurCfgStageJson
        {
            get => cfgStageJsons[_curStageIdx];
        }

        // 当前阶段是否需要抢占活跃
        public bool NeedActive => CurCfgStageJson.StageType == StageType.NormalStage && CurCfgStageJson.StageNormal.Active;

        public SkillConfig cfg;

        private SkillInfo curSkillInfo = null;

        private bool isAutoTurnToTarget = true;
        public bool IsAutoTurnToTarget => isAutoTurnToTarget;

        /// <summary>
        /// 蓄力 单层的 时间
        /// </summary>
        private int energyItemTime = 0;
        public int EnergyItemTime => energyItemTime;

        /// <summary>
        /// 蓄力 最小层数
        /// </summary>
        private int energyMinCount = 0;
        public int EnergyMinCount => energyMinCount;

        /// <summary>
        /// 蓄力 最大层数
        /// </summary>
        private int energyMaxCount = 0;
        public int EnergyMaxCount => energyMaxCount;

        /// <summary>
        /// 已经蓄力的层数,可能蓄力0层
        /// </summary>
        public int energyedCount = 0;


        /// <summary>
        /// 蓄力的总时间
        /// </summary>
        private double energyTotalTime = 0;
        // 蓄力显示的UI总时间
        private double energyUITotalTime = 0;

        /// <summary>
        /// 蓄力的最大阶段时间
        /// </summary>
        public int energyStageMaxTime = 0;
        public int energyMaxTime = 0;

        /// <summary>
        /// 技能优先级
        /// </summary>
        public int Priority
        {
            get => cfg.Priority;
        }

        /// <summary>
        /// 检查 是否是在禁止使用技能的时间范围内
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool CheckInForbidenUseSkillTime(double time)
        {
            return time <= GameConfig.DYNAMIC_FORBID_RATE * TimeUtils.SinglePing + GameConfig.STATIC_SKILL_LAG_TIME;
        }


        /// <summary>
        /// 根据 服务器下发的蓄力结束通知 ,计算 实际的蓄力阶段的时间
        /// 如果传0,
        ///     得到的是蓄力阶段最大时间
        /// </summary>
        /// <param name="serverEnergyEndTime">服务器下发的蓄力结束通知</param>
        /// <param name="energySpeed">蓄力的速度,蓄力速度会改变蓄力的时间</param>
        /// <returns></returns>
        private int CalculateEnergyTime(ulong serverEnergyEndTime)
        {
            // TODO : DL
            // 是否拥有蓄力阶段，不在使用之前的逻辑
            // bool hasEnergyStage = skillInfo.HasEnergyStage();

            int energyTime = 0;
            //如果技能没有蓄力阶段,则每个阶段都是固定的时间点。设置energyTime=0,
            // if (!hasEnergyStage)
            // {
            //     energyTime = 0;
            // }
            // else
            // {
            //     //服务器传过来的已经蓄力的时间大于0（服务器只有蓄力阶段结束,才会在使用协议中告诉客户端蓄能了多久）
            //     //蓄力阶段已经结束,此时蓄力阶段时间设置为蓄力时间
            //     if (serverEnergyEndTime > 0)
            //     {

            //         energyTime = (int)(serverEnergyEndTime - createTime) - cfg.GetSkillProTime();
            //     }
            //     else
            //     {
            //         //服务器没有发蓄力时间,则可能此时还没有开始蓄力或者 蓄力没有结束
            //         //此时,直接用蓄力阶段的最大时间,去定位此时技能所处于什么阶段
            //         energyTime = GetRealSkillEnergyMaxTime(curEnergySpeed);
            //     }
            // }

            return energyTime;
        }

        /// <summary>
        /// 得到 当前蓄力阶段蓄力时间
        /// </summary>
        /// <param name="ignoreInitCount">是否忽视默认层数的时间,如果传true,则返回实时的蓄力时间,哪怕小于1层</param>
        /// <returns></returns>
        public double GetCurEnergyTime(bool ignoreInitCount = false)
        {
            int energyTime = energyItemTime;
            int baseEnergyCount = energyMinCount;

            // 如果小于初始层数*单次蓄力时间
            if (!ignoreInitCount && energyUITotalTime < baseEnergyCount * energyTime)
            {
                return baseEnergyCount * energyTime;
            }
            return energyUITotalTime;
        }

        /// <summary>
        /// 得到 当前蓄力阶段蓄力层数
        /// </summary>
        /// <param name="ignoreInitCount">是否忽视默认层数的时间,如果传true,则返回实时的蓄力时间,哪怕小于1层</param>
        /// <returns></returns>
        public int GetCurEnergyCount(bool ignoreInitCount = false)
        {
            int baseEnergyCount = energyMinCount;

            // 如果小于初始层数*单次蓄力时间
            int curEnergyedCount = energyedCount;
            if (!ignoreInitCount && energyedCount < baseEnergyCount)
            {
                curEnergyedCount = baseEnergyCount;
            }
            return curEnergyedCount;
        }


        private SkillWheelInfo skillWheelInfo = null;

        private SkillWheelInfo EnsureSkillWheelInfo()
        {
            if (skillWheelInfo == null)
            {
                return new SkillWheelInfo();
            }
            return skillWheelInfo;
        }

        public SkillWheelInfo GetSkillWheelConfig()
        {
            // 首先检查这个技能 当前是否开启了用户输入轴
            var effectParam = GetUserInput();

            // 如果未开启用户输入轴, 那就直接返回技能的原始配置
            if (effectParam == null)
            {
                return EnsureSkillWheelInfo().Init(cfg);
            }

            // 如果开启了用户输入, 那就需要获取当前用户轴的技能输入配置
            if (effectParam.SkillEffectType == E_SkillEffect.UserInput)
            {
                var chargeInput = effectParam.BaseEffect as EffectTypeUserInput;
                return EnsureSkillWheelInfo().Init(chargeInput);
            }
            else if (effectParam.SkillEffectType == E_SkillEffect.Energy)
            {
                EffectTypeChargeInput chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;
                return EnsureSkillWheelInfo().Init(cfg, chargeInput);
            }
            else
            {
                return EnsureSkillWheelInfo().Init(cfg);
            }
        }



        #region TODO Delete
        private int baseSkillProTime = 0;

        private int curAttackSpeed = 0;

        private int curEnergySpeed = 0;

        /// <summary>
        /// 当前技能抬手阶段的时长
        /// </summary>
        private int curSkillProTime = 0;

        private void ReleaseSkillPro()
        {
            baseSkillProTime = 0;
            curAttackSpeed = 0;
            curEnergySpeed = 0;
            curSkillProTime = 0;

        }
        /// <summary>
        /// 获取技能前摇的 真实 时间
        /// note:
        ///     1.目前 攻速 属性 只对 技能前摇阶段 进行计算
        /// </summary>
        /// <returns></returns>
        private int GetRealSkillProTime(int attackSpeed)
        {
            if (attackSpeed == 0)
            {
                return baseSkillProTime;
            }


            if (!skillContainer.IsNormalSkill())
            {
                return baseSkillProTime;
            }
            return baseSkillProTime * (int)GetRealAttackSpeed(attackSpeed);
        }

        private float GetRealAttackSpeed(int attackSpeed)
        {
            if (attackSpeed == 0)
            {
                return 1;
            }
            return (GameConfig.AttrMaxRateValue * 1.0f) / attackSpeed;
        }


        private int GetRealSkillEnergyMaxTime(int energySpeed)
        {
            return 0;
            // if (energySpeed == 0)
            // {
            //     return cfg.GetSkillEnergyMaxTime();
            // }

            // return cfg.GetSkillEnergyMaxTime() * energySpeed / GameConfig.AttrMaxRateValue;
        }

        private int GetRealSkillEnergyTime(int energySpeed)
        {
            if (energySpeed == 0)
            {
                return energyItemTime;
            }

            return energyItemTime * energySpeed / GameConfig.AttrMaxRateValue;
        }

        /// <summary>
        /// 获取技能的范围比例
        /// </summary>
        /// <returns></returns>
        public float GetSkillRangeRatio()
        {
            float ratio = 1f;
            // 如果是蓄力技能，要计算蓄力范围的规则
            // if (cfg.GetCastMethod() == (int)E_SkillCastMethodType.GatherWheelCast)
            // {
            //     E_SkillEnergyType dynamicRangeType = (E_SkillEnergyType)cfg.GetDynamicRangeType();
            //     float curEnergyTime = (float)GetCurEnergyTime(true);
            //     if (dynamicRangeType == E_SkillEnergyType.DynamicRangeType_Time)
            //     {
            //         //1、蓄力时间影响：
            //         // 当前蓄力时间 /（蓄力阶段最大蓄力次数*单次蓄力生效周期时间）---  是取最大有效蓄力时间 & 不可超过100%                                                                         
            //         //碰撞盒范围 = 碰撞盒范围数值参数 * 当前蓄力时间 /（蓄力阶段最大蓄力次数*单次蓄力生效周期时间）

            //         float curEnergyTimeRatio = (curEnergyTime / (cfg.GetSkillEnergyMaxNum() * cfg.GetSkillEnergyTime()));
            //         ratio = Mathf.Min(curEnergyTimeRatio, 1f);
            //     }
            //     // 蓄力层数影响
            //     else if (dynamicRangeType == E_SkillEnergyType.DynamicRangeType_Floor)
            //     {
            //         //2、蓄力层数影响：
            //         //碰撞盒范围=碰撞盒范围数值参数*当前蓄力层数/最大蓄力次数
            //         float curEnergyNumRatio = curEnergyTime / cfg.GetSkillEnergyMaxNum();
            //         ratio = Mathf.Min(curEnergyNumRatio, 1f);
            //     }
            // }
            return ratio;
        }

        /// <summary>
        /// 处理黑板数据
        /// </summary>
        /// <param name="BlackList"></param>
        private void HandleBlackList(RepeatedField<BlackBoardNode> BlackList)
        {
            foreach (ProtoMsg.BlackBoardNode item in BlackList)
            {
                object val = item.GetType().GetProperty(item.PropValueCase.ToString()).GetValue(item, null);
                switch (item.Key)
                {
                    //蓄力的层数
                    case "BBEnergy":
                        {
                            energyedCount = Convert.ToInt32(val);
                            // SGF.Debuger.Log($"{TagFlag} HandleBlackList {item.Key} ----> val {energyedCount}");
                            Fire.Utils.SafeRunAction(() =>
                            {
                                ActionOnBlackBord?.Invoke(item.Key, val);
                                ActionOnEnergyEnd?.Invoke(this);
                            });
                        }
                        break;
                    //蓄力的时间
                    case "BBEnergyTime":
                        {
                            TimeSpan cost = DateTime.UtcNow - now;
                            ulong energyBreakTime = Convert.ToUInt64(val);

                            Fire.Utils.SafeRunAction(() =>
                            {
                                ActionOnBlackBord?.Invoke(item.Key, val);
                            });

                        }
                        break;
                }
            }
        }

        #endregion


    }
}