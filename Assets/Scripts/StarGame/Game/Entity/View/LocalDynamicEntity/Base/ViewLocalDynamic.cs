using StarProject.Game.Entity.Factory;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Entity.View.LocalDynamic
{
    public abstract class ViewLocalDynamic : ViewLocal
    {

        protected EntityLocalDynamic m_entity;

        public override string ModelPath
        {
            get
            {
                if (m_entity.modelDataCell != null)
                {
                    return m_entity.modelDataCell.ModelsPath;
                }
                return "";
            }
        }

        public override float ModleScale => m_entity.ModleScale;

        protected override AvatarDataCell AvatarData => m_entity.avatarDataCell;


        protected override void Create(EntityObject entity)
        {
            if (entity == null)
            {
                return;
            }
            m_entity = entity as EntityLocalDynamic;

            base.Create(entity);
        }

        protected override void Reset()
        {
            base.Reset();

            m_entity = null;
        }

        protected override void OnModleViewCreateFinish(bool result)
        {
            if (!result)
            {
                SGF.Debuger.LogWarning($" Create() entityid={m_entity.EntityKey},entitytype={m_entity.Type},triggerkey={m_entity.TriggerKey},ModelPath={ModelPath},模型加载失败 err!!!");

                return;
            }
            m_entity.ActionOnViewCreateFinish?.Invoke();
        }


        protected override void SetEntityPosition(Vector3 pos)
        {
            m_entity.SetPosition(pos);
        }

        protected override void OnEventListener()
        {
            if (m_entity == null)
            {
                return;
            }

            m_entity.ActionOnStopSpecialEffects += BaseStopFx;
            m_entity.ActionOnPlaySpecialEffects += BasePlayEfxAsync;
            m_entity.ActionOnHidden += OnActionOnHidden;
        }

        protected override void OffEventListener()
        {
            if (m_entity == null)
            {
                return;
            }

            m_entity.ActionOnStopSpecialEffects -= BaseStopFx;
            m_entity.ActionOnPlaySpecialEffects -= BasePlayEfxAsync;
            m_entity.ActionOnHidden -= OnActionOnHidden;
        }

    }
}
