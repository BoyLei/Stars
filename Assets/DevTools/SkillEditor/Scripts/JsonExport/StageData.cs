///--------------------------------------------------------------------
/// 文件名   :   StageData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 17:29:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SkillEditor;
using UnityEngine;

namespace SkillEditor
{
    [System.Serializable]
    public class StageData
    {
        [HideInInspector] public StageType StageType;
        
        [LabelText("阶段ID")] public int StageID;
        
        [LabelText("激活")]
        [ShowIf("ActiveLoop")]
        public bool Active;


        [LabelText("触发次数")]
        [ShowIf("ShowTriggerStage")]       
        public int TriggerCount=1;

        [LabelText("循环次数")]
        [ShowIf("ActiveLoop")]
        public int LoopCount=1;

        [LabelText("是否进入CD")]
        [ShowIf("ActiveLoop")]
        public bool IsEntryCD=false;

        [LabelText("原子状态")] 
        [ValueDropdown("_battlestate")]
        [ShowIf("ActiveLoop")]
        public List<BattleState> State;

        [LabelText("打断技能")] 
        [ValueDropdown("_interruptevent")]
        [ShowIf("ActiveLoop")]
        public InterruptEvent Interrupt;


        [LabelText("触发阶段")]
        [ShowIf("ShowTriggerStage")]
        [ValueDropdown("_triggerstageenum")]
        public TriggerStageEnum StageEnum;

        [LabelText("触发条件组")]
        [ShowIf("ShowTriggerStage")]
        public List<TriggerTypeEnumSerialize> ConditionGroup;

        [LabelText("属性")]
        [ShowIf("ShowProp")]
        [TableList(ShowIndexLabels =true,DrawScrollView =true,MaxScrollViewHeight =160)]
        public List<BattleProp> BattleProps;

        public IEnumerable _triggerstageenum()
        {
            return EnumDefineMap._triggerstageenum;
        }

        public bool ShouldSerializeStageEnum()
        {
            return ShowTriggerStage();
        }

        public bool ShouldSerializeConditionGroup()
        {
            return ShowTriggerStage();
        }

        private bool ShowProp()
        {
            return StageType == StageType.NormalStage;
        }

        private bool ShowTriggerStage()
        {
            return StageType == StageType.TriggerStage;
        }

        private bool ActiveLoop()
        {
            return StageType == StageType.NormalStage;
        }
        
        private IEnumerable _battlestate()
        {
            return EnumDefineMap._battlestate;
        }

        private IEnumerable _interruptevent()
        {
            return EnumDefineMap._interruptevent;
        }
    }
}
