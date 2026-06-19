using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using SkillEditor;
using System;

namespace EditorModeTest
{
    /// <summary>
    /// 跟 夏哥沟通, 阶段 上的触发器 绑定在 主运行时身上.当 实体相应响应一个事件的时候，
    /// 会去遍历 实体的所有 运行时
    /// </summary>
    public class LocalServerTrigger
    {
        StageTrigger stageTrigger;

        public StageJson TriggerStageJson;
        public TriggerStageEnum StageEnum;

        /// <summary>
        /// 已经触发了的次数
        /// </summary>
        public int TriggeredCount = 0;

        public int TotalTriggerCount = 0;

        /// <summary>
        /// 触发器 是否 还有效(可以触发)
        /// </summary>
        public bool Active => TriggeredCount < TotalTriggerCount;

        /// <summary>
        /// 响应 触发器 触发的 action
        /// </summary>
        public Action<LocalServerTrigger, object> ActionOnTriggerStageTrigger;

        /// <summary>
        /// 检查触发器 是否可以触发
        /// </summary>
        public Func<LocalServerTrigger, object, bool> FuncOnCheckCanTrigger;

        public LocalServerTrigger(StageJson triggerStage)
        {
            TriggerStageJson = triggerStage;
            stageTrigger = triggerStage.StageTrigger;
            TotalTriggerCount = stageTrigger.TriggerCount;
            StageEnum = stageTrigger.StageEnum;
            TriggeredCount = 0;
        }

        public bool CheckCanTrigger(object triggerData)
        {
            if (!Active)
            {
                return false;
            }

            // 检查运行时 是否可以触发
            return FuncOnCheckCanTrigger.Invoke(this, triggerData);
        }

        public bool Trigger(object triggerData)
        {
            if (!Active)
            {
                return false;
            }
            TriggeredCount++;

            // 请求 触发器执行 触发逻辑
            ActionOnTriggerStageTrigger?.Invoke(this, triggerData);
            return true;
        }

    }
}
