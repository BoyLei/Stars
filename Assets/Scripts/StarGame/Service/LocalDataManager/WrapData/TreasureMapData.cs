//来源表藏宝图表_Treasure.xlsm.xlsx -> sheet:TreasureMap
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TreasureMapData
	{
		[Key(0)]
		public Dictionary<int, TreasureMapDataCell> StaticTreasureMapDatas = new Dictionary<int, TreasureMapDataCell>();
	}
	[MessagePackObject]
	public class TreasureMapDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//玩家最大等级
		[Key(1)]
		public int LevelLimit;
		public int GetLevelLimit()
		{
			return LevelLimit;
		}
		//藏宝图类型
		[Key(2)]
		public int TreasureType;
		public int GetTreasureType()
		{
			return TreasureType;
		}
		//地图组ID
		[Key(3)]
		public int MapGroup;
		public int GetMapGroup()
		{
			return MapGroup;
		}
	}
}
