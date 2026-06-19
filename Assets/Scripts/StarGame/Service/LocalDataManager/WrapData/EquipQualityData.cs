//来源表装备配置表_Equip.xlsm.xlsx -> sheet:EquipQuality
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipQualityData
	{
		[Key(0)]
		public Dictionary<int, EquipQualityDataCell> StaticEquipQualityDatas = new Dictionary<int, EquipQualityDataCell>();
	}
	[MessagePackObject]
	public class EquipQualityDataCell
	{
		//装备品质
		[Key(0)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//条目数量-下限
		[Key(1)]
		public int NumMin;
		public int GetNumMin()
		{
			return NumMin;
		}
		//条目数量-上限
		[Key(2)]
		public int NumMax;
		public int GetNumMax()
		{
			return NumMax;
		}
		//品质库
		[Key(3)]
		public List<int> SubQuality = new List<int>();
		//随机权重
		[Key(4)]
		public List<int> Weight = new List<int>();
	}
}
