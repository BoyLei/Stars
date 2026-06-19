//来源表装备配置表_Equip.xlsm.xlsx -> sheet:Value
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ValueData
	{
		[Key(0)]
		public Dictionary<int, ValueDataCell> StaticValueDatas = new Dictionary<int, ValueDataCell>();
	}
	[MessagePackObject]
	public class ValueDataCell
	{
		//稀有度
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//生成权重
		[Key(1)]
		public int NewWeight;
		public int GetNewWeight()
		{
			return NewWeight;
		}
		//重构权重
		[Key(2)]
		public int RefactWeight;
		public int GetRefactWeight()
		{
			return RefactWeight;
		}
		//补充词条权重
		[Key(3)]
		public int NewSubWeight;
		public int GetNewSubWeight()
		{
			return NewSubWeight;
		}
	}
}
