//来源表护符养成表_Amulet.xlsx -> sheet:AmuletExtract
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AmuletExtractData
	{
		[Key(0)]
		public Dictionary<int, AmuletExtractDataCell> StaticAmuletExtractDatas = new Dictionary<int, AmuletExtractDataCell>();
	}
	[MessagePackObject]
	public class AmuletExtractDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//护符等级
		[Key(1)]
		public int AmuletLevel;
		public int GetAmuletLevel()
		{
			return AmuletLevel;
		}
		//护符品质
		[Key(2)]
		public int AmuletQuality;
		public int GetAmuletQuality()
		{
			return AmuletQuality;
		}
		//萃取次数（%以上）
		[Key(3)]
		public int ExtractNum;
		public int GetExtractNum()
		{
			return ExtractNum;
		}
		//基础货币消耗
		[Key(4)]
		public List<long> MoneyCost = new List<long>();
		//基础道具消耗
		[Key(5)]
		public List<long> ItemCost = new List<long>();
	}
}
