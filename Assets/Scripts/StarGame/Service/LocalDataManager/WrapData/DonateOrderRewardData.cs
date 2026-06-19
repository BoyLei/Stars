//来源表公会捐献_GuildDonate.xlsm.xlsx -> sheet:DonateOrderReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DonateOrderRewardData
	{
		[Key(0)]
		public Dictionary<int, DonateOrderRewardDataCell> StaticDonateOrderRewardDatas = new Dictionary<int, DonateOrderRewardDataCell>();
	}
	[MessagePackObject]
	public class DonateOrderRewardDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//完成订单数量
		[Key(1)]
		public int ComOrderNum;
		public int GetComOrderNum()
		{
			return ComOrderNum;
		}
		//奖励类型
		[Key(2)]
		public int RewardType;
		public int GetRewardType()
		{
			return RewardType;
		}
		//冒险等级下限
		[Key(3)]
		public int AdvantureLvLimit;
		public int GetAdvantureLvLimit()
		{
			return AdvantureLvLimit;
		}
		//奖励
		[Key(4)]
		public List<long> Reward = new List<long>();
		//奖励数量
		[Key(5)]
		public List<long> RewardNum = new List<long>();
		//角色等级下限
		[Key(6)]
		public int LvLimit;
		public int GetLvLimit()
		{
			return LvLimit;
		}
	}
}
