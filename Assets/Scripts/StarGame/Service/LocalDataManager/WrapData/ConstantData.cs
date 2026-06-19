//来源表ConstantData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class ConstantData
	{
		public Dictionary<int, ConstantDataCell> StaticConstantDatas = new Dictionary<int, ConstantDataCell>();
	}
	public class ConstantDataCell
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
		//获取拍卖分红需要的最低提交积分次数
		public string BonusMinScore;
		private int _BonusMinScore = -1;
		public int GetBonusMinScore()
		{
			if (_BonusMinScore == -1 && int.TryParse(BonusMinScore, out _BonusMinScore))
			{
			}
			return _BonusMinScore;
		}
		//公会最大boss领奖次数
		public string MaxKillBossReward;
		private int _MaxKillBossReward = -1;
		public int GetMaxKillBossReward()
		{
			if (_MaxKillBossReward == -1 && int.TryParse(MaxKillBossReward, out _MaxKillBossReward))
			{
			}
			return _MaxKillBossReward;
		}
		//公会最低进入boss排名伤害
		public string RankMinDam;
		private int _RankMinDam = -1;
		public int GetRankMinDam()
		{
			if (_RankMinDam == -1 && int.TryParse(RankMinDam, out _RankMinDam))
			{
			}
			return _RankMinDam;
		}
		//boss刷新初始delay
		public string RefreshDelay;
		private int _RefreshDelay = -1;
		public int GetRefreshDelay()
		{
			if (_RefreshDelay == -1 && int.TryParse(RefreshDelay, out _RefreshDelay))
			{
			}
			return _RefreshDelay;
		}
		//boss刷新间隔
		public string RefreshIntervel;
		private int _RefreshIntervel = -1;
		public int GetRefreshIntervel()
		{
			if (_RefreshIntervel == -1 && int.TryParse(RefreshIntervel, out _RefreshIntervel))
			{
			}
			return _RefreshIntervel;
		}
		//boss刷新总波次
		public string RefreshWave;
		private int _RefreshWave = -1;
		public int GetRefreshWave()
		{
			if (_RefreshWave == -1 && int.TryParse(RefreshWave, out _RefreshWave))
			{
			}
			return _RefreshWave;
		}
		//随机地图数
		public string SelectMapNum;
		private int _SelectMapNum = -1;
		public int GetSelectMapNum()
		{
			if (_SelectMapNum == -1 && int.TryParse(SelectMapNum, out _SelectMapNum))
			{
			}
			return _SelectMapNum;
		}

	}
}
