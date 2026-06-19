//来源表公会表_Guild.xlsm.xlsx -> sheet:GuildLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuildLevelData
	{
		[Key(0)]
		public Dictionary<int, GuildLevelDataCell> StaticGuildLevelDatas = new Dictionary<int, GuildLevelDataCell>();
	}
	[MessagePackObject]
	public class GuildLevelDataCell
	{
		//公会等级
		[Key(0)]
		public int GuildLevel;
		public int GetGuildLevel()
		{
			return GuildLevel;
		}
		//升级消耗经验
		[Key(1)]
		public int ExpCost;
		public int GetExpCost()
		{
			return ExpCost;
		}
		//公会上限人数
		[Key(2)]
		public int GuildNum;
		public int GetGuildNum()
		{
			return GuildNum;
		}
		//副会长上限人数
		[Key(3)]
		public int ViceLeaderNum;
		public int GetViceLeaderNum()
		{
			return ViceLeaderNum;
		}
		//精英上限人数
		[Key(4)]
		public int EliteLeaderNum;
		public int GetEliteLeaderNum()
		{
			return EliteLeaderNum;
		}
	}
}
