//来源表装备强化表_Intensify.xlsm.xlsx -> sheet:EquipSlotUpExpend
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipSlotUpExpendData
	{
		[Key(0)]
		public Dictionary<int, EquipSlotUpExpendDataCell> StaticEquipSlotUpExpendDatas = new Dictionary<int, EquipSlotUpExpendDataCell>();
	}
	[MessagePackObject]
	public class EquipSlotUpExpendDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//装备槽位
		[Key(1)]
		public List<int> SubType = new List<int>();
		//强化等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//货币消耗
		[Key(3)]
		public List<long> Money = new List<long>();
		//道具消耗
		[Key(4)]
		public List<long> Item_id = new List<long>();
		//道具数量
		[Key(5)]
		public List<long> Item_num = new List<long>();
		//基础成功率
		[Key(6)]
		public int Base;
		public int GetBase()
		{
			return Base;
		}
		//补偿成功率
		[Key(7)]
		public int Return;
		public int GetReturn()
		{
			return Return;
		}
		//补偿上限率
		[Key(8)]
		public int Return_max;
		public int GetReturn_max()
		{
			return Return_max;
		}
	}
}
