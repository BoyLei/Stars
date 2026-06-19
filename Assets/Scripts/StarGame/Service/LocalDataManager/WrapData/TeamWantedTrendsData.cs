//来源表通缉任务表_TeamWanted.xlsx -> sheet:TeamWantedTrends
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamWantedTrendsData
	{
		[Key(0)]
		public Dictionary<int, TeamWantedTrendsDataCell> StaticTeamWantedTrendsDatas = new Dictionary<int, TeamWantedTrendsDataCell>();
	}
	[MessagePackObject]
	public class TeamWantedTrendsDataCell
	{
		//开服天数
		[Key(0)]
		public int OpenDays;
		public int GetOpenDays()
		{
			return OpenDays;
		}
		//怪物等级
		[Key(1)]
		public int MonsterLevel;
		public int GetMonsterLevel()
		{
			return MonsterLevel;
		}
		//1阶奖励掉落
		[Key(2)]
		public long Awards1;
		public long GetAwards1()
		{
			return Awards1;
		}
		//2阶奖励掉落
		[Key(3)]
		public long Awards2;
		public long GetAwards2()
		{
			return Awards2;
		}
		//3阶第1名奖励掉落
		[Key(4)]
		public long Awards3First;
		public long GetAwards3First()
		{
			return Awards3First;
		}
		//3阶第2名奖励掉落
		[Key(5)]
		public long Awards3Second;
		public long GetAwards3Second()
		{
			return Awards3Second;
		}
		//3阶第3名奖励掉落
		[Key(6)]
		public long Awards3Third;
		public long GetAwards3Third()
		{
			return Awards3Third;
		}
	}
}
