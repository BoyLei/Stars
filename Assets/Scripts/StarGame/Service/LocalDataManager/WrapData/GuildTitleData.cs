//来源表公会表_Guild.xlsm.xlsx -> sheet:GuildTitle
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuildTitleData
	{
		[Key(0)]
		public Dictionary<int, GuildTitleDataCell> StaticGuildTitleDatas = new Dictionary<int, GuildTitleDataCell>();
	}
	[MessagePackObject]
	public class GuildTitleDataCell
	{
		//职位ID
		[Key(0)]
		public int TitleID;
		public int GetTitleID()
		{
			return TitleID;
		}
		//职位名（在用）-多语言KEY
		[Key(1)]
		public string Comments
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Comments); }
			set { _Comments = value; }
        }
		[IgnoreMember]
		private string _Comments;
		//包含权限
		[Key(2)]
		public List<int> Power = new List<int>();
		//每日禁言次数
		[Key(3)]
		public int DailyMutedLimit;
		public int GetDailyMutedLimit()
		{
			return DailyMutedLimit;
		}
		//每日踢人次数
		[Key(4)]
		public int DailyLickLimit;
		public int GetDailyLickLimit()
		{
			return DailyLickLimit;
		}
		//默认人数限制
		[Key(5)]
		public int NumLimit;
		public int GetNumLimit()
		{
			return NumLimit;
		}
	}
}
