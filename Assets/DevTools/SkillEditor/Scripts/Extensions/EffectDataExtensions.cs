using Newtonsoft.Json;
using Sirenix.OdinInspector;
///--------------------------------------------------------------------
/// 文件名   :   EffectDataExtensions.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/07 18:22:23
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 效果数据
    /// </summary>
    [JsonConverter(typeof(EffectDataConverter))]
    public partial class EffectData
    {
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            InputKeys = EffectArgs.GetInputKey();
            OutputKeys = EffectArgs.GetOutputKey();

            EffectType = EffectArgs.EffectType;
            BaseEffect = EffectArgs.BaseEffect;
        }

        /// <summary>
        /// 标签
        /// <summary>
        [JsonIgnore]
        [MessagePack.IgnoreMember]
        public string TileName
        {
            get
            {
#if UNITY_EDITOR
                try
                {
                    return SkillEditorData.GetEffectTypeDic().ContainsKey(EffectArgs.EffectType) ? $"{EffectID}:{Desc}{SkillEditorData.GetEffectTypeDic()[EffectArgs.EffectType]}" : $"{EffectID}:{Desc}";
                }
                catch
                {
                    return string.Empty;
                }
#else
                return "";
#endif
            }
        }
    }
}
