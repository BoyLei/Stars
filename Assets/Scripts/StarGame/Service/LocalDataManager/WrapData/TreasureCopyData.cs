//来源表藏宝图表_Treasure.xlsm.xlsx -> sheet:TreasureCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TreasureCopyData
	{
		[Key(0)]
		public Dictionary<int, TreasureCopyDataCell> StaticTreasureCopyDatas = new Dictionary<int, TreasureCopyDataCell>();
	}
	[MessagePackObject]
	public class TreasureCopyDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//关卡ID
		[Key(1)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//场景ID
		[Key(2)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
		//关卡组
		[Key(3)]
		public int CopyGroup;
		public int GetCopyGroup()
		{
			return CopyGroup;
		}
	}
}
