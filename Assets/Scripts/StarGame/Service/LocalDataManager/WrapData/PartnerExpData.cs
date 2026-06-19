//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:PartnerExp
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerExpData
	{
		[Key(0)]
		public Dictionary<int, PartnerExpDataCell> StaticPartnerExpDatas = new Dictionary<int, PartnerExpDataCell>();
	}
	[MessagePackObject]
	public class PartnerExpDataCell
	{
		//角色等级
		[Key(0)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//升级所需经验(单次升级)
		[Key(1)]
		public long Exp;
		public long GetExp()
		{
			return Exp;
		}
		//冒险等级限制
		[Key(2)]
		public int AdvGrade;
		public int GetAdvGrade()
		{
			return AdvGrade;
		}
		//角色等级限制
		[Key(3)]
		public int UnlockLevel;
		public int GetUnlockLevel()
		{
			return UnlockLevel;
		}
	}
}
