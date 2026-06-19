
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeMoveWithPos
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MessagePack;
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 根据坐标位移
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeMoveWithPos:BaseEffectType 
    {
        /// <summary>
        /// 位移者Key
        /// <summary>
        [LabelText("位移者Key")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// 位移终点
        /// <summary>
        [LabelText("位移终点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos SingleCenterPos= new SingleCenterPos();

        /// <summary>
        /// 根据自身或者根据坐标
        /// <summary>
        [LabelText("根据自身或者根据坐标")]
        [ValueDropdown("_builderorpos")]
        public BuilderOrPos BuilderOrPos= new BuilderOrPos();

        /// <summary>
        /// 位移标签
        /// <summary>
        [LabelText("位移标签")]
        [ValueDropdown("_movelabel")]
        public MoveLabel MoveLabel=MoveLabel.Normal;

        /// <summary>
        /// 位移类型
        /// <summary>
        [LabelText("位移类型")]
        [ValueDropdown("_movetype")]
        public MoveType MoveType=MoveType.Rush;

        /// <summary>
        /// 位移期间原子状态
        /// <summary>
        [LabelText("位移期间原子状态")]
        [ValueDropdown("_lockstate")]
        public BattleState[] LockState;

        /// <summary>
        /// 持续时间
        /// <summary>
        [LabelText("持续时间")]
        public int DurningTime;

        /// <summary>
        /// 距离
        /// <summary>
        [LabelText("距离")]
        public int Distance;

        /// <summary>
        /// 是否忽略目标模型
        /// <summary>
        [LabelText("是否忽略目标模型")]
        [ValueDropdown("_isignoretarget")]
        public IsIgnoreTarget IsIgnoreTarget=IsIgnoreTarget.CheckTargetWithStart;

        /// <summary>
        /// 移动最大距离
        /// <summary>
        [LabelText("移动最大距离")]
        public int MaxMove=-1;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _builderorpos()
        {
            return EnumDefineMap._builderorpos;
        }

        public IEnumerable _movelabel()
        {
            return EnumDefineMap._movelabel;
        }

        public IEnumerable _movetype()
        {
            return EnumDefineMap._movetype;
        }

        public IEnumerable _lockstate()
        {
            return EnumDefineMap._battlestate;
        }

        public IEnumerable _isignoretarget()
        {
            return EnumDefineMap._isignoretarget;
        }

    }

}