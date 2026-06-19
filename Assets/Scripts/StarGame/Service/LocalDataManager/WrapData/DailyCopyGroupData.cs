//来源表单人日常本表_DailyCopy.xlsm.xlsx -> sheet:DailyCopyGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DailyCopyGroupData
	{
		[Key(0)]
		public Dictionary<int, DailyCopyGroupDataCell> StaticDailyCopyGroupDatas = new Dictionary<int, DailyCopyGroupDataCell>();
	}
	[MessagePackObject]
	public class DailyCopyGroupDataCell
	{
		//编号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//关卡id
		[Key(1)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//所属关卡组
		[Key(2)]
		public int CopyGroupID;
		public int GetCopyGroupID()
		{
			return CopyGroupID;
		}
		//权重
		[Key(3)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//每周出现次数上限
		[Key(4)]
		public int WeekLimit;
		public int GetWeekLimit()
		{
			return WeekLimit;
		}
		//第一波奖励
		[Key(5)]
		public long FirstAwards;
		public long GetFirstAwards()
		{
			return FirstAwards;
		}
		//第二波奖励
		[Key(6)]
		public long SecondAwards;
		public long GetSecondAwards()
		{
			return SecondAwards;
		}
		//第三波奖励
		[Key(7)]
		public long ThirdAwards;
		public long GetThirdAwards()
		{
			return ThirdAwards;
		}
		//首通特殊掉落
		[Key(8)]
		public long FirstSpecialReward;
		public long GetFirstSpecialReward()
		{
			return FirstSpecialReward;
		}
	}
}
