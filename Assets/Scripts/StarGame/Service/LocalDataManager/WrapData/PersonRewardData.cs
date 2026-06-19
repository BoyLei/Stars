//来源表PersonRewardData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class PersonRewardData
	{
		public Dictionary<int, PersonRewardDataCell> StaticPersonRewardDatas = new Dictionary<int, PersonRewardDataCell>();
	}
	public class PersonRewardDataCell
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
		//需求积分量
		public string ScoOneP;
		private int _ScoOneP = -1;
		public int GetScoOneP()
		{
			if (_ScoOneP == -1 && int.TryParse(ScoOneP, out _ScoOneP))
			{
			}
			return _ScoOneP;
		}
		//获取奖励1
		public List<int> Reward = new List<int>();
		//获取奖励1数量
		public List<int> Num = new List<int>();

	}
}
