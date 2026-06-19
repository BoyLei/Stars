//来源表纹章养成表_Heraldry.xlsx -> sheet:HeraldryEquip
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class HeraldryEquipData
	{
		[Key(0)]
		public Dictionary<long, HeraldryEquipDataCell> StaticHeraldryEquipDatas = new Dictionary<long, HeraldryEquipDataCell>();
	}
	[MessagePackObject]
	public class HeraldryEquipDataCell
	{
		//纹章id
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//纹章等级
		[Key(1)]
		public int HeraldryLevel;
		public int GetHeraldryLevel()
		{
			return HeraldryLevel;
		}
		//可镶嵌护符类型
		[Key(2)]
		public int AmuletType;
		public int GetAmuletType()
		{
			return AmuletType;
		}
		//纹章基础属性
		[Key(3)]
		public List<int> BaseAttrId = new List<int>();
		//基础属性值
		[Key(4)]
		public List<int> BaseAttrVal = new List<int>();
		//共鸣槽位
		[Key(5)]
		public int ResonanceSlot;
		public int GetResonanceSlot()
		{
			return ResonanceSlot;
		}
		//纹章共鸣属性
		[Key(6)]
		public List<int> ResonanceAttrID = new List<int>();
		//共鸣属性值
		[Key(7)]
		public List<int> ResonanceAttrVal = new List<int>();
		//战力
		[Key(8)]
		public long Power1;
		public long GetPower1()
		{
			return Power1;
		}
		//共鸣战力
		[Key(9)]
		public long Power2;
		public long GetPower2()
		{
			return Power2;
		}
	}
}
