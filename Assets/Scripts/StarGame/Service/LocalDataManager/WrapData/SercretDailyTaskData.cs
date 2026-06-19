//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SercretDailyTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SercretDailyTaskData
	{
		[Key(0)]
		public Dictionary<int, SercretDailyTaskDataCell> StaticSercretDailyTaskDatas = new Dictionary<int, SercretDailyTaskDataCell>();
	}
	[MessagePackObject]
	public class SercretDailyTaskDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//任务组
		[Key(1)]
		public long TaskGroup;
		public long GetTaskGroup()
		{
			return TaskGroup;
		}
		//玩家等级
		[Key(2)]
		public long Level;
		public long GetLevel()
		{
			return Level;
		}
		//任务描述
		[Key(3)]
		public string TaskDes
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TaskDes); }
			set { _TaskDes = value; }
        }
		[IgnoreMember]
		private string _TaskDes;
		//目标类型
		[Key(4)]
		public int Type;
		public int GetType()
		{
			return Type;
		}
		//目标参数
		[Key(5)]
		public List<string> Paramas = new List<string>();
		//任务奖励1
		[Key(6)]
		public List<long> TaskReward = new List<long>();
		//任务奖励数量1
		[Key(7)]
		public List<long> TaskRewardNum = new List<long>();
	}
}
