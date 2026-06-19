//来源表技能升级表_SkillUp.xlsm.xlsx -> sheet:SkillReset
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SkillResetData
	{
		[Key(0)]
		public Dictionary<int, SkillResetDataCell> StaticSkillResetDatas = new Dictionary<int, SkillResetDataCell>();
	}
	[MessagePackObject]
	public class SkillResetDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//消耗物品
		[Key(1)]
		public List<long> ItemCost = new List<long>();
		//消耗物品数量
		[Key(2)]
		public List<long> ItemNum = new List<long>();
	}
}
