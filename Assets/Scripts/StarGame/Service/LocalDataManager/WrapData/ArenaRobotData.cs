//来源表竞技场表_Arena.xlsx -> sheet:ArenaRobot
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ArenaRobotData
	{
		[Key(0)]
		public Dictionary<int, ArenaRobotDataCell> StaticArenaRobotDatas = new Dictionary<int, ArenaRobotDataCell>();
	}
	[MessagePackObject]
	public class ArenaRobotDataCell
	{
		//排名区间
		[Key(0)]
		public int Rank;
		public int GetRank()
		{
			return Rank;
		}
		//机器人等级上限
		[Key(1)]
		public int LevelUpLimit;
		public int GetLevelUpLimit()
		{
			return LevelUpLimit;
		}
		//机器人等级下限
		[Key(2)]
		public int LevelLowerLimit;
		public int GetLevelLowerLimit()
		{
			return LevelLowerLimit;
		}
		//机器人战力上限
		[Key(3)]
		public int PowerUpperLimit;
		public int GetPowerUpperLimit()
		{
			return PowerUpperLimit;
		}
		//机器人战力下限
		[Key(4)]
		public int PowerLowerLimit;
		public int GetPowerLowerLimit()
		{
			return PowerLowerLimit;
		}
	}
}
