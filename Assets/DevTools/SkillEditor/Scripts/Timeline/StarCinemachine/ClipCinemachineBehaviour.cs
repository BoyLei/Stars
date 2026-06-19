///--------------------------------------------------------------------
/// 文件名   :   ClipCinemachineBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 16:15:18
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using SkillEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor
{
    [System.Serializable]
    public class ClipCinemachineBehaviour : PlayableBehaviour
    {
        [Space]
        [LabelText("配置数据")]
        [ShowInInspector]
        public CameraCustomData CameraCustomData;

        [HideInInspector]
        public GameObject owner;


        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);

        }


        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
        }
    }
}

