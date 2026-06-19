using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillEditor;
using StarProjectDef;
using EffectData = SkillEditor.EffectData;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 效果参数接口,此处单独做一个 effectData数据的 封装,
    /// 一个是为了 接口通用;
    /// 更重要的是为了 对技能编译器生成的 原装 effectData做一个隔离
    /// 防止因为策划 配置改变导致 effectData关联 的改动过大
    /// </summary>
    public interface I_EffectParam
    {
        EffectData EffectData { get; }

        int EffectID { get; }

        string EffectIDStr { get; }
        /// <summary>
        /// 效果的 唯一key , 一般可以使用 runtimeID_stageIDStr_effectID 组成唯一的key
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// 客户端自己定义的效果类型
        /// </summary>
        E_SkillEffect SkillEffectType { get; }
        /// <summary>
        /// 效果基类
        /// </summary>
        BaseEffectType BaseEffect { get; }
        /// <summary>
        /// 后续效果
        /// note: 
        ///     0 : true
        ///     1 : false
        /// </summary>
        List<int> Next { get; }
        /// <summary>
        /// 客户端延迟时间(ms)
        /// note:
        ///     next的效果 执行时间采用的 delayTime
        /// </summary>
        int ClientDelayTime { get; }

        /// <summary>
        /// 客户端执行时间(ms)   Start+客户端延迟时间
        /// note:
        ///     这个时间是客户端在 时间轴上 真正执行的时间
        /// </summary>
        int ClientExecuteTime { get; }

        /// <summary>
        // 新约定, 夏哥说 为了服务器效果能够提前计算, 所以 通过 ServerExecuteTime 和 ClinetExecuteTime 来区分效果的开始时间,
        // ServerNextDelayTime 约定为 效果的 执行时长.
        // 所以 客户端 的下个效果执行时间 是 基于当前时间 + ServerNextDelayTime
        /// </summary>
        int ServerNextDelayTime { get; }

        int EffectEndTime { get; }

        /// <summary>
        /// 这个效果 执行多久
        /// </summary>
        int EffectRuntTime { get; }

        /// <summary>
        /// 是否保存在Skill阶段里
        /// </summary>
        bool SaveSkill { get; }

        /// <summary>
        /// 效果输出的结果
        /// </summary>
        string OutputKey { get; }

        List<string> InputKeys { get; }

        /// <summary>
        ///  效果的builder.
        ///  如果技能效果,就是技能的builder
        /// </summary>
        ulong Builder { get; }
        /// <summary>
        /// 效果的 Owner.
        /// 如果技能效果,就是技能的 Owner
        /// </summary>
        ulong Owner { get; }

        object ExtraData { get; }

        bool IsPrepare { get; }

        E_StageType StageType { get; }

        /// <summary>
        /// 是否可以被标记为 无效的效果. 
        /// 目前主要 是 普攻的 输入效果. 在拖动摇杆的时候, 如果策划配置了
        /// 摇杆 会使预输入 失效， 那 就标记 这个预输入 无效
        /// </summary>
        bool CanMarkInvalid { get; }

        void Reset();

        /// <summary>
        /// 非必要不要这么搞,除非你非要存下来
        /// </summary>
        /// <returns>EffectParam</returns>
        EffectParam Clone();
    }

    public class EffectParam : I_EffectParam
    {
        private EffectData data;
        public EffectData EffectData => data;

        private string uniqueKey;

        public string UniqueKey => $"{uniqueKey}_{effectIDStr}";

        private int effectID;
        public int EffectID => effectID;

        private string effectIDStr = "";

        public string EffectIDStr => effectIDStr;

        private EffectType effectType;


        private E_SkillEffect skillEffectType;

        public E_SkillEffect SkillEffectType => skillEffectType;

        BaseEffectType baseEffect;
        public BaseEffectType BaseEffect => baseEffect;

        private List<int> next = new List<int>();
        public List<int> Next => next;

        private int clientDelayTime;
        public int ClientDelayTime => clientDelayTime;

        private int clientExecuteTime;
        public int ClientExecuteTime => clientExecuteTime;


        private int serverNextDelayTime;
        public int ServerNextDelayTime => serverNextDelayTime;

        public int EffectRuntTime => (serverNextDelayTime);


        public int EffectEndTime { get => EffectData.EffectEndTime; }

        private bool saveSkill;
        public bool SaveSkill => saveSkill;

        private string outputKey;
        public string OutputKey => outputKey;

        private List<string> inputKeys;

        public List<string> InputKeys => inputKeys;

        private ulong builder;
        public ulong Builder => builder;
        private ulong owner;
        public ulong Owner => owner;

        private object extraData;
        public object ExtraData => extraData;

        private bool isPrepare = false;
        public bool IsPrepare => isPrepare;


        private E_StageType stageType = E_StageType.None;

        public E_StageType StageType => stageType;

        private bool canMarkInvalid = false;

        public bool CanMarkInvalid => canMarkInvalid;

        public void Reset()
        {
            data = null;
            effectID = 0;
            effectIDStr = "";
            effectType = EffectType.Empty;
            skillEffectType = E_SkillEffect.Empty;
            next.Clear();
            clientDelayTime = 0;
            clientExecuteTime = 0;
            serverNextDelayTime = 0;
            saveSkill = false;
            builder = 0;
            owner = 0;
            outputKey = "";
            extraData = null;
            stageType = E_StageType.None;
            isPrepare = false;
        }

        public EffectParam Clone()
        {
            EffectParam clone = new EffectParam();
            clone.Init(data, builder, owner, stageType, uniqueKey);
            clone.SetExtraData(extraData);

            return clone;
        }

        public void Init(EffectData effectData, ulong effectBuilder, ulong effectOwner, E_StageType e_stageType, string uniKey)
        {
            Reset();
            data = effectData;
            effectID = effectData.EffectID;
            effectIDStr = effectID.ToString();

            effectType = effectData.EffectType;
            baseEffect = effectData.BaseEffect;
            next.Clear();
            if (effectData.Next != null)
            {
                next = effectData.Next.KToList<int>();
            }

            clientDelayTime = effectData.ClientDelayTime;
            clientExecuteTime = effectData.ClinetExecuteTime;
            serverNextDelayTime = effectData.ServerNextDelayTime;

            canMarkInvalid = false;
            if (effectData.EffectType == EffectType.UserInput)
            {
                canMarkInvalid = ((EffectTypeUserInput)data.BaseEffect).CanMarkInvalid;
            }
            // else if (effectData.EffectType == EffectType.ChargeInput)
            // {
            //     canMarkInvalid = ((EffectTypeChargeInput)data.BaseEffect).CanMarkInvalid;
            // }



            saveSkill = effectData.SaveSkill;
            builder = effectBuilder;
            owner = effectOwner;
            uniqueKey = uniKey;

            stageType = e_stageType;

            inputKeys = effectData.InputKeys;
            initData();
        }
        public void SetExtraData(object data)
        {
            extraData = data;
        }
        private void initData()
        {
            outputKey = GetEffectDataOutPutKey(data);
            // SkillEditorDefine.EffectType 由配置生成,策划可能频繁改名字,而配置生成的枚举引用不会跟随改变, 所以 使用 E_SkillEffect 做一层隔离
            skillEffectType = GetEffectDataESkillEffect(data);

            // 如果是 输入轴效果, 就需要特殊判定是否是 预输入效果. 
            if (skillEffectType == E_SkillEffect.UserInput)
            {
                isPrepare = (baseEffect as EffectTypeUserInput).InputType == InputType.Prepare;
            }
            else
            {
                isPrepare = false;
            }
        }

        /// <summary>
        /// 效果的 outPutKey 跟 gl 约定只用第一个
        /// </summary>
        /// <param name="effectData"></param>
        /// <returns></returns>
        public static string GetEffectDataOutPutKey(EffectData effectData)
        {
            return effectData.OutputKeys.Count > 0 ? effectData.OutputKeys[0] : "";
        }

        /// <summary>
        /// 增加 E_SkillEffect 类型的 原因是 服务器的 EffectType 是配置生成的类型.
        /// 一开始 gl 效果类型的改名 并不会批量替换到 项目工程中. 所以 增加一个 E_SkillEffect 隔离.
        /// 但是 缺点 是 对于客户端需要处理的 效果,都需要手动 添加对应的  E_SkillEffect 效果枚举类型.
        /// </summary>
        /// <param name="effectData"></param>
        /// <returns></returns>
        public static E_SkillEffect GetEffectDataESkillEffect(EffectData effectData)
        {
            // 对于客户端不需要处理的 效果类型, 可以设置 技能类型 为 none.
            E_SkillEffect skillEffectType = E_SkillEffect.None;

            switch (effectData.EffectType)
            {
                case EffectType.Empty:
                    {
                        //dont do anything
                    }
                    break;
                case EffectType.CollisionBox:
                    {
                        skillEffectType = E_SkillEffect.CollisionBox;
                    }
                    break;
                case EffectType.TarGroup:
                    {
                        //dont do anything
                        skillEffectType = E_SkillEffect.TarGroup;
                    }
                    break;
                case EffectType.Treat:
                    {
                        skillEffectType = E_SkillEffect.Treat;
                    }
                    break;
                case EffectType.Damage:
                    {
                        skillEffectType = E_SkillEffect.Damage;
                    }
                    break;
                case EffectType.MoveWithPos:
                    {
                        skillEffectType = E_SkillEffect.MoveWithPos;
                    }
                    break;
                case EffectType.MoveWithRot:
                    {
                        skillEffectType = E_SkillEffect.MoveWithRot;
                    }
                    break;
                case EffectType.ChangeMana:
                    {
                        skillEffectType = E_SkillEffect.ChangeMana;
                    }
                    break;
                case EffectType.ChangeCD:
                    {
                        skillEffectType = E_SkillEffect.ChangeCD;
                    }
                    break;
                case EffectType.BreakCurRuntime:
                    {
                        skillEffectType = E_SkillEffect.BreakCurRuntime;
                    }
                    break;
                case EffectType.ChangeProp:
                    {
                        skillEffectType = E_SkillEffect.ChangeProp;
                    }
                    break;
                case EffectType.AddBuff:
                    {
                        skillEffectType = E_SkillEffect.AddBuff;
                    }
                    break;
                case EffectType.RemoveBuff:
                    {
                        skillEffectType = E_SkillEffect.RemoveBuff;
                    }
                    break;
                case EffectType.CreateBullet:
                    {
                        skillEffectType = E_SkillEffect.CreateBullet;
                    }
                    break;
                case EffectType.DestoryBullet:
                    {
                        skillEffectType = E_SkillEffect.DestoryBullet;
                    }
                    break;
                case EffectType.SetBulletTargetPos:
                    {
                        skillEffectType = E_SkillEffect.SetBulletTargetPos;
                    }
                    break;
                case EffectType.UserInput:
                    {
                        skillEffectType = E_SkillEffect.UserInput;
                    }
                    break;
                case EffectType.ChargeInput:
                    {
                        skillEffectType = E_SkillEffect.Energy;
                    }
                    break;
                case EffectType.ChangeToward:
                    {
                        skillEffectType = E_SkillEffect.ChangeToward;
                    }
                    break;
                case EffectType.ChangeToAbsoluteToward:
                    {
                        skillEffectType = E_SkillEffect.ChangeToAbsoluteToward;
                    }
                    break;
                case EffectType.IsNotEmpty:
                    {
                        // 判断目标key是否为空,目前 这个效果的逻辑是 等待服务器 线回复后
                        // 执行后续的 next 效果逻辑。
                        // 所以客户端线 不需要注册 next效果. 只需要 在执行客户端线的时候,打上对应的标签即可
                        skillEffectType = E_SkillEffect.IsNotEmpty;
                    }
                    break;
                case EffectType.BreakCurRuntimeInBullet:
                    {
                        // 服务器的效果,客户端不执行
                        skillEffectType = E_SkillEffect.BreakCurRuntimeInBullet;
                    }
                    break;
                case EffectType.PlayEffectAtPoint:
                    {
                        skillEffectType = E_SkillEffect.PlayEffectAtPoint;
                    }
                    break;

                case EffectType.PlayEffectAtTarget:
                    {
                        skillEffectType = E_SkillEffect.PlayEffectAtTarget;
                    }
                    break;
                case EffectType.PlayEffectBetweenPoints:
                    {
                        skillEffectType = E_SkillEffect.PlayEffectBetweenPoints;
                    }
                    break;
                case EffectType.Stealth:
                    {
                        skillEffectType = E_SkillEffect.Stealth;
                    }
                    break;
                case EffectType.SingleRandomPoint:
                    {
                        skillEffectType = E_SkillEffect.SingleRandomPoint;
                    }
                    break;
                case EffectType.DamageSecond:
                    {
                        skillEffectType = E_SkillEffect.DamageSecond;
                    }
                    break;
                case EffectType.SelectHitFromKey:
                    {
                        skillEffectType = E_SkillEffect.SelectHitFromKey;
                    }
                    break;
                case EffectType.SpSelectTarget:
                    {
                        skillEffectType = E_SkillEffect.SpSelectTarget;
                    }
                    break;
                case EffectType.ThrowBullet:
                    {
                        skillEffectType = E_SkillEffect.ThrowBullet;
                    }
                    break;
                case EffectType.LaunchBullet:
                    {
                        skillEffectType = E_SkillEffect.LaunchBullet;
                    }
                    break;
                case EffectType.BezierBullet:
                    {
                        skillEffectType = E_SkillEffect.BezierBullet;
                    }
                    break;
                case EffectType.PlayEffectLineRenderer:
                    {
                        skillEffectType = E_SkillEffect.PlayEffectLineRenderer;
                    }
                    break;
                case EffectType.CheckIntKey:
                    {
                        skillEffectType = E_SkillEffect.CheckIntKey;
                    }
                    break;
                case EffectType.CheckToward:
                    {
                        skillEffectType = E_SkillEffect.CheckToward;
                    }
                    break;
                case EffectType.CheckPassive:
                    {
                        skillEffectType = E_SkillEffect.CheckPassive;
                    }
                    break;
                case EffectType.ClientSummonAnim:
                    {
                        skillEffectType = E_SkillEffect.ClientSummonAnim;
                    }
                    break;
                case EffectType.ClientSummonEffect:
                    {
                        skillEffectType = E_SkillEffect.ClientSummonEffect;
                    }
                    break;
                case EffectType.ClientSummonTurnTo:
                    {
                        skillEffectType = E_SkillEffect.ClientSummonTurnTo;
                    }
                    break;
                case EffectType.ClientSummonRemove:
                    {
                        skillEffectType = E_SkillEffect.ClientSummonRemove;
                    }
                    break;
                default:
                    {
                        // SGF.Debuger.Log($"[SkillParam] int effectType {effectData.EffectType} no handle!!!!");
                    }
                    break;
            }

            return skillEffectType;
        }
    }

}
