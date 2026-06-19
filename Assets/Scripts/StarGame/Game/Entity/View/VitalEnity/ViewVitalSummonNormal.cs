using SGF.UI.Framework;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
using UnityEngine;
/// <summary>
/// 召唤物显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalSummonNormal : ViewVitalNPCNormal
    {
        public new SummonEntityBase M_EntityBase
        {
            get
            {
                return m_entity as SummonEntityBase;
            }
        }

        protected override void Create(EntityObject entity)
        {
            m_entity = entity as SummonEntityBase;

            if (M_EntityBase == null)
            {
                return;
            }

            base.Create(entity);
            // 修改人节点的tag
            if (M_EntityBase != null)
            {
                if (M_EntityBase.EntityType == E_EntityType.Monster || M_EntityBase.EntityType == E_EntityType.GVEBoss || M_EntityBase.EntityType == E_EntityType.Robot)
                {
                    gameObject.tag = E_TagType.Enemy.ToString();
                }

                /// 2024/6/18
                /// 目前支持的枚举中均支持穿墙
                /// 1.创建跟随玩家高度
                /// 2.永远跟随玩家高度
                /// 3.创建跟随地面高度
                /// 4.永远跟随地面高度 (还未做, 这种 不需要穿墙, 后面做的时候 需要处理一下)
                {
                    // 如果是子弹，先让子弹默认能够穿墙
                    if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
                    {
                        SetLayer(LayerMask.NameToLayer("Bullet"));// 无碰撞层
                    }

                }
            }


        }

        protected override void Move(Vector3 movingMotion)
        {
            // 如果 是子弹类型的时候, 子弹的 位移 不考虑 y 轴
            if (m_entity != null && m_entity.EntityType == E_EntityType.BulletEntity)
            {
                movingMotion.y = 0;
                m_CharacterController.transform.Translate(movingMotion);
            }
            else
            {
                base.Move(movingMotion);
            }
        }

        protected override void Release()
        {
            base.Release();

            m_entity = null;
            m_context = null;
        }
    }
}
