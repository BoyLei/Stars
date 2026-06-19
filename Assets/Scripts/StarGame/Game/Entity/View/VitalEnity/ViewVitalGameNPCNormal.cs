using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
/// <summary>
/// NPC显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalGameNPCNormal : ViewVitalNPCNormal
    {
        public new GameNPCEntityBase M_EntityBase
        {
            get
            {
                return m_entity as GameNPCEntityBase;
            }
        }

        protected override void Create(EntityObject entity)
        {
            //重力
            m_entity = entity as GameNPCEntityBase;

            if (M_EntityBase == null)
            {
                return;
            }

            base.Create(entity);

            // 修改人节点的tag
            if (M_EntityBase != null)
            {
                gameObject.tag = E_TagType.NPC.ToString();
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
