//来源表环任务表_RingTask.xlsx -> sheet:RingTaskAward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RingTaskAwardData
	{
		[Key(0)]
		public Dictionary<int, RingTaskAwardDataCell> StaticRingTaskAwardDatas = new Dictionary<int, RingTaskAwardDataCell>();
	}
	[MessagePackObject]
	public class RingTaskAwardDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//额外奖励ID
		[Key(1)]
		public List<long> ExAwardID = new List<long>();
		//额外奖励数量
		[Key(2)]
		public List<long> ExAwardNum = new List<long>();
		//额外奖励包ID
		[Key(3)]
		public long ExtraAwardID;
		public long GetExtraAwardID()
		{
			return ExtraAwardID;
		}
		//出现权重
		[Key(4)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//保底次数
		[Key(5)]
		public int Guarantee;
		public int GetGuarantee()
		{
			return Guarantee;
		}
		//每月出现次数上限
		[Key(6)]
		public int MonthlTopLimit;
		public int GetMonthlTopLimit()
		{
			return MonthlTopLimit;
		}
		//奖励序号
		[Key(7)]
		public int AwardNumber;
		public int GetAwardNumber()
		{
			return AwardNumber;
		}
	}
}
