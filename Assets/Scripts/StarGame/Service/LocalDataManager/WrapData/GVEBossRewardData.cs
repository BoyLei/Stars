//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEBossReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEBossRewardData
	{
		[Key(0)]
		public Dictionary<int, GVEBossRewardDataCell> StaticGVEBossRewardDatas = new Dictionary<int, GVEBossRewardDataCell>();
	}
	[MessagePackObject]
	public class GVEBossRewardDataCell
	{
		//主Key
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//生效玩法ID
		[Key(1)]
		public int PlayMode;
		public int GetPlayMode()
		{
			return PlayMode;
		}
		//生效等级
		[Key(2)]
		public int EffectLevel;
		public int GetEffectLevel()
		{
			return EffectLevel;
		}
		//BOSS
		[Key(3)]
		public long Boss;
		public long GetBoss()
		{
			return Boss;
		}
		//排名
		[Key(4)]
		public long Rank;
		public long GetRank()
		{
			return Rank;
		}
		//奖励掉落包
		[Key(5)]
		public long AwardDrop;
		public long GetAwardDrop()
		{
			return AwardDrop;
		}
		//奖励
		[Key(6)]
		public List<long> Reward = new List<long>();
	}
}
