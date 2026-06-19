//来源表商业化_Commerce.xlsm.xlsx -> sheet:ExternalPay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ExternalPayData
	{
		[Key(0)]
		public Dictionary<int, ExternalPayDataCell> StaticExternalPayDatas = new Dictionary<int, ExternalPayDataCell>();
	}
	[MessagePackObject]
	public class ExternalPayDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//支付类型
		[Key(1)]
		public int PayType;
		public int GetPayType()
		{
			return PayType;
		}
		//支付价格
		[Key(2)]
		public int Price;
		public int GetPrice()
		{
			return Price;
		}
		//获取道具
		[Key(3)]
		public List<long> ItemID = new List<long>();
		//道具数量
		[Key(4)]
		public List<int> ItemNum = new List<int>();
	}
}
