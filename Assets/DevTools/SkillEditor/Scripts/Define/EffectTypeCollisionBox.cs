
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCollisionBox
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
    /// 碰撞盒
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class EffectTypeCollisionBox:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray CenterPosArray= new CenterPosArray();

        /// <summary>
        /// 朝向
        /// <summary>
        [LabelText("朝向")]
        [HideReferenceObjectPicker]
        public TowardArray TowardArray= new TowardArray();

        /// <summary>
        /// 形状
        /// <summary>
        [LabelText("形状")]
        [HideReferenceObjectPicker]
        public ShapeSerialize Shape= new ShapeSerialize();

        /// <summary>
        /// 目标类型
        /// <summary>
        [LabelText("目标类型")]
        [ValueDropdown("_collisiontarget")]
        public SelectType CollisionTarget=SelectType.Enemy;

        /// <summary>
        /// 最大目标数量
        /// <summary>
        [LabelText("最大目标数量")]
        public int MaxTar=10;

        /// <summary>
        /// 是否结算命中
        /// <summary>
        [LabelText("是否结算命中")]
        public bool IsSettleHit;

        /// <summary>
        /// 是否不计算目标半径
        /// <summary>
        [LabelText("是否不计算目标半径")]
        public bool IsNotCheckTargetRadius;

        /// <summary>
        /// 是否不重复命中
        /// <summary>
        [LabelText("是否不重复命中")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public bool IsNoDuplication;

        /// <summary>
        /// 是否可以选中不可选中
        /// <summary>
        [LabelText("是否可以选中不可选中")]
        public bool IngoreForbidSelect;

        /// <summary>
        /// 是否可以选中尸体
        /// <summary>
        [LabelText("是否可以选中尸体")]
        public bool IngoreForbidDead;

        /// <summary>
        /// 是否预警
        /// <summary>
        [LabelText("是否预警")]
        public bool IsForewarn;

        /// <summary>
        /// 是否跟随目标移动
        /// <summary>
        [LabelText("是否跟随目标移动")]
        public bool IsPosWithTarget;

        /// <summary>
        /// 是否跟随目标旋转
        /// <summary>
        [LabelText("是否跟随目标旋转")]
        public bool IsRotWithTarget;

        /// <summary>
        /// 预警时间
        /// <summary>
        [LabelText("预警时间")]
        public int ForewarnTime;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        /// <summary>
        /// 重复攻击Key
        /// <summary>
        [LabelText("重复攻击Key")]
        [HideInInspector]
        public OutputKey IsNoDuplicationKey= new OutputKey();

        public IEnumerable _collisiontarget()
        {
            return EnumDefineMap._selecttype;
        }

    }

}