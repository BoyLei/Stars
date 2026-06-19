//来源表玩法开启表_PlayOpenConditions.xlsm.xlsx -> sheet:DailyPlayActivReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DailyPlayActivRewardData
	{
		[Key(0)]
		public Dictionary<int, DailyPlayActivRewardDataCell> StaticDailyPlayActivRewardDatas = new Dictionary<int, DailyPlayActivRewardDataCell>();
	}
	[MessagePackObject]
	public class DailyPlayActivRewardDataCell
	{
		//需求活跃度
		[Key(0)]
		public int NeedActiv;
		public int GetNeedActiv()
		{
			return NeedActiv;
		}
		//奖励1
		[Key(1)]
		public List<long> Reward = new List<long>();
		//奖励1数量
		[Key(2)]
		public List<long> RewardNum = new List<long>();
	}
}
