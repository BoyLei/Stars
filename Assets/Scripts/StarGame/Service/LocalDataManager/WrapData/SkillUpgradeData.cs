using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SkillUpgradeData
	{
		public Dictionary<int, SkillUpgradeDataCell> StaticSkillUpgradeDatas = new Dictionary<int, SkillUpgradeDataCell>();
	}
	public class SkillUpgradeDataCell
	{		//主键
		public string Key;
		private int _Key = -1;
		public int GetKey()
		{
			if (_Key == -1 && int.TryParse(Key, out _Key))
			{
			}
			return _Key;
		}
		//职业技能ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//职业技能等级
		public string Level;
		private int _Level = -1;
		public int GetLevel()
		{
			if (_Level == -1 && int.TryParse(Level, out _Level))
			{
			}
			return _Level;
		}
		//职业技能升级条件
		public List<int> Condition = new List<int>();
		//职业技能升级消耗
		public List<int> Cost = new List<int>();
		//转职返还
		public List<int> TransReturn = new List<int>();

	}
}
