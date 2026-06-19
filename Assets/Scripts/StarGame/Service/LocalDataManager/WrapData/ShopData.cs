//来源表商店配置表_Shop.xlsm.xlsx -> sheet:Shop
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ShopData
	{
		[Key(0)]
		public Dictionary<int, ShopDataCell> StaticShopDatas = new Dictionary<int, ShopDataCell>();
	}
	[MessagePackObject]
	public class ShopDataCell
	{
		//商店id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//商店类型
		[Key(1)]
		public int Shoptype;
		public int GetShoptype()
		{
			return Shoptype;
		}
		//商店名称多语言Key
		[Key(2)]
		public string Shop_name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Shop_name); }
			set { _Shop_name = value; }
        }
		[IgnoreMember]
		private string _Shop_name;
		//商店icon
		[Key(3)]
		public string Shop_icon;
		//子商店id
		[Key(4)]
		public List<int> Sub_shopid = new List<int>();
		//子商店名Key
		[Key(5)]
		public string Sub_shopname
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Sub_shopname); }
			set { _Sub_shopname = value; }
        }
		[IgnoreMember]
		private string _Sub_shopname;
		//商店货币
		[Key(6)]
		public List<int> Currency = new List<int>();
		//化身ID
		[Key(7)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//商店闲聊
		[Key(8)]
		public List<int> Dialog_id = new List<int>();
	}
}
