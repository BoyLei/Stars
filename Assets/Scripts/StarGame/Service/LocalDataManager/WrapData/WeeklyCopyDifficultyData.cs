//来源表周常挑战本_WeeklyCopy.xlsm.xlsx -> sheet:WeeklyCopyDifficulty
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class WeeklyCopyDifficultyData
	{
		[Key(0)]
		public Dictionary<int, WeeklyCopyDifficultyDataCell> StaticWeeklyCopyDifficultyDatas = new Dictionary<int, WeeklyCopyDifficultyDataCell>();
	}
	[MessagePackObject]
	public class WeeklyCopyDifficultyDataCell
	{
		//难度ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//难度类型
		[Key(1)]
		public int DifficultyType;
		public int GetDifficultyType()
		{
			return DifficultyType;
		}
		//所属副本ID
		[Key(2)]
		public int CopyID;
		public int GetCopyID()
		{
			return CopyID;
		}
		//关卡id
		[Key(3)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//解锁条件（冒险等级）
		[Key(4)]
		public int UnlockConditions;
		public int GetUnlockConditions()
		{
			return UnlockConditions;
		}
		//常规奖励展示
		[Key(5)]
		public List<int> BaseAwardsShow = new List<int>();
		//常规奖励（掉落包）
		[Key(6)]
		public long BaseAwards;
		public long GetBaseAwards()
		{
			return BaseAwards;
		}
		//首通奖励1
		[Key(7)]
		public List<long> FirstReward = new List<long>();
		//首通奖励数量1
		[Key(8)]
		public List<long> FirstRewardNum = new List<long>();
	}
}
