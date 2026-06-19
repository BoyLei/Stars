//来源表护符养成表_Amulet.xlsx -> sheet:AmuletEquip
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AmuletEquipData
	{
		[Key(0)]
		public Dictionary<long, AmuletEquipDataCell> StaticAmuletEquipDatas = new Dictionary<long, AmuletEquipDataCell>();
	}
	[MessagePackObject]
	public class AmuletEquipDataCell
	{
		//护符id
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//护符类型
		[Key(1)]
		public int AmuletType;
		public int GetAmuletType()
		{
			return AmuletType;
		}
		//护符等级
		[Key(2)]
		public int EquipLevel;
		public int GetEquipLevel()
		{
			return EquipLevel;
		}
		//穿戴等级
		[Key(3)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//技能库
		[Key(4)]
		public int SubQuality;
		public int GetSubQuality()
		{
			return SubQuality;
		}
		//品质分类
		[Key(5)]
		public List<int> QualityType = new List<int>();
		//随机权重
		[Key(6)]
		public List<int> Weight = new List<int>();
	}
}
