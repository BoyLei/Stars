//来源表CradPoolConstData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class CradPoolConstData
	{
		public Dictionary<int, CradPoolConstDataCell> StaticCradPoolConstDatas = new Dictionary<int, CradPoolConstDataCell>();
	}
	public class CradPoolConstDataCell
	{
		//卡池ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//卡池类型
		public string CardPoolType;
		private int _CardPoolType = -1;
		public int GetCardPoolType()
		{
			if (_CardPoolType == -1 && int.TryParse(CardPoolType, out _CardPoolType))
			{
			}
			return _CardPoolType;
		}
		//抽卡消耗道具
		public string UseToken;
		private long _UseToken = -1;
		public long GetUseToken()
		{
			if (_UseToken == -1 && long.TryParse(UseToken, out _UseToken))
			{
			}
			return _UseToken;
		}
		//使用概率组
		public string UseProbGroup;
		private int _UseProbGroup = -1;
		public int GetUseProbGroup()
		{
			if (_UseProbGroup == -1 && int.TryParse(UseProbGroup, out _UseProbGroup))
			{
			}
			return _UseProbGroup;
		}
		//卡池名称
		public string CardPoolName;
		//卡池描述
		public string CardPoolDesc;
		//池内伙伴组1（N）
		public string NGroup;
		private int _NGroup = -1;
		public int GetNGroup()
		{
			if (_NGroup == -1 && int.TryParse(NGroup, out _NGroup))
			{
			}
			return _NGroup;
		}
		//池内伙伴组2（R）
		public string RGroup;
		private int _RGroup = -1;
		public int GetRGroup()
		{
			if (_RGroup == -1 && int.TryParse(RGroup, out _RGroup))
			{
			}
			return _RGroup;
		}
		//池内伙伴组3（SR）
		public string SRGroup;
		private int _SRGroup = -1;
		public int GetSRGroup()
		{
			if (_SRGroup == -1 && int.TryParse(SRGroup, out _SRGroup))
			{
			}
			return _SRGroup;
		}
		//池内伙伴组4（SSR）
		public string SSRGroup;
		private int _SSRGroup = -1;
		public int GetSSRGroup()
		{
			if (_SSRGroup == -1 && int.TryParse(SSRGroup, out _SSRGroup))
			{
			}
			return _SSRGroup;
		}
		//卡池入口小图
		public string EnterPicSmall;
		//卡池KV图
		public string EnterPicKV;
		//限抽次数
		public string GachaTimesLimits;
		private int _GachaTimesLimits = -1;
		public int GetGachaTimesLimits()
		{
			if (_GachaTimesLimits == -1 && int.TryParse(GachaTimesLimits, out _GachaTimesLimits))
			{
			}
			return _GachaTimesLimits;
		}
		//10连折扣比率
		public string SaleRaito;
		private int _SaleRaito = -1;
		public int GetSaleRaito()
		{
			if (_SaleRaito == -1 && int.TryParse(SaleRaito, out _SaleRaito))
			{
			}
			return _SaleRaito;
		}
		//卡池显示顺序
		public string DisplayOrder;
		private int _DisplayOrder = -1;
		public int GetDisplayOrder()
		{
			if (_DisplayOrder == -1 && int.TryParse(DisplayOrder, out _DisplayOrder))
			{
			}
			return _DisplayOrder;
		}
		//需要玩家等级
		public string DisplayLevel;
		private int _DisplayLevel = -1;
		public int GetDisplayLevel()
		{
			if (_DisplayLevel == -1 && int.TryParse(DisplayLevel, out _DisplayLevel))
			{
			}
			return _DisplayLevel;
		}

	}
}
