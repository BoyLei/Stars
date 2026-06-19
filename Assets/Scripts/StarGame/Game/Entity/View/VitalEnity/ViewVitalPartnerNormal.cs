using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
/// <summary>
/// 伙伴显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalPartnerNormal : ViewVitalNPCNormal
    {
        public new PartnerEntityBase M_EntityBase
        {
            get
            {
                return m_entity as PartnerEntityBase;
            }
        }

        protected override void Create(EntityObject entity)
        {
            m_entity = entity as PartnerEntityBase;

            if (M_EntityBase == null)
            {
                return;
            }

            base.Create(entity);
            // 修改人节点的tag
            if (M_EntityBase != null && M_EntityBase.EntityType == E_EntityType.Monster)
            {
                gameObject.tag = E_TagType.Enemy.ToString();
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
