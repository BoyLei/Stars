//来源表竞技场表_Arena.xlsx -> sheet:ArenaRankAward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ArenaRankAwardData
	{
		[Key(0)]
		public Dictionary<int, ArenaRankAwardDataCell> StaticArenaRankAwardDatas = new Dictionary<int, ArenaRankAwardDataCell>();
	}
	[MessagePackObject]
	public class ArenaRankAwardDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//排名区间1
		[Key(1)]
		public int RankBegin;
		public int GetRankBegin()
		{
			return RankBegin;
		}
		//排名区间2
		[Key(2)]
		public int RankEnd;
		public int GetRankEnd()
		{
			return RankEnd;
		}
		//奖励道具
		[Key(3)]
		public List<long> Reward1 = new List<long>();
		//奖励数量
		[Key(4)]
		public List<long> Reward1Num = new List<long>();
	}
}
