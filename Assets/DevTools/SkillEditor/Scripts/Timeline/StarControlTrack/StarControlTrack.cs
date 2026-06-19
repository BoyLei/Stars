///--------------------------------------------------------------------
/// 文件名   :   StarControlTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 14:46:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("特效效果轨道")]
    [TrackClipType(typeof(ClipStarControlAsset))]
    public class StarControlTrack : ControlTrack
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;
        public bool isRight;

        public PropertyName id { get; }

  
    }
}
