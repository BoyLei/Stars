///--------------------------------------------------------------------
/// 文件名   :   Special_PartnerTutorial.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/05/20 16:48:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Network;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

public class Special_PartnerTutorial
{
    /// <summary>
    /// 是否需要检测
    /// </summary>
    public bool NeedCheck { get; private set; }


    /// <summary>
    /// 是否触发
    /// </summary>
    public bool IsTrigger { get; private set; }

    public int TriggerIndex { get; private set; }

    public Dictionary<int, float> mCD_IsFull = new Dictionary<int, float>();

    public void Init()
    {
        NeedCheck = false;
        IsTrigger = false;
        mCD_IsFull.Clear();
        if (!TaskHelper.IsTaskFinsh(SystemConstConfigs.Guidance_NoInputHLEndTask))
        {
            NeedCheck = true;
            MonoHelper.AddUpdateListener(OnUpdate);
        }

        GlobalEvent.TaskStageChange.AddListener(OnTaskStatechange);
        GlobalEvent.OnMsgPartnerConcretizeRet.AddListener(OnUsePartnerParSkill);
        GlobalEvent.onMainPlayerChanageBattleState.AddListener(OnBattleStateChange);
    }

    private void OnBattleStateChange(E_PlayerStateForMusic  stateForMusic)
    {
        if (!BattleManager.Instance.IsSeverBattleState)
        {
            Debug.Log("非战斗状态清除");
            GlobalEvent.OnShowGuidancePartnerClean.Invoke(0);
            IsTrigger = false;
            TriggerIndex = 0;
            mCD_IsFull.Clear();
        }
    }

    private void OnUsePartnerParSkill(PartnerConcretizeRet msg, long index)
    {
        Debug.Log("使用伙伴技能清除");
        IsTrigger = false;
        TriggerIndex = 0;
        mCD_IsFull.Clear();
    }

    private void OnTaskStatechange(int type, int id)
    {
        //任务完成
        if (type == 3)
        {
            if (id == SystemConstConfigs.Guidance_NoInputHLEndTask)
            {
                Debug.Log("任务完成清除");
                NeedCheck = false;
                GlobalEvent.OnShowGuidancePartnerClean.Invoke(0);
                IsTrigger = false;
                TriggerIndex = 0;
                mCD_IsFull.Clear();
            }
        }
    }

    private void OnUpdate()
    {
        if (NeedCheck)
        {
            if (!BattleManager.Instance.IsSeverBattleState)
            {
                return;
            }

            ///新手关 不处理
            if (GameManager.Instance.GetCurMapId() == 10004)
            {
                return;
            }
            if (!IsTrigger)
            {
                // 上阵未出战的伙伴
                var partners = BusinessManager.Instance.GetPartnerMDMgr().GetInBattlePatner();
                if (partners.Count > 0)
                {
                    foreach (var partner in partners)
                    {
                        if (partner.Index == 1901002 || partner.Index == 1901302)
                        {
                            continue;
                        }
                        var config = LocalDataManager.Instance.GetPartnerDataCell(partner.Index);
                        if (config != null)
                        {
                            int index = (int)partner.Index;
                            if (config.GetGuidUseSkiOp())
                            {
                                if (mCD_IsFull.ContainsKey(index))
                                {
                                    if (Time.time - mCD_IsFull[index] >=
                                        SystemConstConfigs.Guidance_NoParSkillHLInterval)
                                    {
                                        TriggerIndex = index;
                                        GlobalEvent.OnShowGuidancePartner.Invoke(index);
                                        IsTrigger = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    //引导技能
                                    var skillCfg =
                                        LocalDataManager.Instance.GetParSkillDataCell(index, partner.CurStar);
                                    var skillID = skillCfg.GetParSkill();
                                    float m_SkillCD = 0; //当前CD
                                    float m_SkillMaxCD = 0; //最大CD
                                    GameManager.Instance.GetPartnerSkillCDBySkillID(skillID, out m_SkillCD,
                                        out m_SkillMaxCD);

                                    if (m_SkillCD <= 0)
                                    {
                                        //CD 好了 开始计时
                                        if (!mCD_IsFull.ContainsKey(index))
                                        {
                                            mCD_IsFull.Add(index, Time.time);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public void OnRelease()
    {
        MonoHelper.RemoveUpdateListener(OnUpdate);
        GlobalEvent.TaskStageChange.RemoveListener(OnTaskStatechange);
        GlobalEvent.OnMsgPartnerConcretizeRet.RemoveListener(OnUsePartnerParSkill);
        GlobalEvent.onMainPlayerChanageBattleState.RemoveListener(OnBattleStateChange);
    }
}