//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:Qualifications
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class QualificationsData
	{
		[Key(0)]
		public Dictionary<int, QualificationsDataCell> StaticQualificationsDatas = new Dictionary<int, QualificationsDataCell>();
	}
	[MessagePackObject]
	public class QualificationsDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//资质组ID
		[Key(1)]
		public int QualificationID;
		public int GetQualificationID()
		{
			return QualificationID;
		}
		//突破阶段
		[Key(2)]
		public int BreakStep;
		public int GetBreakStep()
		{
			return BreakStep;
		}
		//星级要求
		[Key(3)]
		public int Star;
		public int GetStar()
		{
			return Star;
		}
		//资质id
		[Key(4)]
		public List<int> QualiFications = new List<int>();
		//初始值
		[Key(5)]
		public List<int> Value = new List<int>();
		//上限值
		[Key(6)]
		public List<int> Value_max = new List<int>();
		//货币消耗（突破）
		[Key(7)]
		public List<long> Money = new List<long>();
		//道具消耗（突破）
		[Key(8)]
		public List<long> Item_id = new List<long>();
		//道具数量（突破）
		[Key(9)]
		public List<long> Item_num = new List<long>();
		//伙伴等级限制
		[Key(10)]
		public int LevelGrade;
		public int GetLevelGrade()
		{
			return LevelGrade;
		}
	}
}
