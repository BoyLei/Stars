//来源表背包配置表_ItemSpace.xlsm.xlsx -> sheet:CurrencyCfg
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CurrencyCfgData
	{
		[Key(0)]
		public Dictionary<long, CurrencyCfgDataCell> StaticCurrencyCfgDatas = new Dictionary<long, CurrencyCfgDataCell>();
	}
	[MessagePackObject]
	public class CurrencyCfgDataCell
	{
		//道具ID
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//关联系统开放
		[Key(1)]
		public int SysOpen;
		public int GetSysOpen()
		{
			return SysOpen;
		}
		//是否在全部货币显示
		[Key(2)]
		public bool IsShowAllCurrency;
		public bool GetIsShowAllCurrency()
		{
			return IsShowAllCurrency;
		}
		//货币显示排序
		[Key(3)]
		public int AllCurrencySort;
		public int GetAllCurrencySort()
		{
			return AllCurrencySort;
		}
		//掉落时是否显示
		[Key(4)]
		public bool IsShowDrop;
		public bool GetIsShowDrop()
		{
			return IsShowDrop;
		}
	}
}
