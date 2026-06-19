//来源表护符养成表_Amulet.xlsx -> sheet:AmuletEquipEffect
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AmuletEquipEffectData
	{
		[Key(0)]
		public Dictionary<int, AmuletEquipEffectDataCell> StaticAmuletEquipEffectDatas = new Dictionary<int, AmuletEquipEffectDataCell>();
	}
	[MessagePackObject]
	public class AmuletEquipEffectDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//技能库
		[Key(1)]
		public int SubQuality;
		public int GetSubQuality()
		{
			return SubQuality;
		}
		//品质分类
		[Key(2)]
		public int QualityType;
		public int GetQualityType()
		{
			return QualityType;
		}
		//战技id
		[Key(3)]
		public int WarSkillId;
		public int GetWarSkillId()
		{
			return WarSkillId;
		}
	}
}
