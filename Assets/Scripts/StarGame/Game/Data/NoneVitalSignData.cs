
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Player;
using StarProjectDef;

namespace StarProject.Game.Data
{
    /// <summary>
    /// 非生命体的数据
    /// 
    /// </summary>
    public class NoneVitalSignData : EntityBaseData
    {
        public E_EntityType dataType = E_EntityType.None;

        /// <summary>
        /// 出具层关联的AOI同步的 AOIEntityObject
        /// </summary>
        public AOIEntityObject myOwnerNtt;

        /// <summary>
        /// 每个非生命实体数据所拥有的的EntityCtrlBase
        /// </summary>
        public EntityCtrlBase myOwnerNttGroup;

        public override void Create(ulong entityId, E_EntityType entityType, bool isServerAOI)
        {
            base.Create(entityId, entityType, isServerAOI);
        }

        protected override void Release()
        {
            base.Release();
        }

        protected override void Create()
        {
            throw new System.NotImplementedException();
        }
    }
}