//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEGuildReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEGuildRewardData
	{
		[Key(0)]
		public Dictionary<int, GVEGuildRewardDataCell> StaticGVEGuildRewardDatas = new Dictionary<int, GVEGuildRewardDataCell>();
	}
	[MessagePackObject]
	public class GVEGuildRewardDataCell
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
		//需求积分量
		[Key(3)]
		public int ScoOneP;
		public int GetScoOneP()
		{
			return ScoOneP;
		}
		//获取奖励1
		[Key(4)]
		public List<long> Reward = new List<long>();
		//获取奖励1数量
		[Key(5)]
		public List<long> Num = new List<long>();
	}
}
