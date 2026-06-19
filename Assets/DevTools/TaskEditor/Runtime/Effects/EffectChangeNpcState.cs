///--------------------------------------------------------------------
/// 文件名   :   EffectChangeNpcState.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/24 16:44:10
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using ClientNpc;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class EffectChangeNpcState : BaseEffect
    {

        private IEnumerable _states = new ValueDropdownList<StateEnum>()
        {
            { "待机", StateEnum.Idle },
             { "交互", StateEnum.Inter },
            { "巡逻", StateEnum.Patrol },
        };


        [LabelText("Npc唯一ID")]
        public int NpcID;

        [LabelText("状态")]
        [SuffixLabel("交互状态都是被动进入，不可切换")]
        [ValueDropdown("_states")]
        public StateEnum State;


        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = NpcID.ToString();
            effectJson.Args2 = ((int)State).ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            NpcID = ToInt(effectJson.Args1);
            State = (StateEnum)ToInt(effectJson.Args2);
        }
    }
}
