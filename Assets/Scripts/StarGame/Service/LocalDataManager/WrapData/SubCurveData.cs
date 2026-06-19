//来源表装备配置表_Equip.xlsm.xlsx -> sheet:SubCurve
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SubCurveData
	{
		[Key(0)]
		public Dictionary<int, SubCurveDataCell> StaticSubCurveDatas = new Dictionary<int, SubCurveDataCell>();
	}
	[MessagePackObject]
	public class SubCurveDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//曲线组
		[Key(1)]
		public int Curve;
		public int GetCurve()
		{
			return Curve;
		}
		//等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//倍率
		[Key(3)]
		public int Rate;
		public int GetRate()
		{
			return Rate;
		}
		//战力倍率
		[Key(4)]
		public int PowerRate;
		public int GetPowerRate()
		{
			return PowerRate;
		}
	}
}
