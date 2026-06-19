//来源表七日目标表_SevenDaysTarget.xlsx -> sheet:STDTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class STDTaskData
	{
		[Key(0)]
		public Dictionary<int, STDTaskDataCell> StaticSTDTaskDatas = new Dictionary<int, STDTaskDataCell>();
	}
	[MessagePackObject]
	public class STDTaskDataCell
	{
		//任务id
		[Key(0)]
		public int TaskID;
		public int GetTaskID()
		{
			return TaskID;
		}
		//任务日期
		[Key(1)]
		public int TaskDay;
		public int GetTaskDay()
		{
			return TaskDay;
		}
		//功能跳转
		[Key(2)]
		public int JumpID;
		public int GetJumpID()
		{
			return JumpID;
		}
		//功能跳转参数
		[Key(3)]
		public int JumpID_Args;
		public int GetJumpID_Args()
		{
			return JumpID_Args;
		}
	}
}
