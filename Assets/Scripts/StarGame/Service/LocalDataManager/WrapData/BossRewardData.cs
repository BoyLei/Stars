//来源表BossRewardData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BossRewardData
	{
		public Dictionary<int, BossRewardDataCell> StaticBossRewardDatas = new Dictionary<int, BossRewardDataCell>();
	}
	public class BossRewardDataCell
	{
		//主Key
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//生效玩法ID
		public string PlayMode;
		private int _PlayMode = -1;
		public int GetPlayMode()
		{
			if (_PlayMode == -1 && int.TryParse(PlayMode, out _PlayMode))
			{
			}
			return _PlayMode;
		}
		//生效等级
		public string EffectLevel;
		private int _EffectLevel = -1;
		public int GetEffectLevel()
		{
			if (_EffectLevel == -1 && int.TryParse(EffectLevel, out _EffectLevel))
			{
			}
			return _EffectLevel;
		}
		//BOSS
		public string Boss;
		private long _Boss = -1;
		public long GetBoss()
		{
			if (_Boss == -1 && long.TryParse(Boss, out _Boss))
			{
			}
			return _Boss;
		}
		//排名
		public string Rank;
		private int _Rank = -1;
		public int GetRank()
		{
			if (_Rank == -1 && int.TryParse(Rank, out _Rank))
			{
			}
			return _Rank;
		}
		//奖励
		public List<int> Reward = new List<int>();
		//奖励数量
		public List<int> Num = new List<int>();

	}
}
