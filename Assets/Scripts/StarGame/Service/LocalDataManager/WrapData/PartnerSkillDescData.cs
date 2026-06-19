//来源表战斗文本信息表_DescText.xlsm.xlsx -> sheet:PartnerSkillDesc
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerSkillDescData
	{
		[Key(0)]
		public Dictionary<int, PartnerSkillDescDataCell> StaticPartnerSkillDescDatas = new Dictionary<int, PartnerSkillDescDataCell>();
	}
	[MessagePackObject]
	public class PartnerSkillDescDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//技能id
		[Key(1)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//技能等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//技能名
		[Key(3)]
		public string SkillName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SkillName); }
			set { _SkillName = value; }
        }
		[IgnoreMember]
		private string _SkillName;
		//技能描述
		[Key(4)]
		public string Decs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Decs); }
			set { _Decs = value; }
        }
		[IgnoreMember]
		private string _Decs;
		//技能图标
		[Key(5)]
		public string IconPath;
		//状态图标
		[Key(6)]
		public string IconPath2;
		//战力
		[Key(7)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
