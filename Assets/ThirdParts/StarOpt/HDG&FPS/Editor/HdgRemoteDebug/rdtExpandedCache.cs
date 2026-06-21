using System;
using System.Collections.Generic;
using GameDLL.Hdg;

namespace GameEditor.Hdg
{
	[Serializable]
	public class rdtExpandedCache
	{
		private Dictionary<string, bool> m_expandedState;

		public rdtExpandedCache()
		{
			m_expandedState = new Dictionary<string, bool>();
		}

		public void Clear()
		{
			m_expandedState.Clear();
		}

		public bool IsExpanded(int instanceId, string suffix = null)
		{
			string key = instanceId.ToString();
			if (!string.IsNullOrEmpty(suffix))
				key = key + "." + suffix;
			return m_expandedState.ContainsKey(key) && m_expandedState[key];
		}

		public bool IsExpanded(rdtTcpMessageComponents.Component component, string key = null)
		{
			return IsExpanded(component.m_instanceId, key);
		}

		public bool IsExpanded(rdtTcpMessageComponents.Component component, rdtTcpMessageComponents.Property property, string suffix = null)
		{
			return IsExpanded(component.m_instanceId, property.m_name + suffix);
		}

		public void SetExpanded(bool expanded, rdtTcpMessageComponents.Component component, string key = null)
		{
			SetExpanded(expanded, component.m_instanceId, key);
		}

		public void SetExpanded(bool expanded, rdtTcpMessageComponents.Component component, rdtTcpMessageComponents.Property property)
		{
			SetExpanded(expanded, component.m_instanceId, property.m_name);
		}

		public void SetExpanded(bool expanded, int instanceId, string suffix = null)
		{
			string key = instanceId.ToString();
			if (!string.IsNullOrEmpty(suffix))
				key = key + "." + suffix;
			m_expandedState[key] = expanded;
		}
	}
}
