//来源表战力表_Power.xlsm.xlsx -> sheet:TotalPowerRank
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TotalPowerRankData
	{
		[Key(0)]
		public Dictionary<int, TotalPowerRankDataCell> StaticTotalPowerRankDatas = new Dictionary<int, TotalPowerRankDataCell>();
	}
	[MessagePackObject]
	public class TotalPowerRankDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//大段位ID
		[Key(1)]
		public int ParentRank;
		public int GetParentRank()
		{
			return ParentRank;
		}
		//子段位
		[Key(2)]
		public int SubRank;
		public int GetSubRank()
		{
			return SubRank;
		}
		//生效开服时间
		[Key(3)]
		public int OpenDay;
		public int GetOpenDay()
		{
			return OpenDay;
		}
		//总养成评分
		[Key(4)]
		public long TotalPower;
		public long GetTotalPower()
		{
			return TotalPower;
		}
		//段位到达奖励
		[Key(5)]
		public long Reward;
		public long GetReward()
		{
			return Reward;
		}
		//段位到达奖励数量
		[Key(6)]
		public long RewardNum;
		public long GetRewardNum()
		{
			return RewardNum;
		}
	}
}
