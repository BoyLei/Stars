//来源表装备强化表_Intensify.xlsm.xlsx -> sheet:EquipSlotUpLimit
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipSlotUpLimitData
	{
		[Key(0)]
		public Dictionary<int, EquipSlotUpLimitDataCell> StaticEquipSlotUpLimitDatas = new Dictionary<int, EquipSlotUpLimitDataCell>();
	}
	[MessagePackObject]
	public class EquipSlotUpLimitDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//角色等级限制
		[Key(1)]
		public int Condition;
		public int GetCondition()
		{
			return Condition;
		}
		//全身强化等级
		[Key(2)]
		public int ConditionNum;
		public int GetConditionNum()
		{
			return ConditionNum;
		}
		//强化上限
		[Key(3)]
		public int Max;
		public int GetMax()
		{
			return Max;
		}
	}
}
