//来源表战场表_BattleField.xlsm.xlsx -> sheet:BattleFieldLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattleFieldLevelData
	{
		[Key(0)]
		public Dictionary<int, BattleFieldLevelDataCell> StaticBattleFieldLevelDatas = new Dictionary<int, BattleFieldLevelDataCell>();
	}
	[MessagePackObject]
	public class BattleFieldLevelDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//最低等级
		[Key(1)]
		public int MinLevel;
		public int GetMinLevel()
		{
			return MinLevel;
		}
		//最高等级
		[Key(2)]
		public int MaxLevel;
		public int GetMaxLevel()
		{
			return MaxLevel;
		}
	}
}
