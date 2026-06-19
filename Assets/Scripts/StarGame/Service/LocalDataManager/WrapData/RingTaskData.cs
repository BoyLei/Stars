//来源表环任务表_RingTask.xlsx -> sheet:RingTask
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RingTaskData
	{
		[Key(0)]
		public Dictionary<int, RingTaskDataCell> StaticRingTaskDatas = new Dictionary<int, RingTaskDataCell>();
	}
	[MessagePackObject]
	public class RingTaskDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//任务id
		[Key(1)]
		public int TaskID;
		public int GetTaskID()
		{
			return TaskID;
		}
		//所属任务库
		[Key(2)]
		public int TaskGroupID;
		public int GetTaskGroupID()
		{
			return TaskGroupID;
		}
		//每周出现次数上限
		[Key(3)]
		public int WeekLimit;
		public int GetWeekLimit()
		{
			return WeekLimit;
		}
		//是否必刷
		[Key(4)]
		public bool SurelyCreate;
		public bool GetSurelyCreate()
		{
			return SurelyCreate;
		}
		//额外奖励展示
		[Key(5)]
		public List<int> AwardsDisplay = new List<int>();
		//NPC
		[Key(6)]
		public int Npc;
		public int GetNpc()
		{
			return Npc;
		}
		//环任务开场白
		[Key(7)]
		public string RingTask_Preamble
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RingTask_Preamble); }
			set { _RingTask_Preamble = value; }
        }
		[IgnoreMember]
		private string _RingTask_Preamble;
		//环任务结束语
		[Key(8)]
		public string RingTask_Peroration
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RingTask_Peroration); }
			set { _RingTask_Peroration = value; }
        }
		[IgnoreMember]
		private string _RingTask_Peroration;
		//额外奖励展示道具数量
		[Key(9)]
		public List<int> AwardsDisplayNum = new List<int>();
	}
}
