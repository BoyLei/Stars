//来源表Cost.xlsx -> sheet:Cost
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CostData
	{
		[Key(0)]
		public Dictionary<long, CostDataCell> StaticCostDatas = new Dictionary<long, CostDataCell>();
	}
	[MessagePackObject]
	public class CostDataCell
	{
		//序号
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//消耗物品
		[Key(1)]
		public List<long> ItemCost = new List<long>();
		//消耗物品数量
		[Key(2)]
		public List<long> ItemNum = new List<long>();
	}
}
