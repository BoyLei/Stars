///--------------------------------------------------------------------
/// 文件名   :   CameraShakeJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/17 18:46:50
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SkillEditor
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class CameraShakeJson : CommonClipJson
    {
        [MessagePack.IgnoreMember]
        public AnimationCurve AmplitudeGain;
        [MessagePack.IgnoreMember]
        public AnimationCurve FrequencyGain;
        public string VirtualCameraName;
    }
}