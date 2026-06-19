//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:PartnerNature
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerNatureData
	{
		[Key(0)]
		public Dictionary<int, PartnerNatureDataCell> StaticPartnerNatureDatas = new Dictionary<int, PartnerNatureDataCell>();
	}
	[MessagePackObject]
	public class PartnerNatureDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//属性组
		[Key(1)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//战力
		[Key(3)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
		//属性ID
		[Key(4)]
		public List<int> AttrID = new List<int>();
		//属性值
		[Key(5)]
		public List<int> AttrValue = new List<int>();
		//资质ID
		[Key(6)]
		public List<int> Qualification = new List<int>();
		//资质值
		[Key(7)]
		public List<int> QualificationValue = new List<int>();
	}
}
