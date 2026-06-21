using System;
using System.Collections.Generic;

namespace GameDLL.Hdg
{
	public class rdtDispatcher
	{
		private Queue<Action> m_callbacks = new Queue<Action>();

		public void Clear()
		{
			Queue<Action> callbacks = m_callbacks;
			lock (callbacks)
			{
				m_callbacks.Clear();
			}
		}

		public void Enqueue(Action action)
		{
			Queue<Action> callbacks = m_callbacks;
			lock (callbacks)
			{
				m_callbacks.Enqueue(action);
			}
		}

		public void Update()
		{
			Queue<Action> callbacks = m_callbacks;
			lock (callbacks)
			{
				while (m_callbacks.Count > 0)
				{
					m_callbacks.Dequeue()();
				}
			}
		}
	}
}
