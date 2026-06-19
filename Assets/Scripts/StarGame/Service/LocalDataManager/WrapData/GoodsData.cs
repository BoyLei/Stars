//来源表商店配置表_Shop.xlsm.xlsx -> sheet:Goods
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GoodsData
	{
		[Key(0)]
		public Dictionary<long, GoodsDataCell> StaticGoodsDatas = new Dictionary<long, GoodsDataCell>();
	}
	[MessagePackObject]
	public class GoodsDataCell
	{
		//序列
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//商店子id
		[Key(1)]
		public int Sub_shopid;
		public int GetSub_shopid()
		{
			return Sub_shopid;
		}
		//商品id
		[Key(2)]
		public long Item_id;
		public long GetItem_id()
		{
			return Item_id;
		}
		//商品数量
		[Key(3)]
		public int Item_num;
		public int GetItem_num()
		{
			return Item_num;
		}
		//购买条件
		[Key(4)]
		public List<int> Condition = new List<int>();
		//条件文本多语言Key
		[Key(5)]
		public string Condition_desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Condition_desc); }
			set { _Condition_desc = value; }
        }
		[IgnoreMember]
		private string _Condition_desc;
		//买价
		[Key(6)]
		public List<long> Price = new List<long>();
		//购买所需道具
		[Key(7)]
		public long CurrencyID;
		public long GetCurrencyID()
		{
			return CurrencyID;
		}
		//原价
		[Key(8)]
		public long OriginalPrice;
		public long GetOriginalPrice()
		{
			return OriginalPrice;
		}
		//现价
		[Key(9)]
		public long CurrentPrice;
		public long GetCurrentPrice()
		{
			return CurrentPrice;
		}
		//折扣率
		[Key(10)]
		public int Discount;
		public int GetDiscount()
		{
			return Discount;
		}
		//限售类型
		[Key(11)]
		public int Sell_type;
		public int GetSell_type()
		{
			return Sell_type;
		}
		//限售数量
		[Key(12)]
		public long Sell_num;
		public long GetSell_num()
		{
			return Sell_num;
		}
		//限购类型
		[Key(13)]
		public int Buy_type;
		public int GetBuy_type()
		{
			return Buy_type;
		}
		//限购次数
		[Key(14)]
		public long Buy_num;
		public long GetBuy_num()
		{
			return Buy_num;
		}
		//删除条件
		[Key(15)]
		public List<int> DeleteCondition = new List<int>();
	}
}
