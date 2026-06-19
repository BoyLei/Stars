
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.OutLife;
using System;
using UnityEngine;

namespace StarProject.Game.Entity.View.OutLife
{

	public class FxView : OutLifeEntityViewBase
	{
		protected ParticleSystem m_ps;


		protected override void Create(EntityObject entity)
		{
			base.Create(entity);

			m_ps = this.GetComponentInChildren<ParticleSystem>();

			UnityEngine.ParticleSystem.MainModule main = m_ps.main;
			//要开放
            //main.startLifetimeMultiplier = m_entity.Data.length * 0.033f;

            //m_entity.ShowData.bodyVisible = false;
        }

		protected override void Release()
		{
			m_ps = null;
			base.Release();
		}

		protected override void Update()
		{
			base.Update();
			//播放完毕也要等服务器，logic到view
			//if (m_ps != null && m_entity != null)
			//{
   //             if (m_ps.main.startLifetimeMultiplier != m_entity.Data.length * 0.033f)
   //             {
   //                 UnityEngine.ParticleSystem.MainModule main = m_ps.main;
   //                 main.startLifetimeMultiplier = m_entity.Data.length * 0.033f;
   //             }
   //         }
		}
	}

}