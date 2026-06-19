//来源表战力表_Power.xlsm.xlsx -> sheet:PartUpperLimit
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartUpperLimitData
	{
		[Key(0)]
		public Dictionary<int, PartUpperLimitDataCell> StaticPartUpperLimitDatas = new Dictionary<int, PartUpperLimitDataCell>();
	}
	[MessagePackObject]
	public class PartUpperLimitDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//养成线ID
		[Key(1)]
		public int PartID;
		public int GetPartID()
		{
			return PartID;
		}
		//生效开服时间
		[Key(2)]
		public int OpenDay;
		public int GetOpenDay()
		{
			return OpenDay;
		}
		//理论上限值
		[Key(3)]
		public int UpperLimitPower;
		public int GetUpperLimitPower()
		{
			return UpperLimitPower;
		}
	}
}
