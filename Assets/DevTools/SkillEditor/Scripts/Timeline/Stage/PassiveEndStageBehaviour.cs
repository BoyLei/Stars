///--------------------------------------------------------------------
/// 文件名   :   BuffEndBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 16:12:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor
{

    [System.Serializable]
    public class PassiveEndStageBehaviour : PlayableBehaviour
    {
        [ShowInInspector]
        [HideLabel]
        public StagePassiveEnd data;

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