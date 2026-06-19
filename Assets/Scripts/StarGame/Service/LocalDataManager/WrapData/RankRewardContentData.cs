//来源表排行榜_Rank.xlsm.xlsx -> sheet:RankRewardContent
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RankRewardContentData
	{
		[Key(0)]
		public Dictionary<int, RankRewardContentDataCell> StaticRankRewardContentDatas = new Dictionary<int, RankRewardContentDataCell>();
	}
	[MessagePackObject]
	public class RankRewardContentDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//奖励组ID
		[Key(1)]
		public int RewardGroup;
		public int GetRewardGroup()
		{
			return RewardGroup;
		}
		//排名发奖人数下限
		[Key(2)]
		public int LowerLimmit;
		public int GetLowerLimmit()
		{
			return LowerLimmit;
		}
		//奖励道具
		[Key(3)]
		public List<long> RewardItem = new List<long>();
		//奖励数量
		[Key(4)]
		public List<long> RewardNum = new List<long>();
	}
}
