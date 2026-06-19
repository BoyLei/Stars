//来源表循环玩法开启运营表_RecyOperation.xlsm.xlsx -> sheet:RecyclePlay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RecyclePlayData
	{
		[Key(0)]
		public Dictionary<long, RecyclePlayDataCell> StaticRecyclePlayDatas = new Dictionary<long, RecyclePlayDataCell>();
	}
	[MessagePackObject]
	public class RecyclePlayDataCell
	{
		//主KEY
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//玩法名称
		[Key(1)]
		public string PlayName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PlayName); }
			set { _PlayName = value; }
        }
		[IgnoreMember]
		private string _PlayName;
		//描述
		[Key(2)]
		public string PlayDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PlayDesc); }
			set { _PlayDesc = value; }
        }
		[IgnoreMember]
		private string _PlayDesc;
		//功能介绍索引
		[Key(3)]
		public int IntroId;
		public int GetIntroId()
		{
			return IntroId;
		}
		//预览奖励
		[Key(4)]
		public List<long> AwardID = new List<long>();
		//玩法入口图
		[Key(5)]
		public string EntryPic;
		//玩法大图
		[Key(6)]
		public string KV;
		//玩法日历小图
		[Key(7)]
		public string CalendarSPic;
		//玩法内页场景数据
		[Key(8)]
		public string Scene;
		//开放日期
		[Key(9)]
		public List<string> DateList = new List<string>();
		//开放循环类型
		[Key(10)]
		public int LoopType;
		public int GetLoopType()
		{
			return LoopType;
		}
		//循环时间参数
		[Key(11)]
		public List<int> LoopParam = new List<int>();
		//开放时间组
		[Key(12)]
		public List<string> TimeList = new List<string>();
		//持续时间
		[Key(13)]
		public int MinuteNum;
		public int GetMinuteNum()
		{
			return MinuteNum;
		}
		//玩法类型
		[Key(14)]
		public int GameType;
		public int GetGameType()
		{
			return GameType;
		}
		//玩法对应表ID
		[Key(15)]
		public int GameID;
		public int GetGameID()
		{
			return GameID;
		}
	}
}
