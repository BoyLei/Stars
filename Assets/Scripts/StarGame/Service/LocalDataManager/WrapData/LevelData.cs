using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class LevelData
	{
		public Dictionary<int, LevelDataCell> StaticLevelDatas = new Dictionary<int, LevelDataCell>();
	}
	public class LevelDataCell
	{
		//角色等级
		public string Level;
		private int _Level = -1;
		public int GetLevel()
		{
			if (_Level == -1 && int.TryParse(Level, out _Level))
			{
			}
			return _Level;
		}
		//升级所需经验
		public string Exp;
		private int _Exp = -1;
		public int GetExp()
		{
			if (_Exp == -1 && int.TryParse(Exp, out _Exp))
			{
			}
			return _Exp;
		}

	}
}
