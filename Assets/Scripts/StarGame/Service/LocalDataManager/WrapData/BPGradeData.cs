//来源表通行证表_BattlePass.xlsx -> sheet:BPGrade
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BPGradeData
	{
		[Key(0)]
		public Dictionary<int, BPGradeDataCell> StaticBPGradeDatas = new Dictionary<int, BPGradeDataCell>();
	}
	[MessagePackObject]
	public class BPGradeDataCell
	{
		//唯一主键
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//通行证等级
		[Key(1)]
		public int BPGrade;
		public int GetBPGrade()
		{
			return BPGrade;
		}
		//升级消耗经验
		[Key(2)]
		public int UpGradeCost;
		public int GetUpGradeCost()
		{
			return UpGradeCost;
		}
		//奖励组ID
		[Key(3)]
		public int AwardGroup;
		public int GetAwardGroup()
		{
			return AwardGroup;
		}
		//免费奖励1
		[Key(4)]
		public long FreeAward;
		public long GetFreeAward()
		{
			return FreeAward;
		}
		//免费奖励数量1
		[Key(5)]
		public long FreeAwardNum;
		public long GetFreeAwardNum()
		{
			return FreeAwardNum;
		}
		//付费奖励1
		[Key(6)]
		public List<long> PayAward = new List<long>();
		//付费奖励数量1
		[Key(7)]
		public List<long> PayAwardNum = new List<long>();
	}
}
