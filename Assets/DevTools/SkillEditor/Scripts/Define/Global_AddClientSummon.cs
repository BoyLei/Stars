
///--------------------------------------------------------------------
/// 文件名   :   Global_AddClientSummon
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
    /// 召唤客户端召唤物
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class Global_AddClientSummon:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 化身ID
        /// <summary>
        [LabelText("化身ID")]
        public int AvatarID;

        /// <summary>
        /// 循环特效
        /// <summary>
        [LabelText("循环特效")]
        public List<EffectTypeHitEffect> LoopEffectInEditors= new List<EffectTypeHitEffect>();

        /// <summary>
        /// 起始角度
        /// <summary>
        [LabelText("起始角度")]
        public int MinRot;

        /// <summary>
        /// 结束角度
        /// <summary>
        [LabelText("结束角度")]
        public int MaxRot;

        /// <summary>
        /// 数量
        /// <summary>
        [LabelText("数量")]
        public int Num;

        /// <summary>
        /// 半径
        /// <summary>
        [LabelText("半径")]
        public int Distance;

        /// <summary>
        /// 缓慢旋转角度
        /// <summary>
        [LabelText("缓慢旋转角度")]
        public int IsTurn;

        /// <summary>
        /// 是否跟随玩家旋转
        /// <summary>
        [LabelText("是否跟随玩家旋转")]
        public bool IsFollowPlayerTurn;

    }

}