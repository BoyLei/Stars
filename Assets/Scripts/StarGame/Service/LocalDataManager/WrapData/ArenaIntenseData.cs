//来源表竞技场表_Arena.xlsx -> sheet:ArenaIntense
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ArenaIntenseData
	{
		[Key(0)]
		public Dictionary<int, ArenaIntenseDataCell> StaticArenaIntenseDatas = new Dictionary<int, ArenaIntenseDataCell>();
	}
	[MessagePackObject]
	public class ArenaIntenseDataCell
	{
		//经过时间
		[Key(0)]
		public int CostTime;
		public int GetCostTime()
		{
			return CostTime;
		}
		//攻击加成数值%
		[Key(1)]
		public int AttackAdd;
		public int GetAttackAdd()
		{
			return AttackAdd;
		}
	}
}
