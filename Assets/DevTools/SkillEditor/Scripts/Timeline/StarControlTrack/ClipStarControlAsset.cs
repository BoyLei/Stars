///--------------------------------------------------------------------
/// 文件名   :   ClipStarControlAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 14:48:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Timeline;
using Unity.Mathematics;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;

namespace SkillEditor
{

    /// <summary>
    /// Playable Asset that generates playables for controlling time-related elements on a GameObject.
    /// </summary>
    [Serializable]
    [NotKeyable]
    public class ClipStarControlAsset : ControlPlayableAsset
    {
        [HideLabel]
        public ClipStarControlBehaviour template = new ClipStarControlBehaviour();
    }
}
