///--------------------------------------------------------------------
/// 文件名   :   CameraJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 14:45:52
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
    public class CameraJson:CommonClipJson
    {
        public string ClipName;
        public string ClipPath;
        public CameraCustomData CameraCustomData;
    }
}