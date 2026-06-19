//来源表道具配置表_Item.xlsm.xlsx -> sheet:ItemType
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemTypeData
	{
		[Key(0)]
		public Dictionary<int, ItemTypeDataCell> StaticItemTypeDatas = new Dictionary<int, ItemTypeDataCell>();
	}
	[MessagePackObject]
	public class ItemTypeDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//道具类型
		[Key(1)]
		public int ItemType;
		public int GetItemType()
		{
			return ItemType;
		}
		//道具子类
		[Key(2)]
		public int SubType;
		public int GetSubType()
		{
			return SubType;
		}
		//类型名称
		[Key(3)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//交易行是否显示
		[Key(4)]
		public bool TradVisible;
		public bool GetTradVisible()
		{
			return TradVisible;
		}
		//拍卖行是否显示
		[Key(5)]
		public bool AuctionVisible;
		public bool GetAuctionVisible()
		{
			return AuctionVisible;
		}
	}
}
