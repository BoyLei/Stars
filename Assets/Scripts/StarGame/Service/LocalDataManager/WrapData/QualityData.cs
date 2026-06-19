//来源表装备配置表_Equip.xlsm.xlsx -> sheet:Quality
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class QualityData
	{
		[Key(0)]
		public Dictionary<int, QualityDataCell> StaticQualityDatas = new Dictionary<int, QualityDataCell>();
	}
	[MessagePackObject]
	public class QualityDataCell
	{
		//词条品质
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//权重系数
		[Key(1)]
		public int QualityWeight;
		public int GetQualityWeight()
		{
			return QualityWeight;
		}
	}
}
