//来源表通缉任务表_TeamWanted.xlsx -> sheet:TeamWantedAddition
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamWantedAdditionData
	{
		[Key(0)]
		public Dictionary<int, TeamWantedAdditionDataCell> StaticTeamWantedAdditionDatas = new Dictionary<int, TeamWantedAdditionDataCell>();
	}
	[MessagePackObject]
	public class TeamWantedAdditionDataCell
	{
		//等级差
		[Key(0)]
		public int LevelDiff;
		public int GetLevelDiff()
		{
			return LevelDiff;
		}
		//加成效果
		[Key(1)]
		public int BuffID;
		public int GetBuffID()
		{
			return BuffID;
		}
	}
}
