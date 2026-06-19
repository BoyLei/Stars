using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class EquipIntensifyData
	{
		public Dictionary<int, EquipIntensifyDataCell> StaticEquipIntensifyDatas = new Dictionary<int, EquipIntensifyDataCell>();
	}
	public class EquipIntensifyDataCell
	{
		//编号
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//强化等级
		public string Level;
		private int _Level = -1;
		public int GetLevel()
		{
			if (_Level == -1 && int.TryParse(Level, out _Level))
			{
			}
			return _Level;
		}
		//强化属性
		public List<int> Property = new List<int>();
		//强化值
		public List<int> Value = new List<int>();

	}
}
