//来源表玩法开启表_PlayOpenConditions.xlsm.xlsx -> sheet:DailyPlayActivTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DailyPlayActivTaskData
	{
		[Key(0)]
		public Dictionary<int, DailyPlayActivTaskDataCell> StaticDailyPlayActivTaskDatas = new Dictionary<int, DailyPlayActivTaskDataCell>();
	}
	[MessagePackObject]
	public class DailyPlayActivTaskDataCell
	{
		//任务调用事件
		[Key(0)]
		public int TaskEvent;
		public int GetTaskEvent()
		{
			return TaskEvent;
		}
		//排序用字段
		[Key(1)]
		public int Sort;
		public int GetSort()
		{
			return Sort;
		}
		//最大完成进度
		[Key(2)]
		public int MaxCompProgress;
		public int GetMaxCompProgress()
		{
			return MaxCompProgress;
		}
		//每进度获得活跃度
		[Key(3)]
		public int OnceGetActiv;
		public int GetOnceGetActiv()
		{
			return OnceGetActiv;
		}
		//任务显示文本多语言Key
		[Key(4)]
		public string ShowText
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ShowText); }
			set { _ShowText = value; }
        }
		[IgnoreMember]
		private string _ShowText;
		//前往跳转
		[Key(5)]
		public int SystemJump;
		public int GetSystemJump()
		{
			return SystemJump;
		}
	}
}
