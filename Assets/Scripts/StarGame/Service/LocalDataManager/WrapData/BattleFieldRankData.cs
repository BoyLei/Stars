//来源表战场表_BattleField.xlsm.xlsx -> sheet:BattleFieldRank
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattleFieldRankData
	{
		[Key(0)]
		public Dictionary<int, BattleFieldRankDataCell> StaticBattleFieldRankDatas = new Dictionary<int, BattleFieldRankDataCell>();
	}
	[MessagePackObject]
	public class BattleFieldRankDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//是否获胜
		[Key(1)]
		public bool Result;
		public bool GetResult()
		{
			return Result;
		}
		//排名
		[Key(2)]
		public int Rank;
		public int GetRank()
		{
			return Rank;
		}
		//奖励道具
		[Key(3)]
		public List<long> Reward1 = new List<long>();
		//奖励数量
		[Key(4)]
		public List<long> Reward1Num = new List<long>();
	}
}
