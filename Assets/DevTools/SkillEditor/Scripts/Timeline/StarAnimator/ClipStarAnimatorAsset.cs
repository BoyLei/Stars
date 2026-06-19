///--------------------------------------------------------------------
/// 文件名   :   ClipStarAnimatorAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 09:42:03
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using SkillEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A Playable Asset that represents a single AnimationClip clip.
    /// </summary>
    public class ClipStarAnimatorAsset: AnimationPlayableAsset
    {
        [HideLabel]
        public ClipStarAnimatorBehaviour template = new ClipStarAnimatorBehaviour();
    }
}
