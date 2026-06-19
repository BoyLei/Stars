//来源表跑马灯_Marquee.xlsx -> sheet:Marquee
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MarqueeData
	{
		[Key(0)]
		public Dictionary<int, MarqueeDataCell> StaticMarqueeDatas = new Dictionary<int, MarqueeDataCell>();
	}
	[MessagePackObject]
	public class MarqueeDataCell
	{
		//跑马灯ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//是否副本内显示
		[Key(1)]
		public bool IsDungeon;
		public bool GetIsDungeon()
		{
			return IsDungeon;
		}
		//是否高优先级显示
		[Key(2)]
		public bool IsHighpriority;
		public bool GetIsHighpriority()
		{
			return IsHighpriority;
		}
		//跑马灯播放次数
		[Key(3)]
		public int Times;
		public int GetTimes()
		{
			return Times;
		}
		//是否序列置顶
		[Key(4)]
		public bool IsTop;
		public bool GetIsTop()
		{
			return IsTop;
		}
		//显示条件组
		[Key(5)]
		public int ShowCondition;
		public int GetShowCondition()
		{
			return ShowCondition;
		}
		//跑马灯内容Key
		[Key(6)]
		public string Content
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Content); }
			set { _Content = value; }
        }
		[IgnoreMember]
		private string _Content;
	}
}
