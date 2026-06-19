//来源表单服副本_Level.xlsm.xlsx -> sheet:EctypeTarget
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EctypeTargetData
	{
		[Key(0)]
		public Dictionary<int, EctypeTargetDataCell> StaticEctypeTargetDatas = new Dictionary<int, EctypeTargetDataCell>();
	}
	[MessagePackObject]
	public class EctypeTargetDataCell
	{
		//索引id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//副本目标id
		[Key(1)]
		public int GoalId;
		public int GetGoalId()
		{
			return GoalId;
		}
		//副本类型
		[Key(2)]
		public int ChapterType;
		public int GetChapterType()
		{
			return ChapterType;
		}
		//目标类型
		[Key(3)]
		public int GoalType;
		public int GetGoalType()
		{
			return GoalType;
		}
		//目标标题
		[Key(4)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//目标内容
		[Key(5)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//目标参数
		[Key(6)]
		public long Param;
		public long GetParam()
		{
			return Param;
		}
		//寻路点
		[Key(7)]
		public int Spid;
		public int GetSpid()
		{
			return Spid;
		}
		//完成所需次数
		[Key(8)]
		public int MaxNum;
		public int GetMaxNum()
		{
			return MaxNum;
		}
		//是否显示指引路线
		[Key(9)]
		public bool Road;
		public bool GetRoad()
		{
			return Road;
		}
		//合并目标
		[Key(10)]
		public int TargetGroup;
		public int GetTargetGroup()
		{
			return TargetGroup;
		}
		//进入重置CD
		[Key(11)]
		public bool EnterCleanCD;
		public bool GetEnterCleanCD()
		{
			return EnterCleanCD;
		}
		//退出重置CD
		[Key(12)]
		public bool ExitCleanCD;
		public bool GetExitCleanCD()
		{
			return ExitCleanCD;
		}
	}
}
