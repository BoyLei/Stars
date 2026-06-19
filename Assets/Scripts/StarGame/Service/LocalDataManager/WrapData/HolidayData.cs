//来源表Holiday.xlsx -> sheet:Holiday
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class HolidayData
	{
		[Key(0)]
		public Dictionary<int, HolidayDataCell> StaticHolidayDatas = new Dictionary<int, HolidayDataCell>();
	}
	[MessagePackObject]
	public class HolidayDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//日期
		[Key(1)]
		public string Date;
		//是否放假
		[Key(2)]
		public bool Holiday;
		public bool GetHoliday()
		{
			return Holiday;
		}
	}
}
