///--------------------------------------------------------------------
/// 文件名   :   ClipInputEffectBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/09 15:01:30
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace SkillEditor
{
    [System.Serializable]
    public class ClipInputEffectBehaviour : PlayableBehaviour
    {
        [LabelText("输入数据")]
        [OnValueChanged("OnInputEffectChange",true)]
        public EffectData inputEffect=new EffectData();

        private void OnInputEffectChange()
        {
            if(inputEffect.EffectArgs.EffectType!=EffectType.UserInput && inputEffect.EffectArgs.EffectType != EffectType.ChargeInput)
            {
                inputEffect.EffectArgs.EffectType = EffectType.UserInput;
            }
        }

        [LabelText("效果数据")]
        [ShowInInspector]
        public List<EffectData> frameEffect;

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