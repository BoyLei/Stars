//来源表通行证表_BattlePass.xlsx -> sheet:BPTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BPTaskData
	{
		[Key(0)]
		public Dictionary<int, BPTaskDataCell> StaticBPTaskDatas = new Dictionary<int, BPTaskDataCell>();
	}
	[MessagePackObject]
	public class BPTaskDataCell
	{
		//任务id
		[Key(0)]
		public int TaskID;
		public int GetTaskID()
		{
			return TaskID;
		}
		//任务周期
		[Key(1)]
		public int TaskCycle;
		public int GetTaskCycle()
		{
			return TaskCycle;
		}
		//功能跳转
		[Key(2)]
		public int JumpID;
		public int GetJumpID()
		{
			return JumpID;
		}
	}
}
