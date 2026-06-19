//来源表冒险等级表_AdvGrade.xlsm.xlsx -> sheet:OpeningTime
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class OpeningTimeData
	{
		[Key(0)]
		public Dictionary<int, OpeningTimeDataCell> StaticOpeningTimeDatas = new Dictionary<int, OpeningTimeDataCell>();
	}
	[MessagePackObject]
	public class OpeningTimeDataCell
	{
		//开服天数
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//冒险经验获取提升百分比
		[Key(1)]
		public int AdvGradeExpAddition;
		public int GetAdvGradeExpAddition()
		{
			return AdvGradeExpAddition;
		}
		//每日冒险经验获取上限
		[Key(2)]
		public int AdvGradeExpLimit;
		public int GetAdvGradeExpLimit()
		{
			return AdvGradeExpLimit;
		}
	}
}
