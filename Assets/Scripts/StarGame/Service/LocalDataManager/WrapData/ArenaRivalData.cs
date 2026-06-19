//来源表竞技场表_Arena.xlsx -> sheet:ArenaRival
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ArenaRivalData
	{
		[Key(0)]
		public Dictionary<int, ArenaRivalDataCell> StaticArenaRivalDatas = new Dictionary<int, ArenaRivalDataCell>();
	}
	[MessagePackObject]
	public class ArenaRivalDataCell
	{
		//玩家排名区间
		[Key(0)]
		public int PlayerRank;
		public int GetPlayerRank()
		{
			return PlayerRank;
		}
		//对手1排名上限
		[Key(1)]
		public int Rival1UpLimit;
		public int GetRival1UpLimit()
		{
			return Rival1UpLimit;
		}
		//对手1排名下限
		[Key(2)]
		public int Rival1LowerLimit;
		public int GetRival1LowerLimit()
		{
			return Rival1LowerLimit;
		}
		//对手2排名上限
		[Key(3)]
		public int Rival2UpLimit;
		public int GetRival2UpLimit()
		{
			return Rival2UpLimit;
		}
		//对手2排名下限
		[Key(4)]
		public int Rival2LowerLimit;
		public int GetRival2LowerLimit()
		{
			return Rival2LowerLimit;
		}
		//对手3排名上限
		[Key(5)]
		public int Rival3UpLimit;
		public int GetRival3UpLimit()
		{
			return Rival3UpLimit;
		}
		//对手3排名下限
		[Key(6)]
		public int Rival3LowerLimit;
		public int GetRival3LowerLimit()
		{
			return Rival3LowerLimit;
		}
		//对手4排名上限
		[Key(7)]
		public int Rival4UpLimit;
		public int GetRival4UpLimit()
		{
			return Rival4UpLimit;
		}
		//对手4排名下限
		[Key(8)]
		public int Rival4LowerLimit;
		public int GetRival4LowerLimit()
		{
			return Rival4LowerLimit;
		}
		//对手5排名上限
		[Key(9)]
		public int Rival5UpLimit;
		public int GetRival5UpLimit()
		{
			return Rival5UpLimit;
		}
		//对手5排名下限
		[Key(10)]
		public int Rival5LowerLimit;
		public int GetRival5LowerLimit()
		{
			return Rival5LowerLimit;
		}
		//对手6排名上限
		[Key(11)]
		public int Rival6UpLimit;
		public int GetRival6UpLimit()
		{
			return Rival6UpLimit;
		}
		//对手6排名下限
		[Key(12)]
		public int Rival6LowerLimit;
		public int GetRival6LowerLimit()
		{
			return Rival6LowerLimit;
		}
	}
}
