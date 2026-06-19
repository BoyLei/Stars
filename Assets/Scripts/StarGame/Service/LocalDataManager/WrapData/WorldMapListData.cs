//来源表SceneMap.xlsm.xlsx -> sheet:WorldMapList
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class WorldMapListData
	{
		[Key(0)]
		public Dictionary<int, WorldMapListDataCell> StaticWorldMapListDatas = new Dictionary<int, WorldMapListDataCell>();
	}
	[MessagePackObject]
	public class WorldMapListDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//地图ID
		[Key(1)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
		//地图所属国度
		[Key(2)]
		public int FromNation;
		public int GetFromNation()
		{
			return FromNation;
		}
		//排序优先级
		[Key(3)]
		public int SortPriority;
		public int GetSortPriority()
		{
			return SortPriority;
		}
	}
}
