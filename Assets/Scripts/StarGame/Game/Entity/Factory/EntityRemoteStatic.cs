using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Skill;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
///本類子集包含，具有戰鬥邏輯輔助的辅助实体宙斯的G，Tb的换血
///还有就是辅助显示的
/// </summary>
namespace StarProject.Game.Entity
{
    public class EntityRemoteStatic : EntityObject
    {
        public Action<Vector3> ViewEnterFrameAction;
        private EntityOutLifeType nttType;
        protected Vector3 m_currentPos;
        internal virtual void EnterFrame()
        {
            //base.EnterFrame();
            //技能特殊移动
            ViewEnterFrameAction?.Invoke(Vector3.zero);
        }


        /// <summary>
        /// 非生命体的东西
        /// 1有表现的是子弹
        /// 2没表现的是法术场核心支撑器
        /// 3都依赖黑板
        /// </summary>
        /// 
        StringBuilder sb = new StringBuilder();
        //子类拓展吧，子类需要的都不同
        //TODO：V层数据
        //TODO：数据：黑板数据
        //TODO：黑板数据（数据层的数据）
        public void Create(E_WithOuLifeResType resType, int id/*, Data data*/, Transform container)
        {
            switch (resType)
            {
                case E_WithOuLifeResType.NullEffect:
                    break;
                case E_WithOuLifeResType.Bullet:
                    sb.Append("Perfab/Bullet/Bullet_");
                    sb.Append(id);
                    ViewFactory.CreateView(sb.ToString(), "Perfab/Fx/cm/0", this, container);
                    break;
                case E_WithOuLifeResType.FxUnit:
                    sb.Append("Perfab/Fx/ActorFx/BattleUniteFx/");
                    sb.Append(id);
                    //特效节点View是自己选择挂载的
                    ViewFactory.CreateView(sb.ToString(), "Perfab/Fx/Default/0", this, container);

                    break;
                case E_WithOuLifeResType.UnitPendant:
                    break;
                case E_WithOuLifeResType.UI:
                    sb.Append("Perfab/Fx/UIFX_ABS/BattleUniteFx/");
                    sb.Append(id);
                    //特效节点View是自己选择挂载的
                    ViewFactory.CreateView(sb.ToString(), "Perfab/Fx/Default/0", this, container, "UI");

                    break;
                case E_WithOuLifeResType.Max:
                    break;
                default:
                    break;
            }

            sb.Clear();
        }





        /// <summary>
        /// 逻辑释放基于服务器
        /// 【工具项先不管】//1，特效如果是静态的那就不需要基于服务器【本地动态拼接，依赖美术，客户端顶多帮你加载上去】
        /// 
        /// 【TODO非逻辑实体的简单池特性：没必要同步逻辑】2，如果是动态的：客户端本地的压根不用实体直接特效加载器【本地，动态，可池化】
        /// 
        /// ***3，    /// 【current】3，如果是动态：远端的就要基于服务器释放，就是实体【远端，动态可池化】
        /// </summary>
        protected override void Release()
        {
            base.Release();
            ViewFactory.ReleaseView(this);
        }

        public override Vector3 Position()
        {
            return m_currentPos;
        }

        //protected override void OnnBlackBord(string arg1, object arg2)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void DealSkillStageChange(SkillEntity arg1, E_ULayerSubState arg2, E_ULayerSubState arg3, bool isSuspendByServer)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void OnAttrSync(ulong arg0, string arg1, object arg2)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override void MoveByServer(Vector3 pos, bool isBorn)
        //{
        //    throw new NotImplementedException();
        //}

        public EntityOutLifeType GetyOutLifeType
        {
            get
            {
                return nttType;
            }
            set
            {
                nttType = value;
            }
        }


    }
}