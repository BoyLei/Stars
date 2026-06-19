//来源表Reward.xlsx -> sheet:Reward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RewardData
	{
		[Key(0)]
		public Dictionary<int, RewardDataCell> StaticRewardDatas = new Dictionary<int, RewardDataCell>();
	}
	[MessagePackObject]
	public class RewardDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//道具编号
		[Key(1)]
		public List<long> ItemCost = new List<long>();
		//道具数量
		[Key(2)]
		public List<long> ItemNum = new List<long>();
	}
}
