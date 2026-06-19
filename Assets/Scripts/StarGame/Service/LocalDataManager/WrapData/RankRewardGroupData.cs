//来源表排行榜_Rank.xlsm.xlsx -> sheet:RankRewardGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RankRewardGroupData
	{
		[Key(0)]
		public Dictionary<int, RankRewardGroupDataCell> StaticRankRewardGroupDatas = new Dictionary<int, RankRewardGroupDataCell>();
	}
	[MessagePackObject]
	public class RankRewardGroupDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//排行榜ID
		[Key(1)]
		public int RankID;
		public int GetRankID()
		{
			return RankID;
		}
		//奖励条件
		[Key(2)]
		public int RewardType;
		public int GetRewardType()
		{
			return RewardType;
		}
		//奖励参数
		[Key(3)]
		public int RewardParam;
		public int GetRewardParam()
		{
			return RewardParam;
		}
		//奖励组
		[Key(4)]
		public int RewardGroup;
		public int GetRewardGroup()
		{
			return RewardGroup;
		}
	}
}
