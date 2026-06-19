
///--------------------------------------------------------------------
/// 文件名   :   LineRendererConfig
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
    /// 连线
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class LineRendererConfig 
    {
        /// <summary>
        /// 发射者
        /// <summary>
        [LabelText("发射者")]
        [HideReferenceObjectPicker]
        public InputKey FromTarget= new InputKey();

        /// <summary>
        /// 发射者挂点
        /// <summary>
        [LabelText("发射者挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_fromtargethangpoint")]
        public HangPoint FromTargetHangPoint=HangPoint.Root;

        /// <summary>
        /// 接收者
        /// <summary>
        [LabelText("接收者")]
        [HideReferenceObjectPicker]
        public InputKey ToTarget= new InputKey();

        /// <summary>
        /// 接收者挂点
        /// <summary>
        [LabelText("接收者挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_totargethangpoint")]
        public HangPoint ToTargetHangPoint=HangPoint.Root;

        /// <summary>
        /// 连续/分发
        /// <summary>
        [LabelText("连续/分发")]
        [ValueDropdown("_linktype")]
        public LinkTypeEnum LinkType= new LinkTypeEnum();

        /// <summary>
        /// 连线预制体
        /// <summary>
        [LabelText("连线预制体")]
        [FilePath]
        public string LinePrefab;

        /// <summary>
        /// 随时间变化
        /// <summary>
        [LabelText("随时间变化")]
        public bool IsChangeWithTime;

        /// <summary>
        /// Lerp时间
        /// <summary>
        [LabelText("Lerp时间")]
        public int LerpTime;

        /// <summary>
        /// 结束时间
        /// <summary>
        [LabelText("结束时间")]
        public int EndTime;

        public IEnumerable _fromtargethangpoint()
        {
            return EnumDefineMap._hangpoint;
        }

        public IEnumerable _totargethangpoint()
        {
            return EnumDefineMap._hangpoint;
        }

        public IEnumerable _linktype()
        {
            return EnumDefineMap._linktypeenum;
        }

    }

}