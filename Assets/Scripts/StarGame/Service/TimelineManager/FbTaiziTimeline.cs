///--------------------------------------------------------------------
/// 文件名   :   FbTaiziTimeline.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/09 10:55:50
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.Module.Framework;
using StarProject.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace StarProject.Service.Timeline
{
    public class FbTaiziTimeline : BaseTimeline
    {
        protected override void OnPrepare(PlayableDirector director)
        {
            base.OnPrepare(director);

            GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStartHidden?.Invoke(false);

            if (PartnerManager.Instance.CurConcretizationPartner != null)
            {
                if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                {
                    PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStartHidden?.Invoke(false);
                }
            }
        }


        protected override void OnEnd()
        {
            GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStopHidden?.Invoke(false);
            if (PartnerManager.Instance.CurConcretizationPartner != null)
            {
                if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                {
                    PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStopHidden?.Invoke(false);
                }
            }
            base.OnEnd();
        }
    }
}