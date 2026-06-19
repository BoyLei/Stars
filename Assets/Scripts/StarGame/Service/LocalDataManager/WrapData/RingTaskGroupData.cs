//来源表环任务表_RingTask.xlsx -> sheet:RingTaskGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RingTaskGroupData
	{
		[Key(0)]
		public Dictionary<int, RingTaskGroupDataCell> StaticRingTaskGroupDatas = new Dictionary<int, RingTaskGroupDataCell>();
	}
	[MessagePackObject]
	public class RingTaskGroupDataCell
	{
		//任务库ID
		[Key(0)]
		public int CopyGroupID;
		public int GetCopyGroupID()
		{
			return CopyGroupID;
		}
		//解锁等级
		[Key(1)]
		public int UnlockLevl;
		public int GetUnlockLevl()
		{
			return UnlockLevl;
		}
		//额外奖励
		[Key(2)]
		public long ExtraAwardsID;
		public long GetExtraAwardsID()
		{
			return ExtraAwardsID;
		}
		//额外奖励展示
		[Key(3)]
		public List<int> AwardsDisplay = new List<int>();
		//额外奖励展示道具数量
		[Key(4)]
		public List<int> AwardsDisplayNum = new List<int>();
	}
}
