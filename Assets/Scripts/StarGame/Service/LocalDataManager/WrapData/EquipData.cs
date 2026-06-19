//来源表装备配置表_Equip.xlsm.xlsx -> sheet:Equip
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipData
	{
		[Key(0)]
		public Dictionary<long, EquipDataCell> StaticEquipDatas = new Dictionary<long, EquipDataCell>();
	}
	[MessagePackObject]
	public class EquipDataCell
	{
		//装备id
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//穿戴职业
		[Key(1)]
		public List<int> JobType = new List<int>();
		//穿戴职业
		[Key(2)]
		public int JobBaseType;
		public int GetJobBaseType()
		{
			return JobBaseType;
		}
		//部件
		[Key(3)]
		public int Parts;
		public int GetParts()
		{
			return Parts;
		}
		//品质
		[Key(4)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//装备阶段
		[Key(5)]
		public int Equip_Level;
		public int GetEquip_Level()
		{
			return Equip_Level;
		}
		//穿戴等级
		[Key(6)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//主属性id组
		[Key(7)]
		public List<int> Nature = new List<int>();
		//基础上限
		[Key(8)]
		public List<int> NumMax = new List<int>();
		//副属性库
		[Key(9)]
		public List<int> Group = new List<int>();
		//装备外观
		[Key(10)]
		public int Model;
		public int GetModel()
		{
			return Model;
		}
		//战力
		[Key(11)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
