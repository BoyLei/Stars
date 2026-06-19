//来源表在线奖励表_TimeReward.xlsx -> sheet:TimeRewardGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TimeRewardGroupData
	{
		[Key(0)]
		public Dictionary<int, TimeRewardGroupDataCell> StaticTimeRewardGroupDatas = new Dictionary<int, TimeRewardGroupDataCell>();
	}
	[MessagePackObject]
	public class TimeRewardGroupDataCell
	{
		//奖励组
		[Key(0)]
		public int RewardGroup;
		public int GetRewardGroup()
		{
			return RewardGroup;
		}
		//开服时间(天)
		[Key(1)]
		public int OpeningTime;
		public int GetOpeningTime()
		{
			return OpeningTime;
		}
	}
}
