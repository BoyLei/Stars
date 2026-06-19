///--------------------------------------------------------------------
/// 文件名   :   MotionJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/20 16:43:19
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    [System.Serializable]
    public class MotionJson
    {
        public int Start;
        public int End;
        public int Duration;
        public int Index;
        public StageBulletMotion config;
    }
}