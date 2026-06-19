//来源表鸣器养成表_Etch.xlsm.xlsx -> sheet:EtchUP
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EtchUPData
	{
		[Key(0)]
		public Dictionary<int, EtchUPDataCell> StaticEtchUPDatas = new Dictionary<int, EtchUPDataCell>();
	}
	[MessagePackObject]
	public class EtchUPDataCell
	{
		//id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//强化阶段
		[Key(1)]
		public int Stage;
		public int GetStage()
		{
			return Stage;
		}
		//消耗材料数量
		[Key(2)]
		public long ItemCost;
		public long GetItemCost()
		{
			return ItemCost;
		}
		//消耗材料阶段
		[Key(3)]
		public int ItemCostType;
		public int GetItemCostType()
		{
			return ItemCostType;
		}
		//战技点数
		[Key(4)]
		public int BattleSkillIPoint;
		public int GetBattleSkillIPoint()
		{
			return BattleSkillIPoint;
		}
		//品质
		[Key(5)]
		public int EtchQuality;
		public int GetEtchQuality()
		{
			return EtchQuality;
		}
	}
}
