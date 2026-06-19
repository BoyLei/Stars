//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:MapBev
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MapBevData
	{
		[Key(0)]
		public Dictionary<int, MapBevDataCell> StaticMapBevDatas = new Dictionary<int, MapBevDataCell>();
	}
	[MessagePackObject]
	public class MapBevDataCell
	{
		//唯一键
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//场景
		[Key(1)]
		public int Map;
		public int GetMap()
		{
			return Map;
		}
		//层数
		[Key(2)]
		public int Floor;
		public int GetFloor()
		{
			return Floor;
		}
		//主行为树
		[Key(3)]
		public string MainBev;
		//事件行为树
		[Key(4)]
		public string EventBev;
	}
}
