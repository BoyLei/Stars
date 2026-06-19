///--------------------------------------------------------------------
/// 文件名   :   BaseTrigger.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/05 15:03:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game;
using StarProject.Game.Player;
using System.Collections;
using System.Collections.Generic;
using Task;
using UnityEngine;
namespace Trigger
{
    public abstract class BaseTrigger : ITrigger
    {
        public abstract TrrigerType M_TriggerType { get; }

        /// <summary>
        /// 全局唯一ID
        /// </summary>
        public int Index { get; protected set; }
        /// <summary>
        /// 对象ID
        /// </summary>
        public ulong M_EntityID { get; private set; }

        /// <summary>
        /// 配置ID
        /// </summary>
        public long ConfigID { get; protected set; }
        /// <summary>
        /// 最大触发次数
        /// </summary>
        public int MaxCount { get; private set; }

        /// <summary>
        /// 已经触发次数
        /// </summary>
        public int TriggerCount { get; protected set; }

        
        /// <summary>
        /// 是不是主角
        /// </summary>
        public  bool IsMainPlayer { get; protected set; }
        public PlayerCtrlGroup Player
        {
            get
            {
                return (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            }
        }

        /// <summary>
        /// 触发器是否有效
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return MaxCount == -1 || TriggerCount < MaxCount;
        }

        private System.Action<ulong> TriggerAction;

        public BaseTrigger(int count,bool isMainPlayer, System.Action<ulong> action)
        {
            this.MaxCount = count;
            this.IsMainPlayer = isMainPlayer;
            this.TriggerAction = action;
        }

        protected virtual void OnCreate()
        {
            TriggerCount = 0;
        }

        protected virtual void OnRelease()
        {
            
        }

        public virtual void EnterFrame(int frameIndex)
        {
        }

        public void SetEntityID(ulong entityID)
        {
            M_EntityID = entityID;
        }

        protected void OnTrigger()
        {
            TriggerAction?.Invoke(M_EntityID);
            TriggerCount++;
        }
    }
}