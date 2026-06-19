//来源表护符养成表_Amulet.xlsx -> sheet:AmuletEquipLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AmuletEquipLevelData
	{
		[Key(0)]
		public Dictionary<int, AmuletEquipLevelDataCell> StaticAmuletEquipLevelDatas = new Dictionary<int, AmuletEquipLevelDataCell>();
	}
	[MessagePackObject]
	public class AmuletEquipLevelDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//护符类型
		[Key(1)]
		public int AmuletType;
		public int GetAmuletType()
		{
			return AmuletType;
		}
		//护符等级
		[Key(2)]
		public int AmuletLevel;
		public int GetAmuletLevel()
		{
			return AmuletLevel;
		}
		//词条数量-下限
		[Key(3)]
		public int EntryNumMin;
		public int GetEntryNumMin()
		{
			return EntryNumMin;
		}
		//词条数量-上限
		[Key(4)]
		public int EntryNumMax;
		public int GetEntryNumMax()
		{
			return EntryNumMax;
		}
		//初始同词条限制
		[Key(5)]
		public int EntryLimit;
		public int GetEntryLimit()
		{
			return EntryLimit;
		}
		//可出现的纹章槽位类型
		[Key(6)]
		public List<int> HeraldryType = new List<int>();
		//槽位重置货币消耗
		[Key(7)]
		public List<long> MoneyCost = new List<long>();
		//槽位重置道具消耗
		[Key(8)]
		public List<long> ItemCost = new List<long>();
	}
}
