///--------------------------------------------------------------------
/// 文件名   :   BaseBuffEffect
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/01 15:17:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using SkillEditor;
using System;
using StarProject.Game.Skill;

namespace StarProject.Game.TypeEffect
{
    public abstract class BaseTypeEffect
    {

        protected ulong ownerEntityID;
        protected ulong BuilderID;

        /// <summary>
        /// 效果对应 的 配置ID 。 以前是 buff ID, 后面效果拓展为 子弹/被动/buff ID
        /// </summary>
        protected int CfgID;

        protected ulong RuntimeID;

        public GlobalShowSerialize EffectTypeSerialize;

        /// <summary>
        /// 显示的 GlobalShowType
        /// </summary>
        public GlobalShowType ShowType => EffectTypeSerialize.GlobalShowType;

        /// <summary>
        /// 效果 对应的 黑板, 本来想用的是 运行时,但是 子弹不像被动buff, 它没有继承ServerControlStageEntityBase基类.
        /// 所以此处 先使用 黑板运行时替代. 后续 如果 还是需要 对应的运行时, 再考虑是否需要将子弹 继承ServerControlStageEntityBase。
        /// </summary>
        public BaseBlackBoard BlackBoard;

        public void Init(GlobalShowSerialize typeSerialize)
        {
            EffectTypeSerialize = typeSerialize;

        }

        public void InitBlackBoard(ulong runtimeID, ulong builderID, BaseBlackBoard baseBlackBoard)
        {
            RuntimeID = runtimeID;
            BuilderID = builderID;
            BlackBoard = baseBlackBoard;
        }

        public virtual void OnEnter(ulong owneruid, int cfgId)
        {
            this.ownerEntityID = owneruid;
            this.CfgID = cfgId;
        }

        /// <summary>
        /// 效果退出的时候 先不清空 ownerEntityID 和 CfgID
        /// note:
        ///     策划目前有个需求, 人物死亡的时候，并不立即移除 shader 效果. 而是等动画播放完成后再移除.
        ///     所以 再buff/被动 移除的时候, 效果虽然执行了 OnExit， 但是其实 shder 效果并没有执行退出。
        /// 
        ///     当主角复活后, 需要移除身上的效果。 所以在复活的时候，需要执行 这个shader效果的退出. 
        ///     所以 此时需要用到这两个数据
        /// </summary>
        public virtual void OnExit()
        {
            // this.ownerEntityID = 0;
            // this.CfgID = 0;
        }

        /// <summary>
        /// 显示效果 进入的时候
        /// </summary>
        public virtual void OnShowEnter()
        {

        }

        /// <summary>
        /// 显示效果 刷新的时候
        /// </summary>
        public virtual void OnShowUpdate()
        {

        }


        /// <summary>
        /// 显示效果 退出的时候
        /// </summary>
        public virtual void OnShowExit()
        {

        }

    }
}