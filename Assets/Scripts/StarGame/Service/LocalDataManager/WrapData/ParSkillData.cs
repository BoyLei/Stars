//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:ParSkill
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ParSkillData
	{
		[Key(0)]
		public Dictionary<int, ParSkillDataCell> StaticParSkillDatas = new Dictionary<int, ParSkillDataCell>();
	}
	[MessagePackObject]
	public class ParSkillDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//伙伴ID
		[Key(1)]
		public long Par;
		public long GetPar()
		{
			return Par;
		}
		//星级
		[Key(2)]
		public int Star;
		public int GetStar()
		{
			return Star;
		}
		//伙伴技能
		[Key(3)]
		public List<int> SelfSkill = new List<int>();
		//登场技能
		[Key(4)]
		public int ParSkill;
		public int GetParSkill()
		{
			return ParSkill;
		}
		//技能等级
		[Key(5)]
		public List<int> SelfSkillLV = new List<int>();
		//技能等级
		[Key(6)]
		public int ParSkillLV;
		public int GetParSkillLV()
		{
			return ParSkillLV;
		}
		//伙伴被动
		[Key(7)]
		public List<int> ParPassive = new List<int>();
		//伙伴自身被动
		[Key(8)]
		public List<int> SelfPassive = new List<int>();
		//升星描述
		[Key(9)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
	}
}
