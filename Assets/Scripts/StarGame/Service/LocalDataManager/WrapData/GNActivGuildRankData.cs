//来源表公会午间活动表_GuildNoonActiv.xlsm.xlsx -> sheet:GNActivGuildRank
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNActivGuildRankData
	{
		[Key(0)]
		public Dictionary<int, GNActivGuildRankDataCell> StaticGNActivGuildRankDatas = new Dictionary<int, GNActivGuildRankDataCell>();
	}
	[MessagePackObject]
	public class GNActivGuildRankDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//开服天数
		[Key(1)]
		public int OpenDays;
		public int GetOpenDays()
		{
			return OpenDays;
		}
		//排名1
		[Key(2)]
		public int Rank1;
		public int GetRank1()
		{
			return Rank1;
		}
		//第1名奖励掉落
		[Key(3)]
		public long Reward1;
		public long GetReward1()
		{
			return Reward1;
		}
		//第1名奖励展示
		[Key(4)]
		public List<int> Reward1Show = new List<int>();
		//排名2
		[Key(5)]
		public int Rank2;
		public int GetRank2()
		{
			return Rank2;
		}
		//2-3名奖励掉落
		[Key(6)]
		public long Reward2;
		public long GetReward2()
		{
			return Reward2;
		}
		//2-3名奖励展示
		[Key(7)]
		public List<int> Reward2Show = new List<int>();
		//排名4
		[Key(8)]
		public int Rank3;
		public int GetRank3()
		{
			return Rank3;
		}
		//4-10名奖励掉落
		[Key(9)]
		public long Reward3;
		public long GetReward3()
		{
			return Reward3;
		}
		//4-10名奖励展示
		[Key(10)]
		public List<int> Reward3Show = new List<int>();
		//排名11
		[Key(11)]
		public int Rank4;
		public int GetRank4()
		{
			return Rank4;
		}
		//11-20名奖励掉落
		[Key(12)]
		public long Reward4;
		public long GetReward4()
		{
			return Reward4;
		}
		//11-20名奖励展示
		[Key(13)]
		public List<int> Reward4Show = new List<int>();
		//排名21
		[Key(14)]
		public int Rank5;
		public int GetRank5()
		{
			return Rank5;
		}
		//21名之后奖励掉落
		[Key(15)]
		public long Reward5;
		public long GetReward5()
		{
			return Reward5;
		}
		//21名之后奖励展示
		[Key(16)]
		public List<int> Reward5Show = new List<int>();
	}
}
