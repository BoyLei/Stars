
///--------------------------------------------------------------------
/// 文件名   :   EffectData
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
    /// 效果数据
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class EffectData 
    {
        /// <summary>
        /// 效果ID
        /// <summary>
        [LabelText("效果ID")]
        [FoldoutGroup("$TileName")]
        public int EffectID;

        /// <summary>
        /// 效果描述
        /// <summary>
        [LabelText("效果描述")]
        [FoldoutGroup("$TileName")]
        public string Desc=string.Empty;

        /// <summary>
        /// 效果参数
        /// <summary>
        [LabelText("效果参数")]
        [FoldoutGroup("$TileName")] [JsonIgnore][MessagePack.IgnoreMember]
        public EffectTypeSerialize EffectArgs= new EffectTypeSerialize();

        /// <summary>
        /// 后续效果
        /// <summary>
        [LabelText("后续效果")]
        [FoldoutGroup("$TileName")]
        public int[] Next;

        /// <summary>
        /// 服务器提前时间(ms)
        /// <summary>
        [LabelText("服务器提前时间(ms)")]
        [FoldoutGroup("$TileName")] 
        public int ServerPreTime;

        /// <summary>
        /// 客户端延迟时间(ms)
        /// <summary>
        [LabelText("客户端延迟时间(ms)")]
        [FoldoutGroup("$TileName")]
        public int ClientDelayTime;

        /// <summary>
        /// 客户端效果持续时间
        /// <summary>
        [LabelText("客户端效果持续时间")]
        [FoldoutGroup("$TileName")]
        public int ServerNextDelayTime;

        /// <summary>
        /// 效果是否在技能中运行
        /// <summary>
        [LabelText("效果是否在技能中运行")]
        [FoldoutGroup("$TileName")]
        public bool SaveSkill;

        /// <summary>
        /// 效果是否跟随阶段结束
        /// <summary>
        [LabelText("效果是否跟随阶段结束")]
        [FoldoutGroup("$TileName")]
        public bool IsStageCancel;

        /// <summary>
        /// 效果标签
        /// <summary>
        [LabelText("效果标签")]
        [FoldoutGroup("$TileName")]
        [ValueDropdown("_effectlabels")]
        public EffectLabel[] EffectLabels;

        /// <summary>
        /// 效果类型
        /// <summary>
        [LabelText("效果类型")]
        [HideInInspector]
        [ValueDropdown("_effecttype")]
        public EffectType EffectType= new EffectType();

        /// <summary>
        /// 效果参数
        /// <summary>
        [LabelText("效果参数")]
        [HideReferenceObjectPicker][HideInInspector][SerializeReference]
        public BaseEffectType BaseEffect;

        /// <summary>
        /// 输入Key集合
        /// <summary>
        [LabelText("输入Key集合")]
        [HideInInspector]
        public List<string> InputKeys= new List<string>();

        /// <summary>
        /// 输出Key集合
        /// <summary>
        [LabelText("输出Key集合")]
        [HideInInspector]
        public List<string> OutputKeys= new List<string>();

        /// <summary>
        /// 服务器执行时间(ms)   客户端执行时间-服务器提前时间
        /// <summary>
        [LabelText("服务器执行时间(ms)   客户端执行时间-服务器提前时间")]
        [FoldoutGroup("$TileName")]  [HideInInspector]
        public int ServerExecuteTime;

        /// <summary>
        /// 客户端执行时间   Start+客户端延迟时间
        /// <summary>
        [LabelText("客户端执行时间   Start+客户端延迟时间")]
        [FoldoutGroup("$TileName")]  [HideInInspector]
        public int ClinetExecuteTime;

        /// <summary>
        /// 效果结束时间   客户端执行时间+延迟下一个时间
        /// <summary>
        [LabelText("效果结束时间   客户端执行时间+延迟下一个时间")]
        [FoldoutGroup("$TileName")]  [HideInInspector]
        public int EffectEndTime;

        public IEnumerable _effectlabels()
        {
            return EnumDefineMap._effectlabel;
        }

        public IEnumerable _effecttype()
        {
            return EnumDefineMap._effecttype;
        }

    }

}