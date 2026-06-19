//来源表装备强化表_Intensify.xlsm.xlsx -> sheet:EquipSlotIntensify
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipSlotIntensifyData
	{
		[Key(0)]
		public Dictionary<int, EquipSlotIntensifyDataCell> StaticEquipSlotIntensifyDatas = new Dictionary<int, EquipSlotIntensifyDataCell>();
	}
	[MessagePackObject]
	public class EquipSlotIntensifyDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//职业
		[Key(1)]
		public List<int> Job_id = new List<int>();
		//部位
		[Key(2)]
		public int Slot_id;
		public int GetSlot_id()
		{
			return Slot_id;
		}
		//强化等级
		[Key(3)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//强化属性
		[Key(4)]
		public List<int> Property = new List<int>();
		//强化值
		[Key(5)]
		public List<int> Value = new List<int>();
		//战力
		[Key(6)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
