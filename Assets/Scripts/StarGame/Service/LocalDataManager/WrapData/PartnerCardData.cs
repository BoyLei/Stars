//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:PartnerCard
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerCardData
	{
		[Key(0)]
		public Dictionary<long, PartnerCardDataCell> StaticPartnerCardDatas = new Dictionary<long, PartnerCardDataCell>();
	}
	[MessagePackObject]
	public class PartnerCardDataCell
	{
		//道具编号
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//道具名字
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//使用经验
		[Key(2)]
		public int UseExp;
		public int GetUseExp()
		{
			return UseExp;
		}
	}
}
