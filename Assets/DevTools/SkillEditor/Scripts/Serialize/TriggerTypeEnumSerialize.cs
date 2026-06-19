
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumSerialize
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
    /// 触发器序列化
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumSerialize 
    {
        /// <summary>
        /// 触发条件类型
        /// <summary>
        [LabelText("触发条件类型")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        [ValueDropdown("_triggertypeenum")]
        public TriggerTypeEnum TriggerTypeEnum= new TriggerTypeEnum();

        /// <summary>
        /// 临时伤害数值
        /// <summary>
        [LabelText("临时伤害数值")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataHurt")]
        public TriggerTypeEnumDataHurt DataHurt= new TriggerTypeEnumDataHurt();

        /// <summary>
        /// 临时治疗数值
        /// <summary>
        [LabelText("临时治疗数值")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataCure")]
        public TriggerTypeEnumDataCure DataCure= new TriggerTypeEnumDataCure();

        /// <summary>
        /// 临时命中结果
        /// <summary>
        [LabelText("临时命中结果")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataHit")]
        public TriggerTypeEnumDataHit DataHit= new TriggerTypeEnumDataHit();

        /// <summary>
        /// 临时随机数值
        /// <summary>
        [LabelText("临时随机数值")]
        [SerializeField]
        [ShowIf("ShouldSerializeRandom")]
        public TriggerTypeEnumRandom Random= new TriggerTypeEnumRandom();

        /// <summary>
        /// 临时闪避结果
        /// <summary>
        [LabelText("临时闪避结果")]
        [SerializeField]
        [ShowIf("ShouldSerializeDodgeAttack")]
        public TriggerTypeEnumDodgeAttack DodgeAttack= new TriggerTypeEnumDodgeAttack();

        /// <summary>
        /// 临时暴击结果
        /// <summary>
        [LabelText("临时暴击结果")]
        [SerializeField]
        [ShowIf("ShouldSerializeCriticalHit")]
        public TriggerTypeEnumCriticalHit CriticalHit= new TriggerTypeEnumCriticalHit();

        /// <summary>
        /// 临时数据是buff运行时，判断id
        /// <summary>
        [LabelText("临时数据是buff运行时，判断id")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataBuffID")]
        public TriggerTypeEnumDataBuffID DataBuffID= new TriggerTypeEnumDataBuffID();

        /// <summary>
        /// 临时数据是buff运行时，判断tag
        /// <summary>
        [LabelText("临时数据是buff运行时，判断tag")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataBuffTag")]
        public TriggerTypeEnumDataBuffTag DataBuffTag= new TriggerTypeEnumDataBuffTag();

        /// <summary>
        /// 临时数据是buff运行时，判断层数
        /// <summary>
        [LabelText("临时数据是buff运行时，判断层数")]
        [SerializeField]
        [ShowIf("ShouldSerializeDataBuffCount")]
        public TriggerTypeEnumDataBuffCount DataBuffCount= new TriggerTypeEnumDataBuffCount();

        /// <summary>
        /// 对象当前生命值百分比
        /// <summary>
        [LabelText("对象当前生命值百分比")]
        [SerializeField]
        [ShowIf("ShouldSerializeRemainBlood")]
        public TriggerTypeEnumRemainBlood RemainBlood= new TriggerTypeEnumRemainBlood();

        /// <summary>
        /// 对象当前蓝量百分比
        /// <summary>
        [LabelText("对象当前蓝量百分比")]
        [SerializeField]
        [ShowIf("ShouldSerializeRemainManaPer")]
        public TriggerTypeEnumRemainManaPer RemainManaPer= new TriggerTypeEnumRemainManaPer();

        /// <summary>
        /// 对象当前蓝量绝对值
        /// <summary>
        [LabelText("对象当前蓝量绝对值")]
        [SerializeField]
        [ShowIf("ShouldSerializeRemainMana")]
        public TriggerTypeEnumRemainMana RemainMana= new TriggerTypeEnumRemainMana();

        /// <summary>
        /// 对象对应属性id的属性值
        /// <summary>
        [LabelText("对象对应属性id的属性值")]
        [SerializeField]
        [ShowIf("ShouldSerializePropValue")]
        public TriggerTypeEnumPropValue PropValue= new TriggerTypeEnumPropValue();

        /// <summary>
        /// 对象的怪物类型
        /// <summary>
        [LabelText("对象的怪物类型")]
        [SerializeField]
        [ShowIf("ShouldSerializeCheckTargetMonsterType")]
        public TriggerTypeEnumPCheckTargetMonsterType CheckTargetMonsterType= new TriggerTypeEnumPCheckTargetMonsterType();

        /// <summary>
        /// 伤害节点-命中集合人数
        /// <summary>
        [LabelText("伤害节点-命中集合人数")]
        [SerializeField]
        [ShowIf("ShouldSerializeBlackHit")]
        public TriggerTypeEnumBlackHit BlackHit= new TriggerTypeEnumBlackHit();

        /// <summary>
        /// 伤害节点-判断伤害类型
        /// <summary>
        [LabelText("伤害节点-判断伤害类型")]
        [SerializeField]
        [ShowIf("ShouldSerializeDamageType")]
        public TriggerTypeEnumDamageType DamageType= new TriggerTypeEnumDamageType();

        /// <summary>
        /// 当前效果id
        /// <summary>
        [LabelText("当前效果id")]
        [SerializeField]
        [ShowIf("ShouldSerializeCurrEffectID")]
        public TriggerTypeEnumCurrEffectID CurrEffectID= new TriggerTypeEnumCurrEffectID();

        /// <summary>
        /// 当前效果类型
        /// <summary>
        [LabelText("当前效果类型")]
        [SerializeField]
        [ShowIf("ShouldSerializeCurrEffectType")]
        public TriggerTypeEnumCurrEffectType CurrEffectType= new TriggerTypeEnumCurrEffectType();

        /// <summary>
        /// 当前效果标签
        /// <summary>
        [LabelText("当前效果标签")]
        [SerializeField]
        [ShowIf("ShouldSerializeCurrEffectTag")]
        public TriggerTypeEnumCurrEffectTag CurrEffectTag= new TriggerTypeEnumCurrEffectTag();

        /// <summary>
        /// 是否触发背刺
        /// <summary>
        [LabelText("是否触发背刺")]
        [SerializeField]
        [ShowIf("ShouldSerializeBackstab")]
        public TriggerTypeEnumBackstab Backstab= new TriggerTypeEnumBackstab();

        /// <summary>
        /// 检测指定运行时黑板上的数据
        /// <summary>
        [LabelText("检测指定运行时黑板上的数据")]
        [SerializeField]
        [ShowIf("ShouldSerializeIntNode")]
        public TriggerTypeEnumIntNode IntNode= new TriggerTypeEnumIntNode();

        /// <summary>
        /// 当前技能标签
        /// <summary>
        [LabelText("当前技能标签")]
        [SerializeField]
        [ShowIf("ShouldSerializeSkillTag")]
        public TriggerTypeEnumSkill SkillTag= new TriggerTypeEnumSkill();

        /// <summary>
        /// 调用者与运行时对象关系
        /// <summary>
        [LabelText("调用者与运行时对象关系")]
        [SerializeField]
        [ShowIf("ShouldSerializeRuntimeRelationship")]
        public TriggerTypeEnumRuntimeRelationship RuntimeRelationship= new TriggerTypeEnumRuntimeRelationship();

        /// <summary>
        /// 对象的buffid
        /// <summary>
        [LabelText("对象的buffid")]
        [SerializeField]
        [ShowIf("ShouldSerializeBuffID")]
        public TriggerTypeEnumBuffID BuffID= new TriggerTypeEnumBuffID();

        /// <summary>
        /// 对象的bufftag
        /// <summary>
        [LabelText("对象的bufftag")]
        [SerializeField]
        [ShowIf("ShouldSerializeBuffTag")]
        public TriggerTypeEnumBuffTag BuffTag= new TriggerTypeEnumBuffTag();

        /// <summary>
        /// 对象的指定buff层数
        /// <summary>
        [LabelText("对象的指定buff层数")]
        [SerializeField]
        [ShowIf("ShouldSerializeBuffFloor")]
        public TriggerTypeEnumBuffFloor BuffFloor= new TriggerTypeEnumBuffFloor();

        /// <summary>
        /// 对象的技能冷却
        /// <summary>
        [LabelText("对象的技能冷却")]
        [SerializeField]
        [ShowIf("ShouldSerializeCoolTime")]
        public TriggerTypeEnumCoolTime CoolTime= new TriggerTypeEnumCoolTime();

        /// <summary>
        /// 对象的原子状态
        /// <summary>
        [LabelText("对象的原子状态")]
        [SerializeField]
        [ShowIf("ShouldSerializeState")]
        public TriggerTypeEnumState State= new TriggerTypeEnumState();

        /// <summary>
        /// 对象的阶段ID
        /// <summary>
        [LabelText("对象的阶段ID")]
        [SerializeField]
        [ShowIf("ShouldSerializeStage")]
        public TriggerTypeEnumStage Stage= new TriggerTypeEnumStage();

        /// <summary>
        /// Onwer的战斗状态
        /// <summary>
        [LabelText("Onwer的战斗状态")]
        [SerializeField]
        [ShowIf("ShouldSerializeBattle")]
        public TriggerTypeEnumBattle Battle= new TriggerTypeEnumBattle();

        /// <summary>
        /// 是否能获得利箭点
        /// <summary>
        [LabelText("是否能获得利箭点")]
        [SerializeField]
        [ShowIf("ShouldSerializeCanGetSAPoint")]
        public TriggerTypeEnumCanGetSAPoint CanGetSAPoint= new TriggerTypeEnumCanGetSAPoint();

        public IEnumerable _triggertypeenum()
        {
            return EnumDefineMap._triggertypeenum;
        }

        public bool ShouldSerializeDataHurt()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataHurt;
        }

        public bool ShouldSerializeDataCure()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataCure;
        }

        public bool ShouldSerializeDataHit()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataHit;
        }

        public bool ShouldSerializeRandom()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.Random;
        }

        public bool ShouldSerializeDodgeAttack()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DodgeAttack;
        }

        public bool ShouldSerializeCriticalHit()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CriticalHit;
        }

        public bool ShouldSerializeDataBuffID()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataBuffID;
        }

        public bool ShouldSerializeDataBuffTag()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataBuffTag;
        }

        public bool ShouldSerializeDataBuffCount()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DataBuffCount;
        }

        public bool ShouldSerializeRemainBlood()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.RemainBlood;
        }

        public bool ShouldSerializeRemainManaPer()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.RemainManaPer;
        }

        public bool ShouldSerializeRemainMana()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.RemainMana;
        }

        public bool ShouldSerializePropValue()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.PropValue;
        }

        public bool ShouldSerializeCheckTargetMonsterType()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CheckTargetMonsterType;
        }

        public bool ShouldSerializeBlackHit()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.BlackHit;
        }

        public bool ShouldSerializeDamageType()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.DamageType;
        }

        public bool ShouldSerializeCurrEffectID()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CurrEffectID;
        }

        public bool ShouldSerializeCurrEffectType()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CurrEffectType;
        }

        public bool ShouldSerializeCurrEffectTag()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CurrEffectTag;
        }

        public bool ShouldSerializeBackstab()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.Backstab;
        }

        public bool ShouldSerializeIntNode()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.IntNode;
        }

        public bool ShouldSerializeSkillTag()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.SkillTag;
        }

        public bool ShouldSerializeRuntimeRelationship()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.RuntimeRelationship;
        }

        public bool ShouldSerializeBuffID()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.BuffID;
        }

        public bool ShouldSerializeBuffTag()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.BuffTag;
        }

        public bool ShouldSerializeBuffFloor()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.BuffFloor;
        }

        public bool ShouldSerializeCoolTime()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CoolTime;
        }

        public bool ShouldSerializeState()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.State;
        }

        public bool ShouldSerializeStage()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.Stage;
        }

        public bool ShouldSerializeBattle()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.Battle;
        }

        public bool ShouldSerializeCanGetSAPoint()
        {
            return this.TriggerTypeEnum == TriggerTypeEnum.CanGetSAPoint;
        }

    }

}