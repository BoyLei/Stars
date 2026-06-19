//来源表装备配置表_Equip.xlsm.xlsx -> sheet:EquipResetEvent
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipResetEventData
	{
		[Key(0)]
		public Dictionary<int, EquipResetEventDataCell> StaticEquipResetEventDatas = new Dictionary<int, EquipResetEventDataCell>();
	}
	[MessagePackObject]
	public class EquipResetEventDataCell
	{
		//事件ID
		[Key(0)]
		public int EventID;
		public int GetEventID()
		{
			return EventID;
		}
		//事件描述
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
