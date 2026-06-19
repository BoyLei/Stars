//来源表伙伴装备表_PartnerEquip.xlsm.xlsx -> sheet:Effect
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EffectData
	{
		[Key(0)]
		public Dictionary<int, EffectDataCell> StaticEffectDatas = new Dictionary<int, EffectDataCell>();
	}
	[MessagePackObject]
	public class EffectDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//生效伙伴阵营
		[Key(1)]
		public int PartnerType;
		public int GetPartnerType()
		{
			return PartnerType;
		}
		//特技名
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//特技描述
		[Key(3)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//出战技能ID
		[Key(4)]
		public int PassiveSkillID;
		public int GetPassiveSkillID()
		{
			return PassiveSkillID;
		}
		//助战位属性
		[Key(5)]
		public List<int> PropID = new List<int>();
		//助战位属性值
		[Key(6)]
		public List<int> PropValue = new List<int>();
		//特技图标
		[Key(7)]
		public string IconPath;
	}
}
