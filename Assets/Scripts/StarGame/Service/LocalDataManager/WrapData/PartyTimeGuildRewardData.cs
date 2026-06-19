//来源表派对时刻_PartyTime.xlsm.xlsx -> sheet:PartyTimeGuildReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartyTimeGuildRewardData
	{
		[Key(0)]
		public Dictionary<int, PartyTimeGuildRewardDataCell> StaticPartyTimeGuildRewardDatas = new Dictionary<int, PartyTimeGuildRewardDataCell>();
	}
	[MessagePackObject]
	public class PartyTimeGuildRewardDataCell
	{
		//奖励所需计数
		[Key(0)]
		public int Reward_need_num;
		public int GetReward_need_num()
		{
			return Reward_need_num;
		}
		//奖励1ID
		[Key(1)]
		public List<long> Reward = new List<long>();
		//奖励1数量
		[Key(2)]
		public List<long> RewardNum = new List<long>();
		//通用次数表ID
		[Key(3)]
		public int CmCountID;
		public int GetCmCountID()
		{
			return CmCountID;
		}
	}
}
