///--------------------------------------------------------------------
/// 文件名   :   EffectDataExtensions.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/07 18:22:23
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 效果数据
    /// </summary>
    public partial class BuffConfig:BaseConfig
    {
        public IEnumerable _battlestate()
        {
            return EnumDefineMap._battlestate;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (AddEffectFlys.Count > 0)
            {
                int i = 1;
                foreach (var fly in AddEffectFlys)
                {
                    // 基础信息
                    int id = ID; // 技能ID
                    fly.Value_Key = $"AddEffectFly_{id}_{i}";
                    i++;
                }
            }
        }
    }
}
