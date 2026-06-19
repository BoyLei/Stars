//来源表藏宝图表_Treasure.xlsm.xlsx -> sheet:TreasureMapGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TreasureMapGroupData
	{
		[Key(0)]
		public Dictionary<int, TreasureMapGroupDataCell> StaticTreasureMapGroupDatas = new Dictionary<int, TreasureMapGroupDataCell>();
	}
	[MessagePackObject]
	public class TreasureMapGroupDataCell
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
		//地图组
		[Key(2)]
		public int MapGroupID;
		public int GetMapGroupID()
		{
			return MapGroupID;
		}
		//地图组等级
		[Key(3)]
		public int TreasureMapGroupLevel;
		public int GetTreasureMapGroupLevel()
		{
			return TreasureMapGroupLevel;
		}
		//挖宝点交互物
		[Key(4)]
		public List<int> Interact = new List<int>();
	}
}
