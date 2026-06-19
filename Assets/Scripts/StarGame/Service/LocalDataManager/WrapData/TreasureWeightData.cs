//来源表藏宝图表_Treasure.xlsm.xlsx -> sheet:TreasureWeight
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TreasureWeightData
	{
		[Key(0)]
		public Dictionary<int, TreasureWeightDataCell> StaticTreasureWeightDatas = new Dictionary<int, TreasureWeightDataCell>();
	}
	[MessagePackObject]
	public class TreasureWeightDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//玩家最大等级
		[Key(1)]
		public int LevelLimit;
		public int GetLevelLimit()
		{
			return LevelLimit;
		}
		//藏宝图类型
		[Key(2)]
		public int TreasureType;
		public int GetTreasureType()
		{
			return TreasureType;
		}
		//结果类型
		[Key(3)]
		public int ResultType;
		public int GetResultType()
		{
			return ResultType;
		}
		//出现权重
		[Key(4)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//对应关卡组
		[Key(5)]
		public int CopyGroup;
		public int GetCopyGroup()
		{
			return CopyGroup;
		}
		//对应怪物组
		[Key(6)]
		public List<int> MonGroup = new List<int>();
		//掉落包
		[Key(7)]
		public long AwardDrop;
		public long GetAwardDrop()
		{
			return AwardDrop;
		}
		//额外掉落类型
		[Key(8)]
		public int ExtraDropType;
		public int GetExtraDropType()
		{
			return ExtraDropType;
		}
		//额外掉落效果
		[Key(9)]
		public int ExtraDropValue;
		public int GetExtraDropValue()
		{
			return ExtraDropValue;
		}
	}
}
