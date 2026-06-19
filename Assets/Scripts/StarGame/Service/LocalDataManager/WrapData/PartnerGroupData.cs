//来源表抽卡表_Gacha.xlsm.xlsx -> sheet:PartnerGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerGroupData
	{
		[Key(0)]
		public Dictionary<int, PartnerGroupDataCell> StaticPartnerGroupDatas = new Dictionary<int, PartnerGroupDataCell>();
	}
	[MessagePackObject]
	public class PartnerGroupDataCell
	{
		//伙伴组主KEY
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//包含伙伴
		[Key(1)]
		public long IncludePartner;
		public long GetIncludePartner()
		{
			return IncludePartner;
		}
		//伙伴组ID
		[Key(2)]
		public int PartnerGroup;
		public int GetPartnerGroup()
		{
			return PartnerGroup;
		}
		//是否是UP角色
		[Key(3)]
		public bool IsProbUP;
		public bool GetIsProbUP()
		{
			return IsProbUP;
		}
	}
}
