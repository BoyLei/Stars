//来源表签到表_SignIn.xlsx -> sheet:SignSeason
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SignSeasonData
	{
		[Key(0)]
		public Dictionary<int, SignSeasonDataCell> StaticSignSeasonDatas = new Dictionary<int, SignSeasonDataCell>();
	}
	[MessagePackObject]
	public class SignSeasonDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//签到周期
		[Key(1)]
		public int SeasonID;
		public int GetSeasonID()
		{
			return SeasonID;
		}
		//签到天数
		[Key(2)]
		public int SignNum;
		public int GetSignNum()
		{
			return SignNum;
		}
		//奖励
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
