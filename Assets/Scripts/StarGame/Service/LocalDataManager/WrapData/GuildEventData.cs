//来源表公会表_Guild.xlsm.xlsx -> sheet:GuildEvent
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuildEventData
	{
		[Key(0)]
		public Dictionary<int, GuildEventDataCell> StaticGuildEventDatas = new Dictionary<int, GuildEventDataCell>();
	}
	[MessagePackObject]
	public class GuildEventDataCell
	{
		//事件ID
		[Key(0)]
		public int EventID;
		public int GetEventID()
		{
			return EventID;
		}
		//事件描述-多语言Key
		[Key(1)]
		public string EventDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_EventDec); }
			set { _EventDec = value; }
        }
		[IgnoreMember]
		private string _EventDec;
	}
}
