///--------------------------------------------------------------------
/// 文件名   :   BattleDebugBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/03 09:58:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

namespace BattleDebug
{
    [System.Serializable]
    public class BattleDebugBehaviour : PlayableBehaviour
    {
        [LabelText("效果数据")]
        [ShowInInspector]
        public List<string> Args = new List<string>();



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

