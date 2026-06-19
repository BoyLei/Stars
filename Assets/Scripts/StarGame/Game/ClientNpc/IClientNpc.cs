///--------------------------------------------------------------------
/// 文件名   :   IClientNpc.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/04 13:42:00
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClientNpc
{
    public interface IClientNpc
    {
        /// <summary>
        /// 帧更新
        /// </summary>
        /// <param name="frameIndex"></param>
        void EnterFrame(int frameIndex);

        /// <summary>
        /// 释放
        /// </summary>
        void OnRelease();

        /// <summary>
        /// 进入视野
        /// </summary>
        void OnEnterAOI(EntityCtrlBase  ctrlBase);

        /// <summary>
        /// 离开AOI
        /// </summary>
        void OnLeaveAOI();

    }
}