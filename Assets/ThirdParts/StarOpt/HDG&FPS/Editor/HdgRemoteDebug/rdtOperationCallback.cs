using GameDLL.Hdg;
using System;
using System.Collections.Generic;

namespace GameEditor.Hdg
{
    [Serializable]
    public class rdtOperationCallback
    {
        private Dictionary<string, Action<object>> m_operationCallback;

		public rdtOperationCallback()
		{
			m_operationCallback = new Dictionary<string, Action<object>>();
		}

		public void Clear()
		{
			m_operationCallback.Clear();
		}

		public void SetCallback(string operation, Action<object> callback)
		{
			m_operationCallback[operation] = callback;
		}

		public void ReturnCallback(string operation, object value)
        {
			Action<object> action;
			if (m_operationCallback.TryGetValue(operation, out action))
			{
				if (action != null)
				{
					action(value);
					return;
				}
			}
			rdtDebug.Debug(this, "Operation {0} Return {1}", operation, value);
		}
	}
}
