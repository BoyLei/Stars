using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
namespace StarProject.Game.Skill
{

    /// <summary>
    /// 基于时间线timeline上的事件帧
    /// note:
    ///     基于timeLine事件帧的 拷贝, 如果后续有属性增加,需要在Copy中添加
    /// </summary>
    public class TimeLineFrameEvent
    {
        /// <summary>
        ///  在timeLine上的开始时间
        /// </summary>
        public int TimeLineTime;
        /// <summary>
        ///  在timeLine上的结束时间
        /// </summary>
        public int TimeLineEndTime;

        /// <summary>
        /// 基于阶段 的时间
        /// </summary>
        public int StageFrameTime;

        /// <summary>
        /// 事件帧的长度
        /// </summary>
        public int Duration;


        /// <summary>
        /// 是否是 用来server恢复的 拷贝的开始帧.
        /// note:
        ///     如果一个 动画 横跨 ABC 3个阶段,那么 在 B/C 的开始阶段, 会自动生成一个 copy的 动画开始帧.
        ///     这样, 当我们做 阶段恢复的时候, 不管是在 B 还是 C, 都能够 只依赖 这一个阶段,恢复这个动画
        /// </summary>
        public bool IsRecoverCopyStartFrame = false;

        /// <summary>
        /// 生成的恢复帧 在timeLine轴上 实际跳过了 多久
        /// </summary>
        public int RecoverJumpTime = 0;
        /// <summary>
        /// 优先级,用来 在Time 相同时,排序
        /// </summary>
        public int priority;

        public object frameData;

        public E_StageFrameEventType StageFrameEventType;

        public bool isFrameStart = false;

        public string StageIdStr = "";

        /// <summary>
        /// 基于 整个 TimeLine 事件轴上 的 事件帧
        /// </summary>
        /// <param name="timeLineTime">效果在整个TimeLine上的时间</param>
        /// <param name="duration">事件帧的 长度</param>
        /// <param name="data">事件帧 的 实际 数据</param>
        /// <param name="e_StageFrameEventType">事件帧的 类型</param>
        /// <param name="stageIDStr">事件帧 存在 的 阶段 </param>
        public TimeLineFrameEvent(int timeLineTime, int duration, object data, E_StageFrameEventType e_StageFrameEventType, string stageIDStr)
        {
            TimeLineTime = timeLineTime;
            Duration = duration;
            frameData = data;
            StageFrameEventType = e_StageFrameEventType;
            StageIdStr = stageIDStr;

            TimeLineEndTime = TimeLineTime + Duration;

            IsRecoverCopyStartFrame = false;

            RecoverJumpTime = 0;
        }

        /// <summary>
        /// 设置 事件帧 在阶段上的开始时间, 目前 好像会将事件帧在整个timeLine时间轴上重新排布,
        /// 所以 在排布后, 会 重新设置 它的 阶段 开始时间.
        /// 目前 这块逻辑 有点久远且 稳定, 所以 就先保留, 暂时不管!!!
        /// </summary>
        /// <param name="stageStartTime"></param>
        public void SetStageStartTime(int stageStartTime)
        {

            SetStageFrameTime(TimeLineTime - stageStartTime);
        }

        private void SetStageFrameTime(int time)
        {
            StageFrameTime = time;
        }


        /// <summary>
        /// note:
        ///     基于timeLine事件帧的 拷贝, 如果后续有属性增加,需要在Copy中添加
        /// </summary>
        public TimeLineFrameEvent Copy(bool isRecoverCopy)
        {
            TimeLineFrameEvent copy = new TimeLineFrameEvent(TimeLineTime, Duration, frameData, StageFrameEventType, StageIdStr);
            copy.StageFrameTime =StageFrameTime;

            copy.priority = priority;
            copy.isFrameStart = isFrameStart;
            copy.IsRecoverCopyStartFrame = isRecoverCopy;
            copy.RecoverJumpTime = RecoverJumpTime;
            return copy;
        }
    }
}