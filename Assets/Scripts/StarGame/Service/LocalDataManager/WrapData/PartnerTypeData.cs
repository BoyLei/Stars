//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:PartnerType
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerTypeData
	{
		[Key(0)]
		public Dictionary<int, PartnerTypeDataCell> StaticPartnerTypeDatas = new Dictionary<int, PartnerTypeDataCell>();
	}
	[MessagePackObject]
	public class PartnerTypeDataCell
	{
		//id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//显示名
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//icon资源（阴）
		[Key(2)]
		public string ShowIcon;
		//icon资源2（阳）
		[Key(3)]
		public string ShowIcon_s;
	}
}
