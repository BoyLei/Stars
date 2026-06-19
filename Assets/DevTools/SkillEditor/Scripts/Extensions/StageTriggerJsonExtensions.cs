using MessagePack;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace SkillEditor
{
    /// <summary>
    /// ÆÕÍ¨½×¶Î²ÎÊý
    /// </summary>

    public partial class StageTrigger
    {
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            ConditionGroup = new List<TriggerCondition>();
            foreach (var item in ConditionGroupInEditor)
            {
                TriggerCondition triggerArg = new TriggerCondition();
                triggerArg.TriggerTypeEnum = item.TriggerTypeEnum;
                triggerArg.TriggerArg = item;
                ConditionGroup.Add(triggerArg);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class TriggerCondition
    {
        public TriggerTypeEnum TriggerTypeEnum;
        public TriggerTypeEnumSerialize TriggerArg;
    }
}