//来源表道具配置表_Item.xlsm.xlsx -> sheet:Item
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemData
	{
		[Key(0)]
		public Dictionary<long, ItemDataCell> StaticItemDatas = new Dictionary<long, ItemDataCell>();
	}
	[MessagePackObject]
	public class ItemDataCell
	{
		//道具编号
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//映射道具
		[Key(1)]
		public long Dealid;
		public long GetDealid()
		{
			return Dealid;
		}
		//是否获取流通
		[Key(2)]
		public bool IsGetNotBinding;
		public bool GetIsGetNotBinding()
		{
			return IsGetNotBinding;
		}
		//道具类型
		[Key(3)]
		public int ItemType;
		public int GetItemType()
		{
			return ItemType;
		}
		//道具子类
		[Key(4)]
		public int SubType;
		public int GetSubType()
		{
			return SubType;
		}
		//道具品质
		[Key(5)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//道具名字
		[Key(6)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//道具描述
		[Key(7)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//道具图标
		[Key(8)]
		public string Icon;
		//绑定类型
		[Key(9)]
		public int BindingType;
		public int GetBindingType()
		{
			return BindingType;
		}
		//存放背包
		[Key(10)]
		public int SpaceId;
		public int GetSpaceId()
		{
			return SpaceId;
		}
		//是否可堆叠
		[Key(11)]
		public bool CanOverlay;
		public bool GetCanOverlay()
		{
			return CanOverlay;
		}
		//是否可售卖
		[Key(12)]
		public int CanSell;
		public int GetCanSell()
		{
			return CanSell;
		}
		//是否丢弃
		[Key(13)]
		public int Discard;
		public int GetDiscard()
		{
			return Discard;
		}
		//拾取类型
		[Key(14)]
		public int PickType;
		public int GetPickType()
		{
			return PickType;
		}
		//拾取距离
		[Key(15)]
		public int PickRange;
		public int GetPickRange()
		{
			return PickRange;
		}
		//化身ID
		[Key(16)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//是否支持批量
		[Key(17)]
		public bool CanUse;
		public bool GetCanUse()
		{
			return CanUse;
		}
		//使用ID
		[Key(18)]
		public long UseID;
		public long GetUseID()
		{
			return UseID;
		}
		//时效
		[Key(19)]
		public int LiveTime;
		public int GetLiveTime()
		{
			return LiveTime;
		}
		//道具排序
		[Key(20)]
		public int SortProp;
		public int GetSortProp()
		{
			return SortProp;
		}
		//获取来源
		[Key(21)]
		public List<int> Source = new List<int>();
		//分解返还
		[Key(22)]
		public long BackItem;
		public long GetBackItem()
		{
			return BackItem;
		}
		//道具跳转
		[Key(23)]
		public List<int> UseJump = new List<int>();
		//保底分红价格
		[Key(24)]
		public int MinDivvy;
		public int GetMinDivvy()
		{
			return MinDivvy;
		}
		//公会拍卖起拍价
		[Key(25)]
		public long GuildAuctionPrice;
		public long GetGuildAuctionPrice()
		{
			return GuildAuctionPrice;
		}
		//世界拍卖起拍价
		[Key(26)]
		public long WorldAuctionPrice;
		public long GetWorldAuctionPrice()
		{
			return WorldAuctionPrice;
		}
		//单场流拍数量上限
		[Key(27)]
		public int PassInLimit;
		public int GetPassInLimit()
		{
			return PassInLimit;
		}
		//拍卖一组数量
		[Key(28)]
		public long AuctionSetNum;
		public long GetAuctionSetNum()
		{
			return AuctionSetNum;
		}
		//是否使用/培养绑定
		[Key(29)]
		public bool IsUseBinding;
		public bool GetIsUseBinding()
		{
			return IsUseBinding;
		}
		//是否可交易
		[Key(30)]
		public bool IsTradable;
		public bool GetIsTradable()
		{
			return IsTradable;
		}
		//最大可交易次数
		[Key(31)]
		public int MaxTradeCount;
		public int GetMaxTradeCount()
		{
			return MaxTradeCount;
		}
		//交易行显示等级
		[Key(32)]
		public int TradeDisplayLv;
		public int GetTradeDisplayLv()
		{
			return TradeDisplayLv;
		}
		//道具价值
		[Key(33)]
		public long InitialPrice;
		public long GetInitialPrice()
		{
			return InitialPrice;
		}
		//上限比率
		[Key(34)]
		public int UpperLimit;
		public int GetUpperLimit()
		{
			return UpperLimit;
		}
		//下限比率
		[Key(35)]
		public int LowerLimit;
		public int GetLowerLimit()
		{
			return LowerLimit;
		}
	}
}
