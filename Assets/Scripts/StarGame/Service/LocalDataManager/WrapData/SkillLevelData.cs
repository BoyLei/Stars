//来源表技能升级表_SkillUp.xlsm.xlsx -> sheet:SkillLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SkillLevelData
	{
		[Key(0)]
		public Dictionary<int, SkillLevelDataCell> StaticSkillLevelDatas = new Dictionary<int, SkillLevelDataCell>();
	}
	[MessagePackObject]
	public class SkillLevelDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//升级条件
		[Key(1)]
		public int Condition_id;
		public int GetCondition_id()
		{
			return Condition_id;
		}
		//天赋id
		[Key(2)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//天赋等级
		[Key(3)]
		public int Skill_Level;
		public int GetSkill_Level()
		{
			return Skill_Level;
		}
		//升级点数
		[Key(4)]
		public int Need_Points;
		public int GetNeed_Points()
		{
			return Need_Points;
		}
		//战力
		[Key(5)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
