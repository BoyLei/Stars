//来源表在线奖励表_TimeReward.xlsx -> sheet:TimeReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TimeRewardData
	{
		[Key(0)]
		public Dictionary<int, TimeRewardDataCell> StaticTimeRewardDatas = new Dictionary<int, TimeRewardDataCell>();
	}
	[MessagePackObject]
	public class TimeRewardDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//在线时长(分)
		[Key(1)]
		public int AccrueTime;
		public int GetAccrueTime()
		{
			return AccrueTime;
		}
		//奖励组
		[Key(2)]
		public int RewardGroup;
		public int GetRewardGroup()
		{
			return RewardGroup;
		}
		//奖励道具
		[Key(3)]
		public long Reward;
		public long GetReward()
		{
			return Reward;
		}
		//奖励数量
		[Key(4)]
		public long RewardNum;
		public long GetRewardNum()
		{
			return RewardNum;
		}
	}
}
