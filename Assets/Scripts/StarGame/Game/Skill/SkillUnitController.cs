using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProject.Game.Data;
using Google.Protobuf.Collections;
using ProtoMsg;
using StarProject.Game.Entity.Factory;
using System;
using SGF.Time;
using StarProjectDef;
using StarProject.Service.LocalData;
using SkillEditor;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能槽的控制器
    ///     一个 技能技能位 包含 多个 技能槽(SkillContainer)
    ///     一个 技能槽(SkillContainer) 包含多个 技能(SkillInfo)
    /// </summary>
    public class SkillUnitController
    {
        protected string LOG_TAG
        {
            get => $"[SkillUnitController_{playerData.M_EntityID}]";
        }

        private VitalSignData playerData;

        private Transform container;

        public SkillDispatcher skillDispatcher;

        /// <summary>
        /// 技能位信息，一个技能位，包好多个技能（多段技能）
        /// </summary>
        public DictionaryEx<int, SkillContainer> SkillContainerDic = new DictionaryEx<int, SkillContainer>();


        /// <summary>
        /// 万能技能槽dic,用来存放多个技能,避免单一的技能槽反复来回的release和create
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="SkillContainer"></typeparam>
        /// <returns></returns>
        private DictionaryEx<int, SkillContainer> anySkillContainerDic = new DictionaryEx<int, SkillContainer>();

        /// <summary>
        /// 刷新所有的技能位
        /// </summary>
        public Action ActionOnRefreshAllSkill;

        /// <summary>
        /// 技能开启CD时间的history,单独存放于一个map的原因,
        /// 是因为技能槽中的技能 可能会 变化,有些技能,也可能是填充到 anyContainer中,
        /// 所以如果将 startCDTime 存放在 skill中,技能变化 重新填充后，startCDTime就会丢失
        /// 
        /// note:
        ///     变身技能重新填充,理论上 技能有些信息应该保存,走的是 reset,
        ///     所以,skillStartCDTimeHistroyDic 的释放, 应该放在release中,reset 不清除此数据
        /// </summary>
        /// <typeparam name="int">skillID</typeparam>
        /// <typeparam name="long">startCDTime</typeparam>
        /// <returns></returns>
        public DictionaryEx<int, long> skillStartCDTimeHistroyDic = new DictionaryEx<int, long>();


        public void Create(SkillDispatcher dispatcher, VitalSignData data, Transform parent)
        {
            skillDispatcher = dispatcher;
            playerData = data;
            container = parent;

            playerData.ActionOnAllSkillNotice += OnActionSkillRefresh;
            playerData.ActionOnAllCDNotice += OnActionAllCDNotice;
            playerData.ActionOnRoleAllCDListNtf += OnRoleAllCDListNtf;
            playerData.ActionOnCDUpdateNotice += RefreshSkillUnitCD;

            // 变身这种技能槽刷新的 接口
            playerData.ActionOnChangeSkillSlot += OnChangeSkillSlot;
        }

        public void EnterFrame()
        {
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                item.Value.EnterFrame();
            }
            foreach (KeyValuePair<int, SkillContainer> item in anySkillContainerDic)
            {
                item.Value.EnterFrame();
            }
        }

        /// <summary>
        /// 直接通过 技能list 来初始化技能槽信息
        /// </summary>
        /// <param name="skillLists"></param>
        public void CreateWithSkillLists(List<KeyValuePair<int, List<int>>> skillLists)
        {
            ReleaseSkillContainerDic(SkillContainerDic);

            for (int i = 0; i < skillLists.Count; i++)
            {
                KeyValuePair<int, List<int>> skillKeyValue = skillLists[i];
                int posID = skillKeyValue.Key;
                List<int> skillIds = skillKeyValue.Value;
                if (skillIds.Count == 0)
                {
                    continue;
                }
                SkillContainer skillContainer = EntityFactory.InstanceEntity<SkillContainer>();
                skillContainer.Create(skillIds, playerData, skillDispatcher, container);

                // 如果采用的是 [[1,2],[3]] 嵌套 技能id的方式 初始化技能, 这种方式一般是针对伙伴/或者其它 
                // 非主角初始化技能来用的,它们 都是直接读取配置表中的 技能 id , 而并不是像主角一样,由服务器
                // 下发的 职业技能信息 来初始化技能槽。 所以 此处 单独开发一个 create 接口,用来直接初始化
                // 只有技能id 的技能
                SkillContainerDic.Add(posID, skillContainer);
            }
            ActionOnRefreshAllSkill?.Invoke();
        }

        /// <summary>
        /// 刷新所有的技能位数据
        /// </summary>
        /// <param name="cdLists"></param>
        public void OnActionAllCDNotice(RepeatedField<CDData> cdLists)
        {
            //SGF.Debuger.LogError($"OnActionAllSkillNotice Count {playerData.skillPosList.Count}");

            //刷新对应技能位的CD
            for (int i = 0; i < cdLists.Count; i++)
            {
                RefreshSkillUnitCD(cdLists[i]);
            }
        }

        /// <summary>
        /// 主角 cd 列表的全同步
        /// 这个只会在 副本里发(目前). cdLists.Count == 0时清空所有 cd
        /// </summary>
        /// <param name="cdLists"></param>
        public void OnRoleAllCDListNtf(RepeatedField<CDData> cdLists)
        {
            foreach (var item in cdKey2CDData)
            {
                CDData cdData = item.Value;

                // 先清空所有 已经有的 cdKey 对应的 cd
                RefreshLeastCD(cdData, 0);
            }

            ClearCDRecord();

            foreach (CDData item in cdLists)
            {
                // 刷新 对应的 cdData
                RefreshSkillUnitCD(item);
            }

        }

        public void OnActionSkillRefresh(object o)
        {
            CreateSkillUnits();
        }

        private void CreateSkillUnits()
        {
            if (playerData.HasChangeSkillSlot)
            {
                OnChangeSkillSlot(playerData.changeSkillSlots);
                return;
            }

            RefreshDefaultSkillUnits();
        }

        private void RefreshDefaultSkillUnits()
        {
            ReleaseSkillContainerDic(SkillContainerDic);

            //初始化对应的技能位
            for (int i = 0; i < playerData.skillPosList.Count; i++)
            {
                SkillPos skillPos = playerData.skillPosList[i];
                SkillContainer skillUnit = EntityFactory.InstanceEntity<SkillContainer>();
                //SGF.Debuger.LogError($"OnActionAllSkillNotice i {i}");
                try
                {
                    skillUnit.Create(skillPos, playerData, skillDispatcher, container);
                    SkillContainerDic.Add(skillPos.PosID, skillUnit);
                }
                catch (Exception ex)
                {
                    SGF.Debuger.LogWarning($"OnActionAllSkillNotice 创建技能位 [{skillPos.JobSkillID}] 异常: {ex.Message}");
                }


            }

            ActionOnRefreshAllSkill?.Invoke();
        }

        /// <summary>
        /// 技能槽 刷新接口。 
        ///     目前主要 用于变身时 刷新技能槽技能的效果标签. 
        ///     技能槽所有技能会 统一替换
        /// </summary>
        /// <param name="talentAndSlots"></param>
        public void OnChangeSkillSlot(List<TalentAndSlot> talentAndSlots)
        {
            // 如果 技能槽的 变身技能标签没了, 那就还原 这个技能槽
            if (talentAndSlots == null)
            {
                RefreshDefaultSkillUnits();
                return;
            }

            /// 2024/10/22
            /// gl 需求:
            ///     槽位配置 0 表明隐藏 槽位, 否则都继承 之前的槽位 
            if (talentAndSlots.Count == 0)
            {
                RefreshDefaultSkillUnits();
                return;
            }

            // 如果 技能槽的 变身效果标签 配置的 技能槽为 [],表明 需要隐藏所有技能.
            // if (talentAndSlots.Count == 0)
            // {
            //     ReleaseSkillContainerDic(SkillContainerDic);
            //     return;
            // }
            RefreshCurTalentSlots(talentAndSlots, curTalentSlots);

            // 通过变身效果标签 的技能槽位配置 生成对应的技能槽
            RefreshSkillUnitsWithTalentSlots(curTalentSlots);
        }

        private List<TalentAndSlot> curTalentSlots = new();

        private TalentAndSlot GetTalentAndSlot(SkillPos skillPos)
        {
            var talentAndSlot = new TalentAndSlot();
            talentAndSlot.SlotID = skillPos.PosID;
            talentAndSlot.TalentID = skillPos.PosID;

            return talentAndSlot;
        }

        private void RefreshCurTalentSlots(List<TalentAndSlot> changeTalentSlots, List<TalentAndSlot> curTalentSlots)
        {
            curTalentSlots.Clear();

            // 首先刷一个 默认的身上的 技能槽
            for (int i = 0; i < playerData.skillPosList.Count; i++)
            {
                var skillPos = playerData.skillPosList[i];
                curTalentSlots.Add(GetTalentAndSlot(skillPos));
            }

            for (int i = 0; i < changeTalentSlots.Count; i++)
            {
                var changeSlot = changeTalentSlots[i];

                // 如果 gl 配置 0, 那就隐藏身上的技能位
                if (changeSlot.TalentID == 0)
                {
                    RemoveListTarget(curTalentSlots, (item) =>
                    {
                        return item.SlotID == changeSlot.SlotID;
                    });
                }
                else
                {
                    UnionListTarget(curTalentSlots, changeSlot, (item) =>
                    {
                        return item.SlotID == changeSlot.SlotID;
                    });
                }

            }

        }

        private void RemoveListTarget<T>(List<T> targets, Predicate<T> check)
        {
            var find = targets.Find(check);
            if (find != null)
            {
                targets.Remove(find);
            }
        }

        private void UnionListTarget<T>(List<T> targets, T target, Predicate<T> check)
        {
            var idx = targets.FindIndex(check);
            if (idx == -1)
            {
                targets.Add(target);
            }
            else
            {
                targets[idx] = target;
            }
        }

        private void RefreshSkillUnitsWithTalentSlots(List<TalentAndSlot> talentAndSlots)
        {
            ReleaseSkillContainerDic(SkillContainerDic);

            // 通过 变身效果标签的配置 生成对应的技能槽位
            for (int i = 0; i < talentAndSlots.Count; i++)
            {
                var slot = talentAndSlots[i];
                SkillContainer skillUnit = EntityFactory.InstanceEntity<SkillContainer>();
                try
                {
                    skillUnit.Create(slot, playerData, skillDispatcher, container);
                    SkillContainerDic.Add(slot.SlotID, skillUnit);
                }
                catch (Exception ex)
                {
                    SGF.Debuger.LogWarning($"OnActionAllSkillNotice 创建技能位 [{slot.SlotID}] , 天赋TalentID: {slot.TalentID} 异常: {ex.Message}");
                }
            }

            ActionOnRefreshAllSkill?.Invoke();
        }

        public void RefreshSkillUnitCD(CDData cDData)
        {
            /// 2023/3/21
            /// 发现 服务器 会将 其他人使用 技能的 cd 通知 给主角, 但此时, 主角 没有其他人的 技能信息.
            /// 目前 已知的 约定 如下:
            ///     1. 怪物 伙伴等 , 它们的职业 技能 就是战斗技能;
            ///     2. 对于 其他人的 技能cd , 服务器 同步过来的 只有 职业技能 JobSkillID, 没有战斗技能 信息,
            ///         此时, 客户端 没办法 根据 JobSkillID 推算出 CDData 对应的是 哪些 战斗技能, 
            ///         生成的万能技能 槽 skillInfo(把 JobSkillID 当成 battleSkillID 生成 ) 信息会找不到技能配置.
            /// 
            ///     综上, 需要 服务器 屏蔽 其他人的 CDData 信息同步, 以及 客户端 自己 过滤掉 其他人的CD 信息同步.

            if (!playerData.isMainPlayer && !playerData.myOwnerNtt.M_IsMainPlayerSummon)
            {
                return;
            }

            RecordCDData(cDData);

            int cd = (int)GetLeastCD(cDData.CDKey);

            RefreshLeastCD(cDData, cd);

            // SkillContainer skillContainer = GetSkillUnitByJobSkill(cDData.JobSkillID);
            // if (skillContainer != null)
            // {
            //     skillContainer.RefreshCD(cDData);
            //     return;
            // }

            // // note:
            // // 服务器目前约定, 对于伙伴技能这种, 它们的 技能id就是 jobSkillID            
            // skillContainer = GetSkillUnit(cDData.JobSkillID);
            // if (skillContainer == null)
            // {
            //     return;
            // }
            // skillContainer.RefreshCD(cDData);
        }

        private void RefreshLeastCD(CDData cDData, int cd)
        {
            // 刷新 所有的 技能槽位  关于 cdKey 的 技能skillInfo 数据
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                item.Value.RefreshCD(cDData, cd);
            }

            foreach (KeyValuePair<int, SkillContainer> item in anySkillContainerDic)
            {
                item.Value.RefreshCD(cDData, cd);
            }
        }

        // note:
        // 夏哥墙裂要求，俩个伙伴技能id【绝对不可能公用一个技能id】
        public bool CheckIsMySkillById(int jobSkillID)
        {
            SkillContainer skillContainer = GetSkillUnitByJobSkill(jobSkillID);
            if (skillContainer != null)
            {
                return true;
            }
            return false;
        }

        public List<SkillContainer> GetSkillCDKeySkillUnit(int cdKey)
        {
            List<SkillContainer> skills = new List<SkillContainer>();
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                if (item.Value.IsPadding)
                {
                    bool find = item.Value.EqualSkillCDKey(cdKey);
                    if (find)
                    {
                        skills.Add(item.Value);
                    }
                }
            }

            foreach (KeyValuePair<int, SkillContainer> item in anySkillContainerDic)
            {
                if (item.Value.IsPadding)
                {
                    bool find = item.Value.EqualSkillCDKey(cdKey);
                    if (find)
                    {
                        skills.Add(item.Value);
                    }
                }
            }

            return skills;
        }

        /// <summary>
        /// 根据 jobSkillID 寻找 技能槽
        /// note:
        ///     条件是 不同的 技能槽不能 有相同的 职业技能
        /// </summary>
        /// <param name="jobSkillID"></param>
        /// <returns></returns>
        public SkillContainer GetSkillUnitByJobSkill(int jobSkillID)
        {
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                if (item.Value.IsPadding)
                {
                    bool find = item.Value.EqualJobSkillID(jobSkillID);
                    if (find)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 找到skillId 对应的技能位 的skillPosID
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        private int FindSkillUnitPosId(int skillId, DictionaryEx<int, SkillContainer> containerDic)
        {
            foreach (KeyValuePair<int, SkillContainer> item in containerDic)
            {
                int idx = item.Value.FindSkillIdx(skillId);
                if (idx != -1)
                {
                    return item.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获得 技能 skillId 对应的 技能槽
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public SkillContainer GetSkillUnit(int skillId)
        {
            int idx = FindSkillUnitPosId(skillId, SkillContainerDic);
            if (-1 != idx)
            {
                return SkillContainerDic[idx];
            }

            // 如果找不到,那就去万能技能槽中找
            idx = FindSkillUnitPosId(skillId, anySkillContainerDic);
            // 如果也找不到,那就创建一个新的万能技能槽
            if (-1 == idx)
            {
                SkillContainer skillContainer = EntityFactory.InstanceEntity<SkillContainer>();
                skillContainer.Create(new List<int> { skillId }, playerData, skillDispatcher, container);
                anySkillContainerDic[skillId] = skillContainer;
                return skillContainer;
            }
            else
            {
                return anySkillContainerDic[idx];
            }
        }

        public void ClientStartCD(int skillID)
        {
            skillStartCDTimeHistroyDic[skillID] = TimeUtils.ClientNowStampMilli;
        }

        public long GetSkillCDStartTime(int skillID)
        {
            if (skillStartCDTimeHistroyDic.ContainsKey(skillID))
            {
                return skillStartCDTimeHistroyDic[skillID];
            }
            return 0;
        }

        /// <summary>
        /// 预留的接口,后续看 需求技能槽重置到底需要什么逻辑(例如死亡重生的重置)
        /// </summary>
        public void Reset()
        {

        }


        public void Release()
        {
            Reset();

            ReleaseSkillContainerDic(SkillContainerDic);

            ReleaseSkillContainerDic(anySkillContainerDic);


            skillStartCDTimeHistroyDic.Clear();


            ActionOnRefreshAllSkill = null;
        }

        private void ReleaseSkillContainerDic(DictionaryEx<int, SkillContainer> containerDic)
        {
            foreach (var item in containerDic)
            {
                EntityFactory.ReleaseEntity(item.Value);
            }
            containerDic.Clear();
        }

        private Dictionary<int, CDData> cdKey2CDData = new();
        public void RecordCDData(CDData cDData)
        {
            cdKey2CDData[cDData.CDKey] = cDData;
        }

        public void ClearCDRecord()
        {
            cdKey2CDData.Clear();
        }

        public int GetLeastCD(int cdKey)
        {
            if (!cdKey2CDData.ContainsKey(cdKey))
            {
                return 0;
            }

            var cDData = cdKey2CDData[cdKey];

            int leastCD = 0;

            long cdStartTime = cDData.StartCDTime;
            long cdEndTime = cDData.EndCDTime;

            long cdTime = cdEndTime - cdStartTime;

            long serverNow = TimeUtils.ServerNowStampMilli;


            //普工的情况
            if (cdTime <= 0)
            {
                leastCD = 0;
            }
            else
            {
                if (cdStartTime <= serverNow && serverNow < cdEndTime)
                {
                    leastCD = (int)(cdEndTime - serverNow);
                }
                else if (cdStartTime > serverNow)
                {
                    leastCD = (int)cdTime;
                }
                else
                {
                    leastCD = 0;
                }
            }

            return leastCD;
        }

#if UNITY_EDITOR
        //刷新技能信息
        public void RefreshSkillInfo(int skillID)
        {
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                item.Value.RefreshSkillInfo(skillID);
            }
        }

        public void UpdateSkill(int skillID)
        {
            foreach (KeyValuePair<int, SkillContainer> item in SkillContainerDic)
            {
                SkillPosSetDataCell skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key);
                if (skillPosSetDataCell != null && skillPosSetDataCell.GetSort() == 2)
                {
                    item.Value.ReplaceFirstSkillInfo(skillID);
                    return;
                }

            }
        }
#endif
    }
}
