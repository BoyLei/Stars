//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:StarExpend
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class StarExpendData
	{
		[Key(0)]
		public Dictionary<int, StarExpendDataCell> StaticStarExpendDatas = new Dictionary<int, StarExpendDataCell>();
	}
	[MessagePackObject]
	public class StarExpendDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//伙伴品质
		[Key(1)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//星级
		[Key(2)]
		public int Star;
		public int GetStar()
		{
			return Star;
		}
		//伙伴属性id
		[Key(3)]
		public List<int> Nature = new List<int>();
		//百分比
		[Key(4)]
		public List<int> Value = new List<int>();
		//升星提升
		[Key(5)]
		public int StarUp;
		public int GetStarUp()
		{
			return StarUp;
		}
		//升星道具数量
		[Key(6)]
		public long Item_num;
		public long GetItem_num()
		{
			return Item_num;
		}
		//战力
		[Key(7)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
