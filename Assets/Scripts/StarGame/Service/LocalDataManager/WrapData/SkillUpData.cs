//来源表技能升级表_SkillUp.xlsm.xlsx -> sheet:SkillUp
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SkillUpData
	{
		[Key(0)]
		public Dictionary<int, SkillUpDataCell> StaticSkillUpDatas = new Dictionary<int, SkillUpDataCell>();
	}
	[MessagePackObject]
	public class SkillUpDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//职业类型
		[Key(1)]
		public int Character_Type;
		public int GetCharacter_Type()
		{
			return Character_Type;
		}
		//分支类型
		[Key(2)]
		public int Branch_Type;
		public int GetBranch_Type()
		{
			return Branch_Type;
		}
		//解锁等级
		[Key(3)]
		public int Unlock_Level;
		public int GetUnlock_Level()
		{
			return Unlock_Level;
		}
		//技能组
		[Key(4)]
		public int Skill_Group;
		public int GetSkill_Group()
		{
			return Skill_Group;
		}
		//技能类型
		[Key(5)]
		public int Skill_type;
		public int GetSkill_type()
		{
			return Skill_type;
		}
		//技能id
		[Key(6)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
	}
}
