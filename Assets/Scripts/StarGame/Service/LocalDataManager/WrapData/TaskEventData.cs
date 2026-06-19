//来源表TaskEventData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class TaskEventData
	{
		public Dictionary<int, TaskEventDataCell> StaticTaskEventDatas = new Dictionary<int, TaskEventDataCell>();
	}
	public class TaskEventDataCell
	{
		//任务事件
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//任务ID
		public string TaskID;
		private int _TaskID = -1;
		public int GetTaskID()
		{
			if (_TaskID == -1 && int.TryParse(TaskID, out _TaskID))
			{
			}
			return _TaskID;
		}
		//任务目标描述
		public string Desc;
		//任务类型枚举
		public string TaskType;
		private int _TaskType = -1;
		public int GetTaskType()
		{
			if (_TaskType == -1 && int.TryParse(TaskType, out _TaskType))
			{
			}
			return _TaskType;
		}
		//任务地点枚举
		public string SpaceEnum;
		private int _SpaceEnum = -1;
		public int GetSpaceEnum()
		{
			if (_SpaceEnum == -1 && int.TryParse(SpaceEnum, out _SpaceEnum))
			{
			}
			return _SpaceEnum;
		}
		//任务参数
		public string Param;
		//完成所需次数
		public string MaxNum;
		private int _MaxNum = -1;
		public int GetMaxNum()
		{
			if (_MaxNum == -1 && int.TryParse(MaxNum, out _MaxNum))
			{
			}
			return _MaxNum;
		}
		//地图服务
		public List<int> MapServer = new List<int>();

	}
}
