
///--------------------------------------------------------------------
/// 文件名   :   ShapeSerialize
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
    /// 形状序列化
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ShapeSerialize 
    {
        /// <summary>
        /// 形状类型
        /// <summary>
        [LabelText("形状类型")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_shapetype")]
        public Shape ShapeType= new Shape();

        /// <summary>
        /// 圆形参数
        /// <summary>
        [LabelText("圆形参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeRound")]
        public ShapeRound Round= new ShapeRound();

        /// <summary>
        /// 空心圆参数
        /// <summary>
        [LabelText("空心圆参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeHollowCircle")]
        public ShapeHollowCircle HollowCircle= new ShapeHollowCircle();

        /// <summary>
        /// 扇形参数
        /// <summary>
        [LabelText("扇形参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeSector")]
        public ShapeSector Sector= new ShapeSector();

        /// <summary>
        /// 环扇形参数
        /// <summary>
        [LabelText("环扇形参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeRingFan")]
        public ShapeRingFan RingFan= new ShapeRingFan();

        /// <summary>
        /// 箭头参数
        /// <summary>
        [LabelText("箭头参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeArrow")]
        public ShapeArrow Arrow= new ShapeArrow();

        /// <summary>
        /// 矩形参数
        /// <summary>
        [LabelText("矩形参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeRect")]
        public ShapeRect Rect= new ShapeRect();

        /// <summary>
        /// 朝向路径参数
        /// <summary>
        [LabelText("朝向路径参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeRotRoute")]
        public ShapeRotRoute RotRoute= new ShapeRotRoute();

        /// <summary>
        /// 点位路径参数
        /// <summary>
        [LabelText("点位路径参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializePosRoute")]
        public ShapePosRoute PosRoute= new ShapePosRoute();

        public IEnumerable _shapetype()
        {
            return EnumDefineMap._shape;
        }

        public bool ShouldSerializeRound()
        {
            return this.ShapeType == Shape.Round;
        }

        public bool ShouldSerializeHollowCircle()
        {
            return this.ShapeType == Shape.HollowCircle;
        }

        public bool ShouldSerializeSector()
        {
            return this.ShapeType == Shape.Sector;
        }

        public bool ShouldSerializeRingFan()
        {
            return this.ShapeType == Shape.RingFan;
        }

        public bool ShouldSerializeArrow()
        {
            return this.ShapeType == Shape.Arrow;
        }

        public bool ShouldSerializeRect()
        {
            return this.ShapeType == Shape.Rect;
        }

        public bool ShouldSerializeRotRoute()
        {
            return this.ShapeType == Shape.RotRoute;
        }

        public bool ShouldSerializePosRoute()
        {
            return this.ShapeType == Shape.PosRoute;
        }

    }

}