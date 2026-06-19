using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Data
{
    /// <summary>
    /// 触发 效果 发送的 数据
    /// </summary>
    public class TriggerTypeEffectData
    {
        /// <summary>
        /// 触发效果的 事件Event , 类似于 关闭ui 等到 事件
        /// </summary>
        public string Event;

        /// <summary>
        /// 发送事件 对应的 值
        /// </summary>
        public object Value;

        public TriggerTypeEffectData(string eventName, object value)
        {
            Event = eventName;
            Value = value;
        }
    }
}
