//来源表冒险等级表_AdvGrade.xlsm.xlsx -> sheet:AdvGradeExp
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AdvGradeExpData
	{
		[Key(0)]
		public Dictionary<int, AdvGradeExpDataCell> StaticAdvGradeExpDatas = new Dictionary<int, AdvGradeExpDataCell>();
	}
	[MessagePackObject]
	public class AdvGradeExpDataCell
	{
		//角色冒险等级
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//冒险等级段位（前端显示）
		[Key(1)]
		public string AdvClass
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AdvClass); }
			set { _AdvClass = value; }
        }
		[IgnoreMember]
		private string _AdvClass;
		//冒险等级序号
		[Key(2)]
		public int AdvOrder;
		public int GetAdvOrder()
		{
			return AdvOrder;
		}
		//升级所需冒险经验
		[Key(3)]
		public int Exp;
		public int GetExp()
		{
			return Exp;
		}
		//升级道具奖励ID
		[Key(4)]
		public List<long> AwardItem = new List<long>();
		//升级道具奖励数量
		[Key(5)]
		public List<long> AwardNum = new List<long>();
		//开启功能（前端显示）
		[Key(6)]
		public int FuncID;
		public int GetFuncID()
		{
			return FuncID;
		}
		//加成属性ID
		[Key(7)]
		public List<int> AttrID = new List<int>();
		//加成属性值
		[Key(8)]
		public List<int> AttrValue = new List<int>();
		//战力
		[Key(9)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
		//玩家等级上限
		[Key(10)]
		public int LevelLimit;
		public int GetLevelLimit()
		{
			return LevelLimit;
		}
		//是否需要突破
		[Key(11)]
		public bool IfBreak;
		public bool GetIfBreak()
		{
			return IfBreak;
		}
		//突破条件组ID
		[Key(12)]
		public int BreakConditionGroupID;
		public int GetBreakConditionGroupID()
		{
			return BreakConditionGroupID;
		}
		//突破任务ID
		[Key(13)]
		public int BreakTask;
		public int GetBreakTask()
		{
			return BreakTask;
		}
		//突破道具奖励ID
		[Key(14)]
		public List<long> BreakAward = new List<long>();
		//突破道具奖励数量
		[Key(15)]
		public List<long> BreakAwardNum = new List<long>();
	}
}
