//来源表FramEvent.xlsx -> sheet:FramEvent
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FramEventData
	{
		[Key(0)]
		public Dictionary<int, FramEventDataCell> StaticFramEventDatas = new Dictionary<int, FramEventDataCell>();
	}
	[MessagePackObject]
	public class FramEventDataCell
	{
		//帧事件id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//帧时间(ms)
		[Key(1)]
		public int FramTime;
		public int GetFramTime()
		{
			return FramTime;
		}
		//帧事件类型
		[Key(2)]
		public int FramType;
		public int GetFramType()
		{
			return FramType;
		}
		//帧事件值
		[Key(3)]
		public List<string> Value = new List<string>();
	}
}
