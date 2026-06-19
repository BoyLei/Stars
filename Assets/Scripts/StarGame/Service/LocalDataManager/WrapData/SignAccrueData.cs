//来源表签到表_SignIn.xlsx -> sheet:SignAccrue
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SignAccrueData
	{
		[Key(0)]
		public Dictionary<int, SignAccrueDataCell> StaticSignAccrueDatas = new Dictionary<int, SignAccrueDataCell>();
	}
	[MessagePackObject]
	public class SignAccrueDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//累计签到月份
		[Key(1)]
		public int SeasonID;
		public int GetSeasonID()
		{
			return SeasonID;
		}
		//累计签到天数
		[Key(2)]
		public int AccumulatedNum;
		public int GetAccumulatedNum()
		{
			return AccumulatedNum;
		}
		//累计奖励
		[Key(3)]
		public long Reward;
		public long GetReward()
		{
			return Reward;
		}
		//累计奖励数量
		[Key(4)]
		public long RewardNum;
		public long GetRewardNum()
		{
			return RewardNum;
		}
	}
}
