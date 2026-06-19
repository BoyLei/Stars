
///--------------------------------------------------------------------
/// 文件名   :   ShapeHollowCircle
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
    /// 空心圆
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ShapeHollowCircle:BaseShap 
    {
        /// <summary>
        /// 最小半径
        /// <summary>
        [LabelText("最小半径")]
        public int MinRadius;

        /// <summary>
        /// 最大半径
        /// <summary>
        [LabelText("最大半径")]
        public int MaxRadius;

    }

}