//来源表成就表_Achieve.xlsx -> sheet:Achieve
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AchieveData
	{
		[Key(0)]
		public Dictionary<int, AchieveDataCell> StaticAchieveDatas = new Dictionary<int, AchieveDataCell>();
	}
	[MessagePackObject]
	public class AchieveDataCell
	{
		//成就ID
		[Key(0)]
		public int AchieveID;
		public int GetAchieveID()
		{
			return AchieveID;
		}
		//目标参数
		[Key(1)]
		public List<long> GoalParam = new List<long>();
		//目标类型
		[Key(2)]
		public int Goal;
		public int GetGoal()
		{
			return Goal;
		}
		//成就系统界面是否显示
		[Key(3)]
		public bool IsShow;
		public bool GetIsShow()
		{
			return IsShow;
		}
		//跟随活动时间开放
		[Key(4)]
		public int ByActTimeOpen;
		public int GetByActTimeOpen()
		{
			return ByActTimeOpen;
		}
	}
}
