//来源表通用次数配置_CommonCount.xlsx -> sheet:CommonCount
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CommonCountData
	{
		[Key(0)]
		public Dictionary<int, CommonCountDataCell> StaticCommonCountDatas = new Dictionary<int, CommonCountDataCell>();
	}
	[MessagePackObject]
	public class CommonCountDataCell
	{
		//模板id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//限制方式
		[Key(1)]
		public int LimitType;
		public int GetLimitType()
		{
			return LimitType;
		}
		//数量上限
		[Key(2)]
		public int Max_count;
		public int GetMax_count()
		{
			return Max_count;
		}
		//错误提示文本
		[Key(3)]
		public string Error_prompt;
		//次数清空日期（1-28）
		[Key(4)]
		public int Clear_count_day;
		public int GetClear_count_day()
		{
			return Clear_count_day;
		}
		//次数清空星期（1-7）
		[Key(5)]
		public int Clear_count_weekday;
		public int GetClear_count_weekday()
		{
			return Clear_count_weekday;
		}
		//次数清空小时（0-23）
		[Key(6)]
		public int Clear_count_hour;
		public int GetClear_count_hour()
		{
			return Clear_count_hour;
		}
		//次数清空分钟（0-59）
		[Key(7)]
		public int Clear_count_minute;
		public int GetClear_count_minute()
		{
			return Clear_count_minute;
		}
		//玩法类型
		[Key(8)]
		public int GameplayType;
		public int GetGameplayType()
		{
			return GameplayType;
		}
		//总次数限制，初始数量上限与道具恢复次数和的上限
		[Key(9)]
		public int Total_max_count;
		public int GetTotal_max_count()
		{
			return Total_max_count;
		}
		//恢复道具达到上限错误提示文本
		[Key(10)]
		public string Item_max_error_prompt;
		//事件重置类型
		[Key(11)]
		public int Reset_event_type;
		public int GetReset_event_type()
		{
			return Reset_event_type;
		}
		//是否每日重置
		[Key(12)]
		public int DailyReset;
		public int GetDailyReset()
		{
			return DailyReset;
		}
	}
}
