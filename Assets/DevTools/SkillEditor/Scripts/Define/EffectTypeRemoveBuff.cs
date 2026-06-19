
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeRemoveBuff
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MessagePack;
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 移除BUFF
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeRemoveBuff:BaseEffectType 
    {
        /// <summary>
        /// 移除对象Key
        /// <summary>
        [LabelText("移除对象Key")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// BUFFID
        /// <summary>
        [LabelText("BUFFID")]
        public int[] BuffIDs;

        /// <summary>
        /// BUFF标签
        /// <summary>
        [LabelText("BUFF标签")]
        public int[] BuffTags;

        /// <summary>
        /// 控制BUFF类型
        /// <summary>
        [LabelText("控制BUFF类型")]
        [ValueDropdown("_buffconfronts")]
        public BuffConfront[] BuffConfronts;

        /// <summary>
        /// 总是成功
        /// <summary>
        [LabelText("总是成功")]
        public bool AlwaysSuccess=true;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _buffconfronts()
        {
            return EnumDefineMap._buffconfront;
        }

    }

}