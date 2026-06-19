//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEConstant
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEConstantData
	{
		[Key(0)]
		public Dictionary<int, GVEConstantDataCell> StaticGVEConstantDatas = new Dictionary<int, GVEConstantDataCell>();
	}
	[MessagePackObject]
	public class GVEConstantDataCell
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
		//获取拍卖分红需要的最低提交积分次数
		[Key(2)]
		public int BonusMinScore;
		public int GetBonusMinScore()
		{
			return BonusMinScore;
		}
		//公会最大boss领奖次数
		[Key(3)]
		public int MaxKillBossReward;
		public int GetMaxKillBossReward()
		{
			return MaxKillBossReward;
		}
		//公会最低进入boss排名伤害
		[Key(4)]
		public int RankMinDam;
		public int GetRankMinDam()
		{
			return RankMinDam;
		}
		//boss刷新初始delay
		[Key(5)]
		public int RefreshDelay;
		public int GetRefreshDelay()
		{
			return RefreshDelay;
		}
		//boss刷新间隔
		[Key(6)]
		public int RefreshIntervel;
		public int GetRefreshIntervel()
		{
			return RefreshIntervel;
		}
		//boss刷新总波次
		[Key(7)]
		public int RefreshWave;
		public int GetRefreshWave()
		{
			return RefreshWave;
		}
		//随机地图数
		[Key(8)]
		public int SelectMapNum;
		public int GetSelectMapNum()
		{
			return SelectMapNum;
		}
	}
}
