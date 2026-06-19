//来源表公会午间活动表_GuildNoonActiv.xlsm.xlsx -> sheet:GNActivRank
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNActivRankData
	{
		[Key(0)]
		public Dictionary<int, GNActivRankDataCell> StaticGNActivRankDatas = new Dictionary<int, GNActivRankDataCell>();
	}
	[MessagePackObject]
	public class GNActivRankDataCell
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
		//第1名奖励
		[Key(3)]
		public List<long> Reward1 = new List<long>();
		//第1名奖励数量
		[Key(4)]
		public List<long> Reward1Num = new List<long>();
		//排名2
		[Key(5)]
		public int Rank2;
		public int GetRank2()
		{
			return Rank2;
		}
		//第2名奖励
		[Key(6)]
		public List<long> Reward2 = new List<long>();
		//第2名奖励数量
		[Key(7)]
		public List<long> Reward2Num = new List<long>();
		//排名4
		[Key(8)]
		public int Rank3;
		public int GetRank3()
		{
			return Rank3;
		}
		//第4名奖励
		[Key(9)]
		public List<long> Reward3 = new List<long>();
		//第4名奖励数量
		[Key(10)]
		public List<long> Reward3Num = new List<long>();
		//排名11
		[Key(11)]
		public int Rank4;
		public int GetRank4()
		{
			return Rank4;
		}
		//第11名奖励
		[Key(12)]
		public List<long> Reward4 = new List<long>();
		//第11名奖励数量
		[Key(13)]
		public List<long> Reward4Num = new List<long>();
		//排名21
		[Key(14)]
		public int Rank5;
		public int GetRank5()
		{
			return Rank5;
		}
		//第21名奖励
		[Key(15)]
		public List<long> Reward5 = new List<long>();
		//第21名奖励数量
		[Key(16)]
		public List<long> Reward5Num = new List<long>();
		//排名51
		[Key(17)]
		public int Rank6;
		public int GetRank6()
		{
			return Rank6;
		}
		//第51名奖励
		[Key(18)]
		public List<long> Reward6 = new List<long>();
		//第51名奖励数量
		[Key(19)]
		public List<long> Reward6Num = new List<long>();
		//排名101
		[Key(20)]
		public int Rank7;
		public int GetRank7()
		{
			return Rank7;
		}
		//第101名奖励
		[Key(21)]
		public List<long> Reward7 = new List<long>();
		//第101名奖励数量
		[Key(22)]
		public List<long> Reward7Num = new List<long>();
		//排名201
		[Key(23)]
		public int Rank8;
		public int GetRank8()
		{
			return Rank8;
		}
		//第201名奖励
		[Key(24)]
		public List<long> Reward8 = new List<long>();
		//第201名奖励数量
		[Key(25)]
		public List<long> Reward8Num = new List<long>();
	}
}
