using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SkillEditor
{
    public partial class EffectTypeDamage : BaseEffectType
    {
        public List<string> GetPaths()
        {
            return SkillEditorUtils.GetSounds();
        }   

        /// <summary>
        /// 序列化时调用
        /// </summary>
        [ContextMenu("GetEventID")]
        public void GetEventID()
        {
            var item = SkillEditorUtils.GetEventID(EventPath);
            EventName = item.eventName;
            EventID = item.eventID;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
#if UNITY_EDITOR
            GetEventID();
#endif
        }
    }
}
