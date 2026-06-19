///--------------------------------------------------------------------
/// 文件名   :   StarCinemachineTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:42:42
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [Serializable]
    [TrackClipType(typeof(StarCinemachineShot))]
    [TrackBindingType(typeof(CinemachineBrain), TrackBindingFlags.None)]
    [TrackColor(0.53f, 0.0f, 0.08f)]

    public class StarCinemachineTrack : CinemachineTrack
    {
        public bool isRight;
    }
}