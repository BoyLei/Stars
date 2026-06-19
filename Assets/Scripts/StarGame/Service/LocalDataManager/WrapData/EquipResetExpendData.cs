//来源表装备配置表_Equip.xlsm.xlsx -> sheet:EquipResetExpend
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipResetExpendData
	{
		[Key(0)]
		public Dictionary<int, EquipResetExpendDataCell> StaticEquipResetExpendDatas = new Dictionary<int, EquipResetExpendDataCell>();
	}
	[MessagePackObject]
	public class EquipResetExpendDataCell
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
		public int Job;
		public int GetJob()
		{
			return Job;
		}
		//装备部位
		[Key(2)]
		public List<int> SubType = new List<int>();
		//装备等级
		[Key(3)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//货币消耗
		[Key(4)]
		public List<long> Money = new List<long>();
		//道具消耗
		[Key(5)]
		public List<long> Item_id = new List<long>();
		//道具数量
		[Key(6)]
		public List<long> Item_num = new List<long>();
		//属性id
		[Key(7)]
		public List<int> NatureId = new List<int>();
		//提升下限（/次）
		[Key(8)]
		public List<int> NumMin = new List<int>();
		//提升上限（/次）
		[Key(9)]
		public List<int> NumMax = new List<int>();
	}
}
