//来源表TaskReward.xlsm.xlsx -> sheet:TaskReward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TaskRewardData
	{
		[Key(0)]
		public Dictionary<int, TaskRewardDataCell> StaticTaskRewardDatas = new Dictionary<int, TaskRewardDataCell>();
	}
	[MessagePackObject]
	public class TaskRewardDataCell
	{
		//ID
		[Key(0)]
		public int TaskID;
		public int GetTaskID()
		{
			return TaskID;
		}
		//任务奖励1-不弹出
		[Key(1)]
		public List<long> RewardHid = new List<long>();
		//职业奖励ID
		[Key(2)]
		public long RewardHid_job;
		public long GetRewardHid_job()
		{
			return RewardHid_job;
		}
		//任务奖励1
		[Key(3)]
		public List<long> Reward = new List<long>();
		//通用奖励显示
		[Key(4)]
		public List<int> RewardMark = new List<int>();
		//任务标签
		[Key(5)]
		public string Tips
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Tips); }
			set { _Tips = value; }
        }
		[IgnoreMember]
		private string _Tips;
	}
}
