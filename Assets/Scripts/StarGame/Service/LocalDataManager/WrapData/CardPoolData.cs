//来源表抽卡表_Gacha.xlsm.xlsx -> sheet:CardPool
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CardPoolData
	{
		[Key(0)]
		public Dictionary<int, CardPoolDataCell> StaticCardPoolDatas = new Dictionary<int, CardPoolDataCell>();
	}
	[MessagePackObject]
	public class CardPoolDataCell
	{
		//幻境ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//幻境类型
		[Key(1)]
		public int CardPoolType;
		public int GetCardPoolType()
		{
			return CardPoolType;
		}
		//启梦消耗道具
		[Key(2)]
		public long UseToken;
		public long GetUseToken()
		{
			return UseToken;
		}
		//使用概率组
		[Key(3)]
		public int UseProbGroup;
		public int GetUseProbGroup()
		{
			return UseProbGroup;
		}
		//启梦名称
		[Key(4)]
		public string CardPoolName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_CardPoolName); }
			set { _CardPoolName = value; }
        }
		[IgnoreMember]
		private string _CardPoolName;
		//启梦描述
		[Key(5)]
		public string CardPoolDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_CardPoolDesc); }
			set { _CardPoolDesc = value; }
        }
		[IgnoreMember]
		private string _CardPoolDesc;
		//幻境内伙伴组1（N）
		[Key(6)]
		public int NGroup;
		public int GetNGroup()
		{
			return NGroup;
		}
		//幻境内伙伴组2（R）
		[Key(7)]
		public int RGroup;
		public int GetRGroup()
		{
			return RGroup;
		}
		//幻境内伙伴组3（SR）
		[Key(8)]
		public int SRGroup;
		public int GetSRGroup()
		{
			return SRGroup;
		}
		//幻境内伙伴组4（SSR）
		[Key(9)]
		public int SSRGroup;
		public int GetSSRGroup()
		{
			return SSRGroup;
		}
		//幻境入口小图
		[Key(10)]
		public string EnterPicSmall;
		//幻境KV图
		[Key(11)]
		public string EnterPicKV;
		//限启梦次数
		[Key(12)]
		public int GachaTimesLimits;
		public int GetGachaTimesLimits()
		{
			return GachaTimesLimits;
		}
		//10连折扣比率
		[Key(13)]
		public int SaleRaito;
		public int GetSaleRaito()
		{
			return SaleRaito;
		}
		//幻境显示顺序
		[Key(14)]
		public int DisplayOrder;
		public int GetDisplayOrder()
		{
			return DisplayOrder;
		}
		//条件组ID
		[Key(15)]
		public int DisplayCondition;
		public int GetDisplayCondition()
		{
			return DisplayCondition;
		}
		//幻境概率显示页签名
		[Key(16)]
		public string ProbDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ProbDesc); }
			set { _ProbDesc = value; }
        }
		[IgnoreMember]
		private string _ProbDesc;
		//幻境概率文本
		[Key(17)]
		public string ProbText
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ProbText); }
			set { _ProbText = value; }
        }
		[IgnoreMember]
		private string _ProbText;
		//每启梦伴生代币ID
		[Key(18)]
		public long AssociateToken;
		public long GetAssociateToken()
		{
			return AssociateToken;
		}
		//伴生代币数量
		[Key(19)]
		public long AssociateTokenNum;
		public long GetAssociateTokenNum()
		{
			return AssociateTokenNum;
		}
	}
}
