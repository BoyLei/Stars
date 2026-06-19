//来源表SkillTypeData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SkillTypeData
	{
		public Dictionary<int, SkillTypeDataCell> StaticSkillTypeDatas = new Dictionary<int, SkillTypeDataCell>();
	}
	public class SkillTypeDataCell
	{
		//技能类型枚举
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//限制可上阵数量
		public string LimitNum;
		private int _LimitNum = -1;
		public int GetLimitNum()
		{
			if (_LimitNum == -1 && int.TryParse(LimitNum, out _LimitNum))
			{
			}
			return _LimitNum;
		}

	}
}
