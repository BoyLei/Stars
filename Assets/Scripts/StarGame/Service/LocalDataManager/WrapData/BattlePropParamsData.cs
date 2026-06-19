//来源表战斗技能参数_SkillParams.xlsx -> sheet:BattlePropParams
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattlePropParamsData
	{
		[Key(0)]
		public Dictionary<int, BattlePropParamsDataCell> StaticBattlePropParamsDatas = new Dictionary<int, BattlePropParamsDataCell>();
	}
	[MessagePackObject]
	public class BattlePropParamsDataCell
	{
		//引用ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//引用组ID
		[Key(1)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
		//技能等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//属性ID组
		[Key(3)]
		public List<int> PropIDs = new List<int>();
		//属性值组
		[Key(4)]
		public List<int> PropValues = new List<int>();
	}
}
