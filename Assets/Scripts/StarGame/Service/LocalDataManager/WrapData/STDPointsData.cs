//来源表七日目标表_SevenDaysTarget.xlsx -> sheet:STDPoints
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class STDPointsData
	{
		[Key(0)]
		public Dictionary<int, STDPointsDataCell> StaticSTDPointsDatas = new Dictionary<int, STDPointsDataCell>();
	}
	[MessagePackObject]
	public class STDPointsDataCell
	{
		//累计积分数量
		[Key(0)]
		public int PointsNum;
		public int GetPointsNum()
		{
			return PointsNum;
		}
		//累计奖励
		[Key(1)]
		public long Reward;
		public long GetReward()
		{
			return Reward;
		}
		//累计奖励数量
		[Key(2)]
		public long RewardNum;
		public long GetRewardNum()
		{
			return RewardNum;
		}
	}
}
