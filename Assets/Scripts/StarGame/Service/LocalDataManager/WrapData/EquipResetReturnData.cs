//来源表装备配置表_Equip.xlsm.xlsx -> sheet:EquipResetReturn
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipResetReturnData
	{
		[Key(0)]
		public Dictionary<int, EquipResetReturnDataCell> StaticEquipResetReturnDatas = new Dictionary<int, EquipResetReturnDataCell>();
	}
	[MessagePackObject]
	public class EquipResetReturnDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//素材品质
		[Key(1)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//装备使用等级
		[Key(2)]
		public int EquipLevel;
		public int GetEquipLevel()
		{
			return EquipLevel;
		}
		//完美度
		[Key(3)]
		public int Extent;
		public int GetExtent()
		{
			return Extent;
		}
		//返还道具
		[Key(4)]
		public List<long> ReturnItem_id = new List<long>();
		//返还道具数量
		[Key(5)]
		public List<long> ReturnItem_num = new List<long>();
	}
}
