//来源表新手目标_BeginnerTarget.xlsx -> sheet:BeginnerTargetTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BeginnerTargetTaskData
	{
		[Key(0)]
		public Dictionary<int, BeginnerTargetTaskDataCell> StaticBeginnerTargetTaskDatas = new Dictionary<int, BeginnerTargetTaskDataCell>();
	}
	[MessagePackObject]
	public class BeginnerTargetTaskDataCell
	{
		//任务组
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//下一组任务ID
		[Key(1)]
		public int NextTaskGroup;
		public int GetNextTaskGroup()
		{
			return NextTaskGroup;
		}
		//任务组名
		[Key(2)]
		public string GroupName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_GroupName); }
			set { _GroupName = value; }
        }
		[IgnoreMember]
		private string _GroupName;
		//任务ID
		[Key(3)]
		public List<int> TaskID = new List<int>();
		//任务组奖励
		[Key(4)]
		public List<long> GroupReward = new List<long>();
		//奖励数量
		[Key(5)]
		public List<long> GroupRewardNum = new List<long>();
	}
}
