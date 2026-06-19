//来源表JobSkillData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class JobSkillData
	{
		public Dictionary<int, JobSkillDataCell> StaticJobSkillDatas = new Dictionary<int, JobSkillDataCell>();
	}
	public class JobSkillDataCell
	{
		//职业技能ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//所属职业ID
		public string Job;
		private int _Job = -1;
		public int GetJob()
		{
			if (_Job == -1 && int.TryParse(Job, out _Job))
			{
			}
			return _Job;
		}
		//前置技能
		public string ProSkill;
		private int _ProSkill = -1;
		public int GetProSkill()
		{
			if (_ProSkill == -1 && int.TryParse(ProSkill, out _ProSkill))
			{
			}
			return _ProSkill;
		}
		//上阵位类型
		public string Type;
		private int _Type = -1;
		public int GetType()
		{
			if (_Type == -1 && int.TryParse(Type, out _Type))
			{
			}
			return _Type;
		}
		//技能名称
		public string Name;
		//技能ICON
		public string Icon;

	}
}
