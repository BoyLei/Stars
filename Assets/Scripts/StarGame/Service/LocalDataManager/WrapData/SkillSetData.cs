using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SkillSetData
	{
		public Dictionary<int, SkillSetDataCell> StaticSkillSetDatas = new Dictionary<int, SkillSetDataCell>();
	}
	public class SkillSetDataCell
	{		//技能位
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//技能类型
		public string SkillType;
		private int _SkillType = -1;
		public int GetSkillType()
		{
			if (_SkillType == -1 && int.TryParse(SkillType, out _SkillType))
			{
			}
			return _SkillType;
		}
		//技能位置
		public List<int> SkillPos = new List<int>();
		//按钮缩放比例
		public string Scale;
		private int _Scale = -1;
		public int GetScale()
		{
			if (_Scale == -1 && int.TryParse(Scale, out _Scale))
			{
			}
			return _Scale;
		}
		//默认上阵技能
		public string DefaultJobSkills;
		private int _DefaultJobSkills = -1;
		public int GetDefaultJobSkills()
		{
			if (_DefaultJobSkills == -1 && int.TryParse(DefaultJobSkills, out _DefaultJobSkills))
			{
			}
			return _DefaultJobSkills;
		}

	}
}
