//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SeasonSecret
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SeasonSecretData
	{
		[Key(0)]
		public Dictionary<int, SeasonSecretDataCell> StaticSeasonSecretDatas = new Dictionary<int, SeasonSecretDataCell>();
	}
	[MessagePackObject]
	public class SeasonSecretDataCell
	{
		//赛季ID
		[Key(0)]
		public int SeasonID;
		public int GetSeasonID()
		{
			return SeasonID;
		}
		//可挑战层数上限
		[Key(1)]
		public int LevelLimit;
		public int GetLevelLimit()
		{
			return LevelLimit;
		}
		//排行奖励组ID
		[Key(2)]
		public long RankAward;
		public long GetRankAward()
		{
			return RankAward;
		}
		//当前赛季起始层数
		[Key(3)]
		public int CurrentSeasonLevel;
		public int GetCurrentSeasonLevel()
		{
			return CurrentSeasonLevel;
		}
	}
}
