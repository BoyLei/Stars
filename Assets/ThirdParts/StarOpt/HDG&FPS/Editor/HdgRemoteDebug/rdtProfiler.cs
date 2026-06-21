using GameDLL;
using System;
using UnityEngine;

namespace GameEditor.Hdg
{
	internal class rdtProfiler : IDisposable
	{
		public rdtProfiler(string desc)
		{
			m_start = DateTime.Now;
			m_description = desc;
		}

		public void Dispose()
		{
			TimeSpan delta = DateTime.Now - m_start;
			Debug.LogFormat("{0} took {1}s", m_description, delta.TotalSeconds);
		}

		private DateTime m_start;

		private string m_description;
	}
}
