//来源表竞技场表_Arena.xlsx -> sheet:ArenaBuy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ArenaBuyData
	{
		[Key(0)]
		public Dictionary<int, ArenaBuyDataCell> StaticArenaBuyDatas = new Dictionary<int, ArenaBuyDataCell>();
	}
	[MessagePackObject]
	public class ArenaBuyDataCell
	{
		//付费购买次数
		[Key(0)]
		public int BuyLimit;
		public int GetBuyLimit()
		{
			return BuyLimit;
		}
		//消耗道具数量
		[Key(1)]
		public int CostNum;
		public int GetCostNum()
		{
			return CostNum;
		}
	}
}
