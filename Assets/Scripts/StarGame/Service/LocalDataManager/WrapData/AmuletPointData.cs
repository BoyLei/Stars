//来源表护符养成表_Amulet.xlsx -> sheet:AmuletPoint
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AmuletPointData
	{
		[Key(0)]
		public Dictionary<int, AmuletPointDataCell> StaticAmuletPointDatas = new Dictionary<int, AmuletPointDataCell>();
	}
	[MessagePackObject]
	public class AmuletPointDataCell
	{
		//编号
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
		//点位序列
		[Key(3)]
		public int Point;
		public int GetPoint()
		{
			return Point;
		}
		//激活成功率
		[Key(4)]
		public int SuccessRate;
		public int GetSuccessRate()
		{
			return SuccessRate;
		}
		//常规货币消耗
		[Key(5)]
		public List<long> MoneyCost = new List<long>();
		//常规道具消耗
		[Key(6)]
		public List<long> ItemCost = new List<long>();
		//额外重置消耗
		[Key(7)]
		public List<long> ExtraCost = new List<long>();
		//分解返还
		[Key(8)]
		public long BackItem;
		public long GetBackItem()
		{
			return BackItem;
		}
	}
}
