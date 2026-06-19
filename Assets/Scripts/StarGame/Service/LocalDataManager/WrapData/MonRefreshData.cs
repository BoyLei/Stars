//来源表MonRefreshData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class MonRefreshData
	{
		public Dictionary<int, MonRefreshDataCell> StaticMonRefreshDatas = new Dictionary<int, MonRefreshDataCell>();
	}
	public class MonRefreshDataCell
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
		public List<int> PlayMode = new List<int>();
		//支持地图
		public List<int> Maps = new List<int>();
		//小怪群落
		public List<int> Lackey = new List<int>();
		//精英群落
		public List<int> Elite = new List<int>();
		//boss群落
		public List<int> Boss = new List<int>();
		//小怪积分
		public string LackeyScore;
		private int _LackeyScore = -1;
		public int GetLackeyScore()
		{
			if (_LackeyScore == -1 && int.TryParse(LackeyScore, out _LackeyScore))
			{
			}
			return _LackeyScore;
		}
		//精英积分
		public string EliteScore;
		private int _EliteScore = -1;
		public int GetEliteScore()
		{
			if (_EliteScore == -1 && int.TryParse(EliteScore, out _EliteScore))
			{
			}
			return _EliteScore;
		}
		//boss积分
		public string BossScore;
		private int _BossScore = -1;
		public int GetBossScore()
		{
			if (_BossScore == -1 && int.TryParse(BossScore, out _BossScore))
			{
			}
			return _BossScore;
		}

	}
}
