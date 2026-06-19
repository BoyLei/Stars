///--------------------------------------------------------------------
/// 文件名   :   TimerTrigger.cs
/// 内  容   :   
/// 说  明   :  定时器
/// 创建日期 :   2023/05/04 18:35:36
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Task;
using UnityEngine;
namespace Trigger
{
    public class TimerTrigger : BaseTrigger
    {
        public override TrrigerType M_TriggerType => TrrigerType.TimerTrriger;

        /// <summary>
        /// 延迟时间
        /// </summary>
        public float Delay { get; private set; }

        /// <summary>
        /// 触发间隔
        /// </summary>
        public float Interval { get; private set; }

        /// <summary>
        /// 上次触发时间
        /// </summary>
        private float lastTriggerTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="interval">间隔</param>
        /// <param name="count">触发次数</param>
        /// <param name="action">回调</param>
        public TimerTrigger(int index,long configid, float delay, float interval, int count,bool isMainPlayer, System.Action<ulong> action) : base(count,isMainPlayer, action)
        {
            this.Index = index;
            this.ConfigID = configid;
            this.Delay = delay;
            this.Interval = interval;
            OnCreate();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            lastTriggerTime = 0;
        }

        public override void EnterFrame(int frameIndex)
        {
            if (!IsValid())
            {
                return;
            }

            if (Delay > 0)
            {
                Delay -= Time.deltaTime;

                return;
            }

            if (Time.time - lastTriggerTime < Interval)
            {
                return;
            }

            OnTrigger();
            lastTriggerTime = Time.time;

        }
    }
}