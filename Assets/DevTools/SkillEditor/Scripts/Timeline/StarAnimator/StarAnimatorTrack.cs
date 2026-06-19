///--------------------------------------------------------------------
/// 文件名   :   StarAnimatorTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 09:41:37
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [Serializable]
    [TrackClipType(typeof(ClipStarAnimatorAsset), false)]
    [TrackBindingType(typeof(Animator))]
    [ExcludeFromPreset]
    [TrackColor(0.25f,0,0.89f)]
    [DisplayName("动画轨道")]
    public class StarAnimatorTrack : AnimationTrack
    {
        public bool isRight;
    }
}