using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 重新封装的一层基于TimeLine时间的阶段数据结构
    /// 使用的目的,是为了将每个 事件帧 对应生成的结束帧,能够正确的 排布到基于时间TimeLine的不同阶段上
    /// </summary>
    public class TimeLineStage
    {
        public int Start;
        public int End;

        public int StageID;

        /// <summary>
        /// 阶段的循环 次数
        /// </summary>
        public int Loop;

        public bool isLoopStage;

        public string StageIDStr;

        /// <summary>
        /// 是否时第一个阶段,在第一个阶段事件帧导出时 [] , 其它阶段时 (]
        /// </summary>
        public bool IsFirstStage;

        public List<TimeLineFrameEvent> frameEvents = new List<TimeLineFrameEvent>();

        /// <summary>
        /// next效果effectID 对应的 事件帧,此处需要跟 frameEvents单独区分,因为next效果帧的执行,
        /// 并不像frameEvents一样,按照timeLine上面的时间从前往后 依次执行.
        /// </summary>
        /// <typeparam name="int">effectID</typeparam>
        /// <typeparam name="TimeLineFrameEvent"></typeparam>
        public Dictionary<int, TimeLineFrameEvent> nextFrameEvents = new Dictionary<int, TimeLineFrameEvent>();

        /// <summary>
        /// next效果 对应 parent 效果 id 的map
        /// 
        /// 2023/3/8
        ///     note: 目前  gl 保证 所有的 next效果 id  是 唯一的, 且 每个 next 效果 不存在多个 父效果.
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        public Dictionary<int, int> nextFrame2ParentMap = new Dictionary<int, int>();

        public TimeLineStage(int stageID, int loop, bool isFirstStage, int originLoopCount)
        {
            StageID = stageID;
            Loop = loop;
            StageIDStr = SkillStage.FormatStageIDStr(StageID, Loop);
            IsFirstStage = isFirstStage;
            isLoopStage = -1 == originLoopCount || originLoopCount > 1;
        }

        public void AddFrameEvent(TimeLineFrameEvent frameEvent)
        {
            frameEvents.Add(frameEvent);
        }

        public void AddNextFrameEvent(int effectID, TimeLineFrameEvent frameEvent, int parentEffectID)
        {
            if (nextFrameEvents.ContainsKey(effectID))
            {
                SGF.Debuger.LogWarning($"TimeLineStage AddNextFrameEvent() nextFrameEvents StageIDStr={StageIDStr},effectID={effectID} 相同的key");
            }
            else
            {
                nextFrameEvents.Add(effectID, frameEvent);
            }

            if (nextFrame2ParentMap.ContainsKey(effectID))
            {
                SGF.Debuger.LogWarning($"TimeLineStage AddNextFrameEvent() nextFrame2ParentMap StageIDStr={StageIDStr},effectID={effectID} 相同的key");
            }
            else
            {
                nextFrame2ParentMap.Add(effectID, parentEffectID);
            }
        }

        public bool Equals(int stageID, int loop)
        {
            return StageID == stageID && Loop == loop;
        }
    }
}
