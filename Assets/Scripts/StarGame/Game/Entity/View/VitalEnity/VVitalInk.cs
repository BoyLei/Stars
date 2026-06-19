
using StarProject.Game.Entity.Factory;
using System;
using UnityEngine;

namespace StarProject.Game.Entity.View
{

	public class VVitalInk : ViewModel
    {
		protected ParticleSystem m_ps;

        protected override void Create(EntityObject entity)
        {
            //base.Create(entity);

            m_ps = this.GetComponentInChildren<ParticleSystem>();

            UnityEngine.ParticleSystem.MainModule main = m_ps.main;
            //main.startLifetimeMultiplier = m_entity.Data.length * 0.033f;

            //m_entity.ShowData.bodyVisible = false;
        }

        protected override void Release()
        {
            throw new NotImplementedException();
        }


        /* protected override void Create(EntityObject entity)
         {
             base.Create(entity);

             m_ps = this.GetComponentInChildren<ParticleSystem>();

             UnityEngine.ParticleSystem.MainModule main = m_ps.main;
             //main.startLifetimeMultiplier = m_entity.Data.length * 0.033f;

             m_entity.ShowData.bodyVisible = false;
         }

         protected override void Release()
         {
             m_ps = null;
             base.Release();
         }

         protected override void Update()
         {
             base.Update();

             if (m_ps != null && m_entity != null)
             {
                 //if (m_ps.main.startLifetimeMultiplier != m_entity.Data.length * 0.033f)
                 //{
                 //	UnityEngine.ParticleSystem.MainModule main = m_ps.main;
                 //	main.startLifetimeMultiplier = m_entity.Data.length * 0.033f;
                 //}
             }
         }*/
    }

}