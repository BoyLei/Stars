//来源表技能升级表_SkillUp.xlsm.xlsx -> sheet:SkillTxt
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SkillTxtData
	{
		[Key(0)]
		public Dictionary<int, SkillTxtDataCell> StaticSkillTxtDatas = new Dictionary<int, SkillTxtDataCell>();
	}
	[MessagePackObject]
	public class SkillTxtDataCell
	{
		//序号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//天赋id
		[Key(1)]
		public int Talent;
		public int GetTalent()
		{
			return Talent;
		}
		//技能等级
		[Key(2)]
		public int Skill_Level;
		public int GetSkill_Level()
		{
			return Skill_Level;
		}
		//技能技能描述
		[Key(3)]
		public string Skill_Desc;
	}
}
