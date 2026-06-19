//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:ParConversion
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ParConversionData
	{
		[Key(0)]
		public Dictionary<int, ParConversionDataCell> StaticParConversionDatas = new Dictionary<int, ParConversionDataCell>();
	}
	[MessagePackObject]
	public class ParConversionDataCell
	{
		//id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//资质ID
		[Key(1)]
		public int QualificationID;
		public int GetQualificationID()
		{
			return QualificationID;
		}
		//资质名称
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//转化属性
		[Key(3)]
		public int Nature;
		public int GetNature()
		{
			return Nature;
		}
		//资质区间
		[Key(4)]
		public int Range;
		public int GetRange()
		{
			return Range;
		}
		//转化比例
		[Key(5)]
		public int Ratio;
		public int GetRatio()
		{
			return Ratio;
		}
		//战力
		[Key(6)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
		//战力系数
		[Key(7)]
		public long PowerCo;
		public long GetPowerCo()
		{
			return PowerCo;
		}
	}
}
