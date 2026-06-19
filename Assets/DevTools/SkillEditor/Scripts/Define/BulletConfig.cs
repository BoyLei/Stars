
///--------------------------------------------------------------------
/// 文件名   :   BulletConfig
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
    /// 子弹配置
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class BulletConfig:BaseConfig 
    {
        /// <summary>
        /// 子弹ID
        /// <summary>
        [LabelText("子弹ID")]
        [ReadOnly]
        public int ID;

        /// <summary>
        /// 子弹说明
        /// <summary>
        [LabelText("子弹说明")]
        public string BulletDesc;

        /// <summary>
        /// 子弹时间
        /// <summary>
        [LabelText("子弹时间")]
        public int Time;

        /// <summary>
        /// 子弹高度
        /// <summary>
        [LabelText("子弹高度")]
        public int Hight;

        /// <summary>
        /// 子弹高度跟随类型
        /// <summary>
        [LabelText("子弹高度跟随类型")]
        [ValueDropdown("_bulletheighttype")]
        public BulletHeightType BulletHeightType= new BulletHeightType();

        /// <summary>
        /// 模型ID
        /// <summary>
        [LabelText("模型ID")]
        public int AvatarID=0;

        /// <summary>
        /// 循环特效
        /// <summary>
        [LabelText("循环特效")]
        public List<EffectTypeHitEffect> LoopEffectInEditors= new List<EffectTypeHitEffect>();

        /// <summary>
        /// 表现标签
        /// <summary>
        [LabelText("表现标签")]
        public List<GlobalShowSerialize> GlobalShows= new List<GlobalShowSerialize>();

        /// <summary>
        /// 是否穿墙
        /// <summary>
        [LabelText("是否穿墙")]
        public bool IsThrough;

        /// <summary>
        /// 是否完全由客户端模拟
        /// <summary>
        [LabelText("是否完全由客户端模拟")]
        public bool IsClientMove;

        /// <summary>
        /// 移动速度
        /// <summary>
        [LabelText("移动速度")]
        public int MoveSpeed;

        /// <summary>
        /// Bullet拷贝数据
        /// <summary>
        [LabelText("Bullet拷贝数据")]
        public List<OutputKey> BulletCopyData= new List<OutputKey>();

        /// <summary>
        /// 子弹标签
        /// <summary>
        [LabelText("子弹标签")]
        [ValueDropdown("_bulletlabels")]
        public List<BulletLabel> BulletLabels= new List<BulletLabel>();

        public IEnumerable _bulletheighttype()
        {
            return EnumDefineMap._bulletheighttype;
        }

        public IEnumerable _bulletlabels()
        {
            return EnumDefineMap._bulletlabel;
        }

    }

}