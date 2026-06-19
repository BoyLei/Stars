//来源表单人日常本表_DailyCopy.xlsm.xlsx -> sheet:DailyCopyDifficulty
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DailyCopyDifficultyData
	{
		[Key(0)]
		public Dictionary<int, DailyCopyDifficultyDataCell> StaticDailyCopyDifficultyDatas = new Dictionary<int, DailyCopyDifficultyDataCell>();
	}
	[MessagePackObject]
	public class DailyCopyDifficultyDataCell
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
		//对应关卡组
		[Key(3)]
		public int CopyGroupID;
		public int GetCopyGroupID()
		{
			return CopyGroupID;
		}
		//解锁条件
		[Key(4)]
		public int UnlockConditions;
		public int GetUnlockConditions()
		{
			return UnlockConditions;
		}
		//常规奖励展示
		[Key(5)]
		public List<int> BaseAwardsShow = new List<int>();
		//首通奖励1
		[Key(6)]
		public List<long> FirstReward = new List<long>();
		//首通奖励数量1
		[Key(7)]
		public List<long> FirstRewardNum = new List<long>();
		//副本背景图资源
		[Key(8)]
		public string BG;
	}
}
