//来源表伙伴装备表_PartnerEquip.xlsm.xlsx -> sheet:PartnerEquip
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerEquipData
	{
		[Key(0)]
		public Dictionary<long, PartnerEquipDataCell> StaticPartnerEquipDatas = new Dictionary<long, PartnerEquipDataCell>();
	}
	[MessagePackObject]
	public class PartnerEquipDataCell
	{
		//装备id
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//装备等级
		[Key(1)]
		public int EquipLevel;
		public int GetEquipLevel()
		{
			return EquipLevel;
		}
		//穿戴等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//装备阶数
		[Key(3)]
		public int AdvancedLevel;
		public int GetAdvancedLevel()
		{
			return AdvancedLevel;
		}
		//伙伴装备组
		[Key(4)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
		//资质属性
		[Key(5)]
		public List<int> QualiFications = new List<int>();
		//资质属性值
		[Key(6)]
		public List<int> QualiFicationsValue = new List<int>();
		//玩家属性
		[Key(7)]
		public List<int> Nature = new List<int>();
		//玩家属性值
		[Key(8)]
		public List<int> Value = new List<int>();
		//装备特技
		[Key(9)]
		public List<int> Effect = new List<int>();
		//战力
		[Key(10)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
